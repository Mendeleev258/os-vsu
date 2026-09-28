using System;
using System.Threading;

namespace PostalService
{
    public class User
    {
        private readonly string _id;
        private readonly Mailbox _mailbox;
        private Thread _userThread;
        private bool _isRunning;
        private Random _random;
        private MailService _mailService;
        private Port _assignedPort;

        public event EventHandler<Message> MessageSent;
        public event EventHandler<Message> MessageReceived;
        public event EventHandler<string> StatusChanged;

        public string Id => _id;
        public Mailbox Mailbox => _mailbox;
        public bool IsRunning => _isRunning;
        public Port AssignedPort => _assignedPort;

        public User(string id, Mailbox mailbox, Port port, MailService mailService)
        {
            _id = id ?? throw new ArgumentNullException(nameof(id));
            _mailbox = mailbox ?? throw new ArgumentNullException(nameof(mailbox));
            _assignedPort = port ?? throw new ArgumentNullException(nameof(port));
            _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
            _random = new Random();
            _isRunning = false;

            // Подписываемся на события ящика
            _mailbox.MessageReceived += Mailbox_MessageReceived;
        }

        public void Start()
        {
            if (_isRunning) return;

            _isRunning = true;
            _userThread = new Thread(UserLoop);
            _userThread.IsBackground = true;
            _userThread.Name = $"User-{_id}";
            _userThread.Start();

            OnStatusChanged($"Пользователь {_id} запущен (порт: {_assignedPort.Id})");
        }

        public void Stop()
        {
            _isRunning = false;
            if (_userThread != null && _userThread.IsAlive)
            {
                _userThread.Join(1000);
            }

            OnStatusChanged($"Пользователь {_id} остановлен");
        }

        private void UserLoop()
        {
            while (_isRunning)
            {
                try
                {
                    // Проверяем ящик на новые сообщения
                    CheckMailbox();

                    Thread.Sleep(_random.Next(500, 1000));
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    OnStatusChanged($"Ошибка у пользователя {_id}: {ex.Message}");
                }
            }
        }

        private void CheckMailbox()
        {
            // Обрабатываем все сообщения в ящике
            while (_mailbox.TryGetMessage(out var message))
            {
                OnMessageReceived(message);
                OnStatusChanged($"Пользователь {_id} получил сообщение от {message.From}");
            }
        }

        // ОСНОВНОЙ МЕТОД: ОТПРАВКА СООБЩЕНИЯ ЧЕРЕЗ ПОРТ
        public bool SendMessage(string targetUserId, string content)
        {
            if (!_isRunning)
            {
                OnStatusChanged($"Ошибка: Пользователь {_id} не активен");
                return false;
            }

            if (_assignedPort == null)
            {
                OnStatusChanged($"Ошибка: Пользователь {_id} не привязан к порту");
                return false;
            }

            // Отправляем сообщение через свой порт
            bool success = _assignedPort.SendMessageFromUser(_id, targetUserId, content);

            if (success)
            {
                var message = new Message(_id, targetUserId, MessageType.Send, content);
                OnMessageSent(message);
                OnStatusChanged($"Пользователь {_id} отправил сообщение {targetUserId} через порт {_assignedPort.Id}");
            }
            else
            {
                OnStatusChanged($"Ошибка отправки от {_id} к {targetUserId}");
            }

            return success;
        }

        private void Mailbox_MessageReceived(object sender, Message message)
        {
            OnMessageReceived(message);
        }

        protected virtual void OnMessageSent(Message message)
        {
            MessageSent?.Invoke(this, message);
        }

        protected virtual void OnMessageReceived(Message message)
        {
            MessageReceived?.Invoke(this, message);
        }

        protected virtual void OnStatusChanged(string status)
        {
            StatusChanged?.Invoke(this, status);
        }
    }
}