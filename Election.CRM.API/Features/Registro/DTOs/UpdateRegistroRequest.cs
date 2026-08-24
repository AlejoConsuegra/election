using System.ComponentModel.DataAnnotations;

namespace Election.CRM.API.Features.Registro.DTOs;

public class UpdateRegistroRequest
{
    // Opcional: Admin lo envía (puede cambiar líder), Líder no lo envía
    public int? IdUser { get; set; }

    [Required(ErrorMessage = "El nombre del votante es obligatorio")]
    [StringLength(255, MinimumLength = 3)]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "La cédula es obligatoria")]
    public string Cedula { get; set; } = null!;

    [Required(ErrorMessage = "La dirección es obligatoria")]
    public string Direccion { get; set; } = null!;

    [Required(ErrorMessage = "El barrio es obligatorio")]
    public string Barrio { get; set; } = null!;

    [Required(ErrorMessage = "El celular es obligatorio")]
    public string Celular { get; set; } = null!;

    [Required(ErrorMessage = "El puesto es obligatorio")]
    public string Puesto { get; set; } = null!;
}