using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using door.Infrastructure.Services;
using door.Domain.Repositories;

namespace door.Tests
{
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
        private IServiceProvider GetServiceProvider(Dictionary<string, string> settings, HttpClient httpClient = null)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddScoped<IDiscordNotificationService, DiscordNotificationService>();

            if (httpClient != null)
            {
                services.AddSingleton(httpClient);
            }

            return services.BuildServiceProvider();
        }

        [Fact]
        public async Task NotificationStateChange_SendsCorrectRequestAndInvokesEvent()
        {
            string testMessage = "Test state change message";
            bool eventInvoked = false;

            var handler = new TestHttpMessageHandler(async (request, cancellationToken) =>
            {
                Assert.Equal(HttpMethod.Post, request.Method);
                Assert.Equal("http://example.com/webhook", request.RequestUri.ToString());

                var jsonContent = await request.Content.ReadAsStringAsync();
                var payload = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
                Assert.NotNull(payload);
                Assert.True(payload.ContainsKey("content"));
                Assert.Equal(testMessage, payload["content"]);

                return new HttpResponseMessage(HttpStatusCode.OK);
            });

            var httpClient = new HttpClient(handler);

            var settings = new Dictionary<string, string>
            {
                {"Discord:WebhookUrl", "http://example.com/webhook"}
            };

            var serviceProvider = GetServiceProvider(settings, httpClient);
            var service = serviceProvider.GetRequiredService<IDiscordNotificationService>();

            if (service is DiscordNotificationService discordService)
            {
                var httpClientField = typeof(DiscordNotificationService)
                    .GetField("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance);
                httpClientField.SetValue(discordService, httpClient);

                discordService.OnDoorStateChanged += () => { eventInvoked = true; };

                await discordService.NotificationStateChange(testMessage);

                Assert.True(eventInvoked, "OnDoorStateChanged イベントが発火しませんでした。");
                Assert.True(handler.WasCalled, "HTTP リクエストが送信されませんでした。");
            }
        }

        [Fact]
        public async Task NotificationStateChange_ThrowsException_WhenWebhookUrlNotConfigured()
        {
            var settings = new Dictionary<string, string>
            {
                {"Discord:WebhookUrl", ""}
            };

            var serviceProvider = GetServiceProvider(settings);
            var service = serviceProvider.GetRequiredService<IDiscordNotificationService>();

            var exception = await Assert.ThrowsAsync<Exception>(() => service.NotificationStateChange("Test message"));
            Assert.Equal("Webhook URL is not configured.", exception.Message);
        }

        [Fact]
        public async Task NotificationStateChange_ThrowsException_WhenHttpRequestFails()
        {
            var handler = new TestHttpMessageHandler((request, cancellationToken) =>
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            });

            var httpClient = new HttpClient(handler);

            var settings = new Dictionary<string, string>
            {
                {"Discord:WebhookUrl", "http://example.com/webhook"}
            };

            var serviceProvider = GetServiceProvider(settings, httpClient);
            var service = serviceProvider.GetRequiredService<IDiscordNotificationService>();

            if (service is DiscordNotificationService discordService)
            {
                var httpClientField = typeof(DiscordNotificationService)
                    .GetField("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance);
                httpClientField.SetValue(discordService, httpClient);

                var exception = await Assert.ThrowsAsync<Exception>(() => discordService.NotificationStateChange("Test message"));
                Assert.Contains("Failed to send Discord notification", exception.Message);
            }
        }
    }
}
