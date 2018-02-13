using System;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace autopsy
{
    class ZipUtils
    {
        public static string CompressFolderContents(string inPath, string outPath, string description)
        {
            var zipName = string.Format("{0}_{1}.zip", description, DateTime.Now.ToString("dd-MM-yy"));
            var zipPath = Path.Combine(outPath, zipName);

            try
            {
                using (var fs = File.Create(zipPath))
                {
                    using (var zs = new ZipOutputStream(fs))
                    {
                        zs.SetLevel(5);

                        var files = Directory.GetFiles(inPath);
                        int folderOffset = inPath.Length + (inPath.EndsWith("\\") ? 0 : 1);

                        foreach (var file in files)
                        {
                            var fi = new FileInfo(file);

                            var entryName = file.Substring(folderOffset);
                            entryName = ZipEntry.CleanName(entryName);

                            var newEntry = new ZipEntry(entryName);
                            newEntry.DateTime = fi.LastWriteTime;
                            newEntry.Size = fi.Length;

                            zs.PutNextEntry(newEntry);

                            var buffer = new byte[4096];

                            using (var sr = File.OpenRead(file))
                            {
                                StreamUtils.Copy(sr, zs, buffer);
                            }

                            zs.CloseEntry();
                        }
                    }
                }

                return zipName;
            }

            catch (Exception ex)
            {
                Program.ExitOnError(string.Format("[!] Compression failed: {0}", ex.ToString()));
            }

            return null;
        }

        public static bool ExtractZip(string outPath, string fileName)
        {
            ZipFile zf = null;

            try
            {
                using (var fs = File.OpenRead(fileName))
                {
                    zf = new ZipFile(fs);

                    foreach (ZipEntry ze in zf)
                    {
                        if (!ze.IsFile)
                        {
                            continue;
                        }

                        var entryName = ze.Name;

                        var buffer = new byte[4096];

                        using (var zs = zf.GetInputStream(ze))
                        {
                            var extractPath = Path.Combine(outPath, entryName);
                            var pathName = Path.GetDirectoryName(extractPath);

                            if (pathName.Length > 0)
                            {
                                Directory.CreateDirectory(pathName);
                            }

                            using (var sw = File.Create(extractPath))
                            {
                                StreamUtils.Copy(zs, sw, buffer);
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Program.ExitOnError(string.Format("[!] Decompression failed: {0}", ex.ToString()));
            }

            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true;
                    zf.Close();
                }
            }

            return true;
        }
    }
}
