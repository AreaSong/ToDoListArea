#!/bin/bash

# ===========================================
# ToDoListArea Linux生产环境备份脚本
# 用于备份数据库、配置文件和重要数据
# ===========================================

set -e

INSTALL_DIR="/opt/todolist"
BACKUP_DIR="$INSTALL_DIR/backups"
DATE=$(date +%Y%m%d_%H%M%S)
RETENTION_DAYS=30

# 日志函数
log() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] $1"
}

# 创建备份目录
create_backup_dir() {
    local backup_path="$BACKUP_DIR/$DATE"
    mkdir -p "$backup_path"
    echo "$backup_path"
}

# 备份数据库
backup_database() {
    log "🗄️  开始备份数据库..."
    
    local backup_path="$1"
    local db_backup_file="$backup_path/database_$DATE.bak"
    
    # 从环境变量获取数据库密码
    source "$INSTALL_DIR/.env.production" 2>/dev/null || {
        log "❌ 无法读取环境变量文件"
        return 1
    }
    
    # 使用Docker执行数据库备份
    docker exec todolist-database-prod /opt/mssql-tools/bin/sqlcmd \
        -S localhost -U sa -P "$DB_SA_PASSWORD" \
        -Q "BACKUP DATABASE [ToDoListArea] TO DISK = N'/var/opt/mssql/backup/database_$DATE.bak' WITH NOFORMAT, NOINIT, NAME = 'ToDoListArea-Full Database Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10"
    
    # 复制备份文件到主机
    docker cp "todolist-database-prod:/var/opt/mssql/backup/database_$DATE.bak" "$db_backup_file"
    
    # 压缩备份文件
    gzip "$db_backup_file"
    
    log "✅ 数据库备份完成: $(basename "$db_backup_file.gz")"
}

# 备份Redis数据
backup_redis() {
    log "📦 开始备份Redis数据..."
    
    local backup_path="$1"
    local redis_backup_file="$backup_path/redis_$DATE.rdb"
    
    # 触发Redis保存
    docker exec todolist-redis-prod redis-cli BGSAVE
    
    # 等待保存完成
    sleep 5
    
    # 复制RDB文件
    docker cp "todolist-redis-prod:/data/dump.rdb" "$redis_backup_file"
    
    # 压缩备份文件
    gzip "$redis_backup_file"
    
    log "✅ Redis备份完成: $(basename "$redis_backup_file.gz")"
}

# 备份配置文件
backup_configs() {
    log "⚙️  开始备份配置文件..."
    
    local backup_path="$1"
    local config_backup_file="$backup_path/configs_$DATE.tar.gz"
    
    # 创建配置文件列表
    local config_files=(
        ".env.production"
        "docker-compose.yml"
        "docker-compose.prod.yml"
        "WebCode/todo-frontend/nginx.conf"
        "ApiCode/ToDoListArea/ToDoListArea/appsettings.Production.json"
        "ssl/"
    )
    
    # 打包配置文件
    cd "$INSTALL_DIR"
    tar -czf "$config_backup_file" "${config_files[@]}" 2>/dev/null || {
        log "⚠️  部分配置文件备份失败"
    }
    
    log "✅ 配置文件备份完成: $(basename "$config_backup_file")"
}

# 备份日志文件
backup_logs() {
    log "📋 开始备份日志文件..."
    
    local backup_path="$1"
    local logs_backup_file="$backup_path/logs_$DATE.tar.gz"
    
    # 打包最近7天的日志
    cd "$INSTALL_DIR"
    find logs -name "*.log" -mtime -7 -type f | tar -czf "$logs_backup_file" -T - 2>/dev/null || {
        log "⚠️  日志文件备份失败"
        return 1
    }
    
    log "✅ 日志文件备份完成: $(basename "$logs_backup_file")"
}

# 备份Docker镜像
backup_docker_images() {
    log "🐳 开始备份Docker镜像..."
    
    local backup_path="$1"
    local images_backup_file="$backup_path/docker_images_$DATE.tar"
    
    # 获取项目相关的镜像
    local images=$(docker images --format "{{.Repository}}:{{.Tag}}" | grep -E "(todolist|nginx|redis|mssql)" | head -10)
    
    if [ -n "$images" ]; then
        # 保存镜像
        echo "$images" | xargs docker save -o "$images_backup_file"
        
        # 压缩镜像文件
        gzip "$images_backup_file"
        
        log "✅ Docker镜像备份完成: $(basename "$images_backup_file.gz")"
    else
        log "⚠️  未找到相关Docker镜像"
    fi
}

# 创建备份清单
create_backup_manifest() {
    log "📝 创建备份清单..."
    
    local backup_path="$1"
    local manifest_file="$backup_path/backup_manifest.txt"
    
    {
        echo "ToDoListArea 备份清单"
        echo "===================="
        echo "备份时间: $(date)"
        echo "备份路径: $backup_path"
        echo ""
        echo "备份文件列表:"
        ls -lh "$backup_path"
        echo ""
        echo "系统信息:"
        echo "  主机名: $(hostname)"
        echo "  系统版本: $(cat /etc/os-release | grep PRETTY_NAME | cut -d= -f2 | tr -d '"')"
        echo "  Docker版本: $(docker --version)"
        echo ""
        echo "应用版本信息:"
        if [ -f "$INSTALL_DIR/.git/HEAD" ]; then
            echo "  Git提交: $(cat "$INSTALL_DIR/.git/HEAD")"
        fi
        echo ""
        echo "容器状态:"
        docker ps --format "table {{.Names}}\t{{.Image}}\t{{.Status}}"
    } > "$manifest_file"
    
    log "✅ 备份清单创建完成: $(basename "$manifest_file")"
}

