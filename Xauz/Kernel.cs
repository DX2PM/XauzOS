using Cosmos.System.FileSystem.VFS;
using System;
using System.IO;
using System.Threading;
using Xauz.XSys;
using Sys = Cosmos.System;

namespace Xauz
{
    public class Kernel : Sys.Kernel
    {
        
        protected override void BeforeRun()
        {
            XShell.Init();
            
            try
            {
                VFSManager.CreateFile(@"0:\test");
                if (!Directory.Exists(@"0:\home\xausr"))
                {
                    Directory.CreateDirectory(@"0:\home\xausr");
                }
                if (!Directory.Exists(@"0:\etc"))
                {
                    Directory.CreateDirectory(@"0:\etc");
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("ATA Error"))
                {
                    string[] menuItems = { "Reboot System Now", "Shutdown", "Format Disk", "Setup Defaults (Settings)" };
                    string[] menuIds = { "reboot", "shutdown", "format", "default" };

                    int selectedIndex = 0;

                    while (true)
                    {
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Xauz Recovery Mode. Ctrl + R to reboot");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("Up / Down / Enter - Navigation");
                        Console.WriteLine();

                        for (int i = 0; i < menuItems.Length; i++)
                        {
                            if (i == selectedIndex)
                            {
                                Console.BackgroundColor = ConsoleColor.White;
                                Console.ForegroundColor = ConsoleColor.Black;
                                Console.WriteLine($"> {menuItems[i]} <");
                                Console.ResetColor();
                            }
                            else
                            {
                                Console.WriteLine($"  {menuItems[i]}  ");
                            }
                        }
                        var keyEvent = Sys.KeyboardManager.ReadKey();

                        // Проверка Ctrl + R для быстрой перезагрузки
                        if (keyEvent.Key == Sys.ConsoleKeyEx.R && Sys.KeyboardManager.ControlPressed)
                        {
                            Sys.Power.Reboot();
                        }

                        switch (keyEvent.Key)
                        {
                            case Sys.ConsoleKeyEx.UpArrow:
                                selectedIndex = (selectedIndex == 0) ? menuItems.Length - 1 : selectedIndex - 1;
                                break;

                            case Sys.ConsoleKeyEx.DownArrow:
                                selectedIndex = (selectedIndex == menuItems.Length - 1) ? 0 : selectedIndex + 1;
                                break;

                            case Sys.ConsoleKeyEx.Enter:
                                // Извлекаем строковый ID текущего элемента
                                string currentId = menuIds[selectedIndex];

                                Console.Clear();
                                // Тот самый switch по именам (case "reboot" и т.д.)
                                switch (currentId)
                                {
                                    case "reboot":
                                        Console.WriteLine("Rebooting device...");
                                        Sys.Power.Reboot();
                                        break;

                                    case "shutdown":
                                        Console.WriteLine("Shutting down device...");
                                        Sys.Power.Shutdown();
                                        break;

                                    case "default":
                                        Console.WriteLine("Restoring default settings...");
                                        XShell.RunCommand(@"del HOME");
                                        XShell.RunCommand(@"del USER");
                                        XShell.RunCommand(@"del AUTORUN");
                                        XShell.RunCommand(@"rm 0:\autorun.xsh");
                                        XShell.RunCommand(@"set HOME 0:\home\xausr");
                                        XShell.RunCommand(@"set USER xausr");
                                        XShell.RunCommand(@"set AUTORUN xsh 0:\autorun.xsh");
                                        VFSManager.CreateFile(@"0:\autorun.xsh");
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
                                        Console.WriteLine("Settings restored to defaults.");
                                        break;

                                    case "format":
                                        Console.WriteLine("Warning: Formating disk will erase all saved data on device, and rewrite FS from scratch. This action can be undone\nFormat Drive? (0) y/n");
                                        if (Console.ReadKey(true).Key == ConsoleKey.Y)
                                        {
                                            Console.WriteLine("Erasing 0:");
                                            for (int i = 0; i < VFSManager.GetDisks()[0].Partitions.Count; i++)
                                            {
                                                VFSManager.GetDisks()[0].DeletePartition(i);
                                                Console.WriteLine($"Deleted partition [0:{i}].");
                                            }
                                            Console.WriteLine("Creating primary partition.");
                                            VFSManager.GetDisks()[0].CreatePartition(VFSManager.GetDisks()[0].Size / 1024 / 1024);
                                            Console.WriteLine($"Created partition. Size: {VFSManager.GetDisks()[0].Size / 1024 / 1024}.");
                                            Thread.Sleep(1000);
                                            Console.WriteLine("Formating partition [1] as FAT32.");
                                            VFSManager.GetDisks()[0].FormatPartition(0, "FAT32", true);
                                            Console.WriteLine("Partition [1] formated. Successfully: 1");
                                            Console.WriteLine("Done.");
                                            Console.WriteLine("Any key to return menu.");
                                            Console.ReadKey(true);
                                        }
                                        break;

                                    default:
                                        Console.WriteLine("Unknown command ID.");
                                        break;
                                }

                                Console.WriteLine("\nPress any key to return to menu...");
                                Sys.KeyboardManager.ReadKey();
                                break;
                        }
                    }
                }
            }
            if (File.Exists(@"0:\recovery.trigger"))
            {
                VFSManager.DeleteFile(@"0:\recovery.trigger");
                string[] menuItems = { "Reboot System Now", "Shutdown", "Format Disk", "Setup Defaults (Settings)" };
                string[] menuIds = { "reboot", "shutdown",  "format", "default" };

                int selectedIndex = 0;

                while (true)
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Xauz Recovery Mode. Ctrl + R to reboot");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("Up / Down / Enter - Navigation");
                    Console.WriteLine();

                    for (int i = 0; i < menuItems.Length; i++)
                    {
                        if (i == selectedIndex)
                        {
                            Console.BackgroundColor = ConsoleColor.White;
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.WriteLine($"> {menuItems[i]} <");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.WriteLine($"  {menuItems[i]}  ");
                        }
                    }
                    var keyEvent = Sys.KeyboardManager.ReadKey();

                    // Проверка Ctrl + R для быстрой перезагрузки
                    if (keyEvent.Key == Sys.ConsoleKeyEx.R && Sys.KeyboardManager.ControlPressed)
                    {
                        Sys.Power.Reboot();
                    }

                    switch (keyEvent.Key)
                    {
                        case Sys.ConsoleKeyEx.UpArrow:
                            selectedIndex = (selectedIndex == 0) ? menuItems.Length - 1 : selectedIndex - 1;
                            break;

                        case Sys.ConsoleKeyEx.DownArrow:
                            selectedIndex = (selectedIndex == menuItems.Length - 1) ? 0 : selectedIndex + 1;
                            break;

                        case Sys.ConsoleKeyEx.Enter:
                            // Извлекаем строковый ID текущего элемента
                            string currentId = menuIds[selectedIndex];

                            Console.Clear();
                            // Тот самый switch по именам (case "reboot" и т.д.)
                            switch (currentId)
                            {
                                case "reboot":
                                    Console.WriteLine("Rebooting device...");
                                    Sys.Power.Reboot();
                                    break;

                                case "shutdown":
                                    Console.WriteLine("Shutting down device...");
                                    Sys.Power.Shutdown();
                                    break;

                                case "default":
                                    Console.WriteLine("Restoring default settings...");
                                    XShell.RunCommand(@"del HOME");
                                    XShell.RunCommand(@"del USER");
                                    XShell.RunCommand(@"del AUTORUN");
                                    XShell.RunCommand(@"rm 0:\autorun.xsh");
                                    XShell.RunCommand(@"set HOME 0:\home\xausr");
                                    XShell.RunCommand(@"set USER xausr");
                                    XShell.RunCommand(@"set AUTORUN xsh 0:\autorun.xsh");
                                    VFSManager.CreateFile(@"0:\autorun.xsh");
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
                                    Console.WriteLine("Settings restored to defaults.");
                                    break;

                                case "format":
                                    Console.WriteLine("Warning: Formating disk will erase all saved data on device, and rewrite FS from scratch. This action can be undone\nFormat Drive? (0) y/n");
                                    if (Console.ReadKey(true).Key == ConsoleKey.Y)
                                    {
                                        Console.WriteLine("Erasing 0:");
                                        for (int i = 0; i < VFSManager.GetDisks()[0].Partitions.Count; i++)
                                        {
                                            VFSManager.GetDisks()[0].DeletePartition(i);
                                            Console.WriteLine($"Deleted partition [0:{i}].");
                                        }
                                        Console.WriteLine("Creating primary partition.");
                                        VFSManager.GetDisks()[0].CreatePartition(VFSManager.GetDisks()[0].Size / 1024 / 1024);
                                        Console.WriteLine($"Created partition. Size: {VFSManager.GetDisks()[0].Size / 1024 / 1024}.");
                                        Thread.Sleep(1000);
                                        Console.WriteLine("Formating partition [1] as FAT32.");
                                        VFSManager.GetDisks()[0].FormatPartition(0, "FAT32", true);
                                        Console.WriteLine("Partition [1] formated. Successfully: 1");
                                        Console.WriteLine("Done.");
                                        Console.WriteLine("Any key to return menu.");
                                        Console.ReadKey(true);
                                    }
                                    break;

                                default:
                                    Console.WriteLine("Unknown command ID.");
                                    break;
                            }

                            Console.WriteLine("\nPress any key to return to menu...");
                            Sys.KeyboardManager.ReadKey();
                            break;
                    }
                }
            }
        }

        protected override void Run()
        {
            int result = XShell.Run();
            if (result == 1)
            {
                Console.WriteLine($"in ..shell: err! {XShell.ex.Message}");
            }
        }
    }
}
