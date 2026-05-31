List<int> numeros = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

int somaDosImpares = numeros.SomarImpares();

Console.WriteLine("-------------------------------------------------");
Console.WriteLine($"Lista original: {string.Join(", ", numeros)}");
Console.WriteLine($"Resultado da soma dos ímpares: {somaDosImpares}");
Console.WriteLine("-------------------------------------------------");

// --- DEFINIÇÃO DO MÉTODO DE EXTENSÃO ---

public static class ListExtensions
{
    public static int SomarImpares(this List<int> lista)
    {
        // p % 2 != 0 identifica números onde o resto da divisão por 2 não é zero (ímpares)
        return lista.Where(p => p % 2 != 0).Sum();
    }
}