# 清理旧备份
cleanup_old_backups() {
    log "🧹 清理旧备份文件..."
    
    # 删除超过保留期的备份
    find "$BACKUP_DIR" -maxdepth 1 -type d -name "20*" -mtime +$RETENTION_DAYS -exec rm -rf {} \; 2>/dev/null || true
    
    # 统计当前备份数量和大小
    local backup_count=$(find "$BACKUP_DIR" -maxdepth 1 -type d -name "20*" | wc -l)
    local backup_size=$(du -sh "$BACKUP_DIR" 2>/dev/null | cut -f1)
    
    log "✅ 备份清理完成，当前保留 $backup_count 个备份，总大小 $backup_size"
}

# 验证备份完整性
verify_backup() {
    log "🔍 验证备份完整性..."
    
    local backup_path="$1"
    local errors=0
    
    # 检查备份文件是否存在
    local expected_files=("database_*.bak.gz" "redis_*.rdb.gz" "configs_*.tar.gz" "logs_*.tar.gz")
    
    for pattern in "${expected_files[@]}"; do
        if ! ls "$backup_path"/$pattern 1> /dev/null 2>&1; then
            log "⚠️  备份文件缺失: $pattern"
            errors=$((errors + 1))
        fi
    done
    
    # 检查备份文件大小
    for file in "$backup_path"/*.gz; do
        if [ -f "$file" ]; then
            local size=$(stat -c%s "$file")
            if [ "$size" -lt 1024 ]; then  # 小于1KB可能有问题
                log "⚠️  备份文件过小: $(basename "$file") ($size bytes)"
                errors=$((errors + 1))
            fi
        fi
    done
    
    if [ "$errors" -eq 0 ]; then
        log "✅ 备份完整性验证通过"
        return 0
    else
        log "❌ 备份完整性验证失败，发现 $errors 个问题"
        return 1
    fi
}

# 发送备份通知
send_backup_notification() {
    local backup_path="$1"
    local status="$2"
    
    if command -v mail &> /dev/null && [ -n "${ALERT_EMAIL:-}" ]; then
        local subject="ToDoListArea 备份通知 - $status"
        local message="备份任务已完成
        
备份时间: $(date)
备份路径: $backup_path
备份状态: $status

备份文件列表:
$(ls -lh "$backup_path" 2>/dev/null || echo "无法列出文件")"
        
        echo "$message" | mail -s "$subject" "$ALERT_EMAIL"
        log "📧 备份通知已发送"
    fi
}

# 完整备份流程
full_backup() {
    log "🚀 开始完整备份流程..."
    
    local backup_path
    backup_path=$(create_backup_dir)
    
    local backup_success=true
    
    # 执行各项备份
    backup_database "$backup_path" || backup_success=false
    backup_redis "$backup_path" || backup_success=false
    backup_configs "$backup_path" || backup_success=false
    backup_logs "$backup_path" || backup_success=false
    backup_docker_images "$backup_path" || backup_success=false
    
    # 创建备份清单
    create_backup_manifest "$backup_path"
    
    # 验证备份
    if verify_backup "$backup_path"; then
        log "✅ 完整备份流程成功完成"
        send_backup_notification "$backup_path" "成功"
    else
        backup_success=false
        log "❌ 备份验证失败"
        send_backup_notification "$backup_path" "失败"
    fi
    
    # 清理旧备份
    cleanup_old_backups
    
    if [ "$backup_success" = true ]; then
        log "🎉 备份任务完成: $backup_path"
        return 0
    else
        log "💥 备份任务失败: $backup_path"
        return 1
    fi
}

# 恢复备份
restore_backup() {
    local backup_date="$1"
    
    if [ -z "$backup_date" ]; then
        echo "❌ 请指定备份日期 (格式: YYYYMMDD_HHMMSS)"
        echo "可用备份:"
        ls -1 "$BACKUP_DIR" | grep "^20"
        return 1
    fi
    
    local backup_path="$BACKUP_DIR/$backup_date"
    
    if [ ! -d "$backup_path" ]; then
        echo "❌ 备份不存在: $backup_path"
        return 1
    fi
    
    log "🔄 开始恢复备份: $backup_date"
    
    # 这里可以添加具体的恢复逻辑
    # 注意：恢复操作需要谨慎，建议手动执行
    
    log "⚠️  恢复功能需要手动执行，请参考备份清单文件"
    cat "$backup_path/backup_manifest.txt"
}

# 主函数
main() {
    case "${1:-backup}" in
        "backup"|"full")
            full_backup
            ;;
        "restore")
            restore_backup "$2"
            ;;
        "cleanup")
            cleanup_old_backups
            ;;
        "list")
            echo "可用备份:"
            ls -la "$BACKUP_DIR" | grep "^d" | grep "20"
            ;;
        *)
            echo "用法: $0 [backup|restore|cleanup|list]"
            echo "  backup  - 执行完整备份 (默认)"
            echo "  restore - 恢复指定备份"
            echo "  cleanup - 清理旧备份"
            echo "  list    - 列出可用备份"
            exit 1
            ;;
    esac
}

# 执行主函数
main "$@"
