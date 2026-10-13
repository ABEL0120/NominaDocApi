using Microsoft.AspNetCore.Mvc;
using NominaDocAPI.DTOs;
using NominaDocAPI.Services;
using static NominaDocAPI.Helpers.ResponseHelper;

namespace NominaDocAPI.Controllers
{
    [ApiController]
    [Route("api/archivos")]
    public class ArchivosController : ControllerBase
    {
        private readonly IArchivosService _archivosService;

        public ArchivosController(IArchivosService archivosService)
        {
            _archivosService = archivosService;
        }

        [HttpPost("verificar-recibos")]
        public IActionResult VerificarRecibos([FromBody] VerificarRecibosRequestDto dto)
        {
            var respuesta = _archivosService.VerificarRecibos(dto);
            return Ok(respuesta);
        }

        [HttpGet("descargar/{anio}/{proceso}/{tipo}/{archivo}")]
        public IActionResult DescargarArchivo(int anio, int proceso, string tipo, string archivo, [FromQuery] string? rfcCompania)
        {
            var (bytes, contentType, nombreDescarga) = _archivosService.ObtenerArchivo(anio, proceso, archivo, tipo, rfcCompania);

            if (bytes == null)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "El archivo solicitado no existe en el servidor.",
                    Data = false
                });
            }

            return File(bytes, contentType, nombreDescarga);
        }
    }
}
