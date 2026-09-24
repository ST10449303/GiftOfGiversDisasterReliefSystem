using Microsoft.AspNetCore.Mvc;


// About feature updated for Azure Repos branching task
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
