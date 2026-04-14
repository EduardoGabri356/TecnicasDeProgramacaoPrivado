using System;

namespace SistemaContagemCliques
{
    // A Classe Publicadora
    public class Botao
    {
        // Declaração do Delegate sem parâmetros
        public delegate void CliqueEventHandler();

        // Declaração do Evento baseado no delegate acima
        public event CliqueEventHandler Clique;

        // Método que simula a ação física de clicar no botão
        public void SimularClique()
        {
            // O operador '?' garante que o evento só disparará se houver alguém inscrito
            // invoke chama o método registrado no evento, ou seja, o método do contador de cliques. 
            Clique?.Invoke();
        }
    }

    // A Classe Assinante
    public class ContadorCliques
    {
        // Variável interna para armazenar o estado da contagem
        private int _quantidadeCliques = 0;

        // Método que será executado quando o evento ocorrer (deve ter a mesma assinatura do delegate: void e sem parâmetros)
        public void RegistrarClique()
        {
            _quantidadeCliques++;
            Console.WriteLine($"Botão clicado! Total de cliques: {_quantidadeCliques}");
        }
    }

    // Testando a Aplicação
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== Sistema de Contagem de Cliques ===\n");

            // Instanciando os objetos
            Botao meuBotao = new Botao();
            ContadorCliques meuContador = new ContadorCliques();

            // Inscrevendo o método do contador no evento do botão
            meuBotao.Clique += meuContador.RegistrarClique;

            // Simulando os cliques
            Console.WriteLine("Simulando cliques no botão");

            meuBotao.SimularClique(); // Clique 1
            meuBotao.SimularClique(); // Clique 2
            meuBotao.SimularClique(); // Clique 3

            Console.WriteLine("\nPausa");

            meuBotao.SimularClique(); // Clique 4
            meuBotao.SimularClique(); // Clique 5

            Console.ReadLine();
        }
    }
}