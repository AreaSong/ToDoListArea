#!/bin/bash

# ToDoListArea项目部署脚本
# 用于生产环境的自动化部署

set -e  # 遇到错误立即退出

echo "🚀 开始部署ToDoListArea项目..."

# 检查必要的工具
check_requirements() {
    echo "📋 检查部署环境..."
    
    if ! command -v docker &> /dev/null; then
        echo "❌ Docker未安装，请先安装Docker"
        exit 1
    fi
    
    if ! command -v docker-compose &> /dev/null; then
        echo "❌ Docker Compose未安装，请先安装Docker Compose"
        exit 1
    fi
    
    echo "✅ 环境检查通过"
}

# 创建必要的目录
create_directories() {
    echo "📁 创建必要的目录..."
    mkdir -p logs
    mkdir -p ssl
    mkdir -p backups
    echo "✅ 目录创建完成"
}

# 检查环境变量文件
check_env_file() {
    echo "🔧 检查环境配置..."
    
    if [ ! -f .env ]; then
        if [ -f .env.production ]; then
            echo "📋 使用生产环境配置文件"
            cp .env.production .env
        elif [ -f .env.example ]; then
            echo "📋 使用示例配置文件，请修改其中的敏感信息"
            cp .env.example .env
            echo "⚠️  警告：请修改.env文件中的敏感信息（数据库密码、JWT密钥等）"
            read -p "是否继续部署？(y/N): " -n 1 -r
            echo
            if [[ ! $REPLY =~ ^[Yy]$ ]]; then
                echo "❌ 部署已取消"
                exit 1
            fi
        else
            echo "❌ 未找到环境配置文件，请创建.env文件"
            exit 1
        fi
    fi
    
    echo "✅ 环境配置检查完成"
}

# 生成SSL证书（自签名，仅用于测试）
generate_ssl_cert() {
    echo "🔐 生成SSL证书..."
    
    if [ ! -f ssl/nginx-selfsigned.crt ]; then
        openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
            -keyout ssl/nginx-selfsigned.key \
            -out ssl/nginx-selfsigned.crt \
            -subj "/C=CN/ST=Beijing/L=Beijing/O=ToDoListArea/CN=localhost"
        echo "✅ SSL证书生成完成"
    else
        echo "✅ SSL证书已存在"
    fi
}

# 构建和启动服务
deploy_services() {
    echo "🏗️  构建和启动服务..."
    
    # 停止现有服务
    echo "🛑 停止现有服务..."
    docker-compose down --remove-orphans
    
    # 构建镜像
    echo "🔨 构建Docker镜像..."
    docker-compose build --no-cache
    
    # 启动服务
    echo "🚀 启动服务..."
    docker-compose up -d
    
    echo "✅ 服务启动完成"
}

# 等待服务就绪
wait_for_services() {
    echo "⏳ 等待服务就绪..."
    
    # 等待数据库就绪
    echo "📊 等待数据库启动..."
    timeout=60
    while [ $timeout -gt 0 ]; do
        if docker-compose exec -T database /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "${DB_SA_PASSWORD:-TodoList@2024!}" -Q "SELECT 1" &> /dev/null; then
            echo "✅ 数据库已就绪"
            break
        fi
        sleep 2
        timeout=$((timeout-2))
    done
    
    if [ $timeout -le 0 ]; then
        echo "❌ 数据库启动超时"
        exit 1
    fi
    
    # 等待后端API就绪
    echo "🔧 等待后端API启动..."
    timeout=60
    while [ $timeout -gt 0 ]; do
        if curl -f http://localhost:5006/health &> /dev/null; then
            echo "✅ 后端API已就绪"
            break
        fi
        sleep 2
        timeout=$((timeout-2))
    done
    
    if [ $timeout -le 0 ]; then
        echo "❌ 后端API启动超时"
        exit 1
    fi
    
    # 等待前端就绪
    echo "🌐 等待前端启动..."
    timeout=30
    while [ $timeout -gt 0 ]; do
        if curl -f http://localhost/health &> /dev/null; then
            echo "✅ 前端已就绪"
            break
        fi
        sleep 2
        timeout=$((timeout-2))
    done
    
    if [ $timeout -le 0 ]; then
        echo "❌ 前端启动超时"
        exit 1
    fi
}

# 运行健康检查
health_check() {
    echo "🏥 运行健康检查..."
    
    # 检查所有服务状态
    if docker-compose ps | grep -q "Up"; then
        echo "✅ 所有服务运行正常"
        
        # 显示服务访问地址
        echo ""
        echo "🎉 部署成功！服务访问地址："
        echo "   前端应用: http://localhost"
        echo "   前端应用(HTTPS): https://localhost"
        echo "   后端API: http://localhost:5006"
        echo "   数据库: localhost:1433"
        echo ""
        echo "📋 服务状态："
        docker-compose ps
        
    else
        echo "❌ 部分服务启动失败"
        docker-compose ps
        exit 1
    fi
}

# 主函数
main() {
    echo "🎯 ToDoListArea项目自动化部署脚本"
    echo "=================================="
    
    check_requirements
    create_directories
    check_env_file
    generate_ssl_cert
    deploy_services
    wait_for_services
    health_check
    
    echo ""
    echo "🎉 部署完成！"
    echo "💡 提示：首次部署后，请访问应用并创建管理员账户"
    echo "📝 日志查看：docker-compose logs -f"
    echo "🛑 停止服务：docker-compose down"
}

# 执行主函数
main "$@"
