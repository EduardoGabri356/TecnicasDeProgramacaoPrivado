using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

// ==============================================================================
// BLOCO PRINCIPAL (Execução direta via Top-Level Statements)
// ==============================================================================

// 1. Instanciamos a classe que gerencia a lógica do sistema
Monitoramento monitor = new Monitoramento();

// 2. REGISTRO DE OUVINTE (Assinatura do Evento)
// Dizemos ao sistema: "Quando o AlertaDegelo disparar, execute o método ExibirPerigo".
monitor.AlertaDegelo += AlertaVisual.ExibirPerigo;

// 3. REGISTRO DE DADOS
// Simulamos a entrada de dados. Note que temperaturas > 0 devem acionar o ouvinte imediatamente.
monitor.RegistrarLeitura(new Leitura { SensorId = "S01", Temperatura = -12.5, Horario = DateTime.Now });
monitor.RegistrarLeitura(new Leitura { SensorId = "S01", Temperatura = 1.2, Horario = DateTime.Now }); // GATILHO!
monitor.RegistrarLeitura(new Leitura { SensorId = "S02", Temperatura = -5.0, Horario = DateTime.Now });
monitor.RegistrarLeitura(new Leitura { SensorId = "S02", Temperatura = 0.8, Horario = DateTime.Now }); // GATILHO![cite: 1]

// 4. CONSULTA LINQ
// Chamamos o método que utiliza LINQ para processar a média de um sensor específico[cite: 1].
monitor.CalcularMediaSensor("S01");

Console.WriteLine("\nProcessamento finalizado. Verifique 'temperaturas.json'.");
Console.ReadKey();

// ==============================================================================
// DEFINIÇÃO DE CLASSES, DELEGATES E EVENTOS
// ==============================================================================

// CLASSE DE DADOS (Modelo)
public class Leitura
{
    public string? SensorId { get; set; }
    public double Temperatura { get; set; }
    public DateTime Horario { get; set; }
}

// DELEGATE: Define a "forma" que o método ouvinte deve ter (retorno void, recebe Leitura)[cite: 1].
public delegate void TemperaturaHandler(Leitura l);

public class Monitoramento
{
    // EVENTO: O canal que notificará os assinantes sobre problemas térmicos[cite: 1].
    public event TemperaturaHandler AlertaDegelo;

    // Lista privada para manter os dados em memória durante a execução.
    private List<Leitura> Historico = new List<Leitura>();

    // MÉTODO: REGISTRAR E DISPARAR
    public void RegistrarLeitura(Leitura l)
    {
        // 1. Adiciona o objeto à lista interna.
        Historico.Add(l);
        Console.WriteLine($"[LOG] Sensor {l.SensorId}: {l.Temperatura}°C lido com sucesso.");

        // 2. VERIFICAÇÃO DE REGRA (Gatilho): Se a temperatura subir acima de zero...[cite: 1]
        if (l.Temperatura > 0)
        {
            // ...o evento é disparado para todos os que estiverem "ouvindo"[cite: 1].
            AlertaDegelo?.Invoke(l);
        }

        // 3. PERSISTÊNCIA: Salva o estado atual da lista no arquivo JSON[cite: 1].
        SalvarEmJson();
    }

    // MÉTODO: SALVAR JSON (Nativo)
    private void SalvarEmJson()
    {
        // Configura o JSON para ser "bonito" (identado) ao abrir no bloco de notas.
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(Historico, options);

        // Grava o texto no arquivo físico[cite: 1].
        File.WriteAllText("temperaturas.json", json);
    }

    // MÉTODO: CONSULTA LINQ
    public void CalcularMediaSensor(string id)
    {
        // Filtra a lista pelo ID desejado e calcula a média da propriedade Temperatura[cite: 1].
        // Usamos .Average() que é um método de agregação do LINQ.
        double media = Historico
            .Where(l => l.SensorId == id)
            .Average(l => l.Temperatura);

        Console.WriteLine($"\n--- RESULTADO LINQ ---");
        Console.WriteLine($"Média de temperatura do Sensor {id}: {media:F2}°C");
    }
}

// CLASSE OUVINTE (Reação)
class AlertaVisual
{
    // Método estático que segue a regra do delegate TemperaturaHandler[cite: 1].
    public static void ExibirPerigo(Leitura l)
    {
        // Esta linha só aparece no console quando o evento é disparado[cite: 1].
        Console.WriteLine($"!!! PERIGO: Temperatura em {l.Temperatura}°C no sensor {l.SensorId} !!!");
    }
}