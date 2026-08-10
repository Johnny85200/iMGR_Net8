/*
  Identity Service legacy database preflight.
  Read-only: this script does not alter data or schema.
*/
SET NOCOUNT ON;

SELECT
    DB_NAME() AS [DatabaseName],
    @@SERVERNAME AS [ServerName],
    SYSDATETIME() AS [CheckedAt];

SELECT
    required.[TableName],
    CASE WHEN actual.[object_id] IS NULL THEN 0 ELSE 1 END AS [Exists]
FROM (VALUES ('Users'), ('SystemParameter'), ('LoginAudit')) required([TableName])
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
  AND columns.[TABLE_NAME] IN ('Users', 'SystemParameter', 'LoginAudit')
ORDER BY columns.[TABLE_NAME], columns.[ORDINAL_POSITION];

SELECT
    users.[LoginID],
    COUNT_BIG(*) AS [DuplicateCount]
FROM dbo.[Users] users
WHERE users.[UserAccountStatus] <> 'D'
GROUP BY users.[LoginID]
HAVING COUNT_BIG(*) > 1;

SELECT
    users.[UserAccountStatus],
    COUNT_BIG(*) AS [UserCount]
FROM dbo.[Users] users
GROUP BY users.[UserAccountStatus]
ORDER BY users.[UserAccountStatus];

SELECT
    CASE
        WHEN users.[UserPassword] LIKE '$PBKDF2-SHA256$%' THEN 'PBKDF2-SHA256'
        WHEN LEN(users.[UserPassword]) = 28 THEN 'Probable legacy SHA1 Base64'
        WHEN users.[UserPassword] IS NULL OR users.[UserPassword] = '' THEN 'Missing'
        ELSE 'Unknown'
    END AS [PasswordFormat],
    COUNT_BIG(*) AS [UserCount]
FROM dbo.[Users] users
GROUP BY
    CASE
        WHEN users.[UserPassword] LIKE '$PBKDF2-SHA256$%' THEN 'PBKDF2-SHA256'
        WHEN LEN(users.[UserPassword]) = 28 THEN 'Probable legacy SHA1 Base64'
        WHEN users.[UserPassword] IS NULL OR users.[UserPassword] = '' THEN 'Missing'
        ELSE 'Unknown'
    END;

SELECT
    parameters.[ParameterCode],
    CONVERT(nvarchar(100), parameters.[ParameterValue]) AS [ParameterValue]
FROM dbo.[SystemParameter] parameters
WHERE parameters.[ParameterCode] = 'LOGIN_MAX_FAIL_COUNT';
