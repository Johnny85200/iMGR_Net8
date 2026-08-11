# iMGR2 .NET 8 现代化工程

本工作区是旧版 `C:\0_Fork\iMGR2` 的渐进式重构起点。当前已搭建可独立部署的 `Identity Service` 和 `Employee Service`。Identity 负责登录和 JWT；Employee 负责组织与员工主数据读取，包括员工档案、任职、组织、合同、银行、家属、技能、资历、工作经历和文档元数据。前端暂不在本阶段范围内。

## 当前交付

- 目标框架：.NET 8、ASP.NET Core Web API、EF Core 8、SQL Server。
- 分层：Domain / Application / Infrastructure / API，业务逻辑不依赖 Web 或 EF Core。
- 数据访问：旧库统一采用 Database First；数据库实体集中在 `iMGR.Database`，由 Identity 和 Employee 共用，API 不直接暴露这些实体。
- 旧库兼容：映射租户库的 `dbo.Users`、`dbo.SystemParameter`、`dbo.LoginAudit`，以及 `_imgrCtrl` 的 `CompanyDatabase`、`DatabaseServer`；不自动执行 migration。
- 登录：`POST /api/auth/login`。
- 当前用户：`GET /api/auth/User`，需要 Bearer Token。
- 员工查询：`GET /api/employees`、`GET /api/employees/{employeeId}`，使用 Identity JWT 中的 `tenant` claim。
- 组织查询：`GET /api/organizations`，可按公司和层级过滤。
- 运维：`GET /health/live`、Swagger `/swagger`、RFC 7807 Problem Details。
- 安全：登录端点固定窗口限流；JWT 签名密钥必须至少 32 字符；移除了旧系统中的硬编码通用密码分支。
- 测试：覆盖旧 SHA1 校验、PBKDF2、成功登录、失败锁定、审计和强制改密判断。

## 解决方案结构

```text
iMGR.Modernization.sln
├─ src/BuildingBlocks
│  └─ iMGR.Database
├─ src/Services/Identity
│  ├─ iMGR.Identity.Domain
│  ├─ iMGR.Identity.Application
│  ├─ iMGR.Identity.Infrastructure
│  └─ iMGR.Identity.Api
├─ src/Services/Employee
│  ├─ iMGR.Employee.Domain
│  ├─ iMGR.Employee.Application
│  ├─ iMGR.Employee.Infrastructure
│  └─ iMGR.Employee.Api
├─ tests/Services
│  ├─ Identity/iMGR.Identity.UnitTests
│  └─ Employee/iMGR.Employee.UnitTests
├─ docs/analysis
├─ docs/architecture
└─ infra/sql
```

后续每个微服务沿用相同四层结构。`iMGR.Database` 是针对同一套旧租户数据库的共享适配层；业务 Domain、DTO 和业务逻辑仍不跨服务共享。

## 配置与启动

SaaS 模式只配置 `_imgrCtrl` 连接。登录请求中的 `tenantCode` 会匹配 `CompanyDatabase.CompanyDBClientCode`，服务随后解密 `DatabaseServer` 字段并创建租户数据库连接。建议使用环境变量或 Secret Store，不要把密码提交到配置文件：

```powershell
$env:Identity__ControlDatabase__ConnectionString = 'Server=...;Database=_imgrCtrl;User Id=...;Password=...;TrustServerCertificate=True'
$env:Identity__ControlDatabase__LegacyEncryptionKey = 'replace-with-the-key-used-by-your-imgr2-deployment'
$env:Identity__Jwt__SigningKey = 'replace-with-a-random-secret-of-at-least-32-characters'
dotnet run --project C:\0_Fork\iMGR2_Net8\src\Services\Identity\iMGR.Identity.Api\iMGR.Identity.Api.csproj
```

Employee Service 必须使用与 Identity Service 相同的 JWT signing key，并需要旧租户字段解密密钥：

```powershell
$env:Employee__ControlDatabase__ConnectionString = 'Server=...;Database=_imgrCtrl;User Id=...;Password=...;TrustServerCertificate=True'
$env:Employee__ControlDatabase__LegacyEncryptionKey = 'replace-with-the-key-used-by-your-imgr2-deployment'
$env:Authentication__SigningKey = 'the-same-signing-key-used-by-identity-service'
dotnet run --project C:\0_Fork\iMGR2_Net8\src\Services\Employee\iMGR.Employee.Api\iMGR.Employee.Api.csproj
```

调用登录：

```http
POST /api/auth/login
Content-Type: application/json

{
  "tenantCode": "customer1",
  "loginId": "admin",
  "password": "admin"
}
```

`ControlDatabase.ConnectionString` 非空时，`_imgrCtrl` 是权威租户来源，不会因客户代码查不到而回退到静态连接。只有该连接为空时，服务才使用 `Identity:Tenants:{tenantCode}:ConnectionString`，用于本地开发、单租户或非 SaaS 安装。

旧字段使用 iMGR2 兼容的 Rijndael/AES 解密逻辑，但解密密钥不会进入源代码仓库。部署时必须通过 `Identity:ControlDatabase:LegacyEncryptionKey` 提供当前环境实际使用的密钥。`RequireImgrEnabled` 默认关闭以保持旧登录行为；启用后还会要求 `CompanyDBHasIMGR` 为真。

## 密码兼容策略

旧系统密码是 `Base64(SHA1(UTF-16LE(password)))`。新服务能够验证这种格式，也支持 `PBKDF2-SHA256`。

`Identity:UpgradeLegacyPasswordHashes` 默认是 `false`。原因是旧 Web Forms 登录仍只识别 SHA1；在新旧系统并行期间直接覆盖 `Users.UserPassword` 会导致用户无法再登录旧站点。只有完成登录入口切换后，才可以把此开关设为 `true`，让用户在成功登录时升级哈希。更长期的方案是由 Identity Service 独占凭据表。

## 验证

```powershell
dotnet build C:\0_Fork\iMGR2_Net8\iMGR.Modernization.sln
dotnet test C:\0_Fork\iMGR2_Net8\iMGR.Modernization.sln
```

接入数据库前先运行只读预检脚本：[control-database-preflight.sql](infra/sql/control-database-preflight.sql)、[identity-preflight.sql](infra/sql/identity-preflight.sql) 和 [employee-preflight.sql](infra/sql/employee-preflight.sql)。

数据库结构变更后，在 Visual Studio 中右键 `iMGR.Database`，使用 EF Core Power Tools 的 **Compare DbContext to Database** 检查差异，再通过 **Reverse Engineer / Refresh** 更新共享实体。详细操作见 [Database First 分层与更新规范](docs/architecture/database-first-guidelines.md)。

## 设计文档

- [旧系统分析](docs/analysis/legacy-system-analysis.md)
- [组织与员工主数据分析](docs/analysis/employee-master-data-analysis.md)
- [目标微服务架构与迁移路线](docs/architecture/target-architecture.md)
- [Database First 分层与更新规范](docs/architecture/database-first-guidelines.md)
