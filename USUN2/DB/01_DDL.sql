SET NOCOUNT ON;
GO

/* 資料庫本體由 FinanceDatabaseBootstrap 依連線字串建立；此檔僅負責資料表結構。 */

IF OBJECT_ID(N'dbo.LikeList', N'U') IS NOT NULL
    DROP TABLE dbo.LikeList;
IF OBJECT_ID(N'dbo.Product', N'U') IS NOT NULL
    DROP TABLE dbo.Product;
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
    DROP TABLE dbo.Users;
GO

CREATE TABLE dbo.Users
(
    UserID    NVARCHAR(10)   NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    UserName  NVARCHAR(100)  NOT NULL,
    Email     NVARCHAR(256)  NOT NULL,
    Account   NVARCHAR(50)   NOT NULL,
    CONSTRAINT CK_Users_UserId_Len CHECK (LEN(UserID) = 10)
);
GO

CREATE TABLE dbo.Product
(
    [No]          INT            IDENTITY(1, 1) NOT NULL CONSTRAINT PK_Product PRIMARY KEY,
    ProductName   NVARCHAR(200)  NOT NULL,
    Price         DECIMAL(18, 4) NOT NULL CONSTRAINT CK_Product_Price_Positive CHECK (Price > 0),
    FeeRate       DECIMAL(9, 6)  NOT NULL CONSTRAINT CK_Product_FeeRate CHECK (FeeRate >= 0 AND FeeRate <= 1)
);
GO

CREATE TABLE dbo.LikeList
(
    SN           INT            IDENTITY(1, 1) NOT NULL CONSTRAINT PK_LikeList PRIMARY KEY,
    UserID       NVARCHAR(10)   NOT NULL,
    ProductNo    INT            NOT NULL,
    ProductName  NVARCHAR(200)  NOT NULL,
    Price        DECIMAL(18, 4) NOT NULL,
    FeeRate      DECIMAL(9, 6)  NOT NULL,
    OrderQty     INT            NOT NULL CONSTRAINT CK_LikeList_Qty CHECK (OrderQty > 0),
    Account      NVARCHAR(50)   NOT NULL,
    TotalFee     DECIMAL(18, 4) NOT NULL,
    TotalAmount  DECIMAL(18, 4) NOT NULL,
    CONSTRAINT FK_LikeList_Users FOREIGN KEY (UserID) REFERENCES dbo.Users (UserID) ON DELETE CASCADE,
    CONSTRAINT FK_LikeList_Product FOREIGN KEY (ProductNo) REFERENCES dbo.Product ([No])
);
GO

CREATE INDEX IX_LikeList_UserID ON dbo.LikeList (UserID);
CREATE INDEX IX_LikeList_ProductNo ON dbo.LikeList (ProductNo);
GO
