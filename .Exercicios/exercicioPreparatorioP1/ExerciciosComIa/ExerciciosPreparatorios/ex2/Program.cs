// "Crie um sistema para gerenciar uma academia. O sistema deve cadastrar alunos
// e instrutores, controlar planos de treino, registrar pagamentos, emitir alertas
// quando um aluno falta muito, salvar relatórios em arquivo e buscar dados com LINQ.
// Use herança, interfaces, encapsulamento, composição, delegates, async/await e
// tratamento de exceções."

// =============================================================================
// ① ENCAPSULAMENTO
// Campos privados protegem os dados. Propriedades com get/set controlam o acesso.
// O "set" pode conter regras de validação — ninguém de fora burla a regra.
// =============================================================================
public class Pessoa
{
    private string _nome;
    private string _cpf;
    private int _idade;

    public string Nome
    {
        get => _nome;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Nome não pode ser vazio.");
            _nome = value.Trim();
        }
    }

    public string CPF
    {
        get => _cpf;
        set
        {
            // Validação simples: CPF deve ter 11 dígitos numéricos
            string soDigitos = new string(value.Where(char.IsDigit).ToArray()); // LINQ inline
            if (soDigitos.Length != 11)
                throw new ArgumentException("CPF deve ter 11 dígitos.");
            _cpf = soDigitos;
        }
    }

    public int Idade
    {
        get => _idade;
        set
        {
            if (value < 14 || value > 100)
                throw new ArgumentOutOfRangeException("Idade deve estar entre 14 e 100 anos.");
            _idade = value;
        }
    }

    public Pessoa(string nome, string cpf, int idade)
    {
        Nome = nome;
        CPF = cpf;
        Idade = idade;
    }

    // Método virtual — comportamento base, pode ser substituído nas filhas
    public virtual string Resumo()
    {
        return $"{Nome} | CPF: {_cpf} | {Idade} anos";
    }
}

// =============================================================================
// ② HERANÇA
// Instrutor e Aluno são tipos de Pessoa. Herdam Nome, CPF, Idade.
// Cada um adiciona suas características específicas e sobrescreve Resumo().
// "base()" chama o construtor da classe pai para não repetir inicialização.
// =============================================================================
public class Instrutor : Pessoa
{
    public string Especialidade { get; set; }
    public string CREF { get; set; }  // registro profissional

    public Instrutor(string nome, string cpf, int idade, string especialidade, string cref)
        : base(nome, cpf, idade)  // delega para Pessoa(nome, cpf, idade)
    {
        Especialidade = especialidade;
        CREF = cref;
    }

    // override: substitui o Resumo() de Pessoa para mostrar dados do Instrutor
    public override string Resumo()
    {
        return $"[INSTRUTOR] {Nome} | CREF: {CREF} | Esp: {Especialidade}";
    }
}

public class Aluno : Pessoa
{
    public string Matricula { get; private set; }
    public DateTime DataMatricula { get; private set; }
    private int _faltas = 0;

    // Propriedade calculada — não armazena valor, calcula na hora do get
    public bool EmRiscoDeEvasao => _faltas >= 5;

    public Aluno(string nome, string cpf, int idade, string matricula)
        : base(nome, cpf, idade)
    {
        Matricula = matricula;
        DataMatricula = DateTime.Now;
    }

    public void RegistrarFalta() => _faltas++;
    public void ZerarFaltas() => _faltas = 0;
    public int TotalFaltas => _faltas;

    public override string Resumo()
    {
        string alerta = EmRiscoDeEvasao ? " ⚠ RISCO DE EVASÃO" : "";
        return $"[ALUNO] {Nome} | Mat: {Matricula} | Faltas: {_faltas}{alerta}";
    }
}

