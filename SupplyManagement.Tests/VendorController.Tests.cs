using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SupplyManagement.Models;
using SupplyManagement.Models.Interfaces;
using SupplyManagement.Models.ViewModels;
using SupplyManagement.Tests.Helpers;
using SupplyManagement.WebApp.Areas.Manager.Controllers;
using System.Data.SqlTypes;
using System.Security.Claims;

namespace SupplyManagement.Tests.Controllers
{
    [TestClass]
    public class VendorControllerTests
    {
        private VendorController _controller;
        private Mock<IApplicationDbContext> _contextMock;
        private Mock<UserManager<IdentityUser>> _userManagerMock;
        private Mock<ILogger<VendorController>> _loggerMock;

        [TestInitialize]
        public void SetUp()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _userManagerMock = new Mock<UserManager<IdentityUser>>(Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
            _loggerMock = new Mock<ILogger<VendorController>>();

            _controller = new VendorController(_contextMock.Object, _userManagerMock.Object, _loggerMock.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = Mock.Of<ClaimsPrincipal>() }
            };
        }

        [TestMethod]
        public async Task Index_ReturnsViewWithVendors()
        {
            // Arrange
            var vendors = new List<Vendor> { new Vendor { Id = 1, Name = "Vendor 1" } }.AsQueryable();
            var vendorDbSetMock = new Mock<DbSet<Vendor>>();
            vendorDbSetMock.As<IAsyncEnumerable<Vendor>>().Setup(m => m.GetAsyncEnumerator(default)).Returns(new TestAsyncEnumerator<Vendor>(vendors.GetEnumerator()));
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Vendor>(vendors.Provider));
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.Expression).Returns(vendors.Expression);
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.ElementType).Returns(vendors.ElementType);
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.GetEnumerator()).Returns(vendors.GetEnumerator());
            _contextMock.Setup(c => c.Vendors).Returns(vendorDbSetMock.Object);

            // Act
            var result = await _controller.Index() as ViewResult;
            IEnumerable<Vendor> model = result.Model as IEnumerable<Vendor>;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(vendors.Count(), model.Count());
            Assert.AreEqual(vendors.First(), model.First());
        }

        [TestMethod]
        public async Task Index_ThrowsException_ReturnsErrorView()
        {
            // Arrange
            _contextMock.Setup(db => db.Vendors).Throws<SqlNullValueException>();

            // Act
            var result = await _controller.Index() as ViewResult;
            var model = result.Model as ErrorViewModel;

            // Assert
            Assert.AreEqual("Error", result.ViewName);
            Assert.AreEqual("An error occurred while retrieving vendors. Data is Null. This method or property cannot be called on Null values.", model.ErrorMessage);
        }

        [TestMethod]
        public async Task Details_ReturnsViewWithVendor()
        {
            // Arrange
            int vendorId = 1;
            var vendors = new List<Vendor> { new Vendor { Id = vendorId, Name = "Vendor 1" } }.AsQueryable();
            var vendorDbSetMock = new Mock<DbSet<Vendor>>();
            vendorDbSetMock.As<IAsyncEnumerable<Vendor>>().Setup(m => m.GetAsyncEnumerator(default)).Returns(new TestAsyncEnumerator<Vendor>(vendors.GetEnumerator()));
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Vendor>(vendors.Provider));
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.Expression).Returns(vendors.Expression);
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.ElementType).Returns(vendors.ElementType);
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.GetEnumerator()).Returns(vendors.GetEnumerator());
            vendorDbSetMock.Setup(m => m.FindAsync(vendorId)).ReturnsAsync(vendors.First());
            _contextMock.Setup(c => c.Vendors).Returns(vendorDbSetMock.Object);

            // Act
            var result = await _controller.Details(vendorId) as ViewResult;
            Vendor model = result.Model as Vendor;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(vendors.First(), model);
        }

        [TestMethod]
        public async Task Details_IdNull_ReturnsErrorView()
        {
            // Act
            var result = await _controller.Details(null) as ViewResult;
            var model = result.Model as ErrorViewModel;

            // Assert
            Assert.AreEqual("Error", result.ViewName);
            Assert.AreEqual("An error occurred while vendor details. Value cannot be null. (Parameter 'id')", model.ErrorMessage);
        }

        [TestMethod]
        public void Create_ReturnsView()
        {
            // Act
            var result = _controller.Create() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Create_Post_ValidModelState_RedirectsToIndex()
        {
            // Arrange
            var vendor = new Vendor { Id = 1, Name = "Vendor 1" };
            var vendorDbSetMock = new Mock<DbSet<Vendor>>();
            _contextMock.Setup(c => c.Vendors).Returns(vendorDbSetMock.Object);

            // Act
            var result = await _controller.Create(vendor) as ViewResult;
            Vendor model = result.Model as Vendor;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(vendor, model);
        }

        [TestMethod]
        public async Task Create_ModelStateInvalid_ReturnsViewWithModel()
        {
            // Arrange
            var vendor = new Vendor();
            _controller.ModelState.AddModelError("Name", "Name is required.");

            // Act
            var result = await _controller.Create(vendor) as ViewResult;
            Vendor model = result.Model as Vendor;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(vendor, model);
        }

        [TestMethod]
        public async Task Create_VendorExists_ReturnsViewWithModel()
        {
            int vendorId = 1;
            var vendor = new Vendor { Id = vendorId, Name = "Vendor 1" };
            var vendors = new List<Vendor> { vendor }.AsQueryable();
            var vendorDbSetMock = new Mock<DbSet<Vendor>>();
            vendorDbSetMock.As<IAsyncEnumerable<Vendor>>().Setup(m => m.GetAsyncEnumerator(default)).Returns(new TestAsyncEnumerator<Vendor>(vendors.GetEnumerator()));
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Vendor>(vendors.Provider));
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.Expression).Returns(vendors.Expression);
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.ElementType).Returns(vendors.ElementType);
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.GetEnumerator()).Returns(vendors.GetEnumerator());
            vendorDbSetMock.Setup(m => m.FindAsync(vendorId)).ReturnsAsync(vendor);

            // Act
            var result = await _controller.Create(vendor) as ViewResult;
            var model = result.Model as Vendor;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(vendor, model);
        }

        [TestMethod]
        public async Task Edit_ReturnsViewWithVendor()
        {
            int vendorId = 1;
            var vendors = new List<Vendor> { new Vendor { Id = vendorId, Name = "Vendor 1" } }.AsQueryable();
            var vendorDbSetMock = new Mock<DbSet<Vendor>>();
            vendorDbSetMock.As<IAsyncEnumerable<Vendor>>().Setup(m => m.GetAsyncEnumerator(default)).Returns(new TestAsyncEnumerator<Vendor>(vendors.GetEnumerator()));
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Vendor>(vendors.Provider));
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.Expression).Returns(vendors.Expression);
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.ElementType).Returns(vendors.ElementType);
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.GetEnumerator()).Returns(vendors.GetEnumerator());
            vendorDbSetMock.Setup(m => m.FindAsync(vendorId)).ReturnsAsync(vendors.First());
            _contextMock.Setup(c => c.Vendors).Returns(vendorDbSetMock.Object);

            // Act
            var result = await _controller.Edit(vendorId) as ViewResult;
            Vendor model = result.Model as Vendor;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(vendors.First(), model);
        }

        [TestMethod]
        public async Task Edit_Post_ValidModelState_RedirectsToIndex()
        {
            // Arrange
            int vendorId = 1;
            var vendors = new List<Vendor> { new Vendor { Id = vendorId, Name = "Vendor 1" } }.AsQueryable();
            var vendor = new Vendor { Id = vendorId, Name = "Vendor 2" };
            var vendorDbSetMock = new Mock<DbSet<Vendor>>();
            vendorDbSetMock.As<IAsyncEnumerable<Vendor>>().Setup(m => m.GetAsyncEnumerator(default)).Returns(new TestAsyncEnumerator<Vendor>(vendors.GetEnumerator()));
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Vendor>(vendors.Provider));
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.Expression).Returns(vendors.Expression);
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.ElementType).Returns(vendors.ElementType);
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.GetEnumerator()).Returns(vendors.GetEnumerator());
            vendorDbSetMock.Setup(m => m.FindAsync(vendorId)).ReturnsAsync(vendors.First());
            _contextMock.Setup(c => c.Vendors).Returns(vendorDbSetMock.Object);

            // Act
            var result = await _controller.Edit(vendorId, vendor) as ViewResult;
            Vendor model = result.Model as Vendor;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(vendor, model);
        }

        [TestMethod]
        public async Task Edit_IdMismatch_ReturnsErrorView()
        {
            // Arrange
            int id = 2;
            int vendorId = 1;
            var vendors = new List<Vendor> { new Vendor { Id = vendorId, Name = "Vendor 1" } }.AsQueryable();
            var vendor = new Vendor { Id = vendorId, Name = "Vendor 2" };
            var vendorDbSetMock = new Mock<DbSet<Vendor>>();
            vendorDbSetMock.As<IAsyncEnumerable<Vendor>>().Setup(m => m.GetAsyncEnumerator(default)).Returns(new TestAsyncEnumerator<Vendor>(vendors.GetEnumerator()));
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Vendor>(vendors.Provider));
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.Expression).Returns(vendors.Expression);
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.ElementType).Returns(vendors.ElementType);
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.GetEnumerator()).Returns(vendors.GetEnumerator());
            vendorDbSetMock.Setup(m => m.FindAsync(vendorId)).ReturnsAsync(vendors.First());
            _contextMock.Setup(c => c.Vendors).Returns(vendorDbSetMock.Object);

            // Act
            var result = await _controller.Edit(id, vendor) as ViewResult;
            var model = result.Model as ErrorViewModel;

            // Assert
            Assert.AreEqual("Error", result.ViewName);
            Assert.AreEqual("An error occurred while updating vendor. Incorrect vendor id.", model.ErrorMessage);
        }

        [TestMethod]
        public async Task Delete_ReturnsViewWithVendor()
        {
            // Arrange
            int vendorId = 1;
            var vendors = new List<Vendor> { new Vendor { Id = vendorId, Name = "Vendor 1" } }.AsQueryable();
            var vendorDbSetMock = new Mock<DbSet<Vendor>>();
            vendorDbSetMock.As<IAsyncEnumerable<Vendor>>().Setup(m => m.GetAsyncEnumerator(default)).Returns(new TestAsyncEnumerator<Vendor>(vendors.GetEnumerator()));
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Vendor>(vendors.Provider));
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.Expression).Returns(vendors.Expression);
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.ElementType).Returns(vendors.ElementType);
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.GetEnumerator()).Returns(vendors.GetEnumerator());
            vendorDbSetMock.Setup(m => m.FindAsync(vendorId)).ReturnsAsync(vendors.First());
            _contextMock.Setup(c => c.Vendors).Returns(vendorDbSetMock.Object);

            // Act
            var result = await _controller.Delete(vendorId) as ViewResult;
            Vendor model = result.Model as Vendor;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(vendors.First(), model);
        }

        [TestMethod]
        public async Task Delete_VendorNotFound_ReturnsErrorView()
        {
            // Arrange
            int vendorId = 1;
            var vendors = new List<Vendor> { new Vendor { Id = vendorId, Name = "Vendor 1" } }.AsQueryable();
            var vendorDbSetMock = new Mock<DbSet<Vendor>>();
            vendorDbSetMock.Setup(m => m.FindAsync(vendorId)).ReturnsAsync((Vendor)null);
            _contextMock.Setup(c => c.Vendors).Returns(vendorDbSetMock.Object);

            // Act
            var result = await _controller.Delete(vendorId) as ViewResult;
            var model = result.Model as ErrorViewModel;

            // Assert
            Assert.AreEqual("Error", result.ViewName);
            Assert.AreEqual("An error occurred while retrieving vendor details for deletion. Specified method is not supported.", model.ErrorMessage);
        }

        [TestMethod]
        public async Task DeleteConfirmed_DeletesVendor_RedirectsToIndex()
        {
            // Arrange
            int vendorId = 1;
            var vendor = new Vendor { Id = vendorId, Name = "Vendor 1" };
            var vendors = new List<Vendor> { vendor }.AsQueryable();
            var vendorDbSetMock = new Mock<DbSet<Vendor>>();
            vendorDbSetMock.As<IAsyncEnumerable<Vendor>>().Setup(m => m.GetAsyncEnumerator(default)).Returns(new TestAsyncEnumerator<Vendor>(vendors.GetEnumerator()));
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Vendor>(vendors.Provider));
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.Expression).Returns(vendors.Expression);
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.ElementType).Returns(vendors.ElementType);
            vendorDbSetMock.As<IQueryable<Vendor>>().Setup(m => m.GetEnumerator()).Returns(vendors.GetEnumerator());
            vendorDbSetMock.Setup(m => m.FindAsync(vendorId)).ReturnsAsync(vendors.First());
            vendorDbSetMock.Setup(m => m.Remove(vendor)).Callback<Vendor>(new Action<Vendor>(v => v.Name = "deleted"));
            _contextMock.Setup(c => c.Vendors).Returns(vendorDbSetMock.Object);

            // Act
            var result = await _controller.DeleteConfirmed(vendorId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(vendor.Name, "deleted");
        }
    }
}
