using NominaDocAPI.DTOs;
using static NominaDocAPI.Helpers.ResponseHelper;

namespace NominaDocAPI.Services
{
    public interface IArchivosService
    {
        ApiResponse<List<ReciboDocumentoDto>> VerificarRecibos(VerificarRecibosRequestDto dto);
        (byte[]? ArchivoBytes, string ContentType, string NombreDescarga) ObtenerArchivo(int anio, int proceso, string nombreArchivo, string tipo, string? rfcCompania = null);
    }

    public class ArchivosService : IArchivosService
    {
        private readonly string _directorioRaiz;

        public ArchivosService(IConfiguration configuration)
        {
            _directorioRaiz = configuration["DirectorioNominas"] ?? @"C:\Facturacion\Nomina";
        }

        public ApiResponse<List<ReciboDocumentoDto>> VerificarRecibos(VerificarRecibosRequestDto dto)
        {
            var resultado = new List<ReciboDocumentoDto>();

            if (dto.NumeroEmpleado <= 0 || string.IsNullOrWhiteSpace(dto.RfcCompañia) || dto.Periodos == null || !dto.Periodos.Any())
            {
                return new ApiResponse<List<ReciboDocumentoDto>>
                {
                    Success = false,
                    Message = "Parámetros de búsqueda inválidos o RFC de compañía faltante.",
                    Data = resultado
                };
            }

            string rfcLimpio = dto.RfcCompañia.Trim();
            string numFormateado = dto.NumeroEmpleado.ToString("D8");
            string patron1 = $"{numFormateado}_{dto.ApellidoNombre.Trim()}";
            string patron2 = $"{numFormateado}_{dto.NombreApellido.Trim()}";

            foreach (var per in dto.Periodos)
            {
                string carpeta = Path.Combine(_directorioRaiz, rfcLimpio, per.Anio.ToString(), $"Per_{per.Proceso}");

                if (!Directory.Exists(carpeta)) continue;

                var archivos1 = Directory.GetFiles(carpeta, $"*{per.Proceso}_{patron1}*");
                var archivos2 = !string.IsNullOrWhiteSpace(dto.NombreApellido)
                    ? Directory.GetFiles(carpeta, $"*{per.Proceso}_{patron2}*")
                    : Array.Empty<string>();

                var archivosEncontrados = archivos1.Union(archivos2, StringComparer.OrdinalIgnoreCase).ToArray();

                if (archivosEncontrados.Length == 0)
                {
                    archivosEncontrados = Directory.GetFiles(carpeta, $"*{per.Proceso}*{numFormateado}*");
                }

                if (archivosEncontrados.Length == 0) continue;

                var grupos = archivosEncontrados
                    .GroupBy(a => Path.GetFileNameWithoutExtension(a), StringComparer.OrdinalIgnoreCase);

                foreach (var grupo in grupos)
                {
                    bool tienePdf = grupo.Any(a => a.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase));
                    bool tieneXml = grupo.Any(a => a.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));

                    if (tienePdf || tieneXml)
                    {
                        resultado.Add(new ReciboDocumentoDto
                        {
                            Anio = per.Anio,
                            Proceso = per.Proceso,
                            FechaInicio = per.FechaInicio,
                            FechaTermino = per.FechaTermino,
                            FechaPago = per.FechaPago,
                            NombreArchivo = grupo.Key,
                            TienePdf = tienePdf,
                            TieneXml = tieneXml
                        });
                    }
                }
            }

            return new ApiResponse<List<ReciboDocumentoDto>>
            {
                Success = true,
                Message = $"Se encontraron {resultado.Count} recibos.",
                Data = resultado
            };
        }

        public (byte[]? ArchivoBytes, string ContentType, string NombreDescarga) ObtenerArchivo(int anio, int proceso, string nombreArchivo, string tipo, string? rfcCompania = null)
        {
            string ext = tipo.Trim().ToLower() == "xml" ? ".xml" : ".pdf";
            string contentType = ext == ".xml" ? "application/xml" : "application/pdf";

            string rutaCompleta = string.Empty;

            if (!string.IsNullOrWhiteSpace(rfcCompania))
            {
                rutaCompleta = Path.Combine(_directorioRaiz, rfcCompania.Trim(), anio.ToString(), $"Per_{proceso}", $"{nombreArchivo}{ext}");
            }

            if (string.IsNullOrEmpty(rutaCompleta) || !File.Exists(rutaCompleta))
            {
                var encontrados = Directory.GetFiles(_directorioRaiz, $"{nombreArchivo}{ext}", SearchOption.AllDirectories);
                if (encontrados.Length > 0)
                {
                    rutaCompleta = encontrados[0];
                }
                else
                {
                    return (null, string.Empty, string.Empty);
                }
            }

            byte[] bytes = File.ReadAllBytes(rutaCompleta);
            return (bytes, contentType, $"{nombreArchivo}{ext}");
        }
    }
}
