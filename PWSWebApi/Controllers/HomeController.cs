using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace PWSWebApi.Controllers
{
    [Route("")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            return StatusCode((int)HttpStatusCode.Unauthorized, new { message = "Invalid Route" });
        }
    }
}