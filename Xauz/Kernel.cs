using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using Xauz.System;
using Sys = Cosmos.System;

namespace Xauz
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            XShell.Init();
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
