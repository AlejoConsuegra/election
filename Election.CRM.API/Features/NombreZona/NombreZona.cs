using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

using Assignment = Election.CRM.API.Features.AsignacionZona.AsignacionZona;

namespace Election.CRM.API.Features.NombreZona;

public partial class NombreZona
{
    public int Id { get; set; }
    public string NameZona { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Assignment> AsignacionZonas { get; set; } = new List<Assignment>();
}
