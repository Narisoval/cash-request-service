CREATE TABLE CashRequestStatus (
                                   Id SERIAL PRIMARY KEY,
                                   Name VARCHAR(100) NOT NULL
);

CREATE TABLE CashRequest (
                             Id SERIAL PRIMARY KEY,
                             Amount DECIMAL(18, 2) NOT NULL,
                             ClientId VARCHAR(100) NOT NULL,
                             DepartmentAddress VARCHAR(255) NOT NULL,
                             Currency VARCHAR(255) NOT NULL,
                             StatusId INT NOT NULL REFERENCES CashRequestStatus(Id)
);


CREATE INDEX idx_cashrequest_client_department
    ON CashRequest (ClientId, DepartmentAddress);


CREATE OR REPLACE FUNCTION sp_get_cashrequest_by_id(p_id INT)
RETURNS TABLE(
    Id INT,
    Amount DECIMAL,
    ClientId VARCHAR,
    DepartmentAddress VARCHAR,
    Currency VARCHAR,
    StatusId INT,
    StatusName VARCHAR
) AS $$
BEGIN
RETURN QUERY
SELECT c.Id, c.Amount, c.ClientId, c.DepartmentAddress, c.Currency,
       s.Id AS StatusId, s.Name AS StatusName
FROM CashRequest c
         JOIN CashRequestStatus s ON c.StatusId = s.Id
WHERE c.Id = p_id;
END $$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_get_cashrequests_by_client_department(p_client_id VARCHAR, p_department_address VARCHAR)
RETURNS TABLE(
    Id INT,
    Amount DECIMAL,
    ClientId VARCHAR,
    DepartmentAddress VARCHAR,
    Currency VARCHAR,
    StatusId INT,
    StatusName VARCHAR
) AS $$
BEGIN
RETURN QUERY
SELECT c.Id, c.Amount, c.ClientId, c.DepartmentAddress, c.Currency,
       s.Id AS StatusId, s.Name AS StatusName
FROM CashRequest c
         JOIN CashRequestStatus s ON c.StatusId = s.Id
WHERE c.ClientId = p_client_id AND c.DepartmentAddress = p_department_address;
END $$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE sp_save_cashrequest(
    p_amount DECIMAL,
    p_client_id VARCHAR,
    p_department_address VARCHAR,
    p_currency VARCHAR,
    p_status_id INT,
    OUT new_id INT  
)
LANGUAGE plpgsql AS $$
BEGIN
INSERT INTO CashRequest (Amount, ClientId, DepartmentAddress, Currency, StatusId)
VALUES (p_amount, p_client_id, p_department_address, p_currency, p_status_id)
    RETURNING Id INTO new_id;
END $$;



INSERT INTO CashRequestStatus (Name)
VALUES ('Pending');