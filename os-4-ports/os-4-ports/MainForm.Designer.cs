using System.Drawing;
using System.Windows.Forms;

namespace PostalService
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private Panel pnlControl;
        private Button btnStartService;
        private Button btnStopService;
        private Button btnStartDemo;
        private Button btnGetStatistics;
        private Button btnClearLog;
        private Button btnClearStatus;
        private Button btnRefreshPorts;

        private GroupBox gbCreateUser;
        private Label lblUserId;
        private TextBox txtUserId;
        private Button btnCreateUser;

        private GroupBox gbSendMessage;
        private Label lblSenderUser;
        private ComboBox cmbSenderUser;
        private Label lblTargetUser;
        private ComboBox cmbTargetUser;
        private Label lblMessageContent;
        private TextBox txtMessageContent;
        private Button btnSendMessage;

        private ListView lvUsers;
        private ListView lvPorts;
        private ListBox lstMessageLog;
        private ListBox lstStatus;

        private Label lblUsers;
        private Label lblPorts;
        private Label lblMessageLog;
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
            this.pnlControl = new Panel();
            this.btnStartService = new Button();
            this.btnStopService = new Button();
            this.btnStartDemo = new Button();
            this.btnGetStatistics = new Button();
            this.btnClearLog = new Button();
            this.btnClearStatus = new Button();
            this.btnRefreshPorts = new Button();

            this.gbCreateUser = new GroupBox();
            this.lblUserId = new Label();
            this.txtUserId = new TextBox();
            this.btnCreateUser = new Button();

            this.gbSendMessage = new GroupBox();
            this.lblSenderUser = new Label();
            this.cmbSenderUser = new ComboBox();
            this.lblTargetUser = new Label();
            this.cmbTargetUser = new ComboBox();
            this.lblMessageContent = new Label();
            this.txtMessageContent = new TextBox();
            this.btnSendMessage = new Button();

            this.lvUsers = new ListView();
            this.lvPorts = new ListView();
            this.lstMessageLog = new ListBox();
            this.lstStatus = new ListBox();

            this.lblUsers = new Label();
            this.lblPorts = new Label();
            this.lblMessageLog = new Label();
            this.lblStatus = new Label();

            this.Text = "Почтовая служба - Правильная архитектура";
            this.ClientSize = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9F);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Панель управления
            this.pnlControl.Location = new Point(10, 10);
            this.pnlControl.Size = new Size(1080, 50);
            this.pnlControl.BackColor = Color.Lavender;
            this.pnlControl.BorderStyle = BorderStyle.FixedSingle;

            this.btnStartService.Location = new Point(10, 10);
            this.btnStartService.Size = new Size(120, 30);
            this.btnStartService.Text = "Запустить службу";
            this.btnStartService.BackColor = Color.LightGreen;
            this.btnStartService.Click += new System.EventHandler(this.btnStartService_Click);

            this.btnStopService.Location = new Point(140, 10);
            this.btnStopService.Size = new Size(120, 30);
            this.btnStopService.Text = "Остановить службу";
            this.btnStopService.BackColor = Color.LightCoral;
            this.btnStopService.Enabled = false;
            this.btnStopService.Click += new System.EventHandler(this.btnStopService_Click);

            this.btnStartDemo.Location = new Point(270, 10);
            this.btnStartDemo.Size = new Size(120, 30);
            this.btnStartDemo.Text = "Запустить демо";
            this.btnStartDemo.BackColor = Color.LightBlue;
            this.btnStartDemo.Enabled = false;
            this.btnStartDemo.Click += new System.EventHandler(this.btnStartDemo_Click);

            this.btnGetStatistics.Location = new Point(400, 10);
            this.btnGetStatistics.Size = new Size(120, 30);
            this.btnGetStatistics.Text = "Статистика";
            this.btnGetStatistics.Click += new System.EventHandler(this.btnGetStatistics_Click);

            this.btnClearLog.Location = new Point(530, 10);
            this.btnClearLog.Size = new Size(120, 30);
            this.btnClearLog.Text = "Очистить лог";
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);

            this.btnClearStatus.Location = new Point(660, 10);
            this.btnClearStatus.Size = new Size(120, 30);
            this.btnClearStatus.Text = "Очистить статус";
            this.btnClearStatus.Click += new System.EventHandler(this.btnClearStatus_Click);

            this.btnRefreshPorts.Location = new Point(790, 10);
            this.btnRefreshPorts.Size = new Size(120, 30);
            this.btnRefreshPorts.Text = "Обновить порты";
            this.btnRefreshPorts.Click += new System.EventHandler(this.btnRefreshPorts_Click);

            this.pnlControl.Controls.AddRange(new Control[] {
                btnStartService, btnStopService, btnStartDemo,
                btnGetStatistics, btnClearLog, btnClearStatus, btnRefreshPorts
            });

            // Группа создания пользователя
            this.gbCreateUser.Location = new Point(10, 70);
            this.gbCreateUser.Size = new Size(480, 80);
            this.gbCreateUser.Text = "Создание пользователя";

            this.lblUserId.Location = new Point(10, 25);
            this.lblUserId.Size = new Size(100, 20);
            this.lblUserId.Text = "ID пользователя:";

            this.txtUserId.Location = new Point(120, 25);
            this.txtUserId.Size = new Size(250, 23);
            this.txtUserId.Enabled = false;
            this.txtUserId.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtUserId_KeyPress);

            this.btnCreateUser.Location = new Point(380, 25);
            this.btnCreateUser.Size = new Size(90, 23);
            this.btnCreateUser.Text = "Создать";
            this.btnCreateUser.BackColor = Color.LightSkyBlue;
            this.btnCreateUser.Enabled = false;
            this.btnCreateUser.Click += new System.EventHandler(this.btnCreateUser_Click);

            this.gbCreateUser.Controls.AddRange(new Control[] {
                lblUserId, txtUserId, btnCreateUser
            });

            // Группа отправки сообщений
            this.gbSendMessage.Location = new Point(500, 70);
            this.gbSendMessage.Size = new Size(590, 110);
            this.gbSendMessage.Text = "Отправка сообщения";

            this.lblSenderUser = new Label();
            this.lblSenderUser.Location = new Point(10, 25);
            this.lblSenderUser.Size = new Size(120, 20);
            this.lblSenderUser.Text = "Отправитель:";

            this.cmbSenderUser = new ComboBox();
            this.cmbSenderUser.Location = new Point(140, 25);
            this.cmbSenderUser.Size = new Size(200, 23);
            this.cmbSenderUser.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbSenderUser.Enabled = false;
            this.cmbSenderUser.SelectedIndexChanged += new System.EventHandler(this.cmbSenderUser_SelectedIndexChanged);

            this.lblTargetUser = new Label();
            this.lblTargetUser.Location = new Point(10, 55);
            this.lblTargetUser.Size = new Size(120, 20);
            this.lblTargetUser.Text = "Получатель:";

            this.cmbTargetUser = new ComboBox();
            this.cmbTargetUser.Location = new Point(140, 55);
            this.cmbTargetUser.Size = new Size(200, 23);
            this.cmbTargetUser.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbTargetUser.Enabled = false;
            this.cmbTargetUser.SelectedIndexChanged += new System.EventHandler(this.cmbTargetUser_SelectedIndexChanged);

            this.lblMessageContent = new Label();
            this.lblMessageContent.Location = new Point(10, 85);
            this.lblMessageContent.Size = new Size(120, 20);
            this.lblMessageContent.Text = "Сообщение:";

            this.txtMessageContent = new TextBox();
            this.txtMessageContent.Location = new Point(140, 85);
            this.txtMessageContent.Size = new Size(200, 23);
            this.txtMessageContent.Enabled = false;
            this.txtMessageContent.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtMessageContent_KeyPress);
            this.txtMessageContent.TextChanged += new System.EventHandler(this.txtMessageContent_TextChanged);

            this.btnSendMessage = new Button();
            this.btnSendMessage.Location = new Point(350, 55);
            this.btnSendMessage.Size = new Size(130, 23);
            this.btnSendMessage.Text = "Отправить";
            this.btnSendMessage.BackColor = Color.LightGreen;
            this.btnSendMessage.Enabled = false;
            this.btnSendMessage.Click += new System.EventHandler(this.btnSendMessage_Click);

            this.gbSendMessage.Controls.AddRange(new Control[] {
                lblSenderUser, cmbSenderUser,
                lblTargetUser, cmbTargetUser,
                lblMessageContent, txtMessageContent,
                btnSendMessage
            });

            // Список пользователей
            this.lblUsers = new Label();
            this.lblUsers.Location = new Point(10, 160);
            this.lblUsers.Size = new Size(480, 20);
            this.lblUsers.Text = "Пользователи:";
            this.lblUsers.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            this.lvUsers = new ListView();
            this.lvUsers.Location = new Point(10, 180);
            this.lvUsers.Size = new Size(480, 180);
            this.lvUsers.View = View.Details;
            this.lvUsers.FullRowSelect = true;
            this.lvUsers.MultiSelect = false;
            this.lvUsers.Columns.Add("ID пользователя", 120);
            this.lvUsers.Columns.Add("Ящик", 100);
            this.lvUsers.Columns.Add("Порт", 80);
            this.lvUsers.Columns.Add("Статус", 80);
            this.lvUsers.Columns.Add("Отправлено", 70);
            this.lvUsers.Columns.Add("Получено", 70);
            this.lvUsers.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvUsers_MouseDoubleClick);

            // Список портов
            this.lblPorts = new Label();
            this.lblPorts.Location = new Point(500, 160);
            this.lblPorts.Size = new Size(580, 20);
            this.lblPorts.Text = "Порты (активные маршрутизаторы):";
            this.lblPorts.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            this.lvPorts = new ListView();
            this.lvPorts.Location = new Point(500, 180);
            this.lvPorts.Size = new Size(590, 180);
            this.lvPorts.View = View.Details;
            this.lvPorts.FullRowSelect = true;
            this.lvPorts.MultiSelect = false;
            this.lvPorts.Columns.Add("ID порта", 100);
            this.lvPorts.Columns.Add("Ящиков", 60);
            this.lvPorts.Columns.Add("Статус", 80);
            this.lvPorts.Columns.Add("Отпр.", 50);
            this.lvPorts.Columns.Add("Получ.", 50);
            this.lvPorts.Columns.Add("Маршр.", 50);
            this.lvPorts.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvPorts_MouseDoubleClick);

            // Лог сообщений
            this.lblMessageLog = new Label();
            this.lblMessageLog.Location = new Point(10, 370);
            this.lblMessageLog.Size = new Size(480, 20);
            this.lblMessageLog.Text = "Лог сообщений:";
            this.lblMessageLog.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            this.lstMessageLog = new ListBox();
            this.lstMessageLog.Location = new Point(10, 390);
            this.lstMessageLog.Size = new Size(480, 240);
            this.lstMessageLog.Font = new Font("Consolas", 8F);
            this.lstMessageLog.HorizontalScrollbar = true;

            // Статус системы
            this.lblStatus = new Label();
            this.lblStatus.Location = new Point(500, 370);
            this.lblStatus.Size = new Size(580, 20);
            this.lblStatus.Text = "Статус системы:";
            this.lblStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            this.lstStatus = new ListBox();
            this.lstStatus.Location = new Point(500, 390);
            this.lstStatus.Size = new Size(590, 240);
            this.lstStatus.Font = new Font("Consolas", 8F);
            this.lstStatus.HorizontalScrollbar = true;

            this.Controls.AddRange(new Control[] {
                pnlControl,
                gbCreateUser,
                gbSendMessage,
                lblUsers, lvUsers,
                lblPorts, lvPorts,
                lblMessageLog, lstMessageLog,
                lblStatus, lstStatus
            });
        }
    }
}