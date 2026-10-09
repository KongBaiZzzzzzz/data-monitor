
using backgroundApp.Common;
using backgroundApp.Communication.SignalR;
using Microsoft.VisualBasic;


namespace backgroundApp
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "数据检测平台后台.log");
            File.AppendAllText(logPath, $"{DateTime.Now}: 后台进程启动\n");

            // 每次启动都确保任务指向当前路径（换目录也能自动修复）
            TaskInstaller.Installer(runStartup: false);

            // 启动 SignalR 服务
            var server = new SignalRServer();
            server.Start();

            bool endFlag = true;

            // 命令行支持：Program.exe --install / --uninstall
            if (args.Contains("--install")) { TaskInstaller.Installer(false); return; }
            if (args.Contains("--uninstall")) { TaskInstaller.UninstallTask(); return; }

            while (true)
            {
                while (endFlag)
                {
                    int i = 0;
                    for (i = 0; i < 10; i++)
                    {
                        // 你的后台业务逻辑
                        Thread.Sleep(1000);
                        File.AppendAllText(logPath, $"{DateTime.Now}: 运行中\n");
                    }

                    if (i >= 10)
                    {
                        endFlag = false;
                        //TaskInstaller.UninstallTask();
                    }




                }

                try
                {
                    // 1. 采集/计算数据（这里换成你的真实逻辑）
                    var data = CollectData();

                    // 2. 推送给所有前端
                    _ = SignalRHubContext.BroadcastAsync("ReceiveData", data);


                }
                catch (Exception ex)
                {
                    Logger.Log($"采集/推送失败: {ex.Message}");
                }

                Thread.Sleep(1000); 
            }
        }



        private static object CollectData()
        {
            return new
            {
                Time = DateTime.Now,
                Value = new Random().Next(0, 10000),   // 示例
                Status = "正常"
            };
        }
    }
}
