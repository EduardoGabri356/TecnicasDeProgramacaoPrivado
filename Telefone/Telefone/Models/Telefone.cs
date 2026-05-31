using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TelefoneApi.Models
{
    public class Telefone
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("nome")]
        public string Nome { get; set; } = string.Empty;

        [BsonElement("telefone")]
        public int NumeroTelefone { get; set; }

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;
    }
}
