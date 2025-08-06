# ===========================================
# ToDoListArea 前端生产环境 Dockerfile
# 专为Linux生产环境优化的双阶段构建
# ===========================================

# 构建阶段
FROM node:20-alpine AS builder

# 设置工作目录
WORKDIR /app

# 设置npm镜像源（加速国内下载）
RUN npm config set registry https://registry.npmmirror.com



# # 设置npm镜像源（加速国内下载）
# RUN npm config set registry https://registry.npmmirror.com && \
#     # 删除有问题的配置行 \
#     npm config set sass_binary_site https://npmmirror.com/mirrors/node-sass && \
#     npm config set electron_mirror https://npmmirror.com/mirrors/electron/ && \
#     npm config set puppeteer_download_host https://npmmirror.com/mirrors && \
#     npm config set chromedriver_cdnurl https://npmmirror.com/mirrors/chromedriver && \
#     npm config set operadriver_cdnurl https://npmmirror.com/mirrors/operadriver && \
#     npm config set phantomjs_cdnurl https://npmmirror.com/mirrors/phantomjs && \
#     npm config set selenium_cdnurl https://npmmirror.com/mirrors/selenium && \
#     npm config set node_inspector_cdnurl https://npmmirror.com/mirrors/node-inspector

# 创建.npmrc文件，设置其他镜像源
RUN echo 'sass_binary_site=https://npmmirror.com/mirrors/node-sass' >> ~/.npmrc && \
    echo 'electron_mirror=https://npmmirror.com/mirrors/electron/' >> ~/.npmrc && \
    echo 'puppeteer_download_host=https://npmmirror.com/mirrors/puppeteer' >> ~/.npmrc && \
    echo 'chromedriver_cdnurl=https://npmmirror.com/mirrors/chromedriver' >> ~/.npmrc && \
    echo 'operadriver_cdnurl=https://npmmirror.com/mirrors/operadriver' >> ~/.npmrc && \
    echo 'phantomjs_cdnurl=https://npmmirror.com/mirrors/phantomjs' >> ~/.npmrc && \
    echo 'selenium_cdnurl=https://npmmirror.com/mirrors/selenium' >> ~/.npmrc && \
    echo 'node_inspector_cdnurl=https://npmmirror.com/mirrors/node-inspector' >> ~/.npmrc

# 复制package文件
COPY package*.json ./

# 安装依赖（包括开发依赖，构建需要）
RUN npm install

# 复制源代码
COPY . .

# 构建应用
RUN npm run build

# 清理构建缓存
RUN npm cache clean --force

# 生产阶段 - Nginx静态文件服务
FROM nginx:alpine AS production

# 安装必要工具和优化（使用国内镜像源，最小化安装）
RUN sed -i 's/dl-cdn.alpinelinux.org/mirrors.ustc.edu.cn/g' /etc/apk/repositories && \
    apk add --no-cache --update-cache \
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
