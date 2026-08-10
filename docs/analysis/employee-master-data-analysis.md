# iMGR2 组织与员工主数据分析

## 分析范围与证据

本分析基于旧工程 `C:\0_Fork\iMGR2`，主要证据来自：

- `HROne.Lib\Entities`：旧 ORM 实体、必填规则、字段加密标记。
- `HROneWeb\EmptyDatabase\HROneDBScheme.sql`：基础表和字段定义。
- `HROneWeb\controls\Emp_LeftMenu.ascx`：员工档案功能导航与业务分组。
- `HROne.Lib\AppUtils.GetLastPositionInfo`：截至日期任职选择规则。
- `HROne.DataAccess\Attribute\DBAESEncryptStringFieldAttribute.cs`：租户数据字段加解密方式。
- `HROneWeb\App_Code\TempEmployeeData.cs`、`TempEmpPositionData.cs`：员工资料暂存、提交和审批流程。

## 业务边界

员工主数据以 `EmpPersonalInfo.EmpID` 为聚合根，但旧页面把信息拆成多个页签。Employee Service 将这些内容归入同一个服务边界，因为它们共享员工生命周期、权限、审计和租户数据库。

| 业务能力 | 旧表 | 说明 |
| --- | --- | --- |
| 员工档案 | `EmpPersonalInfo` | 工号、状态、姓名、证件、联系方式、入职/服务/试用日期 |
| 任职历史 | `EmpPositionInfo` | 按生效日期保存公司、职位、职级、雇佣类型等历史记录 |
| 组织归属 | `EmpHierarchy`、`HierarchyElement`、`HierarchyLevel` | 每条任职可同时归属多个组织层级，例如部门、分部、地点 |
| 组织字典 | `Company`、`Position`、`Rank`、`EmploymentType` | 任职所引用的公司和主数据字典 |
| 合同 | `EmpContractTerms` | 合同公司、期间、酬金、币种及计算方式 |
| 银行 | `EmpBankAccount`、`BankList` | 可有多个账户，并以 `EmpAccDefault` 标识默认账户 |
| 家属 | `EmpSpouse`、`EmpDependant` | 配偶与受养人是两类独立记录 |
| 紧急联系人 | `EmpEmergencyContact` | 关系及日间/夜间联系电话 |
| 技能 | `EmpSkill`、`Skill`、`SkillLevel` | 员工、技能、技能等级的关联 |
| 学历/资历 | `EmpQualification`、`Qualification` | 资历类型、院校、期间、学习方式 |
| 工作经历 | `EmpWorkExp` | 前雇主、职位、年月、雇佣类型、相关经验标记 |
| 文档 | `EmpDocument`、`DocumentType` | 文件元数据、压缩和头像标记；文件本体由旧文件存储管理 |

薪资、假期、考勤、培训、福利、养老金、税务和离职结算虽然也出现在员工菜单下，但具有独立规则和生命周期，不纳入 Employee Service 主数据首版；它们应由后续 Payroll、Leave、Attendance 等服务拥有，通过员工 ID 引用员工。

## 关键旧逻辑

### 员工状态

`EmpStatus` 的主要代码为：

- `A`：Active。
- `T`：Terminated。
- `P`：Pending。

旧 `GetActualEmpStatus` 还会结合未来生效的离职记录，将尚未到最后工作日的员工视为 Active。首版 API 返回表内状态；迁移离职服务后，应由离职领域提供“截至日期实际状态”。

### 截至日期任职

旧 `AppUtils.GetLastPositionInfo(date, employeeId)` 的规则不是简单查 `EmpPosEffTo IS NULL`：

1. 优先选择 `EmpPosEffFr <= asOfDate` 中生效日最新的记录。
2. 如果没有历史记录，选择 `EmpPosEffTo IS NULL` 的未来记录中生效日最新的一条。

Employee Domain 的 `EmploymentAssignmentSelector` 保留了这个行为，以兼容未来入职员工。

### 组织模型

旧组织不是一棵有父节点的树。`HierarchyLevel` 定义维度及顺序，`HierarchyElement` 是公司下某个维度的元素，`EmpHierarchy` 把一条员工任职关联到多个层级元素。因此 API 返回按层级顺序排列的组织归属数组，不伪造父子关系。

### 员工修改审批

旧系统存在 `TempEmp*` 表和 `P/A/T` 审批状态，员工及任职资料先暂存后批准写入正式表。直接对正式表开放 CRUD 会绕过审批、审计及生效日期闭合规则。因此首版只开放读取；写 API 必须在下一阶段先明确：

- 草稿、提交、批准、拒绝状态机。
- 任职日期重叠和上一条任职结束日期规则。
- 工号唯一性及公司/职位/层级有效性。
- 审计操作者与修改前后值。
- 文档文件本体的病毒扫描、大小限制及对象存储。

## 加密与隐私

大量姓名、证件、地址、联系方式、银行和组织字典字段标记为 `DBAESEncryptStringField`。其格式为固定密钥/IV 的旧 Rijndael/AES CBC，并允许旧库中同时存在明文。Employee Infrastructure 会在读出后兼容解密，但密钥必须来自 Secret Store。

首版 API 采取以下保护：

- JWT 必须由 Identity Service 的相同 issuer、audience 和 signing key 验证。
- 租户只能来自 JWT 的 `tenant` claim，调用方不能通过 query string 跨租户切换。
- 身份证、护照和银行账号默认掩码。
- 文档只返回元数据，不暴露旧存储文件名或文件本体。
- 当前 JWT 尚无细粒度 HR 权限，因此写入和未掩码 PII API 暂不开放。

## 首版 API

| 方法 | 路径 | 用途 |
| --- | --- | --- |
| `GET` | `/api/employees` | 分页查询；支持状态、截至日期和精确工号/姓名搜索 |
| `GET` | `/api/employees/{employeeId}` | 返回员工档案、任职、组织、合同、银行、家属、技能、资历、经历和文档元数据 |
| `GET` | `/api/organizations` | 查询组织元素，可按 `companyId`、`levelId` 过滤 |
| `GET` | `/health/live` | 进程存活检查，不访问租户库 |

Employee Service 自己拥有 Domain、Application、Infrastructure 和 API，不引用 Identity Service 的业务程序集。两个服务只通过 JWT 契约和相同的租户注册规则协作，因而可独立部署。
