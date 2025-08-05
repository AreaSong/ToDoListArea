# 生产环境配置

## 配置说明

本目录包含两种生产环境配置：

### 1. 标准生产环境 (根目录的docker-compose.yml)
- 包含完整的数据库、后端、前端服务
- 适用于独立部署的全新环境
- 使用80和443端口

### 2. 现有环境适配 (docker-compose.existing.yml)
- 适配已有SQL Server、Nginx、Docker的环境
- 前端使用8080端口，需要现有Nginx代理
- 连接到主机的SQL Server
- 包含Redis缓存服务

## 使用方法

### 标准生产环境部署
```bash
# 使用根目录配置
cd /path/to/project
docker-compose up -d
```

### 现有环境适配部署
```bash
# 使用现有环境配置
cd environments/production
docker-compose -f docker-compose.existing.yml up -d
```

## 配置文件

创建相应的环境变量文件：
- `.env.production` - 生产环境变量
- 参考模板：`.env.production.template`

## 注意事项

1. 生产环境请使用强密码
2. 定期备份数据库
3. 监控系统资源使用情况
4. 配置日志轮转