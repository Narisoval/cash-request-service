CREATE TABLE CashRequestStatus (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL
);
GO

CREATE TABLE CashRequest (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Amount DECIMAL(18, 2) NOT NULL,
    ClientId NVARCHAR(100) NOT NULL,
    DepartmentAddress NVARCHAR(255) NOT NULL,
    Currency NVARCHAR(255) NOT NULL,
    StatusId INT NOT NULL FOREIGN KEY REFERENCES CashRequestStatus(Id)
);
GO

CREATE INDEX idx_cashrequest_client_department
ON CashRequest (ClientId, DepartmentAddress);
GO

CREATE PROCEDURE sp_get_cashrequest_by_id
    @Id INT
AS
BEGIN
    SELECT c.Id, c.Amount, c.ClientId, c.DepartmentAddress, c.Currency, 
           s.Id AS StatusId, s.Name AS StatusName
    FROM CashRequest c
    JOIN CashRequestStatus s ON c.StatusId = s.Id
    WHERE c.Id = @Id;
END;
GO

CREATE PROCEDURE sp_get_cashrequests_by_client_department
    @ClientId NVARCHAR(100),
    @DepartmentAddress NVARCHAR(255)
AS
BEGIN
    SELECT c.Id, c.Amount, c.ClientId, c.DepartmentAddress, c.Currency, 
           s.Id AS StatusId, s.Name AS StatusName
    FROM CashRequest c
    JOIN CashRequestStatus s ON c.StatusId = s.Id
    WHERE c.ClientId = @ClientId AND c.DepartmentAddress = @DepartmentAddress;
END;
GO

CREATE PROCEDURE sp_save_cashrequest
    @Amount DECIMAL(18, 2),
    @ClientId NVARCHAR(100),
    @DepartmentAddress NVARCHAR(255),
    @Currency NVARCHAR(255),
    @StatusId INT
AS
BEGIN
    INSERT INTO CashRequest (Amount, ClientId, DepartmentAddress, Currency, StatusId)
    OUTPUT INSERTED.Id
    VALUES (@Amount, @ClientId, @DepartmentAddress, @Currency, @StatusId);
END;
GO

INSERT INTO CashRequestStatus (Name)
VALUES ('Pending');
GO
