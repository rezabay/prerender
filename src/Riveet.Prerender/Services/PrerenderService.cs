using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Riveet.Prerender.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace Riveet.Prerender.Services
{
    public class PrerenderService
    {
        private static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(60);

        private readonly TimeSpan _pageCacheTimeout;
        private readonly HttpClient _httpClient;
        private readonly WebPageService _webPageService;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private ChromeDriver chromeDriver;

        public PrerenderService(HttpClient httpClient,
                                WebPageService bookService,
                                IConfiguration configuration,
                                ILoggerFactory loggerFactory)
        {
            _httpClient = httpClient;
            _webPageService = bookService;
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger<PrerenderService>();

            _pageCacheTimeout = TimeSpan.Parse(configuration["Settings:Interval"]);
        }

        public async Task Start()
        {
            try
            {
                StartSession();

                var targets = GetTargets();
                foreach (var target in targets)
                {
                    await Render(target);
                }
                _logger.LogInformation("Prerender completed");

                CloseSession();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Prerendering failed");
            }
        }

        private List<PrerenderTarget> GetTargets()
        {
            return new List<PrerenderTarget>
            {
                new PrerenderTarget
                {
                    Url = _configuration["Settings:Host"],
                    Sitemaps = new []
                    {
                        "api/Restaurant/SiteMap"
                    }
                },
                new PrerenderTarget
                {
                    Url = "https://titifood.com/"
                }
            };
        }

        private async Task Render(PrerenderTarget target)
        {
            await Render(target.Url);

            if (target.Sitemaps == null) return;

            foreach (var sitemap in target.Sitemaps)
            {
                var sitemapUrl = Path.Combine(target.Url, sitemap);
                _logger.LogInformation($"Start Rendering {sitemapUrl} sitemap");

                using var response = await _httpClient.GetAsync(sitemapUrl);
                response.EnsureSuccessStatusCode();

                var sitemapXml = await response.Content.ReadAsStringAsync();
                var urls = GetUrlsFromSitemap(sitemapXml).ToList();
                _logger.LogInformation($"Found {urls.Count} in sitemap");

                for (int i = 0; i < urls.Count; i++)
                {
                    var decodedUrl = HttpUtility.UrlDecode(urls[i]);
                    _logger.LogInformation($"Rendering {decodedUrl} [{i}-{urls.Count}]");

                    await Render(decodedUrl);
                }
                _logger.LogInformation($"Completed Rendering {sitemapUrl} sitemap");
            }
        }

        private async Task Render(string url)
        {
            if (await _webPageService.IsPageUpdated(url, _pageCacheTimeout)) 
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                return;
            }

            try
            {
                chromeDriver.Navigate().GoToUrl(url);
                await Task.Delay(TimeSpan.FromSeconds(1));

                await WaitForPageToLoad(chromeDriver);
                LoadIframes(chromeDriver);
                await WaitForPageToLoad(chromeDriver);

                var urlSource = chromeDriver.PageSource;
                await _webPageService.Set(url, urlSource);
            }
            catch (WebDriverException webEx)
            {
                _logger.LogError($"Failed to Render {url}: {webEx.Message}");

                CloseSession();
                StartSession();
            }
        }

        private IEnumerable<string> GetUrlsFromSitemap(string sitemapXml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(sitemapXml);

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                var loc = node.FirstChild;
                if (loc != null)
                {
                    yield return loc.InnerText;
                }
            }
        }

        private ChromeDriver ConnectToChromeDriver()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--no-sandbox");
            chromeOptions.AddArgument("--headless");
            chromeOptions.AddArgument("--disable-gpu");
            chromeOptions.AddArgument("--ignore-certificate-errors");
            chromeOptions.AddArgument("--log-level=3");
            chromeOptions.AddArgument("--disable-dev-shm-usage");
            chromeOptions.AddArgument("--whitelisted-ips");
            chromeOptions.AddArgument("--disable-extensions");

            var driverPath = _configuration["Settings:ChromeDriverPath"];  
            var driverExecutableFileName = _configuration["Settings:ChromeDriverFilename"];
            var service = ChromeDriverService.CreateDefaultService(driverPath, driverExecutableFileName);
            var driver = new ChromeDriver(service, chromeOptions, CommandTimeout);

            driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(20);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(20);

            return driver;
        }

        private async Task WaitForPageToLoad(ChromeDriver chromeDriver)
        {
            object pageLoadStatus;

            do
            {
                pageLoadStatus = chromeDriver.ExecuteScript("return document.readyState");
                await Task.Delay(1000);
            }
            while (!pageLoadStatus.Equals("complete"));
        }

        private void LoadIframes(ChromeDriver chromeDriver)
        {
            var iframes = chromeDriver.FindElements(By.TagName("iframe"));
            if (iframes.Count > 0)
            {
                foreach (IWebElement iframe in iframes)
                {
                    chromeDriver.SwitchTo().Frame(iframe);
                }
            }
        }

        private void StartSession()
        {
            var chromeDriverProcesses = Process.GetProcessesByName("chromedriver");
            foreach (var chromeDriverProcess in chromeDriverProcesses)
            {
                try
                {
                    chromeDriverProcess.Kill();
                }
                catch
                {
                    // Ignore exception
                }
            }

            chromeDriver = ConnectToChromeDriver();
        }

        private void CloseSession()
        {
            try
            {
                chromeDriver.Close();
                chromeDriver.Quit();
                chromeDriver.Dispose();
                chromeDriver = null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Unable to Quit WebDriver: {ex.Message}");
            }
        }
    }
}
