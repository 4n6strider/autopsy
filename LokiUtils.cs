using System;
using System.Diagnostics;
using System.IO;

namespace autopsy
{
    class LokiUtils
    {
        public static bool UpdateLoki(string updatePath, string logPath)
        {
            var logFile = Path.Combine(logPath, string.Format("lokiupdate-{0}.log", DateTime.Now.ToString("dd-MM-yy")));
            var lokiArgs = string.Format("-l \"{0}\"", logFile);

            var startInfo = new ProcessStartInfo(updatePath);
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = false;
            startInfo.Arguments = lokiArgs;

            try
            {
                int exitCode = 0;

                using (var p = new Process())
                {
                    p.StartInfo = startInfo;
                    p.Start();
                    p.WaitForExit();

                    exitCode = p.ExitCode;
                }

                if (exitCode == 0)
                {
                    return true;
                }
            }

            catch (Exception ex)
            {
                Program.ExitOnError(string.Format("[!] Update failed: {0}", ex.ToString()));
            }

            return false;
        }

        public static bool LokiScan(string binPath, string logPath)
        {
            var logFile = Path.Combine(logPath, string.Format("lokiscan-{0}.log", DateTime.Now.ToString("dd-MM-yy")));
            var lokiArgs = string.Format("--dontwait --intense --allreasons --noindicator --reginfs -l \"{0}\"", logFile);

            var startInfo = new ProcessStartInfo(binPath);
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = false;
            startInfo.Arguments = lokiArgs;

            try
            {
                int exitCode = 0;

                using (var p = new Process())
                {
                    p.StartInfo = startInfo;
                    p.Start();
                    p.WaitForExit();

                    exitCode = p.ExitCode;
                }

                if (exitCode == 0)
                {
                    return true;
                }
            }

            catch (Exception ex)
            {
                Program.ExitOnError(string.Format("[!] Scan failed: {0}", ex.ToString()));
            }

            return false;
        }
    }
}
