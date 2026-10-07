using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using data_monitor.Model;
using Microsoft.AspNetCore.SignalR.Client;

namespace data_monitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HubConnection? _connection;
        public MainWindow()
        {
            InitializeComponent();
            _ = ConnectAsync();
        }
        private async Task ConnectAsync()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("http://192.168.0.108:5000/dataHub")   // 例如 http://192.168.1.50:5000/dataHub
                .WithAutomaticReconnect()
                .Build();

            // 接收服务端推送
            _connection.On<MyData>("ReceiveData", data =>
            {
                Dispatcher.Invoke(() =>
                {
                    MyTextBlock.Text = $"时间:{data.Time:hh:mm:ss}"+"/n"+$"值:{data.Value}";
                });
            });

            try
            {
                await _connection.StartAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"连接失败: {ex.Message}");
            }
        }
    }
}