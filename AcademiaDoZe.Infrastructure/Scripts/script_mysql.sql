-- Matheus Marques Stefani

CREATE TABLE IF NOT EXISTS tb_logradouro (
    id_logradouro INT AUTO_INCREMENT PRIMARY KEY,
    cep VARCHAR(8) NOT NULL UNIQUE,
    nome VARCHAR(150) NOT NULL,
    bairro VARCHAR(100) NOT NULL,
    cidade VARCHAR(100) NOT NULL,
    estado VARCHAR(2) NOT NULL,
    pais VARCHAR(50) NOT NULL DEFAULT 'Brasil',
    INDEX ix_tb_logradouro_cidade (cidade),
    INDEX ix_tb_logradouro_bairro (bairro)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS tb_aluno (
    id_aluno INT AUTO_INCREMENT PRIMARY KEY,
    cpf VARCHAR(11) NOT NULL UNIQUE,
    nome VARCHAR(150) NOT NULL,
    nascimento DATE NOT NULL,
    telefone VARCHAR(15) NOT NULL,
    email VARCHAR(150) NOT NULL UNIQUE,
    logradouro_id INT NOT NULL,
    numero VARCHAR(20) NOT NULL,
    complemento VARCHAR(100) NULL,
    senha VARCHAR(255) NOT NULL,
    foto LONGBLOB NULL,
    FOREIGN KEY (logradouro_id) REFERENCES tb_logradouro(id_logradouro),
    INDEX ix_tb_aluno_logradouro_id (logradouro_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS tb_colaborador (
    id_colaborador INT AUTO_INCREMENT PRIMARY KEY,
    cpf VARCHAR(11) NOT NULL UNIQUE,
    nome VARCHAR(150) NOT NULL,
    nascimento DATE NOT NULL,
    telefone VARCHAR(15) NOT NULL,
    email VARCHAR(150) NOT NULL UNIQUE,
    logradouro_id INT NOT NULL,
    numero VARCHAR(20) NOT NULL,
    complemento VARCHAR(100) NULL,
    senha VARCHAR(255) NOT NULL,
    foto LONGBLOB NULL,
    admissao DATE NOT NULL,
    tipo INT NOT NULL,
    vinculo INT NOT NULL,
    FOREIGN KEY (logradouro_id) REFERENCES tb_logradouro(id_logradouro),
    INDEX ix_tb_colaborador_logradouro_id (logradouro_id),
    INDEX ix_tb_colaborador_tipo (tipo),
    INDEX ix_tb_colaborador_vinculo (vinculo)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS tb_matricula (
    id_matricula INT AUTO_INCREMENT PRIMARY KEY,
    aluno_id INT NOT NULL,
    plano INT NOT NULL,
    data_inicio DATE NOT NULL,
    data_fim DATE NOT NULL,
    objetivo VARCHAR(500) NOT NULL,
    restricao_medica INT NOT NULL DEFAULT 0,
    obs_restricao VARCHAR(500) NULL,
    laudo_medico LONGBLOB NULL,
    FOREIGN KEY (aluno_id) REFERENCES tb_aluno(id_aluno) ON DELETE CASCADE,
    INDEX ix_tb_matricula_aluno_id (aluno_id),
    INDEX ix_tb_matricula_data_fim (data_fim)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS tb_acesso (
    id_acesso INT AUTO_INCREMENT PRIMARY KEY,
    pessoa_tipo INT NOT NULL,
    pessoa_id INT NOT NULL,
    data_hora DATETIME NOT NULL,
    INDEX ix_tb_acesso_pessoa_id (pessoa_id),
    INDEX ix_tb_acesso_data_hora (data_hora)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

