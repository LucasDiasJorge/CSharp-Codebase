# SQL Annotations - Comandos Essenciais

## 📚 Comandos Fundamentais

Este documento contém anotações dos comandos SQL mais importantes que você precisa dominar.

## 🎯 DDL - Data Definition Language

### CREATE TABLE
```sql
-- Criação básica de tabela
CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    CategoryId INT,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1
);

-- Tabela com constraints
CREATE TABLE Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(255),
    ParentId INT NULL,
    CONSTRAINT FK_Categories_Parent FOREIGN KEY (ParentId) REFERENCES Categories(Id)
);

-- Tabela com índices
CREATE TABLE Orders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    OrderDate DATETIME2 DEFAULT GETDATE(),
    Total DECIMAL(12,2) NOT NULL,
    Status NVARCHAR(20) DEFAULT 'Pending',
    INDEX IX_Orders_CustomerId (CustomerId),
    INDEX IX_Orders_OrderDate (OrderDate)
);
```

### ALTER TABLE
```sql
-- Adicionar coluna
ALTER TABLE Products ADD Description NVARCHAR(500);

-- Modificar coluna
ALTER TABLE Products ALTER COLUMN Name NVARCHAR(150) NOT NULL;

-- Adicionar constraint
ALTER TABLE Products 
ADD CONSTRAINT FK_Products_Category 
FOREIGN KEY (CategoryId) REFERENCES Categories(Id);

-- Remover coluna
ALTER TABLE Products DROP COLUMN Description;

-- Adicionar índice
CREATE INDEX IX_Products_Name ON Products(Name);
```

### DROP e TRUNCATE
```sql
-- Remover tabela completamente
DROP TABLE Products;

-- Limpar dados mantendo estrutura
TRUNCATE TABLE Products;

-- Remover índice
DROP INDEX IX_Products_Name ON Products;
```

## 🔍 DQL - Data Query Language

### SELECT Básico
```sql
-- Consulta simples
SELECT Id, Name, Price FROM Products;

-- Todas as colunas
SELECT * FROM Products;

-- Com alias
SELECT 
    p.Id as ProductId,
    p.Name as ProductName,
    p.Price * 1.1 as PriceWithTax
FROM Products p;

-- Valores únicos
SELECT DISTINCT CategoryId FROM Products;
```

### WHERE - Filtros
```sql
-- Operadores básicos
SELECT * FROM Products WHERE Price > 100;
SELECT * FROM Products WHERE Price BETWEEN 50 AND 200;
SELECT * FROM Products WHERE Name LIKE '%Phone%';
SELECT * FROM Products WHERE CategoryId IN (1, 2, 3);
SELECT * FROM Products WHERE Price IS NOT NULL;

-- Operadores lógicos
SELECT * FROM Products 
WHERE Price > 100 AND CategoryId = 1;

SELECT * FROM Products 
WHERE Price > 500 OR CategoryId = 2;

SELECT * FROM Products 
WHERE NOT (Price < 50);

-- Pattern matching
SELECT * FROM Products WHERE Name LIKE 'iPhone%';  -- Começa com
SELECT * FROM Products WHERE Name LIKE '%Pro';     -- Termina com
SELECT * FROM Products WHERE Name LIKE '%Air%';    -- Contém
SELECT * FROM Products WHERE Name LIKE '_Phone';   -- Um caractere + Phone
```

### ORDER BY
```sql
-- Ordenação simples
SELECT * FROM Products ORDER BY Price;
SELECT * FROM Products ORDER BY Price DESC;

-- Múltiplas colunas
SELECT * FROM Products 
ORDER BY CategoryId ASC, Price DESC;

-- Com expressões
SELECT * FROM Products 
ORDER BY Price * Quantity DESC;
```

### GROUP BY e HAVING
```sql
-- Agrupamento básico
SELECT CategoryId, COUNT(*) as ProductCount
FROM Products 
GROUP BY CategoryId;

-- Com funções agregadas
SELECT 
    CategoryId,
    COUNT(*) as Total,
    AVG(Price) as AveragePrice,
    MIN(Price) as MinPrice,
    MAX(Price) as MaxPrice,
    SUM(Price) as TotalValue
FROM Products 
GROUP BY CategoryId;

-- HAVING para filtrar grupos
SELECT CategoryId, COUNT(*) as ProductCount
FROM Products 
GROUP BY CategoryId
HAVING COUNT(*) > 5;

-- GROUP BY com múltiplas colunas
SELECT CategoryId, YEAR(CreatedAt), COUNT(*)
FROM Products 
GROUP BY CategoryId, YEAR(CreatedAt);
```

