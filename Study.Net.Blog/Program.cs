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

//ע��Automapper
builder.Services.AddAutoMapper(typeof(DTOMapper));

//�ִ���������Զ�������ע��
builder.Services.AddCustomIOC();

//identityע��
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