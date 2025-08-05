#!/bin/bash

# ===========================================
# ToDoListArea 现有环境适配部署脚本
# 专为已有SQL Server、Nginx、Docker的环境设计
# ===========================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

echo "🔧 ToDoListArea 现有环境适配部署"
echo "=================================="

# 检测现有服务
check_existing_services() {
    echo "🔍 检测现有服务..."
    
    # 检查SQL Server
    if ! (systemctl is-active --quiet mssql-server 2>/dev/null || \
          pgrep -f "sqlservr" > /dev/null 2>&1 || \
          netstat -tlnp 2>/dev/null | grep -q ":1433 "); then
        echo "❌ 未检测到SQL Server，请确保SQL Server正在运行"
        exit 1
    fi
    echo "✅ SQL Server检测通过"
    
    # 检查Docker
    if ! (command -v docker &> /dev/null && systemctl is-active --quiet docker); then
        echo "❌ Docker服务未运行，请启动Docker服务"
        exit 1
    fi
    echo "✅ Docker服务检测通过"
    
    # 检查Docker Compose
    if ! command -v docker-compose &> /dev/null; then
        echo "❌ Docker Compose未安装"
        exit 1
    fi
    echo "✅ Docker Compose检测通过"
    
    # 检查Nginx (可选)
    if systemctl is-active --quiet nginx 2>/dev/null; then
        echo "✅ 检测到现有Nginx服务"
        EXISTING_NGINX=true
    else
        echo "ℹ️  未检测到Nginx服务，将使用容器化Nginx"
        EXISTING_NGINX=false
    fi
}

# 准备环境配置
prepare_environment() {
    echo "📁 准备环境配置..."
    
    # 创建必要目录
    mkdir -p logs/nginx
    mkdir -p ssl
    mkdir -p backups
    
    # 设置权限
    chmod 755 logs
    chmod 755 logs/nginx
    chmod 700 ssl
    chmod 755 backups
    
    # 检查环境变量文件
    if [ ! -f ".env.production" ]; then
        echo "⚠️  .env.production文件不存在，从示例文件创建"
        cp .env.example .env.production
        echo "📝 请编辑 .env.production 文件配置数据库密码等信息"
        echo "⏸️  按任意键继续..."
        read -n 1 -s
    fi
    
    echo "✅ 环境准备完成"
}

# 配置数据库连接
configure_database() {
    echo "🗄️  配置数据库连接..."
    
    # 测试SQL Server连接
    source .env.production
    
    echo "🔍 测试SQL Server连接..."
    if command -v sqlcmd &> /dev/null; then
        if sqlcmd -S localhost -U sa -P "$DB_SA_PASSWORD" -Q "SELECT 1" > /dev/null 2>&1; then
            echo "✅ SQL Server连接测试成功"
        else
            echo "❌ SQL Server连接失败，请检查密码配置"
            exit 1
        fi
    else
        echo "⚠️  sqlcmd未安装，跳过连接测试"
    fi
    
    # 检查数据库是否存在
    echo "🔍 检查ToDoListArea数据库..."
    if command -v sqlcmd &> /dev/null; then
        if sqlcmd -S localhost -U sa -P "$DB_SA_PASSWORD" -Q "SELECT name FROM sys.databases WHERE name = 'ToDoListArea'" -h -1 | grep -q "ToDoListArea"; then
            echo "✅ ToDoListArea数据库已存在"
        else
            echo "📝 创建ToDoListArea数据库..."
            sqlcmd -S localhost -U sa -P "$DB_SA_PASSWORD" -Q "CREATE DATABASE ToDoListArea"
            echo "✅ 数据库创建完成"
        fi
    fi
}

