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



//エンドポイントを提供する
namespace door.UI.Controllers
{
    [ApiController]
    [Route("door")]
    
    public class DoorController : ControllerBase
    {
        private readonly IDataEntryService _dataEntryService;
        private static NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly DoorDbContext _context;

        public DoorController(DoorDbContext context,IDataEntryService dataEntryService)
        {
            _context = context;
            _dataEntryService = dataEntryService;
        }

        [HttpPost("insert")]
        public async Task<IActionResult> InsertDataEntry([FromBody] DataEntryRequestDto request)
        {
            _logger.Info("リクエスト来ました");
            if (request == null)
                return BadRequest("Invalid request");
            
            // データをDBに挿入
            await _dataEntryService.DataEntryInsert(request);

            return Ok(new { message = "Data entry inserted successfully" });
        }

        [HttpPost("notification")]
        public async Task<IActionResult> SendNotification([FromBody] DataEntryRequestDto request)
        {
            if (request == null)
                return BadRequest("Invalid request");

            // データ変換
            DataEntrySQLiteService dataEntrySQLiteService = new DataEntrySQLiteService(_context);
            var dataEntryList = await dataEntrySQLiteService.DataEntryReqTempJointAsync(request);

            if (dataEntryList == null || !dataEntryList.Any())
                return NotFound("No matching data found");

            // DTOリストをDiscord通知用に整形
            var message = new StringBuilder();
            message.AppendLine("🔔 [テスト通知だお]");
            foreach (var entry in dataEntryList)
            {
                message.AppendLine($"📅 日付: {entry.Date} 🕒 時間: {entry.Time} 🏷 状態: {entry.StatusName}");
            }

            // Discord通知を実行
            await _dataEntryService.NotificationStateChange(message.ToString());

            return Ok(new { message = "Notification sent successfully" });
        }
    }
}
