// =============================================================================
// EXERCÍCIO INTEGRADO #3 — SISTEMA DE CLÍNICA MÉDICA
// =============================================================================
// ENUNCIADO (exemplo de prova):
// "Crie um sistema para gerenciar uma clínica médica. O sistema deve cadastrar
// médicos e pacientes, agendar consultas, controlar prontuários, calcular valores
// de convênio, notificar cancelamentos, salvar histórico em arquivo e realizar
// buscas com LINQ. Use todos os pilares de POO, interfaces, delegates, eventos,
// coleções genéricas, async/await e tratamento de exceções."
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

// =============================================================================
// ① ENCAPSULAMENTO
// Cada classe protege seus dados com modificadores de acesso.
// Setters com validação garantem que nenhum estado inválido entre no objeto.
// =============================================================================
public class Pessoa
{
    private string _nome;
    private string _telefone;
    private DateTime _dataNascimento;

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

    public string Telefone
    {
        get => _telefone;
        set
        {
            // Remove tudo que não for dígito e valida comprimento mínimo
            string digits = new string(value.Where(char.IsDigit).ToArray());
            if (digits.Length < 10)
                throw new ArgumentException("Telefone inválido. Mínimo 10 dígitos.");
            _telefone = digits;
        }
    }

    // Propriedade calculada — a partir de DataNascimento calcula a idade
    public int Idade => DateTime.Now.Year - _dataNascimento.Year
                        - (DateTime.Now.DayOfYear < _dataNascimento.DayOfYear ? 1 : 0);

    public DateTime DataNascimento
    {
        get => _dataNascimento;
        set
        {
            if (value > DateTime.Now)
                throw new ArgumentException("Data de nascimento não pode ser no futuro.");
            if (value < DateTime.Now.AddYears(-130))
                throw new ArgumentException("Data de nascimento inválida.");
            _dataNascimento = value;
        }
    }

    public Pessoa(string nome, string telefone, DateTime dataNascimento)
    {
        Nome = nome;
        Telefone = telefone;
        DataNascimento = dataNascimento;
    }

    public virtual string Resumo() => $"{Nome} | Tel: {_telefone} | {Idade} anos";
}

// =============================================================================
// ② HERANÇA
// Medico e Paciente herdam tudo de Pessoa e adicionam seus atributos.
// "sealed" em Paciente impede que outras classes herdem dela — decisão de design.
// =============================================================================
public class Medico : Pessoa
{
    public string CRM { get; private set; }
    public string Especialidade { get; set; }

    // Propriedade que controla agenda disponível (encapsulada como lista privada)
    private List<DateTime> _horariosDisponiveis = new List<DateTime>();
    public IReadOnlyList<DateTime> HorariosDisponiveis => _horariosDisponiveis.AsReadOnly();

    public Medico(string nome, string telefone, DateTime nascimento, string crm, string especialidade)
        : base(nome, telefone, nascimento) // base() delega para Pessoa
    {
        CRM = crm;
        Especialidade = especialidade;
    }

    public void AdicionarHorario(DateTime horario)
    {
        if (horario <= DateTime.Now)
            throw new ArgumentException("Horário deve ser no futuro.");
        if (_horariosDisponiveis.Contains(horario))
            throw new InvalidOperationException("Horário já cadastrado.");
        _horariosDisponiveis.Add(horario);
    }

    public void RemoverHorario(DateTime horario) => _horariosDisponiveis.Remove(horario);

    // override: substitui o Resumo() de Pessoa com dados específicos do Médico
    public override string Resumo()
        => $"[MÉDICO] Dr(a). {Nome} | CRM: {CRM} | {Especialidade}";
}

public sealed class Paciente : Pessoa   // sealed = ninguém pode herdar de Paciente
{
    public string NumeroCarteirinha { get; private set; }
    public string Convenio { get; set; }

    // Prontuário: List<string> de anotações — encapsulado, só adiciona via método
    private List<string> _prontuario = new List<string>();
    public IReadOnlyList<string> Prontuario => _prontuario.AsReadOnly();

