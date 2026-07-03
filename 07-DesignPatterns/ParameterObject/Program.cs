using ParameterObject;

// Sem Parameter Object: chamada com 8 argumentos posicionais.
string contratoSemPattern = GeradorContratoSemPattern.CriarContrato(
    "Ana Souza",
    "ana.souza@email.com",
    "11999990000",
    new DateTime(2026, 1, 1),
    new DateTime(2026, 12, 31),
    1500.00m,
    "BRL",
    true);

Console.WriteLine("== Sem Parameter Object ==");
Console.WriteLine(contratoSemPattern);

// Com Parameter Object: os mesmos dados agrupados em um unico objeto nomeado.
ContratoParametros parametros = new(
    nomeCliente: "Ana Souza",
    email: "ana.souza@email.com",
    telefone: "11999990000",
    dataInicio: new DateTime(2026, 1, 1),
    dataFim: new DateTime(2026, 12, 31),
    valorMensal: 1500.00m,
    moeda: "BRL",
    renovacaoAutomatica: true);

string contratoComPattern = GeradorContratoComPattern.CriarContrato(parametros);

Console.WriteLine();
Console.WriteLine("== Com Parameter Object ==");
Console.WriteLine(contratoComPattern);
