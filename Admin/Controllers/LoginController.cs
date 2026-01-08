using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
