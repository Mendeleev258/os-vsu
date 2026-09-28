using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace PostalService
{
    public partial class MainForm : Form
    {
        private MailService _mailService;
        private Dictionary<string, User> _activeUsers;
        private Dictionary<string, Port> _activePorts;
        private Random _random;
        private bool _isRunning;
        private Thread _demoThread;

        public MainForm()
        {
            InitializeComponent();

            _mailService = new MailService(3, 3); // 3 потока, 3 пользователя на порт
            _activeUsers = new Dictionary<string, User>();
            _activePorts = new Dictionary<string, Port>();
            _random = new Random();
            _isRunning = false;

            SetupEventHandlers();
            UpdateControls();
        }

        private void SetupEventHandlers()
        {
            _mailService.MessageRouted += MailService_MessageRouted;
            _mailService.ServiceStatusChanged += MailService_ServiceStatusChanged;
        }

        private void btnStartService_Click(object sender, EventArgs e)
        {
            if (!_isRunning)
            {
                StartMailService();
            }
        }

        private void btnStopService_Click(object sender, EventArgs e)
        {
            if (_isRunning)
            {
                StopMailService();
            }
        }

        private void btnCreateUser_Click(object sender, EventArgs e)
        {
            string userId = txtUserId.Text.Trim();
            if (string.IsNullOrEmpty(userId))
            {
                MessageBox.Show("Введите ID пользователя", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var user = _mailService.CreateUser(userId);
                _activeUsers[userId] = user;

                // Подписываемся на события пользователя
                user.MessageSent += User_MessageSent;
                user.MessageReceived += User_MessageReceived;
                user.StatusChanged += User_StatusChanged;

                // Подписываемся на события порта
                var port = user.AssignedPort;
                if (port != null && !_activePorts.ContainsKey(port.Id))
                {
                    _activePorts[port.Id] = port;
                    port.StatusChanged += Port_StatusChanged;
                    port.MessageSent += Port_MessageSent;
                    port.MessageReceived += Port_MessageReceived;
                }

                AddUserToUI(user);
                UpdatePortsInfo();
                UpdateUserComboBoxes();

                // Проверяем статус пользователя
                if (user.IsRunning)
                {
                    AddStatus($"Создан и запущен пользователь: {userId} (порт: {port?.Id})");
                }
                else
                {
                    AddStatus($"Создан пользователь: {userId} (не запущен - служба остановлена)");
                }

                txtUserId.Clear();
                txtUserId.Focus();
                UpdateControls();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка создания: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSendMessage_Click(object sender, EventArgs e)
        {
            string senderUserId = cmbSenderUser.SelectedItem?.ToString();
            string targetUserId = cmbTargetUser.SelectedItem?.ToString();
            string messageContent = txtMessageContent.Text.Trim();

            if (string.IsNullOrEmpty(senderUserId))
            {
                MessageBox.Show("Выберите пользователя-отправителя", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(targetUserId))
            {
                MessageBox.Show("Выберите пользователя-получателя", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (senderUserId == targetUserId)
            {
                MessageBox.Show("Нельзя отправить сообщение самому себе!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(messageContent))
            {
                MessageBox.Show("Введите текст сообщения", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_activeUsers.TryGetValue(senderUserId, out var user))
            {
                MessageBox.Show($"Пользователь '{senderUserId}' не найден", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Отправляем сообщение через пользователя (он отправит через свой порт)
            bool success = user.SendMessage(targetUserId, messageContent);

            if (success)
            {
                AddStatus($"Сообщение отправлено: {senderUserId} -> {targetUserId}");
            }
            else
            {
                AddStatus($"Ошибка отправки: {senderUserId} -> {targetUserId}");
            }

            txtMessageContent.Clear();
            txtMessageContent.Focus();
        }

        private void btnStartDemo_Click(object sender, EventArgs e)
        {
            if (_demoThread == null || !_demoThread.IsAlive)
            {
                StartDemoMode();
            }
            else
            {
                StopDemoMode();
            }
        }

        private void StartDemoMode()
        {
            if (!_isRunning)
            {
                MessageBox.Show("Сначала запустите почтовую службу!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_activeUsers.Count < 2)
            {
                MessageBox.Show("Создайте хотя бы 2 пользователя для демонстрации", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _demoThread = new Thread(DemoWorker);
            _demoThread.IsBackground = true;
            _demoThread.Start();

            btnStartDemo.Text = "Остановить демо";
            btnStartDemo.BackColor = Color.LightCoral;
            AddStatus("Демонстрационный режим запущен");
        }

        private void StopDemoMode()
        {
            if (_demoThread != null && _demoThread.IsAlive)
            {
                _demoThread.Abort();
                _demoThread.Join(1000);
            }

            btnStartDemo.Text = "Запустить демо";
            btnStartDemo.BackColor = Color.LightBlue;
            AddStatus("Демонстрационный режим остановлен");
        }

        private void DemoWorker()
        {
            var userList = _activeUsers.Values.ToList();
            var demoMessages = new[]
            {
                "Привет! Как дела?",
                "Тестовое сообщение",
                "Важное уведомление",
                "Данные для обработки",
                "Запрос на выполнение",
                "Результат вычислений",
                "Статус: OK",
                "Срочно свяжитесь со мной",
                "Еженедельный отчет",
                "Напоминание о встрече"
            };

            while (true)
            {
                try
                {
                    if (userList.Count >= 2)
                    {
                        var sender = userList[_random.Next(userList.Count)];

                        var receiver = userList
                            .Where(u => u.Id != sender.Id)
                            .OrderBy(x => _random.Next())
                            .FirstOrDefault();

                        if (receiver != null)
                        {
                            var message = demoMessages[_random.Next(demoMessages.Length)];

                            Invoke(new Action(() =>
                            {
                                sender.SendMessage(receiver.Id, message);
                            }));
                        }
                    }

                    Thread.Sleep(_random.Next(1500, 4000));
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception)
                {
                    // Игнорируем ошибки
                }
            }
        }

        private void StartMailService()
        {
            try
            {
                _mailService.Start();
                _isRunning = true;

                btnStartService.Enabled = false;
                btnStopService.Enabled = true;
                btnStartDemo.Enabled = true;

                AddStatus("Почтовая служба запущена");
                UpdateControls();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка запуска службы: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StopMailService()
        {
            try
            {
                StopDemoMode();
                _mailService.Stop();
                _isRunning = false;

                btnStartService.Enabled = true;
                btnStopService.Enabled = false;
                btnStartDemo.Enabled = false;

                AddStatus("Почтовая служба остановлена");
                UpdateControls();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка остановки службы: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGetStatistics_Click(object sender, EventArgs e)
        {
            if (_mailService != null)
            {
                string stats = _mailService.GetStatistics();
                AddStatus("Статистика:\n" + stats);
            }
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            lstMessageLog.Items.Clear();
            AddStatus("Лог сообщений очищен");
        }

        private void btnClearStatus_Click(object sender, EventArgs e)
        {
            lstStatus.Items.Clear();
        }

        private void AddUserToUI(User user)
        {
            ListViewItem item = new ListViewItem(user.Id);
            item.SubItems.Add(user.Mailbox.Id);
            item.SubItems.Add(user.AssignedPort?.Id ?? "Не назначен");
            item.SubItems.Add(user.IsRunning ? "Активен" : "Неактивен");
            item.SubItems.Add("0"); // Отправлено
            item.SubItems.Add("0"); // Получено
            item.Tag = user;

            lvUsers.Items.Add(item);
        }

        private void UpdatePortsInfo()
        {
            lvPorts.Items.Clear();
            _activePorts.Clear();

            foreach (var port in _mailService.GetAllPorts())
            {
                _activePorts[port.Id] = port;

                ListViewItem item = new ListViewItem(port.Id);
                item.SubItems.Add(port.ManagedMailboxIds.Count.ToString());
                item.SubItems.Add(port.IsRunning ? "Активен" : "Неактивен");
                item.SubItems.Add(port.MessagesSent.ToString());
                item.SubItems.Add(port.MessagesReceived.ToString());
                item.SubItems.Add(port.MessagesRouted.ToString());
                item.Tag = port;

                lvPorts.Items.Add(item);
            }
        }

        private void UpdateUserComboBoxes()
        {
            cmbSenderUser.Items.Clear();
            cmbTargetUser.Items.Clear();

            foreach (var user in _activeUsers.Values)
            {
                if (!cmbSenderUser.Items.Contains(user.Id))
                    cmbSenderUser.Items.Add(user.Id);

                if (!cmbTargetUser.Items.Contains(user.Id))
                    cmbTargetUser.Items.Add(user.Id);
            }

            if (cmbSenderUser.Items.Count > 0)
                cmbSenderUser.SelectedIndex = 0;
            if (cmbTargetUser.Items.Count > 0)
                cmbTargetUser.SelectedIndex = 0;
        }

        private void UpdateUserStatistics(string userId, bool isSent)
        {
            foreach (ListViewItem item in lvUsers.Items)
            {
                if (item.Text == userId)
                {
                    if (isSent)
                    {
                        int sent = int.Parse(item.SubItems[4].Text);
                        item.SubItems[4].Text = (sent + 1).ToString();
                    }
                    else
                    {
                        int received = int.Parse(item.SubItems[5].Text);
                        item.SubItems[5].Text = (received + 1).ToString();
                    }
                    break;
                }
            }
        }

        private void UpdatePortStatistics(string portId, bool isSent)
        {
            foreach (ListViewItem item in lvPorts.Items)
            {
                if (item.Text == portId)
                {
                    if (isSent)
                    {
                        int sent = int.Parse(item.SubItems[3].Text);
                        item.SubItems[3].Text = (sent + 1).ToString();
                    }
                    else
                    {
                        int received = int.Parse(item.SubItems[4].Text);
                        item.SubItems[4].Text = (received + 1).ToString();
                    }
                    break;
                }
            }
        }

        private void UpdatePortRoutedStatistics(string portId)
        {
            foreach (ListViewItem item in lvPorts.Items)
            {
                if (item.Text == portId)
                {
                    if (_activePorts.TryGetValue(portId, out var port))
                    {
                        item.SubItems[5].Text = port.MessagesRouted.ToString();
                    }
                    break;
                }
            }
        }

        private void UpdateControls()
        {
            bool serviceRunning = _isRunning;
            bool hasUsers = _activeUsers.Count > 0;
            bool senderSelected = cmbSenderUser.SelectedItem != null;
            bool targetSelected = cmbTargetUser.SelectedItem != null;
            bool differentUsers = senderSelected && targetSelected &&
                                 cmbSenderUser.SelectedItem.ToString() != cmbTargetUser.SelectedItem.ToString();
            bool hasMessage = !string.IsNullOrWhiteSpace(txtMessageContent.Text);

            btnCreateUser.Enabled = serviceRunning;
            txtUserId.Enabled = serviceRunning;

            btnSendMessage.Enabled = serviceRunning && hasUsers &&
                                     senderSelected && targetSelected &&
                                     differentUsers && hasMessage;
            cmbSenderUser.Enabled = serviceRunning && hasUsers;
            cmbTargetUser.Enabled = serviceRunning && hasUsers;
            txtMessageContent.Enabled = serviceRunning && hasUsers;

            btnStartDemo.Enabled = serviceRunning && hasUsers && _activeUsers.Count >= 2;

            if (_demoThread != null && _demoThread.IsAlive)
            {
                btnStartDemo.Text = "Остановить демо";
                btnStartDemo.BackColor = Color.LightCoral;
            }
            else
            {
                btnStartDemo.Text = "Запустить демо";
                btnStartDemo.BackColor = Color.LightBlue;
            }
        }

        private void MailService_MessageRouted(object sender, Message message, string fromPort, string toPort)
        {
            Invoke(new Action(() =>
            {
                string logEntry = $"[{DateTime.Now:HH:mm:ss}] Маршрутизация: {message.From} -> {message.To} " +
                                $"(Порт {fromPort} → Порт {toPort})";
                lstMessageLog.Items.Add(logEntry);
                lstMessageLog.TopIndex = lstMessageLog.Items.Count - 1;

                // Обновляем статистику маршрутизации для обоих портов
                UpdatePortRoutedStatistics(fromPort);
                UpdatePortRoutedStatistics(toPort);
            }));
        }

        private void MailService_ServiceStatusChanged(object sender, string status)
        {
            Invoke(new Action(() =>
            {
                AddStatus("Служба: " + status);
            }));
        }

        private void User_MessageSent(object sender, Message message)
        {
            Invoke(new Action(() =>
            {
                string logEntry = $"[{DateTime.Now:HH:mm:ss}] Отправка: {message.From} -> {message.To}";
                lstMessageLog.Items.Add(logEntry);

                if (sender is User user)
                {
                    UpdateUserStatistics(user.Id, true);

                    // Обновляем статистику порта отправки
                    if (user.AssignedPort != null)
                    {
                        UpdatePortStatistics(user.AssignedPort.Id, true);
                    }
                }
            }));
        }

        private void User_MessageReceived(object sender, Message message)
        {
            Invoke(new Action(() =>
            {
                string logEntry = $"[{DateTime.Now:HH:mm:ss}] Получение: {message.From} -> {message.To}";
                lstMessageLog.Items.Add(logEntry);
                lstMessageLog.TopIndex = lstMessageLog.Items.Count - 1;

                if (sender is User user)
                {
                    UpdateUserStatistics(user.Id, false);
                }
            }));
        }

        private void User_StatusChanged(object sender, string status)
        {
            Invoke(new Action(() =>
            {
                AddStatus(status);
            }));
        }

        private void Port_StatusChanged(object sender, string status)
        {
            Invoke(new Action(() =>
            {
                if (sender is Port port)
                {
                    AddStatus($"Порт {port.Id}: {status}");

                    // Обновляем статус порта в UI
                    foreach (ListViewItem item in lvPorts.Items)
                    {
                        if (item.Text == port.Id)
                        {
                            item.SubItems[2].Text = port.IsRunning ? "Активен" : "Неактивен";
                            break;
                        }
                    }
                }
            }));
        }

        private void Port_MessageSent(object sender, Message message)
        {
            Invoke(new Action(() =>
            {
                if (sender is Port port)
                {
                    AddStatus($"Порт {port.Id} отправил сообщение от {message.From}");
                }
            }));
        }

        private void Port_MessageReceived(object sender, Message message)
        {
            Invoke(new Action(() =>
            {
                if (sender is Port port)
                {
                    AddStatus($"Порт {port.Id} получил сообщение для {message.To}");
                    UpdatePortStatistics(port.Id, false);
                }
            }));
        }

        private void AddStatus(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            lstStatus.Items.Add($"[{timestamp}] {message}");
            lstStatus.TopIndex = lstStatus.Items.Count - 1;

            if (lstStatus.Items.Count > 100)
                lstStatus.Items.RemoveAt(0);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopMailService();

            foreach (var user in _activeUsers.Values)
            {
                user.MessageSent -= User_MessageSent;
                user.MessageReceived -= User_MessageReceived;
                user.StatusChanged -= User_StatusChanged;
            }

            foreach (var port in _activePorts.Values)
            {
                port.StatusChanged -= Port_StatusChanged;
                port.MessageSent -= Port_MessageSent;
                port.MessageReceived -= Port_MessageReceived;
            }

            _mailService.MessageRouted -= MailService_MessageRouted;
            _mailService.ServiceStatusChanged -= MailService_ServiceStatusChanged;

            base.OnFormClosing(e);
        }

        private void txtUserId_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                btnCreateUser_Click(sender, e);
                e.Handled = true;
            }
        }

        private void txtMessageContent_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && btnSendMessage.Enabled)
            {
                btnSendMessage_Click(sender, e);
                e.Handled = true;
            }
        }

        private void lvUsers_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lvUsers.SelectedItems.Count > 0)
            {
                string userId = lvUsers.SelectedItems[0].Text;
                ShowUserDetails(userId);
            }
        }

        private void lvPorts_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lvPorts.SelectedItems.Count > 0)
            {
                string portId = lvPorts.SelectedItems[0].Text;
                ShowPortDetails(portId);
            }
        }

        private void ShowUserDetails(string userId)
        {
            if (_activeUsers.TryGetValue(userId, out var user))
            {
                string details = $"Пользователь: {userId}\n" +
                               $"Почтовый ящик: {user.Mailbox.Id}\n" +
                               $"Порт: {user.AssignedPort?.Id ?? "Не назначен"}\n" +
                               $"Статус: {(user.IsRunning ? "Активен" : "Неактивен")}\n" +
                               $"Сообщений в ящике: {user.Mailbox.MessageCount}";

                MessageBox.Show(details, "Информация о пользователе",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ShowPortDetails(string portId)
        {
            if (_activePorts.TryGetValue(portId, out var port))
            {
                string details = $"Порт: {portId}\n" +
                               $"Статус: {(port.IsRunning ? "Активен" : "Неактивен")}\n" +
                               $"Управляемых ящиков: {port.ManagedMailboxIds.Count}\n" +
                               $"Отправлено сообщений: {port.MessagesSent}\n" +
                               $"Получено сообщений: {port.MessagesReceived}\n" +
                               $"Маршрутизировано: {port.MessagesRouted}\n\n";

                if (port.ManagedMailboxIds.Count > 0)
                {
                    details += "Управляемые ящики:\n";
                    foreach (var mailboxId in port.ManagedMailboxIds)
                    {
                        details += $"  {mailboxId}\n";
                    }
                }
                else
                {
                    details += "Нет управляемых ящиков";
                }

                MessageBox.Show(details, "Информация о порте",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void cmbSenderUser_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void cmbTargetUser_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void txtMessageContent_TextChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void btnRefreshPorts_Click(object sender, EventArgs e)
        {
            UpdatePortsInfo();
            AddStatus("Информация о портах обновлена");
        }
    }
}