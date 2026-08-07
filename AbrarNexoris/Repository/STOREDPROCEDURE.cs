using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public static class STOREDPROCEDURE
    {
        public static string POS_Branch = "POS_Branch";
        public static string POS_Initialsetup = "_POS_Initialsetup";
        public static string POS_Ledger = "POS_Ledger";
        public static string _4Login = "_4Login";
        public static string POS_Login = "POS_Login";
        public static string POS_ItemMasterDDl = "POS_ItemMasterDDl";
        public static string POS_CustomerDDl = "POS_CustomerDDl";
        public static string POS_dropdown = "POS_dropdown";
        public static string POS_PayMode = "POS_PayMode";
        public static string POS_GeneralPaymodeSetup = "_POS_PayMode_Setup";
        public static string POS_ItemDetalisDDL = "POS_ItemDetalisDDL";
        public static string _POS_Sales_Win = "_POS_Sales_Win";
        public static string _POS_SDetails_Win = "_POS_SDetails_Win";
        public static string POS_Group = "_POS_Group";
        public static string POS_Category = "_POS_Category";
        public static string POS_ItemType = "POS_ItemType";
        public static string POS_ItemMaster = "_POS_ItemMaster";
        public static string POS_ItemMasterStatusRules = "_POS_ItemMasterStatusRules";
        public static string POS_ItemMasterPriceSettings = "_POS_ItemMaster_PriceSettings";
        public static string _POS_SalesInvoice_PriceSettings = "_POS_SalesInvoice_PriceSettings";
        public static string _SalesReturn_PriceSettings = "_SalesReturn_PriceSettings";
        public static string POS_Brand = "POS_Brand";
        public static string POS_UnitMaster = "_POS_Unit";
        public static string POS_State = "_POS_State";
        public static string _POS_Country = "_POS_Country";
        public static string POS_Country = "_POS_Country";
        public static string POS_Currency = "_POS_Currency";
        public static string POS_User = "_POS_User";
        public static string POS_Customer = "POS_Customer";
        public static string POS_CustomerReceiptInfo = "POS_CustomerReceiptInfo";
        public static string POS_SReturnDetails = "_POS_SReturn_Details";
        public static string POS_SalesReturn = "_POS_SalesReturn";
        public static string _POS_PurchaseReturn = "_POS_PurchaseReturn";
        public static string POS_PReturnDetails = "_PurchaseReturn_PReturnDetails_ItemBatch";
        public static string POS_Customer_ContactDetails = "POS_Customer_ContactDetails";
        public static string POS_Vendor = "POS_Vendor";
        public static string POS_VendorPyamentInfo = "POS_VendorPyamentInfo";
        public static string POS_Vouchers = "POS_Vouchers";
        public static string POS_Vendor_ContactDetails = "POS_Vendor_ContactDetails";
        public static string _4GetAccountLedgerDDL = "_4GetAccountLedgerDDL";
        public static string _4GetLedgerIdByLedgerNameAndGroupId = "_4GetLedgerIdByLedgerNameAndGroupId";
        public static string POS = "_POS";
        public static string _POS_GetBill = "_POS_GetBill";
        public static string POS_Purchase = "POS_Purchase";
        public static string POS_Purchase_Details = "POS_Purchase_Details";
        public static string POS_PurchaseOrder = "_PurchaseOrder";
        public static string POS_PurchaseOrder_Details = "_PurchaseOrder_PurchaseOrderDetails";
        public static string POS_PurchaseInvoice_PriceSettings = "POS_PurchaseInvoice_PriceSettings";
        public static string _POS_Sales_Win_Hold = "_POS_Sales_Win_Hold";
        public static string _POS_SDetails_Win_Hold = "_POS_SDetails_Win_Hold";
        public static string POS_StockAdjustemnt = "POS_StockAdjustemnt";
        public static string POS_StockAdjustmentReasonMaster = "POS_StockAdjustmentReasonMaster";
        public static string POS_StockAdjustemntDetails = "POS_StockAdjustment_Details_PriceSettings";
        public static string POS_StockTransfer = "_StockTransfer";
        public static string POS_StockTransferDetails = "_StockTransfer_STDetails_PriceSettings_ItemBatch";
        public static string POS_AccountGroups = "POS_AccountGroups";
        public static string _CustomerReceiptMaster = "_CustomerReceiptMaster";
        public static string _CustomerReceiptDetails = "_CustomerReceiptDetails";
        public static string _VendorPaymentMaster = "_VendorPaymentMaster";
        public static string _VendorPaymentDetails = "_VendorPaymentDetails";
        public static string _CompanyInfo = "_CompanyInfo";
        public static string _POS_CashBankBook = "POS_CashBankBook";
        public static string POS_BankStatementReport = "POS_BankStatementReport";
        public static string _CompanyDetails = "_CompanyDetails";
        public static string _POS_GetWeighingItems = "_POS_GetWeighingItems";
        public const string _POS_DayBook = "POS_DayBook";
        public static string POS_ShiftClosing = "POS_ShiftClosing";
        public static string POS_ShiftClosingDenominations = "POS_ShiftClosingDenominations";
        public static string POS_CounterReport = "POS_CounterReport";
        public static string _POS_Opening_Stock = "_POS_Opening_Stock";
        public static string _CreditNoteMaster = "_CreditNoteMaster";
        public static string _CreditNoteDetails = "_CreditNoteDetails";
        public static string _DebitNoteMaster = "_DebitNoteMaster";
        public static string _DebitNoteDetails = "_DebitNoteDetails";
        public static string  POS_Setting = "POS_Setting";
        public static string _POS_SPaymentDetails = "_POS_SPaymentDetails";
        public static string POS_ItemStockActivityLog = "POS_ItemStockActivityLog";
        public static string POS_ItemHistoryLog = "POS_ItemHistoryLog";
        public static string POS_ItemActivityLog = "POS_ItemActivityLog";
        public static string POS_UserActivityLog = "POS_UserActivityLog";
        public static string POS_TransactionActivityLog = "POS_TransactionActivityLog";                             
        public static string POS_VendorOutstandingListing = "POS_VendorOutstandingListing";


        #region Report Side
        public static string _POS_SALES_REPORT = "_POS_SALES_REPORT";
        public static string _POS_Sales_Master_for_Report = "_POS_Sales_Master_for_Report";
        public static string _POS_Sales_Details_for_Report = "_POS_Sales_Details_for_Report";
        public static string _POS_Purchase_Master_for_Report = "_POS_Purchase_Master_for_Report";
        public static string _POS_Purchase_Details_for_Report = "_POS_Purchase_Details_for_Report";
        public static string POS_VendorPurchaseReport = "POS_VendorPurchaseReport";
        public static string _POS_SalesReturn_Master_for_Report = "_POS_SalesReturn_Master_for_Report";
        public static string _POS_SalesReturn_Details_for_Report = "_POS_SalesReturn_Details_for_Report";
        public static string _POS_PurchaseReturn_Master_for_Report = "_POS_PurchaseReturn_Master_for_Report";
        public static string _POS_PurchaseReturn_Details_for_Report = "_POS_PurchaseReturn_Details_for_Report";
        public static string POS_ItemAuditReport = "POS_ItemAuditReport";
        public static string _POS_ItemReport = "_Test9";
        public static string _POS_StockReportAdvanced = "_Test16";
        public static string POS_ItemwiseSalesSummaryReport = "POS_ItemwiseSalesSummaryReport";
        public static string POS_CustomerwiseSalesSummaryReport = "POS_CustomerwiseSalesSummaryReport";
        public static string POS_SalesmanwiseSalesSummaryReport = "POS_SalesmanwiseSalesSummaryReport";
        public static string POS_LowStockAlertReport = "POS_LowStockAlertReport";
        public static string POS_RolePermission = "POS_RolePermission";
        public static string _SalesProfitReport = "SalesProfitReport";
        public static string _POS_Salesman_Incentive_Report = "_POS_Salesman_Incentive_Report";
        public static string _POS_TradingPLAccount = "POS_TradingPLAccount";
        public static string _POS_BalanceSheet = "POS_BalanceSheet";
        public static string _POS_TrialBalance = "POS_TrialBalance";
        public static string _CustomerOutstandingReport = "_CustomerOutstandingReport";
        public static string _CustomerReceiptReport = "_CustomerReceiptReport";
        public static string _POS_CustomerLedger = "POS_CustomerLedger";
        public static string _VendorOutstandingReport = "_VendorOutstandingReport";
        public static string _VendorPaymentReport = "_VendorPaymentReport";
        public static string _POS_GetSmartReorderSuggestions = "_POS_GetSmartReorderSuggestions";
        public static string _POS_CalculateReorderStats = "_POS_CalculateReorderStats";
        public static string POS_GetGeneralVoucherHistory = "POS_GetGeneralVoucherHistory";
        public static string POS_BankReconciliation = "POS_BankReconciliation";

        #endregion

    }
}
