using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

using UserRol = Election.CRM.API.Features.User.User;

namespace Election.CRM.API.Features.Role
{
    [Table("roles")] 
    public partial class Role
    {   
        [Column("id_rol")]
        public int Id { get; set; }

        [Column("name_rol")]
        public string NameRol { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<UserRol> Users { get; set; } = new List<UserRol>();
    }
}