using backgroundApp.Common;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace backgroundApp.Communication.SignalR
{
    internal class DataHub:Hub
    {
        // 静态，跨所有连接共享
        private static readonly ConcurrentDictionary<string, string> _clients = new();

        public override async Task OnConnectedAsync()
        {
            var ip = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString();

            _clients[Context.ConnectionId] = ip ?? "unknown";
            Logger.Log($"客户端连接: {Context.ConnectionId} ({ip})");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_clients.TryRemove(Context.ConnectionId, out var ip))
                Logger.Log($"客户端断开: {Context.ConnectionId} ({ip})");

            await base.OnDisconnectedAsync(exception);
        }

        // 客户端可调用此方法测试连通性
        public Task<string> Ping() => Task.FromResult("pong");
    }
}
