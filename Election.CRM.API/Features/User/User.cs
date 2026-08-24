using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

using Election.CRM.API.Features.Role;
using Assignment = Election.CRM.API.Features.AsignacionZona.AsignacionZona;
using RegistroUser = Election.CRM.API.Features.Registro.Registro;
using OcasionaleUser = Election.CRM.API.Features.Ocasionale.Ocasionale;

namespace Election.CRM.API.Features.User;

public partial class User
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public int IdRol { get; set; }
    public string Email { get; set; } = null!;
    public string Telefono { get; set; } = null!;
    public string Municipio { get; set; } = null!;
    public string Cedula { get; set; } = null!;
    public string Password { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    [ForeignKey("IdRol")]
    public virtual Features.Role.Role IdRolNavigation { get; set; } = null!;
    
    public virtual ICollection<Assignment> AsignacionZonas { get; set; } = new List<Assignment>();
    
    public virtual ICollection<RegistroUser> Registros { get; set; } = new List<RegistroUser>();
    public virtual ICollection<OcasionaleUser> Ocasionales { get; set; } = new List<OcasionaleUser>();
}