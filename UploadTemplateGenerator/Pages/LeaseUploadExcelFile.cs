namespace UploadTemplateGenerator.Pages
{
    using System;
    using LQPlaywrightAutomation.PageElements;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System.Threading.Tasks;
    using ClosedXML.Excel;
    using System.Linq;
    using Microsoft.Playwright;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.IO;
    using UploadTemplateGenerator.Models;


    public class LeaseUploadExcelFile : BasePage
    {
        public LeaseUploadExcelFile(IPage page, ILogger log, IConfiguration config) : base(page, log, config) { }

        public static readonly NamedSelector BtnUploadExcelFile = new NamedSelector("css=[data-test-id='UploadExcelTab']", "Upload Excel File Button");
        public static readonly NamedSelector LnkChooseFile = new NamedSelector("id=postedFile", "Choose File Link");
        public static readonly NamedSelector BtnUpload = new NamedSelector("css=[data-test-id='UploadSubmit']", "Upload Button");
        public static readonly NamedSelector LnkDownLoadTemplateOnUploadExcel = new NamedSelector("xpath=//div[@id='tab2']//a[text()='Download Excel Template']", "Download Excel Template On Upload Excel File Link");
               
        public static readonly NamedSelector LnkMenuLeases = new NamedSelector("css=[data-test-id='LeasesNav']", "Menu Leases Link");
        public static readonly NamedSelector LnkMenuLeasesAddNewLease = new NamedSelector("css=[data-test-id='AddNewLeaseNav']", "Menu Leases - Add New Link");
        public static readonly NamedSelector DdlClientSelect = new NamedSelector("xpath=//select[@id='ClientId']", "Client Select Dropdown List");

        public async Task<string> DownloadExcel(string clientName)
        {
            await page.ClickAsync(LnkMenuLeases.Selector);
            await page.ClickAsync(LnkMenuLeasesAddNewLease.Selector);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await page.SelectOptionAsync(DdlClientSelect.Selector, new SelectOptionValue
            {
                Label = clientName
            });
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 60 });
            await page.ClickAsync(BtnUploadExcelFile.Selector);
            var waitForDownloadTask = page.WaitForDownloadAsync();
            await page.ClickAsync(LnkDownLoadTemplateOnUploadExcel.Selector);
            var download = await waitForDownloadTask;
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", $"{clientName}UploadTemplate.xlsx");
            await download.SaveAsAsync(path);              
            return path;
        }

        public void WriteDataToExcelTemplate(string testDataFilePath, List<UploadData> uploadData)
        {
            log.LogInformation("Start writing Data To An Excel File");

            var headerRow = 4;
            int startRow = headerRow + 1;
            using (var workbook = new XLWorkbook(testDataFilePath))
            {
                IXLWorksheet ws = workbook.Worksheet("Upload Template");
                InsertSalvageValueIfMissingInTemlate(ws);
                workbook.Save();
                for (int i = 0; i < uploadData.Count; i++)
                {
                    var uploadDataRow = uploadData[i];
                    WriteDataToExecelForARow(ws, headerRow, i + startRow, uploadDataRow);
                }
                workbook.Save();
                log.LogInformation("Complete writing Data To An Excel File");
            }
        }


        public async Task UploadExcelFile(string filePath)
        {
            log.LogInformation("Create leases by uploading excel template");
            page.SetDefaultTimeout(240000);            
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 60 });
            await page.ClickAsync(BtnUploadExcelFile.Selector);
            await page.SetInputFilesAsync(LnkChooseFile.Selector, filePath);
            await page.ClickAsync(BtnUpload.Selector);
            
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 60 });
            log.LogInformation("Complete uploading excel file");
            page.SetDefaultTimeout(60000);
        }
      
        internal IXLCell GetCellByValue(IXLRow row, string value)
        {
            foreach (IXLCell cell in row.Cells(true))
            {
                if (!cell.WorksheetColumn().IsHidden && cell.Value.ToString().Equals(value))
                {
                    return cell;
                }
            }
            return null;
        }

        internal IXLCell GetCellByPartialValue(IXLRow row, string value)
        {
            foreach (IXLCell cell in row.Cells(true))
            {
                string cellValue = RemoveSpecialCharacters(cell.Value.ToString());
                if (!cell.WorksheetColumn().IsHidden && cellValue.Contains(value))
                {
                    return cell;
                }
            }
            return null;
        }

        internal IXLCell GetCellByValueAfterRemovingSpecialCharacters(IXLRow row, string value)
        {
            foreach (IXLCell cell in row.Cells(true))
            {
                string cellValue = RemoveSpecialCharacters(cell.Value.ToString());
                if (cellValue.Equals(value))
                {
                    return cell;
                }
            }
            return null;
        }                            
    
        internal string RemoveSpecialCharacters(string value)
        {
            string newValue = value;
            if (newValue.Contains("("))
            {
                newValue = newValue.Remove(newValue.LastIndexOf("("));
            }
            newValue = Regex.Replace(newValue, "[^a-zA-Z0-9]", string.Empty);
            return newValue;
        }

        internal void WriteDataToExecelForARow(IXLWorksheet worksheet, int headerRow, int row, UploadData uploadData)
        {
            var propertyList = uploadData.GetType().GetProperties();
            for (int i = 0; i < propertyList.Count(); i++)
            {
                var propertyName = propertyList[i].Name;
                var propertyValue = propertyList[i].GetValue(uploadData);
                //if (propertyValue != null)
                //{
                //    switch (propertyName)
                //    {
                //        case "OtherPaymentExpenditure":
                //            var otherExpenditure = uploadData.OtherPaymentExpenditure;
                //            foreach (var kvp in otherExpenditure)
                //            {
                //                propertyName = kvp.Key;
                //                propertyValue = kvp.Value;
                //            }
                //            break;
                //        case "OtherPaymentEliminationExpenditure":
                //            var otherEliminationExpenditure = uploadData.OtherPaymentEliminationExpenditure;
                //            foreach (var kvp in otherEliminationExpenditure)
                //            {
                //                propertyName = kvp.Key;
                //                propertyValue = kvp.Value;
                //            }
                //            break;
                //        default:
                //            break;
                //    }
                //}
                if ((propertyName == "AllocationBeneficiary1" || propertyName == "AllocationPercentage1") && propertyValue != null)
                {
                    var allocationHeaderCell = GetCellByPartialValue(worksheet.Row(headerRow), propertyName);
                    var headerColumn = allocationHeaderCell.Address.ColumnNumber;
                    worksheet.Cell(row, headerColumn).SetValue(propertyValue.ToString());
                }
                else
                {
                    var headerCell = GetCellByValueAfterRemovingSpecialCharacters(worksheet.Row(headerRow), propertyName);
                    if (headerCell != null && propertyValue != null)
                    {
                        var headerColumn = headerCell.Address.ColumnNumber;
                        worksheet.Cell(row, headerColumn).SetValue(propertyValue.ToString());
                    }
                }
            }
        }
   
        public Dictionary<string, string> GetAllocationDataFromJEExcelFile(string updateFilePath, List<string> allocations, string leaseDescription)
        {
            log.LogInformation("Get allocation data in journal entry excel file");
            var allJEData = File.ReadAllLines(updateFilePath).ToList();
            var allocationPercentInJE = new Dictionary<string, string>();
            foreach (var value in allocations)
            {
                foreach (var row in allJEData)
                {
                    if (row.Contains($"(For {value}) , {leaseDescription}") && row.Contains("%"))
                    {
                        var percent = row.Split("Allocation:").Last().Split("%").First().Trim();
                        allocationPercentInJE[value] = int.Parse(percent).ToString("N1");
                    }
                }
            }
            log.LogInformation("Get allocation data in journal entry excel file successfully");
            return allocationPercentInJE;
        }
  
        //workaround for bug https://leasequery.atlassian.net/browse/COR-36940 
        private void InsertSalvageValueIfMissingInTemlate(IXLWorksheet worksheet)
        {
            var cellToUpdate = GetCellByValue(worksheet.Row(4), "Salvage Value");
            if(cellToUpdate == null)
            {
                var lasColumnUsed = worksheet.LastColumnUsed();
                var columnNumber = lasColumnUsed.ColumnNumber();
                worksheet.Column(columnNumber).InsertColumnsAfter(1);
                worksheet.Cell(4, columnNumber + 1).SetValue("Salvage Value");
            }            
        }
    }
}
