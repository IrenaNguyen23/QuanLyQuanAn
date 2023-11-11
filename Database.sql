CREATE DATABASE QuanLyQuanCafe
GO

USE QuanLyQuanCafe
GO

CREATE TABLE TableFood
(
	id INT IDENTITY PRIMARY KEY,
	name NVARCHAR(100) NOT NULL  DEFAULT N'Chưa đặt tên',
	status NVARCHAR(100) NOT NULL,
)
ALTER TABLE TableFood
ALTER COLUMN status NVARCHAR(100) NOT NULL

ALTER TABLE TableFood
ADD CONSTRAINT DF_TableFood_status DEFAULT N'Trống' FOR status

GO

CREATE TABLE Account
(
	UserName NVARCHAR(100) PRIMARY KEY,
	DisplayName NVARCHAR(100) NOT NULL,
	PassWord NVARCHAR(1000) NOT NULL DEFAULT 0,
	Type INT NOT NULL DEFAULT 0
)
GO

CREATE TABLE FoodCategory
(
	id INT IDENTITY PRIMARY KEY,
	name NVARCHAR(100) NOT NULL,
)
GO

CREATE TABLE Food
(
	id INT IDENTITY PRIMARY KEY,
	name NVARCHAR(100) NOT NULL DEFAULT N'Chưa đặt tên',
	idCategory INT NOT NULL,
	price FLOAT NOT NULL

	FOREIGN KEY (idCategory) REFERENCES dbo.FoodCategory(id)
)
GO

CREATE TABLE Bill
(
	id INT IDENTITY PRIMARY KEY,
	DateCheckIn DATE NOT NULL DEFAULT GETDATE(),
	DateCheckOut DATE,
	idTable INT NOT NULL,
	discount INT,
	status INT NOT NULL DEFAULT 0 --chua thanh toan

	FOREIGN KEY (idTable) REFERENCES dbo.TableFood(id)
)

CREATE TABLE BillInfor
(
	id INT IDENTITY PRIMARY KEY,
	idBill INT NOT NULL,
	idFood int NOT NULL,
	count int NOT NULL DEFAULT 0

	FOREIGN KEY (idBill) REFERENCES dbo.Bill(id),
	FOREIGN KEY (idFood) REFERENCES dbo.Food(id)
)
GO

CREATE PROC USP_GetAccountByUserName
@userName nvarchar(100)
AS
BEGIN
	SELECT * FROM Account Where UserName=@userName
END
GO

EXEC USP_GetAccountByUserName @userName = N'staff'

GO

CREATE PROC USP_Login
@userName nvarchar(100), @passWord nvarchar(100)
AS
BEGIN
	SELECT * FROM Account Where UserName=@userName and PassWord=@passWord
END

GO

Exec USP_Login @userName = N'staff', @passWord = N'1'

GO


DECLARE @i INT = 1
WHILE @i <= 20
BEGIN
	INSERT TableFood (name) VALUES (N'Bàn' + CAST(@i AS nvarchar(100)))
	SET @i = @i + 1
END

SELECT * from TableFood
GO
CREATE PROC USP_GetTableList
AS SELECT * FROM TableFood
GO
EXEC USP_GetTableList

Update TableFood set status = N'Có người' where id = 5

GO
INSERT FoodCategory (name) VALUES (N'Hải sản')
INSERT FoodCategory (name) VALUES (N'Nông sản')
INSERT FoodCategory (name) VALUES (N'Lâm sản')
INSERT FoodCategory (name) VALUES (N'Nước')


GO
INSERT Food (name, idCategory, price) VALUES (N'Mực một nắng sa tế',1,12000)
INSERT Food (name, idCategory, price) VALUES (N'Nghêu hấp xả',1,20000)
INSERT Food (name, idCategory, price) VALUES (N'Dú dê nướng sửa',2,12000)
INSERT Food (name, idCategory, price) VALUES (N'Heo rừng nước muối ớt',3,20000)
INSERT Food (name, idCategory, price) VALUES (N'Cafe',4,15000)
INSERT Food (name, idCategory, price) VALUES (N'Sting',4,10000)

GO

INSERT Bill (DateCheckIn,DateCheckOut,idTable,status) VALUES (GETDATE(),NULL,1,0)
INSERT Bill (DateCheckIn,DateCheckOut,idTable,status) VALUES (GETDATE(),NULL,2,0)
INSERT Bill (DateCheckIn,DateCheckOut,idTable,status) VALUES (GETDATE(),GETDATE(),2,1)

GO
INSERT BillInfor (idBill,idFood,count) VALUES (1,1,2)
INSERT BillInfor (idBill,idFood,count) VALUES (1,5,1)
INSERT BillInfor (idBill,idFood,count) VALUES (2,1,2)
INSERT BillInfor (idBill,idFood,count) VALUES (2,6,10)
INSERT BillInfor (idBill,idFood,count) VALUES (3,5,2)