### JOINS
```sql
-- INNER JOIN - Apenas registros que existem em ambas as tabelas
SELECT p.Name, c.Name as CategoryName
FROM Products p
INNER JOIN Categories c ON p.CategoryId = c.Id;

-- LEFT JOIN - Todos da esquerda + matches da direita
SELECT p.Name, c.Name as CategoryName
FROM Products p
LEFT JOIN Categories c ON p.CategoryId = c.Id;

-- RIGHT JOIN - Todos da direita + matches da esquerda
SELECT p.Name, c.Name as CategoryName
FROM Products p
RIGHT JOIN Categories c ON p.CategoryId = c.Id;

-- FULL OUTER JOIN - Todos de ambas as tabelas
SELECT p.Name, c.Name as CategoryName
FROM Products p
FULL OUTER JOIN Categories c ON p.CategoryId = c.Id;

-- CROSS JOIN - Produto cartesiano
SELECT p.Name, c.Name
FROM Products p
CROSS JOIN Categories c;

-- JOIN com múltiplas tabelas
SELECT 
    p.Name as ProductName,
    c.Name as CategoryName,
    o.OrderDate,
    od.Quantity
FROM Products p
INNER JOIN Categories c ON p.CategoryId = c.Id
INNER JOIN OrderDetails od ON p.Id = od.ProductId
INNER JOIN Orders o ON od.OrderId = o.Id;
```

### Subqueries
```sql
-- Subquery no WHERE
SELECT * FROM Products 
WHERE CategoryId IN (
    SELECT Id FROM Categories WHERE Name LIKE '%Electronics%'
);

-- Subquery no SELECT
SELECT 
    Name,
    Price,
    (SELECT AVG(Price) FROM Products) as AvgPrice
FROM Products;

-- EXISTS
SELECT * FROM Categories c
WHERE EXISTS (
    SELECT 1 FROM Products p WHERE p.CategoryId = c.Id
);

-- NOT EXISTS
SELECT * FROM Categories c
WHERE NOT EXISTS (
    SELECT 1 FROM Products p WHERE p.CategoryId = c.Id
);

-- Subquery correlacionada
SELECT p1.Name, p1.Price
FROM Products p1
WHERE p1.Price > (
    SELECT AVG(p2.Price) 
    FROM Products p2 
    WHERE p2.CategoryId = p1.CategoryId
);
```

### Common Table Expressions (CTE)
```sql
-- CTE Simples
WITH ExpensiveProducts AS (
    SELECT * FROM Products WHERE Price > 1000
)
SELECT * FROM ExpensiveProducts ORDER BY Price;

-- CTE Recursiva (para hierarquias)
WITH CategoryHierarchy AS (
    -- Anchor: categorias raiz
    SELECT Id, Name, ParentId, 0 as Level
    FROM Categories 
    WHERE ParentId IS NULL
    
    UNION ALL
    
    -- Recursive: categorias filhas
    SELECT c.Id, c.Name, c.ParentId, ch.Level + 1
    FROM Categories c
    INNER JOIN CategoryHierarchy ch ON c.ParentId = ch.Id
)
SELECT * FROM CategoryHierarchy ORDER BY Level, Name;

-- Múltiplas CTEs
WITH 
CategoryStats AS (
    SELECT CategoryId, COUNT(*) as ProductCount, AVG(Price) as AvgPrice
    FROM Products 
    GROUP BY CategoryId
),
TopCategories AS (
    SELECT TOP 5 * FROM CategoryStats ORDER BY ProductCount DESC
)
SELECT c.Name, cs.ProductCount, cs.AvgPrice
FROM TopCategories cs
INNER JOIN Categories c ON cs.CategoryId = c.Id;
```

## 🔄 DML - Data Manipulation Language

