-- Matheus Marques Stefani
-- Execute uma consulta por vez no DB Browser for SQLite.

-- DEV05: logradouro
SELECT id_logradouro, cep, nome, bairro, cidade, estado, pais
FROM tb_logradouro ORDER BY id_logradouro DESC;

-- DEV06: aluno. Senhas ficticias do exercicio.
SELECT id_aluno, nome, cpf, nascimento, telefone, email, logradouro_id,
       numero, complemento, senha
FROM tb_aluno ORDER BY id_aluno DESC;

-- DEV06: colaborador
SELECT id_colaborador, nome, complemento, senha, tipo, vinculo, admissao,
       cpf, nascimento, telefone, email, logradouro_id, numero
FROM tb_colaborador ORDER BY id_colaborador DESC;

-- DEV07: matricula
SELECT id_matricula, aluno_id, objetivo, obs_restricao, plano,
       data_inicio, data_fim, restricao_medica
FROM tb_matricula ORDER BY id_matricula DESC;
