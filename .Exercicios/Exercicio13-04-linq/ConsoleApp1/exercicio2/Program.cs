List<int> numeros = new List<int> { 5, 12, 8, 20, 3, 15, 7 };


// o .Max() é um método de extensão que retorna o maior valor de uma coleção. Ele percorre a lista e encontra o maior número presente.
int maiorNumero = numeros.Max();

// aqui usamos o .Where() para filtrar os números maiores que 10 e depois o .Sum() para somar apenas esses números filtrados. O resultado é a soma dos números que são maiores que 10.
int somaMaioresQue10 = numeros.Where(n => n > 10).Sum();

// exibindo os resultados
Console.WriteLine("-----------------------------------------------------------");
Console.WriteLine("Lista Original: " + string.Join(", ", numeros));
Console.WriteLine("-----------------------------------------------------------");
Console.WriteLine("Maior número encontrado: " + maiorNumero);
Console.WriteLine("Soma dos números (apenas os > 10): " + somaMaioresQue10);
Console.WriteLine("-----------------------------------------------------------");