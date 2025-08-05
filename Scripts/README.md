# 🚀 ToDoListArea 生产环境部署脚本

## 📋 目录结构

```
Scripts/
├── deploy.sh              # 通用生产环境部署脚本
├── deploy-prod.sh         # Linux生产环境专用部署脚本 (全新环境)
├── deploy-existing.sh     # 现有环境适配部署脚本 🆕
├── env-adapter.sh         # 环境检测和适配工具 🆕
├── docker-optimize.sh     # Docker构建优化脚本
├── init-db.sql           # 数据库初始化脚本
├── monitor.sh            # Linux生产环境监控脚本
├── backup.sh             # Linux生产环境备份脚本
├── todolist.service      # Linux系统服务配置文件
├── nginx-integration.md  # Nginx配置集成指南 🆕
└── README.md             # 本文档
```

## 🎯 功能概述

### 主要部署脚本
- **deploy.sh**: 通用生产环境自动化部署
- **deploy-prod.sh**: Linux生产环境专用部署脚本 (推荐)
- **init-db.sql**: 数据库表结构和初始数据

### Linux生产环境工具
- **monitor.sh**: 系统监控和健康检查脚本
- **backup.sh**: 数据备份和恢复脚本
- **docker-optimize.sh**: Docker构建优化和清理脚本

### 环境适配工具 🆕
- **env-adapter.sh**: 环境检测和适配建议工具
- **deploy-existing.sh**: 现有环境适配部署脚本
- **nginx-integration.md**: Nginx配置集成详细指南
- **todolist.service**: Systemd服务配置文件

## 🚀 快速开始

### 1. 现有环境适配部署（推荐）🆕

**适配已有SQL Server、Nginx、Docker的环境:**
```bash
# 1. 上传项目文件到Linux服务器
scp -r ToDoListArea/ user@server:/tmp/

# 2. 登录服务器并检测环境
ssh user@server
cd /tmp/ToDoListArea
./Scripts/env-adapter.sh detect

# 3. 配置环境变量
cp .env.example .env.production
# 编辑 .env.production 文件，配置数据库密码等
nano .env.production

# 4. 使用适配脚本部署
sudo ./Scripts/deploy-existing.sh
```

### 2. 全新Linux环境部署

**完整的Linux生产环境部署:**
```bash
# 1. 上传项目文件到Linux服务器
scp -r ToDoListArea/ user@server:/tmp/

# 2. 登录服务器
ssh user@server

# 3. 配置环境变量
cd /tmp/ToDoListArea
cp .env.example .env.production
# 编辑 .env.production 填入实际配置

# 4. 运行Linux专用部署脚本 (需要root权限)
sudo ./Scripts/deploy-prod.sh
```

### 3. 通用生产环境部署

**Linux/macOS:**
```bash
# 进入项目根目录
cd ToDoListArea

# 配置环境变量
cp .env.example .env.production
# 编辑 .env.production 填入实际配置

# 运行部署脚本
chmod +x Scripts/deploy.sh
./Scripts/deploy.sh
```

### 2. 环境检测和适配 🆕

**检测现有服务:**
```bash
# 检测现有环境并获取适配建议
./Scripts/env-adapter.sh detect

# 仅检查端口占用情况
./Scripts/env-adapter.sh check-ports
```

### 3. 部署前验证

**验证部署环境:**
```bash
# 检查系统要求
./Scripts/deploy-prod.sh --check-only

# 验证配置文件
docker-compose config

# 优化Docker构建和测试
./Scripts/docker-optimize.sh build
./Scripts/docker-optimize.sh test
```

## 🔧 Nginx 配置管理

### 配置文件说明

| 文件 | 用途 | 特点 |
|------|------|------|
| `nginx.conf` | 生产环境 | HTTPS强制、完整缓存、安全头 |

### 配置验证

