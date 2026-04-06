using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

Console.WriteLine("--- Início da execução do sistema ---");

// Intanciando a fábrica, as máquinas e o operador
Fabrica minhaFabrica = new Fabrica { Nome = "Tech Industrial Solutions" };

Maquina m1 = new Maquina
{
    Nome = "Esteira Automatizada",
    Modelo = "ET-500",
    DataFabricacao = new DateTime(2022, 11, 20),
    HoraOperacao = "06:00 - 14:00"
};
m1.Observacao = "Revisão realizada recentemente.";

Maquina m2 = new Maquina
{
    Nome = "Cortadora a Laser",
    Modelo = "CL-900",
    DataFabricacao = new DateTime(2025, 02, 10),
    HoraOperacao = "14:00 - 22:00"
};

Operador op = new Operador { Nome = "Eduardo" };

// Executando os métodos da fábrica
minhaFabrica.AdicionarMaquina(m1);
minhaFabrica.AdicionarMaquina(m2);

Console.WriteLine("Lista de Máquinas:");
minhaFabrica.ListarMaquinas();

try
{
    await op.OperarMaquinaAsync(minhaFabrica, "ET-500");
    await op.OperarMaquinaAsync(minhaFabrica, "XP-999");
}
catch (MaquinaNaoEncontradaException ex)
{
    Console.WriteLine($"\n[ALERTA DE SISTEMA]: {ex.Message}");
}

Console.WriteLine("\n--- Fim da execução do sistema ---");

//Julgue meus metodos, nunca meus resultados.