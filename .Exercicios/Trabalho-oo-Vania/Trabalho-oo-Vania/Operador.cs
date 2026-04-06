public class Operador
{
    public Operador(string nome, Maquina maquinaOperador)
    {
        Nome = nome;
        MaquinaOperador = maquinaOperador;
    }
    public string? Nome { get; set; }

    public Maquina? MaquinaOperador { get; set; }

    public async Task OperarMaquinaAsync(Fabrica fabrica, string modelo)
    {
        Console.WriteLine($"\n{Nome} está tentando operar a máquina modelo {modelo}...");
        await Task.Delay(2000);

        var maquina = fabrica.BuscarMaquinaPorModelo(modelo);

        if (maquina == null)
        {
            Console.WriteLine($"maquina modelo {modelo} não encontrada na Fabrica {Nome}");
        }
        else 
        {
            Console.WriteLine($"{Nome} agora esta operando a maquina modelo {maquina.Modelo}");
            await Task.Delay(3000);
            Console.WriteLine($"Operação da {maquina.Modelo} finalizada com sucesso!");
        }
    }
}
public class MaquinaNaoEncontradaException : Exception
{
    public MaquinaNaoEncontradaException(string message) : base(message) { }
}
