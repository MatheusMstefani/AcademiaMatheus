-- Matheus Marques Stefani
PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS tb_logradouro (
    id_logradouro INTEGER PRIMARY KEY AUTOINCREMENT,
    cep TEXT COLLATE NOCASE NOT NULL UNIQUE,
    nome TEXT COLLATE NOCASE NOT NULL,
    bairro TEXT COLLATE NOCASE NOT NULL,
    cidade TEXT COLLATE NOCASE NOT NULL,
    estado TEXT COLLATE NOCASE NOT NULL,
    pais TEXT COLLATE NOCASE NOT NULL DEFAULT 'Brasil'
);
CREATE INDEX IF NOT EXISTS ix_tb_logradouro_cidade ON tb_logradouro(cidade);
CREATE INDEX IF NOT EXISTS ix_tb_logradouro_bairro ON tb_logradouro(bairro);

CREATE TABLE IF NOT EXISTS tb_aluno (
    id_aluno INTEGER PRIMARY KEY AUTOINCREMENT,
    cpf TEXT COLLATE NOCASE NOT NULL UNIQUE,
    nome TEXT COLLATE NOCASE NOT NULL,
    nascimento TEXT NOT NULL,
    telefone TEXT COLLATE NOCASE NOT NULL,
    email TEXT COLLATE NOCASE NOT NULL UNIQUE,
    logradouro_id INT NOT NULL,
    numero TEXT COLLATE NOCASE NOT NULL,
    complemento TEXT COLLATE NOCASE NULL,
    senha TEXT COLLATE NOCASE NOT NULL,
    foto BLOB NULL,
    FOREIGN KEY (logradouro_id) REFERENCES tb_logradouro(id_logradouro)
);
CREATE INDEX IF NOT EXISTS ix_tb_aluno_logradouro_id ON tb_aluno(logradouro_id);

CREATE TABLE IF NOT EXISTS tb_colaborador (
    id_colaborador INTEGER PRIMARY KEY AUTOINCREMENT,
    cpf TEXT COLLATE NOCASE NOT NULL UNIQUE,
    nome TEXT COLLATE NOCASE NOT NULL,
    nascimento TEXT NOT NULL,
    telefone TEXT COLLATE NOCASE NOT NULL,
    email TEXT COLLATE NOCASE NOT NULL UNIQUE,
    logradouro_id INT NOT NULL,
    numero TEXT COLLATE NOCASE NOT NULL,
    complemento TEXT COLLATE NOCASE NULL,
    senha TEXT COLLATE NOCASE NOT NULL,
    foto BLOB NULL,
    admissao TEXT NOT NULL,
    tipo INT NOT NULL,
    vinculo INT NOT NULL,
    FOREIGN KEY (logradouro_id) REFERENCES tb_logradouro(id_logradouro)
);
CREATE INDEX IF NOT EXISTS ix_tb_colaborador_logradouro_id ON tb_colaborador(logradouro_id);
CREATE INDEX IF NOT EXISTS ix_tb_colaborador_tipo ON tb_colaborador(tipo);
CREATE INDEX IF NOT EXISTS ix_tb_colaborador_vinculo ON tb_colaborador(vinculo);

CREATE TABLE IF NOT EXISTS tb_matricula (
    id_matricula INTEGER PRIMARY KEY AUTOINCREMENT,
    aluno_id INT NOT NULL,
    plano INT NOT NULL,
    data_inicio TEXT NOT NULL,
    data_fim TEXT NOT NULL,
    objetivo TEXT COLLATE NOCASE NOT NULL,
    restricao_medica INT NOT NULL DEFAULT 0,
    obs_restricao TEXT COLLATE NOCASE NULL,
    laudo_medico BLOB NULL,
    FOREIGN KEY (aluno_id) REFERENCES tb_aluno(id_aluno) ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS ix_tb_matricula_aluno_id ON tb_matricula(aluno_id);
CREATE INDEX IF NOT EXISTS ix_tb_matricula_data_fim ON tb_matricula(data_fim);

CREATE TABLE IF NOT EXISTS tb_acesso (
    id_acesso INTEGER PRIMARY KEY AUTOINCREMENT,
    pessoa_tipo INT NOT NULL,
    pessoa_id INT NOT NULL,
    data_hora TEXT NOT NULL
);
CREATE INDEX IF NOT EXISTS ix_tb_acesso_pessoa_id ON tb_acesso(pessoa_id);
CREATE INDEX IF NOT EXISTS ix_tb_acesso_data_hora ON tb_acesso(data_hora);

