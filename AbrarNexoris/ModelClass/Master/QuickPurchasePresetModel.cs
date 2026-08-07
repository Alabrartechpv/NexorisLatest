using System.Collections.Generic;

namespace ModelClass.Master
{
    /// <summary>
    /// Represents a named quick purchase preset (a saved group of items with an optional vendor).
    /// </summary>
    public class QuickPurchasePreset
    {
        public int PresetId { get; set; }
        public string PresetName { get; set; }
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public List<QuickPurchasePresetItem> Items { get; set; } = new List<QuickPurchasePresetItem>();
    }

    /// <summary>
    /// A single item line stored inside a quick purchase preset.
    /// </summary>
    public class QuickPurchasePresetItem
    {
        public int PresetItemId { get; set; }
        public int PresetId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string Barcode { get; set; }
        public string Unit { get; set; }
        public int UnitId { get; set; }
        public double UnitPrice { get; set; }
        public double Cost { get; set; }
        public int Quantity { get; set; }
    }
}
