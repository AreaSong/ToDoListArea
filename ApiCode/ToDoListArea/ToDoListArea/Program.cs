using DbContextHelp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text;
using System.Text.Json;
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
            ClockSkew = TimeSpan.FromMinutes(5), // 减少时钟偏差容忍度
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            SaveSigninToken = false, // 不保存令牌
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }
        };

        // 配置JWT事件处理
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("JWT authentication failed for {Path}: {Error}",
                    context.Request.Path, context.Exception.Message);
                return System.Threading.Tasks.Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize(new {
                    error = "未授权访问",
                    message = "请提供有效的访问令牌",
                    timestamp = DateTime.UtcNow
                });
                return context.Response.WriteAsync(result);
            }
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

// 注册监控和日志服务
builder.Services.AddSingleton<IMetricsService, MetricsService>();
builder.Services.AddScoped<ILoggingService, LoggingService>();

// Add services to the container.
builder.Services.AddControllers();

// 添加健康检查服务
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ToDoListAreaDbContext>("database")
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

// 配置CORS - 生产环境安全策略
builder.Services.AddCors(options =>
{
    // 开发环境策略
    options.AddPolicy("Development", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });

    // 生产环境策略
    options.AddPolicy("Production", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                           ?? new[] { "https://localhost", "https://your-domain.com" };

        policy.WithOrigins(allowedOrigins)
              .WithHeaders("Content-Type", "Authorization", "X-Requested-With")
              .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });

    // 默认策略（最严格）
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost")
              .WithHeaders("Content-Type", "Authorization")
              .WithMethods("GET", "POST")
              .AllowCredentials();
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

// 使用安全头中间件
app.UseSecurityHeaders();

// 使用监控中间件
app.UseMetricsCollection();
app.UsePerformanceMonitoring();

// 使用全局异常处理中间件
app.UseGlobalExceptionHandling();

// 使用CORS - 根据环境选择策略
if (app.Environment.IsDevelopment())
{
    app.UseCors("Development");
}
else
{
    app.UseCors("Production");
}

app.UseAuthentication();
app.UseAuthorization();

// 使用用户活动追踪中间件
app.UseUserActivityTracking();

app.MapControllers();

// 映射健康检查端点
app.MapHealthChecks("/health");

app.Run();
