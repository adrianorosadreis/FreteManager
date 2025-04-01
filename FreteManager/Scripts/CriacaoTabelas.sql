/*
 * Script de Criação de Tabelas para o Sistema FreteManager
 * 
 * Este script cria a estrutura completa de tabelas, índices, restrições e dados iniciais
 * necessários para o funcionamento do sistema FreteManager.
 *
 * Pré-requisito: O banco de dados FreteManagerDB deve estar criado (execute CriacaoBd.sql primeiro)
 *
 * Data: 26/03/2025
 * Versão: 1.0
 */


-- Usar o banco de dados
USE FreteManagerDB;
GO

PRINT 'Iniciando criação de tabelas no banco de dados FreteManagerDB...';
GO

-- Criação da tabela Clientes
-- Armazena informações de todos os clientes do sistema
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Clientes')
BEGIN
    CREATE TABLE Clientes (
        Id INT IDENTITY(1,1) PRIMARY KEY,             -- Identificador único autoincremental
        Nome NVARCHAR(100) NOT NULL,                  -- Nome ou razão social do cliente
        Endereco NVARCHAR(200) NOT NULL,              -- Endereço completo
        Telefone NVARCHAR(20) NOT NULL,               -- Telefone de contato
        Email NVARCHAR(100) NOT NULL UNIQUE,          -- Email único para comunicação e identificação
        DataCadastro DATETIME DEFAULT GETDATE()       -- Data de cadastro do cliente
    );

    PRINT 'Tabela Clientes criada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Tabela Clientes já existe.';
END
GO

-- Criação da tabela Usuarios
-- Armazena os usuários que têm acesso ao sistema
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Usuarios')
BEGIN
    CREATE TABLE Usuarios (
        Id INT IDENTITY(1,1) PRIMARY KEY,             -- Identificador único autoincremental
        Nome NVARCHAR(50) NOT NULL,                   -- Nome completo do usuário
        Email NVARCHAR(100) NOT NULL UNIQUE,          -- Email único usado para login
        Senha NVARCHAR(200) NOT NULL,                 -- Hash da senha (nunca armazenar senhas em texto plano)
        Role NVARCHAR(20) NOT NULL                    -- Papel/Função do usuário (Admin, Usuario, etc.)
            DEFAULT 'Usuario',                        -- Valor padrão para novos usuários
        DataCadastro DATETIME DEFAULT GETDATE(),      -- Data de criação da conta
        UltimoAcesso DATETIME NULL                    -- Data do último acesso
    );

    PRINT 'Tabela Usuarios criada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Tabela Usuarios já existe.';
END
GO

-- Criação da tabela Pedidos
-- Armazena os pedidos de transporte
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Pedidos')
BEGIN
    CREATE TABLE Pedidos (
        Id INT IDENTITY(1,1) PRIMARY KEY,             -- Identificador único autoincremental
        ClienteId INT NOT NULL,                       -- Referência ao cliente que fez o pedido
        Origem NVARCHAR(200) NOT NULL,                -- Endereço ou CEP de origem
        Destino NVARCHAR(200) NOT NULL,               -- Endereço ou CEP de destino
        DataCriacao DATETIME NOT NULL                 -- Data de criação do pedido
            DEFAULT GETDATE(),
        Status INT NOT NULL                           -- Status do pedido (1=EmProcessamento, 2=Enviado, 3=Entregue, 4=Cancelado)
            DEFAULT 1,
        ValorFrete DECIMAL(18,2) NULL,                -- Valor calculado do frete (pode ser calculado posteriormente)
        ValorDeclarado DECIMAL(18,2) NOT NULL         -- Valor declarado da mercadoria
            DEFAULT 100.00,                           -- Valor padrão para cálculo de frete e seguro
        
        -- Restrição de chave estrangeira para garantir que só possam ser cadastrados pedidos
        -- para clientes que existem no sistema
        CONSTRAINT FK_Pedidos_Clientes 
            FOREIGN KEY (ClienteId) 
            REFERENCES Clientes(Id) 
            ON DELETE NO ACTION                       -- Não permite excluir cliente com pedidos
    );

    PRINT 'Tabela Pedidos criada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Tabela Pedidos já existe.';
END
GO

