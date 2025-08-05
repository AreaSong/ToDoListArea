@echo off
REM ToDoListArea开发环境部署脚本 (Windows版本)
REM 用于本地开发环境的快速部署和调试

echo 🚀 开始部署ToDoListArea开发环境...

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

REM 设置开发环境变量
echo 🔧 设置开发环境配置...
set COMPOSE_FILE=docker-compose.dev.yml
set ASPNETCORE_ENVIRONMENT=Development
set NODE_ENV=development

REM 创建开发环境配置文件
if not exist ".env.dev" (
    echo 📋 创建开发环境配置文件...
    (
        echo # ToDoListArea开发环境配置
        echo DB_SA_PASSWORD=TodoList@2024!Dev
        echo JWT_SECRET_KEY=dev-jwt-secret-key-for-development-only-not-secure-12345678
        echo JWT_ISSUER=ToDoListArea-Dev
        echo JWT_AUDIENCE=ToDoListArea-Dev-Users
        echo JWT_EXPIRATION=120
        echo ASPNETCORE_ENVIRONMENT=Development
        echo NODE_ENV=development
    ) > .env.dev
    echo ✅ 开发环境配置文件创建完成
)

REM 复制配置文件
copy .env.dev .env >nul 2>&1

echo ✅ 开发环境配置完成

REM 停止现有服务
echo 🛑 停止现有开发服务...
docker-compose -f docker-compose.dev.yml down --remove-orphans

REM 清理开发环境数据（可选）
set /p clean_data="是否清理开发环境数据？(y/N): "
if /i "%clean_data%"=="y" (
    echo 🧹 清理开发环境数据...
    docker volume rm todolistarea_db_data_dev 2>nul
    docker volume rm todolistarea_redis_data_dev 2>nul
    echo ✅ 开发环境数据清理完成
)

REM 构建开发镜像
echo 🔨 构建开发环境Docker镜像...
docker-compose -f docker-compose.dev.yml build --no-cache

REM 启动开发服务
echo 🚀 启动开发环境服务...
docker-compose -f docker-compose.dev.yml up -d

echo ✅ 开发环境服务启动完成

REM 等待服务就绪
echo ⏳ 等待开发环境服务就绪...
timeout /t 45 /nobreak >nul

REM 检查服务状态
echo 🏥 检查开发环境服务状态...
docker-compose -f docker-compose.dev.yml ps

echo.
echo 🎉 开发环境部署完成！服务访问地址：
echo    前端开发服务器: http://localhost:5175 (支持热重载)
echo    后端API: http://localhost:5006
echo    数据库: localhost:1433 (sa/TodoList@2024!Dev)
echo    Redis: localhost:6379
echo    数据库管理: http://localhost:8080 (Adminer)
echo.
echo 📋 开发环境常用命令：
echo    查看日志: docker-compose -f docker-compose.dev.yml logs -f
echo    停止服务: docker-compose -f docker-compose.dev.yml down
echo    重启服务: docker-compose -f docker-compose.dev.yml restart
echo    进入容器: docker exec -it todolist-backend-dev bash
echo.
echo 🔧 开发提示：
echo    - 前端代码修改会自动热重载
echo    - 后端代码修改需要重启容器
echo    - 数据库数据会持久化保存
echo    - 使用Adminer可以方便地管理数据库

pause
