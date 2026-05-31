using Microsoft.AspNetCore.Mvc;
using TelefoneApi.Models;
using TelefoneApi.Services;

namespace TelefoneApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelefoneController : ControllerBase
    {
        private readonly TelefoneService _service;
        public TelefoneController(TelefoneService service)
        {
            _service = service;
        }

        // GET api/telefones
        [HttpGet]
        public async Task <ActionResult<List<Telefone>>> ListaTelefones()
        {
            var telefones = await _service.GetAsync();
            return Ok(telefones);
        }

        // POST api/tarefas - criar telefones
        [HttpPost]
        public async Task<IActionResult> PostTelefones(Telefone telefone)
        {
            await _service.CriarTelefone(telefone);
            return CreatedAtAction(nameof(ListaTelefones),
                new { id = telefone.Id }, telefone);
        }
    }
}
