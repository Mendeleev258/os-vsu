using System;
using System.Threading;

namespace PostalService
{
    public class WorkerThread
    {
        private readonly string _id;
        private readonly MailService _mailService;
        private Thread _thread;
        private bool _isRunning;
        private Random _random;

        public string Id => _id;
        public bool IsRunning => _isRunning;

        public WorkerThread(string id, MailService mailService)
        {
            _id = id;
            _mailService = mailService;
            _random = new Random();
        }

        public void Start()
        {
            if (_isRunning) return;

            _isRunning = true;
            _thread = new Thread(WorkerLoop);
            _thread.IsBackground = true;
            _thread.Name = _id;
            _thread.Start();
        }

        public void Stop()
        {
            _isRunning = false;
            if (_thread != null && _thread.IsAlive)
            {
                _thread.Join(1000);
            }
        }

        private void WorkerLoop()
        {
            while (_isRunning)
            {
                try
                {
                    Thread.Sleep(_random.Next(500, 2000));
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
    }
}