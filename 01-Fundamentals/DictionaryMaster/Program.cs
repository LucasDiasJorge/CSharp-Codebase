using System.Globalization;
using System.Text;
using DictionaryMaster.Models;

namespace DictionaryMaster;

public class Program
{
	private static readonly Dictionary<string, decimal> Inventario = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
	private static readonly Dictionary<string, Contato> Agenda = new Dictionary<string, Contato>(StringComparer.OrdinalIgnoreCase);
	private static readonly Dictionary<string, int> Ranking = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
	private static readonly List<PerguntaQuiz> PerguntasQuiz = CriarPerguntasQuiz();

	public static void Main()
	{
		Console.WriteLine("DictionaryMaster - Laboratorio Interativo de Dictionary<TKey, TValue>");
		Console.WriteLine(new string('=', 72));

		while (true)
		{
			Console.WriteLine();
			Console.WriteLine("Menu principal");
			Console.WriteLine("1 - Inventario de Loja");
			Console.WriteLine("2 - Contador de Palavras");
			Console.WriteLine("3 - Agenda de Contatos");
			Console.WriteLine("4 - Ranking de Pontuacao");
			Console.WriteLine("5 - Quiz sobre Dicionarios");
			Console.WriteLine("6 - Sair");

			Int32 opcaoMenu = LerOpcaoNoIntervalo("Escolha uma opcao: ", 1, 6);

			if (opcaoMenu == 6)
			{
				Console.WriteLine("Encerrando o DictionaryMaster. Bons estudos!");
				break;
			}

			try
			{
				switch (opcaoMenu)
				{
					case 1:
						ExecutarModuloInventario();
						break;
					case 2:
						ExecutarModuloContadorPalavras();
						break;
					case 3:
						ExecutarModuloAgendaContatos();
						break;
					case 4:
						ExecutarModuloRankingPontuacao();
						break;
					case 5:
						ExecutarModuloQuiz();
						break;
					default:
						Console.WriteLine("Opcao invalida.");
						break;
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine("Falha inesperada no modulo atual.");
				Console.WriteLine($"Detalhe tecnico: {exception.Message}");
			}
		}
	}

	private static void ExecutarModuloInventario()
	{
		while (true)
		{
			Console.WriteLine();
			Console.WriteLine("Inventario de Loja");
			Console.WriteLine("1 - Adicionar produto");
			Console.WriteLine("2 - Remover produto");
			Console.WriteLine("3 - Consultar preco de um produto");
			Console.WriteLine("4 - Listar produtos");
			Console.WriteLine("5 - Voltar ao menu principal");

			Int32 opcaoModulo = LerOpcaoNoIntervalo("Escolha uma opcao: ", 1, 5);

			if (opcaoModulo == 5)
			{
				return;
			}

			switch (opcaoModulo)
			{
				case 1:
					AdicionarProdutoInventario();
					break;
				case 2:
					RemoverProdutoInventario();
					break;
				case 3:
					ConsultarPrecoProdutoInventario();
					break;
				case 4:
					ListarProdutosInventario();
					break;
				default:
					Console.WriteLine("Opcao invalida.");
					break;
			}
		}
	}

	private static void AdicionarProdutoInventario()
	{
		String nomeProduto = LerTextoObrigatorio("Nome do produto: ");
		Decimal precoProduto = LerDecimal("Preco do produto: ");

		// ContainsKey evita chamar Add com chave repetida, o que causaria ArgumentException.
		if (Inventario.ContainsKey(nomeProduto))
		{
			// TryGetValue recupera o valor atual sem risco de KeyNotFoundException.
			if (Inventario.TryGetValue(nomeProduto, out Decimal precoAtual))
			{
				Console.WriteLine($"Produto ja existe com preco {precoAtual.ToString("C", CultureInfo.GetCultureInfo("pt-BR"))}.");
			}

			Int32 desejaAtualizar = LerOpcaoNoIntervalo("Deseja atualizar o preco? (1-Sim, 2-Nao): ", 1, 2);

			if (desejaAtualizar == 1)
			{
				Inventario[nomeProduto] = precoProduto;
				Console.WriteLine("Preco atualizado com sucesso.");
			}

			return;
		}

		Inventario.Add(nomeProduto, precoProduto);
		Console.WriteLine("Produto adicionado com sucesso.");
	}

	private static void RemoverProdutoInventario()
	{
		String nomeProduto = LerTextoObrigatorio("Nome do produto para remocao: ");

		if (!Inventario.ContainsKey(nomeProduto))
		{
			Console.WriteLine("Produto nao encontrado no inventario.");
			return;
		}

		Boolean removeu = Inventario.Remove(nomeProduto);
		Console.WriteLine(removeu ? "Produto removido com sucesso." : "Falha ao remover o produto.");
	}

	private static void ConsultarPrecoProdutoInventario()
	{
		String nomeProduto = LerTextoObrigatorio("Nome do produto para consulta: ");

		// TryGetValue e a forma recomendada para consulta segura por chave.
		if (Inventario.TryGetValue(nomeProduto, out Decimal precoProduto))
		{
			Console.WriteLine($"Preco atual: {precoProduto.ToString("C", CultureInfo.GetCultureInfo("pt-BR"))}");
			return;
		}

		Console.WriteLine("Produto nao encontrado no inventario.");
	}

	private static void ListarProdutosInventario()
	{
		if (Inventario.Count == 0)
		{
			Console.WriteLine("Inventario vazio.");
			return;
		}

		IEnumerable<KeyValuePair<string, decimal>> produtosOrdenados = Inventario.OrderBy(
			static (KeyValuePair<string, decimal> par) => par.Key,
			StringComparer.OrdinalIgnoreCase);

		Console.WriteLine("Produtos cadastrados:");

		foreach (KeyValuePair<string, decimal> produto in produtosOrdenados)
		{
			Console.WriteLine($"- {produto.Key}: {produto.Value.ToString("C", CultureInfo.GetCultureInfo("pt-BR"))}");
		}
	}

	private static void ExecutarModuloContadorPalavras()
	{
		Console.WriteLine();
		Console.WriteLine("Contador de Palavras");
		String frase = LerTextoObrigatorio("Digite uma frase: ");

		List<string> palavrasNormalizadas = ExtrairPalavras(frase);

		if (palavrasNormalizadas.Count == 0)
		{
			Console.WriteLine("Nenhuma palavra valida foi encontrada.");
			return;
		}

		Dictionary<string, int> frequencias = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

		foreach (String palavra in palavrasNormalizadas)
		{
			// Padrao TryGetValue + incremento evita excecoes e deixa claro o fluxo de contagem.
			if (frequencias.TryGetValue(palavra, out Int32 contagemAtual))
			{
				frequencias[palavra] = contagemAtual + 1;
				continue;
			}

			frequencias[palavra] = 1;
		}

		IEnumerable<KeyValuePair<string, int>> frequenciasOrdenadas = frequencias
			.OrderByDescending(static (KeyValuePair<string, int> par) => par.Value)
			.ThenBy(static (KeyValuePair<string, int> par) => par.Key, StringComparer.OrdinalIgnoreCase);

		Console.WriteLine("Frequencia de palavras:");

		foreach (KeyValuePair<string, int> item in frequenciasOrdenadas)
		{
			Console.WriteLine($"- {item.Key}: {item.Value}");
		}
	}

	private static List<string> ExtrairPalavras(String frase)
	{
		StringBuilder construtor = new StringBuilder(frase.Length);

		foreach (Char caractere in frase)
		{
			if (Char.IsLetterOrDigit(caractere))
			{
				construtor.Append(Char.ToLowerInvariant(caractere));
			}
			else
			{
				construtor.Append(' ');
			}
		}

		String[] partes = construtor
			.ToString()
			.Split(' ', StringSplitOptions.RemoveEmptyEntries);

		return new List<string>(partes);
	}

	private static void ExecutarModuloAgendaContatos()
	{
		while (true)
		{
			Console.WriteLine();
			Console.WriteLine("Agenda de Contatos");
			Console.WriteLine("1 - Criar contato");
			Console.WriteLine("2 - Ler contato");
			Console.WriteLine("3 - Atualizar contato");
			Console.WriteLine("4 - Deletar contato");
			Console.WriteLine("5 - Listar contatos");
			Console.WriteLine("6 - Voltar ao menu principal");

			Int32 opcaoModulo = LerOpcaoNoIntervalo("Escolha uma opcao: ", 1, 6);

			if (opcaoModulo == 6)
			{
				return;
			}

			switch (opcaoModulo)
			{
				case 1:
					CriarContato();
					break;
				case 2:
					LerContato();
					break;
				case 3:
					AtualizarContato();
					break;
				case 4:
					DeletarContato();
					break;
				case 5:
					ListarContatos();
					break;
				default:
					Console.WriteLine("Opcao invalida.");
					break;
			}
		}
	}

	private static void CriarContato()
	{
		String nomeContato = LerTextoObrigatorio("Nome do contato: ");

		if (Agenda.ContainsKey(nomeContato))
		{
			Console.WriteLine("Ja existe um contato com esse nome.");
			return;
		}

		String telefone = LerTextoObrigatorio("Telefone: ");
		String email = LerTextoObrigatorio("Email: ");

		// Aqui o Dictionary armazena um objeto complexo como valor para agrupar multiplos dados do contato.
		Contato novoContato = new Contato(telefone, email);
		Agenda.Add(nomeContato, novoContato);
		Console.WriteLine("Contato criado com sucesso.");
	}

	private static void LerContato()
	{
		String nomeContato = LerTextoObrigatorio("Nome do contato para leitura: ");

		// TryGetValue evita indexacao direta em chave inexistente.
		if (Agenda.TryGetValue(nomeContato, out Contato? contatoEncontrado))
		{
			Console.WriteLine($"Nome: {nomeContato}");
			Console.WriteLine($"Telefone: {contatoEncontrado.Telefone}");
			Console.WriteLine($"Email: {contatoEncontrado.Email}");
			return;
		}

		Console.WriteLine("Contato nao encontrado.");
	}

	private static void AtualizarContato()
	{
		String nomeContato = LerTextoObrigatorio("Nome do contato para atualizacao: ");

		if (!Agenda.TryGetValue(nomeContato, out Contato? contatoAtual))
		{
			Console.WriteLine("Contato nao encontrado.");
			return;
		}

		String novoTelefone = LerTextoOpcional("Novo telefone (pressione Enter para manter): ");
		String novoEmail = LerTextoOpcional("Novo email (pressione Enter para manter): ");

		if (String.IsNullOrWhiteSpace(novoTelefone))
		{
			novoTelefone = contatoAtual.Telefone;
		}

		if (String.IsNullOrWhiteSpace(novoEmail))
		{
			novoEmail = contatoAtual.Email;
		}

		contatoAtual.Atualizar(novoTelefone, novoEmail);
		Console.WriteLine("Contato atualizado com sucesso.");
	}

	private static void DeletarContato()
	{
		String nomeContato = LerTextoObrigatorio("Nome do contato para remocao: ");
		Boolean removeu = Agenda.Remove(nomeContato);
		Console.WriteLine(removeu ? "Contato removido com sucesso." : "Contato nao encontrado.");
	}

	private static void ListarContatos()
	{
		if (Agenda.Count == 0)
		{
			Console.WriteLine("Agenda vazia.");
			return;
		}

		IEnumerable<KeyValuePair<string, Contato>> contatosOrdenados = Agenda.OrderBy(
			static (KeyValuePair<string, Contato> par) => par.Key,
			StringComparer.OrdinalIgnoreCase);

		Console.WriteLine("Contatos cadastrados:");

		foreach (KeyValuePair<string, Contato> contato in contatosOrdenados)
		{
			Console.WriteLine($"- {contato.Key} | Telefone: {contato.Value.Telefone} | Email: {contato.Value.Email}");
		}
	}

	private static void ExecutarModuloRankingPontuacao()
	{
		while (true)
		{
			Console.WriteLine();
			Console.WriteLine("Ranking de Pontuacao");
			Console.WriteLine("1 - Adicionar pontos de jogador");
			Console.WriteLine("2 - Exibir top 3");
			Console.WriteLine("3 - Listar ranking completo");
			Console.WriteLine("4 - Voltar ao menu principal");

			Int32 opcaoModulo = LerOpcaoNoIntervalo("Escolha uma opcao: ", 1, 4);

			if (opcaoModulo == 4)
			{
				return;
			}

			switch (opcaoModulo)
			{
				case 1:
					AdicionarPontuacaoJogador();
					break;
				case 2:
					ExibirTopTresRanking();
					break;
				case 3:
					ExibirRankingCompleto();
					break;
				default:
					Console.WriteLine("Opcao invalida.");
					break;
			}
		}
	}

	private static void AdicionarPontuacaoJogador()
	{
		String nomeJogador = LerTextoObrigatorio("Nome do jogador: ");
		Int32 pontos = LerInteiro("Pontos para adicionar (pode ser negativo): ");

		if (Ranking.TryGetValue(nomeJogador, out Int32 pontuacaoAtual))
		{
			Ranking[nomeJogador] = pontuacaoAtual + pontos;
			Console.WriteLine($"Pontuacao atualizada para {Ranking[nomeJogador]}.");
			return;
		}

		Ranking.Add(nomeJogador, pontos);
		Console.WriteLine("Jogador adicionado ao ranking.");
	}

	private static void ExibirTopTresRanking()
	{
		if (Ranking.Count == 0)
		{
			Console.WriteLine("Ranking vazio.");
			return;
		}

		// OrderByDescending e Take(3) entregam o top 3 sem logica manual de comparacao.
		List<KeyValuePair<string, int>> topTres = Ranking
			.OrderByDescending(static (KeyValuePair<string, int> par) => par.Value)
			.ThenBy(static (KeyValuePair<string, int> par) => par.Key, StringComparer.OrdinalIgnoreCase)
			.Take(3)
			.ToList();

		Console.WriteLine("Top 3 jogadores:");

		// Iteracao com indice deixa explicita a posicao de cada jogador no ranking.
		for (Int32 indice = 0; indice < topTres.Count; indice++)
		{
			KeyValuePair<string, int> jogador = topTres[indice];
			Console.WriteLine($"{indice + 1}o lugar - {jogador.Key}: {jogador.Value} pontos");
		}
	}

	private static void ExibirRankingCompleto()
	{
		if (Ranking.Count == 0)
		{
			Console.WriteLine("Ranking vazio.");
			return;
		}

		List<KeyValuePair<string, int>> rankingOrdenado = Ranking
			.OrderByDescending(static (KeyValuePair<string, int> par) => par.Value)
			.ThenBy(static (KeyValuePair<string, int> par) => par.Key, StringComparer.OrdinalIgnoreCase)
			.ToList();

		Console.WriteLine("Ranking completo:");

		for (Int32 indice = 0; indice < rankingOrdenado.Count; indice++)
		{
			KeyValuePair<string, int> jogador = rankingOrdenado[indice];
			Console.WriteLine($"{indice + 1}o lugar - {jogador.Key}: {jogador.Value} pontos");
		}
	}

	private static void ExecutarModuloQuiz()
	{
		Console.WriteLine();
		Console.WriteLine("Quiz sobre Dicionarios");
		Console.WriteLine("Responda usando A, B, C ou D.");

		Int32 pontuacao = 0;

		for (Int32 indicePergunta = 0; indicePergunta < PerguntasQuiz.Count; indicePergunta++)
		{
			PerguntaQuiz perguntaAtual = PerguntasQuiz[indicePergunta];

			Console.WriteLine();
			Console.WriteLine($"Pergunta {indicePergunta + 1} de {PerguntasQuiz.Count}");
			Console.WriteLine(perguntaAtual.Enunciado);

			IEnumerable<KeyValuePair<char, string>> opcoesOrdenadas = perguntaAtual.Opcoes.OrderBy(
				static (KeyValuePair<char, string> par) => par.Key);

			foreach (KeyValuePair<char, string> opcao in opcoesOrdenadas)
			{
				Console.WriteLine($"{opcao.Key}) {opcao.Value}");
			}

			Char alternativaMarcada = LerAlternativaQuiz(perguntaAtual.Opcoes);

			if (alternativaMarcada == perguntaAtual.AlternativaCorreta)
			{
				pontuacao++;
				Console.WriteLine("Resposta correta.");
			}
			else
			{
				// TryGetValue garante leitura segura da alternativa correta armazenada no Dictionary.
				if (perguntaAtual.Opcoes.TryGetValue(perguntaAtual.AlternativaCorreta, out String? textoAlternativaCorreta))
				{
					Console.WriteLine($"Resposta incorreta. Correta: {perguntaAtual.AlternativaCorreta}) {textoAlternativaCorreta}");
				}
			}

			Console.WriteLine($"Explicacao: {perguntaAtual.Explicacao}");
		}

		Console.WriteLine();
		Console.WriteLine($"Resultado final: {pontuacao} de {PerguntasQuiz.Count} acertos.");
	}

	private static Char LerAlternativaQuiz(Dictionary<char, string> opcoes)
	{
		while (true)
		{
			Console.Write("Sua resposta: ");
			String? entrada = Console.ReadLine();

			if (!String.IsNullOrWhiteSpace(entrada))
			{
				Char alternativa = Char.ToUpperInvariant(entrada.Trim()[0]);

				if (opcoes.TryGetValue(alternativa, out String? _))
				{
					return alternativa;
				}
			}

			Console.WriteLine("Opcao invalida. Digite uma alternativa existente.");
		}
	}

	private static Int32 LerOpcaoNoIntervalo(String mensagem, Int32 minimo, Int32 maximo)
	{
		while (true)
		{
			Console.Write(mensagem);
			String? entrada = Console.ReadLine();

			if (Int32.TryParse(entrada, out Int32 opcao) && opcao >= minimo && opcao <= maximo)
			{
				return opcao;
			}

			Console.WriteLine($"Entrada invalida. Informe um numero entre {minimo} e {maximo}.");
		}
	}

	private static Int32 LerInteiro(String mensagem)
	{
		while (true)
		{
			Console.Write(mensagem);
			String? entrada = Console.ReadLine();

			if (Int32.TryParse(entrada, out Int32 valorInteiro))
			{
				return valorInteiro;
			}

			Console.WriteLine("Valor invalido. Digite um numero inteiro.");
		}
	}

	private static Decimal LerDecimal(String mensagem)
	{
		CultureInfo culturaPtBr = CultureInfo.GetCultureInfo("pt-BR");

		while (true)
		{
			Console.Write(mensagem);
			String? entrada = Console.ReadLine();

			if (Decimal.TryParse(
				entrada,
				NumberStyles.Number,
				culturaPtBr,
				out Decimal valorDecimal))
			{
				return valorDecimal;
			}

			if (Decimal.TryParse(
				entrada,
				NumberStyles.Number,
				CultureInfo.InvariantCulture,
				out valorDecimal))
			{
				return valorDecimal;
			}

			Console.WriteLine("Valor invalido. Exemplos validos: 19,90 ou 19.90");
		}
	}

	private static String LerTextoObrigatorio(String mensagem)
	{
		while (true)
		{
			Console.Write(mensagem);
			String? entrada = Console.ReadLine();

			if (!String.IsNullOrWhiteSpace(entrada))
			{
				return entrada.Trim();
			}

			Console.WriteLine("O campo nao pode ficar vazio.");
		}
	}

	private static String LerTextoOpcional(String mensagem)
	{
		Console.Write(mensagem);
		String? entrada = Console.ReadLine();
		return entrada?.Trim() ?? String.Empty;
	}

	private static List<PerguntaQuiz> CriarPerguntasQuiz()
	{
		return new List<PerguntaQuiz>
		{
			new PerguntaQuiz(
				"O que acontece ao chamar Add para uma chave que ja existe?",
				new Dictionary<char, string>
				{
					{ 'A', "O valor antigo e sobrescrito sem erro" },
					{ 'B', "E lancada ArgumentException" },
					{ 'C', "A chave antiga e removida" },
					{ 'D', "Nada acontece" }
				},
				'B',
				"Add exige chave unica. Se a chave ja existir, ocorre ArgumentException."),

			new PerguntaQuiz(
				"Qual diferenca principal entre usar [chave] e TryGetValue?",
				new Dictionary<char, string>
				{
					{ 'A', "Nao existe diferenca" },
					{ 'B', "TryGetValue sempre substitui o valor" },
					{ 'C', "[chave] pode lancar excecao quando chave nao existe; TryGetValue retorna false" },
					{ 'D', "TryGetValue so funciona com int" }
				},
				'C',
				"TryGetValue evita excecoes por chave ausente e fornece fluxo condicional seguro."),

			new PerguntaQuiz(
				"Qual abordagem evita tentar remover uma chave inexistente sem diagnostico?",
				new Dictionary<char, string>
				{
					{ 'A', "ContainsKey seguido de Remove" },
					{ 'B', "Apenas usar foreach" },
					{ 'C', "Converter para List" },
					{ 'D', "Sempre usar indexador" }
				},
				'A',
				"ContainsKey ajuda a explicar para o usuario por que a remocao falhou antes de chamar Remove."),

			new PerguntaQuiz(
				"Qual tecnica e comum para contar frequencia de palavras com Dictionary?",
				new Dictionary<char, string>
				{
					{ 'A', "Sempre limpar o dicionario no loop" },
					{ 'B', "TryGetValue + incremento" },
					{ 'C', "Usar apenas Remove e Add" },
					{ 'D', "Criar um Dictionary novo para cada palavra" }
				},
				'B',
				"O padrao TryGetValue + incremento e simples, performatico e seguro para agregacoes."),

			new PerguntaQuiz(
				"Como obter os 3 maiores valores de pontuacao de forma clara?",
				new Dictionary<char, string>
				{
					{ 'A', "OrderByDescending + Take(3)" },
					{ 'B', "Sort sem criterio" },
					{ 'C', "Remove de todos os itens" },
					{ 'D', "ContainsKey em loop infinito" }
				},
				'A',
				"OrderByDescending organiza do maior para o menor e Take(3) recorta apenas os tres primeiros.")
		};
	}
}
