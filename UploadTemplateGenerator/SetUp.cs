using DocumentFormat.OpenXml.InkML;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UploadTemplateGenerator
{
    public static class SetUp
    {
        public static async Task<IBrowserContext> InitialSetUp(string envURL)
        {
            var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            var contextOptions = new BrowserNewContextOptions
            {
                BaseURL = envURL,
                ViewportSize = ViewportSize.NoViewport
            };

            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false,
                Args = new[] { "--start-maximized" }
            });

            return await browser.NewContextAsync(contextOptions);
        }
    }
}
