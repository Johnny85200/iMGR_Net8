/*
  Employee Service read-only schema preflight.
  This script returns metadata and row counts only; it does not return employee PII.
*/
SET NOCOUNT ON;

DECLARE @RequiredTables TABLE ([TableName] sysname NOT NULL PRIMARY KEY);
INSERT INTO @RequiredTables ([TableName]) VALUES
('EmpPersonalInfo'), ('EmpPositionInfo'), ('EmpHierarchy'),
('Company'), ('Position'), ('Rank'), ('EmploymentType'),
('HierarchyLevel'), ('HierarchyElement'), ('EmpContractTerms'),
('EmpBankAccount'), ('BankList'), ('EmpSpouse'), ('EmpDependant'),
('EmpEmergencyContact'), ('EmpSkill'), ('Skill'), ('SkillLevel'),
('EmpQualification'), ('Qualification'), ('EmpWorkExp'),
('EmpDocument'), ('DocumentType');

SELECT required.[TableName],
       CASE WHEN actual.[object_id] IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END AS [Exists]
FROM @RequiredTables required
LEFT JOIN sys.tables actual
  ON actual.[name] = required.[TableName]
 AND SCHEMA_NAME(actual.[schema_id]) = 'dbo'
ORDER BY required.[TableName];

DECLARE @RequiredColumns TABLE ([TableName] sysname NOT NULL, [ColumnName] sysname NOT NULL);
INSERT INTO @RequiredColumns ([TableName], [ColumnName]) VALUES
('EmpPersonalInfo', 'EmpID'), ('EmpPersonalInfo', 'EmpNo'), ('EmpPersonalInfo', 'EmpStatus'),
('EmpPositionInfo', 'EmpPosID'), ('EmpPositionInfo', 'EmpID'),
('EmpPositionInfo', 'EmpPosEffFr'), ('EmpPositionInfo', 'EmpPosEffTo'),
('EmpHierarchy', 'EmpPosID'), ('EmpHierarchy', 'HElementID'),
('HierarchyElement', 'HLevelID'), ('HierarchyElement', 'CompanyID'),
('EmpBankAccount', 'EmpAccDefault'), ('EmpDocument', 'EmpDocumentOriginalFileName');

SELECT required.[TableName], required.[ColumnName],
       CASE WHEN columns.[column_id] IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END AS [Exists]
FROM @RequiredColumns required
LEFT JOIN sys.tables tables
  ON tables.[name] = required.[TableName]
 AND SCHEMA_NAME(tables.[schema_id]) = 'dbo'
LEFT JOIN sys.columns columns
  ON columns.[object_id] = tables.[object_id]
 AND columns.[name] = required.[ColumnName]
ORDER BY required.[TableName], required.[ColumnName];

IF OBJECT_ID('dbo.EmpPersonalInfo', 'U') IS NOT NULL
    SELECT COUNT_BIG(*) AS [EmployeeCount] FROM dbo.EmpPersonalInfo;

IF OBJECT_ID('dbo.EmpPositionInfo', 'U') IS NOT NULL
    SELECT COUNT_BIG(*) AS [PositionHistoryCount] FROM dbo.EmpPositionInfo;