// =============================================================================
// ③ INTERFACES
// ICobravel: contrato para qualquer coisa que possa ser cobrada
// IAvaliavel: contrato para qualquer coisa que possa ser avaliada
// Classes que implementam a interface DEVEM ter todos os métodos listados.
// Permite tratar objetos diferentes de forma uniforme (polimorfismo).
// =============================================================================
public interface ICobravel
{
    decimal ValorMensal { get; }
    bool PagamentoEmDia { get; }
    void RegistrarPagamento(decimal valor);
}

public interface IAvaliavel
{
    // Retorna um texto de avaliação do objeto
    string Avaliar();
}

// =============================================================================
// ④ COMPOSIÇÃO
// PlanoTreino TEM UM Instrutor responsável.
// PlanoTreino é composto de uma lista de Exercicios.
// Sem o PlanoTreino, os Exercicios não fazem sentido — é composição forte.
// =============================================================================
public class Exercicio
{
    public string Nome { get; set; }
    public int Series { get; set; }
    public int Repeticoes { get; set; }
    public string GrupoMuscular { get; set; }

    public Exercicio(string nome, int series, int repeticoes, string grupoMuscular)
    {
        Nome = nome;
        Series = series;
        Repeticoes = repeticoes;
        GrupoMuscular = grupoMuscular;
    }

    public override string ToString()
        => $"{Nome} — {Series}x{Repeticoes} ({GrupoMuscular})";
}

public class PlanoTreino : IAvaliavel
{
    public string Objetivo { get; set; }

    // COMPOSIÇÃO: o Instrutor é parte fundamental do PlanoTreino
    public Instrutor InstrutorResponsavel { get; private set; }

    // COMPOSIÇÃO: lista de exercícios pertence ao plano
    private List<Exercicio> _exercicios = new List<Exercicio>();

    public IReadOnlyList<Exercicio> Exercicios => _exercicios.AsReadOnly();

    public PlanoTreino(string objetivo, Instrutor instrutor)
    {
        Objetivo = objetivo;
        InstrutorResponsavel = instrutor;  // composição: recebe e guarda o instrutor
    }

    public void AdicionarExercicio(Exercicio ex) => _exercicios.Add(ex);

    // Implementação da interface IAvaliavel
    public string Avaliar()
    {
        int total = _exercicios.Count;
        // LINQ: agrupa por grupo muscular e conta
        var grupos = _exercicios
            .GroupBy(e => e.GrupoMuscular)
            .Select(g => $"{g.Key}({g.Count()})")
            .ToList();

        return $"Plano '{Objetivo}' | {total} exercícios | Grupos: {string.Join(", ", grupos)} " +
               $"| Instrutor: {InstrutorResponsavel.Nome}";
    }
}

// =============================================================================
// ④ ASSOCIAÇÃO
// Matricula ASSOCIA um Aluno a um PlanoTreino.
// O Aluno e o PlanoTreino existem independentemente — não são criados aqui.
// A Matricula apenas registra essa relação e o plano de pagamento.
// =============================================================================
public class Matricula : ICobravel, IAvaliavel  // implementa duas interfaces
{
    // ASSOCIAÇÃO: Matricula CONHECE Aluno e PlanoTreino, não os cria
    public Aluno AlunoMatriculado { get; }
    public PlanoTreino Plano { get; }
    public DateTime DataInicio { get; }
    private decimal _valorPago = 0;
    private DateTime _ultimoPagamento;

    // Implementação da interface ICobravel
    public decimal ValorMensal { get; }
    public bool PagamentoEmDia => _ultimoPagamento.Month == DateTime.Now.Month
                                  && _ultimoPagamento.Year == DateTime.Now.Year;

    // ⑤ EVENTO — disparado quando pagamento é registrado
    public event Action<Matricula> OnPagamentoRealizado;

    // ⑤ EVENTO — disparado quando aluno atinge limite de faltas
    public event Action<Aluno> OnAlunoEmRisco;

    public Matricula(Aluno aluno, PlanoTreino plano, decimal valorMensal)
    {
        AlunoMatriculado = aluno;
        Plano = plano;
        ValorMensal = valorMensal;
        DataInicio = DateTime.Now;
    }

