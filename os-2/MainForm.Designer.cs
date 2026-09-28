using System;
using System.Windows.Forms;

namespace DirectoryComparator
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private TextBox txtDirectory1;
        private TextBox txtDirectory2;
        private Button btnBrowse1;
        private Button btnBrowse2;
        private Button btnStart;
        private Button btnStop;
        private ListBox lstResults;
        private Label lblDir1;
        private Label lblDir2;

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
            this.txtDirectory1 = new TextBox();
            this.txtDirectory2 = new TextBox();
            this.btnBrowse1 = new Button();
            this.btnBrowse2 = new Button();
            this.btnStart = new Button();
            this.btnStop = new Button();
            this.lstResults = new ListBox();
            this.lblDir1 = new Label();
            this.lblDir2 = new Label();

            // MainForm
            this.SuspendLayout();
            this.Text = "Сравнение каталогов по архивным файлам";
            this.ClientSize = new System.Drawing.Size(700, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new System.Drawing.Size(600, 300);

            // Label Directory 1
            this.lblDir1.Location = new System.Drawing.Point(20, 20);
            this.lblDir1.Size = new System.Drawing.Size(120, 20);
            this.lblDir1.Text = "Каталог 1:";

            // TextBox Directory 1
            this.txtDirectory1.Location = new System.Drawing.Point(140, 20);
            this.txtDirectory1.Size = new System.Drawing.Size(400, 20);
            this.txtDirectory1.ReadOnly = true;

            // Button Browse 1
            this.btnBrowse1.Location = new System.Drawing.Point(550, 20);
            this.btnBrowse1.Size = new System.Drawing.Size(120, 23);
            this.btnBrowse1.Text = "Обзор...";
            this.btnBrowse1.Click += new EventHandler(this.btnBrowse1_Click);

            // Label Directory 2
            this.lblDir2.Location = new System.Drawing.Point(20, 60);
            this.lblDir2.Size = new System.Drawing.Size(120, 20);
            this.lblDir2.Text = "Каталог 2:";

            // TextBox Directory 2
            this.txtDirectory2.Location = new System.Drawing.Point(140, 60);
            this.txtDirectory2.Size = new System.Drawing.Size(400, 20);
            this.txtDirectory2.ReadOnly = true;

            // Button Browse 2
            this.btnBrowse2.Location = new System.Drawing.Point(550, 60);
            this.btnBrowse2.Size = new System.Drawing.Size(120, 23);
            this.btnBrowse2.Text = "Обзор...";
            this.btnBrowse2.Click += new EventHandler(this.btnBrowse2_Click);

            // Button Start
            this.btnStart.Location = new System.Drawing.Point(140, 100);
            this.btnStart.Size = new System.Drawing.Size(120, 30);
            this.btnStart.Text = "Начать сравнение";
            this.btnStart.BackColor = System.Drawing.Color.LightGreen;
            this.btnStart.Click += new EventHandler(this.btnStart_Click);

            // Button Stop
            this.btnStop.Location = new System.Drawing.Point(280, 100);
            this.btnStop.Size = new System.Drawing.Size(120, 30);
            this.btnStop.Text = "Остановить";
            this.btnStop.BackColor = System.Drawing.Color.LightCoral;
            this.btnStop.Click += new EventHandler(this.btnStop_Click);

            // ListBox Results
            this.lstResults.Location = new System.Drawing.Point(20, 150);
            this.lstResults.Size = new System.Drawing.Size(650, 220);
            this.lstResults.Font = new System.Drawing.Font("Consolas", 9F);

            // Add controls to form
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblDir1, this.txtDirectory1, this.btnBrowse1,
                this.lblDir2, this.txtDirectory2, this.btnBrowse2,
                this.btnStart, this.btnStop, this.lstResults
            });

            this.ResumeLayout(false);
        }
    }
}