using System.ComponentModel.DataAnnotations.Schema;
using Election.CRM.API.Features.User;
using Election.CRM.API.Features.NombreZona;

namespace Election.CRM.API.Features.Ocasionale;

public partial class Ocasionale
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Cedula { get; set; } = null!;
    public int IdZona { get; set; }
    public int IdUser { get; set; }
    public string IpAddress { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    [ForeignKey("IdUser")]
    public virtual Features.User.User? IdUserNavigation { get; set; }

    [ForeignKey("IdZona")]
    public virtual Features.NombreZona.NombreZona? IdZonaNavigation { get; set; }
}