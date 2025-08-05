# 开发环境配置

## 快速启动

```bash
# 启动开发环境
cd environments/development
docker-compose -f docker-compose.dev.yml up -d

# 查看日志
docker-compose -f docker-compose.dev.yml logs -f

# 停止环境
docker-compose -f docker-compose.dev.yml down
```

## 访问地址

- 前端: http://localhost:3000
- 后端API: http://localhost:5006
- 数据库: localhost:1433

## 默认账户

- 数据库SA密码: `DevPass123!`
- JWT密钥: `DevSecretKeyForDevelopment123456789`

## 注意事项

- 该配置仅用于开发环境
- 生产环境请使用根目录的docker-compose.yml
- 修改配置后需要重新构建容器