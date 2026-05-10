# USUN2
這是一個用ASP.NET Core做的**金融商品喜好紀錄**練習專案：可以註冊登入、維護個人資料，以及新增/查詢/編輯/刪除喜好金融商品。
畫面是**MVC + Bootstrap**，資料存在**SQL Server**，後端用**預存程序**存取。
---
## 你需要先準備什麼
- 安裝**.NET 10**
- 本機或遠端有一台**SQL Server**
---
## 資料庫怎麼來？（自動建庫）
`Program.cs` 會讀 **`Database:AutoInitialize`**。為 **`true`** 時會呼叫 **`FinanceDatabaseBootstrap.EnsureReadyAsync`**：
1. 依 **`ConnectionStrings:DefaultConnection`** 的資料庫名（`Initial Catalog`），必要時在 **`master`** 執行 **`CREATE DATABASE`**。
2. 若結構**尚未**判定為目前版本，會依序跑 **`USUN2/DB/01_DDL.sql`**、**`02_StoredProcedures.sql`**。
3. 若庫已存在且結構已對，會**略過**腳本，避免每次啟動都重刷。
   
**開關：** 開發環境可在 **`USUN2/appsettings.Development.json`** 設 **`AutoInitialize: true`**；要關掉則在 **`USUN2/appsettings.json`** 設 **`false`**。

連線：**`USUN2/appsettings.json`** → **`ConnectionStrings:DefaultConnection`**，預設庫名 **`USUN2_FinancePreferenceDb`**，請改成你的環境。
---
## 資料庫怎麼來？（手動建庫）
關閉 **`AutoInitialize`** 後，自行建空庫，再用 SSMS/sqlcmd 依序執行 `USUN2/DB` 的 **`01_DDL.sql`**、**`02_StoredProcedures.sql`**。
---
## 這個專案大概能做什麼
1.	新增喜好金融商品：使用者可以透過介面進行新增所喜好的金融商品資訊(產品名稱、產品價格、手續費率)、預計要扣款的帳號、購買數量。
2.	查詢喜好金融商品清單：使用者可以透過介面進行查詢所喜好的金融商品名稱清單以及預計要扣款的帳號、預計扣款總金額、總手續費用、扣款帳號、使用者聯絡電子信箱。
3.	刪除喜好金融商品資訊：使用者可以透過介面進行刪除所喜好的金融商品資訊(產品名稱、產品價格、手續費率)。
4.	更改喜好金融商品資訊：使用者可以透過介面進行更改所喜好的金融商品資訊(產品名稱、產品價格、手續費率)、預計要扣款的帳號、購買數量。
