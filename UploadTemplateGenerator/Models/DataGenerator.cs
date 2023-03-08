using LQUIAutomation;
using Bogus;

namespace UploadTemplateGenerator.Models
{
    public class DataGenerator
    {
        private readonly string[] LeaseType = new string[] { "Operating", "Capital" };
        private readonly string[] LeaseRole = new string[] { "Tenant", "Landlord" };
        private readonly string[] Classification = new string[] { "Land", "Building", "Equipment", "Vehicle", "Other" };
        private readonly string[] Currencies = new string[] { "USD - US Dollar", "AUD - Australian Dollar", "GBP - British Pound", "EUR - Euro", "JPY - Japanese Yen", "CHF - Swiss Franc" };
        private readonly string[] YesNo = new string[] { "Yes", "No" };
        private readonly string[] PaymentFrequencies = new string[] { "Monthly","Quarterly","Annually","EveryOtherMonth","TwiceAYear","OneTime","Weekly","BiWeekly","Daily"};
        Faker faker = new Faker("en_US");


        public List<UploadData> GenerateUploadData(string clientName, string connectionString, int numberOfLeases)
        {
            var db = new PostgresSQLRequests(connectionString);
            var clientId = db.GetClientIdFromName(clientName);
            var isGASB = db.IsGasbEnabled(clientId);
            var isFASB = db.IsFasbEnabled(clientId);
            var allocations = db.GetAllocationItems(clientId);
            var transitionYear = db.GetTransitionYear(clientId);
            var transitionMonth = db.GetTransitionMonth(clientId);
            var transitionDate = new DateTime(transitionYear, transitionMonth, 1);

            var uploadData = new List<UploadData>();
            for(var i = 0; i < numberOfLeases; i++)
            {
                var leaseData = GenerateLeaseData(clientName, connectionString, i, clientId, isGASB, isFASB, allocations, transitionDate);
                uploadData.Add(leaseData);
            }
            return uploadData;
        }

