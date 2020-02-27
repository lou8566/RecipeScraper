using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;

namespace Scaper.Core.Services
{
    public class PageServices
    {
        public async Task<HtmlDocument> GetHtmlDocumentAsync(string url)
        {
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            return htmlDocument;
        }
    }
}