GO
SELECT * FROM FoodCategory
SELECT * FROM Food
SELECT * FROM Bill
SELECT * FROM BillInfor


GO

select * from bill where idTable=1 and status=0
select * from BillInfor where idBill = 1
select * from TableFood
select f.name, bi.count, f.price, f.price*bi.count as total from Bill as b, BillInfor as bi, Food as f 
where bi.idBill = b.id and bi.idFood = f.id and b.idTable = 1


Go
CREATE proc USP_InsertBill
@idTable Int
AS
BEGIN
	INSERT Bill (DateCheckIn,DateCheckOut,idTable,status,discount) VALUES (GETDATE(),NULL,@idTable,0,0)
END

GO
Create proc USP_InsertBillInfor
@idBill Int, @idFood int, @count int
AS
BEGIN

	Declare @isExitBillInfor int;
	Declare @foodCount int = 1;

	Select @isExitBillInfor = id, @foodCount = B.count 
	from BillInfor as B 
	where idBill = @idBill and idFood = @idFood;

	if(@isExitBillInfor > 0)
	begin
		DECLARE @newCount int = @foodCount + @count
		if(@newCount >0)
		Update BillInfor set count = @foodCount + @count where idFood = @idFood and idBill = @idBill
		else
		DELETE BillInfor Where idBill=@idBill and idFood=@idFood
	end
	else
	begin
		INSERT BillInfor (idBill,idFood,count) VALUES (@idBill,@idFood,@count)
	end	
END

SELECT MAX(id) from Bill

DROP PROC USP_InsertBillInfor


--Trigger
GO
CREATE TRIGGER UTG_UpdateBillInfo
ON dbo.BillInfor FOR INSERT, UPDATE
AS
BEGIN
	DECLARE @idBill INT
	
	SELECT @idBill = idBill FROM Inserted
	
	DECLARE @idTable INT
	
	SELECT @idTable = idTable FROM dbo.Bill WHERE id = @idBill AND status = 0	
	
	DECLARE @count INT
	SELECT @count = COUNT(*) FROM dbo.BillInfor WHERE idBill = @idBill
	
	IF (@count > 0)
	BEGIN
	
		PRINT @idTable
		PRINT @idBill
		PRINT @count
		
		UPDATE dbo.TableFood SET status = N'Có người' WHERE id = @idTable		
		
	END		
	ELSE
	BEGIN
	PRINT @idTable
		PRINT @idBill
		PRINT @count
	UPDATE dbo.TableFood SET status = N'Trống' WHERE id = @idTable	
	end
	
END
GO

GO
ALTER TRIGGER UTG_UpdateBill
ON dbo.Bill FOR UPDATE
AS
BEGIN
	DECLARE @idBill INT
	
	SELECT @idBill = id FROM Inserted	
	
	DECLARE @idTable INT
	
	SELECT @idTable = idTable FROM dbo.Bill WHERE id = @idBill
	
	DECLARE @count int = 0
	
	SELECT @count = COUNT(*) FROM dbo.Bill WHERE idTable = @idTable AND status = 0
	
	IF (@count = 0)
		UPDATE dbo.TableFood SET status = N'Trống' WHERE id = @idTable
END
GO