    // Implementação de ICobravel
    public void RegistrarPagamento(decimal valor)
    {
        // ⑥ EXCEÇÃO: valor inválido
        if (valor <= 0)
            throw new ArgumentException("Valor do pagamento deve ser positivo.");
        if (valor < ValorMensal)
            throw new InvalidOperationException(
                $"Valor insuficiente. Esperado: R${ValorMensal:F2}, Recebido: R${valor:F2}");

        _valorPago += valor;
        _ultimoPagamento = DateTime.Now;

        // ⑤ DISPARO DO EVENTO — notifica quem estiver ouvindo
        OnPagamentoRealizado?.Invoke(this);
    }

    public void RegistrarFaltaAluno()
    {
        AlunoMatriculado.RegistrarFalta();

        // Dispara evento de risco se aluno atingiu o limite
        if (AlunoMatriculado.EmRiscoDeEvasao)
            OnAlunoEmRisco?.Invoke(AlunoMatriculado);
    }

    // Implementação de IAvaliavel
    public string Avaliar()
    {
        string statusPag = PagamentoEmDia ? "✔ Pago" : "✘ Pendente";
        return $"{AlunoMatriculado.Nome} | {Plano.Objetivo} | R${ValorMensal:F2}/mês | {statusPag}";
    }
}

// =============================================================================
// ⑤ DELEGATES E EVENTOS — Serviço de Alertas
// Este serviço "ouve" os eventos disparados pela Matricula.
// Não precisa saber nada sobre como o evento é disparado — só reage a ele.
// Isso é desacoplamento: Matricula não depende do ServicoAlertas.
// =============================================================================
public class ServicoAlertas
{
    private List<string> _historico = new List<string>();

    // Assinado no evento OnPagamentoRealizado
    public void AoPagarMensalidade(Matricula matricula)
    {
        string msg = $"[PAGAMENTO] {DateTime.Now:HH:mm} — {matricula.AlunoMatriculado.Nome} " +
                     $"pagou R${matricula.ValorMensal:F2}";
        _historico.Add(msg);
        Console.WriteLine($"✅ {msg}");
    }

    // Assinado no evento OnAlunoEmRisco
    public void AoAlunoEntrarEmRisco(Aluno aluno)
    {
        string msg = $"[ALERTA] {DateTime.Now:HH:mm} — {aluno.Nome} tem {aluno.TotalFaltas} faltas! " +
                     $"Contatar para retenção.";
        _historico.Add(msg);
        Console.WriteLine($"⚠ {msg}");
    }

    // Podemos assinar múltiplos eventos com lambda também — ex: log simples
    public Action<Matricula> LogSimples =>
        m => Console.WriteLine($"[LOG] Pagamento confirmado para matrícula de {m.AlunoMatriculado.Nome}");

    public IReadOnlyList<string> Historico => _historico.AsReadOnly();
}

// =============================================================================
// CLASSE PRINCIPAL — ACADEMIA
// Gerencia coleções e concentra as operações do sistema.
// =============================================================================
public class Academia
{
    // ⑦ LIST<T> — coleção genérica tipada, dinâmica e integrada com LINQ
    private List<Aluno> _alunos = new List<Aluno>();
    private List<Instrutor> _instrutores = new List<Instrutor>();
    private List<Matricula> _matriculas = new List<Matricula>();

    // Adições simples
    public void CadastrarAluno(Aluno a) => _alunos.Add(a);
    public void CadastrarInstrutor(Instrutor i) => _instrutores.Add(i);

