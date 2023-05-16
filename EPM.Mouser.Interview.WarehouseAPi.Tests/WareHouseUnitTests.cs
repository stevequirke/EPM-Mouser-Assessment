using EPM.Mouser.Interview.Data;
using EPM.Mouser.Interview.Models;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Epm.Mouser.Interview.WarehouseApi.Tests
{
    [TestClass]
    public class WareHouseUnitTests
    {

        private Mock<IWarehouseRepository> warehouseRepository = new Mock<IWarehouseRepository>();
        private EPM.Mouser.Interview.Web.Controllers.WarehouseApi? wareHouseApiController = null;

        [TestMethod]
        public void InvalidOrderQuantity()
        {
            wareHouseApiController = new EPM.Mouser.Interview.Web.Controllers.WarehouseApi(warehouseRepository.Object);
            // var products = GetListOfProducts();
            UpdateQuantityRequest updateQuantityRequest = new UpdateQuantityRequest
            {
                Id = 4,
                Quantity = -1
            };

            var result = wareHouseApiController.OrderItem(updateQuantityRequest);

            Assert.AreEqual(ErrorReason.QuantityInvalid, (result.Value as UpdateResponse).ErrorReason);
        }

        [TestMethod]
        public void InvalidOrderId()
        {
            wareHouseApiController = new EPM.Mouser.Interview.Web.Controllers.WarehouseApi(warehouseRepository.Object);
            // var products = GetListOfProducts();
            UpdateQuantityRequest updateQuantityRequest = new UpdateQuantityRequest
            {
                Id = 23,
                Quantity = 100
            };
            warehouseRepository.Setup(w => w.Get(updateQuantityRequest.Id)).Returns(Task.FromResult((Product)null));

            var result = wareHouseApiController.OrderItem(updateQuantityRequest);

            Assert.AreEqual(ErrorReason.InvalidRequest, (result.Value as UpdateResponse).ErrorReason);
        }

        [TestMethod]
        public void NotEnoughQuantity()
        {
            wareHouseApiController = new EPM.Mouser.Interview.Web.Controllers.WarehouseApi(warehouseRepository.Object);
            // var products = GetListOfProducts();
            UpdateQuantityRequest updateQuantityRequest = new UpdateQuantityRequest
            {
                Id = 1,
                Quantity = 100
            };
            warehouseRepository.Setup(w => w.Get(updateQuantityRequest.Id)).Returns(Task.FromResult(new Product
            {
                Id = 1,
                InStockQuantity = 99
            }));

            var result = wareHouseApiController.OrderItem(updateQuantityRequest);

            Assert.AreEqual(ErrorReason.NotEnoughQuantity, (result.Value as UpdateResponse).ErrorReason);
        }

        [TestMethod]
        public void ReserveQuantityUpdatedSuccessfully()
        {
            wareHouseApiController = new EPM.Mouser.Interview.Web.Controllers.WarehouseApi(warehouseRepository.Object);
            // var products = GetListOfProducts();
            UpdateQuantityRequest updateQuantityRequest = new UpdateQuantityRequest
            {
                Id = 1,
                Quantity = 23
            };
            warehouseRepository.Setup(w => w.Get(updateQuantityRequest.Id)).Returns(Task.FromResult(new Product
            {
                Id = 1,
                InStockQuantity = 99
            }));

            var result = wareHouseApiController.OrderItem(updateQuantityRequest);

            Assert.AreEqual(true, (result.Value as UpdateResponse).Success);
        }

        [TestMethod]
        public void AddNewProduct_ReturnsUpdatedProductName()
        {
            var products = GetListOfProducts();
            warehouseRepository.Setup(w => w.List()).Returns(Task.FromResult(products));
            wareHouseApiController = new EPM.Mouser.Interview.Web.Controllers.WarehouseApi(warehouseRepository.Object);
            // var products = GetListOfProducts();
            var product = new Product
            {
                Id = 53,
                Name = "ProductX",
                InStockQuantity = 45,
                ReservedQuantity = 23
            };

            var result = wareHouseApiController.AddNewProduct(product);

            Assert.AreEqual("ProductX(3)", (result.Value as CreateResponse<Product>).Model.Name);
        }


        private List<Product> GetListOfProducts()
        {
            return new List<Product>()
            {
                new Product()
                {
                    Id = 1,
                    Name = "Product1",
                    ReservedQuantity = 8,
                    InStockQuantity= 0
                },
                new Product()
                {
                     Id = 2,
                    Name = "Product2",
                    ReservedQuantity = 8,
                    InStockQuantity= 5
                },
                 new Product()
                {
                      Id = 3,
                    Name = "Product3",
                    ReservedQuantity = 3,
                    InStockQuantity= 7
                },
                  new Product()
                {
                      Id = 4,
                    Name = "Product4",
                    ReservedQuantity = 2,
                    InStockQuantity= 12
                },
                   new Product()
                {
                      Id = 4,
                    Name = "ProductX",
                    ReservedQuantity = 2,
                    InStockQuantity= 12
                },
                 new Product()
                {
                      Id = 4,
                    Name = "ProductX(1)",
                    ReservedQuantity = 2,
                    InStockQuantity= 12
                },
                 new Product()
                {
                      Id = 4,
                    Name = "ProductX(2)",
                    ReservedQuantity = 2,
                    InStockQuantity= 12
                },
                    new Product()
                {
                      Id = 4,
                    Name = "ProductY(1)",
                    ReservedQuantity = 2,
                    InStockQuantity= 12
                },
                 new Product()
                {
                      Id = 4,
                    Name = "ProductY(2)",
                    ReservedQuantity = 2,
                    InStockQuantity= 12
                }
            };
        }
    }
}