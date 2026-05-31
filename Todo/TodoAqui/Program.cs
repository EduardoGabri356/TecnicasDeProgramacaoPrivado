using TodoAqui.Repositories;

// Cria o builder — ponto de partida de toda aplicação ASP.NET Core
var builder = WebApplication.CreateBuilder(args);

// Registra os Controllers para que o framework encontre os endpoints
builder.Services.AddControllers();

// Registra o OpenAPI (documentação automática da API)
builder.Services.AddOpenApi();

// Registra o TarefaRepository como Singleton:
// Singleton = uma única instância compartilhada por toda a aplicação
// (Scoped = uma instância por requisição | Transient = nova instância sempre)
builder.Services.AddSingleton<TarefaRepository>();

// Habilita o Swagger — interface visual para testar a API no navegador
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configura o CORS para permitir o frontend acessar a API
// Sem isso o navegador bloqueia as requisições por segurança
builder.Services.AddCors(options =>
{
    options.AddPolicy("MinhaPoliticaCORS", policy =>
    {
        // Só permite requisições vindas do frontend (localhost:5173 = Vite dev server)
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()  // aceita qualquer header HTTP
              .AllowAnyMethod(); // aceita GET, POST, PUT, DELETE etc.
    });
});

// Constrói a aplicação com todas as configurações acima
var app = builder.Build();

// Ativa a política CORS definida acima
app.UseCors("MinhaPoliticaCORS");

// Em modo Development, exibe o Swagger e o OpenAPI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

// Ativa a verificação de autorização (autenticação)
app.UseAuthorization();

// Mapeia as rotas dos Controllers automaticamente
app.MapControllers();

// Inicia o servidor web — a aplicação começa a ouvir requisições
app.Run();