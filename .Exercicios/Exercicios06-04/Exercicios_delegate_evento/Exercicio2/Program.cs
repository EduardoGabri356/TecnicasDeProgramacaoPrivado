using System;

namespace MonitoramentoTemperatura
{
    // O Delegate Define o "contrato" para quem quiser ouvir o alarme (precisa receber uma string)
    public delegate void AlarmeTemperaturaEventHandler(string mensagem);

    // A Classe que gera a informação
    public class ArCondicionado
    {
        // O Evento baseado no delegate
        public event AlarmeTemperaturaEventHandler AlarmeTemperatura;

        public double Temperatura { get; set; }
        public double LimiteSuperior { get; set; }
        public double LimiteInferior { get; set; }
        public ArCondicionado(double limiteInferior, double limiteSuperior, double temperaturaInicial)
        {
            LimiteInferior = limiteInferior;
            LimiteSuperior = limiteSuperior;
            Temperatura = temperaturaInicial;
        }

        // Método para alterar a temperatura e fazer a validação
        public void AjustarTemperatura(double novaTemperatura)
        {
            Temperatura = novaTemperatura;
            Console.WriteLine($"\n[Ar Condicionado] A temperatura atual é: {Temperatura}°C");

            // Validação dos limites
            if (Temperatura > LimiteSuperior)
            {
                // Chama o método interno para disparar o evento
                DispararAlarme($"PERIGO: Temperatura ({Temperatura}°C) ultrapassou o limite máximo de {LimiteSuperior}°C!");
            }
            else if (Temperatura < LimiteInferior)
            {
                // Chama o método interno para disparar o evento
                DispararAlarme($"PERIGO: Temperatura ({Temperatura}°C) caiu abaixo do limite mínimo de {LimiteInferior}°C!");
            }
        }

        // Método interno para disparar o evento de alarme
        private void DispararAlarme(string mensagem)
        {
            // O operador '?' garante que o evento só será disparado se houver alguém inscrito nele, evitando erros de referência nula.
            AlarmeTemperatura?.Invoke(mensagem);
        }
    }

    // 3. A Classe que escuta a informação 
    public class Monitor
    {
        // Este método tem a mesma assinatura do delegate criado lá em cima
        public void ReceberAlerta(string alerta)
        {
            // Muda a cor da letra para dar destaque ao alarme
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[PAINEL DO MONITOR] {alerta}");
            Console.ResetColor(); // Volta para a cor padrão do console
        }
    }

    // 4. Testando o Sistema
    class Program
    {
        static void Main()
        {
            Console.WriteLine("=== Sistema Inteligente de Monitoramento ===");

            // Cria o Ar Condicionado (Mínimo: 18°C, Máximo: 26°C, Atual: 22°C)
            ArCondicionado ar = new ArCondicionado(18.0, 26.0, 22.0);

            // Cria o Monitor
            Monitor centralDeSeguranca = new Monitor();

            // Inscrevemos o método 'ReceberAlerta' do Monitor no evento 'AlarmeTemperatura' do Ar
            ar.AlarmeTemperatura += centralDeSeguranca.ReceberAlerta;

            // Simulando o funcionamento
            ar.AjustarTemperatura(24.0); // Dentro do padrão
            ar.AjustarTemperatura(25.5); // Dentro do padrão
            ar.AjustarTemperatura(27.5); // Vai disparar o evento (Acima do limite)
            ar.AjustarTemperatura(20.0); // Dentro do padrão
            ar.AjustarTemperatura(15.0); // Vai disparar o evento (Abaixo do limite)

            Console.ReadLine();
        }
    }
}