        public UploadData GenerateLeaseData(string clientName, string connectionString, int i, string clientId, bool isGASB, bool isFASB, List<string> allocations, DateTime transitionDate)
        {            
            //Faker faker = new Faker("en_US");
            var leaseData = new UploadData();

            leaseData.RowID = i;
            leaseData.LeaseType = faker.PickRandom(LeaseType);
            var isCapital = leaseData.LeaseType == "Capital";
            //leaseData.LeaseType = "Capital";
            if (isGASB) { leaseData.OurRole = faker.PickRandom(LeaseRole); }
            else leaseData.OurRole = "Tenant";
            leaseData.LocalTransactionalCurrency = faker.PickRandom(Currencies);
            leaseData.FunctionalCurrency = faker.PickRandom(Currencies);

            //Classification
            leaseData.Classification = faker.PickRandom(Classification);
            if(leaseData.Classification == "Equipment" || leaseData.Classification == "Vehicle" || leaseData.Classification == "Other")
            {
                leaseData.Make = faker.Vehicle.Manufacturer();
                leaseData.Model = faker.Vehicle.Model();
                leaseData.SerialNumber = DateTimeOffset.UtcNow.ToUniversalTime().UtcTicks.ToString();
            }
            leaseData.Address1 = faker.Address.StreetAddress();
            leaseData.Country = "United States";
            leaseData.State = faker.Address.State();
            leaseData.City = faker.Address.City();
            leaseData.ZipCode = faker.Address.ZipCode();

            //Immaterial lease 
            var isImmaterial = false;
            if(faker.Random.Bool() && !isCapital && leaseData.OurRole == "Tenant")
            {
                leaseData.ImmaterialLease = "Yes";
                leaseData.ImmaterialComments = faker.Commerce.ProductName();
                isImmaterial = true;
            }
            
            leaseData.VendorCompanyName = "TestForever";

            //Dates
            var possessionDate = new DateTime();
            if(isGASB && !isFASB && !isCapital)
            {
                possessionDate = faker.Date.Between(transitionDate.AddYears(-2), transitionDate.AddDays(-1));
            }
            else
            {
                possessionDate = faker.Date.Between(transitionDate.AddYears(-2), transitionDate.AddYears(+5));           
            }
            leaseData.PossessionDate = possessionDate.ToShortDateString();
            var endDate = faker.Date.Between(possessionDate.AddMonths(6), possessionDate.AddYears(10));

            //Short term lease condition (only operating lease can be marked as short term)
            var isShortTerm = possessionDate.AddYears(1) > endDate;
            if (isShortTerm && !isCapital) leaseData.ShorttermLease = "Yes";
            else if (isShortTerm && isCapital) endDate = endDate.AddMonths(6);

            leaseData.CurrentEndDate = endDate.ToShortDateString();
            var leaseTerm = 12 * (possessionDate.Year - endDate.Year) + possessionDate.Month - endDate.Month;
            var leaseTermInMoths = Math.Abs(leaseTerm);


            //Useful life, fair value, cap vs op test conditions
            if (isCapital)
            {
                var usefulLifeLimit = (leaseTermInMoths * 75 / 100) - 1;
                if (possessionDate >= transitionDate)
                {
                    leaseData.SpecializedUse = faker.PickRandom(YesNo);
                }
                
                leaseData.TitleTransferatLeaseEnd = faker.PickRandom(YesNo);
                if (faker.Random.Bool())
                {
                    leaseData.PurchaseOption = "Yes";
                    leaseData.PurchaseAmount = faker.Random.Number(1000, 10000).ToString();
                    leaseData.BargainPurchaseOption = faker.PickRandom(YesNo);
                    leaseData.CertaintoPurchase = faker.PickRandom(YesNo);
                }
                if(leaseData.TitleTransferatLeaseEnd == "Yes" || leaseData.BargainPurchaseOption == "Yes" || leaseData.CertaintoPurchase == "Yes")
                {
                    leaseData.SalvageValue = faker.Random.Number(500, 5000).ToString();
                    leaseData.UsefulLife = faker.Random.Number(1, usefulLifeLimit).ToString();
                }
                else if (faker.Random.Bool())
                {
                    leaseData.UsefulLife = faker.Random.Number(0, usefulLifeLimit).ToString();
                }
                if (faker.Random.Bool())
                {
                    leaseData.FairValue = faker.Random.Number(100, 500).ToString();
                }
            }
            else
            {
                var usefulLifeLimit = (leaseTermInMoths * 75 / 100) + 1;
                if (faker.Random.Bool())
                {
                    leaseData.UsefulLife = faker.Random.Number(usefulLifeLimit, 300).ToString();
                }
                if (faker.Random.Bool())
                {
                    leaseData.FairValue = faker.Random.Number(1000000, 5000000).ToString();
                }
            }

            //Base rent paymetns
            leaseData.BorrowingRate = faker.Random.Number(0,10).ToString();

            var br1EndDate = faker.Date.Between(possessionDate.AddMonths(2), endDate.AddMonths(-2));
            var br2StartDate = br1EndDate.AddDays(1);
            var br2EndDate = endDate;

            leaseData.BRAmount1 = faker.Random.Number(100, 3000).ToString();
            leaseData.BRFirstPaymentDate1 = possessionDate.ToShortDateString();
            leaseData.BRFrequency1 = faker.PickRandom(PaymentFrequencies);
            if (leaseData.BRFrequency1 == "OneTime") br1EndDate = possessionDate;
            leaseData.BRLastPaymentDate1 = br1EndDate.ToShortDateString();
            if (possessionDate.AddYears(1) < br1EndDate) leaseData.BREscalation1 = faker.Random.Number(50, 500).ToString();

            leaseData.BRAmount2 = faker.Random.Number(100, 3000).ToString();
            leaseData.BRFirstPaymentDate2 = br2StartDate.ToShortDateString();
            leaseData.BRFrequency2 = faker.PickRandom(PaymentFrequencies);
            if (leaseData.BRFrequency2 == "OneTime") br2EndDate = br2StartDate;
            leaseData.BRLastPaymentDate2 = br2EndDate.ToShortDateString();
            if (br2StartDate.AddYears(1) < br2EndDate) leaseData.BREscalation2 = faker.Random.Number(50, 500).ToString();

            //OtherAmortizations
            if (faker.Random.Bool() && !isShortTerm && !isImmaterial) 
            {
                GenerateOtherAmort(leaseData, isGASB);
            }


            if (allocations.Count > 0)
            {        
                leaseData.AllocationBeneficiary1 = faker.PickRandom(allocations);
                leaseData.AllocationPercentage1 = "100";
            }

            return leaseData;
        }

        private UploadData GenerateOtherAmort(UploadData leaseData, bool isGASB)
        {
            if (faker.Random.Bool())
            {
                leaseData.Incentive = "Yes";
                leaseData.IncentiveAmount = faker.Random.Number(500, 5000).ToString();
                leaseData.IncentiveDescription = faker.Finance.AccountName();
            }
            if (faker.Random.Bool())
            {
                leaseData.InitialDirectCost = "Yes";
                leaseData.InitialDirectCostAmount = faker.Random.Number(500, 5000).ToString();
                leaseData.InitialDirectCostDescription = faker.Finance.AccountName();
            }
            if (faker.Random.Bool() && !isGASB)
            {
                leaseData.FavorableLeaseIntangibleAsset = "Yes";
                leaseData.FavorableLeaseIntangibleAssetAmount = faker.Random.Number(500, 5000).ToString();
                leaseData.FavorableLeaseIntangibleAssetDescription = faker.Finance.AccountName();
            }
            if (faker.Random.Bool() && !isGASB)
            {
                leaseData.UnfavorableLeaseIntangibleLiability = "Yes";
                leaseData.UnfavorableLeaseIntangibleLiabilityAmount = faker.Random.Number(500, 5000).ToString();
                leaseData.UnfavorableLeaseIntangibleLiabilityDescription = faker.Finance.AccountName();
            }
            return leaseData;
        }
    }
}
