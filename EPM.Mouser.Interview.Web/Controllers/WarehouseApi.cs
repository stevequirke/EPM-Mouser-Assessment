using EPM.Mouser.Interview.Data;
using EPM.Mouser.Interview.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace EPM.Mouser.Interview.Web.Controllers
{

    [Route("api/warehouse")]
    public class WarehouseApi : Controller
    {
        IWarehouseRepository _wareHouseRepository;

        public WarehouseApi(IWarehouseRepository wareHouseRepository)
        {
            _wareHouseRepository = wareHouseRepository;
        }
        /*
         *  Action: GET
         *  Url: api/warehouse/id
         *  This action should return a single product for an Id
         */
        [Microsoft.AspNetCore.Mvc.HttpGet("{id}")]
        public JsonResult GetProduct(long id)
        {
            var product = _wareHouseRepository.Get(id).Result;
            return Json(product);
        }

        /*
         *  Action: GET
         *  Url: api/warehouse
         *  This action should return a collection of products in stock
         *  In stock means In Stock Quantity is greater than zero and In Stock Quantity is greater than the Reserved Quantity
         */
        [HttpGet]
        public JsonResult GetPublicInStockProducts()
        {
            var inStockProducts = _wareHouseRepository.Query(GetPublicInStockHelper).Result;
            return Json(inStockProducts);
        }

        //really put this in a private region.
        private bool GetPublicInStockHelper(Product product)
        {
            return product.InStockQuantity > 0 && product.InStockQuantity > product.ReservedQuantity;
        }


        /*
         *  Action: GET
         *  Url: api/warehouse/order
         *  This action should return a EPM.Mouser.Interview.Models.UpdateResponse
         *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.UpdateQuantityRequest in JSON format in the body of the request
         *       {
         *           "id": 1,
         *           "quantity": 1
         *       }
         *
         *  This action should increase the Reserved Quantity for the product requested by the amount requested
         *
         *  This action should return failure (success = false) when:
         *     - ErrorReason.NotEnoughQuantity when: The quantity being requested would increase the Reserved Quantity to be greater than the In Stock Quantity.
         *     - ErrorReason.QuantityInvalid when: A negative number was requested
         *     - ErrorReason.InvalidRequest when: A product for the id does not exist
        */
        [Route("order")]
        [HttpGet]
        public JsonResult OrderItem([FromBody] UpdateQuantityRequest updateQuantityrequest)
        {
            if (updateQuantityrequest.Quantity < 0)
            {
                return Json(new UpdateResponse
                {
                    Success = false,
                    ErrorReason = ErrorReason.QuantityInvalid
                });
            }

            var product = _wareHouseRepository.Get(updateQuantityrequest.Id).Result;

            if (product == null)
            {
                return Json(new UpdateResponse
                {
                    Success = false,
                    ErrorReason = ErrorReason.InvalidRequest
                });
            }
            if (updateQuantityrequest.Quantity + product.ReservedQuantity > product.InStockQuantity)
            {
                return Json(new UpdateResponse
                {
                    Success = false,
                    ErrorReason = ErrorReason.NotEnoughQuantity
                });
            }
            else
            {
                product.ReservedQuantity += updateQuantityrequest.Quantity;
                return Json(new UpdateResponse
                {
                    Success = true,
                });
            }
        }

        /*
         *  Url: api/warehouse/ship
         *  This action should return a EPM.Mouser.Interview.Models.UpdateResponse
         *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.UpdateQuantityRequest in JSON format in the body of the request
         *       {
         *           "id": 1,
         *           "quantity": 1
         *       }
         *
         *
         *  This action should:
         *     - decrease the Reserved Quantity for the product requested by the amount requested to a minimum of zero.
         *     - decrease the In Stock Quantity for the product requested by the amount requested
         *
         *  This action should return failure (success = false) when:
         *     - ErrorReason.NotEnoughQuantity when: The quantity being requested would cause the In Stock Quantity to go below zero.
         *     - ErrorReason.QuantityInvalid when: A negative number was requested
         *     - ErrorReason.InvalidRequest when: A product for the id does not exist
        */
        [Route("ship")]
        [HttpPatch]
        public JsonResult ShipItem([FromBody] UpdateQuantityRequest updateQuantityrequest)
        {
            if (updateQuantityrequest.Id == null)
            {
                return Json(new UpdateResponse
                {
                    Success = false,
                    ErrorReason = ErrorReason.InvalidRequest
                });
            }

            if (updateQuantityrequest.Id < 0)
            {
                return Json(new UpdateResponse
                {
                    Success = false,
                    ErrorReason = ErrorReason.QuantityInvalid
                });
            }

            var product = _wareHouseRepository.Get(updateQuantityrequest.Id).Result;
            if (updateQuantityrequest.Quantity > product.InStockQuantity)
            {
                return Json(new UpdateResponse
                {
                    Success = false,
                    ErrorReason = ErrorReason.NotEnoughQuantity
                });
            }
            else
            {
                product.InStockQuantity -= updateQuantityrequest.Quantity;
                product.ReservedQuantity = Math.Max(0, product.ReservedQuantity - updateQuantityrequest.Quantity);
                return Json(new UpdateResponse
                {
                    Success = false,
                });
            }
        }

        /*
        *  Url: api/warehouse/restock
        *  This action should return a EPM.Mouser.Interview.Models.UpdateResponse
        *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.UpdateQuantityRequest in JSON format in the body of the request
        *       {
        *           "id": 1,
        *           "quantity": 1
        *       }
        *
        *
        *  This action should:
        *     - increase the In Stock Quantity for the product requested by the amount requested
        *
        *  This action should return failure (success = false) when:
        *     - ErrorReason.QuantityInvalid when: A negative number was requested
        *     - ErrorReason.InvalidRequest when: A product for the id does not exist
        */

        [Route("restock")]
        [HttpPatch]
        public JsonResult RestockItem([FromBody] UpdateQuantityRequest updateQuantityrequest)
        {
            if (updateQuantityrequest.Id == null)
            {
                return Json(new UpdateResponse
                {
                    Success = false,
                    ErrorReason = ErrorReason.InvalidRequest
                });
            }

            if (updateQuantityrequest.Id < 0)
            {
                return Json(new UpdateResponse
                {
                    Success = false,
                    ErrorReason = ErrorReason.QuantityInvalid
                });
            }

            var product = _wareHouseRepository.Get(updateQuantityrequest.Id).Result;


            product.InStockQuantity += updateQuantityrequest.Quantity;
            _wareHouseRepository.UpdateQuantities(product);
            return Json(new UpdateResponse
            {
                Success = false,
            });
        }

        /*
        *  Url: api/warehouse/add
        *  This action should return a EPM.Mouser.Interview.Models.CreateResponse<EPM.Mouser.Interview.Models.Product>
        *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.Product in JSON format in the body of the request
        *       {
        *           "id": 1,
        *           "inStockQuantity": 1,
        *           "reservedQuantity": 1,
        *           "name": "product name"
        *       }
        *
        *
        *  This action should:
        *     - create a new product with:
        *          - The requested name - But forced to be unique - see below
        *          - The requested In Stock Quantity
        *          - The Reserved Quantity should be zero
        *
        *       UNIQUE Name requirements
        *          - No two products can have the same name
        *          - Names should have no leading or trailing whitespace before checking for uniqueness
        *          - If a new name is not unique then append "(x)" to the name [like windows file system does, where x is the next avaiable number]
        *
        *
        *  This action should return failure (success = false) and an empty Model property when:
        *     - ErrorReason.QuantityInvalid when: A negative number was requested for the In Stock Quantity
        *     - ErrorReason.InvalidRequest when: A blank or empty name is requested
        */
        [Route("add")]
        [HttpPost]
        public JsonResult AddNewProduct([FromBody] Product product)
        {
            if (product.InStockQuantity < 0 || string.IsNullOrWhiteSpace(product.Name))
            {
                return Json(new CreateResponse<Product>
                {
                    Success = false,
                    Model = new Product()
                });
            }

            var newProduct = new Product
            {
                Name = product.Name.Trim(' '),
                InStockQuantity = product.InStockQuantity,
                ReservedQuantity = product.ReservedQuantity,
            };

            var products = _wareHouseRepository.List().Result;

            var existingProduct = products.FirstOrDefault(p => p.Name == product.Name);

            if (existingProduct == null)
            {
                _wareHouseRepository.Insert(newProduct);
                return Json(new CreateResponse<Product>
                {
                    Success = false,
                    Model = newProduct
                });
            }

            string pattern = @"^.*\((\d+)\).*";
            Regex regex = new Regex(pattern);


            var productCount = products.Where(p=> p.Name.StartsWith(product.Name) && regex.IsMatch(p.Name)).Count();

            newProduct.Name = newProduct.Name + "(" + (productCount + 1) + ")";
            _wareHouseRepository.Insert(newProduct);

            return Json(new CreateResponse<Product>
            {
                Success = false,
                Model = newProduct
            });
        }
    }
}
