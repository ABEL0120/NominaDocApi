using NominaDocAPI.DTOs;
using static NominaDocAPI.Helpers.ResponseHelper;

namespace NominaDocAPI.Services
{
    public interface IArchivosService
    {
        ApiResponse<List<ReciboDocumentoDto>> VerificarRecibos(VerificarRecibosRequestDto dto);
        (byte[]? ArchivoBytes, string ContentType, string NombreDescarga) ObtenerArchivo(int anio, int proceso, string nombreArchivo, string tipo);
    }

    public class ArchivosService : IArchivosService
    {
        private readonly string _directorioRaiz;

        public ArchivosService(IConfiguration configuration)
        {
            _directorioRaiz = configuration["DirectorioNominas"] ?? @"C:\\Facturacion\\Nomina";
        }

        public ApiResponse<List<ReciboDocumentoDto>> VerificarRecibos(VerificarRecibosRequestDto dto)
        {
            var resultado = new List<ReciboDocumentoDto>();

            if (dto.NumeroEmpleado <= 0 || dto.Periodos == null || !dto.Periodos.Any())
            {
                return new ApiResponse<List<ReciboDocumentoDto>>
                {
                    Success = false,
                    Message = "Parámetros de búsqueda inválidos.",
                    Data = resultado
                };
            }

            string numFormateado = dto.NumeroEmpleado.ToString("D8");
            string patronBase = $"{numFormateado}_{dto.NombreEmpleado.Trim()}";

            foreach (var per in dto.Periodos)
            {
                string carpeta = Path.Combine(_directorioRaiz, per.Anio.ToString(), $"Per_{per.Proceso}");

                if (!Directory.Exists(carpeta)) continue;

                string filtro = $"*{per.Proceso}_{patronBase}*";
                var archivosEncontrados = Directory.GetFiles(carpeta, filtro);

                if (archivosEncontrados.Length == 0) continue;

                string baseName = Path.GetFileNameWithoutExtension(archivosEncontrados[0]);
                bool tienePdf = archivosEncontrados.Any(a => a.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase));
                bool tieneXml = archivosEncontrados.Any(a => a.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));

                if (tienePdf || tieneXml)
                {
                    resultado.Add(new ReciboDocumentoDto
                    {
                        Anio = per.Anio,
                        Proceso = per.Proceso,
                        FechaInicio = per.FechaInicio,
                        FechaTermino = per.FechaTermino,
                        FechaPago = per.FechaPago,
                        NombreArchivo = baseName,
                        TienePdf = tienePdf,
                        TieneXml = tieneXml
                    });
                }
            }

            return new ApiResponse<List<ReciboDocumentoDto>>
            {
                Success = true,
                Message = $"Se encontraron {resultado.Count} recibos.",
                Data = resultado
            };
        }

        public (byte[]? ArchivoBytes, string ContentType, string NombreDescarga) ObtenerArchivo(int anio, int proceso, string nombreArchivo, string tipo)
        {
            string ext = tipo.Trim().ToLower() == "xml" ? ".xml" : ".pdf";
            string contentType = ext == ".xml" ? "application/xml" : "application/pdf";
            string rutaCompleta = Path.Combine(_directorioRaiz, anio.ToString(), $"Per_{proceso}", $"{nombreArchivo}{ext}");

            if (!File.Exists(rutaCompleta))
            {
                return (null, string.Empty, string.Empty);
            }

            byte[] bytes = File.ReadAllBytes(rutaCompleta);
            return (bytes, contentType, $"{nombreArchivo}{ext}");
        }
    }
}
