// --- INSTANCIAÇÃO DE DADOS ---
// Produtos -------------------------------------------------------------------
Produto p1 = new Produto { Nome = "Notebook Pro", Preco = 4500.00 };
Produto p2 = new Produto { Nome = "Mouse Gamer", Preco = 250.00 };
Produto p3 = new Produto { Nome = "Teclado Mecânico", Preco = 400.00 };
Produto p4 = new Produto { Nome = "Monitor 4K", Preco = 1800.00 };
Produto p5 = new Produto { Nome = "Cadeira Gamer", Preco = 1200.00 };
Produto p6 = new Produto { Nome = "Headset USB", Preco = 350.00 };
Produto p7 = new Produto { Nome = "Webcam HD", Preco = 280.00 };
Produto p8 = new Produto { Nome = "Suporte Articulado", Preco = 190.00 };

// Clientes -------------------------------------------------------------------
Cliente c1 = new Cliente { Nome = "André Silva", Cpf = "123.456.789-00" };
Cliente c2 = new Cliente { Nome = "Beatriz Oliveira", Cpf = "987.654.321-11" };
Cliente c3 = new Cliente { Nome = "Carlos Andrade", Cpf = "456.123.789-22" };
Cliente c4 = new Cliente { Nome = "Mariana Souza", Cpf = "321.654.987-33" };


// Pedidos -------------------------------------------------------------------
List<Pedido> pedidos = new List<Pedido>
{
    // Pedidos do André (Total alto)
    new Pedido { Produto = p1, Cliente = c1, Quantidade = 1, DataPedido = DateTime.Now },
    new Pedido { Produto = p2, Cliente = c1, Quantidade = 2, DataPedido = DateTime.Now },

    // Pedidos da Beatriz (Total médio)
    new Pedido { Produto = p4, Cliente = c2, Quantidade = 1, DataPedido = DateTime.Now },
    new Pedido { Produto = p8, Cliente = c2, Quantidade = 1, DataPedido = DateTime.Now },

    // Pedidos do Carlos (Total baixo e alto misturado)
    new Pedido { Produto = p3, Cliente = c3, Quantidade = 1, DataPedido = DateTime.Now },
    new Pedido { Produto = p7, Cliente = c3, Quantidade = 1, DataPedido = DateTime.Now },
    new Pedido { Produto = p5, Cliente = c3, Quantidade = 1, DataPedido = DateTime.Now },

    // Pedidos da Mariana (Vários itens pequenos)
    new Pedido { Produto = p2, Cliente = c4, Quantidade = 1, DataPedido = DateTime.Now },
    new Pedido { Produto = p6, Cliente = c4, Quantidade = 1, DataPedido = DateTime.Now },
    new Pedido { Produto = p8, Cliente = c4, Quantidade = 2, DataPedido = DateTime.Now }
};

// --- RESOLUÇÃO DOS ITENS ---

// a. Mostre todos os pedidos agrupados por cliente.
Console.WriteLine("=== A. PEDIDOS AGRUPADOS POR CLIENTE ===");
IEnumerable<IGrouping<Cliente, Pedido>> agrupados = pedidos.GroupBy(p => p.Cliente);

foreach (IGrouping<Cliente, Pedido> grupo in agrupados)
{
    Console.WriteLine($"\nCliente: {grupo.Key.Nome}"); // Key é o objeto Cliente usado no GroupBy
    foreach (Pedido pedido in grupo)
    {
        Console.WriteLine($"- {pedido.Produto.Nome} | Qtd: {pedido.Quantidade} | Data: {pedido.DataPedido:dd/MM/yyyy}");
    }
}

Console.WriteLine("\n" + new string('-', 60));

// b. Trazer os nomes dos clientes que têm pedidos acima de R$ 500
Console.WriteLine("\n=== B. CLIENTES COM PEDIDOS ACIMA DE R$ 500 ===");
IEnumerable<string> clientesVip = pedidos
    .Where(p => (p.Quantidade * p.Produto.Preco) > 500)
    .Select(p => p.Cliente.Nome)
    .Distinct(); // Garante que o nome não apareça repetido

foreach (string nome in clientesVip)
{
    Console.WriteLine($"Cliente: {nome}");
}

Console.WriteLine("\n" + new string('-', 60));

// c. Calcular o valor total de pedidos por cliente
Console.WriteLine("\n=== C. VALOR TOTAL DE PEDIDOS POR CLIENTE ===");
var totalPorCliente = pedidos.GroupBy(p => p.Cliente)
    .Select(g => new {
        NomeCliente = g.Key.Nome,
        TotalGeral = g.Sum(p => p.Quantidade * p.Produto.Preco)
    });

foreach (var resumo in totalPorCliente)
{
    Console.WriteLine($"Cliente: {resumo.NomeCliente} | Total Gasto: {resumo.TotalGeral:C2}");
}

Console.ReadKey();

// --- DEFINIÇÃO DAS CLASSES ---

class Produto
{
    public string Nome { get; set; }
    public double Preco { get; set; }
}

class Cliente
{
    public string Nome { get; set; }
    public string Cpf { get; set; }
}

class Pedido
{
    public int Quantidade { get; set; }
    public DateTime DataPedido { get; set; }
    public Produto Produto { get; set; }
    public Cliente Cliente { get; set; }
}