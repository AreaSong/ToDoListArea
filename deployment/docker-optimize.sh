#!/bin/bash

# ===========================================
# Docker构建优化脚本
# 用于优化Docker镜像构建和清理
# ===========================================

set -e

echo "🐳 Docker构建优化工具"

# 显示当前Docker状态
show_docker_status() {
    echo "📊 当前Docker状态："
    echo "   镜像数量: $(docker images -q | wc -l)"
    echo "   容器数量: $(docker ps -a -q | wc -l)"
    echo "   磁盘使用: $(docker system df --format "table {{.Type}}\t{{.TotalCount}}\t{{.Size}}\t{{.Reclaimable}}")"
}

# 构建优化的镜像
build_optimized_images() {
    echo "🔨 构建优化的Docker镜像..."
    
    # 构建前端镜像
    echo "📦 构建前端镜像..."
    docker build \
        --target production \
        --build-arg BUILDKIT_INLINE_CACHE=1 \
        --cache-from todolist-frontend:latest \
        -t todolist-frontend:latest \
        -t todolist-frontend:$(date +%Y%m%d) \
        ./WebCode/todo-frontend
    
    # 构建后端镜像
    echo "📦 构建后端镜像..."
    docker build \
        --build-arg BUILDKIT_INLINE_CACHE=1 \
        --cache-from todolist-backend:latest \
        -t todolist-backend:latest \
        -t todolist-backend:$(date +%Y%m%d) \
        ./ApiCode/ToDoListArea/ToDoListArea
    
    echo "✅ 镜像构建完成"
}

# 清理Docker资源
cleanup_docker() {
    echo "🧹 清理Docker资源..."
    
    # 清理未使用的镜像
    docker image prune -f
    
    # 清理未使用的容器
    docker container prune -f
    
    # 清理未使用的网络
    docker network prune -f
    
    # 清理未使用的卷
    docker volume prune -f
    
    # 清理构建缓存
    docker builder prune -f
    
    echo "✅ Docker资源清理完成"
}

# 分析镜像大小
analyze_image_sizes() {
    echo "📏 分析镜像大小..."
    
    echo "前端镜像："
    docker images todolist-frontend --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}\t{{.CreatedAt}}"
    
    echo "后端镜像："
    docker images todolist-backend --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}\t{{.CreatedAt}}"
    
    echo "基础镜像："
    docker images | grep -E "(nginx|node|dotnet)" | head -10
}

# 测试镜像
test_images() {
    echo "🧪 测试Docker镜像..."
    
    # 测试前端镜像
    echo "测试前端镜像..."
    docker run --rm -d --name test-frontend -p 8080:80 todolist-frontend:latest
    sleep 5
    
    if curl -f http://localhost:8080/health > /dev/null 2>&1; then
        echo "✅ 前端镜像测试通过"
    else
        echo "❌ 前端镜像测试失败"
    fi
    
    docker stop test-frontend > /dev/null 2>&1 || true
    
    # 测试后端镜像
    echo "测试后端镜像..."
    docker run --rm -d --name test-backend -p 8081:5006 \
        -e ASPNETCORE_ENVIRONMENT=Production \
        todolist-backend:latest
    sleep 10
    
    if curl -f http://localhost:8081/health > /dev/null 2>&1; then
        echo "✅ 后端镜像测试通过"
    else
        echo "❌ 后端镜像测试失败"
    fi
    
    docker stop test-backend > /dev/null 2>&1 || true
}

# 导出镜像
export_images() {
    local export_dir="./docker-images-$(date +%Y%m%d)"
    mkdir -p "$export_dir"
    
    echo "📤 导出Docker镜像到 $export_dir ..."
    
    docker save todolist-frontend:latest | gzip > "$export_dir/todolist-frontend.tar.gz"
    docker save todolist-backend:latest | gzip > "$export_dir/todolist-backend.tar.gz"
    
    echo "✅ 镜像导出完成"
    echo "📊 导出文件大小："
    ls -lh "$export_dir"
}

# 主函数
main() {
    case "${1:-status}" in
        "build")
            show_docker_status
            build_optimized_images
            analyze_image_sizes
            ;;
        "cleanup")
            cleanup_docker
            show_docker_status
            ;;
        "test")
            test_images
            ;;
        "export")
            export_images
            ;;
        "analyze")
            analyze_image_sizes
            ;;
        "status")
            show_docker_status
            ;;
        "all")
            show_docker_status
            build_optimized_images
            test_images
            analyze_image_sizes
            cleanup_docker
            ;;
        *)
            echo "用法: $0 [build|cleanup|test|export|analyze|status|all]"
            echo "  build   - 构建优化的镜像"
            echo "  cleanup - 清理Docker资源"
            echo "  test    - 测试镜像"
            echo "  export  - 导出镜像"
            echo "  analyze - 分析镜像大小"
            echo "  status  - 显示Docker状态"
            echo "  all     - 执行完整流程"
            exit 1
            ;;
    esac
}

# 执行主函数
main "$@"
