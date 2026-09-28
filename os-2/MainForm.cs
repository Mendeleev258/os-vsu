using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace DirectoryComparator
{
    public partial class MainForm : Form
    {
        private Thread searchThread;
        private bool isSearching = false;

        public MainForm()
        {
            InitializeComponent();
            UpdateButtonsState();
        }

        private void btnBrowse1_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtDirectory1.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnBrowse2_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtDirectory2.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtDirectory1.Text) || string.IsNullOrEmpty(txtDirectory2.Text))
            {
                MessageBox.Show("Выберите оба каталога!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Directory.Exists(txtDirectory1.Text) || !Directory.Exists(txtDirectory2.Text))
            {
                MessageBox.Show("Один или оба каталога не существуют!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            StartComparison();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            StopComparison();
        }

        private void StartComparison()
        {
            isSearching = true;
            UpdateButtonsState();
            lstResults.Items.Clear();

            AddStatus("Начато сравнение каталогов...");

            searchThread = new Thread(CompareDirectories);
            searchThread.IsBackground = true;
            searchThread.Start();
        }

        private void StopComparison()
        {
            if (isSearching && searchThread != null && searchThread.IsAlive)
            {
                searchThread.Abort();
                searchThread.Join(1000);
            }

            isSearching = false;
            UpdateButtonsState();
            AddStatus("Сравнение остановлено.");
        }

        private void CompareDirectories()
        {
            try
            {
                string dir1 = txtDirectory1.Text;
                string dir2 = txtDirectory2.Text;

                // Создаем объекты для поиска в каждом каталоге
                var searcher1 = new DirectorySearcher(dir1);
                var searcher2 = new DirectorySearcher(dir2);

                // Запускаем поиск в отдельных потоках
                var thread1 = new Thread(searcher1.SearchArchivedFiles);
                var thread2 = new Thread(searcher2.SearchArchivedFiles);

                AddStatus("Запуск потоков для поиска архивных файлов...");

                thread1.Start();
                thread2.Start();

                // Ожидаем завершения обоих потоков
                thread1.Join();
                thread2.Join();

                // Получаем результаты
                int archivedCount1 = searcher1.ArchivedFilesCount;
                int archivedCount2 = searcher2.ArchivedFilesCount;

                // Выводим результаты в UI потоке
                Invoke(new Action(() =>
                {
                    DisplayResults(dir1, archivedCount1, dir2, archivedCount2);
                }));
            }
            catch (ThreadAbortException)
            {
                // Поток был прерван - это нормально при остановке
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    AddStatus($"Ошибка: {ex.Message}");
                }));
            }
            finally
            {
                isSearching = false;
                Invoke(new Action(UpdateButtonsState));
            }
        }

        private void DisplayResults(string dir1, int count1, string dir2, int count2)
        {
            lstResults.Items.Clear();

            AddStatus("Результаты сравнения:");
            AddStatus($"Каталог 1 ({dir1}): {count1} архивных файлов");
            AddStatus($"Каталог 2 ({dir2}): {count2} архивных файлов");
            AddStatus("");

            if (count1 > count2)
            {
                AddStatus($"Каталог 1 содержит БОЛЬШЕ архивных файлов на {count1 - count2}");
            }
            else if (count2 > count1)
            {
                AddStatus($"Каталог 2 содержит БОЛЬШЕ архивных файлов на {count2 - count1}");
            }
            else
            {
                AddStatus("Каталоги содержат ОДИНАКОВОЕ количество архивных файлов");
            }

            AddStatus("Сравнение завершено.");
        }

        private void AddStatus(string message)
        {
            if (lstResults.InvokeRequired)
            {
                lstResults.Invoke(new Action<string>(AddStatus), message);
            }
            else
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                lstResults.Items.Add($"[{timestamp}] {message}");
                lstResults.TopIndex = lstResults.Items.Count - 1;
            }
        }

        private void UpdateButtonsState()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateButtonsState));
                return;
            }

            btnStart.Enabled = !isSearching;
            btnStop.Enabled = isSearching;
            btnBrowse1.Enabled = !isSearching;
            btnBrowse2.Enabled = !isSearching;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopComparison();
            base.OnFormClosing(e);
        }
    }
}