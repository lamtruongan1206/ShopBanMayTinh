using Microsoft.AspNetCore.Mvc;
using SellComputer.Data;

namespace SellComputer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        protected readonly ShopBanMayTinhContext dbContext;

        public BaseApiController(ShopBanMayTinhContext dbContext)
        {
            this.dbContext = dbContext;
        }
    }
}
