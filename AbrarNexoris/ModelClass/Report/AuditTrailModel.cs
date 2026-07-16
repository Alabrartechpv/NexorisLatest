using System;
using System.Collections.Generic;

namespace ModelClass.Report
{
    public class AuditTrailItem
    {
        public int SlNo { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime ReportDate { get; set; }
        public string TableName { get; set; }
        public int ItemId { get; set; }
        public string ItemNo { get; set; }
        public string Description { get; set; }
        public string CategoryName { get; set; }
        public string GroupName { get; set; }
        public string DocNo { get; set; }
        public string Account { get; set; }
        public string Reference { get; set; }
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public decimal BalanceBF { get; set; }
        public string Action { get; set; }
        public decimal Quantity { get; set; }
        public decimal BalanceCF { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }
        public int GroupId { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public int ModelId { get; set; }
    }

    public class AuditTrailFilter : SessionAwareModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string ActivityKey { get; set; }
        public string Action { get; set; }
        public string SearchText { get; set; }
        public int? ItemId { get; set; }
        public string ItemNo { get; set; }
        public int? GroupId { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public int? ModelId { get; set; }
        public int? SelectedUserId { get; set; }
    }

    public class AuditTrailGrid
    {
        public IEnumerable<AuditTrailItem> List { get; set; }
    }
}
