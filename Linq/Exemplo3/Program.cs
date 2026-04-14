var ListaProdutos = Produto.GetProdutos();

Console.WriteLine("Produtos eletrônicos");

var produtosEletronicos = ListaProdutos.Where(p => p.Categoria == "Eletronico");
Produto.Mostrar(produtosEletronicos);

Console.WriteLine("----------------------------------------------------------------------------------------------");

Console.WriteLine("\nProdutos com preço acima de R$ 1000 e estoque maior que 5:");
var ProdutosCaros = ListaProdutos.Where(p => p.Preco >= 1000 && p.Estoque > 5);
Produto.Mostrar(ProdutosCaros);

Console.WriteLine("----------------------------------------------------------------------------------------------");

Console.WriteLine("\nProdutos com estoque menor que 15, ordenados pelo nome");
var ProdutosBaixoEstoque = ListaProdutos.Where(p => p.Estoque < 15).OrderBy(p => p.Nome);
Produto.Mostrar(ProdutosBaixoEstoque);

Console.WriteLine("----------------------------------------------------------------------------------------------");

Console.WriteLine("\nProdutos Ordenados por categoria e nome");
var ProdutosOrdenados= ListaProdutos.OrderBy(p => p.Categoria ).ThenBy(p => p.Nome);
Produto.Mostrar(ProdutosOrdenados);

Console.WriteLine("----------------------------------------------------------------------------------------------");

//Console.WriteLine("\nFiltrando Produtos menor que R$500,00 com aumento de 10%, ordenado por nome criando um tipo anônimo");
//var ProdutosOrdenados2 = ListaProdutos.OrderBy(p => p.Preco < 500).OrderBy(p => p.Nome).Select
//    (p => new { nomeProduto = p.Nome.ToUpper(), precoComAumento = p.Preco * 1.1, Id = p.Id, Estoque = p.Estoque, Categoria = p.Categoria });
//Produto.Mostrar(ProdutosOrdenados2);

var ProdutosOrdenados2 = ListaProdutos
    .Where(p => p.Preco < 500) // Primeiro filtramos
    .OrderBy(p => p.Nome)
    .Select(p => new { nomeProduto = p.Nome.ToUpper(), precoModificado = p.Preco * 1.1 });

// Chamando apenas .Mostrar, o C# usará a sobrecarga 'dynamic'
Produto.Mostrar(ProdutosOrdenados2);

Console.WriteLine("----------------------------------------------------------------------------------------------");

Console.WriteLine("\nValor Médio do preço dos eletronicos");
double media = ListaProdutos.Where(p => p.Categoria == "Eletronico").Average(p => p.Preco);
Console.WriteLine($"Media dos preços dos eletronicos: R$ {media:F2}");

Console.WriteLine("----------------------------------------------------------------------------------------------");

Console.WriteLine("\nSelecionar produtos com preços maior que R$200 com desconto de 20% ordenado por preço, criando um tipo anônimo");
var ProdutosOrdenados3 = ListaProdutos
    .Where(p => p.Preco > 200)
    .OrderBy(p => p.Preco)
    .Select(p => new { nomeProduto = p.Nome.ToUpper(), precoModificado = p.Preco * 0.8 });

Produto.Mostrar(ProdutosOrdenados3);

Console.WriteLine("----------------------------------------------------------------------------------------------");

Console.ReadKey();

public class Produto
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public double Preco { get; set; }
    public int Estoque { get; set; }
    public string? Categoria { get; set; }

    public static List<Produto> GetProdutos()
    {
        // Sintaxe de inicialização de coleção corrigida
        return new List<Produto>
        {
            new Produto { Id = 1, Nome = "Notebook", Preco = 3990.00, Estoque = 10, Categoria = "Eletronico" },
            new Produto { Id = 2, Nome = "Mouse", Preco = 179.90, Estoque = 30, Categoria = "Eletronico" },
            new Produto { Id = 3, Nome = "Teclado", Preco = 269.90, Estoque = 25, Categoria = "Eletronico" },
            new Produto { Id = 4, Nome = "Mesa", Preco = 550.00, Estoque = 7, Categoria = "Moveis" },
            new Produto { Id = 5, Nome = "Mouse Pad", Preco = 79.90, Estoque = 50, Categoria = "Utilitario" },
            new Produto { Id = 6, Nome = "Fone Bluetooth", Preco = 349.90, Estoque = 15, Categoria = "Eletronico" },
            new Produto { Id = 7, Nome = "Suporte Notebook", Preco = 139.90, Estoque = 38, Categoria = "Utilitario" },
            new Produto { Id = 8, Nome = "Cadeira ergonomica", Preco = 479.90, Estoque = 7, Categoria = "Moveis" },
            new Produto { Id = 9, Nome = "Monitor", Preco = 349.90, Estoque = 22, Categoria = "Eletronico" },
            new Produto { Id = 10, Nome = "Nicho", Preco = 99.90, Estoque = 60, Categoria = "Moveis" },
            new Produto { Id = 11, Nome = "Carregador Notebook", Preco = 139.90, Estoque = 0, Categoria = "Utilitario" }
        };
    }

    // Sobrecarga para a classe Produto
    public static void Mostrar(IEnumerable<Produto> produtos)
    {
        foreach (var item in produtos)
        {
            Console.WriteLine($"ID: {item.Id} | Nome: {item.Nome} | Categoria: {item.Categoria} | Preço: {item.Preco:C}");
        }
    }

    // Sobrecarga para tipos anônimos (usa dynamic)
    public static void Mostrar(IEnumerable<dynamic> produtos)
    {
        foreach (var item in produtos)
        {
            Console.WriteLine($"Nome: {item.nomeProduto}, Preço: {item.precoModificado:C}");
        }
    }
}