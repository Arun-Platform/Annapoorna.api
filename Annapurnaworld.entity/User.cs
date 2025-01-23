using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Annapurnaworld.entity
{
    /// <summary>
    /// User Entity
    /// </summary>
    public class User : BaseEntity
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<Address> Addresss { get; set; }
        public string? RefreshToken { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }

    }

    /// <summary>
    /// Address Entity
    /// </summary>
    public class Address : BaseEntity
    {
        public Guid Id { get; set; }
        public string StreetName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }

        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public virtual User User { get; set; }

    }
}
