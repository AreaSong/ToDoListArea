#!/bin/bash

# ===========================================
# ToDoListArea Linux生产环境监控脚本
# 用于监控系统状态和应用健康度
# ===========================================

set -e

INSTALL_DIR="/opt/todolist"
LOG_FILE="/var/log/todolist-monitor.log"
ALERT_EMAIL="${ALERT_EMAIL:-admin@example.com}"

# 日志函数
log() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] $1" | tee -a "$LOG_FILE"
}

# 检查Docker服务
check_docker_service() {
    log "🐳 检查Docker服务状态..."
    
    if systemctl is-active --quiet docker; then
        log "✅ Docker服务运行正常"
        return 0
    else
        log "❌ Docker服务异常"
        return 1
    fi
}

# 检查应用服务
check_application_service() {
    log "🚀 检查应用服务状态..."
    
    if systemctl is-active --quiet todolist; then
        log "✅ 应用服务运行正常"
        return 0
    else
        log "❌ 应用服务异常"
        return 1
    fi
}

# 检查容器状态
check_containers() {
    log "📦 检查容器状态..."
    
    cd "$INSTALL_DIR"
    
    local containers=("todolist-frontend-prod" "todolist-backend-prod" "todolist-database-prod" "todolist-redis-prod")
    local failed_containers=()
    
    for container in "${containers[@]}"; do
        if docker ps --format "table {{.Names}}" | grep -q "$container"; then
            local health=$(docker inspect --format='{{.State.Health.Status}}' "$container" 2>/dev/null || echo "no-health-check")
            if [ "$health" = "healthy" ] || [ "$health" = "no-health-check" ]; then
                log "✅ 容器 $container 运行正常"
            else
                log "⚠️  容器 $container 健康检查失败: $health"
                failed_containers+=("$container")
            fi
        else
            log "❌ 容器 $container 未运行"
            failed_containers+=("$container")
        fi
    done
    
    if [ ${#failed_containers[@]} -eq 0 ]; then
        return 0
    else
        log "❌ 失败的容器: ${failed_containers[*]}"
        return 1
    fi
}

# 检查网络连通性
check_network() {
    log "🌐 检查网络连通性..."
    
    local endpoints=("http://localhost/health" "http://localhost:5006/health")
    local failed_endpoints=()
    
    for endpoint in "${endpoints[@]}"; do
        if curl -f -s --max-time 10 "$endpoint" > /dev/null; then
            log "✅ 端点 $endpoint 响应正常"
        else
            log "❌ 端点 $endpoint 无响应"
            failed_endpoints+=("$endpoint")
        fi
    done
    
    if [ ${#failed_endpoints[@]} -eq 0 ]; then
        return 0
    else
        log "❌ 失败的端点: ${failed_endpoints[*]}"
        return 1
    fi
}

# 检查系统资源
check_system_resources() {
    log "💻 检查系统资源..."
    
    # 检查内存使用率
    local mem_usage=$(free | grep Mem | awk '{printf "%.1f", $3/$2 * 100.0}')
    log "📊 内存使用率: ${mem_usage}%"
    
    if (( $(echo "$mem_usage > 90" | bc -l) )); then
        log "⚠️  内存使用率过高: ${mem_usage}%"
        return 1
    fi
    
    # 检查磁盘使用率
    local disk_usage=$(df / | tail -1 | awk '{print $5}' | sed 's/%//')
    log "📊 磁盘使用率: ${disk_usage}%"
    
    if [ "$disk_usage" -gt 90 ]; then
        log "⚠️  磁盘使用率过高: ${disk_usage}%"
        return 1
    fi
    
    # 检查CPU负载
    local cpu_load=$(uptime | awk -F'load average:' '{print $2}' | awk '{print $1}' | sed 's/,//')
    local cpu_cores=$(nproc)
    local cpu_usage=$(echo "scale=1; $cpu_load / $cpu_cores * 100" | bc)
    log "📊 CPU负载: ${cpu_load} (${cpu_usage}%)"
    
    if (( $(echo "$cpu_usage > 80" | bc -l) )); then
        log "⚠️  CPU负载过高: ${cpu_usage}%"
        return 1
    fi
    
    return 0
}

# 检查日志错误
check_logs() {
    log "📋 检查应用日志..."
    
    local error_count=0
    
    # 检查系统日志中的错误
    if journalctl -u todolist --since "1 hour ago" | grep -i error > /dev/null; then
        error_count=$((error_count + 1))
        log "⚠️  系统日志中发现错误"
    fi
    
    # 检查Nginx日志中的错误
    if [ -f "$INSTALL_DIR/logs/nginx/error.log" ]; then
        local nginx_errors=$(tail -n 100 "$INSTALL_DIR/logs/nginx/error.log" | grep -c "error" || echo "0")
        if [ "$nginx_errors" -gt 10 ]; then
            error_count=$((error_count + 1))
            log "⚠️  Nginx错误日志过多: $nginx_errors 条"
        fi
    fi
    
    if [ "$error_count" -eq 0 ]; then
        log "✅ 日志检查正常"
        return 0
    else
        log "❌ 发现 $error_count 个日志问题"
        return 1
    fi
}

# 检查SSL证书
check_ssl_certificate() {
    log "🔐 检查SSL证书..."
    
    if [ -f "$INSTALL_DIR/ssl/nginx-selfsigned.crt" ]; then
        local cert_expiry=$(openssl x509 -in "$INSTALL_DIR/ssl/nginx-selfsigned.crt" -noout -enddate | cut -d= -f2)
        local cert_expiry_epoch=$(date -d "$cert_expiry" +%s)
        local current_epoch=$(date +%s)
        local days_until_expiry=$(( (cert_expiry_epoch - current_epoch) / 86400 ))
        
        log "📅 SSL证书剩余天数: $days_until_expiry 天"
        
        if [ "$days_until_expiry" -lt 30 ]; then
            log "⚠️  SSL证书即将过期: $days_until_expiry 天"
            return 1
        else
            log "✅ SSL证书有效期正常"
            return 0
        fi
    else
        log "❌ SSL证书文件不存在"
        return 1
    fi
}

# 发送告警邮件
send_alert() {
    local subject="$1"
    local message="$2"
    
    if command -v mail &> /dev/null && [ -n "$ALERT_EMAIL" ]; then
        echo "$message" | mail -s "$subject" "$ALERT_EMAIL"
        log "📧 告警邮件已发送到: $ALERT_EMAIL"
    else
        log "⚠️  无法发送告警邮件 (mail命令不可用或邮箱未配置)"
    fi
}

# 自动修复
auto_repair() {
    log "🔧 尝试自动修复..."
    
    # 重启异常的服务
    if ! check_application_service; then
        log "🔄 重启应用服务..."
        systemctl restart todolist
        sleep 30
        
        if check_application_service; then
            log "✅ 应用服务重启成功"
        else
            log "❌ 应用服务重启失败"
            send_alert "ToDoListArea服务异常" "应用服务重启失败，需要人工干预"
        fi
    fi
    
    # 清理Docker资源
    log "🧹 清理Docker资源..."
    docker system prune -f > /dev/null 2>&1 || true
    
    # 清理旧日志
    find "$INSTALL_DIR/logs" -name "*.log" -mtime +30 -delete 2>/dev/null || true
}

# 生成监控报告
generate_report() {
    local report_file="/tmp/todolist-monitor-report-$(date +%Y%m%d_%H%M%S).txt"
    
    {
        echo "ToDoListArea 系统监控报告"
        echo "=========================="
        echo "生成时间: $(date)"
        echo ""
        
        echo "系统信息:"
        echo "  主机名: $(hostname)"
        echo "  系统: $(uname -a)"
        echo "  运行时间: $(uptime)"
        echo ""
        
        echo "Docker信息:"
        docker version --format "  版本: {{.Server.Version}}"
        echo "  容器数量: $(docker ps -q | wc -l)"
        echo ""
        
        echo "应用状态:"
        systemctl status todolist --no-pager -l
        echo ""
        
        echo "容器状态:"
        docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
        echo ""
        
        echo "系统资源:"
        echo "  内存使用: $(free -h | grep Mem | awk '{print $3 "/" $2}')"
        echo "  磁盘使用: $(df -h / | tail -1 | awk '{print $3 "/" $2 " (" $5 ")"}')"
        echo "  CPU负载: $(uptime | awk -F'load average:' '{print $2}')"
        echo ""
        
        echo "网络状态:"
        ss -tlnp | grep -E ':(80|443|5006|1433|6379)'
        
    } > "$report_file"
    
    log "📊 监控报告已生成: $report_file"
    echo "$report_file"
}

# 主监控函数
main_monitor() {
    log "🔍 开始系统监控检查..."
    
    local failed_checks=0
    
    # 执行各项检查
    check_docker_service || failed_checks=$((failed_checks + 1))
    check_application_service || failed_checks=$((failed_checks + 1))
    check_containers || failed_checks=$((failed_checks + 1))
    check_network || failed_checks=$((failed_checks + 1))
    check_system_resources || failed_checks=$((failed_checks + 1))
    check_logs || failed_checks=$((failed_checks + 1))
    check_ssl_certificate || failed_checks=$((failed_checks + 1))
    
    if [ "$failed_checks" -eq 0 ]; then
        log "✅ 所有检查通过，系统运行正常"
    else
        log "❌ 发现 $failed_checks 个问题"
        
        # 尝试自动修复
        auto_repair
        
        # 发送告警
        send_alert "ToDoListArea监控告警" "发现 $failed_checks 个问题，请检查系统状态"
    fi
    
    log "🏁 监控检查完成"
}

# 命令行参数处理
case "${1:-monitor}" in
    "monitor")
        main_monitor
        ;;
    "report")
        generate_report
        ;;
    "repair")
        auto_repair
        ;;
    *)
        echo "用法: $0 [monitor|report|repair]"
        echo "  monitor - 执行监控检查 (默认)"
        echo "  report  - 生成监控报告"
        echo "  repair  - 执行自动修复"
        exit 1
        ;;
esac
