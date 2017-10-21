using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Security.Principal;

namespace autopsy
{
    class Program
    {
        static void Main(string[] args)
        {
            Banner.PrintBanner();

            VMUtils.IsVirtualMachine();

            var isElevated = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

            if (!isElevated)
            {
                ExitOnError("[!] You must run this tool with administrative privileges.");
            }

            var offlineRun = string.Equals(ConfigurationManager.AppSettings["offline"].ToLower(), "yes");

            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log");
            var resPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "res");

            if (!Directory.Exists(logPath))
            {
                Console.WriteLine("[*] Making log path...");
                Directory.CreateDirectory(logPath);
            }
            
            if (!Directory.Exists(resPath))
            {
                if (offlineRun)
                {
                    ExitOnError("[!] Resource path not present. Please obtain a full release of autopsy - including signature base.");
                }

                Console.WriteLine("[*] Making resource path...");
                Directory.CreateDirectory(resPath);
            }

            // Fetch operation requirements.
            Console.WriteLine("Would you like to search for indicators of compromise? (yes/no)");
            var runLoki = Console.ReadLine();

            Console.WriteLine("Would you like to perform a memory dump? (yes/no)");
            var runComae = Console.ReadLine();

            if (runLoki.ToLower().StartsWith("y"))
            {
                // Loki download+operation.
                var lokiZip = Path.Combine(resPath, "loki.zip");
                var lokiPath = Path.Combine(resPath, "loki");
                var lokiBin = Path.Combine(lokiPath, "loki.exe");
                var lokiUpdater = Path.Combine(lokiPath, "loki-upgrader.exe");

                if (!Directory.Exists(lokiPath))
                {
                    Console.WriteLine("[*] Retrieving latest Loki release from GitHub...");
                    var lokiUrl = GitUtils.GetLokiDownloadUrl();

                    if (!string.IsNullOrEmpty(lokiUrl))
                    {
                        FetchLoki(resPath, lokiPath, lokiUrl, lokiZip);
                    }

                    else
                    {
                        ExitOnError("[!] Failed to retrieve Loki URL.");
                    }
                }

                else
                {
                    Console.WriteLine("[*] Loki path alreday exists. Continuing...");

                    if (!offlineRun)
                    {
                        var updated = LokiUtils.UpdateLoki(lokiUpdater, logPath);

                        if (!updated)
                        {
                            ExitOnError("[!] Update failed.");
                        }
                    }
                }

                var scanned = LokiUtils.LokiScan(lokiBin, logPath);

                if (scanned)
                {
                    LokiReport(logPath);
                }

                else
                {
                    ExitOnError("[!] Scan failed.");
                }
            }

            if (runComae.ToLower().StartsWith("y"))
            {
                // Comae download+operation.
                var comaeZip = Path.Combine(resPath, "comae.zip");
                var comaePath = Path.Combine(resPath, "comae");
                var comaeBin = Path.Combine(comaePath, "DumpIt.exe");
                
                if (!Directory.Exists(comaePath))
                {
                    FetchComae(comaePath, comaeZip);
                }

                else
                {
                    Console.WriteLine("[*] Comae Memory Toolkit path already exists. Continuing...");
                }

                ComaeReport(comaeBin);
            }

            Console.WriteLine("[*] Operations complete!");
            Console.WriteLine("[*] Press <Enter> to exit.");
            Console.ReadLine();
        }

        public static void FetchComae(string comaePath, string comaeZip)
        {
            Console.WriteLine("[*] Retrieving Comae Memory Toolkit...");
            var comaeUrl = "https://www.comae.io/downloads/Comae_Memory_Toolkit-3.0.109.20161007.zip";

            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(comaeUrl, comaeZip);
                }
            }

            catch (Exception ex)
            {
                ExitOnError(string.Format("[!] Download failed: {0}", ex.ToString()));
            }

            var extractedComae = ZipUtils.ExtractZip(comaePath, comaeZip);

            if (extractedComae)
            {
                File.Delete(comaeZip);
                Console.WriteLine(string.Format("[*] Decompression successful! Comae Memory Toolkit extracted to: {0}", comaePath));
            }

            else
            {
                ExitOnError("[!] Decompression failed.");
            }
        }

        public static void ComaeReport(string comaeBin)
        {
            var dumpFile = ComaeUtils.DoDump(comaeBin, Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

            if (!string.IsNullOrEmpty(dumpFile))
            {
                Console.WriteLine("[*] Reporting complete! Continue with the following steps:");
                Console.WriteLine(string.Format("[-] Find the DMP file '{0}' that has been saved onto your desktop.", dumpFile));

                var memorySize = (new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory >> 20) / 1024;

                Console.WriteLine(string.Format("[-] Copy the file onto a USB stick of at least {0}GB in size.", memorySize));
                Console.WriteLine("[-] Deliver the USB stick to either the security or infrastructure team.");
            }

            else
            {
                ExitOnError("[!] Dump failed.");
            }
        }

        public static void FetchLoki(string resPath, string lokiPath, string lokiUrl, string lokiZip)
        {
            using (var client = new WebClient())
            {
                try
                {
                    client.DownloadFile(lokiUrl, lokiZip);
                }

                catch (Exception ex)
                {
                    ExitOnError(string.Format("[!] Download failed: {0}", ex.ToString()));
                }

                var extractedLoki = ZipUtils.ExtractZip(resPath, lokiZip);

                if (extractedLoki)
                {
                    File.Delete(lokiZip);
                    Console.WriteLine(string.Format("[*] Decompression successful! Loki extracted to: {0}", lokiPath));
                }

                else
                {
                    ExitOnError("[!] Decompression failed.");
                }
            }
        }

        public static void LokiReport(string logPath)
        {
            Console.WriteLine("[*] Scan completed! Assembling log archive...");

            var reportName = ZipUtils.CompressFolderContents(logPath, Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "loki-report");

            if (!string.IsNullOrEmpty(reportName))
            {
                Console.WriteLine("[*] Reporting complete! Continue with the following steps:");
                Console.WriteLine(string.Format("[-] Find the ZIP file '{0}' that has been saved onto your desktop.", reportName));
                Console.WriteLine("[-] Upload the file to https://send.firefox.com to generate a one-time link.");
                Console.WriteLine("[-] Send the one-time link to your security team contact either via email or Google Talk. Do not visit the link yourself or it will be destroyed.");
                Console.WriteLine("[-] If you do not have internet access, transfer the ZIP file to a USB stick and deliver it to the security or infrastructure team in person.");
            }

            else
            {
                ExitOnError("[!] Reporting failed.");
            }
        }

        public static void ExitOnError(string errorMsg)
        {
            Console.WriteLine(errorMsg);
            Console.WriteLine("Press <Enter> to exit.");
            Console.ReadLine();
            Environment.Exit(1);
        }
    }
}
