using TarefasApi.Services;
// instalar o Swashbuckle.AspNetCore versão 9.0.6

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// vamo ter que adicionar isso aqui no program:
// Adiciona o serviço TarefaService como um singleton, ou seja, uma única instância será criada e compartilhada em toda a aplicação
builder.Services.AddSingleton<TarefaService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configura o CORS para permitir o frontend acessar a API
// Sem isso o navegador bloqueia as requisições por segurança
builder.Services.AddCors(options =>
{
    // Define uma política de CORS chamada "MinhaPoliticaCORS"
    options.AddPolicy("MinhaPoliticaCORS", policy =>
    {
        // Só permite requisições vindas do frontend (localhost:5173 = Vite dev server)
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader() // permite qualquer header HTTP, como Content-Type, Authorization, etc.
              .AllowAnyMethod(); // permite qualquer método HTTP, como GET, POST, PUT, DELETE, etc.
    });
});

var app = builder.Build();

// tem que colocar isso aqui depois de criar a política CORS e depois de builder.build, senão ela não vai funcionar
app.UseCors("MinhaPoliticaCORS");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Habilita o Swagger apenas em ambiente de desenvolvimento, para facilitar a documentação e testes da API
    // ambos os métodos, UseSwagger e UseSwaggerUI, são adicionados manualmente
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

/*
 Deleta isso aqui, não precisamos disso porque estamos usando o Swagger para testar a API, e o Swagger roda em HTTP:

 app.UseHttpsRedirection();
*/

app.UseAuthorization();

app.MapControllers();

app.Run();
