using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodPortal.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MenuController : ControllerBase
    {
        private static readonly MenuItem[] menuItems = new MenuItem[]
        {
            new MenuItem {
                Num = "1",
                Name = "Calzone",
                Description = "Do you want to know?",
                Price = 8
            }
        };

        private readonly ILogger<MenuController> _logger;

        public MenuController(ILogger<MenuController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<MenuItem> Get()
        {
            return menuItems;
        }
    }
}
