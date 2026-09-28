using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;

public partial class MainForm : Form
{
    private Buffer buffer;
    private List<Reader> readers;
    private List<Writer> writers;
    private Random random;
    private bool isRunning = false;

    public MainForm()
    {
        InitializeComponent();
        buffer = new Buffer(5); // Буфер размером 5
        readers = new List<Reader>();
        writers = new List<Writer>();
        random = new Random();

        UpdateBufferInfo();
    }

    private void btnStart_Click(object sender, EventArgs e)
    {
        if (!isRunning)
        {
            isRunning = true;
            timer.Start();
            AddStatus("Система запущена");

            // Автоматически создаем несколько потоков при старте
            for (int i = 0; i < 2; i++)
            {
                btnAddWriter_Click(null, null);
                btnAddReader_Click(null, null);
            }
        }
    }

    private void btnStop_Click(object sender, EventArgs e)
    {
        isRunning = false;
        timer.Stop();

        // Останавливаем всех читателей и писателей
        foreach (var reader in readers)
        {
            reader.Stop();
        }
        foreach (var writer in writers)
        {
            writer.Stop();
        }

        readers.Clear();
        writers.Clear();
        AddStatus("Система остановлена");
        UpdateBufferInfo();
    }

    private void btnPause_Click(object sender, EventArgs e)
    {
        foreach (var reader in readers)
        {
            reader.Pause();
        }
        foreach (var writer in writers)
        {
            writer.Pause();
        }
        AddStatus("Все потоки приостановлены");
    }

    private void btnResume_Click(object sender, EventArgs e)
    {
        foreach (var reader in readers)
        {
            reader.Resume();
        }
        foreach (var writer in writers)
        {
            writer.Resume();
        }
        AddStatus("Все потоки возобновлены");
    }

    private void btnAddReader_Click(object sender, EventArgs e)
    {
        if (isRunning)
        {
            int dataToConsume = random.Next(3, 8);
            Reader reader = new Reader(buffer, dataToConsume, AddStatus);
            readers.Add(reader);
            reader.Start();
        }
        else
        {
            MessageBox.Show("Сначала запустите систему!");
        }
    }

    private void btnAddWriter_Click(object sender, EventArgs e)
    {
        if (isRunning)
        {
            int dataToProduce = random.Next(3, 8);
            Writer writer = new Writer(buffer, dataToProduce, AddStatus);
            writers.Add(writer);
            writer.Start();
        }
        else
        {
            MessageBox.Show("Сначала запустите систему!");
        }
    }

    private void timer_Tick(object sender, EventArgs e)
    {
        UpdateBufferInfo();

        // Удаляем завершенные потоки
        readers.RemoveAll(r => !r.IsRunning());
        writers.RemoveAll(w => !w.IsRunning());

        // Обновляем заголовок с количеством активных потоков
        this.Text = $"Производитель-Потребитель (Стек + Monitor) - Читатели: {readers.Count}, Писатели: {writers.Count}";
    }

    private void UpdateBufferInfo()
    {
        if (txtBufferInfo.InvokeRequired)
        {
            txtBufferInfo.Invoke(new Action(UpdateBufferInfo));
        }
        else
        {
            txtBufferInfo.Text = buffer.GetBufferState();
        }
    }

    private void AddStatus(string message)
    {
        if (lstStatus.InvokeRequired)
        {
            lstStatus.Invoke(new Action<string>(AddStatus), message);
        }
        else
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            lstStatus.Items.Add($"[{timestamp}] {message}");
            lstStatus.TopIndex = lstStatus.Items.Count - 1;

            // Ограничиваем размер списка
            if (lstStatus.Items.Count > 100)
            {
                lstStatus.Items.RemoveAt(0);
            }
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        btnStop_Click(null, null);
        base.OnFormClosing(e);
    }
}