using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ModelClass.Report;

namespace Repository.ReportRepository
{
    public class VendorLedgerReportRepository : LedgerReportRepository
    {
        public VendorLedgerReportRepository() : base(STOREDPROCEDURE._POS_VendorLedger)
        {
        }

        protected override CustomerLedgerReportRow CreateReportRow() => new VendorLedgerReportRow();

        protected override void ReadBillDetails(DataSet data, List<CustomerLedgerReportRow> rows)
        {
            var details = data.Tables.Count > 2
                ? data.Tables[2].AsEnumerable().ToLookup(BillKey)
                : Enumerable.Empty<DataRow>().ToLookup(BillKey);
            foreach (VendorLedgerReportRow row in rows)
            {
                var key = Tuple.Create(row.VoucherID, row.VoucherTypeName ?? string.Empty, row.VoucherDate);
                foreach (DataRow bill in details[key])
                {
                    row.Bills.Add(new VendorLedgerBillRow
                    {
                        Document = bill.Field<string>("Document"),
                        BillNumber = bill.Field<string>("BillNumber"),
                        InvoiceNumber = bill.Field<string>("InvoiceNumber"),
                        BillDate = bill.Field<DateTime?>("BillDate"),
                        Detail = bill.Field<string>("Detail"),
                        Quantity = bill.Field<decimal?>("Quantity"),
                        Amount = bill.Field<decimal?>("Amount"),
                        Applied = bill.Field<decimal?>("Applied"),
                        BillBalance = bill.Field<decimal?>("BillBalance")
                    });
                }

                if (row.Bills.Count == 0)
                {
                    row.Bills.Add(new VendorLedgerBillRow
                    {
                        Document = row.VoucherTypeName,
                        BillDate = row.VoucherDate,
                        Detail = "No linked bill was found for this voucher."
                    });
                }
            }
        }

        private static Tuple<long, string, DateTime> BillKey(DataRow row) =>
            Tuple.Create(Convert.ToInt64(row["VoucherID"]), row.Field<string>("VoucherTypeName") ?? string.Empty,
                row.Field<DateTime>("VoucherDate"));
    }
}
