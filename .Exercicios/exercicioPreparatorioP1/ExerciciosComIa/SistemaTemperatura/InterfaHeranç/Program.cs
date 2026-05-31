/* Comportamento do Evento
Sempre que um abastecimento for registrado:

O evento AlertaGastoElevado deve ser disparado se o valor total do abastecimento for maior que R$ 1.000,00.

Ouvintes: Devem exibir uma mensagem de auditoria no console informando a placa do veículo e o valor gasto.

Requisitos Técnicos
Interface IAbastecivel: Contendo o método Abastecer(double litros, decimal precoLitro).  

Herança: Classe base Veiculo e classes derivadas Carro e Caminhao.

Exceção Personalizada: CapacidadeExcedidaException, lançada se os litros abastecidos somados ao nível atual ultrapassarem o tanque.  

Persistência: Salvar o histórico em frota.json.

LINQ: Gerar um relatório de total gasto agrupado por tipo de veículo. */

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Linq;

// 1. EXCEÇÃO PERSONALIZADA
// Criamos uma classe que herda de Exception para tratar erros específicos de negócio[cite: 1].
public class CapacidadeExcedidaException : Exception
{
    public CapacidadeExcedidaException(string mensagem) : base(mensagem) { }
}

// 2. INTERFACE
// Define um "contrato". Qualquer veículo que possa ser abastecido DEVE implementar este método[cite: 1].
public interface IAbastecivel
{
    void Abastecer(double litros, decimal precoLitro);
}

// 3. CLASSE BASE (Herança)
// Contém o que é comum a todos os veículos[cite: 1].
public abstract class Veiculo
{
    public string Placa { get; set; }
    public string Modelo { get; set; }
    public double CapacidadeTanque { get; set; }
    public double NivelAtual { get; set; }

    // Método comum para mostrar dados básicos
    public virtual void ExibirInfo()
    {
        Console.WriteLine($"Veículo: {Modelo} | Placa: {Placa}");
    }
}

// 4. CLASSES DERIVADAS
// Implementam a interface e herdam as propriedades da base[cite: 1].


public class Carro : Veiculo, IAbastecivel
{
    public void Abastecer(double litros, decimal precoLitro)
    {
        // Validação usando a nossa Exceção Personalizada
        if (NivelAtual + litros > CapacidadeTanque)
            throw new CapacidadeExcedidaException($"Tanque do carro suporta apenas {CapacidadeTanque}L!");

        NivelAtual += litros;
        Console.WriteLine($"Carro {Placa} abastecido com {litros}L.");
    }
}

public class Caminhao : Veiculo, IAbastecivel
{
    public void Abastecer(double litros, decimal precoLitro)
    {
        if (NivelAtual + litros > CapacidadeTanque)
            throw new CapacidadeExcedidaException($"Tanque do caminhão suporta apenas {CapacidadeTanque}L!");

        NivelAtual += litros;
        Console.WriteLine($"Caminhão {Placa} abastecido com {litros}L.");
    }
}
// ==============================================================================
// 1. DELEGATE E CLASSE DE HISTÓRICO
// ==============================================================================

// O contrato: quem ouvir o alerta deve estar preparado para receber os detalhes do abastecimento.
public delegate void AlertaHandler(RegistroAbastecimento registro);

// Classe simples para representar o que será salvo no banco de dados JSON.
public class RegistroAbastecimento
{
    public string Placa { get; set; }
    public string TipoVeiculo { get; set; } // "Carro" ou "Caminhão"
    public decimal ValorTotal { get; set; }
    public DateTime Data { get; set; }
}

// ==============================================================================
// 2. GERENCIADOR DE LOGÍSTICA
// ==============================================================================

public class LogisticaManager
{
    // O EVENTO: Disparado quando o gasto é considerado elevado (> 1000)[cite: 1].
    public event AlertaHandler AlertaGastoElevado;

    // Lista interna em memória para o histórico.
    private List<RegistroAbastecimento> Historico = new();

    public void ProcessarAbastecimento(Veiculo v, double litros, decimal precoLitro)
    {
        try
        {
            // Tenta abastecer o veículo (pode lançar a nossa CapacidadeExcedidaException)[cite: 1].
            // Como v é IAbastecivel, fazemos um cast seguro para chamar o método.
            if (v is IAbastecivel veiculoAbastecivel)
            {
                veiculoAbastecivel.Abastecer(litros, precoLitro);

                decimal total = (decimal)litros * precoLitro;

                // Cria o registro para o histórico.
                var novoRegistro = new RegistroAbastecimento
                {
                    Placa = v.Placa,
                    TipoVeiculo = v.GetType().Name, // Captura o nome da classe (Carro/Caminhao)
                    ValorTotal = total,
                    Data = DateTime.Now
                };

                Historico.Add(novoRegistro);

                // GATILHO DO EVENTO: Verifica se o gasto foi alto[cite: 1].
                if (total > 1000)
                {
                    AlertaGastoElevado?.Invoke(novoRegistro);
                }

                // Salva os dados atualizados no JSON[cite: 1].
                SalvarDados();
            }
        }
        catch (CapacidadeExcedidaException ex)
        {
            // Tratamento da exceção personalizada que criamos na Parte 1[cite: 1].
            Console.WriteLine($"[ERRO DE OPERAÇÃO] {ex.Message}");
        }
    }

    private void SalvarDados()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(Historico, options);
        File.WriteAllText("frota.json", json);
    }

    // CONSULTA LINQ: Agrupamento por Tipo de Veículo[cite: 1].
    public void GerarRelatorioGastos()
    {
        Console.WriteLine("\n--- RELATÓRIO DE GASTOS POR CATEGORIA (LINQ) ---");

        var agrupado = Historico
            .GroupBy(r => r.TipoVeiculo)
            .Select(g => new {
                Tipo = g.Key,
                TotalGasto = g.Sum(r => r.ValorTotal)
            });

        foreach (var item in agrupado)
        {
            Console.WriteLine($"Categoria: {item.Tipo} | Gasto Total: {item.TotalGasto:C2}");
        }
    }
}

// ==============================================================================
// 3. EXECUÇÃO (SUBSTITUIU TOP-LEVEL POR MAIN)
// ==============================================================================

public static class Program
{
    public static void Main()
    {
        LogisticaManager manager = new();

        // REGISTRO DE OUVINTE: Auditoria simples no console[cite: 1].
        manager.AlertaGastoElevado += (reg) =>
            Console.WriteLine($"[ALERTA AUDITORIA] Gasto elevado detectado na Placa {reg.Placa}: {reg.ValorTotal:C2}");

        // Criando veículos para teste
        Caminhao caminhao = new() { Placa = "TRK-2024", Modelo = "Volvo FH", CapacidadeTanque = 500, NivelAtual = 100 };
        Carro carro = new() { Placa = "CAR-1234", Modelo = "Civic", CapacidadeTanque = 50, NivelAtual = 10 };

        // Simulando abastecimentos
        manager.ProcessarAbastecimento(carro, 30, 6.50m);     // Valor baixo (~195.00)
        manager.ProcessarAbastecimento(caminhao, 300, 5.80m); // Valor alto (~1740.00) -> DISPARA EVENTO

        // Relatório final
        manager.GerarRelatorioGastos();

        Console.WriteLine("\nSistema Finalizado.");
    }
}