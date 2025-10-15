using Microsoft.AspNetCore.Mvc;

namespace XtalPlayer.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Player()
        {
            return View();
        }
    }
}
