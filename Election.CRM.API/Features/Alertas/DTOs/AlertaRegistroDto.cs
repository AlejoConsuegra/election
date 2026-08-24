namespace Election.CRM.API.Features.Alertas.DTOs;

public class AlertaRegistroDto
{
    public string Cedula { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public int CantidadLideres { get; set; }
    public List<LiderInfoDto> Lideres { get; set; } = new();
}