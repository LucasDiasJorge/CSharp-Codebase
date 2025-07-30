-- Create a sample database
CREATE DATABASE IF NOT EXISTS ProcedureDemo;
USE ProcedureDemo;

-- Create a sample table
CREATE TABLE IF NOT EXISTS Numbers (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Value INT
);

-- Insert some sample data
INSERT INTO Numbers (Value) VALUES (1), (2), (3), (4), (5);

-- Create a stored procedure that increments values
DELIMITER //
CREATE PROCEDURE IncrementNumbers(IN incrementBy INT)
BEGIN
    UPDATE Numbers 
    SET Value = Value + incrementBy;
    
    -- Select the updated values
    SELECT * FROM Numbers;
END //
DELIMITER ;
