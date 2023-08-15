using System;
using System.IO;
using System.IO.Compression;
using WinSCP;

class Program
{
    static string targetDirectory = @"targerDirectory"; // Задаем директорию для отслеживания добавления новых файлов
    static FileSystemWatcher watcher;

    static void Main(string[] args)
    {
        
        Dictionary<string, int> months = new Dictionary<string, int>
        {
            { "Январь", 1 },
            { "Февраль", 2 },
            { "Март", 3 },
            { "Апрель", 4 },
            { "Май", 5 },
            { "Июнь", 6 },
            { "Июль", 7 },
            { "Август", 8 },
            { "Сентябрь", 9 },
            { "Октябрь", 10 },
            { "Ноябрь", 11 },
            { "Декабрь", 12 }
        };

        int currentMonth = DateTime.Now.Month;
        string currentMonthName = months.FirstOrDefault(x => x.Value == currentMonth).Key;

        // Создаем FileSystemWatcher для отслеживания добавления файлов
        watcher = new FileSystemWatcher(targetDirectory + $"/{currentMonthName}");
        watcher.Filter = "*.*"; // Отслеживаем все файлы
        watcher.NotifyFilter = NotifyFilters.FileName;
        watcher.Created += OnFileCreated;
        watcher.EnableRaisingEvents = true;

        Console.WriteLine("Отслеживание началось. Нажмите Enter для завершения.");
        Console.ReadLine();
    }

    private static void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        Console.WriteLine();
        Console.WriteLine($"Новый файл был добавлен, начат процесс архивации и загрузки на сервер: {e.FullPath}");

        ScpLoader();
        Console.WriteLine();
        Console.WriteLine("Архив успешно загружен.");
    }

    private static void ScpLoader()
    {
        var sourceDirName = @"sourceDirName";

        Dictionary<string, int> months = new Dictionary<string, int>
        {
            { "Январь", 1 },
            { "Февраль", 2 },
            { "Март", 3 },
            { "Апрель", 4 },
            { "Май", 5 },
            { "Июнь", 6 },
            { "Июль", 7 },
            { "Август", 8 },
            { "Сентябрь", 9 },
            { "Октябрь", 10 },
            { "Ноябрь", 11 },
            { "Декабрь", 12 }
        };

        if (Directory.Exists(sourceDirName))
        {
            int currentMonth = DateTime.Now.Month;
            string currentMonthName = months.FirstOrDefault(x => x.Value == currentMonth).Key;

            var destinationDirName = Path.Combine(sourceDirName, currentMonthName); // Новая папка с названием месяца

            if (!Directory.Exists(destinationDirName))
            {
                Directory.CreateDirectory(destinationDirName);
            }

            Console.WriteLine("Файлы");
            string[] files = Directory.GetFiles(sourceDirName);

            foreach (string file in files)
            {
                string destinationFilePath = Path.Combine(destinationDirName, Path.GetFileName(file));
                File.Copy(file, destinationFilePath, true); // Копируем файлы в новую папку
                Console.WriteLine($"Скопирован файл: {destinationFilePath}");
            }

            Console.WriteLine();

            string zipFileName = currentMonthName + ".zip";
            string sourceZipFilePath = Path.Combine(sourceDirName, zipFileName);

            string destinationZipFilePath = Path.Combine(sourceDirName, zipFileName);
            ZipFile.CreateFromDirectory(destinationDirName, destinationZipFilePath); // Создаем архив из новой папки

            Console.WriteLine($"Создан архив: {destinationZipFilePath}");

            TransferFilesWithWinSCP(destinationZipFilePath, currentMonth);
            File.Delete(destinationZipFilePath);
        }
    }

    static void TransferFilesWithWinSCP(string destinationZipFilePath, int currentMonth)
    {
        var sessionOptions = new SessionOptions
        {
            Protocol = Protocol.Sftp,
            HostName = "YourHost",
            PortNumber = 22,
            UserName = "UserName",
            Password = "Password",
            SshHostKeyPolicy = SshHostKeyPolicy.AcceptNew
        };

        using (var session = new Session())
        {
            session.Open(sessionOptions);

            var remoteDirectoryPath = $"remoteDirectoryPath{currentMonth}"; // Укажите путь к удаленной директории на сервере
            var remoteArchivePath = $"{remoteDirectoryPath}/{Path.GetFileName(destinationZipFilePath)}";

            var transferResult = session.PutFiles(destinationZipFilePath, remoteArchivePath);

            if (transferResult.IsSuccess)
            {
                foreach (TransferEventArgs transfer in transferResult.Transfers)
                {
                    Console.WriteLine($"Uploaded: {transfer.Destination}");
                }

            }
            else
            {
                Console.WriteLine("Upload failed");
                return;
            }
        }
    }
}
