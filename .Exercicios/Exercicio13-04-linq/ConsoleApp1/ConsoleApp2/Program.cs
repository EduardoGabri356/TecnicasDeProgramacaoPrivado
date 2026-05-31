List<Pessoa> pessoas = new List<Pessoa>
{
    new Pessoa { Nome = "João", Idade = 17 },
    new Pessoa { Nome = "Maria", Idade = 22 },
    new Pessoa { Nome = "Carlos", Idade = 30 }
};

// O retorno aqui é uma coleção de objetos do tipo Pessoa
IEnumerable<Pessoa> maioresDeIdade = pessoas.Where(p => p.Idade > 18);

Console.WriteLine("--- Pessoas com mais de 18 anos ---");
foreach (Pessoa p in maioresDeIdade)
{
    Console.WriteLine($"Nome: {p.Nome}, Idade: {p.Idade}");
}

Console.WriteLine("-------------------------------------------------");

// Primeiro ordenamos a lista original pelo Nome e depois selecionamos apenas a string Nome
IEnumerable<string> nomesOrdenados = pessoas
    // .orderBy(p => p.Nome) ordena a coleção de pessoas em ordem alfabética com base na propriedade Nome.
    // Ele compara os nomes e organiza os objetos Pessoa de acordo com a ordem alfabética dos seus nomes.
    .OrderBy(p => p.Nome)
    // .select(p => p.Nome) é um método de projeção que transforma cada objeto Pessoa em apenas a string do seu Nome.
    // Ele percorre a coleção de pessoas e extrai apenas o nome de cada pessoa, resultando em uma coleção de strings contendo os nomes ordenados.
    .Select(p => p.Nome);

Console.WriteLine("\n--- Nomes em ordem alfabética ---");
foreach (string nome in nomesOrdenados)
{
    Console.WriteLine(nome);
}
class Pessoa
{
    public string Nome { get; set; }
    public int Idade { get; set; }
}