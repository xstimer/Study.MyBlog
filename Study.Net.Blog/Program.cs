using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Study.Net.BaseRepository;
using Study.Net.BaseService;
using Study.Net.Blog.Filters;
using Study.Net.EFCoreEnvironment.DbContexts;
using Study.Net.IBaseRepository;
using Study.Net.IBaseService;
using Study.Net.Model;
using Study.Net.Utility;
using Study.Net.Utility._Mapper;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//启动swagger鉴权组件
builder.Services.AddSwaggerGen(opt =>
{
    var scheme = new OpenApiSecurityScheme()
    {
        Description = $"Authorization header \r\n Example:'Bearer xxxxxxxxxxxxxxxx'",
        Reference = new OpenApiReference() { Type = ReferenceType.SecurityScheme, Id = "Authorization" },
        Scheme = "oauth2",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey

    };
    opt.AddSecurityDefinition("Authorization", scheme);

    var requirment = new OpenApiSecurityRequirement();
    requirment[scheme] = new List<string>();
    opt.AddSecurityRequirement(requirment);
});

//读取配置文件中Jwt的信息，然后通过Configuration配置系统注入到Controller层进行授权
builder.Services.Configure<JwtSetting>(builder.Configuration.GetSection("Jwt"));

//配置jwt：鉴别权限
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        var jwtSetting = builder.Configuration.GetSection("Jwt").Get<JwtSetting>();
        byte[] keyBytes = Encoding.UTF8.GetBytes(jwtSetting.SecKey);
        var secKey = new SymmetricSecurityKey(keyBytes);
        opt.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSetting.Issuer,    //表示颁发Token的web应用程序

            ValidateAudience = true,
            ValidAudience = jwtSetting.Audience,//Token的受理者

            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = secKey,
            ClockSkew = TimeSpan.FromSeconds(jwtSetting.ExpireSeconds)
        };
    });


builder.Services.AddDbContext<MySqlDbContext>(opt =>
{
    opt.UseMySql(builder.Configuration.GetSection("connstr").Value, new MySqlServerVersion(new Version(8, 0, 33)));
    
});

//注入Filter服务
builder.Services.Configure<MvcOptions>(opt =>
{
    opt.Filters.Add<JwtVersionCheckFilter>();
});


//注入Automapper
builder.Services.AddAutoMapper(typeof(DTOMapper));

//仓储、服务层自定义依赖注入
builder.Services.AddCustomIOC();

//identity注入
builder.Services.AddIdentityIOC();

//注入Redis缓存服务
builder.Services.AddStackExchangeRedisCache(opt =>
{
    opt.Configuration = "localhost";    //缓存地址
    opt.InstanceName = "blog_";         //缓存前缀
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//添加到管道中
app.UseAuthentication();

app.UseAuthorization();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();


public static class IOCExtend
{
    /// <summary>
    /// 注入自定义接口
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomIOC(this IServiceCollection services)
    {
        //注入仓储层
        services.AddScoped<IArticleRepository, ArticleRepository>();
        services.AddScoped<IArticleTypeRepository, ArticleTypeRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        //注入服务层
        services.AddScoped<IArticleService, ArticleService>();
        services.AddScoped<IArticleTypeService, ArticleTypeService>();
        services.AddScoped<IUserService, UserService>();
        return services;
    }

    public static IServiceCollection AddIdentityIOC(this IServiceCollection services)
    {
        //注入数据保护
        services.AddDataProtection();

        //配置IdentityCore
        services.AddIdentityCore<User>(opt =>
        {
            opt.Password.RequireDigit = true;   //必须有数字
            opt.Password.RequireLowercase = true;//小写
            opt.Password.RequireUppercase = true;//大写
            opt.Password.RequireNonAlphanumeric = false; //是否需要非字母非数字字符
            opt.Password.RequiredLength = 8;    //最少八位长度
        
            opt.Lockout.MaxFailedAccessAttempts = 5;    //登录失败次数
            opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);//连续五次失败，上锁五分钟

            opt.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;  //密码重置规则
            opt.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultProvider;
        });

        //构建认证框架
        var idBuilder = new IdentityBuilder(typeof(User), typeof(Role), services);

        idBuilder.AddEntityFrameworkStores<MySqlDbContext>()
            .AddDefaultTokenProviders()
            .AddUserManager<UserManager<User>>()
            .AddRoleManager<RoleManager<Role>>();

        return services;
    }
}