    public Matricula CriarMatricula(Aluno aluno, PlanoTreino plano, decimal valor,
                                    ServicoAlertas alertas)
    {
        var matricula = new Matricula(aluno, plano, valor);

        // ⑤ ASSINATURA DE EVENTOS — conecta o evento ao método do ServicoAlertas
        matricula.OnPagamentoRealizado += alertas.AoPagarMensalidade;
        matricula.OnPagamentoRealizado += alertas.LogSimples;   // segundo ouvinte no mesmo evento
        matricula.OnAlunoEmRisco += alertas.AoAlunoEntrarEmRisco;

        _matriculas.Add(matricula);
        Console.WriteLine($"📋 Matrícula criada: {aluno.Nome} → {plano.Objetivo}");
        return matricula;
    }

    // =============================================================================
    // ⑧ LINQ — Consultas variadas sobre as coleções
    // =============================================================================

    // Alunos com pagamento em atraso
    public List<Matricula> MatriculasInadimplentes()
        => _matriculas
            .Where(m => !m.PagamentoEmDia)          // filtro
            .OrderBy(m => m.AlunoMatriculado.Nome)  // ordenação
            .ToList();

    // Alunos em risco de evasão por faltas
    public List<Aluno> AlunosEmRisco()
        => _alunos
            .Where(a => a.EmRiscoDeEvasao)
            .ToList();

    // Receita mensal total (só de quem pagou este mês)
    public decimal ReceitaMensal()
        => _matriculas
            .Where(m => m.PagamentoEmDia)
            .Sum(m => m.ValorMensal);               // agregação

    // Distribuição de alunos por objetivo do plano
    public Dictionary<string, int> AlunosPorObjetivo()
        => _matriculas
            .GroupBy(m => m.Plano.Objetivo)         // agrupa por objetivo
            .ToDictionary(g => g.Key, g => g.Count());

    // Busca aluno por nome (parcial, sem diferenciar maiúsculas)
    public List<Aluno> BuscarAluno(string termo)
        => _alunos
            .Where(a => a.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase))
            .ToList();

    // Instrutor com mais planos atribuídos
    public Instrutor InstrutorMaisAtivo()
        => _matriculas
            .GroupBy(m => m.Plano.InstrutorResponsavel)
            .OrderByDescending(g => g.Count())      // mais planos primeiro
            .Select(g => g.Key)
            .FirstOrDefault();                       // pega o primeiro (mais ativo)

    public void ExibirRelatorio()
    {
        Console.WriteLine("\n📊 RELATÓRIO GERAL:");
        Console.WriteLine($"  Alunos: {_alunos.Count} | Instrutores: {_instrutores.Count}");
        Console.WriteLine($"  Matrículas ativas: {_matriculas.Count}");
        Console.WriteLine($"  Receita do mês: R$ {ReceitaMensal():F2}");
        Console.WriteLine($"  Inadimplentes: {MatriculasInadimplentes().Count}");
        Console.WriteLine($"  Alunos em risco (faltas): {AlunosEmRisco().Count}");

        // IAvaliavel: polimorfismo — chamamos Avaliar() em tipos diferentes
        Console.WriteLine("\n📋 STATUS DAS MATRÍCULAS (via interface IAvaliavel):");
        foreach (var m in _matriculas)
            Console.WriteLine($"  {m.Avaliar()}");
    }

    // =============================================================================
    // ⑨ ASYNC / AWAIT
    // Operações de IO (arquivo, banco, rede) não devem travar a aplicação.
    // "async Task" indica que o método pode usar "await" internamente.
    // Chamador usa "await" para esperar sem bloquear a thread principal.
    // =============================================================================
    public async Task ExportarRelatorioAsync(string caminho)
    {
        Console.WriteLine("\n💾 Exportando relatório...");

        // Montamos o conteúdo usando LINQ para transformar coleções em linhas de texto
        var linhas = new List<string>
        {
            "========================================",
            "     RELATÓRIO DA ACADEMIA",
            $"     Gerado: {DateTime.Now:dd/MM/yyyy HH:mm}",
            "========================================",
            "",
            "--- ALUNOS ---"
        };

        // LINQ: projeta cada aluno em uma linha de texto
        linhas.AddRange(_alunos.Select(a => $"  {a.Resumo()}"));

        linhas.Add("");
        linhas.Add("--- INSTRUTORES ---");
        linhas.AddRange(_instrutores.Select(i => $"  {i.Resumo()}"));

        linhas.Add("");
        linhas.Add("--- MATRÍCULAS ---");
        linhas.AddRange(_matriculas.Select(m => $"  {m.Avaliar()}"));

        linhas.Add("");
        linhas.Add($"Receita do mês: R$ {ReceitaMensal():F2}");

        // =============================================================================
        // ⑩ MANIPULAÇÃO DE ARQUIVO
        // File.WriteAllLinesAsync: escreve uma coleção de strings no arquivo,
        // uma por linha. A versão Async libera a thread enquanto o SO escreve em disco.
        // =============================================================================
        await File.WriteAllLinesAsync(caminho, linhas);
        Console.WriteLine($"✔ Relatório exportado: '{caminho}'");
    }

    public async Task<string[]> ImportarLogAsync(string caminho)
    {
        // ⑥ EXCEÇÃO: arquivo pode não existir — tratamos antes de tentar ler
        if (!File.Exists(caminho))
            throw new FileNotFoundException($"Arquivo de log não encontrado: '{caminho}'");

        // await: aguarda leitura assíncrona do arquivo sem travar a thread
        string[] linhas = await File.ReadAllLinesAsync(caminho);
        return linhas;
    }
}

