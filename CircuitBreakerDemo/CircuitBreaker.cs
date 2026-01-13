using System;

namespace CircuitBreakerDemo;

/// <summary>
/// Circuit Breaker simples e didático
/// </summary>
public class CircuitBreaker
{
    // Configurações
    private readonly int _limiteErros;        // Quantos erros até abrir
    private readonly TimeSpan _tempoEspera;   // Tempo que fica aberto
    
    // Estado interno
    private int _contadorErros = 0;
    private DateTime? _ultimaFalha = null;
    private Estado _estadoAtual = Estado.Fechado;
    
    // Estados possíveis
    public enum Estado
    {
        Fechado,      // ✅ Normal - permite chamadas
        Aberto,       // 🚫 Bloqueado - rejeita chamadas
        MeioAberto    // ⚠️  Testando - permite 1 chamada
    }
    
    public CircuitBreaker(int limiteErros = 3, int segundosEspera = 60)
    {
        _limiteErros = limiteErros;
        _tempoEspera = TimeSpan.FromSeconds(segundosEspera);
    }
    
    /// <summary>
    /// Executa uma operação protegida pelo Circuit Breaker
    /// </summary>
    public T Executar<T>(Func<T> operacao, string nome = "Operação")
    {
        // 1️⃣ Verifica se pode executar
        VerificarEstado();
        
        if (_estadoAtual == Estado.Aberto)
        {
            Console.WriteLine($"🚫 [{nome}] BLOQUEADO - Circuito está ABERTO");
            throw new Exception("Circuit Breaker está ABERTO");
        }
        
        try
        {
            // 2️⃣ Executa a operação
            Console.WriteLine($"⚡ [{nome}] Executando... (Estado: {_estadoAtual})");
            T resultado = operacao();
            
            // 3️⃣ Sucesso! Registra e fecha circuito se estava meio-aberto
            RegistrarSucesso();
            Console.WriteLine($"✅ [{nome}] SUCESSO");
            return resultado;
        }
        catch (Exception)
        {
            // 4️⃣ Falhou! Registra o erro
            RegistrarFalha();
            Console.WriteLine($"❌ [{nome}] FALHOU");
            throw;
        }
    }
    
    /// <summary>
    /// Verifica se deve mudar de estado (Aberto → Meio-Aberto)
    /// </summary>
    private void VerificarEstado()
    {
        if (_estadoAtual == Estado.Aberto && _ultimaFalha.HasValue)
        {
            // Tempo decorrido desde a última falha
            TimeSpan tempoDecorrido = DateTime.UtcNow - _ultimaFalha.Value;
            
            // Se passou o tempo de espera, tenta recuperar
            if (tempoDecorrido >= _tempoEspera)
            {
                Console.WriteLine($"⚠️  Mudando para MEIO-ABERTO (aguardou {tempoDecorrido.TotalSeconds:F1}s)");
                _estadoAtual = Estado.MeioAberto;
            }
        }
    }
    
    /// <summary>
    /// Registra uma falha e abre o circuito se atingir o limite
    /// </summary>
    private void RegistrarFalha()
    {
        _contadorErros++;
        _ultimaFalha = DateTime.UtcNow;
        
        Console.WriteLine($"📊 Erros: {_contadorErros}/{_limiteErros}");
        
        // Se atingiu o limite de erros, ABRE o circuito
        if (_contadorErros >= _limiteErros && _estadoAtual != Estado.Aberto)
        {
            _estadoAtual = Estado.Aberto;
            Console.WriteLine($"🔴 Circuito ABERTO! (Limite de {_limiteErros} erros atingido)");
        }
    }
    
    /// <summary>
    /// Registra um sucesso e fecha o circuito se estava meio-aberto
    /// </summary>
    private void RegistrarSucesso()
    {
        if (_estadoAtual == Estado.MeioAberto)
        {
            // Recuperou! Volta ao normal
            Console.WriteLine("🟢 Circuito FECHADO! (Recuperado)");
            _estadoAtual = Estado.Fechado;
            _contadorErros = 0;
        }
    }
    
    /// <summary>
    /// Obtém o estado atual
    /// </summary>
    public Estado EstadoAtual => _estadoAtual;
}
