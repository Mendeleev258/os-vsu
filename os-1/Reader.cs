using System;
using System.Threading;

public class Reader
{
    private Buffer buffer;
    private int dataToConsume;
    private int consumedCount = 0;
    private Thread thread;
    private bool isPaused = false;
    private bool isStopped = false;
    private Action<string> updateStatus;
    private Random random = new Random();
    private bool isRunning = false;

    public Reader(Buffer buffer, int dataToConsume, Action<string> updateStatus)
    {
        this.buffer = buffer;
        this.dataToConsume = dataToConsume;
        this.updateStatus = updateStatus;
        this.thread = new Thread(Run);
    }

    public void Start()
    {
        isRunning = true;
        thread.Start();
    }

    public void Stop()
    {
        isStopped = true;
        isRunning = false;
        if (thread.IsAlive)
            thread.Join(1000);
    }

    public void Pause()
    {
        isPaused = true;
    }

    public void Resume()
    {
        isPaused = false;
    }

    public bool IsRunning()
    {
        return isRunning && !isStopped && thread.IsAlive;
    }

    private void Run()
    {
        int threadId = Thread.CurrentThread.ManagedThreadId;
        updateStatus($"Читатель {threadId} запущен. Будет прочитано {dataToConsume} данных.");

        while (consumedCount < dataToConsume && !isStopped)
        {
            // Проверка паузы
            while (isPaused && !isStopped)
            {
                Thread.Sleep(100);
            }

            if (isStopped) break;

            // Случайная задержка
            Thread.Sleep(random.Next(500, 1500));

            int data;
            bool success = buffer.Take(out data, threadId);

            if (success)
            {
                consumedCount++;
                updateStatus($"Читатель {threadId} взял: {data} (прогресс: {consumedCount}/{dataToConsume})");
            }
            else
            {
                updateStatus($"Читатель {threadId} ждет - буфер пуст");
            }
        }

        isRunning = false;
        updateStatus($"Читатель {threadId} завершил работу. Прочитано {consumedCount} данных.");
    }
}