using ChatAPI.Models;
using ChatClient.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Win32;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace ChatClient
{
    public partial class MainWindow : Window
    {
        private HubConnection _connection;
        private string _username = "";
        private string? _selectedUser;
        private string? _selectedFile;
        private Dictionary<string, ChatConversation> _conversations = new();
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
            _connection.On<string, string, string>("ReceiveMessage",
            (user, message, time) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (!_conversations.ContainsKey(user))
                    {
                        _conversations[user] = new ChatConversation
                        {
                            Username = user
                        };
                    }

                    _conversations[user].Messages.Add(
                        new ChatMessage
                        {
                            User = user,
                            Text = message,
                            Time = time,
                            IsMine = user == _username
                        });

                    if (_selectedUser == user)
                    {
                        MessagesList.ItemsSource =
                            _conversations[user].Messages;

                        MessagesList.ScrollIntoView(
                            MessagesList.Items[MessagesList.Items.Count - 1]);
                    }
                });
            });
        }
        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Send_Click(sender, e);
            }
        }

        private async Task LoadUsers()
        {
            using var client = new HttpClient();

            var users = await client.GetFromJsonAsync<List<UserDto>>("http://localhost:5090/api/users");

            UsersList.ItemsSource = null;
            UsersList.ItemsSource = users;
        }

        private void UsersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UsersList.SelectedItem is UserDto user)
            {
                _selectedUser = user.Username;

                ChatTitle.Text = user.Username;

                if (!_conversations.ContainsKey(user.Username))
                {
                    _conversations[user.Username] = new ChatConversation
                    {
                        Username = user.Username
                    };
                }


                MessagesList.ItemsSource =
                    _conversations[user.Username].Messages;
            }
        }

        private bool ValidatePassword(string password)
        {
            return Regex.IsMatch(
                password,
                @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$"
            );
        }

        private void Attach_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == true)
            {
                _selectedFile = dialog.FileName;

                MessageBox.Show($"Selected:\n{_selectedFile}");
            }
        }
        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidatePassword(PasswordBox.Password))
            {
                MessageBox.Show(
                    "Password must contain at least 8 characters, uppercase, lowercase, number and special character!"
                );
                return;
            }

            _username = UsernameBox.Text;

            if (string.IsNullOrWhiteSpace(_username))
            {
                MessageBox.Show("Give me your username!");
                return;
            }

            try
            {
                using var client = new HttpClient();
                var response = await client.PostAsJsonAsync(
                    "http://localhost:5090/api/users/login",
                    new
                    {
                        Username = _username,
                        Password = PasswordBox.Password
                    }
                );

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show(error);
                    return;
                }
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
            string? uploadedFile = null;

            if (!string.IsNullOrEmpty(_selectedFile))
            {
                using var client = new HttpClient();

                using var content = new MultipartFormDataContent();

                var fileContent = new StreamContent(File.OpenRead(_selectedFile));

                content.Add(fileContent, "file", Path.GetFileName(_selectedFile));

                var response = await client.PostAsync(
                    "http://localhost:5090/api/files/upload",
                    content);

                uploadedFile = await response.Content.ReadAsStringAsync();

                _selectedFile = null;
            }
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
                MessageInput.Text,
                uploadedFile
            );

            MessageInput.Text = "";
        }
    }
}
