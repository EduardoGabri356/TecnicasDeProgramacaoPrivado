using System.Linq.Expressions;

try{
    //Classe dividendo
    Console.WriteLine("Digite o Dividendo");
    int dividendo = Convert.ToInt32(Console.ReadLine());

    //Classe divisor
    Console.WriteLine("Digite o Divisor");
    int divisor = Convert.ToInt32(Console.ReadLine());

    //calculando Ambos
    var resultado = dividendo / divisor;
// Saida
Console.WriteLine($"o resultado de {dividendo} / {divisor} = {resultado}");
}
// Exeção para numeros inteiros
catch(FormatException){
    Console.WriteLine("Entre com o valor inteiro");
}

// Execção generica para numeros inteiros
catch (Exception ex) when (ex.Message.Contains("Format")){
    Console.WriteLine("Entre com o valor inteiro");
}

// Execção caso o divisor for 0
catch (DivideByZeroException){
    Console.WriteLine("Divisor não pode ser 0");
}

// Qualquer exeção que possa ocorrer
catch (Exception ex){
    Console.WriteLine($"Prolema na divisão {ex.Message}");
}

// Auto explicativo
finally{
    Console.WriteLine("Acabou a Divisão");
}