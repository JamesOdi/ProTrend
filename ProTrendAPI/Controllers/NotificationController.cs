using Microsoft.AspNetCore.Mvc;
using ProTrendAPI.Services.Network;

namespace ProTrendAPI.Controllers
{
    [Route("api/n")]
    [ApiController]
    [ProTrndAuthorizationFilter]
    public class NotificationController : BaseController
    {
        public NotificationController(IServiceProvider serviceProvider) : base(serviceProvider) { }

        [HttpGet("get/{id}")]
        public async Task<ActionResult<List<Notification>>> GetNotifications(Guid id)
        {
            return Ok(await _notificationService.GetNotificationsAsync(id));
        }

        [HttpPut("set/viewed/{id}")]
        public async Task<IActionResult> SetNotificationViewed(Guid id)
        {
            var resultOk = await _notificationService.SetNotificationViewedAsync(id);
            if (resultOk)
                return Ok(new BasicResponse { Success = true, Message = "Notification sent" });
            return BadRequest(new BasicResponse { Message = "Notification not sent" });
        }
    }
}
