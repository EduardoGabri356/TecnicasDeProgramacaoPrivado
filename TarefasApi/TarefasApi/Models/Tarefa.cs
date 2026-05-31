using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TarefasApi.Models
{
    public class Tarefa
    {
        // Atributos para mapear o ID do MongoDB
        [BsonId]
        // Indica que o ID é representado como um ObjectId no MongoDB
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        // Atributo para mapear o título da tarefa
        [BsonElement("titulo")]
        // = string.Empty é usado para garantir que o título seja inicializado como uma string vazia, evitando valores nulos
        public string Titulo { get; set; } = string.Empty;

        // atributo para verificar se a tarefa foi concluída ou não
        [BsonElement("concluida")]
        // = false é usado para garantir que a tarefa seja inicializada como não concluída por padrão
        public bool Concluida { get; set; } = false;
    }
}
