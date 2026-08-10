# iMGR2 旧系统分析

## 1. 分析范围与结论

分析对象是只读目录 `C:\0_Fork\iMGR2`。它不是一个单一项目，而是一个以 ASP.NET Web Forms Web Site `HROneWeb` 为入口、配合二十多个 .NET Framework 2.0 类库和若干 Windows/Console 工具的 HR/Payroll 单体系统。

代码规模快照：

| 项目 | 数量 |
|---|---:|
| C# 文件（排除 bin/obj/packages/Backup） | 1,525 |
| Web Forms `.aspx` | 473 |
| HTTP Handler `.ashx` | 15 |
| 旧库基线表 | 182 |
| 数据库补丁 SQL | 317 |
| 系统功能定义 | 151 |

这套系统已经有业务分层的“痕迹”，例如 `HROne.Lib.Payroll`、`HROne.Lib.Leave`、`HROne.Lib.Attendance` 和三套 Reports 类库；但它们共享数据库、共享实体、共享 Session、共享 Web 工程，因此仍属于部署和数据层面的单体。

## 2. 主要业务拆解

系统功能种子数据中的主要分类包括 Personnel 22 项、System 21 项、Personnel Reports 17 项、Payroll 16 项、Payroll & MPF Reports 15 项、Attendance 15 项，其余为 Leave、Taxation、MPF、Security、Training、e-channel 等。结合页面、实体和类库，可归纳为以下业务边界：

| 业务域 | 旧代码证据 | 主要职责 | 建议目标服务 |
|---|---|---|---|
| 身份与权限 | `Users`、`UserGroup*`、`UserCompany`、`UserRank`、`LoginAudit`、SEC 功能 | 登录、用户、组、功能权限、数据范围 | Identity Service |
| 租户与平台控制面 | `HROne.SaaS`、`HROneSaaSManagementConsole`、`CompanyDatabase`、License | 客户代码、租户数据库路由、订阅、产品功能 | Tenant Service |
| 组织与员工主数据 | 153 个 `Emp_` 页面、`EmpPersonalInfo`、`Company`、`Hierarchy*`、`Position`、`Rank` | 员工档案、任职、组织、合同、银行、家属、技能、文档 | Employee Service |
| 假期 | `HROne.Lib.Leave`、`LeaveApplication`、`LeaveBalance*`、`LeavePlan*` | 假期规则、申请、余额、结转与现金化 | Leave Service |
| 考勤与排班 | `HROne.Lib.Attendance`、36 个 Attendance 页面、`Roster*`、`TimeCard*`、`ShiftDutyCode` | 打卡、考勤计算、班表、迟到豁免、工时 | Time Service |
| 薪资 | `HROne.Lib.Payroll`、132 个 Payroll 页面、`EmpPayroll`、`PaymentRecord`、`PayrollPeriod` | 薪资规则、试算、确认、调整、付款、银行文件 | Payroll Service |
| 法规与退休金 | `HROne.Taxation`、`HROne.MPFFile`、`MPF*`、`ORSO*`、`AVC*`、FinalPayment | 税务、MPF/ORSO、离职结算、合规文件 | Compliance Service |
| 工作流与任务 | `Authorizer*`、`AuthorizationGroup`、`Inbox`、`EmpRequest*`、TempEmp 审批 | 审批、委托、收件箱、后台任务 | Workflow Service |
| 报表 | 118 个 Report 页面、`HROne.Reports.Employee/Payroll/Taxation`、ReportTaskFactory | 报表定义、异步生成、导出与存档 | Reporting Service |
| 集成与导入导出 | `HROne.Import`、`HROne.GatewayClient`、`HROne.HSBC`、e-channel、PeopleHub handlers | 文件导入、银行/外部平台适配、Webhook/API | Integration Service |
| 通用配置与审计 | `SystemParameter`、`AuditTrail`、`EmailLog`、DBPatch | 参数、审计、通知、版本与数据库升级 | Platform 能力，按归属下沉到各服务 |

这里的“建议目标服务”是最终边界，不代表第一天就要同时部署十个服务。正确顺序是先建立 Identity，随后按员工、假期/考勤、薪资的业务依赖逐步切割。

## 3. 旧登录链路

入口位于 `HROneWeb\Login.aspx.cs`，核心校验位于 `HROneWeb\App_Code\WebUtils.cs::ValidateUser`，用户实体位于 `HROne.Lib\Entities\User.cs`。

旧登录依次执行：

1. 判断单库、多库或 SaaS 模式。
2. SaaS 模式使用客户代码查询主库 `CompanyDatabase`，拼出租户数据库连接串。
3. 读取租户库 `Users`，要求 LoginID 匹配旧正则且状态不是删除。
4. 比较 `UserPassword`；算法为 `Base64(SHA1(Encoding.Unicode(password)))`。
5. 密码失败时增加 `FailCount`，达到 `LOGIN_MAX_FAIL_COUNT` 后把状态改为 `I`。
6. 成功时清零失败次数，写入 `LoginAudit`。
7. 根据 `UserChangePassword` 及 D/M/Y 周期判断是否强制改密。
8. 把用户、租户数据库连接和密码哈希放入 ASP.NET Session，再跳转 `Default.aspx`。

新 Identity Service 已兼容第 2 至 7 项，并把 Session 替换为短期 JWT。SaaS 模式通过 EF Core/LINQ 查询 `_imgrCtrl.CompanyDatabase` 与 `DatabaseServer`，兼容解密旧 Rijndael/AES 字段；非 SaaS 模式可使用显式租户连接配置。

## 4. 发现的风险

| 风险 | 影响 | 当前处理 |
|---|---|---|
| SHA1 无盐密码 | 离线破解成本低 | 可验证旧哈希；切换登录入口后升级 PBKDF2 |
| `ValidateUser` 含硬编码通用密码分支 | 可绕过每个用户的真实密码 | 新服务完全移除，生产前应立即审计和删除旧分支 |
| Session 保存密码哈希和数据库连接 | 会话泄露面大，无法跨服务自然扩展 | JWT 只保存最小身份 claims |
| 登录逻辑混合租户路由、SMTP/路径同步、授权和页面跳转 | 无法独立测试或扩容 | 分为 Tenant Registry、LoginService、Store、TokenIssuer |
| 失败后 `Thread.Sleep(1000)` | 占用线程，不能替代限流 | API 使用固定窗口限流；后续接 Redis 分布式限流 |
| `Users` 表属于共享大库 | 服务边界仍被数据库耦合 | 第一阶段兼容映射；最终迁移到 Identity 独占库 |
| 旧 LoginID 正则最多 12 位、数据库列 20 位 | 规则与存储定义不一致 | 第一阶段保持旧行为，产品确认后再放宽 |
| `ExpiryDate` 存在但旧 `ValidateUser` 未校验 | 启用语义不明确 | 新服务暂不改变行为，需业务确认后启用 |
| 新旧系统并行时升级密码会破坏旧登录 | 用户被锁在新入口 | `UpgradeLegacyPasswordHashes=false` 默认关闭 |

## 5. 第一阶段范围

已纳入：登录、`_imgrCtrl` 租户解析、旧数据库服务器字段解密、显式配置回退、状态检查、失败锁定、审计、强制改密标记、JWT、当前用户、EF Core/LINQ、限流、OpenAPI、健康检查、测试。

暂未纳入：用户 CRUD、用户组与功能权限 claims、refresh token/撤销、忘记密码、MFA、租户解析缓存与独立 Tenant Service、分布式锁定计数、旧页面前端替换、数据库拆库。
