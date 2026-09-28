using System;

namespace PostalService
{
    // Типы сообщений
    public enum MessageType
    {
        Send,       // Отправка сообщения
        Receive,    // Получение сообщения
        System      // Системное сообщение
    }

    // Класс сообщения
    public class Message
    {
        public Guid MessageId { get; }                 // Уникальный ID сообщения
        public string From { get; }                    // Отправитель
        public string To { get; }                      // Получатель (ID почтового ящика)
        public MessageType Type { get; }               // Тип сообщения
        public object Content { get; }                 // Содержимое сообщения
        public DateTime Timestamp { get; }             // Время отправки

        public Message(string from, string to, MessageType type, object content)
        {
            MessageId = Guid.NewGuid();
            From = from;
            To = to;
            Type = type;
            Content = content;
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss}] {Type}: {From} → {To} ({Content})";
        }
    }
}