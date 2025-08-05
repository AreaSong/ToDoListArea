@echo off
REM ToDoListArea项目部署脚本 (Windows版本)
REM 用于生产环境的自动化部署

echo 🚀 开始部署ToDoListArea项目...

REM 检查Docker是否安装
docker --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Docker未安装，请先安装Docker Desktop
    pause
    exit /b 1
)

REM 检查Docker Compose是否安装
docker-compose --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Docker Compose未安装，请先安装Docker Compose
    pause
    exit /b 1
)

echo ✅ 环境检查通过

REM 创建必要的目录
echo 📁 创建必要的目录...
if not exist "logs" mkdir logs
if not exist "ssl" mkdir ssl
if not exist "backups" mkdir backups
echo ✅ 目录创建完成

REM 检查环境变量文件
echo 🔧 检查环境配置...
if not exist ".env" (
    if exist ".env.production" (
        echo 📋 使用生产环境配置文件
        copy ".env.production" ".env"
    ) else if exist ".env.example" (
        echo 📋 使用示例配置文件，请修改其中的敏感信息
        copy ".env.example" ".env"
        echo ⚠️  警告：请修改.env文件中的敏感信息（数据库密码、JWT密钥等）
        set /p continue="是否继续部署？(y/N): "
        if /i not "%continue%"=="y" (
            echo ❌ 部署已取消
            pause
            exit /b 1
        )
    ) else (
        echo ❌ 未找到环境配置文件，请创建.env文件
        pause
        exit /b 1
    )
)
echo ✅ 环境配置检查完成

REM 生成SSL证书（自签名，仅用于测试）
echo 🔐 生成SSL证书...
if not exist "ssl\nginx-selfsigned.crt" (
    echo 正在生成自签名SSL证书...
    REM 在Windows上，需要安装OpenSSL或使用PowerShell的New-SelfSignedCertificate
    echo 请手动生成SSL证书或跳过HTTPS配置
)
echo ✅ SSL证书检查完成

REM 停止现有服务
echo 🛑 停止现有服务...
docker-compose down --remove-orphans

REM 构建镜像
echo 🔨 构建Docker镜像...
docker-compose build --no-cache

REM 启动服务
echo 🚀 启动服务...
docker-compose up -d

echo ✅ 服务启动完成

REM 等待服务就绪
echo ⏳ 等待服务就绪...
timeout /t 30 /nobreak >nul

REM 检查服务状态
echo 🏥 检查服务状态...
docker-compose ps

echo.
echo 🎉 部署完成！服务访问地址：
echo    前端应用: http://localhost
echo    前端应用(HTTPS): https://localhost
echo    后端API: http://localhost:5006
echo    数据库: localhost:1433
echo.
echo 📋 常用命令：
echo    查看日志: docker-compose logs -f
echo    停止服务: docker-compose down
echo    重启服务: docker-compose restart

pause
