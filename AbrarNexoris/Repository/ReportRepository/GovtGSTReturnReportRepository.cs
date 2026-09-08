using ModelClass;
using ModelClass.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Repository.ReportRepository
{
    public class GovtGSTReturnReportRepository : BaseRepostitory
    {
        private readonly InputGSTReportRepository _inputRepo;
        private readonly OutputGSTReportRepository _outputRepo;

        public GovtGSTReturnReportRepository()
        {
            _inputRepo = new InputGSTReportRepository();
            _outputRepo = new OutputGSTReportRepository();
        }

        public List<GSTR1WorkingRow> GetGSTR1Working(GovtGSTReturnFilter filter)
        {
            OutputGSTReportFilter oFilter = MapFilter(filter);
            List<SalesGSTRegisterRow> sales = _outputRepo.GetSalesRegister(oFilter);
            List<CreditDebitNoteGSTRow> cdNotes = _outputRepo.GetCreditDebitNotes(oFilter);

            var b2b = sales.Where(x => string.Equals(x.SaleType, "B2B", StringComparison.OrdinalIgnoreCase)).ToList();
            var b2c = sales.Where(x => !string.Equals(x.SaleType, "B2B", StringComparison.OrdinalIgnoreCase)).ToList();

            return new List<GSTR1WorkingRow>
            {
                new GSTR1WorkingRow
                {
                    ReturnSection = "4A, 4B, 6C - B2B Invoices",
                    InvoiceCount = b2b.Select(x => x.InvoiceNo).Distinct().Count(),
                    TaxableValue = b2b.Sum(x => x.TaxableValue),
                    CGSTAmt = b2b.Sum(x => x.CGSTAmt),
                    SGSTAmt = b2b.Sum(x => x.SGSTAmt),
                    IGSTAmt = b2b.Sum(x => x.IGSTAmt),
                    CessAmt = b2b.Sum(x => x.CessAmt),
                    TotalTaxAmount = b2b.Sum(x => x.TotalOutputGST),
                    FilingStatus = "Prepared"
                },
                new GSTR1WorkingRow
                {
                    ReturnSection = "7 - B2C (Others) Supplies",
                    InvoiceCount = b2c.Select(x => x.InvoiceNo).Distinct().Count(),
                    TaxableValue = b2c.Sum(x => x.TaxableValue),
                    CGSTAmt = b2c.Sum(x => x.CGSTAmt),
                    SGSTAmt = b2c.Sum(x => x.SGSTAmt),
                    IGSTAmt = b2c.Sum(x => x.IGSTAmt),
                    CessAmt = b2c.Sum(x => x.CessAmt),
                    TotalTaxAmount = b2c.Sum(x => x.TotalOutputGST),
                    FilingStatus = "Prepared"
                },
                new GSTR1WorkingRow
                {
                    ReturnSection = "9B - Credit / Debit Notes (Registered)",
                    InvoiceCount = cdNotes.Count,
                    TaxableValue = cdNotes.Sum(x => x.TaxableAdjustment),
                    CGSTAmt = cdNotes.Sum(x => x.CGSTAdjustment),
                    SGSTAmt = cdNotes.Sum(x => x.SGSTAdjustment),
                    IGSTAmt = cdNotes.Sum(x => x.IGSTAdjustment),
                    CessAmt = 0m,
                    TotalTaxAmount = cdNotes.Sum(x => x.CGSTAdjustment + x.SGSTAdjustment + x.IGSTAdjustment),
                    FilingStatus = "Prepared"
                },
                new GSTR1WorkingRow
                {
                    ReturnSection = "12 - HSN-wise Summary of Outward Supplies",
                    InvoiceCount = sales.Select(x => x.HSNCode).Distinct().Count(),
                    TaxableValue = sales.Sum(x => x.TaxableValue),
                    CGSTAmt = sales.Sum(x => x.CGSTAmt),
                    SGSTAmt = sales.Sum(x => x.SGSTAmt),
                    IGSTAmt = sales.Sum(x => x.IGSTAmt),
                    CessAmt = sales.Sum(x => x.CessAmt),
                    TotalTaxAmount = sales.Sum(x => x.TotalOutputGST),
                    FilingStatus = "Prepared"
                }
            };
        }

        public List<GSTR3BWorkingRow> GetGSTR3BWorking(GovtGSTReturnFilter filter)
        {
            OutputGSTReportFilter oFilter = MapFilter(filter);
            InputGSTReportFilter iFilter = MapInputFilter(filter);

            List<SalesGSTRegisterRow> sales = _outputRepo.GetSalesRegister(oFilter);
            List<PurchaseGSTRegisterRow> purchases = _inputRepo.GetPurchaseRegister(iFilter);

            decimal salesTaxable = sales.Sum(x => x.TaxableValue);
            decimal salesCGST = sales.Sum(x => x.CGSTAmt);
            decimal salesSGST = sales.Sum(x => x.SGSTAmt);
            decimal salesIGST = sales.Sum(x => x.IGSTAmt);

            decimal purCGST = purchases.Sum(x => x.CGSTAmt);
            decimal purSGST = purchases.Sum(x => x.SGSTAmt);
            decimal purIGST = purchases.Sum(x => x.IGSTAmt);

            return new List<GSTR3BWorkingRow>
            {
                new GSTR3BWorkingRow
                {
                    SectionCode = "3.1 (a)",
                    Description = "Outward Taxable Supplies (other than zero rated, nil rated & exempt)",
                    TaxableValue = salesTaxable,
                    IGSTAmt = salesIGST,
                    CGSTAmt = salesCGST,
                    SGSTAmt = salesSGST,
                    CessAmt = 0m
                },
                new GSTR3BWorkingRow
                {
                    SectionCode = "4 (A) (5)",
                    Description = "All Other Eligible Input Tax Credit (ITC Available)",
                    TaxableValue = purchases.Sum(x => x.TaxableValue),
                    IGSTAmt = purIGST,
                    CGSTAmt = purCGST,
                    SGSTAmt = purSGST,
                    CessAmt = purchases.Sum(x => x.CessAmt)
                },
                new GSTR3BWorkingRow
                {
                    SectionCode = "4 (B)",
                    Description = "ITC Reversed (Ineligible / Blocked Credit)",
                    TaxableValue = 0m, IGSTAmt = 0m, CGSTAmt = 0m, SGSTAmt = 0m, CessAmt = 0m
                },
                new GSTR3BWorkingRow
                {
                    SectionCode = "6.1",
                    Description = "Net Tax Payable after ITC Utilization",
                    TaxableValue = 0m,
                    IGSTAmt = Math.Max(0m, salesIGST - purIGST),
                    CGSTAmt = Math.Max(0m, salesCGST - purCGST),
                    SGSTAmt = Math.Max(0m, salesSGST - purSGST),
                    CessAmt = 0m
                }
            };
        }

        public List<GSTLiabilityUtilizationRow> GetLiabilityUtilization(GovtGSTReturnFilter filter)
        {
            OutputGSTReportFilter oFilter = MapFilter(filter);
            InputGSTReportFilter iFilter = MapInputFilter(filter);

            List<SalesGSTRegisterRow> sales = _outputRepo.GetSalesRegister(oFilter);
            List<PurchaseGSTRegisterRow> purchases = _inputRepo.GetPurchaseRegister(iFilter);

            decimal outCGST = sales.Sum(x => x.CGSTAmt);
            decimal outSGST = sales.Sum(x => x.SGSTAmt);
            decimal outIGST = sales.Sum(x => x.IGSTAmt);

            decimal inCGST = purchases.Sum(x => x.CGSTAmt);
            decimal inSGST = purchases.Sum(x => x.SGSTAmt);
            decimal inIGST = purchases.Sum(x => x.IGSTAmt);

            decimal setCGST = Math.Min(outCGST, inCGST);
            decimal setSGST = Math.Min(outSGST, inSGST);
            decimal setIGST = Math.Min(outIGST, inIGST);

            return new List<GSTLiabilityUtilizationRow>
            {
                new GSTLiabilityUtilizationRow
                {
                    Particulars = "Output Tax Liability",
                    IGSTAmt = outIGST, CGSTAmt = outCGST, SGSTAmt = outSGST, CessAmt = 0m, TotalAmount = outIGST + outCGST + outSGST
                },
                new GSTLiabilityUtilizationRow
                {
                    Particulars = "Eligible ITC Available",
                    IGSTAmt = inIGST, CGSTAmt = inCGST, SGSTAmt = inSGST, CessAmt = 0m, TotalAmount = inIGST + inCGST + inSGST
                },
                new GSTLiabilityUtilizationRow
                {
                    Particulars = "ITC Utilized (Set-off)",
                    IGSTAmt = setIGST, CGSTAmt = setCGST, SGSTAmt = setSGST, CessAmt = 0m, TotalAmount = setIGST + setCGST + setSGST
                },
                new GSTLiabilityUtilizationRow
                {
                    Particulars = "Net Tax Liability Payable in Cash",
                    IGSTAmt = outIGST - setIGST, CGSTAmt = outCGST - setCGST, SGSTAmt = outSGST - setSGST, CessAmt = 0m, TotalAmount = (outIGST - setIGST) + (outCGST - setCGST) + (outSGST - setSGST)
                }
            };
        }

        public MonthlyGSTExecutiveSummary GetMonthlyExecutiveSummary(GovtGSTReturnFilter filter)
        {
            OutputGSTReportFilter oFilter = MapFilter(filter);
            InputGSTReportFilter iFilter = MapInputFilter(filter);

            List<SalesGSTRegisterRow> sales = _outputRepo.GetSalesRegister(oFilter);
            List<PurchaseGSTRegisterRow> purchases = _inputRepo.GetPurchaseRegister(iFilter);

            var b2b = sales.Where(x => string.Equals(x.SaleType, "B2B", StringComparison.OrdinalIgnoreCase)).ToList();
            var b2c = sales.Where(x => !string.Equals(x.SaleType, "B2B", StringComparison.OrdinalIgnoreCase)).ToList();

            decimal outCGST = sales.Sum(x => x.CGSTAmt);
            decimal outSGST = sales.Sum(x => x.SGSTAmt);
            decimal outIGST = sales.Sum(x => x.IGSTAmt);
            decimal totalOutput = outCGST + outSGST + outIGST;

            decimal inCGST = purchases.Sum(x => x.CGSTAmt);
            decimal inSGST = purchases.Sum(x => x.SGSTAmt);
            decimal inIGST = purchases.Sum(x => x.IGSTAmt);
            decimal totalInput = inCGST + inSGST + inIGST;

            decimal netPayable = Math.Max(0m, totalOutput - totalInput);
            decimal itcUsed = Math.Min(totalOutput, totalInput);

            return new MonthlyGSTExecutiveSummary
            {
                CompanyName = "Nexoris Supermarket",
                GSTIN = "32AAAAA1234A1Z5",
                TaxPeriod = $"{filter.FromDate:MMMM yyyy}",
                PurchaseTaxableValue = purchases.Sum(x => x.TaxableValue),
                InputCGST = inCGST,
                InputSGST = inSGST,
                InputIGST = inIGST,
                EligibleITC = totalInput,
                IneligibleITC = 0m,
                GSTR2BMatched = totalInput,
                GSTR2BDifference = 0m,
                SalesTaxableValue = sales.Sum(x => x.TaxableValue),
                B2BSalesValue = b2b.Sum(x => x.TaxableValue),
                B2CSalesValue = b2c.Sum(x => x.TaxableValue),
                OutputCGST = outCGST,
                OutputSGST = outSGST,
                OutputIGST = outIGST,
                TotalOutputGST = totalOutput,
                GSTR1Status = "Prepared",
                GSTR2BStatus = "Reconciled",
                GSTR3BStatus = "Prepared",
                NetGSTLiability = totalOutput,
                ITCUtilized = itcUsed,
                GSTCashPayment = netPayable,
                ReconciliationStatus = "MATCHED"
            };
        }

        private static OutputGSTReportFilter MapFilter(GovtGSTReturnFilter filter)
        {
            return new OutputGSTReportFilter
            {
                CompanyId = filter.CompanyId,
                BranchId = filter.BranchId,
                FinYearId = filter.FinYearId,
                FromDate = filter.FromDate,
                ToDate = filter.ToDate
            };
        }

        private static InputGSTReportFilter MapInputFilter(GovtGSTReturnFilter filter)
        {
            return new InputGSTReportFilter
            {
                CompanyId = filter.CompanyId,
                BranchId = filter.BranchId,
                FinYearId = filter.FinYearId,
                FromDate = filter.FromDate,
                ToDate = filter.ToDate
            };
        }
    }
}
