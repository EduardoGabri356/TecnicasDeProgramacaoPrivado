List<string> nomes = new List<string> { "Nardelli", "Doardo", "Surita", "Bruno" };

var resultado = from nome in nomes where nome.Contains("a") select nome;
foreach (var nome in resultado)
{
    Console.WriteLine(nome);
}