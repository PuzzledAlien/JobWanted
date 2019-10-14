using Microsoft.AspNetCore.Mvc;

namespace JobWanted.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        } 
    }
}
