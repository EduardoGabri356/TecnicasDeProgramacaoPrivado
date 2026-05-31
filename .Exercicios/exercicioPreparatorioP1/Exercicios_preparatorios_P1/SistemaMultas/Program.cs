using System.Text.Json;

// Instanciamos o "cérebro" do sistema, que vai gerenciar as listas, arquivos e eventos.
CentralDeMultas central = new CentralDeMultas();

// Antes de fazer qualquer coisa, tentamos carregar dados antigos que já estejam salvos no HD.
central.CarregarDeJson();

// ASSINATURA DE EVENTO: O operador '+=' significa "inscrever". 
// Estamos dizendo: "Central, quando o evento MultaRegistrada acontecer, chame o método Mensagem da classe VerificarMulta".
central.MultaRegistrada += VerificarMulta.Mensagem;

// Criando e registrando multas de teste.
// Note o sufixo 'm' após os números. Ele avisa ao C# que o valor é do tipo 'decimal' (ideal para dinheiro).
central.Registrar(new Multa { Placa = "ABC-1234", TipoInfracao = "Velocidade", Valor = 180.50m, Data = DateTime.Now });
central.Registrar(new Multa { Placa = "XYZ-9876", TipoInfracao = "Embriaguez", Valor = 2934.70m, Data = DateTime.Now });
central.Registrar(new Multa { Placa = "KJA-4455", TipoInfracao = "Farol apagado", Valor = 130.16m, Data = DateTime.Now });

// Usando o LINQ (dentro do método) para buscar apenas multas acima de R$ 500.
central.FiltrarPorValor(500);

Console.WriteLine("\nFim do processamento.");
Console.ReadKey();
// DECLARAÇÃO DAS CLASSES E TIPOS

// CLASSE DE DADOS (Modelo)
public class Multa
{
    // Construtor vazio padrão. Necessário para que o desserializador do JSON consiga recriar o objeto.
    public Multa() { }

    // Propriedades (get e set permitem ler e gravar os dados)
    public string? Placa { get; set; }
    public string? TipoInfracao { get; set; }
    public decimal Valor { get; set; } // Decimal é o tipo mais preciso e seguro para valores monetários
    public DateTime Data { get; set; }
}

// DELEGATE: O "Contrato" do Evento
// Ele define a regra: "Qualquer método que quiser ouvir meus eventos precisa retornar 'void' e receber um objeto 'Multa' como parâmetro".
public delegate void MultaHandler(Multa m);


// CLASSE GERENCIADORA
public class CentralDeMultas
{
    // EVENTO: É o "alto-falante". Ele usa a regra (delegate) definida acima.
    public event MultaHandler MultaRegistrada;

    // Lista privada (encapsulada) que guarda as multas apenas na memória durante a execução do programa.
    private List<Multa> Multas = new List<Multa>();

    // MÉTODO DE INSERÇÃO
    public void Registrar(Multa m)
    {
        // 1. Adiciona a multa recebida na lista em memória.
        Multas.Add(m);
        Console.WriteLine($"\nRegistrando multa: {m.Placa}...");

        // 2. O sinal '?' verifica se existe alguém ouvindo o evento (se não for nulo).
        // Se houver assinantes (como a nossa classe VerificarMulta), o .Invoke(m) avisa todos eles, mandando a multa 'm' de presente.
        MultaRegistrada?.Invoke(m);

        // 3. Após registrar na memória e avisar os ouvintes, salva tudo no arquivo físico.
        SalvarEmJson();
    }

    // MÉTODO PARA SALVAR EM DISCO
    public void SalvarEmJson()
    {
        // Configura o JSON para ficar formatado com quebras de linha e recuos (fácil de ler por humanos).
        var options = new JsonSerializerOptions { WriteIndented = true };

        // Transforma a nossa Lista de objetos do C# em um grande texto (string) no formato JSON.
        string jsonString = JsonSerializer.Serialize(Multas, options);

        // Escreve esse texto dentro de um arquivo chamado "multas.json" na mesma pasta do executável.
        File.WriteAllText("multas.json", jsonString);
    }

    // MÉTODO PARA CARREGAR DO DISCO
    public void CarregarDeJson()
    {
        // Só tenta ler se o arquivo existir, evitando que o programa quebre na primeira vez que rodar.
        if (File.Exists("multas.json"))
        {
            // Lê todo o conteúdo de texto do arquivo.
            string jsonString = File.ReadAllText("multas.json");

            // Faz o caminho inverso: converte o texto JSON de volta para uma List<Multa> do C#.
            // O operador '??' diz: "Se a conversão falhar ou retornar nulo, crie uma lista vazia nova".
            Multas = JsonSerializer.Deserialize<List<Multa>>(jsonString) ?? new List<Multa>();

            Console.WriteLine("Dados carregados do arquivo JSON.");
            // Mostra na tela o que acabou de ser carregado do arquivo.
            ListarMultas(Multas);
        }
    }

    // CONSULTA LINQ
    public void FiltrarPorValor(decimal valor)
    {
        // O '.Where' percorre a lista. A expressão 'm => m.Valor > valor' é o filtro (Traz quem tem valor maior que o limite).
        // O '.ToList()' transforma o resultado do filtro de volta em uma Lista utilizável.
        var multasCaras = Multas.Where(m => m.Valor > valor).ToList();

        // O ':C2' formata o número automaticamente como moeda (R$) com 2 casas decimais.
        Console.WriteLine($"\n--- Multas acima de {valor:C2} ---");
        ListarMultas(multasCaras);
    }

    // MÉTODO AUXILIAR DE IMPRESSÃO
    // Recebe IEnumerable, ou seja, aceita tanto uma Lista inteira quanto o resultado parcial de um filtro LINQ.
    public void ListarMultas(IEnumerable<Multa> multas)
    {
        foreach (var multa in multas)
        {
            // Ajustado '/n' para '\n' no final, que é o caractere correto de quebra de linha (Enter) na programação.
            Console.WriteLine($"Placa:{multa.Placa} - Tipo de Infracao: {multa.TipoInfracao} - Valor: {multa.Valor:C2} - Data da Multa: {multa.Data}\n");
        }
    }
}

// CLASSE OUVINTE (Subscriber)
class VerificarMulta
{
    // Este método tem EXATAMENTE a mesma assinatura exigida pelo delegate 'MultaHandler' (retorna void, recebe Multa).
    // Ser 'static' significa que podemos usar o método sem precisar instanciar a classe com 'new'.
    public static void Mensagem(Multa m)
    {
        // Regra de negócio isolada: Toda vez que o evento é chamado, ele checa se a multa específica é maior que 500.
        if (m.Valor > 500)
        {
            Console.WriteLine("ALERTA: MULTA GRAVE DETECTADA!");
        }
    }
}