    public Paciente(string nome, string telefone, DateTime nascimento,
                    string carteirinha, string convenio)
        : base(nome, telefone, nascimento)
    {
        NumeroCarteirinha = carteirinha;
        Convenio = convenio;
    }

    public void AdicionarAnotacao(string anotacao)
    {
        if (string.IsNullOrWhiteSpace(anotacao))
            throw new ArgumentException("Anotação não pode ser vazia.");
        _prontuario.Add($"[{DateTime.Now:dd/MM/yyyy HH:mm}] {anotacao}");
    }

    public override string Resumo()
        => $"[PACIENTE] {Nome} | Carteirinha: {NumeroCarteirinha} | Convênio: {Convenio}";
}

// =============================================================================
// ③ INTERFACES
// IAgendavel: qualquer coisa que pode ser agendada deve saber confirmar/cancelar.
// IRelatorio: qualquer coisa que pode gerar relatório deve ter GerarRelatório().
// ICalculavelFinanceiro: contratos de cálculo de valores — desacopla a lógica.
// =============================================================================
public interface IAgendavel
{
    bool EstaConfirmado { get; }
    void Confirmar();
    void Cancelar(string motivo);
}

public interface IRelatorio
{
    string GerarRelatório();
}

public interface ICalculavelFinanceiro
{
    decimal ValorBase { get; }
    decimal CalcularDesconto();
    decimal ValorFinal();
}

// =============================================================================
// ④ COMPOSIÇÃO
// Consulta É composta por um Médico, um Paciente e um Prontuário específico.
// Esses objetos são ESSENCIAIS para a Consulta existir.
// Sem médico ou paciente, não existe consulta — é composição forte.
// =============================================================================
public enum StatusConsulta { Agendada, Confirmada, Cancelada, Realizada }
public enum TipoConvenio { Particular, Basic, Premium, Total }

public class Consulta : IAgendavel, IRelatorio, ICalculavelFinanceiro
{
    // Identificador único gerado automaticamente
    public Guid Id { get; } = Guid.NewGuid();

    // COMPOSIÇÃO: Consulta TEM UM Médico e UM Paciente — são partes dela
    public Medico MedicoResponsavel { get; }
    public Paciente PacienteAtendido { get; }
    public DateTime DataHora { get; private set; }
    public string Especialidade { get; }
    private StatusConsulta _status = StatusConsulta.Agendada;
    public StatusConsulta Status => _status;
    private string _motivoCancelamento;

    // ⑤ EVENTOS — notificações de mudança de estado da consulta
    public event Action<Consulta> OnConfirmada;
    public event Action<Consulta, string> OnCancelada;
    public event Action<Consulta, string> OnRealizada;

    public Consulta(Medico medico, Paciente paciente, DateTime dataHora, string especialidade)
    {
        MedicoResponsavel = medico;
        PacienteAtendido = paciente;
        Especialidade = especialidade;
        Remarcar(dataHora); // já valida data via método
    }

    public void Remarcar(DateTime novaData)
    {
        if (novaData <= DateTime.Now)
            throw new ArgumentException("Data da consulta deve ser no futuro.");
        if (_status == StatusConsulta.Cancelada)
            throw new InvalidOperationException("Não é possível remarcar consulta cancelada.");
        DataHora = novaData;
    }

    // ── Implementação de IAgendavel ──────────────────────────────────────────
    public bool EstaConfirmado => _status == StatusConsulta.Confirmada;

    public void Confirmar()
    {
        if (_status != StatusConsulta.Agendada)
            throw new InvalidOperationException($"Consulta não pode ser confirmada no status '{_status}'.");
        _status = StatusConsulta.Confirmada;
        OnConfirmada?.Invoke(this); // dispara evento de confirmação
    }

