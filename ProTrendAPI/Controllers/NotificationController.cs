using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProTrendAPI.Services;
using ProTrendAPI.Services.Network;

namespace ProTrendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CookieAuthenticationFilter]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _notificationService;
        public NotificationController(NotificationService service)
        {
            _notificationService = service;
        }

        [HttpGet("get/{id}")]
        public async Task<ActionResult<List<Notification>>> GetNotifications(Guid id)
        {
            return Ok(await _notificationService.GetNotificationsAsync(id));
        }

        [HttpPut("set/viewed/{id}")]
        public async Task<IActionResult> SetNotificationViewed(Guid id)
        {
            await _notificationService.SetNotificationViewedAsync(id);
            return Ok(new BasicResponse { Status = Constants.OK, Message = Constants.Success });
        }
    }
}
