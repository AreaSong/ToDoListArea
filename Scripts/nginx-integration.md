# 🌐 Nginx配置集成指南

## 概述

本指南帮助您将ToDoListArea应用集成到现有的Nginx服务中，避免端口冲突并实现统一的反向代理配置。

## 🔍 集成方案

### 方案一：独立站点配置（推荐）

#### 1. 创建独立配置文件

```bash
# 创建ToDoListArea站点配置
sudo nano /etc/nginx/sites-available/todolist
```

#### 2. 配置内容

```nginx
# ToDoListArea 应用配置
server {
    listen 80;
    server_name your-domain.com;  # 替换为您的域名
    
    # 重定向到HTTPS
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name your-domain.com;  # 替换为您的域名
    
    # SSL配置 (根据您的证书路径调整)
    ssl_certificate /etc/ssl/certs/your-cert.crt;
    ssl_certificate_key /etc/ssl/private/your-key.key;
    
    # SSL安全配置
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers ECDHE-RSA-AES256-GCM-SHA512:DHE-RSA-AES256-GCM-SHA512:ECDHE-RSA-AES256-GCM-SHA384:DHE-RSA-AES256-GCM-SHA384;
    ssl_prefer_server_ciphers off;
    ssl_session_timeout 1d;
    ssl_session_cache shared:SSL:50m;
    ssl_session_tickets off;
    
    # 安全头
    add_header Strict-Transport-Security "max-age=63072000; includeSubDomains; preload" always;
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Referrer-Policy "strict-origin-when-cross-origin" always;
    
    # 静态文件和前端应用代理
    location / {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        
        # 超时配置
        proxy_connect_timeout 30s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
    }
    
    # API代理到后端容器
    location /api/ {
        proxy_pass http://localhost:5006/;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        # 超时配置
        proxy_connect_timeout 30s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
        
        # 缓冲配置
        proxy_buffering on;
        proxy_buffer_size 4k;
        proxy_buffers 8 4k;
        proxy_busy_buffers_size 8k;
    }
    
    # 健康检查端点
    location /health {
        proxy_pass http://localhost:8080/health;
        access_log off;
    }
    
    # API健康检查
    location /api/health {
        proxy_pass http://localhost:5006/health;
        access_log off;
    }
}
```

#### 3. 启用配置

```bash
# 启用站点
sudo ln -s /etc/nginx/sites-available/todolist /etc/nginx/sites-enabled/

# 测试配置
sudo nginx -t

# 重载Nginx
sudo systemctl reload nginx
```

### 方案二：子路径配置

如果您希望在现有站点的子路径下运行ToDoListArea：

```nginx
# 在现有server块中添加以下location配置

# ToDoListArea前端
location /todolist/ {
    proxy_pass http://localhost:8080/;
    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection 'upgrade';
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
    proxy_cache_bypass $http_upgrade;
}

# ToDoListArea API
location /todolist/api/ {
    proxy_pass http://localhost:5006/;
    proxy_http_version 1.1;
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
}
```

## 🔧 配置验证

### 1. 检查Nginx配置语法

```bash
sudo nginx -t
```

### 2. 检查端口监听

```bash
sudo netstat -tlnp | grep nginx
```

### 3. 测试代理功能

```bash
# 测试前端代理
curl -I http://localhost/

# 测试API代理
curl -I http://localhost/api/health
```

## 🚨 故障排除

### 常见问题

#### 1. 502 Bad Gateway错误

**原因**: 后端服务未启动或端口不可达

**解决方案**:
```bash
# 检查容器状态
docker-compose -f docker-compose.existing.yml ps

# 检查端口监听
netstat -tlnp | grep -E ':(5006|8080)'

# 重启服务
docker-compose -f docker-compose.existing.yml restart
```

#### 2. SSL证书问题

**原因**: SSL证书路径错误或证书过期

**解决方案**:
```bash
# 检查证书文件
sudo ls -la /etc/ssl/certs/your-cert.crt
sudo ls -la /etc/ssl/private/your-key.key

# 检查证书有效期
sudo openssl x509 -in /etc/ssl/certs/your-cert.crt -text -noout | grep -A2 "Validity"
```

#### 3. 权限问题

**原因**: Nginx用户无法访问证书文件

**解决方案**:
```bash
# 设置正确的权限
sudo chown root:root /etc/ssl/certs/your-cert.crt
sudo chown root:root /etc/ssl/private/your-key.key
sudo chmod 644 /etc/ssl/certs/your-cert.crt
sudo chmod 600 /etc/ssl/private/your-key.key
```

## 📊 性能优化

### 1. 启用Gzip压缩

在http块中添加：

```nginx
gzip on;
gzip_vary on;
gzip_min_length 1024;
gzip_proxied any;
gzip_comp_level 6;
gzip_types
    text/plain
    text/css
    text/xml
    text/javascript
    application/json
    application/javascript
    application/xml+rss
    application/atom+xml
    image/svg+xml;
```

### 2. 配置缓存

```nginx
# 静态资源缓存
location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
    proxy_pass http://localhost:8080;
    proxy_cache_valid 200 1y;
    add_header Cache-Control "public, immutable";
    expires 1y;
}
```

### 3. 连接优化

```nginx
# 在server块中添加
keepalive_timeout 65;
keepalive_requests 100;
```

## 📝 维护建议

1. **定期检查日志**:
   ```bash
   sudo tail -f /var/log/nginx/access.log
   sudo tail -f /var/log/nginx/error.log
   ```

2. **监控代理状态**:
   ```bash
   # 检查上游服务状态
   curl -f http://localhost:8080/health
   curl -f http://localhost:5006/health
   ```

3. **备份配置**:
   ```bash
   sudo cp /etc/nginx/sites-available/todolist /etc/nginx/sites-available/todolist.backup
   ```

## 🔄 回滚方案

如果需要回滚配置：

```bash
# 禁用站点
sudo rm /etc/nginx/sites-enabled/todolist

# 测试配置
sudo nginx -t

# 重载Nginx
sudo systemctl reload nginx
```
