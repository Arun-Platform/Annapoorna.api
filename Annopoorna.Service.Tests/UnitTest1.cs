namespace Annopoorna.Service.Tests
{
    using Moq;
    using Xunit;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Annapurnaworld.data;
    using Annapurnaworld.entity;
    using Annapurnaworld.service;
    using DocumentFormat.OpenXml.Spreadsheet;

    namespace UserServiceTests
    {
        public class UserServiceTests
        {
            private ApplicationDbContext _applicationDbContext;
            private UserService _userService;
              
            public UserServiceTests()
            {
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                            .Options;

                _applicationDbContext = new ApplicationDbContext(options,null);
                _userService = new UserService(_applicationDbContext);
            }

            [Fact]
            public async Task GetUser_ValidEmailAndPassword_ReturnsUser()
            {
                // Arrange
                var email = "test@example.com";
                var password = "password123";

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    Password = password,
                    FullName = "Arun",
                    Addresss = new List<Address> { new Address { City = "city", Country = "s", StreetName = "s", State = "s" } },
                    IsActive = true,
                    RefreshToken = "RefreshToken",
                    UserRoles = new List<UserRole> { new UserRole { RoleId = Guid.NewGuid() } }
                };

                _applicationDbContext.Users.Add(user);
                await _applicationDbContext.SaveChangesAsync();

                // Act
                var result = await _userService.GetUser(email, password);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(email, result.Email);
            }

            [Fact]
            public async Task AddUser_ValidUser_AddsUserToDb()
            {
                var email = "test@example.com";
                var password = "password123";
                // Arrange
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    Password = password,
                    FullName = "Arun",
                    Addresss = new List<Address> { new Address { City = "city", Country = "s", StreetName = "s", State = "s" } },
                    IsActive = true,
                    RefreshToken = "RefreshToken",
                    UserRoles = new List<UserRole> { new UserRole { RoleId = Guid.NewGuid() } }
                };

                // Act
                await _userService.AddUser(user);

                // Assert
                var savedUser = await _applicationDbContext.Users.FindAsync(user.Id);
                Assert.NotNull(savedUser);
                Assert.Equal(user.Email, savedUser.Email);
            }

        }
    }

}