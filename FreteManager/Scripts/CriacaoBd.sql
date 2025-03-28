-- Verificar se o banco de dados já existe, se não existir, criá-lo
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'FreteManagerDB')
BEGIN
    CREATE DATABASE FreteManagerDB;
    PRINT 'Banco de dados FreteManagerDB criado com sucesso.';
END
ELSE
BEGIN
    PRINT 'Banco de dados FreteManagerDB já existe.';
END
GO