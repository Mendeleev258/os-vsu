using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace DirectoryComparator
{
    public class DirectorySearcher
    {
        private string directoryPath;
        private int archivedFilesCount = 0;
        private object lockObject = new object();

        public int ArchivedFilesCount => archivedFilesCount;
        public string DirectoryPath => directoryPath;

        public DirectorySearcher(string path)
        {
            directoryPath = path;
        }

        public void SearchArchivedFiles()
        {
            try
            {
                SearchInDirectory(directoryPath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при поиске в каталоге {directoryPath}: {ex.Message}");
            }
        }

        private void SearchInDirectory(string path)
        {
            // Структура для данных о файле
            WIN32_FIND_DATA findData;

            // Формируем путь для поиска
            string searchPath = Path.Combine(path, "*");

            // Начинаем поиск
            IntPtr findHandle = FindFirstFile(searchPath, out findData);

            if (findHandle == INVALID_HANDLE_VALUE)
                return;

            try
            {
                do
                {
                    // Пропускаем текущий и родительский каталоги
                    if (findData.cFileName == "." || findData.cFileName == "..")
                        continue;

                    string fullPath = Path.Combine(path, findData.cFileName);

                    // Проверяем, является ли элемент каталогом
                    if ((findData.dwFileAttributes & FileAttributes.Directory) != 0)
                    {
                        // Рекурсивный поиск в подкаталоге
                        SearchInDirectory(fullPath);
                    }
                    else
                    {
                        // Проверяем, является ли файл архивным
                        if ((findData.dwFileAttributes & FileAttributes.Archive) != 0)
                        {
                            lock (lockObject)
                            {
                                archivedFilesCount++;
                            }
                        }
                    }
                }
                while (FindNextFile(findHandle, out findData));
            }
            finally
            {
                FindClose(findHandle);
            }
        }

        #region WinAPI Imports

        private const int MAX_PATH = 260;

        [Serializable]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct WIN32_FIND_DATA
        {
            public FileAttributes dwFileAttributes;
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
            public int nFileSizeHigh;
            public int nFileSizeLow;
            public int dwReserved0;
            public int dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternate;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FindClose(IntPtr hFindFile);

        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        #endregion
    }
}