using System;
using System.Diagnostics;
using System.IO;

namespace autopsy
{
    class ComaeUtils
    {
        public static string DoDump(string comaePath, string outPath)
        {
            var comaeBin = Path.Combine(comaePath, "DumpIt.exe");

            var dumpFile = Path.Combine(outPath, string.Format("{0}-{1}.raw", Environment.MachineName, DateTime.Now.ToString("dd-MM-yy")));

            var dumpArgs = string.Format("/N /Q /T RAW /O \"{0}\"", dumpFile);

            if (VMUtils.IsVirtualMachine())
            {
                dumpFile = Path.Combine(outPath, string.Format("{0}-{1}.dmp", Environment.MachineName, DateTime.Now.ToString("dd-MM-yy")));
                dumpArgs = string.Format("/N /Q /T DMP /O \"{0}\"", dumpFile);
            }

            var startInfo = new ProcessStartInfo(comaeBin);
            startInfo.WorkingDirectory = comaePath;
            startInfo.UseShellExecute = true;
            startInfo.Verb = "runas";
            startInfo.CreateNoWindow = false;
            startInfo.Arguments = dumpArgs;

            try
            {
                int exitCode = 0;

                using (var p = new Process())
                {
                    p.StartInfo = startInfo;
                    p.Start();
                    p.WaitForExit();
                }

                return dumpFile;
            }

            catch (Exception ex)
            {
                Program.ExitOnError(string.Format("[!] Update failed: {0}", ex.ToString()));
            }

            return null;
        }
    }
}
