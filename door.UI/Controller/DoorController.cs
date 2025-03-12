using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using door.Domain.DTO;
using door.Domain.Repositories;
using System;


//エンドポイントを提供する
namespace door.UI.Controllers
{
    [ApiController]
    [Route("door")]
    
    public class DoorController : ControllerBase
    {
        private readonly IDataEntryService _dataEntryService;
        private static NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public DoorController(IDataEntryService dataEntryService)
        {
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

            // Discord通知を実行
            await _dataEntryService.NotificationStateChange($"[通知] {request.Date} {request.Time} 状態: {request.DoorStatusId}");

            return Ok(new { message = "Notification sent successfully" });
        }
    }
}
