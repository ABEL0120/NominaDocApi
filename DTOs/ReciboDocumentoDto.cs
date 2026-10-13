namespace NominaDocAPI.DTOs
{
    public class VerificarRecibosRequestDto
    {
        public int NumeroEmpleado { get; set; }
        public string ApellidoNombre { get; set; } = string.Empty;
        public string NombreApellido { get; set; } = string.Empty;
        public string RfcCompañia { get; set; } = string.Empty;
        public List<PeriodoBusquedaDto> Periodos { get; set; } = new();
    }

    public class PeriodoBusquedaDto
    {
        public int Anio { get; set; }
        public int Proceso { get; set; }
        public string FechaInicio { get; set; } = string.Empty;
        public string FechaTermino { get; set; } = string.Empty;
        public string FechaPago { get; set; } = string.Empty;
    }

    public class ReciboDocumentoDto
    {
        public int Anio { get; set; }
        public int Proceso { get; set; }
        public string FechaInicio { get; set; } = string.Empty;
        public string FechaTermino { get; set; } = string.Empty;
        public string FechaPago { get; set; } = string.Empty;
        public string NombreArchivo { get; set; } = string.Empty;
        public bool TienePdf { get; set; }
        public bool TieneXml { get; set; }
    }
}
