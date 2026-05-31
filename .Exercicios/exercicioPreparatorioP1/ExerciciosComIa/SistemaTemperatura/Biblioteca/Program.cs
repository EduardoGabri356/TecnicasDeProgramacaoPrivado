using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// ==============================================================================
// BLOCO PRINCIPAL (Top-Level Statements)
// ==============================================================================

// 1. Instanciamos a classe que gerencia a biblioteca
Biblioteca biblioteca = new Biblioteca();

// 2. REGISTRO DE MÚLTIPLOS OUVINTES (Multicast Delegate)
// O mesmo evento disparará duas reações em classes diferentes.
// Ouvinte 1: Calcula e exibe a multa no console.
biblioteca.LivroAtrasado += Financeiro.CalcularMulta;

// Ouvinte 2: Registra o nome do devedor em um arquivo de texto.
biblioteca.LivroAtrasado += Relatorio.RegistrarDevedor;

// 3. REGISTRO DE DEVOLUÇÕES
// Vamos simular devoluções. Se a DataEntregaReal for maior que a DataEsperada, o evento dispara.
Console.WriteLine("--- Processando Devoluções ---\n");

// Exemplo 1: Devolvido no prazo
biblioteca.RegistrarDevolucao(new Emprestimo
{
    TituloLivro = "C# Moderno",
    NomeCliente = "Ana Silva",
    DataEsperada = new DateTime(2024, 05, 01),
    DataEntregaReal = new DateTime(2024, 05, 01)
});

// Exemplo 2: Atraso de 3 dias (Gatilho!)
biblioteca.RegistrarDevolucao(new Emprestimo
{
    TituloLivro = "Algoritmos",
    NomeCliente = "Bruno Souza",
    DataEsperada = new DateTime(2024, 05, 01),
    DataEntregaReal = new DateTime(2024, 05, 04)
});

// Exemplo 3: Outro atraso de 5 dias para o mesmo cliente (Gatilho!)
biblioteca.RegistrarDevolucao(new Emprestimo
{
    TituloLivro = "Banco de Dados",
    NomeCliente = "Bruno Souza",
    DataEsperada = new DateTime(2024, 05, 01),
    DataEntregaReal = new DateTime(2024, 05, 06)
});

// 4. CONSULTA LINQ
// Agrupar por cliente e contar o total de livros devolvidos (no prazo ou não).
biblioteca.MostrarResumoPorCliente();

Console.WriteLine("\nSistema encerrado.");
Console.ReadKey();

// ==============================================================================
// CLASSES, DELEGATES E EVENTOS
// ==============================================================================

// CLASSE DE DADOS (Modelo)
public class Emprestimo
{
    public string TituloLivro { get; set; }
    public string NomeCliente { get; set; }
    public DateTime DataEsperada { get; set; }
    public DateTime DataEntregaReal { get; set; }
}

// DELEGATE: O contrato para o evento de atraso
public delegate void AtrasoHandler(Emprestimo e);

public class Biblioteca
{
    // EVENTO: Notifica quando um livro é entregue fora do prazo
    public event AtrasoHandler LivroAtrasado;

    // Lista em memória para armazenar todos os registros de empréstimos finalizados
    private List<Emprestimo> HistoricoDevolucoes = new List<Emprestimo>();

    public void RegistrarDevolucao(Emprestimo e)
    {
        HistoricoDevolucoes.Add(e);
        Console.WriteLine($"Recebendo livro: '{e.TituloLivro}' de {e.NomeCliente}...");

        // GATILHO: Compara as datas para verificar atraso
        if (e.DataEntregaReal > e.DataEsperada)
        {
            // Dispara o evento para todos os ouvintes (Financeiro e Relatorio)
            LivroAtrasado?.Invoke(e);
        }
    }

    // MÉTODO COM LINQ: Agrupamento
    public void MostrarResumoPorCliente()
    {
        Console.WriteLine("\n--- RESUMO DE ATIVIDADE POR CLIENTE (LINQ) ---");

        // Agrupamos a lista pelo nome do cliente e contamos quantos itens existem em cada grupo
        var resumo = HistoricoDevolucoes
            .GroupBy(e => e.NomeCliente)
            .Select(g => new { Nome = g.Key, TotalLivros = g.Count() });

        foreach (var item in resumo)
        {
            Console.WriteLine($"Cliente: {item.Nome} | Livros Devolvidos: {item.TotalLivros}");
        }
    }
}

// OUVINTE 1: Lógica Financeira
class Financeiro
{
    public static void CalcularMulta(Emprestimo e)
    {
        // Calcula a diferença de dias entre as datas
        TimeSpan diferenca = e.DataEntregaReal - e.DataEsperada;
        int diasAtraso = diferenca.Days;

        // Regra: R$ 2,00 por dia
        decimal multa = diasAtraso * 2.00m;

        Console.WriteLine($"[FINANCEIRO] Livro com {diasAtraso} dias de atraso. Multa gerada: {multa:C2}");
    }
}

// OUVINTE 2: Geração de arquivo de texto
class Relatorio
{
    public static void RegistrarDevedor(Emprestimo e)
    {
        string linha = $"Devedor: {e.NomeCliente} | Livro: {e.TituloLivro} | Data: {DateTime.Now}\n";

        // Salva no arquivo 'devedores.txt'. Se o arquivo não existir, ele cria; se existir, adiciona no fim.
        File.AppendAllText("devedores.txt", linha);
        Console.WriteLine("[RELATÓRIO] Cliente adicionado à lista de devedores (txt).");
    }
}