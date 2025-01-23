using Annapurnaworld.data;
using Annapurnaworld.entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Annapurnaworld.service
{
    /// <summary>
    /// Implimenatation class for user service
    /// </summary>
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _applicationDbContext;

        /// <summary>
        /// contructor to initalize objects
        /// </summary>
        /// <param name="applicationDbContext"></param>
        public UserService(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        /// <summary>
        /// Get user information from email and password
        /// </summary>
        /// <param name="email">email id</param>
        /// <param name="password">passsword</param>
        /// <returns>user object</returns>
        public async Task<User> GetUser(string email, string password)
        {
            var currentUser = await _applicationDbContext.Users.Include(x => x.UserRoles).FirstOrDefaultAsync(x => x.Email == email && x.Password == password);
            if (currentUser != null && currentUser.UserRoles.Any())
            {
                var roles = await _applicationDbContext.Roles.ToListAsync();
                foreach (var role in currentUser.UserRoles)
                {
                    if (role.Role == null)
                    {
                        role.Role = roles.Find(x => x.Id == role.RoleId);
                    }
                }
            }
            return currentUser;
        }

        /// <summary>
        /// Update user details
        /// </summary>
        /// <param name="user">user object</param>
        /// <returns>void</returns>
        public async Task<User> GetUserById(Guid userId)
        {
            var currentUser = await _applicationDbContext.Users.Include(x => x.UserRoles).FirstOrDefaultAsync(x => x.Id == userId);
            if (currentUser != null && currentUser.UserRoles.Any())
            {
                var roles = await _applicationDbContext.Roles.ToListAsync();
                foreach (var role in currentUser.UserRoles)
                {
                    if (role.Role == null)
                    {
                        role.Role = roles.Find(x => x.Id == role.RoleId);
                    }
                }
            }
            return currentUser;
        }

        /// <summary>
        /// Get user by userId
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>user</returns>
        public async Task UpdateUser(User user)
        {
            _applicationDbContext.Users.Update(user);
            await _applicationDbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Add User
        /// </summary>
        /// <param name="user">user object</param>
        /// <returns>user</returns>
        public async Task AddUser(User user)
        {
            _applicationDbContext.Users.Add(user);
            await _applicationDbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Get user information from email and password
        /// </summary>
        /// <param name="email">email id</param>
        /// <param name="password">passsword</param>
        /// <returns>user object</returns>
        public async Task<User> GetUserByEmail(string email)
        {
            var currentUser = await _applicationDbContext.Users.Include(x => x.UserRoles).FirstOrDefaultAsync(x => x.Email == email);
            if (currentUser != null && currentUser.UserRoles.Any())
            {
                var roles = await _applicationDbContext.Roles.ToListAsync();
                foreach (var role in currentUser.UserRoles)
                {
                    if (role.Role == null)
                    {
                        role.Role = roles.Find(x => x.Id == role.RoleId);
                    }
                }
            }
            return currentUser;
        }

        public async Task<Role> GetRoleByName(string name)
        {
            var role = await _applicationDbContext.Roles.FirstOrDefaultAsync(x=>x.Name == name);
            return role;
        }

        /// <summary>
        /// Get all users.
        /// </summary>
        /// <returns>user object</returns>
        public async Task<List<User>> GetAllUsersAsync()
        {
            var users =   await _applicationDbContext.Users.ToListAsync();
            users.ForEach(x => x.Password = null);
            return users;
        }
    }
}
