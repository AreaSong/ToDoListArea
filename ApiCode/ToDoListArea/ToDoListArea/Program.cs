using DbContextHelp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ToDoListArea.Services;
using ToDoListArea.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 配置数据库连接
builder.Services.AddDbContext<ToDoListAreaDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            // 配置连接重试策略
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);

            // 配置命令超时时间
            sqlOptions.CommandTimeout(30);
        });

    // 在开发环境启用敏感数据日志记录
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// 配置JWT设置
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// 配置JWT认证
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings?.Issuer,
            ValidAudience = jwtSettings?.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings?.SecretKey ?? "")),
            ClockSkew = TimeSpan.Zero
        };
    });

// 配置授权策略
builder.Services.AddAuthorization(options =>
{
    // 数据一致性查看权限：普通认证用户即可
    options.AddPolicy("DataConsistencyRead", policy =>
        policy.RequireAuthenticatedUser());

    // 数据一致性修复权限：需要管理员角色
    options.AddPolicy("DataConsistencyWrite", policy =>
        policy.RequireRole("Admin"));

    // 系统管理权限：需要管理员角色和特定声明
    options.AddPolicy("SystemAdmin", policy =>
        policy.RequireRole("Admin")
              .RequireClaim("Permission", "SystemManagement"));
});

// 注册服务层
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<DataConsistencyService>();

// Add services to the container.
builder.Services.AddControllers();

// 配置CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ToDoListArea API", Version = "v1" });

    // 配置JWT认证
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// 确保数据库已创建（仅在开发环境）
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ToDoListAreaDbContext>();
        try
        {
            // 检查数据库连接
            await context.Database.CanConnectAsync();
            app.Logger.LogInformation("数据库连接成功");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "数据库连接失败");
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ToDoListArea API V1");
        c.RoutePrefix = "swagger"; // 设置Swagger UI为 /swagger 路径
    });
}

// 使用全局异常处理中间件
app.UseGlobalExceptionHandling();

// 使用CORS
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
