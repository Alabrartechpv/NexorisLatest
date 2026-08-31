
using ModelClass;
using ModelClass.Master;
using PosBranch_Win.DialogBox;
using PosBranch_Win.Transaction;
using Repository.MasterRepositry;
using Repository.SettingsRepo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Infragistics.Win.UltraWinGrid;
using Infragistics.Win;
using Repository;

namespace PosBranch_Win.Master
{
    public partial class frmItemMasterNew : Form
    {
        // Static event to notify other forms when item master is updated
        // The int parameter is the ItemId that was updated
        public static event Action<int> OnItemMasterUpdated;

        private System.Windows.Forms.Timer orderCycleSpinnerTimer;
        private int orderCycleSpinnerDirection = 0;
        private bool orderCycleSpinnerIsInitialDelay = true;

        // Helper method to raise the item master update event safely
        private static void RaiseItemMasterUpdated(int itemId)
        {
            try
            {
                OnItemMasterUpdated?.Invoke(itemId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error raising OnItemMasterUpdated event: {ex.Message}");
            }
        }

        // Helper method to notify other forms of real-time changes (not just on save)
        private void NotifyItemMasterChanged()
        {
            try
            {
                // Only notify if there's a current item loaded
                int itemId = 0;
                if (ItemMaster != null && ItemMaster.ItemId > 0)
                {
                    itemId = ItemMaster.ItemId;
                }
                else if (CurrentItemId > 0)
                {
                    itemId = CurrentItemId;
                }

                if (itemId > 0)
                {
                    RaiseItemMasterUpdated(itemId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error notifying item master change: {ex.Message}");
            }
        }

        private void LogItemActivity(string activityType, string activityDetails)
        {
            try
            {
                int itemId = 0;
                if (ItemMaster != null && ItemMaster.ItemId > 0)
                {
                    itemId = ItemMaster.ItemId;
                }
                else if (CurrentItemId > 0)
                {
                    itemId = CurrentItemId;
                }

                string itemNo = txt_ItemNo?.Text?.Trim() ?? (ItemMaster != null ? ItemMaster.ItemNo.ToString() : string.Empty);
                string itemName = txt_description?.Text?.Trim() ?? (ItemMaster != null ? ItemMaster.Description : string.Empty);
                string barcode = string.Empty;

                try
                {
                    var txtBarcodeCtrl = GetMainBarcodeEditor();
                    barcode = txtBarcodeCtrl != null ? (txtBarcodeCtrl.Text ?? string.Empty).Trim() : (ItemMaster != null ? ItemMaster.Barcode : string.Empty);
                }
                catch
                {
                    barcode = ItemMaster != null ? ItemMaster.Barcode : string.Empty;
                }

                // Collect new item fields
                decimal? quantity = ParseNullableDecimal(txt_qty?.Text);
                decimal? available = ParseNullableDecimal(txt_available?.Text);
                decimal? onHold = ParseNullableDecimal(txt_hold?.Text);
                decimal? reorder = ParseNullableDecimal(textBox13?.Text);
                int? orderCycleDays = ParseNullableInt(ultraOrderCycle?.Text);
                int? boxQty = ParseNullableInt(ultraBoxQty?.Text);
                string itemType = txt_ItemType?.Text?.Trim();
                string category = txt_Category?.Text?.Trim();
                string itemGroup = txt_Group?.Text?.Trim();
                string hsn = textBox4?.Text?.Trim();
                string itemStatus = GetSelectedItemStatus();

                ItemActivityLogRepository.SaveItemActivity(
                    itemId,
                    itemNo,
                    itemName,
                    barcode,
                    activityType,
                    activityDetails,
                    ParseNullableDecimal(Txt_UnitCost?.Text),
                    ParseNullableDecimal(txt_Retail?.Text),
                    ParseNullableDecimal(txt_walkin?.Text),
                    quantity,
                    available,
                    onHold,
                    reorder,
                    orderCycleDays,
                    boxQty,
                    itemType,
                    category,
                    itemGroup,
                    hsn,
                    itemStatus);

                // Notify any open ActivityLog window to refresh in real-time
                OnItemMasterUpdated?.Invoke(itemId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Item activity log failed: {ex.Message}");
            }
        }

        private static decimal? ParseNullableDecimal(string value)
        {
            decimal parsed;
            return decimal.TryParse((value ?? string.Empty).Trim(), out parsed) ? parsed : (decimal?)null;
        }

        private static int? ParseNullableInt(string value)
        {
            int parsed;
            return int.TryParse((value ?? string.Empty).Trim(), out parsed) ? parsed : (int?)null;
        }

        private class PriceSnapshot
        {
            public decimal UnitCost { get; set; }
            public decimal MarkUpPer { get; set; }
            public decimal RetailPrice { get; set; }
            public decimal WalkinPrice { get; set; }
            public decimal CreditPrice { get; set; }
            public decimal MRP { get; set; }
            public decimal CardPrice { get; set; }
            public decimal StaffPrice { get; set; }
            public decimal MinPrice { get; set; }
        }

        private PriceSnapshot GetCurrentBasePriceSnapshot(int itemId)
        {
            try
            {
                if (itemId <= 0)
                {
                    return null;
                }

                var prices = ItemRepository.GetItemPriceSettings(itemId);
                var basePrice = prices?
                    .OrderBy(p => string.Equals(p.IsBaseUnit, "Y", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                    .ThenBy(p => Math.Abs(p.Packing - 1d) < 0.0001d ? 0 : 1)
                    .ThenBy(p => p.Packing)
                    .FirstOrDefault();

                if (basePrice == null)
                {
                    return null;
                }

                return new PriceSnapshot
                {
                    UnitCost = Convert.ToDecimal(basePrice.Cost),
                    MarkUpPer = Convert.ToDecimal(basePrice.MarginPer),
                    RetailPrice = Convert.ToDecimal(basePrice.WholeSalePrice),
                    WalkinPrice = Convert.ToDecimal(basePrice.RetailPrice),
                    CreditPrice = Convert.ToDecimal(basePrice.CreditPrice),
                    MRP = Convert.ToDecimal(basePrice.MRP),
                    CardPrice = Convert.ToDecimal(basePrice.CardPrice),
                    StaffPrice = Convert.ToDecimal(basePrice.StaffPrice),
                    MinPrice = Convert.ToDecimal(basePrice.MinPrice)
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unable to read existing item prices for activity log: {ex.Message}");
                return null;
            }
        }

        private string GetOldItemStatus(int itemId)
        {
            try
            {
                DataTable statusTable = ExecuteStoredProcedureTable(
                    STOREDPROCEDURE.POS_ItemMasterStatusRules,
                    CreateSqlParameter("@_Operation", ItemMasterOperationGetStatus),
                    CreateSqlParameter("@ItemId", itemId));

                if (statusTable.Rows.Count > 0)
                {
                    return statusTable.Rows[0]["StatusName"]?.ToString() ?? "Active";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting old status for log: {ex.Message}");
            }
            return "Active";
        }

        private string BuildSaveActivityDetails()
        {
            var details = new List<string>();
            
            string itemName = txt_description?.Text?.Trim() ?? string.Empty;
            string localLang = txt_LocalLanguage?.Text?.Trim() ?? string.Empty;
            string barcode = string.Empty;
            try
            {
                var txtBarcodeCtrl = GetMainBarcodeEditor();
                barcode = txtBarcodeCtrl != null ? (txtBarcodeCtrl.Text ?? string.Empty).Trim() : string.Empty;
            }
            catch { }

            details.Add($"- Item Name: {itemName}");
            if (!string.IsNullOrEmpty(localLang)) details.Add($"- Local Language Name: {localLang}");
            
            string itemType = txt_ItemType?.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(itemType)) details.Add($"- Item Type: {itemType}");

            string category = txt_Category?.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(category)) details.Add($"- Category: {category}");

            string group = txt_Group?.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(group)) details.Add($"- Group: {group}");

            string brand = txt_Brand?.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(brand)) details.Add($"- Brand: {brand}");

            string hsn = textBox4?.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(hsn)) details.Add($"- HSN: {hsn}");

            string customerType = txt_CustomerType?.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(customerType)) details.Add($"- Customer Type: {customerType}");

            string baseUnit = txt_BaseUnit?.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(baseUnit)) details.Add($"- Base Unit: {baseUnit}");

            string orderCycle = ultraOrderCycle?.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(orderCycle)) details.Add($"- Order Cycle Days: {orderCycle}");

            string size = ultraBoxQty?.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(size)) details.Add($"- Size: {size}");

            string status = GetSelectedItemStatus() ?? "Active";
            details.Add($"- Item Status: {status}");

            decimal? unitCost = ParseNullableDecimal(Txt_UnitCost?.Text);
            if (unitCost.HasValue) details.Add($"- Unit Cost (txt_unitcost): {FormatPrice(unitCost.Value)}");

            decimal? markup = ParseNullableDecimal(textBox1?.Text);
            if (markup.HasValue) details.Add($"- Mark Up %: {markup.Value:0.00}");

            bool openPrice = ultraCheckEditor2 != null && ultraCheckEditor2.Checked;
            if (openPrice) details.Add("- Open Price: Yes");

            bool nonDiscount = ultraCheckEditor1 != null && ultraCheckEditor1.Checked;
            if (nonDiscount) details.Add("- Non-Discount Item: Yes");

            bool isPerishable = ultraIsPerishable != null && ultraIsPerishable.Checked;
            if (isPerishable) details.Add("- Is Perishable: Yes");

            decimal? retailPrice = ParseNullableDecimal(txt_Retail?.Text);
            if (retailPrice.HasValue) details.Add($"- Retail Price: {FormatPrice(retailPrice.Value)}");

            decimal? walkinPrice = ParseNullableDecimal(txt_walkin?.Text);
            if (walkinPrice.HasValue) details.Add($"- Walkin Price: {FormatPrice(walkinPrice.Value)}");

            decimal? creditPrice = ParseNullableDecimal(txt_CEP?.Text);
            if (creditPrice.HasValue) details.Add($"- Credit Price: {FormatPrice(creditPrice.Value)}");

            decimal? mrp = ParseNullableDecimal(txt_Mrp?.Text);
            if (mrp.HasValue) details.Add($"- MRP: {FormatPrice(mrp.Value)}");

            decimal? cardPrice = ParseNullableDecimal(txt_CardP?.Text);
            if (cardPrice.HasValue) details.Add($"- Card Price: {FormatPrice(cardPrice.Value)}");

            decimal? staffPrice = ParseNullableDecimal(txt_SF?.Text);
            if (staffPrice.HasValue) details.Add($"- Staff Price: {FormatPrice(staffPrice.Value)}");

            decimal? minPrice = ParseNullableDecimal(txt_MinP?.Text);
            if (minPrice.HasValue) details.Add($"- Min Price: {FormatPrice(minPrice.Value)}");

            decimal? qty = ParseNullableDecimal(txt_qty?.Text);
            if (qty.HasValue) details.Add($"- Quantity: {qty.Value}");

            decimal? available = ParseNullableDecimal(txt_available?.Text);
            if (available.HasValue) details.Add($"- Available Stock: {available.Value}");

            decimal? hold = ParseNullableDecimal(txt_hold?.Text);
            if (hold.HasValue) details.Add($"- On Hold Stock: {hold.Value}");

            // Include any added units in ultraGrid1/Ult_Price
            try
            {
                DataGridView currentGrid = ConvertUltPriceToDataGridView();
                if (currentGrid != null)
                {
                    foreach (DataGridViewRow row in currentGrid.Rows)
                    {
                        string unitName = row.Cells["Unit"].Value?.ToString()?.Trim() ?? string.Empty;
                        if (string.IsNullOrEmpty(unitName) || string.Equals(unitName, baseUnit, StringComparison.OrdinalIgnoreCase)) continue;

                        decimal newPacking = ParseNullableDecimal(row.Cells["Packing"].Value?.ToString()) ?? 1m;
                        decimal newCost = ParseNullableDecimal(row.Cells["Cost"].Value?.ToString()) ?? 0m;
                        decimal newRetail = ParseNullableDecimal(row.Cells["RetailPrice"].Value?.ToString()) ?? 0m;
                        decimal newWalkin = ParseNullableDecimal(row.Cells["WholeSalePrice"].Value?.ToString()) ?? 0m;
                        decimal newMRP = ParseNullableDecimal(row.Cells["MRP"].Value?.ToString()) ?? 0m;

                        details.Add($"- Added Unit '{unitName}': Packing = {newPacking}, Cost = {FormatPrice(newCost)}, Retail Price = {FormatPrice(newRetail)}, Walkin Price = {FormatPrice(newWalkin)}, MRP = {FormatPrice(newMRP)}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load added units for save log: {ex.Message}");
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Item '{itemName}' (Barcode: {barcode}) created.");
            sb.AppendLine("Details:");
            foreach (var detail in details)
            {
                sb.AppendLine(detail);
            }
            return sb.ToString().TrimEnd();
        }

        private string BuildUpdateActivityDetails(ItemGet oldItem, string oldStatus, PriceSnapshot oldPrice)
        {
            var changes = new List<string>();

            // 1. Compare Base Prices & Unit Cost
            AddPriceChange(changes, "Unit Cost (txt_unitcost)", oldPrice?.UnitCost, ParseNullableDecimal(Txt_UnitCost?.Text));
            AddPriceChange(changes, "Retail Price", oldPrice?.RetailPrice, ParseNullableDecimal(txt_Retail?.Text));
            AddPriceChange(changes, "Walkin Price", oldPrice?.WalkinPrice, ParseNullableDecimal(txt_walkin?.Text));
            AddPriceChange(changes, "Credit Price", oldPrice?.CreditPrice, ParseNullableDecimal(txt_CEP?.Text));
            AddPriceChange(changes, "MRP", oldPrice?.MRP, ParseNullableDecimal(txt_Mrp?.Text));
            AddPriceChange(changes, "Card Price", oldPrice?.CardPrice, ParseNullableDecimal(txt_CardP?.Text));
            AddPriceChange(changes, "Staff Price", oldPrice?.StaffPrice, ParseNullableDecimal(txt_SF?.Text));
            AddPriceChange(changes, "Min Price", oldPrice?.MinPrice, ParseNullableDecimal(txt_MinP?.Text));
            AddPriceChange(changes, "Mark Up %", oldPrice?.MarkUpPer, ParseNullableDecimal(textBox1?.Text));

            bool nameChanged = false;
            string oldName = string.Empty;
            string newName = string.Empty;

            try
            {
                if (oldItem != null)
                {
                    // Description / Item Name
                    string newDesc = txt_description?.Text?.Trim() ?? string.Empty;
                    string oldDesc = oldItem.Description?.Trim() ?? string.Empty;
                    if (newDesc != oldDesc)
                    {
                        nameChanged = true;
                        oldName = oldDesc;
                        newName = newDesc;
                    }

                    // Local Language Name
                    string newLocalLang = txt_LocalLanguage?.Text?.Trim() ?? string.Empty;
                    string oldLocalLang = oldItem.NameInLocalLanguage?.Trim() ?? string.Empty;
                    if (!string.Equals(newLocalLang, oldLocalLang, StringComparison.OrdinalIgnoreCase))
                    {
                        changes.Add($"Local Language Name changed from '{oldLocalLang}' to '{newLocalLang}'");
                    }

                    // Base Unit
                    string newBaseUnit = txt_BaseUnit?.Text?.Trim() ?? string.Empty;
                    string oldBaseUnit = oldItem.UnitName?.Trim() ?? string.Empty;
                    if (!string.Equals(newBaseUnit, oldBaseUnit, StringComparison.OrdinalIgnoreCase))
                    {
                        changes.Add($"Base Unit changed from '{oldBaseUnit}' to '{newBaseUnit}'");
                    }

                    // Item Type
                    string newType = txt_ItemType?.Text?.Trim() ?? string.Empty;
                    string oldType = oldItem.ItemType?.Trim() ?? string.Empty;
                    if (!string.Equals(newType, oldType, StringComparison.OrdinalIgnoreCase))
                    {
                        changes.Add($"Item Type changed from '{oldType}' to '{newType}'");
                    }

                    // Category
                    string newCategory = txt_Category?.Text?.Trim() ?? string.Empty;
                    string oldCategory = oldItem.CategoryName?.Trim() ?? string.Empty;
                    if (!string.Equals(newCategory, oldCategory, StringComparison.OrdinalIgnoreCase))
                    {
                        changes.Add($"Category changed from '{oldCategory}' to '{newCategory}'");
                    }

                    // Group
                    string newGroup = txt_Group?.Text?.Trim() ?? string.Empty;
                    string oldGroup = oldItem.GroupName?.Trim() ?? string.Empty;
                    if (!string.Equals(newGroup, oldGroup, StringComparison.OrdinalIgnoreCase))
                    {
                        changes.Add($"Group changed from '{oldGroup}' to '{newGroup}'");
                    }

                    // Brand
                    string newBrand = txt_Brand?.Text?.Trim() ?? string.Empty;
                    string oldBrand = oldItem.BrandName?.Trim() ?? string.Empty;
                    if (!string.Equals(newBrand, oldBrand, StringComparison.OrdinalIgnoreCase))
                    {
                        changes.Add($"Brand changed from '{oldBrand}' to '{newBrand}'");
                    }

                    // HSN
                    string newHsn = textBox4?.Text?.Trim() ?? string.Empty;
                    string oldHsn = oldItem.HSNCode?.Trim() ?? string.Empty;
                    if (!string.Equals(newHsn, oldHsn, StringComparison.OrdinalIgnoreCase))
                    {
                        changes.Add($"HSN changed from '{oldHsn}' to '{newHsn}'");
                    }

                    // Customer Type
                    string newCustType = txt_CustomerType?.Text?.Trim() ?? string.Empty;
                    string oldCustType = oldItem.ForCustomerType?.Trim() ?? string.Empty;
                    if (!string.Equals(newCustType, oldCustType, StringComparison.OrdinalIgnoreCase))
                    {
                        changes.Add($"Customer Type changed from '{oldCustType}' to '{newCustType}'");
                    }

                    // Item Status
                    string newStatus = GetSelectedItemStatus() ?? string.Empty;
                    if (!string.Equals(newStatus, oldStatus, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(oldStatus))
                    {
                        changes.Add($"Item Status changed from '{oldStatus}' to '{newStatus}'");
                    }

                    // Compare UOM Grid / Price Grid changes in ultraGrid1/Ult_Price
                    DataGridView currentGrid = ConvertUltPriceToDataGridView();
                    if (currentGrid != null && oldItem.List != null)
                    {
                        var matchedOldUnits = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                        foreach (DataGridViewRow row in currentGrid.Rows)
                        {
                            string unitName = row.Cells["Unit"].Value?.ToString()?.Trim() ?? string.Empty;
                            if (string.IsNullOrEmpty(unitName)) continue;

                            decimal newPacking = ParseNullableDecimal(row.Cells["Packing"].Value?.ToString()) ?? 1m;
                            decimal newCost = ParseNullableDecimal(row.Cells["Cost"].Value?.ToString()) ?? 0m;
                            decimal newRetail = ParseNullableDecimal(row.Cells["RetailPrice"].Value?.ToString()) ?? 0m;
                            decimal newWalkin = ParseNullableDecimal(row.Cells["WholeSalePrice"].Value?.ToString()) ?? 0m;
                            decimal newMRP = ParseNullableDecimal(row.Cells["MRP"].Value?.ToString()) ?? 0m;
                            decimal newCard = ParseNullableDecimal(row.Cells["CardPrice"].Value?.ToString()) ?? 0m;
                            decimal newStaff = ParseNullableDecimal(row.Cells["StaffPrice"].Value?.ToString()) ?? 0m;
                            decimal newMin = ParseNullableDecimal(row.Cells["MinPrice"].Value?.ToString()) ?? 0m;
                            decimal newTaxPer = ParseNullableDecimal(row.Cells["TaxPer"].Value?.ToString()) ?? 0m;

                            var oldSetting = oldItem.List.FirstOrDefault(x => string.Equals(x.Unit?.Trim(), unitName, StringComparison.OrdinalIgnoreCase));
                            if (oldSetting != null)
                            {
                                matchedOldUnits.Add(unitName);

                                // Compare Packing
                                decimal oldPacking = Convert.ToDecimal(oldSetting.Packing);
                                if (Math.Abs(oldPacking - newPacking) > 0.0001m)
                                {
                                    changes.Add($"Unit '{unitName}' Packing changed from {oldPacking} to {newPacking}");
                                }

                                // Compare Cost
                                decimal oldCost = Convert.ToDecimal(oldSetting.Cost);
                                if (Math.Abs(oldCost - newCost) > 0.0001m)
                                {
                                    changes.Add($"Unit '{unitName}' Cost changed from {FormatPrice(oldCost)} to {FormatPrice(newCost)}");
                                }

                                AddUnitPriceChange(changes, unitName, "Retail Price", Convert.ToDecimal(oldSetting.WholeSalePrice), newRetail);
                                AddUnitPriceChange(changes, unitName, "Walkin Price", Convert.ToDecimal(oldSetting.RetailPrice), newWalkin);
                                AddUnitPriceChange(changes, unitName, "MRP", Convert.ToDecimal(oldSetting.MRP), newMRP);
                                AddUnitPriceChange(changes, unitName, "Card Price", Convert.ToDecimal(oldSetting.CardPrice), newCard);
                                AddUnitPriceChange(changes, unitName, "Staff Price", Convert.ToDecimal(oldSetting.StaffPrice), newStaff);
                                AddUnitPriceChange(changes, unitName, "Min Price", Convert.ToDecimal(oldSetting.MinPrice), newMin);
                                AddUnitPriceChange(changes, unitName, "Tax %", Convert.ToDecimal(oldSetting.TaxPer), newTaxPer);
                            }
                            else
                            {
                                changes.Add($"Added new Unit '{unitName}': Packing = {newPacking}, Cost = {FormatPrice(newCost)}, Retail Price = {FormatPrice(newRetail)}, Walkin Price = {FormatPrice(newWalkin)}, MRP = {FormatPrice(newMRP)}");
                            }
                        }

                        // Check for deleted units
                        foreach (var oldSetting in oldItem.List)
                        {
                            string oldUnit = oldSetting.Unit?.Trim();
                            if (!string.IsNullOrEmpty(oldUnit) && !matchedOldUnits.Contains(oldUnit) && !string.Equals(oldUnit, oldItem.UnitName?.Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                changes.Add($"Removed / Deleted Unit '{oldUnit}'");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to retrieve old item details for activity log comparison: {ex.Message}");
            }

            if (changes.Count == 0 && !nameChanged)
            {
                return "Item updated from item master. No change detected.";
            }

            string itemNameHeader = txt_description?.Text?.Trim() ?? string.Empty;
            string barcodeHeader = string.Empty;
            try
            {
                var txtBarcodeCtrl = GetMainBarcodeEditor();
                barcodeHeader = txtBarcodeCtrl != null ? (txtBarcodeCtrl.Text ?? string.Empty).Trim() : string.Empty;
            }
            catch { }

            StringBuilder sb = new StringBuilder();
            if (nameChanged)
            {
                sb.AppendLine($"Item '{oldName}' changed to '{newName}' (Barcode: {barcodeHeader})");
            }
            else
            {
                sb.AppendLine($"Item '{itemNameHeader}' (Barcode: {barcodeHeader}) updated.");
            }

            if (changes.Count > 0)
            {
                sb.AppendLine("Updates:");
                foreach (var change in changes)
                {
                    sb.AppendLine($"- {change}");
                }
            }
            return sb.ToString().TrimEnd();
        }

        private static void AddPriceChange(List<string> changes, string label, decimal? oldValue, decimal? newValue)
        {
            if (!oldValue.HasValue || !newValue.HasValue)
            {
                return;
            }

            if (Math.Abs(oldValue.Value - newValue.Value) < 0.0001m)
            {
                return;
            }

            changes.Add($"{label} changed from {FormatPrice(oldValue.Value)} to {FormatPrice(newValue.Value)}");
        }

        private static void AddUnitPriceChange(List<string> changes, string unitName, string label, decimal oldValue, decimal newValue)
        {
            if (Math.Abs(oldValue - newValue) < 0.0001m)
            {
                return;
            }

            changes.Add($"Unit '{unitName}' {label} changed from {FormatPrice(oldValue)} to {FormatPrice(newValue)}");
        }

        private static string FormatPrice(decimal value)
        {
            return value.ToString("0.####");
        }

        /// <summary>
        /// Here column properties for ultraGrid1
        /// </summary>
        string colUnit = "Unit";
        string colUnitId = "UnitId";
        string colPacking = "Packing";
        // string colBarcode = "BarCode"; // Removed
        string colReorder = "Reorder";
        string colOpenStock = "OpnStk";

        // Public methods to set values
        public void SetQtyValue(string value)
        {
            try
            {
                if (txt_qty != null)
                {
                    txt_qty.Text = value;
                    System.Diagnostics.Debug.WriteLine($"Successfully set txt_qty to {value}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("txt_qty is null!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting qty: {ex.Message}");
            }
        }

        public void SetAvailableValue(string value)
        {
            try
            {
                if (txt_available != null)
                {
                    txt_available.Text = value;
                    System.Diagnostics.Debug.WriteLine($"Successfully set txt_available to {value}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("txt_available is null!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting available: {ex.Message}");
            }
        }

        public void SetHoldValue(string value)
        {
            try
            {
                if (txt_hold != null)
                {
                    txt_hold.Text = value;
                    System.Diagnostics.Debug.WriteLine($"Successfully set txt_hold to {value}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("txt_hold is null!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting hold: {ex.Message}");
            }
        }

        public void SetSmartReorderValues(int orderCycleDays, int boxQuantity, bool isPerishable)
        {
            try
            {
                if (ultraOrderCycle != null)
                {
                    ultraOrderCycle.Text = orderCycleDays >= 0 ? orderCycleDays.ToString() : "0";
                }

                if (ultraBoxQty != null)
                {
                    ultraBoxQty.Text = boxQuantity >= 0 ? boxQuantity.ToString() : "0";
                }

                if (ultraIsPerishable != null)
                {
                    ultraIsPerishable.Checked = isPerishable;
                }

                ItemMaster.Order_Cycle_Days = orderCycleDays >= 0 ? orderCycleDays : 0;
                ItemMaster.Box_Quantity = boxQuantity >= 0 ? boxQuantity : 0;
                ItemMaster.Is_Perishable = isPerishable;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting smart reorder values: {ex.Message}");
            }
        }

        private int GetSmartReorderOrderCycleDays()
        {
            int orderCycleDays;
            return int.TryParse(ultraOrderCycle?.Text, out orderCycleDays) && orderCycleDays >= 0 ? orderCycleDays : 0;
        }

        private int GetSmartReorderBoxQuantity()
        {
            int boxQuantity;
            return int.TryParse(ultraBoxQty?.Text, out boxQuantity) && boxQuantity >= 0 ? boxQuantity : 0;
        }

        private bool GetSmartReorderIsPerishable()
        {
            return ultraIsPerishable != null && ultraIsPerishable.Checked;
        }

        // Method to set the walking price value
        public void SetWalkingPriceValue(string value)
        {
            try
            {
                if (txt_walkin != null)
                {
                    txt_walkin.Text = value;
                    System.Diagnostics.Debug.WriteLine($"Successfully set txt_walkin to {value}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("txt_walkin is null!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting walking price: {ex.Message}");
            }
        }

        // Method to set the retail price value
        public void SetRetailPriceValue(string value)
        {
            try
            {
                if (txt_Retail != null)
                {
                    txt_Retail.Text = value;
                    System.Diagnostics.Debug.WriteLine($"Successfully set txt_Retail to {value}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("txt_Retail is null!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting retail price: {ex.Message}");
            }
        }

        // Method to set the credit price value
        public void SetCreditPriceValue(string value)
        {
            try
            {
                if (txt_CEP != null)
                {
                    txt_CEP.Text = value;
                    System.Diagnostics.Debug.WriteLine($"Successfully set txt_CEP to {value}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("txt_CEP is null!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting credit price: {ex.Message}");
            }
        }

        // Method to set the MRP value
        public void SetMrpValue(string value)
        {
            try
            {
                if (txt_Mrp != null)
                {
                    txt_Mrp.Text = value;
                    System.Diagnostics.Debug.WriteLine($"Successfully set txt_Mrp to {value}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("txt_Mrp is null!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting MRP: {ex.Message}");
            }
        }

        // Method to set the Card Price value
        public void SetCardPriceValue(string value)
        {
            try
            {
                if (txt_CardP != null)
                {
                    txt_CardP.Text = value;
                    System.Diagnostics.Debug.WriteLine($"Successfully set txt_CardP to {value}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("txt_CardP is null!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting card price: {ex.Message}");
            }
        }

        /// <summary>
        /// Here column for dgv_price
        /// </summary>
        DataGridViewTextBoxColumn colUnit4Price = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colPacking4Price = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colCost = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colMargin = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colMarginPer = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colMrp = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colWalking = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colWholeSale = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colCredit = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colCard = new DataGridViewTextBoxColumn();


        /// <summary>
        /// here column for dgv_tax
        /// </summary>
        /// 
        DataGridViewTextBoxColumn colTaxType = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colTaxPer = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colTaxAmt = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colUnitTax = new DataGridViewTextBoxColumn();

        /// <summary>
        /// Property to maintain compatibility with existing code that uses dgv_Uom
        /// This converts ultraGrid1 data to a DataGridView for backward compatibility
        /// </summary>
        public DataGridView UomDataGridView
        {
            get
            {
                // Create a temporary DataGridView for compatibility
                DataGridView tempDgv = new DataGridView();
                tempDgv.AllowUserToAddRows = false;

                // Copy column structure
                tempDgv.Columns.Add(colUnit, "Unit");
                tempDgv.Columns.Add(colUnitId, "UnitId");
                tempDgv.Columns.Add(colPacking, "Packing");
                // tempDgv.Columns.Add(colBarcode, "BarCode"); // Removed
                tempDgv.Columns.Add(colReorder, "Reorder");
                tempDgv.Columns.Add(colOpenStock, "OpnStk");
                tempDgv.Columns.Add("AliasBarcode", "AliasBarcode");

                // Copy data from ultraGrid1 to tempDgv
                DataTable dt = ultraGrid1.DataSource as DataTable;
                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        DataGridViewRow dgvRow = new DataGridViewRow();
                        tempDgv.Rows.Add(dgvRow);
                        int rowIndex = tempDgv.Rows.Count - 1;

                        tempDgv.Rows[rowIndex].Cells[colUnit].Value = row[colUnit].ToString();
                        tempDgv.Rows[rowIndex].Cells[colUnitId].Value = row[colUnitId].ToString();
                        tempDgv.Rows[rowIndex].Cells[colPacking].Value = row[colPacking].ToString();
                        // tempDgv.Rows[rowIndex].Cells[colBarcode].Value = row[colBarcode].ToString(); // Removed
                        tempDgv.Rows[rowIndex].Cells[colReorder].Value = row[colReorder].ToString();
                        tempDgv.Rows[rowIndex].Cells[colOpenStock].Value = row[colOpenStock].ToString();
                        // Copy AliasBarcode if column exists
                        if (dt.Columns.Contains("AliasBarcode"))
                        {
                            tempDgv.Rows[rowIndex].Cells["AliasBarcode"].Value = row["AliasBarcode"]?.ToString() ?? string.Empty;
                        }
                    }
                }

                return tempDgv;
            }
            set
            {
                // Convert DataGridView to DataTable for ultraGrid1
                if (value != null && value.Rows.Count > 0)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add(colUnit, typeof(string));
                    dt.Columns.Add(colUnitId, typeof(string));
                    dt.Columns.Add(colPacking, typeof(string));
                    // dt.Columns.Add(colBarcode, typeof(string)); // Removed
                    dt.Columns.Add(colReorder, typeof(string));
                    dt.Columns.Add(colOpenStock, typeof(string));

                    foreach (DataGridViewRow row in value.Rows)
                    {
                        DataRow dtRow = dt.NewRow();
                        dtRow[colUnit] = row.Cells[colUnit].Value?.ToString() ?? string.Empty;
                        dtRow[colUnitId] = row.Cells[colUnitId].Value?.ToString() ?? string.Empty;
                        dtRow[colPacking] = row.Cells[colPacking].Value?.ToString() ?? string.Empty;
                        // dtRow[colBarcode] = row.Cells[colBarcode].Value?.ToString() ?? string.Empty; // Removed
                        dtRow[colReorder] = row.Cells[colReorder].Value?.ToString() ?? string.Empty;
                        dtRow[colOpenStock] = row.Cells[colOpenStock].Value?.ToString() ?? string.Empty;
                        dt.Rows.Add(dtRow);
                    }

                    ultraGrid1.DataSource = dt;
                }
            }
        }

        // For backward compatibility, provide the old property name as well
        public DataGridView dgv_Uom
        {
            get { return UomDataGridView; }
            set { UomDataGridView = value; }
        }

        Item ItemMaster = new Item();
        ItemMasterPriceSettings ItemPriceSettings = new ItemMasterPriceSettings();
        ItemMasterRepository ItemRepository = new ItemMasterRepository();
        ItemActivityLogRepository ItemActivityLogRepository = new ItemActivityLogRepository();
        internal object lblCost;

        // Add after other field declarations (e.g., after line 304)
        private Infragistics.Win.Misc.UltraPanel gridFooterPanel;
        private Dictionary<string, Label> footerLabels = new Dictionary<string, Label>();
        private Dictionary<string, string> columnAggregations = new Dictionary<string, string>();
        private calculate_unit_cost_base_on_selling_price_and_mark_up unitCostCalculator;
        private bool isUpdatingMarkup;
        private bool isUpdatingProfitMargins;
        private bool isProcessingProfitMarginEnter;
        private bool isLoadingItem = false; // Flag to prevent master field behavior during item loading
        private System.Windows.Forms.Timer unitCostSyncTimer;
        private bool isProcessingMarkdown = false;
        private Dictionary<string, double> lastAppliedMarkdown = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, double> lastAppliedMarkdownRetail = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        private bool isEditingStaffPrice = false; // track user typing in txt_SF to avoid caret jumps
        private bool isEditingMinPrice = false; // track user typing in txt_MinP to avoid caret jumps
        private bool isEditingMdStaff = false; // track user typing in ultraTextEditor12 markdown field to avoid caret jumps
        private bool isEditingMdMin = false; // track user typing in ultraTextEditor11 markdown field to avoid caret jumps
        private bool hasGeneratedItemNumberForBarcode = false; // track if item number has been auto-generated for current barcode entry
        private int lastLoadedItemNo = 0;
        private readonly Dictionary<int, int> purchasePidCache = new Dictionary<int, int>();
        private string loadedItemMainBarcode = string.Empty;
        private DateTime lastBarcodeRefreshClickTime = DateTime.MinValue;
        private bool isInitializingItemStatusControls;
        private bool itemStatusTableEnsured;
        private bool itemStatusHandlersWired;

        private const string ItemStatusActive = "Active";
        private const string ItemStatusInactive = "Inactive";
        private const string ItemStatusBlockedForSale = "Blocked for Sale";
        private const string ItemStatusBlockedForPurchase = "Blocked for Purchase";
        private const string ItemStatusDiscontinued = "Discontinued";
        private const string ItemStatusTableName = "POS_ItemMasterStatusRules";

        private static readonly string[] availableItemStatuses = new[]
        {
            ItemStatusActive,
            ItemStatusInactive,
            ItemStatusBlockedForSale,
            ItemStatusBlockedForPurchase,
            ItemStatusDiscontinued
        };

        private sealed class ItemStatusRuleSnapshot
        {
            public string StatusName { get; set; }
            public string Reason { get; set; }
            public DateTime StatusDate { get; set; }
            public bool BlockSale { get; set; }
            public bool BlockPurchase { get; set; }
        }

        private const string ItemMasterOperationEnsureStatusStorage = "ENSURESTATUSSTORAGE";
        private const string ItemMasterOperationGetStatus = "GETSTATUS";
        private const string ItemMasterOperationSaveStatus = "SAVESTATUS";
        private const string PurchaseOperationGetPidByPurchaseNo = "GETPIDBYPURCHASENO";

        private static SqlParameter CreateSqlParameter(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        private object ExecuteStoredProcedureScalar(string storedProcedure, params SqlParameter[] parameters)
        {
            using (BaseRepostitory repo = new BaseRepostitory())
            {
                SqlConnection connection = repo.DataConnection as SqlConnection;
                if (connection == null)
                {
                    return null;
                }

                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                using (SqlCommand cmd = new SqlCommand(storedProcedure, connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (parameters != null && parameters.Length > 0)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    return cmd.ExecuteScalar();
                }
            }
        }

        private int ExecuteStoredProcedureIntScalar(string storedProcedure, params SqlParameter[] parameters)
        {
            object result = ExecuteStoredProcedureScalar(storedProcedure, parameters);
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        private DataTable ExecuteStoredProcedureTable(string storedProcedure, params SqlParameter[] parameters)
        {
            using (BaseRepostitory repo = new BaseRepostitory())
            {
                SqlConnection connection = repo.DataConnection as SqlConnection;
                if (connection == null)
                {
                    return new DataTable();
                }

                using (SqlCommand cmd = new SqlCommand(storedProcedure, connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (parameters != null && parameters.Length > 0)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);
                        return table;
                    }
                }
            }
        }

        private UnitMaster GetUnitByNameFromStoredProcedure(string unitName)
        {
            if (string.IsNullOrWhiteSpace(unitName))
            {
                return null;
            }

            using (UnitMasterRepository unitRepository = new UnitMasterRepository())
            {
                int unitId = unitRepository.GetUnitIdByName(unitName);
                return unitId > 0 ? unitRepository.GetByIdUnit(unitId) : null;
            }
        }

        private static readonly string[] uomPriceColumnKeys = new[]
        {
                "Cost",
                "MarginAmt",
                "MarginPer",
                "TaxPer",
                "TaxAmt",
                "RetailPrice",
                "MRP",
                "WholeSalePrice",
                "CreditPrice",
                "CardPrice",
                "StaffPrice",
                "MinPrice",
                "AliasBarcode"
            };

        private static readonly Dictionary<string, string> uomPriceColumnCaptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Cost"] = "Cost",
            ["MarginAmt"] = "Margin Amount",
            ["MarginPer"] = "Margin %",
            ["TaxPer"] = "Tax %",
            ["TaxAmt"] = "Tax Amount",
            ["MRP"] = "MRP",
            ["RetailPrice"] = "Retail Price",
            ["WholeSalePrice"] = "Walking Price",
            ["CreditPrice"] = "CreditPrice",
            ["CardPrice"] = "CardPrice",
            ["StaffPrice"] = "StaffPrice",
            ["MinPrice"] = "MinPrice",
            ["AliasBarcode"] = "AliasBarcode"
        };

        private static readonly Dictionary<string, int> uomPriceColumnWidths = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Cost"] = 100,
            ["MarginAmt"] = 110,
            ["MarginPer"] = 90,
            ["TaxPer"] = 90,
            ["TaxAmt"] = 110,
            ["MRP"] = 90,
            ["RetailPrice"] = 110,
            ["WholeSalePrice"] = 110,
            ["CreditPrice"] = 110,
            ["CardPrice"] = 110,
            ["StaffPrice"] = 110,
            ["MinPrice"] = 110,
            ["AliasBarcode"] = 130
        };

        // Property to store current item ID for hold details
        public int CurrentItemId { get; set; }

        // Image handling state for pictureBoxItem
        private byte[] currentImageBytes;
        private ContextMenuStrip pictureBoxContextMenu;
        private ComboBox comboTaxType; // dropdown for tax type (incl/excl)

        // Guard flag to prevent recursive CellChange events when updating Cost
        private bool isUpdatingCostCell = false;

        public frmItemMasterNew()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeComponent();
            ApplyAppearanceTheme();
        }

        private void EnsureItemStatusControlsCreated()
        {
            if (ultraPanel1 == null || itemStatusPanel == null || cmbItemStatus == null ||
                txtItemStatusReason == null || dtpItemStatusDate == null ||
                lblItemStatusReason == null || lblItemStatusSaleRule == null || lblItemStatusPurchaseRule == null)
            {
                return;
            }

            if (cmbItemStatus.Items.Count == 0)
            {
                cmbItemStatus.Items.AddRange(availableItemStatuses);
            }

            if (!itemStatusHandlersWired)
            {
                cmbItemStatus.SelectedIndexChanged -= ItemStatusEditor_ValueChanged;
                txtItemStatusReason.TextChanged -= ItemStatusEditor_ValueChanged;
                dtpItemStatusDate.ValueChanged -= ItemStatusEditor_ValueChanged;

                cmbItemStatus.SelectedIndexChanged += ItemStatusEditor_ValueChanged;
                txtItemStatusReason.TextChanged += ItemStatusEditor_ValueChanged;
                dtpItemStatusDate.ValueChanged += ItemStatusEditor_ValueChanged;
                itemStatusHandlersWired = true;
            }

            if (cmbItemStatus.SelectedIndex < 0 && cmbItemStatus.Items.Count > 0)
            {
                cmbItemStatus.SelectedItem = ItemStatusActive;
            }

            if (btnItemStatus != null)
            {
                btnItemStatus.Click -= BtnItemStatus_Click;
                btnItemStatus.Click += BtnItemStatus_Click;
            }
            UpdateItemStatusButtonState();
        }

        private void BtnItemStatus_Click(object sender, EventArgs e)
        {
            OpenItemStatusDialog();
        }

        private void UpdateItemStatusButtonState()
        {
            if (btnItemStatus == null)
            {
                return;
            }

            string statusName = ItemStatusActive;
            if (cmbItemStatus != null)
            {
                statusName = NormalizeItemStatusName(cmbItemStatus.SelectedItem?.ToString() ?? cmbItemStatus.Text);
            }

            bool blocked = DoesStatusBlockSale(statusName) || DoesStatusBlockPurchase(statusName);
            btnItemStatus.Text = $"Status: {statusName}";
            btnItemStatus.BackColor = blocked ? Color.FromArgb(255, 224, 224) : Color.FromArgb(232, 240, 255);
            btnItemStatus.ForeColor = blocked ? Color.Firebrick : Color.MidnightBlue;
        }

        private void OpenItemStatusDialog()
        {
            EnsureItemStatusControlsCreated();
            ItemStatusRuleSnapshot snapshot = GetCurrentItemStatusRuleSnapshot();

            using (frmItemStatusPopup popup = new frmItemStatusPopup())
            {
                popup.SetStatusOptions(availableItemStatuses);
                popup.SetStatusValues(snapshot.StatusName, snapshot.Reason, snapshot.StatusDate);

                if (btnItemStatus != null)
                {
                    Point screenPoint = btnItemStatus.PointToScreen(new Point(0, btnItemStatus.Height + 2));
                    popup.StartPosition = FormStartPosition.Manual;
                    popup.Location = screenPoint;
                }

                if (popup.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                ApplyItemStatusSnapshot(new ItemStatusRuleSnapshot
                {
                    StatusName = popup.SelectedStatus,
                    Reason = popup.StatusReason,
                    StatusDate = popup.StatusDate,
                    BlockSale = DoesStatusBlockSale(popup.SelectedStatus),
                    BlockPurchase = DoesStatusBlockPurchase(popup.SelectedStatus)
                });
            }
        }

        private void ItemStatusEditor_ValueChanged(object sender, EventArgs e)
        {
            if (isInitializingItemStatusControls)
            {
                return;
            }

            ApplyItemStatusUiState();
        }

        private void ResetItemStatusEditor()
        {
            ApplyItemStatusSnapshot(CreateDefaultItemStatusRule());
        }

        private ItemStatusRuleSnapshot CreateDefaultItemStatusRule()
        {
            return new ItemStatusRuleSnapshot
            {
                StatusName = ItemStatusActive,
                Reason = string.Empty,
                StatusDate = DateTime.Today,
                BlockSale = false,
                BlockPurchase = false
            };
        }

        private string NormalizeItemStatusName(string statusName)
        {
            if (!string.IsNullOrWhiteSpace(statusName))
            {
                string normalized = availableItemStatuses
                    .FirstOrDefault(status => string.Equals(status, statusName.Trim(), StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(normalized))
                {
                    return normalized;
                }
            }

            return ItemStatusActive;
        }

        private string GetSelectedItemStatus()
        {
            EnsureItemStatusControlsCreated();

            if (cmbItemStatus == null)
            {
                return ItemStatusActive;
            }

            return NormalizeItemStatusName(cmbItemStatus.SelectedItem?.ToString() ?? cmbItemStatus.Text);
        }

        private bool DoesStatusBlockSale(string statusName)
        {
            switch (NormalizeItemStatusName(statusName))
            {
                case ItemStatusInactive:
                case ItemStatusBlockedForSale:
                case ItemStatusDiscontinued:
                    return true;
                default:
                    return false;
            }
        }

        private bool DoesStatusBlockPurchase(string statusName)
        {
            switch (NormalizeItemStatusName(statusName))
            {
                case ItemStatusInactive:
                case ItemStatusBlockedForPurchase:
                case ItemStatusDiscontinued:
                    return true;
                default:
                    return false;
            }
        }

        private bool IsItemStatusReasonRequired(string statusName)
        {
            return !string.Equals(NormalizeItemStatusName(statusName), ItemStatusActive, StringComparison.OrdinalIgnoreCase);
        }

        private void ApplyItemStatusUiState()
        {
            EnsureItemStatusControlsCreated();

            if (cmbItemStatus == null || txtItemStatusReason == null || lblItemStatusReason == null ||
                lblItemStatusSaleRule == null || lblItemStatusPurchaseRule == null)
            {
                return;
            }

            string statusName = GetSelectedItemStatus();
            bool blockSale = DoesStatusBlockSale(statusName);
            bool blockPurchase = DoesStatusBlockPurchase(statusName);
            bool reasonRequired = IsItemStatusReasonRequired(statusName);

            lblItemStatusReason.Text = reasonRequired ? "Reason *" : "Reason";
            txtItemStatusReason.BackColor = reasonRequired ? Color.FromArgb(255, 224, 192) : Color.White;

            lblItemStatusSaleRule.Text = $"Sale: {(blockSale ? "Blocked" : "Allowed")}";
            lblItemStatusSaleRule.ForeColor = blockSale ? Color.Firebrick : Color.ForestGreen;

            lblItemStatusPurchaseRule.Text = $"Purchase: {(blockPurchase ? "Blocked" : "Allowed")}";
            lblItemStatusPurchaseRule.ForeColor = blockPurchase ? Color.Firebrick : Color.ForestGreen;
            UpdateItemStatusButtonState();
        }

        private void ApplyItemStatusSnapshot(ItemStatusRuleSnapshot snapshot)
        {
            EnsureItemStatusControlsCreated();

            if (cmbItemStatus == null || txtItemStatusReason == null || dtpItemStatusDate == null)
            {
                return;
            }

            isInitializingItemStatusControls = true;
            try
            {
                string statusName = NormalizeItemStatusName(snapshot?.StatusName);
                cmbItemStatus.SelectedItem = statusName;
                txtItemStatusReason.Text = snapshot?.Reason ?? string.Empty;

                DateTime statusDate = snapshot != null && snapshot.StatusDate > DateTime.MinValue
                    ? snapshot.StatusDate.Date
                    : DateTime.Today;
                dtpItemStatusDate.Value = statusDate;
            }
            finally
            {
                isInitializingItemStatusControls = false;
            }

            ApplyItemStatusUiState();
        }

        private ItemStatusRuleSnapshot GetCurrentItemStatusRuleSnapshot()
        {
            string statusName = GetSelectedItemStatus();
            return new ItemStatusRuleSnapshot
            {
                StatusName = statusName,
                Reason = txtItemStatusReason?.Text?.Trim() ?? string.Empty,
                StatusDate = dtpItemStatusDate != null ? dtpItemStatusDate.Value.Date : DateTime.Today,
                BlockSale = DoesStatusBlockSale(statusName),
                BlockPurchase = DoesStatusBlockPurchase(statusName)
            };
        }

        private bool ValidateItemStatusInputs()
        {
            ItemStatusRuleSnapshot snapshot = GetCurrentItemStatusRuleSnapshot();
            if (IsItemStatusReasonRequired(snapshot.StatusName) && string.IsNullOrWhiteSpace(snapshot.Reason))
            {
                MessageBox.Show($"Please enter a reason for '{snapshot.StatusName}'.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                OpenItemStatusDialog();
                return false;
            }

            return true;
        }

        private bool EnsureItemStatusStorage()
        {
            if (itemStatusTableEnsured)
            {
                return true;
            }

            try
            {
                ExecuteStoredProcedureScalar(
                    STOREDPROCEDURE.POS_ItemMasterStatusRules,
                    CreateSqlParameter("@_Operation", ItemMasterOperationEnsureStatusStorage));

                itemStatusTableEnsured = true;
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ensuring item status storage: {ex.Message}");
                return false;
            }
        }

        private void LoadItemStatusForItemId(int itemId)
        {
            EnsureItemStatusControlsCreated();

            if (itemId <= 0)
            {
                ResetItemStatusEditor();
                return;
            }

            if (!EnsureItemStatusStorage())
            {
                ResetItemStatusEditor();
                return;
            }

            try
            {
                DataTable statusTable = ExecuteStoredProcedureTable(
                    STOREDPROCEDURE.POS_ItemMasterStatusRules,
                    CreateSqlParameter("@_Operation", ItemMasterOperationGetStatus),
                    CreateSqlParameter("@ItemId", itemId));

                if (statusTable.Rows.Count > 0)
                {
                    DataRow row = statusTable.Rows[0];
                    ItemStatusRuleSnapshot snapshot = new ItemStatusRuleSnapshot
                    {
                        StatusName = row["StatusName"]?.ToString(),
                        Reason = row["StatusReason"] == DBNull.Value ? string.Empty : row["StatusReason"].ToString(),
                        StatusDate = row["StatusDate"] == DBNull.Value
                            ? DateTime.Today
                            : Convert.ToDateTime(row["StatusDate"]),
                        BlockSale = row["BlockSale"] != DBNull.Value && Convert.ToBoolean(row["BlockSale"]),
                        BlockPurchase = row["BlockPurchase"] != DBNull.Value && Convert.ToBoolean(row["BlockPurchase"])
                    };

                    ApplyItemStatusSnapshot(snapshot);
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading item status for ItemId {itemId}: {ex.Message}");
            }

            ResetItemStatusEditor();
        }

        private bool SaveItemStatusForItem(int itemId, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (itemId <= 0)
            {
                return true;
            }

            if (!EnsureItemStatusStorage())
            {
                errorMessage = "Unable to access item status storage.";
                return false;
            }

            try
            {
                ItemStatusRuleSnapshot snapshot = GetCurrentItemStatusRuleSnapshot();
                ExecuteStoredProcedureScalar(
                    STOREDPROCEDURE.POS_ItemMasterStatusRules,
                    CreateSqlParameter("@_Operation", ItemMasterOperationSaveStatus),
                    CreateSqlParameter("@ItemId", itemId),
                    CreateSqlParameter("@CompanyId", Convert.ToInt32(ModelClass.DataBase.CompanyId)),
                    CreateSqlParameter("@BranchId", Convert.ToInt32(ModelClass.DataBase.BranchId)),
                    CreateSqlParameter("@StatusName", snapshot.StatusName),
                    CreateSqlParameter("@StatusReason", string.IsNullOrWhiteSpace(snapshot.Reason) ? (object)DBNull.Value : snapshot.Reason),
                    CreateSqlParameter("@StatusDate", snapshot.StatusDate),
                    CreateSqlParameter("@BlockSale", snapshot.BlockSale),
                    CreateSqlParameter("@BlockPurchase", snapshot.BlockPurchase));

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                System.Diagnostics.Debug.WriteLine($"Error saving item status for ItemId {itemId}: {ex.Message}");
                return false;
            }
        }

        private bool TryPersistItemStatusForCurrentItem(bool showWarning)
        {
            int itemId = ItemMaster != null && ItemMaster.ItemId > 0 ? ItemMaster.ItemId : CurrentItemId;
            if (SaveItemStatusForItem(itemId, out string errorMessage))
            {
                return true;
            }

            if (showWarning)
            {
                MessageBox.Show($"Item saved, but status rules could not be saved.\n\n{errorMessage}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return false;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            //string Params = "ItemMasterGrid";
            //frmUnitDialog unitDialog = new frmUnitDialog(Params);
            //unitDialog.ShowDialog();
        }

        private void btn_ItemLoad_Click(object sender, EventArgs e)
        {
            string Params = "FromItemMaster";
            frmdialForItemMaster item = new frmdialForItemMaster(Params);
            item.ShowDialog();

        }

        private void SetupAutoComplete()
        {
            try
            {
                Repository.Dropdowns drop = new Repository.Dropdowns();

                if (txt_Group != null)
                {
                    var groups = drop.getGroupDDl()?.List?.ToList();
                    if (groups != null)
                    {
                        var groupSource = new AutoCompleteStringCollection();
                        groupSource.AddRange(groups.Select(g => g.GroupName ?? "").Where(s => !string.IsNullOrEmpty(s)).ToArray());
                        TextBox innerTb = txt_Group.Controls.Count > 0 ? txt_Group.Controls[0] as TextBox : null;
                        if (innerTb != null)
                        {
                            innerTb.AutoCompleteCustomSource = groupSource;
                            innerTb.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
                            innerTb.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
                        }
                    }
                }

                if (txt_Category != null)
                {
                    var cats = drop.getCategoryDDl("")?.List?.ToList();
                    if (cats != null)
                    {
                        var catSource = new AutoCompleteStringCollection();
                        catSource.AddRange(cats.Select(c => c.CategoryName ?? "").Where(s => !string.IsNullOrEmpty(s)).ToArray());
                        TextBox innerTb = txt_Category.Controls.Count > 0 ? txt_Category.Controls[0] as TextBox : null;
                        if (innerTb != null)
                        {
                            innerTb.AutoCompleteCustomSource = catSource;
                            innerTb.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
                            innerTb.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
                        }
                    }
                }

                if (txt_ItemType != null)
                {
                    var types = drop.getItemTypeDDl()?.List?.ToList();
                    if (types != null)
                    {
                        var typeSource = new AutoCompleteStringCollection();
                        typeSource.AddRange(types.Select(t => t.ItemType ?? "").Where(s => !string.IsNullOrEmpty(s)).ToArray());
                        TextBox innerTb = txt_ItemType.Controls.Count > 0 ? txt_ItemType.Controls[0] as TextBox : null;
                        if (innerTb != null)
                        {
                            innerTb.AutoCompleteCustomSource = typeSource;
                            innerTb.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
                            innerTb.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
                        }
                    }
                }

                if (txt_Brand != null)
                {
                    var brands = drop.getBrandDDl()?.List?.ToList();
                    if (brands != null)
                    {
                        var brandSource = new AutoCompleteStringCollection();
                        brandSource.AddRange(brands.Select(b => b.BrandName ?? "").Where(s => !string.IsNullOrEmpty(s)).ToArray());
                        TextBox innerTb = txt_Brand.Controls.Count > 0 ? txt_Brand.Controls[0] as TextBox : null;
                        if (innerTb != null)
                        {
                            innerTb.AutoCompleteCustomSource = brandSource;
                            innerTb.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
                            innerTb.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting up autocomplete: {ex.Message}");
            }
        }

        private void frmItemMasterNew_Load(object sender, EventArgs e)
        {
            SetupAutoComplete();
            KeyPreview = true;
            this.KeyDown += frmItemMasterNew_KeyDown;

            // Enforce CAPITAL / UPPERCASE always for txt_description & txt_LocalLanguage
            if (this.txt_description != null)
            {
                this.txt_description.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
                this.txt_description.TextChanged -= MaintainUppercaseTxtDescription;
                this.txt_description.TextChanged += MaintainUppercaseTxtDescription;
            }

            if (this.txt_LocalLanguage != null)
            {
                this.txt_LocalLanguage.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
                this.txt_LocalLanguage.TextChanged -= MaintainUppercaseTxtLocalLanguage;
                this.txt_LocalLanguage.TextChanged += MaintainUppercaseTxtLocalLanguage;
            }

            this.SetupUltraGrid();
            this.GetPriceDesing();
            // this.GetTaxDesing();
            this.GetImagesDesing();
            this.EnsureItemStatusControlsCreated();
            this.EnsureItemStatusStorage();

            // Setup vendor details grid
            this.SetupVendorGrid();

            // Setup Alternative Barcode Grid
            this.SetupAlternativeBarcodeGrid();

            // Ensure ultraGrid2 is properly set up
            this.EnsureVendorGridExists();

            // Connect Ult_Price events
            this.ConnectUltPriceEvents();

            // Attach generic formatting handler to all price fields to strictly enforce 3 decimals
            var priceFields = new Infragistics.Win.UltraWinEditors.UltraTextEditor[]
            {
                this.txt_Retail, this.txt_walkin, this.txt_CEP, this.txt_Mrp, this.txt_CardP, this.txt_SF, this.txt_MinP
            };
            foreach (var field in priceFields)
            {
                if (field != null)
                {
                    field.Leave -= FormatPriceToThreeDecimals; // Prevent double subscription
                    field.Leave += FormatPriceToThreeDecimals;
                }
            }

            // Attach generic formatting handler to all percentage/markup/markdown fields to strictly enforce 2 decimals
            var pctFields = new Infragistics.Win.UltraWinEditors.UltraTextEditor[]
            {
                this.ultraTextEditor16, this.ultraTextEditor15, this.ultraTextEditor14, this.ultraTextEditor13,
                this.ultraTextEditor12, this.ultraTextEditor11, this.ultraTextEditor4, this.ultraTextEditor10,
                this.ultraTextEditor5, this.ultraTextEditor9, this.ultraTextEditor8, this.ultraTextEditor7,
                this.ultraTextEditor6
            };

            // Attach to textBox1 (TextBox, not UltraTextEditor)
            if (this.textBox1 != null)
            {
                this.textBox1.Leave -= FormatPercentageToTwoDecimals;
                this.textBox1.Leave += FormatPercentageToTwoDecimals;
            }

            foreach (var field in pctFields)
            {
                if (field != null)
                {
                    field.Leave -= FormatPercentageToTwoDecimals; // Prevent double subscription
                    field.Leave += FormatPercentageToTwoDecimals;
                }
            }

            // Connect ultraGrid1 events for better behavior
            ultraGrid1.BeforeCellUpdate += UltraGrid1_BeforeCellUpdate;
            ultraGrid1.AfterCellUpdate += UltraGrid1_AfterCellUpdate;
            ultraGrid1.KeyDown += UltraGrid1_KeyDown;
            ultraGrid1.BeforeEnterEditMode += UltraGrid1_BeforeEnterEditMode;
            // Style all ultraPanels
            StyleAllUltraPanels();

            // Apply complete image appearance and theme
            ApplyAppearanceTheme();

            SetupAllGridsGridReportThemeAndFunctionality();

            // Ensure ultraPictureBox7 has transparent background
            if (this.Controls.Find("ultraPictureBox7", true).Length > 0)
            {
                Infragistics.Win.UltraWinEditors.UltraPictureBox pic =
                    (Infragistics.Win.UltraWinEditors.UltraPictureBox)this.Controls.Find("ultraPictureBox7", true)[0];
                pic.BackColor = Color.Transparent;
                pic.BackColorInternal = Color.Transparent;
            }

            // Connect btn_unit click event
            btn_unit.Click += btn_unit_Click;

            // Connect txt_BaseUnit text changed event for automatic synchronization
            if (txt_BaseUnit != null)
            {
                txt_BaseUnit.TextChanged += txt_BaseUnit_TextChanged;
            }

            // Connect btn_Remov_Item click event (Remove Unit)
            if (this.Controls.Find("btn_Remov_Item", true).Length > 0)
            {
                var btnRemoveItem = this.Controls.Find("btn_Remov_Item", true)[0] as Control;
                btnRemoveItem.Click += btn_Remov_Item_Click;
            }

            // Connect btn_Add_UnitIm click event (Add Unit)
            if (this.Controls.Find("btn_Add_UnitIm", true).Length > 0)
            {
                var btnAddUnit = this.Controls.Find("btn_Add_UnitIm", true)[0] as Control;
                btnAddUnit.Click += (s, evt) =>
                {
                    string Params = "ItemMasterGrid";
                    // Pass current item id if available
                    frmUnitDialog unitDialog = new frmUnitDialog(Params, this.CurrentItemId);
                    unitDialog.StartPosition = FormStartPosition.CenterScreen;
                    unitDialog.ShowDialog();

                    // After adding units, ensure they are consistent with base unit
                    if (unitDialog.DialogResult == DialogResult.OK)
                    {
                        SynchronizeAddedUnitsWithBaseUnit();
                    }
                };
            }

            // Remove unwanted popup on description click (ensure no dialog opens)
            // txt_description.Click += txt_description_Click; // disabled

            // Connect btn_Add_Brand click event
            btn_Add_Brand.Click += btn_Add_Brand_Click;

            // Connect btn_Add_Custm click event
            btn_Add_Custm.Click += btn_Add_Custm_Click;

            // Connect btn_Add_ItemIype click event
            btn_Add_ItemIype.Click += btn_Add_ItemIype_Click;

            // Connect btn_Add_Cate click event
            btn_Add_Cate.Click += btn_Add_Cate_Click;

            // Connect btn_Add_Grup click event
            btn_Add_Grup.Click += btn_Add_Grup_Click;

            // Connect btnIemLoad_ById click event
            btnIemLoad_ById.Click += btnIemLoad_ById_Click;

            // Connect button1 click event
            button1.Click += button1_Click;

            // Connect alternative barcode buttons if they exist
            try
            {
                var btn11 = this.Controls.Find("button11", true).FirstOrDefault() as System.Windows.Forms.Button;
                if (btn11 != null) btn11.Click += button11_Click;

                var btn12 = this.Controls.Find("button12", true).FirstOrDefault() as System.Windows.Forms.Button;
                if (btn12 != null) btn12.Click += button12_Click;

                var btn13 = this.Controls.Find("button13", true).FirstOrDefault() as System.Windows.Forms.Button;
                if (btn13 != null) btn13.Click += button11_Click;

                var btn14 = this.Controls.Find("button14", true).FirstOrDefault() as System.Windows.Forms.Button;
                if (btn14 != null) btn14.Click += button12_Click;

                if (button13 != null) button13.Click += button11_Click;
                if (button14 != null) button14.Click += button12_Click;
            }
            catch { }

            // Ensure Save and Update buttons are wired
            if (button3 != null) button3.Click += button3_Click;
            if (btnUpdate != null) btnUpdate.Click += btnUpdate_Click;
            if (button6 != null) button6.Click += button6_Click; // Tax Per dialog

            // Connect button7 click event (Clear button) - find dynamically if needed
            var btn7 = button7 ?? this.Controls.Find("button7", true).FirstOrDefault() as System.Windows.Forms.Button;
            if (btn7 != null)
            {
                btn7.Click += button7_Click;
            }

            // Default to Save mode on fresh load
            if (button3 != null) button3.Visible = true;
            if (btnUpdate != null) btnUpdate.Visible = false;

            // Connect txt_walkin value changed event
            txt_walkin.ValueChanged += txt_walkin_ValueChanged;

            // Connect txt_Retail value changed event
            txt_Retail.ValueChanged += txt_Retail_ValueChanged;

            // Connect txt_TaxAmount TextChanged event for real-time updates
            try
            {
                var txtTaxAmount = this.Controls.Find("txt_TaxAmount", true).FirstOrDefault() as TextBox;
                if (txtTaxAmount != null)
                {
                    txtTaxAmount.TextChanged += txt_TaxAmount_TextChanged;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error wiring txt_TaxAmount TextChanged: {ex.Message}");
            }

            // Also compute markdown on Enter for non-master selling price fields
            if (txt_walkin != null) txt_walkin.KeyDown += SellingPriceField_KeyDown;
            if (txt_CEP != null) txt_CEP.KeyDown += SellingPriceField_KeyDown;
            if (txt_Mrp != null) txt_Mrp.KeyDown += SellingPriceField_KeyDown;
            if (txt_CardP != null) txt_CardP.KeyDown += SellingPriceField_KeyDown;

            // Connect txt_Retail KeyDown event for master field behavior
            txt_Retail.KeyDown += txt_Retail_KeyDown;

            // When user leaves txt_Retail: run all recalculations that were previously in ValueChanged.
            // This way, the caret is never stolen while the user types ? updates happen only on focus loss.
            txt_Retail.Leave += (s, ev) =>
            {
                try
                {
                    RefreshAllUnitPrices();
                    UpdateProfitMarginForField(txt_Retail, ultraTextEditor4);
                    UpdateInclusiveExclusiveTaxDisplay();
                    if (!isLoadingItem && !isUpdatingMarkup) RecalculateMarkupPercentage();
                    RecomputeTaxAmountFromRetailAndTax();
                    NotifyItemMasterChanged();
                }
                catch (Exception ex2) { System.Diagnostics.Debug.WriteLine($"txt_Retail Leave error: {ex2.Message}"); }
            };

            // Connect txt_CEP value changed event
            txt_CEP.ValueChanged += txt_CEP_ValueChanged;
            txt_CEP.Leave += (s, ev) =>
            {
                try { RefreshAllUnitPrices(); UpdateProfitMarginForField(txt_CEP, ultraTextEditor9); CalculateMarkdownFromSellingPrice(txt_CEP, ultraTextEditor15); }
                catch (Exception ex2) { System.Diagnostics.Debug.WriteLine($"txt_CEP Leave error: {ex2.Message}"); }
            };

            // Connect txt_Mrp value changed event
            txt_Mrp.ValueChanged += txt_Mrp_ValueChanged;
            txt_Mrp.Leave += (s, ev) =>
            {
                try { RefreshAllUnitPrices(); UpdateProfitMarginForField(txt_Mrp, ultraTextEditor8); CalculateMarkdownFromSellingPrice(txt_Mrp, ultraTextEditor14); }
                catch (Exception ex2) { System.Diagnostics.Debug.WriteLine($"txt_Mrp Leave error: {ex2.Message}"); }
            };

            // Connect txt_CardP value changed event
            txt_CardP.ValueChanged += txt_CardP_ValueChanged;
            txt_CardP.Leave += (s, ev) =>
            {
                try { RefreshAllUnitPrices(); UpdateProfitMarginForField(txt_CardP, ultraTextEditor7); CalculateMarkdownFromSellingPrice(txt_CardP, ultraTextEditor13); }
                catch (Exception ex2) { System.Diagnostics.Debug.WriteLine($"txt_CardP Leave error: {ex2.Message}"); }
            };

            // txt_walkin Leave: run recalculations deferred from ValueChanged
            txt_walkin.Leave += (s, ev) =>
            {
                try { RefreshAllUnitPrices(); UpdateProfitMarginForField(txt_walkin, ultraTextEditor10); CalculateMarkdownFromSellingPrice(txt_walkin, ultraTextEditor16); RecomputeTaxAmountFromRetailAndTax(); NotifyItemMasterChanged(); }
                catch (Exception ex2) { System.Diagnostics.Debug.WriteLine($"txt_walkin Leave error: {ex2.Message}"); }
            };

            // Connect txt_SF and txt_MinP events if they exist (support any Control type)
            var txt_SF = this.Controls.Find("txt_SF", true).FirstOrDefault() as Control;
            if (txt_SF != null)
            {
                txt_SF.TextChanged += txt_SF_TextChanged;
                txt_SF.Enter += (s, e2) => { isEditingStaffPrice = true; };
                txt_SF.Leave += (s, e2) => { isEditingStaffPrice = false; SyncStaffPriceToPriceGridFromTxtSF(txt_SF.Text); };
                txt_SF.KeyDown += (s, e2) =>
                {
                    var ke = e2 as KeyEventArgs;
                    if (ke != null && ke.KeyCode == Keys.Enter)
                    {
                        SyncStaffPriceToPriceGridFromTxtSF(txt_SF.Text);
                        ke.Handled = true; ke.SuppressKeyPress = true;
                    }
                };
            }

            var txt_MinP = this.Controls.Find("txt_MinP", true).FirstOrDefault() as Control;
            if (txt_MinP != null)
            {
                txt_MinP.TextChanged += txt_MinP_TextChanged;
                txt_MinP.Enter += (s, e2) => { isEditingMinPrice = true; };
                txt_MinP.Leave += (s, e2) => { isEditingMinPrice = false; SyncMinPriceToPriceGridFromTxtMinP(txt_MinP.Text); };
                txt_MinP.KeyDown += (s, e2) =>
                {
                    var ke = e2 as KeyEventArgs;
                    if (ke != null && ke.KeyCode == Keys.Enter)
                    {
                        SyncMinPriceToPriceGridFromTxtMinP(txt_MinP.Text);
                        ke.Handled = true; ke.SuppressKeyPress = true;
                    }
                };
            }

            // In frmItemMasterNew_Load, after SetupUltraGrid();
            this.SetupRowFooter();

            // Sync changes of markup textbox with calculator when edited by user
            if (textBox1 != null)
            {
                textBox1.TextChanged += textBox1_TextChanged;
                // Apply markup to compute selling price and margins when user presses Enter
                textBox1.KeyDown += textBox1_KeyDown;
            }

            // Sync profit margin changes initiated by the user
            if (ultraTextEditor4 != null)
            {
                ultraTextEditor4.TextChanged += ultraTextEditor4_TextChanged;
                ultraTextEditor4.KeyDown += ultraTextEditor4_KeyDown;
            }

            // Hook Enter-only handlers for markdown calculation (calculate once per value)
            if (ultraTextEditor16 != null) ultraTextEditor16.KeyDown += MarkdownEditor_KeyDown;
            if (ultraTextEditor15 != null) ultraTextEditor15.KeyDown += MarkdownEditor_KeyDown;
            if (ultraTextEditor14 != null) ultraTextEditor14.KeyDown += MarkdownEditor_KeyDown;
            if (ultraTextEditor13 != null) ultraTextEditor13.KeyDown += MarkdownEditor_KeyDown;
            var ultraTextEditor12 = this.Controls.Find("ultraTextEditor12", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
            var ultraTextEditor11 = this.Controls.Find("ultraTextEditor11", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
            if (ultraTextEditor12 != null)
            {
                ultraTextEditor12.KeyDown += MarkdownEditor_KeyDown;
                ultraTextEditor12.Enter += (s, e2) => { isEditingMdStaff = true; };
                ultraTextEditor12.Leave += (s, e2) => { isEditingMdStaff = false; };
                ultraTextEditor12.TextChanged += (s, e2) =>
                {
                    // Recompute selling price txt_SF from master retail when markdown changes
                    var txt_SF_ctrl = this.Controls.Find("txt_SF", true).FirstOrDefault() as Control;
                    if (txt_Retail != null && txt_SF_ctrl != null)
                    {
                        // Avoid overriding while user is typing in markdown field to prevent caret jumping
                        if (!isEditingMdStaff)
                            SetPriceFromMasterConsideringMarkdown(txt_SF_ctrl, ultraTextEditor12, txt_Retail.Text);
                    }
                };
            }
            if (ultraTextEditor11 != null)
            {
                ultraTextEditor11.KeyDown += MarkdownEditor_KeyDown;
                ultraTextEditor11.Enter += (s, e2) => { isEditingMdMin = true; };
                ultraTextEditor11.Leave += (s, e2) => { isEditingMdMin = false; };
                ultraTextEditor11.TextChanged += (s, e2) =>
                {
                    // Recompute selling price txt_MinP from master retail when markdown changes
                    var txt_MinP_ctrl = this.Controls.Find("txt_MinP", true).FirstOrDefault() as Control;
                    if (txt_Retail != null && txt_MinP_ctrl != null)
                    {
                        // Avoid overriding while user is typing in markdown field to prevent caret jumping
                        if (!isEditingMdMin)
                            SetPriceFromMasterConsideringMarkdown(txt_MinP_ctrl, ultraTextEditor11, txt_Retail.Text);
                    }
                };
            }

            // Hook Enter-only handlers for profit margin calculation (calculate selling price and markdown)
            if (ultraTextEditor10 != null) ultraTextEditor10.KeyDown += ProfitMarginEditor_KeyDown;
            if (ultraTextEditor9 != null) ultraTextEditor9.KeyDown += ProfitMarginEditor_KeyDown;
            if (ultraTextEditor8 != null) ultraTextEditor8.KeyDown += ProfitMarginEditor_KeyDown;
            if (ultraTextEditor7 != null) ultraTextEditor7.KeyDown += ProfitMarginEditor_KeyDown;
            if (ultraTextEditor6 != null) ultraTextEditor6.KeyDown += ProfitMarginEditor_KeyDown;
            // Also attach by control name to ensure handler wires even if field refs are null
            try
            {
                var u6 = this.Controls.Find("ultraTextEditor6", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                if (u6 != null) u6.KeyDown += ProfitMarginEditor_KeyDown;
            }
            catch { }
            if (ultraTextEditor5 != null) ultraTextEditor5.KeyDown += ProfitMarginEditor_KeyDown;
            try
            {
                var u5 = this.Controls.Find("ultraTextEditor5", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                if (u5 != null) u5.KeyDown += ProfitMarginEditor_KeyDown;
            }
            catch { }

            // Connect txt_barcode TextChanged event for auto-generating item number
            var txtBarcodeForNewItem = GetMainBarcodeEditor();
            if (txtBarcodeForNewItem != null)
            {
                txtBarcodeForNewItem.TextChanged += txt_barcode_TextChanged;
                WireBarcodeRefreshMouseEvents(txtBarcodeForNewItem);
                BeginInvoke((MethodInvoker)delegate { WireBarcodeRefreshMouseEvents(txtBarcodeForNewItem); });
            }

            if (Txt_UnitCost != null)
            {
                WireUnitCostRefreshMouseEvents(Txt_UnitCost);
                BeginInvoke((MethodInvoker)delegate { WireUnitCostRefreshMouseEvents(Txt_UnitCost); });
            }

            // Ensure grid MarginPer reflects master profit margin at startup
            SyncUltPriceMarginPerFromMaster();

            // Setup dropdown for Tax Type (incl/excl) overlaying txt_TaxType
            try
            {
                if (txt_TaxType != null && txt_TaxType.Parent != null)
                {
                    if (comboTaxType == null)
                    {
                        comboTaxType = new ComboBox();
                        comboTaxType.DropDownStyle = ComboBoxStyle.DropDownList;
                        comboTaxType.Items.Clear();
                        comboTaxType.Items.Add("incl");
                        comboTaxType.Items.Add("excl");
                        comboTaxType.Width = txt_TaxType.Width;
                        comboTaxType.Height = txt_TaxType.Height;
                        comboTaxType.Left = txt_TaxType.Left;
                        comboTaxType.Top = txt_TaxType.Top;
                        comboTaxType.Anchor = txt_TaxType.Anchor;
                        comboTaxType.TabIndex = txt_TaxType.TabIndex;
                        comboTaxType.Font = txt_TaxType.Font;

                        // Initialize selection from existing text
                        string initial = (txt_TaxType.Text ?? string.Empty).Trim();
                        int idx = initial.IndexOf("incl", StringComparison.OrdinalIgnoreCase) >= 0 ?
                                comboTaxType.Items.IndexOf("incl") :
                                (initial.IndexOf("excl", StringComparison.OrdinalIgnoreCase) >= 0 ? comboTaxType.Items.IndexOf("excl") : -1);
                        comboTaxType.SelectedIndex = idx >= 0 ? idx : 0; // default to incl

                        // Keep TextBox hidden but synchronized
                        comboTaxType.SelectedIndexChanged += (s, evt) =>
                        {
                            try
                            {
                                string sel = Convert.ToString(comboTaxType.SelectedItem) ?? "";
                                if (!string.Equals(txt_TaxType.Text, sel, StringComparison.OrdinalIgnoreCase))
                                {
                                    txt_TaxType.Text = sel;
                                }

                                // Recompute tax display based on new mode
                                UpdateInclusiveExclusiveTaxDisplay();

                                // Notify other forms of real-time change
                                NotifyItemMasterChanged();
                            }
                            catch { }
                        };

                        // Also sync if code changes txt_TaxType.Text (e.g., when loading item)
                        txt_TaxType.TextChanged += (s, evt) =>
                        {
                            try
                            {
                                string text = (txt_TaxType.Text ?? string.Empty).Trim();
                                int want = text.IndexOf("incl", StringComparison.OrdinalIgnoreCase) >= 0 ? 0 :
                                        (text.IndexOf("excl", StringComparison.OrdinalIgnoreCase) >= 0 ? 1 : comboTaxType.SelectedIndex);
                                if (want >= 0 && want < comboTaxType.Items.Count && comboTaxType.SelectedIndex != want)
                                {
                                    comboTaxType.SelectedIndex = want;
                                }
                            }
                            catch { }
                        };

                        // Add to same parent and hide textbox
                        txt_TaxType.Parent.Controls.Add(comboTaxType);
                        comboTaxType.BringToFront();
                        txt_TaxType.Visible = false;
                    }
                }
            }
            catch { }

            // Initialize picture box image behavior and context menu
            InitializePictureBoxImageFeatures();

            // Sync txt_barcode changes to ultraGrid1 barcode cell for new items
            try
            {
                var txtBarcodeCtrl = GetMainBarcodeEditor();
                if (txtBarcodeCtrl != null)
                {
                    txtBarcodeCtrl.TextChanged += txt_barcode_TextChanged;
                    WireBarcodeRefreshMouseEvents(txtBarcodeCtrl);
                    BeginInvoke((MethodInvoker)delegate { WireBarcodeRefreshMouseEvents(txtBarcodeCtrl); });

                    // Also sync when barcode text field loses focus to ensure grid is updated
                    txtBarcodeCtrl.LostFocus += txt_barcode_LostFocus;
                }
            }
            catch { }

            // Subscribe to FrmPurchase price update event to refresh price grid in real-time
            try
            {
                PosBranch_Win.Transaction.FrmPurchase.OnPriceSettingsUpdated += OnPriceSettingsUpdatedHandler;
                // Unsubscribe when form closes to prevent memory leaks
                this.FormClosed += (s, args) =>
                {
                    try
                    {
                        PosBranch_Win.Transaction.FrmPurchase.OnPriceSettingsUpdated -= OnPriceSettingsUpdatedHandler;
                    }
                    catch { }
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error subscribing to price update event: {ex.Message}");
            }

            // Setup Enter key focus navigation for specific fields only
            SetupEnterKeyFocusNavigation();

            // Set initial focus on txt_barcode when form loads or becomes active
            this.Shown += (s, args) => FocusBarcodeBox();
            this.Activated += (s, args) => FocusBarcodeBox();
            this.Enter += (s, args) => FocusBarcodeBox();
            this.VisibleChanged += (s, args) => { if (this.Visible) FocusBarcodeBox(); };
        }

        private void MaintainUppercaseTxtDescription(object sender, EventArgs e)
        {
            try
            {
                if (txt_description != null && !string.IsNullOrEmpty(txt_description.Text))
                {
                    string upper = txt_description.Text.ToUpper();
                    if (txt_description.Text != upper)
                    {
                        int selStart = txt_description.SelectionStart;
                        int selLen = txt_description.SelectionLength;
                        txt_description.Text = upper;
                        txt_description.SelectionStart = selStart;
                        txt_description.SelectionLength = selLen;
                    }
                }
            }
            catch { }
        }

        private void MaintainUppercaseTxtLocalLanguage(object sender, EventArgs e)
        {
            try
            {
                if (txt_LocalLanguage != null && !string.IsNullOrEmpty(txt_LocalLanguage.Text))
                {
                    string upper = txt_LocalLanguage.Text.ToUpper();
                    if (txt_LocalLanguage.Text != upper)
                    {
                        int selStart = txt_LocalLanguage.SelectionStart;
                        int selLen = txt_LocalLanguage.SelectionLength;
                        txt_LocalLanguage.Text = upper;
                        txt_LocalLanguage.SelectionStart = selStart;
                        txt_LocalLanguage.SelectionLength = selLen;
                    }
                }
            }
            catch { }
        }

        private void BarcodeCtrl_ClickToRefresh(object sender, EventArgs e)
        {
            try
            {
                if ((DateTime.Now - lastBarcodeRefreshClickTime).TotalMilliseconds < 300)
                {
                    return;
                }
                lastBarcodeRefreshClickTime = DateTime.Now;

                // 1. Refresh autocomplete dropdowns (Category, Group, Brand, ItemType) from DB
                SetupAutoComplete();

                // 2. Check if current barcode text in txt_barcode matches an item in DB FIRST
                int itemIdToRefresh = 0;
                var txtBarcodeCtrl = GetMainBarcodeEditor();
                string barcodeText = txtBarcodeCtrl?.Text?.Trim() ?? txt_barcode?.Text?.Trim();

                if (!string.IsNullOrWhiteSpace(barcodeText))
                {
                    ItemMasterRepository itemRepo = new ItemMasterRepository();
                    itemIdToRefresh = itemRepo.GetItemIdByBarcode(barcodeText);
                    if (itemIdToRefresh <= 0)
                    {
                        try { itemIdToRefresh = itemRepo.GetItemIdByAliasBarcode(barcodeText); } catch { }
                    }
                    if (itemIdToRefresh <= 0)
                    {
                        try { itemIdToRefresh = itemRepo.GetItemIdByAlternativeBarcode(barcodeText); } catch { }
                    }
                }

                // Fallback to loaded item ID if barcode lookup produced no result
                if (itemIdToRefresh <= 0)
                {
                    if (ItemMaster != null && ItemMaster.ItemId > 0)
                    {
                        itemIdToRefresh = ItemMaster.ItemId;
                    }
                    else if (CurrentItemId > 0)
                    {
                        itemIdToRefresh = CurrentItemId;
                    }
                    else if (!string.IsNullOrEmpty(txt_ItemNo?.Text))
                    {
                        int itemNo = 0;
                        if (int.TryParse(txt_ItemNo.Text, out itemNo) && itemNo > 0)
                        {
                            ItemMasterRepository itemRepo = new ItemMasterRepository();
                            itemIdToRefresh = itemRepo.NavigateItem("CURRENT", itemNo);
                        }
                    }
                }

                // 3. Re-fetch and update/refresh complete item master form (stock levels, prices, UOM, status)
                if (itemIdToRefresh > 0)
                {
                    LoadItemById(itemIdToRefresh);
                    System.Diagnostics.Debug.WriteLine($"Barcode click/focus refreshed item master form completely for ItemId: {itemIdToRefresh}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in BarcodeCtrl_ClickToRefresh: {ex.Message}");
            }
        }

        /// <summary>
        /// Public helper to focus and select all text in txt_barcode
        /// </summary>
        public void FocusBarcodeBox()
        {
            try
            {
                this.BeginInvoke(new Action(() =>
                {
                    if (txt_barcode != null)
                    {
                        txt_barcode.Focus();
                        txt_barcode.SelectAll();
                    }
                }));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FocusBarcodeBox error: {ex.Message}");
            }
        }

        private void FormatPriceToThreeDecimals(object sender, EventArgs e)
        {
            if (sender is Infragistics.Win.UltraWinEditors.UltraTextEditor editor)
            {
                if (decimal.TryParse(editor.Text, out decimal val))
                {
                    editor.Text = val.ToString("0.000");
                }
                else if (string.IsNullOrWhiteSpace(editor.Text))
                {
                    editor.Text = "0.000";
                }
            }
            else if (sender is TextBox textBox)
            {
                if (decimal.TryParse(textBox.Text, out decimal val))
                {
                    textBox.Text = val.ToString("0.000");
                }
                else if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = "0.000";
                }
            }
        }

        private void FormatPercentageToTwoDecimals(object sender, EventArgs e)
        {
            if (sender is Infragistics.Win.UltraWinEditors.UltraTextEditor editor)
            {
                if (decimal.TryParse(editor.Text, out decimal val))
                {
                    editor.Text = val.ToString("0.00");
                }
                else if (string.IsNullOrWhiteSpace(editor.Text))
                {
                    editor.Text = "0.00";
                }
            }
            else if (sender is TextBox textBox)
            {
                if (decimal.TryParse(textBox.Text, out decimal val))
                {
                    textBox.Text = val.ToString("0.00");
                }
                else if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = "0.00";
                }
            }
        }

        /// <summary>
        /// Sets up Enter key focus navigation for specific fields only.
        /// Navigation sequence: txt_barcode ? txt_description ? txt_LocalLanguage ? Txt_UnitCost ? txt_Retail
        /// </summary>
        private void SetupEnterKeyFocusNavigation()
        {
            // Setup markdown editors Enter key sequence (16 -> 15 -> 14 -> 13 -> 12 -> 11)
            var mdEditors = new Infragistics.Win.UltraWinEditors.UltraTextEditor[]
            {
                this.ultraTextEditor16, this.ultraTextEditor15, this.ultraTextEditor14,
                this.ultraTextEditor13, this.ultraTextEditor12, this.ultraTextEditor11
            };

            for (int i = 0; i < mdEditors.Length; i++)
            {
                var currentEditor = mdEditors[i];
                var nextEditor = (i + 1 < mdEditors.Length) ? mdEditors[i + 1] : null;

                if (currentEditor != null)
                {
                    currentEditor.KeyDown += (s, e) =>
                    {
                        if (e.KeyCode == Keys.Enter)
                        {
                            e.Handled = true;
                            e.SuppressKeyPress = true;

                            // Format current editor
                            FormatPercentageToTwoDecimals(currentEditor, EventArgs.Empty);

                            // Move focus and select text
                            if (nextEditor != null)
                            {
                                nextEditor.Focus();
                                nextEditor.SelectAll();
                            }
                            else
                            {
                                currentEditor.SelectAll();
                            }
                        }
                    };
                }
            }

            // Field 1: txt_barcode - on Enter, search for item by barcode and load it
            // If barcode is found in PriceSettings (BarCode or AliasBarcode), load the complete item
            // If not found, go to txt_description
            if (txt_barcode != null)
            {
                txt_barcode.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        e.Handled = true;
                        e.SuppressKeyPress = true;

                        // Get the barcode from the textbox
                        string barcode = txt_barcode.Text?.Trim();

                        if (!string.IsNullOrWhiteSpace(barcode))
                        {
                            try
                            {
                                // Search for ItemId by barcode in PriceSettings
                                ItemMasterRepository itemRepo = new ItemMasterRepository();
                                int itemId = itemRepo.GetItemIdByBarcode(barcode);

                                // If not found by regular barcode, try searching by AliasBarcode
                                if (itemId <= 0)
                                {
                                    try
                                    {
                                        itemId = itemRepo.GetItemIdByAliasBarcode(barcode);
                                        if (itemId > 0)
                                        {
                                            System.Diagnostics.Debug.WriteLine($"Found item ID {itemId} for alias barcode '{barcode}'");
                                        }
                                    }
                                    catch (MissingMethodException)
                                    {
                                        // Method not available - skip alias barcode search
                                        System.Diagnostics.Debug.WriteLine("GetItemIdByAliasBarcode method not found. Rebuild Repository.");
                                    }
                                }

                                // If still not found, try searching by Alternative Barcode (ultraGrid3)
                                if (itemId <= 0)
                                {
                                    try
                                    {
                                        itemId = itemRepo.GetItemIdByAlternativeBarcode(barcode);
                                        if (itemId > 0)
                                        {
                                            System.Diagnostics.Debug.WriteLine($"Found item ID {itemId} for alternative barcode '{barcode}'");
                                        }
                                    }
                                    catch (MissingMethodException)
                                    {
                                        System.Diagnostics.Debug.WriteLine("GetItemIdByAlternativeBarcode method not found. Rebuild Repository.");
                                    }
                                }

                                if (itemId > 0)
                                {
                                    // Found the item! Load it completely
                                    LoadItemById(itemId);
                                    return; // Don't navigate to next field after loading item
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"No item found for barcode/alias barcode '{barcode}'");
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error searching barcode: {ex.Message}");
                            }
                        }

                        // If no barcode or item not found, navigate to txt_description
                        if (txt_description != null)
                            txt_description.Focus();
                    }
                };
            }

            // Field 2: txt_description - on Enter, go to txt_LocalLanguage
            if (txt_description != null)
            {
                txt_description.KeyDown -= txt_BaseUnit_KeyDown; // Remove old handler
                txt_description.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        if (txt_LocalLanguage != null)
                            txt_LocalLanguage.Focus();
                    }
                };
            }

            // Field 3: txt_LocalLanguage - on Enter, go to Txt_UnitCost
            if (txt_LocalLanguage != null)
            {
                txt_LocalLanguage.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        if (Txt_UnitCost != null)
                            Txt_UnitCost.Focus();
                    }
                };
            }

            // Field 4: Txt_UnitCost - on Enter, go to txt_Retail
            if (Txt_UnitCost != null)
            {
                Txt_UnitCost.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        if (txt_Retail != null)
                            txt_Retail.Focus();
                    }
                };
            }

            // Field 5: txt_Retail → txt_walkin → txt_CEP → txt_Mrp → txt_CardP → txt_SF → txt_MinP
            // Each field formats its value to .000 on Enter and moves focus to the next field.
            void WireEnterFocusForPriceField(
                Infragistics.Win.UltraWinEditors.UltraTextEditor current,
                Control next)
            {
                if (current == null) return;
                current.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        // Format current value to 3 decimal places
                        if (float.TryParse(current.Text, out float v))
                            current.Text = v.ToString("0.000");
                        if (next != null)
                        {
                            next.Focus();
                            if (next is Infragistics.Win.UltraWinEditors.UltraTextEditor ute)
                                ute.SelectAll();
                        }
                    }
                };
                // Also format on Leave
                current.Leave += (s, e) =>
                {
                    if (float.TryParse(current.Text, out float v))
                        current.Text = v.ToString("0.000");
                };
            }

            // Find txt_SF and txt_MinP — they are UltraTextEditor, so cast as Control
            var txtSFCtrl = this.Controls.Find("txt_SF", true).FirstOrDefault() as Control;
            var txtMinPCtrl = this.Controls.Find("txt_MinP", true).FirstOrDefault() as Control;

            // Helper: focus a Control and select all its text
            void FocusAndSelect(Control ctrl)
            {
                if (ctrl == null) return;
                ctrl.Focus();
                // SelectAll works for both UltraTextEditor and TextBox via dynamic
                try { ((dynamic)ctrl).SelectAll(); } catch { /* no-op if method absent */ }
            }

            // Build the chain: txt_Retail → txt_walkin → txt_CEP → txt_Mrp → txt_CardP → txt_SF → txt_MinP
            WireEnterFocusForPriceField(txt_Retail, txt_walkin);
            WireEnterFocusForPriceField(txt_walkin, txt_CEP);
            WireEnterFocusForPriceField(txt_CEP, txt_Mrp);
            WireEnterFocusForPriceField(txt_Mrp, txt_CardP);

            // txt_CardP → txt_SF
            if (txt_CardP != null)
            {
                txt_CardP.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        if (float.TryParse(txt_CardP.Text, out float v))
                            txt_CardP.Text = v.ToString("0.000");
                        FocusAndSelect(txtSFCtrl);
                    }
                };
                txt_CardP.Leave += (s, e) =>
                {
                    if (float.TryParse(txt_CardP.Text, out float v))
                        txt_CardP.Text = v.ToString("0.000");
                };
            }

            // txt_SF → txt_MinP
            if (txtSFCtrl != null)
            {
                txtSFCtrl.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        if (float.TryParse(txtSFCtrl.Text, out float v))
                            txtSFCtrl.Text = v.ToString("0.000");
                        FocusAndSelect(txtMinPCtrl);
                    }
                };
                txtSFCtrl.Leave += (s, e) =>
                {
                    if (float.TryParse(txtSFCtrl.Text, out float v))
                        txtSFCtrl.Text = v.ToString("0.000");
                };
            }

            // txt_MinP — end of chain
            if (txtMinPCtrl != null)
            {
                txtMinPCtrl.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        if (float.TryParse(txtMinPCtrl.Text, out float v))
                            txtMinPCtrl.Text = v.ToString("0.000");
                        // End of chain — keep focus here, select all for convenience
                        FocusAndSelect(txtMinPCtrl);
                    }
                };
                txtMinPCtrl.Leave += (s, e) =>
                {
                    if (float.TryParse(txtMinPCtrl.Text, out float v))
                        txtMinPCtrl.Text = v.ToString("0.000");
                };
            }
        }


        /// <summary>
        /// Handler for FrmPurchase price update event - refreshes price grid from database if the updated item matches current item
        /// <summary>
        /// Real-time event handler when prices/costs are updated from FrmPurchase
        /// </summary>
        private void OnPriceSettingsUpdatedHandler(int updatedItemId)
        {
            try
            {
                // Only refresh if the updated item matches the currently loaded item
                if (CurrentItemId > 0 && CurrentItemId == updatedItemId)
                {
                    // Use Invoke to ensure we're on the UI thread and reload all form fields (including Txt_UnitCost)
                    if (this.InvokeRequired)
                    {
                        this.Invoke(new Action(() => LoadItemById(updatedItemId)));
                    }
                    else
                    {
                        LoadItemById(updatedItemId);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnPriceSettingsUpdatedHandler: {ex.Message}");
            }
        }

        /// <summary>
        /// Refreshes the Ult_Price grid from database for a specific item
        /// Called when prices are updated from FrmPurchase
        /// </summary>
        private void RefreshPriceGridFromDatabase(int itemId)
        {
            try
            {
                if (itemId <= 0)
                    return;

                // Get the Ult_Price grid
                Infragistics.Win.UltraWinGrid.UltraGrid Ult_Price =
                    this.Controls.Find("Ult_Price", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;

                if (Ult_Price == null)
                    return;

                // Fetch fresh data from database
                ItemGet getItem = ItemRepository.GetByIdItem(itemId);

                if (getItem == null || getItem.List == null || getItem.List.Length == 0)
                    return;

                // Update Txt_UnitCost field with the latest base unit cost from database
                if (Txt_UnitCost != null && getItem.List[0] != null)
                {
                    Txt_UnitCost.Text = getItem.List[0].Cost.ToString("0.000");
                }

                // Create DataTable for Ult_Price with proper column types
                DataTable dtPrice = new DataTable();
                dtPrice.Columns.Add("Unit", typeof(string));
                dtPrice.Columns.Add("Packing", typeof(int));
                dtPrice.Columns.Add("Cost", typeof(float));
                dtPrice.Columns.Add("MarginAmt", typeof(float));
                dtPrice.Columns.Add("MarginPer", typeof(float));
                dtPrice.Columns.Add("TaxPer", typeof(float));
                dtPrice.Columns.Add("TaxAmt", typeof(float));
                dtPrice.Columns.Add("RetailPrice", typeof(float));
                dtPrice.Columns.Add("MRP", typeof(float));
                dtPrice.Columns.Add("WholeSalePrice", typeof(float));
                dtPrice.Columns.Add("CreditPrice", typeof(float));
                dtPrice.Columns.Add("CardPrice", typeof(float));
                dtPrice.Columns.Add("StaffPrice", typeof(float));
                dtPrice.Columns.Add("MinPrice", typeof(float));

                // Add rows from database
                for (int i = 0; i < getItem.List.Length; i++)
                {
                    DataRow row = dtPrice.NewRow();
                    row["Unit"] = getItem.List[i].Unit ?? string.Empty;
                    row["Packing"] = Convert.ToInt32(getItem.List[i].Packing);
                    row["Cost"] = getItem.List[i].Cost;
                    row["MarginAmt"] = getItem.List[i].MarginAmt;
                    row["MarginPer"] = getItem.List[i].MarginPer;
                    row["TaxPer"] = getItem.List[i].TaxPer;
                    row["TaxAmt"] = getItem.List[i].TaxAmt;
                    row["RetailPrice"] = getItem.List[i].WholeSalePrice; // DB.WholeSalePrice = retail ? grid RetailPrice (visual "Retail Price")
                    row["MRP"] = getItem.List[i].MRP;
                    row["WholeSalePrice"] = getItem.List[i].RetailPrice; // DB.RetailPrice = walking ? grid WholeSalePrice (visual "Walking Price")
                    row["CreditPrice"] = getItem.List[i].CreditPrice;
                    row["CardPrice"] = getItem.List[i].CardPrice;
                    row["StaffPrice"] = getItem.List[i].StaffPrice;
                    row["MinPrice"] = getItem.List[i].MinPrice;
                    if (dtPrice.Columns.Contains("AliasBarcode")) row["AliasBarcode"] = getItem.List[i].AliasBarcode ?? string.Empty;
                    dtPrice.Rows.Add(row);
                }

                // Update Ult_Price DataSource
                Ult_Price.DataSource = dtPrice;

                // Format columns
                if (Ult_Price.DisplayLayout.Bands.Count > 0)
                {
                    var band = Ult_Price.DisplayLayout.Bands[0];
                    if (band.Columns.Exists("Cost")) band.Columns["Cost"].Format = "N2";
                    if (band.Columns.Exists("MarginAmt")) band.Columns["MarginAmt"].Format = "N2";
                    if (band.Columns.Exists("MarginPer")) band.Columns["MarginPer"].Format = "N2";
                    if (band.Columns.Exists("TaxPer")) band.Columns["TaxPer"].Format = "N2";
                    if (band.Columns.Exists("TaxAmt")) band.Columns["TaxAmt"].Format = "N2";
                    if (band.Columns.Exists("RetailPrice")) band.Columns["RetailPrice"].Format = "N2";
                    if (band.Columns.Exists("MRP")) band.Columns["MRP"].Format = "N2";
                    if (band.Columns.Exists("WholeSalePrice")) band.Columns["WholeSalePrice"].Format = "N2";
                    if (band.Columns.Exists("CreditPrice")) band.Columns["CreditPrice"].Format = "N2";
                    if (band.Columns.Exists("CardPrice")) band.Columns["CardPrice"].Format = "N2";
                    if (band.Columns.Exists("StaffPrice")) band.Columns["StaffPrice"].Format = "N2";
                    if (band.Columns.Exists("MinPrice")) band.Columns["MinPrice"].Format = "N2";
                }

                Ult_Price.Refresh();

                // Also update the master price text fields from unit 1's prices
                if (getItem.List.Length > 0)
                {
                    // Find unit 1 (base unit) to update master fields
                    var unit1 = getItem.List.FirstOrDefault(u => u.Packing == 1);
                    if (unit1 != null)
                    {
                        // txt_Retail shows WholeSalePrice (DB.WholeSalePrice = retail price)
                        if (txt_Retail != null)
                        {
                            string newRetailStr = unit1.WholeSalePrice.ToString("0.00");
                            // Only trigger the ripple if the value actually changed to avoid unnecessary recalculations
                            if (txt_Retail.Text != newRetailStr)
                            {
                                txt_Retail.Text = newRetailStr;

                                // Act as if the user just typed into txt_Retail and hit Enter
                                // This ripples the price to all other textboxes (walk-in, staff, etc.),
                                // recalculates profit margins, and refreshes the entire unit price grid
                                txt_Retail_KeyDown(null, new KeyEventArgs(Keys.Enter));
                            }
                        }

                        // txt_walkin shows RetailPrice (DB.RetailPrice = walking price)
                        // It may have already been updated by the ripple above, but we set it here 
                        // from DB just in case it doesn't auto-calculate from retail price
                        if (txt_walkin != null)
                        {
                            txt_walkin.Text = unit1.RetailPrice.ToString("0.000");
                        }
                    }
                }

                // Sync with UOM grid
                SyncUomGridWithPriceGrid();

                System.Diagnostics.Debug.WriteLine($"Price grid refreshed from database for ItemId: {itemId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing price grid from database: {ex.Message}");
            }
        }

        // Setup SizeMode and context menu for pictureBoxItem
        private void InitializePictureBoxImageFeatures()
        {
            try
            {
                if (this.pictureBoxItem != null)
                {
                    pictureBoxItem.SizeMode = PictureBoxSizeMode.Zoom; // fit without overfill

                    // Build context menu
                    pictureBoxContextMenu = new ContextMenuStrip();

                    var cutItem = new ToolStripMenuItem("Cut", null, (s, e) => CutCurrentImage()) { Name = "Cut" };
                    var copyItem = new ToolStripMenuItem("Copy", null, (s, e) => CopyCurrentImage()) { Name = "Copy" };
                    var pasteItem = new ToolStripMenuItem("Paste", null, (s, e) => PasteImageFromClipboard()) { Name = "Paste" };
                    var deleteItem = new ToolStripMenuItem("Delete", null, (s, e) => DeleteCurrentImage()) { Name = "Delete" };
                    pictureBoxContextMenu.Items.Add(cutItem);
                    pictureBoxContextMenu.Items.Add(copyItem);
                    pictureBoxContextMenu.Items.Add(pasteItem);
                    pictureBoxContextMenu.Items.Add(deleteItem);
                    pictureBoxContextMenu.Items.Add(new ToolStripSeparator());

                    var loadItem = new ToolStripMenuItem("Load", null, (s, e) => LoadImageFromFile()) { Name = "Load" };
                    var saveItem = new ToolStripMenuItem("Save", null, (s, e) => SaveImageToFile()) { Name = "Save" };
                    pictureBoxContextMenu.Items.Add(loadItem);
                    pictureBoxContextMenu.Items.Add(saveItem);

                    pictureBoxContextMenu.Opening += (s, e) =>
                    {
                        bool hasImage = pictureBoxItem.Image != null && currentImageBytes != null && currentImageBytes.Length > 0;
                        cutItem.Enabled = hasImage;
                        copyItem.Enabled = hasImage;
                        deleteItem.Enabled = hasImage;
                        saveItem.Enabled = hasImage;

                        // Enable paste only if clipboard has an image
                        pasteItem.Enabled = Clipboard.ContainsImage();
                    };

                    pictureBoxItem.ContextMenuStrip = pictureBoxContextMenu;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing pictureBox context menu: {ex.Message}");
            }
        }

        // Helper to set both Image and backing bytes safely
        private void SetCurrentImage(byte[] imageBytes)
        {
            try
            {
                currentImageBytes = imageBytes;

                // Dispose previous image to free file locks
                Image old = pictureBoxItem.Image;
                pictureBoxItem.Image = null;
                if (old != null)
                {
                    old.Dispose();
                }

                if (imageBytes == null || imageBytes.Length == 0)
                {
                    return;
                }

                using (var ms = new MemoryStream(imageBytes))
                {
                    pictureBoxItem.Image = Image.FromStream(ms);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting current image: {ex.Message}");
            }
        }

        // Public method for external callers (e.g., selection dialog) to set the item's photo
        public void SetItemPhoto(byte[] imageBytes)
        {
            SetCurrentImage(imageBytes);
        }

        private void LoadImageFromFile()
        {
            try
            {
                if (openFileDialog1 == null) return;
                openFileDialog1.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                openFileDialog1.Title = "Select an Image";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    byte[] bytes = File.ReadAllBytes(openFileDialog1.FileName);
                    SetCurrentImage(bytes);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading image: {ex.Message}");
            }
        }

        private void SaveImageToFile()
        {
            try
            {
                if (currentImageBytes == null || currentImageBytes.Length == 0) return;
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = "PNG Image|*.png|JPEG Image|*.jpg;*.jpeg|Bitmap Image|*.bmp|GIF Image|*.gif|All Files|*.*";
                    sfd.Title = "Save Image";
                    sfd.FileName = "item_image";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        // If saving as PNG/JPEG/BMP, we can save raw bytes if formats match; safest: recompress from Image
                        using (var ms = new MemoryStream(currentImageBytes))
                        using (var img = Image.FromStream(ms))
                        {
                            var ext = Path.GetExtension(sfd.FileName).ToLowerInvariant();
                            if (ext == ".jpg" || ext == ".jpeg") img.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                            else if (ext == ".bmp") img.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                            else if (ext == ".gif") img.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Gif);
                            else img.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving image: {ex.Message}");
            }
        }

        private void CopyCurrentImage()
        {
            try
            {
                if (pictureBoxItem.Image != null)
                {
                    Clipboard.SetImage(pictureBoxItem.Image);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error copying image: {ex.Message}");
            }
        }

        private void CutCurrentImage()
        {
            try
            {
                if (pictureBoxItem.Image != null)
                {
                    Clipboard.SetImage(pictureBoxItem.Image);
                    DeleteCurrentImage();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cutting image: {ex.Message}");
            }
        }

        private void PasteImageFromClipboard()
        {
            try
            {
                if (Clipboard.ContainsImage())
                {
                    using (var img = Clipboard.GetImage())
                    {
                        using (var ms = new MemoryStream())
                        {
                            img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            SetCurrentImage(ms.ToArray());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error pasting image: {ex.Message}");
            }
        }

        private void DeleteCurrentImage()
        {
            try
            {
                SetCurrentImage(null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting image: {ex.Message}");
            }
        }

        // Public helper to add a UOM row into ultraGrid1's DataSource
        public void AddOrUpdateUomRow(string unitName, int unitId, float packing, float reorder = 5, string barcode = "0", float openStock = 0)
        {
            DataTable dt = ultraGrid1.DataSource as DataTable;
            if (dt == null)
            {
                dt = new DataTable();
                dt.Columns.Add("Unit", typeof(string));
                dt.Columns.Add("UnitId", typeof(string));
                dt.Columns.Add("Packing", typeof(string));
                // dt.Columns.Add("BarCode", typeof(string)); // Removed
                dt.Columns.Add("Reorder", typeof(string));
                dt.Columns.Add("OpnStk", typeof(string));
                EnsureUomGridPriceColumns(dt);
                ultraGrid1.DataSource = dt;
            }
            else
            {
                EnsureUomGridPriceColumns(dt);
            }

            // Check if this is the base unit
            bool isBaseUnit = string.Equals(unitName, txt_BaseUnit?.Text?.Trim(), StringComparison.OrdinalIgnoreCase);

            // If this is the base unit, ensure packing = 1
            if (isBaseUnit)
            {
                packing = 1.0f;
            }

            // CRITICAL: Calculate cost = packing * Txt_UnitCost
            float cost = 0;
            if (!string.IsNullOrWhiteSpace(Txt_UnitCost.Text))
            {
                float unitCost = 0;
                if (float.TryParse(Txt_UnitCost.Text, out unitCost))
                {
                    cost = packing * unitCost;
                }
            }

            // Do not add duplicate Unit if it already exists
            foreach (DataRow existing in dt.Rows)
            {
                if (string.Equals(Convert.ToString(existing["Unit"]), unitName, StringComparison.OrdinalIgnoreCase))
                {
                    existing["UnitId"] = unitId.ToString();
                    existing["Packing"] = packing.ToString();
                    existing["Cost"] = cost; // Update cost!
                    // If a non-empty barcode is provided, update it
                    // if (!string.IsNullOrWhiteSpace(barcode) && barcode != "0")
                    // existing["BarCode"] = barcode;
                    existing["Reorder"] = reorder.ToString();
                    existing["OpnStk"] = openStock.ToString();
                    SyncUomRowWithPriceGrid(existing);
                    ultraGrid1.Refresh();
                    return;
                }
            }

            DataRow row = dt.NewRow();
            row["Unit"] = unitName;
            row["UnitId"] = unitId.ToString();
            row["Packing"] = packing.ToString();
            row["Cost"] = cost; // Set calculated cost!
            // row["BarCode"] = string.IsNullOrWhiteSpace(barcode) ? "0" : barcode;
            row["Reorder"] = reorder.ToString();
            row["OpnStk"] = openStock.ToString();
            dt.Rows.Add(row);
            SyncUomRowWithPriceGrid(row);
            ultraGrid1.DataSource = dt;

            // Ensure base unit is always first row
            EnsureBaseUnitFirstRow();

            ultraGrid1.Refresh();

            System.Diagnostics.Debug.WriteLine($"AddOrUpdateUomRow: Added {unitName} with packing={packing}, cost={cost}");
        }

        // Public helper to add additional units to the grids (for multiple unit support)
        public void AddAdditionalUnit(string unitName, int unitId, float packing, float reorder = 5, string barcode = "0", float openStock = 0)
        {
            // CRITICAL FIX: Add to price grid FIRST so cost is calculated and available
            // Then add to UOM grid - the sync will copy cost from Ult_Price
            AddOrUpdatePriceRowFromBase(unitName, packing);

            // Add to UOM grid - this will sync with the price grid and get the calculated cost
            AddOrUpdateUomRow(unitName, unitId, packing, reorder, barcode, openStock);

            // Ensure base unit is always first row
            EnsureBaseUnitFirstRow();

            System.Diagnostics.Debug.WriteLine($"AddAdditionalUnit: Completed for {unitName}, packing={packing}");
        }

        // Helper method to ensure base unit row (Packing=1) is always in first position of ultraGrid1
        private void EnsureBaseUnitFirstRow()
        {
            try
            {
                DataTable dt = ultraGrid1.DataSource as DataTable;
                if (dt == null || dt.Rows.Count <= 1)
                    return;

                // Find the base unit row (Packing = 1 or closest to 1)
                int baseUnitIndex = -1;
                float minPacking = float.MaxValue;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    float packing = 0;
                    if (float.TryParse(dt.Rows[i][colPacking]?.ToString(), out packing))
                    {
                        // Base unit has Packing = 1
                        if (packing == 1.0f)
                        {
                            baseUnitIndex = i;
                            break;
                        }
                        // Track the smallest packing as fallback
                        if (packing < minPacking)
                        {
                            minPacking = packing;
                            baseUnitIndex = i;
                        }
                    }
                }

                // If base unit is not first row, move it to first position
                if (baseUnitIndex > 0)
                {
                    DataRow baseUnitRow = dt.Rows[baseUnitIndex];
                    object[] itemArray = baseUnitRow.ItemArray;

                    // Create a new row with base unit data and insert at position 0
                    DataRow newRow = dt.NewRow();
                    newRow.ItemArray = itemArray;

                    // Remove the old row and insert new at first position
                    dt.Rows.RemoveAt(baseUnitIndex);
                    dt.Rows.InsertAt(newRow, 0);

                    ultraGrid1.Refresh();
                    System.Diagnostics.Debug.WriteLine($"EnsureBaseUnitFirstRow: Moved base unit from index {baseUnitIndex} to index 0");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in EnsureBaseUnitFirstRow: {ex.Message}");
            }
        }

        // Public helper to add a price row into Ult_Price's DataSource based on first row as base and packing multiplier
        public void AddOrUpdatePriceRowFromBase(string unitName, float packing)
        {
            System.Diagnostics.Debug.WriteLine($"AddOrUpdatePriceRowFromBase: START - unitName={unitName}, packing={packing}, Txt_UnitCost={Txt_UnitCost?.Text}");

            var Ult_Price = this.Controls.Find("Ult_Price", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;
            if (Ult_Price == null)
            {
                System.Diagnostics.Debug.WriteLine("AddOrUpdatePriceRowFromBase: Ult_Price is NULL!");
                return;
            }

            DataTable dt = Ult_Price.DataSource as DataTable;
            if (dt == null)
            {
                System.Diagnostics.Debug.WriteLine("AddOrUpdatePriceRowFromBase: Creating new DataTable for Ult_Price");
                dt = new DataTable();
                dt.Columns.Add("Unit", typeof(string));
                dt.Columns.Add("Packing", typeof(int));
                dt.Columns.Add("Cost", typeof(float));
                dt.Columns.Add("MarginAmt", typeof(float));
                dt.Columns.Add("MarginPer", typeof(float));
                dt.Columns.Add("TaxPer", typeof(float));
                dt.Columns.Add("TaxAmt", typeof(float));
                dt.Columns.Add("RetailPrice", typeof(float));
                dt.Columns.Add("MRP", typeof(float));
                dt.Columns.Add("WholeSalePrice", typeof(float));
                dt.Columns.Add("CreditPrice", typeof(float));
                dt.Columns.Add("CardPrice", typeof(float));
                dt.Columns.Add("StaffPrice", typeof(float));
                dt.Columns.Add("MinPrice", typeof(float));
            }

            System.Diagnostics.Debug.WriteLine($"AddOrUpdatePriceRowFromBase: DataTable has {dt.Rows.Count} existing rows");

            // If row for unit exists, update it instead of duplicate
            foreach (DataRow r in dt.Rows)
            {
                if (string.Equals(Convert.ToString(r["Unit"]), unitName, StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine($"AddOrUpdatePriceRowFromBase: Found existing row for {unitName}, updating packing only");
                    // Only update Packing - do NOT recalculate prices
                    // User-entered price values should be preserved
                    r["Packing"] = Convert.ToInt32(packing);
                    // Note: MultiplyPriceRowFromBase removed to preserve user-entered values
                    Ult_Price.DataSource = dt;
                    Ult_Price.Refresh();
                    SyncUomGridWithPriceGrid();
                    return;
                }
            }

            System.Diagnostics.Debug.WriteLine($"AddOrUpdatePriceRowFromBase: Creating NEW row for {unitName}");
            DataRow newRow = dt.NewRow();
            newRow["Unit"] = unitName;
            newRow["Packing"] = Convert.ToInt32(packing);
            MultiplyPriceRowFromBase(dt, newRow, packing);

            System.Diagnostics.Debug.WriteLine($"AddOrUpdatePriceRowFromBase: After MultiplyPriceRowFromBase, Cost in newRow = {newRow["Cost"]}");

            dt.Rows.Add(newRow);
            Ult_Price.DataSource = dt;
            Ult_Price.Refresh();

            // Verify the cost is in the grid
            foreach (Infragistics.Win.UltraWinGrid.UltraGridRow row in Ult_Price.Rows)
            {
                if (row.Cells.Exists("Unit") && row.Cells.Exists("Cost"))
                {
                    System.Diagnostics.Debug.WriteLine($"AddOrUpdatePriceRowFromBase: Ult_Price row - Unit={row.Cells["Unit"].Value}, Cost={row.Cells["Cost"].Value}");
                }
            }

            SyncUomGridWithPriceGrid();
        }

        private void MultiplyPriceRowFromBase(DataTable dt, DataRow targetRow, float packing)
        {
            // CRITICAL FIX: Calculate cost from Txt_UnitCost instead of base row
            // This ensures new units get the correct cost = packing * Txt_UnitCost
            float baseCost = 0f;
            if (!string.IsNullOrWhiteSpace(Txt_UnitCost.Text))
            {
                float.TryParse(Txt_UnitCost.Text, out baseCost);
            }

            float baseMRP = 0f, baseRetail = 0f, baseWholeSale = 0f, baseCredit = 0f, baseCard = 0f, baseStaff = 0f, baseMin = 0f, baseMarginAmt = 0f, baseMarginPer = 0f, baseTaxPer = 0f, baseTaxAmt = 0f;
            if (dt.Rows.Count > 0)
            {
                DataRow baseRow = dt.Rows[0];
                // Cost is calculated from Txt_UnitCost above, not from base row
                float.TryParse(Convert.ToString(baseRow["MRP"]), out baseMRP);
                float.TryParse(Convert.ToString(baseRow["RetailPrice"]), out baseRetail);
                float.TryParse(Convert.ToString(baseRow["WholeSalePrice"]), out baseWholeSale);
                float.TryParse(Convert.ToString(baseRow["CreditPrice"]), out baseCredit);
                float.TryParse(Convert.ToString(baseRow["CardPrice"]), out baseCard);
                if (dt.Columns.Contains("StaffPrice")) float.TryParse(Convert.ToString(baseRow["StaffPrice"]), out baseStaff);
                if (dt.Columns.Contains("MinPrice")) float.TryParse(Convert.ToString(baseRow["MinPrice"]), out baseMin);
                float.TryParse(Convert.ToString(baseRow["MarginAmt"]), out baseMarginAmt);
                float.TryParse(Convert.ToString(baseRow["MarginPer"]), out baseMarginPer);
                float.TryParse(Convert.ToString(baseRow["TaxPer"]), out baseTaxPer);
                float.TryParse(Convert.ToString(baseRow["TaxAmt"]), out baseTaxAmt);
            }

            targetRow["Cost"] = baseCost * packing; // Uses Txt_UnitCost now, not base row!
            targetRow["MRP"] = baseMRP * packing;
            targetRow["RetailPrice"] = baseRetail * packing;
            targetRow["WholeSalePrice"] = baseWholeSale * packing;
            targetRow["CreditPrice"] = baseCredit * packing;
            targetRow["CardPrice"] = baseCard * packing;
            if (dt.Columns.Contains("StaffPrice")) targetRow["StaffPrice"] = baseStaff * packing;
            if (dt.Columns.Contains("MinPrice")) targetRow["MinPrice"] = baseMin * packing;
            targetRow["MarginAmt"] = baseMarginAmt * packing;
            targetRow["MarginPer"] = baseMarginPer; // percentage stays same
            targetRow["TaxPer"] = baseTaxPer;       // percentage stays same
            targetRow["TaxAmt"] = baseTaxAmt * packing;

            System.Diagnostics.Debug.WriteLine($"MultiplyPriceRowFromBase: Set cost to {baseCost * packing} (Txt_UnitCost={baseCost} * packing={packing})");
        }

        // Add the BeforeCellUpdate event handler
        private void UltraGrid1_BeforeCellUpdate(object sender, BeforeCellUpdateEventArgs e)
        {
            // Save original value for comparison
            object originalValue = e.Cell.Value;

            // Handle cell update logic as needed
            if (e.Cell.Column.Key == colPacking || e.Cell.Column.Key == colReorder || e.Cell.Column.Key == colOpenStock)
            {
                // Validate the value for numeric columns
                float value;
                if (!float.TryParse(e.NewValue.ToString(), out value))
                {
                    // If invalid, cancel the update
                    e.Cancel = true;
                    return;
                }
            }
        }

        // Add the AfterCellUpdate event handler
        private void UltraGrid1_AfterCellUpdate(object sender, CellEventArgs e)
        {
            try
            {
                // Prevent recursive calls when we're updating cells programmatically
                if (isUpdatingCostCell) return;

                // Auto-calculate Cost when Packing changes: Cost = Packing ? Txt_UnitCost
                if (e.Cell.Column.Key == colPacking)
                {
                    // Get the packing value
                    float packing = 0;
                    if (e.Cell.Value != null && e.Cell.Value != DBNull.Value)
                    {
                        float.TryParse(e.Cell.Value.ToString(), out packing);
                    }

                    // Get unit cost from Txt_UnitCost
                    float unitCost = 0;
                    if (!string.IsNullOrWhiteSpace(Txt_UnitCost.Text))
                    {
                        float.TryParse(Txt_UnitCost.Text, out unitCost);
                    }

                    // Calculate cost
                    float cost = packing * unitCost;

                    // Update Cost cell in the same row (with guard flag to prevent recursion)
                    if (e.Cell.Row.Cells.Exists("Cost"))
                    {
                        isUpdatingCostCell = true;
                        try
                        {
                            e.Cell.Row.Cells["Cost"].Value = cost;

                            // CRITICAL: Sync ALL fields (including cost) to Ult_Price grid so they get saved
                            SyncCostToPriceGrid(e.Cell.Row);
                        }
                        finally
                        {
                            isUpdatingCostCell = false;
                        }
                    }
                }
                // Real-time propagation: When RetailPrice changes, copy to other price cells
                else if (e.Cell.Column.Key == "RetailPrice")
                {
                    // Get the RetailPrice value
                    float retailPrice = 0;
                    if (e.Cell.Value != null && e.Cell.Value != DBNull.Value)
                    {
                        float.TryParse(e.Cell.Value.ToString(), out retailPrice);
                    }

                    // Copy to all other price cells in the same row in real-time
                    string[] priceCols = { "MRP", "WholeSalePrice", "CreditPrice", "CardPrice", "StaffPrice", "MinPrice" };
                    foreach (string colKey in priceCols)
                    {
                        if (e.Cell.Row.Cells.Exists(colKey))
                        {
                            e.Cell.Row.Cells[colKey].Value = retailPrice;
                        }
                    }

                    // Sync all price fields to Ult_Price so they get saved
                    SyncCostToPriceGrid(e.Cell.Row);
                }
                // Sync to Ult_Price when any other price field is updated
                else if (e.Cell.Column.Key == "MRP" ||
                         e.Cell.Column.Key == "WholeSalePrice" ||
                         e.Cell.Column.Key == "CreditPrice" ||
                         e.Cell.Column.Key == "CardPrice" ||
                         e.Cell.Column.Key == "StaffPrice" ||
                         e.Cell.Column.Key == "MinPrice" ||
                         e.Cell.Column.Key == "Cost")
                {
                    // Sync all price fields to Ult_Price so they get saved
                    SyncCostToPriceGrid(e.Cell.Row);
                }
                // Update related fields based on cell changes
                else if (e.Cell.Column.Key == colOpenStock || e.Cell.Column.Key == colReorder)
                {
                    // For numeric columns, format the display value
                    if (e.Cell.Value != null)
                    {
                        float value;
                        if (float.TryParse(e.Cell.Value.ToString(), out value))
                        {
                            // Keep the value as is, just make sure it's properly displayed
                            e.Cell.Value = value;
                        }
                    }
                }

                // Note: Barcode cell in ultraGrid1 acts as independent alias barcode
                // No synchronization with txt_barcode when barcode cell is edited
                // Note: Barcode cell removed from ultraGrid1
                // logic related to colBarcode removed
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UltraGrid1_AfterCellUpdate: {ex.Message}");
            }
        }

        // Add KeyDown event handler for ultraGrid1
        private void UltraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            UltraGrid grid = sender as UltraGrid;
            if (grid == null)
            {
                return;
            }

            // Handle key press events
            if (e.KeyCode == Keys.Enter)
            {
                if (TryNavigateUltraGridCell(grid, true))
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.Right)
            {
                if (TryNavigateUltraGridCell(grid, true))
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.Left)
            {
                if (TryNavigateUltraGridCell(grid, false))
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                // Handle Delete key to remove selected row
                RemoveSelectedUnitFromGrid();
                e.Handled = true;
            }
        }

        private bool TryNavigateUltraGridCell(UltraGrid grid, bool moveForward)
        {
            if (grid?.ActiveCell == null || grid.ActiveRow == null)
            {
                return false;
            }

            try
            {
                if (grid.ActiveCell.IsInEditMode)
                {
                    grid.PerformAction(UltraGridAction.ExitEditMode);
                }
            }
            catch
            {
                // Continue navigation even if edit-mode exit is not available.
            }

            UltraGridCell targetCell = moveForward
                ? FindNextEditableCell(grid.ActiveRow, grid.ActiveCell)
                : FindPreviousEditableCell(grid.ActiveRow, grid.ActiveCell);

            if (targetCell == null)
            {
                int adjacentRowIndex = grid.ActiveRow.Index + (moveForward ? 1 : -1);
                if (adjacentRowIndex >= 0 && adjacentRowIndex < grid.Rows.Count)
                {
                    UltraGridRow adjacentRow = grid.Rows[adjacentRowIndex];
                    targetCell = moveForward
                        ? FindFirstEditableCell(adjacentRow)
                        : FindLastEditableCell(adjacentRow);
                }
            }

            if (targetCell == null)
            {
                return false;
            }

            grid.ActiveRow = targetCell.Row;
            grid.ActiveCell = targetCell;
            grid.PerformAction(UltraGridAction.EnterEditMode);
            return true;
        }

        // Prevent editing the first row (base unit / 1 UNIT) in ultraGrid1, except for AliasBarcode
        private void UltraGrid1_BeforeEnterEditMode(object sender, CancelEventArgs e)
        {
            try
            {
                UltraGrid grid = sender as UltraGrid;
                if (grid?.ActiveRow != null && grid.ActiveRow.Index == 0)
                {
                    // First row is the base unit - only allow editing the AliasBarcode column
                    if (grid.ActiveCell != null &&
                        string.Equals(grid.ActiveCell.Column.Key, "AliasBarcode", StringComparison.OrdinalIgnoreCase))
                    {
                        // Allow editing AliasBarcode column
                        return;
                    }
                    // Block editing for all other columns in first row
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UltraGrid1_BeforeEnterEditMode: {ex.Message}");
            }
        }

        private UltraGridCell FindNextEditableCell(UltraGridRow row, UltraGridCell currentCell)
        {
            if (row == null || currentCell == null)
                return null;

            // Get all visible columns sorted by their visual position
            var visibleColumns = row.Band.Columns
                .Cast<UltraGridColumn>()
                .Where(c => !c.Hidden)
                .OrderBy(c => c.Header.VisiblePosition)
                .ToList();

            bool foundCurrent = false;

            // Go through columns in visual order
            foreach (var col in visibleColumns)
            {
                var cell = row.Cells[col.Key];

                // If we already found the current cell, check if this one is editable
                if (foundCurrent)
                {
                    if (cell.Column.CellActivation == Activation.AllowEdit)
                        return cell;
                }
                // Mark when we find the current cell
                else if (cell == currentCell)
                {
                    foundCurrent = true;
                }
            }

            return null;
        }

        private UltraGridCell FindFirstEditableCell(UltraGridRow row)
        {
            if (row == null)
                return null;

            // Get all visible columns sorted by their visual position
            var visibleColumns = row.Band.Columns
                .Cast<UltraGridColumn>()
                .Where(c => !c.Hidden)
                .OrderBy(c => c.Header.VisiblePosition)
                .ToList();

            // Go through columns in visual order
            foreach (var col in visibleColumns)
            {
                var cell = row.Cells[col.Key];
                if (cell.Column.CellActivation == Activation.AllowEdit)
                    return cell;
            }

            return null;
        }

        private UltraGridCell FindPreviousEditableCell(UltraGridRow row, UltraGridCell currentCell)
        {
            if (row == null || currentCell == null)
                return null;

            var visibleColumns = row.Band.Columns
                .Cast<UltraGridColumn>()
                .Where(c => !c.Hidden)
                .OrderBy(c => c.Header.VisiblePosition)
                .ToList();

            UltraGridCell previousEditableCell = null;

            foreach (var col in visibleColumns)
            {
                var cell = row.Cells[col.Key];
                if (cell == currentCell)
                {
                    return previousEditableCell;
                }

                if (cell.Column.CellActivation == Activation.AllowEdit)
                {
                    previousEditableCell = cell;
                }
            }

            return previousEditableCell;
        }

        private UltraGridCell FindLastEditableCell(UltraGridRow row)
        {
            if (row == null)
                return null;

            var visibleColumns = row.Band.Columns
                .Cast<UltraGridColumn>()
                .Where(c => !c.Hidden)
                .OrderByDescending(c => c.Header.VisiblePosition)
                .ToList();

            foreach (var col in visibleColumns)
            {
                var cell = row.Cells[col.Key];
                if (cell.Column.CellActivation == Activation.AllowEdit)
                    return cell;
            }

            return null;
        }

        // Handle btn_Remov_Item button click to remove selected unit from ultraGrid1
        private void btn_Remov_Item_Click(object sender, EventArgs e)
        {
            RemoveSelectedUnitFromGrid();
        }

        // Remove selected unit from ultraGrid1 while preventing base unit deletion for existing items
        private void RemoveSelectedUnitFromGrid()
        {
            try
            {
                if (ultraGrid1 == null || ultraGrid1.ActiveRow == null)
                {
                    MessageBox.Show("Please select a unit to remove.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Get the selected row
                UltraGridRow selectedRow = ultraGrid1.ActiveRow;

                // Get the unit name and packing from the selected row
                string unitName = selectedRow.Cells[colUnit].Value?.ToString();
                string packingText = selectedRow.Cells[colPacking].Value?.ToString();

                if (string.IsNullOrEmpty(unitName))
                {
                    MessageBox.Show("Unable to identify the unit. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Check if this is the base unit (packing = 1) - only prevent removal for existing items
                float packing = 0;
                if (float.TryParse(packingText, out packing) && packing == 1.0f && CurrentItemId > 0)
                {
                    MessageBox.Show("Cannot remove the base unit (1 UNIT, 1 KG, etc.) for existing items. Please select a different unit to remove.", "Base Unit Protected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Confirm deletion
                DialogResult result = MessageBox.Show($"Are you sure you want to remove the unit '{unitName}'?", "Confirm Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Remove the row from the DataTable
                    DataTable dt = ultraGrid1.DataSource as DataTable;
                    if (dt != null)
                    {
                        // Find the row in the DataTable that matches the selected row
                        DataRow[] matchingRows = dt.Select($"{colUnit} = '{unitName}' AND {colPacking} = '{packingText}'");
                        if (matchingRows.Length > 0)
                        {
                            dt.Rows.Remove(matchingRows[0]);
                            ultraGrid1.DataSource = dt;
                            ultraGrid1.Refresh();

                            // Also remove from Ult_Price grid if it exists
                            RemoveUnitFromPriceGrid(unitName);
                            LogItemActivity("REMOVE_UNIT", $"Unit '{unitName}' removed from item master.");

                            MessageBox.Show($"Unit '{unitName}' has been removed successfully.", "Unit Removed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error removing unit from grid: {ex.Message}");
                MessageBox.Show($"Error removing unit: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Remove unit from Ult_Price grid when removing from ultraGrid1
        private void RemoveUnitFromPriceGrid(string unitName)
        {
            try
            {
                Infragistics.Win.UltraWinGrid.UltraGrid Ult_Price =
                    this.Controls.Find("Ult_Price", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;

                if (Ult_Price != null)
                {
                    DataTable dtPrice = Ult_Price.DataSource as DataTable;
                    if (dtPrice != null)
                    {
                        // Find and remove the row with matching unit name
                        DataRow[] priceRows = dtPrice.Select($"Unit = '{unitName}'");
                        if (priceRows.Length > 0)
                        {
                            dtPrice.Rows.Remove(priceRows[0]);
                            Ult_Price.DataSource = dtPrice;
                            Ult_Price.Refresh();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error removing unit from price grid: {ex.Message}");
            }
        }

        // New method to generate a new item number and clear form fields
        private void btnIemLoad_ById_Click(object sender, EventArgs e)
        {
            try
            {
                int fallbackBaseItemNo = GetCurrentItemNoForNextNumber();

                // Clear all form fields first - use enhanced clear method
                ClearAllFields();


                GenerateNextItemNumberOnly(fallbackBaseItemNo);

                // Load default unit (Unit 1)
                LoadDefaultUnit();

                // Load default item type (Stock Item - ID 1)
                LoadDefaultItemType();

                // Set focus to the next field after item number (likely description)
                if (txt_barcode != null) txt_barcode.Focus();

                // Switch to Save mode for new item
                if (button3 != null) button3.Visible = true;
                if (btnUpdate != null) btnUpdate.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating new item number: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetNextItemNumber()
        {
            return ItemRepository.GetNextItemNumber();
        }

        private int GetCurrentItemNoForNextNumber()
        {
            int currentItemNo;
            if (txt_ItemNo != null && int.TryParse(txt_ItemNo.Text, out currentItemNo) && currentItemNo > 0)
                return currentItemNo;

            if (ItemMaster != null && ItemMaster.ItemNo > 0)
                return ItemMaster.ItemNo;

            return lastLoadedItemNo;
        }

        private void GenerateNextItemNumberOnly(int fallbackBaseItemNo = 0)
        {
            try
            {
                int nextItemNo = GetNextItemNumber();
                if (fallbackBaseItemNo > 0 && nextItemNo <= 1)
                    nextItemNo = fallbackBaseItemNo + 1;

                txt_ItemNo.Text = nextItemNo.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating next item number: {ex.Message}");
                throw;
            }
        }

        // Helper method to load default unit
        private void LoadDefaultUnit()
        {
            try
            {
                Dropdowns drop = new Dropdowns();
                DataBase.Operations = "Unit";
                UnitDDlGrid allUnitsGrid = drop.getUnitDDl();

                if (allUnitsGrid != null && allUnitsGrid.List != null && allUnitsGrid.List.Any())
                {
                    // Get the first unit
                    var firstUnit = allUnitsGrid.List.FirstOrDefault();
                    if (firstUnit != null)
                    {
                        // Set to txt_BaseUnit
                        if (txt_BaseUnit != null)
                        {
                            txt_BaseUnit.Text = firstUnit.UnitName;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading default unit: " + ex.Message);
            }
        }

        // Helper method to load default item type (Stock Item - ID 1)
        // Helper method to load default item type dynamically from repository
        private void LoadDefaultItemType()
        {
            try
            {
                var repo = new Repository.MasterRepositry.ItemTypeRepository();
                var defaultItem = repo.GetDefaultItemType();

                if (defaultItem != null && !string.IsNullOrWhiteSpace(defaultItem.ItemTypeName))
                {
                    if (txt_ItemType != null)
                    {
                        txt_ItemType.Text = defaultItem.ItemTypeName;
                    }
                }
                else
                {
                    Dropdowns drop = new Dropdowns();
                    var itemTypeGrid = drop.getItemTypeDDl();
                    if (itemTypeGrid != null && itemTypeGrid.List != null && itemTypeGrid.List.Any())
                    {
                        var firstItem = itemTypeGrid.List.FirstOrDefault();
                        if (firstItem != null && txt_ItemType != null)
                        {
                            txt_ItemType.Text = firstItem.ItemType;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading default item type: " + ex.Message);
            }
        }

        // Enhanced method to clear all fields completely
        private void ClearAllFields()
        {
            try
            {
                // Set flag to prevent master field behavior during clearing
                isLoadingItem = true;

                // Reset current item ID
                CurrentItemId = 0;

                // Reset ItemMaster object to prevent stale data during updates
                ItemMaster = new Item();
                SetMainBarcodeEditability(true, string.Empty);
                ResetItemStatusEditor();

                // Clear all text fields in the form recursively
                ClearControlsRecursive(this);

                // Reset specific fields that need to be explicitly cleared
                if (txt_ItemNo != null) txt_ItemNo.Clear();
                if (txt_description != null) txt_description.Clear();
                if (txt_LocalLanguage != null) txt_LocalLanguage.Clear();
                if (txt_ItemType != null) txt_ItemType.Clear();
                if (txt_Category != null) txt_Category.Clear();
                if (txt_Group != null) txt_Group.Clear();
                if (txt_Brand != null) txt_Brand.Clear();
                if (txt_BaseUnit != null) txt_BaseUnit.Clear();
                if (txt_CustomerType != null) txt_CustomerType.Clear();
                if (txt_TaxType != null) txt_TaxType.Clear();
                if (txt_TaxPer != null) txt_TaxPer.Clear();
                if (txt_TaxAmount != null) txt_TaxAmount.Text = "0";
                if (Txt_UnitCost != null) Txt_UnitCost.Text = "0.000";
                if (textBox1 != null) textBox1.Text = "0.00";
                if (txt_qty != null) txt_qty.Clear();
                if (txt_available != null) txt_available.Clear();
                if (txt_hold != null) txt_hold.Text = "0.00";
                if (txt_walkin != null) txt_walkin.Text = "0.000";
                if (txt_Retail != null) txt_Retail.Text = "0.000";
                if (txt_CEP != null) txt_CEP.Text = "0.000";
                if (txt_Mrp != null) txt_Mrp.Text = "0.000";
                if (txt_CardP != null) txt_CardP.Text = "0.000";
                if (ultraOrderCycle != null) ultraOrderCycle.Text = "0";
                if (ultraBoxQty != null) ultraBoxQty.Text = "0";
                if (ultraIsPerishable != null) ultraIsPerishable.Checked = false;

                // Clear selling price fields (use Control type to match Load event handling)
                var txt_SF = this.Controls.Find("txt_SF", true).FirstOrDefault() as Control;
                if (txt_SF != null)
                {
                    txt_SF.Text = "0.000";
                }

                var txt_MinP = this.Controls.Find("txt_MinP", true).FirstOrDefault() as Control;
                if (txt_MinP != null)
                {
                    txt_MinP.Text = "0.000";
                }

                // Clear markdown fields
                var ultraTextEditor11 = this.Controls.Find("ultraTextEditor11", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                if (ultraTextEditor11 != null) ultraTextEditor11.Text = "0.00";

                var ultraTextEditor12 = this.Controls.Find("ultraTextEditor12", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                if (ultraTextEditor12 != null) ultraTextEditor12.Text = "0.00";

                var ultraTextEditor13 = this.Controls.Find("ultraTextEditor13", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                if (ultraTextEditor13 != null) ultraTextEditor13.Text = "0.00";

                var ultraTextEditor14 = this.Controls.Find("ultraTextEditor14", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                if (ultraTextEditor14 != null) ultraTextEditor14.Text = "0.00";

                var ultraTextEditor15 = this.Controls.Find("ultraTextEditor15", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                if (ultraTextEditor15 != null) ultraTextEditor15.Text = "0.00";

                var ultraTextEditor16 = this.Controls.Find("ultraTextEditor16", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                if (ultraTextEditor16 != null) ultraTextEditor16.Text = "0.00";

                // Clear profit margin fields
                ClearAllProfitMargins();


                // Clear price grid (Ult_Price)
                Infragistics.Win.UltraWinGrid.UltraGrid Ult_Price =
                    this.Controls.Find("Ult_Price", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;
                if (Ult_Price != null)
                {
                    // Create empty DataTable for price grid
                    DataTable dtPrice = new DataTable();
                    dtPrice.Columns.Add("Unit", typeof(string));
                    dtPrice.Columns.Add("Packing", typeof(string));
                    dtPrice.Columns.Add("Cost", typeof(float));
                    dtPrice.Columns.Add("MarginAmt", typeof(float));
                    dtPrice.Columns.Add("MarginPer", typeof(float));
                    dtPrice.Columns.Add("TaxPer", typeof(float));
                    dtPrice.Columns.Add("TaxAmt", typeof(float));
                    dtPrice.Columns.Add("MRP", typeof(float));
                    dtPrice.Columns.Add("RetailPrice", typeof(float));
                    dtPrice.Columns.Add("WholeSalePrice", typeof(float));
                    dtPrice.Columns.Add("CreditPrice", typeof(float));
                    dtPrice.Columns.Add("CardPrice", typeof(float));

                    Ult_Price.DataSource = dtPrice;
                    GetPriceDesing(); // Reinitialize the price grid
                }

                // Clear ultraGrid1 (UOM grid)
                if (ultraGrid1 != null)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add(colUnit, typeof(string));
                    dt.Columns.Add(colUnitId, typeof(string));
                    dt.Columns.Add(colPacking, typeof(string));
                    // dt.Columns.Add(colBarcode, typeof(string)); // Removed
                    dt.Columns.Add(colReorder, typeof(string));
                    dt.Columns.Add(colOpenStock, typeof(string));
                    ultraGrid1.DataSource = dt;
                }

                // Clear ultraGrid3 (Alternative Barcode Grid)
                var altGrid = this.Controls.Find("ultraGrid3", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;
                if (altGrid != null)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("Barcode", typeof(string));
                    altGrid.DataSource = dt;
                }

                // Clear ultraGrid2 (Vendor details grid)
                Infragistics.Win.UltraWinGrid.UltraGrid ultraGrid2 =
                    this.Controls.Find("ultraGrid2", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;
                if (ultraGrid2 != null)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("LedgerID", typeof(int));
                    dt.Columns.Add("VendorName", typeof(string));
                    dt.Columns.Add("Cost", typeof(double));
                    dt.Columns.Add("Unit", typeof(string));
                    dt.Columns.Add("InvoiceDate", typeof(DateTime));
                    dt.Columns.Add("PurchaseNo", typeof(int));
                    dt.Columns.Add("InvoiceNo", typeof(string));
                    ultraGrid2.DataSource = dt;
                }

                // Reset any image controls if needed
                if (pictureBoxItem != null)
                {
                    DeleteCurrentImage();
                }

                // After clearing, default to Save mode (new item)
                if (button3 != null) button3.Visible = true;
                if (btnUpdate != null) btnUpdate.Visible = false;
                ResetItemStatusEditor();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ClearAllFields: {ex.Message}");
            }
            finally
            {
                // Reset flag to allow master field behavior after clearing
                isLoadingItem = false;
                // Reset flag to allow new item number generation on next barcode entry
                hasGeneratedItemNumberForBarcode = false;
            }
        }

        // Helper method to recursively clear all controls in the form
        private void ClearControlsRecursive(Control parentControl)
        {
            foreach (Control control in parentControl.Controls)
            {
                // Clear based on control type
                if (control is TextBox)
                {
                    ((TextBox)control).Clear();
                }
                else if (control is ComboBox)
                {
                    ComboBox comboBox = (ComboBox)control;
                    if (comboBox.Items.Count > 0)
                        comboBox.SelectedIndex = -1;
                }
                else if (control is CheckBox)
                {
                    ((CheckBox)control).Checked = false;
                }
                else if (control is RadioButton)
                {
                    ((RadioButton)control).Checked = false;
                }
                else if (control is DateTimePicker)
                {
                    ((DateTimePicker)control).Value = DateTime.Now;
                }

                // If the control contains other controls, recursively clear them
                if (control.HasChildren)
                {
                    ClearControlsRecursive(control);
                }
            }
        }

        public void clear()
        {
            // Call the enhanced clear method
            ClearAllFields();
        }

        private Infragistics.Win.UltraWinEditors.UltraTextEditor GetMainBarcodeEditor()
        {
            return txt_barcode ?? this.Controls.Find("txt_barcode", true)
                .OfType<Infragistics.Win.UltraWinEditors.UltraTextEditor>()
                .FirstOrDefault();
        }

        private void WireBarcodeRefreshMouseEvents(Control control)
        {
            if (control == null)
            {
                return;
            }

            try
            {
                control.Click -= BarcodeCtrl_ClickToRefresh;
                control.Click += BarcodeCtrl_ClickToRefresh;

                control.MouseClick -= BarcodeCtrl_ClickToRefresh;
                control.MouseClick += BarcodeCtrl_ClickToRefresh;

                control.MouseDown -= BarcodeCtrl_ClickToRefresh;
                control.MouseDown += BarcodeCtrl_ClickToRefresh;

                control.GotFocus -= BarcodeCtrl_ClickToRefresh;
                control.GotFocus += BarcodeCtrl_ClickToRefresh;

                control.Enter -= BarcodeCtrl_ClickToRefresh;
                control.Enter += BarcodeCtrl_ClickToRefresh;

                foreach (Control child in control.Controls)
                {
                    WireBarcodeRefreshMouseEvents(child);
                }
            }
            catch { }
        }

        private void WireUnitCostRefreshMouseEvents(Control control)
        {
            if (control == null)
            {
                return;
            }

            control.Click -= Txt_UnitCost_Click;
            control.Click += Txt_UnitCost_Click;
            control.MouseClick -= Txt_UnitCost_MouseClick;
            control.MouseClick += Txt_UnitCost_MouseClick;
            control.MouseDown -= Txt_UnitCost_MouseDown;
            control.MouseDown += Txt_UnitCost_MouseDown;
            control.GotFocus -= Txt_UnitCost_Click;
            control.GotFocus += Txt_UnitCost_Click;

            foreach (Control child in control.Controls)
            {
                WireUnitCostRefreshMouseEvents(child);
            }
        }
        private void SetMainBarcodeEditability(bool allowEdit, string barcode = null)
        {
            var txtBarcodeCtrl = GetMainBarcodeEditor();
            if (txtBarcodeCtrl != null)
            {
                if (barcode != null)
                {
                    txtBarcodeCtrl.Text = barcode;
                }

                txtBarcodeCtrl.ReadOnly = !allowEdit;
                txtBarcodeCtrl.BackColor = Color.FromArgb(255, 224, 192);
                txtBarcodeCtrl.Appearance.BackColor = Color.FromArgb(255, 224, 192);
            }

            loadedItemMainBarcode = allowEdit ? string.Empty : ((barcode ?? txtBarcodeCtrl?.Text) ?? string.Empty).Trim();

            if (!allowEdit && !string.IsNullOrWhiteSpace(loadedItemMainBarcode))
            {
                ItemMaster.Barcode = loadedItemMainBarcode;
            }
        }

        public void SetLoadedItemBarcode(string barcode)
        {
            SetMainBarcodeEditability(false, barcode ?? string.Empty);
        }

        private bool ValidateLoadedItemBarcodeIsUnchanged(string currentBarcode)
        {
            bool isExistingItem = (ItemMaster != null && ItemMaster.ItemId > 0) || CurrentItemId > 0;
            if (!isExistingItem)
            {
                return true;
            }

            string originalBarcode = !string.IsNullOrWhiteSpace(loadedItemMainBarcode)
                ? loadedItemMainBarcode.Trim()
                : (ItemMaster?.Barcode ?? string.Empty).Trim();
            string normalizedCurrentBarcode = (currentBarcode ?? string.Empty).Trim();

            if (string.Equals(originalBarcode, normalizedCurrentBarcode, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            MessageBox.Show("Main barcode cannot be changed for an existing item.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            SetLoadedItemBarcode(originalBarcode);
            GetMainBarcodeEditor()?.Focus();
            return false;
        }

        private void btn_unit_Click(object sender, EventArgs e)
        {
            // For new items, allow setting base unit even when CurrentItemId is 0
            // For existing items, CurrentItemId will be > 0
            string Params = "ItemMasterMaster";
            frmUnitDialog unitDialog = new frmUnitDialog(Params, CurrentItemId);
            unitDialog.StartPosition = FormStartPosition.CenterScreen;

            // Show dialog and check result
            if (unitDialog.ShowDialog() == DialogResult.OK)
            {
                // The dialog should have set the base unit text and ID
                // Now we need to ensure ultraGrid1 reflects this base unit selection
                // and clear any existing units that don't match the new base unit
                SynchronizeBaseUnitWithGrid();
            }
        }

        // New method to load the selected unit's details into ultraGrid1 and Ult_Price
        private void LoadSelectedUnitDetails()
        {
            try
            {
                // Get the selected unit information from the form
                string selectedUnitName = txt_BaseUnit.Text;
                int selectedUnitId = 0;

                if (string.IsNullOrEmpty(selectedUnitName))
                {
                    System.Diagnostics.Debug.WriteLine("No unit selected");
                    return;
                }

                UnitMaster selectedUnit = GetUnitByNameFromStoredProcedure(selectedUnitName);
                selectedUnitId = selectedUnit?.UnitID ?? 0;

                if (selectedUnitId <= 0)
                {
                    System.Diagnostics.Debug.WriteLine("Unit ID not found for unit: " + selectedUnitName);
                    return;
                }

                string unitName = selectedUnit.UnitName;
                float packing = Convert.ToSingle(selectedUnit.Packing);

                            // Clear existing data in ultraGrid1
                            DataTable dtUom = ultraGrid1.DataSource as DataTable;
                            if (dtUom != null)
                            {
                                dtUom.Rows.Clear();
                            }
                            else
                            {
                                dtUom = new DataTable();
                                dtUom.Columns.Add("Unit", typeof(string));
                                dtUom.Columns.Add("UnitId", typeof(string));
                                dtUom.Columns.Add("Packing", typeof(string));
                                dtUom.Columns.Add("BarCode", typeof(string));
                                dtUom.Columns.Add("Reorder", typeof(string));
                                dtUom.Columns.Add("OpnStk", typeof(string));
                                ultraGrid1.DataSource = dtUom;
                            }

                            // Add the selected unit to ultraGrid1, pass current barcode if any
                            string currentBarcode = string.Empty;
                            try
                            {
                                var txtBarcodeCtrl = GetMainBarcodeEditor();
                                if (txtBarcodeCtrl != null) currentBarcode = txtBarcodeCtrl.Text ?? string.Empty;
                            }
                            catch { }
                            AddOrUpdateUomRow(unitName, selectedUnitId, packing, 5, string.IsNullOrWhiteSpace(currentBarcode) ? "0" : currentBarcode, 0);

                            // Clear existing data in Ult_Price
                            Infragistics.Win.UltraWinGrid.UltraGrid Ult_Price =
                                this.Controls.Find("Ult_Price", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;

                            if (Ult_Price != null)
                            {
                                DataTable dtPrice = Ult_Price.DataSource as DataTable;
                                if (dtPrice != null)
                                {
                                    dtPrice.Rows.Clear();
                                }
                                else
                                {
                                    dtPrice = new DataTable();
                                    dtPrice.Columns.Add("Unit", typeof(string));
                                    dtPrice.Columns.Add("Packing", typeof(int));
                                    dtPrice.Columns.Add("Cost", typeof(float));
                                    dtPrice.Columns.Add("MarginAmt", typeof(float));
                                    dtPrice.Columns.Add("MarginPer", typeof(float));
                                    dtPrice.Columns.Add("TaxPer", typeof(float));
                                    dtPrice.Columns.Add("TaxAmt", typeof(float));
                                    dtPrice.Columns.Add("MRP", typeof(float));
                                    dtPrice.Columns.Add("RetailPrice", typeof(float));
                                    dtPrice.Columns.Add("WholeSalePrice", typeof(float));
                                    dtPrice.Columns.Add("CreditPrice", typeof(float));
                                    dtPrice.Columns.Add("CardPrice", typeof(float));
                                    dtPrice.Columns.Add("StaffPrice", typeof(float));
                                    dtPrice.Columns.Add("MinPrice", typeof(float));
                                    Ult_Price.DataSource = dtPrice;
                                }

                                // Get base unit cost and prices from the form
                                float baseCost = 0;
                                float.TryParse(Txt_UnitCost.Text, out baseCost);

                                float baseMRP = 0;
                                float.TryParse(txt_Mrp.Text, out baseMRP);

                                float baseRetailPrice = 0;
                                float.TryParse(txt_Retail.Text, out baseRetailPrice);

                                float baseWalkingPrice = 0;
                                float.TryParse(txt_walkin.Text, out baseWalkingPrice);

                                float baseCreditPrice = 0;
                                float.TryParse(txt_CEP.Text, out baseCreditPrice);

                                float baseCardPrice = 0;
                                float.TryParse(txt_CardP.Text, out baseCardPrice);

                                // Calculate tax percentage and amount
                                float taxPer = 0;
                                float.TryParse(txt_TaxPer.Text, out taxPer);

                                // Add the selected unit to Ult_Price with calculated values
                                DataRow newRow = dtPrice.NewRow();
                                newRow["Unit"] = unitName;
                                newRow["Packing"] = Convert.ToInt32(packing);

                                // Calculate values based on packing (1 UNIT = base values, other units = base * packing)
                                newRow["Cost"] = baseCost * packing;
                                newRow["MRP"] = baseMRP * packing;
                                newRow["RetailPrice"] = baseRetailPrice * packing; // Visual "Retail Price"
                                newRow["WholeSalePrice"] = baseWalkingPrice * packing; // Visual "Walking Price"
                                newRow["CreditPrice"] = baseCreditPrice * packing;
                                newRow["CardPrice"] = baseCardPrice * packing;
                                if (dtPrice.Columns.Contains("StaffPrice")) newRow["StaffPrice"] = 0f;
                                if (dtPrice.Columns.Contains("MinPrice")) newRow["MinPrice"] = 0f;

                                // Calculate margin amount based on Retail (master selling price)
                                float marginAmount = (baseRetailPrice * packing) - (baseCost * packing);
                                // Margin % mirrors txt_Retail's profit margin editor (ultraTextEditor4)
                                double retailMarginPercent = 0;
                                double.TryParse(ultraTextEditor4 != null ? ultraTextEditor4.Text : "0", out retailMarginPercent);
                                float marginPercentage = (float)retailMarginPercent;

                                newRow["MarginAmt"] = marginAmount;
                                newRow["MarginPer"] = marginPercentage;
                                newRow["TaxPer"] = taxPer;

                                // Calculate tax amount for row based on incl/excl
                                float taxAmount = (float)ComputeTaxAmountForGridRow(baseRetailPrice * packing, taxPer);
                                newRow["TaxAmt"] = taxAmount;

                                dtPrice.Rows.Add(newRow);
                                Ult_Price.DataSource = dtPrice;
                                Ult_Price.Refresh();

                                // Apply column layout configuration to ensure all columns are visible
                                GetPriceDesing();

                                // Recompute tax display to reflect the current mode and retail
                                UpdateInclusiveExclusiveTaxDisplay();

                                System.Diagnostics.Debug.WriteLine($"Successfully loaded unit details for {unitName} with packing {packing}");
                                System.Diagnostics.Debug.WriteLine($"Base unit (1 UNIT) values: Cost={baseCost}, MRP={baseMRP}, Walking={baseWalkingPrice}, Retail={baseRetailPrice}");
                                System.Diagnostics.Debug.WriteLine($"Calculated values for {unitName}: Cost={baseCost * packing}, MRP={baseMRP * packing}, Walking={baseWalkingPrice * packing}");
                            }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading selected unit details: {ex.Message}");
                MessageBox.Show($"Error loading unit details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Method to style all ultraPanels in the form
        private void StyleAllUltraPanels()
        {
            // List of panel names to style
            string[] panelNames = { "ultraPanel2", "ultraPanel3", "ultraPanel4", "ultraPanel5",
                                            "ultraPanel8", "ultraPanel9", "ultraPanel10", "ultraPanel11", "ultraPanel12", "ultraPanel13",
                                            "ultraPanel16", "ultraPanel17", "ultraPanel18", "ultraPanel19", "ultraPanel20",
                                            "ultraPanel21", "ultraPanel22", "ultraPanel23", "ultraPanel24", "ultraPanel25", "ultraPanel26" };

            foreach (string panelName in panelNames)
            {
                if (this.Controls.Find(panelName, true).Length > 0)
                {
                    Infragistics.Win.Misc.UltraPanel panel = (Infragistics.Win.Misc.UltraPanel)this.Controls.Find(panelName, true)[0];
                    StyleIconPanel(panel);
                }
            }

            // Connect panel click events
            ConnectPanelClickEvents();
        }

        private void StyleIconPanel(Infragistics.Win.Misc.UltraPanel panel)
        {
            if (panel == null) return;

            panel.UseAppStyling = false;

            // ReportFormat button theme colors (matching ultraPanel6 of frmReportFormatDialog.cs)
            Color topColor = Color.FromArgb(234, 244, 255);       // #EAF4FF
            Color bottomColor = Color.FromArgb(152, 188, 235);    // #98BCEB
            Color borderColor = Color.FromArgb(73, 119, 184);     // #4977B8
            Color textColor = Color.FromArgb(0, 46, 127);         // #002E7F bold dark blue

            Color hoverTop = Color.FromArgb(245, 250, 255);
            Color hoverBottom = Color.FromArgb(170, 206, 244);

            Color pressedTop = Color.FromArgb(205, 226, 248);
            Color pressedBottom = Color.FromArgb(128, 170, 224);

            panel.Appearance.BackColor = topColor;
            panel.Appearance.BackColor2 = bottomColor;
            panel.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;

            panel.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded1;
            panel.Appearance.BorderColor = borderColor;

            Action setHoverState = () =>
            {
                panel.Appearance.BackColor = hoverTop;
                panel.Appearance.BackColor2 = hoverBottom;
            };

            Action setNormalState = () =>
            {
                panel.Appearance.BackColor = topColor;
                panel.Appearance.BackColor2 = bottomColor;
            };

            Action setPressedState = () =>
            {
                panel.Appearance.BackColor = pressedTop;
                panel.Appearance.BackColor2 = pressedBottom;
            };

            foreach (Control control in panel.ClientArea.Controls)
            {
                if (control is Infragistics.Win.UltraWinEditors.UltraPictureBox pic)
                {
                    pic.BackColor = Color.Transparent;
                    pic.BackColorInternal = Color.Transparent;
                    pic.BorderShadowColor = Color.Transparent;
                    pic.Cursor = Cursors.Hand;

                    pic.MouseEnter += (s, e) => setHoverState();
                    pic.MouseLeave += (s, e) => setNormalState();
                    pic.MouseDown += (s, e) => setPressedState();
                    pic.MouseUp += (s, e) => setHoverState();
                }
                else if (control is Label lbl)
                {
                    lbl.BackColor = Color.Transparent;
                    lbl.ForeColor = textColor;
                    lbl.Font = new Font("Microsoft Sans Serif", lbl.Font.SizeInPoints > 0 ? lbl.Font.SizeInPoints : 9.75F, FontStyle.Regular);
                    lbl.Cursor = Cursors.Hand;

                    lbl.MouseEnter += (s, e) => setHoverState();
                    lbl.MouseLeave += (s, e) => setNormalState();
                    lbl.MouseDown += (s, e) => setPressedState();
                    lbl.MouseUp += (s, e) => setHoverState();
                }
            }

            panel.ClientArea.MouseEnter += (s, e) => setHoverState();
            panel.ClientArea.MouseLeave += (s, e) => setNormalState();
            panel.ClientArea.MouseDown += (s, e) => setPressedState();
            panel.ClientArea.MouseUp += (s, e) => setHoverState();

            panel.ClientArea.Cursor = Cursors.Hand;
        }

        private void StepOrderCycle(int delta)
        {
            if (ultraOrderCycle != null)
            {
                int val = 0;
                int.TryParse(ultraOrderCycle.Text, out val);
                val += delta;
                if (val < 0) val = 0;
                ultraOrderCycle.Text = val.ToString();
            }
        }

        private void SetupOrderCycleSpinner(Infragistics.Win.Misc.UltraPanel panel, int direction)
        {
            if (panel == null) return;

            panel.Cursor = Cursors.Hand;
            panel.ClientArea.Cursor = Cursors.Hand;

            if (orderCycleSpinnerTimer == null)
            {
                orderCycleSpinnerTimer = new System.Windows.Forms.Timer();
                orderCycleSpinnerTimer.Tick += (s, e) =>
                {
                    if (orderCycleSpinnerIsInitialDelay)
                    {
                        orderCycleSpinnerIsInitialDelay = false;
                        orderCycleSpinnerTimer.Interval = 60; // Fast continuous repeat interval after initial delay
                    }
                    if (orderCycleSpinnerDirection != 0)
                    {
                        StepOrderCycle(orderCycleSpinnerDirection);
                    }
                };
            }

            MouseEventHandler onMouseDown = (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    orderCycleSpinnerDirection = direction;
                    StepOrderCycle(direction); // Immediate single click effect on 1st click!
                    orderCycleSpinnerIsInitialDelay = true;
                    orderCycleSpinnerTimer.Interval = 350; // Initial hold delay before continuous repeat
                    orderCycleSpinnerTimer.Stop();
                    orderCycleSpinnerTimer.Start();
                }
            };

            MouseEventHandler onMouseUp = (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    orderCycleSpinnerTimer.Stop();
                    orderCycleSpinnerDirection = 0;
                }
            };

            EventHandler onMouseLeave = (s, e) =>
            {
                orderCycleSpinnerTimer.Stop();
                orderCycleSpinnerDirection = 0;
            };

            panel.MouseDown += onMouseDown;
            panel.MouseUp += onMouseUp;
            panel.MouseLeave += onMouseLeave;

            panel.ClientArea.MouseDown += onMouseDown;
            panel.ClientArea.MouseUp += onMouseUp;
            panel.ClientArea.MouseLeave += onMouseLeave;

            foreach (Control child in panel.ClientArea.Controls)
            {
                child.Cursor = Cursors.Hand;
                child.MouseDown += onMouseDown;
                child.MouseUp += onMouseUp;
                child.MouseLeave += onMouseLeave;
            }
        }

        private void ConnectPanelClickEvents()
        {
            // Connect click events for panels
            string[] panelNames = { "ultraPanel2", "ultraPanel3", "ultraPanel4", "ultraPanel5",
                                            "ultraPanel8", "ultraPanel9", "ultraPanel10", "ultraPanel11", "ultraPanel12", "ultraPanel13",
                                            "ultraPanel16", "ultraPanel17", "ultraPanel18", "ultraPanel19", "ultraPanel20",
                                            "ultraPanel21", "ultraPanel22", "ultraPanel23", "ultraPanel24", "ultraPanel25", "ultraPanel26" };

            foreach (string panelName in panelNames)
            {
                if (this.Controls.Find(panelName, true).Length > 0)
                {
                    Infragistics.Win.Misc.UltraPanel panel = (Infragistics.Win.Misc.UltraPanel)this.Controls.Find(panelName, true)[0];

                    // Connect panel click events
                    panel.Click += (sender, e) => Panel_Click(sender, e, panelName);
                    panel.ClientArea.Click += (sender, e) => Panel_Click(sender, e, panelName);

                    // Connect click events for child controls too
                    foreach (Control control in panel.ClientArea.Controls)
                    {
                        if (control is Label || control is Infragistics.Win.UltraWinEditors.UltraPictureBox)
                        {
                            control.Click += (sender, e) => Panel_Click(sender, e, panelName);
                        }
                    }

                    // Set up hover effect synchronization for specific panel-picturebox pairs
                    if (panelName == "ultraPanel3" && this.Controls.Find("ultraPictureBox2", true).Length > 0)
                    {
                        SetupHoverEffectSync(panel, (Infragistics.Win.UltraWinEditors.UltraPictureBox)this.Controls.Find("ultraPictureBox2", true)[0], null);
                    }
                    else if (panelName == "ultraPanel9" && this.Controls.Find("ultraPictureBox4", true).Length > 0)
                    {
                        SetupHoverEffectSync(panel, (Infragistics.Win.UltraWinEditors.UltraPictureBox)this.Controls.Find("ultraPictureBox4", true)[0], null);
                    }
                    else if (panelName == "ultraPanel8" && this.Controls.Find("ultraPictureBox6", true).Length > 0)
                    {
                        SetupHoverEffectSync(panel, (Infragistics.Win.UltraWinEditors.UltraPictureBox)this.Controls.Find("ultraPictureBox6", true)[0], null);
                    }
                    else if (panelName == "ultraPanel10" && this.Controls.Find("ultraPictureBox5", true).Length > 0)
                    {
                        SetupHoverEffectSync(panel, (Infragistics.Win.UltraWinEditors.UltraPictureBox)this.Controls.Find("ultraPictureBox5", true)[0], null);
                    }
                    // New panel-picturebox-label combinations
                    else if (panelName == "ultraPanel11" && this.Controls.Find("ultraPictureBox1", true).Length > 0)
                    {
                        Label label29 = null;
                        if (this.Controls.Find("label29", true).Length > 0)
                        {
                            label29 = (Label)this.Controls.Find("label29", true)[0];
                        }
                        SetupHoverEffectSync(panel, (Infragistics.Win.UltraWinEditors.UltraPictureBox)this.Controls.Find("ultraPictureBox1", true)[0], label29);
                    }
                    else if (panelName == "ultraPanel13" && this.Controls.Find("ultraPictureBox7", true).Length > 0)
                    {
                        Label label44 = null;
                        if (this.Controls.Find("label44", true).Length > 0)
                        {
                            label44 = (Label)this.Controls.Find("label44", true)[0];
                        }
                        SetupHoverEffectSync(panel, (Infragistics.Win.UltraWinEditors.UltraPictureBox)this.Controls.Find("ultraPictureBox7", true)[0], label44);
                    }
                    else if (panelName == "ultraPanel4")
                    {
                        Label label31 = null;
                        if (this.Controls.Find("label31", true).Length > 0)
                        {
                            label31 = (Label)this.Controls.Find("label31", true)[0];
                            SetupHoverEffectSync(panel, null, label31);
                        }
                    }
                    else if (panelName == "ultraPanel5" && this.Controls.Find("ultraPictureBox3", true).Length > 0)
                    {
                        Label label30 = null;
                        if (this.Controls.Find("label30", true).Length > 0)
                        {
                            label30 = (Label)this.Controls.Find("label30", true)[0];
                        }
                        SetupHoverEffectSync(panel, (Infragistics.Win.UltraWinEditors.UltraPictureBox)this.Controls.Find("ultraPictureBox3", true)[0], label30);
                    }
                }
            }

            // Connect dedicated MouseDown + Hold-Repeat Spinners for ultraPanel27 (+) and ultraPanel28 (-)
            if (this.Controls.Find("ultraPanel27", true).Length > 0)
            {
                var p27 = (Infragistics.Win.Misc.UltraPanel)this.Controls.Find("ultraPanel27", true)[0];
                SetupOrderCycleSpinner(p27, +1);
            }
            if (this.Controls.Find("ultraPanel28", true).Length > 0)
            {
                var p28 = (Infragistics.Win.Misc.UltraPanel)this.Controls.Find("ultraPanel28", true)[0];
                SetupOrderCycleSpinner(p28, -1);
            }
        }

        // Updated method for synchronizing hover effects between panels, picture boxes, and labels
        private void SetupHoverEffectSync(Infragistics.Win.Misc.UltraPanel panel, Infragistics.Win.UltraWinEditors.UltraPictureBox pictureBox, Label label)
        {
            if (panel == null)
                return;

            // Store original colors
            Color originalBackColor = panel.Appearance.BackColor;
            Color originalBackColor2 = panel.Appearance.BackColor2;

            // Define hover colors - brighter versions of the original colors
            Color hoverBackColor = Color.FromArgb(
                Math.Min(originalBackColor.R + 30, 255),
                Math.Min(originalBackColor.G + 30, 255),
                Math.Min(originalBackColor.B + 30, 255));
            Color hoverBackColor2 = Color.FromArgb(
                Math.Min(originalBackColor2.R + 30, 255),
                Math.Min(originalBackColor2.G + 30, 255),
                Math.Min(originalBackColor2.B + 30, 255));

            // When mouse enters the picture box, change the panel appearance
            if (pictureBox != null)
            {
                pictureBox.MouseEnter += (s, e) =>
                {
                    panel.Appearance.BackColor = hoverBackColor;
                    panel.Appearance.BackColor2 = hoverBackColor2;
                    pictureBox.Cursor = Cursors.Hand;
                };

                // When mouse leaves the picture box, restore the panel appearance
                // but only if the mouse isn't still over the panel
                pictureBox.MouseLeave += (s, e) =>
                {
                    Point mousePos = panel.PointToClient(Control.MousePosition);
                    if (!panel.ClientRectangle.Contains(mousePos))
                    {
                        panel.Appearance.BackColor = originalBackColor;
                        panel.Appearance.BackColor2 = originalBackColor2;
                    }
                };
            }

            // When mouse enters the label, change the panel appearance
            if (label != null)
            {
                label.MouseEnter += (s, e) =>
                {
                    panel.Appearance.BackColor = hoverBackColor;
                    panel.Appearance.BackColor2 = hoverBackColor2;
                    label.Cursor = Cursors.Hand;
                };

                // When mouse leaves the label, restore the panel appearance
                // but only if the mouse isn't still over the panel
                label.MouseLeave += (s, e) =>
                {
                    Point mousePos = panel.PointToClient(Control.MousePosition);
                    if (!panel.ClientRectangle.Contains(mousePos))
                    {
                        panel.Appearance.BackColor = originalBackColor;
                        panel.Appearance.BackColor2 = originalBackColor2;
                    }
                };
            }

            // Make sure panel hover events are still working properly
            panel.MouseEnter += (s, e) =>
            {
                panel.Appearance.BackColor = hoverBackColor;
                panel.Appearance.BackColor2 = hoverBackColor2;
                panel.ClientArea.Cursor = Cursors.Hand;
            };

            panel.MouseLeave += (s, e) =>
            {
                panel.Appearance.BackColor = originalBackColor;
                panel.Appearance.BackColor2 = originalBackColor2;
                panel.ClientArea.Cursor = Cursors.Default;
            };

            // Apply the same effect to the client area of the panel
            panel.ClientArea.MouseEnter += (s, e) =>
            {
                panel.Appearance.BackColor = hoverBackColor;
                panel.Appearance.BackColor2 = hoverBackColor2;
                panel.ClientArea.Cursor = Cursors.Hand;
            };

            panel.ClientArea.MouseLeave += (s, e) =>
            {
                panel.Appearance.BackColor = originalBackColor;
                panel.Appearance.BackColor2 = originalBackColor2;
                panel.ClientArea.Cursor = Cursors.Default;
            };
        }

        private void Panel_Click(object sender, EventArgs e, string panelName)
        {
            // Handle panel clicks based on panel name
            switch (panelName)
            {
                case "ultraPanel3":
                    // Load first item (the one with the lowest item number)
                    NavigateToItem("FIRST");
                    break;
                case "ultraPanel9":
                    // Load previous item based on current item number
                    NavigateToItem("PREVIOUS");
                    break;
                case "ultraPanel8":
                    // Load next item based on current item number
                    NavigateToItem("NEXT");
                    break;
                case "ultraPanel10":
                    // Load the last item (the one with the highest item number)
                    NavigateToItem("LAST");
                    break;
                case "ultraPanel19":
                    btn_ItemLoad_Click(sender, e);
                    break;
                case "ultraPanel20":
                    btnIemLoad_ById_Click(sender, e);
                    break;
                case "ultraPanel21":
                    btn_Add_Brand_Click(sender, e);
                    break;
                case "ultraPanel22":
                    btn_unit_Click(sender, e);
                    break;
                case "ultraPanel23":
                    btn_Add_Custm_Click(sender, e);
                    break;
                case "ultraPanel24":
                    btn_Add_ItemIype_Click(sender, e);
                    break;
                case "ultraPanel25":
                    btn_Add_Cate_Click(sender, e);
                    break;
                case "ultraPanel26":
                    btn_Add_Grup_Click(sender, e);
                    break;
                case "ultraPanel2":
                    OpenPurchaseHistoryForSelectedRow();
                    break;
                case "ultraPanel4":
                    // Open the digview form with hold details
                    try
                    {
                        // Create and show the digview form
                        view viewForm = new view();
                        viewForm.StartPosition = FormStartPosition.CenterScreen;

                        // Pass the current item ID to load hold details
                        double totalHoldQty = viewForm.LoadHoldDetails(CurrentItemId);

                        viewForm.ShowDialog();

                        // Refresh hold quantity after dialog is closed
                        UpdateHoldQuantityFromHoldDetails();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    break;
                case "ultraPanel5":
                    // Open Stock Adjustment form in tab and load current item
                    OpenStockAdjustmentInTab();
                    break;
                case "ultraPanel11":
                    CloneCurrentItemAsVariant();
                    break;
                case "ultraPanel12":
                    OpenInactiveActiveItemDialog();
                    break;
                case "ultraPanel13":
                    // Open the Barcode form in UltraTabControl
                    OpenBarcodeFormInTab();
                    break;
                case "ultraPanel17":
                    // Open FrmPurchase and load the selected GRN/purchase number
                    OpenPurchaseHistoryForSelectedRow();
                    break;
                case "ultraPanel16":
                    // Open FrmVendor and load the vendor from the selected purchase row
                    OpenVendorForSelectedRow();
                    break;
                case "ultraPanel18":
                    // Open Vendor Purchase Report filtered by the vendor from selected row
                    OpenVendorPurchaseReportForSelectedRow();
                    break;
            }
        }

        private void OpenInactiveActiveItemDialog()
        {
            try
            {
                using (inactiveactiveitemdig inactiveItemsDialog = new inactiveactiveitemdig())
                {
                    inactiveItemsDialog.StartPosition = FormStartPosition.CenterScreen;
                    inactiveItemsDialog.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening inactive items: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenPurchaseHistoryForSelectedRow()
        {
            try
            {
                var vendorGrid = this.Controls.Find("ultraGrid2", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;
                if (vendorGrid == null)
                {
                    MessageBox.Show("Purchase history grid is not available.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Infragistics.Win.UltraWinGrid.UltraGridRow selectedRow = vendorGrid.ActiveRow;
                if (selectedRow == null && vendorGrid.Selected.Rows.Count > 0)
                {
                    selectedRow = vendorGrid.Selected.Rows[0];
                }

                if (selectedRow == null)
                {
                    MessageBox.Show("Please select a purchase entry to view.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int pid = 0;
                int purchaseNo = 0;
                string invoiceNo = string.Empty;

                // 1. Check hidden Pid value if an older bound grid still has it.
                if (selectedRow.Cells.Exists("Pid") && selectedRow.Cells["Pid"].Value != null && selectedRow.Cells["Pid"].Value != DBNull.Value)
                {
                    int.TryParse(Convert.ToString(selectedRow.Cells["Pid"].Value), out pid);
                }

                // 2. Check PurchaseNo cell
                if (selectedRow.Cells.Exists("PurchaseNo") && selectedRow.Cells["PurchaseNo"].Value != null && selectedRow.Cells["PurchaseNo"].Value != DBNull.Value)
                {
                    string valStr = Convert.ToString(selectedRow.Cells["PurchaseNo"].Value).Trim();
                    if (int.TryParse(valStr, out int pNoDirect))
                    {
                        purchaseNo = pNoDirect;
                    }
                    else
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(valStr, @"\d+");
                        if (match.Success && int.TryParse(match.Value, out int pNoParsed))
                        {
                            purchaseNo = pNoParsed;
                        }
                    }
                }

                // 3. Check InvoiceNo cell
                if (selectedRow.Cells.Exists("InvoiceNo") && selectedRow.Cells["InvoiceNo"].Value != null && selectedRow.Cells["InvoiceNo"].Value != DBNull.Value)
                {
                    invoiceNo = Convert.ToString(selectedRow.Cells["InvoiceNo"].Value).Trim();
                    if (purchaseNo <= 0 && !string.IsNullOrEmpty(invoiceNo))
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(invoiceNo, @"\d+");
                        if (match.Success && int.TryParse(match.Value, out int pNoFromInv))
                        {
                            purchaseNo = pNoFromInv;
                        }
                    }
                }

                // 4. If Pid is not resolved yet, try ResolvePidFromPurchaseNo
                if (pid <= 0 && purchaseNo > 0)
                {
                    pid = ResolvePidFromPurchaseNo(purchaseNo);
                }

                // 5. If Pid is still not resolved, query PMaster directly
                if (pid <= 0)
                {
                    try
                    {
                        using (Repository.BaseRepostitory repo = new Repository.BaseRepostitory())
                        {
                            SqlConnection conn = repo.DataConnection as SqlConnection;
                            if (conn != null)
                            {
                                if (conn.State != ConnectionState.Open)
                                {
                                    conn.Open();
                                }

                                string sql = @"SELECT TOP 1 Pid FROM PMaster 
                                               WHERE (@PNo > 0 AND PurchaseNo = @PNo)
                                                  OR (@InvNo <> '' AND InvoiceNo = @InvNo)
                                               ORDER BY Pid DESC";
                                using (var cmd = new System.Data.SqlClient.SqlCommand(sql, conn))
                                {
                                    cmd.Parameters.AddWithValue("@PNo", purchaseNo > 0 ? purchaseNo : 0);
                                    cmd.Parameters.AddWithValue("@InvNo", invoiceNo ?? string.Empty);
                                    object scalarRes = cmd.ExecuteScalar();
                                    if (scalarRes != null && scalarRes != DBNull.Value && int.TryParse(scalarRes.ToString(), out int foundPid) && foundPid > 0)
                                    {
                                        pid = foundPid;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception exDb)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error querying PMaster directly: {exDb.Message}");
                    }
                }

                if (pid <= 0)
                {
                    MessageBox.Show("Unable to determine the purchase to load for the selected entry.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Find the parent Home form
                Form parentHome = FindParentHome();

                if (parentHome != null)
                {
                    // Check if FrmPurchase is already open in a tab
                    var openFormInTabSafeMethod = parentHome.GetType().GetMethod("OpenFormInTabSafe",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (openFormInTabSafeMethod != null)
                    {
                        // Check if purchase form already exists in a tab
                        var tabControlMainField = parentHome.GetType().GetField("tabControlMain",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                        if (tabControlMainField != null)
                        {
                            var tabControl = tabControlMainField.GetValue(parentHome) as Infragistics.Win.UltraWinTabControl.UltraTabControl;

                            if (tabControl != null)
                            {
                                // Check for existing Purchase tab
                                foreach (Infragistics.Win.UltraWinTabControl.UltraTab tab in tabControl.Tabs)
                                {
                                    if (tab.Text == "Purchase" && tab.TabPage.Controls.Count > 0 &&
                                        tab.TabPage.Controls[0] is PosBranch_Win.Transaction.FrmPurchase existingForm &&
                                        !existingForm.IsDisposed)
                                    {
                                        // Activate existing tab and load data
                                        tabControl.SelectedTab = tab;
                                        existingForm.BringToFront();
                                        existingForm.Focus();
                                        existingForm.LoadPurchaseDataReadOnly(pid);
                                        return;
                                    }
                                }
                            }
                        }

                        // Create new purchase form and open in tab
                        var purchaseForm = new PosBranch_Win.Transaction.FrmPurchase();

                        // Open in tab
                        openFormInTabSafeMethod.Invoke(parentHome, new object[] { purchaseForm, "Purchase" });
                        purchaseForm.BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                purchaseForm.LoadPurchaseDataReadOnly(pid);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error loading purchase data: {ex.Message}");
                                MessageBox.Show("Purchase form opened, but failed to load data. Please try again.",
                                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }));
                        return;
                    }
                }

                // Fallback: Open as standalone window if Home form or method not found
                var existingPurchaseForm = Application.OpenForms
                    .OfType<PosBranch_Win.Transaction.FrmPurchase>()
                    .FirstOrDefault(f => !f.IsDisposed);

                if (existingPurchaseForm == null)
                {
                    var purchaseForm = new PosBranch_Win.Transaction.FrmPurchase();
                    EventHandler loadHandler = null;
                    loadHandler = (s, e) =>
                    {
                        purchaseForm.Load -= loadHandler;
                        purchaseForm.LoadPurchaseDataReadOnly(pid);
                    };
                    purchaseForm.Load += loadHandler;
                    purchaseForm.StartPosition = FormStartPosition.CenterScreen;
                    purchaseForm.Show();
                }
                else
                {
                    if (existingPurchaseForm.WindowState == FormWindowState.Minimized)
                    {
                        existingPurchaseForm.WindowState = FormWindowState.Normal;
                    }
                    existingPurchaseForm.BringToFront();
                    existingPurchaseForm.LoadPurchaseDataReadOnly(pid);
                    existingPurchaseForm.Focus();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening purchase history: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show("Unable to open the purchase history. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Opens FrmVendor and loads the vendor from the currently selected purchase history row.
        /// ultraPanel16 click handler.
        /// </summary>
        private void OpenVendorForSelectedRow()
        {
            try
            {
                var vendorGrid = this.Controls.Find("ultraGrid2", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;
                if (vendorGrid == null)
                {
                    MessageBox.Show("Purchase history grid is not available.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Infragistics.Win.UltraWinGrid.UltraGridRow selectedRow = vendorGrid.ActiveRow;
                if (selectedRow == null && vendorGrid.Selected.Rows.Count > 0)
                    selectedRow = vendorGrid.Selected.Rows[0];

                if (selectedRow == null)
                {
                    MessageBox.Show("Please select a purchase entry to view the vendor.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int ledgerId = 0;
                if (selectedRow.Cells.Exists("LedgerID"))
                    int.TryParse(Convert.ToString(selectedRow.Cells["LedgerID"].Value), out ledgerId);

                if (ledgerId <= 0)
                {
                    MessageBox.Show("Vendor information is not available for the selected entry.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Try to open in a tab via Home form
                Form parentHome = FindParentHome();
                if (parentHome != null)
                {
                    var openFormInTabSafeMethod = parentHome.GetType().GetMethod("OpenFormInTabSafe",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (openFormInTabSafeMethod != null)
                    {
                        var tabControlMainField = parentHome.GetType().GetField("tabControlMain",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (tabControlMainField != null)
                        {
                            var tabControl = tabControlMainField.GetValue(parentHome) as Infragistics.Win.UltraWinTabControl.UltraTabControl;
                            if (tabControl != null)
                            {
                                foreach (Infragistics.Win.UltraWinTabControl.UltraTab tab in tabControl.Tabs)
                                {
                                    if (tab.Text == "Vendor" && tab.TabPage.Controls.Count > 0 &&
                                        tab.TabPage.Controls[0] is PosBranch_Win.Accounts.FrmVendor existingVendorForm &&
                                        !existingVendorForm.IsDisposed)
                                    {
                                        tabControl.SelectedTab = tab;
                                        existingVendorForm.BringToFront();
                                        existingVendorForm.Focus();
                                        existingVendorForm.LoadVendorById(ledgerId);
                                        return;
                                    }
                                }
                            }
                        }

                        int capturedLedgerId = ledgerId;
                        var vendorForm = new PosBranch_Win.Accounts.FrmVendor();
                        EventHandler shownHandler = null;
                        shownHandler = (s, e) =>
                        {
                            vendorForm.Shown -= shownHandler;
                            vendorForm.BeginInvoke(new Action(() =>
                            {
                                try { vendorForm.LoadVendorById(capturedLedgerId); }
                                catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error loading vendor: {ex.Message}"); }
                            }));
                        };
                        vendorForm.Shown += shownHandler;
                        openFormInTabSafeMethod.Invoke(parentHome, new object[] { vendorForm, "Vendor" });
                        return;
                    }
                }

                // Fallback: open as standalone window
                var existingForm = Application.OpenForms.OfType<PosBranch_Win.Accounts.FrmVendor>().FirstOrDefault(f => !f.IsDisposed);
                if (existingForm == null)
                {
                    var newVendorForm = new PosBranch_Win.Accounts.FrmVendor();
                    int capturedLedgerId = ledgerId;
                    newVendorForm.Load += (s, e) => newVendorForm.LoadVendorById(capturedLedgerId);
                    newVendorForm.StartPosition = FormStartPosition.CenterScreen;
                    newVendorForm.Show();
                }
                else
                {
                    if (existingForm.WindowState == FormWindowState.Minimized)
                        existingForm.WindowState = FormWindowState.Normal;
                    existingForm.BringToFront();
                    existingForm.LoadVendorById(ledgerId);
                    existingForm.Focus();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening vendor: {ex.Message}");
                MessageBox.Show("Unable to open the vendor. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Opens frmvendorpurchasereport pre-filtered by the vendor from the currently selected purchase history row.
        /// ultraPanel18 click handler.
        /// </summary>
        private void OpenVendorPurchaseReportForSelectedRow()
        {
            try
            {
                var vendorGrid = this.Controls.Find("ultraGrid2", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;
                if (vendorGrid == null)
                {
                    MessageBox.Show("Purchase history grid is not available.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Infragistics.Win.UltraWinGrid.UltraGridRow selectedRow = vendorGrid.ActiveRow;
                if (selectedRow == null && vendorGrid.Selected.Rows.Count > 0)
                    selectedRow = vendorGrid.Selected.Rows[0];

                if (selectedRow == null)
                {
                    MessageBox.Show("Please select a purchase entry to view the vendor report.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int ledgerId = 0;
                if (selectedRow.Cells.Exists("LedgerID"))
                    int.TryParse(Convert.ToString(selectedRow.Cells["LedgerID"].Value), out ledgerId);

                string vendorName = string.Empty;
                if (selectedRow.Cells.Exists("VendorName"))
                    vendorName = Convert.ToString(selectedRow.Cells["VendorName"].Value) ?? string.Empty;

                if (ledgerId <= 0)
                {
                    MessageBox.Show("Vendor information is not available for the selected entry.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int capturedLedgerId = ledgerId;
                string capturedVendorName = vendorName;

                // Try to open in a tab via Home form
                Form parentHome = FindParentHome();
                if (parentHome != null)
                {
                    var openFormInTabSafeMethod = parentHome.GetType().GetMethod("OpenFormInTabSafe",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (openFormInTabSafeMethod != null)
                    {
                        var tabControlMainField = parentHome.GetType().GetField("tabControlMain",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (tabControlMainField != null)
                        {
                            var tabControl = tabControlMainField.GetValue(parentHome) as Infragistics.Win.UltraWinTabControl.UltraTabControl;
                            if (tabControl != null)
                            {
                                foreach (Infragistics.Win.UltraWinTabControl.UltraTab tab in tabControl.Tabs)
                                {
                                    if (tab.Text == "Vendor Purchase Report" && tab.TabPage.Controls.Count > 0 &&
                                        tab.TabPage.Controls[0] is PosBranch_Win.Reports.PurchaseReports.frmvendorpurchasereport existingRptForm &&
                                        !existingRptForm.IsDisposed)
                                    {
                                        tabControl.SelectedTab = tab;
                                        existingRptForm.BringToFront();
                                        existingRptForm.Focus();
                                        existingRptForm.OpenWithVendor(capturedLedgerId, capturedVendorName);
                                        return;
                                    }
                                }
                            }
                        }

                        var rptForm = new PosBranch_Win.Reports.PurchaseReports.frmvendorpurchasereport();
                        EventHandler shownHandler = null;
                        shownHandler = (s, e) =>
                        {
                            rptForm.Shown -= shownHandler;
                            rptForm.BeginInvoke(new Action(() =>
                            {
                                try { rptForm.OpenWithVendor(capturedLedgerId, capturedVendorName); }
                                catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error opening vendor report: {ex.Message}"); }
                            }));
                        };
                        rptForm.Shown += shownHandler;
                        openFormInTabSafeMethod.Invoke(parentHome, new object[] { rptForm, "Vendor Purchase Report" });
                        return;
                    }
                }

                // Fallback: open as standalone window
                var existingRpt = Application.OpenForms.OfType<PosBranch_Win.Reports.PurchaseReports.frmvendorpurchasereport>().FirstOrDefault(f => !f.IsDisposed);
                if (existingRpt == null)
                {
                    var newRptForm = new PosBranch_Win.Reports.PurchaseReports.frmvendorpurchasereport();
                    newRptForm.Load += (s, e) => newRptForm.OpenWithVendor(capturedLedgerId, capturedVendorName);
                    newRptForm.StartPosition = FormStartPosition.CenterScreen;
                    newRptForm.Show();
                }
                else
                {
                    if (existingRpt.WindowState == FormWindowState.Minimized)
                        existingRpt.WindowState = FormWindowState.Normal;
                    existingRpt.BringToFront();
                    existingRpt.OpenWithVendor(capturedLedgerId, capturedVendorName);
                    existingRpt.Focus();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening vendor purchase report: {ex.Message}");
                MessageBox.Show("Unable to open the vendor purchase report. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenStockAdjustmentInTab()
        {
            try
            {
                // Check if an item is loaded
                if (CurrentItemId <= 0)
                {
                    MessageBox.Show("Please load an item first before opening the stock adjustment form.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Get current item data
                string itemId = CurrentItemId.ToString();
                string barcode = "";
                string description = "";
                string unit = "";
                string stockQty = "0";

                // Get barcode from txt_barcode field
                var txtBarcodeCtrl = GetMainBarcodeEditor();
                if (txtBarcodeCtrl != null)
                {
                    barcode = txtBarcodeCtrl.Text?.Trim() ?? "";
                }

                // Get description from txt_description field
                if (txt_description != null)
                {
                    description = txt_description.Text?.Trim() ?? "";
                }

                // Get unit from txt_BaseUnit field
                if (txt_BaseUnit != null)
                {
                    unit = txt_BaseUnit.Text?.Trim() ?? "";
                }

                // Get stock quantity from txt_qty or txt_available field
                var txtQtyCtrl = this.Controls.Find("txt_qty", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                if (txtQtyCtrl != null && !string.IsNullOrEmpty(txtQtyCtrl.Text))
                {
                    stockQty = txtQtyCtrl.Text.Trim();
                }
                else
                {
                    var txtAvailableCtrl = this.Controls.Find("txt_available", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                    if (txtAvailableCtrl != null && !string.IsNullOrEmpty(txtAvailableCtrl.Text))
                    {
                        stockQty = txtAvailableCtrl.Text.Trim();
                    }
                }

                // Find the parent Home form
                Form parentHome = FindParentHome();

                if (parentHome != null)
                {
                    // Check if FrmStockAdjustment is already open in a tab
                    var openFormInTabSafeMethod = parentHome.GetType().GetMethod("OpenFormInTabSafe",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (openFormInTabSafeMethod != null)
                    {
                        // Check if stock adjustment form already exists in a tab
                        var tabControlMainField = parentHome.GetType().GetField("tabControlMain",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                        PosBranch_Win.Transaction.FrmStockAdjustment stockAdjustmentForm = null;

                        if (tabControlMainField != null)
                        {
                            var tabControl = tabControlMainField.GetValue(parentHome) as Infragistics.Win.UltraWinTabControl.UltraTabControl;

                            if (tabControl != null)
                            {
                                // Check for existing Stock Adjustment tab
                                foreach (Infragistics.Win.UltraWinTabControl.UltraTab tab in tabControl.Tabs)
                                {
                                    if (tab.Text == "Stock Adjustment" && tab.TabPage.Controls.Count > 0 &&
                                        tab.TabPage.Controls[0] is PosBranch_Win.Transaction.FrmStockAdjustment existingForm &&
                                        !existingForm.IsDisposed)
                                    {
                                        // Activate existing tab
                                        stockAdjustmentForm = existingForm;
                                        tabControl.SelectedTab = tab;
                                        existingForm.BringToFront();
                                        existingForm.Focus();

                                        // Load the current item into the stock adjustment form
                                        stockAdjustmentForm.AddItemToGrid(itemId, barcode, description, unit, stockQty);
                                        return;
                                    }
                                }
                            }
                        }

                        // Create new stock adjustment form and open in tab
                        stockAdjustmentForm = new PosBranch_Win.Transaction.FrmStockAdjustment();

                        // Open in tab - the form's Load event will handle initialization
                        openFormInTabSafeMethod.Invoke(parentHome, new object[] { stockAdjustmentForm, "Stock Adjustment" });

                        // Wait for form to load, then add the item
                        // Use BeginInvoke to ensure form is fully loaded
                        stockAdjustmentForm.BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                stockAdjustmentForm.AddItemToGrid(itemId, barcode, description, unit, stockQty);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error adding item to stock adjustment: {ex.Message}");
                            }
                        }));
                        return;
                    }
                }

                // Fallback: Open as standalone window if Home form or method not found
                var existingStockAdjustmentForm = Application.OpenForms
                    .OfType<PosBranch_Win.Transaction.FrmStockAdjustment>()
                    .FirstOrDefault(f => !f.IsDisposed);

                if (existingStockAdjustmentForm == null)
                {
                    var stockAdjustmentForm = new PosBranch_Win.Transaction.FrmStockAdjustment();
                    stockAdjustmentForm.StartPosition = FormStartPosition.CenterScreen;
                    stockAdjustmentForm.Show();

                    // Wait for form to load, then add the item
                    stockAdjustmentForm.Shown += (s, e) =>
                    {
                        try
                        {
                            stockAdjustmentForm.AddItemToGrid(itemId, barcode, description, unit, stockQty);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error adding item to stock adjustment: {ex.Message}");
                        }
                    };
                }
                else
                {
                    if (existingStockAdjustmentForm.WindowState == FormWindowState.Minimized)
                    {
                        existingStockAdjustmentForm.WindowState = FormWindowState.Normal;
                    }
                    existingStockAdjustmentForm.BringToFront();
                    existingStockAdjustmentForm.Focus();

                    // Load the current item into the stock adjustment form
                    existingStockAdjustmentForm.AddItemToGrid(itemId, barcode, description, unit, stockQty);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening stock adjustment: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show("Unable to open the stock adjustment form. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenBarcodeFormInTab()
        {
            try
            {
                // Check if an item is loaded - if yes, get item data; if no, just open the form
                bool isItemLoaded = CurrentItemId > 0;

                // Get current item data (will be empty if no item is loaded)
                string barcode = "";
                string description = "";
                decimal retailPrice = 0;

                if (isItemLoaded)
                {
                    // Get barcode from txt_barcode field
                    var txtBarcodeCtrl = GetMainBarcodeEditor();
                    if (txtBarcodeCtrl != null)
                    {
                        barcode = txtBarcodeCtrl.Text?.Trim() ?? "";
                    }

                    // Get description from txt_description field
                    if (txt_description != null)
                    {
                        description = txt_description.Text?.Trim() ?? "";
                    }

                    // Get retail price from txt_walkin or txt_Retail field
                    var txtWalkinCtrl = this.Controls.Find("txt_walkin", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                    if (txtWalkinCtrl != null && !string.IsNullOrEmpty(txtWalkinCtrl.Text))
                    {
                        decimal.TryParse(txtWalkinCtrl.Text, out retailPrice);
                    }
                    else
                    {
                        var txtRetailCtrl = this.Controls.Find("txt_Retail", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                        if (txtRetailCtrl != null && !string.IsNullOrEmpty(txtRetailCtrl.Text))
                        {
                            decimal.TryParse(txtRetailCtrl.Text, out retailPrice);
                        }
                    }
                }

                // Find the parent Home form
                Form parentHome = FindParentHome();

                if (parentHome != null)
                {
                    // Check if frmBarcode is already open in a tab
                    var openFormInTabSafeMethod = parentHome.GetType().GetMethod("OpenFormInTabSafe",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (openFormInTabSafeMethod != null)
                    {
                        // Check if barcode form already exists in a tab
                        var tabControlMainField = parentHome.GetType().GetField("tabControlMain",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                        if (tabControlMainField != null)
                        {
                            var tabControl = tabControlMainField.GetValue(parentHome) as Infragistics.Win.UltraWinTabControl.UltraTabControl;

                            if (tabControl != null)
                            {
                                // Check for existing Barcode tab
                                foreach (Infragistics.Win.UltraWinTabControl.UltraTab tab in tabControl.Tabs)
                                {
                                    if (tab.Text == "Barcode" && tab.TabPage.Controls.Count > 0 &&
                                        tab.TabPage.Controls[0] is PosBranch_Win.Utilities.frmBarcode existingForm &&
                                        !existingForm.IsDisposed)
                                    {
                                        // Activate existing tab
                                        tabControl.SelectedTab = tab;
                                        existingForm.BringToFront();
                                        existingForm.Focus();

                                        // Only load current item into the barcode form if an item is loaded
                                        if (isItemLoaded)
                                        {
                                            existingForm.LoadItemFromItemId(CurrentItemId, barcode, description, retailPrice);
                                        }
                                        return;
                                    }
                                }
                            }
                        }

                        // Create new barcode form and open in tab
                        var barcodeForm = new PosBranch_Win.Utilities.frmBarcode();

                        // Open in tab
                        openFormInTabSafeMethod.Invoke(parentHome, new object[] { barcodeForm, "Barcode" });

                        // Only load current item into the barcode form if an item is loaded
                        if (isItemLoaded)
                        {
                            barcodeForm.LoadItemFromItemId(CurrentItemId, barcode, description, retailPrice);
                        }
                        return;
                    }
                }

                // Fallback: Open as standalone window if Home form or method not found
                var existingBarcodeForm = Application.OpenForms
                    .OfType<PosBranch_Win.Utilities.frmBarcode>()
                    .FirstOrDefault(f => !f.IsDisposed);

                if (existingBarcodeForm == null)
                {
                    var barcodeForm = new PosBranch_Win.Utilities.frmBarcode();
                    barcodeForm.StartPosition = FormStartPosition.CenterScreen;
                    barcodeForm.Show();

                    // Only load current item into the barcode form if an item is loaded
                    if (isItemLoaded)
                    {
                        barcodeForm.LoadItemFromItemId(CurrentItemId, barcode, description, retailPrice);
                    }
                }
                else
                {
                    if (existingBarcodeForm.WindowState == FormWindowState.Minimized)
                    {
                        existingBarcodeForm.WindowState = FormWindowState.Normal;
                    }
                    existingBarcodeForm.BringToFront();
                    existingBarcodeForm.Focus();

                    // Only load current item into the barcode form if an item is loaded
                    if (isItemLoaded)
                    {
                        existingBarcodeForm.LoadItemFromItemId(CurrentItemId, barcode, description, retailPrice);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening barcode form: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show("Unable to open the barcode form. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Form FindParentHome()
        {
            // Try to find the Home form by traversing up the parent chain
            Control current = this.Parent;
            while (current != null)
            {
                if (current is Form form && form.GetType().Name == "Home")
                {
                    return form;
                }
                current = current.Parent;
            }

            // If not found in parent chain, search through open forms
            foreach (Form form in Application.OpenForms)
            {
                if (form.GetType().Name == "Home" && !form.IsDisposed)
                {
                    return form;
                }
            }

            return null;
        }

        // Navigate to an item based on the navigation type
        private void NavigateToItem(string navigationType)
        {
            try
            {
                int currentItemNo = 0;
                if (!string.IsNullOrEmpty(txt_ItemNo.Text))
                {
                    int.TryParse(txt_ItemNo.Text, out currentItemNo);
                }

                if ((navigationType == "PREVIOUS" || navigationType == "NEXT") && currentItemNo <= 0)
                {
                    return;
                }

                int itemId = ItemRepository.NavigateItem(navigationType, currentItemNo);

                if (itemId <= 0)
                {
                    if (navigationType == "NEXT")
                    {
                        MessageBox.Show("This is the last available item.", "End of Items",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (navigationType == "PREVIOUS")
                    {
                        MessageBox.Show("This is the first available item.", "Start of Items",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("No items found.", "No Items",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    return;
                }

                LoadItemById(itemId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navigating to {navigationType} item: {ex.Message}");
                MessageBox.Show($"Error navigating to {navigationType} item: {ex.Message}",
                    "Navigation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EnsureDefaultItemType()
        {
            try
            {
                if (txt_ItemType != null && string.IsNullOrWhiteSpace(txt_ItemType.Text))
                {
                    var repo = new Repository.MasterRepositry.ItemTypeRepository();
                    var defaultType = repo.GetDefaultItemType();
                    if (defaultType != null && !string.IsNullOrWhiteSpace(defaultType.ItemTypeName))
                    {
                        txt_ItemType.Text = defaultType.ItemTypeName;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EnsureDefaultItemType error: {ex.Message}");
            }
        }

        // Load an item by its ID
        private void LoadItemById(int itemId)
        {
            try
            {
                // Set flag to prevent master field behavior during loading
                isLoadingItem = true;

                // Clear existing data
                ClearAllFields();
                // CRITICAL: ClearAllFields() resets isLoadingItem to false in its finally block.
                // We MUST set it back to true here to protect the rest of the loading process.
                isLoadingItem = true;

                // Set current item ID for hold details
                SetCurrentItemId(itemId);

                // Get the item data from the repository
                ItemMasterRepository itemRepo = new ItemMasterRepository();
                ItemGet getItem = itemRepo.GetByIdItem(itemId);

                System.Diagnostics.Debug.WriteLine($"LoadItemById: ItemId={itemId}, getItem is null: {getItem == null}");
                System.Diagnostics.Debug.WriteLine($"LoadItemById: List contains {getItem?.List?.Length ?? 0} units");

                if (getItem != null)
                {
                    // CRITICAL: Populate the ItemMaster object from loaded data for proper update operations
                    ItemMaster.ItemId = getItem.ItemId;
                    ItemMaster.ItemNo = Convert.ToInt32(getItem.ItemNo ?? "0");
                    ItemMaster.Description = getItem.Description;
                    ItemMaster.Barcode = getItem.Barcode;
                    ItemMaster.ItemTypeId = getItem.ItemTypeId;
                    ItemMaster.VendorId = getItem.VendorId;
                    ItemMaster.BrandId = getItem.BrandId;
                    ItemMaster.GroupId = getItem.GroupId;
                    ItemMaster.CategoryId = getItem.CategoryId;
                    ItemMaster.BaseUnitId = getItem.BaseUnitId;
                    ItemMaster.ForCustomerType = getItem.ForCustomerType;
                    ItemMaster.NameInLocalLanguage = getItem.NameInLocalLanguage;
                    ItemMaster.HSNCode = getItem.HSNCode;
                    ItemMaster.Order_Cycle_Days = getItem.Order_Cycle_Days > 0 ? getItem.Order_Cycle_Days : 0;
                    ItemMaster.Box_Quantity = getItem.Box_Quantity > 0 ? getItem.Box_Quantity : 0;
                    ItemMaster.Is_Perishable = getItem.Is_Perishable;
                    ItemMaster.CompanyId = getItem.CompanyId;
                    ItemMaster.BranchId = getItem.BranchId;
                    ItemMaster.FinYearId = getItem.FinYearId;

                    // Set the item number in UI
                    txt_ItemNo.Text = getItem.ItemNo.ToString();
                    int loadedItemNo;
                    if (int.TryParse(getItem.ItemNo, out loadedItemNo) && loadedItemNo > 0)
                        lastLoadedItemNo = loadedItemNo;

                    // Populate the form fields

                    txt_description.Text = getItem.Description;
                    txt_LocalLanguage.Text = getItem.NameInLocalLanguage;
                    // NOTE: Do NOT set txt_BaseUnit.Text here - set it AFTER populating the UOM grid
                    // to prevent SynchronizeBaseUnitWithGrid from clearing the grid before it's populated.
                    // We will populate ALL saved unit rows into ultraGrid1 below from getItem.List
                    // to ensure multiple units (e.g., 1 UNIT, 4 OTR) are loaded exactly like btn_ItemLoad.

                    // Additional item details
                    txt_Brand.Text = getItem.BrandName;
                    txt_Category.Text = getItem.CategoryName;

                    txt_CustomerType.Text = getItem.ForCustomerType;
                    txt_Group.Text = getItem.GroupName;

                    txt_ItemType.Text = getItem.ItemType;
                    EnsureDefaultItemType();
                    SetSmartReorderValues(getItem.Order_Cycle_Days, getItem.Box_Quantity, getItem.Is_Perishable);

                    // Load H.S.N code into textBox4 using repository's enriched result (which explicitly fetched HSNCode)
                    try
                    {
                        var hsnTextBox = this.Controls.Find("textBox4", true).FirstOrDefault() as TextBox;
                        if (hsnTextBox != null)
                        {
                            string hsn = string.Empty;
                            try { hsn = getItem.HSNCode; } catch { hsn = string.Empty; }
                            hsnTextBox.Text = hsn ?? string.Empty;
                        }
                    }
                    catch { }

                    // Load barcode into txt_barcode text field
                    // CRITICAL: Stored procedure's first table doesn't return Barcode,
                    // so we try to get it from the first price setting row if available.
                    try
                    {
                        var txtBarcodeCtrl = GetMainBarcodeEditor();
                        if (txtBarcodeCtrl != null)
                        {
                            string barcode = getItem.Barcode;
                            if (string.IsNullOrWhiteSpace(barcode) && getItem.List != null && getItem.List.Length > 0)
                            {
                                barcode = getItem.List[0].BarCode;
                            }

                            txtBarcodeCtrl.Text = barcode ?? string.Empty;
                            SetLoadedItemBarcode(txtBarcodeCtrl.Text);
                            System.Diagnostics.Debug.WriteLine($"Loaded barcode into txt_barcode: {txtBarcodeCtrl.Text}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading barcode into txt_barcode: {ex.Message}");
                    }



                    // Handle price and unit data if available

                    // Load Alternative Barcodes
                    try
                    {
                        var altGrid = this.Controls.Find("ultraGrid3", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;
                        if (altGrid != null && altGrid.DataSource is DataTable dtAlt)
                        {
                            dtAlt.Rows.Clear();
                            HashSet<string> aliasBarcodesForItem = BuildAliasBarcodeSet(getItem.List);
                            HashSet<string> addedAlternativeBarcodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            if (getItem.ListAlternativeBarcodes != null)
                            {
                                foreach (var altBcode in getItem.ListAlternativeBarcodes)
                                {
                                    string alternativeBarcode = altBcode?.Barcode?.Trim() ?? string.Empty;
                                    if (string.IsNullOrWhiteSpace(alternativeBarcode) ||
                                        aliasBarcodesForItem.Contains(alternativeBarcode) ||
                                        !addedAlternativeBarcodes.Add(alternativeBarcode))
                                    {
                                        continue;
                                    }

                                    DataRow newRow = dtAlt.NewRow();
                                    newRow["Barcode"] = alternativeBarcode;
                                    dtAlt.Rows.Add(newRow);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading alternative barcodes: {ex.Message}");
                    }

                    // Load Vendor Purchase History into ultraGrid2
                    try
                    {
                        var vendorGrid = this.Controls.Find("ultraGrid2", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;
                        if (vendorGrid != null)
                        {
                            DataTable dtVendor = new DataTable();
                            dtVendor.Columns.Add("LedgerID", typeof(int));
                            dtVendor.Columns.Add("VendorName", typeof(string));
                            dtVendor.Columns.Add("Cost", typeof(double));
                            dtVendor.Columns.Add("Unit", typeof(string));
                            dtVendor.Columns.Add("InvoiceDate", typeof(DateTime));
                            dtVendor.Columns.Add("PurchaseNo", typeof(int));
                            dtVendor.Columns.Add("InvoiceNo", typeof(string));

                            if (getItem.ListVendor != null)
                            {
                                foreach (var vDet in getItem.ListVendor)
                                {
                                    DataRow vRow = dtVendor.NewRow();
                                    vRow["LedgerID"] = vDet.LedgerID;
                                    vRow["VendorName"] = vDet.VendorName ?? string.Empty;
                                    vRow["Cost"] = vDet.Cost;
                                    vRow["Unit"] = vDet.Unit ?? string.Empty;
                                    vRow["InvoiceDate"] = vDet.InvoiceDate;
                                    vRow["PurchaseNo"] = vDet.PurchaseNo;
                                    vRow["InvoiceNo"] = vDet.InvoiceNo ?? string.Empty;
                                    dtVendor.Rows.Add(vRow);
                                }
                            }
                            vendorGrid.DataSource = dtVendor;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading purchase history into ultraGrid2: {ex.Message}");
                    }

                    if (getItem.List != null && getItem.List.Length > 0)
                    {
                        if (getItem.List[0] != null)
                        {
                            // Txt_UnitCost always shows 3 decimal places (.000) on load
                            Txt_UnitCost.Text = getItem.List[0].Cost.ToString("0.000");

                            // Set tax information
                            txt_TaxType.Text = getItem.List[0].TaxType;
                            txt_TaxPer.Text = getItem.List[0].TaxPer.ToString();
                            // Do not directly set isinclexcl or txt_TaxAmount; use recompute instead

                            // Set stock values if available
                            // In PriceSettings, Stock already represents (actual stock + held quantity)
                            float stock = (float)getItem.List[0].Stock;          // total stock from PriceSettings (includes held)
                            float orderedStock = (float)getItem.List[0].OrderedStock; // held quantity

                            // Double check directly from DB to guarantee 100% accurate post-purchase stock quantities
                            try
                            {
                                using (Repository.BaseRepostitory bRepo = new Repository.BaseRepostitory())
                                {
                                    if (bRepo.DataConnection is System.Data.SqlClient.SqlConnection dbConn)
                                    {
                                        bool wasClosed = dbConn.State == ConnectionState.Closed;
                                        if (wasClosed) dbConn.Open();
                                        try
                                        {
                                            using (System.Data.SqlClient.SqlCommand sCmd = new System.Data.SqlClient.SqlCommand(
                                                "SELECT ISNULL(SUM(Stock), 0) AS TotalStock, ISNULL(SUM(OrderedStock), 0) AS TotalHold FROM ItemMasterPriceSettings WHERE ItemId = @ItemId AND (IsBaseUnit = 'Y' OR Packing = 1)",
                                                dbConn))
                                            {
                                                sCmd.Parameters.AddWithValue("@ItemId", itemId);
                                                using (System.Data.SqlClient.SqlDataReader sRdr = sCmd.ExecuteReader())
                                                {
                                                    if (sRdr.Read())
                                                    {
                                                        double dbStock = Convert.ToDouble(sRdr["TotalStock"]);
                                                        double dbHold = Convert.ToDouble(sRdr["TotalHold"]);
                                                        if (dbStock > 0 || stock == 0)
                                                        {
                                                            stock = (float)dbStock;
                                                            orderedStock = (float)dbHold;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        finally
                                        {
                                            if (wasClosed && dbConn.State == ConnectionState.Open) dbConn.Close();
                                        }
                                    }
                                }
                            }
                            catch { }

                            // txt_qty should show the total stock value from PriceSettings
                            txt_qty.Text = stock.ToString("0");

                            // txt_hold shows the held quantity
                            txt_hold.Text = orderedStock.ToString("0");

                            // txt_available = total stock - held quantity
                            float availableQty = stock - orderedStock;
                            txt_available.Text = availableQty.ToString("0");

                            // Set walking price (DB.RetailPrice stores walking price)
                            if (txt_walkin != null)
                            {
                                txt_walkin.Text = getItem.List[0].RetailPrice.ToString("0.000");
                            }

                            // Set retail price (DB.WholeSalePrice stores retail price)
                            if (txt_Retail != null)
                            {
                                txt_Retail.Text = getItem.List[0].WholeSalePrice.ToString("0.000");
                            }

                            // Set credit price
                            if (txt_CEP != null)
                            {
                                txt_CEP.Text = getItem.List[0].CreditPrice.ToString("0.000");
                            }

                            // Set MRP
                            if (txt_Mrp != null)
                            {
                                txt_Mrp.Text = getItem.List[0].MRP.ToString("0.000");
                            }

                            // Set Card Price
                            if (txt_CardP != null)
                            {
                                txt_CardP.Text = getItem.List[0].CardPrice.ToString("0.000");
                            }

                            // Load markdown values from the first price setting record
                            if (getItem.List.Length > 0)
                            {
                                var priceSettings = getItem.List[0];

                                // Load walking markdown (ultraTextEditor16)
                                if (ultraTextEditor16 != null)
                                {
                                    ultraTextEditor16.Text = priceSettings.MDWalkinPrice.ToString("0.00");
                                }

                                // Load credit markdown (ultraTextEditor15)
                                if (ultraTextEditor15 != null)
                                {
                                    ultraTextEditor15.Text = priceSettings.MDCreditPrice.ToString("0.00");
                                }

                                // Load MRP markdown (ultraTextEditor14)
                                if (ultraTextEditor14 != null)
                                {
                                    ultraTextEditor14.Text = priceSettings.MDMrpPrice.ToString("0.00");
                                }

                                // Load card markdown (ultraTextEditor13)
                                if (ultraTextEditor13 != null)
                                {
                                    ultraTextEditor13.Text = priceSettings.MDCardPrice.ToString("0.00");
                                }
                                var ultraTextEditor12 = this.Controls.Find("ultraTextEditor12", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                                var ultraTextEditor11 = this.Controls.Find("ultraTextEditor11", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                                if (ultraTextEditor12 != null) ultraTextEditor12.Text = priceSettings.MDStaffPrice.ToString("0.00");
                                if (ultraTextEditor11 != null) ultraTextEditor11.Text = priceSettings.MDMinPrice.ToString("0.00");
                            }

                            // Set txt_SF with StaffPrice from database
                            var txt_SF = this.Controls.Find("txt_SF", true).FirstOrDefault() as Control;
                            if (txt_SF != null)
                            {
                                txt_SF.Text = getItem.List[0].StaffPrice.ToString("0.000");
                            }

                            // Set txt_MinP with MinPrice from database
                            var txt_MinP = this.Controls.Find("txt_MinP", true).FirstOrDefault() as Control;
                            if (txt_MinP != null)
                            {
                                txt_MinP.Text = getItem.List[0].MinPrice.ToString("0.000");
                            }

                            // Load item image using repository helper that reads from PriceSettings
                            try
                            {
                                byte[] photoBytes = ItemRepository.GetItemPhoto(itemId);
                                SetCurrentImage(photoBytes);
                            }
                            catch (Exception imgEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error loading item image: {imgEx.Message}");
                            }
                        }

                        // Create DataTable for UOM grid
                        DataTable dtUom = new DataTable();
                        dtUom.Columns.Add("Unit", typeof(string));
                        dtUom.Columns.Add("UnitId", typeof(string));
                        dtUom.Columns.Add("Packing", typeof(string));
                        // BarCode column removed as per request
                        dtUom.Columns.Add("Reorder", typeof(string));
                        dtUom.Columns.Add("OpnStk", typeof(string));
                        dtUom.Columns.Add("AliasBarcode", typeof(string));

                        // Add rows to the UOM DataTable
                        System.Diagnostics.Debug.WriteLine($"LoadItemById: About to add {getItem.List?.Length ?? 0} units to dtUom");
                        foreach (var item in getItem.List)
                        {
                            System.Diagnostics.Debug.WriteLine($"LoadItemById: Adding unit '{item.Unit}' Packing={item.Packing} UnitId={item.UnitId}");
                            DataRow row = dtUom.NewRow();
                            row["Unit"] = item.Unit;
                            row["UnitId"] = item.UnitId.ToString();
                            row["Packing"] = item.Packing.ToString();
                            // row["BarCode"] removed
                            row["Reorder"] = item.ReOrder.ToString();
                            row["OpnStk"] = item.OpnStk.ToString();
                            row["AliasBarcode"] = item.AliasBarcode ?? string.Empty;
                            dtUom.Rows.Add(row);
                        }
                        System.Diagnostics.Debug.WriteLine($"LoadItemById: dtUom now has {dtUom.Rows.Count} rows");

                        // Set the DataTable as DataSource for ultraGrid1
                        SetUltraGridDataSource(dtUom);

                        // Now set the base unit text AFTER the UOM grid is populated
                        // This prevents the SynchronizeBaseUnitWithGrid from clearing the grid
                        txt_BaseUnit.Text = getItem.UnitName;

                        // Populate the price grid (Ult_Price)
                        Infragistics.Win.UltraWinGrid.UltraGrid Ult_Price =
                            this.Controls.Find("Ult_Price", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;

                        if (Ult_Price != null)
                        {
                            // Create DataTable for Ult_Price with proper column types
                            DataTable dtPrice = new DataTable();
                            dtPrice.Columns.Add("Unit", typeof(string));
                            dtPrice.Columns.Add("Packing", typeof(int));
                            dtPrice.Columns.Add("Cost", typeof(float));
                            dtPrice.Columns.Add("MarginAmt", typeof(float));
                            dtPrice.Columns.Add("MarginPer", typeof(float));
                            dtPrice.Columns.Add("TaxPer", typeof(float));
                            dtPrice.Columns.Add("TaxAmt", typeof(float));
                            dtPrice.Columns.Add("RetailPrice", typeof(float));
                            dtPrice.Columns.Add("MRP", typeof(float));
                            dtPrice.Columns.Add("WholeSalePrice", typeof(float));
                            dtPrice.Columns.Add("CreditPrice", typeof(float));
                            dtPrice.Columns.Add("CardPrice", typeof(float));
                            dtPrice.Columns.Add("StaffPrice", typeof(float));
                            dtPrice.Columns.Add("MinPrice", typeof(float));

                            // Add rows to the DataTable with proper type conversion
                            for (int i = 0; getItem.List.Length > i; i++)
                            {
                                DataRow row = dtPrice.NewRow();
                                row["Unit"] = getItem.List[i].Unit;
                                row["Packing"] = Convert.ToInt32(getItem.List[i].Packing);
                                row["Cost"] = getItem.List[i].Cost;
                                row["MarginAmt"] = getItem.List[i].MarginAmt;
                                row["MarginPer"] = getItem.List[i].MarginPer;
                                row["TaxPer"] = getItem.List[i].TaxPer;
                                row["TaxAmt"] = getItem.List[i].TaxAmt;
                                row["MRP"] = getItem.List[i].MRP;
                                row["RetailPrice"] = getItem.List[i].WholeSalePrice; // DB.WholeSalePrice = retail ? grid RetailPrice (visual "Retail Price")
                                row["WholeSalePrice"] = getItem.List[i].RetailPrice; // DB.RetailPrice = walking ? grid WholeSalePrice (visual "Walking Price")
                                row["CreditPrice"] = getItem.List[i].CreditPrice;
                                row["CardPrice"] = getItem.List[i].CardPrice;
                                if (dtPrice.Columns.Contains("StaffPrice")) row["StaffPrice"] = getItem.List[i].StaffPrice;
                                if (dtPrice.Columns.Contains("MinPrice")) row["MinPrice"] = getItem.List[i].MinPrice;
                                if (dtPrice.Columns.Contains("AliasBarcode")) row["AliasBarcode"] = getItem.List[i].AliasBarcode ?? string.Empty;
                                dtPrice.Rows.Add(row);
                            }

                            // Set the DataTable as the DataSource for Ult_Price
                            Ult_Price.DataSource = dtPrice;
                            SyncUomGridWithPriceGrid();

                            // Configure columns if needed
                            if (Ult_Price.DisplayLayout.Bands.Count > 0)
                            {
                                // Format numeric columns
                                Ult_Price.DisplayLayout.Bands[0].Columns["Cost"].Format = "N2";
                                Ult_Price.DisplayLayout.Bands[0].Columns["MarginAmt"].Format = "N2";
                                Ult_Price.DisplayLayout.Bands[0].Columns["MarginPer"].Format = "N2";
                                Ult_Price.DisplayLayout.Bands[0].Columns["TaxPer"].Format = "N2";
                                Ult_Price.DisplayLayout.Bands[0].Columns["TaxAmt"].Format = "N2";
                                Ult_Price.DisplayLayout.Bands[0].Columns["MRP"].Format = "N2";
                                Ult_Price.DisplayLayout.Bands[0].Columns["RetailPrice"].Format = "N2";
                                Ult_Price.DisplayLayout.Bands[0].Columns["WholeSalePrice"].Format = "N2";
                                Ult_Price.DisplayLayout.Bands[0].Columns["CreditPrice"].Format = "N2";
                                Ult_Price.DisplayLayout.Bands[0].Columns["CardPrice"].Format = "N2";
                                if (Ult_Price.DisplayLayout.Bands[0].Columns.Exists("StaffPrice"))
                                    Ult_Price.DisplayLayout.Bands[0].Columns["StaffPrice"].Format = "N2";
                                if (Ult_Price.DisplayLayout.Bands[0].Columns.Exists("MinPrice"))
                                    Ult_Price.DisplayLayout.Bands[0].Columns["MinPrice"].Format = "N2";
                                if (Ult_Price.DisplayLayout.Bands[0].Columns.Exists("StaffPrice"))
                                    Ult_Price.DisplayLayout.Bands[0].Columns["StaffPrice"].Format = "N2";
                                if (Ult_Price.DisplayLayout.Bands[0].Columns.Exists("MinPrice"))
                                    Ult_Price.DisplayLayout.Bands[0].Columns["MinPrice"].Format = "N2";
                            }

                            // Do not overwrite markup textbox (textBox1) from DB-derived margin values
                        }
                    }

                    // Update hold quantity from hold details
                    UpdateHoldQuantityFromHoldDetails();

                    // Update all profit margins after loading item data
                    UpdateAllProfitMargins();
                    RecalculateMarkupPercentage(true);

                    // Note: Barcode in ultraGrid1 acts as independent alias barcode (no sync with txt_barcode)

                    // Update visibility of buttons
                    btnUpdate.Visible = true;
                    button3.Visible = false;

                    // Apply UI updates immediately
                    this.Refresh();
                    Application.DoEvents();

                    // Update tax amount display (isinclexcl) based on current Retail and Tax %
                    UpdateInclusiveExclusiveTaxDisplay();

                    // NEW: Ensure markdown fields are reloaded from detailed price settings if available
                    try
                    {
                        var detailedPriceList = ItemRepository.GetItemPriceSettings(itemId);
                        if (detailedPriceList != null && detailedPriceList.Count > 0)
                        {
                            var ps0 = detailedPriceList[0];
                            if (ultraTextEditor16 != null) ultraTextEditor16.Text = ps0.MDWalkinPrice.ToString("0.00");
                            if (ultraTextEditor15 != null) ultraTextEditor15.Text = ps0.MDCreditPrice.ToString("0.00");
                            if (ultraTextEditor14 != null) ultraTextEditor14.Text = ps0.MDMrpPrice.ToString("0.00");
                            if (ultraTextEditor13 != null) ultraTextEditor13.Text = ps0.MDCardPrice.ToString("0.00");
                            var ultraTextEditor12b = this.Controls.Find("ultraTextEditor12", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                            var ultraTextEditor11b = this.Controls.Find("ultraTextEditor11", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                            if (ultraTextEditor12b != null) ultraTextEditor12b.Text = ps0.MDStaffPrice.ToString("0.00");
                            if (ultraTextEditor11b != null) ultraTextEditor11b.Text = ps0.MDMinPrice.ToString("0.00");
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading item {itemId}: {ex.Message}");
            }
            finally
            {
                // Delay resetting the flag to ensure any pending TextChanged timers (500ms)
                // complete while isLoadingItem is still true, preventing grid clearing
                System.Threading.Timer delayTimer = null;
                delayTimer = new System.Threading.Timer((state) =>
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        isLoadingItem = false;
                        System.Diagnostics.Debug.WriteLine("LoadItemById: isLoadingItem set to false after delay");
                        delayTimer?.Dispose();
                    }));
                }, null, 600, System.Threading.Timeout.Infinite);
            }
        }

        // Attempt to refresh the item selection dialog list after save/update
        private void TryRefreshItemDialog()
        {
            try
            {
                var dlg = Application.OpenForms["frmdialForItemMaster"] as PosBranch_Win.DialogBox.frmdialForItemMaster;
                if (dlg != null)
                {
                    // Reload from DB if method exists; fallback to re-applying current filter.
                    var reloadMethod = dlg.GetType().GetMethod("LoadAllDataAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (reloadMethod != null)
                    {
                        reloadMethod.Invoke(dlg, null);
                    }
                    else
                    {
                        var tb = dlg.Controls.Find("textBox1", true).FirstOrDefault() as TextBox;
                        string search = tb != null ? (tb.Text ?? string.Empty) : string.Empty;
                        var applyFilterMethod = dlg.GetType().GetMethod("ApplyFilter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (applyFilterMethod != null)
                        {
                            applyFilterMethod.Invoke(dlg, new object[] { search });
                        }
                    }
                }
            }
            catch { }
        }

        private void EnsureUomGridPriceColumns(DataTable dt)
        {
            if (dt == null) return;

            EnsureUomGridColumn(dt, "Cost");
            EnsureUomGridColumn(dt, "MarginAmt");
            EnsureUomGridColumn(dt, "MarginPer");
            EnsureUomGridColumn(dt, "TaxPer");
            EnsureUomGridColumn(dt, "TaxAmt");
            EnsureUomGridColumn(dt, "RetailPrice");
            EnsureUomGridColumn(dt, "MRP");
            EnsureUomGridColumn(dt, "WholeSalePrice");
            EnsureUomGridColumn(dt, "CreditPrice");
            EnsureUomGridColumn(dt, "CardPrice");
            EnsureUomGridColumn(dt, "StaffPrice");
            EnsureUomGridColumn(dt, "MinPrice");
            EnsureUomGridStringColumn(dt, "AliasBarcode");
            EnsureRetailPriceMrpDataColumnOrder(dt);
        }

        private void EnsureRetailPriceMrpDataColumnOrder(DataTable dt)
        {
            if (dt == null || !dt.Columns.Contains("RetailPrice") || !dt.Columns.Contains("MRP"))
            {
                return;
            }

            int targetRetailOrdinal = Math.Min(dt.Columns["RetailPrice"].Ordinal, dt.Columns["MRP"].Ordinal);
            if (dt.Columns["RetailPrice"].Ordinal != targetRetailOrdinal)
            {
                dt.Columns["RetailPrice"].SetOrdinal(targetRetailOrdinal);
            }

            int targetMrpOrdinal = Math.Min(targetRetailOrdinal + 1, dt.Columns.Count - 1);
            if (dt.Columns["MRP"].Ordinal != targetMrpOrdinal)
            {
                dt.Columns["MRP"].SetOrdinal(targetMrpOrdinal);
            }
        }

        private void EnsureRetailPriceMrpDisplayOrder(UltraGridBand band)
        {
            if (band == null || !band.Columns.Exists("RetailPrice") || !band.Columns.Exists("MRP"))
            {
                return;
            }

            UltraGridColumn retailColumn = band.Columns["RetailPrice"];
            UltraGridColumn mrpColumn = band.Columns["MRP"];

            int targetRetailPosition = Math.Min(retailColumn.Header.VisiblePosition, mrpColumn.Header.VisiblePosition);
            if (retailColumn.Header.VisiblePosition != targetRetailPosition)
            {
                retailColumn.Header.VisiblePosition = targetRetailPosition;
            }

            int targetMrpPosition = targetRetailPosition + 1;
            if (mrpColumn.Header.VisiblePosition != targetMrpPosition)
            {
                mrpColumn.Header.VisiblePosition = targetMrpPosition;
            }
        }

        private void EnsureUomGridColumn(DataTable dt, string columnName)
        {
            if (!dt.Columns.Contains(columnName))
            {
                dt.Columns.Add(columnName, typeof(float));
                foreach (DataRow row in dt.Rows)
                {
                    row[columnName] = 0f;
                }
            }
            else
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (row[columnName] == DBNull.Value || row[columnName] == null)
                    {
                        row[columnName] = 0f;
                    }
                }
            }
        }

        private void EnsureUomGridStringColumn(DataTable dt, string columnName)
        {
            if (!dt.Columns.Contains(columnName))
            {
                dt.Columns.Add(columnName, typeof(string));
                foreach (DataRow row in dt.Rows)
                {
                    row[columnName] = string.Empty;
                }
            }
            else
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (row[columnName] == DBNull.Value || row[columnName] == null)
                    {
                        row[columnName] = string.Empty;
                    }
                }
            }
        }

        private Infragistics.Win.UltraWinGrid.UltraGrid GetPriceGridControl()
        {
            return this.Controls.Find("Ult_Price", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;
        }

        private DataTable GetPriceGridDataTable()
        {
            return GetPriceGridControl()?.DataSource as DataTable;
        }

        private string NormalizeUnitName(object value)
        {
            return Convert.ToString(value ?? string.Empty).Trim();
        }

        private float ConvertToFloat(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0f;

            if (value is float f) return f;
            if (value is double d) return (float)d;
            if (value is decimal m) return (float)m;
            if (value is int i) return i;
            if (value is long l) return l;

            float parsed;
            return float.TryParse(Convert.ToString(value), out parsed) ? parsed : 0f;
        }

        private Infragistics.Win.UltraWinGrid.UltraGridRow FindPriceGridRow(string unitName, double packingValue)
        {
            var priceGrid = GetPriceGridControl();
            if (priceGrid == null || priceGrid.Rows == null)
                return null;

            Infragistics.Win.UltraWinGrid.UltraGridRow fallbackRow = null;

            foreach (Infragistics.Win.UltraWinGrid.UltraGridRow row in priceGrid.Rows)
            {
                if (row == null || row.IsFilteredOut || row.IsAddRow)
                    continue;

                string priceUnit = NormalizeUnitName(row.Cells.Exists("Unit") ? row.Cells["Unit"].Value : null);
                if (!string.IsNullOrEmpty(unitName) &&
                    priceUnit.Equals(unitName, StringComparison.OrdinalIgnoreCase))
                {
                    return row;
                }

                if (fallbackRow == null && packingValue > 0 && row.Cells.Exists("Packing"))
                {
                    double pricePacking;
                    double.TryParse(Convert.ToString(row.Cells["Packing"].Value ?? "0"), out pricePacking);
                    if (Math.Abs(pricePacking - packingValue) < 0.0001)
                    {
                        fallbackRow = row;
                    }
                }
            }

            return fallbackRow;
        }

        private DataRow FindPriceDataRow(string unitName, double packingValue)
        {
            var priceDt = GetPriceGridDataTable();
            if (priceDt == null)
                return null;

            DataRow fallbackRow = null;
            foreach (DataRow row in priceDt.Rows)
            {
                string priceUnit = NormalizeUnitName(row["Unit"]);
                if (!string.IsNullOrEmpty(unitName) &&
                    priceUnit.Equals(unitName, StringComparison.OrdinalIgnoreCase))
                {
                    return row;
                }

                if (fallbackRow == null && packingValue > 0 && row.Table.Columns.Contains("Packing"))
                {
                    double pricePacking;
                    double.TryParse(Convert.ToString(row["Packing"] ?? "0"), out pricePacking);
                    if (Math.Abs(pricePacking - packingValue) < 0.0001)
                    {
                        fallbackRow = row;
                    }
                }
            }

            return fallbackRow;
        }

        internal int ResolvePidFromPurchaseNo(int purchaseNo)
        {
            if (purchaseNo <= 0)
                return 0;

            if (purchasePidCache.TryGetValue(purchaseNo, out int cachedPid))
                return cachedPid;

            try
            {
                int branchId;
                int.TryParse(DataBase.BranchId, out branchId);

                int companyId;
                int.TryParse(DataBase.CompanyId, out companyId);

                int pid = ExecuteStoredProcedureIntScalar(
                    STOREDPROCEDURE.POS_Purchase,
                    CreateSqlParameter("@_Operation", PurchaseOperationGetPidByPurchaseNo),
                    CreateSqlParameter("@PurchaseNo", purchaseNo),
                    CreateSqlParameter("@BranchId", branchId),
                    CreateSqlParameter("@CompanyId", companyId));

                if (pid <= 0)
                {
                    pid = ResolvePidFromPMaster(purchaseNo, branchId, companyId);
                }

                purchasePidCache[purchaseNo] = pid;
                return pid;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error resolving Pid for PurchaseNo {purchaseNo}: {ex.Message}");

                int branchId;
                int.TryParse(DataBase.BranchId, out branchId);

                int companyId;
                int.TryParse(DataBase.CompanyId, out companyId);

                int pid = ResolvePidFromPMaster(purchaseNo, branchId, companyId);
                purchasePidCache[purchaseNo] = pid;
                return pid;
            }
        }

        private int ResolvePidFromPMaster(int purchaseNo, int branchId, int companyId)
        {
            if (purchaseNo <= 0)
                return 0;

            try
            {
                int finYearId;
                int.TryParse(DataBase.FinyearId, out finYearId);
                if (finYearId <= 0)
                {
                    finYearId = SessionContext.FinYearId;
                }

                if (branchId <= 0)
                {
                    branchId = SessionContext.BranchId;
                }

                if (companyId <= 0)
                {
                    companyId = SessionContext.CompanyId;
                }

                using (BaseRepostitory repo = new BaseRepostitory())
                {
                    SqlConnection connection = repo.DataConnection as SqlConnection;
                    if (connection == null)
                    {
                        return 0;
                    }

                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
                    }

                    string sql = @"SELECT TOP 1 Pid
                                   FROM PMaster
                                   WHERE PurchaseNo = @PurchaseNo
                                     AND (@BranchId <= 0 OR BranchID = @BranchId)
                                     AND (@CompanyId <= 0 OR CompanyId = @CompanyId)
                                     AND (@FinYearId <= 0 OR FinYearId = @FinYearId)
                                     AND ISNULL(CancelFlag, 0) = 0
                                   ORDER BY Pid DESC";

                    using (SqlCommand cmd = new SqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@PurchaseNo", purchaseNo);
                        cmd.Parameters.AddWithValue("@BranchId", branchId);
                        cmd.Parameters.AddWithValue("@CompanyId", companyId);
                        cmd.Parameters.AddWithValue("@FinYearId", finYearId);

                        object result = cmd.ExecuteScalar();
                        int pid;
                        return result != null && result != DBNull.Value && int.TryParse(Convert.ToString(result), out pid)
                            ? pid
                            : 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error querying PMaster for PurchaseNo {purchaseNo}: {ex.Message}");
                return 0;
            }
        }

        private void CopyPriceValuesFromGridRow(DataRow target, Infragistics.Win.UltraWinGrid.UltraGridRow sourceRow)
        {
            if (target == null || sourceRow == null) return;

            foreach (string key in uomPriceColumnKeys)
            {
                if (target.Table.Columns.Contains(key) && sourceRow.Cells.Exists(key))
                {
                    if (key == "AliasBarcode")
                    {
                        target[key] = sourceRow.Cells[key].Value?.ToString() ?? "";
                    }
                    else
                    {
                        target[key] = ConvertToFloat(sourceRow.Cells[key].Value);
                    }
                }
            }
        }

        private void CopyPriceValuesToRow(DataRow target, DataRow source)
        {
            if (target == null || source == null) return;

            foreach (string key in uomPriceColumnKeys)
            {
                if (target.Table.Columns.Contains(key) && source.Table.Columns.Contains(key))
                {
                    if (key == "AliasBarcode")
                    {
                        target[key] = source[key]?.ToString() ?? "";
                        continue;
                    }

                    object rawValue = source[key];
                    if (rawValue == null || rawValue == DBNull.Value)
                    {
                        target[key] = 0f;
                    }
                    else
                    {
                        double parsed;
                        if (double.TryParse(Convert.ToString(rawValue), out parsed))
                        {
                            target[key] = Convert.ToSingle(parsed);
                        }
                        else
                        {
                            target[key] = 0f;
                        }
                    }
                }
            }
        }

        private void ResetUomPriceValues(DataRow row)
        {
            if (row == null) return;

            foreach (string key in uomPriceColumnKeys)
            {
                if (row.Table.Columns.Contains(key))
                {
                    // AliasBarcode is loaded directly into the UOM grid and shouldn't be reset
                    // just because the price grid is empty or out of sync.
                    if (key == "AliasBarcode") continue;

                    row[key] = 0f;
                }
            }
        }

        private void SyncUomRowWithPriceGrid(DataRow uomRow)
        {
            if (uomRow == null) return;

            string unitName = NormalizeUnitName(uomRow[colUnit]);
            double packingValue;
            double.TryParse(Convert.ToString(uomRow[colPacking] ?? "0"), out packingValue);

            var priceGridRow = FindPriceGridRow(unitName, packingValue);
            if (priceGridRow != null)
            {
                CopyPriceValuesFromGridRow(uomRow, priceGridRow);
                return;
            }

            var priceDataRow = FindPriceDataRow(unitName, packingValue);
            if (priceDataRow != null)
            {
                CopyPriceValuesToRow(uomRow, priceDataRow);
            }
            else
            {
                ResetUomPriceValues(uomRow);
            }
        }

        public void SyncUomGridWithPriceGrid()
        {
            try
            {
                var uomDt = ultraGrid1?.DataSource as DataTable;
                var priceDt = GetPriceGridDataTable();

                if (uomDt == null || priceDt == null)
                    return;

                EnsureUomGridPriceColumns(uomDt);

                foreach (DataRow uomRow in uomDt.Rows)
                {
                    SyncUomRowWithPriceGrid(uomRow);
                }

                ultraGrid1?.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error syncing UOM grid with price grid: {ex.Message}");
            }
        }

        // Sync all price-related fields from ultraGrid1 row to Ult_Price grid
        // This ensures user-entered values for Cost, MRP, RetailPrice, etc. get saved to database
        private void SyncCostToPriceGrid(Infragistics.Win.UltraWinGrid.UltraGridRow uomRow)
        {
            try
            {
                if (uomRow == null) return;

                // Get unit name from ultraGrid1 row
                string unitName = uomRow.Cells.Exists("Unit") ? (uomRow.Cells["Unit"].Value?.ToString() ?? "") : "";
                if (string.IsNullOrWhiteSpace(unitName)) return;

                // Get all values from ultraGrid1 row
                float packing = 0;
                float cost = 0;
                float mrp = 0;
                float retailPrice = 0;
                float wholeSalePrice = 0;
                float creditPrice = 0;
                float cardPrice = 0;
                float staffPrice = 0;
                float minPrice = 0;

                if (uomRow.Cells.Exists("Packing"))
                    float.TryParse(uomRow.Cells["Packing"].Value?.ToString(), out packing);
                if (uomRow.Cells.Exists("Cost"))
                    float.TryParse(uomRow.Cells["Cost"].Value?.ToString(), out cost);
                if (uomRow.Cells.Exists("MRP"))
                    float.TryParse(uomRow.Cells["MRP"].Value?.ToString(), out mrp);
                if (uomRow.Cells.Exists("RetailPrice"))
                    float.TryParse(uomRow.Cells["RetailPrice"].Value?.ToString(), out retailPrice);
                if (uomRow.Cells.Exists("WholeSalePrice"))
                    float.TryParse(uomRow.Cells["WholeSalePrice"].Value?.ToString(), out wholeSalePrice);
                if (uomRow.Cells.Exists("CreditPrice"))
                    float.TryParse(uomRow.Cells["CreditPrice"].Value?.ToString(), out creditPrice);
                if (uomRow.Cells.Exists("CardPrice"))
                    float.TryParse(uomRow.Cells["CardPrice"].Value?.ToString(), out cardPrice);
                if (uomRow.Cells.Exists("StaffPrice"))
                    float.TryParse(uomRow.Cells["StaffPrice"].Value?.ToString(), out staffPrice);
                if (uomRow.Cells.Exists("MinPrice"))
                    float.TryParse(uomRow.Cells["MinPrice"].Value?.ToString(), out minPrice);

                System.Diagnostics.Debug.WriteLine($"SyncCostToPriceGrid: Unit={unitName}, Packing={packing}, Cost={cost}, MRP={mrp}, Retail={retailPrice}");

                // Find Ult_Price grid
                var Ult_Price = this.Controls.Find("Ult_Price", true).FirstOrDefault()
                                as Infragistics.Win.UltraWinGrid.UltraGrid;
                if (Ult_Price == null)
                {
                    System.Diagnostics.Debug.WriteLine("SyncCostToPriceGrid: Ult_Price is NULL!");
                    return;
                }

                // Find matching row in Ult_Price by unit name ONLY
                bool found = false;
                foreach (Infragistics.Win.UltraWinGrid.UltraGridRow priceRow in Ult_Price.Rows)
                {
                    if (priceRow == null || priceRow.IsFilteredOut || priceRow.IsAddRow)
                        continue;

                    string priceUnit = priceRow.Cells.Exists("Unit") ? (priceRow.Cells["Unit"].Value?.ToString() ?? "") : "";

                    // Match by unit name ONLY - user may have changed packing
                    if (string.Equals(unitName, priceUnit, StringComparison.OrdinalIgnoreCase))
                    {
                        // Update ALL price-related fields in Ult_Price
                        if (priceRow.Cells.Exists("Packing"))
                            priceRow.Cells["Packing"].Value = Convert.ToInt32(packing);
                        if (priceRow.Cells.Exists("Cost"))
                            priceRow.Cells["Cost"].Value = cost;
                        if (priceRow.Cells.Exists("MRP"))
                            priceRow.Cells["MRP"].Value = mrp;
                        if (priceRow.Cells.Exists("RetailPrice"))
                            priceRow.Cells["RetailPrice"].Value = retailPrice;
                        if (priceRow.Cells.Exists("WholeSalePrice"))
                            priceRow.Cells["WholeSalePrice"].Value = wholeSalePrice;
                        if (priceRow.Cells.Exists("CreditPrice"))
                            priceRow.Cells["CreditPrice"].Value = creditPrice;
                        if (priceRow.Cells.Exists("CardPrice"))
                            priceRow.Cells["CardPrice"].Value = cardPrice;
                        if (priceRow.Cells.Exists("StaffPrice"))
                            priceRow.Cells["StaffPrice"].Value = staffPrice;
                        if (priceRow.Cells.Exists("MinPrice"))
                            priceRow.Cells["MinPrice"].Value = minPrice;

                        System.Diagnostics.Debug.WriteLine($"SyncCostToPriceGrid: Updated Ult_Price - Unit={unitName}, Packing={packing}, Cost={cost}, MRP={mrp}, Retail={retailPrice}");
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    System.Diagnostics.Debug.WriteLine($"SyncCostToPriceGrid: No matching row found in Ult_Price for unit '{unitName}'!");
                }

                Ult_Price.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error syncing to price grid: {ex.Message}");
            }
        }

        private void SetupUltraGrid()
        {
            try
            {
                // Configure the grid appearance
                ultraGrid1.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
                ultraGrid1.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
                ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;

                // Disable AutoFitStyle to prevent columns from auto-resizing when others are hidden
                ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.None;

                // Disable automatic column resizing
                ultraGrid1.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;

                // Hide the group-by area (gray bar)
                ultraGrid1.DisplayLayout.GroupByBox.Hidden = true;
                ultraGrid1.DisplayLayout.GroupByBox.Prompt = string.Empty;
                ultraGrid1.DisplayLayout.GroupByBox.Hidden = true;

                // Set rounded borders for the entire grid
                ultraGrid1.DisplayLayout.BorderStyle = UIElementBorderStyle.Rounded3;

                // Configure grid lines - single line borders for rows and columns
                ultraGrid1.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleRowSelector = UIElementBorderStyle.Solid;

                // Set border width to single line
                ultraGrid1.DisplayLayout.Override.RowAppearance.BorderAlpha = Alpha.Opaque;
                ultraGrid1.DisplayLayout.Override.CellAppearance.BorderAlpha = Alpha.Opaque;

                // Ensure consistent single line borders
                ultraGrid1.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;

                // Remove cell padding/spacing
                ultraGrid1.DisplayLayout.Override.CellPadding = 0;
                ultraGrid1.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;
                ultraGrid1.DisplayLayout.Override.RowSpacingBefore = 0;
                ultraGrid1.DisplayLayout.Override.RowSpacingAfter = 0;
                ultraGrid1.DisplayLayout.Override.CellSpacing = 0;

                Color lightBlue = Color.FromArgb(197, 217, 241);
                Color gridHeaderBlue = Color.FromArgb(93, 151, 214);
                Color gridHeaderBlueDark = Color.FromArgb(67, 118, 184);
                Color headerBorder = Color.FromArgb(118, 154, 198);
                Color headerBlue = gridHeaderBlue;

                ultraGrid1.UseAppStyling = false;
                ultraGrid1.UseOsThemes = DefaultableBoolean.False;

                // Apply border colors
                ultraGrid1.DisplayLayout.Override.CellAppearance.BorderColor = lightBlue;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BorderColor = lightBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BorderColor = headerBorder;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BorderColor = headerBorder;

                // Configure row height - increase to match the clean look
                ultraGrid1.DisplayLayout.Override.MinRowHeight = 30;
                ultraGrid1.DisplayLayout.Override.DefaultRowHeight = 30;

                // Add header styling - exact gridreport cell header look from frmvendorpurchasereport
                ultraGrid1.DisplayLayout.Override.HeaderStyle = HeaderStyle.Standard;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = gridHeaderBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = gridHeaderBlueDark;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

                // Configure row selector appearance with blue gradient
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor = gridHeaderBlueDark;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = gridHeaderBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.Default;
                ultraGrid1.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.None; // Remove numbers
                ultraGrid1.DisplayLayout.Override.RowSelectorWidth = 15; // Smaller width

                // Set all cells to have white background (no alternate row coloring)
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor2 = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackGradientStyle = GradientStyle.None;

                // Remove alternate row appearance (make all rows white)
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor2 = Color.White;
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackGradientStyle = GradientStyle.None;

                // Configure selected row appearance with highlight that maintains readability
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(210, 232, 255); // Very light blue highlight matching FrmPurchaseDisplayDialog
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor2 = Color.FromArgb(210, 232, 255);
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.ForeColor = Color.Black; // Keep text readable

                // Configure spacing and expansion behavior
                ultraGrid1.DisplayLayout.InterBandSpacing = 0;
                ultraGrid1.DisplayLayout.Override.ExpansionIndicator = ShowExpansionIndicator.Never;

                // Configure scrollbar style
                ultraGrid1.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
                ultraGrid1.DisplayLayout.ScrollStyle = ScrollStyle.Immediate;

                // Configure the scrollbar look
                if (ultraGrid1.DisplayLayout.ScrollBarLook != null)
                {
                    // Configure button appearance
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor2 = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackGradientStyle = GradientStyle.None;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BorderColor = headerBlue;

                    // Configure track appearance
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor = Color.White;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor2 = Color.White;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackGradientStyle = GradientStyle.None;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BorderColor = lightBlue;

                    // Configure thumb appearance
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor2 = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackGradientStyle = GradientStyle.None;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BorderColor = headerBlue;
                }

                // Configure cell appearance to increase vertical content alignment
                ultraGrid1.DisplayLayout.Override.CellAppearance.TextVAlign = VAlign.Middle;

                // Setup data source
                DataTable dt = new DataTable();
                dt.Columns.Add(colUnit, typeof(string));
                dt.Columns.Add(colUnitId, typeof(string));
                dt.Columns.Add(colPacking, typeof(string));
                // dt.Columns.Add(colBarcode, typeof(string)); // Removed
                dt.Columns.Add(colReorder, typeof(string));
                dt.Columns.Add(colOpenStock, typeof(string));
                EnsureUomGridPriceColumns(dt);

                // Set the data source
                ultraGrid1.DataSource = dt;

                // Configure column headers and visibility
                ultraGrid1.DisplayLayout.Bands[0].Columns[colUnitId].Hidden = true;
                ultraGrid1.DisplayLayout.Bands[0].Columns[colReorder].Hidden = true;
                ultraGrid1.DisplayLayout.Bands[0].Columns[colOpenStock].Hidden = true;
                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("MarginAmt"))
                    ultraGrid1.DisplayLayout.Bands[0].Columns["MarginAmt"].Hidden = true;
                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("MarginPer"))
                    ultraGrid1.DisplayLayout.Bands[0].Columns["MarginPer"].Hidden = true;
                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("TaxPer"))
                    ultraGrid1.DisplayLayout.Bands[0].Columns["TaxPer"].Hidden = true;
                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("TaxAmt"))
                    ultraGrid1.DisplayLayout.Bands[0].Columns["TaxAmt"].Hidden = true;

                // Set column widths
                ultraGrid1.DisplayLayout.Bands[0].Columns[colUnit].Width = 120;
                ultraGrid1.DisplayLayout.Bands[0].Columns[colPacking].Width = 120;
                // ultraGrid1.DisplayLayout.Bands[0].Columns[colBarcode].Width = 180; // Removed
                ultraGrid1.DisplayLayout.Bands[0].Columns[colReorder].Width = 110;
                ultraGrid1.DisplayLayout.Bands[0].Columns[colOpenStock].Width = 110;

                // Set column headers
                ultraGrid1.DisplayLayout.Bands[0].Columns[colUnit].Header.Caption = "Unit";
                ultraGrid1.DisplayLayout.Bands[0].Columns[colPacking].Header.Caption = "Packing";
                // ultraGrid1.DisplayLayout.Bands[0].Columns[colBarcode].Header.Caption = "Barcode"; // Removed
                ultraGrid1.DisplayLayout.Bands[0].Columns[colReorder].Header.Caption = "Reorder";
                ultraGrid1.DisplayLayout.Bands[0].Columns[colOpenStock].Header.Caption = "Opening Stock";

                // Set numeric column formats
                ultraGrid1.DisplayLayout.Bands[0].Columns[colPacking].CellAppearance.TextHAlign = HAlign.Right;
                ultraGrid1.DisplayLayout.Bands[0].Columns[colPacking].Format = "N0";
                ultraGrid1.DisplayLayout.Bands[0].Columns[colReorder].CellAppearance.TextHAlign = HAlign.Right;
                ultraGrid1.DisplayLayout.Bands[0].Columns[colReorder].Format = "N0";
                ultraGrid1.DisplayLayout.Bands[0].Columns[colOpenStock].CellAppearance.TextHAlign = HAlign.Right;
                ultraGrid1.DisplayLayout.Bands[0].Columns[colOpenStock].Format = "N0";

                // Set appearance for text columns
                ultraGrid1.DisplayLayout.Bands[0].Columns[colUnit].CellAppearance.TextHAlign = HAlign.Left;
                // ultraGrid1.DisplayLayout.Bands[0].Columns[colBarcode].CellAppearance.TextHAlign = HAlign.Center; // Removed

                // Configure newly added price columns to mirror Ult_Price
                foreach (string key in uomPriceColumnKeys)
                {
                    if (!ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(key))
                        continue;

                    var column = ultraGrid1.DisplayLayout.Bands[0].Columns[key];
                    if (uomPriceColumnCaptions.ContainsKey(key))
                    {
                        column.Header.Caption = uomPriceColumnCaptions[key];
                    }
                    if (uomPriceColumnWidths.ContainsKey(key))
                    {
                        column.Width = uomPriceColumnWidths[key];
                    }

                    column.CellAppearance.TextHAlign = HAlign.Right;

                    // AliasBarcode is a string column and should be editable
                    if (string.Equals(key, "AliasBarcode", StringComparison.OrdinalIgnoreCase))
                    {
                        column.CellActivation = Activation.AllowEdit;
                        column.CellAppearance.TextHAlign = HAlign.Left;
                    }
                    // Cost should be editable
                    else if (string.Equals(key, "Cost", StringComparison.OrdinalIgnoreCase))
                    {
                        column.CellActivation = Activation.AllowEdit;
                    }
                    else
                    {
                        column.Format = "N2";
                        column.CellActivation = Activation.AllowEdit;
                    }
                }

                // Make Packing column editable
                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(colPacking))
                {
                    ultraGrid1.DisplayLayout.Bands[0].Columns[colPacking].CellActivation = Activation.AllowEdit;
                }

                // Make Barcode column editable references removed

                // Subscribe to InitializeLayout event for consistent styling
                ultraGrid1.InitializeLayout += UltraGrid1_InitializeLayout;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting up UltraGrid: {ex.Message}");
            }
        }

        private void UltraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                // Define colors - matching FrmPurchaseDisplayDialog.cs exactly
                Color lightBlue = Color.FromArgb(173, 216, 230); // Light blue for borders
                Color headerBlue = Color.FromArgb(0, 123, 255); // Solid blue color for headers
                Color selectedBlue = Color.FromArgb(210, 232, 255); // Very light blue for selection (matching FrmPurchaseDisplayDialog)

                // Apply proper grid line styles
                e.Layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
                e.Layout.Override.BorderStyleRowSelector = UIElementBorderStyle.Solid;

                // Set grid line colors
                e.Layout.Override.RowAppearance.BorderColor = lightBlue;
                e.Layout.Override.CellAppearance.BorderColor = lightBlue;
                e.Layout.Appearance.BorderColor = lightBlue;

                // Set border style for the main grid
                e.Layout.BorderStyle = UIElementBorderStyle.Solid;

                // Remove cell padding/spacing
                e.Layout.Override.CellPadding = 0;
                e.Layout.Override.RowSpacingBefore = 0;
                e.Layout.Override.RowSpacingAfter = 0;
                e.Layout.Override.CellSpacing = 0;
                e.Layout.InterBandSpacing = 0;

                // Configure row height - matching FrmPurchaseDisplayDialog (30 pixels)
                e.Layout.Override.MinRowHeight = 30;
                e.Layout.Override.DefaultRowHeight = 30;

                // Set default alignment for all cells
                e.Layout.Override.CellAppearance.TextVAlign = VAlign.Middle;

                // Set font size for all cells - matching FrmPurchaseDisplayDialog (10 points)
                e.Layout.Override.CellAppearance.FontData.SizeInPoints = 10;
                e.Layout.Override.RowAppearance.FontData.SizeInPoints = 10;
                e.Layout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
                e.Layout.Override.RowAppearance.FontData.Name = "Microsoft Sans Serif";

                // Set all cells to white background (no alternate row coloring)
                e.Layout.Override.RowAppearance.BackColor = Color.White;
                e.Layout.Override.RowAppearance.BackColor2 = Color.White;
                e.Layout.Override.RowAppearance.BackGradientStyle = GradientStyle.None;

                // Remove alternate row appearance (make all rows white)
                e.Layout.Override.RowAlternateAppearance.BackColor = Color.White;
                e.Layout.Override.RowAlternateAppearance.BackColor2 = Color.White;
                e.Layout.Override.RowAlternateAppearance.BackGradientStyle = GradientStyle.None;

                // Add header styling with blue gradient - exact gridreport cell header look from frmvendorpurchasereport
                e.Layout.Override.HeaderStyle = HeaderStyle.Standard;
                e.Layout.Override.HeaderAppearance.BackColor = Color.FromArgb(93, 151, 214);
                e.Layout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(67, 118, 184);
                e.Layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
                e.Layout.Override.HeaderAppearance.ForeColor = Color.White;
                e.Layout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
                e.Layout.Override.HeaderAppearance.TextVAlign = VAlign.Middle;
                e.Layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                e.Layout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
                e.Layout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;
                e.Layout.Override.HeaderAppearance.BorderColor = Color.FromArgb(118, 154, 198);
                e.Layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;

                // Configure row selector appearance - matching gridreport
                e.Layout.Override.RowSelectorAppearance.BackColor = Color.FromArgb(67, 118, 184);
                e.Layout.Override.RowSelectorAppearance.BackColor2 = Color.FromArgb(93, 151, 214);
                e.Layout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
                e.Layout.Override.RowSelectorAppearance.ForeColor = Color.White;
                e.Layout.Override.RowSelectorAppearance.BorderColor = Color.FromArgb(118, 154, 198);
                e.Layout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.Default;
                e.Layout.Override.RowSelectorWidth = 15;
                e.Layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.None;
                e.Layout.Override.ExpansionIndicator = ShowExpansionIndicator.Never;

                // Configure selected row appearance - matching FrmPurchaseDisplayDialog exactly
                e.Layout.Override.SelectedRowAppearance.BackColor = selectedBlue;
                e.Layout.Override.SelectedRowAppearance.BackColor2 = selectedBlue;
                e.Layout.Override.SelectedRowAppearance.BackGradientStyle = GradientStyle.None;
                e.Layout.Override.SelectedRowAppearance.ForeColor = Color.Black; // Black text for readability

                // Configure active row appearance - same as selected row
                e.Layout.Override.ActiveRowAppearance.BackColor = selectedBlue;
                e.Layout.Override.ActiveRowAppearance.BackColor2 = selectedBlue;
                e.Layout.Override.ActiveRowAppearance.BackGradientStyle = GradientStyle.None;
                e.Layout.Override.ActiveRowAppearance.ForeColor = Color.Black;

                // Configure scrollbar style - matching FrmPurchaseDisplayDialog
                e.Layout.ScrollBounds = ScrollBounds.ScrollToFill;
                e.Layout.ScrollStyle = ScrollStyle.Immediate;

                // Configure scrollbar look
                if (e.Layout.ScrollBarLook != null)
                {
                    e.Layout.ScrollBarLook.ButtonAppearance.BackColor = headerBlue;
                    e.Layout.ScrollBarLook.ButtonAppearance.BackColor2 = headerBlue;
                    e.Layout.ScrollBarLook.ButtonAppearance.BackGradientStyle = GradientStyle.None;
                    e.Layout.ScrollBarLook.TrackAppearance.BackColor = Color.White;
                    e.Layout.ScrollBarLook.TrackAppearance.BackColor2 = Color.White;
                    e.Layout.ScrollBarLook.TrackAppearance.BackGradientStyle = GradientStyle.None;
                    e.Layout.ScrollBarLook.TrackAppearance.BorderColor = lightBlue;
                    e.Layout.ScrollBarLook.ThumbAppearance.BackColor = headerBlue;
                    e.Layout.ScrollBarLook.ThumbAppearance.BackColor2 = headerBlue;
                    e.Layout.ScrollBarLook.ThumbAppearance.BorderColor = headerBlue;
                }

                // Disable AutoFitStyle to prevent columns from auto-resizing
                e.Layout.AutoFitStyle = AutoFitStyle.None;

                // Allow column sizing
                e.Layout.Override.AllowColSizing = AllowColSizing.Free;
                e.Layout.Override.AllowColMoving = AllowColMoving.NotAllowed;
                e.Layout.Override.AllowColSwapping = AllowColSwapping.NotAllowed;

                // Disable filter indicators and other unnecessary features
                e.Layout.Override.AllowRowFiltering = DefaultableBoolean.False;

                if (e.Layout.Bands.Count > 0)
                {
                    foreach (UltraGridColumn col in e.Layout.Bands[0].Columns)
                    {
                        // Customize column header with solid blue color
                        col.Header.Appearance.BackColor = headerBlue;
                        col.Header.Appearance.BackColor2 = headerBlue;
                        col.Header.Appearance.BackGradientStyle = GradientStyle.None;
                        col.Header.Appearance.ForeColor = Color.White;
                        col.Header.Appearance.BorderColor = headerBlue;
                        col.Header.Appearance.TextHAlign = HAlign.Center;
                        col.Header.Appearance.FontData.Bold = DefaultableBoolean.True;

                        // Set cell appearance with solid borders
                        col.CellAppearance.TextVAlign = VAlign.Middle;
                        col.CellAppearance.BorderColor = lightBlue;

                        // Apply shared captions from the UOM column metadata.
                        if (uomPriceColumnCaptions.TryGetValue(col.Key, out string caption))
                        {
                            col.Header.Caption = caption;
                        }
                    }

                    EnsureRetailPriceMrpDisplayOrder(e.Layout.Bands[0]);

                    // Make AliasBarcode column editable
                    if (e.Layout.Bands[0].Columns.Exists("AliasBarcode"))
                    {
                        e.Layout.Bands[0].Columns["AliasBarcode"].CellActivation = Activation.AllowEdit;
                        e.Layout.Bands[0].Columns["AliasBarcode"].CellAppearance.TextHAlign = HAlign.Left;
                    }

                    // Hide specified columns
                    if (e.Layout.Bands[0].Columns.Exists(colReorder))
                        e.Layout.Bands[0].Columns[colReorder].Hidden = true;
                    if (e.Layout.Bands[0].Columns.Exists(colOpenStock))
                        e.Layout.Bands[0].Columns[colOpenStock].Hidden = true;
                    if (e.Layout.Bands[0].Columns.Exists("MarginAmt"))
                        e.Layout.Bands[0].Columns["MarginAmt"].Hidden = true;
                    if (e.Layout.Bands[0].Columns.Exists("MarginPer"))
                        e.Layout.Bands[0].Columns["MarginPer"].Hidden = true;
                    if (e.Layout.Bands[0].Columns.Exists("TaxPer"))
                        e.Layout.Bands[0].Columns["TaxPer"].Hidden = true;
                    if (e.Layout.Bands[0].Columns.Exists("TaxAmt"))
                        e.Layout.Bands[0].Columns["TaxAmt"].Hidden = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UltraGrid1_InitializeLayout: {ex.Message}");
            }
        }

        // Add a method to remove sort indicators after the grid is loaded
        private void RemoveSortIndicators()
        {
            try
            {
                if (ultraGrid1 != null && ultraGrid1.DisplayLayout != null && ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    // Clear any sorted columns
                    ultraGrid1.DisplayLayout.Bands[0].SortedColumns.Clear();

                    // Set all columns' SortIndicator property to None
                    foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                    {
                        col.SortIndicator = SortIndicator.None;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error removing sort indicators: {ex.Message}");
            }
        }

        // Setup ultraGrid2 for vendor details
        private void SetupVendorGrid()
        {
            // Check if ultraGrid2 exists
            Infragistics.Win.UltraWinGrid.UltraGrid ultraGrid2 =
                this.Controls.Find("ultraGrid2", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;

            if (ultraGrid2 == null)
            {
                System.Diagnostics.Debug.WriteLine("ultraGrid2 not found in the form");
                return;
            }

            // Configure the grid appearance
            ultraGrid2.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            ultraGrid2.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            ultraGrid2.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False; // Read-only
            ultraGrid2.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            ultraGrid2.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
            ultraGrid2.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            ultraGrid2.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;

            // Disable AutoFitStyle to prevent columns from auto-resizing when others are hidden
            ultraGrid2.DisplayLayout.AutoFitStyle = AutoFitStyle.None;

            // Disable automatic column resizing
            ultraGrid2.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;

            // Hide the group-by area (gray bar)
            ultraGrid2.DisplayLayout.GroupByBox.Hidden = true;
            ultraGrid2.DisplayLayout.GroupByBox.Prompt = string.Empty;
            ultraGrid2.DisplayLayout.GroupByBox.Hidden = true;

            // Set rounded borders for the entire grid
            ultraGrid2.DisplayLayout.BorderStyle = UIElementBorderStyle.Rounded3;

            // Configure grid lines - single line borders for rows and columns
            ultraGrid2.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            ultraGrid2.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            ultraGrid2.DisplayLayout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            ultraGrid2.DisplayLayout.Override.BorderStyleRowSelector = UIElementBorderStyle.Solid;

            // Set border width to single line
            ultraGrid2.DisplayLayout.Override.RowAppearance.BorderAlpha = Alpha.Opaque;
            ultraGrid2.DisplayLayout.Override.CellAppearance.BorderAlpha = Alpha.Opaque;

            // Ensure consistent single line borders
            ultraGrid2.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;

            // Remove cell padding/spacing
            ultraGrid2.DisplayLayout.Override.CellPadding = 0;
            ultraGrid2.DisplayLayout.Override.CellClickAction = CellClickAction.CellSelect;
            ultraGrid2.DisplayLayout.Override.RowSpacingBefore = 0;
            ultraGrid2.DisplayLayout.Override.RowSpacingAfter = 0;
            ultraGrid2.DisplayLayout.Override.CellSpacing = 0;

            // Set light blue border color for cells
            Color lightBlue = Color.FromArgb(173, 216, 230); // Light blue for borders
            Color headerBlue = Color.FromArgb(0, 123, 255); // Slightly darker blue for headers

            // Apply border colors
            ultraGrid2.DisplayLayout.Override.CellAppearance.BorderColor = lightBlue;
            ultraGrid2.DisplayLayout.Override.RowAppearance.BorderColor = lightBlue;
            ultraGrid2.DisplayLayout.Override.HeaderAppearance.BorderColor = headerBlue;
            ultraGrid2.DisplayLayout.Override.RowSelectorAppearance.BorderColor = headerBlue;

            // Configure row height - match ultraGrid1
            ultraGrid2.DisplayLayout.Override.MinRowHeight = 22;
            ultraGrid2.DisplayLayout.Override.DefaultRowHeight = 22;

            // Add header styling - blue headers
            ultraGrid2.DisplayLayout.Override.HeaderStyle = HeaderStyle.WindowsXPCommand;
            ultraGrid2.DisplayLayout.Override.HeaderAppearance.BackColor = headerBlue;
            ultraGrid2.DisplayLayout.Override.HeaderAppearance.BackColor2 = headerBlue; // Same color for no gradient
            ultraGrid2.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.None;
            ultraGrid2.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            ultraGrid2.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
            ultraGrid2.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            ultraGrid2.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
            ultraGrid2.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

            // Configure row selector appearance with blue - clean row headers
            ultraGrid2.DisplayLayout.Override.RowSelectorAppearance.BackColor = headerBlue;
            ultraGrid2.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = headerBlue; // Same color for no gradient
            ultraGrid2.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.None;
            ultraGrid2.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
            ultraGrid2.DisplayLayout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.Default;
            ultraGrid2.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.None; // Remove numbers
            ultraGrid2.DisplayLayout.Override.RowSelectorWidth = 15; // Smaller width

            // Set all cells to have white background (no alternate row coloring)
            ultraGrid2.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            ultraGrid2.DisplayLayout.Override.RowAppearance.BackColor2 = Color.White;
            ultraGrid2.DisplayLayout.Override.RowAppearance.BackGradientStyle = GradientStyle.None;

            // Remove alternate row appearance (make all rows white)
            ultraGrid2.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.White;
            ultraGrid2.DisplayLayout.Override.RowAlternateAppearance.BackColor2 = Color.White;
            ultraGrid2.DisplayLayout.Override.RowAlternateAppearance.BackGradientStyle = GradientStyle.None;

            // Configure selected row appearance with highlight that maintains readability
            ultraGrid2.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(173, 216, 255); // Light blue highlight matching ultraGrid1
            ultraGrid2.DisplayLayout.Override.SelectedRowAppearance.BackColor2 = Color.FromArgb(173, 216, 255);
            ultraGrid2.DisplayLayout.Override.SelectedRowAppearance.BackGradientStyle = GradientStyle.None;
            ultraGrid2.DisplayLayout.Override.SelectedRowAppearance.ForeColor = SystemColors.ControlText; // Black text matching ultraGrid1

            // Configure active row appearance - make it same as selected row (matching FrmPurchase.cs)
            ultraGrid2.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(173, 216, 255);
            ultraGrid2.DisplayLayout.Override.ActiveRowAppearance.BackColor2 = Color.FromArgb(173, 216, 255);
            ultraGrid2.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = GradientStyle.None;
            ultraGrid2.DisplayLayout.Override.ActiveRowAppearance.ForeColor = SystemColors.ControlText;
            ultraGrid2.DisplayLayout.Override.ActiveRowAppearance.FontData.Bold = DefaultableBoolean.False;

            // Configure spacing and expansion behavior
            ultraGrid2.DisplayLayout.InterBandSpacing = 0;
            ultraGrid2.DisplayLayout.Override.ExpansionIndicator = ShowExpansionIndicator.Never;

            // Configure scrollbar style
            ultraGrid2.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
            ultraGrid2.DisplayLayout.ScrollStyle = ScrollStyle.Immediate;

            // Configure the scrollbar look
            if (ultraGrid2.DisplayLayout.ScrollBarLook != null)
            {
                // Configure button appearance
                ultraGrid2.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor = headerBlue;
                ultraGrid2.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor2 = headerBlue;
                ultraGrid2.DisplayLayout.ScrollBarLook.ButtonAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid2.DisplayLayout.ScrollBarLook.ButtonAppearance.BorderColor = headerBlue;

                // Configure track appearance
                ultraGrid2.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor = Color.White;
                ultraGrid2.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor2 = Color.White;
                ultraGrid2.DisplayLayout.ScrollBarLook.TrackAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid2.DisplayLayout.ScrollBarLook.TrackAppearance.BorderColor = lightBlue;

                // Configure thumb appearance
                ultraGrid2.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor = headerBlue;
                ultraGrid2.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor2 = headerBlue;
                ultraGrid2.DisplayLayout.ScrollBarLook.ThumbAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid2.DisplayLayout.ScrollBarLook.ThumbAppearance.BorderColor = headerBlue;
            }

            // Configure cell appearance to increase vertical content alignment
            ultraGrid2.DisplayLayout.Override.CellAppearance.TextVAlign = VAlign.Middle;

            // Create empty DataTable for initial setup
            DataTable dt = new DataTable();
            dt.Columns.Add("LedgerID", typeof(int));
            dt.Columns.Add("VendorName", typeof(string));
            dt.Columns.Add("Cost", typeof(double));
            dt.Columns.Add("Unit", typeof(string));
            dt.Columns.Add("InvoiceDate", typeof(DateTime));
            dt.Columns.Add("PurchaseNo", typeof(int));
            dt.Columns.Add("InvoiceNo", typeof(string));

            // Set the data source
            ultraGrid2.DataSource = dt;

            // Configure column headers and visibility
            if (ultraGrid2.DisplayLayout.Bands.Count > 0)
            {
                // Set column headers
                ultraGrid2.DisplayLayout.Bands[0].Columns["LedgerID"].Header.Caption = "Ledger ID";
                ultraGrid2.DisplayLayout.Bands[0].Columns["VendorName"].Header.Caption = "Vendor Name";
                ultraGrid2.DisplayLayout.Bands[0].Columns["Cost"].Header.Caption = "Cost";
                ultraGrid2.DisplayLayout.Bands[0].Columns["Unit"].Header.Caption = "Unit";
                ultraGrid2.DisplayLayout.Bands[0].Columns["InvoiceDate"].Header.Caption = "Invoice Date";
                ultraGrid2.DisplayLayout.Bands[0].Columns["PurchaseNo"].Header.Caption = "Purchase No";
                ultraGrid2.DisplayLayout.Bands[0].Columns["InvoiceNo"].Header.Caption = "Invoice No";
                // Set column widths - match ultraGrid1 pattern
                ultraGrid2.DisplayLayout.Bands[0].Columns["VendorName"].Width = 200;
                ultraGrid2.DisplayLayout.Bands[0].Columns["InvoiceNo"].Width = 120;
                ultraGrid2.DisplayLayout.Bands[0].Columns["InvoiceDate"].Width = 100;
                ultraGrid2.DisplayLayout.Bands[0].Columns["Cost"].Width = 100;
                ultraGrid2.DisplayLayout.Bands[0].Columns["Unit"].Width = 80;
                ultraGrid2.DisplayLayout.Bands[0].Columns["PurchaseNo"].Width = 120;

                // Format date column
                ultraGrid2.DisplayLayout.Bands[0].Columns["InvoiceDate"].Format = "dd/MM/yyyy";

                // Format cost column
                ultraGrid2.DisplayLayout.Bands[0].Columns["Cost"].Format = "N2";

                // Hide LedgerID column
                ultraGrid2.DisplayLayout.Bands[0].Columns["LedgerID"].Hidden = true;

                // Set appearance for text columns
                ultraGrid2.DisplayLayout.Bands[0].Columns["VendorName"].CellAppearance.TextHAlign = HAlign.Left;
                ultraGrid2.DisplayLayout.Bands[0].Columns["Unit"].CellAppearance.TextHAlign = HAlign.Left;
                ultraGrid2.DisplayLayout.Bands[0].Columns["InvoiceNo"].CellAppearance.TextHAlign = HAlign.Left;
                ultraGrid2.DisplayLayout.Bands[0].Columns["PurchaseNo"].CellAppearance.TextHAlign = HAlign.Right;
                ultraGrid2.DisplayLayout.Bands[0].Columns["Cost"].CellAppearance.TextHAlign = HAlign.Right;
                ultraGrid2.DisplayLayout.Bands[0].Columns["InvoiceDate"].CellAppearance.TextHAlign = HAlign.Center;
            }
        }

        private void GetPriceDesing()
        {
            // Setup Ult_Price UltraGrid for price details
            Infragistics.Win.UltraWinGrid.UltraGrid Ult_Price =
                this.Controls.Find("Ult_Price", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;

            if (Ult_Price == null)
            {
                System.Diagnostics.Debug.WriteLine("Ult_Price control not found in the form");
                return;
            }

            // Configure the UltraGrid for price details
            Ult_Price.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            Ult_Price.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            Ult_Price.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.True;
            Ult_Price.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            Ult_Price.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
            Ult_Price.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            Ult_Price.DisplayLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;

            // Disable AutoFitStyle to prevent columns from auto-resizing
            Ult_Price.DisplayLayout.AutoFitStyle = AutoFitStyle.None;

            // Disable automatic column resizing
            Ult_Price.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;

            // Hide the group-by area (gray bar)
            Ult_Price.DisplayLayout.GroupByBox.Hidden = true;
            Ult_Price.DisplayLayout.GroupByBox.Prompt = string.Empty;

            // Set rounded borders for the entire grid
            Ult_Price.DisplayLayout.BorderStyle = UIElementBorderStyle.Rounded3;

            // Configure grid lines - single line borders for rows and columns
            Ult_Price.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            Ult_Price.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            Ult_Price.DisplayLayout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            Ult_Price.DisplayLayout.Override.BorderStyleRowSelector = UIElementBorderStyle.Solid;

            // Set border width to single line
            Ult_Price.DisplayLayout.Override.RowAppearance.BorderAlpha = Alpha.Opaque;
            Ult_Price.DisplayLayout.Override.CellAppearance.BorderAlpha = Alpha.Opaque;

            // Ensure consistent single line borders
            Ult_Price.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;

            // Remove cell padding/spacing
            Ult_Price.DisplayLayout.Override.CellPadding = 0;
            Ult_Price.DisplayLayout.Override.CellClickAction = CellClickAction.CellSelect;
            Ult_Price.DisplayLayout.Override.RowSpacingBefore = 0;
            Ult_Price.DisplayLayout.Override.RowSpacingAfter = 0;
            Ult_Price.DisplayLayout.Override.CellSpacing = 0;

            // Set light blue border color for cells
            Color lightBlue = Color.FromArgb(173, 216, 230); // Light blue for borders
            Color headerBlue = Color.FromArgb(0, 123, 255); // Slightly darker blue for headers

            // Apply border colors
            Ult_Price.DisplayLayout.Override.CellAppearance.BorderColor = lightBlue;
            Ult_Price.DisplayLayout.Override.RowAppearance.BorderColor = lightBlue;
            Ult_Price.DisplayLayout.Override.HeaderAppearance.BorderColor = headerBlue;
            Ult_Price.DisplayLayout.Override.RowSelectorAppearance.BorderColor = headerBlue;

            // Configure row height - match ultraGrid1
            Ult_Price.DisplayLayout.Override.MinRowHeight = 22;
            Ult_Price.DisplayLayout.Override.DefaultRowHeight = 22;

            // Add header styling - blue headers
            Ult_Price.DisplayLayout.Override.HeaderStyle = HeaderStyle.WindowsXPCommand;
            Ult_Price.DisplayLayout.Override.HeaderAppearance.BackColor = headerBlue;
            Ult_Price.DisplayLayout.Override.HeaderAppearance.BackColor2 = headerBlue; // Same color for no gradient
            Ult_Price.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.None;
            Ult_Price.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            Ult_Price.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
            Ult_Price.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            Ult_Price.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
            Ult_Price.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

            // Configure row selector appearance with blue - clean row headers
            Ult_Price.DisplayLayout.Override.RowSelectorAppearance.BackColor = headerBlue;
            Ult_Price.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = headerBlue; // Same color for no gradient
            Ult_Price.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.None;
            Ult_Price.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
            Ult_Price.DisplayLayout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.Default;
            Ult_Price.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.None; // Remove numbers
            Ult_Price.DisplayLayout.Override.RowSelectorWidth = 15; // Smaller width

            // Set all cells to have white background (no alternate row coloring)
            Ult_Price.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            Ult_Price.DisplayLayout.Override.RowAppearance.BackColor2 = Color.White;
            Ult_Price.DisplayLayout.Override.RowAppearance.BackGradientStyle = GradientStyle.None;

            // Remove alternate row appearance (make all rows white)
            Ult_Price.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.White;
            Ult_Price.DisplayLayout.Override.RowAlternateAppearance.BackColor2 = Color.White;
            Ult_Price.DisplayLayout.Override.RowAlternateAppearance.BackGradientStyle = GradientStyle.None;

            // Configure selected row appearance with highlight that maintains readability
            Ult_Price.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(173, 216, 255); // Light blue highlight matching ultraGrid1
            Ult_Price.DisplayLayout.Override.SelectedRowAppearance.BackColor2 = Color.FromArgb(173, 216, 255);
            Ult_Price.DisplayLayout.Override.SelectedRowAppearance.BackGradientStyle = GradientStyle.None;
            Ult_Price.DisplayLayout.Override.SelectedRowAppearance.ForeColor = SystemColors.ControlText; // Black text matching ultraGrid1

            // Configure active row appearance - make it same as selected row (matching FrmPurchase.cs)
            Ult_Price.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(173, 216, 255);
            Ult_Price.DisplayLayout.Override.ActiveRowAppearance.BackColor2 = Color.FromArgb(173, 216, 255);
            Ult_Price.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = GradientStyle.None;
            Ult_Price.DisplayLayout.Override.ActiveRowAppearance.ForeColor = SystemColors.ControlText;
            Ult_Price.DisplayLayout.Override.ActiveRowAppearance.FontData.Bold = DefaultableBoolean.False;

            // Configure spacing and expansion behavior
            Ult_Price.DisplayLayout.InterBandSpacing = 0;
            Ult_Price.DisplayLayout.Override.ExpansionIndicator = ShowExpansionIndicator.Never;

            // Configure scrollbar style
            Ult_Price.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
            Ult_Price.DisplayLayout.ScrollStyle = ScrollStyle.Immediate;

            // Configure the scrollbar look
            if (Ult_Price.DisplayLayout.ScrollBarLook != null)
            {
                // Configure button appearance
                Ult_Price.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor = headerBlue;
                Ult_Price.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor2 = headerBlue;
                Ult_Price.DisplayLayout.ScrollBarLook.ButtonAppearance.BackGradientStyle = GradientStyle.None;
                Ult_Price.DisplayLayout.ScrollBarLook.ButtonAppearance.BorderColor = headerBlue;

                // Configure track appearance
                Ult_Price.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor = Color.White;
                Ult_Price.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor2 = Color.White;
                Ult_Price.DisplayLayout.ScrollBarLook.TrackAppearance.BackGradientStyle = GradientStyle.None;
                Ult_Price.DisplayLayout.ScrollBarLook.TrackAppearance.BorderColor = lightBlue;

                // Configure thumb appearance
                Ult_Price.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor = headerBlue;
                Ult_Price.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor2 = headerBlue;
                Ult_Price.DisplayLayout.ScrollBarLook.ThumbAppearance.BackGradientStyle = GradientStyle.None;
                Ult_Price.DisplayLayout.ScrollBarLook.ThumbAppearance.BorderColor = headerBlue;
            }

            // Configure cell appearance to increase vertical content alignment
            Ult_Price.DisplayLayout.Override.CellAppearance.TextVAlign = VAlign.Middle;

            // Create empty DataTable for initial setup
            DataTable dt = new DataTable();
            dt.Columns.Add("Unit", typeof(string));
            dt.Columns.Add("Packing", typeof(string));
            dt.Columns.Add("Cost", typeof(float));
            dt.Columns.Add("MarginAmt", typeof(float));
            dt.Columns.Add("MarginPer", typeof(float));
            dt.Columns.Add("TaxPer", typeof(float));
            dt.Columns.Add("TaxAmt", typeof(float));
            dt.Columns.Add("MRP", typeof(float));
            dt.Columns.Add("RetailPrice", typeof(float));
            dt.Columns.Add("WholeSalePrice", typeof(float));
            dt.Columns.Add("CreditPrice", typeof(float));
            dt.Columns.Add("CardPrice", typeof(string));
            dt.Columns.Add("StaffPrice", typeof(float));
            dt.Columns.Add("MinPrice", typeof(float));

            // Set the data source
            Ult_Price.DataSource = dt;

            // Configure column headers and visibility
            if (Ult_Price.DisplayLayout.Bands.Count > 0)
            {
                // Set column headers
                Ult_Price.DisplayLayout.Bands[0].Columns["Unit"].Header.Caption = "Unit Name";
                Ult_Price.DisplayLayout.Bands[0].Columns["Packing"].Header.Caption = "Packing";
                Ult_Price.DisplayLayout.Bands[0].Columns["Cost"].Header.Caption = "Cost";
                Ult_Price.DisplayLayout.Bands[0].Columns["MarginAmt"].Header.Caption = "Margin Amount";
                Ult_Price.DisplayLayout.Bands[0].Columns["MarginPer"].Header.Caption = "Margin %";
                Ult_Price.DisplayLayout.Bands[0].Columns["TaxPer"].Header.Caption = "Tax %";
                Ult_Price.DisplayLayout.Bands[0].Columns["TaxAmt"].Header.Caption = "Tax Amount";
                Ult_Price.DisplayLayout.Bands[0].Columns["MRP"].Header.Caption = "MRP";
                Ult_Price.DisplayLayout.Bands[0].Columns["RetailPrice"].Header.Caption = "Retail Price";
                Ult_Price.DisplayLayout.Bands[0].Columns["WholeSalePrice"].Header.Caption = "Walking Price";
                Ult_Price.DisplayLayout.Bands[0].Columns["CreditPrice"].Header.Caption = "Credit Price";
                Ult_Price.DisplayLayout.Bands[0].Columns["CardPrice"].Header.Caption = "Card Price";
                if (Ult_Price.DisplayLayout.Bands[0].Columns.Exists("StaffPrice"))
                    Ult_Price.DisplayLayout.Bands[0].Columns["StaffPrice"].Header.Caption = "Staff Price";
                if (Ult_Price.DisplayLayout.Bands[0].Columns.Exists("MinPrice"))
                    Ult_Price.DisplayLayout.Bands[0].Columns["MinPrice"].Header.Caption = "Min Price";

                // Set column widths - match ultraGrid1 pattern
                Ult_Price.DisplayLayout.Bands[0].Columns["Unit"].Width = 80;
                Ult_Price.DisplayLayout.Bands[0].Columns["Packing"].Width = 80;
                Ult_Price.DisplayLayout.Bands[0].Columns["Cost"].Width = 100;
                Ult_Price.DisplayLayout.Bands[0].Columns["MarginAmt"].Width = 120;
                Ult_Price.DisplayLayout.Bands[0].Columns["MarginPer"].Width = 80;
                Ult_Price.DisplayLayout.Bands[0].Columns["TaxPer"].Width = 80;
                Ult_Price.DisplayLayout.Bands[0].Columns["TaxAmt"].Width = 120;
                Ult_Price.DisplayLayout.Bands[0].Columns["MRP"].Width = 100;
                Ult_Price.DisplayLayout.Bands[0].Columns["RetailPrice"].Width = 120;
                Ult_Price.DisplayLayout.Bands[0].Columns["WholeSalePrice"].Width = 120;
                Ult_Price.DisplayLayout.Bands[0].Columns["CreditPrice"].Width = 120;
                Ult_Price.DisplayLayout.Bands[0].Columns["CardPrice"].Width = 120;
                if (Ult_Price.DisplayLayout.Bands[0].Columns.Exists("StaffPrice"))
                    Ult_Price.DisplayLayout.Bands[0].Columns["StaffPrice"].Width = 120;
                if (Ult_Price.DisplayLayout.Bands[0].Columns.Exists("MinPrice"))
                    Ult_Price.DisplayLayout.Bands[0].Columns["MinPrice"].Width = 120;

                // Format numeric columns
                Ult_Price.DisplayLayout.Bands[0].Columns["Cost"].Format = "N2";
                Ult_Price.DisplayLayout.Bands[0].Columns["MarginAmt"].Format = "N2";
                Ult_Price.DisplayLayout.Bands[0].Columns["MarginPer"].Format = "N2";
                Ult_Price.DisplayLayout.Bands[0].Columns["TaxPer"].Format = "N2";
                Ult_Price.DisplayLayout.Bands[0].Columns["TaxAmt"].Format = "N2";
                Ult_Price.DisplayLayout.Bands[0].Columns["MRP"].Format = "N2";
                Ult_Price.DisplayLayout.Bands[0].Columns["RetailPrice"].Format = "N2";
                Ult_Price.DisplayLayout.Bands[0].Columns["WholeSalePrice"].Format = "N2";
                Ult_Price.DisplayLayout.Bands[0].Columns["CreditPrice"].Format = "N2";
                Ult_Price.DisplayLayout.Bands[0].Columns["CardPrice"].Format = "N2";
                if (Ult_Price.DisplayLayout.Bands[0].Columns.Exists("StaffPrice"))
                    Ult_Price.DisplayLayout.Bands[0].Columns["StaffPrice"].Format = "N2";
                if (Ult_Price.DisplayLayout.Bands[0].Columns.Exists("MinPrice"))
                    Ult_Price.DisplayLayout.Bands[0].Columns["MinPrice"].Format = "N2";

                // Set appearance
                Ult_Price.DisplayLayout.Override.CellAppearance.TextHAlign = HAlign.Right;
                Ult_Price.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;

                // Make specific price columns editable on click
                var editableCols = new[] { "Cost", "MRP", "RetailPrice", "WholeSalePrice", "CreditPrice", "CardPrice", "StaffPrice", "MinPrice" };
                foreach (var key in editableCols)
                {
                    if (Ult_Price.DisplayLayout.Bands[0].Columns.Exists(key))
                    {
                        Ult_Price.DisplayLayout.Bands[0].Columns[key].CellActivation = Activation.AllowEdit;
                        Ult_Price.DisplayLayout.Bands[0].Columns[key].CellClickAction = CellClickAction.EditAndSelectText;
                    }
                }
            }
        }

        private void GetImagesDesing()
        {

        }

        private void btn_Add_ItemIype_Click(object sender, EventArgs e)
        {
            frmItemTypeDialog itemTypeDialog = new frmItemTypeDialog();
            itemTypeDialog.StartPosition = FormStartPosition.CenterScreen;
            itemTypeDialog.ShowDialog();
        }

        private void btn_Add_Cate_Click(object sender, EventArgs e)
        {
            string Params = "frmItemMasterNew";
            frmCategoryDialog category = new frmCategoryDialog(Params);
            category.StartPosition = FormStartPosition.CenterScreen;
            category.ShowDialog();
            SetupAutoComplete();
        }

        private void btn_Add_Grup_Click(object sender, EventArgs e)
        {
            frmGroupDialog groupDialog = new frmGroupDialog();
            groupDialog.StartPosition = FormStartPosition.CenterScreen;
            groupDialog.ShowDialog();
            SetupAutoComplete();
        }

        private void btn_Add_Brand_Click(object sender, EventArgs e)
        {
            frmBrandDialog brandDialog = new frmBrandDialog();
            brandDialog.StartPosition = FormStartPosition.CenterScreen;
            brandDialog.ShowDialog();
        }

        private void btn_Add_Custm_Click(object sender, EventArgs e)
        {
            frmCustomerTypeDDl customerTypeDialog = new frmCustomerTypeDDl();
            customerTypeDialog.StartPosition = FormStartPosition.CenterScreen;
            customerTypeDialog.ShowDialog();
        }

        private void btn_BaseUnit_Click(object sender, EventArgs e)
        {
            string Params = "ItemMasterMaster";
            frmUnitDialog unitDialog = new frmUnitDialog(Params);
            unitDialog.StartPosition = FormStartPosition.CenterScreen;

            // Show dialog and check result
            if (unitDialog.ShowDialog() == DialogResult.OK)
            {
                // The unit should be set by the dialog, but we can check here if needed
            }
        }

        private void txt_description_TextChanged(object sender, EventArgs e)
        {

        }

        private void txt_description_Click(object sender, EventArgs e)
        {
            // Intentionally left blank: typing description should not open any dialog
        }

        private void txt_description_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void txt_description_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {

            }
        }

        // txt_barcode represents the item's main barcode only
        // The BarCode column in ultraGrid1 acts as an independent alias barcode
        // Also auto-generates new item number when user starts typing a barcode for a new item
        private void txt_barcode_Click(object sender, EventArgs e)
        {
            try
            {
                RefreshCurrentItemFromBarcodeClick();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing item from barcode click: {ex.Message}");
            }
        }

        private void txt_barcode_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                RefreshCurrentItemFromBarcodeClick();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing item from barcode mouse click: {ex.Message}");
            }
        }

        private void txt_barcode_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                RefreshCurrentItemFromBarcodeClick();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing item from barcode mouse down: {ex.Message}");
            }
        }

        private DateTime lastUnitCostRefreshClickTime = DateTime.MinValue;

        private void Txt_UnitCost_Click(object sender, EventArgs e)
        {
            try
            {
                RefreshCurrentItemFromUnitCostClick();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing item from unit cost click: {ex.Message}");
            }
        }

        private void Txt_UnitCost_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                RefreshCurrentItemFromUnitCostClick();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing item from unit cost mouse click: {ex.Message}");
            }
        }

        private void Txt_UnitCost_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                RefreshCurrentItemFromUnitCostClick();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing item from unit cost mouse down: {ex.Message}");
            }
        }

        private void RefreshCurrentItemFromUnitCostClick()
        {
            if (isLoadingItem)
            {
                return;
            }

            DateTime now = DateTime.Now;
            if ((now - lastUnitCostRefreshClickTime).TotalMilliseconds < 300)
            {
                return;
            }

            int loadedId = CurrentItemId > 0 ? CurrentItemId : (ItemMaster != null ? ItemMaster.ItemId : 0);

            // If an item is ALREADY loaded into the form, clicking inside Txt_UnitCost must NOT re-trigger LoadItemById from DB!
            // Re-triggering LoadItemById on an already loaded item resets textBox1 (Markup %) to 0.00 and overwrites active form edits.
            if (loadedId > 0)
            {
                RecalculateMarkupPercentage();
                UpdateAllProfitMargins();
                return;
            }

            // Fallback: If NO item is currently loaded, search by barcode in txt_barcode and load item
            var txtBarcodeCtrl = GetMainBarcodeEditor();
            string barcode = (txtBarcodeCtrl?.Text ?? string.Empty).Trim();
            int itemId = 0;
            if (!string.IsNullOrWhiteSpace(barcode))
            {
                itemId = FindItemIdByAnyBarcode(barcode);
            }

            if (itemId <= 0)
            {
                return;
            }

            lastUnitCostRefreshClickTime = now;

            // Reload the item completely from database to update all fields
            LoadItemById(itemId);
        }
        private void RefreshCurrentItemFromBarcodeClick()
        {
            if (isLoadingItem)
            {
                return;
            }

            DateTime now = DateTime.Now;
            if ((now - lastBarcodeRefreshClickTime).TotalMilliseconds < 300)
            {
                return;
            }

            var txtBarcodeCtrl = GetMainBarcodeEditor();
            string barcode = (txtBarcodeCtrl?.Text ?? string.Empty).Trim();

            int loadedId = CurrentItemId > 0 ? CurrentItemId : (ItemMaster != null ? ItemMaster.ItemId : 0);

            // If an item is ALREADY loaded into the form and matches current barcode or loadedId > 0,
            // clicking inside txt_barcode must NOT re-trigger LoadItemById from DB!
            // Re-triggering LoadItemById on an already loaded item resets textBox1 (Markup %) to 0.00 and overwrites active form edits.
            if (loadedId > 0)
            {
                int barcodeItemId = !string.IsNullOrWhiteSpace(barcode) ? FindItemIdByAnyBarcode(barcode) : 0;
                if (barcodeItemId <= 0 || barcodeItemId == loadedId)
                {
                    RecalculateMarkupPercentage();
                    UpdateAllProfitMargins();
                    return;
                }
            }

            int itemId = 0;
            if (!string.IsNullOrWhiteSpace(barcode))
            {
                itemId = FindItemIdByAnyBarcode(barcode);
            }

            if (itemId <= 0)
            {
                return;
            }

            lastBarcodeRefreshClickTime = now;

            // Reload the item completely from database to update all fields
            LoadItemById(itemId);

            BeginInvoke((MethodInvoker)delegate
            {
                var refreshedBarcodeCtrl = GetMainBarcodeEditor();
                refreshedBarcodeCtrl?.Focus();
                refreshedBarcodeCtrl?.SelectAll();
            });
        }

        private int FindItemIdByAnyBarcode(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
            {
                return 0;
            }

            ItemMasterRepository itemRepo = new ItemMasterRepository();
            int itemId = itemRepo.GetItemIdByBarcode(barcode);

            if (itemId <= 0)
            {
                try
                {
                    itemId = itemRepo.GetItemIdByAliasBarcode(barcode);
                }
                catch (MissingMethodException)
                {
                    System.Diagnostics.Debug.WriteLine("GetItemIdByAliasBarcode method not found. Rebuild Repository.");
                }
            }

            if (itemId <= 0)
            {
                try
                {
                    itemId = itemRepo.GetItemIdByAlternativeBarcode(barcode);
                }
                catch (MissingMethodException)
                {
                    System.Diagnostics.Debug.WriteLine("GetItemIdByAlternativeBarcode method not found. Rebuild Repository.");
                }
            }

            return itemId;
        }
        private void txt_barcode_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Skip if currently loading an existing item
                if (isLoadingItem)
                    return;

                // Skip if item number already generated for this barcode entry session
                if (hasGeneratedItemNumberForBarcode)
                    return;

                // Skip if txt_ItemNo already has a value (existing item or already generated)
                if (!string.IsNullOrWhiteSpace(txt_ItemNo.Text))
                    return;

                string barcodeText = string.Empty;
                Control txtBarcodeField = sender as Control;
                if (txtBarcodeField != null)
                {
                    barcodeText = txtBarcodeField.Text;
                }
                else if (txt_barcode != null)
                {
                    barcodeText = txt_barcode.Text;
                }

                if (string.IsNullOrWhiteSpace(barcodeText))
                    return;

                // Generate new item number (same logic as btnIemLoad_ById_Click but without message box)
                GenerateNewItemNumber();

                // Set flag to prevent repeated generation
                hasGeneratedItemNumberForBarcode = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in txt_barcode_TextChanged: {ex.Message}");
            }
        }

        // txt_barcode represents the item's main barcode only
        // The BarCode column in ultraGrid1 acts as an independent alias barcode
        private void txt_barcode_LostFocus(object sender, EventArgs e)
        {
            // No synchronization with grid - barcode cell in ultraGrid1 is independent (alias barcode)
        }

        private void txt_LocalLanguage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                if (txt_Brand != null)
                {
                    this.BeginInvoke((MethodInvoker)delegate { txt_Brand.Focus(); });
                }
            }
        }

        private void txt_ItemType_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txt_Category?.Focus();
                e.Handled = true;
            }
        }

        private void txt_Category_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string categoryName = txt_Category?.Text?.Trim() ?? string.Empty;
                if (!string.IsNullOrEmpty(categoryName))
                {
                    Repository.Dropdowns drop = new Repository.Dropdowns();
                    var cats = drop.getCategoryDDl(categoryName)?.List?.ToList();
                    var match = cats?.FirstOrDefault(c => string.Equals(c.CategoryName, categoryName, StringComparison.OrdinalIgnoreCase));
                    if (match == null)
                    {
                        try
                        {
                            var catRepo = new Repository.MasterRepositry.CategoryRepository();
                            // If group is set, use it; otherwise 0
                            int groupId = 0;
                            string groupName = txt_Group?.Text?.Trim() ?? string.Empty;
                            if (!string.IsNullOrEmpty(groupName))
                            {
                                var groups = drop.getGroupDDl()?.List?.ToList();
                                var groupMatch = groups?.FirstOrDefault(g => string.Equals(g.GroupName, groupName, StringComparison.OrdinalIgnoreCase));
                                if (groupMatch != null) groupId = groupMatch.Id;
                            }
                            var newCat = new ModelClass.Master.Category { CategoryName = categoryName, GroupId = groupId, _Operation = "CREATE" };
                            catRepo.SaveCategory(newCat);
                            SetupAutoComplete();
                        }
                        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error creating category on Enter: {ex.Message}"); }
                    }
                }
                txt_Group?.Focus();
                e.Handled = true;
            }
        }

        private void txt_Group_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string groupName = txt_Group?.Text?.Trim() ?? string.Empty;
                if (!string.IsNullOrEmpty(groupName))
                {
                    Repository.Dropdowns drop = new Repository.Dropdowns();
                    var groups = drop.getGroupDDl()?.List?.ToList();
                    var match = groups?.FirstOrDefault(g => string.Equals(g.GroupName, groupName, StringComparison.OrdinalIgnoreCase));
                    if (match == null)
                    {
                        try
                        {
                            var groupRepo = new Repository.MasterRepositry.GroupRepository();
                            var newGroup = new ModelClass.Master.Group { GroupName = groupName, _Operation = "CREATE", BranchId = 0 };
                            groupRepo.SaveGroup(newGroup);
                            SetupAutoComplete();
                        }
                        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error creating group on Enter: {ex.Message}"); }
                    }
                }
            }
        }

        private void txt_Brand_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                HandleBrandAutoCreate();
                if (Txt_UnitCost != null)
                {
                    this.BeginInvoke((MethodInvoker)delegate { Txt_UnitCost.Focus(); });
                }
            }
        }

        private void txt_Brand_Leave(object sender, EventArgs e)
        {
            HandleBrandAutoCreate();
        }

        private void HandleBrandAutoCreate()
        {
            string brandName = txt_Brand?.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(brandName))
            {
                Repository.Dropdowns drop = new Repository.Dropdowns();
                var brands = drop.getBrandDDl()?.List?.ToList();
                var match = brands?.FirstOrDefault(b => string.Equals(b.BrandName, brandName, StringComparison.OrdinalIgnoreCase));

                if (match == null)
                {
                    try
                    {
                        var clientOps = new Repository.ClientOperations();
                        var newBrand = new ModelClass.Master.Brand { BrandName = brandName, _Operation = "CREATE" };
                        clientOps.SaveBrand(newBrand);
                        SetupAutoComplete(); // Refresh the autocomplete list
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error creating brand on auto-create: {ex.Message}");
                    }
                }
            }
        }

        private void txt_BaseUnit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (TabControll.Text != "")
                {
                    this.ActiveControl = btn_Add_UnitIm;
                }

                // Synchronize the base unit selection with the grid
                SynchronizeBaseUnitWithGrid();
            }
        }

        private void txt_BaseUnit_TextChanged(object sender, EventArgs e)
        {
            // Skip if currently loading an item - check immediately
            if (isLoadingItem)
                return;

            // Use a small delay to avoid excessive synchronization during typing
            // CRITICAL: Check isLoadingItem AGAIN inside the timer callback
            // because the flag might have been set during the 500ms delay
            System.Threading.Timer timer = null;
            timer = new System.Threading.Timer((state) =>
            {
                this.BeginInvoke(new Action(() =>
                {
                    // Double-check isLoadingItem inside the callback
                    // This prevents grid clearing if LoadItemById started during the delay
                    if (!isLoadingItem)
                    {
                        SynchronizeBaseUnitWithGrid();
                    }
                    timer?.Dispose();
                }));
            }, null, 500, System.Threading.Timeout.Infinite);
        }
        private void Txt_UnitCost_Leave(object sender, EventArgs e)
        {
            if (float.TryParse(Txt_UnitCost.Text, out float unitCost))
            {
                Txt_UnitCost.Text = unitCost.ToString("0.000");
            }
        }

        private void Txt_UnitCost_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // Trigger formatting when Enter is pressed as well
                if (float.TryParse(Txt_UnitCost.Text, out float unitCost))
                {
                    Txt_UnitCost.Text = unitCost.ToString("0.000");
                }

                e.Handled = true;
                e.SuppressKeyPress = true;
                if (txt_Retail != null)
                {
                    this.BeginInvoke((MethodInvoker)delegate { txt_Retail.Focus(); });
                }
            }
        }

        private void txt_CustomerType_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {

            }
        }

        private void Txt_UnitCost_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (isLoadingItem) return; // do not auto-recalculate while loading existing item
                                           // Recalculate price grid values based on the updated 
                                           // Recalculate price grid values based on the updated Unit Cost

                // Recalculate Cost cells in ultraGrid1: Cost = Packing ? Txt_UnitCost
                if (ultraGrid1 != null && ultraGrid1.Rows != null)
                {
                    float unitCost = 0;
                    if (!string.IsNullOrWhiteSpace(Txt_UnitCost.Text))
                    {
                        float.TryParse(Txt_UnitCost.Text, out unitCost);
                    }

                    foreach (var row in ultraGrid1.Rows)
                    {
                        if (row.Cells.Exists(colPacking) && row.Cells.Exists("Cost"))
                        {
                            float packing = 0;
                            if (row.Cells[colPacking].Value != null && row.Cells[colPacking].Value != DBNull.Value)
                            {
                                float.TryParse(row.Cells[colPacking].Value.ToString(), out packing);
                            }

                            float cost = packing * unitCost;
                            row.Cells["Cost"].Value = cost;
                        }
                    }
                }

                // If the calculator dialog is open, push the new unit cost to its read-only field
                if (unitCostCalculator != null && !unitCostCalculator.IsDisposed && unitCostCalculator.Visible)
                {
                    unitCostCalculator.SetUnitCost(Txt_UnitCost.Text);
                }

                // Update markup % in textBox1 based on current Retail Price
                if (!isUpdatingMarkup && !isLoadingItem && textBox1 != null)
                {
                    float unitCost;
                    float retailPrice;
                    if (float.TryParse(Txt_UnitCost.Text, out unitCost) &&
                        float.TryParse(txt_Retail.Text, out retailPrice) &&
                        unitCost > 0)
                    {
                        double markupPercent = (retailPrice / unitCost - 1.0) * 100.0;
                        isUpdatingMarkup = true;
                        textBox1.Text = markupPercent.ToString("0.00");
                        isUpdatingMarkup = false;

                        // Reflect the markup change into the calculator, if open
                        if (unitCostCalculator != null && !unitCostCalculator.IsDisposed && unitCostCalculator.Visible)
                        {
                            unitCostCalculator.SetMarginPercentage(textBox1.Text);
                        }
                    }
                }

                // Update all profit margins when unit cost changes
                UpdateAllProfitMargins();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Txt_UnitCost_TextChanged: {ex.Message}");
            }
        }

        // Event handler for Ult_Price cell value changed
        private void Ult_Price_CellChange(object sender, Infragistics.Win.UltraWinGrid.CellEventArgs e)
        {
            // Skip heavy recalculations when the cell change comes from textbox typing
            if (_isUpdatingPriceFromTextbox) return;
            try
            {
                if (e.Cell == null || e.Cell.Row == null)
                    return;

                Infragistics.Win.UltraWinGrid.UltraGridRow row = e.Cell.Row;

                // Only recalculate if RetailPrice, MRP, Cost, or TaxPer changed
                if (e.Cell.Column.Key == "RetailPrice" || e.Cell.Column.Key == "MRP" ||
                    e.Cell.Column.Key == "Cost" || e.Cell.Column.Key == "TaxPer")
                {
                    // Make sure all required values are present
                    if (row.Cells["RetailPrice"].Value != null &&
                        row.Cells["MRP"].Value != null &&
                        row.Cells["Packing"].Value != null &&
                        row.Cells["Cost"].Value != null)
                    {
                        float retailPrice = Convert.ToSingle(row.Cells["RetailPrice"].Value); // master retail per row
                        float mrp = Convert.ToSingle(row.Cells["MRP"].Value);
                        int packing = Convert.ToInt32(row.Cells["Packing"].Value);
                        float cost = Convert.ToSingle(row.Cells["Cost"].Value);

                        // Calculate margin amount and margin % (margin % mirrors txt_Retail profit margin)
                        float margin = retailPrice - cost; // values already per row
                        double retailMarginPercent = 0;
                        double.TryParse(ultraTextEditor4 != null ? ultraTextEditor4.Text : "0", out retailMarginPercent);
                        float marginPer = (float)retailMarginPercent;

                        // Set tax percentage if not already set
                        if (row.Cells["TaxPer"].Value == null)
                        {
                            if (!string.IsNullOrEmpty(txt_TaxPer.Text))
                            {
                                row.Cells["TaxPer"].Value = float.Parse(txt_TaxPer.Text);
                            }
                            else
                            {
                                row.Cells["TaxPer"].Value = 0;
                            }
                        }

                        // Calculate tax amount
                        float taxPer = Convert.ToSingle(row.Cells["TaxPer"].Value);
                        float taxAmt = retailPrice * taxPer / 100;

                        // Update values
                        row.Cells["MarginAmt"].Value = margin;
                        row.Cells["MarginPer"].Value = marginPer;
                        row.Cells["TaxAmt"].Value = taxAmt;

                        // Recompute tax display to reflect the current mode and retail
                        UpdateInclusiveExclusiveTaxDisplay();

                        SyncUomGridWithPriceGrid();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Ult_Price_CellChange: {ex.Message}");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //using (OpenFileDialog openFileDialog = new OpenFileDialog())
            //{
            // Set filter for image files
            openFileDialog1.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            openFileDialog1.Title = "Select an Image";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Get the path of the selected file
                string filePath = openFileDialog1.FileName;

                //pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
                //// Load the image into the PictureBox
                //pictureBox1.Image = Image.FromFile(filePath);


            }
            //}
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Ask user if they want to go to stock adjustment after saving
            DialogResult result = MessageBox.Show(
                "Do you want to go to stock adjustment?",
                "Stock Adjustment",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Save the item first and check if successful
                bool saveSuccess = SaveMasterAndReturnStatus();
                if (saveSuccess)
                {
                    // Open stock adjustment with the saved item
                    OpenStockAdjustmentAfterSave();
                }
            }
            else
            {
                // Just save normally
                this.SaveMaster();
            }
        }

        public void Save()
        {
            bool isUpdateMode = (btnUpdate != null && btnUpdate.Visible) ||
                ((button3 == null || !button3.Visible) && ((ItemMaster != null && ItemMaster.ItemId > 0) || CurrentItemId > 0));

            if (isUpdateMode)
            {
                btnUpdate_Click(this, EventArgs.Empty);
                return;
            }

            button3_Click(this, EventArgs.Empty);
        }

        /// <summary>
        /// Saves the item master and returns true if successful, false otherwise.
        /// Used when we need to know the save result before proceeding with other actions.
        /// </summary>
        private bool SaveMasterAndReturnStatus()
        {
            try
            {
                // Ensure ItemPriceSettings is properly initialized
                if (ItemPriceSettings == null)
                {
                    ItemPriceSettings = new ItemMasterPriceSettings();
                }

                // Basic validations - Required fields
                string desc = txt_description?.Text?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(desc))
                {
                    MessageBox.Show("Please enter Description.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txt_description?.Focus();
                    return false;
                }

                // Validate Item Type
                string itemType = txt_ItemType?.Text?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(itemType))
                {
                    MessageBox.Show("Please enter Item Type.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txt_ItemType?.Focus();
                    return false;
                }

                // Validate Barcode
                string barcode = string.Empty;
                try
                {
                    var txtBarcodeCtrl = GetMainBarcodeEditor();
                    barcode = txtBarcodeCtrl != null ? (txtBarcodeCtrl.Text ?? string.Empty).Trim() : string.Empty;
                }
                catch { barcode = string.Empty; }

                if (string.IsNullOrWhiteSpace(barcode))
                {
                    MessageBox.Show("Please enter Barcode.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    var txtBarcodeCtrl = GetMainBarcodeEditor();
                    txtBarcodeCtrl?.Focus();
                    return false;
                }

                // Validate Unit Cost
                string unitCost = Txt_UnitCost?.Text?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(unitCost))
                {
                    MessageBox.Show("Please enter Unit Cost.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Txt_UnitCost?.Focus();
                    return false;
                }
                // Validate that Unit Cost is a valid number
                float unitCostValue;
                if (!float.TryParse(unitCost, out unitCostValue) || unitCostValue <= 0)
                {
                    MessageBox.Show("Please enter a valid Unit Cost (must be greater than 0).", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Txt_UnitCost?.Focus();
                    return false;
                }

                // Validate Retail Price
                string retailPriceText = txt_Retail?.Text?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(retailPriceText))
                {
                    MessageBox.Show("Please enter Retail Price.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txt_Retail?.Focus();
                    return false;
                }
                // Validate that Retail Price is a valid number
                float retailPriceVal;
                if (!float.TryParse(retailPriceText, out retailPriceVal) || retailPriceVal <= 0)
                {
                    MessageBox.Show("Please enter a valid Retail Price (must be greater than 0).", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txt_Retail?.Focus();
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txt_BaseUnit?.Text))
                {
                    MessageBox.Show("Please select Base Unit.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txt_BaseUnit?.Focus();
                    return false;
                }

                // Ensure at least one UOM row exists
                var uomDt = ultraGrid1?.DataSource as DataTable;
                if (uomDt == null || uomDt.Rows.Count == 0)
                {
                    MessageBox.Show("Please add at least one Unit in the UOM grid.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (!ValidateItemStatusInputs())
                {
                    return false;
                }

                // Check if ItemType is WEIGHT ITEM
                bool isWeightItem = !string.IsNullOrWhiteSpace(itemType) &&
                    string.Equals(itemType, "WEIGHT ITEM", StringComparison.OrdinalIgnoreCase);

                // Validate barcode for WEIGHT ITEM (must be 7-9 characters)
                if (isWeightItem)
                {
                    if (string.IsNullOrWhiteSpace(barcode))
                    {
                        MessageBox.Show("Barcode is required for WEIGHT ITEM.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        var txtBarcodeCtrl = GetMainBarcodeEditor();
                        txtBarcodeCtrl?.Focus();
                        return false;
                    }

                    int barcodeLength = barcode.Length;
                    if (barcodeLength < 7 || barcodeLength > 9)
                    {
                        MessageBox.Show("Barcode must be 7-9 characters for WEIGHT ITEM.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        var txtBarcodeCtrl = GetMainBarcodeEditor();
                        txtBarcodeCtrl?.Focus();
                        return false;
                    }
                }

                if (!ValidateMainAndAlternativeBarcodeUniqueness(barcode, 0))
                {
                    return false;
                }

                // Populate ItemMaster core fields from form
                try
                {
                    ItemMaster.CompanyId = Convert.ToInt32(ModelClass.DataBase.CompanyId);
                    ItemMaster.BranchId = Convert.ToInt32(ModelClass.DataBase.BranchId);
                    ItemMaster.FinYearId = SessionContext.FinYearId;
                    int parsedItemNo;
                    if (int.TryParse(txt_ItemNo?.Text, out parsedItemNo))
                    {
                        ItemMaster.ItemNo = parsedItemNo;
                    }
                    ItemMaster.Description = desc;
                    ItemMaster.Barcode = barcode;
                    ItemMaster.NameInLocalLanguage = txt_LocalLanguage?.Text ?? string.Empty;
                    ItemMaster.Order_Cycle_Days = GetSmartReorderOrderCycleDays();
                    ItemMaster.Box_Quantity = GetSmartReorderBoxQuantity();
                    ItemMaster.Is_Perishable = GetSmartReorderIsPerishable();
                    // Resolve IDs from text controls where only names are present
                    ResolveAndAssignMasterIds();
                }
                catch { }

                ItemMaster.VendorId = ItemMaster.VendorId; // keep existing or resolved
                ItemPriceSettings.TaxType = txt_TaxType.Text;
                float taxPerVal; float taxAmtVal;
                float.TryParse(txt_TaxPer.Text, out taxPerVal); ItemPriceSettings.TaxPer = taxPerVal;
                float.TryParse(txt_TaxAmount.Text, out taxAmtVal); ItemPriceSettings.TaxAmt = taxAmtVal;

                // Set walking price: DB.RetailPrice stores walking price
                if (txt_walkin != null && !string.IsNullOrEmpty(txt_walkin.Text))
                {
                    float walkingPrice = 0;
                    if (float.TryParse(txt_walkin.Text, out walkingPrice))
                    {
                        ItemPriceSettings.RetailPrice = walkingPrice;
                    }
                }
                // Set retail price: DB.WholeSalePrice stores retail price
                if (txt_Retail != null && !string.IsNullOrEmpty(txt_Retail.Text))
                {
                    float retailPrice = 0;
                    if (float.TryParse(txt_Retail.Text, out retailPrice))
                    {
                        ItemPriceSettings.WholeSalePrice = retailPrice;
                    }
                }
                // Set credit price if available
                if (txt_CEP != null && !string.IsNullOrEmpty(txt_CEP.Text))
                {
                    float creditPrice = 0;
                    if (float.TryParse(txt_CEP.Text, out creditPrice))
                    {
                        ItemPriceSettings.CreditPrice = creditPrice;
                    }
                }
                // Set MRP if available
                if (txt_Mrp != null && !string.IsNullOrEmpty(txt_Mrp.Text))
                {
                    float mrpValue = 0;
                    if (float.TryParse(txt_Mrp.Text, out mrpValue))
                    {
                        ItemPriceSettings.MRP = mrpValue;
                    }
                }
                // Set Card Price if available
                if (txt_CardP != null && !string.IsNullOrEmpty(txt_CardP.Text))
                {
                    float cardPrice = 0;
                    if (float.TryParse(txt_CardP.Text, out cardPrice))
                    {
                        ItemPriceSettings.CardPrice = cardPrice;
                    }
                }

                // Capture markdown values from markdown editors
                if (ultraTextEditor16 != null && !string.IsNullOrEmpty(ultraTextEditor16.Text))
                {
                    double walkingMarkdown = 0;
                    if (double.TryParse(ultraTextEditor16.Text, out walkingMarkdown))
                    {
                        ItemPriceSettings.MDWalkinPrice = walkingMarkdown;
                    }
                }

                if (ultraTextEditor15 != null && !string.IsNullOrEmpty(ultraTextEditor15.Text))
                {
                    double creditMarkdown = 0;
                    if (double.TryParse(ultraTextEditor15.Text, out creditMarkdown))
                    {
                        ItemPriceSettings.MDCreditPrice = creditMarkdown;
                    }
                }

                if (ultraTextEditor14 != null && !string.IsNullOrEmpty(ultraTextEditor14.Text))
                {
                    double mrpMarkdown = 0;
                    if (double.TryParse(ultraTextEditor14.Text, out mrpMarkdown))
                    {
                        ItemPriceSettings.MDMrpPrice = mrpMarkdown;
                    }
                }

                if (ultraTextEditor13 != null && !string.IsNullOrEmpty(ultraTextEditor13.Text))
                {
                    double cardMarkdown = 0;
                    if (double.TryParse(ultraTextEditor13.Text, out cardMarkdown))
                    {
                        ItemPriceSettings.MDCardPrice = cardMarkdown;
                    }
                }
                var ultraTextEditor12 = this.Controls.Find("ultraTextEditor12", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                var ultraTextEditor11 = this.Controls.Find("ultraTextEditor11", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                if (ultraTextEditor12 != null && !string.IsNullOrEmpty(ultraTextEditor12.Text))
                {
                    double staffMarkdown = 0;
                    if (double.TryParse(ultraTextEditor12.Text, out staffMarkdown))
                    {
                        ItemPriceSettings.MDStaffPrice = staffMarkdown;
                    }
                }
                if (ultraTextEditor11 != null && !string.IsNullOrEmpty(ultraTextEditor11.Text))
                {
                    double minMarkdown = 0;
                    if (double.TryParse(ultraTextEditor11.Text, out minMarkdown))
                    {
                        ItemPriceSettings.MDMinPrice = minMarkdown;
                    }
                }

                // Retail markdown is always 0 since retail is the base price
                ItemPriceSettings.MDRetailPrice = 0;

                // Set costing field to "AVERAGE" as required
                ItemPriceSettings.Costing = "AVERAGE";

                // Attach current image bytes if present
                ItemPriceSettings.PhotoByteArray = currentImageBytes;

                // Ensure barcode is synchronized between text field and grid before saving
                SynchronizeBarcodeBeforeSave();

                // Ensure TaxPer and TaxType are synchronized to the price grid before saving
                SynchronizeTaxFieldsToPriceGrid();

                // Ensure Staff/Min values are calculated from their profit margins
                // and synchronized to the price grid just before saving
                try
                {
                    var staffMarginEditor = this.Controls.Find("ultraTextEditor6", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                    var staffPriceTextBox = this.Controls.Find("txt_SF", true).FirstOrDefault() as TextBox;
                    var staffMarkdownEditor = this.Controls.Find("ultraTextEditor12", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                    if (staffMarginEditor != null && staffPriceTextBox != null)
                    {
                        CalculateSellingPriceAndMarkdownFromProfitMargin(staffMarginEditor, staffPriceTextBox, staffMarkdownEditor);
                        SyncStaffPriceToPriceGridFromTxtSF(staffPriceTextBox.Text);
                    }

                    var minMarginEditor = this.Controls.Find("ultraTextEditor5", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                    var minPriceTextBox = this.Controls.Find("txt_MinP", true).FirstOrDefault() as TextBox;
                    var minMarkdownEditor = this.Controls.Find("ultraTextEditor11", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                    if (minMarginEditor != null && minPriceTextBox != null)
                    {
                        CalculateSellingPriceAndMarkdownFromProfitMargin(minMarginEditor, minPriceTextBox, minMarkdownEditor);
                        SyncMinPriceToPriceGridFromTxtMinP(minPriceTextBox.Text);
                    }
                }
                catch { }

                EnsureUomUnitIdsBeforeSave();

                // Use the UomDataGridView property which handles the conversion from ultraGrid1
                // Get Ult_Price data and convert to DataGridView for backward compatibility
                DataGridView tempPriceGrid = ConvertUltPriceToDataGridView();

                string Message = ItemRepository.SaveItemMaster(ItemMaster, ItemPriceSettings, UomDataGridView, tempPriceGrid, GetAlternativeBarcodesDataGridView());

                if (!string.IsNullOrEmpty(Message) && Message.StartsWith("Success"))
                {
                    TryPersistItemStatusForCurrentItem(true);
                    LogItemActivity("SAVE", BuildSaveActivityDetails());

                    // Raise event to notify other forms that item was updated
                    if (ItemMaster.ItemId > 0)
                    {
                        RaiseItemMasterUpdated(ItemMaster.ItemId);
                    }

                    var details = new Dictionary<string, string>
                    {
                        { "Barcode", barcode },
                        { "Item Name", desc },
                        { "Selling Price", "₹" + (!string.IsNullOrWhiteSpace(retailPriceText) ? retailPriceText : "0.00") }
                    };
                    frmSuccesMsg success = new frmSuccesMsg(
                        "Item saved successfully.",
                        "The item has been saved in Item Master.",
                        details
                    );
                    success.ShowDialog();
                    return true;
                }
                else
                {
                    string err = string.IsNullOrEmpty(Message) ? "Unknown error while saving item." : Message;
                    MessageBox.Show($"Save failed: {err}", "Save Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while saving: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Opens the stock adjustment form and loads the currently saved item into it.
        /// Called after a successful save when user chose to go to stock adjustment.
        /// </summary>
        private void OpenStockAdjustmentAfterSave()
        {
            try
            {
                // Get item data from the form (before clearing)
                string itemId = ItemMaster.ItemId > 0 ? ItemMaster.ItemId.ToString() : CurrentItemId.ToString();
                string barcode = "";
                string description = "";
                string unit = "";
                string stockQty = "0";

                // Get barcode from txt_barcode field
                var txtBarcodeCtrl = GetMainBarcodeEditor();
                if (txtBarcodeCtrl != null)
                {
                    barcode = txtBarcodeCtrl.Text?.Trim() ?? "";
                }

                // Get description from txt_description field
                if (txt_description != null)
                {
                    description = txt_description.Text?.Trim() ?? "";
                }

                // Get unit from txt_BaseUnit field
                if (txt_BaseUnit != null)
                {
                    unit = txt_BaseUnit.Text?.Trim() ?? "";
                }

                // Get stock quantity from txt_qty or txt_available field
                var txtQtyCtrl = this.Controls.Find("txt_qty", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                if (txtQtyCtrl != null && !string.IsNullOrEmpty(txtQtyCtrl.Text))
                {
                    stockQty = txtQtyCtrl.Text.Trim();
                }
                else
                {
                    var txtAvailableCtrl = this.Controls.Find("txt_available", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                    if (txtAvailableCtrl != null && !string.IsNullOrEmpty(txtAvailableCtrl.Text))
                    {
                        stockQty = txtAvailableCtrl.Text.Trim();
                    }
                }

                // Clear the form after saving (same as normal save)
                this.clear();
                TryRefreshItemDialog();

                // Find the parent Home form
                Form parentHome = FindParentHome();

                if (parentHome != null)
                {
                    // Check if FrmStockAdjustment is already open in a tab
                    var openFormInTabSafeMethod = parentHome.GetType().GetMethod("OpenFormInTabSafe",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (openFormInTabSafeMethod != null)
                    {
                        // Check if stock adjustment form already exists in a tab
                        var tabControlMainField = parentHome.GetType().GetField("tabControlMain",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                        PosBranch_Win.Transaction.FrmStockAdjustment stockAdjustmentForm = null;

                        if (tabControlMainField != null)
                        {
                            var tabControl = tabControlMainField.GetValue(parentHome) as Infragistics.Win.UltraWinTabControl.UltraTabControl;

                            if (tabControl != null)
                            {
                                // Check for existing Stock Adjustment tab
                                foreach (Infragistics.Win.UltraWinTabControl.UltraTab tab in tabControl.Tabs)
                                {
                                    if (tab.Text == "Stock Adjustment" && tab.TabPage.Controls.Count > 0 &&
                                        tab.TabPage.Controls[0] is PosBranch_Win.Transaction.FrmStockAdjustment existingForm &&
                                        !existingForm.IsDisposed)
                                    {
                                        // Activate existing tab
                                        stockAdjustmentForm = existingForm;
                                        tabControl.SelectedTab = tab;
                                        existingForm.BringToFront();
                                        existingForm.Focus();

                                        // Load the current item into the stock adjustment form
                                        stockAdjustmentForm.AddItemToGrid(itemId, barcode, description, unit, stockQty);
                                        return;
                                    }
                                }
                            }
                        }

                        // Create new stock adjustment form and open in tab
                        stockAdjustmentForm = new PosBranch_Win.Transaction.FrmStockAdjustment();

                        // Open in tab - the form's Load event will handle initialization
                        openFormInTabSafeMethod.Invoke(parentHome, new object[] { stockAdjustmentForm, "Stock Adjustment" });

                        // Wait for form to load, then add the item
                        // Use BeginInvoke to ensure form is fully loaded
                        stockAdjustmentForm.BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                stockAdjustmentForm.AddItemToGrid(itemId, barcode, description, unit, stockQty);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error adding item to stock adjustment: {ex.Message}");
                            }
                        }));
                        return;
                    }
                }

                // Fallback: Open as standalone window if Home form or method not found
                var existingStockAdjustmentForm = Application.OpenForms
                    .OfType<PosBranch_Win.Transaction.FrmStockAdjustment>()
                    .FirstOrDefault(f => !f.IsDisposed);

                if (existingStockAdjustmentForm == null)
                {
                    var stockAdjustmentForm = new PosBranch_Win.Transaction.FrmStockAdjustment();
                    stockAdjustmentForm.StartPosition = FormStartPosition.CenterScreen;
                    stockAdjustmentForm.Show();

                    // Wait for form to load, then add the item
                    stockAdjustmentForm.Shown += (s, evt) =>
                    {
                        try
                        {
                            stockAdjustmentForm.AddItemToGrid(itemId, barcode, description, unit, stockQty);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error adding item to stock adjustment: {ex.Message}");
                        }
                    };
                }
                else
                {
                    if (existingStockAdjustmentForm.WindowState == FormWindowState.Minimized)
                    {
                        existingStockAdjustmentForm.WindowState = FormWindowState.Normal;
                    }
                    existingStockAdjustmentForm.BringToFront();
                    existingStockAdjustmentForm.Focus();

                    // Load the current item into the stock adjustment form
                    existingStockAdjustmentForm.AddItemToGrid(itemId, barcode, description, unit, stockQty);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening stock adjustment: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show("Item saved successfully, but unable to open the stock adjustment form. Please open it manually.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        public void SaveMaster()
        {
            try
            {
                // Ensure ItemPriceSettings is properly initialized
                if (ItemPriceSettings == null)
                {
                    ItemPriceSettings = new ItemMasterPriceSettings();
                }

                // Basic validations - Required fields
                string desc = txt_description?.Text?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(desc))
                {
                    MessageBox.Show("Please enter Description.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txt_description?.Focus();
                    return;
                }

                // Validate Item Type
                string itemType = txt_ItemType?.Text?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(itemType))
                {
                    MessageBox.Show("Please enter Item Type.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txt_ItemType?.Focus();
                    return;
                }

                // Validate Barcode
                string barcode = string.Empty;
                try
                {
                    var txtBarcodeCtrl = GetMainBarcodeEditor();
                    barcode = txtBarcodeCtrl != null ? (txtBarcodeCtrl.Text ?? string.Empty).Trim() : string.Empty;
                }
                catch { barcode = string.Empty; }

                if (string.IsNullOrWhiteSpace(barcode))
                {
                    MessageBox.Show("Please enter Barcode.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    var txtBarcodeCtrl = GetMainBarcodeEditor();
                    txtBarcodeCtrl?.Focus();
                    return;
                }

                // Validate Unit Cost
                string unitCost = Txt_UnitCost?.Text?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(unitCost))
                {
                    MessageBox.Show("Please enter Unit Cost.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Txt_UnitCost?.Focus();
                    return;
                }
                // Validate that Unit Cost is a valid number
                float unitCostValue;
                if (!float.TryParse(unitCost, out unitCostValue) || unitCostValue <= 0)
                {
                    MessageBox.Show("Please enter a valid Unit Cost (must be greater than 0).", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Txt_UnitCost?.Focus();
                    return;
                }

                // Validate Retail Price
                string retailPriceText = txt_Retail?.Text?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(retailPriceText))
                {
                    MessageBox.Show("Please enter Retail Price.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txt_Retail?.Focus();
                    return;
                }
                // Validate that Retail Price is a valid number
                float retailPriceVal;
                if (!float.TryParse(retailPriceText, out retailPriceVal) || retailPriceVal <= 0)
                {
                    MessageBox.Show("Please enter a valid Retail Price (must be greater than 0).", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txt_Retail?.Focus();
                    return;
                }

                // Validate that Unit Cost is not higher than Retail Price
                if (unitCostValue > retailPriceVal)
                {
                    MessageBox.Show("Unit Cost cannot be higher than Retail Price.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Txt_UnitCost?.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txt_BaseUnit?.Text))
                {
                    MessageBox.Show("Please select Base Unit.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txt_BaseUnit?.Focus();
                    return;
                }

                // Ensure at least one UOM row exists
                var uomDt = ultraGrid1?.DataSource as DataTable;
                if (uomDt == null || uomDt.Rows.Count == 0)
                {
                    MessageBox.Show("Please add at least one Unit in the UOM grid.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!ValidateItemStatusInputs())
                {
                    return;
                }

                // Check if ItemType is WEIGHT ITEM (itemType already retrieved in validation above)
                bool isWeightItem = !string.IsNullOrWhiteSpace(itemType) &&
                    string.Equals(itemType, "WEIGHT ITEM", StringComparison.OrdinalIgnoreCase);

                // Validate barcode for WEIGHT ITEM (must be 7-9 characters)
                if (isWeightItem)
                {
                    if (string.IsNullOrWhiteSpace(barcode))
                    {
                        MessageBox.Show("Barcode is required for WEIGHT ITEM.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        var txtBarcodeCtrl = GetMainBarcodeEditor();
                        txtBarcodeCtrl?.Focus();
                        return;
                    }

                    int barcodeLength = barcode.Length;
                    if (barcodeLength < 7 || barcodeLength > 9)
                    {
                        MessageBox.Show("Barcode must be 7-9 characters for WEIGHT ITEM.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        var txtBarcodeCtrl = GetMainBarcodeEditor();
                        txtBarcodeCtrl?.Focus();
                        return;
                    }
                }

                if (!ValidateMainAndAlternativeBarcodeUniqueness(barcode, 0))
                {
                    return;
                }

                // Validate AliasBarcode uniqueness in ultraGrid1
                try
                {
                    var aliasBarcodes = new List<string>();
                    if (ultraGrid1?.Rows != null)
                    {
                        foreach (var row in ultraGrid1.Rows)
                        {
                            if (row.Cells.Exists("AliasBarcode"))
                            {
                                string aliasBarcode = row.Cells["AliasBarcode"].Value?.ToString()?.Trim();
                                if (!string.IsNullOrWhiteSpace(aliasBarcode))
                                {
                                    aliasBarcodes.Add(aliasBarcode);
                                }
                            }
                        }
                    }
                    if (aliasBarcodes.Any())
                    {
                        string duplicateAlias = ItemRepository.CheckAliasBarcodesExist(aliasBarcodes, 0);
                        if (!string.IsNullOrEmpty(duplicateAlias))
                        {
                            MessageBox.Show($"Alias Barcode '{duplicateAlias}' already exists.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                }
                catch (MissingMethodException)
                {
                    // Method not found in Repository DLL - skip validation (rebuild solution to enable)
                    System.Diagnostics.Debug.WriteLine("CheckAliasBarcodesExist method not found. Please rebuild the Repository project.");
                }

                // Populate ItemMaster core fields from form
                try
                {
                    ItemMaster.CompanyId = Convert.ToInt32(ModelClass.DataBase.CompanyId);
                    ItemMaster.BranchId = Convert.ToInt32(ModelClass.DataBase.BranchId);
                    ItemMaster.FinYearId = SessionContext.FinYearId;
                    int parsedItemNo;
                    if (int.TryParse(txt_ItemNo?.Text, out parsedItemNo))
                    {
                        ItemMaster.ItemNo = parsedItemNo;
                    }
                    ItemMaster.Description = desc;
                    ItemMaster.Barcode = barcode;
                    ItemMaster.NameInLocalLanguage = txt_LocalLanguage?.Text ?? string.Empty;
                    ItemMaster.Order_Cycle_Days = GetSmartReorderOrderCycleDays();
                    ItemMaster.Box_Quantity = GetSmartReorderBoxQuantity();
                    ItemMaster.Is_Perishable = GetSmartReorderIsPerishable();
                    // Resolve IDs from text controls where only names are present
                    ResolveAndAssignMasterIds();
                }
                catch { }

                ItemMaster.VendorId = ItemMaster.VendorId; // keep existing or resolved
                ItemPriceSettings.TaxType = txt_TaxType.Text;
                float taxPerVal; float taxAmtVal;
                float.TryParse(txt_TaxPer.Text, out taxPerVal); ItemPriceSettings.TaxPer = taxPerVal;
                float.TryParse(txt_TaxAmount.Text, out taxAmtVal); ItemPriceSettings.TaxAmt = taxAmtVal;

                // Set walking price: DB.RetailPrice stores walking price
                if (txt_walkin != null && !string.IsNullOrEmpty(txt_walkin.Text))
                {
                    float walkingPrice = 0;
                    if (float.TryParse(txt_walkin.Text, out walkingPrice))
                    {
                        ItemPriceSettings.RetailPrice = walkingPrice;
                    }
                }
                // Set retail price: DB.WholeSalePrice stores retail price
                if (txt_Retail != null && !string.IsNullOrEmpty(txt_Retail.Text))
                {
                    float retailPrice = 0;
                    if (float.TryParse(txt_Retail.Text, out retailPrice))
                    {
                        ItemPriceSettings.WholeSalePrice = retailPrice;
                    }
                }
                // Set credit price if available
                if (txt_CEP != null && !string.IsNullOrEmpty(txt_CEP.Text))
                {
                    float creditPrice = 0;
                    if (float.TryParse(txt_CEP.Text, out creditPrice))
                    {
                        ItemPriceSettings.CreditPrice = creditPrice;
                    }
                }
                // Set MRP if available
                if (txt_Mrp != null && !string.IsNullOrEmpty(txt_Mrp.Text))
                {
                    float mrpValue = 0;
                    if (float.TryParse(txt_Mrp.Text, out mrpValue))
                    {
                        ItemPriceSettings.MRP = mrpValue;
                    }
                }
                // Set Card Price if available
                if (txt_CardP != null && !string.IsNullOrEmpty(txt_CardP.Text))
                {
                    float cardPrice = 0;
                    if (float.TryParse(txt_CardP.Text, out cardPrice))
                    {
                        ItemPriceSettings.CardPrice = cardPrice;
                    }
                }

                // Capture markdown values from markdown editors
                if (ultraTextEditor16 != null && !string.IsNullOrEmpty(ultraTextEditor16.Text))
                {
                    double walkingMarkdown = 0;
                    if (double.TryParse(ultraTextEditor16.Text, out walkingMarkdown))
                    {
                        ItemPriceSettings.MDWalkinPrice = walkingMarkdown;
                    }
                }

                if (ultraTextEditor15 != null && !string.IsNullOrEmpty(ultraTextEditor15.Text))
                {
                    double creditMarkdown = 0;
                    if (double.TryParse(ultraTextEditor15.Text, out creditMarkdown))
                    {
                        ItemPriceSettings.MDCreditPrice = creditMarkdown;
                    }
                }

                if (ultraTextEditor14 != null && !string.IsNullOrEmpty(ultraTextEditor14.Text))
                {
                    double mrpMarkdown = 0;
                    if (double.TryParse(ultraTextEditor14.Text, out mrpMarkdown))
                    {
                        ItemPriceSettings.MDMrpPrice = mrpMarkdown;
                    }
                }

                if (ultraTextEditor13 != null && !string.IsNullOrEmpty(ultraTextEditor13.Text))
                {
                    double cardMarkdown = 0;
                    if (double.TryParse(ultraTextEditor13.Text, out cardMarkdown))
                    {
                        ItemPriceSettings.MDCardPrice = cardMarkdown;
                    }
                }
                var ultraTextEditor12 = this.Controls.Find("ultraTextEditor12", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                var ultraTextEditor11 = this.Controls.Find("ultraTextEditor11", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                if (ultraTextEditor12 != null && !string.IsNullOrEmpty(ultraTextEditor12.Text))
                {
                    double staffMarkdown = 0;
                    if (double.TryParse(ultraTextEditor12.Text, out staffMarkdown))
                    {
                        ItemPriceSettings.MDStaffPrice = staffMarkdown;
                    }
                }
                if (ultraTextEditor11 != null && !string.IsNullOrEmpty(ultraTextEditor11.Text))
                {
                    double minMarkdown = 0;
                    if (double.TryParse(ultraTextEditor11.Text, out minMarkdown))
                    {
                        ItemPriceSettings.MDMinPrice = minMarkdown;
                    }
                }

                // Retail markdown is always 0 since retail is the base price
                ItemPriceSettings.MDRetailPrice = 0;

                // Set costing field to "AVERAGE" as required
                ItemPriceSettings.Costing = "AVERAGE";

                // Attach current image bytes if present
                ItemPriceSettings.PhotoByteArray = currentImageBytes;

                // Note: Barcode in ultraGrid1 acts as independent alias barcode (no sync with txt_barcode)

                // Ensure TaxPer and TaxType are synchronized to the price grid before saving
                SynchronizeTaxFieldsToPriceGrid();

                // Ensure Staff/Min values are calculated from their profit margins (like ultraTextEditor7)
                // and synchronized to the price grid just before saving
                try
                {
                    var staffMarginEditor = this.Controls.Find("ultraTextEditor6", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                    var staffPriceTextBox = this.Controls.Find("txt_SF", true).FirstOrDefault() as TextBox;
                    var staffMarkdownEditor = this.Controls.Find("ultraTextEditor12", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                    if (staffMarginEditor != null && staffPriceTextBox != null)
                    {
                        CalculateSellingPriceAndMarkdownFromProfitMargin(staffMarginEditor, staffPriceTextBox, staffMarkdownEditor);
                        SyncStaffPriceToPriceGridFromTxtSF(staffPriceTextBox.Text);
                    }

                    var minMarginEditor = this.Controls.Find("ultraTextEditor5", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                    var minPriceTextBox = this.Controls.Find("txt_MinP", true).FirstOrDefault() as TextBox;
                    var minMarkdownEditor = this.Controls.Find("ultraTextEditor11", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                    if (minMarginEditor != null && minPriceTextBox != null)
                    {
                        CalculateSellingPriceAndMarkdownFromProfitMargin(minMarginEditor, minPriceTextBox, minMarkdownEditor);
                        SyncMinPriceToPriceGridFromTxtMinP(minPriceTextBox.Text);
                    }
                }
                catch { }

                EnsureUomUnitIdsBeforeSave();

                // Use the UomDataGridView property which handles the conversion from ultraGrid1
                // Get Ult_Price data and convert to DataGridView for backward compatibility
                DataGridView tempPriceGrid = ConvertUltPriceToDataGridView();

                // Add debugging information
                System.Diagnostics.Debug.WriteLine($"Saving item: {ItemMaster.Description}");
                System.Diagnostics.Debug.WriteLine($"UOM Grid Rows: {UomDataGridView.Rows.Count}");
                System.Diagnostics.Debug.WriteLine($"Price Grid Rows: {tempPriceGrid.Rows.Count}");
                System.Diagnostics.Debug.WriteLine($"ItemMaster.CompanyId: {ItemMaster.CompanyId}");
                System.Diagnostics.Debug.WriteLine($"ItemMaster.BranchId: {ItemMaster.BranchId}");
                System.Diagnostics.Debug.WriteLine($"ItemMaster.FinYearId: {ItemMaster.FinYearId}");

                string Message = ItemRepository.SaveItemMaster(ItemMaster, ItemPriceSettings, UomDataGridView, tempPriceGrid, GetAlternativeBarcodesDataGridView());

                // Add debugging for the result
                System.Diagnostics.Debug.WriteLine($"Save result: {Message}");

                if (!string.IsNullOrEmpty(Message) && Message.StartsWith("Success"))
                {
                    TryPersistItemStatusForCurrentItem(true);
                    LogItemActivity("SAVE", BuildSaveActivityDetails());

                    // Raise event to notify other forms that item was updated
                    if (ItemMaster.ItemId > 0)
                    {
                        RaiseItemMasterUpdated(ItemMaster.ItemId);
                    }

                    var details = new Dictionary<string, string>
                    {
                        { "Barcode", barcode },
                        { "Item Name", desc },
                        { "Selling Price", "₹" + (!string.IsNullOrWhiteSpace(retailPriceText) ? retailPriceText : "0.00") }
                    };
                    frmSuccesMsg success = new frmSuccesMsg(
                        "Item saved successfully.",
                        "The item has been saved in Item Master.",
                        details
                    );
                    success.ShowDialog();
                    // Clear everything after successful save
                    this.clear();
                    TryRefreshItemDialog();

                    var txtBarcodeCtrl = GetMainBarcodeEditor();
                    txtBarcodeCtrl?.Focus();
                }
                else
                {
                    // Show failure message clearly with more details
                    string err = string.IsNullOrEmpty(Message) ? "Unknown error while saving item." : Message;
                    System.Diagnostics.Debug.WriteLine($"Save failed: {err}");
                    MessageBox.Show($"Save failed: {err}\n\nDebug Info:\nUOM Rows: {UomDataGridView.Rows.Count}\nPrice Rows: {tempPriceGrid.Rows.Count}\nCompanyId: {ItemMaster.CompanyId}\nBranchId: {ItemMaster.BranchId}", "Save Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while saving: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void UpdateItem()
        {
            // Ensure ItemPriceSettings is properly initialized
            if (ItemPriceSettings == null)
            {
                ItemPriceSettings = new ItemMasterPriceSettings();
            }

            // Get current item ID for update (to exclude from barcode check)
            int currentItemId = 0;
            if (ItemMaster.ItemId > 0)
            {
                currentItemId = ItemMaster.ItemId;
            }
            else if (this.CurrentItemId > 0)
            {
                currentItemId = this.CurrentItemId;
            }

            // Basic validations - Required fields
            string desc = txt_description?.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(desc))
            {
                MessageBox.Show("Please enter Description.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txt_description?.Focus();
                return;
            }

            // Validate Item Type
            string itemType = txt_ItemType?.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(itemType))
            {
                MessageBox.Show("Please enter Item Type.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txt_ItemType?.Focus();
                return;
            }

            // Validate Barcode
            string barcode = string.Empty;
            try
            {
                var txtBarcodeCtrl = GetMainBarcodeEditor();
                barcode = txtBarcodeCtrl != null ? (txtBarcodeCtrl.Text ?? string.Empty).Trim() : string.Empty;
            }
            catch { barcode = string.Empty; }

            if (string.IsNullOrWhiteSpace(barcode))
            {
                MessageBox.Show("Please enter Barcode.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                var txtBarcodeCtrl = GetMainBarcodeEditor();
                txtBarcodeCtrl?.Focus();
                return;
            }

            // Validate Unit Cost
            string unitCost = Txt_UnitCost?.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(unitCost))
            {
                MessageBox.Show("Please enter Unit Cost.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Txt_UnitCost?.Focus();
                return;
            }
            // Validate that Unit Cost is a valid number
            float unitCostValue;
            if (!float.TryParse(unitCost, out unitCostValue) || unitCostValue <= 0)
            {
                MessageBox.Show("Please enter a valid Unit Cost (must be greater than 0).", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Txt_UnitCost?.Focus();
                return;
            }

            // Validate Retail Price
            string retailPriceText = txt_Retail?.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(retailPriceText))
            {
                MessageBox.Show("Please enter Retail Price.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txt_Retail?.Focus();
                return;
            }
            // Validate that Retail Price is a valid number
            float retailPriceVal;
            if (!float.TryParse(retailPriceText, out retailPriceVal) || retailPriceVal <= 0)
            {
                MessageBox.Show("Please enter a valid Retail Price (must be greater than 0).", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txt_Retail?.Focus();
                return;
            }

            // Validate that Unit Cost is not higher than Retail Price
            if (unitCostValue > retailPriceVal)
            {
                MessageBox.Show("Unit Cost cannot be higher than Retail Price.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Txt_UnitCost?.Focus();
                return;
            }

            if (!ValidateItemStatusInputs())
            {
                return;
            }

            // Check if ItemType is WEIGHT ITEM
            bool isWeightItem = !string.IsNullOrWhiteSpace(itemType) &&
                string.Equals(itemType, "WEIGHT ITEM", StringComparison.OrdinalIgnoreCase);

            // Validate barcode for WEIGHT ITEM (must be 7-9 characters)
            if (isWeightItem)
            {
                if (string.IsNullOrWhiteSpace(barcode))
                {
                    MessageBox.Show("Barcode is required for WEIGHT ITEM.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    var txtBarcodeCtrl = GetMainBarcodeEditor();
                    txtBarcodeCtrl?.Focus();
                    return;
                }

                int barcodeLength = barcode.Length;
                if (barcodeLength < 7 || barcodeLength > 9)
                {
                    MessageBox.Show("Barcode must be 7-9 characters for WEIGHT ITEM.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    var txtBarcodeCtrl = GetMainBarcodeEditor();
                    txtBarcodeCtrl?.Focus();
                    return;
                }
            }

            if (!ValidateLoadedItemBarcodeIsUnchanged(barcode))
            {
                return;
            }

            if (!ValidateMainAndAlternativeBarcodeUniqueness(barcode, currentItemId, false))
            {
                return;
            }

            // Validate AliasBarcode uniqueness in ultraGrid1 (exclude current item)
            try
            {
                var aliasBarcodes = new List<string>();
                if (ultraGrid1?.Rows != null)
                {
                    foreach (var row in ultraGrid1.Rows)
                    {
                        if (row.Cells.Exists("AliasBarcode"))
                        {
                            string aliasBarcode = row.Cells["AliasBarcode"].Value?.ToString()?.Trim();
                            if (!string.IsNullOrWhiteSpace(aliasBarcode))
                            {
                                aliasBarcodes.Add(aliasBarcode);
                            }
                        }
                    }
                }
                if (aliasBarcodes.Any())
                {
                    string duplicateAlias = ItemRepository.CheckAliasBarcodesExist(aliasBarcodes, currentItemId);
                    if (!string.IsNullOrEmpty(duplicateAlias))
                    {
                        MessageBox.Show($"Alias Barcode '{duplicateAlias}' already exists.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
            }
            catch (MissingMethodException)
            {
                // Method not found in Repository DLL - skip validation (rebuild solution to enable)
                System.Diagnostics.Debug.WriteLine("CheckAliasBarcodesExist method not found. Please rebuild the Repository project.");
            }

            // Populate ItemMaster fields for update
            try
            {
                ItemMaster.CompanyId = Convert.ToInt32(ModelClass.DataBase.CompanyId);
                ItemMaster.BranchId = Convert.ToInt32(ModelClass.DataBase.BranchId);
                ItemMaster.FinYearId = SessionContext.FinYearId;
                int parsedItemNo;
                if (int.TryParse(txt_ItemNo?.Text, out parsedItemNo))
                {
                    ItemMaster.ItemNo = parsedItemNo;
                }
                // Ensure ItemId is set for update
                if (ItemMaster.ItemId <= 0 && this.CurrentItemId > 0)
                    ItemMaster.ItemId = this.CurrentItemId;
                ItemMaster.Description = txt_description?.Text ?? string.Empty;
                ItemMaster.Barcode = barcode;
                ItemMaster.NameInLocalLanguage = txt_LocalLanguage?.Text ?? string.Empty;
                ItemMaster.Order_Cycle_Days = GetSmartReorderOrderCycleDays();
                ItemMaster.Box_Quantity = GetSmartReorderBoxQuantity();
                ItemMaster.Is_Perishable = GetSmartReorderIsPerishable();
                ResolveAndAssignMasterIds();
            }
            catch { }
            ItemMaster.VendorId = ItemMaster.VendorId;
            ItemPriceSettings.TaxType = txt_TaxType.Text;
            // Safe parsing for tax fields
            float taxPerVal = 0f; float taxAmtVal = 0f;
            float.TryParse(txt_TaxPer.Text, out taxPerVal);
            float.TryParse(txt_TaxAmount.Text, out taxAmtVal);
            ItemPriceSettings.TaxPer = taxPerVal;
            ItemPriceSettings.TaxAmt = taxAmtVal;
            // Set costing field to "AVERAGE" as required
            ItemPriceSettings.Costing = "AVERAGE";
            // Attach current image bytes for update as well
            ItemPriceSettings.PhotoByteArray = currentImageBytes;

            // Ensure barcode is synchronized between text field and grid before updating
            SynchronizeBarcodeBeforeSave();

            // Ensure TaxPer and TaxType are synchronized to the price grid before updating
            SynchronizeTaxFieldsToPriceGrid();

            // Ensure Staff/Min values are calculated from their profit margins (like ultraTextEditor7)
            // and synchronized to the price grid just before updating
            try
            {
                var staffMarginEditor = this.Controls.Find("ultraTextEditor6", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                var staffPriceTextBox = this.Controls.Find("txt_SF", true).FirstOrDefault() as TextBox;
                var staffMarkdownEditor = this.Controls.Find("ultraTextEditor12", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                if (staffMarginEditor != null && staffPriceTextBox != null)
                {
                    CalculateSellingPriceAndMarkdownFromProfitMargin(staffMarginEditor, staffPriceTextBox, staffMarkdownEditor);
                    SyncStaffPriceToPriceGridFromTxtSF(staffPriceTextBox.Text);
                }

                var minMarginEditor = this.Controls.Find("ultraTextEditor5", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                var minPriceTextBox = this.Controls.Find("txt_MinP", true).FirstOrDefault() as TextBox;
                var minMarkdownEditor = this.Controls.Find("ultraTextEditor11", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                if (minMarginEditor != null && minPriceTextBox != null)
                {
                    CalculateSellingPriceAndMarkdownFromProfitMargin(minMarginEditor, minPriceTextBox, minMarkdownEditor);
                    SyncMinPriceToPriceGridFromTxtMinP(minPriceTextBox.Text);
                }
            }
            catch { }

            // Set walking price: DB.RetailPrice stores walking price
            if (txt_walkin != null && !string.IsNullOrEmpty(txt_walkin.Text))
            {
                float walkingPrice = 0;
                if (float.TryParse(txt_walkin.Text, out walkingPrice))
                {
                    ItemPriceSettings.RetailPrice = walkingPrice;
                }
            }
            // Set retail price: DB.WholeSalePrice stores retail price
            if (txt_Retail != null && !string.IsNullOrEmpty(txt_Retail.Text))
            {
                float retailPrice = 0;
                if (float.TryParse(txt_Retail.Text, out retailPrice))
                {
                    ItemPriceSettings.WholeSalePrice = retailPrice;
                }
            }
            // Set credit price if available
            if (txt_CEP != null && !string.IsNullOrEmpty(txt_CEP.Text))
            {
                float creditPrice = 0;
                if (float.TryParse(txt_CEP.Text, out creditPrice))
                {
                    ItemPriceSettings.CreditPrice = creditPrice;
                }
            }
            // Set MRP if available
            if (txt_Mrp != null && !string.IsNullOrEmpty(txt_Mrp.Text))
            {
                float mrpValue = 0;
                if (float.TryParse(txt_Mrp.Text, out mrpValue))
                {
                    ItemPriceSettings.MRP = mrpValue;
                }
            }
            // Set Card Price if available
            if (txt_CardP != null && !string.IsNullOrEmpty(txt_CardP.Text))
            {
                float cardPrice = 0;
                if (float.TryParse(txt_CardP.Text, out cardPrice))
                {
                    ItemPriceSettings.CardPrice = cardPrice;
                }
            }

            // Capture markdown values from markdown editors for update
            if (ultraTextEditor16 != null && !string.IsNullOrEmpty(ultraTextEditor16.Text))
            {
                double walkingMarkdown = 0;
                if (double.TryParse(ultraTextEditor16.Text, out walkingMarkdown))
                {
                    ItemPriceSettings.MDWalkinPrice = walkingMarkdown;
                }
            }

            if (ultraTextEditor15 != null && !string.IsNullOrEmpty(ultraTextEditor15.Text))
            {
                double creditMarkdown = 0;
                if (double.TryParse(ultraTextEditor15.Text, out creditMarkdown))
                {
                    ItemPriceSettings.MDCreditPrice = creditMarkdown;
                }
            }

            if (ultraTextEditor14 != null && !string.IsNullOrEmpty(ultraTextEditor14.Text))
            {
                double mrpMarkdown = 0;
                if (double.TryParse(ultraTextEditor14.Text, out mrpMarkdown))
                {
                    ItemPriceSettings.MDMrpPrice = mrpMarkdown;
                }
            }

            if (ultraTextEditor13 != null && !string.IsNullOrEmpty(ultraTextEditor13.Text))
            {
                double cardMarkdown = 0;
                if (double.TryParse(ultraTextEditor13.Text, out cardMarkdown))
                {
                    ItemPriceSettings.MDCardPrice = cardMarkdown;
                }
            }
            var ultraTextEditor12s = this.Controls.Find("ultraTextEditor12", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
            var ultraTextEditor11s = this.Controls.Find("ultraTextEditor11", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
            if (ultraTextEditor12s != null && !string.IsNullOrEmpty(ultraTextEditor12s.Text))
            {
                double staffMarkdown = 0;
                if (double.TryParse(ultraTextEditor12s.Text, out staffMarkdown))
                {
                    ItemPriceSettings.MDStaffPrice = staffMarkdown;
                }
            }
            if (ultraTextEditor11s != null && !string.IsNullOrEmpty(ultraTextEditor11s.Text))
            {
                double minMarkdown = 0;
                if (double.TryParse(ultraTextEditor11s.Text, out minMarkdown))
                {
                    ItemPriceSettings.MDMinPrice = minMarkdown;
                }
            }

            // Retail markdown is always 0 since retail is the base price
            ItemPriceSettings.MDRetailPrice = 0;

            EnsureUomUnitIdsBeforeSave();

            // Get Ult_Price data and convert to DataGridView for backward compatibility
            DataGridView tempPriceGrid = ConvertUltPriceToDataGridView();
            ItemGet oldItemSnapshot = ItemRepository.GetByIdItem(currentItemId);
            string oldStatusSnapshot = GetOldItemStatus(currentItemId);
            PriceSnapshot oldPriceSnapshot = GetCurrentBasePriceSnapshot(currentItemId);
            string Message = ItemRepository.UpdateItemMaster(ItemMaster, ItemPriceSettings, UomDataGridView, tempPriceGrid, GetAlternativeBarcodesDataGridView());
            if (!string.IsNullOrEmpty(Message) && Message.StartsWith("Success"))
            {
                TryPersistItemStatusForCurrentItem(true);
                LogItemActivity("UPDATE", BuildUpdateActivityDetails(oldItemSnapshot, oldStatusSnapshot, oldPriceSnapshot));

                // Raise event to notify other forms that item was updated
                if (ItemMaster.ItemId > 0)
                {
                    RaiseItemMasterUpdated(ItemMaster.ItemId);
                }

                var details = new Dictionary<string, string>
                {
                    { "Barcode", ItemMaster != null && !string.IsNullOrWhiteSpace(ItemMaster.Barcode) ? ItemMaster.Barcode : (txt_barcode != null ? txt_barcode.Text : "") },
                    { "Item Name", ItemMaster != null && !string.IsNullOrWhiteSpace(ItemMaster.Description) ? ItemMaster.Description : (txt_description != null ? txt_description.Text : "") },
                    { "Selling Price", "₹" + (txt_Retail != null && !string.IsNullOrWhiteSpace(txt_Retail.Text) ? txt_Retail.Text : "0.00") }
                };
                frmSuccesMsg success = new frmSuccesMsg(
                    "Item updated successfully.",
                    "The item details have been updated.",
                    details
                );
                success.ShowDialog();
                // Clear everything after successful update
                this.clear();
                TryRefreshItemDialog();

                var txtBarcodeCtrl = GetMainBarcodeEditor();
                txtBarcodeCtrl?.Focus();
            }
            else
            {
                MessageBox.Show(Message);
            }
        }

        // Resolve and assign master foreign key IDs from visible text fields
        private void ResolveAndAssignMasterIds()
        {
            try
            {
                Dropdowns drop = new Dropdowns();

                // Item Type
                string itemTypeName = txt_ItemType?.Text?.Trim() ?? string.Empty;
                if (!string.IsNullOrEmpty(itemTypeName))
                {
                    var types = drop.getItemTypeDDl()?.List?.ToList();
                    var match = types?.FirstOrDefault(t => string.Equals(t.ItemType, itemTypeName, StringComparison.OrdinalIgnoreCase));
                    if (match != null) ItemMaster.ItemTypeId = match.Id;
                }

                // Brand
                string brandName = txt_Brand?.Text?.Trim() ?? string.Empty;
                if (!string.IsNullOrEmpty(brandName))
                {
                    var brands = drop.getBrandDDl()?.List?.ToList();
                    var match = brands?.FirstOrDefault(b => string.Equals(b.BrandName, brandName, StringComparison.OrdinalIgnoreCase));
                    if (match != null) ItemMaster.BrandId = match.Id;
                }

                // Group
                string groupName = txt_Group?.Text?.Trim() ?? string.Empty;
                if (!string.IsNullOrEmpty(groupName))
                {
                    var groups = drop.getGroupDDl()?.List?.ToList();
                    var match = groups?.FirstOrDefault(g => string.Equals(g.GroupName, groupName, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                    {
                        ItemMaster.GroupId = match.Id;
                    }
                    else
                    {
                        // Auto-create missing group
                        try
                        {
                            var groupRepo = new Repository.MasterRepositry.GroupRepository();
                            var newGroup = new ModelClass.Master.Group { GroupName = groupName, _Operation = "CREATE", BranchId = 0 };
                            groupRepo.SaveGroup(newGroup);

                            // Re-fetch to get new ID
                            var updatedGroups = drop.getGroupDDl()?.List?.ToList();
                            var newMatch = updatedGroups?.FirstOrDefault(g => string.Equals(g.GroupName, groupName, StringComparison.OrdinalIgnoreCase));
                            if (newMatch != null) ItemMaster.GroupId = newMatch.Id;
                        }
                        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Auto-create group failed: {ex.Message}"); }
                    }
                }

                // Category
                string categoryName = txt_Category?.Text?.Trim() ?? string.Empty;
                if (!string.IsNullOrEmpty(categoryName))
                {
                    var cats = drop.getCategoryDDl(categoryName)?.List?.ToList();
                    var match = cats?.FirstOrDefault(c => string.Equals(c.CategoryName, categoryName, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                    {
                        ItemMaster.CategoryId = match.Id;
                    }
                    else
                    {
                        // Auto-create missing category
                        try
                        {
                            var catRepo = new Repository.MasterRepositry.CategoryRepository();
                            var newCat = new ModelClass.Master.Category { CategoryName = categoryName, GroupId = ItemMaster.GroupId > 0 ? ItemMaster.GroupId : 0, _Operation = "CREATE" };
                            catRepo.SaveCategory(newCat);

                            // Re-fetch to get new ID
                            var updatedCats = drop.getCategoryDDl(categoryName)?.List?.ToList();
                            var newMatch = updatedCats?.FirstOrDefault(c => string.Equals(c.CategoryName, categoryName, StringComparison.OrdinalIgnoreCase));
                            if (newMatch != null) ItemMaster.CategoryId = newMatch.Id;
                        }
                        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Auto-create category failed: {ex.Message}"); }
                    }
                }

                // Base Unit
                string unitName = txt_BaseUnit?.Text?.Trim() ?? string.Empty;
                if (!string.IsNullOrEmpty(unitName))
                {
                    var units = drop.getUnitDDl()?.List?.ToList();
                    var match = units?.FirstOrDefault(u => string.Equals(u.UnitName, unitName, StringComparison.OrdinalIgnoreCase));
                    if (match != null) ItemMaster.BaseUnitId = match.UnitID;
                }

                // For Customer Type
                ItemMaster.ForCustomerType = txt_CustomerType?.Text ?? string.Empty;

                // HSN Code
                try
                {
                    var hsnTextBox = this.Controls.Find("textBox4", true).FirstOrDefault() as TextBox;
                    if (hsnTextBox != null)
                    {
                        ItemMaster.HSNCode = hsnTextBox.Text ?? string.Empty;
                    }
                }
                catch { }
            }
            catch { }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            frmTaxTypeDialog frmTaxType = new frmTaxTypeDialog();
            frmTaxType.StartPosition = FormStartPosition.CenterScreen;
            frmTaxType.ShowDialog();
        }

        private void btn_Add_ItemIype_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            frmTaxPerDialog taxPer = new frmTaxPerDialog();
            taxPer.StartPosition = FormStartPosition.CenterScreen;
            taxPer.ShowDialog();
        }

        private void txt_TaxPer_TextChanged(object sender, EventArgs e)
        {
            // Keep isinclexcl in sync while editing tax percentage
            UpdateInclusiveExclusiveTaxDisplay();

            // Update price grid with new TaxPer value immediately
            try
            {
                Infragistics.Win.UltraWinGrid.UltraGrid Ult_Price =
                    this.Controls.Find("Ult_Price", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;

                if (Ult_Price != null && Ult_Price.Rows.Count > 0 && txt_TaxPer != null && !string.IsNullOrEmpty(txt_TaxPer.Text))
                {
                    float taxPer = 0f;
                    if (float.TryParse(txt_TaxPer.Text, out taxPer))
                    {
                        foreach (Infragistics.Win.UltraWinGrid.UltraGridRow row in Ult_Price.Rows)
                        {
                            if (row.Cells.Exists("TaxPer"))
                            {
                                row.Cells["TaxPer"].Value = taxPer;
                            }
                        }

                        // Recalculate tax amounts based on new TaxPer
                        RecalculatePriceGrid();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating price grid TaxPer in TextChanged: {ex.Message}");
            }

            // Notify other forms of real-time change
            NotifyItemMasterChanged();
        }

        private void txt_TaxPer_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void txt_TaxPer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                DataTable dt = ultraGrid1.DataSource as DataTable;
                if (dt != null && dt.Rows.Count > 0)
                {
                    // Find Ult_Price control
                    Infragistics.Win.UltraWinGrid.UltraGrid Ult_Price =
                        this.Controls.Find("Ult_Price", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;

                    if (Ult_Price != null && Ult_Price.Rows.Count > 0)
                    {
                        float taxPer = float.Parse(txt_TaxPer.Text);

                        foreach (Infragistics.Win.UltraWinGrid.UltraGridRow row in Ult_Price.Rows)
                        {
                            row.Cells["TaxPer"].Value = taxPer;
                        }

                        // Recalculate tax amounts
                        RecalculatePriceGrid();

                        // Update tax amount display (isinclexcl)
                        UpdateInclusiveExclusiveTaxDisplay();

                        // Notify other forms of real-time change
                        NotifyItemMasterChanged();
                    }
                }
            }
        }

        private void txt_TaxAmount_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                DataTable dt = ultraGrid1.DataSource as DataTable;
                if (dt != null && dt.Rows.Count > 0)
                {
                    // Find Ult_Price control
                    Infragistics.Win.UltraWinGrid.UltraGrid Ult_Price =
                        this.Controls.Find("Ult_Price", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;

                    if (Ult_Price != null && Ult_Price.Rows.Count > 0)
                    {
                        float taxAmt = float.Parse(txt_TaxAmount.Text);

                        foreach (Infragistics.Win.UltraWinGrid.UltraGridRow row in Ult_Price.Rows)
                        {
                            row.Cells["TaxAmt"].Value = taxAmt;
                        }

                        // Notify other forms of real-time change
                        NotifyItemMasterChanged();
                    }
                }
            }
        }

        // Add TextChanged handler for txt_TaxAmount
        private void txt_TaxAmount_TextChanged(object sender, EventArgs e)
        {
            // Notify other forms of real-time change
            NotifyItemMasterChanged();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.clear();
            var txtBarcodeCtrl = GetMainBarcodeEditor();
            txtBarcodeCtrl?.Focus();
        }

        private void CloneCurrentItemAsVariant()
        {
            try
            {
                int sourceItemId = 0;
                if (ItemMaster != null && ItemMaster.ItemId > 0)
                {
                    sourceItemId = ItemMaster.ItemId;
                }
                else if (CurrentItemId > 0)
                {
                    sourceItemId = CurrentItemId;
                }

                if (sourceItemId <= 0)
                {
                    MessageBox.Show("Please load an existing item first, then use Copy to create a variant.", "Clone Item", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                DataTable uomTable = CloneDataTable(ultraGrid1.DataSource as DataTable);
                DataTable priceTable = CloneDataTable(Ult_Price.DataSource as DataTable);
                DataTable altBarcodeTable = CloneDataTable(GetAlternativeBarcodeGrid()?.DataSource as DataTable);
                byte[] imageBytes = currentImageBytes != null ? (byte[])currentImageBytes.Clone() : null;
                string sourceDescription = txt_description?.Text?.Trim() ?? string.Empty;
                Item sourceMaster = ItemMaster;

                ClearClonedBarcodeIdentityValues(uomTable, "AliasBarcode");
                ClearClonedBarcodeIdentityValues(altBarcodeTable, "Barcode");

                if (uomTable != null && uomTable.Columns.Contains(colOpenStock))
                {
                    foreach (DataRow row in uomTable.Rows)
                    {
                        row[colOpenStock] = "0";
                    }
                }

                CurrentItemId = 0;
                ItemMaster = new Item();
                ItemPriceSettings = new ItemMasterPriceSettings();
                loadedItemMainBarcode = string.Empty;
                hasGeneratedItemNumberForBarcode = true;

                if (sourceMaster != null)
                {
                    // Keep master FKs from the loaded item so cloned saves remain discoverable in dialog joins.
                    ItemMaster.ItemTypeId = sourceMaster.ItemTypeId;
                    ItemMaster.VendorId = sourceMaster.VendorId;
                    ItemMaster.BrandId = sourceMaster.BrandId;
                    ItemMaster.GroupId = sourceMaster.GroupId;
                    ItemMaster.CategoryId = sourceMaster.CategoryId;
                    ItemMaster.BaseUnitId = sourceMaster.BaseUnitId;
                    ItemMaster.ForCustomerType = sourceMaster.ForCustomerType;
                    ItemMaster.HSNCode = sourceMaster.HSNCode;
                }

                GenerateNextItemNumberOnly();
                ResetItemStatusEditor();

                if (txt_barcode != null)
                {
                    txt_barcode.Clear();
                }

                SetMainBarcodeEditability(true, string.Empty);

                if (txt_qty != null) txt_qty.Text = "0";
                if (txt_available != null) txt_available.Text = "0";
                if (txt_hold != null) txt_hold.Text = "0.00";

                if (uomTable != null)
                {
                    EnsureUomGridPriceColumns(uomTable);
                    ultraGrid1.DataSource = uomTable;
                    if (ultraGrid1.DisplayLayout != null && ultraGrid1.DisplayLayout.Bands.Count > 0)
                    {
                        EnsureRetailPriceMrpDisplayOrder(ultraGrid1.DisplayLayout.Bands[0]);
                    }
                }

                if (priceTable != null)
                {
                    Ult_Price.DataSource = priceTable;
                }

                if (altBarcodeTable != null && GetAlternativeBarcodeGrid() != null)
                {
                    GetAlternativeBarcodeGrid().DataSource = altBarcodeTable;
                    ApplyUltraGrid1ThemeToAlternativeBarcodeGrid(GetAlternativeBarcodeGrid());
                }

                SetCurrentImage(imageBytes);
                ApplyItemStatusUiState();

                if (button3 != null) button3.Visible = true;
                if (btnUpdate != null) btnUpdate.Visible = false;

                string clonedDescription = BuildCloneDefaultDescription(sourceDescription);
                if (!string.IsNullOrWhiteSpace(clonedDescription) && txt_description != null)
                {
                    txt_description.Text = clonedDescription;
                }

                txt_barcode?.Focus();
                txt_barcode?.SelectAll();

                MessageBox.Show("Item copied into new-item mode. Enter a new barcode identity and save the variant.", "Clone Item", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while cloning item: " + ex.Message, "Clone Item", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void ClearClonedBarcodeIdentityValues(DataTable table, string columnName)
        {
            if (table == null || string.IsNullOrWhiteSpace(columnName) || !table.Columns.Contains(columnName))
            {
                return;
            }

            foreach (DataRow row in table.Rows)
            {
                if (row == null || row.RowState == DataRowState.Deleted)
                {
                    continue;
                }

                row[columnName] = string.Empty;
            }
        }

        private static DataTable CloneDataTable(DataTable source)
        {
            if (source == null)
            {
                return null;
            }

            return source.Copy();
        }

        private string BuildCloneDefaultDescription(string sourceDescription)
        {
            string baseDescription = (sourceDescription ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(baseDescription))
            {
                baseDescription = "Item";
            }

            string cloneSuffix = $" - V{(txt_ItemNo?.Text ?? string.Empty).Trim()}";
            if (string.IsNullOrWhiteSpace(cloneSuffix.Replace("-", string.Empty).Replace("V", string.Empty)))
            {
                cloneSuffix = " - VAR";
            }

            const int maxDescriptionLength = 50;
            int allowedBaseLength = Math.Max(1, maxDescriptionLength - cloneSuffix.Length);
            if (baseDescription.Length > allowedBaseLength)
            {
                baseDescription = baseDescription.Substring(0, allowedBaseLength).TrimEnd();
            }

            return (baseDescription + cloneSuffix).Trim();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Open the Unit Cost calculator and keep a reference for live syncing
            if (unitCostCalculator == null || unitCostCalculator.IsDisposed)
            {
                unitCostCalculator = new calculate_unit_cost_base_on_selling_price_and_mark_up();
                unitCostCalculator.FormClosed += (s, args) =>
                {
                    unitCostCalculator = null;
                    if (unitCostSyncTimer != null)
                    {
                        unitCostSyncTimer.Stop();
                        unitCostSyncTimer.Dispose();
                        unitCostSyncTimer = null;
                    }
                };
            }

            // Pass the current unit cost value to the calculator form
            if (!string.IsNullOrEmpty(Txt_UnitCost.Text))
            {
                unitCostCalculator.SetUnitCost(Txt_UnitCost.Text);
            }

            // Get margin percentage from Ult_Price grid if available
            string marginPercentage = "0.00";
            Infragistics.Win.UltraWinGrid.UltraGrid Ult_Price =
                this.Controls.Find("Ult_Price", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;

            if (Ult_Price != null && Ult_Price.Rows.Count > 0)
            {
                var firstRow = Ult_Price.Rows[0];
                if (firstRow.Cells["MarginPer"].Value != null)
                {
                    marginPercentage = firstRow.Cells["MarginPer"].Value.ToString();
                }
            }

            // Pass the RETAIL PRICE (not walking price) to the calculator
            string retailPrice = (txt_Retail != null && !string.IsNullOrEmpty(txt_Retail.Text)) ? txt_Retail.Text : "0.000";
            string markup = textBox1.Text; // Assuming textBox1 is used for markup %
            string unitCost = Txt_UnitCost.Text;

            // IMPORTANT: Only set last values if we don't have a valid markup from textBox1
            // This prevents overwriting user-entered markup values
            if (string.IsNullOrWhiteSpace(markup) || markup == "0" || markup == "0.000" || markup == "0.00")
            {
                // No saved markup, use default values
                unitCostCalculator.SetLastValues(retailPrice, marginPercentage, unitCost);
                // Set margin percentage from Ult_Price to calculator's textBox2
                unitCostCalculator.SetMarginPercentage(marginPercentage);
            }
            else
            {
                // We have a saved markup, preserve it and don't overwrite
                unitCostCalculator.SetLastValues(retailPrice, markup, unitCost);
                // Set the saved markup percentage to maintain user's input
                unitCostCalculator.SetMarginPercentage(markup);
            }

            // IMPORTANT: Restore user's previously entered markup AFTER setting other values
            // This ensures the user's markup takes precedence over any default values
            unitCostCalculator.RestoreUserMarkup();

            // Set the selling price that will be applied to multiple fields
            // Use the current retail price as the base selling price
            unitCostCalculator.SetSellingPriceForMultipleFields(retailPrice);

            unitCostCalculator.StartPosition = FormStartPosition.CenterScreen;

            // Start periodic sync from calculator (while open) to reflect markup changes
            if (unitCostSyncTimer == null)
            {
                unitCostSyncTimer = new System.Windows.Forms.Timer();
                unitCostSyncTimer.Interval = 200;
                unitCostSyncTimer.Tick += (ts, te) =>
                {
                    try
                    {
                        if (unitCostCalculator != null && !unitCostCalculator.IsDisposed && unitCostCalculator.Visible)
                        {
                            // Mirror markup % from calculator's textbox2
                            string mk = unitCostCalculator.MarkupPercentage;
                            if (textBox1 != null && !string.IsNullOrWhiteSpace(mk) && textBox1.Text != mk)
                            {
                                isUpdatingMarkup = true;
                                textBox1.Text = mk;
                                isUpdatingMarkup = false;
                            }
                        }
                    }
                    catch { }
                };
            }
            unitCostSyncTimer.Start();

            // Capture local reference to avoid FormClosed event nulling the field before access
            var calc = unitCostCalculator;
            if (calc != null && calc.ShowDialog() == DialogResult.OK)
            {
                // Save textBox1 (Unit Cost) to Txt_UnitCost
                if (!string.IsNullOrEmpty(calc.CalculatedUnitCost))
                {
                    Txt_UnitCost.Text = calc.CalculatedUnitCost;
                }

                // Save textBox2 (Markup %) to textBox1
                if (!string.IsNullOrEmpty(calc.MarkupPercentage))
                {
                    textBox1.Text = calc.MarkupPercentage;
                }

                // Apply the selling price from calculator to multiple price fields
                if (!string.IsNullOrEmpty(calc.SellingPriceForMultipleFields))
                {
                    string sellingPrice = calc.SellingPriceForMultipleFields;

                    try
                    {
                        // Apply master retail
                        if (txt_Retail != null) txt_Retail.Text = sellingPrice;

                        // Apply prices to linked fields considering their current markdown values
                        // This will respect negative markdown (markup) values in the markdown editors
                        ApplyMasterPricesWithMarkdownRespect(sellingPrice);

                        // Check if txt_SF exists and apply to it as well
                        var txt_SF = this.Controls.Find("txt_SF", true).FirstOrDefault() as TextBox;
                        if (txt_SF != null && !isEditingStaffPrice) txt_SF.Text = sellingPrice;
                        if (txt_SF != null && !isEditingStaffPrice) txt_SF.Text = sellingPrice;

                        // Refresh all unit prices after updating the base prices
                        RefreshAllUnitPrices();

                        // Update all profit margins after applying selling price
                        UpdateAllProfitMargins();

                        System.Diagnostics.Debug.WriteLine($"Successfully applied selling price {sellingPrice} to all price fields");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error applying selling price to price fields: {ex.Message}");
                        // Continue with the operation even if some fields fail
                    }
                }

                // Show confirmation message
                MessageBox.Show("Values saved successfully!\n\nUnit Cost: " + calc.CalculatedUnitCost + "\nMarkup %: " + calc.MarkupPercentage + "\nSelling Price applied to all price fields: " + calc.SellingPriceForMultipleFields,
                    "Calculator Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (isUpdatingMarkup)
                    return;

                // Push markup change to calculator if it is open
                if (unitCostCalculator != null && !unitCostCalculator.IsDisposed && unitCostCalculator.Visible)
                {
                    unitCostCalculator.SetMarginPercentage(textBox1.Text);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in textBox1_TextChanged: {ex.Message}");
            }
        }

        // Handle Enter on markup textbox to compute selling price, apply markdown and update profit margins
        // Now properly handles negative markdown values when applying prices
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            try
            {
                // Parse unit cost and markup %
                double unitCost = 0;
                double markupPercent = 0;
                double.TryParse(Txt_UnitCost.Text ?? "0", out unitCost);
                double.TryParse(textBox1.Text ?? "0", out markupPercent);

                if (unitCost <= 0)
                {
                    e.Handled = true; e.SuppressKeyPress = true; return;
                }

                // selling price from markup: SP = UC * (1 + markup/100)
                double sellingPrice = unitCost * (1.0 + (markupPercent / 100.0));
                string sp = sellingPrice.ToString("0.000");

                // Apply master retail (txt_Retail) using this selling price
                if (txt_Retail != null) txt_Retail.Text = sp;

                // Apply prices to linked fields considering their current markdown values
                // This will respect negative markdown (markup) values in the markdown editors
                ApplyMasterPricesWithMarkdownRespect(sp);

                // Optional: propagate to txt_SF if present
                var txt_SF = this.Controls.Find("txt_SF", true).FirstOrDefault() as TextBox;
                if (txt_SF != null && !isEditingStaffPrice) txt_SF.Text = sp;

                // Refresh price grid and profit margins
                RefreshAllUnitPrices();
                UpdateAllProfitMargins(); // safe; won't alter textBox1

                // Recompute tax display to keep txt_TaxAmount in sync with new selling price
                RecomputeTaxAmountFromRetailAndTax();

                // Also push markup to calculator if open
                if (unitCostCalculator != null && !unitCostCalculator.IsDisposed && unitCostCalculator.Visible)
                {
                    unitCostCalculator.SetMarginPercentage(textBox1.Text);
                }

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in textBox1_KeyDown: {ex.Message}");
            }
        }

        // When user edits profit margin (as a percentage of selling price),
        // update all margin fields, recompute selling prices, and update markup.
        // NOTE: Only apply effects when triggered by Enter key via isProcessingProfitMarginEnter flag.
        // Now properly handles negative markdown values when calculating selling prices.
        private void ultraTextEditor4_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Always mirror master margin % into Ult_Price.MarginPer cells
                SyncUltPriceMarginPerFromMaster();

                // Apply changes only when Enter was pressed
                if (!isProcessingProfitMarginEnter)
                    return;

                if (isUpdatingProfitMargins || isLoadingItem)
                    return;

                double marginPercent;
                if (!double.TryParse(ultraTextEditor4.Text, out marginPercent))
                    return;

                // Validate range 0..100. If >100, reset ALL related fields at once and stop.
                if (marginPercent > 100)
                {
                    isUpdatingProfitMargins = true;
                    // Reset profit margin fields
                    ultraTextEditor4.Text = "0.00";
                    if (ultraTextEditor10 != null) ultraTextEditor10.Text = "0.00";
                    if (ultraTextEditor9 != null) ultraTextEditor9.Text = "0.00";
                    if (ultraTextEditor8 != null) ultraTextEditor8.Text = "0.00";
                    if (ultraTextEditor7 != null) ultraTextEditor7.Text = "0.00";

                    // Reset selling price fields
                    if (txt_Retail != null) txt_Retail.Text = "0.000";
                    if (txt_walkin != null) txt_walkin.Text = "0.000";
                    if (txt_CEP != null) txt_CEP.Text = "0.000";
                    if (txt_Mrp != null) txt_Mrp.Text = "0.000";
                    if (txt_CardP != null) txt_CardP.Text = "0.000";

                    // Reset markup field
                    if (textBox1 != null)
                    {
                        isUpdatingMarkup = true;
                        textBox1.Text = "0.00";
                        isUpdatingMarkup = false;
                    }

                    isUpdatingProfitMargins = false;

                    // Keep UI consistent
                    RefreshAllUnitPrices();
                    UpdateAllProfitMargins();

                    return;
                }

                // Clamp negatives to 0, and max 100 handled above
                if (marginPercent < 0) marginPercent = 0;
                if (marginPercent == 100) marginPercent = 99.999; // avoid div by zero

                isUpdatingProfitMargins = true;

                // Mirror margin to sibling fields
                if (ultraTextEditor10 != null) ultraTextEditor10.Text = marginPercent.ToString("0.00");
                if (ultraTextEditor9 != null) ultraTextEditor9.Text = marginPercent.ToString("0.00");
                if (ultraTextEditor8 != null) ultraTextEditor8.Text = marginPercent.ToString("0.00");
                if (ultraTextEditor7 != null) ultraTextEditor7.Text = marginPercent.ToString("0.00");

                // Recalculate selling prices from margin % and unit cost
                double unitCost = 0;
                double.TryParse(Txt_UnitCost.Text, out unitCost);
                if (unitCost > 0)
                {
                    double sellingPrice = unitCost / (1.0 - (marginPercent / 100.0));
                    string sp = sellingPrice.ToString("0.000");

                    if (txt_Retail != null) txt_Retail.Text = sp;

                    // Apply prices to linked fields considering their current markdown values
                    // This will respect negative markdown (markup) values in the markdown editors
                    ApplyMasterPricesWithMarkdownRespect(sp);

                    // Update markup %
                    if (textBox1 != null && !isUpdatingMarkup)
                    {
                        double markupPercent = (sellingPrice / unitCost - 1.0) * 100.0;
                        isUpdatingMarkup = true;
                        textBox1.Text = markupPercent.ToString("0.00");
                        isUpdatingMarkup = false;
                    }

                    // Keep grids and dependent margins in sync
                    RefreshAllUnitPrices();
                    UpdateAllProfitMargins();

                    // Recompute tax display to keep txt_TaxAmount in sync with new selling price
                    RecomputeTaxAmountFromRetailAndTax();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ultraTextEditor4_TextChanged: {ex.Message}");
            }
            finally
            {
                isUpdatingProfitMargins = false;
                isProcessingProfitMarginEnter = false;
            }
        }

        private void ultraTextEditor4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // mark that TextChanged effects should apply
                isProcessingProfitMarginEnter = true;
                // Re-assign current text to trigger TextChanged if needed
                // Some controls may not fire TextChanged on Enter if text didn't change;
                // force processing by calling the handler explicitly.
                ultraTextEditor4_TextChanged(sender, EventArgs.Empty);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            this.UpdateItem();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (txt_ItemNo.Text != "")
            {

            }
            else
            {
                MessageBox.Show("Please Select an Item");
            }
        }

        private bool isF7DialogOpen = false;
        private bool isF8SaveInProgress = false;
        private DateTime lastF7PressTime = DateTime.MinValue;
        private DateTime lastF8PressTime = DateTime.MinValue;

        private void frmItemMasterNew_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                // Clear everything when F1 is pressed
                this.clear();
                var txtBarcodeCtrl = GetMainBarcodeEditor();
                txtBarcodeCtrl?.Focus();
            }
            else if (e.KeyCode == Keys.F7)
            {
                if (isF7DialogOpen)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }

                // Prevent auto-repeat or double-press within 1000ms
                if ((DateTime.Now - lastF7PressTime).TotalMilliseconds < 1000)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }

                try
                {
                    isF7DialogOpen = true;
                    // Set these BEFORE opening the dialog to block queued events when it returns
                    lastF7PressTime = DateTime.Now;
                    e.Handled = true;
                    e.SuppressKeyPress = true;

                    string Params = "FromItemMaster";
                    frmdialForItemMaster item = new frmdialForItemMaster(Params);
                    item.ShowDialog();
                }
                finally
                {
                    isF7DialogOpen = false;
                    // Reset time AFTER dialog closes to prevent any queued F7 events from firing immediately
                    lastF7PressTime = DateTime.Now;
                }
            }
            else if (e.KeyCode == Keys.F8)
            {
                if (isF8SaveInProgress)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }

                // Prevent auto-repeat or double-press within 1000ms from firing a second save/validation
                if ((DateTime.Now - lastF8PressTime).TotalMilliseconds < 1000)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }

                try
                {
                    isF8SaveInProgress = true;
                    // Pre-block queued events during the long save operation
                    lastF8PressTime = DateTime.Now;
                    e.Handled = true;
                    e.SuppressKeyPress = true;

                    // Save or Update based on whether item exists
                    if (!string.IsNullOrWhiteSpace(txt_ItemNo.Text) && CurrentItemId > 0)
                    {
                        // Item exists - Update
                        this.UpdateItem();
                    }
                    else
                    {
                        // New item - Save
                        this.SaveMaster();
                    }
                }
                finally
                {
                    isF8SaveInProgress = false;
                    // Reset time AFTER save finishes to prevent queued F8 events from firing immediately
                    lastF8PressTime = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Public method to set the DataSource of ultraGrid1
        /// </summary>
        /// <param name="dataSource">The DataSource to set for ultraGrid1</param>
        public void SetUltraGridDataSource(object dataSource)
        {
            if (ultraGrid1 != null)
            {
                if (dataSource is DataTable dt)
                {
                    EnsureUomGridPriceColumns(dt);
                    ultraGrid1.DataSource = dataSource;
                    foreach (DataRow row in dt.Rows)
                    {
                        SyncUomRowWithPriceGrid(row);
                    }
                    if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                    {
                        EnsureRetailPriceMrpDisplayOrder(ultraGrid1.DisplayLayout.Bands[0]);
                    }
                    ultraGrid1.Refresh();

                    // Hide specified columns after setting data source
                    HideUltraGrid1Columns();
                }
                else
                {
                    ultraGrid1.DataSource = dataSource;
                }
            }
        }

        // Helper method to hide specified columns in ultraGrid1
        private void HideUltraGrid1Columns()
        {
            if (ultraGrid1 != null && ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(colReorder))
                    ultraGrid1.DisplayLayout.Bands[0].Columns[colReorder].Hidden = true;
                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(colOpenStock))
                    ultraGrid1.DisplayLayout.Bands[0].Columns[colOpenStock].Hidden = true;
                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("MarginAmt"))
                    ultraGrid1.DisplayLayout.Bands[0].Columns["MarginAmt"].Hidden = true;
                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("MarginPer"))
                    ultraGrid1.DisplayLayout.Bands[0].Columns["MarginPer"].Hidden = true;
                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("TaxPer"))
                    ultraGrid1.DisplayLayout.Bands[0].Columns["TaxPer"].Hidden = true;
                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("TaxAmt"))
                    ultraGrid1.DisplayLayout.Bands[0].Columns["TaxAmt"].Hidden = true;

                // Make Cost column read-only — it is auto-calculated (Packing × Txt_UnitCost)
                if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists("Cost"))
                {
                    ultraGrid1.DisplayLayout.Bands[0].Columns["Cost"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                }

                EnsureRetailPriceMrpDisplayOrder(ultraGrid1.DisplayLayout.Bands[0]);
            }
        }

        // Public method to set the base unit text
        public void SetBaseUnitText(string unitName)
        {
            if (this.txt_BaseUnit != null && !string.IsNullOrEmpty(unitName))
            {
                this.txt_BaseUnit.Text = unitName;

                // After setting the base unit, synchronize with the grid
                // Use BeginInvoke to ensure the UI is updated first
                this.BeginInvoke(new Action(() =>
                {
                    SynchronizeBaseUnitWithGrid();
                }));
            }
        }

        // Method to set current item ID for hold details
        public void SetCurrentItemId(int itemId)
        {
            CurrentItemId = itemId;
            LoadItemStatusForItemId(itemId);
        }

        // Method to set the loading flag to prevent synchronization during loading
        public void SetLoadingFlag(bool loading)
        {
            isLoadingItem = loading;

            // Re-apply 2-decimal formatting right at the end of the item load cycle.
            // This safely forces .00 onto any Mark Down or Profit Margin editors that lost their decimals
            // due to binding syncs or hidden numeric casting in the background.
            if (!loading)
            {
                try
                {
                    Infragistics.Win.UltraWinEditors.UltraTextEditor[] allPercentageEditors = new Infragistics.Win.UltraWinEditors.UltraTextEditor[]
                    {
                        ultraTextEditor4, ultraTextEditor5, ultraTextEditor6, ultraTextEditor7, ultraTextEditor8, ultraTextEditor9, ultraTextEditor10,
                        ultraTextEditor11, ultraTextEditor12, ultraTextEditor13, ultraTextEditor14, ultraTextEditor15, ultraTextEditor16
                    };

                    foreach (var editor in allPercentageEditors)
                    {
                        if (editor != null && !string.IsNullOrWhiteSpace(editor.Text))
                        {
                            if (double.TryParse(editor.Text, out double val))
                            {
                                editor.Text = val.ToString("0.00");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error re-formatting percentage editors at load finish: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Sets profit margin values for all price types
        /// </summary>
        /// <param name="retailMargin">Profit margin for retail price</param>
        /// <param name="walkingMargin">Profit margin for walking price</param>
        /// <param name="creditMargin">Profit margin for credit price</param>
        /// <param name="mrpMargin">Profit margin for MRP</param>
        /// <param name="cardMargin">Profit margin for card price</param>
        public void SetProfitMarginValues(double retailMargin, double walkingMargin, double creditMargin, double mrpMargin, double cardMargin)
        {
            try
            {
                // Set flag to prevent synchronization during loading
                isUpdatingProfitMargins = true;

                // Set profit margin values
                if (ultraTextEditor4 != null) ultraTextEditor4.Text = retailMargin.ToString("0.00");
                if (ultraTextEditor10 != null) ultraTextEditor10.Text = walkingMargin.ToString("0.00");
                if (ultraTextEditor9 != null) ultraTextEditor9.Text = creditMargin.ToString("0.00");
                if (ultraTextEditor8 != null) ultraTextEditor8.Text = mrpMargin.ToString("0.00");
                if (ultraTextEditor7 != null) ultraTextEditor7.Text = cardMargin.ToString("0.00");

                System.Diagnostics.Debug.WriteLine($"Set profit margins - Retail: {retailMargin}, Walking: {walkingMargin}, Credit: {creditMargin}, MRP: {mrpMargin}, Card: {cardMargin}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting profit margin values: {ex.Message}");
            }
            finally
            {
                isUpdatingProfitMargins = false;
            }
        }




        // Method to update hold quantity from hold details
        public void UpdateHoldQuantityFromHoldDetails()
        {
            try
            {
                if (CurrentItemId > 0)
                {
                    // Get hold details from repository
                    ItemMasterRepository itemRepo = new ItemMasterRepository();
                    List<HoldItemDetails> holdDetails = itemRepo.GetHoldItemDetails(CurrentItemId);

                    // Calculate total hold quantity
                    double totalHoldQty = 0;
                    foreach (var detail in holdDetails)
                    {
                        totalHoldQty += detail.HoldQty;
                    }

                    // Update the txt_hold field
                    if (txt_hold != null)
                    {
                        txt_hold.Text = totalHoldQty.ToString("N2");
                    }

                    // Update txt_qty = Stock (from PriceSettings) + txt_hold (hold items)
                    UpdateQtyFromStockAndHold();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating hold quantity: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates txt_available = Stock (from PriceSettings, shown in txt_qty) - txt_hold (held items)
        /// </summary>
        private void UpdateQtyFromStockAndHold()
        {
            try
            {
                if (txt_available == null)
                    return;

                // Get stock from PriceSettings (txt_qty now contains the total stock value)
                float stock = 0;
                if (txt_qty != null && !string.IsNullOrEmpty(txt_qty.Text))
                {
                    float.TryParse(txt_qty.Text, out stock);
                }

                // Get hold quantity from txt_hold
                float holdQty = 0;
                if (txt_hold != null && !string.IsNullOrEmpty(txt_hold.Text))
                {
                    float.TryParse(txt_hold.Text, out holdQty);
                }

                // Calculate available: txt_available = Stock - txt_hold
                float availableQty = stock - holdQty;
                // Show quantity as whole number string (no decimals)
                txt_available.Text = availableQty.ToString("0");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating txt_qty from stock and hold: {ex.Message}");
            }
        }

        // Method to synchronize base unit selection with ultraGrid1
        private void SynchronizeBaseUnitWithGrid()
        {
            // Skip synchronization if currently loading an item to avoid clearing already-loaded units
            if (isLoadingItem)
                return;

            try
            {
                if (string.IsNullOrWhiteSpace(txt_BaseUnit.Text))
                {
                    // If no base unit is selected, clear the grid
                    ClearUomGrid();
                    return;
                }

                // Get the selected base unit information
                string selectedUnitName = txt_BaseUnit.Text.Trim();

                UnitMaster selectedUnit = GetUnitByNameFromStoredProcedure(selectedUnitName);
                if (selectedUnit != null)
                {
                            int unitId = selectedUnit.UnitID;
                            string unitName = selectedUnit.UnitName;
                            float packing = Convert.ToSingle(selectedUnit.Packing);

                            // Clear existing data in ultraGrid1
                            ClearUomGrid();

                            // Add the base unit as the first row with packing = 1
                            string currentBarcode = string.Empty;
                            try
                            {
                                var txtBarcodeCtrl = GetMainBarcodeEditor();
                                if (txtBarcodeCtrl != null) currentBarcode = txtBarcodeCtrl.Text ?? string.Empty;
                            }
                            catch { }

                            // Add base unit with packing = 1 (this is the BASE UNIT)
                            AddOrUpdateUomRow(unitName, unitId, 1.0f, 5,
                                string.IsNullOrWhiteSpace(currentBarcode) ? "0" : currentBarcode, 0);

                            // Update price grid to reflect the new base unit
                            UpdatePriceGridForBaseUnit(unitName, 1.0f);

                            // Ensure all other units in the grid are updated to maintain consistency
                            UpdateOtherUnitsInGrid(unitName);

                            System.Diagnostics.Debug.WriteLine($"Successfully synchronized base unit '{unitName}' with ultraGrid1");
                }

                // Hide specified columns after synchronizing
                HideUltraGrid1Columns();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error synchronizing base unit with grid: {ex.Message}");
                MessageBox.Show($"Error synchronizing base unit: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Method to ensure added units are consistent with base unit
        private void SynchronizeAddedUnitsWithBaseUnit()
        {
            // Skip synchronization if currently loading an item to avoid clearing already-loaded units
            if (isLoadingItem)
                return;

            try
            {
                if (string.IsNullOrWhiteSpace(txt_BaseUnit.Text))
                {
                    // If no base unit is selected, clear all units
                    ClearUomGrid();
                    return;
                }

                // Get the base unit name
                string baseUnitName = txt_BaseUnit.Text.Trim();

                // Check if the base unit exists in ultraGrid1
                DataTable dt = ultraGrid1.DataSource as DataTable;
                if (dt != null)
                {
                    bool baseUnitExists = false;
                    foreach (DataRow row in dt.Rows)
                    {
                        string unitName = row["Unit"]?.ToString() ?? string.Empty;
                        if (string.Equals(unitName, baseUnitName, StringComparison.OrdinalIgnoreCase))
                        {
                            baseUnitExists = true;
                            break;
                        }
                    }

                    // If base unit doesn't exist in grid, add it
                    if (!baseUnitExists)
                    {
                        UnitMaster baseUnit = GetUnitByNameFromStoredProcedure(baseUnitName);
                        if (baseUnit != null)
                        {
                                    int unitId = baseUnit.UnitID;
                                    string unitName = baseUnit.UnitName;
                                    float packing = Convert.ToSingle(baseUnit.Packing);

                                    // Add base unit with packing = 1
                                    string currentBarcode = string.Empty;
                                    try
                                    {
                                        var txtBarcodeCtrl = GetMainBarcodeEditor();
                                        if (txtBarcodeCtrl != null) currentBarcode = txtBarcodeCtrl.Text ?? string.Empty;
                                    }
                                    catch { }

                                    AddOrUpdateUomRow(unitName, unitId, 1.0f, 5,
                                        string.IsNullOrWhiteSpace(currentBarcode) ? "0" : currentBarcode, 0);

                                    // Update price grid
                                    UpdatePriceGridForBaseUnit(unitName, 1.0f);
                        }
                    }
                }

                // Ensure all units in the grid have proper pricing based on base unit
                RefreshAllUnitPrices();

                // Hide specified columns after synchronizing
                HideUltraGrid1Columns();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error synchronizing added units with base unit: {ex.Message}");
            }
        }

        // Helper method to clear the UOM grid
        private void ClearUomGrid()
        {
            try
            {
                if (ultraGrid1 != null)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add(colUnit, typeof(string));
                    dt.Columns.Add(colUnitId, typeof(string));
                    dt.Columns.Add(colPacking, typeof(string));
                    // dt.Columns.Add(colBarcode, typeof(string)); // Removed
                    dt.Columns.Add(colReorder, typeof(string));
                    dt.Columns.Add(colOpenStock, typeof(string));
                    ultraGrid1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing UOM grid: {ex.Message}");
            }
        }

        // Helper method to update price grid for base unit
        private void UpdatePriceGridForBaseUnit(string unitName, float packing)
        {
            try
            {
                Infragistics.Win.UltraWinGrid.UltraGrid Ult_Price =
                    this.Controls.Find("Ult_Price", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;

                if (Ult_Price != null)
                {
                    DataTable dtPrice = Ult_Price.DataSource as DataTable;
                    if (dtPrice != null)
                    {
                        // Clear existing price data
                        dtPrice.Rows.Clear();

                        // Get base prices from form
                        float baseCost = 0;
                        float.TryParse(Txt_UnitCost.Text, out baseCost);

                        float baseMRP = 0;
                        float.TryParse(txt_Mrp.Text, out baseMRP);

                        float baseRetailPrice = 0;
                        float.TryParse(txt_Retail.Text, out baseRetailPrice);

                        float baseWalkingPrice = 0;
                        float.TryParse(txt_walkin.Text, out baseWalkingPrice);

                        float baseCreditPrice = 0;
                        float.TryParse(txt_CEP.Text, out baseCreditPrice);

                        float baseCardPrice = 0;
                        float.TryParse(txt_CardP.Text, out baseCardPrice);

                        float taxPer = 0;
                        float.TryParse(txt_TaxPer.Text, out taxPer);

                        // Add base unit row to price grid
                        DataRow newRow = dtPrice.NewRow();
                        newRow["Unit"] = unitName;
                        newRow["Packing"] = Convert.ToInt32(packing);
                        newRow["Cost"] = baseCost * packing;
                        newRow["MRP"] = baseMRP * packing;
                        newRow["RetailPrice"] = baseRetailPrice * packing; // Visual "Retail Price"
                        newRow["WholeSalePrice"] = baseWalkingPrice * packing; // Visual "Walking Price"
                        newRow["CreditPrice"] = baseCreditPrice * packing;
                        newRow["CardPrice"] = baseCardPrice * packing;

                        // Calculate margin based on Retail (master selling price) and mirror margin % from txt_Retail
                        float marginAmount = (baseRetailPrice * packing) - (baseCost * packing);
                        double retailMarginPercent = 0;
                        double.TryParse(ultraTextEditor4 != null ? ultraTextEditor4.Text : "0", out retailMarginPercent);
                        float marginPercentage = (float)retailMarginPercent;

                        newRow["MarginAmt"] = marginAmount;
                        newRow["MarginPer"] = marginPercentage;
                        newRow["TaxPer"] = taxPer;
                        newRow["TaxAmt"] = (baseRetailPrice * packing) * (taxPer / 100);

                        dtPrice.Rows.Add(newRow);
                        Ult_Price.DataSource = dtPrice;
                        Ult_Price.Refresh();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating price grid for base unit: {ex.Message}");
            }
        }

        // Helper method to update other units in the grid to maintain consistency with base unit
        private void UpdateOtherUnitsInGrid(string baseUnitName)
        {
            try
            {
                DataTable dt = ultraGrid1.DataSource as DataTable;
                if (dt == null || dt.Rows.Count <= 1) return; // Need at least base unit + one other unit

                // Get base unit prices from form
                float baseCost = 0;
                float.TryParse(Txt_UnitCost.Text, out baseCost);

                float baseMRP = 0;
                float.TryParse(txt_Mrp.Text, out baseMRP);

                float baseRetailPrice = 0;
                float.TryParse(txt_Retail.Text, out baseRetailPrice);

                float baseWalkingPrice = 0;
                float.TryParse(txt_walkin.Text, out baseWalkingPrice);

                float baseCreditPrice = 0;
                float.TryParse(txt_CEP.Text, out baseCreditPrice);

                float baseCardPrice = 0;
                float.TryParse(txt_CardP.Text, out baseCardPrice);

                float taxPer = 0;
                float.TryParse(txt_TaxPer.Text, out taxPer);

                // Update price grid for all units
                Infragistics.Win.UltraWinGrid.UltraGrid Ult_Price =
                    this.Controls.Find("Ult_Price", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;

                if (Ult_Price != null)
                {
                    DataTable dtPrice = Ult_Price.DataSource as DataTable;
                    if (dtPrice != null)
                    {
                        // Clear existing price data
                        dtPrice.Rows.Clear();

                        // Add rows for all units in ultraGrid1
                        foreach (DataRow uomRow in dt.Rows)
                        {
                            string unitName = uomRow["Unit"]?.ToString() ?? string.Empty;
                            float packing = 0;
                            float.TryParse(uomRow["Packing"]?.ToString(), out packing);

                            if (packing > 0)
                            {
                                DataRow priceRow = dtPrice.NewRow();
                                priceRow["Unit"] = unitName;
                                priceRow["Packing"] = Convert.ToInt32(packing);
                                priceRow["Cost"] = baseCost * packing;
                                priceRow["MRP"] = baseMRP * packing;
                                priceRow["RetailPrice"] = baseRetailPrice * packing; // Visual "Retail Price"
                                priceRow["WholeSalePrice"] = baseWalkingPrice * packing; // Visual "Walking Price"
                                priceRow["CreditPrice"] = baseCreditPrice * packing;
                                priceRow["CardPrice"] = baseCardPrice * packing;

                                // Calculate margin amount based on Retail; margin % mirrors Retail profit margin
                                float marginAmount = (baseRetailPrice * packing) - (baseCost * packing);
                                double retailMarginPercent = 0;
                                double.TryParse(ultraTextEditor4 != null ? ultraTextEditor4.Text : "0", out retailMarginPercent);
                                float marginPercentage = (float)retailMarginPercent;

                                priceRow["MarginAmt"] = marginAmount;
                                priceRow["MarginPer"] = marginPercentage;
                                priceRow["TaxPer"] = taxPer;
                                priceRow["TaxAmt"] = (float)ComputeTaxAmountForGridRow(baseRetailPrice * packing, taxPer);

                                dtPrice.Rows.Add(priceRow);
                            }
                        }

                        Ult_Price.DataSource = dtPrice;
                        Ult_Price.Refresh();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating other units in grid: {ex.Message}");
            }
        }

        // Method to ensure barcode is synchronized before saving
        // Helper method to synchronize TaxPer and TaxType from text fields to price grid before save
        private void SynchronizeTaxFieldsToPriceGrid()
        {
            try
            {
                // Find Ult_Price control
                Infragistics.Win.UltraWinGrid.UltraGrid Ult_Price =
                    this.Controls.Find("Ult_Price", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;

                if (Ult_Price != null && Ult_Price.Rows.Count > 0)
                {
                    // Get TaxPer from txt_TaxPer field
                    float taxPer = 0f;
                    if (txt_TaxPer != null && !string.IsNullOrEmpty(txt_TaxPer.Text))
                    {
                        float.TryParse(txt_TaxPer.Text, out taxPer);
                    }

                    // Get TaxType from txt_TaxType or comboTaxType
                    string taxType = string.Empty;
                    if (comboTaxType != null && comboTaxType.SelectedItem != null)
                    {
                        taxType = comboTaxType.SelectedItem.ToString();
                    }
                    else if (txt_TaxType != null && !string.IsNullOrEmpty(txt_TaxType.Text))
                    {
                        taxType = txt_TaxType.Text;
                    }

                    // Update all rows in the price grid with the current TaxPer and TaxType
                    foreach (Infragistics.Win.UltraWinGrid.UltraGridRow row in Ult_Price.Rows)
                    {
                        if (row.Cells.Exists("TaxPer"))
                        {
                            row.Cells["TaxPer"].Value = taxPer;
                        }
                        if (row.Cells.Exists("TaxType"))
                        {
                            row.Cells["TaxType"].Value = taxType;
                        }
                    }

                    // Recalculate tax amounts based on the updated TaxPer
                    if (taxPer > 0)
                    {
                        RecalculatePriceGrid();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error synchronizing tax fields to price grid: {ex.Message}");
            }
        }

        private void SynchronizeBarcodeBeforeSave()
        {
            // Method emptied as BarCode column is removed
        }

        // Method to ensure barcode consistency after loading an item (base unit mirrors textbox; others remain unique)
        private void SynchronizeBarcodeAfterLoad()
        {
            /* try
            {
                // Get the barcode from the text field
                var txtBarcodeCtrl = GetMainBarcodeEditor();
                string barcodeFromField = txtBarcodeCtrl != null ? (txtBarcodeCtrl.Text ?? string.Empty) : string.Empty;

                // If the text field has a barcode, ensure base unit row has same barcode
                if (!string.IsNullOrWhiteSpace(barcodeFromField))
                {
                    if (ultraGrid1 != null && ultraGrid1.DataSource != null)
                    {
                        DataTable dt = ultraGrid1.DataSource as DataTable;
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                string unitName = Convert.ToString(row[colUnit] ?? "");
                                string packingText = Convert.ToString(row[colPacking] ?? "");
                                float pk = 0; float.TryParse(packingText, out pk);
                                bool isBaseUnitRow = pk == 1.0f || (!string.IsNullOrWhiteSpace(txt_BaseUnit?.Text) &&
                                    string.Equals(unitName, txt_BaseUnit.Text.Trim(), StringComparison.OrdinalIgnoreCase));
                                if (isBaseUnitRow)
                                {
                                    row[colBarcode] = barcodeFromField;
                                    break;
                                }
                            }
                            ultraGrid1.Refresh();
                        }
                    }
                }
                // If the text field is empty but grid has barcode in base unit row, sync from that row to text field
                else
                {
                    if (ultraGrid1 != null && ultraGrid1.DataSource != null)
                    {
                        DataTable dt = ultraGrid1.DataSource as DataTable;
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                string unitName = Convert.ToString(row[colUnit] ?? "");
                                string packingText = Convert.ToString(row[colPacking] ?? "");
                                float pk = 0; float.TryParse(packingText, out pk);
                                bool isBaseUnitRow = pk == 1.0f || (!string.IsNullOrWhiteSpace(txt_BaseUnit?.Text) &&
                                    string.Equals(unitName, txt_BaseUnit.Text.Trim(), StringComparison.OrdinalIgnoreCase));
                                if (isBaseUnitRow)
                                {
                                    string barcodeFromGrid = row[colBarcode]?.ToString() ?? string.Empty;
                                    if (!string.IsNullOrWhiteSpace(barcodeFromGrid) && txtBarcodeCtrl != null)
                                    {
                                        txtBarcodeCtrl.Text = barcodeFromGrid;
                                        System.Diagnostics.Debug.WriteLine($"Synced barcode from base unit row to text field: {barcodeFromGrid}");
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Barcode synchronized after load: {barcodeFromField}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error synchronizing barcode after load: {ex.Message}");
            } */
        }

        // Helper: fetch HSNCode directly via _POS_ItemMaster stored procedure
        private string GetHSNCodeFromStoredProcedure(int itemId)
        {
            try
            {
                BaseRepostitory con = new BaseRepostitory();
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_ItemMaster, (SqlConnection)con.DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    // Try a common GETBYID operation first
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");
                    cmd.Parameters.AddWithValue("@ItemId", itemId);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        da.Fill(ds);
                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                        {
                            DataTable dt = ds.Tables[0];
                            if (dt.Columns.Contains("HSNCode"))
                            {
                                object val = dt.Rows[0]["HSNCode"];
                                return val == DBNull.Value ? string.Empty : Convert.ToString(val);
                            }
                        }
                    }

                    // Fallback: try GETITEM if GETBYID not supported
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@_Operation", "GETITEM");
                    cmd.Parameters.AddWithValue("@ItemId", itemId);
                    using (SqlDataAdapter da2 = new SqlDataAdapter(cmd))
                    {
                        DataSet ds2 = new DataSet();
                        da2.Fill(ds2);
                        if (ds2 != null && ds2.Tables.Count > 0 && ds2.Tables[0] != null && ds2.Tables[0].Rows.Count > 0)
                        {
                            DataTable dt2 = ds2.Tables[0];
                            if (dt2.Columns.Contains("HSNCode"))
                            {
                                object val = dt2.Rows[0]["HSNCode"];
                                return val == DBNull.Value ? string.Empty : Convert.ToString(val);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching HSNCode via _POS_ItemMaster: {ex.Message}");
            }
            return string.Empty;
        }

        // Add method to connect Ult_Price CellChange event
        private void ConnectUltPriceEvents()
        {
            // Find Ult_Price control
            Infragistics.Win.UltraWinGrid.UltraGrid Ult_Price =
                this.Controls.Find("Ult_Price", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;

            if (Ult_Price != null)
            {
                // Connect the cell change event
                Ult_Price.AfterCellUpdate += Ult_Price_CellChange;
            }
        }

        // Guard flag to suppress Ult_Price_CellChange during textbox typing
        private bool _isUpdatingPriceFromTextbox = false;

        // Event handler for txt_walkin value changed
        private void txt_walkin_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                _isUpdatingPriceFromTextbox = true;
                // Update WholeSalePrice in Ult_Price for visual feedback (caption "Walking Price").
                // On Leave, RefreshAllUnitPrices rebuilds with correct DB mapping.
                Infragistics.Win.UltraWinGrid.UltraGrid Ult_Price =
                    this.Controls.Find("Ult_Price", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;

                if (Ult_Price != null && Ult_Price.Rows.Count > 0)
                {
                    float newWalking;
                    if (float.TryParse(txt_walkin.Text, out newWalking))
                    {
                        if (Ult_Price.Rows.Count > 0)
                        {
                            var row = Ult_Price.Rows[0];
                            int packing = 1;
                            try { packing = Convert.ToInt32(row.Cells["Packing"].Value); } catch { }
                            row.Cells["WholeSalePrice"].Value = newWalking * packing;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating walking price: {ex.Message}");
            }
            finally
            {
                _isUpdatingPriceFromTextbox = false;
            }
        }

        // Event handler for txt_Retail value changed
        private void txt_Retail_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                _isUpdatingPriceFromTextbox = true;
                // Only update the RetailPrice column in Ult_Price for the base unit row.
                // All heavy recalculations (RefreshAllUnitPrices, UpdateAllProfitMargins,
                // RecalculateMarkupPercentage, UpdateInclusiveExclusiveTaxDisplay, etc.)
                // are deferred to Enter-key (txt_Retail_KeyDown) or Leave to avoid cascading
                // UI writes on every keystroke which steal focus and reset the caret.
                Infragistics.Win.UltraWinGrid.UltraGrid Ult_Price =
                    this.Controls.Find("Ult_Price", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;

                if (Ult_Price != null && Ult_Price.Rows.Count > 0)
                {
                    float newRetail;
                    if (float.TryParse(txt_Retail.Text, out newRetail))
                    {
                        var row = Ult_Price.Rows[0];
                        int packing = 1;
                        try { packing = Convert.ToInt32(row.Cells["Packing"].Value); } catch { }
                        // txt_Retail updates RetailPrice for visual feedback (caption "Retail Price")
                        // On Leave, RefreshAllUnitPrices rebuilds with correct DB mapping.
                        if (row.Cells.Exists("RetailPrice")) row.Cells["RetailPrice"].Value = newRetail * packing;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating retail price: {ex.Message}");
            }
            finally
            {
                _isUpdatingPriceFromTextbox = false;
            }
        }

        // Event handler for txt_Retail KeyDown - Master field behavior
        private void txt_Retail_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    // Skip if we're currently loading an item
                    if (isLoadingItem)
                    {
                        System.Diagnostics.Debug.WriteLine("Skipping master field behavior during item loading");
                        return;
                    }

                    // Get the new retail price value
                    string retailPrice = txt_Retail.Text;

                    if (!string.IsNullOrWhiteSpace(retailPrice))
                    {
                        // Apply prices to linked fields considering their current markdown values
                        // This will respect negative markdown (markup) values in the markdown editors
                        ApplyMasterPricesWithMarkdownRespect(retailPrice);

                        // Check if txt_SF exists and apply to it as well
                        var txt_SF = this.Controls.Find("txt_SF", true).FirstOrDefault() as TextBox;
                        if (txt_SF != null) txt_SF.Text = retailPrice;

                        // Do not auto-recalculate markup while applying retail change

                        // Update markup in calculator if it's open
                        UpdateCalculatorMarkup();

                        // Refresh all unit prices after updating the base prices
                        RefreshAllUnitPrices();

                        // Update all profit margins after price synchronization
                        UpdateAllProfitMargins();

                        // Recompute tax display to keep txt_TaxAmount in sync with user-entered price
                        RecomputeTaxAmountFromRetailAndTax();

                        // Notify other forms of real-time change
                        NotifyItemMasterChanged();

                        System.Diagnostics.Debug.WriteLine($"Successfully synchronized retail price {retailPrice} to all other price fields with markdown respect");

                        // Visual feedback - briefly change background color
                        Color originalColor = txt_Retail.BackColor;
                        txt_Retail.BackColor = Color.LightGreen;

                        // Use a timer to restore the original color
                        Timer timer = new Timer();
                        timer.Interval = 200; // 200ms
                        timer.Tick += (timerSender, timerArgs) =>
                        {
                            txt_Retail.BackColor = originalColor;
                            timer.Stop();
                            timer.Dispose();
                        };
                        timer.Start();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in txt_Retail_KeyDown: {ex.Message}");
                }

                e.Handled = true;
                e.SuppressKeyPress = true;

                // Format all price fields to .000 immediately without waiting for Leave event
                var priceFieldsToFormat = new Control[] { txt_Retail, txt_walkin, txt_CEP, txt_Mrp, txt_CardP, txt_MinP };
                foreach (var pField in priceFieldsToFormat)
                {
                    if (pField != null) FormatPriceToThreeDecimals(pField, EventArgs.Empty);
                }
                var txt_SF_Control = this.Controls.Find("txt_SF", true).FirstOrDefault();
                if (txt_SF_Control != null) FormatPriceToThreeDecimals(txt_SF_Control, EventArgs.Empty);

                // Focus deliberately remains on txt_Retail as requested, and text is selected for rapid overwriting
                txt_Retail.SelectAll();
            }
        }

        // Event handler for txt_CEP value changed
        private void txt_CEP_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                // Only directly update CreditPrice in the price grid; all heavy recalculations
                // (RefreshAllUnitPrices, profit margin, markdown, NotifyItemMasterChanged)
                // are deferred to the Leave event to avoid stealing focus on every keystroke.
                Infragistics.Win.UltraWinGrid.UltraGrid Ult_Price =
                    this.Controls.Find("Ult_Price", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;

                if (Ult_Price != null && Ult_Price.Rows.Count > 0 && txt_CEP.Text.Trim() != "")
                {
                    float creditPrice = 0;
                    if (float.TryParse(txt_CEP.Text, out creditPrice))
                    {
                        foreach (Infragistics.Win.UltraWinGrid.UltraGridRow row in Ult_Price.Rows)
                        {
                            row.Cells["CreditPrice"].Value = creditPrice;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating credit price: {ex.Message}");
            }
        }

        // Event handler for txt_Mrp value changed
        private void txt_Mrp_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                // Minimal update - defer heavy recalculations to Leave event
                // to avoid stealing focus on every keystroke.
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating MRP: {ex.Message}");
            }
        }

        // Event handler for txt_CardP value changed
        private void txt_CardP_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                // Only directly update CardPrice in the price grid; all heavy recalculations
                // (RefreshAllUnitPrices, profit margin, markdown, NotifyItemMasterChanged)
                // are deferred to the Leave event to avoid stealing focus on every keystroke.
                Infragistics.Win.UltraWinGrid.UltraGrid Ult_Price =
                    this.Controls.Find("Ult_Price", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;

                if (Ult_Price != null && Ult_Price.Rows.Count > 0 && txt_CardP.Text.Trim() != "")
                {
                    float cardPrice = 0;
                    if (float.TryParse(txt_CardP.Text, out cardPrice))
                    {
                        foreach (Infragistics.Win.UltraWinGrid.UltraGridRow row in Ult_Price.Rows)
                        {
                            row.Cells["CardPrice"].Value = cardPrice;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating card price: {ex.Message}");
            }
        }

        // Common handler: when user presses Enter in a non-master selling price field,
        // compute its markdown relative to the master retail price and sync grids/margins
        private void SellingPriceField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            try
            {
                Control priceField = sender as Control;
                if (priceField == null) return;

                // Determine the matching markdown editor
                var markdownEditor = GetMarkdownEditorForPriceControl(priceField);
                if (markdownEditor != null)
                {
                    CalculateMarkdownFromSellingPrice(priceField, markdownEditor);
                }

                // Keep everything in sync after direct price edit
                RefreshAllUnitPrices();
                UpdateAllProfitMargins();
                RecomputeTaxAmountFromRetailAndTax();

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SellingPriceField_KeyDown: {ex.Message}");
            }
        }

        // Helper method to recalculate price grid values
        private void RecalculatePriceGrid()
        {
            try
            {
                // Find Ult_Price control
                Infragistics.Win.UltraWinGrid.UltraGrid Ult_Price =
                    this.Controls.Find("Ult_Price", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;

                if (Ult_Price != null && Ult_Price.Rows.Count > 0)
                {
                    foreach (Infragistics.Win.UltraWinGrid.UltraGridRow row in Ult_Price.Rows)
                    {
                        if (row.Cells["MRP"].Value != null &&
                            row.Cells["Cost"].Value != null &&
                            row.Cells["Packing"].Value != null &&
                            row.Cells["WholeSalePrice"].Value != null)
                        {
                            float mrp = Convert.ToSingle(row.Cells["MRP"].Value);
                            float cost = Convert.ToSingle(row.Cells["Cost"].Value);
                            int packing = Convert.ToInt32(row.Cells["Packing"].Value);
                            float retailPrice = Convert.ToSingle(row.Cells["WholeSalePrice"].Value); // Master selling price (Retail)

                            // Calculate margin amount and percentage
                            float margin = (retailPrice * packing) - cost;
                            float marginPer = margin / mrp * 100;

                            // Update the grid cells
                            row.Cells["MarginAmt"].Value = margin;
                            row.Cells["MarginPer"].Value = marginPer;

                            // Calculate tax amount if tax percentage is available
                            if (row.Cells["TaxPer"].Value != null)
                            {
                                float taxPer = Convert.ToSingle(row.Cells["TaxPer"].Value);
                                float taxAmt = (float)ComputeTaxAmountForGridRow(retailPrice, taxPer);
                                row.Cells["TaxAmt"].Value = taxAmt;
                            }
                        }
                    }
                    SyncUomGridWithPriceGrid();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error recalculating price grid: {ex.Message}");
            }
        }

        // Method to recalculate markup percentage in textBox1
        private void RecalculateMarkupPercentage(bool ignoreLoadingFlag = false)
        {
            try
            {
                if (isLoadingItem && !ignoreLoadingFlag) return; // don't recalc while loading unless explicitly requested
                if (textBox1 != null && !string.IsNullOrWhiteSpace(Txt_UnitCost?.Text) && !string.IsNullOrWhiteSpace(txt_Retail?.Text))
                {
                    float unitCost;
                    float retailPrice;
                    if (float.TryParse(Txt_UnitCost.Text, out unitCost) &&
                        float.TryParse(txt_Retail.Text, out retailPrice) &&
                        unitCost > 0)
                    {
                        // Calculate markup percentage: ((Retail Price / Unit Cost) - 1) * 100
                        double markupPercent = (retailPrice / unitCost - 1.0) * 100.0;

                        isUpdatingMarkup = true;
                        textBox1.Text = markupPercent.ToString("0.00");
                        isUpdatingMarkup = false;

                        System.Diagnostics.Debug.WriteLine($"Recalculated markup percentage: {markupPercent}%");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error recalculating markup percentage: {ex.Message}");
            }
        }

        // Method to update markup in calculator if it's open
        private void UpdateCalculatorMarkup()
        {
            try
            {
                if (unitCostCalculator != null && !unitCostCalculator.IsDisposed && unitCostCalculator.Visible)
                {
                    if (!string.IsNullOrWhiteSpace(textBox1.Text))
                    {
                        unitCostCalculator.SetMarginPercentage(textBox1.Text);
                        System.Diagnostics.Debug.WriteLine($"Updated calculator markup to: {textBox1.Text}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating calculator markup: {ex.Message}");
            }
        }

        // Keep Ult_Price grid's MarginPer column in sync with master profit margin (ultraTextEditor4)
        private void SyncUltPriceMarginPerFromMaster()
        {
            try
            {
                var Ult_Price = this.Controls.Find("Ult_Price", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;
                if (Ult_Price == null || Ult_Price.Rows.Count == 0) return;

                double masterMarginPer = 0;
                if (ultraTextEditor4 != null && !string.IsNullOrWhiteSpace(ultraTextEditor4.Text))
                {
                    double.TryParse(ultraTextEditor4.Text, out masterMarginPer);
                }

                foreach (Infragistics.Win.UltraWinGrid.UltraGridRow row in Ult_Price.Rows)
                {
                    if (row.Cells.Exists("MarginPer"))
                    {
                        row.Cells["MarginPer"].Value = (float)masterMarginPer;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error syncing grid MarginPer from master: {ex.Message}");
            }
        }

        // Compute and display:
        // - isinclexcl: tax delta (Retail * Tax%)
        // - txt_TaxAmount: total amount after tax (Retail + tax delta)
        private void UpdateInclusiveExclusiveTaxDisplay()
        {
            try
            {
                var isinclexclCtrl = this.Controls.Find("isinclexcl", true).FirstOrDefault() as TextBox;
                // isinclexcl is optional; but txt_TaxAmount is required per request
                var taxAmountCtrl = txt_TaxAmount;
                if (isinclexclCtrl == null && taxAmountCtrl == null) return;

                double retail = 0, taxPer = 0;
                double.TryParse(txt_Retail != null ? txt_Retail.Text : "0", out retail);
                double.TryParse(txt_TaxPer != null ? txt_TaxPer.Text : "0", out taxPer);

                if (retail <= 0 || taxPer < 0)
                {
                    if (isinclexclCtrl != null) isinclexclCtrl.Text = "0.00";
                    if (taxAmountCtrl != null) taxAmountCtrl.Text = retail.ToString("0.00");
                    return;
                }

                // Determine tax mode from txt_TaxType/combobox
                string mode = (txt_TaxType != null ? (txt_TaxType.Text ?? string.Empty) : string.Empty).ToLowerInvariant();
                bool isInclusive = mode.Contains("incl");

                double taxAmount;
                double totalWithTax;
                if (isInclusive)
                {
                    // Retail is tax-inclusive: extract tax component
                    // taxAmount = retail - (retail / (1 + taxPer/100))
                    double divisor = 1.0 + (taxPer / 100.0);
                    double basePrice = divisor > 0 ? (retail / divisor) : retail;
                    taxAmount = retail - basePrice;
                    totalWithTax = retail; // already includes tax
                }
                else
                {
                    // Exclusive: add tax on top
                    taxAmount = retail * (taxPer / 100.0);
                    totalWithTax = retail + taxAmount;
                }

                if (isinclexclCtrl != null) isinclexclCtrl.Text = taxAmount.ToString("0.00");
                if (taxAmountCtrl != null) taxAmountCtrl.Text = totalWithTax.ToString("0.00");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating isinclexcl: {ex.Message}");
            }
        }

        // Helper to compute per-row tax amount based on incl/excl mode
        private double ComputeTaxAmountForGridRow(double retailPerRow, double taxPer)
        {
            try
            {
                if (retailPerRow <= 0 || taxPer < 0) return 0;
                string mode = (txt_TaxType != null ? (txt_TaxType.Text ?? string.Empty) : string.Empty).ToLowerInvariant();
                bool isInclusive = mode.Contains("incl");
                if (isInclusive)
                {
                    double divisor = 1.0 + (taxPer / 100.0);
                    double basePrice = divisor > 0 ? (retailPerRow / divisor) : retailPerRow;
                    return retailPerRow - basePrice;
                }
                else
                {
                    return retailPerRow * (taxPer / 100.0);
                }
            }
            catch { return 0; }
        }

        // Public wrapper so external callers (e.g., selection dialog) can recalculate
        // txt_TaxAmount using the same logic as navigation load
        public void RecomputeTaxAmountFromRetailAndTax()
        {
            UpdateInclusiveExclusiveTaxDisplay();
        }

        // Method to refresh all unit prices when base prices change
        private void RefreshAllUnitPrices()
        {
            try
            {
                // Get current base prices from form
                float baseCost = 0;
                float.TryParse(Txt_UnitCost.Text, out baseCost);

                float baseMRP = 0;
                float.TryParse(txt_Mrp.Text, out baseMRP);

                float baseRetailPrice = 0;
                float.TryParse(txt_Retail.Text, out baseRetailPrice);

                float baseWalkingPrice = 0;
                float.TryParse(txt_walkin.Text, out baseWalkingPrice);

                float baseCreditPrice = 0;
                float.TryParse(txt_CEP.Text, out baseCreditPrice);

                float baseCardPrice = 0;
                float.TryParse(txt_CardP.Text, out baseCardPrice);

                float taxPer = 0;
                float.TryParse(txt_TaxPer.Text, out taxPer);

                // Find Ult_Price control
                Infragistics.Win.UltraWinGrid.UltraGrid Ult_Price =
                    this.Controls.Find("Ult_Price", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;

                if (Ult_Price != null && Ult_Price.Rows.Count > 0)
                {
                    int rowIndex = 0;
                    foreach (Infragistics.Win.UltraWinGrid.UltraGridRow row in Ult_Price.Rows)
                    {
                        if (row.Cells["Packing"].Value != null)
                        {
                            int packing = Convert.ToInt32(row.Cells["Packing"].Value);

                            // Only update base unit row (row 0) from master fields
                            // Non-base unit rows preserve their user-entered values
                            if (rowIndex == 0)
                            {
                                // Update all price values based on new base prices
                                row.Cells["Cost"].Value = baseCost * packing;
                                row.Cells["MRP"].Value = baseMRP * packing;
                                // Visual mapping: RetailPrice = retail, WholeSalePrice = walking
                                row.Cells["RetailPrice"].Value = baseRetailPrice * packing;
                                row.Cells["WholeSalePrice"].Value = baseWalkingPrice * packing;
                                row.Cells["CreditPrice"].Value = baseCreditPrice * packing;
                                row.Cells["CardPrice"].Value = baseCardPrice * packing;

                                // Recalculate margin and tax; margin % mirrors Retail profit margin
                                float marginAmount = (baseRetailPrice * packing) - (baseCost * packing);
                                double retailMarginPercent = 0;
                                double.TryParse(ultraTextEditor4 != null ? ultraTextEditor4.Text : "0", out retailMarginPercent);
                                float marginPercentage = (float)retailMarginPercent;

                                row.Cells["MarginAmt"].Value = marginAmount;
                                row.Cells["MarginPer"].Value = marginPercentage;
                                row.Cells["TaxPer"].Value = taxPer;
                                row.Cells["TaxAmt"].Value = (float)ComputeTaxAmountForGridRow(baseRetailPrice * packing, taxPer);
                            }
                            // else: Non-base units - preserve user-entered values
                        }
                        rowIndex++;
                    }

                    Ult_Price.Refresh();
                }

                SyncUomGridWithPriceGrid();

                // Update all profit margins after refreshing unit prices
                UpdateAllProfitMargins();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing unit prices: {ex.Message}");
            }
        }

        private void EnsureVendorGridExists()
        {
            // Check if ultraGrid2 exists
            Infragistics.Win.UltraWinGrid.UltraGrid ultraGrid2 =
                this.Controls.Find("ultraGrid2", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;

            if (ultraGrid2 == null)
            {
                System.Diagnostics.Debug.WriteLine("ultraGrid2 not found in the form");
                return;
            }

            // Configure the grid appearance
            ultraGrid2.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            ultraGrid2.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            ultraGrid2.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False; // Read-only
            ultraGrid2.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            ultraGrid2.DisplayLayout.Override.SelectTypeRow = SelectType.Single;
            ultraGrid2.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            ultraGrid2.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;

            // Disable AutoFitStyle to prevent columns from auto-resizing when others are hidden
            ultraGrid2.DisplayLayout.AutoFitStyle = AutoFitStyle.None;

            // Disable automatic column resizing
            ultraGrid2.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;

            // Hide the group-by area (gray bar)
            ultraGrid2.DisplayLayout.GroupByBox.Hidden = true;
            ultraGrid2.DisplayLayout.GroupByBox.Prompt = string.Empty;
            ultraGrid2.DisplayLayout.GroupByBox.Hidden = true;

            // Set rounded borders for the entire grid
            ultraGrid2.DisplayLayout.BorderStyle = UIElementBorderStyle.Rounded3;

            // Configure grid lines - single line borders for rows and columns
            ultraGrid2.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            ultraGrid2.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            ultraGrid2.DisplayLayout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            ultraGrid2.DisplayLayout.Override.BorderStyleRowSelector = UIElementBorderStyle.Solid;

            // Set border width to single line
            ultraGrid2.DisplayLayout.Override.RowAppearance.BorderAlpha = Alpha.Opaque;
            ultraGrid2.DisplayLayout.Override.CellAppearance.BorderAlpha = Alpha.Opaque;

            // Ensure consistent single line borders
            ultraGrid2.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;

            // Remove cell padding/spacing
            ultraGrid2.DisplayLayout.Override.CellPadding = 0;
            ultraGrid2.DisplayLayout.Override.CellClickAction = CellClickAction.CellSelect;
            ultraGrid2.DisplayLayout.Override.RowSpacingBefore = 0;
            ultraGrid2.DisplayLayout.Override.RowSpacingAfter = 0;
            ultraGrid2.DisplayLayout.Override.CellSpacing = 0;

            // Set light blue border color for cells
            Color lightBlue = Color.FromArgb(173, 216, 230); // Light blue for borders
            Color headerBlue = Color.FromArgb(0, 123, 255); // Slightly darker blue for headers

            // Apply border colors
            ultraGrid2.DisplayLayout.Override.CellAppearance.BorderColor = lightBlue;
            ultraGrid2.DisplayLayout.Override.RowAppearance.BorderColor = lightBlue;
            ultraGrid2.DisplayLayout.Override.HeaderAppearance.BorderColor = headerBlue;
            ultraGrid2.DisplayLayout.Override.RowSelectorAppearance.BorderColor = headerBlue;

            // Configure row height - match ultraGrid1
            ultraGrid2.DisplayLayout.Override.MinRowHeight = 22;
            ultraGrid2.DisplayLayout.Override.DefaultRowHeight = 22;

            // Add header styling - blue headers
            ultraGrid2.DisplayLayout.Override.HeaderStyle = HeaderStyle.WindowsXPCommand;
            ultraGrid2.DisplayLayout.Override.HeaderAppearance.BackColor = headerBlue;
            ultraGrid2.DisplayLayout.Override.HeaderAppearance.BackColor2 = headerBlue; // Same color for no gradient
            ultraGrid2.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.None;
            ultraGrid2.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            ultraGrid2.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
            ultraGrid2.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            ultraGrid2.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
            ultraGrid2.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

            // Configure row selector appearance with blue - clean row headers
            ultraGrid2.DisplayLayout.Override.RowSelectorAppearance.BackColor = headerBlue;
            ultraGrid2.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = headerBlue; // Same color for no gradient
            ultraGrid2.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.None;
            ultraGrid2.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
            ultraGrid2.DisplayLayout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.Default;
            ultraGrid2.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.None; // Remove numbers
            ultraGrid2.DisplayLayout.Override.RowSelectorWidth = 15; // Smaller width

            // Set all cells to have white background (no alternate row coloring)
            ultraGrid2.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            ultraGrid2.DisplayLayout.Override.RowAppearance.BackColor2 = Color.White;
            ultraGrid2.DisplayLayout.Override.RowAppearance.BackGradientStyle = GradientStyle.None;

            // Remove alternate row appearance (make all rows white)
            ultraGrid2.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.White;
            ultraGrid2.DisplayLayout.Override.RowAlternateAppearance.BackColor2 = Color.White;
            ultraGrid2.DisplayLayout.Override.RowAlternateAppearance.BackGradientStyle = GradientStyle.None;

            // Configure selected row appearance with highlight that maintains readability
            ultraGrid2.DisplayLayout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(173, 216, 255); // Light blue highlight matching ultraGrid1
            ultraGrid2.DisplayLayout.Override.SelectedRowAppearance.BackColor2 = Color.FromArgb(173, 216, 255);
            ultraGrid2.DisplayLayout.Override.SelectedRowAppearance.BackGradientStyle = GradientStyle.None;
            ultraGrid2.DisplayLayout.Override.SelectedRowAppearance.ForeColor = SystemColors.ControlText; // Black text matching ultraGrid1

            // Configure active row appearance - make it same as selected row (matching FrmPurchase.cs)
            ultraGrid2.DisplayLayout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(173, 216, 255);
            ultraGrid2.DisplayLayout.Override.ActiveRowAppearance.BackColor2 = Color.FromArgb(173, 216, 255);
            ultraGrid2.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = GradientStyle.None;
            ultraGrid2.DisplayLayout.Override.ActiveRowAppearance.ForeColor = SystemColors.ControlText;
            ultraGrid2.DisplayLayout.Override.ActiveRowAppearance.FontData.Bold = DefaultableBoolean.False;

            // Configure spacing and expansion behavior
            ultraGrid2.DisplayLayout.InterBandSpacing = 0;
            ultraGrid2.DisplayLayout.Override.ExpansionIndicator = ShowExpansionIndicator.Never;

            // Configure scrollbar style
            ultraGrid2.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
            ultraGrid2.DisplayLayout.ScrollStyle = ScrollStyle.Immediate;

            // Configure the scrollbar look
            if (ultraGrid2.DisplayLayout.ScrollBarLook != null)
            {
                // Configure button appearance
                ultraGrid2.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor = headerBlue;
                ultraGrid2.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor2 = headerBlue;
                ultraGrid2.DisplayLayout.ScrollBarLook.ButtonAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid2.DisplayLayout.ScrollBarLook.ButtonAppearance.BorderColor = headerBlue;

                // Configure track appearance
                ultraGrid2.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor = Color.White;
                ultraGrid2.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor2 = Color.White;
                ultraGrid2.DisplayLayout.ScrollBarLook.TrackAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid2.DisplayLayout.ScrollBarLook.TrackAppearance.BorderColor = lightBlue;

                // Configure thumb appearance
                ultraGrid2.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor = headerBlue;
                ultraGrid2.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor2 = headerBlue;
                ultraGrid2.DisplayLayout.ScrollBarLook.ThumbAppearance.BackGradientStyle = GradientStyle.None;
                ultraGrid2.DisplayLayout.ScrollBarLook.ThumbAppearance.BorderColor = headerBlue;
            }

            // Configure cell appearance to increase vertical content alignment
            ultraGrid2.DisplayLayout.Override.CellAppearance.TextVAlign = VAlign.Middle;

            // Create empty DataTable for initial setup
            DataTable dt = new DataTable();
            dt.Columns.Add("LedgerID", typeof(int));
            dt.Columns.Add("VendorName", typeof(string));
            dt.Columns.Add("Cost", typeof(double));
            dt.Columns.Add("Unit", typeof(string));
            dt.Columns.Add("InvoiceDate", typeof(DateTime));
            dt.Columns.Add("PurchaseNo", typeof(int));
            dt.Columns.Add("InvoiceNo", typeof(string));

            // Set the data source
            ultraGrid2.DataSource = dt;

            // Configure column headers and visibility
            if (ultraGrid2.DisplayLayout.Bands.Count > 0)
            {
                // Set column headers
                ultraGrid2.DisplayLayout.Bands[0].Columns["LedgerID"].Header.Caption = "Ledger ID";
                ultraGrid2.DisplayLayout.Bands[0].Columns["VendorName"].Header.Caption = "Vendor Name";
                ultraGrid2.DisplayLayout.Bands[0].Columns["Cost"].Header.Caption = "Cost";
                ultraGrid2.DisplayLayout.Bands[0].Columns["Unit"].Header.Caption = "Unit";
                ultraGrid2.DisplayLayout.Bands[0].Columns["InvoiceDate"].Header.Caption = "Invoice Date";
                ultraGrid2.DisplayLayout.Bands[0].Columns["PurchaseNo"].Header.Caption = "Purchase No";
                ultraGrid2.DisplayLayout.Bands[0].Columns["InvoiceNo"].Header.Caption = "Invoice No";

                // Set column widths - match ultraGrid1 pattern
                ultraGrid2.DisplayLayout.Bands[0].Columns["VendorName"].Width = 200;
                ultraGrid2.DisplayLayout.Bands[0].Columns["InvoiceNo"].Width = 120;
                ultraGrid2.DisplayLayout.Bands[0].Columns["InvoiceDate"].Width = 100;
                ultraGrid2.DisplayLayout.Bands[0].Columns["Cost"].Width = 100;
                ultraGrid2.DisplayLayout.Bands[0].Columns["Unit"].Width = 80;
                ultraGrid2.DisplayLayout.Bands[0].Columns["PurchaseNo"].Width = 120;

                // Format date column
                ultraGrid2.DisplayLayout.Bands[0].Columns["InvoiceDate"].Format = "dd/MM/yyyy";

                // Format cost column
                ultraGrid2.DisplayLayout.Bands[0].Columns["Cost"].Format = "N2";

                // Hide LedgerID column
                ultraGrid2.DisplayLayout.Bands[0].Columns["LedgerID"].Hidden = true;

                // Set appearance for text columns
                ultraGrid2.DisplayLayout.Bands[0].Columns["VendorName"].CellAppearance.TextHAlign = HAlign.Left;
                ultraGrid2.DisplayLayout.Bands[0].Columns["Unit"].CellAppearance.TextHAlign = HAlign.Left;
                ultraGrid2.DisplayLayout.Bands[0].Columns["InvoiceNo"].CellAppearance.TextHAlign = HAlign.Left;
                ultraGrid2.DisplayLayout.Bands[0].Columns["PurchaseNo"].CellAppearance.TextHAlign = HAlign.Right;
                ultraGrid2.DisplayLayout.Bands[0].Columns["Cost"].CellAppearance.TextHAlign = HAlign.Right;
                ultraGrid2.DisplayLayout.Bands[0].Columns["InvoiceDate"].CellAppearance.TextHAlign = HAlign.Center;
            }
        }

        // Method to convert Ult_Price UltraGrid data to DataGridView format for backward compatibility
        private DataGridView ConvertUltPriceToDataGridView()
        {
            // Create a temporary DataGridView for compatibility
            DataGridView tempDgv = new DataGridView();
            tempDgv.AllowUserToAddRows = false;

            // Add necessary columns
            tempDgv.Columns.Add("Unit", "Unit");
            tempDgv.Columns.Add("Packing", "Packing");
            tempDgv.Columns.Add("Cost", "Cost");
            tempDgv.Columns.Add("MarginAmt", "MarginAmt");
            tempDgv.Columns.Add("MarginPer", "MarginPer");
            tempDgv.Columns.Add("TaxPer", "TaxPer");
            tempDgv.Columns.Add("TaxAmt", "TaxAmt");
            tempDgv.Columns.Add("RetailPrice", "RetailPrice");
            tempDgv.Columns.Add("MRP", "MRP");
            tempDgv.Columns.Add("WholeSalePrice", "WholeSalePrice");
            tempDgv.Columns.Add("CreditPrice", "CreditPrice");
            tempDgv.Columns.Add("CardPrice", "CardPrice");
            tempDgv.Columns.Add("StaffPrice", "StaffPrice");
            tempDgv.Columns.Add("MinPrice", "MinPrice");

            // Find Ult_Price control
            Infragistics.Win.UltraWinGrid.UltraGrid Ult_Price =
                this.Controls.Find("Ult_Price", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;

            if (Ult_Price != null && Ult_Price.Rows.Count > 0)
            {
                // Copy data from Ult_Price to tempDgv
                foreach (Infragistics.Win.UltraWinGrid.UltraGridRow row in Ult_Price.Rows)
                {
                    DataGridViewRow dgvRow = new DataGridViewRow();
                    tempDgv.Rows.Add(dgvRow);
                    int rowIndex = tempDgv.Rows.Count - 1;

                    // Copy each cell value
                    tempDgv.Rows[rowIndex].Cells["Unit"].Value = row.Cells["Unit"].Value?.ToString() ?? string.Empty;
                    tempDgv.Rows[rowIndex].Cells["Packing"].Value = row.Cells["Packing"].Value?.ToString() ?? string.Empty;
                    tempDgv.Rows[rowIndex].Cells["Cost"].Value = row.Cells["Cost"].Value?.ToString() ?? "0";
                    tempDgv.Rows[rowIndex].Cells["MarginAmt"].Value = row.Cells["MarginAmt"].Value?.ToString() ?? "0";
                    tempDgv.Rows[rowIndex].Cells["MarginPer"].Value = row.Cells["MarginPer"].Value?.ToString() ?? "0";
                    tempDgv.Rows[rowIndex].Cells["TaxPer"].Value = row.Cells["TaxPer"].Value?.ToString() ?? "0";
                    tempDgv.Rows[rowIndex].Cells["TaxAmt"].Value = row.Cells["TaxAmt"].Value?.ToString() ?? "0";
                    tempDgv.Rows[rowIndex].Cells["MRP"].Value = row.Cells["MRP"].Value?.ToString() ?? "0";
                    // SWAP for DB: grid RetailPrice (visual retail) ? tempDgv WholeSalePrice (DB retail)
                    //              grid WholeSalePrice (visual walking) ? tempDgv RetailPrice (DB walking)
                    tempDgv.Rows[rowIndex].Cells["RetailPrice"].Value = row.Cells["WholeSalePrice"].Value?.ToString() ?? "0";
                    tempDgv.Rows[rowIndex].Cells["WholeSalePrice"].Value = row.Cells["RetailPrice"].Value?.ToString() ?? "0";
                    tempDgv.Rows[rowIndex].Cells["CreditPrice"].Value = row.Cells["CreditPrice"].Value?.ToString() ?? "0";
                    tempDgv.Rows[rowIndex].Cells["CardPrice"].Value = row.Cells["CardPrice"].Value?.ToString() ?? "0";
                    if (row.Cells.Exists("StaffPrice"))
                        tempDgv.Rows[rowIndex].Cells["StaffPrice"].Value = row.Cells["StaffPrice"].Value?.ToString() ?? "0";
                    if (row.Cells.Exists("MinPrice"))
                        tempDgv.Rows[rowIndex].Cells["MinPrice"].Value = row.Cells["MinPrice"].Value?.ToString() ?? "0";
                }
            }

            return tempDgv;
        }

        private void SetupRowFooter()
        {
            try
            {
                if (gridFooterPanel == null)
                    gridFooterPanel = new Infragistics.Win.Misc.UltraPanel();
                Control parent = ultraGrid1.Parent;
                if (parent == null) return;
                if (gridFooterPanel.Parent != parent)
                    parent.Controls.Add(gridFooterPanel);
                int footerHeight = 22;
                if (ultraGrid1 != null && ultraGrid1.DisplayLayout != null && ultraGrid1.DisplayLayout.Override != null)
                {
                    footerHeight = ultraGrid1.DisplayLayout.Override.DefaultRowHeight;
                    footerHeight = Math.Max(footerHeight, 22);
                }
                gridFooterPanel.Top = ultraGrid1.Bottom;
                gridFooterPanel.Left = ultraGrid1.Left;
                gridFooterPanel.Width = ultraGrid1.Width;
                gridFooterPanel.Height = footerHeight;
                gridFooterPanel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
                ultraGrid1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
                ultraGrid1.Height -= gridFooterPanel.Height;
                ultraGrid1.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
                Color headerBlue = Color.FromArgb(0, 123, 255);
                gridFooterPanel.Appearance.BorderColor = headerBlue;
                gridFooterPanel.BorderStyle = Infragistics.Win.UIElementBorderStyle.None;
                gridFooterPanel.Appearance.BackColor = headerBlue;
                gridFooterPanel.Appearance.BackColor2 = headerBlue;
                gridFooterPanel.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                gridFooterPanel.ClientArea.Controls.Clear();
                CreateFooterCells();
                ultraGrid1.AfterRowInsert += (s, e) => UpdateFooterValues();
                ultraGrid1.AfterRowsDeleted += (s, e) => UpdateFooterValues();
                ultraGrid1.AfterCellUpdate += (s, e) => UpdateFooterValues();
                ultraGrid1.InitializeLayout += (s, e) =>
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        UpdateFooterCellPositions();
                        UpdateFooterValues();
                    }));
                };
                gridFooterPanel.Visible = true;
                gridFooterPanel.BringToFront();
                // Timer to keep footer in sync
                var timer = new System.Windows.Forms.Timer();
                timer.Interval = 500;
                timer.Tick += (s, e) => UpdateFooterCellPositions();
                timer.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error setting up row footer: " + ex.Message);
            }
        }

        private void CreateFooterCells()
        {
            try
            {
                gridFooterPanel.ClientArea.Controls.Clear();
                footerLabels.Clear();
                if (ultraGrid1.DisplayLayout.Bands.Count == 0)
                    return;
                UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
                int rowSelectorWidth = ultraGrid1.DisplayLayout.Override.RowSelectorWidth;
                int xOffset = rowSelectorWidth;
                foreach (UltraGridColumn col in band.Columns)
                {
                    if (col.Hidden) continue;
                    Label lblFooter = new Label();
                    lblFooter.Name = "footer_" + col.Key;
                    lblFooter.Text = "";
                    lblFooter.TextAlign = ContentAlignment.MiddleCenter;
                    lblFooter.BackColor = Color.FromArgb(0, 123, 255);
                    lblFooter.BorderStyle = BorderStyle.None;
                    lblFooter.AutoSize = false;
                    lblFooter.Width = col.Width;
                    lblFooter.Height = gridFooterPanel.Height - 2;
                    lblFooter.Left = xOffset;
                    lblFooter.Top = 1;
                    lblFooter.Tag = col.Key;
                    lblFooter.ForeColor = Color.White;
                    lblFooter.Paint += FooterLabel_Paint;
                    ContextMenuStrip menu = CreateFooterContextMenu(col.Key);
                    lblFooter.ContextMenuStrip = menu;
                    gridFooterPanel.ClientArea.Controls.Add(lblFooter);
                    footerLabels[col.Key] = lblFooter;
                    xOffset += col.Width;
                }
                if (columnAggregations.Count == 0)
                {
                    foreach (UltraGridColumn col in band.Columns)
                    {
                        columnAggregations[col.Key] = "None";
                    }
                }
                UpdateFooterValues();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error creating footer cells: " + ex.Message);
            }
        }

        private ContextMenuStrip CreateFooterContextMenu(string columnKey)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Tag = columnKey;
            bool isNumeric = IsNumericColumn(ultraGrid1.DisplayLayout.Bands[0].Columns[columnKey]);
            ToolStripMenuItem itemSum = new ToolStripMenuItem("Sum") { Enabled = isNumeric, Tag = "Sum" };
            itemSum.Click += FooterContextMenu_Click;
            ToolStripMenuItem itemMin = new ToolStripMenuItem("Min") { Tag = "Min" };
            itemMin.Click += FooterContextMenu_Click;
            ToolStripMenuItem itemMax = new ToolStripMenuItem("Max") { Tag = "Max" };
            itemMax.Click += FooterContextMenu_Click;
            ToolStripMenuItem itemCount = new ToolStripMenuItem("Count") { Tag = "Count" };
            itemCount.Click += FooterContextMenu_Click;
            ToolStripMenuItem itemAvg = new ToolStripMenuItem("Average") { Enabled = isNumeric, Tag = "Avg" };
            itemAvg.Click += FooterContextMenu_Click;
            ToolStripMenuItem itemNone = new ToolStripMenuItem("None") { Tag = "None" };
            itemNone.Click += FooterContextMenu_Click;
            menu.Items.Add(itemSum);
            menu.Items.Add(itemMin);
            menu.Items.Add(itemMax);
            menu.Items.Add(itemCount);
            menu.Items.Add(itemAvg);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(itemNone);
            menu.Opening += (s, e) =>
            {
                string currentAgg = columnAggregations.ContainsKey(columnKey) ? columnAggregations[columnKey] : "None";
                foreach (ToolStripItem item in menu.Items)
                {
                    if (item is ToolStripMenuItem menuItem && menuItem.Tag != null)
                        menuItem.Checked = (menuItem.Tag.ToString() == currentAgg);
                }
            };
            return menu;
        }

        private void FooterContextMenu_Click(object sender, EventArgs e)
        {
            try
            {
                ToolStripMenuItem item = sender as ToolStripMenuItem;
                if (item == null) return;
                ContextMenuStrip menu = item.Owner as ContextMenuStrip;
                if (menu == null || menu.Tag == null) return;
                string columnKey = menu.Tag.ToString();
                string aggregation = item.Tag.ToString();
                columnAggregations[columnKey] = aggregation;
                if (aggregation == "None" && footerLabels.ContainsKey(columnKey))
                {
                    footerLabels[columnKey].Text = "";
                    footerLabels[columnKey].Tag = new Tuple<string, string>(columnKey, "");
                    footerLabels[columnKey].Invalidate();
                }
                UpdateFooterValues();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error handling footer menu click: " + ex.Message);
            }
        }

        private bool IsNumericColumn(UltraGridColumn column)
        {
            if (column == null) return false;
            return column.Key == colPacking || column.Key == colReorder || column.Key == colOpenStock ||
                column.DataType == typeof(int) || column.DataType == typeof(double) ||
                column.DataType == typeof(float) || column.DataType == typeof(decimal) ||
                column.DataType == typeof(long);
        }

        private void UpdateFooterValues()
        {
            try
            {
                if (gridFooterPanel == null || !gridFooterPanel.Visible) return;
                if (ultraGrid1 == null || ultraGrid1.DisplayLayout == null) return;
                DataTable dt = ultraGrid1.DataSource as DataTable;
                if (dt == null || dt.Rows.Count == 0)
                {
                    foreach (string key in footerLabels.Keys)
                    {
                        if (footerLabels.ContainsKey(key))
                        {
                            footerLabels[key].Text = "";
                            footerLabels[key].Tag = new Tuple<string, string>(key, "");
                            footerLabels[key].ForeColor = Color.White;
                            footerLabels[key].Invalidate();
                        }
                    }
                    return;
                }
                foreach (string columnKey in footerLabels.Keys)
                {
                    if (!columnAggregations.ContainsKey(columnKey) || columnAggregations[columnKey] == "None" || !footerLabels.ContainsKey(columnKey))
                    {
                        footerLabels[columnKey].Text = "";
                        footerLabels[columnKey].Tag = new Tuple<string, string>(columnKey, "");
                        footerLabels[columnKey].ForeColor = Color.White;
                        footerLabels[columnKey].Invalidate();
                        continue;
                    }
                    string aggregation = columnAggregations[columnKey];
                    bool isNumeric = IsNumericColumn(ultraGrid1.DisplayLayout.Bands[0].Columns[columnKey]);
                    if ((aggregation == "Sum" || aggregation == "Avg") && !isNumeric)
                    {
                        footerLabels[columnKey].Text = "";
                        footerLabels[columnKey].ForeColor = Color.White;
                        continue;
                    }
                    object result = null;
                    switch (aggregation)
                    {
                        case "Sum": result = CalculateSum(dt, columnKey); break;
                        case "Min": result = CalculateMin(dt, columnKey); break;
                        case "Max": result = CalculateMax(dt, columnKey); break;
                        case "Count": result = dt.Rows.Count; break;
                        case "Avg": result = CalculateAverage(dt, columnKey); break;
                    }
                    string displayValue = FormatAggregationResult(result, columnKey, aggregation);
                    footerLabels[columnKey].Tag = new Tuple<string, string>(columnKey, displayValue);
                    footerLabels[columnKey].Text = displayValue;
                    footerLabels[columnKey].ForeColor = Color.White;
                    footerLabels[columnKey].Invalidate();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error updating footer values: " + ex.Message);
            }
        }

        private object CalculateSum(DataTable dt, string columnKey)
        {
            try
            {
                double sum = 0;
                foreach (DataRow row in dt.Rows)
                {
                    if (row[columnKey] != DBNull.Value)
                    {
                        double value;
                        if (double.TryParse(row[columnKey].ToString(), out value))
                            sum += value;
                    }
                }
                return sum;
            }
            catch { return 0; }
        }
        private object CalculateMin(DataTable dt, string columnKey)
        {
            try
            {
                bool hasValue = false;
                object minVal = null;
                foreach (DataRow row in dt.Rows)
                {
                    if (row[columnKey] != DBNull.Value)
                    {
                        if (!hasValue)
                        {
                            minVal = row[columnKey];
                            hasValue = true;
                        }
                        else if (row[columnKey] is IComparable && ((IComparable)row[columnKey]).CompareTo(minVal) < 0)
                        {
                            minVal = row[columnKey];
                        }
                    }
                }
                return hasValue ? minVal : null;
            }
            catch { return null; }
        }
        private object CalculateMax(DataTable dt, string columnKey)
        {
            try
            {
                bool hasValue = false;
                object maxVal = null;
                foreach (DataRow row in dt.Rows)
                {
                    if (row[columnKey] != DBNull.Value)
                    {
                        if (!hasValue)
                        {
                            maxVal = row[columnKey];
                            hasValue = true;
                        }
                        else if (row[columnKey] is IComparable && ((IComparable)row[columnKey]).CompareTo(maxVal) > 0)
                        {
                            maxVal = row[columnKey];
                        }
                    }
                }
                return hasValue ? maxVal : null;
            }
            catch { return null; }
        }
        private object CalculateAverage(DataTable dt, string columnKey)
        {
            try
            {
                double sum = 0;
                int count = 0;
                foreach (DataRow row in dt.Rows)
                {
                    if (row[columnKey] != DBNull.Value)
                    {
                        double value;
                        if (double.TryParse(row[columnKey].ToString(), out value))
                        {
                            sum += value;
                            count++;
                        }
                    }
                }
                return count > 0 ? sum / count : 0;
            }
            catch { return 0; }
        }
        private string FormatAggregationResult(object result, string columnKey, string aggregation)
        {
            if (result == null) return "";
            try
            {
                UltraGridColumn column = ultraGrid1.DisplayLayout.Bands[0].Columns[columnKey];
                if (aggregation == "Count") return result.ToString();
                if (IsNumericColumn(column))
                {
                    double value;
                    if (double.TryParse(result.ToString(), out value))
                    {
                        if (!string.IsNullOrEmpty(column.Format))
                            return value.ToString(column.Format);
                        else
                            return value.ToString("N2");
                    }
                }
                return result.ToString();
            }
            catch { return result.ToString(); }
        }
        private void UpdateFooterCellPositions()
        {
            try
            {
                if (ultraGrid1 == null || gridFooterPanel == null || !gridFooterPanel.Visible) return;
                if (ultraGrid1.DisplayLayout == null) return;
                if (ultraGrid1.DisplayLayout.Bands == null || ultraGrid1.DisplayLayout.Bands.Count == 0) return;
                UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
                int rowSelectorWidth = ultraGrid1.DisplayLayout.Override.RowSelectorWidth;
                int xOffset = rowSelectorWidth;
                foreach (UltraGridColumn col in band.Columns)
                {
                    if (col.Hidden) continue;
                    if (footerLabels.ContainsKey(col.Key))
                    {
                        Label lblFooter = footerLabels[col.Key];
                        lblFooter.Left = xOffset;
                        lblFooter.Width = col.Width;
                    }
                    xOffset += col.Width;
                }
                gridFooterPanel.Top = ultraGrid1.Bottom;
                gridFooterPanel.Width = ultraGrid1.Width;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error updating footer cell positions: " + ex.Message);
            }
        }
        private void FooterLabel_Paint(object sender, PaintEventArgs e)
        {
            Label lbl = sender as Label;
            if (lbl == null) return;
            string displayText = lbl.Text;
            string columnKey = "";
            if (lbl.Tag is Tuple<string, string>)
            {
                Tuple<string, string> tagData = (Tuple<string, string>)lbl.Tag;
                columnKey = tagData.Item1;
                displayText = tagData.Item2;
            }
            if (string.IsNullOrEmpty(displayText)) return;
            if (columnAggregations.ContainsKey(columnKey) && columnAggregations[columnKey] == "None") return;
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            SizeF textSize = g.MeasureString(displayText, lbl.Font);
            int padding = 6;
            int cornerRadius = 6;
            int margin = 1;
            int boxWidth = lbl.Width - (margin * 2);
            int boxHeight = (int)textSize.Height + padding;
            int x = margin;
            int y = (lbl.Height - boxHeight) / 2;
            Rectangle rect = new Rectangle(x, y, boxWidth, boxHeight);
            Color boxColor = Color.FromArgb(0, 80, 160);
            using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddArc(rect.X, rect.Y, cornerRadius * 2, cornerRadius * 2, 180, 90);
                path.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y, cornerRadius * 2, cornerRadius * 2, 270, 90);
                path.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y + rect.Height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, 90);
                path.AddArc(rect.X, rect.Y + rect.Height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 90, 90);
                path.CloseAllFigures();
                using (SolidBrush brush = new SolidBrush(boxColor))
                {
                    g.FillPath(brush, path);
                }
            }
            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                float textX = x + (boxWidth - textSize.Width) / 2;
                float textY = y + (boxHeight - textSize.Height) / 2;
                textY -= 1;
                g.DrawString(displayText, lbl.Font, textBrush, textX, textY);
            }
            lbl.Text = "";
        }

        // Method to calculate profit margin percentage
        private double CalculateProfitMargin(double unitCost, double sellingPrice)
        {
            try
            {
                if (sellingPrice <= 0)
                    return 0;

                // Profit Margin % = (Selling Price - Cost) ? Selling Price ? 100
                double profitMargin = ((sellingPrice - unitCost) / sellingPrice) * 100;
                return Math.Round(profitMargin, 2); // Round to 2 decimal places
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating profit margin: {ex.Message}");
                return 0;
            }
        }

        // Method to update all profit margin fields
        public void UpdateAllProfitMargins()
        {
            try
            {
                // Get unit cost
                double unitCost = 0;
                if (!string.IsNullOrWhiteSpace(Txt_UnitCost.Text))
                {
                    double.TryParse(Txt_UnitCost.Text, out unitCost);
                }

                if (unitCost <= 0)
                {
                    // If no unit cost, clear all profit margin fields
                    ClearAllProfitMargins();
                    return;
                }

                // Calculate profit margins for each price field
                UpdateProfitMarginForField(txt_Retail, ultraTextEditor4);
                UpdateProfitMarginForField(txt_walkin, ultraTextEditor10);
                UpdateProfitMarginForField(txt_CEP, ultraTextEditor9);
                UpdateProfitMarginForField(txt_Mrp, ultraTextEditor8);
                UpdateProfitMarginForField(txt_CardP, ultraTextEditor7);

                // Update txt_SF profit margin and recalc from markdown only if markdown is not empty
                var txt_SF = this.Controls.Find("txt_SF", true).FirstOrDefault();
                if (txt_SF != null)
                {
                    UpdateProfitMarginForField(txt_SF, ultraTextEditor6);
                    var mdStaff = this.Controls.Find("ultraTextEditor12", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                    if (mdStaff != null && txt_Retail != null && !string.IsNullOrWhiteSpace(mdStaff.Text) && mdStaff.Text != "0" && mdStaff.Text != "0.00")
                    {
                        SetPriceFromMasterConsideringMarkdown(txt_SF, mdStaff, txt_Retail.Text);
                    }
                }

                // Update txt_MinP profit margin and recalc from markdown only if markdown is not empty
                var txt_MinP = this.Controls.Find("txt_MinP", true).FirstOrDefault();
                if (txt_MinP != null)
                {
                    UpdateProfitMarginForField(txt_MinP, ultraTextEditor5);
                    var mdMin = this.Controls.Find("ultraTextEditor11", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                    if (mdMin != null && txt_Retail != null && !string.IsNullOrWhiteSpace(mdMin.Text) && mdMin.Text != "0" && mdMin.Text != "0.00")
                    {
                        SetPriceFromMasterConsideringMarkdown(txt_MinP, mdMin, txt_Retail.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating all profit margins: {ex.Message}");
            }
        }

        // Method to update profit margin for a specific price field
        private void UpdateProfitMarginForField(Control priceField, Infragistics.Win.UltraWinEditors.UltraTextEditor profitMarginField)
        {
            try
            {
                if (priceField == null || profitMarginField == null)
                    return;

                // Get unit cost
                double unitCost = 0;
                if (!string.IsNullOrWhiteSpace(Txt_UnitCost.Text))
                {
                    double.TryParse(Txt_UnitCost.Text, out unitCost);
                }

                if (unitCost <= 0)
                {
                    profitMarginField.Text = "0.00";
                    return;
                }

                // Get selling price
                double sellingPrice = 0;
                if (!string.IsNullOrWhiteSpace(priceField.Text))
                {
                    double.TryParse(priceField.Text, out sellingPrice);
                }

                // Calculate profit margin
                double profitMargin = CalculateProfitMargin(unitCost, sellingPrice);

                // Update the profit margin field
                profitMarginField.Text = profitMargin.ToString("0.00");

                System.Diagnostics.Debug.WriteLine($"Updated profit margin for {priceField.Name}: {profitMargin}% (Cost: {unitCost}, Price: {sellingPrice})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating profit margin for {priceField?.Name}: {ex.Message}");
            }
        }

        // Method to clear all profit margin fields
        private void ClearAllProfitMargins()
        {
            try
            {
                if (ultraTextEditor4 != null) ultraTextEditor4.Text = "0.00";
                if (ultraTextEditor10 != null) ultraTextEditor10.Text = "0.00";
                if (ultraTextEditor9 != null) ultraTextEditor9.Text = "0.00";
                if (ultraTextEditor8 != null) ultraTextEditor8.Text = "0.00";
                if (ultraTextEditor7 != null) ultraTextEditor7.Text = "0.00";

                // Clear profit margin fields with fallback to dynamic finding
                var u6 = ultraTextEditor6 ?? (this.Controls.Find("ultraTextEditor6", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor);
                if (u6 != null) u6.Text = "0.00";

                var u5 = ultraTextEditor5 ?? (this.Controls.Find("ultraTextEditor5", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor);
                if (u5 != null) u5.Text = "0.00";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing profit margins: {ex.Message}");
            }
        }

        // Event handler for txt_SF text changed
        private void txt_SF_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Update profit margin for txt_SF
                var control = sender as Control;
                if (control != null)
                {
                    var staffTextBox = control as TextBox;
                    if (staffTextBox != null)
                    {
                        // Resolve profit margin editor (ultraTextEditor6) even if field reference is null
                        var profitEditor = ultraTextEditor6 ?? (this.Controls.Find("ultraTextEditor6", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor);
                        if (profitEditor != null)
                        {
                            UpdateProfitMarginForField(staffTextBox, profitEditor);
                        }
                        // Mirror into grid
                        SyncStaffPriceToPriceGridFromTxtSF(staffTextBox.Text);
                        // Keep markdown (ultraTextEditor12) in sync with selling price
                        var mdStaff = this.Controls.Find("ultraTextEditor12", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                        if (mdStaff != null)
                        {
                            CalculateMarkdownFromSellingPrice(staffTextBox, mdStaff);
                        }
                    }
                    else
                    {
                        // For other control types that expose Text
                        SyncStaffPriceToPriceGridFromTxtSF(control.Text);
                        var mdStaff = this.Controls.Find("ultraTextEditor12", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                        if (mdStaff != null)
                        {
                            CalculateMarkdownFromSellingPrice(control, mdStaff);
                        }
                        // Also try to push profit margin if possible
                        var profitEditor = ultraTextEditor6 ?? (this.Controls.Find("ultraTextEditor6", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor);
                        if (profitEditor != null)
                        {
                            UpdateProfitMarginForField(control, profitEditor);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating txt_SF profit margin: {ex.Message}");
            }
        }

        // Event handler for txt_MinP text changed
        private void txt_MinP_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Update profit margin for txt_MinP and mirror to grid
                var control = sender as Control;
                if (control != null)
                {
                    var minTextBox = control as TextBox;
                    if (minTextBox != null)
                    {
                        // Resolve profit margin editor (ultraTextEditor5) even if field reference is null
                        var profitEditor = ultraTextEditor5 ?? (this.Controls.Find("ultraTextEditor5", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor);
                        if (profitEditor != null)
                        {
                            UpdateProfitMarginForField(minTextBox, profitEditor);
                        }
                        SyncMinPriceToPriceGridFromTxtMinP(minTextBox.Text);
                        // Keep markdown (ultraTextEditor11) in sync with selling price
                        var mdMin = this.Controls.Find("ultraTextEditor11", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                        if (mdMin != null)
                        {
                            CalculateMarkdownFromSellingPrice(minTextBox, mdMin);
                        }
                    }
                    else
                    {
                        SyncMinPriceToPriceGridFromTxtMinP(control.Text);
                        var mdMin = this.Controls.Find("ultraTextEditor11", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                        if (mdMin != null)
                        {
                            CalculateMarkdownFromSellingPrice(control, mdMin);
                        }
                        // Also try to push profit margin if possible
                        var profitEditor = ultraTextEditor5 ?? (this.Controls.Find("ultraTextEditor5", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor);
                        if (profitEditor != null)
                        {
                            UpdateProfitMarginForField(control, profitEditor);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating txt_MinP profit margin: {ex.Message}");
            }
        }

        // Mirror txt_MinP to Ult_Price grid MinPrice for base unit row
        private void SyncMinPriceToPriceGridFromTxtMinP(string minPriceText)
        {
            try
            {
                double minPriceVal;
                if (!double.TryParse(minPriceText, out minPriceVal)) return;

                var Ult_Price = this.Controls.Find("Ult_Price", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;
                if (Ult_Price == null) return;

                var dt = Ult_Price.DataSource as DataTable;
                if (dt == null) return;

                // Ensure MinPrice column exists
                if (!dt.Columns.Contains("MinPrice"))
                {
                    dt.Columns.Add("MinPrice", typeof(float));
                }

                // Find base unit row (Packing == 1) else first row
                DataRow baseRow = null;
                if (dt.Columns.Contains("Packing"))
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        double p = 0; double.TryParse(Convert.ToString(r["Packing"]), out p);
                        if (Math.Abs(p - 1d) < 0.00001) { baseRow = r; break; }
                    }
                }
                if (baseRow == null && dt.Rows.Count > 0) baseRow = dt.Rows[0];

                if (baseRow != null)
                {
                    baseRow["MinPrice"] = minPriceVal;
                    Ult_Price.DataSource = dt;
                    Ult_Price.Refresh();
                }
            }
            catch { }
        }

        // Mirror txt_SF to Ult_Price grid StaffPrice for base unit row
        private void SyncStaffPriceToPriceGridFromTxtSF(string staffPriceText)
        {
            try
            {
                double staffPriceVal;
                if (!double.TryParse(staffPriceText, out staffPriceVal)) return;

                var Ult_Price = this.Controls.Find("Ult_Price", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;
                if (Ult_Price == null) return;

                var dt = Ult_Price.DataSource as DataTable;
                if (dt == null) return;

                // Ensure StaffPrice column exists
                if (!dt.Columns.Contains("StaffPrice"))
                {
                    dt.Columns.Add("StaffPrice", typeof(float));
                }

                // Find base unit row (Packing == 1) else first row
                DataRow baseRow = null;
                if (dt.Columns.Contains("Packing"))
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        double p = 0; double.TryParse(Convert.ToString(r["Packing"]), out p);
                        if (Math.Abs(p - 1d) < 0.00001) { baseRow = r; break; }
                    }
                }
                if (baseRow == null && dt.Rows.Count > 0) baseRow = dt.Rows[0];

                if (baseRow != null)
                {
                    baseRow["StaffPrice"] = staffPriceVal;
                    Ult_Price.DataSource = dt;
                    Ult_Price.Refresh();
                }
            }
            catch { }
        }

        // Calculate price from master retail using markdown %: newPrice = retail * (1 - md/100)
        // Now handles negative markdown values (which represent markup/increase in price)
        private string CalculatePriceWithMarkdown(string masterRetailText, double markdownPercent)
        {
            double retail;
            if (!double.TryParse(masterRetailText, out retail)) retail = 0;

            // Handle negative markdown (which represents markup/increase)
            // For negative values: newPrice = retail * (1 + |markdown|/100)
            // For positive values: newPrice = retail * (1 - markdown/100)
            double newPrice;
            if (markdownPercent < 0)
            {
                // Negative markdown = markup (increase price)
                newPrice = retail * (1.0 + (Math.Abs(markdownPercent) / 100.0));
            }
            else if (markdownPercent > 100)
            {
                // Cap at 100% discount (free item)
                newPrice = 0;
            }
            else
            {
                // Normal markdown (decrease price)
                newPrice = retail * (1.0 - (markdownPercent / 100.0));
            }

            return newPrice.ToString("0.000");
        }

        // Calculate markdown percentage from selling price: markdown = (1 - sellingPrice/masterRetail) * 100
        // Now properly handles cases where selling price > master retail (negative markdown = markup)
        private void CalculateMarkdownFromSellingPrice(Control sellingPriceField, Infragistics.Win.UltraWinEditors.UltraTextEditor markdownEditor)
        {
            try
            {
                if (sellingPriceField == null || markdownEditor == null)
                    return;

                // Get master retail price (base price)
                string masterRetailText = txt_Retail != null ? txt_Retail.Text : "0";
                double masterRetail = 0;
                if (!double.TryParse(masterRetailText, out masterRetail) || masterRetail <= 0)
                    return;

                // Get selling price
                string sellingPriceText = sellingPriceField.Text ?? "0";
                double sellingPrice = 0;
                if (!double.TryParse(sellingPriceText, out sellingPrice))
                    return;

                // Calculate markdown percentage: markdown = (1 - sellingPrice/masterRetail) * 100
                // This will be negative if selling price > master retail (markup case)
                double markdownPercent = (1.0 - (sellingPrice / masterRetail)) * 100.0;

                // Update the markdown editor - allow negative values for markup
                markdownEditor.Text = markdownPercent.ToString("0.00");

                System.Diagnostics.Debug.WriteLine($"Calculated markdown for {sellingPriceField.Name}: {markdownPercent}% (Master: {masterRetail}, Selling: {sellingPrice})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating markdown from selling price: {ex.Message}");
            }
        }

        // Calculate selling price and markdown from profit margin: sellingPrice = unitCost / (1 - profitMargin/100)
        private void CalculateSellingPriceAndMarkdownFromProfitMargin(Infragistics.Win.UltraWinEditors.UltraTextEditor profitMarginEditor, Control sellingPriceField, Infragistics.Win.UltraWinEditors.UltraTextEditor markdownEditor)
        {
            try
            {
                if (profitMarginEditor == null || sellingPriceField == null || markdownEditor == null)
                    return;

                // Get unit cost
                string unitCostText = Txt_UnitCost != null ? Txt_UnitCost.Text : "0";
                double unitCost = 0;
                if (!double.TryParse(unitCostText, out unitCost) || unitCost <= 0)
                    return;

                // Get profit margin percentage
                string profitMarginText = profitMarginEditor.Text ?? "0";
                double profitMarginPercent = 0;
                if (!double.TryParse(profitMarginText, out profitMarginPercent))
                    return;

                // Validate profit margin range (0-100)
                if (profitMarginPercent < 0) profitMarginPercent = 0;
                if (profitMarginPercent >= 100) profitMarginPercent = 99.99; // Avoid division by zero

                // Calculate selling price: sellingPrice = unitCost / (1 - profitMargin/100)
                double sellingPrice = unitCost / (1.0 - (profitMarginPercent / 100.0));

                // Update the selling price field
                sellingPriceField.Text = sellingPrice.ToString("0.000");

                // Also calculate and set the corresponding markdown from the new selling price
                // for non-master fields so both values stay in sync
                CalculateMarkdownFromSellingPrice(sellingPriceField, markdownEditor);

                System.Diagnostics.Debug.WriteLine($"Calculated from profit margin {profitMarginPercent}%: Selling Price = {sellingPrice} (Unit Cost: {unitCost})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating selling price and markdown from profit margin: {ex.Message}");
            }
        }

        // Map price field to its profit margin editor
        private Infragistics.Win.UltraWinEditors.UltraTextEditor GetProfitEditorForPriceControl(Control priceControl)
        {
            if (priceControl == null) return null;
            string name = priceControl.Name ?? string.Empty;
            switch (name)
            {
                case nameof(txt_walkin): return ultraTextEditor10;
                case nameof(txt_CEP): return ultraTextEditor9;
                case nameof(txt_Mrp): return ultraTextEditor8;
                case nameof(txt_CardP): return ultraTextEditor7;
                default: return null;
            }
        }

        // Map price field to its markdown editor
        private Infragistics.Win.UltraWinEditors.UltraTextEditor GetMarkdownEditorForPriceControl(Control priceControl)
        {
            if (priceControl == null) return null;
            string name = priceControl.Name ?? string.Empty;
            switch (name)
            {
                case nameof(txt_walkin): return ultraTextEditor16;
                case nameof(txt_CEP): return ultraTextEditor15;
                case nameof(txt_Mrp): return ultraTextEditor14;
                case nameof(txt_CardP): return ultraTextEditor13;
                case nameof(txt_SF):
                    return this.Controls.Find("ultraTextEditor12", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                case nameof(txt_MinP):
                    return this.Controls.Find("ultraTextEditor11", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                default: return null;
            }
        }

        // Map profit margin editor to its selling price field
        private Control GetSellingPriceFieldForProfitMarginEditor(Infragistics.Win.UltraWinEditors.UltraTextEditor profitMarginEditor)
        {
            if (profitMarginEditor == null) return null;

            // Prefer direct instance mapping when available
            if (ultraTextEditor10 != null && profitMarginEditor == ultraTextEditor10) return txt_walkin;
            if (ultraTextEditor9 != null && profitMarginEditor == ultraTextEditor9) return txt_CEP;
            if (ultraTextEditor8 != null && profitMarginEditor == ultraTextEditor8) return txt_Mrp;
            if (ultraTextEditor7 != null && profitMarginEditor == ultraTextEditor7) return txt_CardP;
            if (ultraTextEditor6 != null && profitMarginEditor == ultraTextEditor6)
            {
                var txt_SF_ctrl = this.Controls.Find("txt_SF", true).FirstOrDefault() as Control;
                return txt_SF_ctrl;
            }
            if (ultraTextEditor5 != null && profitMarginEditor == ultraTextEditor5)
            {
                var txt_MinP_ctrl = this.Controls.Find("txt_MinP", true).FirstOrDefault() as Control;
                return txt_MinP_ctrl;
            }

            // Fallback to name-based mapping (robust even if field refs are null)
            string editorName = profitMarginEditor.Name ?? string.Empty;
            if (string.Equals(editorName, "ultraTextEditor10", StringComparison.OrdinalIgnoreCase)) return txt_walkin;
            if (string.Equals(editorName, "ultraTextEditor9", StringComparison.OrdinalIgnoreCase)) return txt_CEP;
            if (string.Equals(editorName, "ultraTextEditor8", StringComparison.OrdinalIgnoreCase)) return txt_Mrp;
            if (string.Equals(editorName, "ultraTextEditor7", StringComparison.OrdinalIgnoreCase)) return txt_CardP;
            if (string.Equals(editorName, "ultraTextEditor6", StringComparison.OrdinalIgnoreCase))
            {
                var txt_SF_ctrl = this.Controls.Find("txt_SF", true).FirstOrDefault() as Control;
                return txt_SF_ctrl;
            }
            if (string.Equals(editorName, "ultraTextEditor5", StringComparison.OrdinalIgnoreCase))
            {
                var txt_MinP_ctrl = this.Controls.Find("txt_MinP", true).FirstOrDefault() as Control;
                return txt_MinP_ctrl;
            }

            return null;
        }

        // Apply master retail to a linked price field considering markdown value
        // Now properly handles negative markdown values (markup) and zero markdown
        private void SetPriceFromMasterConsideringMarkdown(Control priceField, Infragistics.Win.UltraWinEditors.UltraTextEditor markdownEditor, string masterRetail)
        {
            if (priceField == null) return;

            double markdownPercent = 0;
            if (markdownEditor != null && !string.IsNullOrWhiteSpace(markdownEditor.Text))
                double.TryParse(markdownEditor.Text, out markdownPercent);

            // Calculate new price based on markdown value
            // If markdown is exactly zero, mirror master retail
            // If markdown is negative (markup), increase price
            // If markdown is positive, decrease price
            string newPrice;
            if (markdownPercent == 0)
            {
                newPrice = masterRetail;
            }
            else
            {
                newPrice = CalculatePriceWithMarkdown(masterRetail, markdownPercent);
            }

            // If user is actively editing txt_SF, do not change its Text to avoid caret jumps
            if (object.ReferenceEquals(priceField, this.Controls.Find("txt_SF", true).FirstOrDefault()) && isEditingStaffPrice)
            {
                // Still update the corresponding profit margin for live feedback
                var profitEditorWhileEditing = GetProfitEditorForPriceControl(priceField);
                if (profitEditorWhileEditing != null)
                {
                    UpdateProfitMarginForField(priceField, profitEditorWhileEditing);
                }
            }
            else if (object.ReferenceEquals(priceField, this.Controls.Find("txt_MinP", true).FirstOrDefault()) && isEditingMinPrice)
            {
                // Avoid overriding min price while user is typing
                var profitEditorWhileEditing = GetProfitEditorForPriceControl(priceField);
                if (profitEditorWhileEditing != null)
                {
                    UpdateProfitMarginForField(priceField, profitEditorWhileEditing);
                }
            }
            else
            {
                // Avoid recursive triggers if text unchanged
                if (!string.Equals(priceField.Text, newPrice, StringComparison.Ordinal))
                {
                    priceField.Text = newPrice;
                }
            }

            // Update corresponding profit margin
            var profitEditor = GetProfitEditorForPriceControl(priceField);
            if (profitEditor != null)
            {
                UpdateProfitMarginForField(priceField, profitEditor);
            }
        }

        // New method to apply master prices to all linked fields while respecting their current markdown values
        // This ensures negative markdown (markup) values are properly considered when updating prices
        private void ApplyMasterPricesWithMarkdownRespect(string masterRetail)
        {
            try
            {
                // Apply to walking price (txt_walkin) with its markdown editor (ultraTextEditor16)
                SetPriceFromMasterConsideringMarkdown(txt_walkin, ultraTextEditor16, masterRetail);

                // Apply to credit price (txt_CEP) with its markdown editor (ultraTextEditor15)
                SetPriceFromMasterConsideringMarkdown(txt_CEP, ultraTextEditor15, masterRetail);

                // Apply to MRP (txt_Mrp) with its markdown editor (ultraTextEditor14)
                SetPriceFromMasterConsideringMarkdown(txt_Mrp, ultraTextEditor14, masterRetail);

                // Apply to card price (txt_CardP) with its markdown editor (ultraTextEditor13)
                SetPriceFromMasterConsideringMarkdown(txt_CardP, ultraTextEditor13, masterRetail);

                // Apply to staff price and min price if present
                var txt_SF = this.Controls.Find("txt_SF", true).FirstOrDefault() as Control;
                var mdStaff = this.Controls.Find("ultraTextEditor12", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                if (txt_SF != null && mdStaff != null)
                {
                    // Do not override while user edits staff price
                    if (!isEditingStaffPrice)
                        SetPriceFromMasterConsideringMarkdown(txt_SF, mdStaff, masterRetail);
                }

                var txt_MinP = this.Controls.Find("txt_MinP", true).FirstOrDefault() as Control;
                var mdMin = this.Controls.Find("ultraTextEditor11", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                if (txt_MinP != null && mdMin != null)
                {
                    // Do not override while user edits min price
                    if (!isEditingMinPrice)
                        SetPriceFromMasterConsideringMarkdown(txt_MinP, mdMin, masterRetail);
                }

                System.Diagnostics.Debug.WriteLine($"Successfully applied master price {masterRetail} to all linked fields with markdown respect");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying master prices with markdown respect: {ex.Message}");
            }
        }

        // Handle Enter key in markdown editors; calculate once per unchanged value
        // Now properly handles negative markdown values (markup) for price calculations
        private void MarkdownEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            if (isProcessingMarkdown) { e.Handled = true; e.SuppressKeyPress = true; return; }

            try
            {
                isProcessingMarkdown = true;
                var editor = sender as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                if (editor == null) return;

                // Parse markdown value - allow negative values for markup
                double mdPercent = 0;
                double.TryParse(editor.Text, out mdPercent);

                // Determine linked price field using the mapping method
                Control linkedPrice = null;
                var markdownEditor = GetMarkdownEditorForPriceControl(txt_walkin);
                if (editor == markdownEditor) linkedPrice = txt_walkin;
                else
                {
                    markdownEditor = GetMarkdownEditorForPriceControl(txt_CEP);
                    if (editor == markdownEditor) linkedPrice = txt_CEP;
                    else
                    {
                        markdownEditor = GetMarkdownEditorForPriceControl(txt_Mrp);
                        if (editor == markdownEditor) linkedPrice = txt_Mrp;
                        else
                        {
                            markdownEditor = GetMarkdownEditorForPriceControl(txt_CardP);
                            if (editor == markdownEditor) linkedPrice = txt_CardP;
                            else
                            {
                                var txt_SF = this.Controls.Find("txt_SF", true).FirstOrDefault();
                                markdownEditor = GetMarkdownEditorForPriceControl(txt_SF);
                                if (txt_SF != null && editor == markdownEditor) linkedPrice = txt_SF;
                                else
                                {
                                    var txt_MinP = this.Controls.Find("txt_MinP", true).FirstOrDefault();
                                    markdownEditor = GetMarkdownEditorForPriceControl(txt_MinP);
                                    if (txt_MinP != null && editor == markdownEditor) linkedPrice = txt_MinP;
                                }
                            }
                        }
                    }
                }

                // If markdown >= 200, set selling price and its profit margin to 0 and exit
                if (mdPercent >= 200 && linkedPrice != null)
                {
                    try
                    {
                        linkedPrice.Text = "0.000";
                        var profitEditor = GetProfitEditorForPriceControl(linkedPrice);
                        if (profitEditor != null) profitEditor.Text = "0.00";

                        bool prevUpdating = isUpdatingProfitMargins;
                        isUpdatingProfitMargins = true;
                        RefreshAllUnitPrices();
                        isUpdatingProfitMargins = prevUpdating;

                        // Make sure single-field profit margin is correct
                        if (profitEditor != null) UpdateProfitMarginForField(linkedPrice, profitEditor);

                        e.Handled = true; e.SuppressKeyPress = true;
                        return;
                    }
                    catch { }
                }

                // Master retail
                string masterRetail = txt_Retail != null ? (txt_Retail.Text ?? "0") : "0";

                // Skip if same markdown and same retail already applied
                string key = editor.Name ?? Guid.NewGuid().ToString();
                double lastMd = lastAppliedMarkdown.ContainsKey(key) ? lastAppliedMarkdown[key] : double.NaN;
                double lastRetail = lastAppliedMarkdownRetail.ContainsKey(key) ? lastAppliedMarkdownRetail[key] : double.NaN;
                double currentRetailVal; double.TryParse(masterRetail, out currentRetailVal);

                if (!double.IsNaN(lastMd) && !double.IsNaN(lastRetail) && Math.Abs(lastMd - mdPercent) < 0.00001 && Math.Abs(lastRetail - currentRetailVal) < 0.00001)
                {
                    e.Handled = true; e.SuppressKeyPress = true; return;
                }

                // Apply calculation - now handles negative markdown (markup) properly
                if (linkedPrice != null)
                {
                    SetPriceFromMasterConsideringMarkdown(linkedPrice, editor, masterRetail);
                }

                // Record last applied state
                lastAppliedMarkdown[key] = mdPercent;
                lastAppliedMarkdownRetail[key] = currentRetailVal;

                // Keep grids/margins in sync
                RefreshAllUnitPrices();
                UpdateAllProfitMargins();

                e.Handled = true; e.SuppressKeyPress = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in MarkdownEditor_KeyDown: {ex.Message}");
            }
            finally
            {
                isProcessingMarkdown = false;
            }
        }

        // Handle Enter key in profit margin editors; calculate selling price and markdown
        private void ProfitMarginEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            try
            {
                var editor = sender as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                if (editor == null) return;

                // Special handling for Staff profit margin (ultraTextEditor6):
                // Calculate Staff price (txt_SF) and Staff markdown (ultraTextEditor12) only,
                // without disturbing caret while typing in txt_SF.
                if (string.Equals(editor.Name, "ultraTextEditor6", StringComparison.OrdinalIgnoreCase) || editor == ultraTextEditor6)
                {
                    double unitCost = 0;
                    double.TryParse(Txt_UnitCost != null ? Txt_UnitCost.Text : "0", out unitCost);
                    double staffMarginPercent = 0;
                    double.TryParse(editor.Text ?? "0", out staffMarginPercent);

                    if (unitCost > 0)
                    {
                        // sellingPrice = unitCost / (1 - margin/100)
                        if (staffMarginPercent >= 100) staffMarginPercent = 99.999; // avoid division by zero
                        double staffSelling = unitCost / (1.0 - (staffMarginPercent / 100.0));

                        // Update txt_SF without caret jump if user is currently typing
                        var txt_SF_ctrl = this.Controls.Find("txt_SF", true).FirstOrDefault() as TextBox;
                        if (txt_SF_ctrl != null)
                        {
                            if (!isEditingStaffPrice)
                            {
                                string newVal = staffSelling.ToString("0.000");
                                if (!string.Equals(txt_SF_ctrl.Text, newVal, StringComparison.Ordinal))
                                {
                                    txt_SF_ctrl.Text = newVal;
                                }
                            }
                        }

                        // Update ultraTextEditor12 (Staff markdown) from staff price vs master retail
                        var mdStaff = this.Controls.Find("ultraTextEditor12", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                        if (mdStaff != null)
                        {
                            // markdown = (1 - staffPrice / retail) * 100
                            double retail = 0; double.TryParse(txt_Retail != null ? txt_Retail.Text : "0", out retail);
                            if (retail > 0)
                            {
                                double md = (1.0 - (staffSelling / retail)) * 100.0;
                                mdStaff.Text = md.ToString("0.00");
                            }
                        }
                    }

                    // Do not propagate further recalculations; keep scope limited to Staff
                    e.Handled = true; e.SuppressKeyPress = true;
                    return;
                }

                // Special handling for Min price profit margin (ultraTextEditor5):
                // Calculate Min price (txt_MinP) and its markdown (ultraTextEditor11) only,
                // without disturbing caret while typing in txt_MinP.
                if (string.Equals(editor.Name, "ultraTextEditor5", StringComparison.OrdinalIgnoreCase) || editor == ultraTextEditor5)
                {
                    double unitCost2 = 0;
                    double.TryParse(Txt_UnitCost != null ? Txt_UnitCost.Text : "0", out unitCost2);
                    double minMarginPercent = 0;
                    double.TryParse(editor.Text ?? "0", out minMarginPercent);

                    if (unitCost2 > 0)
                    {
                        if (minMarginPercent >= 100) minMarginPercent = 99.999; // avoid division by zero
                        double minSelling = unitCost2 / (1.0 - (minMarginPercent / 100.0));

                        var txt_MinP_ctrl = this.Controls.Find("txt_MinP", true).FirstOrDefault() as TextBox;
                        if (txt_MinP_ctrl != null)
                        {
                            if (!isEditingMinPrice)
                            {
                                string newVal2 = minSelling.ToString("0.000");
                                if (!string.Equals(txt_MinP_ctrl.Text, newVal2, StringComparison.Ordinal))
                                {
                                    txt_MinP_ctrl.Text = newVal2;
                                }
                            }
                        }

                        var mdMin = this.Controls.Find("ultraTextEditor11", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                        if (mdMin != null)
                        {
                            double retail2 = 0; double.TryParse(txt_Retail != null ? txt_Retail.Text : "0", out retail2);
                            if (retail2 > 0)
                            {
                                double md2 = (1.0 - (minSelling / retail2)) * 100.0;
                                mdMin.Text = md2.ToString("0.00");
                            }
                        }
                    }

                    e.Handled = true; e.SuppressKeyPress = true;
                    return;
                }

                // Get the corresponding selling price field and markdown editor
                Control sellingPriceField = GetSellingPriceFieldForProfitMarginEditor(editor);
                Infragistics.Win.UltraWinEditors.UltraTextEditor markdownEditor = null;

                // Find the corresponding markdown editor
                if (sellingPriceField != null)
                {
                    switch (sellingPriceField.Name)
                    {
                        case nameof(txt_walkin):
                            markdownEditor = ultraTextEditor16;
                            break;
                        case nameof(txt_CEP):
                            markdownEditor = ultraTextEditor15;
                            break;
                        case nameof(txt_Mrp):
                            markdownEditor = ultraTextEditor14;
                            break;
                        case nameof(txt_CardP):
                            markdownEditor = ultraTextEditor13;
                            break;
                        case "txt_SF":
                            // Staff markdown
                            markdownEditor = this.Controls.Find("ultraTextEditor12", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                            break;
                        case "txt_MinP":
                            // Min price markdown
                            markdownEditor = this.Controls.Find("ultraTextEditor11", true).FirstOrDefault() as Infragistics.Win.UltraWinEditors.UltraTextEditor;
                            break;
                    }
                }

                if (sellingPriceField != null)
                {
                    // Calculate selling price and markdown from profit margin
                    CalculateSellingPriceAndMarkdownFromProfitMargin(editor, sellingPriceField, markdownEditor);

                    // Keep grids and margins in sync
                    RefreshAllUnitPrices();
                    UpdateAllProfitMargins();

                    System.Diagnostics.Debug.WriteLine($"Successfully calculated selling price and markdown from profit margin in {editor.Name}");
                }

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ProfitMarginEditor_KeyDown: {ex.Message}");
            }
        }

        // Helper method to generate a new item number (triggered by txt_barcode_TextChanged)
        // NOTE: Does NOT clear fields or change focus - user is typing in txt_barcode
        private void GenerateNewItemNumber()
        {
            try
            {
                GenerateNextItemNumberOnly(GetCurrentItemNoForNextNumber());

                // Load default unit (Unit 1)
                LoadDefaultUnit();

                // Load default item type (Stock Item - ID 1)
                LoadDefaultItemType();

                // Switch to Save mode for new item
                if (button3 != null) button3.Visible = true;
                if (btnUpdate != null) btnUpdate.Visible = false;

                // DO NOT change focus - user is typing in txt_barcode
                // DO NOT clear fields - preserve what user is typing

                System.Diagnostics.Debug.WriteLine($"Auto-generated new item number: {txt_ItemNo?.Text}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating new item number: {ex.Message}");
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        private static string[] _navigationFieldOrder;
        private static string[] NavigationFieldOrder
        {
            get
            {
                if (_navigationFieldOrder == null)
                {
                    _navigationFieldOrder = new string[]
                    {
                        "txt_barcode",
                        "txt_description",
                        "txt_LocalLanguage",
                        "txt_Brand",
                        "txt_BaseUnit",
                        "Txt_UnitCost",
                        "textBox1",
                        "txt_Retail",
                        "txt_walkin",
                        "txt_CEP",
                        "txt_Mrp",
                        "txt_CardP",
                        "txt_SF",
                        "txt_MinP",
                        "ultraTextEditor16",
                        "ultraTextEditor15",
                        "ultraTextEditor14",
                        "ultraTextEditor13",
                        "ultraTextEditor12",
                        "ultraTextEditor11",
                        "ultraTextEditor4",
                        "ultraTextEditor10",
                        "ultraTextEditor9",
                        "ultraTextEditor8",
                        "ultraTextEditor7",
                        "ultraTextEditor6",
                        "ultraTextEditor5",
                        "txt_ItemType",
                        "txt_Category",
                        "txt_Group"
                    };
                }
                return _navigationFieldOrder;
            }
        }

        private string GetConfiguredControlName(Control ctrl)
        {
            if (ctrl == null) return null;
            if (!string.IsNullOrEmpty(ctrl.Name) && Array.IndexOf(NavigationFieldOrder, ctrl.Name) >= 0)
                return ctrl.Name;
            return GetConfiguredControlName(ctrl.Parent);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Check if focus is in a grid to let grid handle its own navigation
            if (this.ActiveControl != null &&
                (this.ActiveControl is Infragistics.Win.UltraWinGrid.UltraGrid ||
                 this.ActiveControl.Parent is Infragistics.Win.UltraWinGrid.UltraGrid))
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }

            // Handle Up/Down arrow for navigation
            if (keyData == Keys.Up || keyData == Keys.Down)
            {
                Control activeCtrl = this.ActiveControl;

                // Allow AutoComplete suggesting fields to use Up/Down arrow natively for their dropdown lists
                if (activeCtrl == txt_Brand || activeCtrl == txt_ItemType ||
                    activeCtrl == txt_Category || activeCtrl == txt_Group)
                {
                    // Check if AutoComplete dropdown is open
                    // WinForms uses 'Auto-Suggest Dropdown' class named "Auto-Suggest Dropdown"
                    IntPtr handle = FindWindow("Auto-Suggest Dropdown", null);
                    if (handle != IntPtr.Zero && IsWindowVisible(handle))
                    {
                        // Dropdown is visible, let base handle navigation through the list
                        return base.ProcessCmdKey(ref msg, keyData);
                    }
                }

                string navName = GetConfiguredControlName(activeCtrl);

                if (!string.IsNullOrEmpty(navName))
                {
                    int currentIndex = Array.IndexOf(NavigationFieldOrder, navName);
                    if (currentIndex >= 0)
                    {
                        int nextIndex = currentIndex + (keyData == Keys.Down ? 1 : -1);

                        // Navigate to next control if within bounds
                        if (nextIndex >= 0 && nextIndex < NavigationFieldOrder.Length)
                        {
                            string nextName = NavigationFieldOrder[nextIndex];
                            Control[] foundControls = this.Controls.Find(nextName, true);
                            if (foundControls.Length > 0)
                            {
                                foundControls[0].Focus();
                                return true;
                            }
                        }
                        else
                        {
                            // At edge of array, prevent default action
                            return true;
                        }
                    }
                }

                // Fallback: Navigate if active control is a textbox or combo box but not in our list
                if (activeCtrl is TextBox ||
                    activeCtrl is ComboBox ||
                    activeCtrl is Infragistics.Win.UltraWinEditors.UltraTextEditor ||
                    activeCtrl is Infragistics.Win.UltraWinEditors.UltraComboEditor ||
                    (activeCtrl != null && activeCtrl.GetType().Name.Contains("TextBox")) ||
                    (activeCtrl != null && activeCtrl.GetType().Name.Contains("UltraTextEditor")) ||
                    (activeCtrl != null && activeCtrl.GetType().Name.Contains("MaskedEdit")))
                {
                    bool forward = (keyData == Keys.Down);
                    this.SelectNextControl(activeCtrl, forward, true, true, true);
                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        #region Alternative Barcode Grid Logic

        private void SetupAlternativeBarcodeGrid()
        {
            try
            {
                var grid = this.Controls.Find("ultraGrid3", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;
                if (grid == null) return;

                grid.InitializeLayout -= UltraGrid3_InitializeLayout;
                grid.InitializeLayout += UltraGrid3_InitializeLayout;

                // Create DataTable
                DataTable dt = new DataTable();
                dt.Columns.Add("Barcode", typeof(string));
                grid.DataSource = dt;

                ApplyUltraGrid1ThemeToAlternativeBarcodeGrid(grid);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting up alternative barcode grid: {ex.Message}");
            }
        }

        private void UltraGrid3_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                ApplyUltraGrid1ThemeToAlternativeBarcodeGrid(sender as Infragistics.Win.UltraWinGrid.UltraGrid, e.Layout);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UltraGrid3_InitializeLayout: {ex.Message}");
            }
        }

        private void ApplyUltraGrid1ThemeToAlternativeBarcodeGrid(Infragistics.Win.UltraWinGrid.UltraGrid grid, UltraGridLayout layout = null)
        {
            if (grid == null)
            {
                return;
            }

            UltraGridLayout targetLayout = layout ?? grid.DisplayLayout;
            if (targetLayout == null)
            {
                return;
            }

            Color lightBlue = Color.FromArgb(173, 216, 230);
            Color headerBlue = Color.FromArgb(0, 123, 255);
            Color selectedBlue = Color.FromArgb(210, 232, 255);

            targetLayout.Override.AllowAddNew = AllowAddNew.No;
            targetLayout.Override.AllowDelete = DefaultableBoolean.True;
            targetLayout.Override.AllowUpdate = DefaultableBoolean.True;
            targetLayout.Override.RowSelectors = DefaultableBoolean.True;
            targetLayout.Override.SelectTypeRow = SelectType.Single;
            targetLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            targetLayout.Override.CellClickAction = CellClickAction.EditAndSelectText;

            targetLayout.AutoFitStyle = AutoFitStyle.None;
            targetLayout.Override.AllowColSizing = AllowColSizing.Free;
            targetLayout.Override.AllowColMoving = AllowColMoving.NotAllowed;
            targetLayout.Override.AllowColSwapping = AllowColSwapping.NotAllowed;
            targetLayout.Override.AllowRowFiltering = DefaultableBoolean.False;

            targetLayout.GroupByBox.Hidden = true;
            targetLayout.GroupByBox.Prompt = string.Empty;

            targetLayout.BorderStyle = UIElementBorderStyle.Solid;
            targetLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            targetLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            targetLayout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            targetLayout.Override.BorderStyleRowSelector = UIElementBorderStyle.Solid;
            targetLayout.Override.RowAppearance.BorderAlpha = Alpha.Opaque;
            targetLayout.Override.CellAppearance.BorderAlpha = Alpha.Opaque;

            targetLayout.Override.CellPadding = 0;
            targetLayout.Override.RowSpacingBefore = 0;
            targetLayout.Override.RowSpacingAfter = 0;
            targetLayout.Override.CellSpacing = 0;
            targetLayout.InterBandSpacing = 0;

            targetLayout.Override.CellAppearance.BorderColor = lightBlue;
            targetLayout.Override.RowAppearance.BorderColor = lightBlue;
            targetLayout.Override.HeaderAppearance.BorderColor = headerBlue;
            targetLayout.Override.RowSelectorAppearance.BorderColor = headerBlue;
            targetLayout.Appearance.BorderColor = lightBlue;

            targetLayout.Override.MinRowHeight = 30;
            targetLayout.Override.DefaultRowHeight = 30;
            targetLayout.Override.CellAppearance.TextVAlign = VAlign.Middle;
            targetLayout.Override.CellAppearance.FontData.SizeInPoints = 10;
            targetLayout.Override.RowAppearance.FontData.SizeInPoints = 10;
            targetLayout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            targetLayout.Override.RowAppearance.FontData.Name = "Microsoft Sans Serif";

            targetLayout.Override.HeaderStyle = HeaderStyle.WindowsXPCommand;
            targetLayout.Override.HeaderAppearance.BackColor = headerBlue;
            targetLayout.Override.HeaderAppearance.BackColor2 = headerBlue;
            targetLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.None;
            targetLayout.Override.HeaderAppearance.ForeColor = Color.White;
            targetLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
            targetLayout.Override.HeaderAppearance.TextVAlign = VAlign.Middle;
            targetLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            targetLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
            targetLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

            targetLayout.Override.RowSelectorAppearance.BackColor = headerBlue;
            targetLayout.Override.RowSelectorAppearance.BackColor2 = headerBlue;
            targetLayout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.None;
            targetLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
            targetLayout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.Default;
            targetLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.None;
            targetLayout.Override.RowSelectorWidth = 15;
            targetLayout.Override.ExpansionIndicator = ShowExpansionIndicator.Never;

            targetLayout.Override.RowAppearance.BackColor = Color.White;
            targetLayout.Override.RowAppearance.BackColor2 = Color.White;
            targetLayout.Override.RowAppearance.BackGradientStyle = GradientStyle.None;

            targetLayout.Override.RowAlternateAppearance.BackColor = Color.White;
            targetLayout.Override.RowAlternateAppearance.BackColor2 = Color.White;
            targetLayout.Override.RowAlternateAppearance.BackGradientStyle = GradientStyle.None;

            targetLayout.Override.SelectedRowAppearance.BackColor = selectedBlue;
            targetLayout.Override.SelectedRowAppearance.BackColor2 = selectedBlue;
            targetLayout.Override.SelectedRowAppearance.BackGradientStyle = GradientStyle.None;
            targetLayout.Override.SelectedRowAppearance.ForeColor = Color.Black;

            targetLayout.Override.ActiveRowAppearance.BackColor = selectedBlue;
            targetLayout.Override.ActiveRowAppearance.BackColor2 = selectedBlue;
            targetLayout.Override.ActiveRowAppearance.BackGradientStyle = GradientStyle.None;
            targetLayout.Override.ActiveRowAppearance.ForeColor = Color.Black;

            targetLayout.ScrollBounds = ScrollBounds.ScrollToFill;
            targetLayout.ScrollStyle = ScrollStyle.Immediate;

            if (targetLayout.ScrollBarLook != null)
            {
                targetLayout.ScrollBarLook.ButtonAppearance.BackColor = headerBlue;
                targetLayout.ScrollBarLook.ButtonAppearance.BackColor2 = headerBlue;
                targetLayout.ScrollBarLook.ButtonAppearance.BackGradientStyle = GradientStyle.None;
                targetLayout.ScrollBarLook.ButtonAppearance.BorderColor = headerBlue;

                targetLayout.ScrollBarLook.TrackAppearance.BackColor = Color.White;
                targetLayout.ScrollBarLook.TrackAppearance.BackColor2 = Color.White;
                targetLayout.ScrollBarLook.TrackAppearance.BackGradientStyle = GradientStyle.None;
                targetLayout.ScrollBarLook.TrackAppearance.BorderColor = lightBlue;

                targetLayout.ScrollBarLook.ThumbAppearance.BackColor = headerBlue;
                targetLayout.ScrollBarLook.ThumbAppearance.BackColor2 = headerBlue;
                targetLayout.ScrollBarLook.ThumbAppearance.BackGradientStyle = GradientStyle.None;
                targetLayout.ScrollBarLook.ThumbAppearance.BorderColor = headerBlue;
            }

            if (targetLayout.Bands.Count > 0)
            {
                foreach (UltraGridColumn col in targetLayout.Bands[0].Columns)
                {
                    col.Header.Appearance.BackColor = headerBlue;
                    col.Header.Appearance.BackColor2 = headerBlue;
                    col.Header.Appearance.BackGradientStyle = GradientStyle.None;
                    col.Header.Appearance.ForeColor = Color.White;
                    col.Header.Appearance.BorderColor = headerBlue;
                    col.Header.Appearance.TextHAlign = HAlign.Center;
                    col.Header.Appearance.FontData.Bold = DefaultableBoolean.True;

                    col.CellAppearance.TextVAlign = VAlign.Middle;
                    col.CellAppearance.BorderColor = lightBlue;
                }

                if (targetLayout.Bands[0].Columns.Exists("Barcode"))
                {
                    UltraGridColumn barcodeColumn = targetLayout.Bands[0].Columns["Barcode"];
                    barcodeColumn.Header.Caption = "Alternative Barcode";
                    barcodeColumn.CellActivation = Activation.AllowEdit;
                    barcodeColumn.CellAppearance.TextHAlign = HAlign.Left;
                    barcodeColumn.Width = 220;
                }
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            try
            {
                var grid = this.Controls.Find("ultraGrid3", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;
                if (grid == null) return;

                CommitAlternativeBarcodeGridEdits(grid);

                DataTable dt = grid.DataSource as DataTable;
                if (dt != null)
                {
                    DataRow newRow = dt.NewRow();
                    newRow["Barcode"] = "";
                    dt.Rows.Add(newRow);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding alternative barcode row: {ex.Message}");
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            try
            {
                var grid = this.Controls.Find("ultraGrid3", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;
                if (grid == null || grid.ActiveRow == null) return;

                CommitAlternativeBarcodeGridEdits(grid);

                if (grid.ActiveRow.IsDataRow)
                {
                    grid.ActiveRow.Delete(false); // deletes without prompting
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error removing alternative barcode row: {ex.Message}");
            }
        }

        private Infragistics.Win.UltraWinGrid.UltraGrid GetAlternativeBarcodeGrid()
        {
            return this.Controls.Find("ultraGrid3", true).FirstOrDefault() as Infragistics.Win.UltraWinGrid.UltraGrid;
        }

        private void CommitAlternativeBarcodeGridEdits(Infragistics.Win.UltraWinGrid.UltraGrid grid)
        {
            if (grid == null)
            {
                return;
            }

            try
            {
                if (grid.ActiveCell != null && grid.ActiveCell.IsInEditMode)
                {
                    grid.PerformAction(UltraGridAction.ExitEditMode);
                }

                grid.UpdateData();

                if (grid.DataSource != null)
                {
                    CurrencyManager currencyManager = null;
                    try
                    {
                        currencyManager = this.BindingContext[grid.DataSource, grid.DataMember] as CurrencyManager;
                    }
                    catch
                    {
                        currencyManager = this.BindingContext[grid.DataSource] as CurrencyManager;
                    }

                    currencyManager?.EndCurrentEdit();
                }

                DataTable dt = grid.DataSource as DataTable;
                if (dt != null && dt.Columns.Contains("Barcode"))
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        if (row.RowState == DataRowState.Deleted)
                        {
                            continue;
                        }

                        string barcode = row["Barcode"]?.ToString();
                        row["Barcode"] = string.IsNullOrWhiteSpace(barcode) ? string.Empty : barcode.Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error committing alternative barcode edits: {ex.Message}");
            }
        }

        private void FocusAlternativeBarcodeRow(int rowIndex)
        {
            var grid = GetAlternativeBarcodeGrid();
            if (grid == null)
            {
                return;
            }

            if (rowIndex >= 0 && rowIndex < grid.Rows.Count)
            {
                var row = grid.Rows[rowIndex];
                grid.ActiveRow = row;

                if (row.Cells.Exists("Barcode"))
                {
                    grid.ActiveCell = row.Cells["Barcode"];
                }
            }

            grid.Focus();
        }

        private HashSet<string> BuildAliasBarcodeSet(IEnumerable<ItemMasterPriceSettings> priceSettings)
        {
            HashSet<string> aliasBarcodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (priceSettings == null)
            {
                return aliasBarcodes;
            }

            foreach (var priceSetting in priceSettings)
            {
                string aliasBarcode = priceSetting?.AliasBarcode?.Trim() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(aliasBarcode))
                {
                    aliasBarcodes.Add(aliasBarcode);
                }
            }

            return aliasBarcodes;
        }

        private HashSet<string> GetCurrentAliasBarcodeSet()
        {
            HashSet<string> aliasBarcodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                DataTable uomTable = ultraGrid1?.DataSource as DataTable;
                if (uomTable == null || !uomTable.Columns.Contains("AliasBarcode"))
                {
                    return aliasBarcodes;
                }

                foreach (DataRow row in uomTable.Rows)
                {
                    if (row == null || row.RowState == DataRowState.Deleted)
                    {
                        continue;
                    }

                    string aliasBarcode = row["AliasBarcode"]?.ToString()?.Trim() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(aliasBarcode))
                    {
                        aliasBarcodes.Add(aliasBarcode);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading alias barcodes: {ex.Message}");
            }

            return aliasBarcodes;
        }

        private bool ValidateMainAndAlternativeBarcodeUniqueness(string mainBarcode, int excludeItemId, bool validateMainBarcodeConflicts = true)
        {
            try
            {
                string normalizedMainBarcode = (mainBarcode ?? string.Empty).Trim();

                if (validateMainBarcodeConflicts && !string.IsNullOrWhiteSpace(normalizedMainBarcode))
                {
                    if (ItemRepository.CheckBarcodeExists(normalizedMainBarcode, excludeItemId))
                    {
                        MessageBox.Show($"Main barcode '{normalizedMainBarcode}' already exists.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        var txtBarcodeCtrl = GetMainBarcodeEditor();
                        txtBarcodeCtrl?.Focus();
                        return false;
                    }

                    int mainBarcodeAlternativeOwner = ItemRepository.GetItemIdByAlternativeBarcode(normalizedMainBarcode);
                    if (mainBarcodeAlternativeOwner > 0 && mainBarcodeAlternativeOwner != excludeItemId)
                    {
                        MessageBox.Show($"Main barcode '{normalizedMainBarcode}' already exists as an alternative barcode.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        var txtBarcodeCtrl = GetMainBarcodeEditor();
                        txtBarcodeCtrl?.Focus();
                        return false;
                    }
                }

                var grid = GetAlternativeBarcodeGrid();
                if (grid == null)
                {
                    return true;
                }

                CommitAlternativeBarcodeGridEdits(grid);

                DataTable dt = grid.DataSource as DataTable;
                if (dt == null || !dt.Columns.Contains("Barcode"))
                {
                    return true;
                }

                HashSet<string> currentAlternativeBarcodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                HashSet<string> currentAliasBarcodes = GetCurrentAliasBarcodeSet();

                for (int rowIndex = 0; rowIndex < dt.Rows.Count; rowIndex++)
                {
                    DataRow row = dt.Rows[rowIndex];
                    if (row == null || row.RowState == DataRowState.Deleted)
                    {
                        continue;
                    }

                    string alternativeBarcode = row["Barcode"]?.ToString()?.Trim() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(alternativeBarcode))
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(normalizedMainBarcode) &&
                        alternativeBarcode.Equals(normalizedMainBarcode, StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show($"Alternative barcode '{alternativeBarcode}' cannot be the same as the main barcode.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        FocusAlternativeBarcodeRow(rowIndex);
                        return false;
                    }

                    if (currentAliasBarcodes.Contains(alternativeBarcode))
                    {
                        MessageBox.Show($"Alternative barcode '{alternativeBarcode}' cannot be the same as an alias barcode.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        FocusAlternativeBarcodeRow(rowIndex);
                        return false;
                    }

                    if (!currentAlternativeBarcodes.Add(alternativeBarcode))
                    {
                        MessageBox.Show($"Alternative barcode '{alternativeBarcode}' is entered more than once.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        FocusAlternativeBarcodeRow(rowIndex);
                        return false;
                    }

                    if (ItemRepository.CheckBarcodeExists(alternativeBarcode, excludeItemId))
                    {
                        MessageBox.Show($"Alternative barcode '{alternativeBarcode}' already exists as a main barcode.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        FocusAlternativeBarcodeRow(rowIndex);
                        return false;
                    }

                    int alternativeOwnerItemId = ItemRepository.GetItemIdByAlternativeBarcode(alternativeBarcode);
                    if (alternativeOwnerItemId > 0 && alternativeOwnerItemId != excludeItemId)
                    {
                        MessageBox.Show($"Alternative barcode '{alternativeBarcode}' already exists for another item.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        FocusAlternativeBarcodeRow(rowIndex);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error validating alternative barcodes: " + ex.Message, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void EnsureUomUnitIdsBeforeSave()
        {
            try
            {
                var uomTable = ultraGrid1?.DataSource as DataTable;
                if (uomTable == null || !uomTable.Columns.Contains(colUnit) || !uomTable.Columns.Contains(colUnitId))
                {
                    return;
                }

                Dropdowns drop = new Dropdowns();
                var units = drop.getUnitDDl()?.List?.ToList();
                if (units == null || units.Count == 0)
                {
                    return;
                }

                foreach (DataRow row in uomTable.Rows)
                {
                    if (row == null || row.RowState == DataRowState.Deleted)
                    {
                        continue;
                    }

                    string unitName = row[colUnit]?.ToString()?.Trim() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(unitName))
                    {
                        continue;
                    }

                    int parsedUnitId = 0;
                    int.TryParse(row[colUnitId]?.ToString(), out parsedUnitId);
                    if (parsedUnitId > 0)
                    {
                        continue;
                    }

                    var unitMatch = units.FirstOrDefault(u => string.Equals(u.UnitName, unitName, StringComparison.OrdinalIgnoreCase));
                    if (unitMatch != null)
                    {
                        row[colUnitId] = unitMatch.UnitID.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EnsureUomUnitIdsBeforeSave error: {ex.Message}");
            }
        }

        // Helper to get DataGridView from ultraGrid3 for saving
        private DataGridView GetAlternativeBarcodesDataGridView()
        {
            try
            {
                var grid = GetAlternativeBarcodeGrid();
                if (grid == null) return null;

                CommitAlternativeBarcodeGridEdits(grid);

                DataTable dt = grid.DataSource as DataTable;
                if (dt == null) return null;

                DataGridView tempDgv = new DataGridView();
                tempDgv.AllowUserToAddRows = false;
                tempDgv.Columns.Add("Barcode", "Barcode");

                HashSet<string> aliasBarcodes = GetCurrentAliasBarcodeSet();
                HashSet<string> addedAlternativeBarcodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (DataRow row in dt.Rows)
                {
                    if (row.RowState == DataRowState.Deleted)
                    {
                        continue;
                    }

                    string bcode = row["Barcode"]?.ToString()?.Trim();
                    if (!string.IsNullOrWhiteSpace(bcode) &&
                        !aliasBarcodes.Contains(bcode) &&
                        addedAlternativeBarcodes.Add(bcode))
                    {
                        tempDgv.Rows.Add(bcode);
                    }
                }
                return tempDgv;
            }
            catch
            {
                return null;
            }
        }

        private void MakeControlActAndLookLikeGlossyButton(Control ctrl)
        {
            if (ctrl == null) return;

            Color normalTop = Color.FromArgb(212, 232, 255);
            Color normalBottom = Color.FromArgb(172, 202, 245);
            Color hoverTop = Color.FromArgb(232, 244, 255);
            Color hoverBottom = Color.FromArgb(188, 216, 255);
            Color pressedTop = Color.FromArgb(155, 190, 238);
            Color pressedBottom = Color.FromArgb(185, 212, 248);
            Color border = Color.FromArgb(110, 150, 215);
            Color textNavy = Color.FromArgb(10, 35, 80);

            ctrl.Cursor = Cursors.Hand;

            if (ctrl is Infragistics.Win.Misc.UltraPanel up)
            {
                up.UseAppStyling = false;
                up.UseOsThemes = DefaultableBoolean.False;
                up.BorderStyle = UIElementBorderStyle.Solid;
                up.Appearance.BackColor = normalTop;
                up.Appearance.BackColor2 = normalBottom;
                up.Appearance.BackGradientStyle = GradientStyle.Vertical;
                up.Appearance.BorderColor = border;
                up.Appearance.ForeColor = textNavy;

                // Mouse events for hover and press
                up.MouseEnter -= Panel_MouseEnter;
                up.MouseEnter += Panel_MouseEnter;
                up.MouseLeave -= Panel_MouseLeave;
                up.MouseLeave += Panel_MouseLeave;
                up.MouseDown -= Panel_MouseDown;
                up.MouseDown += Panel_MouseDown;
                up.MouseUp -= Panel_MouseUp;
                up.MouseUp += Panel_MouseUp;

                if (up.ClientArea != null)
                {
                    foreach (Control child in up.ClientArea.Controls)
                    {
                        child.Cursor = Cursors.Hand;
                        child.MouseEnter -= Child_MouseEnter;
                        child.MouseEnter += Child_MouseEnter;
                        child.MouseLeave -= Child_MouseLeave;
                        child.MouseLeave += Child_MouseLeave;
                        child.MouseDown -= Child_MouseDown;
                        child.MouseDown += Child_MouseDown;
                        child.MouseUp -= Child_MouseUp;
                        child.MouseUp += Child_MouseUp;
                        child.Click -= Child_Click;
                        child.Click += Child_Click;
                    }
                }
            }
            else if (ctrl is Button btn)
            {
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderColor = border;
                btn.FlatAppearance.BorderSize = 1;
                btn.BackColor = Color.FromArgb(195, 218, 248);
                btn.ForeColor = textNavy;
                btn.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                btn.Cursor = Cursors.Hand;
            }
            else if (ctrl is Infragistics.Win.Misc.UltraButton ubtn)
            {
                ubtn.UseAppStyling = false;
                ubtn.UseOsThemes = DefaultableBoolean.False;
                ubtn.Appearance.BackColor = normalTop;
                ubtn.Appearance.BackColor2 = normalBottom;
                ubtn.Appearance.BackGradientStyle = GradientStyle.Vertical;
                ubtn.Appearance.BorderColor = border;
                ubtn.Appearance.ForeColor = textNavy;
                ubtn.Appearance.FontData.Bold = DefaultableBoolean.True;
                ubtn.Cursor = Cursors.Hand;
            }
        }

        private void Panel_MouseEnter(object sender, EventArgs e)
        {
            if (sender is Infragistics.Win.Misc.UltraPanel up)
            {
                up.Appearance.BackColor = Color.FromArgb(232, 244, 255);
                up.Appearance.BackColor2 = Color.FromArgb(188, 216, 255);
            }
        }

        private void Panel_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Infragistics.Win.Misc.UltraPanel up)
            {
                up.Appearance.BackColor = Color.FromArgb(212, 232, 255);
                up.Appearance.BackColor2 = Color.FromArgb(172, 202, 245);
            }
        }

        private void Panel_MouseDown(object sender, MouseEventArgs e)
        {
            if (sender is Infragistics.Win.Misc.UltraPanel up)
            {
                up.Appearance.BackColor = Color.FromArgb(155, 190, 238);
                up.Appearance.BackColor2 = Color.FromArgb(185, 212, 248);
            }
        }

        private void Panel_MouseUp(object sender, MouseEventArgs e)
        {
            if (sender is Infragistics.Win.Misc.UltraPanel up)
            {
                up.Appearance.BackColor = Color.FromArgb(232, 244, 255);
                up.Appearance.BackColor2 = Color.FromArgb(188, 216, 255);
            }
        }

        private void Child_MouseEnter(object sender, EventArgs e)
        {
            if (sender is Control c && c.Parent != null && c.Parent.Parent is Infragistics.Win.Misc.UltraPanel up)
            {
                Panel_MouseEnter(up, e);
            }
        }

        private void Child_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Control c && c.Parent != null && c.Parent.Parent is Infragistics.Win.Misc.UltraPanel up)
            {
                Panel_MouseLeave(up, e);
            }
        }

        private void Child_MouseDown(object sender, MouseEventArgs e)
        {
            if (sender is Control c && c.Parent != null && c.Parent.Parent is Infragistics.Win.Misc.UltraPanel up)
            {
                Panel_MouseDown(up, e);
            }
        }

        private void Child_MouseUp(object sender, MouseEventArgs e)
        {
            if (sender is Control c && c.Parent != null && c.Parent.Parent is Infragistics.Win.Misc.UltraPanel up)
            {
                Panel_MouseUp(up, e);
            }
        }

        private void Child_Click(object sender, EventArgs e)
        {
            if (sender is Control c && c.Parent != null && c.Parent.Parent is Infragistics.Win.Misc.UltraPanel up)
            {
                // Trigger the parent UltraPanel's PerformClick / OnClick if needed
                up.Focus();
            }
        }

        #region Appearance Theme Application

        private void ApplyAppearanceTheme()
        {
            try
            {
                Color bgSkyBlue = Color.FromArgb(226, 239, 255);
                Color peachRequiredBg = Color.FromArgb(255, 224, 192); // #FFE0C0 (255, 224, 192)
                Color readOnlyGrayBg = Color.FromArgb(215, 225, 238);
                Color skyBlueOutline = Color.FromArgb(136, 176, 228); // #88B0E4 (136, 176, 228)
                Color navyText = Color.FromArgb(10, 35, 80);

                this.BackColor = bgSkyBlue;

                // Primary required fields with peach/champagne fill and skyblue outline
                Control[] peachFields = new Control[]
                {
                    txt_barcode, txt_ItemNo, txt_description, txt_ItemType, Txt_UnitCost,
                    txt_walkin, txt_Retail, txt_SF, txt_CEP, txt_Mrp, txt_CardP, txt_MinP
                };

                foreach (Control ctrl in peachFields)
                {
                    if (ctrl == null) continue;
                    if (ctrl is Infragistics.Win.UltraWinEditors.UltraTextEditor ute)
                    {
                        ute.UseAppStyling = false;
                        ute.UseOsThemes = DefaultableBoolean.False;
                        ute.Appearance.BackColor = peachRequiredBg;
                        ute.Appearance.ForeColor = Color.Black;
                        ute.Appearance.BorderColor = skyBlueOutline;
                        ute.BorderStyle = UIElementBorderStyle.Solid;
                    }
                }

                // Read-only quantity fields with soft metallic gray fill and skyblue outline
                Infragistics.Win.UltraWinEditors.UltraTextEditor[] grayQuantityFields = new Infragistics.Win.UltraWinEditors.UltraTextEditor[]
                {
                    txt_qty, txt_available, txt_hold
                };

                foreach (Infragistics.Win.UltraWinEditors.UltraTextEditor ute in grayQuantityFields)
                {
                    if (ute == null) continue;
                    ute.UseAppStyling = false;
                    ute.UseOsThemes = DefaultableBoolean.False;
                    ute.Appearance.BackColor = readOnlyGrayBg;
                    ute.Appearance.ForeColor = Color.Black;
                    ute.Appearance.BorderColor = skyBlueOutline;
                    ute.BorderStyle = UIElementBorderStyle.Solid;
                }

                // Apply button appearance to target panels ultraPanel14..18 and action buttons
                string[] buttonPanelNames = new string[] { "ultraPanel14", "ultraPanel15", "ultraPanel16", "ultraPanel17", "ultraPanel18" };
                foreach (string pName in buttonPanelNames)
                {
                    Control[] found = this.Controls.Find(pName, true);
                    foreach (Control pCtrl in found)
                    {
                        MakeControlActAndLookLikeGlossyButton(pCtrl);
                    }
                }

                // UltraTabControl styling matching image2
                if (ultraTabControl1 != null)
                {
                    ultraTabControl1.UseAppStyling = false;
                    ultraTabControl1.UseOsThemes = DefaultableBoolean.False;
                    ultraTabControl1.Style = Infragistics.Win.UltraWinTabControl.UltraTabControlStyle.Office2007Ribbon;
                    ultraTabControl1.Appearance.BackColor = bgSkyBlue;
                    ultraTabControl1.Appearance.BackColor2 = bgSkyBlue;
                    ultraTabControl1.Appearance.BackGradientStyle = GradientStyle.None;
                    ultraTabControl1.Appearance.BorderColor = skyBlueOutline;

                    ultraTabControl1.ActiveTabAppearance.BackColor = Color.FromArgb(235, 243, 255);
                    ultraTabControl1.ActiveTabAppearance.ForeColor = Color.FromArgb(10, 40, 95);
                    ultraTabControl1.ActiveTabAppearance.BorderColor = skyBlueOutline;
                    ultraTabControl1.ActiveTabAppearance.FontData.Bold = DefaultableBoolean.True;

                    foreach (Infragistics.Win.UltraWinTabControl.UltraTab tab in ultraTabControl1.Tabs)
                    {
                        tab.Appearance.BackColor = Color.FromArgb(185, 212, 248);
                        tab.Appearance.BackColor2 = Color.FromArgb(165, 198, 244);
                        tab.Appearance.BackGradientStyle = GradientStyle.Vertical;
                        tab.Appearance.ForeColor = Color.FromArgb(15, 45, 100);
                        tab.Appearance.BorderColor = skyBlueOutline;

                        if (tab.TabPage != null)
                        {
                            tab.TabPage.BackColor = bgSkyBlue;
                        }
                    }
                }

                ApplyControlThemeRecursive(this, bgSkyBlue, peachRequiredBg, readOnlyGrayBg, skyBlueOutline, navyText);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying appearance theme to frmItemMasterNew: {ex.Message}");
            }
        }

        private void ApplyControlThemeRecursive(Control parent, Color bgSkyBlue, Color peachBg, Color grayBg, Color skyBlueOutline, Color navyText)
        {
            if (parent == null) return;

            foreach (Control c in parent.Controls)
            {
                if (c is Label lbl)
                {
                    if (lbl.Name != null && (lbl.Name.StartsWith("lblFooter_", StringComparison.OrdinalIgnoreCase) ||
                        lbl.Name == "label29" || lbl.Name == "label30" || lbl.Name == "label31" || lbl.Name == "label44"))
                    {
                        lbl.BackColor = Color.Transparent;
                        lbl.ForeColor = navyText;
                        lbl.Font = new Font("Microsoft Sans Serif", lbl.Font.SizeInPoints > 0 ? lbl.Font.SizeInPoints : 9.75F, FontStyle.Regular);
                        continue;
                    }
                    lbl.BackColor = Color.Transparent;
                    lbl.ForeColor = navyText;
                }
                else if (c is CheckBox chk)
                {
                    chk.BackColor = Color.Transparent;
                    chk.ForeColor = navyText;
                }
                else if (c is RadioButton rdo)
                {
                    rdo.BackColor = Color.Transparent;
                    rdo.ForeColor = navyText;
                }
                else if (c is Button btn)
                {
                    MakeControlActAndLookLikeGlossyButton(btn);
                }
                else if (c is Infragistics.Win.Misc.UltraButton ubtn)
                {
                    MakeControlActAndLookLikeGlossyButton(ubtn);
                }
                else if (c is TextBox txt)
                {
                    txt.BorderStyle = BorderStyle.FixedSingle;
                    txt.BackColor = Color.White;
                    txt.ForeColor = Color.Black;
                }
                else if (c is Infragistics.Win.UltraWinEditors.UltraTextEditor ute)
                {
                    ute.UseAppStyling = false;
                    ute.UseOsThemes = DefaultableBoolean.False;
                    ute.BorderStyle = UIElementBorderStyle.Solid;
                    ute.Appearance.BorderColor = skyBlueOutline;

                    if (ute != txt_barcode && ute != txt_ItemNo && ute != txt_description && ute != txt_ItemType && ute != Txt_UnitCost &&
                        ute != txt_walkin && ute != txt_Retail && ute != txt_SF && ute != txt_CEP &&
                        ute != txt_Mrp && ute != txt_CardP && ute != txt_MinP &&
                        ute != txt_qty && ute != txt_available && ute != txt_hold)
                    {
                        ute.Appearance.BackColor = Color.White;
                        ute.Appearance.ForeColor = Color.Black;
                    }
                }
                else if (c is Infragistics.Win.UltraWinEditors.UltraComboEditor uce)
                {
                    uce.UseAppStyling = false;
                    uce.UseOsThemes = DefaultableBoolean.False;
                    uce.BorderStyle = UIElementBorderStyle.Solid;
                    uce.Appearance.BorderColor = skyBlueOutline;
                }
                else if (c is Infragistics.Win.UltraWinGrid.UltraCombo uc)
                {
                    uc.UseAppStyling = false;
                    uc.UseOsThemes = DefaultableBoolean.False;
                    uc.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
                    uc.DisplayLayout.Appearance.BorderColor = skyBlueOutline;
                }
                else if (c is Infragistics.Win.Misc.UltraGroupBox ugb)
                {
                    ugb.UseAppStyling = false;
                    ugb.UseOsThemes = DefaultableBoolean.False;
                    ugb.Appearance.BackColor = Color.Transparent;
                    ugb.Appearance.BackColor2 = Color.Transparent;
                    ugb.Appearance.BackGradientStyle = GradientStyle.None;
                    ugb.Appearance.BorderColor = skyBlueOutline;
                    ugb.BorderStyle = Infragistics.Win.Misc.GroupBoxBorderStyle.RectangularSolid;
                    ugb.HeaderAppearance.ForeColor = navyText;
                    ugb.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                }
                else if (c is GroupBox gb)
                {
                    gb.BackColor = Color.Transparent;
                    gb.ForeColor = navyText;
                }
                else if (c is Infragistics.Win.Misc.UltraPanel up)
                {
                    if (up.Name != null && up.Name.EndsWith("FooterPanel", StringComparison.OrdinalIgnoreCase))
                    {
                        up.Appearance.BorderColor = skyBlueOutline;
                        continue;
                    }
                    if (up.Name != null && (
                        up.Name.Equals("ultraPanel14", StringComparison.OrdinalIgnoreCase) ||
                        up.Name.Equals("ultraPanel15", StringComparison.OrdinalIgnoreCase) ||
                        up.Name.Equals("ultraPanel16", StringComparison.OrdinalIgnoreCase) ||
                        up.Name.Equals("ultraPanel17", StringComparison.OrdinalIgnoreCase) ||
                        up.Name.Equals("ultraPanel18", StringComparison.OrdinalIgnoreCase)))
                    {
                        MakeControlActAndLookLikeGlossyButton(up);
                        continue;
                    }
                    up.UseAppStyling = false;
                    up.UseOsThemes = DefaultableBoolean.False;
                    up.Appearance.BackColor = Color.Transparent;
                    up.Appearance.BorderColor = skyBlueOutline;
                    if (up.ClientArea != null)
                    {
                        ApplyControlThemeRecursive(up.ClientArea, bgSkyBlue, peachBg, grayBg, skyBlueOutline, navyText);
                    }
                }
                else if (c is Infragistics.Win.UltraWinGrid.UltraGrid grid)
                {
                    grid.DisplayLayout.Appearance.BorderColor = skyBlueOutline;
                    if (grid != ultraGrid1 && grid != Ult_Price && grid != ultraGrid2 && grid != ultraGrid3)
                    {
                        grid.UseAppStyling = false;
                        grid.UseOsThemes = DefaultableBoolean.False;
                        grid.DisplayLayout.Appearance.BackColor = bgSkyBlue;
                        grid.DisplayLayout.Override.HeaderAppearance.BackColor = Color.FromArgb(185, 212, 248);
                        grid.DisplayLayout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(155, 190, 240);
                        grid.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
                        grid.DisplayLayout.Override.HeaderAppearance.ForeColor = navyText;
                        grid.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                        grid.DisplayLayout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(245, 250, 255);
                    }
                }

                if (c.HasChildren && !(c is Infragistics.Win.Misc.UltraPanel))
                {
                    ApplyControlThemeRecursive(c, bgSkyBlue, peachBg, grayBg, skyBlueOutline, navyText);
                }
            }
        }

        #endregion

        #region UltraGrid GridReport Theme and Functionality Helper

        private readonly Dictionary<UltraGrid, UltraGridReportThemeHelper> gridThemeHelpers = new Dictionary<UltraGrid, UltraGridReportThemeHelper>();

        private void SetupAllGridsGridReportThemeAndFunctionality()
        {
            UltraGrid[] grids = new UltraGrid[] { ultraGrid1, Ult_Price, ultraGrid2, ultraGrid3 };
            foreach (UltraGrid g in grids)
            {
                if (g != null)
                {
                    if (!gridThemeHelpers.ContainsKey(g))
                    {
                        gridThemeHelpers[g] = new UltraGridReportThemeHelper(g);
                    }
                    gridThemeHelpers[g].ApplyThemeAndFunctionality();
                }
            }

            if (ultraTabControl1 != null)
            {
                ultraTabControl1.SelectedTabChanged -= UltraTabControl1_SelectedTabChanged_FooterSync;
                ultraTabControl1.SelectedTabChanged += UltraTabControl1_SelectedTabChanged_FooterSync;
            }
        }

        private void UltraTabControl1_SelectedTabChanged_FooterSync(object sender, Infragistics.Win.UltraWinTabControl.SelectedTabChangedEventArgs e)
        {
            foreach (var helper in gridThemeHelpers.Values)
            {
                helper.RefreshLayoutAndValues();
            }
        }

        public class UltraGridReportThemeHelper
        {
            private readonly UltraGrid grid;
            private Infragistics.Win.Misc.UltraPanel footerPanel;
            private readonly Dictionary<string, Label> footerLabels = new Dictionary<string, Label>();
            private readonly Dictionary<string, string> columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            private Form columnChooserForm;
            private ListBox columnChooserListBox;
            private bool isDraggingHeaderToHide;
            private UltraGridColumn columnBeingDragged;
            private Point headerDragStartPoint;
            private readonly System.Windows.Forms.ToolTip headerToolTip = new System.Windows.Forms.ToolTip();
            private readonly HashSet<string> userHiddenColumnKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            private static Cursor blackXCursor;

            public UltraGridReportThemeHelper(UltraGrid targetGrid)
            {
                grid = targetGrid;
            }

            public void ApplyThemeAndFunctionality()
            {
                if (grid == null) return;

                if (blackXCursor == null)
                {
                    blackXCursor = CreateBlackXCursor();
                }

                EnsureFooterPanel();

                StyleGridLikeGridReport(grid.DisplayLayout);

                grid.InitializeLayout -= Grid_InitializeLayout;
                grid.InitializeLayout += Grid_InitializeLayout;

                grid.Resize -= Grid_LayoutChanged;
                grid.Resize += Grid_LayoutChanged;

                grid.AfterColPosChanged -= Grid_AfterColPosChanged;
                grid.AfterColPosChanged += Grid_AfterColPosChanged;

                grid.AfterColRegionScroll -= Grid_LayoutChanged;
                grid.AfterColRegionScroll += Grid_LayoutChanged;

                grid.AfterRowRegionScroll -= Grid_LayoutChanged;
                grid.AfterRowRegionScroll += Grid_LayoutChanged;

                grid.Paint -= Grid_LayoutChanged;
                grid.Paint += Grid_LayoutChanged;

                grid.AfterCellUpdate -= Grid_AfterCellUpdate;
                grid.AfterCellUpdate += Grid_AfterCellUpdate;

                grid.AllowDrop = true;
                grid.MouseDown -= Grid_MouseDown;
                grid.MouseDown += Grid_MouseDown;
                grid.MouseMove -= Grid_MouseMove;
                grid.MouseMove += Grid_MouseMove;
                grid.MouseUp -= Grid_MouseUp;
                grid.MouseUp += Grid_MouseUp;
                grid.DragOver -= Grid_DragOver;
                grid.DragOver += Grid_DragOver;
                grid.DragDrop -= Grid_DragDrop;
                grid.DragDrop += Grid_DragDrop;

                ContextMenuStrip headerMenu = new ContextMenuStrip { Font = new Font("Segoe UI", 9F) };
                ToolStripMenuItem chooserItem = new ToolStripMenuItem("📋 Field / Column Chooser...", null, (s, e) => ShowColumnChooserForm());
                chooserItem.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
                headerMenu.Items.Add(chooserItem);

                ToolStripMenuItem showAllItem = new ToolStripMenuItem("🔓 Show / Unhide All Columns", null, (s, e) => UnhideAllColumns());
                headerMenu.Items.Add(showAllItem);

                grid.ContextMenuStrip = headerMenu;

                RefreshLayoutAndValues();
            }

            public void RefreshLayoutAndValues()
            {
                SyncGridAndFooterSize();
                RebuildFooterLabels();
                UpdateFooterCellPositions();
                UpdateFooterValues();
            }

            private static Cursor CreateBlackXCursor()
            {
                try
                {
                    using (Bitmap bmp = new Bitmap(32, 32))
                    {
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            g.Clear(Color.Transparent);

                            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(220, 20, 20, 20)))
                            {
                                g.FillEllipse(bgBrush, 4, 4, 24, 24);
                            }

                            using (Pen borderPen = new Pen(Color.White, 2f))
                            {
                                g.DrawEllipse(borderPen, 4, 4, 24, 24);
                            }

                            using (Pen whitePen = new Pen(Color.White, 3.5f))
                            {
                                whitePen.StartCap = LineCap.Round;
                                whitePen.EndCap = LineCap.Round;
                                g.DrawLine(whitePen, 11, 11, 21, 21);
                                g.DrawLine(whitePen, 21, 11, 11, 21);
                            }

                            IntPtr hIcon = bmp.GetHicon();
                            return new Cursor(hIcon);
                        }
                    }
                }
                catch
                {
                    return Cursors.No;
                }
            }

            private void EnsureFooterPanel()
            {
                if (grid == null || grid.Parent == null) return;

                Control parent = grid.Parent;
                string panelName = grid.Name + "FooterPanel";

                // Destroy existing footer panel to replace with brand new clean ultraPanelGridFooter
                Control[] existing = parent.Controls.Find(panelName, false);
                foreach (Control oldCtrl in existing)
                {
                    parent.Controls.Remove(oldCtrl);
                    oldCtrl.Dispose();
                }

                footerPanel = new Infragistics.Win.Misc.UltraPanel();
                footerPanel.Name = panelName;
                footerPanel.Height = 26;
                footerPanel.UseAppStyling = false;
                footerPanel.UseOsThemes = DefaultableBoolean.False;
                footerPanel.Appearance.BackColor = Color.FromArgb(93, 151, 214);
                footerPanel.Appearance.BackColor2 = Color.FromArgb(93, 151, 214);
                footerPanel.Appearance.BackGradientStyle = GradientStyle.None;
                footerPanel.Appearance.BorderColor = Color.FromArgb(118, 154, 198);
                footerPanel.BorderStyle = UIElementBorderStyle.Solid;

                grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                footerPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

                int parentH = parent.ClientSize.Height > 0 ? parent.ClientSize.Height : parent.Height;
                int availHeight = parentH - grid.Top;
                if (availHeight > 50)
                {
                    grid.Height = availHeight - 26;
                }

                footerPanel.Location = new Point(grid.Left, parentH - 26);
                footerPanel.Width = grid.Width;

                parent.Controls.Add(footerPanel);
                footerPanel.BringToFront();

                parent.Resize -= Parent_Resize;
                parent.Resize += Parent_Resize;
            }

            private void Parent_Resize(object sender, EventArgs e)
            {
                SyncGridAndFooterSize();
            }

            private void SyncGridAndFooterSize()
            {
                if (grid == null || grid.Parent == null || footerPanel == null) return;

                Control parent = grid.Parent;
                int parentH = parent.ClientSize.Height > 0 ? parent.ClientSize.Height : parent.Height;
                int availHeight = parentH - grid.Top;
                if (availHeight > 50)
                {
                    grid.Height = availHeight - 26;
                }

                footerPanel.Location = new Point(grid.Left, parentH - 26);
                footerPanel.Width = grid.Width;
                footerPanel.BringToFront();
                UpdateFooterCellPositions();
            }

            private void Grid_LayoutChanged(object sender, EventArgs e)
            {
                SyncGridAndFooterSize();
            }

            private void Grid_AfterColPosChanged(object sender, AfterColPosChangedEventArgs e)
            {
                RefreshLayoutAndValues();
            }

            private void Grid_AfterCellUpdate(object sender, CellEventArgs e)
            {
                UpdateFooterValues();
            }

            private void Grid_InitializeLayout(object sender, InitializeLayoutEventArgs e)
            {
                StyleGridLikeGridReport(e.Layout);
                ApplyUserHiddenColumns(e.Layout);
                Form topForm = grid.FindForm();
                if (topForm != null && topForm.IsHandleCreated)
                {
                    topForm.BeginInvoke(new Action(() => {
                        RefreshLayoutAndValues();
                    }));
                }
            }

            private void StyleGridLikeGridReport(UltraGridLayout layout)
            {
                if (layout == null) return;

                if (grid != null)
                {
                    grid.UseAppStyling = false;
                    grid.UseOsThemes = DefaultableBoolean.False;
                }

                Color pageBack = Color.FromArgb(226, 239, 255);
                Color gridHeaderBlue = Color.FromArgb(93, 151, 214);
                Color gridHeaderBlueDark = Color.FromArgb(67, 118, 184);
                Color gridSelectedBlue = Color.FromArgb(126, 126, 245);
                Color gridRowLine = Color.FromArgb(197, 217, 241);
                Color gridAltRow = Color.FromArgb(246, 250, 255);

                layout.CaptionVisible = DefaultableBoolean.False;
                layout.BorderStyle = UIElementBorderStyle.Solid;
                layout.GroupByBox.Hidden = true;
                layout.AutoFitStyle = AutoFitStyle.None;

                // Remove standard Infragistics summary footer styling bar
                layout.Override.SummaryFooterCaptionVisible = DefaultableBoolean.False;
                layout.Override.SummaryFooterAppearance.BackColor = gridHeaderBlue;
                layout.Override.SummaryFooterAppearance.ForeColor = Color.White;
                layout.Override.SummaryValueAppearance.BackColor = gridHeaderBlue;
                layout.Override.SummaryValueAppearance.ForeColor = Color.White;

                layout.Override.AllowAddNew = AllowAddNew.No;
                layout.Override.AllowDelete = DefaultableBoolean.False;
                layout.Override.AllowUpdate = DefaultableBoolean.True;
                layout.Override.CellClickAction = CellClickAction.EditAndSelectText;
                layout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
                layout.Override.SelectTypeRow = SelectType.Single;
                layout.Override.RowSelectors = DefaultableBoolean.True;
                layout.Override.AllowRowFiltering = DefaultableBoolean.False;

                layout.Appearance.BackColor = pageBack;
                layout.Appearance.BorderColor = Color.FromArgb(118, 154, 198);

                layout.Override.HeaderStyle = HeaderStyle.Standard;
                layout.Override.HeaderAppearance.BackColor = gridHeaderBlue;
                layout.Override.HeaderAppearance.BackColor2 = gridHeaderBlueDark;
                layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
                layout.Override.HeaderAppearance.ForeColor = Color.White;
                layout.Override.HeaderAppearance.BorderColor = Color.FromArgb(118, 154, 198);
                layout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;
                layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                layout.Override.HeaderAppearance.FontData.SizeInPoints = 9;

                layout.Override.RowAppearance.BackColor = Color.White;
                layout.Override.RowAlternateAppearance.BackColor = gridAltRow;
                layout.Override.RowAppearance.BorderColor = gridRowLine;
                layout.Override.RowAlternateAppearance.BorderColor = gridRowLine;

                layout.Override.ActiveRowAppearance.BackColor = gridSelectedBlue;
                layout.Override.ActiveRowAppearance.ForeColor = Color.White;
                layout.Override.SelectedRowAppearance.BackColor = gridSelectedBlue;
                layout.Override.SelectedRowAppearance.ForeColor = Color.White;

                layout.Override.CellAppearance.BorderColor = gridRowLine;
                layout.Override.CellAppearance.ForeColor = Color.FromArgb(10, 31, 79);
                layout.Override.CellAppearance.FontData.SizeInPoints = 9;

                layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
                layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
                layout.Override.DefaultRowHeight = 26;
                layout.Override.MinRowHeight = 26;

                if (layout.Bands.Count > 0)
                {
                    foreach (UltraGridColumn col in layout.Bands[0].Columns)
                    {
                        col.Header.Appearance.BackColor = gridHeaderBlue;
                        col.Header.Appearance.BackColor2 = gridHeaderBlueDark;
                        col.Header.Appearance.BackGradientStyle = GradientStyle.Vertical;
                        col.Header.Appearance.ForeColor = Color.White;
                        col.Header.Appearance.FontData.Bold = DefaultableBoolean.True;
                        col.CellAppearance.TextVAlign = VAlign.Middle;
                    }
                }
            }

            private void RebuildFooterLabels()
            {
                if (grid == null || footerPanel == null || grid.DisplayLayout.Bands.Count == 0)
                    return;

                UltraGridBand band = grid.DisplayLayout.Bands[0];
                HashSet<string> currentKeys = new HashSet<string>(band.Columns.Cast<UltraGridColumn>().Select(c => c.Key), StringComparer.OrdinalIgnoreCase);

                List<string> toRemove = footerLabels.Keys.Where(k => !currentKeys.Contains(k)).ToList();
                foreach (string key in toRemove)
                {
                    if (footerLabels.TryGetValue(key, out Label lbl))
                    {
                        footerPanel.ClientArea.Controls.Remove(lbl);
                        lbl.Dispose();
                    }
                    footerLabels.Remove(key);
                    columnAggregations.Remove(key);
                }

                foreach (UltraGridColumn column in band.Columns)
                {
                    if (!footerLabels.ContainsKey(column.Key))
                    {
                        Label footerLabel = new Label
                        {
                            Name = "lblFooter_" + column.Key,
                            AutoSize = false,
                            Text = string.Empty,
                            BackColor = Color.Transparent,
                            ForeColor = Color.White,
                            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                            Tag = Tuple.Create(column.Key, string.Empty),
                            ContextMenuStrip = CreateFooterContextMenu(column.Key)
                        };

                        footerLabel.Paint += FooterLabel_Paint;
                        footerLabels[column.Key] = footerLabel;
                        footerPanel.ClientArea.Controls.Add(footerLabel);
                    }

                    if (!columnAggregations.ContainsKey(column.Key))
                    {
                        columnAggregations[column.Key] = "None";
                    }
                }
            }

            private static bool IsNumericOrPriceColumn(UltraGridColumn column)
            {
                if (column == null) return false;

                if (IsSummableColumn(column)) return true;

                string key = (column.Key ?? "").ToLowerInvariant();
                string caption = (column.Header?.Caption ?? "").ToLowerInvariant();

                if (key.Equals("packing", StringComparison.OrdinalIgnoreCase)) return false;

                return key.Contains("cost") || key.Contains("price") || key.Contains("mrp") ||
                       key.Contains("amount") || key.Contains("qty") || key.Contains("total") ||
                       key.Contains("rate") || key.Contains("tax") || key.Contains("discount") ||
                       caption.Contains("cost") || caption.Contains("price") || caption.Contains("mrp") ||
                       caption.Contains("amount") || caption.Contains("qty") || caption.Contains("total");
            }

            private void FooterLabel_Paint(object sender, PaintEventArgs e)
            {
                Label footerLabel = sender as Label;
                if (footerLabel == null) return;

                Tuple<string, string> tagData = footerLabel.Tag as Tuple<string, string>;
                string columnKey = tagData != null ? tagData.Item1 : string.Empty;
                string displayText = tagData != null ? tagData.Item2 : footerLabel.Text;

                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                Color gridHeaderBlue = Color.FromArgb(93, 151, 214);
                Color borderLine = Color.FromArgb(118, 154, 198);

                Rectangle rect = new Rectangle(0, 0, footerLabel.Width, footerLabel.Height);
                using (SolidBrush bgBrush = new SolidBrush(gridHeaderBlue))
                {
                    g.FillRectangle(bgBrush, rect);
                }

                using (Pen borderPen = new Pen(borderLine, 1))
                {
                    g.DrawLine(borderPen, footerLabel.Width - 1, 0, footerLabel.Width - 1, footerLabel.Height);
                    g.DrawLine(borderPen, 0, 0, footerLabel.Width, 0);
                }

                if (string.IsNullOrWhiteSpace(displayText))
                {
                    return;
                }

                if (columnAggregations.ContainsKey(columnKey) &&
                    string.Equals(columnAggregations[columnKey], "None", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                bool isNumeric = IsNumericOrPriceColumn(grid.DisplayLayout.Bands.Count > 0 && grid.DisplayLayout.Bands[0].Columns.Exists(columnKey) ? grid.DisplayLayout.Bands[0].Columns[columnKey] : null);

                TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine;
                if (isNumeric)
                {
                    flags |= TextFormatFlags.Right;
                }
                else
                {
                    flags |= TextFormatFlags.Left;
                }

                Rectangle textRect = new Rectangle(4, 0, Math.Max(0, footerLabel.Width - 8), footerLabel.Height);
                using (Font textFont = new Font("Segoe UI", 9F, FontStyle.Bold))
                {
                    TextRenderer.DrawText(g, displayText, textFont, textRect, Color.White, flags);
                }
            }

            private void UpdateFooterCellPositions()
            {
                if (grid == null || grid.DisplayLayout == null || grid.DisplayLayout.Bands.Count == 0 || footerPanel == null)
                    return;

                UltraGridBand band = grid.DisplayLayout.Bands[0];

                if (footerLabels.Count != band.Columns.Count(c => !c.Hidden))
                {
                    RebuildFooterLabels();
                }

                int rowSelectorWidth = grid.DisplayLayout.Override.RowSelectors == DefaultableBoolean.True ? grid.DisplayLayout.Override.RowSelectorWidth : 0;
                if (rowSelectorWidth <= 0) rowSelectorWidth = 15;

                int scrollOffset = 0;
                if (grid.ActiveColScrollRegion != null)
                {
                    scrollOffset = grid.ActiveColScrollRegion.Position;
                }

                int calculatedX = rowSelectorWidth - scrollOffset;

                foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
                {
                    if (column.Hidden)
                    {
                        if (footerLabels.ContainsKey(column.Key))
                        {
                            footerLabels[column.Key].Visible = false;
                        }
                        continue;
                    }

                    if (!footerLabels.ContainsKey(column.Key))
                    {
                        RebuildFooterLabels();
                    }

                    if (!footerLabels.ContainsKey(column.Key)) continue;

                    Label footerLabel = footerLabels[column.Key];
                    var headerUI = column.Header.GetUIElement();
                    int left, width;

                    if (headerUI != null && headerUI.Rect.Width > 0)
                    {
                        left = headerUI.Rect.Left;
                        width = headerUI.Rect.Width;
                    }
                    else
                    {
                        left = calculatedX;
                        width = column.Width > 0 ? column.Width : 80;
                    }

                    calculatedX += width;

                    footerLabel.Left = left;
                    footerLabel.Width = width;
                    footerLabel.Top = 0;
                    footerLabel.Height = footerPanel.Height;
                    footerLabel.Visible = true;
                    footerLabel.BringToFront();
                    footerLabel.Invalidate();
                }
            }

            private void UpdateFooterValues()
            {
                if (footerLabels.Count == 0 || grid == null)
                    return;

                List<UltraGridRow> visibleRows = GetVisibleDataRows().ToList();
                foreach (KeyValuePair<string, Label> footerEntry in footerLabels)
                {
                    string columnKey = footerEntry.Key;
                    Label footerLabel = footerEntry.Value;

                    if (!columnAggregations.ContainsKey(columnKey) ||
                        string.Equals(columnAggregations[columnKey], "None", StringComparison.OrdinalIgnoreCase))
                    {
                        footerLabel.Text = string.Empty;
                        footerLabel.Tag = Tuple.Create(columnKey, string.Empty);
                        footerLabel.Invalidate();
                        continue;
                    }

                    object result = CalculateAggregation(columnKey, columnAggregations[columnKey], visibleRows);
                    string displayValue = FormatAggregationResult(columnKey, columnAggregations[columnKey], result);

                    footerLabel.Text = displayValue;
                    footerLabel.Tag = Tuple.Create(columnKey, displayValue);
                    footerLabel.ForeColor = Color.White;
                    footerLabel.Invalidate();
                }
            }

            private IEnumerable<UltraGridRow> GetVisibleDataRows()
            {
                if (grid == null || grid.Rows == null) yield break;
                foreach (UltraGridRow row in grid.Rows)
                {
                    if (row != null && row.IsDataRow && !row.IsFilteredOut)
                        yield return row;
                }
            }

            private object CalculateAggregation(string columnKey, string aggregation, List<UltraGridRow> visibleRows)
            {
                if (visibleRows == null || visibleRows.Count == 0)
                    return aggregation == "Count" ? (object)0 : null;

                switch (aggregation)
                {
                    case "Sum":
                        return visibleRows
                            .Where(row => row.Cells.Exists(columnKey))
                            .Select(row => GetNumericValue(row.Cells[columnKey].Value))
                            .Where(value => value.HasValue)
                            .Sum(value => value.Value);
                    case "Min":
                        return visibleRows
                            .Where(row => row.Cells.Exists(columnKey))
                            .Select(row => row.Cells[columnKey].Value)
                            .Where(HasCellValue)
                            .Cast<IComparable>()
                            .OrderBy(value => value)
                            .FirstOrDefault();
                    case "Max":
                        return visibleRows
                            .Where(row => row.Cells.Exists(columnKey))
                            .Select(row => row.Cells[columnKey].Value)
                            .Where(HasCellValue)
                            .Cast<IComparable>()
                            .OrderByDescending(value => value)
                            .FirstOrDefault();
                    case "Count":
                        return visibleRows.Count(row => row.Cells.Exists(columnKey) && HasCellValue(row.Cells[columnKey].Value));
                    case "Avg":
                        List<decimal> values = visibleRows
                            .Where(row => row.Cells.Exists(columnKey))
                            .Select(row => GetNumericValue(row.Cells[columnKey].Value))
                            .Where(value => value.HasValue)
                            .Select(value => value.Value)
                            .ToList();
                        return values.Count == 0 ? 0m : values.Average();
                    default:
                        return null;
                }
            }

            private string FormatAggregationResult(string columnKey, string aggregation, object result)
            {
                if (result == null)
                    return string.Empty;

                if (aggregation == "Count")
                    return Convert.ToString(result);

                if (grid.DisplayLayout != null &&
                    grid.DisplayLayout.Bands.Count > 0 &&
                    grid.DisplayLayout.Bands[0].Columns.Exists(columnKey))
                {
                    UltraGridColumn column = grid.DisplayLayout.Bands[0].Columns[columnKey];
                    decimal? numericValue = GetNumericValue(result);
                    if (numericValue.HasValue)
                    {
                        if (!string.IsNullOrWhiteSpace(column.Format))
                            return numericValue.Value.ToString(column.Format);

                        return numericValue.Value.ToString("N2");
                    }
                }

                return Convert.ToString(result);
            }

            private ContextMenuStrip CreateFooterContextMenu(string columnKey)
            {
                ContextMenuStrip menu = new ContextMenuStrip();
                menu.Tag = columnKey;

                bool isNumeric = IsNumericOrPriceColumn(grid.DisplayLayout.Bands.Count > 0 && grid.DisplayLayout.Bands[0].Columns.Exists(columnKey) ? grid.DisplayLayout.Bands[0].Columns[columnKey] : null);

                ToolStripMenuItem itemSum = new ToolStripMenuItem("Sum");
                itemSum.Tag = "Sum";
                itemSum.Enabled = isNumeric;
                itemSum.Click += FooterContextMenu_Click;

                ToolStripMenuItem itemMin = new ToolStripMenuItem("Min");
                itemMin.Tag = "Min";
                itemMin.Click += FooterContextMenu_Click;

                ToolStripMenuItem itemMax = new ToolStripMenuItem("Max");
                itemMax.Tag = "Max";
                itemMax.Click += FooterContextMenu_Click;

                ToolStripMenuItem itemCount = new ToolStripMenuItem("Count");
                itemCount.Tag = "Count";
                itemCount.Click += FooterContextMenu_Click;

                ToolStripMenuItem itemAverage = new ToolStripMenuItem("Average");
                itemAverage.Tag = "Avg";
                itemAverage.Enabled = isNumeric;
                itemAverage.Click += FooterContextMenu_Click;

                ToolStripMenuItem itemNone = new ToolStripMenuItem("None");
                itemNone.Tag = "None";
                itemNone.Click += FooterContextMenu_Click;

                menu.Items.Add(itemSum);
                menu.Items.Add(itemMin);
                menu.Items.Add(itemMax);
                menu.Items.Add(itemCount);
                menu.Items.Add(itemAverage);
                menu.Items.Add(new ToolStripSeparator());
                menu.Items.Add(itemNone);

                menu.Opening += (sender, e) =>
                {
                    string currentAggregation = columnAggregations.ContainsKey(columnKey)
                        ? columnAggregations[columnKey]
                        : "None";

                    foreach (ToolStripItem menuItem in menu.Items)
                    {
                        ToolStripMenuItem toolStripMenuItem = menuItem as ToolStripMenuItem;
                        if (toolStripMenuItem != null && toolStripMenuItem.Tag != null)
                        {
                            toolStripMenuItem.Checked = string.Equals(toolStripMenuItem.Tag.ToString(), currentAggregation, StringComparison.OrdinalIgnoreCase);
                        }
                    }
                };

                return menu;
            }

            private void FooterContextMenu_Click(object sender, EventArgs e)
            {
                ToolStripMenuItem item = sender as ToolStripMenuItem;
                if (item == null)
                    return;

                ContextMenuStrip menu = item.Owner as ContextMenuStrip;
                if (menu == null || menu.Tag == null || item.Tag == null)
                    return;

                string columnKey = menu.Tag.ToString();
                string aggregation = item.Tag.ToString();

                columnAggregations[columnKey] = aggregation;
                UpdateFooterValues();
            }

            private void ApplyUserHiddenColumns(UltraGridLayout layout = null)
            {
                UltraGridLayout targetLayout = layout ?? grid?.DisplayLayout;
                if (targetLayout == null || targetLayout.Bands.Count == 0) return;

                UltraGridBand band = targetLayout.Bands[0];
                foreach (UltraGridColumn col in band.Columns)
                {
                    if (userHiddenColumnKeys.Contains(col.Key))
                    {
                        col.Hidden = true;
                    }
                }
            }

            private void Grid_MouseDown(object sender, MouseEventArgs e)
            {
                if (grid == null || grid.DisplayLayout == null || grid.DisplayLayout.Bands.Count == 0)
                    return;

                UIElement element = grid.DisplayLayout.UIElement?.ElementFromPoint(new Point(e.X, e.Y));
                HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

                UltraGridColumn col = headerUI?.Header?.Column;
                if (headerUI != null && col != null)
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        ShowHeaderContextMenu(col, e.Location);
                        return;
                    }

                    if (e.Button == MouseButtons.Left)
                    {
                        isDraggingHeaderToHide = true;
                        columnBeingDragged = col;
                        headerDragStartPoint = new Point(e.X, e.Y);
                    }
                }
                else if (e.Button == MouseButtons.Right)
                {
                    CellUIElement cellUI = element as CellUIElement ?? element?.GetAncestor(typeof(CellUIElement)) as CellUIElement;
                    UltraGridColumn cellCol = cellUI?.Cell?.Column;
                    if (cellCol != null)
                    {
                        ShowCellContextMenu(cellCol, e.Location);
                    }
                }
            }

            private void Grid_MouseMove(object sender, MouseEventArgs e)
            {
                if (!isDraggingHeaderToHide || columnBeingDragged == null || e.Button != MouseButtons.Left)
                    return;

                int deltaX = Math.Abs(e.X - headerDragStartPoint.X);
                int deltaY = e.Y - headerDragStartPoint.Y;

                if (deltaY > 25 && deltaY > deltaX)
                {
                    grid.Cursor = blackXCursor;
                    string colName = !string.IsNullOrEmpty(columnBeingDragged.Header.Caption) ? columnBeingDragged.Header.Caption : columnBeingDragged.Key;
                    headerToolTip.SetToolTip(grid, $"✖ Drag down to hide '{colName}' column");

                    if (deltaY > 50)
                    {
                        HideColumn(columnBeingDragged);
                        isDraggingHeaderToHide = false;
                        columnBeingDragged = null;
                        grid.Cursor = Cursors.Default;
                        headerToolTip.SetToolTip(grid, string.Empty);
                    }
                }
            }

            private void Grid_MouseUp(object sender, MouseEventArgs e)
            {
                if (isDraggingHeaderToHide)
                {
                    if (columnBeingDragged != null && (e.Y - headerDragStartPoint.Y) > 40)
                    {
                        HideColumn(columnBeingDragged);
                    }
                    isDraggingHeaderToHide = false;
                    columnBeingDragged = null;
                    grid.Cursor = Cursors.Default;
                    headerToolTip.SetToolTip(grid, string.Empty);
                }
            }

            private void Grid_DragOver(object sender, DragEventArgs e)
            {
                if (e.Data.GetDataPresent(typeof(ColumnChooserItem)))
                {
                    e.Effect = DragDropEffects.Move;
                }
            }

            private void Grid_DragDrop(object sender, DragEventArgs e)
            {
                if (e.Data.GetData(typeof(ColumnChooserItem)) is ColumnChooserItem item)
                {
                    Point clientPt = grid.PointToClient(new Point(e.X, e.Y));
                    int dropPosition = GetTargetColumnPositionFromPoint(clientPt);
                    UnhideColumn(item.ColumnKey, dropPosition);
                }
            }

            private int GetTargetColumnPositionFromPoint(Point pt)
            {
                if (grid == null || grid.DisplayLayout == null || grid.DisplayLayout.Bands.Count == 0)
                    return 0;

                UIElement element = grid.DisplayLayout.UIElement?.ElementFromPoint(pt);
                HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

                if (headerUI != null && headerUI.Header?.Column != null)
                {
                    return headerUI.Header.Column.Header.VisiblePosition;
                }

                UltraGridBand band = grid.DisplayLayout.Bands[0];
                foreach (UltraGridColumn col in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
                {
                    if (!col.Hidden)
                    {
                        UIElement hUI = col.Header.GetUIElement();
                        if (hUI != null && pt.X >= hUI.Rect.Left && pt.X <= hUI.Rect.Right)
                        {
                            return col.Header.VisiblePosition;
                        }
                    }
                }

                return band.Columns.Count;
            }

            private void HideColumn(UltraGridColumn col)
            {
                if (col == null) return;
                userHiddenColumnKeys.Add(col.Key);
                col.Hidden = true;
                UpdateFooterCellPositions();
                UpdateFooterValues();
                if (columnChooserForm != null && columnChooserForm.Visible)
                {
                    PopulateColumnChooserListBox();
                }
            }

            private void ShowHeaderContextMenu(UltraGridColumn col, Point location)
            {
                if (col == null) return;
                ContextMenuStrip menu = new ContextMenuStrip { Font = new Font("Segoe UI", 9F) };
                string colName = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;

                bool isPinned = col.Header.Fixed;
                ToolStripMenuItem pinItem = new ToolStripMenuItem(
                    isPinned ? $"🔓 Unpin '{colName}' Column" : $"📌 Pin / Lock '{colName}' Column",
                    null,
                    (s, e) => ToggleColumnPin(col)
                )
                {
                    Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold)
                };
                menu.Items.Add(pinItem);

                ToolStripMenuItem hideItem = new ToolStripMenuItem($"🙈 Hide Column '{colName}'", null, (s, e) => HideColumn(col));
                menu.Items.Add(hideItem);

                menu.Items.Add(new ToolStripSeparator());

                ToolStripMenuItem chooserItem = new ToolStripMenuItem("📋 Field / Column Chooser...", null, (s, e) => ShowColumnChooserForm());
                menu.Items.Add(chooserItem);

                ToolStripMenuItem showAllItem = new ToolStripMenuItem("🔓 Show / Unhide All Columns", null, (s, e) => UnhideAllColumns());
                menu.Items.Add(showAllItem);

                menu.Show(grid, location);
            }

            private void ShowCellContextMenu(UltraGridColumn col, Point location)
            {
                if (col == null) return;
                ContextMenuStrip menu = new ContextMenuStrip { Font = new Font("Segoe UI", 9F) };
                string colName = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;

                bool isPinned = col.Header.Fixed;
                ToolStripMenuItem pinItem = new ToolStripMenuItem(
                    isPinned ? $"🔓 Unpin '{colName}' Column" : $"📌 Pin / Lock '{colName}' Column",
                    null,
                    (s, e) => ToggleColumnPin(col)
                )
                {
                    Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold)
                };
                menu.Items.Add(pinItem);

                menu.Show(grid, location);
            }

            private void ToggleColumnPin(UltraGridColumn col)
            {
                if (col == null) return;
                bool isPinned = col.Header.Fixed;
                col.Header.Fixed = !isPinned;

                string cleanTitle = (col.Header.Caption ?? col.Key).Replace("📌 ", "").Trim();
                if (!isPinned)
                {
                    col.Header.Caption = "📌 " + cleanTitle;
                }
                else
                {
                    col.Header.Caption = cleanTitle;
                }
            }

            private void ShowColumnChooserForm()
            {
                if (columnChooserForm == null || columnChooserForm.IsDisposed)
                {
                    CreateColumnChooserForm();
                }

                PopulateColumnChooserListBox();
                Form parentForm = grid.FindForm();
                if (parentForm != null)
                {
                    columnChooserForm.Show(parentForm);
                    PositionColumnChooser();
                }
                else
                {
                    columnChooserForm.Show();
                }
            }

            private void CreateColumnChooserForm()
            {
                columnChooserForm = new Form
                {
                    Text = "Customization (Field Chooser)",
                    Size = new Size(240, 300),
                    FormBorderStyle = FormBorderStyle.FixedSingle,
                    StartPosition = FormStartPosition.Manual,
                    TopMost = true,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    BackColor = Color.FromArgb(240, 244, 248),
                    ShowIcon = false,
                    ShowInTaskbar = false
                };

                columnChooserForm.FormClosing += (s, e) =>
                {
                    e.Cancel = true;
                    columnChooserForm.Hide();
                };

                columnChooserListBox = new ListBox
                {
                    Dock = DockStyle.Fill,
                    AllowDrop = true,
                    DrawMode = DrawMode.OwnerDrawFixed,
                    BorderStyle = BorderStyle.None,
                    BackColor = Color.FromArgb(240, 244, 248),
                    ItemHeight = 34,
                    IntegralHeight = false
                };

                columnChooserListBox.DrawItem += ColumnChooserListBox_DrawItem;
                columnChooserListBox.DoubleClick += ColumnChooserListBox_DoubleClick;
                columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;

                columnChooserForm.Controls.Add(columnChooserListBox);
            }

            private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left && columnChooserListBox != null)
                {
                    int index = columnChooserListBox.IndexFromPoint(e.Location);
                    if (index >= 0 && index < columnChooserListBox.Items.Count)
                    {
                        if (columnChooserListBox.Items[index] is ColumnChooserItem item)
                        {
                            columnChooserListBox.DoDragDrop(item, DragDropEffects.Move);
                        }
                    }
                }
            }

            private void PopulateColumnChooserListBox()
            {
                if (columnChooserListBox == null || grid == null || grid.DisplayLayout.Bands.Count == 0)
                    return;

                columnChooserListBox.Items.Clear();
                UltraGridBand band = grid.DisplayLayout.Bands[0];

                foreach (UltraGridColumn col in band.Columns)
                {
                    if (col.Hidden && !col.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                    {
                        string caption = !string.IsNullOrEmpty(col.Header.Caption) ? col.Header.Caption : col.Key;
                        columnChooserListBox.Items.Add(new ColumnChooserItem(col.Key, caption));
                    }
                }
            }

            private void ColumnChooserListBox_DoubleClick(object sender, EventArgs e)
            {
                if (columnChooserListBox.SelectedItem is ColumnChooserItem item)
                {
                    UnhideColumn(item.ColumnKey);
                }
            }

            private void UnhideColumn(string columnKey, int? targetVisiblePosition = null)
            {
                userHiddenColumnKeys.Remove(columnKey);
                if (grid != null && grid.DisplayLayout.Bands.Count > 0 && grid.DisplayLayout.Bands[0].Columns.Exists(columnKey))
                {
                    UltraGridColumn col = grid.DisplayLayout.Bands[0].Columns[columnKey];
                    col.Hidden = false;
                    if (targetVisiblePosition.HasValue)
                    {
                        col.Header.VisiblePosition = targetVisiblePosition.Value;
                    }
                    UpdateFooterCellPositions();
                    UpdateFooterValues();
                    PopulateColumnChooserListBox();
                }
            }

            private void UnhideAllColumns()
            {
                userHiddenColumnKeys.Clear();
                if (grid == null || grid.DisplayLayout.Bands.Count == 0) return;
                UltraGridBand band = grid.DisplayLayout.Bands[0];
                foreach (UltraGridColumn col in band.Columns)
                {
                    if (!col.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                    {
                        col.Hidden = false;
                    }
                }
                UpdateFooterCellPositions();
                UpdateFooterValues();
                PopulateColumnChooserListBox();
            }

            private void PositionColumnChooser()
            {
                Form parentForm = grid.FindForm();
                if (columnChooserForm != null && !columnChooserForm.IsDisposed && columnChooserForm.Visible && parentForm != null)
                {
                    columnChooserForm.Location = new Point(
                        parentForm.Right - columnChooserForm.Width - 30,
                        parentForm.Bottom - columnChooserForm.Height - 30);
                    columnChooserForm.BringToFront();
                }
            }

            private void ColumnChooserListBox_DrawItem(object sender, DrawItemEventArgs e)
            {
                if (e.Index < 0 || columnChooserListBox == null || e.Index >= columnChooserListBox.Items.Count)
                    return;

                if (!(columnChooserListBox.Items[e.Index] is ColumnChooserItem item))
                    return;

                Rectangle rect = e.Bounds;
                rect.Inflate(-4, -3);

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(0, 121, 211)))
                using (GraphicsPath path = RoundedRect(rect, 4))
                {
                    e.Graphics.FillPath(bgBrush, path);
                }

                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    StringFormat sf = new StringFormat
                    {
                        LineAlignment = StringAlignment.Center,
                        Alignment = StringAlignment.Center
                    };
                    using (Font textFont = new Font("Segoe UI", 9F, FontStyle.Bold))
                    {
                        e.Graphics.DrawString(item.DisplayText, textFont, textBrush, rect, sf);
                    }
                }
            }

            private static bool HasCellValue(object value)
            {
                return value != null &&
                       value != DBNull.Value &&
                       !string.IsNullOrWhiteSpace(Convert.ToString(value));
            }

            private static decimal? GetNumericValue(object value)
            {
                if (value == null || value == DBNull.Value)
                    return null;

                string str = Convert.ToString(value).Trim();
                if (string.IsNullOrEmpty(str)) return null;

                str = str.Replace("$", "").Replace("Rs", "").Replace("PKR", "").Replace(",", "").Trim();

                if (decimal.TryParse(str, out decimal result))
                    return result;

                return null;
            }

            private static bool IsSummableColumn(UltraGridColumn column)
            {
                if (column == null || column.DataType == null)
                    return false;

                Type type = System.Nullable.GetUnderlyingType(column.DataType) ?? column.DataType;
                return type == typeof(decimal) ||
                       type == typeof(double) ||
                       type == typeof(float) ||
                       type == typeof(int) ||
                       type == typeof(long) ||
                       type == typeof(short) ||
                       type == typeof(byte);
            }

            private sealed class ColumnChooserItem
            {
                public string ColumnKey { get; }
                public string DisplayText { get; }

                public ColumnChooserItem(string key, string text)
                {
                    ColumnKey = key;
                    DisplayText = text;
                }

                public override string ToString()
                {
                    return DisplayText;
                }
            }

            private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
            {
                int diameter = radius * 2;
                Size size = new Size(diameter, diameter);
                Rectangle arc = new Rectangle(bounds.Location, size);
                GraphicsPath path = new GraphicsPath();

                if (radius == 0)
                {
                    path.AddRectangle(bounds);
                    return path;
                }

                path.AddArc(arc, 180, 90);
                arc.X = bounds.Right - diameter;
                path.AddArc(arc, 270, 90);
                arc.Y = bounds.Bottom - diameter;
                path.AddArc(arc, 0, 90);
                arc.X = bounds.Left;
                path.AddArc(arc, 90, 90);
                path.CloseFigure();
                return path;
            }
        }

        #endregion

        #endregion

        private void ultraTabControl1_SelectedTabChanged(object sender, Infragistics.Win.UltraWinTabControl.SelectedTabChangedEventArgs e)
        {

        }
    }
}

