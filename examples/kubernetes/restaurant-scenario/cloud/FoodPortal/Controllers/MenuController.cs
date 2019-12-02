using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoodPortal.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly ILogger<MenuController> _logger;

        public MenuController(ILogger<MenuController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<MenuItem>> Get([FromQuery] string rid)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var rsp = await httpClient.GetAsync("http://localhost:8001/menu");
                    var bytes = await rsp.Content.ReadAsStreamAsync();
                    return await JsonSerializer.DeserializeAsync<MenuItem[]>(bytes, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Failed request");
                throw;
            }
        }
    }
}
