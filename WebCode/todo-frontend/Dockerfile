# ===========================================
# ToDoListArea 前端生产环境 Dockerfile
# 专为Linux生产环境优化的双阶段构建
# ===========================================

# 构建阶段
FROM node:18-alpine AS builder

# 设置工作目录
WORKDIR /app

# 设置npm镜像源（加速国内下载）
RUN npm config set registry https://registry.npmmirror.com

# 复制package文件
COPY package*.json ./

# 安装依赖（包括开发依赖，构建需要）
RUN npm ci

# 复制源代码
COPY . .

# 构建应用
RUN npm run build

# 清理构建缓存
RUN npm cache clean --force

# 生产阶段 - Nginx静态文件服务
FROM nginx:alpine AS production

# 安装必要工具和优化
RUN apk add --no-cache \
    curl \
    openssl \
    && rm -rf /var/cache/apk/*

# 创建SSL证书目录
RUN mkdir -p /etc/ssl/certs /etc/ssl/private

# 复制构建产物到nginx目录
COPY --from=builder /app/dist /usr/share/nginx/html

# 复制nginx配置文件
COPY nginx.conf /etc/nginx/nginx.conf

# 设置正确的权限
RUN chown -R nginx:nginx /usr/share/nginx/html && \
    chmod -R 755 /usr/share/nginx/html

# 创建健康检查脚本
RUN echo '#!/bin/sh' > /usr/local/bin/healthcheck.sh && \
    echo 'curl -f http://localhost/health || exit 1' >> /usr/local/bin/healthcheck.sh && \
    chmod +x /usr/local/bin/healthcheck.sh

# 暴露端口
EXPOSE 80 443

# 健康检查
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD /usr/local/bin/healthcheck.sh

# 启动nginx
CMD ["nginx", "-g", "daemon off;"]
