using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyPhotoBiz.Data;
using MyPhotoBiz.Enums;
using MyPhotoBiz.Models;
using MyPhotoBiz.Services;
using Xunit;

namespace MyPhotoBiz.Tests
{
    public class ClientServiceTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetClientByIdAsync_IncludesUserAndPhotoShootsAndInvoices()
        {
            using var context = GetInMemoryDbContext();

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "client1@example.com",
                Email = "client1@example.com",
                FirstName = "Jane",
                LastName = "Doe",
                EmailConfirmed = true
            };

            var client = new Client
            {
                FirstName = "John",
                LastName = "Smith",
                Email = "john@example.com",
                UserId = user.Id,
                User = user,
            };

            var photoshoot = new PhotoShoot
            {
                Title = "Family Shoot",
                Client = client,
                ClientId = client.Id,
                Location = "Studio",
                ScheduledDate = DateTime.Now.AddDays(7),
                UpdatedDate = DateTime.Now,
                Status = Enums.PhotoShootStatus.Scheduled,
                Price = 100
            };

            var invoice = new Invoice
            {
                Client = client,
                ClientId = client.Id,
                Amount = 100,
                Tax = 10,
                InvoiceDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(30),
                Status = Enums.InvoiceStatus.Pending
            };

            context.Users.Add(user);
            context.Clients.Add(client);
            context.PhotoShoots.Add(photoshoot);
            context.Invoices.Add(invoice);
            await context.SaveChangesAsync();

            var service = new ClientService(context);
            var result = await service.GetClientByIdAsync(client.Id);

            Assert.NotNull(result);
            Assert.NotNull(result.User);
            Assert.Single(result.PhotoShoots);
            Assert.Single(result.Invoices);
        }
    }
}
