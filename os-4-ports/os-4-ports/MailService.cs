using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PostalService
{
    public class MailService
    {
        private readonly Dictionary<string, Port> _ports;
        private readonly Dictionary<string, Mailbox> _mailboxes;
        private readonly Dictionary<string, User> _users;
        private readonly Dictionary<string, string> _mailboxToPortMap; // mailboxId -> portId
        private readonly List<WorkerThread> _workerThreads;
        private readonly object _lockObject = new object();
        private readonly int _maxThreads;
        private readonly int _maxUsersPerPort;
        private bool _isRunning;
        private int _nextPortNumber;

        public delegate void MessageRoutedHandler(object sender, Message message, string fromPort, string toPort);
        public delegate void ServiceStatusChangedHandler(object sender, string status);

        public event MessageRoutedHandler MessageRouted;
        public event ServiceStatusChangedHandler ServiceStatusChanged;

        public MailService(int maxThreads = 5, int maxUsersPerPort = 3)
        {
            _maxThreads = maxThreads;
            _maxUsersPerPort = maxUsersPerPort;
            _ports = new Dictionary<string, Port>();
            _mailboxes = new Dictionary<string, Mailbox>();
            _users = new Dictionary<string, User>();
            _mailboxToPortMap = new Dictionary<string, string>();
            _workerThreads = new List<WorkerThread>();
            _isRunning = false;
            _nextPortNumber = 1;
        }

        public void Start()
        {
            lock (_lockObject)
            {
                if (_isRunning) return;

                _isRunning = true;

                // Создаем рабочие потоки
                for (int i = 0; i < _maxThreads; i++)
                {
                    var worker = new WorkerThread($"Worker-{i}", this);
                    _workerThreads.Add(worker);
                    worker.Start();
                }

                // Запускаем всех пользователей (тех, кто еще не запущен)
                foreach (var user in _users.Values)
                {
                    if (!user.IsRunning)
                    {
                        user.Start();
                    }
                }

                // Запускаем все порты
                foreach (var port in _ports.Values)
                {
                    port.Connect(this);
                }

                OnServiceStatusChanged("Почтовая служба запущена");
            }
        }

        public void Stop()
        {
            lock (_lockObject)
            {
                if (!_isRunning) return;

                _isRunning = false;

                // Останавливаем всех пользователей
                foreach (var user in _users.Values)
                {
                    user.Stop();
                }

                // Останавливаем все порты
                foreach (var port in _ports.Values)
                {
                    port.Disconnect();
                }

                // Останавливаем рабочие потоки
                foreach (var worker in _workerThreads)
                {
                    worker.Stop();
                }
                _workerThreads.Clear();

                OnServiceStatusChanged("Почтовая служба остановлена");
            }
        }

        // СОЗДАНИЕ ПОЛЬЗОВАТЕЛЯ С АВТОМАТИЧЕСКОЙ ПРИВЯЗКОЙ К ПОРТУ
        public User CreateUser(string userId)
        {
            lock (_lockObject)
            {
                if (_users.ContainsKey(userId))
                {
                    throw new ArgumentException($"Пользователь {userId} уже существует");
                }

                // 1. Создаем ящик
                string mailboxId = GenerateUniqueMailboxId(userId);
                var mailbox = new Mailbox(mailboxId);
                _mailboxes[mailboxId] = mailbox;

                // 2. Находим или создаем порт для этого ящика
                var port = FindOrCreatePortForMailbox(mailboxId);

                // 3. Привязываем ящик к порту
                port.AddMailbox(mailbox);
                _mailboxToPortMap[mailboxId] = port.Id;

                // 4. Создаем пользователя
                var user = new User(userId, mailbox, port, this);
                _users[userId] = user;

                // 5. ЗАПУСКАЕМ ПОЛЬЗОВАТЕЛЯ, ЕСЛИ СЛУЖБА ЗАПУЩЕНА
                if (_isRunning)
                {
                    user.Start();
                }

                OnServiceStatusChanged($"Создан пользователь: {userId} (ящик: {mailboxId}, порт: {port.Id})");

                return user;
            }
        }

        private string GenerateUniqueMailboxId(string baseId)
        {
            string mailboxId = baseId;
            int counter = 1;

            while (_mailboxes.ContainsKey(mailboxId))
            {
                mailboxId = $"{baseId}_{counter}";
                counter++;
            }

            return mailboxId;
        }

        private Port FindOrCreatePortForMailbox(string mailboxId)
        {
            // Ищем порт с минимальным количеством ящиков
            var availablePort = _ports.Values
                .Where(p => p.ManagedMailboxIds.Count < _maxUsersPerPort)
                .OrderBy(p => p.ManagedMailboxIds.Count)
                .FirstOrDefault();

            if (availablePort != null)
            {
                return availablePort;
            }

            // Создаем новый порт
            string newPortId = $"Port{_nextPortNumber++}";
            var newPort = new Port(newPortId);
            _ports[newPortId] = newPort;

            return newPort;
        }

        // ОСНОВНОЙ МЕТОД: МАРШРУТИЗАЦИЯ СООБЩЕНИЙ МЕЖДУ ПОРТАМИ
        public bool RouteMessage(Message message, string senderPortId)
        {
            if (!_isRunning)
            {
                OnServiceStatusChanged("Ошибка: Служба не запущена");
                return false;
            }

            lock (_lockObject)
            {
                string targetUserId = message.To;

                // 1. Проверяем существование получателя
                if (!_users.ContainsKey(targetUserId))
                {
                    OnServiceStatusChanged($"Ошибка: Пользователь {targetUserId} не найден");
                    return false;
                }

                // 2. Находим ящик получателя
                if (!_users.TryGetValue(targetUserId, out var targetUser))
                {
                    OnServiceStatusChanged($"Ошибка: Не удалось получить пользователя {targetUserId}");
                    return false;
                }

                // 3. Находим порт получателя
                var receiverPort = targetUser.AssignedPort;
                if (receiverPort == null)
                {
                    OnServiceStatusChanged($"Ошибка: Пользователь {targetUserId} не привязан к порту");
                    return false;
                }

                // 4. Если отправитель и получатель в одном порту - быстрая доставка
                if (senderPortId == receiverPort.Id)
                {
                    OnServiceStatusChanged($"Локальная доставка в порту {senderPortId}: {message.From} → {message.To}");
                    return receiverPort.ReceiveMessageFromPort(message, senderPortId);
                }

                // 5. МЕЖПОРТОВАЯ МАРШРУТИЗАЦИЯ
                OnServiceStatusChanged($"Маршрутизация: порт {senderPortId} → порт {receiverPort.Id}");

                // Уведомляем о маршрутизации
                OnMessageRouted(message, senderPortId, receiverPort.Id);

                // Передаем сообщение порту получателя
                bool success = receiverPort.ReceiveMessageFromPort(message, senderPortId);

                if (success)
                {
                    OnServiceStatusChanged($"Сообщение успешно маршрутизировано: {message.From} → {message.To}");
                }
                else
                {
                    OnServiceStatusChanged($"Ошибка маршрутизации: {message.From} → {message.To}");
                }

                return success;
            }
        }

        // ПОЛУЧИТЬ ПОРТ ДЛЯ ПОЛЬЗОВАТЕЛЯ
        public Port GetPortForUser(string userId)
        {
            lock (_lockObject)
            {
                if (_users.TryGetValue(userId, out var user))
                {
                    return user.AssignedPort;
                }
                return null;
            }
        }

        // ПОЛУЧИТЬ ПОРТ ПО ID
        public Port GetPort(string portId)
        {
            lock (_lockObject)
            {
                _ports.TryGetValue(portId, out var port);
                return port;
            }
        }

        public IEnumerable<User> GetAllUsers()
        {
            lock (_lockObject)
            {
                return _users.Values.ToList();
            }
        }

        public IEnumerable<Port> GetAllPorts()
        {
            lock (_lockObject)
            {
                return _ports.Values.ToList();
            }
        }

        public IEnumerable<Mailbox> GetAllMailboxes()
        {
            lock (_lockObject)
            {
                return _mailboxes.Values.ToList();
            }
        }

        public string GetStatistics()
        {
            lock (_lockObject)
            {
                int totalMessages = _mailboxes.Values.Sum(m => m.MessageCount);
                int totalPorts = _ports.Count;
                int totalUsers = _users.Count;
                int activeThreads = _workerThreads.Count(w => w.IsRunning);
                int activeUsers = _users.Values.Count(u => u.IsRunning);

                // Статистика по портам
                int totalSent = _ports.Values.Sum(p => p.MessagesSent);
                int totalReceived = _ports.Values.Sum(p => p.MessagesReceived);
                int totalRouted = _ports.Values.Sum(p => p.MessagesRouted);

                return $"Пользователей: {totalUsers}, Порт: {totalPorts}, " +
                       $"Ящиков: {_mailboxes.Count}, Сообщений в ящиках: {totalMessages}\n" +
                       $"Отправлено: {totalSent}, Получено: {totalReceived}, " +
                       $"Маршрутизировано: {totalRouted}";
            }
        }

        protected virtual void OnMessageRouted(Message message, string fromPort, string toPort)
        {
            MessageRouted?.Invoke(this, message, fromPort, toPort);
        }

        protected virtual void OnServiceStatusChanged(string status)
        {
            ServiceStatusChanged?.Invoke(this, status);
        }
    }
}