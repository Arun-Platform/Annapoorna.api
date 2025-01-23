using Annapurnaworld.common;
using Annapurnaworld.entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Annapurnaworld.data
{
    /// <summary>
    /// Initalize Database seed.
    /// </summary>
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Check if the database has been seeded
            if (context.Users.Any())
            {
                return; // DB has been seeded
            }
            //Add seed data
            var adminRoleId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var UserId = Guid.NewGuid();
            var userRoleId = Guid.NewGuid();
            context.Roles.AddRange(new Role { Id = adminRoleId, Name = "Admin" });
            context.Roles.AddRange(new Role { Id = userRoleId, Name = "User" });
            context.Users.AddRange(
                new User { Id = adminId, FullName = "Rahul Mahendra", Email = "rahulmahindra@gmail.com", IsActive = true, Password = PasswordHelper.HashPassword("Rahul_123") }
            );
            context.UserRoles.Add(new UserRole() { RoleId = adminRoleId,UserId = adminId });
            context.UserRoles.Add(new UserRole() { RoleId = userRoleId, UserId = UserId });
            context.SaveChanges();
        }
    }

}