### INSERT
```sql
-- Insert simples
INSERT INTO Products (Name, Price, CategoryId)
VALUES ('iPhone 15', 999.99, 1);

-- Insert múltiplos registros
INSERT INTO Products (Name, Price, CategoryId)
VALUES 
    ('Galaxy S24', 899.99, 1),
    ('Pixel 8', 799.99, 1),
    ('OnePlus 12', 749.99, 1);

-- Insert com SELECT
INSERT INTO ProductArchive (Name, Price, CategoryId, ArchivedDate)
SELECT Name, Price, CategoryId, GETDATE()
FROM Products 
WHERE CreatedAt < DATEADD(YEAR, -1, GETDATE());

-- Insert e retornar ID
INSERT INTO Products (Name, Price, CategoryId)
OUTPUT INSERTED.Id
VALUES ('New Product', 199.99, 2);

-- Insert com MERGE (Upsert)
MERGE Products AS target
USING (VALUES ('iPhone 15', 999.99, 1)) AS source (Name, Price, CategoryId)
ON target.Name = source.Name
WHEN MATCHED THEN
    UPDATE SET Price = source.Price, CategoryId = source.CategoryId
WHEN NOT MATCHED THEN
    INSERT (Name, Price, CategoryId) VALUES (source.Name, source.Price, source.CategoryId);
```

### UPDATE
```sql
-- Update simples
UPDATE Products 
SET Price = 899.99 
WHERE Id = 1;

-- Update múltiplas colunas
UPDATE Products 
SET 
    Price = Price * 1.1,
    ModifiedAt = GETDATE()
WHERE CategoryId = 1;

-- Update com JOIN
UPDATE p
SET p.CategoryName = c.Name
FROM Products p
INNER JOIN Categories c ON p.CategoryId = c.Id;

-- Update condicional
UPDATE Products 
SET Price = CASE 
    WHEN Price < 100 THEN Price * 1.2
    WHEN Price < 500 THEN Price * 1.1
    ELSE Price * 1.05
END;

-- Update com OUTPUT
UPDATE Products 
SET Price = Price * 1.1
OUTPUT DELETED.Id, DELETED.Price as OldPrice, INSERTED.Price as NewPrice
WHERE CategoryId = 1;
```

### DELETE
```sql
-- Delete simples
DELETE FROM Products WHERE Id = 1;

-- Delete com condição
DELETE FROM Products 
WHERE Price < 10 AND CreatedAt < DATEADD(MONTH, -6, GETDATE());

-- Delete com JOIN
DELETE p
FROM Products p
INNER JOIN Categories c ON p.CategoryId = c.Id
WHERE c.Name = 'Discontinued';

-- Delete com OUTPUT
DELETE FROM Products 
OUTPUT DELETED.*
WHERE CategoryId = 999;

-- Delete TOP N
DELETE TOP (100) FROM Products 
WHERE IsActive = 0;
```

## 📊 Funções Essenciais

### Funções de String
```sql
-- Concatenação
SELECT CONCAT(FirstName, ' ', LastName) as FullName;
SELECT FirstName + ' ' + LastName as FullName; -- SQL Server

-- Substring
SELECT SUBSTRING(Name, 1, 10) as ShortName;
SELECT LEFT(Name, 5), RIGHT(Name, 3);

-- Case conversion
SELECT UPPER(Name), LOWER(Name), PROPER(Name);

-- Trim
SELECT LTRIM(RTRIM(Name)) as CleanName;
SELECT TRIM(Name) as CleanName; -- SQL Server 2017+

-- Replace
SELECT REPLACE(Description, 'old', 'new');

-- Busca
SELECT * FROM Products WHERE CHARINDEX('Pro', Name) > 0;
SELECT LEN(Name) as NameLength;
```

### Funções de Data
```sql
-- Data atual
SELECT GETDATE(), GETUTCDATE();
SELECT SYSDATETIME(); -- Mais preciso

-- Partes da data
SELECT YEAR(OrderDate), MONTH(OrderDate), DAY(OrderDate);
SELECT DATEPART(QUARTER, OrderDate);
SELECT DATENAME(WEEKDAY, OrderDate);

-- Aritmética de datas
SELECT DATEADD(DAY, 30, OrderDate) as DueDate;
SELECT DATEADD(MONTH, -1, GETDATE()) as LastMonth;
SELECT DATEDIFF(DAY, OrderDate, GETDATE()) as DaysAgo;

-- Formatação
SELECT FORMAT(OrderDate, 'yyyy-MM-dd') as ISODate;
SELECT FORMAT(OrderDate, 'dd/MM/yyyy') as BrazilianDate;
SELECT CONVERT(VARCHAR(10), OrderDate, 103) as DateString;
```

### Funções Matemáticas
```sql
-- Básicas
SELECT ABS(-10), ROUND(3.14159, 2), CEILING(3.1), FLOOR(3.9);
SELECT POWER(2, 3), SQRT(16), EXP(1), LOG(10);

-- Trigonométricas
SELECT SIN(PI()/2), COS(0), TAN(PI()/4);

-- Aleatório
SELECT RAND(), RAND(123); -- Com seed
```

