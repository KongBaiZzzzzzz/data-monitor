using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace backgroundApp.Communication.SignalR
{
    internal class SignalRServer
    {
        private WebApplication? _app;

        public void Start()
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                ContentRootPath = AppDomain.CurrentDomain.BaseDirectory
            });

            // 监听所有网卡的 5000 端口（跨网段的关键）
            builder.WebHost.UseUrls("http://0.0.0.0:5000");

            builder.Services.AddSignalR();

            // 静默 ASP.NET Core 自带日志（避免写控制台）
            builder.Logging.ClearProviders();

            _app = builder.Build();

            _app.MapHub<DataHub>("/dataHub");

            // 保存 HubContext，方便服务端主动广播
            SignalRHubContext.Current =
                _app.Services.GetRequiredService<IHubContext<DataHub>>();

            // 后台运行，不阻塞
            _ = _app.RunAsync();

            Program.Log("SignalR 服务已启动: http://0.0.0.0:5000/dataHub");
        }

        public void Stop()
        {
            try { _app?.StopAsync().Wait(3000); } catch { }
        }
    }
    internal static class SignalRHubContext
    {
        public static IHubContext<DataHub>? Current { get; set; }

        public static async Task BroadcastAsync(string method, object data)
        {
            if (Current != null)
                await Current.Clients.All.SendAsync(method, data);
        }
    }
}
