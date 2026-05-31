using Microsoft.AspNetCore.Mvc;
using TarefasApi.Models;
using TarefasApi.Services;

namespace TarefasApi.Controllers
{
    // [ApiController] ativa validações automáticas e formatação JSON
    [ApiController]
    // [Route] define a rota base para este controlador, usando o nome do controlador
    [Route("api/[controller]")]
    public class TarefasController : ControllerBase
    {
        // Atributo para acessar os métodos de negócio relacionados às tarefas, injetado via construtor
        private readonly TarefaService _service;

        // Construtor que recebe o serviço de tarefas, a injeção de dependência é configurada no Program.cs
        public TarefasController(TarefaService service)
        {
            _service = service;
        }

        // GET api/tarefas — lista todas
        [HttpGet]
        // ActionResult<List<Tarefa>> permite retornar a lista ou um erro HTTP
        public async Task<ActionResult<List<Tarefa>>> GetTarefas()
        {
            // Chama o serviço para obter a lista de todas tarefas 
            var tarefas = await _service.GetAsync();
            return Ok(tarefas); 
        }

        // POST api/tarefas — cria nova tarefa
        [HttpPost]
        // IActionResult permite retornar diferentes tipos de resposta HTTP, como CreatedAtAction para indicar que um recurso foi criado
        public async Task<IActionResult> PostTarefa(Tarefa tarefa) // 
        {
            // Chama o serviço para criar a nova tarefa
            await _service.CreateTaskAsync(tarefa);
            // CreatedAtAction retorna um status 201 Created e inclui um link para a ação GetTarefas com o ID da nova tarefa
            return CreatedAtAction(nameof(GetTarefas),
                new { id = tarefa.Id }, tarefa);
        }

        // PUT api/tarefas/{id} — edita tarefa existente
        [HttpPut("{id}")]
        // IActionResult permite retornar NoContent
        public async Task<IActionResult> PutTarefa( // PutTarefa é o nome do método
            string id, Tarefa tarefaAtualizada) // Recebe o ID da tarefa a ser atualizada e os dados atualizados da tarefa
        {
            // Garante que o ID do objeto bate com o ID da rota
            tarefaAtualizada.Id = id;
            // Chama o serviço para atualizar a tarefa existente
            await _service.TaskUpdateAsync(id, tarefaAtualizada);
            // NoContent retorna um status 204 indicando que a atualização foi bem-sucedida, mas não há conteúdo para retornar
            return NoContent();
        }

        // DELETE api/tarefas/{id} — exclui tarefa
        // semelhante ao método de atualização, recebe o ID da tarefa a ser removida e usa DeleteOneAsync para remover o documento correspondente da coleção
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTarefa(string id)
        {
            await _service.RemoveAsync(id);
            return NoContent();
        }
    }
}