    public void Cancelar(string motivo)
    {
        if (_status == StatusConsulta.Realizada)
            throw new InvalidOperationException("Consulta já realizada não pode ser cancelada.");
        if (_status == StatusConsulta.Cancelada)
            throw new InvalidOperationException("Consulta já está cancelada.");
        _status = StatusConsulta.Cancelada;
        _motivoCancelamento = motivo;
        OnCancelada?.Invoke(this, motivo); // dispara evento de cancelamento
    }

    public void Realizar(string anotacaoMedica)
    {
        if (_status != StatusConsulta.Confirmada)
            throw new InvalidOperationException("Só é possível realizar consulta confirmada.");
        _status = StatusConsulta.Realizada;
        // Adiciona anotação diretamente no prontuário do paciente (composição em ação)
        PacienteAtendido.AdicionarAnotacao($"Dr(a). {MedicoResponsavel.Nome}: {anotacaoMedica}");
        OnRealizada?.Invoke(this, anotacaoMedica);
    }

    // ── Implementação de ICalculavelFinanceiro ───────────────────────────────
    public decimal ValorBase { get; } = 250.00m;

    public decimal CalcularDesconto()
    {
        // Desconto depende do convênio do paciente — switch expression (C# 8+)
        return PacienteAtendido.Convenio switch
        {
            "Particular" => 0m,
            "Basic" => ValorBase * 0.10m,  // 10%
            "Premium" => ValorBase * 0.30m,  // 30%
            "Total" => ValorBase * 0.50m,  // 50%
            _ => 0m
        };
    }

    public decimal ValorFinal() => ValorBase - CalcularDesconto();

    // ── Implementação de IRelatorio ──────────────────────────────────────────
    public string GerarRelatório()
    {
        string cancelInfo = _status == StatusConsulta.Cancelada
            ? $" | Motivo: {_motivoCancelamento}" : "";
        return $"[{_status}] {DataHora:dd/MM/yyyy HH:mm} | " +
               $"Dr(a). {MedicoResponsavel.Nome} ({Especialidade}) | " +
               $"Paciente: {PacienteAtendido.Nome} | " +
               $"R$ {ValorFinal():F2}{cancelInfo}";
    }
}

// =============================================================================
// ④ ASSOCIAÇÃO
// HistoricoConsultas ASSOCIA um Paciente às suas consultas.
// O Paciente e as Consultas existem independentemente — são apenas referenciados.
// =============================================================================
public class HistoricoConsultas : IRelatorio
{
    // ASSOCIAÇÃO: referencia o Paciente sem "possuí-lo"
    public Paciente PacienteRef { get; }
    private List<Consulta> _consultas = new List<Consulta>();

    public HistoricoConsultas(Paciente paciente) => PacienteRef = paciente;

    public void Adicionar(Consulta c) => _consultas.Add(c);

    // ⑧ LINQ no histórico
    public int TotalRealizadas => _consultas.Count(c => c.Status == StatusConsulta.Realizada);
    public decimal GastoTotal => _consultas
        .Where(c => c.Status == StatusConsulta.Realizada)
        .Sum(c => c.ValorFinal());

    // IRelatorio
    public string GerarRelatório()
    {
        var linhas = _consultas
            .OrderByDescending(c => c.DataHora) // mais recentes primeiro
            .Select(c => $"  • {c.GerarRelatório()}");
        return $"Histórico de {PacienteRef.Nome}:\n" + string.Join("\n", linhas);
    }
}

// =============================================================================
// ⑤ DELEGATES E EVENTOS — Central de Notificações
// Recebe e reage aos eventos disparados pelas Consultas.
// Completamente desacoplada — não importa de onde o evento veio.
// =============================================================================
public class CentralNotificacoes
{
    private List<string> _log = new List<string>();
    public IReadOnlyList<string> Log => _log.AsReadOnly();

    // Método assinado em OnConfirmada
    public void AoConfirmar(Consulta c)
    {
        string msg = $"[CONFIRMAÇÃO] {c.DataHora:dd/MM/yyyy HH:mm} | " +
                     $"{c.PacienteAtendido.Nome} com Dr(a). {c.MedicoResponsavel.Nome}";
        _log.Add(msg);
        Console.WriteLine($"📅 {msg}");
    }

