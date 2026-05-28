using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BookDL.Infrastructure
{
    public interface IShellService
    {
        void OpenFolder(string path);
    }
    public class ShellService : IShellService
    {
        public void OpenFolder(string path)
        {
            var psi = new ProcessStartInfo
            {
                UseShellExecute = true,
                Verb = "Open",
                FileName = path
            };
            using var _ = Process.Start(psi);
        }
    }
}