----------------------------------------------------------------------------- Contineu bai 13
ALTER PROC USP_SwitchTable
@idTable1 INT, @idTable2 int
AS BEGIN

	DECLARE @idFirstBill int
	DECLARE @idSeconrdBill INT
	
	DECLARE @isFirstTablEmty INT = 1
	DECLARE @isSecondTablEmty INT = 1
	
	
	SELECT @idSeconrdBill = id FROM dbo.Bill WHERE idTable = @idTable2 AND status = 0
	SELECT @idFirstBill = id FROM dbo.Bill WHERE idTable = @idTable1 AND status = 0
	
	PRINT @idFirstBill
	PRINT @idSeconrdBill
	PRINT '-----------'
	
	IF (@idFirstBill IS NULL)
	BEGIN
		PRINT '0000001'
		INSERT dbo.Bill
		        ( DateCheckIn ,
		          DateCheckOut ,
		          idTable ,
		          status
		        )
		VALUES  ( GETDATE() , -- DateCheckIn - date
		          NULL , -- DateCheckOut - date
		          @idTable1 , -- idTable - int
		          0  -- status - int
		        )
		        
		SELECT @idFirstBill = MAX(id) FROM dbo.Bill WHERE idTable = @idTable1 AND status = 0
		
	END
	
	SELECT @isFirstTablEmty = COUNT(*) FROM dbo.BillInfor WHERE idBill = @idFirstBill
	
	PRINT @idFirstBill
	PRINT @idSeconrdBill
	PRINT '-----------'
	
	IF (@idSeconrdBill IS NULL)
	BEGIN
		PRINT '0000002'
		INSERT dbo.Bill
		        ( DateCheckIn ,
		          DateCheckOut ,
		          idTable ,
		          status
		        )
		VALUES  ( GETDATE() , -- DateCheckIn - date
		          NULL , -- DateCheckOut - date
		          @idTable2 , -- idTable - int
		          0  -- status - int
		        )
		SELECT @idSeconrdBill = MAX(id) FROM dbo.Bill WHERE idTable = @idTable2 AND status = 0
		
	END
	
	SELECT @isSecondTablEmty = COUNT(*) FROM dbo.BillInfor WHERE idBill = @idSeconrdBill
	
	PRINT @idFirstBill
	PRINT @idSeconrdBill
	PRINT '-----------'

	SELECT id INTO IDBillInfoTable FROM dbo.BillInfor WHERE idBill = @idSeconrdBill
	
	UPDATE dbo.BillInfor SET idBill = @idSeconrdBill WHERE idBill = @idFirstBill
	
	UPDATE dbo.BillInfor SET idBill = @idFirstBill WHERE id IN (SELECT * FROM IDBillInfoTable)
	
	DROP TABLE IDBillInfoTable
	
	IF (@isFirstTablEmty = 0)
		UPDATE dbo.TableFood SET status = N'Trống' WHERE id = @idTable2
		
	IF (@isSecondTablEmty= 0)
		UPDATE dbo.TableFood SET status = N'Trống' WHERE id = @idTable1
END
GO

GO
SELECT * from TableFood
GO

DELETE FROM BillInfor
DELETE FROM Bill
UPDATE TableFood set status = N'Trống'
select * from TableFood

-------------------
ALter table bill add totalPrice float
GO
select t.name,b.totalPrice, DateCheckIn,DateCheckOut,discount 
from Bill as b,TableFood as t
where DateCheckIn >='20231101' and DateCheckIn <='20231130' and b.status = 1 and t.id =b.idTable 

GO
ALter PROC USP_GetListBillByDate
@checkIn Date, @checkOut date
AS
BEGIN
	select t.name as [Tên bàn], b.totalPrice as [Tổng tiền], DateCheckIn as [Ngày vào], DateCheckOut as [Ngày ra], discount as [Giảm giá] 
	from Bill as b,TableFood as t
	where DateCheckIn >= @checkIn and DateCheckIn <= @checkOut and b.status = 1 and t.id =b.idTable 

END
GO

select * from Account
Select * from Account
update Account set Type = '0' where UserName = 'staff'
Go
CREATE PROC USP_UpdateAccount
@userName NVARCHAR(100), @displayName NVARCHAR(100), @password NVARCHAR(100), @newPassword NVARCHAR(100)
AS
BEGIN
	DECLARE @isRightPass INT = 0
	
	SELECT @isRightPass = COUNT(*) FROM dbo.Account WHERE USERName = @userName AND PassWord = @password
	
	IF (@isRightPass = 1)
	BEGIN
		IF (@newPassword = NULL OR @newPassword = '')
		BEGIN
			UPDATE dbo.Account SET DisplayName = @displayName WHERE UserName = @userName
		END		
		ELSE
			UPDATE dbo.Account SET DisplayName = @displayName, PassWord = @newPassword WHERE UserName = @userName
	end
END
GO
select f.id as [ID Food], f.name as [Tên món ăn], f.price as [Giá tiền], fb.name as [Danh mục]  from food as f, FoodCategory as fb where fb.id = f.idCategory
select * from BillInfor
GO

CREATE TRIGGER UTG_DeleteBillInfo
ON BillInfor FOR DELETE
AS 
BEGIN
	DECLARE @idBillInfo INT
	DECLARE @idBill INT
	SELECT @idBillInfo = id, @idBill = Deleted.idBill FROM Deleted
	
	DECLARE @idTable INT
	SELECT @idTable = idTable FROM dbo.Bill WHERE id = @idBill
	
	DECLARE @count INT = 0
	
	SELECT @count = COUNT(*) FROM dbo.BillInfor AS bi, dbo.Bill AS b WHERE b.id = bi.idBill AND b.id = @idBill AND b.status = 0
	
	IF (@count = 0)
		UPDATE dbo.TableFood SET status = N'Trống' WHERE id = @idTable
END
GO
select * from food
	SELECT @count = COUNT(*) FROM dbo.BillInfo AS bi, dbo.Bill AS b WHERE b.id = bi.idBill AND b.id = @idBill AND b.status = 0
	
	IF (@count = 0)
		UPDATE dbo.TableFood SET status = N'Trống' WHERE id = @idTable
END
GO