// =============================================================================
// ⑥ TRATAMENTO DE EXCEÇÕES
// Exceção customizada: quando o negócio tem uma regra muito específica,
// criar uma Exception própria torna o código mais expressivo e fácil de depurar.
// =============================================================================
public class AcademiaException : Exception
{
    public string Codigo { get; }

    public AcademiaException(string mensagem, string codigo) : base(mensagem)
    {
        Codigo = codigo;
    }
}

public static class ExecutorSeguro
{
    // Sobrecarga 1: ação síncrona
    public static void Executar(Action acao, string contexto)
    {
        try
        {
            acao();
        }
        catch (AcademiaException ex)
        {
            // Exceção de domínio customizada — regras de negócio da academia
            Console.WriteLine($"[REGRA NEGÓCIO][{ex.Codigo}] {contexto}: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"[OP. INVÁLIDA] {contexto}: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"[ARGUMENTO] {contexto}: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERRO] {contexto}: {ex.Message}");
        }
        finally
        {
            // finally executa SEMPRE — ideal para liberar recursos, fechar conexões, etc.
            Console.WriteLine($"[FIM] '{contexto}'\n");
        }
    }

    // Sobrecarga 2: ação assíncrona (aceita Task)
    public static async Task ExecutarAsync(Func<Task> acao, string contexto)
    {
        try
        {
            await acao();
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"[ARQUIVO] {contexto}: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERRO ASYNC] {contexto}: {ex.Message}");
        }
        finally
        {
            Console.WriteLine($"[FIM ASYNC] '{contexto}'\n");
        }
    }
}

