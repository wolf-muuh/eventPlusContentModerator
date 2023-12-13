using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.ContentModerator;
using System.Text;
using webapi.event_.Domains;
using webapi.event_.Repositories;

namespace webapi.event_.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ComentariosEventoController : ControllerBase
    {
        //Acesso aos métodos do repositorio
        ComentariosEventoRepository comentario = new ComentariosEventoRepository();

        //Armazena dados da API externa (IA - Azure)
        private readonly ContentModeratorClient _contentModeratorClient;

        /// <summary>
        /// Construtor que recebe os dados nescessarios para o acesso ao serviço externo
        /// </summary>
        /// <param name="contentModeratorClient">objeto do tipo ContentModeratorClient</param>
        public ComentariosEventoController(ContentModeratorClient contentModeratorClient)
        {
            _contentModeratorClient = contentModeratorClient;
        }

        [HttpPost("CadastroIA")]
        public async Task<IActionResult> PostIA(ComentariosEvento comentariosEvento)
        {
            try
            {
                // Se a descrição do comentário não for passada no objeto
                if (string.IsNullOrEmpty(comentariosEvento.Descricao))
                {
                    return BadRequest("O texto a ser analisado não pode ser vazio!");
                }

                // Converte a string do comentário em um MemoryStream
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(comentariosEvento.Descricao));

                // Realiza a moderação do conteúdo
                var moderationResult = await _contentModeratorClient.TextModeration.ScreenTextAsync("text/plain", stream, "por", false, false, null, true);

                // Se existir termos ofensivos
                if (moderationResult.Terms != null)
                {
                    // Atribuir false ao "Exibe"
                    comentariosEvento.Exibe = false;

                    // Cadastra o comentário
                    comentario.Cadastrar(comentariosEvento);
                }
                else
                {
                    // Atribuir True ao "Exibe"
                    comentariosEvento.Exibe = true;

                    // Cadastra o comentário
                    comentario.Cadastrar(comentariosEvento);
                }
                return StatusCode(201, comentariosEvento);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }


        [HttpGet("LitarSomenteExibe")]
        public IActionResult GetIA()
        {
            try
            {
                return Ok(comentario.ListarSomenteExibe());
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }


        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                return Ok(comentario.Listar());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("BuscarPorIdUsuario/{id}")]

        public IActionResult GetByIdUser(Guid idUsuario, Guid idEvento)
        {
            try
            {
                return Ok(comentario.BuscarPorIdUsuario(idUsuario, idEvento));
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }

        }

        [HttpPost]
        public IActionResult Post(ComentariosEvento novoComentario)
        {
            try
            {
                comentario.Cadastrar(novoComentario);
                return StatusCode(201, novoComentario);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }
        [HttpDelete]
        public IActionResult Delete(Guid id)
        {
            try
            {
                comentario.Deletar(id);
                return NoContent();
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }
    }
}
