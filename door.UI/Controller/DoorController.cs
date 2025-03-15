using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using door.Domain.DTO;
using door.Domain.Repositories;
using System;
using door.Infrastructure.SQLite;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using door.Infrastructure;
using System.Text;
using door.Infrastructure.Services;
using Microsoft.Extensions.Configuration;


//エンドポイントを提供する
namespace door.UI.Controllers
{
    [ApiController]
    [Route("door")]
    
    public class DoorController : ControllerBase
    {
      
        private static NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly DoorDbContext _context;
        private readonly DiscordNotificationService _discordNotificationService;
        private readonly DataEntrySQLiteService _dataEntrySQLiteService;



        //public DoorController(DoorDbContext context, DiscordNotificationService discordNotificationService, DataEntrySQLiteService dataEntrySQLiteService)
        //{
        //    _context = context;
        //    _discordNotificationService = discordNotificationService;
        //    _dataEntrySQLiteService = dataEntrySQLiteService;
        //}

        public DoorController(DataEntrySQLiteService dataEntrySQLiteService, DiscordNotificationService discordNotificationService)
        {
            _dataEntrySQLiteService = dataEntrySQLiteService;
            _discordNotificationService = discordNotificationService;
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
            //CameraNotificationService層の処理を呼ぶ
            // データをDBに挿入
            //DataEntrySQLiteService _dataEntrySQLiteService = new DataEntrySQLiteService(_context);
            await _dataEntrySQLiteService.DataEntryInsertAsync(request);

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
            //DataEntrySQLiteService dataEntrySQLiteService = new DataEntrySQLiteService(_context);
            var dataEntryList = await _dataEntrySQLiteService.DataEntryReqTempJointAsync(request);

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
            //await _discordNotificationService.HandleDoorStateChange(message.ToString());
            await _discordNotificationService.NotificationStateChange(message.ToString());
            
            return Ok(new { message = "Notification sent successfully" });
        }
    }
}
