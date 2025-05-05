using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using door.Domain.DTO;
using door.Infrastructure.Services;
using System.Text;
using NLog;
using door.Domain.Repositories;

//エンドポイントを提供する
namespace door.UI.Controllers
{
    [ApiController]
    [Route("door")]
    
    public class DoorController : ControllerBase
    {
        private static NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IDataEntryService _dataEntryService;
        private readonly INotificationService _notificationService;

        public DoorController(IDataEntryService dataEntryService, INotificationService notificationService)
        {
            _dataEntryService = dataEntryService;
            _notificationService = notificationService;
        }

        /// <summary>
        /// DB挿入処理
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("insert")]
        public async Task<IActionResult> ReqInsertEndpoint([FromBody] DataEntryRequestDto request)
        {
            _logger.Info("リクエスト来ました");
            if (request == null)
                return BadRequest("Invalid request");
            // データをDBに挿入
            await _dataEntryService.DataEntryInsertAsync(request);

            return Ok(new { message = "Data entry inserted successfully" });
        }
        /// <summary>
        /// Discord通知 通知前データ整形など準備を担当
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("notification")]
        public async Task<IActionResult> ReqDiscordNotification([FromBody] DataEntryRequestDto request)
        {
            if (request == null)
                return BadRequest("Invalid request");

            var dataEntryList = await _dataEntryService.DataEntryReqTempJointAsync(request);

            if (dataEntryList == null || !dataEntryList.Any())
                return NotFound("No matching data found");

            var entry = dataEntryList.First(); // 最新データ1件のみで通知
            var statusEmoji = entry.StatusName.Contains("開") ? "🟢" : "🔴"; // 「開」「閉」で判断（日本語でも対応）
            var statusLabel = entry.StatusName;

            var message = new StringBuilder();
            message.AppendLine($"{statusEmoji} ドアが「{statusLabel}」になりました！");
            message.AppendLine($"📅 {entry.Date} 🕒 {entry.Time}");

            await _notificationService.NotificationStateChange(message.ToString());

            return Ok(new { message = "Notification sent successfully" });
        }
    }
}
