-- Matheus Marques Stefani

IF OBJECT_ID(N'dbo.tb_logradouro', N'U') IS NULL
BEGIN
CREATE TABLE tb_logradouro (
    id_logradouro INT IDENTITY(1,1) PRIMARY KEY,
    cep NVARCHAR(8) NOT NULL UNIQUE,
    nome NVARCHAR(150) NOT NULL,
    bairro NVARCHAR(100) NOT NULL,
    cidade NVARCHAR(100) NOT NULL,
    estado NVARCHAR(2) NOT NULL,
    pais NVARCHAR(50) NOT NULL DEFAULT 'Brasil'
);
CREATE INDEX ix_tb_logradouro_cidade ON tb_logradouro(cidade);
CREATE INDEX ix_tb_logradouro_bairro ON tb_logradouro(bairro);
END;

IF OBJECT_ID(N'dbo.tb_aluno', N'U') IS NULL
BEGIN
CREATE TABLE tb_aluno (
    id_aluno INT IDENTITY(1,1) PRIMARY KEY,
    cpf NVARCHAR(11) NOT NULL UNIQUE,
    nome NVARCHAR(150) NOT NULL,
    nascimento DATE NOT NULL,
    telefone NVARCHAR(15) NOT NULL,
    email NVARCHAR(150) NOT NULL UNIQUE,
    logradouro_id INT NOT NULL,
    numero NVARCHAR(20) NOT NULL,
    complemento NVARCHAR(100) NULL,
    senha NVARCHAR(255) NOT NULL,
    foto VARBINARY(MAX) NULL,
    FOREIGN KEY (logradouro_id) REFERENCES tb_logradouro(id_logradouro)
);
CREATE INDEX ix_tb_aluno_logradouro_id ON tb_aluno(logradouro_id);
END;

IF OBJECT_ID(N'dbo.tb_colaborador', N'U') IS NULL
BEGIN
CREATE TABLE tb_colaborador (
    id_colaborador INT IDENTITY(1,1) PRIMARY KEY,
    cpf NVARCHAR(11) NOT NULL UNIQUE,
    nome NVARCHAR(150) NOT NULL,
    nascimento DATE NOT NULL,
    telefone NVARCHAR(15) NOT NULL,
    email NVARCHAR(150) NOT NULL UNIQUE,
    logradouro_id INT NOT NULL,
    numero NVARCHAR(20) NOT NULL,
    complemento NVARCHAR(100) NULL,
    senha NVARCHAR(255) NOT NULL,
    foto VARBINARY(MAX) NULL,
    admissao DATE NOT NULL,
    tipo INT NOT NULL,
    vinculo INT NOT NULL,
    FOREIGN KEY (logradouro_id) REFERENCES tb_logradouro(id_logradouro)
);
CREATE INDEX ix_tb_colaborador_logradouro_id ON tb_colaborador(logradouro_id);
CREATE INDEX ix_tb_colaborador_tipo ON tb_colaborador(tipo);
CREATE INDEX ix_tb_colaborador_vinculo ON tb_colaborador(vinculo);
END;

IF OBJECT_ID(N'dbo.tb_matricula', N'U') IS NULL
BEGIN
CREATE TABLE tb_matricula (
    id_matricula INT IDENTITY(1,1) PRIMARY KEY,
    aluno_id INT NOT NULL,
    plano INT NOT NULL,
    data_inicio DATE NOT NULL,
    data_fim DATE NOT NULL,
    objetivo NVARCHAR(500) NOT NULL,
    restricao_medica INT NOT NULL DEFAULT 0,
    obs_restricao NVARCHAR(500) NULL,
    laudo_medico VARBINARY(MAX) NULL,
    FOREIGN KEY (aluno_id) REFERENCES tb_aluno(id_aluno) ON DELETE CASCADE
);
CREATE INDEX ix_tb_matricula_aluno_id ON tb_matricula(aluno_id);
CREATE INDEX ix_tb_matricula_data_fim ON tb_matricula(data_fim);
END;

IF OBJECT_ID(N'dbo.tb_acesso', N'U') IS NULL
BEGIN
CREATE TABLE tb_acesso (
    id_acesso INT IDENTITY(1,1) PRIMARY KEY,
    pessoa_tipo INT NOT NULL,
    pessoa_id INT NOT NULL,
    data_hora DATETIME2 NOT NULL
);
CREATE INDEX ix_tb_acesso_pessoa_id ON tb_acesso(pessoa_id);
CREATE INDEX ix_tb_acesso_data_hora ON tb_acesso(data_hora);
END;