```bash
# 验证Nginx配置语法
docker run --rm -v "$(pwd)/WebCode/todo-frontend/nginx.conf:/etc/nginx/nginx.conf:ro" nginx:alpine nginx -t

# 测试配置文件
docker-compose config
```

## 📊 部署架构

### 服务组件
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Nginx         │    │   ASP.NET Core  │    │   SQL Server    │
│   (前端+代理)    │────│   (后端API)     │────│   (数据库)      │
│   Port: 80/443  │    │   Port: 5006    │    │   Port: 1433    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                                              │
         └──────────────────┬───────────────────────────┘
                           │
                  ┌─────────────────┐
                  │     Redis       │
                  │   (缓存服务)     │
                  │   Port: 6379    │
                  └─────────────────┘
```

### 网络配置
- **前端访问**: http://localhost → 自动重定向到 https://localhost
- **API代理**: https://localhost/api/* → http://backend:5006/*
- **健康检查**: https://localhost/health
- **监控状态**: https://localhost/nginx_status (仅内网)

## 🔒 SSL 证书配置

### 自动生成（测试环境）
部署脚本会自动生成自签名证书用于测试：
```bash
# 证书位置
ssl/nginx-selfsigned.crt    # 证书文件
ssl/nginx-selfsigned.key    # 私钥文件
```

### 生产环境证书
1. 获取有效的SSL证书（Let's Encrypt、商业证书等）
2. 将证书文件放置到 `ssl/` 目录
3. 更新环境变量：
   ```env
   SSL_CERT_PATH=/path/to/your/certificate.crt
   SSL_KEY_PATH=/path/to/your/private.key
   ```

## 🌍 环境变量配置

### 根目录环境变量 (.env.production)
```env
# 数据库配置
DB_SA_PASSWORD=YourSecurePassword123!

# JWT配置
JWT_SECRET_KEY=your-production-jwt-secret-key
JWT_ISSUER=ToDoListArea-Production
JWT_AUDIENCE=ToDoListArea-Production-Users
JWT_EXPIRATION=30

# 后端应用环境
ASPNETCORE_ENVIRONMENT=Production

# SSL证书配置
SSL_CERT_PATH=/etc/ssl/certs/your-domain.crt
SSL_KEY_PATH=/etc/ssl/private/your-domain.key

# 域名配置
DOMAIN_NAME=your-production-domain.com
```

### 前端环境变量 (WebCode/todo-frontend/.env.production)
```env
# API配置
VITE_API_BASE_URL=https://your-api-domain.com/api

# 应用信息
VITE_APP_TITLE=智能提醒事项管理系统
VITE_APP_VERSION=1.0.0

# 功能开关
VITE_ENABLE_ANALYTICS=true
VITE_ENABLE_ERROR_REPORTING=true
```

## 🔍 故障排除

### 常见问题

**1. SSL 证书错误**
```bash
# 检查证书有效性
openssl x509 -in ssl/nginx-selfsigned.crt -text -noout

# 重新生成证书
rm -f ssl/nginx-selfsigned.*
./Scripts/deploy.sh
```

**2. Nginx 配置错误**
```bash
# 测试配置语法
./Scripts/test-nginx.sh config

# 查看详细错误
docker logs todolist-frontend
```

**3. 前端构建失败**
```bash
# 测试前端构建
./Scripts/test-nginx.sh build

# 清理并重新构建
cd WebCode/todo-frontend
rm -rf node_modules dist
npm install
npm run build
```

**4. 服务无法访问**
```bash
# 检查服务状态
docker-compose ps

# 查看服务日志
docker-compose logs frontend
docker-compose logs backend

# 检查端口占用
netstat -tlnp | grep :80
netstat -tlnp | grep :443
```

### 日志查看

```bash
# 查看所有服务日志
docker-compose logs -f

# 查看特定服务日志
docker-compose logs -f frontend
docker-compose logs -f backend

# 查看 Nginx 访问日志
docker exec -it todolist-frontend tail -f /var/log/nginx/access.log

