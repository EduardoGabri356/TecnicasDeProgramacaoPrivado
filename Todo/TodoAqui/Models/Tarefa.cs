using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TodoAqui.Models
{
    public class Tarefa
    {
        // [BsonId] diz que essa propriedade é a chave primária do documento
        // [BsonRepresentation] converte o ObjectId do Mongo para string no C#
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        // [BsonElement("titulo")] mapeia o campo C# para o campo "titulo" no MongoDB
        [BsonElement("titulo")]
        public string? Titulo { get; set; }

        [BsonElement("descricao")]
        public string? Descricao { get; set; }

        [BsonElement("concluida")]
        public bool Concluida { get; set; } = false;
    }
}
