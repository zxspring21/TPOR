# TPOR Intranet Environment Setup Guide

本指南提供三種環境的詳細設置步驟和測試說明。

## 🚀 快速開始

### 1. 環境設置
```powershell
# 本地開發環境 (無數據庫依賴)
pwsh scripts/setup-environment.ps1 -Environment development-local

# Docker 開發環境 (需要數據庫)
pwsh scripts/setup-environment.ps1 -Environment development-docker

# 生產環境 (Google Cloud)
pwsh scripts/setup-environment.ps1 -Environment production
```

### 2. 測試服務
```powershell
# 測試 API 服務
pwsh scripts/test-api-by-environment.ps1 -Environment development-local

# 測試 Worker 服務
pwsh scripts/test-worker-by-environment.ps1 -Environment development-local
```

## 📋 環境詳細設置

### Development Local (本地開發)

**特點**：
- ✅ API 和 Worker 都無數據庫依賴
- ✅ 本地文件存儲
- ✅ Mock 消息隊列
- ✅ 環境變數認證

**步驟**：

1. **設置環境**
```powershell
pwsh scripts/setup-environment.ps1 -Environment development-local
```

2. **啟動 API 服務**
```powershell
cd src/TPOR.Intranet.API
dotnet run
```

3. **啟動 Worker 服務** (新終端)
```powershell
cd src/TPOR.Intranet.Worker
dotnet run
```

4. **測試服務**
```powershell
# 測試 API
pwsh scripts/test-api-by-environment.ps1 -Environment development-local

# 測試 Worker
pwsh scripts/test-worker-by-environment.ps1 -Environment development-local
```

**訪問服務**：
- API 文檔：http://localhost:5001/swagger
- 健康檢查：http://localhost:5001/health

### Development Docker (Docker 開發)

**特點**：
- ✅ Docker MySQL 數據庫
- ✅ 本地文件存儲
- ✅ Mock 消息隊列
- ✅ 環境變數認證

**步驟**：

1. **設置環境**
```powershell
pwsh scripts/setup-environment.ps1 -Environment development-docker
```

2. **啟動 Docker 服務**
```powershell
docker-compose up -d
```

3. **等待 MySQL 就緒**
```powershell
docker-compose logs mysql
```

4. **啟動應用服務**
```powershell
docker-compose up
```

5. **測試服務**
```powershell
# 測試 API
pwsh scripts/test-api-by-environment.ps1 -Environment development-docker

# 測試 Worker
pwsh scripts/test-worker-by-environment.ps1 -Environment development-docker
```

### Production (生產環境)

**特點**：
- ✅ Google Cloud SQL 數據庫
- ✅ Google Cloud Storage
- ✅ Google Pub/Sub 消息隊列
- ✅ Google Secret Manager 認證

**步驟**：

1. **設置環境**
```powershell
pwsh scripts/setup-environment.ps1 -Environment production
```

2. **配置 Google Cloud 憑證**
```bash
export GOOGLE_APPLICATION_CREDENTIALS="path/to/your/service-account-key.json"
export GOOGLE_CLOUD_PROJECT_ID="your-project-id"
```

3. **創建 Google Cloud 資源**
```bash
# 創建 Pub/Sub Topic
gcloud pubsub topics create file-processing-topic-prod

# 創建 Pub/Sub Subscription
gcloud pubsub subscriptions create file-processing-subscription-prod \
  --topic=file-processing-topic-prod

# 創建 Cloud Storage Bucket
gsutil mb gs://tpor-intranet-storage-prod

# 創建 Secret Manager Secrets
echo "your-jwt-secret" | gcloud secrets create jwt-secret-prod --data-file=-
echo "admin" | gcloud secrets create tpor-auth-username --data-file=-
echo "password" | gcloud secrets create tpor-auth-password --data-file=-
```

4. **部署到 Google Cloud Run**
```bash
pwsh scripts/deploy.sh
```

## 🔧 環境變數配置

### Development Local
```bash
JWT_SECRET=dev-super-secret-jwt-key-that-is-at-least-32-characters-long
AUTH_USERNAME=admin
AUTH_PASSWORD=password
LOCAL_STORAGE_PATH=uploads/dev
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5001
```

### Development Docker
```bash
JWT_SECRET=dev-super-secret-jwt-key-that-is-at-least-32-characters-long
AUTH_USERNAME=admin
AUTH_PASSWORD=password
LOCAL_STORAGE_PATH=uploads/dev
DATABASE_CONNECTION_STRING=Server=mysql;Port=3306;Database=TPOR_Dev;Uid=root;Pwd=password;
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5001
```

### Production
```bash
GOOGLE_CLOUD_PROJECT_ID=your-project-id
PUBSUB_TOPIC_NAME=file-processing-topic-prod
BUCKET_NAME=tpor-intranet-storage-prod
JWT_SECRET_NAME=jwt-secret-prod
AUTH_USERNAME_SECRET_NAME=tpor-auth-username
AUTH_PASSWORD_SECRET_NAME=tpor-auth-password
DATABASE_CONNECTION_STRING=Server=your-cloud-sql-ip;Database=TPOR_Prod;Uid=user;Pwd=password;
ASPNETCORE_ENVIRONMENT=Production
```

## 🧪 測試說明

### API 服務測試
- ✅ 健康檢查端點
- ✅ JWT Token 生成
- ✅ JWT Token 驗證
- ✅ 文件上傳功能
- ✅ 環境特定認證

### Worker 服務測試
- ✅ 環境配置檢查
- ✅ 數據庫連接 (如果需要)
- ✅ 文件存儲訪問
- ✅ 服務構建和啟動
- ✅ 環境特定認證

## 🔐 安全注意事項

### 開發環境
- 憑證存儲在 `.env` 文件中
- JWT 密鑰為開發用途硬編碼
- 數據庫密碼為明文
- **永遠不要將 `.env` 文件提交到版本控制**

### 生產環境
- 所有密鑰存儲在 Google Secret Manager
- JWT 密鑰動態檢索
- 數據庫連接字符串加密
- 憑證由 Google Cloud IAM 管理

## 🛠️ 故障排除

### 常見問題

1. **API 服務無法啟動**
   - 檢查端口 5001 是否可用
   - 確保 `.env` 文件中設置了 JWT_SECRET
   - 驗證環境配置

2. **Worker 服務數據庫錯誤**
   - 確保 MySQL 正在運行
   - 檢查 `.env` 文件中的 DATABASE_CONNECTION_STRING
   - 運行數據庫設置腳本

3. **認證失敗**
   - 驗證 `.env` 文件中的 AUTH_USERNAME 和 AUTH_PASSWORD
   - 檢查 JWT_SECRET 配置
   - 確保環境變數正確加載

4. **文件上傳問題**
   - 檢查 LOCAL_STORAGE_PATH 是否存在
   - 驗證文件權限
   - 確保 JWT Token 有效

### 環境特定問題

**Development Local**：
- API 服務應在無數據庫情況下啟動
- Worker 服務需要 MySQL 連接
- 文件存儲使用本地目錄

**Development Docker**：
- 兩個服務都需要數據庫連接
- MySQL 容器必須運行
- 文件存儲使用本地目錄

**Production**：
- 所有服務都需要 Google Cloud 憑證
- 密鑰必須在 Secret Manager 中配置
- 文件存儲使用 Google Cloud Storage

## 📚 相關文檔

- [README.md](README.md) - 主要項目文檔
- [API 使用說明](README.md#-api-使用說明) - API 詳細使用指南
- [部署指南](README.md#-部署到-google-cloud-run) - 生產環境部署說明