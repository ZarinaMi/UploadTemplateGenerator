namespace UploadTemplateGenerator.Pages
{
    using LQPlaywrightAutomation.PageElements;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;
    using System.Threading;
    using Microsoft.Playwright;
    using System.Web;

    public class LeaseUploadResults : BasePage
    {
        public LeaseUploadResults(IPage page, ILogger log, IConfiguration config) : base(page, log, config) { }

        public static readonly NamedSelector BtnContinueUpload = new NamedSelector("id=continue-upload", "Continue Upload Button");
        public static readonly NamedSelector TxtApprovers = new NamedSelector("css=.select2-searchfield", "Approvers Text Box");
        public static readonly NamedSelector BtnSendNotification = new NamedSelector("css=input[value='Send Notification']", "Send Notification Button");
        public static readonly NamedSelector LnkDashboardPage = new NamedSelector("css=a[data-test-id='DashboardNav'].selected", "Dashboard Page Link");
        public static readonly NamedSelector BtnCloseAlert = new NamedSelector("id=btnAlertClose", "Close Alert Button");
        public static readonly NamedSelector LblLeaseUploadResults = new NamedSelector("xpath=//h1[text()='Lease Upload Results']", "Lease Upload Result Label");
        public static NamedSelector LstError(string errorMessage) => new NamedSelector($"xpath=//table[@class ='contentGrid']//tr[contains(@class,'contentList')]/td[contains(.,\"{errorMessage}\")]", "Error List");

        public async Task ConfirmUpload()
        {
            page.SetDefaultTimeout(900000);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 60 });
            await page.ClickAsync(BtnContinueUpload.Selector);
            page.SetDefaultTimeout(60000);
        }

        public async Task SendMailToApprovalUser(string approveUserEmail)
        {
            log.LogInformation("Send mail to approval user");
            // Try to fix issue 502 Bad Gateway
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 60 });
            await page.FillAsync(TxtApprovers.Selector, approveUserEmail);
            await page.Keyboard.PressAsync("Enter");
            await page.ClickAsync(BtnSendNotification.Selector);
        }

        public async Task VerifyDashBoardPagePresent()
        {
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 60 });
            //await assert.ElementPresent(LnkDashboardPage, 30);
            await page.IsVisibleAsync(LnkDashboardPage.Selector);
        }

        public async Task CloseAlertPopup()
        {
            log.LogInformation("Close Quick Note Pop-up");
            await page.WaitForSelectorAsync(BtnCloseAlert.Selector);
            await page.ClickAsync(BtnCloseAlert.Selector);
        }

        public async Task<int> GetNumberOfErrorMessages(string errorMessage)
        {
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 60 });
            await page.WaitForSelectorAsync(LblLeaseUploadResults.Selector);
            var numberOfErrorMessages = (await page.QuerySelectorAllAsync(LstError(errorMessage).Selector)).Count;
            return numberOfErrorMessages;
        }

        public Task<string> GetBatchId(string url)
        {
            var uri = new Uri(page.Url);
            var query = uri.Query;
            var parms = HttpUtility.ParseQueryString(query);
            return Task.FromResult<string>(parms["batchId"]);
        }
    }
}
