using System;
using System.Collections.Generic;
using System.Globalization;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace PosBranch_Win.Reports.FinancialReports
{
    internal static class ActivityReportText
    {
        // Labels shared by current records and recorded changes. Fields are in display order.
        private static readonly Field[] Fields =
        {
            new Field("Item", "itemname", "description"), new Field("Name", "name", "ledgername", "categoryname",
                "subcategoryname", "brandname", "groupname", "unitname", "branchname", "companyname", "currencyname",
                "reasonname", "taxtype", "offername", "rolename", "settingkey", "partytype"),
            new Field("Supplier", "vendorname"), new Field("Customer", "customername", "partyname"),
            new Field("Invoice", "invoiceno"), new Field("Date", "billdate", "purchasedate", "voucherdate", "orderdate",
                "porderdate", "podate", "stockadjustmentdate", "transferdate", "entrydate", "documentdate", "date"),
            new Field("Status date", "statusdate"),
            new Field("Payment method", "paymodename", "paymode", "mode", "paymenttype"),
            new Field("Barcode", "barcode"), new Field("Alternate barcode", "aliasbarcode"),
            new Field("Unit", "unit"), new Field("Pack size", "packing"),
            new Field("Qty", "qty", "quantity", "returnqty", "loadedqty", "unloadedqty"),
            new Field("Free qty", "free"), new Field("Cost", "cost", "unitcost", "rate"),
            new Field("Price", "unitprice", "salesprice", "sellingprice", "sprice"),
            new Field("Retail price", "retailprice"), new Field("Wholesale price", "wholesaleprice"),
            new Field("MRP", "mrp"), new Field("Credit price", "creditprice"), new Field("Card price", "cardprice"),
            new Field("Stock", "stock", "available"), new Field("System stock", "systemstock"),
            new Field("Counted stock", "physicalstock"), new Field("Stock adjustment", "qtydifference"),
            new Field("Discount", "billdiscountamt", "orderdiscountamt", "discountamt", "discountamount", "disamt"),
            new Field("Tax", "taxamt"), new Field("Tax rate", "taxper"), new Field("Rounding", "roundamount", "roundoff"),
            new Field("Total", "netamount", "grandtotal", "totalamount", "nettotal", "totalprice", "amount", "amt",
                "paymentamount", "receiptamount", "settlementamount", "creditamount", "debitamount"),
            new Field("Bill amount", "billamount"), new Field("Balance", "balanceamount", "balance"),
            new Field("Debit", "debit", "opndebit"), new Field("Credit", "credit", "opncredit"),
            new Field("Credit limit", "creditlimit"), new Field("Credit days", "creditperiod", "creditdays"),
            new Field("Status", "statusname", "status", "itemstatus", "orderstatus", "officesupplystatus"),
            new Field("Active", "isactive", "active"), new Field("Cancelled", "cancelflag"),
            new Field("Deleted", "isdelete", "isdeleted"), new Field("Sales blocked", "blocksale"),
            new Field("Purchases blocked", "blockpurchase"),
            new Field("Reason", "statusreason", "reason"), new Field("Reference", "reference"), new Field("Notes", "narration", "comments", "remarks")
        };
        private static readonly HashSet<string> ShowZero = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Stock", "System stock", "Counted stock", "Stock adjustment", "Qty", "Total", "Sales blocked", "Purchases blocked", "Active"
        };
        private static readonly Regex DatabaseFields = new Regex(
            @"(?:^|[|\r\n])\s*([A-Za-z][A-Za-z0-9_ ]*)\s*[:=]\s*(.*?)(?=\s*(?:[|\r\n]\s*[A-Za-z][A-Za-z0-9_ ]*\s*[:=]|$))",
            RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex ScreenFields = new Regex(
            @"\b(txtInvoiceNo|txtBilledBy|DtpInoviceDate|dtpPurchaseDate|txtCustomer|txtDisc|textBoxround|textBoxhold|cmpPrice|cmbPaymt|TaxAmt|NetAmt|Paymode|SlNo)\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Dictionary<string, string> ScreenLabels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["txtInvoiceNo"] = "Invoice", ["txtBilledBy"] = "Billed by", ["DtpInoviceDate"] = "Invoice date",
            ["dtpPurchaseDate"] = "Purchase date", ["txtCustomer"] = "Customer", ["txtDisc"] = "Discount",
            ["textBoxround"] = "Rounding", ["textBoxhold"] = "Hold reference", ["cmpPrice"] = "Price level",
            ["cmbPaymt"] = "Payment method", ["TaxAmt"] = "Tax", ["NetAmt"] = "Total",
            ["Paymode"] = "Payment method", ["SlNo"] = "Line"
        };

        internal static string ActionLabel(string value)
        {
            switch (value.ToUpperInvariant())
            {
                case "SAVE": return "Saved";
                case "UPDATE": return "Updated";
                case "DELETE":
                case "DELETED": return "Deleted";
                case "CANCEL":
                case "CANCELLED": return "Cancelled";
                case "HOLD": return "Held";
                case "ON FILE": return "On file";
                default: return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower(CultureInfo.CurrentCulture));
            }
        }

        internal static string ReadLog(string text)
        {
            var lines = ScreenFields.Replace(text.Replace(" (txt_unitcost)", ""), match => ScreenLabels[match.Value])
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .Select(line => line.Trim(' ', '|'))
                .Where(line => line.Length > 0 && !line.Equals("Header:", StringComparison.OrdinalIgnoreCase) &&
                    !line.Equals("Details:", StringComparison.OrdinalIgnoreCase));
            return string.Join(Environment.NewLine, lines);
        }

        internal static string Summary(string action, string module, string details, object amount)
        {
            string description = ActionLabel(action);
            if (amount != DBNull.Value)
                return $"{description} · Total {amount:N2}";

            var fields = Parse(details);
            if (module == "Item Master")
            {
                string name = Find(fields, "itemname", "description");
                if (!string.IsNullOrEmpty(name))
                    return description + " · " + name;
            }
            if (fields.Count == 0 && !string.IsNullOrWhiteSpace(details))
            {
                string firstLine = ReadLog(details).Split('\n')[0].TrimEnd('\r');
                if (firstLine.Length > 0)
                    return firstLine.Length <= 110 ? firstLine : firstLine.Substring(0, 107) + "…";
            }
            return description;
        }

        internal static Detail SavedItemDetails(string notes, IEnumerable<DataRow> images, string document)
        {
            var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string line in ReadLog(notes).Split('\n'))
            {
                string entry = line.Trim().TrimStart('-').TrimStart();
                int separator = entry.IndexOf(':');
                if (separator > 0)
                    fields[entry.Substring(0, separator)] = entry.Substring(separator + 1).Trim();
            }

            // Older saves recorded the tax rate on the unit rather than in the item notes.
            var taxes = images.Where(row => Convert.ToString(row["Module"]) == "Item Price and Stock" &&
                                            Convert.ToString(row["RowVersion"]) == "After")
                .Select(row => Parse(Convert.ToString(row["Details"]))).ToList();
            string rates = string.Join(", ", taxes.Select(tax => Find(tax, "taxper")).Where(rate => rate.Length > 0)
                .Select(rate => DisplayValue("Tax rate", rate) + "%").Distinct());
            string types = string.Join(", ", taxes.Select(tax => Find(tax, "taxtype")).Where(type => type.Length > 0).Distinct());
            if (!fields.ContainsKey("Tax rate") && rates.Length > 0)
                fields["Tax rate"] = rates;
            if (!fields.ContainsKey("Tax type") && types.Length > 0)
                fields["Tax type"] = types;

            string hsn = images.Where(row => Convert.ToString(row["Module"]) == "Item Master" &&
                                             Convert.ToString(row["RowVersion"]) == "After")
                .Select(row => Find(Parse(Convert.ToString(row["Details"])), "hsncode")).LastOrDefault(value => value.Length > 0);
            if (!fields.ContainsKey("HSN") && !string.IsNullOrEmpty(hsn))
                fields["HSN"] = hsn;

            string description = string.Join(" · ", new[] { "Category", "Brand", "HSN", "Tax type", "Tax rate", "Group" }
                .Where(label => fields.ContainsKey(label) && fields[label].Length > 0)
                .Select(label => $"{label}: {fields[label]}"));
            return description.Length == 0 ? null : new Detail("Item details", null, document, description);
        }

        internal static string ItemClassificationChanges(string notes)
        {
            return string.Join(" · ", ReadLog(notes).Split('\n').Select(line => line.Trim().TrimStart('-').TrimStart())
                .Where(line => Regex.IsMatch(line, @"^(Category|Group|Brand|HSN) changed from ", RegexOptions.IgnoreCase))
                .Select(line => line.Replace(" changed from ", " ").Replace("' to '", "' → '").Replace(" to ", " → ")));
        }

        internal static IEnumerable<Detail> CapturedChanges(IEnumerable<DataRow> rows)
        {
            var seen = new HashSet<Tuple<string, string, long?, string>>();
            var records = rows.Select(row => new Image(row)).GroupBy(image => Tuple.Create(
                image.Module, image.Document, image.Identity, image.Transaction));
            foreach (var record in records)
            {
                var statements = record.GroupBy(image => image.Statement)
                    .OrderBy(statement => statement.Min(image => image.Date)).ToList();

                var first = statements[0].ToList();
                var last = statements[statements.Count - 1].ToList();
                var original = first.Where(image => image.Version == "Before").ToList();
                var final = last.Where(image => image.Version == "After").ToList();

                // An edit can replace a unit or line by deleting and reinserting it.
                // Compare the endpoints when the business key identifies exactly one record.
                if (original.Count == 1 && final.Count == 1)
                {
                    Detail detail = final[0].Detail(ChangedFields(original[0].Values, final[0].Values));
                    if (detail != null && seen.Add(detail.Key))
                        yield return detail;
                    continue;
                }

                if (statements.Count > 1 && first.All(image => image.Action == "SAVE") && final.Count == 1)
                {
                    Detail detail = final[0].Detail(Describe(final[0].Values), "Saved");
                    if (detail != null && seen.Add(detail.Key))
                        yield return detail;
                    continue;
                }

                foreach (var statement in statements)
                {
                    foreach (Detail detail in DescribeStatement(statement))
                        if (seen.Add(detail.Key))
                            yield return detail;
                }
            }
        }

        private static IEnumerable<Detail> DescribeStatement(IEnumerable<Image> statement)
        {
            var images = statement.ToList();
            foreach (var item in images.GroupBy(image => image.Identity))
            {
                var before = item.Where(image => image.Version == "Before").ToList();
                var after = item.Where(image => image.Version == "After").ToList();
                if (images[0].Action == "UPDATE")
                {
                    // Pair unchanged images first to collapse repeated database writes.
                    for (int i = before.Count - 1; i >= 0; i--)
                    {
                        int index = after.FindIndex(image => image.Text == before[i].Text);
                        if (index < 0)
                            continue;
                        before.RemoveAt(i);
                        after.RemoveAt(index);
                    }
                    if (before.Count == 1 && after.Count == 1)
                    {
                        Image updated = after[0];
                        var changed = ChangedFields(before[0].Values, updated.Values);
                        Detail detail = updated.Detail(changed);
                        if (detail != null)
                            yield return detail;
                        continue;
                    }
                }

                foreach (Image image in before.Concat(after))
                {
                    string section = image.Action == "UPDATE"
                        ? image.Version == "Before" ? "Previous values" : "Updated values"
                        : image.Module == "Item Status Rule" ? "Item status"
                        : image.Module.EndsWith(" Posting", StringComparison.Ordinal) || image.Module == "General Payment" ||
                          image.Module == "General Receipt" ? "Ledger posting" : ActionLabel(image.Action);
                    Detail detail = image.Detail(Describe(image.Values), section);
                    if (detail != null)
                        yield return detail;
                }
            }
        }

        internal static Detail CurrentDetail(DataRow row)
        {
            string section = Convert.ToString(row["Section"]);
            string record = Convert.ToString(row["Record"]);
            string text = Convert.ToString(row["Details"]);
            var fields = Parse(text);
            string readable = Describe(fields, record);
            if (readable.Length == 0 && fields.Count == 0)
                readable = ReadLog(text);
            return readable.Length == 0 ? null : new Detail(section, row.Field<long?>("Line"), record, readable);
        }

        private static Dictionary<string, string> Parse(string text)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match match in DatabaseFields.Matches(text))
            {
                string value = match.Groups[2].Value.Trim();
                if (value.Length > 0 && !value.Equals("(null)", StringComparison.OrdinalIgnoreCase))
                    result[match.Groups[1].Value.Replace(" ", "")] = value;
            }
            return result;
        }

        private static string Describe(Dictionary<string, string> values, string record = null)
        {
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var parts = new List<string>();
            foreach (Field field in Fields)
            {
                string key = field.Keys.FirstOrDefault(values.ContainsKey);
                if (key == null || !used.Add(key))
                    continue;

                string value = values[key];
                if ((field.Label == "Item" || field.Label == "Name" || field.Label == "Supplier" || field.Label == "Customer")
                    && value.Equals(record, StringComparison.CurrentCultureIgnoreCase))
                    continue;
                if (value == "0" && !ShowZero.Contains(field.Label))
                    continue;
                parts.Add(field.Label + " " + DisplayValue(field.Label, value));
            }
            return string.Join(" · ", parts);
        }

        private static string ChangedFields(Dictionary<string, string> before, Dictionary<string, string> after)
        {
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var changes = new List<string>();
            foreach (Field field in Fields)
            {
                foreach (string key in field.Keys)
                {
                    before.TryGetValue(key, out string oldValue);
                    after.TryGetValue(key, out string newValue);
                    if (oldValue == newValue || !used.Add(key))
                        continue;

                    changes.Add($"{field.Label} {DisplayValue(field.Label, oldValue)} → {DisplayValue(field.Label, newValue)}");
                }
            }
            return string.Join(" · ", changes.Distinct());
        }

        private static string DisplayValue(string label, string value)
        {
            if (string.IsNullOrEmpty(value))
                return "—";
            if (label == "Active" || label == "Cancelled" || label == "Deleted" || label.EndsWith(" blocked", StringComparison.Ordinal))
            {
                if (value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase))
                    return "Yes";
                if (value == "0" || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                    return "No";
            }
            if (label.EndsWith("date", StringComparison.OrdinalIgnoreCase) && DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                return date.ToString("dd MMM yyyy");
            if (!label.EndsWith("barcode", StringComparison.OrdinalIgnoreCase) && label != "Invoice" && decimal.TryParse(value, NumberStyles.Number,
                CultureInfo.InvariantCulture, out decimal amount))
                return amount.ToString("#,##0.####", CultureInfo.CurrentCulture);
            return value;
        }

        private static string Find(Dictionary<string, string> values, params string[] keys)
        {
            foreach (string key in keys)
                if (values.TryGetValue(key, out string value))
                    return value;
            return "";
        }

        private sealed class Field
        {
            internal readonly string Label;
            internal readonly string[] Keys;

            internal Field(string label, params string[] keys)
            {
                Label = label;
                Keys = keys;
            }
        }

        private sealed class Image
        {
            internal readonly string Module;
            internal readonly string Document;
            internal readonly string Action;
            internal readonly string Version;
            internal readonly string Text;
            internal readonly string Statement;
            internal readonly DateTime Date;
            internal readonly string Transaction;
            internal readonly Dictionary<string, string> Values;

            internal Image(DataRow row)
            {
                Module = Convert.ToString(row["Module"]);
                Document = Convert.ToString(row["DocumentNo"]);
                Action = Convert.ToString(row["ActionType"]);
                Version = Convert.ToString(row["RowVersion"]);
                Text = Convert.ToString(row["Details"]);
                Date = (DateTime)row["ActivityDate"];
                Statement = row["WriteStatementId"] is Guid id
                    ? id.ToString() : Date.ToString("O", CultureInfo.InvariantCulture);
                Transaction = row["WriteTransactionId"] is long transaction
                    ? transaction.ToString(CultureInfo.InvariantCulture) : string.Empty;
                Values = Parse(Text);
            }

            internal string Identity => string.Join("|", new[] { Find(Values, "slno"), Find(Values, "unitid"),
                Find(Values, "packing"), Find(Values, "batchid", "batchno") });

            internal Detail Detail(string description, string section = "Changed")
            {
                if (string.IsNullOrEmpty(description))
                    return null;
                if (Module == "Item Status Rule")
                    section = "Item status";
                else if (section == "Saved" && Module.EndsWith(" Detail", StringComparison.Ordinal))
                    section = "Item line";
                string record = Find(Values, "itemname", "ledgername", "description");
                if (string.IsNullOrEmpty(record))
                    record = (Module == "Item Status Rule" ? "Item" : Module) + " " + Document;
                long? line = long.TryParse(Find(Values, "slno"), out long number) ? number : (long?)null;
                if (section == "Item line" && line.HasValue && record == Module + " " + Document)
                    record = "Line " + line.Value.ToString(CultureInfo.CurrentCulture);
                return new Detail(section, line, record.Trim(), description);
            }
        }

        internal sealed class Detail
        {
            internal readonly string Section;
            internal readonly long? Line;
            internal readonly string Record;
            internal readonly string Text;
            internal Tuple<string, string, long?, string> Key => Tuple.Create(Section, Record, Line, Text);

            internal Detail(string section, long? line, string record, string text)
            {
                Section = section;
                Line = line;
                Record = record;
                Text = text;
            }
        }
    }
}
