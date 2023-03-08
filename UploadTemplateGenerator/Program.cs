using Microsoft.Extensions.Logging;
using Serilog;
using UploadTemplateGenerator.Models;
using UploadTemplateGenerator.Pages;

namespace UploadTemplateGenerator
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var serilog = new LoggerConfiguration().MinimumLevel.Verbose().CreateLogger();
            var loggerFactory = new LoggerFactory().AddSerilog(serilog);
            var logger = loggerFactory.CreateLogger("Log");
            var config = HelperMethods.CreateConfig();
            var clietnName = config["clientName"];
            var connectionString = config["connectionString"];
            var numberOfLeases = int.Parse(config["amountOfLeases"]);
            var dataGen = new DataGenerator();
            var context = await SetUp.InitialSetUp(config["environmentURL"]);
            var page = await context.NewPageAsync(); 

            var loginPage = new LoginPage(page, logger, config);
            var uploadPage = new LeaseUploadExcelFile(page, logger, config);
            var uploadResultPage = new LeaseUploadResults(page, logger, config);
            var uploadApprovalPage = new LeaseUploadApproval(page, logger, config);

            await page.GotoAsync("/");                         
            await loginPage.Login(config["userName"], config["userPassword"]);

            var filePath = await uploadPage.DownloadExcel(clietnName); //"C:\\Users\\ZarinaAllen\\source\\repos\\UploadTemplateGenerator\\UploadTemplateGenerator\\bin\\Debug\\net6.0\\Data\\Zarina Test GAAP IFRSUploadTemplate.xlsx";
            var uploadData = dataGen.GenerateUploadData(clietnName, connectionString, numberOfLeases);
            uploadPage.WriteDataToExcelTemplate(filePath, uploadData);
            Console.WriteLine($"Upload Template saved at {filePath}");
            await uploadPage.UploadExcelFile(filePath);
            await uploadResultPage.ConfirmUpload();
            var batchID = uploadResultPage.GetBatchId(page.Url).GetAwaiter().GetResult();
            await uploadApprovalPage.NavigateToUploadApproval(batchID);
            await uploadApprovalPage.ApproveUpload();
            await uploadApprovalPage.VerifyApproveUploadSuccessfully();
            await uploadApprovalPage.CloseAlertPopup();

        }
    }
}


