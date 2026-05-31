using MongoDB.Driver;
using TelefoneApi.Models;

namespace TelefoneApi.Services
{
    public class TelefoneService
    {
        private readonly IMongoCollection<Telefone> _telefones;

        public TelefoneService(IConfiguration config)
        {
            var connectionString = config.GetConnectionString("MongoConnection");

            var client = new MongoClient(connectionString);

            var database = client.GetDatabase("TelefonesDatabase");

            _telefones = database.GetCollection<Telefone>("telefones");
        }

        public async Task<List<Telefone>> GetAsync() =>
            await _telefones.Find(_ => true).ToListAsync();

        public async Task<Telefone> CriarTelefone(Telefone novoTelefone)
        {
            await _telefones.InsertOneAsync(novoTelefone);
            return novoTelefone;
        }

        public async Task EditarTelefone(string id, Telefone telefoneAtualizado) =>
            await _telefones.ReplaceOneAsync(t => t.Id == id, telefoneAtualizado);

        public async Task DeletarTelefone(string id) =>
            await _telefones.DeleteOneAsync(t => t.Id == id);
    }
}