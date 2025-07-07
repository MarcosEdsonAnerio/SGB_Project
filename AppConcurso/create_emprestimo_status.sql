CREATE TABLE IF NOT EXISTS EmprestimoStatus (
    IdEmprestimo INT PRIMARY KEY,
    DataDevolucao DATETIME NOT NULL,
    FOREIGN KEY (IdEmprestimo) REFERENCES Emprestimos(IdEmprestimo) ON DELETE CASCADE
);
