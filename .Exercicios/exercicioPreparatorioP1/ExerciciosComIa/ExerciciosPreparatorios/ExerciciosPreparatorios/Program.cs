// "Crie um sistema de gerenciamento de uma biblioteca. O sistema deve cadastrar
// livros e autores, calcular multas por atraso, notificar eventos de devolução,
// salvar/carregar dados em arquivo, e permitir buscas com LINQ. Use herança,
// interfaces, encapsulamento, composição, delegates e async/await."

// =============================================================================
// ① ORIENTAÇÃO A OBJETOS — Encapsulamento
// O que é: proteger os dados internos de uma classe usando modificadores de acesso.
// Por que usar: evita que valores inválidos sejam atribuídos diretamente.
// Como usar: campos privados + propriedades públicas com get/set.
// =============================================================================
public class Pessoa
{
    // Campo privado — só a classe acessa diretamente
    private string _nome;
    private int _idade;

    // Propriedade pública — o mundo externo usa isso
    public string Nome
    {
        get => _nome;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Nome não pode ser vazio."); // ⑥ Exceção validando entrada
            _nome = value;
        }
    }

    public int Idade
    {
        get => _idade;
        set
        {
            if (value < 0) throw new ArgumentOutOfRangeException("Idade não pode ser negativa.");
            _idade = value;
        }
    }

    // Construtor — inicializa o objeto com valores obrigatórios
    public Pessoa(string nome, int idade)
    {
        Nome = nome;   // passa pelo set, que valida
        Idade = idade;
    }

    // Método virtual — pode ser sobrescrito por classes filhas (base da herança)
    public virtual string Apresentar()
    {
        return $"Pessoa: {Nome}, {Idade} anos";
    }
}

// =============================================================================
// ② HERANÇA
// O que é: uma classe filha herda atributos e métodos da classe pai.
// Por que usar: evita repetição de código (reaproveitamento).
// Como usar: class Filho : Pai — e usa "override" para redefinir comportamentos.
// =============================================================================
public class Autor : Pessoa
{
    public string Nacionalidade { get; set; }

    public Autor(string nome, int idade, string nacionalidade) : base(nome, idade)
    // "base(...)" chama o construtor da classe pai (Pessoa)
    {
        Nacionalidade = nacionalidade;
    }

    // override — substitui o comportamento da classe pai para Autor
    public override string Apresentar()
    {
        return $"Autor: {Nome} ({Nacionalidade}), {Idade} anos";
    }
}

public class MembroBiblioteca : Pessoa
{
    public string Matricula { get; private set; }

    public MembroBiblioteca(string nome, int idade, string matricula) : base(nome, idade)
    {
        Matricula = matricula;
    }

    public override string Apresentar()
    {
        return $"Membro: {Nome} | Matrícula: {Matricula}";
    }
}

// =============================================================================
// ③ INTERFACE
// O que é: um contrato que obriga classes a implementar determinados métodos.
// Por que usar: garante que objetos diferentes possam ser usados da mesma forma.
// Como usar: interface IAlgo { ... } — e a classe faz "class X : IAlgo".
// =============================================================================
public interface IEmprestavel
{
    // Qualquer coisa emprestável DEVE ter estes métodos
    void Emprestar(MembroBiblioteca membro);
    void Devolver();
    bool EstaDisponivel();
}

public interface INotificavel
{
    // Qualquer coisa notificável deve ter este método
    string GerarMensagem();
}

// =============================================================================
// ④ COMPOSIÇÃO
// O que é: uma classe "tem" outra classe como parte dela (relação HAS-A).
// Por que usar: modela relações onde um objeto é composto de outros.
// Como usar: instanciar um objeto de outra classe dentro da classe atual.
// Diferença de Herança: herança é IS-A (Autor É UMA Pessoa),
//                       composição é HAS-A (Livro TEM UM Autor).
// =============================================================================
public class Livro : IEmprestavel, INotificavel  // implementa duas interfaces
{
    public string Titulo { get; set; }
    public string ISBN { get; set; }

    // COMPOSIÇÃO: Livro TEM UM Autor — o Autor faz parte do Livro
    public Autor AutorDoLivro { get; set; }

    private bool _disponivel = true;
    private MembroBiblioteca _membroAtual;
    private DateTime _dataEmprestimo;

