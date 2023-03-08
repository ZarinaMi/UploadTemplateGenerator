namespace UploadTemplateGenerator.Pages
{
    using System.Threading.Tasks;
    using LQPlaywrightAutomation.Actions;
    using LQPlaywrightAutomation.Assertions;
    using LQPlaywrightAutomation.PageElements;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Playwright;

    public class LoginPage : BasePage
    {
        public LoginPage(IPage page, ILogger log, IConfiguration config) : base(page, log, config) { }

        public static readonly NamedSelector TxtEmailAddress = new NamedSelector("id=EmailAddress", "Email Address Text Box");
        public static readonly NamedSelector TxtPassword = new NamedSelector("id=Password", "Password Text Box");
        public static readonly NamedSelector BtnSignIn = new NamedSelector("id=btnSignIn", "Sign In Button");
        public static readonly NamedSelector MnuDashboard = new NamedSelector("css=[data-test-id='DashboardNav']", "Dash Board Menu");
        public static readonly NamedSelector AssetTypePopupLoadingDashBoard = new NamedSelector("xpath=//div[@class='assettype-content']/div[@class='assetchartloading strippedprogress']", "Loading Popup on Dashboard");

        public async Task Login(string email, string password)
        {
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            log.Log(LogLevel.Information, $"Logging in as {email}");
            await page.FillAsync(TxtEmailAddress.Selector, email);
            await page.FillAsync(TxtPassword.Selector, password);
            await page.ClickAsync(BtnSignIn.Selector);
            await WaitForDashboardLoaded();
        }

        public async Task WaitForDashboardLoaded()
        {
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            //if (await page.IsElementPresentAsync(AssetTypePopupLoadingDashBoard, 5))
            //{
                var element = page.Locator(AssetTypePopupLoadingDashBoard.Selector);
                await element.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 120000 });
            //}
        }
    }
}
