namespace UploadTemplateGenerator.Pages
{
    using LQPlaywrightAutomation.PageElements;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;
    using Microsoft.Playwright;
    using System.Collections.Generic;
    using System;
    using ClosedXML.Excel;

    public class LeaseUploadApproval : BasePage
    {
        public LeaseUploadApproval(IPage page, ILogger log, IConfiguration config) : base(page, log, config) { }

        private static readonly PageRunAndWaitForResponseOptions PageRunAndWaitForResponseOptions = new PageRunAndWaitForResponseOptions { Timeout = 15000 };
        public static readonly NamedSelector BtnApprove = new NamedSelector("css=[data-test-id='ApproveBulkUpload']", "Approve Button");
        public static readonly NamedSelector BtnAlertClose = new NamedSelector("id=btnAlertClose", "Alert Close Button");
        public static readonly NamedSelector LnkDownLoadTemplate = new NamedSelector("xpath=//*[@id = 'newLease']//a[contains(@href, '/BulkApprove/Download/')]", "Down Load Template Link");
        public static readonly NamedSelector TxaApprovedCompletely = new NamedSelector("id=alert-message", "Approved Completely Text Area");
        public static readonly NamedSelector DdlBatchId = new NamedSelector("id=batchId", "Batch ID Drop Down List");
        public static readonly NamedSelector ChbSelectAll = new NamedSelector("css=[name='selectall']", "Select All Check box");
        public static readonly NamedSelector BtnReviewForApproval = new NamedSelector("id=btnDownload", "Review For Approval Button");
        public static readonly NamedSelector ChbBulkUploadMessage = new NamedSelector("id=chkBulkUploadMessage", "Bulk Upload Message Check box");
        public static readonly NamedSelector BtnExecuteApproval = new NamedSelector("id=btnExecuteApproval", "Execute Approval Button");
        public static readonly NamedSelector TxtSearchBulkRenewal = new NamedSelector($"css=div#tblBulkUploadStatusfilter > label >input", "Search Bulk Renewal Text Box");
        public static NamedSelector LblBatchID(string leaseName) => new NamedSelector($"xpath=//td[contains(.,'{leaseName}')]/preceding::td[1]", "Batch ID Label");
        public static readonly NamedSelector BtnGo = new NamedSelector("css=[type='button'][value='Go']", "Go Button");
        public static readonly NamedSelector DdlClientSelect = new NamedSelector("id=SelectedClientId", "Client Select Drop Down List");


        public async Task NavigateToUploadApproval(string batchID)
        {
            var uri = new Uri(page.Url);
            log.LogInformation($"Navigating to: {uri.Scheme}://{uri.Host}/BulkApprove/Index/{batchID}");
            await page.GotoAsync($"{uri.Scheme}://{uri.Host}/BulkApprove/Index/{batchID}");
        }
        public async Task ApproveUpload()
        {
            log.LogInformation("Approve Lease Upload");
            // Set the default timeout of the page to a long time to make sure uploading a large number of leases can be completed
            page.SetDefaultTimeout(300000);
            await page.ClickAsync(LnkDownLoadTemplate.Selector);
            await page.ClickAsync(BtnApprove.Selector);
            // Reset the default timeout of the page
            page.SetDefaultTimeout(60000);
        }

        //public async Task<string> DownloadApprovedExcel(string fileName)
        //{
        //    log.LogInformation("Download the approve excel template");
        //    var waitForDownloadTask = page.WaitForDownloadAsync();
        //    await page.ClickAsync(LnkDownLoadTemplate);
        //    var download = await waitForDownloadTask;
        //    await download.SaveAsAsync($"{fileName}.xlsx");
        //    var path = Directory.GetCurrentDirectory();
        //    log.LogInformation($"The file is located at {path}\\{fileName}");
        //    return $"{path}\\{fileName}.xlsx";
        //}

        public async Task ClickApproveUpload()
        {
            await page.ClickAsync(BtnApprove.Selector);
        }

        //public Dictionary<string, string> GetLeaseDataFromTemplate(string testDataFilePath, int startRow, int step, string workSheetName)
        //{
        //    Dictionary<string, string> leaseData = new Dictionary<string, string>();
        //    List<string> attributesList = new List<string>();
        //    using (var workbook = new XLWorkbook(testDataFilePath))
        //    {
        //        IXLWorksheet ws = workbook.Worksheet(workSheetName);
        //        foreach (IXLCell cell in ws.Row(startRow).Cells(true))
        //        {
        //            IXLCells cells = ws.Row(startRow).Cells();
        //            if (!cell.WorksheetColumn().IsHidden && cell.CellBelow(step).Value.ToString().Trim() != "")
        //            {
        //                if (cell.Value.ToString().Trim() == "Serial Number")
        //                {
        //                    leaseData[cell.Value.ToString()] = cell.CellBelow(step).GetDouble().ToString();
        //                }
        //                leaseData[cell.Value.ToString()] = cell.CellBelow(step).Value.ToString();
        //            }
        //        }
        //    }
        //    return leaseData;
        //}

        public async Task VerifyApproveUploadSuccessfully()
        {
            await page.IsVisibleAsync(TxaApprovedCompletely.Selector);
            //await assert.ElementPresent(TxaApprovedCompletely);
        }

        public async Task CloseAlertPopup()
        {
            await page.ClickAsync(BtnAlertClose.Selector);
        }

        //public async Task ApproveBulkRenewalUpload(string batchID)
        //{
        //    log.LogInformation("Approve Bulk Renewal Upload");
        //    // Set the default timeout of the page to a long time to make sure uploading a large number of leases can be completed
        //    page.SetDefaultTimeout(300000);
        //    await page.SelectByLabelAsync(DdlBatchId, batchID);
        //    await page.ClickAsync(ChbSelectAll);
        //    await page.RunAndWaitForResponseAsync(async () => { await page.ClickAsync(BtnReviewForApproval); }, "**/BulkUpload/LeaseDownload", PageRunAndWaitForResponseOptions);
        //    await page.ClickAsync(ChbBulkUploadMessage);
        //    await page.ClickAsync(BtnExecuteApproval);
        //    // Reset the default timeout of the page
        //    page.SetDefaultTimeout(60000);
        //}

        //public async Task<string> GetRenewalBatchId(string leaseName)
        //{
        //    log.LogInformation("Find Renewal Batch ID");
        //    await page.WaitForSelectorAsync(TxtSearchBulkRenewal, 30);
        //    await page.FillAsync(TxtSearchBulkRenewal, leaseName);
        //    var batchId = (await page.TextContentAsync(LblBatchID(leaseName))).Trim();
        //    await page.ClearAndFillAsync(TxtSearchBulkRenewal, "");
        //    await page.Keyboard.PressAsync("Enter");
        //    return batchId;
        //}

        //public async Task SelectClient(string clientName)
        //{
        //    log.LogInformation($"Select Client {clientName}");
        //    await page.SelectByLabelAsync(DdlClientSelect, clientName);
        //    await page.ClickAsync(BtnGo);
        //}
    }
}