    // ⑤ DELEGATE/EVENTO — declarado aqui, disparado quando o livro é devolvido
    public event Action<Livro, MembroBiblioteca> OnDevolvido;

    public Livro(string titulo, string isbn, Autor autor)
    {
        Titulo = titulo;
        ISBN = isbn;
        AutorDoLivro = autor;  // composição: recebemos o Autor e guardamos
    }

    // =============================================================================
    // IMPLEMENTAÇÃO DA INTERFACE IEmprestavel
    // =============================================================================
    public void Emprestar(MembroBiblioteca membro)
    {
        // ⑥ TRATAMENTO DE EXCEÇÃO — lançamos exceção se regra de negócio falhar
        if (!_disponivel)
            throw new InvalidOperationException($"Livro '{Titulo}' já está emprestado.");

        _disponivel = false;
        _membroAtual = membro;
        _dataEmprestimo = DateTime.Now;
        Console.WriteLine($"✔ '{Titulo}' emprestado para {membro.Nome} em {_dataEmprestimo:dd/MM/yyyy}");
    }

    public void Devolver()
    {
        if (_disponivel)
            throw new InvalidOperationException($"Livro '{Titulo}' não estava emprestado.");

        _disponivel = true;
        Console.WriteLine($"✔ '{Titulo}' devolvido por {_membroAtual.Nome}");

        // ⑤ DISPARO DE EVENTO — avisa todos os "ouvintes" que o livro foi devolvido
        // O "?" verifica se há alguém inscrito antes de disparar (evita NullRef)
        OnDevolvido?.Invoke(this, _membroAtual);

        _membroAtual = null;
    }

    public bool EstaDisponivel() => _disponivel;

    // Implementação da interface INotificavel
    public string GerarMensagem()
    {
        return _disponivel
            ? $"📗 '{Titulo}' está disponível para empréstimo."
            : $"📕 '{Titulo}' está emprestado para {_membroAtual?.Nome}.";
    }

    // Calcula multa com base em dias de atraso
    public decimal CalcularMulta(int diasAtraso)
    {
        const decimal multaPorDia = 1.50m;
        return diasAtraso > 0 ? diasAtraso * multaPorDia : 0;
    }
}

// =============================================================================
// ⑤ DELEGATES E EVENTOS
// O que é: delegates são "ponteiros para métodos" — permitem passar métodos como
//          parâmetros. Eventos usam delegates para notificar quando algo acontece.
// Por que usar: desacoplamento — quem dispara o evento não sabe quem vai reagir.
// Como usar: declara com "event Action<...>", assina com "+=", dispara com "Invoke".
// =============================================================================
public class SistemaNotificacao
{
    // Este método será ASSINADO (subscrito) no evento OnDevolvido do Livro
    public void AoReceberDevolucao(Livro livro, MembroBiblioteca membro)
    {
        Console.WriteLine($"[NOTIFICAÇÃO] O livro '{livro.Titulo}' foi devolvido por {membro.Nome}. " +
                          $"Agora está disponível!");
    }
}

// =============================================================================
// ④ ASSOCIAÇÃO
// O que é: relação mais fraca que composição. Um objeto USA outro, mas sem
//          "possuí-lo". O objeto associado pode existir independentemente.
// Diferença: na Composição, o Autor foi criado PARA o Livro e faz parte dele.
//            Na Associação, o Membro usa o Livro, mas ambos existem separadamente.
// =============================================================================
public class Emprestimo
{
    // ASSOCIAÇÃO: Emprestimo CONHECE um Livro e um Membro, mas não os cria nem os destrói
    public Livro LivroAssociado { get; }
    public MembroBiblioteca MembroAssociado { get; }
    public DateTime DataPrevistaDevolucao { get; }

    public Emprestimo(Livro livro, MembroBiblioteca membro, int diasPrazo)
    {
        LivroAssociado = livro;
        MembroAssociado = membro;
        DataPrevistaDevolucao = DateTime.Now.AddDays(diasPrazo);
    }
}

// =============================================================================
// ⑦ ARRAY, ARRAYLIST E LIST<T>
// - Array: tamanho fixo, tipado       → int[] nums = new int[5];
// - ArrayList: tamanho dinâmico, não tipado (evitar — é legado do .NET 1.x)
// - List<T>: tamanho dinâmico, TIPADO → preferir sempre!
// Por que List<T>: segurança de tipo em tempo de compilação, performance, LINQ.
// =============================================================================
public class Biblioteca
{
    // List<T> — coleção genérica tipada, preferida em C# moderno
    private List<Livro> _acervo = new List<Livro>();
    private List<MembroBiblioteca> _membros = new List<MembroBiblioteca>();
    private List<Emprestimo> _emprestimos = new List<Emprestimo>();

