namespace Election.CRM.API.Features.Registro.DTOs;

public class CargaMasivaResponse
{
    public int TotalFilas { get; set; }
    public int RegistrosExitosos { get; set; }
    public int RegistrosFallidos { get; set; }
    public int Duplicados { get; set; }
    public List<ErrorFila> Errores { get; set; } = new();
}

public class ErrorFila
{
    public int NumeroFila { get; set; }
    public string Cedula { get; set; } = null!;
    public string Motivo { get; set; } = null!;
}