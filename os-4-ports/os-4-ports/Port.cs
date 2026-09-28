using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PostalService
{
    public class Port
    {
        private readonly string _id;
        private readonly Dictionary<string, Mailbox> _mailboxes; // Ящики под управлением этого порта
        private MailService _mailService;
        private Thread _processingThread;
        private bool _isRunning;
        private readonly object _lockObject = new object();

        // Статистика
        private int _messagesSent;
        private int _messagesReceived;
        private int _messagesRouted;

        public string Id => _id;
        public bool IsRunning => _isRunning;
        public int MessagesSent => _messagesSent;
        public int MessagesReceived => _messagesReceived;
        public int MessagesRouted => _messagesRouted;
        public IReadOnlyList<string> ManagedMailboxIds => _mailboxes.Keys.ToList();

        public event EventHandler<string> StatusChanged;
        public event EventHandler<Message> MessageSent;
        public event EventHandler<Message> MessageReceived;
        public event EventHandler<Message> MessageRouted;

        public Port(string id)
        {
            _id = id ?? throw new ArgumentNullException(nameof(id));
            _mailboxes = new Dictionary<string, Mailbox>();
            _isRunning = false;
            _messagesSent = 0;
            _messagesReceived = 0;
            _messagesRouted = 0;
        }

        public void Connect(MailService mailService)
        {
            lock (_lockObject)
            {
                _mailService = mailService;
                StartProcessing();
            }
        }

        public void Disconnect()
        {
            lock (_lockObject)
            {
                StopProcessing();
                _mailService = null;
            }
        }

        private void StartProcessing()
        {
            if (_isRunning) return;

            _isRunning = true;
            _processingThread = new Thread(ProcessMessages);
            _processingThread.IsBackground = true;
            _processingThread.Start();

            OnStatusChanged($"Порт {_id} запущен и готов к работе");
        }

        private void StopProcessing()
        {
            _isRunning = false;
            if (_processingThread != null && _processingThread.IsAlive)
            {
                _processingThread.Join(1000);
            }

            OnStatusChanged($"Порт {_id} остановлен");
        }

        // Добавить ящик под управление порта
        public void AddMailbox(Mailbox mailbox)
        {
            lock (_lockObject)
            {
                if (!_mailboxes.ContainsKey(mailbox.Id))
                {
                    _mailboxes[mailbox.Id] = mailbox;

                    // Подписываемся на события ящика
                    mailbox.MessageReceived += (s, msg) =>
                        OnStatusChanged($"В ящике {mailbox.Id} новое сообщение от {msg.From}");

                    OnStatusChanged($"Порт {_id} управляет ящиком {mailbox.Id}");
                }
            }
        }

        // Убрать ящик из управления
        public void RemoveMailbox(string mailboxId)
        {
            lock (_lockObject)
            {
                if (_mailboxes.ContainsKey(mailboxId))
                {
                    _mailboxes.Remove(mailboxId);
                    OnStatusChanged($"Порт {_id} больше не управляет ящиком {mailboxId}");
                }
            }
        }

        // Проверить, управляет ли порт этим ящиком
        public bool ManagesMailbox(string mailboxId)
        {
            lock (_lockObject)
            {
                return _mailboxes.ContainsKey(mailboxId);
            }
        }

        // ОСНОВНОЙ МЕТОД: ОТПРАВКА СООБЩЕНИЯ ОТ ПОЛЬЗОВАТЕЛЯ
        public bool SendMessageFromUser(string fromUserId, string toUserId, object content)
        {
            if (_mailService == null)
            {
                OnStatusChanged($"Ошибка: Порт {_id} не подключен к службе");
                return false;
            }

            // Проверяем, что отправитель принадлежит этому порту
            if (!ManagesMailbox(fromUserId))
            {
                OnStatusChanged($"Ошибка: Пользователь {fromUserId} не принадлежит порту {_id}");
                return false;
            }

            // Создаем сообщение
            var message = new Message(fromUserId, toUserId, MessageType.Send, content);

            // Увеличиваем счетчик отправленных
            Interlocked.Increment(ref _messagesSent);

            // Уведомляем о отправке
            OnMessageSent(message);
            OnStatusChanged($"Порт {_id} отправляет сообщение от {fromUserId} к {toUserId}");

            // Передаем сообщение в MailService для маршрутизации
            return _mailService.RouteMessage(message, _id);
        }

        // МЕТОД: ПОЛУЧЕНИЕ СООБЩЕНИЯ ОТ ДРУГОГО ПОРТА (через MailService)
        public bool ReceiveMessageFromPort(Message message, string senderPortId)
        {
            if (!_isRunning)
            {
                OnStatusChanged($"Ошибка: Порт {_id} не активен");
                return false;
            }

            string targetUserId = message.To;

            // Проверяем, что получатель принадлежит этому порту
            if (!ManagesMailbox(targetUserId))
            {
                OnStatusChanged($"Ошибка: Получатель {targetUserId} не принадлежит порту {_id}");
                return false;
            }

            // Находим ящик получателя
            if (!_mailboxes.TryGetValue(targetUserId, out var mailbox))
            {
                OnStatusChanged($"Ошибка: Ящик {targetUserId} не найден в порте {_id}");
                return false;
            }

            // Доставляем сообщение в ящик
            mailbox.PutMessage(message);

            // Увеличиваем счетчики
            Interlocked.Increment(ref _messagesReceived);
            Interlocked.Increment(ref _messagesRouted);

            // Уведомляем о получении
            OnMessageReceived(message);
            OnMessageRouted(message);
            OnStatusChanged($"Порт {_id} доставил сообщение от порта {senderPortId} в ящик {targetUserId}");

            return true;
        }

        // Фоновая обработка сообщений в ящиках
        private void ProcessMessages()
        {
            while (_isRunning)
            {
                try
                {
                    // Здесь можно добавить обработку системных сообщений
                    // или периодические проверки состояния

                    Thread.Sleep(1000); // Не загружаем CPU
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    OnStatusChanged($"Ошибка в порте {_id}: {ex.Message}");
                }
            }
        }

        // Методы для вызова событий
        protected virtual void OnStatusChanged(string status)
        {
            StatusChanged?.Invoke(this, status);
        }

        protected virtual void OnMessageSent(Message message)
        {
            MessageSent?.Invoke(this, message);
        }

        protected virtual void OnMessageReceived(Message message)
        {
            MessageReceived?.Invoke(this, message);
        }

        protected virtual void OnMessageRouted(Message message)
        {
            MessageRouted?.Invoke(this, message);
        }
    }
}