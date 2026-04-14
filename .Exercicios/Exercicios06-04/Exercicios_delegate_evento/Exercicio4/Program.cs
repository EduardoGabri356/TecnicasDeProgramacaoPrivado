using System;

namespace SistemaGerenciamentoEstoque
{
    // Classe Publicadora
    public class Estoque
    {
        public string NomeProduto { get; private set; }
        public int Quantidade { get; private set; }
        public int LimiteMinimo { get; private set; }

        // Evento usando o delegate pré-definido Action, que é uma forma mais simples de declarar eventos sem a necessidade de criar um delegate personalizado.
        // <string, int> define que o evento enviará o nome do produto e a quantidade
        public event Action<string, int> EstoqueBaixo;
        // Construtor para inicializar o estoque do produto
        public Estoque(string nome, int quantidadeInicial, int limiteMinimo = 5)
        {
            NomeProduto = nome;
            Quantidade = quantidadeInicial;
            LimiteMinimo = limiteMinimo;
        }

        // Método para simular a venda do produto, reduzindo a quantidade em estoque
        public void BaixarEstoque(int quantidadeVendida)
        {
            Quantidade -= quantidadeVendida;
            Console.WriteLine($"Venda realizada: {quantidadeVendida} unid. de {NomeProduto}. Estoque atual: {Quantidade}");

            // Verifica se atingiu o limite crítico
            if (Quantidade < LimiteMinimo)
            {
                // Dispara o evento para os assinantes, EstoqueBaixo é o evento, e Invoke é o método que chama os métodos inscritos no evento,
                // passando o nome do produto e a quantidade atual como argumentos.
                EstoqueBaixo?.Invoke(NomeProduto, Quantidade);
            }
        }
    }

    // Classe Assinante
    public class SetorCompras
    {
        public void GerarAlertaReposicao(string produto, int quantidadeAtual)
        {;
            Console.WriteLine($"\n !!! ALERTA DE ESTOQUE CRÍTICO: {produto} !!! ");
            Console.WriteLine($"Quantidade atual ({quantidadeAtual}) abaixo do limite permitido. Solicitar reposição imediata.\n");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Criando um produto (ex: Refrigerante) com 10 unidades e limite de 5
            Estoque itemEstoque = new Estoque("Refrigerante Lata", 10, 5);
            SetorCompras compras = new SetorCompras();

            // Inscrevendo o método no evento (assinatura)
            itemEstoque.EstoqueBaixo += compras.GerarAlertaReposicao;

            // Simulando movimentações
            itemEstoque.BaixarEstoque(2); // Vai para 8
            itemEstoque.BaixarEstoque(4); // Vai para 4 (Dispara o Alerta!)
        }
    }
}