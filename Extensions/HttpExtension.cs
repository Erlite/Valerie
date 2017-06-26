using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rick.Extensions
{
    public static class HttpExtension
    {
        public static async Task DownloadAsync(this HttpClient client, Uri requestUri, string filename)
        {
            using (client = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                {
                    using ( Stream contentStream = await (await client.SendAsync(request)).Content.ReadAsStreamAsync(), stream = 
                        new FileStream (filename, FileMode.Create, FileAccess.Write, FileShare.None, 3145728, true))
                    {
                        await contentStream.CopyToAsync(stream).ConfigureAwait(false);
                    }
                }
            }
        }

        public static void Headers(this HttpClient http)
        {
            http.DefaultRequestHeaders.Clear();
            http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/14.0.835.202 Safari/535.1");
            http.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        }
    }
}
