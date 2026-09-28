using System;
using System.Windows.Forms;

namespace PostalService
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Включаем визуальные стили Windows
            Application.EnableVisualStyles();

            // Устанавливаем совместимость отрисовки текста
            Application.SetCompatibleTextRenderingDefault(false);

            // Создаем и запускаем главную форму
            Application.Run(new MainForm());
        }
    }
}