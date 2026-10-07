using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace backgroundApp
{
    internal class TaskInstaller
    {
        //后台任务类
        public static void Installer(bool runStartup)
        {
            string taskName = "数据检测平台后台";
            //string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "xl的后台采集程序.exe");

            //string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;//

            string exePath = Environment.ProcessPath!;

            try
            {
                using (var ts = new TaskService())
                {
                    // 先尝试删除旧任务（路径可能已变）
                    try { ts.RootFolder.DeleteTask(taskName, false); } catch { }

                    var td = ts.NewTask();
                    td.RegistrationInfo.Description = "数据检测平台后台";
                    td.Settings.ExecutionTimeLimit = TimeSpan.Zero;          // 无运行时间限制
                    td.Settings.DisallowStartIfOnBatteries = false;
                    td.Settings.StopIfGoingOnBatteries = false;
                    td.Settings.AllowHardTerminate = false;
                    td.Settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew;
                    td.Settings.RestartInterval = TimeSpan.FromMinutes(1);   // 失败后 1 分钟重启
                    td.Settings.RestartCount = 3;

                    if (runStartup)
                    {
                        // 系统启动时运行（需要管理员权限，以 SYSTEM 身份运行）
                        td.Triggers.Add(new BootTrigger { Delay = TimeSpan.FromSeconds(30) });
                        td.Principal.UserId = "SYSTEM";
                        td.Principal.LogonType = TaskLogonType.ServiceAccount;
                        td.Principal.RunLevel = TaskRunLevel.Highest;
                    }
                    else
                    {
                        // 用户登录时运行（无需管理员权限）
                        td.Triggers.Add(new LogonTrigger { UserId = WindowsIdentity.GetCurrent().Name });
                        td.Principal.UserId = WindowsIdentity.GetCurrent().Name;
                        td.Principal.LogonType = TaskLogonType.InteractiveToken;
                    }

                    td.Actions.Add(new ExecAction(exePath, null, null));

                    // 注册任务（存在则更新）
                    ts.RootFolder.RegisterTaskDefinition(
                        taskName,
                        td,
                        TaskCreation.CreateOrUpdate,
                        null, null,                      // 使用 td.Principal 中的设置
                        TaskLogonType.InteractiveToken); // 对于 SYSTEM 任务，此参数会被忽略

                }
            }
            catch (Exception ex)
            {
                try
                {
                    System.IO.File.AppendAllText(
                        System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "install.log"),
                        $"{DateTime.Now} 注册任务失败: {ex.Message}\n");
                }
                catch
                {

                }
            }



        }

        public static void UninstallTask()
        {
            string taskName = "数据检测平台后台";
            using (var ts = new TaskService())
            {
                ts.RootFolder.DeleteTask(taskName, false);
            }
        }
    }

}
