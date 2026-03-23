-- cria banco se não existir
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'direcional_db')
BEGIN
    CREATE DATABASE direcional_db;
END
GO

USE direcional_db;
GO

--cria a tabela clientes	
IF OBJECT_ID('clientes', 'U') IS NULL
BEGIN
    CREATE TABLE clientes (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        nome NVARCHAR(150) NOT NULL,
        email NVARCHAR(150) NOT NULL UNIQUE,
        telefone NVARCHAR(20),
        criado_em DATETIME2 NOT NULL DEFAULT GETDATE()
    );
END
GO

--cria a tabela corretores
IF OBJECT_ID('corretores', 'U') IS NULL
BEGIN
    CREATE TABLE corretores (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        nome NVARCHAR(150) NOT NULL,
        email NVARCHAR(150) NOT NULL UNIQUE,
        senha_hash NVARCHAR(255) NOT NULL,
        ativo BIT NOT NULL DEFAULT 1,
        criado_em DATETIME2 NOT NULL DEFAULT GETDATE()
    );
END
GO

--cria a tabela apartamentos
IF OBJECT_ID('apartamentos', 'U') IS NULL
BEGIN
    CREATE TABLE apartamentos (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        numero NVARCHAR(10) NOT NULL,
        andar INT NOT NULL,
        valor DECIMAL(18,2) NOT NULL,
        status NVARCHAR(20) NOT NULL,
        criado_em DATETIME2 NOT NULL DEFAULT GETDATE(),

        CONSTRAINT CK_apartamento_status
        CHECK (status IN ('Disponível', 'Reservado', 'Vendido')),

        CONSTRAINT UQ_apartamentos_numero_andar
        UNIQUE (numero, andar)
    );
END
GO

--cria a tabela reservas
IF OBJECT_ID('reservas', 'U') IS NULL
BEGIN
    CREATE TABLE reservas (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        cliente_id UNIQUEIDENTIFIER NOT NULL,
        apartamento_id UNIQUEIDENTIFIER NOT NULL,
        corretor_id UNIQUEIDENTIFIER NULL,
        data_reserva DATETIME2 NOT NULL DEFAULT GETDATE(),
        status NVARCHAR(20) NOT NULL,

        CONSTRAINT FK_reserva_cliente
        FOREIGN KEY (cliente_id) REFERENCES clientes(id),

        CONSTRAINT FK_reserva_apartamento
        FOREIGN KEY (apartamento_id) REFERENCES apartamentos(id),

        CONSTRAINT FK_reserva_corretor
        FOREIGN KEY (corretor_id) REFERENCES corretores(id),

        CONSTRAINT CK_reserva_status
        CHECK (status IN ('Ativa', 'Cancelada', 'Expirada', 'Fechada'))
    );
END
GO

--cria a tabela vendas
IF OBJECT_ID('vendas', 'U') IS NULL
BEGIN
    CREATE TABLE vendas (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        cliente_id UNIQUEIDENTIFIER NOT NULL,
        apartamento_id UNIQUEIDENTIFIER NOT NULL,
        reserva_id UNIQUEIDENTIFIER NULL,
        corretor_id UNIQUEIDENTIFIER NULL,
        valor_final DECIMAL(18,2) NOT NULL,
        data_venda DATETIME2 NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_venda_cliente
        FOREIGN KEY (cliente_id) REFERENCES clientes(id),

        CONSTRAINT FK_venda_apartamento
        FOREIGN KEY (apartamento_id) REFERENCES apartamentos(id),

        CONSTRAINT FK_venda_reserva
        FOREIGN KEY (reserva_id) REFERENCES reservas(id),

        CONSTRAINT FK_venda_corretor
        FOREIGN KEY (corretor_id) REFERENCES corretores(id)
    );
END
GO

--mocks clientes
IF NOT EXISTS (SELECT 1 FROM clientes WHERE email = 'joao@email.com')
BEGIN
    INSERT INTO clientes (nome, email, telefone)
    VALUES ('João Silva', 'joao@email.com', '11999999999');
END
GO

IF NOT EXISTS (SELECT 1 FROM clientes WHERE email = 'maria@email.com')
BEGIN
    INSERT INTO clientes (nome, email, telefone)
    VALUES ('Maria Oliveira', 'maria@email.com', '11988888888');
END
GO

--mocks apartamentos
IF NOT EXISTS (SELECT 1 FROM apartamentos WHERE numero = '101' AND andar = 1)
BEGIN
    INSERT INTO apartamentos (numero, andar, valor, status)
    VALUES ('101', 1, 250000, 'Disponível');
END
GO

IF NOT EXISTS (SELECT 1 FROM apartamentos WHERE numero = '102' AND andar = 1)
BEGIN
    INSERT INTO apartamentos (numero, andar, valor, status)
    VALUES ('102', 1, 260000, 'Disponível');
END
GO

IF NOT EXISTS (SELECT 1 FROM apartamentos WHERE numero = '201' AND andar = 2)
BEGIN
    INSERT INTO apartamentos (numero, andar, valor, status)
    VALUES ('201', 2, 300000, 'Disponível');
END
GO

--mock corretor adm
IF NOT EXISTS (SELECT 1 FROM corretores WHERE email = 'admin@direcional.com')
BEGIN
    INSERT INTO corretores (nome, email, senha_hash, ativo)
    VALUES (
        'Corretor Admin',
        'admin@direcional.com',
        '$2a$11$YoJONtdVazgqzwXMnF4kmuV57xgkYv2j/vWEJGok/8dlZgtDMore.',
        1
    );
END
GO
