using EPM.Mouser.Interview.Data;
using EPM.Mouser.Interview.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
//using System.Text.Json didnt work had to use NewtonSoft.;

namespace EPM.Mouser.Interview.Web.Controllers
{
    [Route("product")]
    public class ProductController : Controller
    {
        private readonly HttpClient _httpClient;

        public ProductController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public IActionResult Index()
        {
            return View();
        }

        
        [Microsoft.AspNetCore.Mvc.HttpGet("{id}")]
        public async Task<IActionResult> GetProductDetails(long id)
        {
           var productResponse = await _httpClient.GetAsync("https://localhost:7021/api/warehouse/" + id);
           var productdetails = productResponse.Content.ReadAsStringAsync().Result;
           var product = JsonConvert.DeserializeObject<Product>(productdetails);
           return View(product);
           
        }
    }
}
