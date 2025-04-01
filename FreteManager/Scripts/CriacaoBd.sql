/*
 * Script de Criação do Banco de Dados para o Sistema FreteManager
 * 
 * Este script cria o banco de dados FreteManagerDB se ele ainda não existir.
 * Se o banco já existir, nenhuma alteração será feita para preservar os dados.
 *
 * Data: 26/03/2025
 * Versão: 1.0
 */

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