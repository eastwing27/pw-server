using Microsoft.AspNetCore.Mvc;

namespace pwServer.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return new StatusCodeResult(418);            
        }
    }
}
