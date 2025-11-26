using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MyPhotoBiz.Controllers;
using MyPhotoBiz.Models;
using MyPhotoBiz.ViewModels;
using Xunit;

namespace MyPhotoBiz.Tests
{
    public class RolesControllerTests
    {
        private Mock<RoleManager<IdentityRole>> GetMockRoleManager(IEnumerable<IdentityRole> roles)
        {
            var store = new Mock<IRoleStore<IdentityRole>>().Object;
            var mock = new Mock<RoleManager<IdentityRole>>(store, null, null, null, null);
            mock.Setup(r => r.Roles).Returns(roles.AsQueryable());
            return mock;
        }

        private Mock<UserManager<ApplicationUser>> GetMockUserManager(List<ApplicationUser> users)
        {
            var store = new Mock<IUserStore<ApplicationUser>>().Object;
            var mock = new Mock<UserManager<ApplicationUser>>(store, null, null, null, null, null, null, null, null);
            mock.Setup(u => u.Users).Returns(users.AsQueryable());
            return mock;
        }

        [Fact]
        public async Task Index_Returns_RolesIndexViewModel_With_Roles()
        {
            // Arrange
            var roles = new List<IdentityRole>
            {
                new IdentityRole { Id = "1", Name = "Administrator" },
                new IdentityRole { Id = "2", Name = "Photographer" }
            };

            var user = new ApplicationUser { Id = "u1", UserName = "user1", FirstName = "Test", LastName = "User" };
            var users = new List<ApplicationUser> { user };

            var roleManagerMock = GetMockRoleManager(roles);
            var userManagerMock = GetMockUserManager(users);

            // Setup GetUsersInRoleAsync to return our user for any role
            userManagerMock.Setup(um => um.GetUsersInRoleAsync(It.IsAny<string>()))
                .ReturnsAsync((string roleName) => users);

            // Setup GetRolesAsync for users
            userManagerMock.Setup(um => um.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync((ApplicationUser u) => new List<string> { "Administrator" });

            var controller = new RolesController(roleManagerMock.Object, userManagerMock.Object);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var vm = Assert.IsAssignableFrom<RolesIndexViewModel>(viewResult.Model);
            Assert.Equal(2, vm.Roles.Count);
            Assert.NotEmpty(vm.UserRoles);
        }

        [Fact]
        public async Task Create_Returns_Json_Success_When_Creating_Role()
        {
            // Arrange
            var roles = new List<IdentityRole>();
            var users = new List<ApplicationUser>();
            var roleManagerMock = GetMockRoleManager(roles);
            var userManagerMock = GetMockUserManager(users);

            // Setup CreateAsync to return success
            roleManagerMock.Setup(r => r.CreateAsync(It.IsAny<IdentityRole>()))
                .ReturnsAsync(IdentityResult.Success);

            var controller = new RolesController(roleManagerMock.Object, userManagerMock.Object);
            var model = new CreateRoleViewModel { RoleName = "NewRole", Description = "Test" };

            // Act
            var result = await controller.Create(model);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            dynamic data = json.Value!;
            Assert.True((bool)data.success);
        }

        [Fact]
        public async Task Edit_Returns_Json_Success_When_Updating_Role()
        {
            // Arrange
            var role = new IdentityRole { Id = "1", Name = "OldRole" };
            var roles = new List<IdentityRole> { role };
            var users = new List<ApplicationUser>();
            var roleManagerMock = GetMockRoleManager(roles);
            var userManagerMock = GetMockUserManager(users);

            roleManagerMock.Setup(r => r.FindByIdAsync(role.Id)).ReturnsAsync(role);
            roleManagerMock.Setup(r => r.UpdateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);

            var controller = new RolesController(roleManagerMock.Object, userManagerMock.Object);
            var model = new EditRoleViewModel { Id = role.Id, RoleName = "UpdatedRole" };

            // Act
            var result = await controller.Edit(model);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            dynamic data = json.Value!;
            Assert.True((bool)data.success);
        }

        [Fact]
        public async Task Delete_Returns_Json_Success_When_Deleting_Role_With_No_Users()
        {
            // Arrange
            var role = new IdentityRole { Id = "1", Name = "ToDelete" };
            var roles = new List<IdentityRole> { role };
            var users = new List<ApplicationUser>();
            var roleManagerMock = GetMockRoleManager(roles);
            var userManagerMock = GetMockUserManager(users);

            roleManagerMock.Setup(r => r.FindByIdAsync(role.Id)).ReturnsAsync(role);
            userManagerMock.Setup(u => u.GetUsersInRoleAsync(role.Name)).ReturnsAsync(new List<ApplicationUser>());
            roleManagerMock.Setup(r => r.DeleteAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);

            var controller = new RolesController(roleManagerMock.Object, userManagerMock.Object);

            // Act
            var result = await controller.Delete(role.Id);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            dynamic data = json.Value!;
            Assert.True((bool)data.success);
        }
    }
}