// =============================================================================
// PROGRAMA PRINCIPAL
// =============================================================================
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("==============================================");
        Console.WriteLine("   SISTEMA DE ACADEMIA — Exercício POO #2    ");
        Console.WriteLine("==============================================\n");

        // ──────────────────────────────────────────────────────────────
        // ① + ② Encapsulamento e Herança
        // ──────────────────────────────────────────────────────────────
        Console.WriteLine("--- CADASTROS ---");
        var instrutor1 = new Instrutor("Ricardo Ferreira", "12345678901", 35, "Musculação", "CREF-001");
        var instrutor2 = new Instrutor("Patrícia Lima", "98765432100", 29, "Funcional", "CREF-002");

        var aluno1 = new Aluno("João Mendes", "11122233344", 22, "MAT-001");
        var aluno2 = new Aluno("Fernanda Costa", "55566677788", 30, "MAT-002");
        var aluno3 = new Aluno("Bruno Oliveira", "99988877711", 25, "MAT-003");

        // override de Resumo() — cada tipo imprime de forma diferente (polimorfismo)
        Console.WriteLine(instrutor1.Resumo());
        Console.WriteLine(aluno1.Resumo());

        // ──────────────────────────────────────────────────────────────
        // ④ Composição: Plano TEM Instrutor TEM Exercicios
        // ──────────────────────────────────────────────────────────────
        Console.WriteLine("\n--- PLANOS DE TREINO ---");
        var planoMusc = new PlanoTreino("Hipertrofia", instrutor1);
        planoMusc.AdicionarExercicio(new Exercicio("Supino Reto", 4, 12, "Peito"));
        planoMusc.AdicionarExercicio(new Exercicio("Agachamento", 4, 10, "Pernas"));
        planoMusc.AdicionarExercicio(new Exercicio("Puxada Alta", 3, 12, "Costas"));

        var planoFunc = new PlanoTreino("Condicionamento", instrutor2);
        planoFunc.AdicionarExercicio(new Exercicio("Burpee", 3, 15, "Corpo Inteiro"));
        planoFunc.AdicionarExercicio(new Exercicio("Kettlebell Swing", 4, 20, "Glúteo/Core"));

        // IAvaliavel: polimorfismo — PlanoTreino.Avaliar() gera relatório
        Console.WriteLine(planoMusc.Avaliar());
        Console.WriteLine(planoFunc.Avaliar());

        // ──────────────────────────────────────────────────────────────
        // ⑤ Eventos + ④ Associação + ⑦ List<T>
        // ──────────────────────────────────────────────────────────────
        Console.WriteLine("\n--- MATRÍCULAS E EVENTOS ---");
        var academia = new Academia();
        var alertas = new ServicoAlertas();

        academia.CadastrarAluno(aluno1);
        academia.CadastrarAluno(aluno2);
        academia.CadastrarAluno(aluno3);
        academia.CadastrarInstrutor(instrutor1);
        academia.CadastrarInstrutor(instrutor2);

        // CriarMatricula também assina os eventos — ao chamar Registrar* os alertas disparam
        var mat1 = academia.CriarMatricula(aluno1, planoMusc, 120.00m, alertas);
        var mat2 = academia.CriarMatricula(aluno2, planoFunc, 99.90m, alertas);
        var mat3 = academia.CriarMatricula(aluno3, planoMusc, 120.00m, alertas);

        // ──────────────────────────────────────────────────────────────
        // ⑥ Exceções — pagamentos válidos e inválidos
        // ──────────────────────────────────────────────────────────────
        Console.WriteLine("\n--- PAGAMENTOS ---");
        ExecutorSeguro.Executar(
            () => mat1.RegistrarPagamento(120.00m),
            "Pagamento João"
        );
        ExecutorSeguro.Executar(
            () => mat2.RegistrarPagamento(50.00m),  // valor insuficiente → exceção
            "Pagamento Fernanda (valor baixo)"
        );
        ExecutorSeguro.Executar(
            () => mat2.RegistrarPagamento(-10m),    // negativo → exceção
            "Pagamento negativo"
        );
        // Fernanda pagou depois (corretamente)
        ExecutorSeguro.Executar(
            () => mat2.RegistrarPagamento(99.90m),
            "Pagamento Fernanda (correto)"
        );

        // ──────────────────────────────────────────────────────────────
        // ⑤ Evento OnAlunoEmRisco: disparado ao acumular 5 faltas
        // ──────────────────────────────────────────────────────────────
        Console.WriteLine("--- FALTAS (Bruno acumulará 5 → evento de risco) ---");
        for (int i = 1; i <= 5; i++)
        {
            ExecutorSeguro.Executar(
                () => mat3.RegistrarFaltaAluno(),
                $"Falta {i} do Bruno"
            );
        }

        // ──────────────────────────────────────────────────────────────
        // ⑧ LINQ — consultas diversas
        // ──────────────────────────────────────────────────────────────
        Console.WriteLine("--- CONSULTAS LINQ ---");

        var inadimplentes = academia.MatriculasInadimplentes();
        Console.WriteLine($"Inadimplentes: {inadimplentes.Count}");
        inadimplentes.ForEach(m => Console.WriteLine($"  - {m.AlunoMatriculado.Nome}"));

        var emRisco = academia.AlunosEmRisco();
        Console.WriteLine($"\nAlunos em risco de evasão: {emRisco.Count}");
        emRisco.ForEach(a => Console.WriteLine($"  - {a.Nome} ({a.TotalFaltas} faltas)"));

        var distribuicao = academia.AlunosPorObjetivo();
        Console.WriteLine("\nAlunos por objetivo:");
        foreach (var (objetivo, count) in distribuicao)
            Console.WriteLine($"  {objetivo}: {count} aluno(s)");

        var maisAtivo = academia.InstrutorMaisAtivo();
        Console.WriteLine($"\nInstrutor mais ativo: {maisAtivo?.Nome ?? "nenhum"}");

        var busca = academia.BuscarAluno("costa");
        Console.WriteLine($"\nBusca 'costa': {busca.Count} resultado(s)");
        busca.ForEach(a => Console.WriteLine($"  - {a.Nome}"));

        // ──────────────────────────────────────────────────────────────
        // ⑨ Async/Await + ⑩ Arquivo
        // ──────────────────────────────────────────────────────────────
        academia.ExibirRelatorio();

        await ExecutorSeguro.ExecutarAsync(
            async () => await academia.ExportarRelatorioAsync("relatorio_academia.txt"),
            "Exportar relatório"
        );

        // Reimporta o arquivo e exibe as primeiras linhas
        await ExecutorSeguro.ExecutarAsync(
            async () =>
            {
                var linhas = await academia.ImportarLogAsync("relatorio_academia.txt");
                Console.WriteLine($"\n📄 Arquivo lido — {linhas.Length} linhas. Prévia:");
                // LINQ: Take(6) pega só as primeiras 6 linhas
                foreach (var l in linhas.Take(6))
                    Console.WriteLine($"   {l}");
            },
            "Reimportar relatório"
        );

        // Testa leitura de arquivo inexistente → FileNotFoundException
        await ExecutorSeguro.ExecutarAsync(
            async () =>
            {
                var _ = await academia.ImportarLogAsync("nao_existe.txt");
            },
            "Importar arquivo inexistente"
        );

        // Histórico de alertas (guardado pelo ServicoAlertas)
        Console.WriteLine("--- HISTÓRICO DE ALERTAS ---");
        alertas.Historico.ToList().ForEach(h => Console.WriteLine($"  {h}"));

        Console.WriteLine("\n✅ Exercício #2 concluído!");

/*
==============================================
CONCEITOS USADOS NESTE EXERCÍCIO:
==============================================
① Encapsulamento  → Pessoa: _nome, _cpf, _idade com validação nos setters
② Herança         → Instrutor e Aluno herdam de Pessoa, override Resumo()
③ Interface       → ICobravel (Matricula) e IAvaliavel (PlanoTreino, Matricula)
④ Composição      → PlanoTreino TEM Instrutor e List<Exercicio>
   Associação     → Matricula CONHECE Aluno e PlanoTreino
⑤ Delegate/Evento → OnPagamentoRealizado e OnAlunoEmRisco em Matricula
⑥ Exceções        → AcademiaException customizada + ExecutorSeguro (sync/async)
⑦ List<T>         → _alunos, _instrutores, _matriculas na Academia
⑧ LINQ            → Where, OrderBy, GroupBy, Sum, Select, FirstOrDefault, Take
⑨ Async/Await     → ExportarRelatorioAsync e ImportarLogAsync com Task
⑩ Arquivo (IO)    → WriteAllLinesAsync / ReadAllLinesAsync
==============================================");
*/    
    }
}