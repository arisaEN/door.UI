using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using door.Domain.DTO;
using door.Domain.Entities;
using door.Domain.Repositories;


using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using door.Infrastructure.SQLite;

namespace door.Infrastructure.Services
{
    public class CameraNotificationService
    {
        private readonly DoorDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly string _webhookUrl;

        public CameraNotificationService(DoorDbContext context)
        {
            _context = context;
            _httpClient = new HttpClient();
          
        }
        /// <summary>
        /// DB挿入用
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        //public async Task DataEntryInsert(DataEntryRequestDto request)
        //{
        //    //挿入処理 SQLiteに処理はまかせる
        //    DataEntrySQLiteService dataEntrySQLiteService = new DataEntrySQLiteService(_context);
        //    await dataEntrySQLiteService.DataEntryInsertAsync(request);
        //}

        /// <summary>
        /// 受け取ったメッセージをdiscord通知 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task NotificationStateChange(string message)
        {
            if (string.IsNullOrEmpty(_webhookUrl))
            {
                throw new Exception("Webhook URL is not configured.");
            }

            var payload = new { content = message };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_webhookUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to send Discord notification: {response.StatusCode}");
            }
        }
    }
}