    // Método assinado em OnCancelada
    public void AoCancelar(Consulta c, string motivo)
    {
        string msg = $"[CANCELAMENTO] {c.PacienteAtendido.Nome} | Motivo: {motivo}";
        _log.Add(msg);
        Console.WriteLine($"❌ {msg}");
    }

    // Método assinado em OnRealizada
    public void AoRealizar(Consulta c, string anotacao)
    {
        string msg = $"[REALIZADA] Dr(a). {c.MedicoResponsavel.Nome} " +
                     $"atendeu {c.PacienteAtendido.Nome}";
        _log.Add(msg);
        Console.WriteLine($"✅ {msg}");
    }
}

// =============================================================================
// CLÍNICA — orquestra tudo, contém as coleções e os métodos de negócio
// =============================================================================
public class Clinica
{
    private string _nome;

    // ⑦ LIST<T> — coleções tipadas e dinâmicas
    private List<Medico> _medicos = new List<Medico>();
    private List<Paciente> _pacientes = new List<Paciente>();
    private List<Consulta> _consultas = new List<Consulta>();
    private List<HistoricoConsultas> _historicos = new List<HistoricoConsultas>();

    public Clinica(string nome) => _nome = nome;

    public void CadastrarMedico(Medico m) => _medicos.Add(m);
    public void CadastrarPaciente(Paciente p)
    {
        _pacientes.Add(p);
        _historicos.Add(new HistoricoConsultas(p)); // cria histórico junto
    }

    public Consulta AgendarConsulta(Medico medico, Paciente paciente,
                                    DateTime dataHora, CentralNotificacoes central)
    {
        // ⑥ Exceção de negócio: médico deve estar cadastrado
        if (!_medicos.Contains(medico))
            throw new ClinicaException("Médico não cadastrado na clínica.", "MED-001");
        if (!_pacientes.Contains(paciente))
            throw new ClinicaException("Paciente não cadastrado na clínica.", "PAC-001");

        var consulta = new Consulta(medico, paciente, dataHora, medico.Especialidade);

        // ⑤ Assina os eventos da nova consulta
        consulta.OnConfirmada += central.AoConfirmar;
        consulta.OnCancelada += central.AoCancelar;
        consulta.OnRealizada += central.AoRealizar;
        // Lambda como segundo ouvinte em OnRealizada — log compacto
        consulta.OnRealizada += (c, _) =>
            Console.WriteLine($"[LOG] Prontuário de {c.PacienteAtendido.Nome} atualizado.");

        _consultas.Add(consulta);
        // Adiciona ao histórico do paciente
        _historicos.First(h => h.PacienteRef == paciente).Adicionar(consulta);

        Console.WriteLine($"📋 Consulta agendada: {paciente.Nome} → Dr(a). {medico.Nome} " +
                          $"em {dataHora:dd/MM/yyyy HH:mm}");
        return consulta;
    }

    // =============================================================================
    // ⑧ LINQ — consultas variadas
    // =============================================================================

