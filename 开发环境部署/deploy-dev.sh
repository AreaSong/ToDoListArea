#!/bin/bash
# ToDoListArea开发环境部署脚本 (Linux/macOS版本)
# 用于本地开发环境的快速部署和调试

echo "🚀 开始部署ToDoListArea开发环境..."

# 检查Docker是否安装
if ! command -v docker &> /dev/null; then
    echo "❌ Docker未安装，请先安装Docker"
    exit 1
fi

# 检查Docker Compose是否安装
if ! command -v docker-compose &> /dev/null; then
    echo "❌ Docker Compose未安装，请先安装Docker Compose"
    exit 1
fi

echo "✅ 环境检查通过"

# 设置开发环境变量
echo "🔧 设置开发环境配置..."
export COMPOSE_FILE=docker-compose.dev.yml
export ASPNETCORE_ENVIRONMENT=Development
export NODE_ENV=development

# 创建开发环境配置文件
if [ ! -f ".env.dev" ]; then
    echo "📋 创建开发环境配置文件..."
    cat > .env.dev << EOF
# ToDoListArea开发环境配置
DB_SA_PASSWORD=TodoList@2024!Dev
JWT_SECRET_KEY=dev-jwt-secret-key-for-development-only-not-secure-12345678
JWT_ISSUER=ToDoListArea-Dev
JWT_AUDIENCE=ToDoListArea-Dev-Users
JWT_EXPIRATION=120
ASPNETCORE_ENVIRONMENT=Development
NODE_ENV=development
EOF
    echo "✅ 开发环境配置文件创建完成"
fi

# 复制配置文件
cp .env.dev .env

echo "✅ 开发环境配置完成"

# 停止现有服务
echo "🛑 停止现有开发服务..."
docker-compose -f docker-compose.dev.yml down --remove-orphans

# 清理开发环境数据（可选）
read -p "是否清理开发环境数据？(y/N): " clean_data
if [[ $clean_data =~ ^[Yy]$ ]]; then
    echo "🧹 清理开发环境数据..."
    docker volume rm todolistarea_db_data_dev 2>/dev/null || true
    docker volume rm todolistarea_redis_data_dev 2>/dev/null || true
    echo "✅ 开发环境数据清理完成"
fi

# 构建开发镜像
echo "🔨 构建开发环境Docker镜像..."
docker-compose -f docker-compose.dev.yml build --no-cache

# 启动开发服务
echo "🚀 启动开发环境服务..."
docker-compose -f docker-compose.dev.yml up -d

echo "✅ 开发环境服务启动完成"

# 等待服务就绪
echo "⏳ 等待开发环境服务就绪..."
sleep 45

# 检查服务状态
echo "🏥 检查开发环境服务状态..."
docker-compose -f docker-compose.dev.yml ps

echo ""
echo "🎉 开发环境部署完成！服务访问地址："
echo "   前端开发服务器: http://localhost:5175 (支持热重载)"
echo "   后端API: http://localhost:5006"
echo "   数据库: localhost:1433 (sa/TodoList@2024!Dev)"
echo "   Redis: localhost:6379"
echo "   数据库管理: http://localhost:8080 (Adminer)"
echo ""
echo "📋 开发环境常用命令："
echo "   查看日志: docker-compose -f docker-compose.dev.yml logs -f"
echo "   停止服务: docker-compose -f docker-compose.dev.yml down"
echo "   重启服务: docker-compose -f docker-compose.dev.yml restart"
echo "   进入容器: docker exec -it todolist-backend-dev bash"
echo ""
echo "🔧 开发提示："
echo "   - 前端代码修改会自动热重载"
echo "   - 后端代码修改需要重启容器"
echo "   - 数据库数据会持久化保存"
echo "   - 使用Adminer可以方便地管理数据库"

# 设置脚本可执行权限
chmod +x deploy-dev.sh
