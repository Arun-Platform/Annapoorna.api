using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Annapurnaworld.entity
{
    public class Role
    {
        public Guid Id  { get; set; }
        public string Name { get; set; }
        public virtual SubRole SubRole { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
    }

    public class UserRole
    {
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Guid RoleId { get; set; }
        public Role Role { get; set; }

    }

    public class SubRole
    {
        public Guid Id { get; set; }

        [ForeignKey("Role")]
        public Guid RoleId { get; set; }
        public string Name { get; set; }
    }
}
