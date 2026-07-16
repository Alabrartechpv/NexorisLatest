IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_CounterSessions_OpenUser' AND object_id = OBJECT_ID('dbo.CounterSessions'))
BEGIN
    DROP INDEX UX_CounterSessions_OpenUser ON dbo.CounterSessions;
END
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_CounterSessions_OpenCounter' AND object_id = OBJECT_ID('dbo.CounterSessions'))
BEGIN
    DROP INDEX UX_CounterSessions_OpenCounter ON dbo.CounterSessions;
END
GO


ALTER PROCEDURE [dbo].[POS_CounterSession]
(
    @CompanyId INT,
    @BranchId INT,
    @FinYearId INT,
    @CounterId INT,
    @CounterName VARCHAR(50) = NULL,
    @UserId INT,
    @SystemName VARCHAR(100) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SessionId BIGINT;
    DECLARE @IsAdmin BIT = 0;

    -- Check if the user has Administrator role
    IF EXISTS (
        SELECT 1 
        FROM dbo.Users U
        INNER JOIN dbo.Userlevels UL ON UL.UserLevelID = U.UserLevelID
        WHERE U.UserID = @UserId 
          AND UL.UserLevel = 'Administrator'
    )
    BEGIN
        SET @IsAdmin = 1;
    END

    -- Same counter already open by another user: block (bypassed for Administrators)
    IF @IsAdmin = 0 AND EXISTS (
        SELECT 1
        FROM dbo.CounterSessions WITH (UPDLOCK, HOLDLOCK)
        WHERE BranchId = @BranchId
          AND CounterId = @CounterId
          AND Status = 'Open'
          AND UserId <> @UserId
    )
    BEGIN
        RAISERROR('This counter is already open by another user. Please close that counter session first.', 16, 1);
        RETURN;
    END

    -- Same user already open in another counter: block (bypassed for Administrators)
    IF @IsAdmin = 0 AND EXISTS (
        SELECT 1
        FROM dbo.CounterSessions WITH (UPDLOCK, HOLDLOCK)
        WHERE BranchId = @BranchId
          AND UserId = @UserId
          AND Status = 'Open'
          AND CounterId <> @CounterId
    )
    BEGIN
        RAISERROR('This user already has an open session in another counter. Please close that session first.', 16, 1);
        RETURN;
    END

    -- Same user + same counter already open: resume
    SELECT TOP 1 @SessionId = SessionId
    FROM dbo.CounterSessions WITH (UPDLOCK, HOLDLOCK)
    WHERE BranchId = @BranchId
      AND CounterId = @CounterId
      AND UserId = @UserId
      AND Status = 'Open'
    ORDER BY LoginTime DESC;

    IF @SessionId IS NOT NULL
    BEGIN
        UPDATE dbo.CounterSessions
        SET CompanyId = @CompanyId,
            FinYearId = @FinYearId,
            CounterName = ISNULL(@CounterName, CounterName),
            SystemName = ISNULL(@SystemName, SystemName),
            ModifiedDate = GETDATE()
        WHERE SessionId = @SessionId;

        SELECT *
        FROM dbo.CounterSessions
        WHERE SessionId = @SessionId;

        RETURN;
    END

    -- No open session: create new
    INSERT INTO dbo.CounterSessions
    (
        CompanyId, BranchId, FinYearId, CounterId, CounterName,
        UserId, LoginTime, Status, SystemName, CreatedDate
    )
    VALUES
    (
        @CompanyId, @BranchId, @FinYearId, @CounterId, @CounterName,
        @UserId, GETDATE(), 'Open', @SystemName, GETDATE()
    );

    SET @SessionId = SCOPE_IDENTITY();

    SELECT *
    FROM dbo.CounterSessions
    WHERE SessionId = @SessionId;
END
