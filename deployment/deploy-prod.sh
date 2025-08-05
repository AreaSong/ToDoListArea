#!/bin/bash

# ===========================================
# ToDoListArea Linux生产环境专用部署脚本
# 专为Linux生产环境设计的完整部署方案
# ===========================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
INSTALL_DIR="/opt/todolist"
SERVICE_USER="todolist"

echo "🐧 ToDoListArea Linux生产环境部署脚本"
echo "=================================="

# 检测现有服务并提供建议
detect_existing_services() {
    echo "🔍 检测现有服务..."

    local has_existing_services=false

    # 检测SQL Server
    if systemctl is-active --quiet mssql-server 2>/dev/null || \
       pgrep -f "sqlservr" > /dev/null 2>&1 || \
       netstat -tlnp 2>/dev/null | grep -q ":1433 "; then
        echo "✅ 检测到现有SQL Server服务"
        has_existing_services=true
    fi

    # 检测Nginx
    if systemctl is-active --quiet nginx 2>/dev/null; then
        echo "✅ 检测到现有Nginx服务"
        has_existing_services=true
    fi

    # 检测Docker
    if command -v docker &> /dev/null && systemctl is-active --quiet docker; then
        echo "✅ 检测到现有Docker服务"
        has_existing_services=true
    fi

    if [ "$has_existing_services" = true ]; then
        echo ""
        echo "⚠️  检测到现有服务，建议使用适配脚本："
        echo "   ./Scripts/env-adapter.sh detect"
        echo "   ./Scripts/deploy-existing.sh"
        echo ""
        echo "是否继续使用全新环境部署脚本？(y/N)"
        read -r response
        if [[ ! "$response" =~ ^[Yy]$ ]]; then
            echo "💡 请使用适配现有环境的部署方案"
            exit 0
        fi
    fi
}

# 检查root权限
check_root() {
    if [ "$EUID" -ne 0 ]; then
        echo "❌ 此脚本需要root权限运行"
        echo "💡 请使用: sudo $0"
        exit 1
    fi
}

# 检查Linux发行版
detect_linux_distro() {
    echo "🔍 检测Linux发行版..."
    
    if [ -f /etc/os-release ]; then
        . /etc/os-release
        DISTRO=$ID
        VERSION=$VERSION_ID
        echo "✅ 检测到: $PRETTY_NAME"
    else
        echo "❌ 无法检测Linux发行版"
        exit 1
    fi
}

# 安装系统依赖
install_dependencies() {
    echo "📦 安装系统依赖..."
    
    case $DISTRO in
        ubuntu|debian)
            apt-get update
            apt-get install -y \
                curl \
                wget \
                git \
                openssl \
                logrotate \
                certbot \
                python3-certbot-nginx \
                ufw \
                fail2ban
            ;;
        centos|rhel|fedora)
            if command -v dnf &> /dev/null; then
                dnf update -y
                dnf install -y \
                    curl \
                    wget \
                    git \
                    openssl \
                    logrotate \
                    certbot \
                    python3-certbot-nginx \
                    firewalld \
                    fail2ban
            else
                yum update -y
                yum install -y \
                    curl \
                    wget \
                    git \
                    openssl \
                    logrotate \
                    certbot \
                    python3-certbot-nginx \
                    firewalld \
                    fail2ban
            fi
            ;;
        *)
            echo "⚠️  未知的Linux发行版: $DISTRO"
            echo "💡 请手动安装必要的依赖包"
            ;;
    esac
    
    echo "✅ 系统依赖安装完成"
}

# 安装Docker (检测现有安装)
install_docker() {
    echo "🐳 检查Docker安装..."

    if command -v docker &> /dev/null && systemctl is-active --quiet docker; then
        echo "✅ Docker已安装并运行"

        # 检查Docker Compose
        if command -v docker-compose &> /dev/null; then
            echo "✅ Docker Compose已安装"
        else
            echo "📦 安装Docker Compose..."
            curl -L "https://github.com/docker/compose/releases/download/1.29.2/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
            chmod +x /usr/local/bin/docker-compose
            echo "✅ Docker Compose安装完成"
        fi
        return
    fi

    echo "📦 安装Docker..."
    # 安装Docker官方脚本
    curl -fsSL https://get.docker.com -o get-docker.sh
    sh get-docker.sh
    rm get-docker.sh

    # 启动Docker服务
    systemctl enable docker
    systemctl start docker

    # 安装Docker Compose
    curl -L "https://github.com/docker/compose/releases/download/1.29.2/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
    chmod +x /usr/local/bin/docker-compose

    echo "✅ Docker安装完成"
}

# 创建系统用户
create_system_user() {
    echo "👤 创建系统用户..."
    
    if id "$SERVICE_USER" &>/dev/null; then
        echo "✅ 用户 $SERVICE_USER 已存在"
    else
        useradd -r -s /bin/bash -d $INSTALL_DIR -m $SERVICE_USER
        usermod -aG docker $SERVICE_USER
        echo "✅ 用户 $SERVICE_USER 创建完成"
    fi
}