-- Criação da tabela Pacotes
-- Armazena os pacotes associados a cada pedido
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Pacotes')
BEGIN
    CREATE TABLE Pacotes (
        Id INT IDENTITY(1,1) PRIMARY KEY,             -- Identificador único autoincremental
        PedidoId INT NOT NULL,                        -- Referência ao pedido ao qual o pacote pertence
        Altura DECIMAL(10,2) NOT NULL,                -- Altura em centímetros
        Largura DECIMAL(10,2) NOT NULL,               -- Largura em centímetros
        Comprimento DECIMAL(10,2) NOT NULL,           -- Comprimento em centímetros
        Peso DECIMAL(10,2) NOT NULL,                  -- Peso em quilogramas
        Quantidade INT NOT NULL DEFAULT 1,            -- Quantidade de pacotes com estas dimensões
        
        -- Restrição de chave estrangeira para garantir que pacotes só existam
        -- vinculados a pedidos existentes
        CONSTRAINT FK_Pacotes_Pedidos 
            FOREIGN KEY (PedidoId) 
            REFERENCES Pedidos(Id) 
            ON DELETE CASCADE                         -- Se o pedido for excluído, os pacotes também serão
    );

    PRINT 'Tabela Pacotes criada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Tabela Pacotes já existe.';
END
GO

-- Criação da tabela HistoricoStatus
-- Armazena o histórico de mudanças de status dos pedidos para auditoria
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'HistoricoStatus')
BEGIN
    CREATE TABLE HistoricoStatus (
        Id INT IDENTITY(1,1) PRIMARY KEY,             -- Identificador único autoincremental
        PedidoId INT NOT NULL,                        -- Referência ao pedido
        StatusAnterior INT NULL,                      -- Status anterior (NULL se for o status inicial)
        StatusNovo INT NOT NULL,                      -- Novo status
        DataAlteracao DATETIME NOT NULL               -- Data da alteração
            DEFAULT GETDATE(),
        UsuarioId INT NULL,                           -- Usuário que fez a alteração (se disponível)
        Observacao NVARCHAR(500) NULL,                -- Observação opcional sobre a mudança
        
        -- Restrições de chave estrangeira
        CONSTRAINT FK_HistoricoStatus_Pedidos 
            FOREIGN KEY (PedidoId) 
            REFERENCES Pedidos(Id) 
            ON DELETE CASCADE,                        -- Se o pedido for excluído, o histórico também será
        
        CONSTRAINT FK_HistoricoStatus_Usuarios 
            FOREIGN KEY (UsuarioId) 
            REFERENCES Usuarios(Id) 
            ON DELETE SET NULL                        -- Se o usuário for excluído, mantém o registro mas sem referência
    );

    PRINT 'Tabela HistoricoStatus criada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Tabela HistoricoStatus já existe.';
END
GO

-- Criação dos índices para otimizar consultas comuns
PRINT 'Criando índices para otimização de consultas...';
GO

-- Índice para busca de clientes por email (usado em login e verificações)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Clientes_Email')
BEGIN
    CREATE INDEX IX_Clientes_Email ON Clientes(Email);
    PRINT 'Índice IX_Clientes_Email criado com sucesso.';
END
ELSE
BEGIN
    PRINT 'Índice IX_Clientes_Email já existe.';
END
GO

-- Índice para busca de pedidos por cliente (para listar pedidos de um cliente)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Pedidos_ClienteId')
BEGIN
    CREATE INDEX IX_Pedidos_ClienteId ON Pedidos(ClienteId);
    PRINT 'Índice IX_Pedidos_ClienteId criado com sucesso.';
END
ELSE
BEGIN
    PRINT 'Índice IX_Pedidos_ClienteId já existe.';
END
GO

-- Índice para busca de pedidos por status (para filtrar pedidos por status)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Pedidos_Status')
BEGIN
    CREATE INDEX IX_Pedidos_Status ON Pedidos(Status);
    PRINT 'Índice IX_Pedidos_Status criado com sucesso.';
END
ELSE
BEGIN
    PRINT 'Índice IX_Pedidos_Status já existe.';
END
GO


-- Índice para busca de pacotes por pedido (para encontrar todos os pacotes de um pedido)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Pacotes_PedidoId')
BEGIN
    CREATE INDEX IX_Pacotes_PedidoId ON Pacotes(PedidoId);
    PRINT 'Índice IX_Pacotes_PedidoId criado com sucesso.';
END
ELSE
BEGIN
    PRINT 'Índice IX_Pacotes_PedidoId já existe.';
END
GO

-- Inserir dados iniciais
PRINT 'Inserindo dados iniciais para o ambiente de testes...';
GO

