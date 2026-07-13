using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookstaGram.Controllers
{
    [Authorize]
    public class UserOrderController : Controller
    {
        private readonly IUserOrderService _userOrderService;

        public UserOrderController(IUserOrderService userOrderRepo)
        {
            _userOrderService = userOrderRepo;
        }
        public async Task<IActionResult> UserOrders()
        {
            var orders = await _userOrderService.UserOrders();
            return View(orders);
        }
    }
}
