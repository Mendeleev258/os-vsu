using System;
using System.Threading;

public class Writer
{
    private Buffer buffer;
    private int dataToProduce;
    private int producedCount = 0;
    private Thread thread;
    private bool isPaused = false;
    private bool isStopped = false;
    private Action<string> updateStatus;
    private Random random = new Random();
    private bool isRunning = false;

    public Writer(Buffer buffer, int dataToProduce, Action<string> updateStatus)
    {
        this.buffer = buffer;
        this.dataToProduce = dataToProduce;
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
        updateStatus($"Писатель {threadId} запущен. Будет создано {dataToProduce} данных.");

        while (producedCount < dataToProduce && !isStopped)
        {
            // Проверка паузы
            while (isPaused && !isStopped)
            {
                Thread.Sleep(100);
            }

            if (isStopped) break;

            // Случайная задержка
            Thread.Sleep(random.Next(500, 1500));

            int data = random.Next(1, 100);
            bool success = buffer.Put(data, threadId);

            if (success)
            {
                producedCount++;
                updateStatus($"Писатель {threadId} добавил: {data} (прогресс: {producedCount}/{dataToProduce})");
            }
            else
            {
                updateStatus($"Писатель {threadId} ждет - буфер полон");
            }
        }

        isRunning = false;
        updateStatus($"Писатель {threadId} завершил работу. Создано {producedCount} данных.");
    }
}