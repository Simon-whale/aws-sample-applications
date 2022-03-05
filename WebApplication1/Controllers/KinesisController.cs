using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

public class Kinesis : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}