using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Study.Net.BaseRepository;
using Study.Net.BaseService;
using Study.Net.EFCoreEnvironment.DbContexts;
using Study.Net.IBaseRepository;
using Study.Net.IBaseService;
using Study.Net.Model;
using Study.Net.Utility._Mapper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MySqlDbContext>(opt =>
{
    opt.UseMySql(builder.Configuration.GetSection("connstr").Value, new MySqlServerVersion(new Version(8, 0, 33)));
    
});

//注入Automapper
builder.Services.AddAutoMapper(typeof(DTOMapper));

//仓储、服务层自定义依赖注入
builder.Services.AddCustomIOC();

//identity注入
builder.Services.AddIdentityIOC();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

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