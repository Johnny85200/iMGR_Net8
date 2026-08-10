/*
  _imgrCtrl preflight for the Identity Service tenant registry.
  Read-only: this script does not return database passwords and does not alter data or schema.
*/
SET NOCOUNT ON;

SELECT
    DB_NAME() AS [DatabaseName],
    @@SERVERNAME AS [ServerName],
    SYSDATETIME() AS [CheckedAt];

SELECT
    required.[TableName],
    CASE WHEN actual.[object_id] IS NULL THEN 0 ELSE 1 END AS [Exists]
FROM (VALUES ('CompanyDatabase'), ('DatabaseServer')) required([TableName])
LEFT JOIN sys.tables actual
    ON actual.[name] = required.[TableName]
   AND SCHEMA_NAME(actual.[schema_id]) = 'dbo';

SELECT
    columns.[TABLE_NAME],
    columns.[COLUMN_NAME],
    columns.[DATA_TYPE],
    columns.[CHARACTER_MAXIMUM_LENGTH],
    columns.[IS_NULLABLE]
FROM INFORMATION_SCHEMA.COLUMNS columns
WHERE columns.[TABLE_SCHEMA] = 'dbo'
  AND
  (
      (columns.[TABLE_NAME] = 'CompanyDatabase'
       AND columns.[COLUMN_NAME] IN
           ('CompanyDBID', 'CompanyDBClientCode', 'DBServerID', 'CompanyDBSchemaName',
            'CompanyDBIsActive', 'CompanyDBHasIMGR'))
      OR
      (columns.[TABLE_NAME] = 'DatabaseServer'
       AND columns.[COLUMN_NAME] IN
           ('DBServerID', 'DBServerDBType', 'DBServerLocation', 'DBServerUserID', 'DBServerPassword'))
  )
ORDER BY columns.[TABLE_NAME], columns.[ORDINAL_POSITION];

SELECT
    company.[CompanyDBClientCode],
    COUNT_BIG(*) AS [DuplicateCount]
FROM dbo.[CompanyDatabase] company
GROUP BY company.[CompanyDBClientCode]
HAVING COUNT_BIG(*) > 1;

SELECT
    company.[CompanyDBID],
    company.[CompanyDBClientCode],
    company.[DBServerID],
    company.[CompanyDBIsActive],
    company.[CompanyDBHasIMGR]
FROM dbo.[CompanyDatabase] company
LEFT JOIN dbo.[DatabaseServer] server
    ON server.[DBServerID] = company.[DBServerID]
WHERE company.[DBServerID] IS NULL
   OR server.[DBServerID] IS NULL
ORDER BY company.[CompanyDBClientCode];