# 部署应用文件
deploy_application() {
    echo "📁 部署应用文件..."
    
    # 创建安装目录
    mkdir -p $INSTALL_DIR
    
    # 复制项目文件
    rsync -av --exclude='.git' --exclude='node_modules' --exclude='bin' --exclude='obj' \
        "$PROJECT_ROOT/" "$INSTALL_DIR/"
    
    # 设置权限
    chown -R $SERVICE_USER:$SERVICE_USER $INSTALL_DIR
    chmod +x $INSTALL_DIR/Scripts/*.sh
    
    # 创建必要目录
    mkdir -p $INSTALL_DIR/{logs,ssl,backups,data}
    chown -R $SERVICE_USER:$SERVICE_USER $INSTALL_DIR/{logs,ssl,backups,data}
    
    echo "✅ 应用文件部署完成"
}

# 配置防火墙
configure_firewall() {
    echo "🔥 配置防火墙..."
    
    case $DISTRO in
        ubuntu|debian)
            # 使用ufw
            ufw --force enable
            ufw allow ssh
            ufw allow 80/tcp
            ufw allow 443/tcp
            ufw reload
            ;;
        centos|rhel|fedora)
            # 使用firewalld
            systemctl enable firewalld
            systemctl start firewalld
            firewall-cmd --permanent --add-service=ssh
            firewall-cmd --permanent --add-service=http
            firewall-cmd --permanent --add-service=https
            firewall-cmd --reload
            ;;
    esac
    
    echo "✅ 防火墙配置完成"
}

# 配置系统服务
configure_systemd_service() {
    echo "⚙️  配置系统服务..."
    
    # 复制服务文件
    cp "$INSTALL_DIR/Scripts/todolist.service" /etc/systemd/system/
    
    # 修改服务文件中的路径
    sed -i "s|WorkingDirectory=.*|WorkingDirectory=$INSTALL_DIR|g" /etc/systemd/system/todolist.service
    sed -i "s|EnvironmentFile=.*|EnvironmentFile=-$INSTALL_DIR/.env.production|g" /etc/systemd/system/todolist.service
    
    # 重载systemd
    systemctl daemon-reload
    systemctl enable todolist.service
    
    echo "✅ 系统服务配置完成"
}

# 配置SSL证书
configure_ssl() {
    echo "🔐 配置SSL证书..."
    
    cd $INSTALL_DIR
    
    # 切换到服务用户执行
    sudo -u $SERVICE_USER bash -c "
        source .env.production 2>/dev/null || true
        
        if [ -n \"\${LETSENCRYPT_EMAIL:-}\" ] && [ -n \"\${DOMAIN_NAME:-}\" ] && [ \"\$DOMAIN_NAME\" != \"localhost\" ]; then
            echo '🌐 配置Let'\''s Encrypt证书...'
            
            # 临时启动nginx获取证书
            docker-compose up -d frontend
            sleep 10
            
            # 获取证书
            certbot certonly --webroot \
                -w /var/lib/letsencrypt \
                --email \"\$LETSENCRYPT_EMAIL\" \
                --agree-tos \
                --no-eff-email \
                -d \"\$DOMAIN_NAME\" \
                --non-interactive
            
            if [ \$? -eq 0 ]; then
                # 复制证书
                cp \"/etc/letsencrypt/live/\$DOMAIN_NAME/fullchain.pem\" ssl/nginx-selfsigned.crt
                cp \"/etc/letsencrypt/live/\$DOMAIN_NAME/privkey.pem\" ssl/nginx-selfsigned.key
                chmod 644 ssl/nginx-selfsigned.crt
                chmod 600 ssl/nginx-selfsigned.key
                echo '✅ Let'\''s Encrypt证书配置完成'
            else
                echo '⚠️  Let'\''s Encrypt证书获取失败，使用自签名证书'
                ./Scripts/deploy.sh
            fi
            
            docker-compose down
        else
            echo '🔄 使用自签名证书...'
            ./Scripts/deploy.sh
        fi
    "
    
    echo "✅ SSL证书配置完成"
}

# 启动服务
start_services() {
    echo "🚀 启动服务..."
    
    # 启动应用服务
    systemctl start todolist.service
    
    # 检查服务状态
    sleep 10
    if systemctl is-active --quiet todolist.service; then
        echo "✅ 服务启动成功"
    else
        echo "❌ 服务启动失败"
        systemctl status todolist.service
        exit 1
    fi
}

# 显示部署信息
show_deployment_info() {
    echo ""
    echo "🎉 Linux生产环境部署完成！"
    echo "=================================="
    echo "📍 安装位置: $INSTALL_DIR"
    echo "👤 服务用户: $SERVICE_USER"
    echo "🌐 访问地址: https://$(hostname -I | awk '{print $1}')"
    echo ""
    echo "📋 常用命令:"
    echo "   启动服务: sudo systemctl start todolist"
    echo "   停止服务: sudo systemctl stop todolist"
    echo "   重启服务: sudo systemctl restart todolist"
    echo "   查看状态: sudo systemctl status todolist"
    echo "   查看日志: sudo journalctl -u todolist -f"
    echo ""
    echo "📁 重要目录:"
    echo "   应用目录: $INSTALL_DIR"
    echo "   日志目录: $INSTALL_DIR/logs"
    echo "   SSL证书: $INSTALL_DIR/ssl"
    echo "   备份目录: $INSTALL_DIR/backups"
    echo ""
    echo "🔧 配置文件:"
    echo "   环境变量: $INSTALL_DIR/.env.production"
    echo "   系统服务: /etc/systemd/system/todolist.service"
    echo ""
    echo "💡 提示:"
    echo "   - 请确保 $INSTALL_DIR/.env.production 配置正确"
    echo "   - 生产环境建议使用有效的SSL证书"
    echo "   - 定期备份数据库和重要文件"
}

# 主函数
main() {
    check_root
    detect_existing_services
    detect_linux_distro
    install_dependencies
    install_docker
    create_system_user
    deploy_application
    configure_firewall
    configure_systemd_service
    configure_ssl
    start_services
    show_deployment_info
}

# 执行主函数
main "$@"
