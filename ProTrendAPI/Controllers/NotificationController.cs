using Microsoft.AspNetCore.Mvc;
using ProTrendAPI.Services.Network;

namespace ProTrendAPI.Controllers
{
    [Route("api/n")]
    [ApiController]
    [CookieAuthenticationFilter]
    public class NotificationController : BaseController
    {
        public NotificationController(IServiceProvider serviceProvider) : base(serviceProvider) { }

        [HttpGet("get/{id}")]
        public async Task<ActionResult<ActionResponse>> GetNotifications(Guid id)
        {
            return Ok(new ActionResponse { Successful = true, StatusCode = 200, Message = ActionResponseMessage.OK, Data = await _notificationService.GetNotificationsAsync(id) });
        }

        [HttpPut("set/viewed/{id}")]
        public async Task<ActionResult<ActionResponse>> SetNotificationViewed(Guid id)
        {
            var resultOk = await _notificationService.SetNotificationViewedAsync(id);
            if (!resultOk)
                return BadRequest(new ActionResponse { StatusCode = 400, Message = "Notification set viewed failed" });
            return Ok(new ActionResponse { Successful = true, StatusCode = 200, Message = "Notification set viewed ok" });
        }
    }
}
