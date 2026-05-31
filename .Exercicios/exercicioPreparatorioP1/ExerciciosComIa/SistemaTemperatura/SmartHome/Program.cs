using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

// ==============================================================================
// 1. BLOCO DE EXECUÇÃO (TOP-LEVEL)
// ==============================================================================

Console.WriteLine("--- Iniciando Monitoramento de Smart Home ---\n");

// PASSO 1: Preparar os dados de teste (JSON)
var dadosTeste = new List<SensorData> {
    new SensorData { Id = "S01", Local = "Cozinha", Status = "Fumaça" },
    new SensorData { Id = "S02", Local = "Sala", Status = "Normal" },
    new SensorData { Id = "S03", Local = "Garagem", Status = "Invasão" }
};
File.WriteAllText("sensores.json", JsonSerializer.Serialize(dadosTeste));

// PASSO 2: Instanciar a central e configurar os ouvintes (Assinatura de eventos)[cite: 1]
CentralSeguranca central = new CentralSeguranca();

central.AlarmeAtivado += LogSeguranca.GravarNoArquivo; // Ouvinte 1
central.AlarmeAtivado += ServicoEmergencia.ChamarAutoridades; // Ouvinte 2 (Async)

// PASSO 3: Chamar o carregamento assíncrono[cite: 1]
await central.CarregarConfiguracoesAsync("sensores.json");

// Pequena pausa para garantir que o console não feche antes da "chamada" terminar
await Task.Delay(3000);

Console.WriteLine("\nMonitoramento concluído.");


// ==============================================================================
// 2. DEFINIÇÕES DE TIPOS (Delegates, Interfaces e Classes)
// Devem vir SEMPRE depois do código de execução acima.
// ==============================================================================

// DELEGATE: O contrato do evento[cite: 1]
public delegate void AlarmeHandler(string local, string tipoAlerta);

// INTERFACE: O contrato dos dados[cite: 1]
public interface ISensor
{
    string Id { get; set; }
    string Local { get; set; }
    string Status { get; set; }
}

// CLASSE DE DADOS[cite: 1]
public class SensorData : ISensor
{
    public string Id { get; set; }
    public string Local { get; set; }
    public string Status { get; set; }
}

// CLASSE GERENCIADORA (Lógica de Negócio)[cite: 1]
public class CentralSeguranca
{
    public event AlarmeHandler AlarmeAtivado;

    public async Task CarregarConfiguracoesAsync(string caminho)
    {
        if (!File.Exists(caminho)) return;

        // Leitura assíncrona de arquivo[cite: 1]
        string json = await File.ReadAllTextAsync(caminho);
        var sensores = JsonSerializer.Deserialize<List<SensorData>>(json);

        if (sensores != null)
        {
            foreach (var s in sensores)
            {
                Console.WriteLine($"[SISTEMA] Checando sensor {s.Id}...");

                // Gatilho do evento se o status não for Normal[cite: 1]
                if (s.Status != "Normal")
                {
                    AlarmeAtivado?.Invoke(s.Local, s.Status);
                }
            }
        }
    }
}

// OUVINTE 1: Gravação de arquivo TXT[cite: 1]
class LogSeguranca
{
    public static void GravarNoArquivo(string local, string tipo)
    {
        string mensagem = $"{DateTime.Now} | ALERTA: {tipo} em {local}.\n";
        File.AppendAllText("alerta_seguranca.txt", mensagem);
        Console.WriteLine("[LOG] Alerta registrado em TXT.");
    }
}

// OUVINTE 2: Simulação de chamada assíncrona[cite: 1]
class ServicoEmergencia
{
    public static async void ChamarAutoridades(string local, string tipo)
    {
        Console.WriteLine($"[EMERGÊNCIA] Acionando autoridades para {tipo}...");
        await Task.Delay(2000); // Simula delay de rede/chamada[cite: 1]
        Console.WriteLine($"[EMERGÊNCIA] Polícia/Bombeiros enviados para: {local}!");
    }
}