    public void AdicionarLivro(Livro livro) => _acervo.Add(livro);
    public void AdicionarMembro(MembroBiblioteca membro) => _membros.Add(membro);

    public void RegistrarEmprestimo(Livro livro, MembroBiblioteca membro, int diasPrazo)
    {
        livro.Emprestar(membro);
        _emprestimos.Add(new Emprestimo(livro, membro, diasPrazo));
    }

    // =============================================================================
    // ⑧ LINQ (Language Integrated Query)
    // O que é: sintaxe de consulta integrada ao C# para filtrar, ordenar e
    //          transformar coleções.
    // Por que usar: código mais limpo e expressivo que loops manuais.
    // Como usar: .Where(), .Select(), .OrderBy(), .First(), .Any(), etc.
    // =============================================================================
    public List<Livro> BuscarLivrosDisponiveis()
    {
        // LINQ — filtra apenas livros disponíveis e ordena por título
        return _acervo
            .Where(livro => livro.EstaDisponivel())      // filtro
            .OrderBy(livro => livro.Titulo)              // ordenação
            .ToList();                                   // converte para List<T>
    }

    public List<Livro> BuscarPorAutor(string nomeAutor)
    {
        // LINQ com condição em propriedade de objeto composto
        return _acervo
            .Where(l => l.AutorDoLivro.Nome.Contains(nomeAutor, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public decimal TotalMultasMembro(MembroBiblioteca membro, int diasAtraso)
    {
        // LINQ com agregação: soma multas de todos os livros emprestados ao membro
        return _emprestimos
            .Where(e => e.MembroAssociado == membro)
            .Sum(e => e.LivroAssociado.CalcularMulta(diasAtraso));
    }

    public void ExibirRelatorioAcervo()
    {
        Console.WriteLine("\n📚 RELATÓRIO DO ACERVO:");
        // LINQ com Select para projetar uma string de exibição
        var linhas = _acervo.Select(l => $"  [{(l.EstaDisponivel() ? "✔" : "✘")}] {l.Titulo} — {l.AutorDoLivro.Nome}");
        foreach (var linha in linhas)
            Console.WriteLine(linha);
    }

    // Propriedade que usa LINQ para contar disponíveis
    public int TotalDisponiveis => _acervo.Count(l => l.EstaDisponivel());

    // Acesso ao acervo completo (somente leitura via IReadOnlyList)
    public IReadOnlyList<Livro> Acervo => _acervo.AsReadOnly();

    // =============================================================================
    // ⑨ ASYNC / AWAIT — Programação Assíncrona
    // O que é: permite executar operações demoradas (IO, rede) sem travar a thread.
    // Por que usar: performance — a thread fica livre para outras tarefas enquanto
    //               espera. Essencial para operações de arquivo, banco, API.
    // Como usar: método retorna Task ou Task<T>, usa "await" antes da operação lenta.
    // =============================================================================
    public async Task SalvarDadosAsync(string caminhoArquivo)
    {
        Console.WriteLine("\n💾 Salvando dados em arquivo...");

        // Construímos o conteúdo a salvar
        var linhas = new List<string>();
        linhas.Add("=== ACERVO DA BIBLIOTECA ===");
        linhas.Add($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}");
        linhas.Add("");

        foreach (var livro in _acervo)
        {
            // Usamos o método da interface INotificavel para gerar a linha
            linhas.Add(livro.GerarMensagem());
            linhas.Add($"   ISBN: {livro.ISBN} | Autor: {livro.AutorDoLivro.Apresentar()}");
        }

        // =============================================================================
        // ⑩ MANIPULAÇÃO DE ARQUIVOS
        // O que é: leitura e escrita de arquivos no sistema de arquivos.
        // Por que usar: persistência de dados entre execuções do programa.
        // Como usar: File.WriteAllLinesAsync, File.ReadAllLinesAsync, StreamWriter, etc.
        // ASYNC: usamos a versão assíncrona para não travar enquanto escreve.
        // =============================================================================
        await File.WriteAllLinesAsync(caminhoArquivo, linhas);
        Console.WriteLine($"✔ Dados salvos em '{caminhoArquivo}'");
    }

    public async Task<List<string>> CarregarDadosAsync(string caminhoArquivo)
    {
        // ⑥ EXCEÇÃO — verificamos se o arquivo existe antes de tentar ler
        if (!File.Exists(caminhoArquivo))
            throw new FileNotFoundException($"Arquivo '{caminhoArquivo}' não encontrado.");

        // await — espera a leitura terminar sem travar a thread
        var linhas = await File.ReadAllLinesAsync(caminhoArquivo);
        return linhas.ToList(); // LINQ: ToList() converte array para List<T>
    }
}

// =============================================================================
// ⑥ TRATAMENTO DE EXCEÇÕES — Revisão centralizada
// O que é: mecanismo para lidar com erros em tempo de execução.
// Por que usar: evita que o programa "quebre" e permite dar mensagens úteis.
// Como usar: try { código arriscado } catch (TipoException e) { trate } finally { sempre executa }
// =============================================================================
public static class GerenciadorDeExcecoes
{
    public static void ExecutarComSeguranca(Action acao, string contexto = "")
    {
        try
        {
            acao(); // tenta executar
        }
        catch (InvalidOperationException ex)
        {
            // Exceção específica de operação inválida (ex: emprestar livro já emprestado)
            Console.WriteLine($"[ERRO DE OPERAÇÃO] {contexto}: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            // Exceção de argumento inválido (ex: nome vazio)
            Console.WriteLine($"[ERRO DE ARGUMENTO] {contexto}: {ex.Message}");
        }
        catch (FileNotFoundException ex)
        {
            // Exceção de arquivo não encontrado
            Console.WriteLine($"[ERRO DE ARQUIVO] {contexto}: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Captura qualquer outra exceção não prevista
            Console.WriteLine($"[ERRO INESPERADO] {contexto}: {ex.Message}");
        }
        finally
        {
            // finally SEMPRE executa, com erro ou sem
            Console.WriteLine($"[LOG] Operação '{contexto}' finalizada.\n");
        }
    }
}

// =============================================================================
// PROGRAMA PRINCIPAL — Une todos os conceitos em um fluxo coerente
// =============================================================================
class Program
{
    // "async Task Main" — ponto de entrada assíncrono (C# 7.1+)
    static async Task Main(string[] args)
    {
        Console.WriteLine("======================================================");
        Console.WriteLine("   SISTEMA DE BIBLIOTECA — Demonstração POO Completa  ");
        Console.WriteLine("======================================================\n");

        // --- ① + ② Criando objetos com Herança e Encapsulamento ---
        var autor1 = new Autor("Machado de Assis", 69, "Brasileiro");
        var autor2 = new Autor("J.K. Rowling", 58, "Britânica");

        Console.WriteLine(autor1.Apresentar()); // override do método virtual
        Console.WriteLine(autor2.Apresentar());

        var membro1 = new MembroBiblioteca("Ana Silva", 25, "MAT-001");
        var membro2 = new MembroBiblioteca("Carlos Souza", 30, "MAT-002");

        // --- ④ Composição: Livro TEM UM Autor ---
        var livro1 = new Livro("Dom Casmurro", "978-85-01", autor1);
        var livro2 = new Livro("Harry Potter", "978-85-02", autor2);
        var livro3 = new Livro("Memórias Póstumas", "978-85-03", autor1);

        // --- ⑤ Delegate/Evento: inscrevendo método no evento de devolução ---
        var notificacao = new SistemaNotificacao();
        // "+=" assina o evento — quando livro for devolvido, AoReceberDevolucao será chamado
        livro1.OnDevolvido += notificacao.AoReceberDevolucao;
        livro2.OnDevolvido += notificacao.AoReceberDevolucao;
        // Podemos inscrever MÚLTIPLOS métodos no mesmo evento:
        livro1.OnDevolvido += (l, m) => Console.WriteLine($"[LAMBDA] Log extra: '{l.Titulo}' disponível novamente!");

        // --- ⑦ List<T>: adicionando à biblioteca ---
        var biblioteca = new Biblioteca();
        biblioteca.AdicionarLivro(livro1);
        biblioteca.AdicionarLivro(livro2);
        biblioteca.AdicionarLivro(livro3);
        biblioteca.AdicionarMembro(membro1);
        biblioteca.AdicionarMembro(membro2);

        // --- Empréstimos com tratamento de exceção ---
        Console.WriteLine("\n--- EMPRÉSTIMOS ---");
        GerenciadorDeExcecoes.ExecutarComSeguranca(
            () => biblioteca.RegistrarEmprestimo(livro1, membro1, 7),
            "Emprestar Dom Casmurro"
        );
        GerenciadorDeExcecoes.ExecutarComSeguranca(
            () => biblioteca.RegistrarEmprestimo(livro2, membro2, 14),
            "Emprestar Harry Potter"
        );

        // Tentativa de emprestar livro já emprestado — vai lançar e capturar exceção
        GerenciadorDeExcecoes.ExecutarComSeguranca(
            () => biblioteca.RegistrarEmprestimo(livro1, membro2, 7),
            "Tentar emprestar Dom Casmurro (já emprestado)"
        );

        // --- ⑧ LINQ: consultas ---
        Console.WriteLine("--- CONSULTAS LINQ ---");
        var disponiveis = biblioteca.BuscarLivrosDisponiveis();
        Console.WriteLine($"Livros disponíveis ({disponiveis.Count}):");
        disponiveis.ForEach(l => Console.WriteLine($"  - {l.Titulo}"));

        var livrosMachado = biblioteca.BuscarPorAutor("Machado");
        Console.WriteLine($"\nLivros de Machado ({livrosMachado.Count}):");
        livrosMachado.ForEach(l => Console.WriteLine($"  - {l.Titulo} [{(l.EstaDisponivel() ? "disponível" : "emprestado")}]"));

        // --- Relatório usando interface INotificavel ---
        biblioteca.ExibirRelatorioAcervo();

        // --- Devoluções — dispara os eventos ---
        Console.WriteLine("\n--- DEVOLUÇÕES ---");
        GerenciadorDeExcecoes.ExecutarComSeguranca(
            () => livro1.Devolver(),
            "Devolução Dom Casmurro"
        );

        // --- Multa ---
        decimal multa = biblioteca.TotalMultasMembro(membro2, diasAtraso: 5);
        Console.WriteLine($"\n💰 Multa de {membro2.Nome} por 5 dias de atraso: R$ {multa:F2}");

        // --- ⑨ + ⑩ Async + Manipulação de Arquivo ---
        Console.WriteLine("\n--- ARQUIVO ASSÍNCRONO ---");
        string arquivo = "biblioteca_dados.txt";

        await biblioteca.SalvarDadosAsync(arquivo);

        // Lendo de volta o arquivo
        GerenciadorDeExcecoes.ExecutarComSeguranca(
            async () =>
            {
                var conteudo = await biblioteca.CarregarDadosAsync(arquivo);
                Console.WriteLine($"\n📄 Arquivo lido ({conteudo.Count} linhas):");
                conteudo.Take(5).ToList().ForEach(l => Console.WriteLine($"   {l}"));
            },
            "Leitura do arquivo"
        );

        // Tentando ler arquivo que não existe — exceção esperada
        GerenciadorDeExcecoes.ExecutarComSeguranca(
            async () =>
            {
                var _ = await biblioteca.CarregarDadosAsync("nao_existe.txt");
            },
            "Leitura de arquivo inexistente"
        );
    }
}

        // --- RESUMO DOS CONCEITOS USADOS ---
/*
=======================================================
RESUMO DOS CONCEITOS DEMONSTRADOS:
=======================================================
① Encapsulamento  → campos privados + propriedades em Pessoa
② Herança         → Autor e MembroBiblioteca herdam de Pessoa
③ Interface       → IEmprestavel e INotificavel implementadas em Livro
④ Composição      → Livro TEM UM Autor
   Associação     → Emprestimo CONHECE Livro e Membro
⑤ Delegate/Evento → OnDevolvido disparado ao devolver livro
⑥ Exceções        → try/catch/finally em GerenciadorDeExcecoes
⑦ List<T>         → acervo, membros e emprestimos na Biblioteca
⑧ LINQ            → Where, OrderBy, Select, Sum, Count, ToList
⑨ Async/Await     → SalvarDadosAsync e CarregarDadosAsync
⑩ Arquivo (IO)    → File.WriteAllLinesAsync / ReadAllLinesAsync
=======================================================");
*/