### Funções de Conversão
```sql
-- Conversão de tipos
SELECT CAST(Price as INT);
SELECT CONVERT(VARCHAR(10), Price);
SELECT TRY_CAST('abc' as INT); -- Retorna NULL se falhar
SELECT TRY_CONVERT(INT, '123');

-- Verificação de tipo
SELECT ISNUMERIC('123'), ISDATE('2023-01-01');
```

### Funções de Janela (Window Functions)
```sql
-- ROW_NUMBER
SELECT 
    Name, 
    Price,
    ROW_NUMBER() OVER (ORDER BY Price DESC) as RowNum
FROM Products;

-- RANK e DENSE_RANK
SELECT 
    Name, 
    Price,
    RANK() OVER (ORDER BY Price DESC) as Rank,
    DENSE_RANK() OVER (ORDER BY Price DESC) as DenseRank
FROM Products;

-- PARTITION BY
SELECT 
    Name, 
    Price,
    CategoryId,
    ROW_NUMBER() OVER (PARTITION BY CategoryId ORDER BY Price DESC) as RankInCategory
FROM Products;

-- LAG e LEAD
SELECT 
    Name,
    Price,
    LAG(Price) OVER (ORDER BY Id) as PreviousPrice,
    LEAD(Price) OVER (ORDER BY Id) as NextPrice
FROM Products;

-- Funções agregadas como Window Functions
SELECT 
    Name,
    Price,
    SUM(Price) OVER (ORDER BY Id ROWS UNBOUNDED PRECEDING) as RunningTotal,
    AVG(Price) OVER (ORDER BY Id ROWS 2 PRECEDING) as MovingAverage
FROM Products;
```

## 🔒 Controle de Transações

### Transações Básicas
```sql
-- Transação simples
BEGIN TRANSACTION;
    UPDATE Products SET Price = Price * 1.1 WHERE CategoryId = 1;
    INSERT INTO PriceHistory (ProductId, OldPrice, NewPrice, ChangeDate)
    SELECT Id, Price / 1.1, Price, GETDATE() FROM Products WHERE CategoryId = 1;
COMMIT TRANSACTION;

-- Com tratamento de erro
BEGIN TRY
    BEGIN TRANSACTION;
        -- Operações aqui
        DELETE FROM Products WHERE Id = @ProductId;
        UPDATE Categories SET ProductCount = ProductCount - 1 WHERE Id = @CategoryId;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    THROW; -- Re-lança o erro
END CATCH;
```

### Savepoints
```sql
BEGIN TRANSACTION;
    UPDATE Products SET Price = Price * 1.1;
    SAVE TRANSACTION SavePoint1;
    
    DELETE FROM Products WHERE Price > 2000;
    -- Se algo der errado, volta ao savepoint
    ROLLBACK TRANSACTION SavePoint1;
    
COMMIT TRANSACTION;
```

## 🎯 Stored Procedures e Functions

### Stored Procedures
```sql
-- Procedure simples
CREATE PROCEDURE GetProductsByCategory
    @CategoryId INT
AS
BEGIN
    SELECT * FROM Products WHERE CategoryId = @CategoryId;
END;

-- Procedure com parâmetros de saída
CREATE PROCEDURE GetProductStats
    @CategoryId INT,
    @TotalProducts INT OUTPUT,
    @AveragePrice DECIMAL(10,2) OUTPUT
AS
BEGIN
    SELECT 
        @TotalProducts = COUNT(*),
        @AveragePrice = AVG(Price)
    FROM Products 
    WHERE CategoryId = @CategoryId;
END;

-- Procedure com tratamento de erro
CREATE PROCEDURE UpdateProductPrice
    @ProductId INT,
    @NewPrice DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
            
            IF NOT EXISTS (SELECT 1 FROM Products WHERE Id = @ProductId)
            BEGIN
                RAISERROR('Product not found', 16, 1);
                RETURN;
            END;
            
            UPDATE Products 
            SET Price = @NewPrice, ModifiedAt = GETDATE()
            WHERE Id = @ProductId;
            
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
```

