using System;
using System.Drawing;
using System.Windows.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private Button btnStart;
    private Button btnStop;
    private Button btnPause;
    private Button btnResume;
    private Button btnAddReader;
    private Button btnAddWriter;
    private TextBox txtBufferInfo;
    private ListBox lstStatus;
    private Label lblBuffer;
    private Label lblStatus;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.btnStart = new Button();
        this.btnStop = new Button();
        this.btnPause = new Button();
        this.btnResume = new Button();
        this.btnAddReader = new Button();
        this.btnAddWriter = new Button();
        this.txtBufferInfo = new TextBox();
        this.lstStatus = new ListBox();
        this.lblBuffer = new Label();
        this.lblStatus = new Label();
        this.timer = new System.Windows.Forms.Timer(this.components);

        SuspendLayout();

        // btnStart
        this.btnStart.Location = new Point(20, 20);
        this.btnStart.Size = new Size(100, 30);
        this.btnStart.Text = "Запуск";
        this.btnStart.Click += new EventHandler(btnStart_Click);

        // btnStop
        this.btnStop.Location = new Point(130, 20);
        this.btnStop.Size = new Size(100, 30);
        this.btnStop.Text = "Стоп";
        this.btnStop.Click += new EventHandler(btnStop_Click);

        // btnPause
        this.btnPause.Location = new Point(240, 20);
        this.btnPause.Size = new Size(100, 30);
        this.btnPause.Text = "Пауза";
        this.btnPause.Click += new EventHandler(btnPause_Click);

        // btnResume
        this.btnResume.Location = new Point(350, 20);
        this.btnResume.Size = new Size(100, 30);
        this.btnResume.Text = "Продолжить";
        this.btnResume.Click += new EventHandler(btnResume_Click);

        // btnAddReader
        this.btnAddReader.Location = new Point(460, 20);
        this.btnAddReader.Size = new Size(120, 30);
        this.btnAddReader.Text = "Добавить читателя";
        this.btnAddReader.Click += new EventHandler(btnAddReader_Click);

        // btnAddWriter
        this.btnAddWriter.Location = new Point(590, 20);
        this.btnAddWriter.Size = new Size(120, 30);
        this.btnAddWriter.Text = "Добавить писателя";
        this.btnAddWriter.Click += new EventHandler(btnAddWriter_Click);

        // txtBufferInfo
        this.txtBufferInfo.Location = new Point(20, 80);
        this.txtBufferInfo.Size = new Size(300, 200);
        this.txtBufferInfo.Multiline = true;
        this.txtBufferInfo.ScrollBars = ScrollBars.Vertical;
        this.txtBufferInfo.ReadOnly = true;

        // lstStatus
        this.lstStatus.Location = new Point(330, 80);
        this.lstStatus.Size = new Size(450, 200);

        // lblBuffer
        this.lblBuffer.Location = new Point(20, 60);
        this.lblBuffer.Text = "Состояние буфера:";

        // lblStatus
        this.lblStatus.Location = new Point(330, 60);
        this.lblStatus.Text = "Статус потоков:";

        // timer
        this.timer.Interval = 500;
        this.timer.Tick += new EventHandler(timer_Tick);

        // MainForm
        this.ClientSize = new Size(800, 300);
        this.Text = "Производитель-Потребитель (Стек + Монитор)";
        this.Controls.AddRange(new Control[] {
            btnStart, btnStop, btnPause, btnResume, btnAddReader, btnAddWriter,
            txtBufferInfo, lstStatus, lblBuffer, lblStatus
        });

        ResumeLayout(false);
    }

    private System.Windows.Forms.Timer timer;
}