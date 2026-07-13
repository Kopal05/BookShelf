using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


// This is the controller correspoiding to all the actions corresponding to admin

namespace BookstaGram.Controllers;
// Adding the authorization to admin only role

[Authorize(Roles = nameof(Roles.Admin))]
public class AdminOperationsController : Controller
{
    private readonly IUserOrderService _userOrderService;

    // Constructor injection

    public AdminOperationsController(IUserOrderService userOrderService)
    {
        _userOrderService = userOrderService;
    }


    // Show all the orders to the user

    public async Task<IActionResult> AllOrders()
    {
        var orders = await _userOrderService.UserOrders(true);
        return View(orders);
    }

    // This action is corresponding to toggling to payment status 
    // also the exception is being handled

    public async Task<IActionResult> TogglePaymentStatus(int orderId)
    {
        try
        {
            await _userOrderService.TogglePaymentStatus(orderId);
        }
        catch (Exception ex)
        {
            // YAtendra will log it later 
        }
        return RedirectToAction(nameof(AllOrders));
    }


    // Admin can update the status of the order given orderID as the parameter 
    public async Task<IActionResult> UpdateOrderStatus(int orderId)
    {
        var order = await _userOrderService.GetOrderById(orderId);
        if (order == null)
        {
            throw new InvalidOperationException($"Order with id:{orderId} does not found.");
        }
        var orderStatusList = (await _userOrderService.GetOrderStatuses()).Select(orderStatus =>
        {
            return new SelectListItem { Value = orderStatus.Id.ToString(), Text = orderStatus.StatusName, Selected = order.OrderStatusId == orderStatus.Id };
        });
        var data = new UpdateOrderStatusModel
        {
            OrderId = orderId,
            OrderStatusId = order.OrderStatusId,
            OrderStatusList = orderStatusList
        };
        return View(data);
    }




    [HttpPost]
    public async Task<IActionResult> UpdateOrderStatus(UpdateOrderStatusModel data)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                data.OrderStatusList = (await _userOrderService.GetOrderStatuses()).Select(orderStatus =>
                {
                    return new SelectListItem { Value = orderStatus.Id.ToString(), Text = orderStatus.StatusName, Selected = orderStatus.Id == data.OrderStatusId };
                });

                return View(data);
            }
            await _userOrderService.ChangeOrderStatus(data);
            TempData["msg"] = "Updated successfully";
        }
        catch (Exception ex)
        {
            // catch exception here
            TempData["msg"] = "Something went wrong";
        }
        return RedirectToAction(nameof(UpdateOrderStatus), new { orderId = data.OrderId });
    }

    public IActionResult Book()
    {
        return View();
    }

}
