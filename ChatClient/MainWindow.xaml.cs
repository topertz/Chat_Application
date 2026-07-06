using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Net.Http;
using System.Net.Http.Json;
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
using ChatClient.Models;

namespace ChatClient
{
    public partial class MainWindow : Window
    {
        private HubConnection _connection;
        private string _username = "";
        private string? _selectedUser;
        public MainWindow()
        {
            InitializeComponent();
            _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5090/chatHub", options =>
            {
                options.HttpMessageHandlerFactory = _ =>
                {
                    return new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                };
            })
            .WithAutomaticReconnect()
            .Build();
            _connection.On <string, string, string>("ReceiveMessage",
                (user, message, time) => 
            {
                Dispatcher.Invoke(() =>
                {
                    MessagesList.Items.Add($"[{time}] {user}: {message}");
                });
            });
        }

        private async Task LoadUsers()
        {
            using var client = new HttpClient();

            var users = await client.GetFromJsonAsync<List<UserDto>>("http://localhost:5090/api/users");

            UsersList.ItemsSource = null;
            UsersList.ItemsSource = users;
            UsersList.DisplayMemberPath = "Username";
        }

        private void UsersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UsersList.SelectedItem is UserDto user)
            {
                _selectedUser = user.Username;
            }
        }
        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            _username = UsernameBox.Text;

            if (string.IsNullOrWhiteSpace(_username))
            {
                MessageBox.Show("Give me your username!");
                return;
            }

            try
            {
                using var client = new HttpClient();
                await client.PostAsJsonAsync(
                    "http://localhost:5090/api/users/login",
                    new { Username = _username }
                );
                if (_connection.State == HubConnectionState.Disconnected)
                {
                    await _connection.StartAsync();
                }
                await _connection.InvokeAsync("Register", _username);
                await LoadUsers();

                MessageBox.Show("Connected!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            if (_connection.State != HubConnectionState.Connected)
            {
                MessageBox.Show("No connection!");
                return;
            }

            if (string.IsNullOrWhiteSpace(_selectedUser))
            {
                MessageBox.Show("Select a user first!");
                return;
            }

            if (string.IsNullOrWhiteSpace(MessageInput.Text))
            {
                return;
            }

            await _connection.SendAsync(
                "SendMessage",
                _username,
                _selectedUser,
                MessageInput.Text
            );

            MessageInput.Text = "";
        }
    }
}
