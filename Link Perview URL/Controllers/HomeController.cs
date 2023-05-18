using HtmlAgilityPack;
using Link_Perview_URL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;

namespace Link_Perview_URL.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;
        private HtmlDocument _htmlDocument;
        public string Url { get; set; }

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Privacy(string url)
        {
            ViewBag.URL = url??"";
            if (url == null) {
                ViewBag.LinkPreview = new LinkPreview();

                return View();
            }

            Url = url;
            var htmlContent = await _httpClient.GetStringAsync(url);
            _htmlDocument = new HtmlDocument();
            _htmlDocument.LoadHtml(htmlContent);
            // Use HtmlAgilityPack to parse the HTML and extract the relevant information
            // For example, you can extract the page title, description, and thumbnail URL

            var linkPreview = new LinkPreview
            {
                Title = GetWebsiteTitle(),
                Description = GetWebsiteDescription(),
                ThumbnailUrl = GetWebsiteImages(),
                
            };
            
            ViewBag.LinkPreview = linkPreview;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



        public string GetWebsiteTitle()
        {
            var titleNode = _htmlDocument.DocumentNode.SelectSingleNode("//title");
            return titleNode?.InnerText.Trim();
        }

        public string GetWebsiteDescription()
        {
            var descriptionNode = _htmlDocument.DocumentNode.SelectSingleNode("//meta[@name='description']");
            return descriptionNode?.GetAttributeValue("content", string.Empty)?.Trim();
        }

        public string GetWebsiteImages()
        {
            var ogImageNode = _htmlDocument.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
            if (ogImageNode != null)
            {
                return ogImageNode.GetAttributeValue("content", string.Empty)?.Trim();
            }

            var imageNode = _htmlDocument.DocumentNode.SelectSingleNode("//img");
            if (imageNode != null)
            {
                var imageUrl = imageNode.GetAttributeValue("src", string.Empty)?.Trim();
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    if (!imageUrl.StartsWith("http") && !imageUrl.StartsWith("//"))
                    {
                        var baseUrl = new Uri(Url).GetLeftPart(UriPartial.Authority);
                        imageUrl = baseUrl.TrimEnd('/') + '/' + imageUrl.TrimStart('/');
                    }
                    return imageUrl;
                }
            }

            return string.Empty;
        }
    }
}
