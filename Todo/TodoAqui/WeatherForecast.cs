// Model simples que representa uma previsão do tempo
// Não usa MongoDB — é só uma classe C# pura
public class WeatherForecast
{
    public DateOnly Date { get; set; }      // data da previsão
    public int TemperatureC { get; set; }   // temperatura em Celsius
    public string? Summary { get; set; }    // descrição do clima ("Hot", "Cold"...)

    // Propriedade calculada: converte Celsius para Fahrenheit automaticamente
    // Não tem "set" — é só leitura, calculada na hora que alguém acessa
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}