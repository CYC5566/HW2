USE USUN2_FinancePreferenceDb;
GO

IF OBJECT_ID(N'dbo.sp_AppUser_GetById', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_AppUser_GetById;
IF OBJECT_ID(N'dbo.sp_AppUser_Insert', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_AppUser_Insert;
IF OBJECT_ID(N'dbo.sp_AppUser_Update', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_AppUser_Update;
IF OBJECT_ID(N'dbo.sp_AppUser_Delete', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_AppUser_Delete;
IF OBJECT_ID(N'dbo.sp_LikeList_GetBySn', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_LikeList_GetBySn;
IF OBJECT_ID(N'dbo.sp_LikeList_SelectAll', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_LikeList_SelectAll;
IF OBJECT_ID(N'dbo.sp_LikeList_SelectByUser', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_LikeList_SelectByUser;
IF OBJECT_ID(N'dbo.sp_LikeList_InsertWithProduct', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_LikeList_InsertWithProduct;
IF OBJECT_ID(N'dbo.sp_LikeList_UpdateWithProduct', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_LikeList_UpdateWithProduct;
IF OBJECT_ID(N'dbo.sp_LikeList_DeleteWithOrphanProduct', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_LikeList_DeleteWithOrphanProduct;
GO



CREATE PROCEDURE dbo.sp_AppUser_GetById
    @UserId NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UserID, UserName, Email, Account
    FROM dbo.Users
    WHERE UserID = @UserId;
END
GO

CREATE PROCEDURE dbo.sp_AppUser_Insert
    @UserId NVARCHAR(10),
    @UserName NVARCHAR(100),
    @Email NVARCHAR(256),
    @Account NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Users (UserID, UserName, Email, Account)
    VALUES (@UserId, @UserName, @Email, @Account);
END
GO

CREATE PROCEDURE dbo.sp_AppUser_Update
    @UserId NVARCHAR(10),
    @UserName NVARCHAR(100),
    @Email NVARCHAR(256),
    @Account NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Users
    SET UserName = @UserName, Email = @Email, Account = @Account
    WHERE UserID = @UserId;

    IF @@ROWCOUNT = 0
        THROW 50004, N'使用者不存在', 1;

    UPDATE dbo.LikeList
    SET Account = @Account
    WHERE UserID = @UserId;
END
GO

CREATE PROCEDURE dbo.sp_AppUser_Delete
    @UserId NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE FROM dbo.Users WHERE UserID = @UserId;

        DELETE p
        FROM dbo.Product p
        WHERE NOT EXISTS (SELECT 1 FROM dbo.LikeList l WHERE l.ProductNo = p.[No]);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO



CREATE PROCEDURE dbo.sp_LikeList_SelectAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        l.SN,
        u.UserID,
        u.UserName,
        u.Email,
        l.ProductName,
        l.Price,
        l.FeeRate,
        l.OrderQty,
        u.Account,
        l.TotalAmount,
        l.TotalFee
    FROM dbo.LikeList l
    INNER JOIN dbo.Users u ON u.UserID = l.UserID
    ORDER BY l.SN;
END
GO

CREATE PROCEDURE dbo.sp_LikeList_SelectByUser
    @UserId NVARCHAR(10),
    @ProductNameContains NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        l.SN,
        u.UserID,
        u.UserName,
        u.Email,
        l.ProductName,
        l.Price,
        l.FeeRate,
        l.OrderQty,
        u.Account,
        l.TotalAmount,
        l.TotalFee
    FROM dbo.LikeList l
    INNER JOIN dbo.Users u ON u.UserID = l.UserID
    WHERE l.UserID = @UserId
      AND (
          @ProductNameContains IS NULL
          OR LTRIM(RTRIM(@ProductNameContains)) = N''
          OR l.ProductName LIKE N'%' + @ProductNameContains + N'%'
      )
    ORDER BY l.SN;
END
GO

CREATE PROCEDURE dbo.sp_LikeList_GetBySn
    @Sn INT,
    @UserId NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        l.SN,
        u.UserID,
        u.UserName,
        u.Email,
        l.ProductName,
        l.Price,
        l.FeeRate,
        l.OrderQty,
        u.Account,
        l.TotalAmount,
        l.TotalFee
    FROM dbo.LikeList l
    INNER JOIN dbo.Users u ON u.UserID = l.UserID
    WHERE l.SN = @Sn AND l.UserID = @UserId;
END
GO

CREATE PROCEDURE dbo.sp_LikeList_InsertWithProduct
    @UserId NVARCHAR(10),
    @ProductName NVARCHAR(200),
    @Price DECIMAL(18, 4),
    @FeeRate DECIMAL(9, 6),
    @OrderQty INT,
    @DebitAccount NVARCHAR(50),
    @NewSn INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE UserID = @UserId)
            THROW 50002, N'使用者不存在', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE UserID = @UserId AND Account = @DebitAccount)
            THROW 50007, N'扣款帳號必須與使用者檔一致', 1;

        DECLARE @ProductNo INT;
        DECLARE @ProductAmount DECIMAL(18, 4) = @Price * @OrderQty;
        DECLARE @TotalFee DECIMAL(18, 4) = @ProductAmount * @FeeRate;
        DECLARE @TotalAmount DECIMAL(18, 4) = @ProductAmount + @TotalFee;

        INSERT INTO dbo.Product (ProductName, Price, FeeRate)
        VALUES (@ProductName, @Price, @FeeRate);

        SET @ProductNo = CAST(SCOPE_IDENTITY() AS INT);

        INSERT INTO dbo.LikeList (UserID, ProductNo, ProductName, Price, FeeRate, OrderQty, Account, TotalFee, TotalAmount)
        VALUES (@UserId, @ProductNo, @ProductName, @Price, @FeeRate, @OrderQty, @DebitAccount, @TotalFee, @TotalAmount);

        SET @NewSn = CAST(SCOPE_IDENTITY() AS INT);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE PROCEDURE dbo.sp_LikeList_UpdateWithProduct
    @Sn INT,
    @UserId NVARCHAR(10),
    @ProductName NVARCHAR(200),
    @Price DECIMAL(18, 4),
    @FeeRate DECIMAL(9, 6),
    @OrderQty INT,
    @DebitAccount NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @ProductNo INT;
        SELECT @ProductNo = ProductNo
        FROM dbo.LikeList WITH (UPDLOCK, HOLDLOCK)
        WHERE SN = @Sn AND UserID = @UserId;

        IF @ProductNo IS NULL
            THROW 50003, N'喜好清單項目不存在', 1;

        IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE UserID = @UserId AND Account = @DebitAccount)
            THROW 50007, N'扣款帳號必須與使用者檔一致', 1;

        DECLARE @ProductAmount DECIMAL(18, 4) = @Price * @OrderQty;
        DECLARE @TotalFee DECIMAL(18, 4) = @ProductAmount * @FeeRate;
        DECLARE @TotalAmount DECIMAL(18, 4) = @ProductAmount + @TotalFee;

        UPDATE dbo.Product
        SET ProductName = @ProductName, Price = @Price, FeeRate = @FeeRate
        WHERE [No] = @ProductNo;

        UPDATE dbo.LikeList
        SET ProductName = @ProductName,
            Price = @Price,
            FeeRate = @FeeRate,
            OrderQty = @OrderQty,
            Account = @DebitAccount,
            TotalAmount = @TotalAmount,
            TotalFee = @TotalFee
        WHERE SN = @Sn AND UserID = @UserId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

CREATE PROCEDURE dbo.sp_LikeList_DeleteWithOrphanProduct
    @Sn INT,
    @UserId NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @ProductNo INT;
        SELECT @ProductNo = ProductNo
        FROM dbo.LikeList WITH (UPDLOCK, HOLDLOCK)
        WHERE SN = @Sn AND UserID = @UserId;

        IF @ProductNo IS NULL
        BEGIN
            COMMIT TRANSACTION;
            RETURN;
        END

        DELETE FROM dbo.LikeList WHERE SN = @Sn AND UserID = @UserId;

        IF NOT EXISTS (SELECT 1 FROM dbo.LikeList WHERE ProductNo = @ProductNo)
            DELETE FROM dbo.Product WHERE [No] = @ProductNo;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
