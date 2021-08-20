using LunchOrderingSystem.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LunchOrderingSystem.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly ILogger<MenuController> _logger;
        private readonly MenuDbContext _dbContext;
        private readonly DbSet<OrderInfo> _orderInfo;
        private readonly string _remoteIpAddress;

        public MenuController(ILogger<MenuController> logger, MenuDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _dbContext = dbContext;
            _orderInfo = dbContext.OrderInfo;
            _remoteIpAddress = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
        }

        [HttpGet]
        public async Task<IEnumerable<MenuItem>> Get()
        {
            var currentOrders = await _orderInfo.Where(item => item.OrderTime.Date == DateTime.Today).ToListAsync();

            if (currentOrders?.Count > 0)
            {
                var result = LazyStaticResources.MenuInfoData.DeepClone();

                foreach (var order in currentOrders)
                {
                    var menu = result.FirstOrDefault(item => item.ID == order.MenuId);
                    if (menu != null)
                    {
                        menu.HasBeenPick = true;
                        if (order.UserIP == _remoteIpAddress)
                        {
                            menu.PickByMyself = true;
                        }
                    }
                }

                return result;
            }

            return LazyStaticResources.MenuInfoData;
        }

        [HttpDelete]
        public async Task<ResponseResult> CancelOrderByMenuId(int menuId)
        {
            var result = new ResponseResult
            {
                Success = false,
                Msg = "请刷新页面后重试"
            };

            var currentOrder = await _orderInfo.SingleOrDefaultAsync(item => item.MenuId == menuId && item.OrderTime.Date == DateTime.Today);

            if (currentOrder != null)
            {
                _orderInfo.Remove(currentOrder);
                await _dbContext.SaveChangesAsync();
                result.Success = true;
                result.Msg = "取消成功";
                return result;
            }

            return result;
        }

        [HttpPost]
        public async Task<ResponseResult> Pick(Order data)
        {
            var result = new ResponseResult
            {
                Success = false,
                Msg = "请刷新页面后重试"
            };

            var hasOrder = await _orderInfo.AnyAsync(item => item.UserIP == _remoteIpAddress && item.OrderTime.Date == DateTime.Today);

            if (hasOrder)
            {
                result.Msg = "今天您已经点过菜了，请取消后再点";
                return result;
            }

            if (data != null)
            {
                var menuId = data.Id;
                var order = await _orderInfo.SingleOrDefaultAsync(item => item.MenuId == menuId && item.OrderTime.Date == DateTime.Today);
                if (order != null)
                {
                    result.Msg = "今天这个菜已经被点，请刷新页面后重试";
                }
                else
                {
                    _orderInfo.Add(new OrderInfo
                    {
                        OrderTime = DateTime.Now,
                        MenuId = menuId,
                        UserIP = _remoteIpAddress
                    });

                    await _dbContext.SaveChangesAsync();
                    result.Success = true;
                    result.Msg = "点餐成功";
                    return result;
                }
            }

            return result;
        }
    }
}
