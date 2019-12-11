using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace FoodPortal.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<MenuController> _logger;

        public MenuController(IConfiguration config, ILogger<MenuController> logger)
        {
            _config = config;
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<MenuItem> Get()
        {
            string fileName = $"menuitems." + Environment.GetEnvironmentVariable("RID") + ".json";
            var jsonData = System.IO.File.ReadAllBytes(fileName);
            var jsonReader = new Utf8JsonReader(jsonData, isFinalBlock: true, state: default);
            return JsonSerializer.Deserialize<MenuItem[]>(ref jsonReader, new JsonSerializerOptions{PropertyNameCaseInsensitive=true});
        }
    }
}
