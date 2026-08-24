using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Election.CRM.API.Features;

namespace Election.CRM.API.Features.AsignacionZona;

public partial class AsignacionZona
{
    public int Id { get; set; }
    public int IdZona { get; set; }
    public int IdUser { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public virtual Features.User.User IdUserNavigation { get; set; } = null!;
    
    public virtual Features.NombreZona.NombreZona IdZonaNavigation { get; set; } = null!;
}
