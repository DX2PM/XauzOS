using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using IL2CPU.API.Attribs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Sys = Cosmos.System;

namespace Xauz.XSys
{
    public static class XShell
    {
        public static CosmosVFS fs = new CosmosVFS();
        public static string uname = "xausr";
        public static int uid = 0;
        public static string pcname = "xauz-pc";
        public static string cwd = @"0:\home\xausr\";
        public static Exception ex;
        public static List<string> history = new List<string>();
        public static Dictionary<string, string> EnvVars = new Dictionary<string, string>();

        public static void LoadPath()
        {
            if (File.Exists(@"0:\etc\pth"))
            {
                string[] lines = File.ReadAllLines(@"0:\etc\pth");
                foreach (var line in lines)
                {
                    if (line.Contains("="))
                    {
                        var parts = line.Split('=');
                        EnvVars[parts[0].Trim()] = parts[1].Trim();
                    }
                }
            }
        }
        public static void Welcome()
        {
            Console.Write("Welcome to ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Xauz ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Type '");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("help");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("' for more ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("commands");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(".\n");

        }
        public static bool AreArraysEqual(string[] a, string[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }
        public static void Init()
        {
            Console.Clear();
            Console.Write("[ WAITING ] Initializating FS\r");
            VFSManager.RegisterVFS(fs);
            Console.WriteLine("[ OK ] Initializating FS");
            Console.Write("[ WAITING ] Collecting Heap\r");
            Heap.Collect();
            Console.WriteLine("[ OK ] Collecting Heap");
            if (!Directory.Exists(cwd))
            {
                fs.CreateDirectory(cwd);
            }
            if (!EnvVars.ContainsKey("HOME"))
            {
                RunCommand("set HOME " + cwd);
            }
            if (!EnvVars.ContainsKey("USER"))
            {
                RunCommand("set USER " + uname);
            }
            if (!EnvVars.ContainsKey("AUTORUN"))
            {
                RunCommand(@"set AUTORUN xsh 0:\autorun.xsh");
            }
            if (!File.Exists(@"0:\autorun.xsh"))
            {
                fs.CreateFile(@"0:\autorun.xsh");
                string[] ls =
                {
                    "# This is autorun script. It will be executed every time when XShell starts.",
                    "# You can put any commands here, for example, to set env variables or start some programs.",
                    "# Lines starting with # are comments and will be ignored.",
                    "",
                    "echo Hello, $USER! Welcome to Xauz Shell!",
                    "echo Your current directory is $HOME",
                    "echo Type 'help' to see available commands."
                };
                File.WriteAllLines(@"0:\autorun.xsh", ls);
            }
            string[] lines =
                {
                    "# This is autorun script. It will be executed every time when XShell starts.",
                    "# You can put any commands here, for example, to set env variables or start some programs.",
                    "# Lines starting with # are comments and will be ignored.",
                    "",
                    "echo Hello, $USER! Welcome to Xauz Shell!",
                    "echo Your current directory is $HOME",
                    "echo Type 'help' to see available commands."
                };
            if (!AreArraysEqual(File.ReadAllLines(@"0:\autorun.xsh"), lines))
            {
                Console.WriteLine("Default autorun script has been overwriten. Do you want to set it to default? y/n");
                if (Console.ReadKey(true).Key == ConsoleKey.Y)
                {
                    File.WriteAllLines(@"0:\autorun.xsh", lines);
                    Console.WriteLine("Autorun script has been set to default.");
                }
                else
                {
                    Console.WriteLine("Continuing with custom AUTORUN");
                }
            }
            LoadPath();
            Console.Clear();
            BModes.ChangeMode();
        }

        public static string ExpandVars(string input)
        {
            foreach (var var in EnvVars)
            {
                // Заменяем $NAME на значение из словаря
                input = input.Replace("$" + var.Key, var.Value);
            }
            return input;
        }

        public static void Promt()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{uname} ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"@{pcname} ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"@{cwd} ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("> ");
        }


        public static int Run()
        {
            Promt();
            var input = ReadCmd();

            if (string.IsNullOrWhiteSpace(input)) return 0;

            history.Add(input);

            try
            {
                RunCommand(input);
                return 0;
            }
            catch (Exception ex)
            {
                XShell.ex = ex;
                Cosmos.HAL.PCSpeaker.Beep(200, 200);
                Cosmos.HAL.PCSpeaker.Beep(150, 400);
                return 1;
            }
        }

        public static void RunCommand(string input)
        {
            input = input.Trim();
            input = ExpandVars(input);
            string[] parts = input.Split(' ');
            switch (parts[0])
            {
                    case "": break;
                case "matrix":
                    Console.Clear();

                    int width = Console.WindowWidth;
                    int height = Console.WindowHeight;

                    // Массив для хранения текущей позиции 'головы' в каждой колонке
                    int[] yPositions = new int[width];
                    Random rnd = new Random();

                    // Инициализируем случайными стартовыми позициями (чтобы не все сразу падали)
                    for (int i = 0; i < width; i++) yPositions[i] = rnd.Next(0, height);

                    char[] chars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

                    while (true)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            // 1. Рисуем "Голову" (яркий белый или светло-зеленый)
                            Console.SetCursorPosition(x, yPositions[x]);
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write(chars[rnd.Next(chars.Length)]);

                            int bodyY = (yPositions[x] - 1 + height) % height;
                            Console.SetCursorPosition(x, bodyY);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(chars[rnd.Next(chars.Length)]);

                            int tailY = (yPositions[x] - 3 + height) % height;
                            Console.SetCursorPosition(x, tailY);
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.Write(chars[rnd.Next(chars.Length)]);

                            // 4. "Стираем" старый хвост (черный)
                            int clearY = (yPositions[x] - 4 + height) % height;
                            Console.SetCursorPosition(x, clearY);
                            Console.Write(" ");

                            // Двигаем голову вниз
                            yPositions[x]++;
                            if (yPositions[x] >= height) yPositions[x] = 0;
                        }

                        // Скорость обновления
                        Thread.Sleep(30);
                        if (Sys.KeyboardManager.TryReadKey(out var k))
                        {
                            if (Sys.KeyboardManager.ControlPressed && k.Key == Sys.ConsoleKeyEx.C)
                            {
                                break;
                            }
                        }
                    }
                    break;
                    case "help":
                    if (parts.Length == 1)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(@"Available commands:
help - show this msg     cd - choice directory     poweroff - shutdown PC
echo - print text        cat - read file content   set - add env variable
ls - list entries        file - show file info     sleep - wait some time
touch - create new file  pcinfo - show pc info     history - show command history
mkdir - create directory noted - open text editor  $VAR - expand env variable
rm - remove file         cp - copy file            xsh - run script
rmdir - remove directory mv - move file            del - remove env variable
cls - clear screen       reboot - reset CPU");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else
                    {
                        switch (parts[1]) 
                        {
                            case "help":
                                Console.WriteLine("Show list of available commands, or info about one command, and it usage. Usage: help [command]");
                                break;
                            case "echo":
                                Console.WriteLine("Print text to console. Usage: echo [text]");
                                break;
                            case "ls":
                                Console.WriteLine("List entries in current directory. Usage: ls");
                                break;
                            case "touch":
                                Console.WriteLine("Create new empty file. Usage: touch [filename]");
                                break;
                            case "mkdir":
                                Console.WriteLine("Create new directory. Usage: mkdir [directoryname]");
                                break;
                            case "rm":
                                Console.WriteLine("Remove file. Usage: rm [filename]");
                                break;
                            case "rmdir":
                                Console.WriteLine("Remove directory. Usage: rmdir [directoryname]");
                                break;
                            case "cls":
                                Console.WriteLine("Clear console. Usage: cls");
                                break;
                            case "cd":
                                Console.WriteLine("Change current directory. Usage: cd [path]\n" +
                                    "Path can be absolute (starting with 0:\\) or relative.\n" +
                                    "Special paths: ~ (home), .. (parent), root or / (root directory)");
                                break;
                            case "cat":
                                Console.WriteLine("Print file content to console. Usage: cat [filename]");
                                break;
                            case "file":
                                Console.WriteLine("Show file info. Usage: file [filename]");
                                break;
                            case "sleep":
                                Console.WriteLine("Wait some time. Usage: sleep [milliseconds]");
                                break;
                            case "set":
                                Console.WriteLine("Add environment variable. Usage: set [name] [value]");
                                break;
                            case "del":
                                Console.WriteLine("Remove environment variable. Usage: del [name]");
                                break;
                            case "history":
                                Console.WriteLine("Show command history. Usage: history");
                                break;
                            case "reboot":
                                Console.WriteLine("Reboot PC. Usage: reboot");
                                break;
                            case "poweroff":
                                Console.WriteLine("Shutdown PC. Usage: poweroff");
                                break;
                            case "noted":
                                Console.WriteLine("Open Noted text editor. Usage: noted [filename]");
                                break;
                            case "cp":
                                Console.WriteLine("Copy file. Usage: cp [source] [destination]");
                                break;
                            case "mv":
                                Console.WriteLine("Move file. Usage: mv [source] [destination]");
                                break;
                            case "pcinfo":
                                Console.WriteLine("Show info about CPU and RAM");
                                break;
                            case "debug":
                                Console.Clear();
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.BackgroundColor = ConsoleColor.Blue;
                                Console.Clear();
                                Console.WriteLine(new string(' ', (Console.WindowWidth / 2) - 18) + "Forced Debug Mode!");
                                Console.WriteLine("this mode is not error, and not BSoD. it's debug mode, using for. . . secret)");
                                Console.WriteLine(new string(' ', (Console.WindowWidth / 2) - 27) + "'exit' to return into XShell");
                                while (true)
                                {
                                    Console.Write($"!dbg > {cwd} # ");
                                    input = Console.ReadLine();
                                    parts = input.Split(' ');
                                    if (parts[0] == "exit")
                                    {
                                        Console.ResetColor();
                                        Console.Clear();
                                        break;
                                    }
                                    switch (parts[0])
                                    {
                                        case "throw":
                                            if (parts[1] == "fnfe")
                                            {
                                                throw new FileNotFoundException();
                                            }
                                            else if (parts[1] == "ex")
                                            {
                                                throw new Exception("Test exception");
                                            }
                                            break;
                                        case "ls":
                                            foreach (var entry in fs.GetDirectoryListing(cwd))
                                            {
                                                if (entry.mEntryType == Sys.FileSystem.Listing.DirectoryEntryTypeEnum.File)
                                                {
                                                    Console.ForegroundColor = ConsoleColor.Magenta;
                                                    Console.WriteLine(entry.mName);
                                                }
                                                else
                                                {
                                                    Console.ForegroundColor = ConsoleColor.White;
                                                    Console.WriteLine($"{entry.mName}");
                                                }
                                            }
                                            Console.ForegroundColor = ConsoleColor.White;
                                            break;
                                        case "touch":
                                            if (parts.Length == 1) { Console.WriteLine("Usage: touch [filename]"); break; }
                                            fs.CreateFile(cwd + parts[1]);
                                            break;
                                        case "mkdir":
                                            if (parts.Length == 1) { Console.WriteLine("Usage: mkdir [directoryname]"); break; }
                                            fs.CreateDirectory(cwd + parts[1]);
                                            break;
                                        case "rm":
                                            if (parts.Length == 1) { Console.WriteLine("Usage: rm [filename]"); break; }
                                            if (File.Exists(cwd + parts[1]))
                                            {
                                                File.Delete(cwd + parts[1]);
                                            }
                                            break;
                                        case "rmdir":
                                            if (parts.Length == 1) { Console.WriteLine("Usage: rmdir [directoryname]"); break; }
                                            if (Directory.Exists(cwd + parts[1]))
                                            {
                                                Directory.Delete(cwd + parts[1]);
                                            }
                                            break;
                                        case "cd":
                                            if (parts.Length == 1) { Console.WriteLine("Usage: cd [path]"); break; }

                                            string trgt = parts[1];
                                            string ntDr = "";

                                            // 1. Обработка сокращений (Aliases)
                                            if (trgt == "~")
                                            {
                                                // Берем из EnvVars, если нет - дефолт
                                                ntDr = EnvVars.ContainsKey("HOME") ? EnvVars["HOME"] : @"0:\home\xausr\";
                                            }
                                            else if (trgt == "..")
                                            {
                                                // Логика выхода на уровень выше
                                                var parent = Directory.GetParent(cwd.TrimEnd('\\'));
                                                if (parent != null)
                                                {
                                                    ntDr = parent.FullName;
                                                }
                                                else
                                                {
                                                    ntDr = cwd; // Мы уже в корне
                                                }
                                            }
                                            else if (trgt == "root" || trgt == "/")
                                            {
                                                ntDr = @"0:\";
                                            }
                                            else if (trgt.Contains(@":\"))
                                            {
                                                ntDr = trgt;
                                            }
                                            else
                                            {
                                                // Обычный относительный путь
                                                ntDr = cwd.EndsWith("\\") ? cwd + trgt : cwd + "\\" + trgt;
                                            }

                                            // 2. Нормализация пути (чтобы не было 0:\home\\folder)
                                            if (!ntDr.EndsWith("\\")) ntDr += "\\";

                                            // 3. Проверка и переход
                                            if (Directory.Exists(ntDr))
                                            {
                                                cwd = ntDr;
                                                Directory.SetCurrentDirectory(cwd); // Важно для работы системных методов
                                            }
                                            else
                                            {
                                                Console.WriteLine("Directory not found: " + ntDr);
                                            }
                                            break;
                                        case "cat":
                                            if (parts.Length == 1) { Console.WriteLine("Usage: cat [filename]"); break; }
                                            foreach (byte b in File.ReadAllBytes(cwd + parts[1]))
                                            {
                                                if (b >= 32 || b == 10 || b == 13)
                                                {
                                                    Console.Write((char)b);
                                                }
                                                else
                                                {
                                                    Console.Write(".");
                                                }
                                            }
                                            break;
                                        case "file":
                                            if (parts.Length == 1) { Console.WriteLine("Usage: file [filename]"); break; }
                                            if (File.Exists(cwd + parts[1]))
                                            {
                                                var file = fs.GetFile(cwd + parts[1]);
                                                Console.WriteLine($@"File info:
size {file.mSize}
name {file.mName}
parrent {file.mParent.mName}
path {file.mFullPath}");
                                            }
                                            break;
                                        case "help":
                                            Console.WriteLine(@"Avialable Commands:
trigger - simulate an PANIC
crash - call an 0x06 interrupt (InvalidOpCode06)
file managment - same as in shell
throw - throw an exception (fnfe - FileNotFound, ex - Test Exception)
exit - return to terminal");
                                            break;
                                        case "trigger":
                                            unsafe
                                            {
                                                byte* memPtr = (byte*)0x100000;

                                                int w = 80;
                                                int h = 25;
                                                int blockSize = w * h;

                                                while (true)
                                                {
                                                    Console.Clear();
                                                    // Читаем блок 80x25
                                                    for (int i = 0; i < blockSize; i++)
                                                    {
                                                        byte data = *(memPtr + i);

                                                        // Преобразуем байт в читаемый символ. 
                                                        // Если символ непечатный (управляющий), заменим его на точку, чтобы не сломать консоль.
                                                        char c = (data < 32 || data > 126) ? '.' : (char)data;

                                                        Console.Write(c);
                                                        try
                                                        {
                                                            File.WriteAllText(@"0:\crashdump.bin", c.ToString());
                                                        }
                                                        catch
                                                        {}
                                                    }

                                                    // Звуковой сигнал и пауза
                                                    Cosmos.HAL.PCSpeaker.Beep(200, 500);

                                                    // В Cosmos Thread.Sleep работает через PIT (Programmable Interval Timer)
                                                    Thread.Sleep(100);

                                                    // Переходим к следующему блоку памяти
                                                    memPtr += blockSize;

                                                    // Небольшая защита: если дошли до конца (условно 4 ГБ для x86), прыгаем в начало
                                                    if ((uint)memPtr >= 0xFFFFFFFF - blockSize)
                                                    {
                                                        break;
                                                    }
                                                }
                                            }
                                            break;
                                    }
                                }
                                break;
                            default:
                                Console.WriteLine("Unknown command, or it doesn't have description");
                                break;
                        }
                    }
                    break;
                case "sleep":
                        if (parts.Length == 1 || !int.TryParse(parts[1], out int ms)) { Console.WriteLine("Usage: sleep [milliseconds]"); break; }
                        Thread.Sleep(ms);
                        break;
                case "run":
                    Console.WriteLine("Running binaries is currently unaviable.");
                    break;
                case "lsp":
                    // Шапка таблицы
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("+----------------------+----------------------------------------+");
                    Console.Write("| ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Variable Name       ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("| ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Value                                  |");

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("+----------------------+----------------------------------------+");

                    foreach (var entry in EnvVars)
                    {
                        string name = entry.Key;
                        string val = entry.Value;

                        // Обрезка, чтобы не ломать границы (20 символов для имени, 38 для значения)
                        if (name.Length > 20) name = name.Substring(0, 17) + "...";
                        if (val.Length > 38) val = val.Substring(0, 35) + "...";

                        // Форматирование строк
                        string nameRow = name.PadRight(20);
                        string valRow = val.PadRight(38);

                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write("| ");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(nameRow);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write(" | ");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write(valRow);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine(" |");
                    }

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("+----------------------+----------------------------------------+");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($" Total: {EnvVars.Count} variables");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case "echo":
                        if (parts.Length == 1) { Console.WriteLine("Usage: echo [text]"); break; }
                    foreach (char c in input.Skip(5))
                        Console.Write(c);
                    Console.WriteLine();
                        break;
                    case "cls":
                        Console.Clear();
                        break;
                case "set":
                    if (parts.Length < 3) { Console.WriteLine("Usage: addenv [name] [value]"); break; }
                    string varName = parts[1];
                    string varValue = ManualJoin(parts, 2); // Склеиваем всё, начиная с 3-го слова

                    EnvVars[varName] = varValue;
                    try
                    {
                        List<string> lines = new List<string>();
                        foreach (var entry in EnvVars)
                        {
                            lines.Add($"{entry.Key}={entry.Value}");
                        }
                        File.WriteAllLines(@"0:\etc\pth", lines.ToArray());
                        Console.WriteLine($"Env '{parts[1]}' saved to pth.");
                        LoadPath();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to save to pth: " + ex.Message);
                    }
                    break;
                case "del":
                    if (parts.Length < 2) { Console.WriteLine("Usage: del [name]"); break; }
                    if (EnvVars.Remove(parts[1]))
                    {
                        try
                        {
                            List<string> lines = new List<string>();
                            foreach (var entry in EnvVars)
                            {
                                lines.Add($"{entry.Key}={entry.Value}");
                            }
                            File.WriteAllLines(@"0:\etc\pth", lines.ToArray());
                            Console.WriteLine($"Env '{parts[1]}' removed from pth.");
                            LoadPath();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to save to pth: " + ex.Message);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Env '{parts[1]}' not found.");
                    }
                    break;
                case "history":
                    for (int i = 0; i < history.Count; i++)
                        Console.WriteLine($"{i}: {history[i]}");
                    break;
                case "reboot":
                        Cosmos.System.Power.Reboot();
                        break;
                    case "poweroff":
                        Cosmos.System.Power.Shutdown();
                        break;
                case "ls":

                        foreach (var entry in fs.GetDirectoryListing(cwd))
                        {
                            if (entry.mEntryType == Sys.FileSystem.Listing.DirectoryEntryTypeEnum.File)
                            {
                                Console.ForegroundColor = ConsoleColor.Magenta;
                                Console.WriteLine(entry.mName);
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine($"{entry.mName}");
                            }
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case "touch":
                        if (parts.Length == 1) { Console.WriteLine("Usage: touch [filename]"); break; }
                        fs.CreateFile(cwd + parts[1]);
                        break;
                    case "mkdir":
                        if (parts.Length == 1) { Console.WriteLine("Usage: mkdir [directoryname]"); break; }
                        fs.CreateDirectory(cwd + parts[1]);
                        break;
                    case "rm":
                        if (parts.Length == 1) { Console.WriteLine("Usage: rm [filename]"); break; }
                        if (File.Exists(cwd + parts[1]))
                        {
                            File.Delete(cwd + parts[1]);
                        }
                        break;
                    case "rmdir":
                        if (parts.Length == 1) { Console.WriteLine("Usage: rmdir [directoryname]"); break; }
                        if (Directory.Exists(cwd + parts[1]))
                        {
                            Directory.Delete(cwd + parts[1]);
                        }
                        break;
                case "throw":
                    if (parts[1] == "fnfe")
                    {
                        throw new FileNotFoundException();
                    }
                    else if (parts[1] == "ex")
                    {
                        throw new Exception("Test exception");
                    }
                    break;
                case "cd":
                    if (parts.Length == 1) { Console.WriteLine("Usage: cd [path]"); break; }

                    string target = parts[1];
                    string nextDir = "";

                    // 1. Обработка сокращений (Aliases)
                    if (target == "~")
                    {
                        // Берем из EnvVars, если нет - дефолт
                        nextDir = EnvVars.ContainsKey("HOME") ? EnvVars["HOME"] : @"0:\home\xausr\";
                    }
                    else if (target == "..")
                    {
                        // Логика выхода на уровень выше
                        var parent = Directory.GetParent(cwd.TrimEnd('\\'));
                        if (parent != null)
                        {
                            nextDir = parent.FullName;
                        }
                        else
                        {
                            nextDir = cwd; // Мы уже в корне
                        }
                    }
                    else if (target == "root" || target == "/")
                    {
                        nextDir = @"0:\";
                    }
                    else if (target.Contains(@":\"))
                    {
                        nextDir = target;
                    }
                    else
                    {
                        // Обычный относительный путь
                        nextDir = cwd.EndsWith("\\") ? cwd + target : cwd + "\\" + target;
                    }

                    // 2. Нормализация пути (чтобы не было 0:\home\\folder)
                    if (!nextDir.EndsWith("\\")) nextDir += "\\";

                    // 3. Проверка и переход
                    if (Directory.Exists(nextDir))
                    {
                        cwd = nextDir;
                        Directory.SetCurrentDirectory(cwd); // Важно для работы системных методов
                    }
                    else
                    {
                        Console.WriteLine("Directory not found: " + nextDir);
                    }
                    break;
                case "cat":
                        if (parts.Length == 1) { Console.WriteLine("Usage: cat [filename]"); break; }
                        foreach (byte b in File.ReadAllBytes(cwd + parts[1]))
                        {
                            if (b >= 32 || b == 10 || b == 13)
                            {
                                Console.Write((char)b);
                            }
                            else
                            {
                                Console.Write(".");
                            }
                        }
                        break;
                    case "file":
                        if (parts.Length == 1) { Console.WriteLine("Usage: file [filename]"); break; }
                        if (File.Exists(cwd + parts[1]))
                        {
                            var file = fs.GetFile(cwd + parts[1]);
                            Console.WriteLine($@"File info:
size {file.mSize}
name {file.mName}
parrent {file.mParent.mName}
path {file.mFullPath}");
                        }
                        break;
                case "fastfetch":
                    case "neofetch":
                    case "sysinfo":
                    case "fetch":
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(@$"
____  ___       ____ _____________  OS-name: Xauz
\   \/  /____  |    |   \____    /  Kernel: CosmosXZ
 \     /\__  \ |    |   / /     /   PC-name: {pcname}
 /     \ / __ \|    |  / /     /_   username: {uname}
/___/\  (____  /______/ /_______ \  CPU: {CPU.GetCPUBrandString()}
      \_/    \/                 \/  user-id: {uid}");
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case "pcinfo":
                        string cpu = CPU.GetCPUBrandString();
                        string ram = $"{GCImplementation.GetUsedRAM() / 1024} / {CPU.GetAmountOfRAM() / 1024} KB";
                        Console.WriteLine($@"PC info:
CPU [1]: {cpu}
RAM: {ram}");
                        break;
                    case "noted":
                        NotedEditor(cwd + (parts.Length > 1 ? parts[1] : "untitled.txt"));
                    break;
                    case "cp":
                        if (parts.Length < 3) { Console.WriteLine("Usage: cp [source] [destination]"); break; }
                        File.Copy(cwd + parts[1], cwd + parts[2]);
                        break;
                    case "mv":
                        if (parts.Length < 3) { Console.WriteLine("Usage: mv [source] [destination]"); break; }
                        File.Copy(cwd + parts[1], cwd + parts[2]);
                        File.Delete(cwd + parts[1]);
                        break;
                case "xsh":
                    if (parts.Length == 1) { Console.WriteLine("Usage: xsh [filename]"); break; }
                    if (parts[1].Contains(@":\"))
                    {
                        xsh(parts[1]);
                    }
                    else
                    xsh(cwd + parts[1]);
                    break;
                case "pdef":
                    RunCommand("del HOME");
                    RunCommand("del USER");
                    RunCommand("del AUTORUN");
                    foreach (var entry in EnvVars)
                    {
                        RunCommand("del " + entry.Key);
                    }
                    if (!EnvVars.ContainsKey("HOME"))
                    {
                        RunCommand("set HOME " + cwd);
                    }
                    if (!EnvVars.ContainsKey("USER"))
                    {
                        RunCommand("set USER " + uname);
                    }
                    if (!EnvVars.ContainsKey("AUTORUN"))
                    {
                        RunCommand(@"set AUTORUN xsh 0:\autorun.xsh");
                    }
                    break;
                case "if":
                    // Структура: if $VAR == VALUE COMMAND
                    if (parts.Length >= 5)
                    {
                        varName = parts[1]; // Например, "$status"
                        string op = parts[2];      // "=="
                        string targetValue = parts[3]; // "0"

                        // Получаем значение переменной (у тебя уже должна быть логика расширения $VAR)
                        string actualValue = ExpandVars(varName);


                        if (op == "==")
                        {
                            if (actualValue == targetValue)
                            {
                                // Выполняем оставшуюся часть строки как команду
                                string commandToRun = string.Join(" ", parts.Skip(4));
                                RunCommand(commandToRun);
                            }
                        }
                        else if (op == "!=")
                        {
                            if (actualValue != targetValue)
                            {
                                string commandToRun = string.Join(" ", parts.Skip(4));
                                RunCommand(commandToRun);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Usage: if [var] [==/!=] [value] [command]");
                    }
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Unknown command: " + parts[0]);
                    string sg = GetSuggestion(cwd + parts[0]);
                    Console.ForegroundColor = ConsoleColor.Green;
                    if (!string.IsNullOrEmpty(sg)){
                        Console.WriteLine("Maybe you may "+sg);
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }
            }

        public static string ManualJoin(string[] parts, int startIndex)
        {
            string result = "";
            for (int i = startIndex; i < parts.Length; i++)
            {
                result += parts[i];
                if (i < parts.Length - 1)
                {
                    result += " "; // Добавляем пробел между словами
                }
            }
            return result;
        }

        public static void xsh(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Console.WriteLine("empty!");
                return;
            }
            else if (!File.Exists(path))
            {
                Console.WriteLine("No such file");
                return;
            }
            else
            {
                string[] ctx = File.ReadAllLines(path);
                for (int i = 0; i < ctx.Length; i++)
                {
                    if (ctx[i].StartsWith("#")) continue;
                    else if (ctx[i].StartsWith("jmp")) { i = int.Parse(ctx[i].Split(' ')[1]) - 1; } // -1 because of i++
                    else
                    {
                        try
                        {
                            RunCommand(ctx[i]);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error in line {i + 1}: {ex.Message}");
                            break;
                        }
                    }
                }
            }
        }

        public static void NotedEditor(string filename)
        {
            List<string> lines = new List<string>();

            // 1. Загрузка файла
            if (File.Exists(filename))
            {
                lines.AddRange(File.ReadAllLines(filename));
            }
            if (lines.Count == 0) lines.Add(""); // Пустая строка, если файл новый

            int curX = 0, curY = 0; // Позиция в тексте
            bool running = true;

            while (running)
            {
                Console.Clear();
                // 2. Отрисовка интерфейса
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.WriteLine($" NOTED Editor - {filename} | F2: Save | Esc: Exit ");
                Console.ResetColor();

                // 3. Отрисовка текста
                for (int i = 0; i < lines.Count; i++)
                {
                    Console.WriteLine(lines[i]);
                }

                // Ставим курсор туда, где мы сейчас в тексте
                // +1 к Y, так как первая строка занята заголовком
                Console.SetCursorPosition(curX, curY + 1);

                var key = Console.ReadKey(true);

                // 4. Логика управления
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (curY > 0) curY--;
                        curX = Math.Min(curX, lines[curY].Length);
                        break;
                    case ConsoleKey.DownArrow:
                        if (curY < lines.Count - 1) curY++;
                        curX = Math.Min(curX, lines[curY].Length);
                        break;
                    case ConsoleKey.LeftArrow:
                        if (curX > 0) curX--;
                        break;
                    case ConsoleKey.RightArrow:
                        if (curX < lines[curY].Length) curX++;
                        break;
                    case ConsoleKey.Enter:
                        // Разрезаем строку на две
                        string left = lines[curY].Substring(0, curX);
                        string right = lines[curY].Substring(curX);
                        lines[curY] = left;
                        lines.Insert(curY + 1, right);
                        curY++;
                        curX = 0;
                        break;
                    case ConsoleKey.Backspace:
                        if (curX > 0)
                        {
                            lines[curY] = lines[curY].Remove(curX - 1, 1);
                            curX--;
                        }
                        else if (curY > 0)
                        {
                            // Склеиваем с верхней строкой
                            curX = lines[curY - 1].Length;
                            lines[curY - 1] += lines[curY];
                            lines.RemoveAt(curY);
                            curY--;
                        }
                        break;
                    case ConsoleKey.F2:
                        File.WriteAllLines(filename, lines.ToArray());
                        Console.BackgroundColor = ConsoleColor.Green;
                        Console.SetCursorPosition(0, Console.WindowHeight - 1);
                        Console.Write(" Saved successfully! ");
                        Console.ResetColor();
                        Thread.Sleep(500);
                        break;
                    case ConsoleKey.Escape:
                        running = false;
                        break;
                    default:
                        if (key.KeyChar >= 32)
                        {
                            lines[curY] = lines[curY].Insert(curX, key.KeyChar.ToString());
                            curX++;
                        }
                        break;
                }
            }
            Console.Clear();
        }

        public static string ReadCmd()
        {
            List<char> buf = new List<char>();
            int historyIndex = history.Count;
            int lastSuggestionLength = 0;

            // 1. ЗАПОМИНАЕМ, ГДЕ КОНЧАЕТСЯ ПРОМПТ
            int startX = Console.CursorLeft;
            int startY = Console.CursorTop;

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    int currentX = Console.CursorLeft;
                    int currentY = Console.CursorTop;

                    if (key.Key == ConsoleKey.Enter)
                    {
                        ClearFrom(startX, startY, buf.Count + lastSuggestionLength);
                        Console.SetCursorPosition(startX, startY);
                        Console.Write(new string(buf.ToArray())); // Печатаем финальный текст без серого
                        Console.WriteLine();
                        return new string(buf.ToArray());
                    }
                    else if (Sys.KeyboardManager.ControlPressed && key.Key == ConsoleKey.R)
                    {
                        try
                        {
                            fs.CreateFile(@"0:\recovery.trigger");
                            Sys.Power.Reboot();
                        }
                        catch (Exception e){ Console.WriteLine(e.Message); }
                    }
                    else if (key.Key == ConsoleKey.Tab)
                    {
                        string currentInput = new string(buf.ToArray());
                        string suggestion = GetSuggestion(currentInput);
                        if (!string.IsNullOrEmpty(suggestion))
                        {
                            ClearFrom(startX, startY, buf.Count + lastSuggestionLength);
                            buf.Clear();
                            buf.AddRange(suggestion.ToCharArray());
                            Console.SetCursorPosition(startX, startY);
                            Console.Write(suggestion);
                            lastSuggestionLength = 0;
                        }
                        continue;
                    }
                    else if (key.Key == ConsoleKey.Backspace)
                    {
                        if (buf.Count > 0)
                        {
                            // Стираем всё ПРАВЕЕ промпта
                            ClearFrom(startX, startY, buf.Count + lastSuggestionLength);
                            buf.RemoveAt(buf.Count - 1);

                            // Рисуем буфер заново
                            Console.SetCursorPosition(startX, startY);
                            Console.Write(new string(buf.ToArray()));
                            lastSuggestionLength = 0;
                        }
                        // Не делаем continue, чтобы отрисовалась новая подсказка
                    }
                    else if (key.KeyChar >= 32)
                    {
                        buf.Add(key.KeyChar);
                        Console.Write(key.KeyChar);
                    }
                    else if (key.Key == ConsoleKey.UpArrow)
                    {
                        if (history.Count > 0 && historyIndex > 0)
                        {
                            historyIndex--;
                            // 1. Стираем всё, что было введено (от startX до конца текущего буфера + подсказки)
                            ClearFrom(startX, startY, buf.Count + lastSuggestionLength);

                            // 2. Достаем команду из истории
                            string cmd = history[historyIndex];
                            buf.Clear();
                            buf.AddRange(cmd.ToCharArray());

                            // 3. Печатаем её и сбрасываем подсказку
                            Console.SetCursorPosition(startX, startY);
                            Console.Write(cmd);
                            lastSuggestionLength = 0;
                        }
                    }
                    else if (key.Key == ConsoleKey.DownArrow)
                    {
                        if (historyIndex < history.Count - 1)
                        {
                            historyIndex++;
                            ClearFrom(startX, startY, buf.Count + lastSuggestionLength);

                            string cmd = history[historyIndex];
                            buf.Clear();
                            buf.AddRange(cmd.ToCharArray());

                            Console.SetCursorPosition(startX, startY);
                            Console.Write(cmd);
                            lastSuggestionLength = 0;
                        }
                        else if (historyIndex == history.Count - 1)
                        {
                            // Если спустились в самый низ, показываем пустую строку
                            historyIndex = history.Count;
                            ClearFrom(startX, startY, buf.Count + lastSuggestionLength);
                            buf.Clear();
                            lastSuggestionLength = 0;
                        }
                    }

                    // ОТРИСОВКА ПОДСКАЗКИ
                    currentX = Console.CursorLeft;
                    currentY = Console.CursorTop;

                    // Чистим только место под старую подсказку
                    ClearRight(lastSuggestionLength + 1);
                    Console.SetCursorPosition(currentX, currentY);

                    string inputStr = new string(buf.ToArray());
                    string foundSuggestion = GetSuggestion(inputStr);

                    if (!string.IsNullOrEmpty(foundSuggestion) && inputStr.Length > 0 && foundSuggestion != inputStr)
                    {
                        string tail = foundSuggestion.Substring(inputStr.Length);
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(tail);
                        Console.ResetColor();
                        lastSuggestionLength = tail.Length;
                        Console.SetCursorPosition(currentX, currentY);
                    }
                }
            }
        }

        // Новый метод: чистит строку, не трогая промпт
        private static void ClearFrom(int x, int y, int length)
        {
            Console.SetCursorPosition(x, y);
            for (int i = 0; i < length + 1; i++) Console.Write(" ");
            Console.SetCursorPosition(x, y);
        }

        // Вспомогательные методы (без \b, только хардкорные координаты)
        private static void ClearRight(int length)
        {
            if (length <= 0) return;
            int x = Console.CursorLeft;
            int y = Console.CursorTop;
            for (int i = 0; i < length; i++) Console.Write(" ");
            Console.SetCursorPosition(x, y);
        }

        private static void ClearCurrentLine(int length)
        {
            // Откатываем курсор в начало (учитывая длину буфера) и затираем всё
            for (int i = 0; i < length; i++)
            {
                if (Console.CursorLeft > 0)
                {
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    Console.Write(" ");
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                }
            }
        }

        // Поиск совпадения в истории (или можно добавить файлы)
        private static string GetSuggestion(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return history.LastOrDefault(s => s.StartsWith(input, StringComparison.OrdinalIgnoreCase)) ?? "";
        }
    }
}
