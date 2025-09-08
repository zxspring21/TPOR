# TPOR Intranet System

一個基於 ASP.NET 8.0 的企業內網文件處理系統，包含 API 服務和 Worker 服務，支持本地開發、Docker 環境和 Google Cloud 生產部署。

## 🏗️ 系統架構

### 核心服務
- **TPOR-Intranet-API**: REST API 服務，處理文件上傳和身份驗證
- **TPOR-Intranet-Worker**: 後台服務，處理文件解析和數據庫操作

### 環境配置

| 環境 | API 數據庫 | Worker 數據庫 | 文件存儲 | 消息隊列 | 身份驗證 |
|------|------------|---------------|----------|----------|----------|
| **Development Local** | ❌ 無 | ❌ 無 | 本地文件 | Mock | 環境變數 |
| **Development Docker** | ✅ 有 | ✅ 有 | 本地文件 | Mock | 環境變數 |
| **Production** | ✅ 有 | ✅ 有 | Cloud Storage | Pub/Sub | Secret Manager |

## 📁 文件命名規則

系統支持以下格式的 ZIP 文件：
```
{customerCode}_{projectCode}_{tester}_{lot}_{wafer}_{testprogram}_timestamp.zip
```

**範例**: `ACME_PROJ1_T100_L001_W01_TP001_20240101120000.zip`

## 🗄️ 數據庫表結構

系統會自動創建以下數據表：
- `RefCustomers` - 客戶信息
- `RefTesters` - 測試設備信息  
- `RefTestPrograms` - 測試程序信息
- `RefFamilies` - 產品系列信息
- `RefWafers` - 晶圓信息
- `RefLots` - 批次信息
- `RefRefreshTokens` - 刷新令牌
- `BucketObjectLogs` - 文件處理日誌
- `DataLotAttributes` - 批次屬性

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

### 2. 本地開發 (推薦)

```powershell
# 設置本地環境
pwsh scripts/setup-environment.ps1 -Environment development-local

# 啟動 API 服務
cd src/TPOR.Intranet.API
dotnet run

# 啟動 Worker 服務 (新終端)
cd src/TPOR.Intranet.Worker  
dotnet run
```

### 3. Docker 開發

```powershell
# 設置 Docker 環境
pwsh scripts/setup-environment.ps1 -Environment development-docker

# 啟動所有服務
docker-compose up -d
```

### 4. 測試服務

```powershell
# 測試 API 服務
pwsh scripts/test-api-by-environment.ps1 -Environment development-local

# 測試 Worker 服務
pwsh scripts/test-worker-by-environment.ps1 -Environment development-local

# 驗證環境設置
pwsh scripts/test-environment.ps1 -Environment development-local
```

**訪問服務**：
- API 文檔：http://localhost:5001/swagger/index.html
- 健康檢查：http://localhost:5001/api/health
- 根目錄：http://localhost:5001/

## 📂 項目結構

```
TPOR/
├── src/                          # 源代碼
│   ├── TPOR.Intranet.API/        # API 服務
│   ├── TPOR.Intranet.Worker/     # Worker 服務
│   └── TPOR.Shared/              # 共享庫
├── env-configs/                  # 環境配置文件
│   ├── api-development-local.env.example
│   ├── api-development-docker.env.example
│   ├── api-production.env.example
│   ├── worker-development-local.env.example
│   ├── worker-development-docker.env.example
│   └── worker-production.env.example
├── scripts/                      # 腳本文件
│   ├── setup-environment.ps1     # 環境設置
│   ├── test-api-by-environment.ps1  # API 測試
│   ├── test-worker-by-environment.ps1  # Worker 測試
│   ├── test-environment.ps1      # 環境驗證
│   └── deploy.sh                 # 部署腳本
├── cloud-run/                    # Google Cloud Run 配置
├── README.md                     # 項目說明
├── ENVIRONMENT_SETUP_GUIDE.md    # 環境設置指南
├── PROJECT_SUMMARY.md            # 項目摘要
└── docker-compose.yml            # Docker 配置
```

## 🔧 API 使用說明

### 身份驗證

1. **獲取 JWT Token**
```bash
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "password"}'
```

2. **使用 Token 上傳文件**
```bash
curl -X POST http://localhost:5001/api/fileupload/upload \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "file=@your-file.zip"
```

### 文件處理流程

1. 客戶端上傳 ZIP 文件到 API
2. API 驗證文件格式和 JWT Token
3. API 保存文件到存儲系統
4. API 發送消息到消息隊列
5. Worker 接收消息並處理文件
6. Worker 解析文件名並更新數據庫
7. Worker 重命名文件（添加下劃線前綴）

## ☁️ 部署到 Google Cloud Run

### 準備工作

1. **安裝 Google Cloud SDK**
2. **配置項目 ID**
```bash
export GOOGLE_CLOUD_PROJECT_ID="your-project-id"
```

3. **創建必要的 Google Cloud 資源**
```bash
# 創建 Pub/Sub Topic
gcloud pubsub topics create file-processing-topic-prod

# 創建 Pub/Sub Subscription  
gcloud pubsub subscriptions create file-processing-subscription-prod \
  --topic=file-processing-topic-prod

# 創建 Cloud Storage Bucket
gsutil mb gs://tpor-intranet-storage-prod

# 創建 Secret Manager Secret
echo "your-jwt-secret" | gcloud secrets create jwt-secret-prod --data-file=-
```

### 部署

```bash
./scripts/deploy.sh
```

## 🔐 環境變量配置

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

## 🔍 監控和日誌

- **應用日誌**: ASP.NET Core 內建日誌系統
- **數據庫操作日誌**: 記錄在 `BucketObjectLogs` 表中
- **Google Cloud 日誌**: 通過 Cloud Logging 查看

## 🛠️ 故障排除

### 常見問題

1. **文件上傳失敗**
   - 檢查文件格式是否為 ZIP
   - 檢查文件名是否符合命名規則
   - 檢查 JWT Token 是否有效

2. **Worker 無法處理文件**
   - 檢查消息隊列連接
   - 檢查數據庫連接
   - 查看應用日誌

3. **數據庫連接問題**
   - 檢查連接字符串
   - 確認數據庫服務運行正常
   - 檢查網絡連接

## 🛠️ 技術棧

- **.NET 8.0**: 主要開發框架
- **ASP.NET Core**: Web API 框架
- **Entity Framework Core**: ORM 框架
- **MySQL**: 數據庫
- **Google Cloud**: 雲服務平台
- **Docker**: 容器化
- **JWT**: 身份驗證

## 📚 相關文檔

- [環境設置指南](ENVIRONMENT_SETUP_GUIDE.md) - 詳細的環境配置和步驟說明

## 🤝 貢獻指南

1. Fork 項目
2. 創建功能分支
3. 提交更改
4. 推送到分支
5. 創建 Pull Request

## 📄 許可證

此項目使用 MIT 許可證。