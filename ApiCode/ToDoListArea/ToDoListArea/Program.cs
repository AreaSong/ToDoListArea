using DbContextHelp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text;
using System.Text.Json;
using ToDoListArea.Services;
using ToDoListArea.Middleware;
using ToDoListArea.Tools;

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

    // 生产环境配置：禁用敏感数据日志
    options.EnableSensitiveDataLogging(false);
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
    // 管理员权限策略
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("admin"));

    // 用户权限策略（管理员和普通用户都可以）
    options.AddPolicy("UserAccess", policy =>
        policy.RequireRole("admin", "user"));

    // 数据一致性查看权限：普通认证用户即可
    options.AddPolicy("DataConsistencyRead", policy =>
        policy.RequireAuthenticatedUser());

    // 数据一致性修复权限：需要管理员角色
    options.AddPolicy("DataConsistencyWrite", policy =>
        policy.RequireRole("admin"));

    // 系统管理权限：需要管理员角色和特定声明
    options.AddPolicy("SystemAdmin", policy =>
        policy.RequireRole("admin")
              .RequireClaim("Permission", "SystemManagement"));
});

// 注册服务层
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IInvitationCodeService, InvitationCodeService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
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

// 生产环境：确保数据库存在
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ToDoListAreaDbContext>();
    try
    {
        await context.Database.EnsureCreatedAsync();
    }
    catch (Exception)
    {
        // 生产环境下静默处理数据库连接问题
        // 应通过监控系统检测数据库状态
    }
}

// Configure the HTTP request pipeline.

// 开发环境配置
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ToDoListArea API v1");
        c.RoutePrefix = "swagger";
    });
}

// 使用安全头中间件
app.UseSecurityHeaders();

// 使用监控中间件
app.UseMetricsCollection();
app.UsePerformanceMonitoring();

// 使用全局异常处理中间件
app.UseGlobalExceptionHandling();

// 根据环境使用不同的CORS策略
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

// 使用角色权限中间件
app.UseRoleAuthorization();

// 使用用户活动追踪中间件
app.UseUserActivityTracking();

app.MapControllers();

// 映射健康检查端点
app.MapHealthChecks("/health");

app.Run();
