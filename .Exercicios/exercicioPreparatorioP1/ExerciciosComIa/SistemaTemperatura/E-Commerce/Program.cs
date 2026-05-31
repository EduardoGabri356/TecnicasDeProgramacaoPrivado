using System;
using System.Collections.Generic;
using System.Linq;

// ==============================================================================
// 1. BLOCO DE EXECUÇÃO (TOP-LEVEL STATEMENTS)
// ==============================================================================

Console.WriteLine("--- SISTEMA DE CHECKOUT E-COMMERCE ---\n");

// PASSO 1: Criando dados para teste (Associação e Composição)
Cliente cliente = new Cliente { Nome = "Carlos Oliveira", Email = "carlos@email.com" };

List<Produto> itensNoCarrinho = new List<Produto>
{
    new Produto { Nome = "Mouse Gamer", Categoria = "Eletrônicos", Preco = 150.00m, Estoque = 10, QtdDesejada = 2 },
    new Produto { Nome = "Teclado Mecânico", Categoria = "Eletrônicos", Preco = 350.00m, Estoque = 5, QtdDesejada = 1 },
    new Produto { Nome = "Cadeira Office", Categoria = "Móveis", Preco = 800.00m, Estoque = 2, QtdDesejada = 3 } // VAI DAR ERRO (Estoque = 2, deseja 3)
};

Pedido pedido = new Pedido(cliente, itensNoCarrinho);

// PASSO 2: Registrar Ouvinte do Evento
pedido.PedidoConcluido += (p) => {
    Console.WriteLine($"[NOTIFICAÇÃO] E-mail de confirmação enviado para {p.ClientePedido.Email}!");
};

// PASSO 3: Tentar processar o pedido com tratamento de erro
try
{
    // Aplicando LINQ antes de finalizar para ver quanto vai custar
    decimal total = itensNoCarrinho.Sum(p => p.Preco * p.QtdDesejada);
    Console.WriteLine($"Total do Carrinho: {total:C2}");

    pedido.FinalizarCheckout();
}
catch (EstoqueBaixoException ex)
{
    // Captura a nossa exceção personalizada
    Console.WriteLine($"[ERRO DE ESTOQUE] {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"[ERRO INESPERADO] {ex.Message}");
}

// PASSO 4: Consulta LINQ - Mostrar apenas Eletrônicos no carrinho[cite: 1]
Console.WriteLine("\n--- ITENS DA CATEGORIA ELETRÔNICOS (LINQ) ---");
var eletronicos = itensNoCarrinho.Where(p => p.Categoria == "Eletrônicos");

foreach (var e in eletronicos)
{
    Console.WriteLine($"- {e.Nome} | Preço: {e.Preco:C2}");
}

Console.WriteLine("\nFim do processamento.");

// ==============================================================================
// 2. DEFINIÇÕES DE TIPOS (CLASSES, INTERFACES E EXCEÇÕES)
// ==============================================================================

// EXCEÇÃO PERSONALIZADA[cite: 1]
public class EstoqueBaixoException : Exception
{
    public EstoqueBaixoException(string mensagem) : base(mensagem) { }
}

// CLASSES DE DADOS (Modelo)[cite: 1]
public class Cliente
{
    public string Nome { get; set; }
    public string Email { get; set; }
}

public class Produto
{
    public string Nome { get; set; }
    public string Categoria { get; set; }
    public decimal Preco { get; set; }
    public int Estoque { get; set; }
    public int QtdDesejada { get; set; }
}

// DELEGATE PARA O EVENTO[cite: 1]
public delegate void PedidoHandler(Pedido p);

// CLASSE PRINCIPAL (Lógica de Negócio)[cite: 1]
public class Pedido
{
    // ASSOCIAÇÃO: O pedido "tem um" cliente e "tem vários" produtos[cite: 1]
    public Cliente ClientePedido { get; set; }
    public List<Produto> Produtos { get; set; }

    public event PedidoHandler PedidoConcluido;

    public Pedido(Cliente c, List<Produto> p)
    {
        ClientePedido = c;
        Produtos = p;
    }

    public void FinalizarCheckout()
    {
        Console.WriteLine("\nValidando estoque...");

        foreach (var p in Produtos)
        {
            // Verificação de estoque[cite: 1]
            if (p.QtdDesejada > p.Estoque)
            {
                // Dispara a exceção personalizada se faltar produto[cite: 1]
                throw new EstoqueBaixoException($"Produto '{p.Nome}' não possui estoque suficiente (Disponível: {p.Estoque}).");
            }
        }

        Console.WriteLine("Pagamento aprovado!");

        // Dispara o evento de conclusão[cite: 1]
        PedidoConcluido?.Invoke(this);
    }
}