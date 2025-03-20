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
        private readonly IDataEntrySQLiteService _dataEntryService;
        private readonly IDiscordNotificationService _notificationService;

        public DoorController(IDataEntrySQLiteService dataEntryService, IDiscordNotificationService notificationService)
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
            //CameraNotificationService層の処理を呼ぶ
            if (request == null)
                return BadRequest("Invalid request");

            // データ変換
            var dataEntryList = await _dataEntryService.DataEntryReqTempJointAsync(request);

            if (dataEntryList == null || !dataEntryList.Any())
                return NotFound("No matching data found");

            // データ編加算されたリストDTOをDiscord通知用に整形
            var message = new StringBuilder();
            message.AppendLine("🔔 [テスト通知だお]");
            foreach (var entry in dataEntryList)
            {
                message.AppendLine($"📅 日付: {entry.Date} 🕒 時間: {entry.Time} 🏷 状態: {entry.StatusName}");
            }

            // Discord通知を実行            
            await _notificationService.NotificationStateChange(message.ToString());
            
            return Ok(new { message = "Notification sent successfully" });
        }
    }
}