    // Todas as consultas de uma especialidade
    public List<Consulta> PorEspecialidade(string especialidade)
        => _consultas
            .Where(c => c.Especialidade.Equals(especialidade, StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.DataHora)
            .ToList();

    // Consultas de hoje
    public List<Consulta> ConsultasHoje()
        => _consultas
            .Where(c => c.DataHora.Date == DateTime.Today
                        && c.Status != StatusConsulta.Cancelada)
            .OrderBy(c => c.DataHora)
            .ToList();

    // Receita total do período (só realizadas)
    public decimal ReceitaPeriodo(DateTime inicio, DateTime fim)
        => _consultas
            .Where(c => c.Status == StatusConsulta.Realizada
                        && c.DataHora >= inicio && c.DataHora <= fim)
            .Sum(c => c.ValorFinal());

    // Médico com mais consultas realizadas
    public Medico MedicoDestaque()
        => _consultas
            .Where(c => c.Status == StatusConsulta.Realizada)
            .GroupBy(c => c.MedicoResponsavel)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

    // Pacientes sem consulta nos últimos 6 meses (inativos)
    public List<Paciente> PacientesInativos()
    {
        var limite = DateTime.Now.AddMonths(-6);
        // Pacientes cujas consultas mais recentes são antigas OU não têm nenhuma
        return _pacientes
            .Where(p => !_consultas
                .Any(c => c.PacienteAtendido == p && c.DataHora >= limite))
            .ToList();
    }

    // Taxa de cancelamento por médico
    public Dictionary<string, double> TaxaCancelamentoPorMedico()
        => _consultas
            .GroupBy(c => c.MedicoResponsavel.Nome)
            .ToDictionary(
                g => g.Key,
                g => g.Count(c => c.Status == StatusConsulta.Cancelada) * 100.0 / g.Count()
            );

    // Consultas agrupadas por status
    public Dictionary<StatusConsulta, int> ConsultasPorStatus()
        => _consultas
            .GroupBy(c => c.Status)
            .ToDictionary(g => g.Key, g => g.Count());

    public HistoricoConsultas ObterHistorico(Paciente p)
        => _historicos.FirstOrDefault(h => h.PacienteRef == p);

    public void ExibirPainel()
    {
        Console.WriteLine($"\n🏥 PAINEL — {_nome}");
        Console.WriteLine($"  Médicos: {_medicos.Count} | Pacientes: {_pacientes.Count}");
        Console.WriteLine($"  Total de consultas: {_consultas.Count}");

        var porStatus = ConsultasPorStatus();
        foreach (var (status, qty) in porStatus)
            Console.WriteLine($"    {status}: {qty}");

        var destaque = MedicoDestaque();
        if (destaque != null)
            Console.WriteLine($"  Médico destaque: Dr(a). {destaque.Nome}");

        Console.WriteLine($"  Pacientes inativos: {PacientesInativos().Count}");
    }

    // =============================================================================
    // ⑨ ASYNC / AWAIT + ⑩ MANIPULAÇÃO DE ARQUIVO
    // Salva e carrega dados assincronamente para não travar a aplicação.
    // StreamWriter é uma alternativa ao File.WriteAllLines, permite escrita linha a linha.
    // =============================================================================
    public async Task SalvarLogAsync(string caminho, IReadOnlyList<string> linhas)
    {
        Console.WriteLine("\n💾 Salvando log de notificações...");

        // StreamWriter: usado quando queremos escrever linha a linha de forma eficiente
        // "using await" garante que o arquivo é fechado mesmo se der exceção
        await using var writer = new StreamWriter(caminho, append: false);
        await writer.WriteLineAsync($"=== LOG — {_nome} ===");
        await writer.WriteLineAsync($"Gerado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
        await writer.WriteLineAsync(new string('-', 40));

        foreach (var linha in linhas)
            await writer.WriteLineAsync(linha);

        Console.WriteLine($"✔ Log salvo: '{caminho}' ({linhas.Count} entradas)");
    }

    public async Task SalvarRelatoriosAsync(string caminho)
    {
        Console.WriteLine("💾 Salvando relatórios de histórico...");

        var conteudo = new List<string>();
        conteudo.Add($"=== HISTÓRICOS DE PACIENTES — {_nome} ===");
        conteudo.Add($"Gerado: {DateTime.Now:dd/MM/yyyy HH:mm}");
        conteudo.Add("");

        // IRelatorio: polimorfismo — GerarRelatório() funciona em qualquer IRelatorio
        foreach (var h in _historicos)
        {
            conteudo.Add(h.GerarRelatório());
            conteudo.Add($"  Total realizadas: {h.TotalRealizadas} | Gasto: R$ {h.GastoTotal:F2}");
            conteudo.Add("");
        }

        // File.WriteAllLinesAsync: mais simples quando já temos a coleção pronta
        await File.WriteAllLinesAsync(caminho, conteudo);
        Console.WriteLine($"✔ Relatórios salvos: '{caminho}'");
    }

    public async Task<List<string>> LerArquivoAsync(string caminho)
    {
        if (!File.Exists(caminho))
            throw new FileNotFoundException($"Arquivo não encontrado: '{caminho}'");

        // File.ReadAllLinesAsync: lê todas as linhas de forma assíncrona
        var linhas = await File.ReadAllLinesAsync(caminho);
        return linhas.ToList();
    }
}

// =============================================================================
// ⑥ EXCEÇÃO CUSTOMIZADA
// ClinicaException carrega um código de erro além da mensagem.
// Permite que o catch diferencie erros de domínio dos erros do sistema.
// =============================================================================
public class ClinicaException : Exception
{
    public string Codigo { get; }
    public ClinicaException(string mensagem, string codigo) : base(mensagem)
        => Codigo = codigo;
}

// try/catch centralizado: evita repetição de blocos try/catch espalhados no código
public static class Safe
{
    public static void Run(Action acao, string ctx)
    {
        try { acao(); }
        catch (ClinicaException ex) { Console.WriteLine($"[CLÍNICA][{ex.Codigo}] {ctx}: {ex.Message}"); }
        catch (InvalidOperationException ex) { Console.WriteLine($"[OP. INVÁLIDA] {ctx}: {ex.Message}"); }
        catch (ArgumentException ex) { Console.WriteLine($"[ARGUMENTO] {ctx}: {ex.Message}"); }
        catch (Exception ex) { Console.WriteLine($"[ERRO] {ctx}: {ex.Message}"); }
        finally { Console.WriteLine($"[FIM] {ctx}\n"); }
    }

    public static async Task RunAsync(Func<Task> acao, string ctx)
    {
        try { await acao(); }
        catch (FileNotFoundException ex) { Console.WriteLine($"[ARQUIVO] {ctx}: {ex.Message}"); }
        catch (Exception ex) { Console.WriteLine($"[ERRO ASYNC] {ctx}: {ex.Message}"); }
        finally { Console.WriteLine($"[FIM ASYNC] {ctx}\n"); }
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
        Console.WriteLine("   SISTEMA DE CLÍNICA — Exercício POO #3    ");
        Console.WriteLine("==============================================\n");

        // ──────────────────────────────────────────────────────────────────────
        // ① + ② Encapsulamento e Herança
        // ──────────────────────────────────────────────────────────────────────
        Console.WriteLine("--- CADASTROS ---");
        var medico1 = new Medico("Ana Beatriz", "11999990001",
            new DateTime(1980, 5, 10), "CRM-12345", "Cardiologia");
        var medico2 = new Medico("Carlos Eduardo", "11999990002",
            new DateTime(1975, 8, 22), "CRM-67890", "Ortopedia");

        var paciente1 = new Paciente("Marcos Ribeiro", "11988880001",
            new DateTime(1990, 3, 15), "CART-001", "Premium");
        var paciente2 = new Paciente("Lúcia Fernandes", "11988880002",
            new DateTime(1965, 11, 30), "CART-002", "Total");
        var paciente3 = new Paciente("Rafael Souza", "11988880003",
            new DateTime(2000, 7, 4), "CART-003", "Particular");

        // override de Resumo() — polimorfismo em ação
        Console.WriteLine(medico1.Resumo());
        Console.WriteLine(paciente1.Resumo());

        // ──────────────────────────────────────────────────────────────────────
        // ⑤ Eventos + ④ Composição/Associação + ⑦ List<T>
        // ──────────────────────────────────────────────────────────────────────
        Console.WriteLine("\n--- CONFIGURANDO CLÍNICA ---");
        var clinica = new Clinica("Clínica Vida Plena");
        var central = new CentralNotificacoes();

        clinica.CadastrarMedico(medico1);
        clinica.CadastrarMedico(medico2);
        clinica.CadastrarPaciente(paciente1);
        clinica.CadastrarPaciente(paciente2);
        clinica.CadastrarPaciente(paciente3);

        // Médico adiciona horários disponíveis
        medico1.AdicionarHorario(DateTime.Now.AddDays(1).Date.AddHours(9));
        medico1.AdicionarHorario(DateTime.Now.AddDays(1).Date.AddHours(10));
        medico2.AdicionarHorario(DateTime.Now.AddDays(2).Date.AddHours(14));

        // ──────────────────────────────────────────────────────────────────────
        // Agendando consultas
        // ──────────────────────────────────────────────────────────────────────
        Console.WriteLine("\n--- AGENDAMENTOS ---");
        var c1 = clinica.AgendarConsulta(medico1, paciente1,
            DateTime.Now.AddDays(1).Date.AddHours(9), central);
        var c2 = clinica.AgendarConsulta(medico1, paciente2,
            DateTime.Now.AddDays(1).Date.AddHours(10), central);
        var c3 = clinica.AgendarConsulta(medico2, paciente3,
            DateTime.Now.AddDays(2).Date.AddHours(14), central);

        // Tentativa de agendar médico não cadastrado → ClinicaException
        var medicoForaClinica = new Medico("Dr. Forasteiro", "11900000000",
            new DateTime(1985, 1, 1), "CRM-99999", "Dermatologia");
        Safe.Run(
            () => clinica.AgendarConsulta(medicoForaClinica, paciente1,
                                          DateTime.Now.AddDays(3), central),
            "Agendar médico não cadastrado"
        );

        // ──────────────────────────────────────────────────────────────────────
        // ⑥ Exceções + ⑤ Eventos de estado
        // ──────────────────────────────────────────────────────────────────────
        Console.WriteLine("--- CONFIRMAR E REALIZAR ---");
        Safe.Run(() => c1.Confirmar(), "Confirmar c1");  // → dispara OnConfirmada
        Safe.Run(() => c2.Confirmar(), "Confirmar c2");

        // Tentar confirmar duas vezes → exceção
        Safe.Run(() => c1.Confirmar(), "Confirmar c1 novamente");

        // Realizar consulta → dispara OnRealizada e adiciona ao prontuário
        Safe.Run(() => c1.Realizar("Paciente com leve arritmia. Prescrito Atenolol 25mg."),
            "Realizar c1");
        Safe.Run(() => c2.Realizar("Hipertensão estabilizada. Manter medicação atual."),
            "Realizar c2");

        // Tentar realizar sem confirmar → exceção
        Safe.Run(() => c3.Realizar("Consulta sem confirmação"), "Realizar c3 sem confirmar");

        // Cancelamento com motivo → dispara OnCancelada
        Console.WriteLine("\n--- CANCELAMENTO ---");
        Safe.Run(() => c3.Cancelar("Paciente viajou."), "Cancelar c3");
        // Tentar cancelar já cancelada → exceção
        Safe.Run(() => c3.Cancelar("Segundo cancelamento"), "Cancelar c3 novamente");

        // ──────────────────────────────────────────────────────────────────────
        // ③ Interface ICalculavelFinanceiro — valores por convênio
        // ──────────────────────────────────────────────────────────────────────
        Console.WriteLine("--- VALORES POR CONVÊNIO ---");
        foreach (var c in new[] { c1, c2, c3 })
        {
            Console.WriteLine($"  {c.PacienteAtendido.Convenio,-12} | " +
                              $"Base: R${c.ValorBase:F2} | " +
                              $"Desconto: R${c.CalcularDesconto():F2} | " +
                              $"Final: R${c.ValorFinal():F2}");
        }

        // ──────────────────────────────────────────────────────────────────────
        // ⑧ LINQ — consultas variadas
        // ──────────────────────────────────────────────────────────────────────
        Console.WriteLine("\n--- CONSULTAS LINQ ---");

        var cardio = clinica.PorEspecialidade("Cardiologia");
        Console.WriteLine($"Consultas em Cardiologia: {cardio.Count}");
        cardio.ForEach(c => Console.WriteLine($"  {c.GerarRelatório()}"));

        var porStatus = clinica.ConsultasPorStatus();
        Console.WriteLine("\nPor status:");
        foreach (var (status, qty) in porStatus)
            Console.WriteLine($"  {status}: {qty}");

        var taxas = clinica.TaxaCancelamentoPorMedico();
        Console.WriteLine("\nTaxa de cancelamento por médico:");
        foreach (var (nome, taxa) in taxas)
            Console.WriteLine($"  Dr(a). {nome}: {taxa:F1}%");

        // Histórico do Paciente 1 (IRelatorio + IAssociação)
        var hist1 = clinica.ObterHistorico(paciente1);
        Console.WriteLine($"\n{hist1.GerarRelatório()}");
        Console.WriteLine($"  Gasto total: R$ {hist1.GastoTotal:F2}");

        // Prontuário anotado durante Realizar()
        Console.WriteLine($"\nProntuário de {paciente1.Nome}:");
        paciente1.Prontuario.ToList().ForEach(a => Console.WriteLine($"  {a}"));

        // ──────────────────────────────────────────────────────────────────────
        // Painel geral
        // ──────────────────────────────────────────────────────────────────────
        clinica.ExibirPainel();

        // ──────────────────────────────────────────────────────────────────────
        // ⑨ Async/Await + ⑩ Arquivo
        // ──────────────────────────────────────────────────────────────────────
        Console.WriteLine("\n--- PERSISTÊNCIA ASSÍNCRONA ---");
        await Safe.RunAsync(
            async () => await clinica.SalvarLogAsync("log_clinica.txt", central.Log),
            "Salvar log"
        );
        await Safe.RunAsync(
            async () => await clinica.SalvarRelatoriosAsync("historicos_clinica.txt"),
            "Salvar históricos"
        );

        // Lê de volta e mostra prévia
        await Safe.RunAsync(async () =>
        {
            var linhas = await clinica.LerArquivoAsync("log_clinica.txt");
            Console.WriteLine($"\n📄 log_clinica.txt ({linhas.Count} linhas):");
            linhas.Take(5).ToList().ForEach(l => Console.WriteLine($"   {l}"));
        }, "Ler log");

        // Arquivo inexistente → FileNotFoundException
        await Safe.RunAsync(
            async () => { var _ = await clinica.LerArquivoAsync("inexistente.txt"); },
            "Ler arquivo inexistente"
        );

        Console.WriteLine("✅ Exercício #3 concluído!\n");

/*
==============================================
CONCEITOS USADOS NESTE EXERCÍCIO:
==============================================
① Encapsulamento   → Pessoa: _nome/_telefone com validação nos setters
                     Idade calculada no get (propriedade derivada)
② Herança          → Medico e Paciente herdam de Pessoa
                     sealed em Paciente impede novas heranças
③ Interfaces       → IAgendavel, IRelatorio, ICalculavelFinanceiro em Consulta
                     IRelatorio em HistoricoConsultas
④ Composição       → Consulta TEM Medico e Paciente
   Associação      → HistoricoConsultas REFERENCIA Paciente e Consultas
⑤ Delegate/Evento  → OnConfirmada, OnCancelada, OnRealizada em Consulta
                     Múltiplos ouvintes (+= lambda e método)
⑥ Exceções         → ClinicaException customizada com Codigo
                     Safe.Run e Safe.RunAsync centralizados
⑦ List<T>          → _medicos, _pacientes, _consultas, _historicos
⑧ LINQ             → Where, OrderBy, GroupBy, Sum, Any, First, ToDictionary
⑨ Async/Await      → SalvarLogAsync, SalvarRelatoriosAsync, LerArquivoAsync
⑩ Arquivo (IO)     → StreamWriter (linha a linha) + File.WriteAllLinesAsync
                     File.ReadAllLinesAsync*/
    }
}