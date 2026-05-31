namespace ExemploDelegateInterativo
{
    class Program
    {
        // todo método que chama Operacao, PRECISA ter dois parâmetros do tipo
        // double e retornar um double para ser compatível com o delegate, caso contrário, o código não compila.
        public delegate double Operacao(double a, double b);

        static void Main()
        {
            double num1 = 0;
            double num2 = 0;

            Console.WriteLine("=== Calculadora com Delegates ===");

            // Capturando e validando o primeiro número
            Console.Write("Digite o primeiro número: ");
            // enquanto a conversão falhar, o método while junto com o comando "!double.TryParse"
            // continua pedindo uma entrada válida, "out num1" é a variável onde o valor convertido
            // será armazenado caso a conversão seja bem-sucedida.
            while (!double.TryParse(Console.ReadLine(), out num1))
            {
                Console.Write("Entrada inválida. Por favor, digite um número válido: ");
            }

            // Capturando e validando o segundo número
            Console.Write("Digite o segundo número: ");
            while (!double.TryParse(Console.ReadLine(), out num2))
            {
                Console.Write("Entrada inválida. Por favor, digite um número válido: ");
            }

            Console.WriteLine($"\nCalculando as operações para os valores: {num1} e {num2}...\n");

            // Utilizando o delegate para as operações
            Operacao minhaOperacao;

            minhaOperacao = Somar;
            Console.WriteLine($"Soma:          {minhaOperacao(num1, num2)}");

            minhaOperacao = Subtrair;
            Console.WriteLine($"Subtração:     {minhaOperacao(num1, num2)}");

            minhaOperacao = Multiplicar;
            Console.WriteLine($"Multiplicação: {minhaOperacao(num1, num2)}");

            minhaOperacao = Dividir;
            Console.WriteLine($"Divisão:       {minhaOperacao(num1, num2)}");
        }

        // Métodos de operação que correspondem à assinatura do delegate "Operacao". Cada método realiza uma operação matemática diferente.

        static double Somar(double a, double b) => a + b;

        static double Subtrair(double a, double b) => a - b;

        static double Multiplicar(double a, double b) => a * b;

        static double Dividir(double a, double b)
        {
            // Verificação para evitar divisão por zero 
            if (b == 0)
            {
                Console.WriteLine("Erro: Não é possível dividir por zero.");
                return 0;
            }
            return a / b;
        }
    }
}