CREATE FUNCTION [dbo].[fuConvertToUnsign1] ( @strInput NVARCHAR(4000) ) RETURNS NVARCHAR(4000) AS BEGIN IF @strInput IS NULL RETURN @strInput IF @strInput = '' RETURN @strInput DECLARE @RT NVARCHAR(4000) DECLARE @SIGN_CHARS NCHAR(136) DECLARE @UNSIGN_CHARS NCHAR (136) SET @SIGN_CHARS = N'ăâđêôơưàảãạáằẳẵặắầẩẫậấèẻẽẹéềểễệế ìỉĩịíòỏõọóồổỗộốờởỡợớùủũụúừửữựứỳỷỹỵý ĂÂĐÊÔƠƯÀẢÃẠÁẰẲẴẶẮẦẨẪẬẤÈẺẼẸÉỀỂỄỆẾÌỈĨỊÍ ÒỎÕỌÓỒỔỖỘỐỜỞỠỢỚÙỦŨỤÚỪỬỮỰỨỲỶỸỴÝ' +NCHAR(272)+ NCHAR(208) SET @UNSIGN_CHARS = N'aadeoouaaaaaaaaaaaaaaaeeeeeeeeee iiiiiooooooooooooooouuuuuuuuuuyyyyy AADEOOUAAAAAAAAAAAAAAAEEEEEEEEEEIIIII OOOOOOOOOOOOOOOUUUUUUUUUUYYYYYDD' DECLARE @COUNTER int DECLARE @COUNTER1 int SET @COUNTER = 1 WHILE (@COUNTER <=LEN(@strInput)) BEGIN SET @COUNTER1 = 1 WHILE (@COUNTER1 <=LEN(@SIGN_CHARS)+1) BEGIN IF UNICODE(SUBSTRING(@SIGN_CHARS, @COUNTER1,1)) = UNICODE(SUBSTRING(@strInput,@COUNTER ,1) ) BEGIN IF @COUNTER=1 SET @strInput = SUBSTRING(@UNSIGN_CHARS, @COUNTER1,1) + SUBSTRING(@strInput, @COUNTER+1,LEN(@strInput)-1) ELSE SET @strInput = SUBSTRING(@strInput, 1, @COUNTER-1) +SUBSTRING(@UNSIGN_CHARS, @COUNTER1,1) + SUBSTRING(@strInput, @COUNTER+1,LEN(@strInput)- @COUNTER) BREAK END SET @COUNTER1 = @COUNTER1 +1 END SET @COUNTER = @COUNTER +1 END SET @strInput = replace(@strInput,' ','-') RETURN @strInput END

GO

CREATE PROC USP_GetListBillByDateAndPage
@checkIn date, @checkOut date, @page int
AS 
BEGIN
	DECLARE @pageRows INT = 10
	DECLARE @selectRows INT = @pageRows
	DECLARE @exceptRows INT = (@page - 1) * @pageRows
	
	;WITH BillShow AS( SELECT b.ID, t.name AS [Tên bàn], b.totalPrice AS [Tổng tiền], DateCheckIn AS [Ngày vào], DateCheckOut AS [Ngày ra], discount AS [Giảm giá]
	FROM dbo.Bill AS b,dbo.TableFood AS t
	WHERE DateCheckIn >= @checkIn AND DateCheckOut <= @checkOut AND b.status = 1
	AND t.id = b.idTable)
	
	SELECT TOP (@selectRows) * FROM BillShow WHERE id NOT IN (SELECT TOP (@exceptRows) id FROM BillShow)
END
GO

CREATE PROC USP_GetNumBillByDate
@checkIn date, @checkOut date
AS 
BEGIN
	SELECT COUNT(*)
	FROM dbo.Bill AS b,dbo.TableFood AS t
	WHERE DateCheckIn >= @checkIn AND DateCheckOut <= @checkOut AND b.status = 1
	AND t.id = b.idTable
END
GO

CREATE TRIGGER PreventFoodCategoryDeletion
ON FoodCategory
INSTEAD OF DELETE
AS
BEGIN
    -- Kiểm tra xem danh mục đang bị xóa có món ăn thuộc danh mục đó hay không
    IF EXISTS (
        SELECT id 
        FROM Food as f
        WHERE f.idCategory IN (SELECT id FROM deleted)
    )
    BEGIN
        -- Nếu có món ăn thuộc danh mục đang bị xóa, hủy xóa
        RAISERROR('Không thể xóa danh mục có chứa món ăn.', 16, 1)
        ROLLBACK TRANSACTION
    END
    ELSE
    BEGIN
        -- Nếu không có món ăn thuộc danh mục đang bị xóa, thực hiện xóa
        DELETE FROM FoodCategory
        WHERE id IN (SELECT id FROM deleted)
    END
END

select * from TableFood

GO

