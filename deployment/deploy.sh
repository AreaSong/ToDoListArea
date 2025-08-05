#!/bin/bash

# ToDoListArea项目部署脚本
# 用于生产环境的自动化部署

set -e  # 遇到错误立即退出

echo "🚀 开始部署ToDoListArea项目..."

# 检查必要的工具
check_requirements() {
    echo "📋 检查Linux生产环境..."

    # 检查操作系统
    if [[ "$OSTYPE" != "linux-gnu"* ]]; then
        echo "⚠️  警告：当前不是Linux环境，生产部署建议使用Linux"
    fi

    # 检查Docker
    if ! command -v docker &> /dev/null; then
        echo "❌ Docker未安装"
        echo "💡 Ubuntu/Debian安装: sudo apt-get update && sudo apt-get install docker.io"
        echo "💡 CentOS/RHEL安装: sudo yum install docker"
        exit 1
    fi

    # 检查Docker Compose
    if ! command -v docker-compose &> /dev/null; then
        echo "❌ Docker Compose未安装"
        echo "💡 安装命令: sudo curl -L \"https://github.com/docker/compose/releases/download/1.29.2/docker-compose-\$(uname -s)-\$(uname -m)\" -o /usr/local/bin/docker-compose"
        echo "💡 设置权限: sudo chmod +x /usr/local/bin/docker-compose"
        exit 1
    fi

    # 检查Docker服务状态
    if ! systemctl is-active --quiet docker; then
        echo "⚠️  Docker服务未运行，尝试启动..."
        sudo systemctl start docker
        if ! systemctl is-active --quiet docker; then
            echo "❌ Docker服务启动失败"
            exit 1
        fi
    fi

    # 检查用户权限
    if ! groups $USER | grep -q docker; then
        echo "⚠️  当前用户不在docker组中"
        echo "💡 添加用户到docker组: sudo usermod -aG docker $USER"
        echo "💡 然后重新登录或运行: newgrp docker"
    fi

    # 检查系统资源
    local mem_gb=$(free -g | awk '/^Mem:/{print $2}')
    if [ "$mem_gb" -lt 2 ]; then
        echo "⚠️  系统内存不足2GB，可能影响部署性能"
    fi

    local disk_gb=$(df -BG . | awk 'NR==2{print $4}' | sed 's/G//')
    if [ "$disk_gb" -lt 10 ]; then
        echo "⚠️  磁盘可用空间不足10GB，可能影响部署"
    fi

    echo "✅ Linux生产环境检查通过"
}

