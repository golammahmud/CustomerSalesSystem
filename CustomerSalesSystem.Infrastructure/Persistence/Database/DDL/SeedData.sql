DECLARE @i INT = 1;

WHILE @i <= 555
BEGIN
    INSERT INTO Products (Name, Price)
    VALUES (
        CONCAT('Product ', @i),
        ROUND(RAND(CHECKSUM(NEWID())) * 900 + 100, 2) -- Generates price between 100 and 1000
    );

    SET @i += 1;
END;