### Functions
```sql
-- Scalar Function
CREATE FUNCTION GetProductTotal(@ProductId INT, @Quantity INT)
RETURNS DECIMAL(10,2)
AS
BEGIN
    DECLARE @Price DECIMAL(10,2);
    SELECT @Price = Price FROM Products WHERE Id = @ProductId;
    RETURN @Price * @Quantity;
END;

-- Table-Valued Function
CREATE FUNCTION GetProductsByPriceRange(@MinPrice DECIMAL(10,2), @MaxPrice DECIMAL(10,2))
RETURNS TABLE
AS
RETURN
(
    SELECT * FROM Products 
    WHERE Price BETWEEN @MinPrice AND @MaxPrice
);
```

## 📈 Índices e Performance

### Criação de Índices
```sql
-- Índice simples
CREATE INDEX IX_Products_Price ON Products(Price);

-- Índice composto
CREATE INDEX IX_Products_Category_Price ON Products(CategoryId, Price);

-- Índice único
CREATE UNIQUE INDEX IX_Products_SKU ON Products(SKU);

-- Índice com colunas incluídas
CREATE INDEX IX_Products_Category_Include 
ON Products(CategoryId) 
INCLUDE (Name, Price);

-- Índice filtrado
CREATE INDEX IX_Products_Active 
ON Products(CategoryId) 
WHERE IsActive = 1;
```

### Análise de Performance
```sql
-- Plano de execução
SET STATISTICS IO ON;
SET STATISTICS TIME ON;

-- Verificar índices utilizados
SELECT 
    i.name as IndexName,
    s.user_seeks,
    s.user_scans,
    s.user_lookups,
    s.user_updates
FROM sys.indexes i
INNER JOIN sys.dm_db_index_usage_stats s 
    ON i.object_id = s.object_id AND i.index_id = s.index_id
WHERE OBJECT_NAME(i.object_id) = 'Products';
```

## 🔧 Comandos Utilitários

### Informações do Sistema
```sql
-- Informações das tabelas
SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Products';

-- Tamanho das tabelas
SELECT 
    t.NAME AS TableName,
    p.rows AS RowCounts,
    SUM(a.total_pages) * 8 AS TotalSpaceKB
FROM sys.tables t
INNER JOIN sys.indexes i ON t.OBJECT_ID = i.object_id
INNER JOIN sys.partitions p ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
GROUP BY t.Name, p.Rows
ORDER BY TotalSpaceKB DESC;

-- Constraints de uma tabela
SELECT 
    CONSTRAINT_NAME,
    CONSTRAINT_TYPE,
    COLUMN_NAME
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu 
    ON tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
WHERE tc.TABLE_NAME = 'Products';
```

### Backup e Restore
```sql
-- Backup
BACKUP DATABASE MyDatabase 
TO DISK = 'C:\Backup\MyDatabase.bak'
WITH FORMAT, COMPRESSION;

-- Restore
RESTORE DATABASE MyDatabase 
FROM DISK = 'C:\Backup\MyDatabase.bak'
WITH REPLACE;
```

## 💡 Dicas Importantes

### Performance
- Use índices apropriados
- Evite SELECT * em produção
- Use WHERE para limitar resultados
- Prefira INNER JOIN a subconsultas quando possível
- Use LIMIT/TOP para paginação

### Segurança
- Sempre use parâmetros para evitar SQL Injection
- Aplique princípio do menor privilégio
- Valide entrada de dados
- Use transações para operações críticas

### Manutenibilidade
- Use aliases claros
- Comente SQL complexo
- Mantenha padrões de nomenclatura
- Evite funções em WHERE quando possível

## 📚 Referências Rápidas

### Operadores de Comparação
- `=` Igual
- `<>` ou `!=` Diferente
- `<` Menor
- `>` Maior
- `<=` Menor ou igual
- `>=` Maior ou igual
- `BETWEEN` Entre valores
- `IN` Na lista
- `LIKE` Padrão de texto
- `IS NULL` É nulo
- `IS NOT NULL` Não é nulo

### Wildcards para LIKE
- `%` Zero ou mais caracteres
- `_` Exatamente um caractere
- `[abc]` Qualquer caractere na lista
- `[a-z]` Qualquer caractere no intervalo
- `[^abc]` Qualquer caractere não na lista

### Funções Agregadas
- `COUNT()` Conta registros
- `SUM()` Soma valores
- `AVG()` Média
- `MIN()` Valor mínimo
- `MAX()` Valor máximo
- `GROUP_CONCAT()` Concatena valores do grupo

Este guia serve como referência rápida para os comandos SQL mais utilizados no dia a dia!
