using System;

namespace ModelClass.Settings
{
    /// <summary>
    /// Represents a POS system setting stored per company/branch
    /// </summary>
    public class POSSetting
    {
        /// <summary>
        /// Primary key for the setting
        /// </summary>
        public int SettingId { get; set; }

        /// <summary>
        /// Company ID this setting belongs to
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// Branch ID this setting belongs to
        /// </summary>
        public int BranchId { get; set; }

        /// <summary>
        /// Unique key for the setting (e.g., "DuplicateItemBehavior")
        /// </summary>
        public string SettingKey { get; set; }

        /// <summary>
        /// Value of the setting as string
        /// </summary>
        public string SettingValue { get; set; }

        /// <summary>
        /// Data type of the setting: Boolean, String, Integer
        /// </summary>
        public string SettingType { get; set; }

        /// <summary>
        /// Human-readable description of the setting
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Category for grouping: Sales, Inventory, Printing, Display
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// When the setting was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// When the setting was last modified
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        #region Helper Methods

        /// <summary>
        /// Gets the setting value as boolean
        /// </summary>
        public bool GetBoolValue(bool defaultValue = false)
        {
            if (string.IsNullOrEmpty(SettingValue))
                return defaultValue;

            string trimmed = SettingValue.Trim().ToLowerInvariant();
            if (trimmed == "true" || trimmed == "1" || trimmed == "yes" || trimmed == "y")
                return true;
            if (trimmed == "false" || trimmed == "0" || trimmed == "no" || trimmed == "n")
                return false;

            if (bool.TryParse(SettingValue, out bool result))
                return result;

            return defaultValue;
        }

        /// <summary>
        /// Gets the setting value as integer
        /// </summary>
        public int GetIntValue(int defaultValue = 0)
        {
            if (string.IsNullOrEmpty(SettingValue))
                return defaultValue;

            if (int.TryParse(SettingValue.Trim(), out int result))
                return result;

            return defaultValue;
        }

        /// <summary>
        /// Gets the setting value as string
        /// </summary>
        public string GetStringValue(string defaultValue = "")
        {
            return string.IsNullOrEmpty(SettingValue) ? defaultValue : SettingValue.Trim();
        }

        #endregion
    }

    /// <summary>
    /// Known setting keys for type-safe access
    /// </summary>
    public static class SettingKeys
    {
        // Sales Settings
        public const string DuplicateItemBehavior = "DuplicateItemBehavior";
        public const string AllowNegativeStock = "AllowNegativeStock";
        public const string AutoPrintAfterSave = "AutoPrintAfterSave";
        public const string DefaultSaleType = "DefaultSaleType";
        public const string DefaultCreditDays = "DefaultCreditDays";
        public const string DefaultPriceLevel = "DefaultPriceLevel";
        public const string RoundingMethod = "RoundingMethod";
        public const string MaxDiscountPercent = "MaxDiscountPercent";
        public const string AllowSaleBelowCost = "AllowSaleBelowCost";

        // Display Settings
        public const string ShowCostToUser = "ShowCostToUser";
        public const string ShowMarginColumn = "ShowMarginColumn";

        // Printing Settings
        public const string PrinterName = "PrinterName";
        public const string ReceiptFormat = "ReceiptFormat";
        public const string PrintCopies = "PrintCopies";
    }

    /// <summary>
    /// Possible values for DuplicateItemBehavior setting
    /// </summary>
    public static class DuplicateItemBehaviorValues
    {
        public const string MergeQuantity = "MergeQuantity";
        public const string SeparateRows = "SeparateRows";
    }

    /// <summary>
    /// Possible values for RoundingMethod setting
    /// </summary>
    public static class RoundingMethodValues
    {
        public const string None = "None";
        public const string Nearest5 = "Nearest5";
        public const string Nearest10 = "Nearest10";
        public const string Up = "Up";
        public const string Down = "Down";
    }

    /// <summary>
    /// Possible values for DefaultSaleType setting
    /// </summary>
    public static class DefaultSaleTypeValues
    {
        public const string Cash = "Cash";
        public const string Credit = "Credit";
    }
}
