using System;

namespace ModelClass.TransactionModels
{
    public class SoldItemHistory
    {
        public long BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public string CustomerName { get; set; }
        public string PaymodeName { get; set; }
        public double TaxAmt { get; set; }
        public double SubTotal { get; set; }
        public double NetAmount { get; set; }
        public double ReceivedAmount { get; set; }
        public int Qty { get; set; }
        public float UnitPrice { get; set; }
    }
}
