using Microsoft.AspNetCore.Mvc; 

namespace VideoStreamingService.APIs
{
    [ApiController]
    [Route("Gateway")]
    public class Gateway : ControllerBase    
    {
        [HttpGet("Connect")]
        public async Task<IActionResult> Connect()
        {

        }
    }
}
