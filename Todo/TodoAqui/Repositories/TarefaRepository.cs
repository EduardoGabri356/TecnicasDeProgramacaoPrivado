using MongoDB.Driver;
using TodoAqui.Models;

namespace TodoAqui.Repositories
{
    public class TarefaRepository
    {
        // Campo privado que representa a coleção "Tarefas" no MongoDB
        // IMongoCollection é como uma "tabela" no banco relacional
        private readonly IMongoCollection<Tarefa> _tarefasCollection;

        // O construtor recebe IConfiguration para ler o appsettings.json
        public TarefaRepository(IConfiguration configuration)
        {
            // Lê a string de conexão do arquivo appsettings.json
            var connectionString = configuration.GetConnectionString("MongoConnection");

            // ⚠️ BUG: no appsettings.json está "localhost27017" sem os dois-pontos ":"
            // Correto: "mongodb://localhost:27017"
            var client = new MongoClient(connectionString);

            // Seleciona o banco de dados chamado "TodoDatabase"
            var database = client.GetDatabase("TodoDatabase");

            // Seleciona (ou cria automaticamente) a coleção "Tarefas"
            _tarefasCollection = database.GetCollection<Tarefa>("Tarefas");
        }

        // Busca TODAS as tarefas do banco de forma assíncrona
        // "_ => true" é um filtro que aceita qualquer documento (equivale a SELECT *)
        public async Task<List<Tarefa>> ObterTodasTarefasAsync() =>
            await _tarefasCollection.Find(_ => true).ToListAsync();

        // Insere uma nova tarefa no banco
        // InsertOneAsync adiciona um único documento à coleção
        public async Task CriarTarefaAsync(Tarefa novaTarefa) =>
            await _tarefasCollection.InsertOneAsync(novaTarefa);

        // Funções extras, DELETAR e ATUALIZAR
        // Deleta uma tarefa pelo ID
        public async Task DeletarTarefaAsync(string id) =>
            await _tarefasCollection.DeleteOneAsync(t => t.Id == id);

        // Atualiza uma tarefa existente pelo ID
        // ReplaceOneAsync substitui o documento inteiro pelo novo
        public async Task AtualizarTarefaAsync(string id, Tarefa tarefaAtualizada) =>
            await _tarefasCollection.ReplaceOneAsync(t => t.Id == id, tarefaAtualizada);
    }
}