# 创建必要的目录和Linux系统优化
create_directories() {
    echo "📁 创建必要的目录..."

    # 创建基础目录
    mkdir -p logs/nginx
    mkdir -p ssl
    mkdir -p backups
    mkdir -p data/db
    mkdir -p data/redis

    # 设置正确的权限（Linux生产环境）
    chmod 755 logs
    chmod 755 logs/nginx
    chmod 700 ssl
    chmod 755 backups
    chmod 755 data

    # 创建日志轮转配置（Linux特性）
    if command -v logrotate &> /dev/null; then
        echo "📋 配置日志轮转..."
        sudo tee /etc/logrotate.d/todolist > /dev/null <<EOF
$(pwd)/logs/nginx/*.log {
    daily
    missingok
    rotate 30
    compress
    delaycompress
    notifempty
    create 644 root root
    postrotate
        docker exec todolist-frontend nginx -s reload 2>/dev/null || true
    endscript
}
EOF
        echo "✅ 日志轮转配置完成"
    fi

    # 优化系统参数（Linux生产环境）
    echo "⚡ 优化Linux系统参数..."

    # 临时优化（重启后失效）
    echo "net.core.somaxconn = 65535" | sudo tee -a /etc/sysctl.conf > /dev/null
    echo "net.ipv4.tcp_max_syn_backlog = 65535" | sudo tee -a /etc/sysctl.conf > /dev/null
    echo "fs.file-max = 100000" | sudo tee -a /etc/sysctl.conf > /dev/null

    # 应用系统参数
    sudo sysctl -p > /dev/null 2>&1 || echo "⚠️  系统参数优化需要root权限"

    echo "✅ 目录创建和系统优化完成"
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

# SSL证书管理（Linux生产环境优化）
generate_ssl_cert() {
    echo "🔐 SSL证书管理..."

    # 检查是否使用Let's Encrypt
    if [ -n "${LETSENCRYPT_EMAIL:-}" ] && [ -n "${DOMAIN_NAME:-}" ] && [ "$DOMAIN_NAME" != "localhost" ]; then
        echo "🌐 检测到Let's Encrypt配置，尝试获取免费SSL证书..."

        # 检查certbot是否安装
        if command -v certbot &> /dev/null; then
            echo "📋 使用Let's Encrypt获取SSL证书..."

            # 停止可能占用80端口的服务
            docker-compose down > /dev/null 2>&1 || true

            # 获取证书
            sudo certbot certonly --standalone \
                --email "${LETSENCRYPT_EMAIL}" \
                --agree-tos \
                --no-eff-email \
                -d "${DOMAIN_NAME}" \
                --non-interactive

            if [ $? -eq 0 ]; then
                # 复制证书到项目目录
                sudo cp "/etc/letsencrypt/live/${DOMAIN_NAME}/fullchain.pem" ssl/nginx-selfsigned.crt
                sudo cp "/etc/letsencrypt/live/${DOMAIN_NAME}/privkey.pem" ssl/nginx-selfsigned.key

                # 设置权限
                sudo chown $USER:$USER ssl/nginx-selfsigned.*
                chmod 644 ssl/nginx-selfsigned.crt
                chmod 600 ssl/nginx-selfsigned.key

                echo "✅ Let's Encrypt SSL证书获取成功"

                # 设置自动续期
                setup_cert_renewal
            else
                echo "❌ Let's Encrypt证书获取失败，回退到自签名证书"
                generate_self_signed_cert
            fi
        else
            echo "⚠️  certbot未安装，无法获取Let's Encrypt证书"
            echo "💡 安装certbot: sudo apt-get install certbot (Ubuntu/Debian)"
            echo "💡 安装certbot: sudo yum install certbot (CentOS/RHEL)"
            echo "🔄 回退到自签名证书..."
            generate_self_signed_cert
        fi
    else
        echo "🔄 使用自签名证书（测试环境）..."
        generate_self_signed_cert
    fi
}

# 生成自签名证书
generate_self_signed_cert() {
    if [ ! -f ssl/nginx-selfsigned.crt ]; then
        echo "📋 生成自签名SSL证书..."

        # 创建证书配置文件
        cat > ssl/cert.conf <<EOF
[req]
default_bits = 2048
prompt = no
default_md = sha256
distinguished_name = dn
req_extensions = v3_req

[dn]
C=CN
ST=Beijing
L=Beijing
O=ToDoListArea
CN=${DOMAIN_NAME:-localhost}

[v3_req]
basicConstraints = CA:FALSE
keyUsage = nonRepudiation, digitalSignature, keyEncipherment
subjectAltName = @alt_names

[alt_names]
DNS.1 = ${DOMAIN_NAME:-localhost}
DNS.2 = www.${DOMAIN_NAME:-localhost}
IP.1 = 127.0.0.1
EOF

        # 生成证书
        openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
            -keyout ssl/nginx-selfsigned.key \
            -out ssl/nginx-selfsigned.crt \
            -config ssl/cert.conf \
            -extensions v3_req

        # 设置权限
        chmod 600 ssl/nginx-selfsigned.key
        chmod 644 ssl/nginx-selfsigned.crt
        rm ssl/cert.conf

        echo "✅ 自签名SSL证书生成完成"
        echo "⚠️  注意：这是自签名证书，仅用于测试环境"
        echo "💡 生产环境建议使用Let's Encrypt或商业证书"
    else
        echo "✅ SSL证书已存在"

        # 检查证书有效期
        if openssl x509 -checkend 86400 -noout -in ssl/nginx-selfsigned.crt; then
            echo "✅ SSL证书有效期正常"
        else
            echo "⚠️  SSL证书即将过期，建议更新"
        fi
    fi
}

# 设置证书自动续期（Let's Encrypt）
setup_cert_renewal() {
    echo "🔄 设置SSL证书自动续期..."

    # 创建续期脚本
    cat > /tmp/renew-cert.sh <<EOF
#!/bin/bash
# SSL证书自动续期脚本

cd $(pwd)

# 续期证书
certbot renew --quiet

# 如果证书更新了，重新复制到项目目录
if [ -f "/etc/letsencrypt/live/${DOMAIN_NAME}/fullchain.pem" ]; then
    cp "/etc/letsencrypt/live/${DOMAIN_NAME}/fullchain.pem" ssl/nginx-selfsigned.crt
    cp "/etc/letsencrypt/live/${DOMAIN_NAME}/privkey.pem" ssl/nginx-selfsigned.key

    # 重启nginx
    docker-compose restart frontend
fi
EOF

    # 安装续期脚本
    sudo mv /tmp/renew-cert.sh /usr/local/bin/todolist-renew-cert.sh
    sudo chmod +x /usr/local/bin/todolist-renew-cert.sh

    # 添加到crontab（每天检查一次）
    (crontab -l 2>/dev/null; echo "0 2 * * * /usr/local/bin/todolist-renew-cert.sh") | crontab -

    echo "✅ SSL证书自动续期设置完成"
}

# 构建和启动服务
deploy_services() {
    echo "🏗️  构建和启动服务..."

    # 停止现有服务
    echo "🛑 停止现有服务..."
    docker-compose down --remove-orphans

    # 清理未使用的镜像（可选）
    echo "🧹 清理Docker缓存..."
    docker system prune -f

    # 构建镜像
    echo "🔨 构建Docker镜像..."
    echo "   - 构建后端API镜像..."
    docker-compose build --no-cache backend
    echo "   - 构建前端Nginx镜像..."
    docker-compose build --no-cache frontend

    # 启动服务
    echo "🚀 启动服务..."
    docker-compose up -d

    echo "✅ 服务启动完成"

    # 显示镜像信息
    echo "📋 构建的镜像信息："
    docker images | grep todolist
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
