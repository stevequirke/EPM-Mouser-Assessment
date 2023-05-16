using EPM.Mouser.Interview.Data;
using EPM.Mouser.Interview.Models;
using Microsoft.AspNetCore.Mvc;

namespace EPM.Mouser.Interview.Web.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        IWarehouseRepository _wareHouseRepository;
        public HomeController(IWarehouseRepository warehouseRepository)
        {
            _wareHouseRepository = warehouseRepository;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _wareHouseRepository.List();
            return View(products);
        }
    }
}
