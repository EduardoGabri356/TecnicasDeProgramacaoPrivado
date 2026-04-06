ContaBancaria conta = new ContaBancaria(300.00m);
try
{
    conta.Sacar("100");
}
catch(SaldoInsuficienteException ex)
{
    Console.WriteLine($"Erro de saldo: {ex.Message}");
}

try
{
    conta.Sacar("500");
}
catch(SaldoInsuficienteException ex)
{
    Console.WriteLine($"Erro de saldo: {ex.Message}");
}

try
{
    conta.Sacar("dabnfafbnwa");
}
catch(SaldoInsuficienteException ex)
{
    Console.WriteLine(ex.Message);
}

class ContaBancaria
{
    public decimal Saldo { get; private set; }
    public ContaBancaria(decimal saldoInicial)
    {
        Saldo = saldoInicial;
    }

    public void Sacar(string valorTexto)
    {
        try
        {
            decimal valor = decimal.Parse(valorTexto);
            if (valor > Saldo)
            {
                throw new SaldoInsuficienteException($"Saldo insuficiente. Saldo atual: R${Saldo} Tentativa de saque no valor de R${valor}");
            }
            Saldo -= valor;
            Console.WriteLine($"saldo Atual: R${Saldo}");
        }
        catch ( FormatException fe)
        {
            throw new ApplicationException("Erro ao converter o valor do saque", fe);
        }
        finally
        {
            Console.WriteLine("Fim do saque");
        }
    }// Fim do método Sacar
} // Fim da classe

public class SaldoInsuficienteException : Exception
{
    public SaldoInsuficienteException() { }
    public SaldoInsuficienteException(string message) : base(message) { }
    public SaldoInsuficienteException(string message, Exception innerException) : base(message, innerException) { }
}