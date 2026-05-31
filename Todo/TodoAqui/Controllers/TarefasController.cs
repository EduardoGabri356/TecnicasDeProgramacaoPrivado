using Microsoft.AspNetCore.Mvc;
using TodoAqui.Models;
using TodoAqui.Repositories;

namespace TodoApi.Controllers
{
    // [ApiController] ativa validações automáticas e formatação JSON
    [ApiController]

    // [Route] define o caminho base: /api/tarefas
    // [controller] é substituído pelo nome da classe sem "Controller"
    [Route("api/[controller]")]
    public class TarefasController : Controller
    {
        // Injeção de dependência: o repository é passado pelo Program.cs
        private readonly TarefaRepository _repository;

        public TarefasController(TarefaRepository repository)
        {
            _repository = repository;
        }

        // GET /api/tarefas — retorna todas as tarefas
        // ActionResult<List<Tarefa>> permite retornar a lista ou um erro HTTP
        [HttpGet]
        public async Task<ActionResult<List<Tarefa>>> Get()
        {
            var tarefas = await _repository.ObterTodasTarefasAsync();
            return Ok(tarefas); // 200 OK com a lista em JSON
        }

        // POST /api/tarefas — cria uma nova tarefa
        // O corpo da requisição (JSON) é deserializado automaticamente em Tarefa
        [HttpPost]
        public async Task<IActionResult> Post(Tarefa novaTarefa)
        {
            await _repository.CriarTarefaAsync(novaTarefa);

            // CreatedAtAction retorna 201 Created com o header Location apontando para o recurso
            return CreatedAtAction(nameof(Get), new { id = novaTarefa.Id }, novaTarefa);
        }

        // Funções extras: DELETE e PUT para completar o CRUD
        // DELETE /api/tarefas/{id} — deleta uma tarefa pelo ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _repository.DeletarTarefaAsync(id);
            return NoContent(); // 204 — sucesso sem corpo de resposta
        }

        // PUT /api/tarefas/{id} — atualiza uma tarefa existente
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, Tarefa tarefaAtualizada)
        {
            // Garante que o ID do objeto bate com o ID da rota
            tarefaAtualizada.Id = id;
            await _repository.AtualizarTarefaAsync(id, tarefaAtualizada);
            return NoContent(); // 204 — atualizado com sucesso
        }
    }
}