using System.Net.Sockets;
using System.Net;
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
using System.Linq;
using Client;

namespace HW_NP_1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ServerClient _serverClient = new ServerClient();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void connectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsValidIPAddress(iPTextBox.Text) || !IsValidPort(portTextBox.Text, out int port))
            {
                MessageBox.Show("Некоректна IP-адреса або порт.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                await _serverClient.ConnectAsync(iPTextBox.Text, port);
                MessageBox.Show("Успішно підключено до сервера.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалося підключитися: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void sendButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(messageTextBox.Text))
            {
                MessageBox.Show("Повідомлення не може бути порожнім.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                string response = await _serverClient.SendAndReceiveAsync(messageTextBox.Text);
                serverMessagesDataGrid.Items.Add(new { Message = response });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка під час надсилання повідомлення: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsValidIPAddress(string ipAddress) => IPAddress.TryParse(ipAddress, out var address);

        private bool IsValidPort(string portText, out int port) => Int32.TryParse(portText, out port) && port > 0 && port <= IPEndPoint.MaxPort;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _serverClient.Close();
        }
    }
}