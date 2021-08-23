using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LunchOrderingSystem.Server.Service
{
    public class DingTalkNotifier : IHostedService, IDisposable
    {
        private readonly ILogger<DingTalkNotifier> _logger;
        private readonly IServiceProvider _services;
        private readonly string _hostUrl;
        private Timer _timer;
        private bool _todayNotified = false;
        private bool _todaySentOrder = false;

        public DingTalkNotifier(ILogger<DingTalkNotifier> logger, IServiceProvider services, IConfiguration configuration)
        {
            _logger = logger;
            _services = services;
            _hostUrl = configuration.GetSection("HostUrl").Get<string>();
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(30));

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            if (DateTime.Now.Hour == 0)
            {
                _todayNotified = false;
                _todaySentOrder = false;
            }

            if (!_todayNotified && DateTime.Now.Hour == 12)
            {
                using var scope = _services.CreateScope();
                var dingTalkCaller = scope.ServiceProvider.GetRequiredService<DingTalkCaller>();
                var msg = $"点餐时间到，需要吃炒菜的请进行点餐：{_hostUrl}";
                await dingTalkCaller.SendTextMsgAsync(msg, true);
                _todayNotified = true;
            }

            if (!_todaySentOrder && DateTime.Now.Hour == 12 && DateTime.Now.Minute == 20)
            {
                using var scope = _services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<MenuDbContext>();
                var dingTalkCaller = scope.ServiceProvider.GetRequiredService<DingTalkCaller>();
                var orderContent = dbContext.OrderInfo.Where(item => item.OrderTime.Date == DateTime.Today).ToListAsync();
                await dingTalkCaller.SendTextMsgAsync(string.Join("\n", orderContent), true);
                _todaySentOrder = true;
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