-- Inserir um usuário administrador para testes
IF NOT EXISTS (SELECT TOP 1 * FROM Usuarios WHERE Email = 'admin@fretemanager.com')
BEGIN
    -- Senha: 'Admin@123' (hash usando PBKDF2 - na aplicação real deve ser gerado com salt)
    INSERT INTO Usuarios (Nome, Email, Senha, Role)
    VALUES ('Administrador', 'admin@fretemanager.com', 'SEtGdUhrbFNGV3Q4VWRXc3FzOU4zQnliajJtT2o0YWU1UlRsQ2JXUWdvQT0=', 'Admin');
    
    PRINT 'Usuário admin criado com sucesso.';
END
ELSE
BEGIN
    PRINT 'Usuário admin já existe.';
END
GO

-- Inserir um usuário comum para testes
IF NOT EXISTS (SELECT TOP 1 * FROM Usuarios WHERE Email = 'usuario@fretemanager.com')
BEGIN
    -- Senha: 'Usuario@123' (hash usando PBKDF2 - na aplicação real deve ser gerado com salt)
    INSERT INTO Usuarios (Nome, Email, Senha, Role)
    VALUES ('Usuário Teste', 'usuario@fretemanager.com', 'QUhYZ2RMSjg5NlpXQWcwakQ0ZlNudz09', 'Usuario');
    
    PRINT 'Usuário comum criado com sucesso.';
END
ELSE
BEGIN
    PRINT 'Usuário comum já existe.';
END
GO

-- Inserir alguns clientes para teste
IF NOT EXISTS (SELECT TOP 1 * FROM Clientes)
BEGIN
    INSERT INTO Clientes (Nome, Endereco, Telefone, Email)
    VALUES 
        ('Empresa ABC', 'Av. Paulista, 1000, São Paulo, SP', '(11) 3456-7890', 'contato@empresaabc.com'),
        ('Distribuidora XYZ', 'Rua das Flores, 123, Rio de Janeiro, RJ', '(21) 2345-6789', 'vendas@distribuidoraxyz.com'),
        ('Comércio Rápido', 'Av. Brasil, 500, Belo Horizonte, MG', '(31) 3456-7890', 'pedidos@comerciorapido.com');
    
    PRINT 'Clientes de teste criados com sucesso.';
END
ELSE
BEGIN
    PRINT 'Já existem clientes cadastrados. Nenhum cliente de teste foi adicionado.';
END
GO

-- Inserir alguns pedidos para teste
IF NOT EXISTS (SELECT TOP 1 * FROM Pedidos)
BEGIN
    -- Obter os IDs dos clientes inseridos
    DECLARE @ClienteId1 INT = (SELECT Id FROM Clientes WHERE Email = 'contato@empresaabc.com');
    DECLARE @ClienteId2 INT = (SELECT Id FROM Clientes WHERE Email = 'vendas@distribuidoraxyz.com');
    
    -- Inserir pedidos
    INSERT INTO Pedidos (ClienteId, Origem, Destino, Status, ValorFrete, ValorDeclarado)
    VALUES 
        (@ClienteId1, '01000-000', '02000-000', 1, 150.00, 500.00),  -- Pedido em processamento
        (@ClienteId1, '01000-000', '03000-000', 2, 200.00, 1000.00), -- Pedido enviado
        (@ClienteId2, '20000-000', '01000-000', 3, 180.00, 750.00);  -- Pedido entregue
    
    PRINT 'Pedidos de teste criados com sucesso.';
    
    -- Inserir pacotes para os pedidos criados
    INSERT INTO Pacotes (PedidoId, Altura, Largura, Comprimento, Peso, Quantidade)
    VALUES 
        (1, 20.0, 30.0, 40.0, 5.0, 1),  -- Pacote para o primeiro pedido
        (2, 15.0, 25.0, 35.0, 3.0, 2),  -- Pacote para o segundo pedido
        (2, 10.0, 20.0, 30.0, 2.0, 1),  -- Outro pacote para o segundo pedido
        (3, 30.0, 40.0, 50.0, 10.0, 1); -- Pacote para o terceiro pedido
    
    PRINT 'Pacotes de teste criados com sucesso.';
END
ELSE
BEGIN
    PRINT 'Já existem pedidos cadastrados. Nenhum pedido de teste foi adicionado.';
END
GO

PRINT 'Criação da estrutura do banco de dados FreteManagerDB concluída com sucesso!';
GO