using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Valerie.Extensions
{
    public static class HTTPExtension
    {
        public static async Task DownloadAsync(this HttpClient HttpClient, Uri requestUri, string filename)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                using (Stream contentStream = await (await HttpClient.SendAsync(request).ConfigureAwait(false)).Content.ReadAsStreamAsync().ConfigureAwait(false), stream =
                    new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 3145728, true))
                {
                    await contentStream.CopyToAsync(stream).ConfigureAwait(false);
                }
                request.Dispose();
            }
        }

        public static void Headers(this HttpClient HttpClient)
        {
            HttpClient.DefaultRequestHeaders.Clear();
            HttpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/14.0.835.202 Safari/535.1");
            HttpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        }
    }
}
