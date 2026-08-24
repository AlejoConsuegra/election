using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Election.CRM.API.Features.Registro;

public partial class Registro
{
    public int Id { get; set; }
    public int? IdUser { get; set; }
    public string Nombre { get; set; } = null!;
    public string Cedula { get; set; } = null!;
    public string Direccion { get; set; } = null!;
    public string Barrio { get; set; } = null!;
    public string Celular { get; set; } = null!;
    public string Puesto { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    [ForeignKey("IdUser")]
    public virtual Features.User.User? IdUserNavigation { get; set; }
    
    // ✅ AGREGAR esta línea:
    public virtual Features.Votante.Votante? Votante { get; set; }
}