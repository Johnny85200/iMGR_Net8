# Database First 与 EF Core Power Tools

旧 iMGR 数据库是数据库结构的权威来源。Identity、Employee 及后续微服务共同使用 `iMGR.LegacyData` 中的 EF Core 数据库实体和 DbContext，但各服务仍保留独立的 Domain、Application DTO 和 API 合同。

```text
旧租户数据库 / _imgrCtrl
            ↓ EF Core Power Tools GUI
       iMGR.LegacyData
       ├─ Tenant/LegacyTenantDbContext
       ├─ Tenant/Entities
       ├─ Control/LegacyControlDbContext
       └─ Control/Entities
            ↓ 显式映射
 Identity Domain          Employee Domain
            ↓                   ↓
 Identity API             Employee API
```

## Visual Studio 图形界面

EF Core 本身没有 EF6 `.edmx` 设计器。Visual Studio 2022 安装 **EF Core Power Tools** 扩展后，可以在解决方案资源管理器中右键 `iMGR.LegacyData` 项目操作模型。

### 检查实体和数据库是否一致

1. 右键 `iMGR.LegacyData`。
2. 选择 **EF Core Power Tools → Compare DbContext to Database**。
3. 租户库选择 `LegacyTenantDbContext`，并连接一个代表性的租户数据库。
4. 控制库选择 `LegacyControlDbContext`，并连接 `_imgrCtrl`。
5. 查看缺少的表、列以及类型、长度、可空性等差异。

Compare 只生成差异报告，不修改数据库或 C# 文件。

### 用数据库刷新项目实体

1. 右键 `iMGR.LegacyData`，选择 **EF Core Power Tools → Reverse Engineer**。
2. 选择 SQL Server 连接和需要维护的全部表。后续新增表时必须保留原有表的选择。
3. DbContext 分别使用 `LegacyTenantDbContext` 或 `LegacyControlDbContext`。
4. 实体目录分别设置为 `Tenant/Entities` 或 `Control/Entities`。
5. 不要在生成代码中保存连接字符串。
6. 完成后工具会在项目中保存 `efpt.config.json`。以后右键该配置文件选择 **Refresh** 即可重新生成。
7. 刷新后检查 Git diff，更新服务中的 LINQ 映射和测试，再提交代码。

共享项目包含两个 DbContext。EF Core Power Tools 支持为多个 DbContext 保存多个 `efpt.*.config.json`，租户库和控制库应分别保存配置。

## 边界规则

- `iMGR.LegacyData` 只包含数据库实体、DbContext 和映射，不放业务规则。
- Employee 与 Identity 可以共享 EF 实体，但不能直接返回这些实体作为 API 响应。
- Domain 和 DTO 继续由各微服务独立维护。
- 不使用 EF Migration 修改旧 iMGR 数据库。
- 刷新实体前先确认各租户数据库的 schema 版本一致。
- 自动生成文件中的自定义逻辑应放到 partial class 或独立映射类，避免刷新时丢失。
