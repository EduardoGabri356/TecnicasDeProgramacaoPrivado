using Microsoft.AspNetCore.Mvc;

namespace TodoAqui.Controllers
{
    // Igual ao TarefasController: [ApiController] + [Route] definem o endpoint
    // Aqui a rota é só "[controller]" = /WeatherForecast (sem o prefixo "api/")
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        // Array estático com os possíveis resumos do clima
        // "static readonly" = existe uma única cópia, compartilhada por todas as instâncias
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild",
            "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        // ILogger injeta o sistema de logs do ASP.NET
        // Permite registrar mensagens de debug, info, erro etc.
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger; // armazena o logger para uso futuro
        }

        // GET /WeatherForecast — retorna 5 previsões do tempo aleatórias
        // Diferente do TarefasController: esse não é async pois não acessa banco
        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            // Enumerable.Range(1, 5) gera os números [1, 2, 3, 4, 5]
            // .Select() transforma cada número em um objeto WeatherForecast
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                // DateTime.Now.AddDays(index) = hoje + 1 dia, hoje + 2 dias etc.
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),

                // Random.Shared.Next(-20, 55) gera um número aleatório entre -20 e 54
                TemperatureC = Random.Shared.Next(-20, 55),

                // Pega um resumo aleatório do array Summaries
                // Random.Shared.Next(Summaries.Length) = índice entre 0 e 9
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray(); // converte o resultado para array e retorna
        }
    }
}