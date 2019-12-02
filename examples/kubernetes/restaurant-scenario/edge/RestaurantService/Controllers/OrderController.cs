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
    public class OrderController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IConfiguration config, ILogger<OrderController> logger)
        {
            _config = config;
            _logger = logger;
        }

        [HttpPost]
        public IEnumerable<OrderItem> Post(IEnumerable<OrderItem> order)
        {
            foreach( var item in order) {
              item.Status = 1;
            }
            return order;           
        }
    }
}