# 配置Nginx集成
configure_nginx_integration() {
    if [ "$EXISTING_NGINX" = true ]; then
        echo "🌐 配置Nginx集成..."
        
        # 生成Nginx配置片段
        cat > /tmp/todolist-nginx.conf <<EOF
# ToDoListArea 应用配置
server {
    listen 80;
    server_name ${DOMAIN_NAME:-localhost};
    
    # 重定向到HTTPS
    return 301 https://\$server_name\$request_uri;
}

server {
    listen 443 ssl http2;
    server_name ${DOMAIN_NAME:-localhost};
    
    # SSL配置 (请根据实际情况调整证书路径)
    ssl_certificate /etc/ssl/certs/your-cert.crt;
    ssl_certificate_key /etc/ssl/private/your-key.key;
    
    # SSL安全配置
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers ECDHE-RSA-AES256-GCM-SHA512:DHE-RSA-AES256-GCM-SHA512:ECDHE-RSA-AES256-GCM-SHA384:DHE-RSA-AES256-GCM-SHA384;
    ssl_prefer_server_ciphers off;
    
    # 静态文件代理到前端容器
    location / {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_cache_bypass \$http_upgrade;
    }
    
    # API代理到后端容器
    location /api/ {
        proxy_pass http://localhost:5006/;
        proxy_http_version 1.1;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        
        # 超时配置
        proxy_connect_timeout 30s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
    }
    
    # 健康检查
    location /health {
        proxy_pass http://localhost:8080/health;
        access_log off;
    }
}
EOF
        
        echo "📝 Nginx配置已生成: /tmp/todolist-nginx.conf"
        echo "⚠️  请手动将此配置合并到现有Nginx配置中"
        echo "💡 建议操作："
        echo "   1. sudo cp /tmp/todolist-nginx.conf /etc/nginx/sites-available/todolist"
        echo "   2. sudo ln -s /etc/nginx/sites-available/todolist /etc/nginx/sites-enabled/"
        echo "   3. sudo nginx -t"
        echo "   4. sudo systemctl reload nginx"
        echo ""
        echo "⏸️  配置完成后按任意键继续..."
        read -n 1 -s
    fi
}

# 部署应用
deploy_application() {
    echo "🚀 部署ToDoListArea应用..."
    
    # 使用适配现有环境的配置
    if [ ! -f "docker-compose.existing.yml" ]; then
        echo "❌ docker-compose.existing.yml文件不存在"
        exit 1
    fi
    
    # 停止现有容器
    echo "🛑 停止现有容器..."
    docker-compose -f docker-compose.existing.yml down --remove-orphans 2>/dev/null || true
    
    # 构建和启动服务
    echo "🔨 构建Docker镜像..."
    docker-compose -f docker-compose.existing.yml build --no-cache
    
    echo "🚀 启动服务..."
    docker-compose -f docker-compose.existing.yml up -d
    
    # 等待服务启动
    echo "⏳ 等待服务启动..."
    sleep 30
    
    # 检查服务状态
    echo "🔍 检查服务状态..."
    docker-compose -f docker-compose.existing.yml ps
}

# 初始化数据库
initialize_database() {
    echo "🗄️  初始化数据库..."
    
    # 等待后端服务完全启动
    echo "⏳ 等待后端服务启动..."
    for i in {1..30}; do
        if curl -f http://localhost:5006/health > /dev/null 2>&1; then
            echo "✅ 后端服务已启动"
            break
        fi
        echo "⏳ 等待中... ($i/30)"
        sleep 2
    done
    
    # 运行数据库迁移
    echo "🔄 运行数据库迁移..."
    if [ -f "Scripts/init-db.sql" ]; then
        docker exec -i todolist-backend bash -c "
            if command -v sqlcmd &> /dev/null; then
                sqlcmd -S host.docker.internal -U sa -P '$DB_SA_PASSWORD' -d ToDoListArea -i /app/Scripts/init-db.sql
            else
                echo '⚠️  sqlcmd不可用，请手动执行数据库初始化'
            fi
        " 2>/dev/null || echo "⚠️  数据库初始化可能需要手动执行"
    fi
}

# 验证部署
verify_deployment() {
    echo "✅ 验证部署结果..."
    
    # 检查容器状态
    echo "📊 容器状态："
    docker-compose -f docker-compose.existing.yml ps
    
    # 检查服务健康状态
    echo "🔍 服务健康检查："
    
    # 检查后端API
    if curl -f http://localhost:5006/health > /dev/null 2>&1; then
        echo "✅ 后端API服务正常"
    else
        echo "❌ 后端API服务异常"
    fi
    
    # 检查前端服务
    if curl -f http://localhost:8080/health > /dev/null 2>&1; then
        echo "✅ 前端服务正常"
    else
        echo "❌ 前端服务异常"
    fi
    
    echo ""
    echo "🎉 部署完成！"
    echo "=================="
    echo "📍 服务访问地址："
    if [ "$EXISTING_NGINX" = true ]; then
        echo "   🌐 前端应用: https://${DOMAIN_NAME:-localhost}"
        echo "   🔧 后端API: https://${DOMAIN_NAME:-localhost}/api"
    else
        echo "   🌐 前端应用: http://localhost:8080"
        echo "   🔧 后端API: http://localhost:5006"
    fi
    echo ""
    echo "📋 管理命令："
    echo "   查看日志: docker-compose -f docker-compose.existing.yml logs -f"
    echo "   重启服务: docker-compose -f docker-compose.existing.yml restart"
    echo "   停止服务: docker-compose -f docker-compose.existing.yml down"
}

# 主函数
main() {
    check_existing_services
    prepare_environment
    configure_database
    configure_nginx_integration
    deploy_application
    initialize_database
    verify_deployment
}

# 执行主函数
main "$@"