# 查看 Nginx 错误日志
docker exec -it todolist-frontend tail -f /var/log/nginx/error.log
```

## 📈 性能优化

### Nginx 优化特性
- ✅ Gzip 压缩 (压缩比 6)
- ✅ 静态资源缓存 (1年)
- ✅ HTTP/2 支持
- ✅ 连接复用
- ✅ 缓冲优化
- ✅ 安全头配置

### 监控指标
- 访问 `https://localhost/nginx_status` 查看 Nginx 状态
- 使用 `docker stats` 查看容器资源使用情况
- 查看日志文件分析访问模式

## 🔄 更新和维护

### 应用更新
```bash
# 拉取最新代码
git pull origin main

# 重新部署
./Scripts/deploy.sh
```

### 证书更新
```bash
# 更新证书文件
cp new-certificate.crt ssl/nginx-selfsigned.crt
cp new-private.key ssl/nginx-selfsigned.key

# 重启前端服务
docker-compose restart frontend
```

### 配置更新
```bash
# 测试新配置
./Scripts/test-nginx.sh config

# 应用新配置
docker-compose restart frontend
```

## 🐧 Linux生产环境管理 🆕

### 系统服务管理
```bash
# 查看服务状态
sudo systemctl status todolist

# 启动服务
sudo systemctl start todolist

# 停止服务
sudo systemctl stop todolist

# 重启服务
sudo systemctl restart todolist

# 查看服务日志
sudo journalctl -u todolist -f
```

### 监控和维护
```bash
# 执行系统监控检查
sudo /opt/todolist/Scripts/monitor.sh

# 生成监控报告
sudo /opt/todolist/Scripts/monitor.sh report

# 执行自动修复
sudo /opt/todolist/Scripts/monitor.sh repair
```

### 数据备份和恢复
```bash
# 执行完整备份
sudo /opt/todolist/Scripts/backup.sh

# 列出可用备份
sudo /opt/todolist/Scripts/backup.sh list

# 清理旧备份
sudo /opt/todolist/Scripts/backup.sh cleanup

# 恢复备份 (需要手动操作)
sudo /opt/todolist/Scripts/backup.sh restore 20240101_120000
```

### SSL证书管理
```bash
# 手动续期Let's Encrypt证书
sudo certbot renew

# 检查证书状态
sudo certbot certificates

# 重新获取证书
sudo certbot certonly --webroot -w /var/lib/letsencrypt -d your-domain.com
```

### 性能优化
```bash
# 查看容器资源使用
docker stats

# 查看系统资源
htop
iotop
nethogs

# 优化Docker
docker system prune -f
docker volume prune -f
```

### Docker优化管理 🆕
```bash
# 构建优化的镜像
./Scripts/docker-optimize.sh build

# 清理Docker资源
./Scripts/docker-optimize.sh cleanup

# 测试镜像功能
./Scripts/docker-optimize.sh test

# 分析镜像大小
./Scripts/docker-optimize.sh analyze

# 导出镜像备份
./Scripts/docker-optimize.sh export

# 查看Docker状态
./Scripts/docker-optimize.sh status

# 执行完整优化流程
./Scripts/docker-optimize.sh all
```

### 故障排除
```bash
# 查看容器日志
docker logs todolist-frontend-prod
docker logs todolist-backend-prod
docker logs todolist-database-prod

# 进入容器调试
docker exec -it todolist-frontend-prod /bin/sh
docker exec -it todolist-backend-prod /bin/bash

# 检查网络连通性
curl -I http://localhost/health
curl -I http://localhost:5006/health

# 检查端口占用
ss -tlnp | grep -E ':(80|443|5006|1433|6379)'

# Docker镜像问题排查
./Scripts/docker-optimize.sh analyze
docker system df
docker images --filter "dangling=true"
```

---

**📞 技术支持**: 如遇到问题，请查看项目文档或提交 Issue。
