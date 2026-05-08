using Cosmos.Core.Memory;
using Cosmos.Debug.Kernel;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using IL2CPU.API.Attribs;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using Xauz.XSys;
using Sys = Cosmos.System;

namespace Xauz
{
    public class Kernel : Sys.Kernel
    {
        Canvas c;
        PCScreenFont f = PCScreenFont.Default;
        CosmosVFS fs = new CosmosVFS();
        bool fsexists = false;
        protected override void BeforeRun()
        {
            try
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
                    BModes.Recovery(ex.Message);
                }
                if (BModes.cmode == 0)
                {
                }
                else if (BModes.cmode == 1)
                {
                    XShell.Welcome();
                    Cosmos.HAL.PCSpeaker.Beep(440, 100); // Ля
                    Cosmos.HAL.PCSpeaker.Beep(523, 100); // До
                    Cosmos.HAL.PCSpeaker.Beep(659, 150); // Ми
                    XShell.RunCommand("xsh 0:\\autorun.xsh");
                }

                if (File.Exists(@"0:\recovery.trigger"))
                {
                    VFSManager.DeleteFile(@"0:\recovery.trigger");
                    BModes.Recovery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"in ..before run: err! {ex.Message}");
                Console.WriteLine("Kernel stopped by error in preload process. P: Jump to panic. Any other key: recovery mode.");
                if (Console.ReadKey(true).Key == ConsoleKey.P)
                {
                    XShell.ex = ex;
                    XShell.RunCommand("trigger");
                }
                else
                {
                    BModes.Recovery(ex.Message);
                }
            }
        }

        protected override void Run()
        {
            if (BModes.cmode == 0)
            {
                bool initializated = false;
                int nocollect = 0;
                while (true)
                {
                    if (!initializated)
                    {
                        string[] scrcfg = { };
                        string sw = ""; string sh = "";
                        Console.WriteLine("Is FE?");
                        if (File.Exists(@"0:\etc\scrcfg.cfg"))
                        {
                            Console.WriteLine("Screen config found, loading...");
                            try
                            {
                                Console.WriteLine("Reading screen config...");
                                scrcfg = File.ReadAllLines(@"0:\etc\scrcfg.cfg");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error reading screen config: {ex.Message}");
                                Console.WriteLine("Using default resolution 800x600");
                                sw = "800"; sh = "600";
                                Console.ReadKey(true);
                            }
                            Console.WriteLine("Parsing screen config...");
                            sw = scrcfg[0].Trim().Replace("width=", "");
                            sh = scrcfg[1].Trim().Replace("height=", "");
                        }
                        else
                        {
                            Console.WriteLine("No screen config found, creating...");
                            fs.CreateDirectory(@"0:\etc");
                            string[] scrcfgdata = { };
                            while (true)
                            {
                                Console.Write($"Set Screen Width: ");
                                sw = Console.ReadLine();
                                Console.Write("Set Screen Height: ");
                                sh = Console.ReadLine();
                                scrcfgdata = new string[] { $"width={sw}", $"height={sh}" };
                                try
                                {
                                    Mode md = new Mode(int.Parse(sw), int.Parse(sh), ColorDepth.ColorDepth32);
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error setting screen resolution: {ex.Message}");
                                    Console.WriteLine("Set a valid screen res");
                                    continue;
                                }
                            }
                            File.WriteAllLines(@"0:\etc\scrcfg.cfg", scrcfgdata);
                            Sys.Power.Reboot();
                        }
                        c = FullScreenCanvas.GetFullScreenCanvas(new Mode(int.Parse(sw), int.Parse(sh), ColorDepth.ColorDepth32));
                        c.Clear(Color.Black);
                        c.DrawString("XauzOS 1.3", f, new Pen(Color.White), (c.Mode.Columns / 2) - 80, (c.Mode.Rows / 2) - 8);
                        c.Display();
                        Sys.MouseManager.ScreenWidth = (uint)c.Mode.Columns; Sys.MouseManager.ScreenHeight = (uint)c.Mode.Rows;
                        initializated = true;
                    }
                    else
                    {
                        c.Clear(Color.Orange);
                        c.DrawSquare(new Pen(Color.White), 10, 10, 100);
                        c.DrawString("Shutdown", f, new Pen(Color.White), 20, 50);
                        c.DrawSquare(new Pen(Color.White), 10, 120, 100);
                        c.DrawString("Reboot", f, new Pen(Color.White), 20, 160);
                        c.DrawSquare(new Pen(Color.White), 10, 230, 100);
                        c.DrawString("Files", f, new Pen(Color.White), 20, 270);
                        c.DrawFilledRectangle(new Pen(Color.White), (int)Sys.MouseManager.X, (int)Sys.MouseManager.Y, 10, 10);
                        if ((int)Sys.MouseManager.X > 10 && (int)Sys.MouseManager.X < 110 && (int)Sys.MouseManager.Y > 10 && (int)Sys.MouseManager.Y < 110 && Sys.MouseManager.MouseState == Sys.MouseState.Left)
                        {
                            c.Disable();
                            Sys.Power.Shutdown();
                        }
                        if ((int)Sys.MouseManager.X > 10 && (int)Sys.MouseManager.X < 110 && (int)Sys.MouseManager.Y > 120 && (int)Sys.MouseManager.Y < 220 && Sys.MouseManager.MouseState == Sys.MouseState.Left)
                        {
                            c.Disable();
                            Sys.Power.Reboot();
                        }
                        if ((int)Sys.MouseManager.X > 10 && (int)Sys.MouseManager.X < 110 && (int)Sys.MouseManager.Y > 230 && (int)Sys.MouseManager.Y < 330 && Sys.MouseManager.MouseState == Sys.MouseState.Left)
                        {
                            c.Clear(Color.Black);
                            c.DrawString("Esc for exit", f, new Pen(Color.White), (c.Mode.Columns / 2) - (12 * 8), 10);
                            c.DrawString("Files in root:", f, new Pen(Color.White), 10, 10);
                            int i = 30;
                            foreach (var file in VFSManager.GetDirectoryListing(@"0:\"))
                            {
                                c.DrawString(file.mName, f, new Pen(Color.White), 10, 10 * i);
                                i += 2;
                            }
                            c.Display();
                            while (true)
                            {
                                if (Sys.KeyboardManager.ReadKey().Key == Sys.ConsoleKeyEx.Escape)
                                {
                                    break;
                                }
                            }
                        }
                        nocollect++;
                        c.Display();
                        Heap.Collect();
                    }
                }
            }
            else
            {
                int result = XShell.Run();
                if (result == 1)
                {
                    Console.WriteLine($"in ..shell: err! {XShell.ex.Message}");
                }
            }
        }
    }
}
