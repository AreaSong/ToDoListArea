# 使用官方.NET 8 SDK镜像作为构建环境
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# 设置工作目录
WORKDIR /src

# 复制项目文件
COPY ["ToDoListArea.csproj", "./"]

# 还原依赖
RUN dotnet restore "ToDoListArea.csproj"

# 复制所有源代码
COPY . .

# 构建应用
RUN dotnet build "ToDoListArea.csproj" -c Release -o /app/build

# 发布应用
FROM build AS publish
RUN dotnet publish "ToDoListArea.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 使用官方.NET 8运行时镜像作为最终镜像
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# 安装健康检查工具
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# 设置工作目录
WORKDIR /app

# 创建非root用户
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app

# 复制发布的应用
COPY --from=publish /app/publish .

# 设置文件权限
RUN chown -R appuser:appuser /app

# 切换到非root用户
USER appuser

# 暴露端口
EXPOSE 5006

# 设置环境变量
ENV ASPNETCORE_URLS=http://+:5006
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# 健康检查
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:5006/health || exit 1

# 启动应用
ENTRYPOINT ["dotnet", "ToDoListArea.dll"]
