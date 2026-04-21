using Microsoft.AspNetCore.Mvc;

namespace TechStore.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
    }
}
