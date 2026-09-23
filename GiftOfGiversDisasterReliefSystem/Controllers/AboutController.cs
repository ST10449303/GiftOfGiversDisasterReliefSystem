using Microsoft.AspNetCore.Mvc;

namespace GiftOfGiversDisasterReliefSystem.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
