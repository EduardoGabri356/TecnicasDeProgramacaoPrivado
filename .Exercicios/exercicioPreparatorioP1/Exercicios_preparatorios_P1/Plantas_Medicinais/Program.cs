
using System.Text.Json; 

// BLOCO PRINCIPAL (Top-Level Statements)

// 1. Instanciamos a classe principal que gerencia o inventário
InventarioDePlantas inventario = new InventarioDePlantas();

// 2. REGISTRO DE OUVINTES (Assinaturas do Evento)
// O mesmo evento vai disparar DUAS ações diferentes!

// Ouvinte 1: Quando o evento ocorrer, execute o método LogTxt da classe GravarLog
inventario.PlantaEmExticao += GravarLog.LogTxt;

// Ouvinte 2: Quando o mesmo evento ocorrer, execute também o método ExibirColeta da classe Exibir
inventario.PlantaEmExticao += Exibir.ExibirColeta;

// 3. PREPARAÇÃO DO AMBIENTE
// Verifica se o arquivo já existe. Se não existir, chama o método que cria dados fictícios.
// Isso evita que o programa quebre na primeira execução por falta do arquivo "plantas.json"
if (!File.Exists("plantas.json"))
{
    CriarJsonTeste();
}

// 4. EXECUÇÃO
// Carrega os dados do arquivo JSON
// ATENÇÃO: É dentro deste método que o evento será disparado se a planta estiver em extinção!
inventario.CarregarDeArquivoJson("plantas.json");

// 5. EXIBIÇÃO DOS RELATÓRIOS (LINQ)
// Executa as consultas para mostrar quantidades e agrupamentos por local
inventario.MostrarConsultasColetas();

Console.WriteLine("\nProcesso Finalizado");
// FIM DO BLOCO PRINCIPAL


// MÉTODO AUXILIAR: Cria o arquivo de teste inicial
static void CriarJsonTeste()
{
    var lista = new List<Planta>
    {
        new Planta { NomePopular = "Samambaia-Açu", NomeCientifico = "Dicksonia sellowiana", LocalColeta = "Mata Atlântica", EmExtincao = true, DataColeta = DateTime.Now },
        new Planta { NomePopular = "Ipê Amarelo", NomeCientifico = "Handroanthus albus", LocalColeta = "Cerrado", EmExtincao = false, DataColeta = DateTime.Now },
        new Planta { NomePopular = "Pau-Brasil", NomeCientifico = "Paubrasilia echinata", LocalColeta = "Mata Atlântica", EmExtincao = true, DataColeta = DateTime.Now },
        new Planta { NomePopular = "Ipê Amarelo", NomeCientifico = "Handroanthus albus", LocalColeta = "Mata Atlântica", EmExtincao = false, DataColeta = DateTime.Now.AddDays(-1) }
    };
    // Transforma a lista em texto e salva fisicamente no computador
    File.WriteAllText("plantas.json", JsonSerializer.Serialize(lista));
}

// CLASSES E DELEGATES

// CLASSE DE DADOS (Modelo da Planta)
public class Planta
{
    public string? NomeCientifico { get; set; }
    public string? NomePopular { get; set; }
    public string? LocalColeta { get; set; }
    public bool EmExtincao { get; set; }
    public DateTime DataColeta { get; set; }
}

// DELEGATE E EVENTO
// O delegate define que qualquer ouvinte precisa receber um objeto 'Planta'
public delegate void PlantaHandler(Planta p);

public class InventarioDePlantas
{
    // Declaração do evento que usará o delegate acima
    public event PlantaHandler PlantaEmExticao;

    // Lista interna e privada para armazenar as plantas em memória
    private List<Planta> Plantas = new List<Planta>();

    // MÉTODO: CARREGAR E DISPARAR EVENTO
    public void CarregarDeArquivoJson(string caminho)
    {
        if (!File.Exists(caminho))
        {
            Console.WriteLine("Arquivo JSON não encontrado!");
            return; // Interrompe o método se o arquivo não existir
        }

        // Lê o texto do arquivo
        string jsonString = File.ReadAllText(caminho);

        // Converte o texto JSON de volta para uma Lista de Plantas no C#.
        Plantas = JsonSerializer.Deserialize<List<Planta>>(jsonString) ?? new List<Planta>();

        // Percorre todas as plantas que acabaram de ser carregadas do banco/arquivo
        foreach (var p in Plantas)
        {
            // Regra de Negócio: Se a planta está ameaçada de extinção...
            if (p.EmExtincao)
            {
                // ...Dispara o evento! O '?.Invoke' garante que o programa não quebre se não houver ouvintes.
                // Como registramos dois ouvintes no início do código, ambos serão chamados agora!
                PlantaEmExticao?.Invoke(p);
            }
        }
    }

    // MÉTODO: CONSULTAS LINQ
    public void MostrarConsultasColetas()
    {
        Console.WriteLine("\n--- INVENTÁRIO ---");

        // LINQ 1: Mostrar a quantidade de coletas por planta
        // O GroupBy junta todas as plantas com o mesmo nome.
        // O Select cria um tipo anônimo com o Nome (a Key do grupo) e a quantidade (Count do grupo).
        var coletasPorPlanta = Plantas
            .GroupBy(p => p.NomePopular)
            .Select(g => new { Nome = g.Key, Qtd = g.Count() });

        Console.WriteLine("\nQuantidade de coletas por planta:");
        foreach (var item in coletasPorPlanta)
            Console.WriteLine($"- {item.Nome}: {item.Qtd} coleta(s)");


        // LINQ 2: Agrupar por Local de Coleta.
        // Aqui, a 'Key' será o nome do local (Ex: "Mata Atlântica"). 
        // Dentro de cada grupo, teremos a lista de plantas coletadas lá.
        var agrupadoPorLocal = Plantas.GroupBy(p => p.LocalColeta);

        Console.WriteLine("\nPlantas agrupadas por Local:");
        foreach (var grupo in agrupadoPorLocal)
        {
            Console.WriteLine($"\nLocal: {grupo.Key}");
            // Iterando dentro do grupo para listar os itens que pertencem a ele
            foreach (var p in grupo)
                Console.WriteLine($"  > {p.NomePopular} ({p.NomeCientifico})");
        }
    }
}

// CLASSES OUVINTES (Subscribers do Evento)

// OUVINTE 1: Responsável apenas por criar o arquivo de Log
class GravarLog
{
    public static void LogTxt(Planta p)
    {
        // Monta a string com a mensagem de alerta específica
        string logMsg = $"{p.NomePopular} | {p.DataColeta:dd/MM/yyyy} | Alerta: Planta em perigo de extinção.\n";

        // Diferente do WriteAllText (que apaga e recria), o AppendAllText adiciona a linha no final do arquivo existente
        File.AppendAllText("log.txt", logMsg);
    }
}

// OUVINTE 2: Responsável apenas por avisar o usuário na tela do Console
class Exibir
{
    public static void ExibirColeta(Planta p)
    {
        // Exibe o alerta no console com o nome e o local[cite: 1].
        Console.WriteLine($"[ALERTA EXTINÇÃO] Nome: {p.NomePopular} | Local: {p.LocalColeta}");
    }
}