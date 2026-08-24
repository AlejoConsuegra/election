namespace Election.CRM.API.Features.Alertas.DTOs;

public class AlertaVotanteDto
{
    public string Nombre { get; set; } = null!;
    public int CantidadLideres { get; set; }
    public List<LiderInfoDto> Lideres { get; set; } = new();
}