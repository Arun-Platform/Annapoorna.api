using Annapurnaworld.entity;

namespace Annapurnaworld.service
{
    /// <summary>
    /// Interface for the user service class
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Get user information from email and password
        /// </summary>
        /// <param name="email">email id</param>
        /// <param name="password">passsword</param>
        /// <returns>user object</returns>
        Task<User> GetUser(string email, string password);

        /// <summary>
        /// Get user information from email and password
        /// </summary>
        /// <param name="email">email id</param>
        /// <param name="password">passsword</param>
        /// <returns>user object</returns>
        Task<User> GetUserByEmail(string email);

        /// <summary>
        /// Update user details
        /// </summary>
        /// <param name="user">user object</param>
        /// <returns>void</returns>
        Task UpdateUser(User user);

        /// <summary>
        /// Get user by userId
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>user</returns>
        Task<User> GetUserById(Guid userId );

        /// <summary>
        /// Get role by name.
        /// </summary>
        /// <param name="name">name</param>
        /// <returns>Role</returns>
        Task<Role> GetRoleByName(string name);

        /// <summary>
        /// Add User
        /// </summary>
        /// <param name="user">user object</param>
        /// <returns>user</returns>
        Task AddUser(User user);

        /// <summary>
        /// Get all users.
        /// </summary>
        /// <returns>user object</returns>
        Task<List<User>> GetAllUsersAsync();
    }
}
