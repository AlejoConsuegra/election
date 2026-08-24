namespace Election.CRM.API.Features.Votante.DTOs;

public class CedulaNoRegistradaResponse
{
    public bool Error { get; set; } = true;
    public string Codigo { get; set; } = null!;
    public string Mensaje { get; set; } = null!;
    public SugerenciaRegistro Sugerencia { get; set; } = null!;
}

public class SugerenciaRegistro
{
    public string Accion { get; set; } = null!;
    public string Mensaje { get; set; } = null!;
    public string Endpoint { get; set; } = null!;
    public Dictionary<string, object> DatosPrellenados { get; set; } = null!;
}