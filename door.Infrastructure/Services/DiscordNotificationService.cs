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
    public class DiscordNotificationService : INotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _webhookUrl;
        public event Action? OnDoorStateChanged; // UI更新用イベント

        public DiscordNotificationService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            //program.csからurlを取得
            _webhookUrl = configuration["Discord:WebhookUrl"] ?? throw new InvalidOperationException("Webhook URL is not set.");

        }


        /// <summary>
        /// 受け取ったメッセージをdiscord通知 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task NotificationStateChange(string stateMessage)
        {
            if (string.IsNullOrEmpty(_webhookUrl))
            {
                throw new Exception("Webhook URL is not configured.");
            }

            var payload = new { content = stateMessage };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_webhookUrl, content);

            // UIに通知
            OnDoorStateChanged?.Invoke();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to send Discord notification: {response.StatusCode}");
            }
        }




    }

}