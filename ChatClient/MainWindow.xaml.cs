using ChatShared.Validators;
using ChatShared.Models;
using ChatClient.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.AspNetCore.Http.Connections;

namespace ChatClient
{
    public partial class MainWindow : Window
    {
        private HubConnection _connection;
        private string _username = "";
        private string? _selectedUser;
        private string? _selectedFile;
        private string? _token;
        private HashSet<string> _onlineUsers = new();
        public MainWindow()
        {
            InitializeComponent();
            _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5090/chatHub", options =>
            {
                options.Transports =
                    HttpTransportType.WebSockets;

                options.AccessTokenProvider = () =>
                {
                    return Task.FromResult(_token);
                };

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
            _connection.On<string, string, string, string?>(
            "ReceiveMessage",
            (user, message, time, file) =>
            {
                Dispatcher.Invoke(async () =>
                {
                    if (_selectedUser == user)
                    {
                        await LoadMessages(user);
                    }
                });
            });
            _connection.On<string>(
            "UserConnected",
            async (username) =>
            {
                await Dispatcher.Invoke(async () =>
                {
                    _onlineUsers.Add(username);

                    if (_selectedUser == username)
                    {
                        OnlineStatusText.Text = "Online";
                        OnlineStatusText.Foreground = Brushes.LimeGreen;
                    }

                    await LoadUsers();
                });
            });


            _connection.On<string>(
            "UserDisconnected",
            async (username) =>
            {
                await Dispatcher.Invoke(async () =>
                {
                    _onlineUsers.Remove(username);

                    if (_selectedUser == username)
                    {
                        OnlineStatusText.Text = "Offline";
                        OnlineStatusText.Foreground = Brushes.Gray;
                    }

                    await LoadUsers();
                });
            });

            _connection.On<List<string>>(
            "OnlineUsers",
            (users) =>
            {
                Dispatcher.Invoke(async () =>
                {
                    _onlineUsers = users.ToHashSet();

                    await LoadUsers();
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

        private void File_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb &&
                !string.IsNullOrWhiteSpace(tb.Text))
            {
                try
                {
                    System.Diagnostics.Process.Start(
                        new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = tb.Text,
                            UseShellExecute = true
                        });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn &&
               btn.Tag is string url)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
        }

        private async Task LoadUsers()
        {
            using var client = new HttpClient();

            var users = await client.GetFromJsonAsync<List<UserDto>>("http://localhost:5090/api/users")
                ?? new List<UserDto>();

            foreach(var user in users)
            {
                user.IsOnline = _onlineUsers.Contains(user.Username);
            }

            UsersList.ItemsSource = null;
            UsersList.ItemsSource = users;
        }

        private async Task LoadMessages(string otherUser)
        {
            try
            {
                using var client = new HttpClient();

                var messages =
                    await client.GetFromJsonAsync<List<MessageDto>>(
                        $"http://localhost:5090/api/messages/{_username}/{otherUser}"
                    );


                if (messages == null)
                    return;


                foreach (var msg in messages)
                {
                    msg.IsMine = msg.Sender == _username;
                }


                MessagesList.ItemsSource = messages;


                if (MessagesList.Items.Count > 0)
                {
                    MessagesList.ScrollIntoView(
                        MessagesList.Items[MessagesList.Items.Count - 1]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Loading messages failed: " + ex.Message);
            }
        }

        private async void UsersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UsersList.SelectedItem is UserDto user)
            {
                _selectedUser = user.Username;

                ChatTitle.Text = user.Username;


                if (user.IsOnline)
                {
                    OnlineStatusText.Text = "Online";
                    OnlineStatusText.Foreground = Brushes.LimeGreen;
                }
                else
                {
                    OnlineStatusText.Text = "Offline";
                    OnlineStatusText.Foreground = Brushes.Gray;
                }


                await LoadMessages(user.Username);
            }
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

        private void Image_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image img && img.DataContext is MessageDto message)
            {
                if (!string.IsNullOrEmpty(message.FileUrl))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = message.FileUrl,
                        UseShellExecute = true
                    });
                }
            }
        }

        private async void Download_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.Tag is string url)
                {
                    using HttpClient client = new();

                    var bytes = await client.GetByteArrayAsync(url);

                    SaveFileDialog dialog = new SaveFileDialog
                    {
                        FileName = Path.GetFileName(url)
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        await File.WriteAllBytesAsync(dialog.FileName, bytes);

                        MessageBox.Show("Downloaded!");
                    }
                }
            }
            catch (HttpRequestException)
            {
                MessageBox.Show(
                    "Server is unavailable. Check your connection."
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Download failed: " + ex.Message
                );
            }
        }
        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            if (!PasswordValidator.IsValid(PasswordBox.Password))
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

                var raw = await response.Content.ReadAsStringAsync();

                var loginResult =
                    System.Text.Json.JsonSerializer.Deserialize<LoginResponse>(raw);

                if (loginResult == null)
                {
                    MessageBox.Show("Invalid login response");
                    return;
                }

                _token = loginResult.Token;
                _username = loginResult.Username;

                if (_connection.State == HubConnectionState.Disconnected)
                {
                    await _connection.StartAsync();
                }
                await _connection.InvokeAsync("Register");
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
            try
            {
                string? uploadedFile = null;

                if (!string.IsNullOrEmpty(_selectedFile))
                {
                    using var client = new HttpClient();

                    using var content = new MultipartFormDataContent();

                    using var stream = File.OpenRead(_selectedFile);

                    var fileContent = new StreamContent(stream);

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

                if (string.IsNullOrWhiteSpace(MessageInput.Text) &&
                    string.IsNullOrWhiteSpace(uploadedFile))
                {
                    return;
                }

                await _connection.SendAsync(
                    "SendMessage",
                    _selectedUser,
                    MessageInput.Text,
                    uploadedFile
                );

                await LoadMessages(_selectedUser);

                MessageInput.Text = "";
            }
            catch (HttpRequestException)
            {
                MessageBox.Show(
                    "Server is unavailable. Check your connection."
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Sending message failed: " + ex.Message
                );
            }
        }
    }
}
