#!/bin/bash

# ===========================================
# 环境适配检测脚本
# 检测现有服务并提供适配建议
# ===========================================

set -e

echo "🔍 ToDoListArea 环境适配检测工具"

# 检测现有服务
detect_existing_services() {
    echo "📊 检测现有服务..."
    
    local services_status=()
    
    # 检测SQL Server
    if systemctl is-active --quiet mssql-server 2>/dev/null || \
       pgrep -f "sqlservr" > /dev/null 2>&1 || \
       netstat -tlnp 2>/dev/null | grep -q ":1433 "; then
        echo "✅ 检测到SQL Server (端口1433)"
        services_status+=("sqlserver:installed")
    else
        echo "❌ 未检测到SQL Server"
        services_status+=("sqlserver:missing")
    fi
    
    # 检测Docker
    if command -v docker &> /dev/null && systemctl is-active --quiet docker; then
        echo "✅ 检测到Docker服务"
        services_status+=("docker:installed")
        
        # 检测Docker Compose
        if command -v docker-compose &> /dev/null; then
            echo "✅ 检测到Docker Compose"
            services_status+=("docker-compose:installed")
        else
            echo "⚠️  Docker Compose未安装"
            services_status+=("docker-compose:missing")
        fi
    else
        echo "❌ 未检测到Docker服务"
        services_status+=("docker:missing")
    fi
    
    # 检测Nginx
    if command -v nginx &> /dev/null && systemctl is-active --quiet nginx; then
        echo "✅ 检测到Nginx服务"
        services_status+=("nginx:installed")
        
        # 检查Nginx配置
        local nginx_config=$(nginx -T 2>/dev/null | grep -E "listen.*80|listen.*443" | wc -l)
        if [ "$nginx_config" -gt 0 ]; then
            echo "⚠️  Nginx已占用80/443端口"
            services_status+=("nginx:port-conflict")
        fi
    else
        echo "❌ 未检测到Nginx服务"
        services_status+=("nginx:missing")
    fi
    
    # 检测端口占用
    echo "🔍 检测端口占用情况..."
    check_port_usage 80 "HTTP"
    check_port_usage 443 "HTTPS"
    check_port_usage 1433 "SQL Server"
    check_port_usage 5006 "后端API"
    check_port_usage 6379 "Redis"
    
    # 返回检测结果
    printf '%s\n' "${services_status[@]}"
}

# 检查端口使用情况
check_port_usage() {
    local port=$1
    local service_name=$2
    
    if netstat -tlnp 2>/dev/null | grep -q ":$port "; then
        local process=$(netstat -tlnp 2>/dev/null | grep ":$port " | awk '{print $7}' | head -1)
        echo "⚠️  端口 $port ($service_name) 已被占用: $process"
        return 1
    else
        echo "✅ 端口 $port ($service_name) 可用"
        return 0
    fi
}

# 生成适配建议
generate_adaptation_advice() {
    local services=("$@")
    
    echo ""
    echo "📋 环境适配建议："
    echo "=================="
    
    local has_sqlserver=false
    local has_docker=false
    local has_nginx=false
    local has_port_conflict=false
    
    for service in "${services[@]}"; do
        case "$service" in
            "sqlserver:installed")
                has_sqlserver=true
                ;;
            "docker:installed")
                has_docker=true
                ;;
            "nginx:installed")
                has_nginx=true
                ;;
            "nginx:port-conflict")
                has_port_conflict=true
                ;;
        esac
    done
    
    # SQL Server适配建议
    if [ "$has_sqlserver" = true ]; then
        echo "🗄️  SQL Server适配："
        echo "   ✅ 使用现有SQL Server实例"
        echo "   📝 建议使用: docker-compose.existing.yml"
        echo "   🔧 需要创建ToDoListArea数据库"
        echo ""
    else
        echo "🗄️  SQL Server安装："
        echo "   📦 将安装Docker化的SQL Server"
        echo "   📝 建议使用: docker-compose.yml"
        echo ""
    fi
    
    # Docker适配建议
    if [ "$has_docker" = true ]; then
        echo "🐳 Docker适配："
        echo "   ✅ 跳过Docker安装步骤"
        echo "   🔧 直接使用现有Docker服务"
        echo ""
    else
        echo "🐳 Docker安装："
        echo "   📦 需要安装Docker和Docker Compose"
        echo ""
    fi
    
    # Nginx适配建议
    if [ "$has_nginx" = true ]; then
        echo "🌐 Nginx适配："
        echo "   ✅ 使用现有Nginx实例"
        echo "   🔧 需要添加ToDoListArea配置"
        if [ "$has_port_conflict" = true ]; then
            echo "   ⚠️  前端容器将使用8080端口"
            echo "   📝 需要配置Nginx反向代理"
        fi
        echo ""
    else
        echo "🌐 Nginx配置："
        echo "   📦 将使用Docker化的Nginx"
        echo "   🔧 自动配置SSL和反向代理"
        echo ""
    fi
}

# 生成推荐的部署命令
generate_deployment_commands() {
    local services=("$@")
    
    echo "🚀 推荐的部署命令："
    echo "=================="
    
    local has_existing_services=false
    for service in "${services[@]}"; do
        if [[ "$service" == *":installed" ]]; then
            has_existing_services=true
            break
        fi
    done
    
    if [ "$has_existing_services" = true ]; then
        echo "# 适配现有环境的部署方式："
        echo "cp docker-compose.existing.yml docker-compose.yml"
        echo "./Scripts/deploy-existing.sh"
        echo ""
        echo "# 或者手动部署："
        echo "docker-compose -f docker-compose.existing.yml up -d"
    else
        echo "# 全新环境部署方式："
        echo "./Scripts/deploy-prod.sh"
        echo ""
        echo "# 或者："
        echo "docker-compose up -d"
    fi
    
    echo ""
    echo "📝 部署后需要的配置："
    echo "   1. 初始化数据库: docker exec -it todolist-backend dotnet ef database update"
    echo "   2. 配置Nginx代理 (如果使用现有Nginx)"
    echo "   3. 配置SSL证书"
    echo "   4. 验证服务健康状态"
}

# 主函数
main() {
    case "${1:-detect}" in
        "detect")
            local services
            mapfile -t services < <(detect_existing_services)
            generate_adaptation_advice "${services[@]}"
            generate_deployment_commands "${services[@]}"
            ;;
        "check-ports")
            echo "🔍 检查端口占用..."
            check_port_usage 80 "HTTP"
            check_port_usage 443 "HTTPS"
            check_port_usage 1433 "SQL Server"
            check_port_usage 5006 "后端API"
            check_port_usage 6379 "Redis"
            ;;
        *)
            echo "用法: $0 [detect|check-ports]"
            echo "  detect     - 检测现有服务并生成适配建议 (默认)"
            echo "  check-ports - 检查端口占用情况"
            exit 1
            ;;
    esac
}

# 执行主函数
main "$@"
