using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass
{
    public static class VoucherType
    {
        public static string Contra = "Contra";
        public static string CreditNote = "Credit Note";
        public static string DebitNote = "Debit Note";
        public static string Journal = "Journal";
        public static string Payment = "Payment";
        public static string Purchase = "Purchase";
        public static string Sales = "Sales";
        public static string Receipt = "Receipt";
        public static string DeliveryNote = "Delivery Note";
        public static string ReceiptNote = "Receipt Note";
        public static string StockJournal = "Stock Journal";
        public static string PhysicalStock = "Physical Stock";
        public static string Indent = "Indent";
        public static string PurchaseOrder = "Purchase Order";
        public static string SalesOrder = "Sales Order";
        public static string ManufacturingJournal = "Manufacturing Journal";
        public static string PurchaseReturn = "PR";
        public static string StockTransfer = "Stock Transfer";
    }

    public static class DefaultLedgers
    {
        public static string PURCHASE = "PURCHASE";
        public static string CASH = "CASH-IN-HAND";
        public static string BEGINSTOCK = "STOCK IN HAND"; // Primary Stock In Hand ledger (Group 18) used in PhysicalStock vouchers
        //public static string TAXPAYABLE = "TAX PAYABLE"; // REMOVE LATER
        //public static string TAXRECEIVABLE = "TAX RECEIVABLE"; // REMOVE LATER
        public static string DISCOUNTRECEIVED = "DISCOUNT RECEIVED";
        public static string DISCOUNTALLOWED = "DISCOUNT ALLOWED";
        public static string SALE = "SALES";
        public static string DEFAULTCUSTOMER = "DEFAULT CUSTOMER";
        public static string DEFAULTTRANSPORTER = "DEFAULT TRANSPORTER";
        public static string DELIVERYCHARGE = "DELIVERY CHARGE";
        public static string LOCALPURCHASE = "LOCAL PURCHASE";
        public static string LOCALSALE = "LOCAL SALES";
        public static string OFFICESUPPLIES = "OFFICE SUPPLIES";
        public static string CASHEXCESSORSHORT = "CASH EXCESS OR SHORT";

        // CURRENTLY NOT USING IGST
        //public static string INPUTIGST5 = "INPUT IGST 5%";
        //public static string INPUTIGST12 = "INPUT IGST 12%";
        //public static string INPUTIGST18 = "INPUT IGST 18%";
        //public static string INPUTIGST28 = "INPUT IGST 28%";
        //public static string OUTPUTIGST5 = "OUTPUT IGST 5%";
        //public static string OUTPUTIGST12 = "OUTPUT IGST 12%";
        //public static string OUTPUTIGST18 = "OUTPUT IGST 18%";
        //public static string OUTPUTIGST28 = "OUTPUT IGST 28%";

        public static string INPUTCGST2point5 = "INPUT CGST 2.5%";
        public static string INPUTCGST6 = "INPUT CGST 6%";
        public static string INPUTCGST9 = "INPUT CGST 9%";
        public static string INPUTCGST14 = "INPUT CGST 14%";
        public static string OUTPUTCGST2point5 = "OUTPUT CGST 2.5%";
        public static string OUTPUTCGST6 = "OUTPUT CGST 6%";
        public static string OUTPUTCGST9 = "OUTPUT CGST 9%";
        public static string OUTPUTCGST14 = "OUTPUT CGST 14%";

        public static string INPUTSGST2point5 = "INPUT SGST 2.5%";
        public static string INPUTSGST6 = "INPUT SGST 6%";
        public static string INPUTSGST9 = "INPUT SGST 9%";
        public static string INPUTSGST14 = "INPUT SGST 14%";
        public static string OUTPUTSGST2point5 = "OUTPUT SGST 2.5%";
        public static string OUTPUTSGST6 = "OUTPUT SGST 6%";
        public static string OUTPUTSGST9 = "OUTPUT SGST 9%";
        public static string OUTPUTSGST14 = "OUTPUT SGST 14%";

        public static string INPUTGSTCESS28 = "INPUT GST CESS 28%";
        public static string OUTPUTGSTCESS28 = "OUTPUT GST CESS 28%";

        public static string KERALAFLOODCESS1 = "KERALA FLOOD CESS 1%";

        public static string FREIGHTIN = "FREIGHT-IN";
        public static string PURCHASEEXPENSE = "PURCHASE-EXPENSE";
        public static string PURCHASEOTHEREXPENSE = "PURCHASE-OTHER-EXPENSE";
    }

    public enum AccountGroup
    {
        CAPITAL_ACCOUNT = 1,
        LOANS_LIABILITY = 2,
        CURRENT_LIABILITIES = 3,
        FIXED_ASSETS = 4,
        INVESTMENTS = 5,
        CURRENT_ASSETS = 6,
        MISC_EXPENSES_ASSETS = 7,
        SALES_ACCOUNT = 8,
        PURCHASE_ACCOUNT = 9,
        DIRECT_EXPENSES = 10,
        DIRECT_INCOME = 11,
        INDIRECT_EXPENSES = 12,
        INDIRECT_INCOME = 13,
        CASH_IN_HAND = 14,
        BANK_ACCOUNTS = 15,
        SUNDRY_DEBTORS = 16,
        SUNDRY_CREDITORS = 17,
        STOCK_IN_HAND = 18,
        BRANCH_OR_DIVISIONS = 19,
        RESERVES_AND_SURPLUS = 20,
        DEPOSITS_ASSETS = 21,
        LOANS_AND_ADVANCES_ASSET = 22,
        DUTIES_AND_TAXES = 23,
        PROVISIONS = 24,
        BANK_OD_AC = 25,
        SECURED_LOANS = 26,
        UNSECURED_LOANS = 27,
        SUSPENSE_AC = 28,
        SUNDRY_TRANSPORTER = 29
    }



}
