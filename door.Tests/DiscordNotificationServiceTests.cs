using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Xunit;
using door.Infrastructure.Services;

namespace door.Tests
{
    // カスタム HttpMessageHandler: テスト時に HTTP リクエストを捕捉して任意のレスポンスを返す
    public class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handlerFunc;
        public bool WasCalled { get; private set; }

        public TestHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc)
        {
            _handlerFunc = handlerFunc;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            WasCalled = true;
            return _handlerFunc(request, cancellationToken);
        }
    }

    public class DiscordNotificationServiceTests
    {
        [Fact]
        public async Task NotificationStateChange_SendsCorrectRequestAndInvokesEvent()
        {
            // Arrange
            string testMessage = "Test state change message";
            bool eventInvoked = false;

            // カスタムハンドラ: リクエスト内容の検証を行い、200 OK を返す
            var handler = new TestHttpMessageHandler(async (request, cancellationToken) =>
            {
                // HTTP メソッドが POST であること
                Assert.Equal(HttpMethod.Post, request.Method);

                // 設定した Webhook URL が使用されていること
                Assert.Equal("http://example.com/webhook", request.RequestUri.ToString());

                // リクエストボディの JSON 内容の検証
                var jsonContent = await request.Content.ReadAsStringAsync();
                // JSON から動的にデシリアライズして "content" プロパティを検証する例
                var payload = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
                Assert.NotNull(payload);
                Assert.True(payload.ContainsKey("content"));
                Assert.Equal(testMessage, payload["content"]);

                return new HttpResponseMessage(HttpStatusCode.OK);
            });

            var httpClient = new HttpClient(handler);

            // 設定用の in-memory configuration を用意
            var inMemorySettings = new Dictionary<string, string>
            {
                {"Discord:WebhookUrl", "http://example.com/webhook"}
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // テスト対象のサービスを生成
            var service = new DiscordNotificationService(configuration);

            // 反射を用いて内部の _httpClient フィールドをテスト用の HttpClient に置き換え
            var httpClientField = typeof(DiscordNotificationService)
                .GetField("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance);
            httpClientField.SetValue(service, httpClient);

            // イベントハンドラの登録（イベントが発火するかを確認するため）
            service.OnDoorStateChanged += () => { eventInvoked = true; };

            // Act
            await service.NotificationStateChange(testMessage);

            // Assert
            Assert.True(eventInvoked, "OnDoorStateChanged イベントが発火しませんでした。");
            Assert.True(handler.WasCalled, "HTTP リクエストが送信されませんでした。");
        }

        [Fact]
        public async Task NotificationStateChange_ThrowsException_WhenWebhookUrlNotConfigured()
        {
            // Arrange: Webhook URL が設定されていない（空文字）の場合
            var inMemorySettings = new Dictionary<string, string>
            {
                {"Discord:WebhookUrl", ""}
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var service = new DiscordNotificationService(configuration);

            // Act & Assert: Webhook URL が未設定の場合、例外が発生すること
            var exception = await Assert.ThrowsAsync<Exception>(() => service.NotificationStateChange("Test message"));
            Assert.Equal("Webhook URL is not configured.", exception.Message);
        }

        [Fact]
        public async Task NotificationStateChange_ThrowsException_WhenHttpRequestFails()
        {
            // Arrange: 失敗する HTTP レスポンス（500 InternalServerError）を返すハンドラを設定
            var handler = new TestHttpMessageHandler((request, cancellationToken) =>
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            });
            var httpClient = new HttpClient(handler);

            var inMemorySettings = new Dictionary<string, string>
            {
                {"Discord:WebhookUrl", "http://example.com/webhook"}
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var service = new DiscordNotificationService(configuration);
            // 反射を用いて内部の HttpClient を差し替え
            var httpClientField = typeof(DiscordNotificationService)
                .GetField("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance);
            httpClientField.SetValue(service, httpClient);

            // Act & Assert: HTTP リクエストが失敗した場合、例外が発生することを検証
            var exception = await Assert.ThrowsAsync<Exception>(() => service.NotificationStateChange("Test message"));
            Assert.Contains("Failed to send Discord notification", exception.Message);
        }
    }
}
