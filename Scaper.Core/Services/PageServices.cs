using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Scaper.Core.Services
{
    public class PageServices
    {
        public async Task<HtmlDocument> GetHtmlDocumentAsync(string url)
        {

            try
            {
                var httpClient = new HttpClient();
                Thread.Sleep(5000);
                var html = await httpClient.GetStringAsync(url);

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                return htmlDocument;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<HtmlNode> GetHtml(HtmlDocument htmlDocument, string targetElement, string attributeName, string attributeValue)
        {
            return htmlDocument.DocumentNode.Descendants(targetElement)
                .Where(node => node.GetAttributeValue(attributeName, "")
                    .Equals(attributeValue)).ToList();
        }
    }
}
