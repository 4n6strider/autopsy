using System;
using System.Linq;

namespace autopsy
{
    class GitUtils
    {
        public static string GetLokiDownloadUrl()
        {
            try
            {
                var client = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("Octokit"));
                var apiResult = client.Repository.Release.GetAll("Neo23x0", "Loki").Result;

                if (apiResult.Count > 0)
                {
                    var releaseObj = apiResult.First();

                    if (releaseObj.Assets.Count > 0)
                    {
                        return releaseObj.Assets[0].BrowserDownloadUrl;
                    }
                }
            }

            catch(Exception ex)
            {
                Program.ExitOnError(string.Format("[!] Failed to query GitHub: {0}", ex.ToString()));
            }

            return null;
        }
    }
}
