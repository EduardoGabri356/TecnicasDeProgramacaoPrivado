using MongoDB.Driver;
using TarefasApi.Models;

namespace TarefasApi.Services
{
    public class TarefaService
    {
        // Atributo para acessar a coleção de tarefas no MongoDB, IMongoCollection é como uma "tabela" no banco relacional
        private readonly IMongoCollection<Tarefa> _tarefas;

        // Construtor que recebe a configuração para conectar ao MongoDB no appsettings.json
        public TarefaService(IConfiguration config)
        {
            // Lê a string de conexão do MongoDB a partir do arquivo de configuração, "MongoConnection" é o nome da chave no appsettings.json
            var connectionString = config.GetConnectionString("MongoConnection");

            // Cria um cliente MongoDB usando a string de conexão
            var client = new MongoClient(connectionString);

            // Acessa o banco de dados "TarefasDatabase" no MongoDB, se não existir, ele será criado automaticamente quando a primeira tarefa for inserida
            var database = client.GetDatabase("TarefasDatabase");

            // Acessa a coleção "tarefas" dentro do banco de dados, se não existir, ela será criada automaticamente quando a primeira tarefa for inserida
            _tarefas = database.GetCollection<Tarefa>("tarefas");
        }

        // cria um método assíncrono, Task representa uma operação assíncrona, <List<Tarefa>> é o tipo de dado que será retornado quando a operação for concluída
        // GetAsync é o nome do método, () indica que ele não recebe parâmetros
        public async Task<List<Tarefa>> GetAsync() =>
            // await é usado para esperar a conclusão de uma operação assíncrona, _tarefas.Find(_ => true) é uma consulta que retorna todas as tarefas na coleção
            // ToListAsync() converte o resultado da consulta em uma lista de tarefas
            await _tarefas.Find(_ => true).ToListAsync();

        // Cria um método assíncrono para criar uma nova tarefa, recebe um objeto Tarefa como parâmetro
        public async Task<Tarefa> CreateTaskAsync(Tarefa novaTarefa) // CreateTaskAsync é o nome do método
        {
            // Insere a tarefa na coleção 
            await _tarefas.InsertOneAsync(novaTarefa);
            // retorna a tarefa criada
            return novaTarefa;
        }

        // busca uma tarefa pelo Id, recebe o Id como parâmetro e retorna a tarefa correspondente
        public async Task TaskUpdateAsync(string id, Tarefa tarefaAtualizada) =>
            // Substitui a tarefa com o Id correspondente pela tarefa atualizada, ReplaceOneAsync é usado para substituir o documento inteiro
            await _tarefas.ReplaceOneAsync(t => t.Id == id, tarefaAtualizada); // t => t.Id == id é a condição para encontrar a tarefa a ser atualizada, tarefaAtualizada é o novo documento que substituirá o antigo

        // Remove uma tarefa pelo Id
        // semelhante ao método de atualização, recebe o Id da tarefa a ser removida e usa DeleteOneAsync para remover o documento correspondente da coleção
        public async Task RemoveAsync(string id) =>
            await _tarefas.DeleteOneAsync(t => t.Id == id); // t => t.Id == id é a condição para encontrar a tarefa a ser removida
    }
}
