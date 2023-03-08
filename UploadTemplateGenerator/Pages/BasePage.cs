namespace UploadTemplateGenerator.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using LQPlaywrightAutomation.Actions;
    using LQPlaywrightAutomation.Assertions;
    using LQPlaywrightAutomation.PageElements;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Playwright;

    public class BasePage
    {
        public IPage page;
        public ILogger log;
        public IConfiguration configuration;
        //public CommonAssertions assert;

        /// <summary>Initializes a new instance of the <see cref="BasePage"/> class.</summary>
        /// <param name="page">The page action object.</param>
        /// <param name="log">The message logger.</param>
        /// <param name="config">The configuration setting.</param>
        /// <param name="assert">The standard automation framework assertions.</param>
        public BasePage(IPage page, ILogger log, IConfiguration config)
        {
            this.page = page;
            this.log = log;
            configuration = config;
        }

        public static readonly NamedSelector PopupLoader = new NamedSelector("css=.loaderContents", "Loader Popup");
        public static readonly NamedSelector LoaderPopupPresent = new NamedSelector("css=[data-test-id='modalDisplayed']", "Loader Popup Present");

        public async Task WaitForLoaderPopupGone()
        {
            log.Log(LogLevel.Information, "Wait for the Loader Popup gone");
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            var element = page.Locator(PopupLoader.Selector);
            await element.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 600000 });
        }

        public async Task WaitForLoaderPopupGoneTesting()
        {
            log.Log(LogLevel.Information, "Wait for the Loader Popup gone");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 90000 });
            var element = page.Locator(PopupLoader.Selector);
            await element.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 60000 });
        }

        //public async Task<int> FindElementIndexByProperty(NamedSelector selector, string propertyName, string propertyValue)
        //{
        //    await page.WaitForSelectorAsync(selector, 60000);
        //    IReadOnlyList<IElementHandle> selectorList = await page.QuerySelectorAllAsync(selector);
        //    int index = 0;
        //    for (int i = 1; i < selectorList.Count + 1; i++)
        //    {
        //        if (selectorList[i - 1].GetPropertyAsync(propertyName).Result.ToString().Trim() == propertyValue)
        //        {
        //            index = i;
        //            break;
        //        }
        //    }
        //    return index;
        //}

        /// <param name="waitTime">Seconds to wait</param>
        public async Task<bool> LoaderPopupNotDisplayed(double waitTime = 60)
        {
            var loader = page.Locator(LoaderPopupPresent.Selector);
            var timeUsed = 0.00;
            var maxWait = TimeSpan.FromSeconds(waitTime).TotalMilliseconds;
            var loaderNotPresent = false;
            while (!loaderNotPresent && timeUsed <= maxWait)
            {
                try
                {
                    await loader.IsVisibleAsync();
                    loaderNotPresent = false;
                }
                catch (Exception)
                {
                    loaderNotPresent = true;
                }

                Thread.Sleep(250);
                timeUsed += 250;
            }

            return loaderNotPresent;
        }
    }
}
