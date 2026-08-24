using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Election.CRM.API.Features.Votante;

public partial class Votante
{
    public int Id { get; set; }
    public int IdRegistro { get; set; }
    public string Cedula { get; set; } = null!;
    public string IpAddress { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    [ForeignKey("IdRegistro")]
    public virtual Features.Registro.Registro IdRegistroNavigation { get; set; } = null!;
}