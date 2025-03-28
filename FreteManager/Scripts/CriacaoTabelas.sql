
-- Usar o banco de dados
USE FreteManagerDB;
GO

-- Criação da tabela Clientes
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Clientes')
BEGIN
    CREATE TABLE Clientes (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nome NVARCHAR(100) NOT NULL,
        Endereco NVARCHAR(200) NOT NULL,
        Telefone NVARCHAR(20) NOT NULL,
        Email NVARCHAR(100) NOT NULL UNIQUE
    );

    PRINT 'Tabela Clientes criada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Tabela Clientes já existe.';
END

-- Criação da tabela Usuarios
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Usuarios')
BEGIN
    CREATE TABLE Usuarios (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nome NVARCHAR(50) NOT NULL,
        Email NVARCHAR(100) NOT NULL UNIQUE,
        Senha NVARCHAR(200) NOT NULL,
        Role NVARCHAR(20) NOT NULL DEFAULT 'Usuario'
    );

    PRINT 'Tabela Usuarios criada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Tabela Usuarios já existe.';
END

-- Criação da tabela Pedidos
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Pedidos')
BEGIN
    CREATE TABLE Pedidos (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ClienteId INT NOT NULL,
        Origem NVARCHAR(200) NOT NULL,
        Destino NVARCHAR(200) NOT NULL,
        DataCriacao DATETIME NOT NULL DEFAULT GETDATE(),
        Status INT NOT NULL, -- 1: EmProcessamento, 2: Enviado, 3: Entregue, 4: Cancelado
        ValorFrete DECIMAL(18,2),
        CONSTRAINT FK_Pedidos_Clientes FOREIGN KEY (ClienteId) REFERENCES Clientes(Id) ON DELETE NO ACTION
    );

    PRINT 'Tabela Pedidos criada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Tabela Pedidos já existe.';
END

-- Criação dos índices
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Clientes_Email')
BEGIN
    CREATE INDEX IX_Clientes_Email ON Clientes(Email);
    PRINT 'Índice IX_Clientes_Email criado com sucesso.';
END
ELSE
BEGIN
    PRINT 'Índice IX_Clientes_Email já existe.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Pedidos_ClienteId')
BEGIN
    CREATE INDEX IX_Pedidos_ClienteId ON Pedidos(ClienteId);
    PRINT 'Índice IX_Pedidos_ClienteId criado com sucesso.';
END
ELSE
BEGIN
    PRINT 'Índice IX_Pedidos_ClienteId já existe.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Usuarios_Email')
BEGIN
    CREATE INDEX IX_Usuarios_Email ON Usuarios(Email);
    PRINT 'Índice IX_Usuarios_Email criado com sucesso.';
END
ELSE
BEGIN
    PRINT 'Índice IX_Usuarios_Email já existe.';
END

-- Inserir dados iniciais (opcional)
-- Inserir um usuário administrador para testes
IF NOT EXISTS (SELECT TOP 1 * FROM Usuarios)
BEGIN
    -- Senha: 'Admin@123' (em um cenário real, usaríamos hash)
    INSERT INTO Usuarios (Nome, Email, Senha, Role)
    VALUES ('Administrador', 'admin@fretemanager.com', 'SEtGdUhrbFNGV3Q4VWRXc3FzOU4zQnliajJtT2o0YWU1UlRsQ2JXUWdvQT0=', 'Admin');
    
    PRINT 'Usuário admin criado com sucesso.';
END

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