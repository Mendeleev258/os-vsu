using System;
using System.Collections.Concurrent;
using System.Threading;

namespace PostalService
{
    public class Mailbox
    {
        private readonly string _id;
        private readonly BlockingCollection<Message> _messages;

        public string Id => _id;
        public int MessageCount => _messages.Count;
        public bool HasMessages => _messages.Count > 0;

        public event EventHandler<Message> MessageReceived;

        public Mailbox(string id)
        {
            _id = id ?? throw new ArgumentNullException(nameof(id));
            _messages = new BlockingCollection<Message>();
        }

        public void PutMessage(Message message)
        {
            _messages.Add(message);
            MessageReceived?.Invoke(this, message);
        }

        public Message GetMessage(int timeout = Timeout.Infinite)
        {
            try
            {
                if (_messages.TryTake(out Message message, timeout))
                {
                    return message;
                }
                return null;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        public bool TryGetMessage(out Message message)
        {
            return _messages.TryTake(out message);
        }

        public void Clear()
        {
            while (_messages.TryTake(out _)) { }
        }

        public Message[] PeekAllMessages()
        {
            return _messages.ToArray();
        }
    }
}