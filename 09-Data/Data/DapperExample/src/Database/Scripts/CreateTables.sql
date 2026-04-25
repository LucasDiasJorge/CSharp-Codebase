CREATE DATABASE IF NOT EXISTS `my-db`;
USE `my-db`;

-- Tabela Users
CREATE TABLE IF NOT EXISTS Users (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE, -- Índice para busca eficiente
    Password VARCHAR(255) NOT NULL -- Maior tamanho para senhas hash
);

-- Tabela Companies
CREATE TABLE IF NOT EXISTS Companies (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(100) NOT NULL UNIQUE -- Empresas devem ter nomes distintos
);

-- Adicionando Foreign Key corretamente
ALTER TABLE Users ADD COLUMN company_id INT NULL;
ALTER TABLE Users ADD CONSTRAINT fk_company FOREIGN KEY (company_id) REFERENCES Companies(Id) 
ON DELETE SET NULL ON UPDATE CASCADE; -- Mantém consistência

-- Garantindo integridade nos IDs
ALTER TABLE Companies MODIFY COLUMN Id INT AUTO_INCREMENT NOT NULL;
ALTER TABLE Users MODIFY COLUMN Id INT AUTO_INCREMENT NOT NULL;
