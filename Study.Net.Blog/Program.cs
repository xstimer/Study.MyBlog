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

//����swagger��Ȩ���
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

//��ȡ�����ļ���Jwt����Ϣ��Ȼ��ͨ��Configuration����ϵͳע�뵽Controller�������Ȩ
builder.Services.Configure<JwtSetting>(builder.Configuration.GetSection("Jwt"));

//����jwt������Ȩ��
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        var jwtSetting = builder.Configuration.GetSection("Jwt").Get<JwtSetting>();
        byte[] keyBytes = Encoding.UTF8.GetBytes(jwtSetting.SecKey);
        var secKey = new SymmetricSecurityKey(keyBytes);
        opt.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSetting.Issuer,    //��ʾ�䷢Token��webӦ�ó���

            ValidateAudience = true,
            ValidAudience = jwtSetting.Audience,//Token��������

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

//ע��Filter����
builder.Services.Configure<MvcOptions>(opt =>
{
    opt.Filters.Add<JwtVersionCheckFilter>();
});


//ע��Automapper
builder.Services.AddAutoMapper(typeof(DTOMapper));

//�ִ���������Զ�������ע��
builder.Services.AddCustomIOC();

//identityע��
builder.Services.AddIdentityIOC();

//ע��Redis�������
builder.Services.AddStackExchangeRedisCache(opt =>
{
    opt.Configuration = "localhost";    //�����ַ
    opt.InstanceName = "blog_";         //����ǰ׺
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//��ӵ��ܵ���
app.UseAuthentication();

app.UseAuthorization();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();


public static class IOCExtend
{
    /// <summary>
    /// ע���Զ���ӿ�
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomIOC(this IServiceCollection services)
    {
        //ע��ִ���
        services.AddScoped<IArticleRepository, ArticleRepository>();
        services.AddScoped<IArticleTypeRepository, ArticleTypeRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        //ע������
        services.AddScoped<IArticleService, ArticleService>();
        services.AddScoped<IArticleTypeService, ArticleTypeService>();
        services.AddScoped<IUserService, UserService>();
        return services;
    }

    public static IServiceCollection AddIdentityIOC(this IServiceCollection services)
    {
        //ע�����ݱ���
        services.AddDataProtection();

        //����IdentityCore
        services.AddIdentityCore<User>(opt =>
        {
            opt.Password.RequireDigit = true;   //����������
            opt.Password.RequireLowercase = true;//Сд
            opt.Password.RequireUppercase = true;//��д
            opt.Password.RequireNonAlphanumeric = false; //�Ƿ���Ҫ����ĸ�������ַ�
            opt.Password.RequiredLength = 8;    //���ٰ�λ����
        
            opt.Lockout.MaxFailedAccessAttempts = 5;    //��¼ʧ�ܴ���
            opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);//�������ʧ�ܣ����������

            opt.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;  //�������ù���
            opt.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultProvider;
        });

        //������֤���
        var idBuilder = new IdentityBuilder(typeof(User), typeof(Role), services);

        idBuilder.AddEntityFrameworkStores<MySqlDbContext>()
            .AddDefaultTokenProviders()
            .AddUserManager<UserManager<User>>()
            .AddRoleManager<RoleManager<Role>>();

        return services;
    }
}