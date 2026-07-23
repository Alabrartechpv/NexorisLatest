using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinGrid;
using Infragistics.Win.UltraWinEditors;
using ModelClass;
using ModelClass.Master;
using ModelClass.Report;
using ModelClass.TransactionModels;
using PosBranch_Win.DialogBox;
using Repository;
using Repository.MasterRepositry;
using Repository.TransactionRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PosBranch_Win.Transaction
{
    public partial class frmPurchaseOrder : Form
    {
        private static readonly Color FormBackColor = Color.FromArgb(214, 230, 240);
        private static readonly Color PanelBackColor = Color.FromArgb(214, 229, 241);
        private static readonly Color BorderBlue = Color.FromArgb(118, 154, 198);
        private static readonly Color ControlBackColor = Color.White;
        private static readonly Color WarmControlBackColor = Color.FromArgb(255, 229, 198);
        private static readonly Color SoftReadonlyBackColor = Color.FromArgb(233, 231, 214);
        private static readonly Color ControlTextColor = Color.FromArgb(18, 49, 102);
        private static readonly Color GridHeaderBlue = Color.FromArgb(93, 151, 214);
        private static readonly Color GridHeaderBlueDark = Color.FromArgb(67, 118, 184);
        private static readonly Color GridSelectedBlue = Color.FromArgb(126, 126, 245);
        private static readonly Color GridRowLine = Color.FromArgb(197, 217, 241);
        private static readonly Color GridAltRow = Color.FromArgb(246, 250, 255);
        private static readonly Color GridFooterBorder = Color.FromArgb(144, 181, 223);
        private static readonly Color ActionPanelBackColor = Color.FromArgb(206, 223, 238);
        private static readonly Color SkyBlueOutline = Color.FromArgb(160, 210, 255);
        private static readonly Color ButtonBlueTop = Color.FromArgb(232, 241, 252);
        private static readonly Color ButtonBlueBottom = Color.FromArgb(145, 181, 224);
        private static readonly Color ButtonBlueBorder = Color.FromArgb(62, 104, 166);
        private static readonly Color ButtonTextBlue = Color.FromArgb(14, 47, 108);
        private static readonly Color PanelHoverTopColor = Color.FromArgb(245, 250, 255);
        private static readonly Color PanelHoverBottomColor = Color.FromArgb(170, 206, 244);
        private static readonly Color PanelPressedTopColor = Color.FromArgb(205, 226, 248);
        private static readonly Color PanelPressedBottomColor = Color.FromArgb(128, 170, 224);
        private readonly Dictionary<string, Label> _footerLabels = new Dictionary<string, Label>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly List<SmartReorderItemModel> _pendingSmartReorderItems = new List<SmartReorderItemModel>();
        private readonly ItemMasterRepository _itemMasterRepository = new ItemMasterRepository();
        private PurchaseOrderRepository _purchaseOrderRepository;
        private bool _gridInitialized;
        private int _currentPurchaseOrderId;
        private Button btnExportGrid;

        private enum GridExportFormat
        {
            Excel,
            Pdf
        }

        public frmPurchaseOrder()
        {
            InitializeComponent();
            Load += frmPurchaseOrder_Load;
            gridReport.InitializeLayout += gridReport_InitializeLayout;
            gridReport.Resize += gridReport_Resize;
            gridReport.AfterCellUpdate += gridReport_AfterCellUpdate;
            gridReport.DoubleClickCell += gridReport_DoubleClickCell;
            txtBarcode.KeyDown += txtBarcode_KeyDown;
        }

        private void frmPurchaseOrder_Load(object sender, EventArgs e)
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            Text = "Purchase Order";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1200, 720);

            ApplyTheme();
            InitializeDefaults();
            SetupGrid();
            _gridInitialized = true;
            LoadNextDocumentNumber();
            ApplyPendingSmartReorderItems();
        }

        public void LoadSmartReorderItems(IEnumerable<SmartReorderItemModel> items)
        {
            _pendingSmartReorderItems.Clear();

            if (items != null)
            {
                _pendingSmartReorderItems.AddRange(
                    items.Where(x => x != null && x.FinalQuantity > 0)
                         .Select(CloneSmartReorderItem));
            }

            if (_gridInitialized)
            {
                ApplyPendingSmartReorderItems();
            }
        }

        private void ApplyTheme()
        {
            BackColor = FormBackColor;
            ultraPanelMain.Appearance.BackColor = FormBackColor;
            ultraPanelMain.Appearance.BackColor2 = FormBackColor;
            ultraPanelMain.Appearance.BackGradientStyle = GradientStyle.None;

            ultraPanelHeader.Appearance.BackColor = PanelBackColor;
            ultraPanelHeader.Appearance.BorderColor = BorderBlue;
            ultraPanelHeader.BorderStyle = UIElementBorderStyle.Solid;

            ultraPanelGridFooter.Appearance.BackColor = GridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackColor2 = GridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackGradientStyle = GradientStyle.None;
            ultraPanelGridFooter.Appearance.BorderColor = GridFooterBorder;
            ultraPanelGridFooter.BorderStyle = UIElementBorderStyle.Solid;

            grpDocumentHeader.BackColor = PanelBackColor;
            grpDocumentHeader.ForeColor = Color.Black;

            StyleTextEditor(txtAccount, true);
            StyleTextEditor(txtNavigator, false, null, SoftReadonlyBackColor);
            StyleTextEditor(txtDocNo, false);
            StyleDateEditor(dtpDate);
            StyleTextEditor(txtReference, false);
            StyleTextEditor(txtShipTo1, false);
            StyleTextEditor(txtShipTo2, false);
            StyleTextEditor(txtShipTo3, false);
            StyleTextEditor(txtShipTo4, false);
            StyleTextEditor(txtRemark, false);
            StyleDateEditor(dtpExpectedDate);
            StyleTextEditor(txtTelephone, false);
            StyleTextEditor(txtOrderBy, false);
            StyleTextEditor(txtBarcode, true);
            StyleTextEditor(txtSubtotal, false, null, SoftReadonlyBackColor);
            StyleTextEditor(txtPurchaseTax, false, null, SoftReadonlyBackColor);
            StyleTextEditor(txtDisc, false);
            StyleTextEditor(txtRounding, false, null, SoftReadonlyBackColor);
            StyleTextEditor(txtTotal, false, Color.Black);

            txtRemark.Multiline = true;
            //txtRemark.ScrollBars = ScrollBars.Vertical;
            txtNavigator.ReadOnly = true;
            txtDocNo.ReadOnly = true;
            txtSubtotal.ReadOnly = true;
            txtPurchaseTax.ReadOnly = true;
            txtDisc.ReadOnly = true;
            txtRounding.ReadOnly = true;
            txtTotal.ReadOnly = true;


            txtTotal.BackColor = Color.Black;
            txtTotal.ForeColor = Color.Yellow;
            txtTotal.Font = new Font("Tahoma", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtSubtotal.Appearance.TextHAlign = HAlign.Center;
            txtDisc.Appearance.TextHAlign = HAlign.Center;
            txtPurchaseTax.Appearance.TextHAlign = HAlign.Center;
            txtRounding.Appearance.TextHAlign = HAlign.Center;
            txtTotal.Appearance.TextHAlign = HAlign.Center;


            Button shipToLookupButton = grpDocumentHeader.Controls["btnShipToLookup"] as Button;
            if (shipToLookupButton != null)
            {
                StyleButton(shipToLookupButton, "...");
            }

            ultraCheckEditorRound.Appearance.ForeColor = Color.Black;
            ultraCheckEditorRound.BackColor = FormBackColor;
            ultraCheckEditorRound.Checked = true;
            ultraCheckEditorRound.Text = "Activate Total Rounding";

            foreach (Control control in grpDocumentHeader.Controls)
            {
                if (control is Label label)
                {
                    label.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
                    label.ForeColor = Color.Black;
                    label.BackColor = Color.Transparent;
                    label.TextAlign = ContentAlignment.MiddleRight;
                }
            }

            foreach (Control control in ultraPanelHeader.ClientArea.Controls)
            {
                if (control is Label label)
                {
                    label.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
                    label.ForeColor = Color.Black;
                    label.BackColor = Color.Transparent;
                }
            }

            lblSubtotal.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblPurchaseTax.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblDisc.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblRounding.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblTotal.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);

            InitializePanelButtons();
            EnsureExportGridButton();
        }

        private void InitializeDefaults()
        {
            dtpDate.Value = DateTime.Today;

            txtAccount.Text = string.Empty;
            txtNavigator.Text = string.Empty;
            txtDocNo.Text = string.Empty;
            txtReference.Text = string.Empty;
            txtShipTo1.Text = string.Empty;
            txtShipTo2.Text = string.Empty;
            txtShipTo3.Text = string.Empty;
            txtShipTo4.Text = string.Empty;
            txtTelephone.Text = string.Empty;
            txtOrderBy.Text = GetUserName();
            txtBarcode.Text = string.Empty;
            txtRemark.Text = string.Empty;
            dtpExpectedDate.Value = DateTime.Today;

            txtSubtotal.Text = "0.00";
            txtPurchaseTax.Text = "0.00";
            txtDisc.Text = "0.00";
            txtRounding.Text = "0.00";
            txtTotal.Text = "0.00";
        }

        private void SetupGrid()
        {
            gridReport.DisplayLayout.Reset();
            gridReport.UseAppStyling = false;
            gridReport.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = gridReport.DisplayLayout;
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.BorderStyle = UIElementBorderStyle.Solid;
            layout.GroupByBox.Hidden = false;
            layout.GroupByBox.BandLabelAppearance.BackColor = GridHeaderBlueDark;
            layout.GroupByBox.BandLabelAppearance.ForeColor = Color.White;
            layout.GroupByBox.BandLabelAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.GroupByBox.PromptAppearance.BackColor = GridHeaderBlue;
            layout.GroupByBox.PromptAppearance.BackColor2 = GridHeaderBlueDark;
            layout.GroupByBox.PromptAppearance.BackGradientStyle = GradientStyle.Horizontal;
            layout.GroupByBox.PromptAppearance.ForeColor = Color.White;
            layout.GroupByBox.Prompt = "Drag a column header here to group by that column";
            layout.GroupByBox.Appearance.BackColor = Color.FromArgb(109, 167, 226);
            layout.GroupByBox.Appearance.BackColor2 = Color.FromArgb(69, 125, 190);
            layout.GroupByBox.Appearance.BackGradientStyle = GradientStyle.Vertical;

            layout.Override.AllowAddNew = AllowAddNew.No;
            layout.Override.AllowDelete = DefaultableBoolean.False;
            layout.Override.AllowUpdate = DefaultableBoolean.True;
            layout.Override.CellClickAction = CellClickAction.EditAndSelectText;
            layout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            layout.Override.SelectTypeRow = SelectType.Single;
            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorWidth = 20;
            layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;

            layout.Appearance.BackColor = Color.White;
            layout.Appearance.BorderColor = BorderBlue;
            layout.Appearance.BackColor2 = FormBackColor;
            layout.Appearance.BackGradientStyle = GradientStyle.None;
            layout.Override.RowSelectorAppearance.BackColor = GridHeaderBlueDark;
            layout.Override.RowSelectorAppearance.BackColor2 = GridHeaderBlue;
            layout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.RowSelectorAppearance.BorderColor = BorderBlue;
            layout.Override.RowSelectorAppearance.ForeColor = Color.White;
            layout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.RowSelectorAppearance.TextHAlign = HAlign.Center;
            layout.ScrollBounds = ScrollBounds.ScrollToFill;
            layout.ScrollStyle = ScrollStyle.Immediate;

            layout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            layout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor = Color.White;
            layout.Override.HeaderAppearance.BorderColor = BorderBlue;
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
            layout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;

            layout.Override.RowAppearance.BackColor = Color.White;
            layout.Override.RowAlternateAppearance.BackColor = GridAltRow;
            layout.Override.RowAppearance.BorderColor = GridRowLine;
            layout.Override.RowAlternateAppearance.BorderColor = GridRowLine;
            layout.Override.ActiveRowAppearance.BackColor = GridSelectedBlue;
            layout.Override.ActiveRowAppearance.ForeColor = Color.White;
            layout.Override.ActiveRowAppearance.BorderColor = BorderBlue;
            layout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
            layout.Override.SelectedRowAppearance.ForeColor = Color.White;
            layout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.False;
            layout.Override.CellAppearance.BorderColor = GridRowLine;
            layout.Override.CellAppearance.ForeColor = Color.FromArgb(10, 31, 79);
            layout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.CellAppearance.FontData.SizeInPoints = 8.25F;
            layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            layout.Override.MinRowHeight = 19;
            layout.Override.DefaultRowHeight = 19;
            layout.RowConnectorStyle = RowConnectorStyle.Solid;
            layout.RowConnectorColor = GridRowLine;
            layout.ScrollBarLook.Appearance.BackColor = ActionPanelBackColor;
            layout.ScrollBarLook.Appearance.BorderColor = BorderBlue;
            layout.ScrollBarLook.TrackAppearance.BackColor = Color.FromArgb(225, 236, 246);
            layout.ScrollBarLook.ButtonAppearance.BackColor = GridHeaderBlue;
            layout.ScrollBarLook.ButtonAppearance.BackColor2 = GridHeaderBlueDark;
            layout.ScrollBarLook.ButtonAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.ScrollBarLook.ButtonAppearance.BorderColor = BorderBlue;
            gridReport.BackColor = FormBackColor;
            gridReport.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);

            DataTable table = new DataTable();
            table.Columns.Add("No", typeof(int));
            table.Columns.Add("ItemId", typeof(int));
            table.Columns.Add("UnitId", typeof(int));
            table.Columns.Add("Barcode", typeof(string));
            table.Columns.Add("ItemNo", typeof(string));
            table.Columns.Add("Description");
            table.Columns.Add("UOM");
            table.Columns.Add("Qty", typeof(decimal));
            table.Columns.Add("Cost", typeof(decimal));
            table.Columns.Add("UPrice", typeof(decimal));
            table.Columns.Add("Amount", typeof(decimal));
            table.Columns.Add("Remark");
            table.Columns.Add("Packing", typeof(decimal));
            table.Columns.Add("BaseUnit", typeof(string));

            gridReport.DataSource = table;
            InitializeGridFooter();
        }

        private void ApplyPendingSmartReorderItems()
        {
            if (!_gridInitialized || _pendingSmartReorderItems.Count == 0)
                return;

            DataTable table = gridReport.DataSource as DataTable;
            if (table == null)
                return;

            foreach (SmartReorderItemModel item in _pendingSmartReorderItems)
            {
                AddOrMergeSmartReorderItem(table, item);
            }

            RenumberGridRows(table);
            UpdateFooterValues();
            RecalculateTotals();
            gridReport.Refresh();
            _pendingSmartReorderItems.Clear();
        }

        private static SmartReorderItemModel CloneSmartReorderItem(SmartReorderItemModel item)
        {
            return new SmartReorderItemModel
            {
                ItemId = item.ItemId,
                UnitId = item.UnitId,
                ItemName = item.ItemName,
                Barcode = item.Barcode,
                Unit = item.Unit,
                CurrentStock = item.CurrentStock,
                SuggestedQuantity = item.SuggestedQuantity,
                FinalQuantity = item.FinalQuantity
            };
        }

        private sealed class SmartReorderPurchaseDefaults
        {
            public decimal Cost { get; set; }
            public decimal UnitCost { get; set; }
            public decimal TaxPercent { get; set; }
            public decimal Packing { get; set; } = 1m;
            public string BaseUnit { get; set; } = "Y";
        }

        private SmartReorderPurchaseDefaults GetSmartReorderPurchaseDefaults(SmartReorderItemModel item)
        {
            SmartReorderPurchaseDefaults defaults = new SmartReorderPurchaseDefaults();
            if (item == null || item.ItemId <= 0)
            {
                return defaults;
            }

            try
            {
                List<ItemMasterPriceSettings> priceSettings = _itemMasterRepository.GetItemPriceSettings(item.ItemId);
                if (priceSettings == null || priceSettings.Count == 0)
                {
                    return defaults;
                }

                ItemMasterPriceSettings match = null;

                if (item.UnitId > 0)
                {
                    match = priceSettings.FirstOrDefault(x => x != null && x.UnitId == item.UnitId);
                }

                if (match == null && !string.IsNullOrWhiteSpace(item.Unit))
                {
                    match = priceSettings.FirstOrDefault(x =>
                        x != null &&
                        string.Equals((x.Unit ?? string.Empty).Trim(), item.Unit.Trim(), StringComparison.OrdinalIgnoreCase));
                }

                if (match == null && !string.IsNullOrWhiteSpace(item.Barcode))
                {
                    match = priceSettings.FirstOrDefault(x =>
                        x != null &&
                        string.Equals((x.BarCode ?? string.Empty).Trim(), item.Barcode.Trim(), StringComparison.OrdinalIgnoreCase));
                }

                if (match == null)
                {
                    match = priceSettings.FirstOrDefault(x =>
                        x != null &&
                        string.Equals(NormalizeBaseUnitValue(x.IsBaseUnit), "Y", StringComparison.OrdinalIgnoreCase));
                }

                if (match == null)
                {
                    match = priceSettings.FirstOrDefault(x => x != null);
                }

                if (match != null)
                {
                    defaults.UnitCost = Convert.ToDecimal(match.Cost);
                    defaults.Cost = GetLatestPurchaseItemUnitCost(item.ItemId, match.UnitId, match.Unit, defaults.UnitCost);
                    defaults.TaxPercent = Convert.ToDecimal(match.TaxPer);
                    defaults.Packing = Convert.ToDecimal(match.Packing <= 0 ? 1d : match.Packing);
                    defaults.BaseUnit = NormalizeBaseUnitValue(match.IsBaseUnit);
                }
            }
            catch
            {
                // Keep safe defaults if price-settings lookup fails.
            }

            return defaults;
        }

        private void AddOrMergeSmartReorderItem(DataTable table, SmartReorderItemModel item)
        {
            decimal quantity = item.FinalQuantity > 0 ? item.FinalQuantity : item.SuggestedQuantity;
            if (quantity <= 0)
                return;

            string itemNo = !string.IsNullOrWhiteSpace(item.Barcode)
                ? item.Barcode.Trim()
                : item.ItemId.ToString();
            string description = string.IsNullOrWhiteSpace(item.ItemName) ? itemNo : item.ItemName.Trim();
            string uom = string.IsNullOrWhiteSpace(item.Unit) ? string.Empty : item.Unit.Trim();
            SmartReorderPurchaseDefaults defaults = GetSmartReorderPurchaseDefaults(item);
            decimal packing = defaults.Packing > 0 ? defaults.Packing : 1m;
            decimal amount = Math.Round(quantity * defaults.Cost, 2);

            DataRow existingRow = null;
            foreach (DataRow row in table.Rows)
            {
                if (string.Equals(Convert.ToString(row["ItemNo"]), itemNo, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(Convert.ToString(row["UOM"]), uom, StringComparison.OrdinalIgnoreCase))
                {
                    existingRow = row;
                    break;
                }
            }

            if (existingRow == null)
            {
                DataRow newRow = table.NewRow();
                newRow["No"] = table.Rows.Count + 1;
                newRow["ItemId"] = item.ItemId;
                newRow["UnitId"] = item.UnitId;
                newRow["Barcode"] = item.Barcode ?? string.Empty;
                newRow["ItemNo"] = itemNo;
                newRow["Description"] = description;
                newRow["UOM"] = uom;
                newRow["Qty"] = quantity;
                newRow["Cost"] = defaults.Cost;
                newRow["UPrice"] = defaults.UnitCost;
                newRow["Amount"] = amount;
                newRow["Remark"] = "Smart Reorder";
                newRow["Packing"] = packing;
                newRow["BaseUnit"] = defaults.BaseUnit;
                table.Rows.Add(newRow);
                return;
            }

            decimal updatedQuantity = GetDecimalValue(existingRow, "Qty") + quantity;

            existingRow["ItemId"] = item.ItemId;
            existingRow["UnitId"] = item.UnitId;
            existingRow["Barcode"] = item.Barcode ?? string.Empty;
            existingRow["Description"] = description;
            existingRow["Qty"] = updatedQuantity;
            existingRow["Cost"] = defaults.Cost;
            existingRow["UPrice"] = defaults.UnitCost;
            existingRow["Amount"] = Math.Round(updatedQuantity * defaults.Cost, 2);
            existingRow["Packing"] = packing;
            existingRow["BaseUnit"] = defaults.BaseUnit;

            if (string.IsNullOrWhiteSpace(Convert.ToString(existingRow["Remark"])))
            {
                existingRow["Remark"] = "Smart Reorder";
            }
        }

        private static decimal GetDecimalValue(DataRow row, string columnName)
        {
            decimal value;
            return decimal.TryParse(Convert.ToString(row[columnName]), out value) ? value : 0m;
        }

        private static string NormalizeBaseUnitValue(string value)
        {
            return string.Equals((value ?? string.Empty).Trim(), "Y", StringComparison.OrdinalIgnoreCase) ? "Y" : "N";
        }

        private static void RenumberGridRows(DataTable table)
        {
            int rowNumber = 1;
            foreach (DataRow row in table.Rows)
            {
                row["No"] = rowNumber++;
            }
        }

        private void gridReport_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0)
                return;

            UltraGridBand band = e.Layout.Bands[0];

            foreach (UltraGridColumn column in band.Columns)
            {
                column.Hidden = true;
            }

            ConfigureGridColumn(band, "No", "No", 48, "0", HAlign.Center, 0);
            ConfigureGridColumn(band, "ItemNo", "Item No", 92, null, HAlign.Left, 1);
            ConfigureGridColumn(band, "Description", "Description", 185, null, HAlign.Left, 2);
            ConfigureGridColumn(band, "UOM", "UOM", 58, null, HAlign.Center, 3);
            ConfigureGridColumn(band, "Qty", "Qty", 60, "#,##0.##", HAlign.Right, 4);
            ConfigureGridColumn(band, "Cost", "Cost", 76, "#,##0.00", HAlign.Right, 5);
            ConfigureGridColumn(band, "UPrice", "U/Cost", 76, "#,##0.00", HAlign.Right, 6);
            ConfigureGridColumn(band, "Amount", "Amount", 88, "#,##0.00", HAlign.Right, 7);
            ConfigureGridColumn(band, "Remark", "Remark", 108, null, HAlign.Left, 8);

            e.Layout.AutoFitStyle = AutoFitStyle.None;
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
            RecalculateTotals();
        }

        private void ConfigureGridColumn(UltraGridBand band, string key, string header, int width, string format, HAlign align, int visiblePosition)
        {
            if (!band.Columns.Exists(key))
                return;

            UltraGridColumn column = band.Columns[key];
            column.Hidden = false;
            column.Header.Caption = header;
            column.Width = width;
            column.Header.VisiblePosition = visiblePosition;
            column.Header.Appearance.BorderColor = GridRowLine;
            column.CellAppearance.BorderColor = GridRowLine;
            column.CellAppearance.TextHAlign = align;
            column.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            column.CellAppearance.FontData.SizeInPoints = 8.25F;

            if (!string.IsNullOrWhiteSpace(format))
            {
                column.Format = format;
            }
        }

        private void InitializeGridFooter()
        {
            ultraPanelGridFooter.ClientArea.Controls.Clear();
            _footerLabels.Clear();
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
        }

        private void CreateFooterCells()
        {
            ultraPanelGridFooter.ClientArea.Controls.Clear();
            _footerLabels.Clear();

            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            int xOffset = gridReport.DisplayLayout.Override.RowSelectorWidth;

            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden)
                    continue;

                Label footerLabel = new Label();
                footerLabel.Name = "footer_" + column.Key;
                footerLabel.Text = string.Empty;
                footerLabel.TextAlign = ContentAlignment.MiddleCenter;
                footerLabel.BackColor = GridHeaderBlue;
                footerLabel.BorderStyle = BorderStyle.None;
                footerLabel.AutoSize = false;
                footerLabel.Width = column.Width;
                footerLabel.Height = Math.Max(ultraPanelGridFooter.Height - 2, 20);
                footerLabel.Left = xOffset;
                footerLabel.Top = 1;
                footerLabel.Tag = Tuple.Create(column.Key, string.Empty);
                footerLabel.ForeColor = Color.White;
                footerLabel.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
                footerLabel.Paint += FooterLabel_Paint;
                footerLabel.ContextMenuStrip = CreateFooterContextMenu(column.Key);

                ultraPanelGridFooter.ClientArea.Controls.Add(footerLabel);
                _footerLabels[column.Key] = footerLabel;

                if (!_columnAggregations.ContainsKey(column.Key))
                {
                    _columnAggregations[column.Key] = "None";
                }

                xOffset += column.Width;
            }
        }

        private ContextMenuStrip CreateFooterContextMenu(string columnKey)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Tag = columnKey;

            bool isNumeric = gridReport.DisplayLayout.Bands.Count > 0 &&
                             gridReport.DisplayLayout.Bands[0].Columns.Exists(columnKey) &&
                             IsSummableColumn(gridReport.DisplayLayout.Bands[0].Columns[columnKey]);

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
                string currentAggregation = _columnAggregations.ContainsKey(columnKey)
                    ? _columnAggregations[columnKey]
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

            _columnAggregations[columnKey] = aggregation;
            UpdateFooterValues();
        }

        private void UpdateFooterCellPositions()
        {
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0 || _footerLabels.Count == 0)
                return;

            int xOffset = gridReport.DisplayLayout.Override.RowSelectorWidth;
            foreach (UltraGridColumn column in gridReport.DisplayLayout.Bands[0].Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden || !_footerLabels.ContainsKey(column.Key))
                    continue;

                Label footerLabel = _footerLabels[column.Key];
                footerLabel.Left = xOffset;
                footerLabel.Width = column.Width;
                footerLabel.Height = Math.Max(ultraPanelGridFooter.Height - 2, 20);
                xOffset += column.Width;
            }
        }

        private void UpdateFooterValues()
        {
            if (_footerLabels.Count == 0)
                return;

            List<UltraGridRow> visibleRows = GetVisibleDataRows().ToList();
            foreach (KeyValuePair<string, Label> footerEntry in _footerLabels)
            {
                string columnKey = footerEntry.Key;
                Label footerLabel = footerEntry.Value;

                if (!_columnAggregations.ContainsKey(columnKey) ||
                    string.Equals(_columnAggregations[columnKey], "None", StringComparison.OrdinalIgnoreCase))
                {
                    footerLabel.Text = string.Empty;
                    footerLabel.Tag = Tuple.Create(columnKey, string.Empty);
                    footerLabel.Invalidate();
                    continue;
                }

                object result = CalculateAggregation(columnKey, _columnAggregations[columnKey], visibleRows);
                string displayValue = FormatAggregationResult(columnKey, _columnAggregations[columnKey], result);

                footerLabel.Text = displayValue;
                footerLabel.Tag = Tuple.Create(columnKey, displayValue);
                footerLabel.ForeColor = Color.White;
                footerLabel.Invalidate();
            }
        }

        private object CalculateAggregation(string columnKey, string aggregation, List<UltraGridRow> visibleRows)
        {
            if (visibleRows == null || visibleRows.Count == 0)
            {
                return aggregation == "Count" ? (object)0 : null;
            }

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

        private void gridReport_Resize(object sender, EventArgs e)
        {
            UpdateFooterCellPositions();
        }

        private void gridReport_AfterCellUpdate(object sender, Infragistics.Win.UltraWinGrid.CellEventArgs e)
        {
            if (e == null || e.Cell == null || e.Cell.Row == null)
                return;

            string columnKey = e.Cell.Column != null ? e.Cell.Column.Key : string.Empty;
            if (columnKey == "Qty" || columnKey == "Cost")
            {
                RecalculateRow(e.Cell.Row);
            }

            UpdateFooterValues();
            RecalculateTotals();
        }

        private string FormatAggregationResult(string columnKey, string aggregation, object result)
        {
            if (result == null)
                return string.Empty;

            if (aggregation == "Count")
                return Convert.ToString(result);

            if (gridReport.DisplayLayout != null &&
                gridReport.DisplayLayout.Bands.Count > 0 &&
                gridReport.DisplayLayout.Bands[0].Columns.Exists(columnKey))
            {
                UltraGridColumn column = gridReport.DisplayLayout.Bands[0].Columns[columnKey];
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

        private IEnumerable<UltraGridRow> GetVisibleDataRows()
        {
            foreach (UltraGridRow row in gridReport.Rows)
            {
                if (row != null && row.IsDataRow && !row.IsFilteredOut)
                {
                    yield return row;
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

            decimal result;
            return decimal.TryParse(Convert.ToString(value), out result) ? result : (decimal?)null;
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

        private void FooterLabel_Paint(object sender, PaintEventArgs e)
        {
            Label footerLabel = sender as Label;
            if (footerLabel == null)
                return;

            Tuple<string, string> tagData = footerLabel.Tag as Tuple<string, string>;
            string columnKey = tagData != null ? tagData.Item1 : string.Empty;
            string displayText = tagData != null ? tagData.Item2 : footerLabel.Text;

            if (string.IsNullOrWhiteSpace(displayText))
                return;

            if (_columnAggregations.ContainsKey(columnKey) &&
                string.Equals(_columnAggregations[columnKey], "None", StringComparison.OrdinalIgnoreCase))
                return;

            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            SizeF textSize = graphics.MeasureString(displayText, footerLabel.Font);
            int padding = 6;
            int cornerRadius = 6;
            int margin = 1;
            int boxWidth = footerLabel.Width - (margin * 2);
            int boxHeight = (int)textSize.Height + padding;
            int x = margin;
            int y = (footerLabel.Height - boxHeight) / 2;

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
                    graphics.FillPath(brush, path);
                }
            }

            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                float textX = x + (boxWidth - textSize.Width) / 2;
                float textY = y + (boxHeight - textSize.Height) / 2 - 1;
                graphics.DrawString(displayText, footerLabel.Font, textBrush, textX, textY);
            }

            footerLabel.Text = string.Empty;
        }

        private static void StyleTextEditor(Infragistics.Win.UltraWinEditors.UltraTextEditor editor, bool warmBackColor, Color? foreColorOverride = null, Color? backColorOverride = null)
        {
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.Appearance.BackColor = backColorOverride ?? (warmBackColor ? WarmControlBackColor : ControlBackColor);
            editor.Appearance.BorderColor = BorderBlue;
            editor.Appearance.ForeColor = foreColorOverride ?? ControlTextColor;
            editor.Appearance.FontData.Name = "Tahoma";
            editor.Appearance.FontData.SizeInPoints = 9F;
        }

        private static void StyleDateEditor(Infragistics.Win.UltraWinEditors.UltraDateTimeEditor editor)
        {
            editor.UseAppStyling = false;
            editor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.Appearance.BackColor = ControlBackColor;
            editor.Appearance.BorderColor = SkyBlueOutline;
            editor.Appearance.ForeColor = ControlTextColor;
            editor.Appearance.FontData.Name = "Tahoma";
            editor.Appearance.FontData.SizeInPoints = 10F;
            editor.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
            editor.MaskInput = "{date}";
            editor.FormatString = "dd/MM/yyyy";
        }

        private static void StyleCombo(Infragistics.Win.UltraWinEditors.UltraComboEditor combo)
        {
            combo.UseAppStyling = false;
            combo.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            combo.BorderStyle = UIElementBorderStyle.Solid;
            combo.UseOsThemes = DefaultableBoolean.False;
            combo.Appearance.BackColor = ControlBackColor;
            combo.Appearance.BorderColor = SkyBlueOutline;
            combo.Appearance.ForeColor = ControlTextColor;
            combo.Appearance.FontData.Name = "Tahoma";
            combo.Appearance.FontData.SizeInPoints = 10F;
            combo.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
            combo.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            combo.AutoCompleteMode = Infragistics.Win.AutoCompleteMode.SuggestAppend;
        }

        private static void StyleButton(Button button, string text)
        {
            button.Text = text;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = ButtonBlueBorder;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(169, 197, 230);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(126, 166, 214);
            button.BackColor = Color.FromArgb(155, 188, 224);
            button.ForeColor = ButtonTextBlue;
            button.Font = new Font("Tahoma", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
        }

        private void InitializePanelButtons()
        {
            RegisterPanelButton(ultraPanel1);
            RegisterPanelButton(ultraPanel2);
            RegisterPanelButton(ultraPanel3);
            RegisterPanelButton(ultraPanel4);
            RegisterPanelButton(ultraPanel5);
            RegisterPanelButton(ultraPanel6);
            RegisterPanelButton(ultraPanel7);
            RegisterPanelButton(ultraPanel8);
            RegisterPanelButton(ultraPanel9);
            RegisterPanelButton(ultraPanel10);
            RegisterPanelButton(ultraPanel11);
            RegisterPanelButton(ultraPanel12);
            RegisterPanelButton(ultraPanel13);
            RegisterPanelButton(ultraPanel14);
        }

        private void RegisterPanelButton(UltraPanel panel)
        {
            if (panel == null)
                return;

            panel.UseAppStyling = false;
            panel.Cursor = Cursors.Hand;
            panel.BorderStyle = UIElementBorderStyle.Rounded1;
            ApplyPanelButtonStyle(panel, false, false);

            panel.Click += PanelButton_Click;
            panel.MouseEnter += PanelButton_MouseEnter;
            panel.MouseLeave += PanelButton_MouseLeave;
            panel.MouseDown += PanelButton_MouseDown;
            panel.MouseUp += PanelButton_MouseUp;

            panel.ClientArea.Click += PanelButton_Click;
            panel.ClientArea.MouseEnter += PanelButton_MouseEnter;
            panel.ClientArea.MouseLeave += PanelButton_MouseLeave;
            panel.ClientArea.MouseDown += PanelButton_MouseDown;
            panel.ClientArea.MouseUp += PanelButton_MouseUp;

            foreach (Control child in panel.ClientArea.Controls)
            {
                child.Cursor = Cursors.Hand;
                child.Click += PanelButton_Click;
                child.MouseEnter += PanelButton_MouseEnter;
                child.MouseLeave += PanelButton_MouseLeave;
                child.MouseDown += PanelButton_MouseDown;
                child.MouseUp += PanelButton_MouseUp;
            }
        }

        private static void ApplyPanelButtonStyle(UltraPanel panel, bool isHover, bool isPressed)
        {
            Infragistics.Win.AppearanceBase appearance = panel.Appearance;
            appearance.BackGradientStyle = GradientStyle.Vertical;
            appearance.BorderColor = ButtonBlueBorder;
            appearance.ForeColor = ButtonTextBlue;

            if (isPressed)
            {
                appearance.BackColor = PanelPressedTopColor;
                appearance.BackColor2 = PanelPressedBottomColor;
            }
            else if (isHover)
            {
                appearance.BackColor = PanelHoverTopColor;
                appearance.BackColor2 = PanelHoverBottomColor;
            }
            else
            {
                appearance.BackColor = ButtonBlueTop;
                appearance.BackColor2 = ButtonBlueBottom;
            }
        }

        private UltraPanel GetActionPanel(object sender)
        {
            Control control = sender as Control;
            while (control != null)
            {
                UltraPanel panel = control as UltraPanel;
                if (panel != null)
                    return panel;

                control = control.Parent;
            }

            return null;
        }

        private void PanelButton_MouseEnter(object sender, EventArgs e)
        {
            UltraPanel panel = GetActionPanel(sender);
            if (panel != null)
            {
                ApplyPanelButtonStyle(panel, true, false);
            }
        }

        private void PanelButton_MouseLeave(object sender, EventArgs e)
        {
            UltraPanel panel = GetActionPanel(sender);
            if (panel != null)
            {
                Point clientPoint = panel.PointToClient(Control.MousePosition);
                bool isInside = panel.ClientRectangle.Contains(clientPoint);
                ApplyPanelButtonStyle(panel, isInside, false);
            }
        }

        private void PanelButton_MouseDown(object sender, MouseEventArgs e)
        {
            UltraPanel panel = GetActionPanel(sender);
            if (panel != null && e.Button == MouseButtons.Left)
            {
                ApplyPanelButtonStyle(panel, true, true);
            }
        }

        private void PanelButton_MouseUp(object sender, MouseEventArgs e)
        {
            UltraPanel panel = GetActionPanel(sender);
            if (panel != null)
            {
                Point clientPoint = panel.PointToClient(Control.MousePosition);
                bool isInside = panel.ClientRectangle.Contains(clientPoint);
                ApplyPanelButtonStyle(panel, isInside, false);
            }
        }

        private void PanelButton_Click(object sender, EventArgs e)
        {
            UltraPanel panel = GetActionPanel(sender);
            if (panel != null)
            {
                panel.Focus();

                if (panel == ultraPanel1)
                {
                    OpenVendorDialog();
                }
                else if (panel == ultraPanel14)
                {
                    OpenPurchaseOrderDialog();
                }
                else if (panel == ultraPanel12)
                {
                    OpenItemDialog();
                }
            }
        }

        private void EnsureExportGridButton()
        {
            if (btnExportGrid != null)
                return;

            btnExportGrid = new Button
            {
                Name = "btnExportGrid",
                Text = "Export Grid",
                Size = new Size(92, 27),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btnExportGrid.Location = new Point(384, 232);
            btnExportGrid.Click += btnExportGrid_Click;
            StyleButton(btnExportGrid, "Export Grid");
            ultraPanelHeader.ClientArea.Controls.Add(btnExportGrid);
            btnExportGrid.BringToFront();
        }

        private void btnExportGrid_Click(object sender, EventArgs e)
        {
            ExportGrid();
        }

        private void ExportGrid()
        {
            if (gridReport == null || gridReport.DisplayLayout.Bands.Count == 0 || !GetVisibleDataRows().Any())
            {
                MessageBox.Show("There is no grid data to export.", "Export Grid",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            GridExportFormat? exportFormat = AskGridExportFormat();
            if (!exportFormat.HasValue)
                return;

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                string documentNo = GetSafeFileName(txtDocNo.Text);
                if (string.IsNullOrWhiteSpace(documentNo))
                    documentNo = "New";

                if (exportFormat.Value == GridExportFormat.Excel)
                {
                    dialog.Filter = "Excel CSV files (*.csv)|*.csv";
                    dialog.FileName = string.Format("PurchaseOrder_{0}_{1:yyyyMMdd_HHmmss}.csv", documentNo, DateTime.Now);
                }
                else
                {
                    dialog.Filter = "PDF files (*.pdf)|*.pdf";
                    dialog.FileName = string.Format("PurchaseOrder_{0}_{1:yyyyMMdd_HHmmss}.pdf", documentNo, DateTime.Now);
                }

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                Cursor previousCursor = Cursor;
                Cursor = Cursors.WaitCursor;

                try
                {
                    if (exportFormat.Value == GridExportFormat.Excel)
                    {
                        ExportGridToExcel(dialog.FileName);
                    }
                    else
                    {
                        ExportGridToPdf(dialog.FileName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Grid export failed.\n\n" + ex.Message, "Export Grid",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor = previousCursor;
                }
            }
        }

        private GridExportFormat? AskGridExportFormat()
        {
            DialogResult result = MessageBox.Show(
                "Export grid as Excel?\n\nYes = Excel\nNo = PDF\nCancel = Stop",
                "Export Grid",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
                return GridExportFormat.Excel;

            if (result == DialogResult.No)
                return GridExportFormat.Pdf;

            return null;
        }

        private void ExportGridToExcel(string filePath)
        {
            List<UltraGridColumn> columns = GetExportColumns();
            List<UltraGridRow> rows = GetVisibleDataRows().ToList();

            // Write CSV with UTF-8 BOM so Excel opens it correctly with proper encoding
            using (StreamWriter writer = new StreamWriter(filePath, false, new System.Text.UTF8Encoding(true)))
            {
                // Info header rows
                writer.WriteLine(CsvQuote("Purchase Order Details"));
                writer.WriteLine(CsvQuote("Doc No: " + GetControlText(txtDocNo) +
                    "    Date: " + GetEditorDateText(dtpDate) +
                    "    Vendor: " + GetControlText(txtNavigator)));
                writer.WriteLine(CsvQuote("Expected Receive Date: " + GetEditorDateText(dtpExpectedDate) +
                    "    Reference: " + GetControlText(txtReference)));
                writer.WriteLine(); // blank line

                // Column header row
                writer.WriteLine(string.Join(",", columns.Select(c => CsvQuote(c.Header.Caption))));

                // Data rows
                foreach (UltraGridRow row in rows)
                {
                    var cells = columns.Select(col =>
                    {
                        object val = row.Cells.Exists(col.Key) ? row.Cells[col.Key].Value : null;
                        return CsvQuote(FormatGridExportValue(val, col));
                    });
                    writer.WriteLine(string.Join(",", cells));
                }
            }

            // Open the file in Excel automatically
            try { System.Diagnostics.Process.Start(filePath); } catch { }

            MessageBox.Show("Grid exported to Excel successfully.", "Export Grid",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static string CsvQuote(string value)
        {
            if (value == null) value = string.Empty;
            // Wrap in quotes; escape any existing quotes by doubling them
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private void ExportGridToPdf(string filePath)
        {
            if (!PrinterSettings.InstalledPrinters.Cast<string>().Any(x => string.Equals(x, "Microsoft Print to PDF", StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Microsoft Print to PDF printer is not installed.");
            }

            UltraGridPrintDocument printDocument = new UltraGridPrintDocument();
            ConfigureGridPrintDocument(printDocument);
            printDocument.PrinterSettings.PrinterName = "Microsoft Print to PDF";
            printDocument.PrinterSettings.PrintToFile = true;
            printDocument.PrinterSettings.PrintFileName = filePath;
            printDocument.Print();

            MessageBox.Show("Grid exported to PDF successfully.", "Export Grid",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ConfigureGridPrintDocument(UltraGridPrintDocument printDocument)
        {
            printDocument.Grid = gridReport;
            printDocument.DefaultPageSettings.Landscape = true;
            printDocument.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(35, 35, 55, 45);
            printDocument.Header.TextCenter =
                "Purchase Order Details" + Environment.NewLine +
                "Doc No: " + GetControlText(txtDocNo) + "    Vendor: " + GetControlText(txtNavigator);
            printDocument.Header.TextRight = "Exported: " + DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt");
            printDocument.Header.Appearance.FontData.Bold = DefaultableBoolean.True;
            printDocument.Header.Appearance.FontData.SizeInPoints = 11;
            printDocument.Header.Appearance.TextHAlign = HAlign.Center;
            printDocument.Footer.TextCenter = "Page [Page #]";
            printDocument.FitWidthToPages = 1;
        }

        private List<UltraGridColumn> GetExportColumns()
        {
            if (gridReport.DisplayLayout.Bands.Count == 0)
                return new List<UltraGridColumn>();

            return gridReport.DisplayLayout.Bands[0].Columns
                .Cast<UltraGridColumn>()
                .Where(column => !column.Hidden)
                .OrderBy(column => column.Header.VisiblePosition)
                .ToList();
        }

        private static void WriteExcelTextRow(StreamWriter writer, string text, string style, int columnCount)
        {
            writer.Write("<Row>");
            string styleText = string.IsNullOrWhiteSpace(style) ? string.Empty : " ss:StyleID=\"" + style + "\"";
            writer.Write("<Cell" + styleText + " ss:MergeAcross=\"" + Math.Max(0, columnCount - 1) + "\"><Data ss:Type=\"String\">" + EscapeXml(text) + "</Data></Cell>");
            writer.WriteLine("</Row>");
        }

        private static void WriteExcelCell(StreamWriter writer, string value, string style)
        {
            string styleText = string.IsNullOrWhiteSpace(style) ? string.Empty : " ss:StyleID=\"" + style + "\"";
            writer.Write("<Cell" + styleText + "><Data ss:Type=\"String\">" + EscapeXml(value) + "</Data></Cell>");
        }

        private string FormatGridExportValue(object value, UltraGridColumn column)
        {
            if (value == null || value == DBNull.Value)
                return string.Empty;

            DateTime dateValue;
            if (DateTime.TryParse(Convert.ToString(value), out dateValue) && column != null && column.DataType == typeof(DateTime))
                return dateValue.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

            IFormattable formattableValue = value as IFormattable;
            if (formattableValue != null && column != null && !string.IsNullOrWhiteSpace(column.Format))
                return formattableValue.ToString(column.Format, CultureInfo.InvariantCulture);

            return Convert.ToString(value);
        }

        private static string EscapeXml(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }

        private static string GetSafeFileName(string value)
        {
            string fileName = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(invalidChar, '_');
            }

            return fileName;
        }

        private static string GetControlText(Control control)
        {
            return control == null ? string.Empty : control.Text;
        }

        private static string GetEditorDateText(UltraDateTimeEditor editor)
        {
            if (editor == null || editor.Value == null)
                return string.Empty;

            DateTime dateValue;
            return DateTime.TryParse(Convert.ToString(editor.Value), out dateValue)
                ? dateValue.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture)
                : Convert.ToString(editor.Value);
        }

        private void OpenPurchaseOrderDialog()
        {
            using (frmPurchaseOrderDig dialog = new frmPurchaseOrderDig())
            {
                if (dialog.ShowDialog(this) == DialogResult.OK && dialog.SelectedPurchaseOrderId > 0)
                {
                    LoadPurchaseOrder(dialog.SelectedPurchaseOrderId);
                }
            }
        }

        private void OpenVendorDialog()
        {
            using (frmVendorDig vendorDialog = new frmVendorDig())
            {
                if (vendorDialog.ShowDialog(this) == DialogResult.OK)
                {
                    txtAccount.Text = vendorDialog.SelectedVendorId > 0
                        ? vendorDialog.SelectedVendorId.ToString()
                        : string.Empty;
                    txtNavigator.Text = vendorDialog.SelectedVendorName ?? string.Empty;
                    RefreshGridCostsFromLatestPurchases();
                }
            }
        }

        private int GetSelectedVendorLedgerId()
        {
            return ParseInt(txtAccount.Text);
        }

        private void OpenItemDialog()
        {
            using (frmdialForItemMaster itemDialog = new frmdialForItemMaster("FrmBarcode"))
            {
                if (itemDialog.ShowDialog(this) != DialogResult.OK)
                    return;

                Dictionary<string, object> itemData = itemDialog.GetSelectedItemData();
                if (itemData == null || itemData.Count == 0)
                    return;

                AddSelectedItemToGrid(itemData);
            }
        }

        private void AddSelectedItemToGrid(Dictionary<string, object> itemData)
        {
            DataTable table = gridReport.DataSource as DataTable;
            if (table == null)
                return;

            if (GetSelectedVendorLedgerId() <= 0)
            {
                MessageBox.Show("Please select a vendor first so the cost can use that vendor's latest purchase price.", "Vendor Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string itemNo = GetString(itemData, "BarCode", "Barcode", "ItemNo", "ItemId");
            string description = GetString(itemData, "Description", "ItemName", "ProductName");
            string uom = GetString(itemData, "Unit", "UOM");
            int itemId = GetInt(itemData, "ItemId", "ID", "ItemID");
            int unitId = GetInt(itemData, "UnitId", "UID");
            decimal qty = 1m;
            decimal itemMasterUnitCost = GetItemMasterUnitCost(itemId, unitId, uom, GetDecimal(itemData, "Cost", "PurchasePrice", "RetailPrice", "UnitPrice"));
            decimal cost = GetLatestPurchaseItemUnitCost(itemId, unitId, uom, itemMasterUnitCost);
            decimal amount = Math.Round(qty * cost, 2);
            decimal packing = GetDecimal(itemData, "Packing");
            if (packing <= 0)
            {
                packing = 1m;
            }
            string baseUnit = GetString(itemData, "BaseUnit", "IsBaseUnit");
            if (string.IsNullOrWhiteSpace(baseUnit))
            {
                baseUnit = packing == 1m ? "Y" : "N";
            }
            string targetItemNo = itemNo;
            string targetUom = uom;

            DataRow existingRow = table.Rows.Cast<DataRow>().FirstOrDefault(row =>
                string.Equals(Convert.ToString(row["ItemNo"]), itemNo, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(Convert.ToString(row["UOM"]), uom, StringComparison.OrdinalIgnoreCase));

            if (existingRow == null)
            {
                DataRow newRow = table.NewRow();
                newRow["No"] = table.Rows.Count + 1;
                newRow["ItemId"] = itemId;
                newRow["UnitId"] = unitId;
                newRow["Barcode"] = itemNo;
                newRow["ItemNo"] = itemNo;
                newRow["Description"] = description;
                newRow["UOM"] = uom;
                newRow["Qty"] = qty;
                newRow["Cost"] = cost;
                newRow["UPrice"] = itemMasterUnitCost;
                newRow["Amount"] = amount;
                newRow["Remark"] = string.Empty;
                newRow["Packing"] = packing;
                newRow["BaseUnit"] = baseUnit;
                table.Rows.Add(newRow);
            }
            else
            {
                decimal updatedQty = GetDecimalValue(existingRow, "Qty") + qty;
                existingRow["ItemId"] = itemId;
                existingRow["UnitId"] = unitId;
                existingRow["Barcode"] = itemNo;
                existingRow["Description"] = description;
                existingRow["Qty"] = updatedQty;
                existingRow["Cost"] = cost;
                existingRow["UPrice"] = itemMasterUnitCost;
                existingRow["Amount"] = Math.Round(updatedQty * cost, 2);
                existingRow["Packing"] = packing;
                existingRow["BaseUnit"] = baseUnit;
            }

            RenumberGridRows(table);
            UpdateFooterValues();
            RecalculateTotals();
            gridReport.Refresh();
            HighlightLatestLoadedItem(targetItemNo, targetUom);
        }

        private void HighlightLatestLoadedItem(string itemNo, string uom)
        {
            if (gridReport.Rows == null || gridReport.Rows.Count == 0)
                return;

            UltraGridRow targetRow = null;
            foreach (UltraGridRow row in gridReport.Rows)
            {
                if (row.Cells.Exists("ItemNo") &&
                    row.Cells.Exists("UOM") &&
                    string.Equals(Convert.ToString(row.Cells["ItemNo"].Value), itemNo, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(Convert.ToString(row.Cells["UOM"].Value), uom, StringComparison.OrdinalIgnoreCase))
                {
                    targetRow = row;
                }
            }

            if (targetRow == null)
                return;

            gridReport.Selected.Rows.Clear();
            targetRow.Selected = true;
            gridReport.ActiveRow = targetRow;

            try
            {
                targetRow.Activate();
                if (gridReport.DisplayLayout.RowScrollRegions.Count > 0)
                {
                    gridReport.DisplayLayout.RowScrollRegions[0].ScrollRowIntoView(targetRow);
                }
            }
            catch
            {
                // Keep selection active even if a specific scroll action is not available.
            }
        }

        private void txtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e == null || e.KeyCode != Keys.Enter)
                return;

            e.Handled = true;
            e.SuppressKeyPress = true;

            string input = (txtBarcode.Text ?? string.Empty).Trim();

            // Handle "u" or "U" to open Unit dialog for active row
            if (string.Equals(input, "u", StringComparison.OrdinalIgnoreCase))
            {
                txtBarcode.Clear();
                OpenUnitDialogForActivePurchaseOrderRow(true);
                return;
            }

            // Handle "*number" to set quantity on the active row
            if (input.StartsWith("*") && input.Length > 1)
            {
                string numberStr = input.Substring(1);
                decimal newQty;
                if (decimal.TryParse(numberStr, out newQty) && newQty > 0)
                {
                    UltraGridRow activeRow = gridReport.ActiveRow;
                    if (activeRow == null || !activeRow.IsDataRow)
                    {
                        // Try selected rows fallback
                        foreach (UltraGridRow selectedRow in gridReport.Selected.Rows)
                        {
                            if (selectedRow != null && selectedRow.IsDataRow)
                            {
                                activeRow = selectedRow;
                                break;
                            }
                        }
                    }

                    if (activeRow != null && activeRow.IsDataRow)
                    {
                        if (activeRow.Cells.Exists("Qty"))
                            activeRow.Cells["Qty"].Value = newQty;

                        RecalculateRow(activeRow);
                        RecalculateTotals();
                    }
                }

                txtBarcode.Clear();
                txtBarcode.Focus();
                return;
            }
        }

        private void gridReport_DoubleClickCell(object sender, DoubleClickCellEventArgs e)
        {
            if (e == null || e.Cell == null || e.Cell.Row == null || !e.Cell.Row.IsDataRow)
                return;

            if (e.Cell.Column != null && string.Equals(e.Cell.Column.Key, "UOM", StringComparison.OrdinalIgnoreCase))
            {
                OpenUnitDialogForPurchaseOrderRow(e.Cell.Row, false);
            }
        }

        private void OpenUnitDialogForActivePurchaseOrderRow(bool refocusBarcodeAfterClose)
        {
            UltraGridRow row = gridReport.ActiveRow;
            if (row == null || !row.IsDataRow)
            {
                foreach (UltraGridRow selectedRow in gridReport.Selected.Rows)
                {
                    if (selectedRow != null && selectedRow.IsDataRow)
                    {
                        row = selectedRow;
                        break;
                    }
                }
            }

            OpenUnitDialogForPurchaseOrderRow(row, refocusBarcodeAfterClose);
        }

        private bool TryGetSelectedUnitFromDialogTag(object dialogTag, out string selectedUnit, out decimal iuRate)
        {
            selectedUnit = string.Empty;
            iuRate = 0;

            string tagValue = dialogTag?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(tagValue))
                return false;

            if (tagValue.Contains("|"))
            {
                string[] parts = tagValue.Split('|');
                selectedUnit = parts[0];
                if (parts.Length > 1)
                {
                    decimal.TryParse(parts[1], out iuRate);
                }
            }
            else
            {
                selectedUnit = tagValue;
            }

            return !string.IsNullOrWhiteSpace(selectedUnit);
        }

        private void OpenUnitDialogForPurchaseOrderRow(UltraGridRow targetRow, bool refocusBarcodeAfterClose)
        {
            try
            {
                if (targetRow == null)
                    return;

                int itemId = ParseInt(Convert.ToString(targetRow.Cells["ItemId"].Value));
                if (itemId <= 0)
                    return;

                using (frmUnitDialog uomDialog = new frmUnitDialog("frmSalesInvoice", itemId))
                {
                    if (uomDialog.ShowDialog(this) != DialogResult.OK)
                        return;

                    string selectedUnit;
                    decimal iuRate;
                    if (!TryGetSelectedUnitFromDialogTag(uomDialog.Tag, out selectedUnit, out iuRate))
                        return;

                    ApplySelectedUnitToPurchaseOrderRow(targetRow, itemId, selectedUnit, iuRate);
                }
            }
            finally
            {
                if (refocusBarcodeAfterClose)
                {
                    txtBarcode.Focus();
                }
            }
        }

        private void ApplySelectedUnitToPurchaseOrderRow(UltraGridRow targetRow, int itemId, string selectedUnit, decimal iuRate)
        {
            if (targetRow == null || itemId <= 0 || string.IsNullOrWhiteSpace(selectedUnit))
                return;

            ItemDDl unitData = GetItemUnitData(itemId, selectedUnit);
            decimal packing = unitData != null && unitData.Packing > 0 ? Convert.ToDecimal(unitData.Packing) : 1m;
            int unitId = unitData != null && unitData.UnitId > 0
                ? unitData.UnitId
                : ParseInt(Convert.ToString(targetRow.Cells["UnitId"].Value));
            decimal itemMasterUnitCost = GetItemMasterUnitCost(itemId, unitId, selectedUnit, unitData != null ? Convert.ToDecimal(unitData.Cost) : 0m);
            decimal latestCost = GetLatestPurchaseItemUnitCost(itemId, unitId, selectedUnit, itemMasterUnitCost);

            targetRow.Cells["UOM"].Value = selectedUnit;
            targetRow.Cells["UnitId"].Value = unitId;
            targetRow.Cells["Packing"].Value = packing;
            targetRow.Cells["Cost"].Value = latestCost;
            targetRow.Cells["UPrice"].Value = itemMasterUnitCost;
            targetRow.Cells["BaseUnit"].Value = packing == 1m ? "Y" : "N";

            RecalculateRow(targetRow);
            UpdateFooterValues();
            RecalculateTotals();
            gridReport.Refresh();
        }

        private ItemDDl GetItemUnitData(int itemId, string selectedUnit)
        {
            try
            {
                Dropdowns dropdowns = new Dropdowns();
                ItemDDlGrid unitGrid = dropdowns.GetItemUnits(itemId);
                return unitGrid?.List?.FirstOrDefault(x =>
                    x != null &&
                    string.Equals((x.Unit ?? string.Empty).Trim(), selectedUnit.Trim(), StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return null;
            }
        }

        private decimal GetItemMasterUnitCost(int itemId, int unitId, string unitName, decimal fallbackCost)
        {
            try
            {
                List<ItemMasterPriceSettings> priceSettings = _itemMasterRepository.GetItemPriceSettings(itemId);
                ItemMasterPriceSettings match = FindItemPriceSetting(priceSettings, unitId, unitName);
                if (match != null)
                    return Convert.ToDecimal(match.Cost);
            }
            catch
            {
                // Fall back to the item dialog/dropdown cost if price-settings lookup is unavailable.
            }

            return fallbackCost;
        }

        private decimal GetLatestPurchaseItemUnitCost(int itemId, int unitId, string unitName, decimal fallbackCost)
        {
            int resolvedUnitId = unitId;
            decimal itemMasterCost = GetItemMasterUnitCost(itemId, unitId, unitName, fallbackCost);
            int vendorLedgerId = GetSelectedVendorLedgerId();

            if (vendorLedgerId <= 0)
                return itemMasterCost;

            try
            {
                List<ItemMasterPriceSettings> priceSettings = _itemMasterRepository.GetItemPriceSettings(itemId);
                ItemMasterPriceSettings match = FindItemPriceSetting(priceSettings, resolvedUnitId, unitName);
                if (match != null)
                    resolvedUnitId = match.UnitId;
            }
            catch
            {
                // Use the provided unit id if price-settings lookup is unavailable.
            }

            decimal purchaseCost = GetPurchaseOrderRepository().GetLatestItemPurchaseCost(
                itemId,
                resolvedUnitId,
                unitName,
                GetCompanyId(),
                GetBranchId(),
                vendorLedgerId);

            return purchaseCost > 0 ? purchaseCost : itemMasterCost;
        }

        private static ItemMasterPriceSettings FindItemPriceSetting(List<ItemMasterPriceSettings> priceSettings, int unitId, string unitName)
        {
            if (priceSettings == null || priceSettings.Count == 0)
                return null;

            ItemMasterPriceSettings match = null;

            if (unitId > 0)
                match = priceSettings.FirstOrDefault(x => x != null && x.UnitId == unitId);

            if (match == null && !string.IsNullOrWhiteSpace(unitName))
            {
                match = priceSettings.FirstOrDefault(x =>
                    x != null &&
                    string.Equals((x.Unit ?? string.Empty).Trim(), unitName.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            return match;
        }

        private void RefreshGridCostsFromLatestPurchases()
        {
            if (gridReport == null || gridReport.Rows == null)
                return;

            foreach (UltraGridRow row in gridReport.Rows)
            {
                if (row == null || !row.IsDataRow)
                    continue;

                int itemId = ParseInt(Convert.ToString(row.Cells["ItemId"].Value));
                int unitId = ParseInt(Convert.ToString(row.Cells["UnitId"].Value));
                string unit = Convert.ToString(row.Cells["UOM"].Value);
                decimal itemMasterUnitCost = GetItemMasterUnitCost(itemId, unitId, unit, GetNumericValue(row.Cells.Exists("UPrice") ? row.Cells["UPrice"].Value : null) ?? 0m);
                decimal latestCost = GetLatestPurchaseItemUnitCost(itemId, unitId, unit, itemMasterUnitCost);

                row.Cells["Cost"].Value = latestCost;
                row.Cells["UPrice"].Value = itemMasterUnitCost;
                RecalculateRow(row);
            }

            UpdateFooterValues();
            RecalculateTotals();
            gridReport.Refresh();
        }

        private static string GetString(Dictionary<string, object> data, params string[] keys)
        {
            foreach (string key in keys)
            {
                if (data.TryGetValue(key, out object value) && value != null)
                {
                    string text = Convert.ToString(value);
                    if (!string.IsNullOrWhiteSpace(text))
                        return text.Trim();
                }
            }

            return string.Empty;
        }

        private static decimal GetDecimal(Dictionary<string, object> data, params string[] keys)
        {
            foreach (string key in keys)
            {
                if (data.TryGetValue(key, out object value) && value != null)
                {
                    decimal parsed;
                    if (decimal.TryParse(Convert.ToString(value), out parsed))
                        return parsed;
                }
            }

            return 0m;
        }

        private static int GetInt(Dictionary<string, object> data, params string[] keys)
        {
            foreach (string key in keys)
            {
                if (data.TryGetValue(key, out object value) && value != null)
                {
                    int parsed;
                    if (int.TryParse(Convert.ToString(value), out parsed))
                        return parsed;
                }
            }

            return 0;
        }

        private void RecalculateRow(UltraGridRow row)
        {
            if (row == null)
                return;

            decimal qty = GetNumericValue(row.Cells.Exists("Qty") ? row.Cells["Qty"].Value : null) ?? 0m;
            decimal cost = GetNumericValue(row.Cells.Exists("Cost") ? row.Cells["Cost"].Value : null) ?? 0m;
            decimal amount = Math.Round(qty * cost, 2);

            if (row.Cells.Exists("Amount"))
                row.Cells["Amount"].Value = amount;
        }

        private void RecalculateTotals()
        {
            decimal subTotal = 0m;

            foreach (UltraGridRow row in gridReport.Rows)
            {
                if (row == null || !row.IsDataRow)
                    continue;

                subTotal += GetNumericValue(row.Cells.Exists("Amount") ? row.Cells["Amount"].Value : null) ?? 0m;
            }

            decimal discount = ParseDecimal(txtDisc.Text);
            decimal grossTotal = subTotal - discount;
            decimal rounding = ultraCheckEditorRound.Checked
                ? Math.Round(grossTotal, 0, MidpointRounding.AwayFromZero) - grossTotal
                : 0m;
            decimal total = grossTotal + rounding;

            txtSubtotal.Text = subTotal.ToString("0.00");
            txtPurchaseTax.Text = "0.00";
            txtRounding.Text = rounding.ToString("0.00");
            txtTotal.Text = total.ToString("0.00");
        }

        private static decimal ParseDecimal(string text)
        {
            decimal value;
            return decimal.TryParse(text, out value) ? value : 0m;
        }

        private static DateTime GetEditorDate(UltraDateTimeEditor editor)
        {
            if (editor != null && editor.Value != null)
            {
                DateTime parsedDate;
                if (DateTime.TryParse(Convert.ToString(editor.Value), out parsedDate))
                    return parsedDate.Date;
            }

            return DateTime.Today;
        }

        private void LoadNextDocumentNumber()
        {
            if (IsDesignerHosted())
            {
                txtDocNo.Text = "PO-";
                return;
            }

            int purchaseOrderNo = GetPurchaseOrderRepository().GeneratePurchaseOrderNo(GetFinYearId(), GetBranchId());
            txtDocNo.Text = purchaseOrderNo > 0 ? "PO-" + purchaseOrderNo.ToString() : string.Empty;
        }

        public void SaveData()
        {
            if (string.IsNullOrWhiteSpace(txtNavigator.Text))
            {
                MessageBox.Show("Please select a vendor.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<PurchaseOrderDetail> details = BuildPurchaseOrderDetails();
            if (details.Count == 0)
            {
                MessageBox.Show("Please add at least one item.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            PurchaseOrderMaster master = BuildPurchaseOrderMaster();
            bool isUpdate = _currentPurchaseOrderId > 0;
            string result = isUpdate
                ? GetPurchaseOrderRepository().UpdatePurchaseOrder(master, details)
                : GetPurchaseOrderRepository().SavePurchaseOrder(master, details);

            if (!string.Equals(result, "SUCCESS", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(result, "Save Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show(
                isUpdate ? "Purchase order updated successfully." : "Purchase order saved successfully.",
                isUpdate ? "Update" : "Save",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            ClearForm();
        }

        public void RibbonClear()
        {
            ClearForm();
        }

        public void ClearForm()
        {
            InitializeDefaults();
            _currentPurchaseOrderId = 0;

            DataTable table = gridReport.DataSource as DataTable;
            if (table != null)
            {
                table.Rows.Clear();
            }

            gridReport.Refresh();
            UpdateFooterValues();
            RecalculateTotals();
            LoadNextDocumentNumber();
        }

        private PurchaseOrderMaster BuildPurchaseOrderMaster()
        {
            decimal subTotal = ParseDecimal(txtSubtotal.Text);
            decimal taxAmount = ParseDecimal(txtPurchaseTax.Text);
            decimal roundOff = ParseDecimal(txtRounding.Text);
            decimal grandTotal = ParseDecimal(txtTotal.Text);
            DateTime documentDate = GetEditorDate(dtpDate);
            DateTime expectedDate = GetEditorDate(dtpExpectedDate);

            return new PurchaseOrderMaster
            {
                CompanyId = GetCompanyId(),
                FinYearId = GetFinYearId(),
                BranchId = GetBranchId(),
                BranchName = string.Empty,
                PurchaseNo = ParseDocumentNumber(txtDocNo.Text),
                PurchaseDate = documentDate,
                InvoiceNo = txtReference.Text.Trim(),
                InvoiceDate = expectedDate,
                LedgerID = ParseInt(txtAccount.Text),
                VendorName = txtNavigator.Text.Trim(),
                PaymodeID = 0,
                Paymode = string.Empty,
                PaymodeLedgerID = 0,
                CreditPeriod = 0,
                SubTotal = Convert.ToDouble(subTotal),
                SpDisPer = 0,
                SpDsiAmt = 0,
                BillDiscountPer = 0,
                BillDiscountAmt = Convert.ToDouble(ParseDecimal(txtDisc.Text)),
                TaxPer = 0,
                TaxAmt = Convert.ToDouble(taxAmount),
                Frieght = 0,
                ExpenseAmt = 0,
                OtherExpAmt = 0,
                GrandTotal = Convert.ToDouble(grandTotal),
                CancelFlag = false,
                UserID = GetUserId(),
                UserName = GetUserName(),
                TaxType = "I",
                Remarks = BuildRemarks(),
                RoundOff = Convert.ToDouble(roundOff),
                CessPer = 0,
                CessAmt = 0,
                CalAfterTax = 0,
                CurrencyID = 0,
                CurSymbol = string.Empty,
                SeriesID = 0,
                VoucherID = 0,
                IsSyncd = false,
                Paid = false,
                Pid = 0,
                POrderMasterId = _currentPurchaseOrderId,
                PayedAmount = 0,
                BilledBy = txtOrderBy.Text.Trim(),
                TrnsType = "Purchase Order",
                NetTotal = Convert.ToDouble(grandTotal),
                ReferenceNo = txtReference.Text.Trim(),
                ShipTo1 = txtShipTo1.Text.Trim(),
                ShipTo2 = txtShipTo2.Text.Trim(),
                ShipTo3 = txtShipTo3.Text.Trim(),
                ShipTo4 = txtShipTo4.Text.Trim(),
                Telephone = txtTelephone.Text.Trim(),
                OrderBy = txtOrderBy.Text.Trim(),
                ExpectedDate = expectedDate,
                CreditPeriodTerm = string.Empty,
                ApprovedBy = string.Empty,
                CreatedBy = GetUserName(),
                ShipVia = string.Empty,
                FobPoints = string.Empty
            };
        }

        private List<PurchaseOrderDetail> BuildPurchaseOrderDetails()
        {
            List<PurchaseOrderDetail> details = new List<PurchaseOrderDetail>();

            foreach (UltraGridRow row in gridReport.Rows)
            {
                if (row == null || !row.IsDataRow)
                    continue;

                int itemId = ParseInt(Convert.ToString(row.Cells["ItemId"].Value));
                if (itemId <= 0)
                    continue;

                details.Add(new PurchaseOrderDetail
                {
                    CompanyId = GetCompanyId(),
                    FinYearId = GetFinYearId(),
                    BranchID = GetBranchId(),
                    PurchaseDate = GetEditorDate(dtpDate),
                    InvoiceNo = txtReference.Text.Trim(),
                    SlNo = ParseInt(Convert.ToString(row.Cells["No"].Value)),
                    ItemID = itemId,
                    ItemName = Convert.ToString(row.Cells["Description"].Value),
                    UnitId = ParseInt(Convert.ToString(row.Cells["UnitId"].Value)),
                    Unit = Convert.ToString(row.Cells["UOM"].Value),
                    BaseUnit = NormalizeBaseUnitValue(Convert.ToString(row.Cells["BaseUnit"].Value)),
                    Packing = Convert.ToDouble(GetNumericValue(row.Cells["Packing"].Value) ?? 1m),
                    Qty = Convert.ToDouble(GetNumericValue(row.Cells["Qty"].Value) ?? 0m),
                    Free = 0,
                    Cost = Convert.ToDouble(GetNumericValue(row.Cells["Cost"].Value) ?? 0m),
                    DisPer = 0,
                    DisAmt = 0,
                    SalesPrice = Convert.ToDouble(GetNumericValue(row.Cells["Amount"].Value) ?? 0m),
                    TaxPer = 0,
                    TaxAmt = 0,
                    TotalSP = Convert.ToDouble(GetNumericValue(row.Cells["Amount"].Value) ?? 0m),
                    OriginalCost = Convert.ToDouble(GetNumericValue(row.Cells["Cost"].Value) ?? 0m),
                    OriginalSP = Convert.ToDouble(GetNumericValue(row.Cells["Amount"].Value) ?? 0m),
                    IsExpiry = false,
                    TaxType = string.Empty,
                    SeriesID = 0,
                    CessAmt = 0,
                    CessPer = 0,
                    IsSyncd = false,
                    OldQty = 0,
                    RetailPrice = 0,
                    WholeSalePrice = 0,
                    CreditPrice = 0,
                    Barcode = Convert.ToString(row.Cells["Barcode"].Value),
                    SingleItemCost = Convert.ToDouble(GetNumericValue(row.Cells["Cost"].Value) ?? 0m),
                    TrnsType = "Purchase Order",
                    RowRemark = Convert.ToString(row.Cells["Remark"].Value),
                    BaseQty = Convert.ToDouble((GetNumericValue(row.Cells["Qty"].Value) ?? 0m) * (GetNumericValue(row.Cells["Packing"].Value) ?? 1m)),
                    BaseQtyReceived = 0,
                    TotalSst = 0
                });
            }

            return details;
        }

        private string BuildRemarks()
        {
            StringBuilder builder = new StringBuilder();

            AppendRemarkLine(builder, "Reference", txtReference.Text);
            AppendRemarkLine(builder, "ShipTo1", txtShipTo1.Text);
            AppendRemarkLine(builder, "ShipTo2", txtShipTo2.Text);
            AppendRemarkLine(builder, "ShipTo3", txtShipTo3.Text);
            AppendRemarkLine(builder, "ShipTo4", txtShipTo4.Text);
            AppendRemarkLine(builder, "Telephone", txtTelephone.Text);
            AppendRemarkLine(builder, "OrderBy", txtOrderBy.Text);
            AppendRemarkLine(builder, "ExpectedDate", GetEditorDate(dtpExpectedDate).ToString("dd/MM/yyyy"));
            AppendRemarkLine(builder, "Remark", txtRemark.Text);

            return builder.ToString();
        }

        private static void AppendRemarkLine(StringBuilder builder, string label, string value)
        {
            string text = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
            if (string.IsNullOrWhiteSpace(text))
                return;

            if (builder.Length > 0)
                builder.AppendLine();

            builder.Append(label);
            builder.Append(": ");
            builder.Append(text);
        }

        private void LoadPurchaseOrder(int purchaseOrderId)
        {
            PurchaseOrderLoadResult result = GetPurchaseOrderRepository().GetPurchaseOrderById(purchaseOrderId);
            if (result == null || result.Master == null)
            {
                MessageBox.Show("Unable to load the selected purchase order.", "Purchase Order", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            PurchaseOrderMaster master = result.Master;
            Dictionary<string, string> remarks = ParseRemarks(master.Remarks);

            _currentPurchaseOrderId = master.POrderMasterId > 0 ? master.POrderMasterId : purchaseOrderId;

            txtDocNo.Text = master.PurchaseNo > 0 ? "PO-" + master.PurchaseNo.ToString() : string.Empty;
            dtpDate.Value = master.PurchaseDate == DateTime.MinValue ? DateTime.Today : master.PurchaseDate;
            txtReference.Text = GetRemarkValue(remarks, "Reference", master.InvoiceNo);
            txtAccount.Text = master.LedgerID > 0 ? master.LedgerID.ToString() : string.Empty;
            txtNavigator.Text = master.VendorName ?? string.Empty;
            txtShipTo1.Text = GetRemarkValue(remarks, "ShipTo1", master.ShipTo1);
            txtShipTo2.Text = GetRemarkValue(remarks, "ShipTo2", master.ShipTo2);
            txtShipTo3.Text = GetRemarkValue(remarks, "ShipTo3", master.ShipTo3);
            txtShipTo4.Text = GetRemarkValue(remarks, "ShipTo4", master.ShipTo4);
            txtTelephone.Text = GetRemarkValue(remarks, "Telephone", master.Telephone);
            txtOrderBy.Text = GetRemarkValue(remarks, "OrderBy", master.OrderBy);
            txtRemark.Text = GetRemarkValue(remarks, "Remark", string.Empty);
            dtpExpectedDate.Value = GetRemarkDate(remarks, "ExpectedDate", master.InvoiceDate);
            txtDisc.Text = Convert.ToDecimal(master.BillDiscountAmt).ToString("0.00");
            txtSubtotal.Text = Convert.ToDecimal(master.SubTotal).ToString("0.00");
            txtPurchaseTax.Text = Convert.ToDecimal(master.TaxAmt).ToString("0.00");
            txtRounding.Text = Convert.ToDecimal(master.RoundOff).ToString("0.00");
            txtTotal.Text = Convert.ToDecimal(master.GrandTotal).ToString("0.00");

            PopulateGrid(result.Details);
            UpdateFooterValues();
            RecalculateTotals();
        }

        private void PopulateGrid(IEnumerable<PurchaseOrderDetail> details)
        {
            DataTable table = gridReport.DataSource as DataTable;
            if (table == null)
                return;

            table.Rows.Clear();

            if (details != null)
            {
                int rowNumber = 1;
                foreach (PurchaseOrderDetail detail in details)
                {
                    if (detail == null)
                        continue;

                    DataRow row = table.NewRow();
                    row["No"] = rowNumber++;
                    row["ItemId"] = detail.ItemID;
                    row["UnitId"] = detail.UnitId;
                    row["Barcode"] = detail.Barcode ?? string.Empty;
                    row["ItemNo"] = !string.IsNullOrWhiteSpace(detail.Barcode) ? detail.Barcode : detail.ItemID.ToString();
                    row["Description"] = detail.ItemName ?? string.Empty;
                    row["UOM"] = detail.Unit ?? string.Empty;
                    row["Qty"] = Convert.ToDecimal(detail.Qty);
                    row["Cost"] = GetLatestPurchaseItemUnitCost(detail.ItemID, detail.UnitId, detail.Unit, Convert.ToDecimal(detail.Cost));
                    row["UPrice"] = GetItemMasterUnitCost(detail.ItemID, detail.UnitId, detail.Unit, Convert.ToDecimal(detail.Cost));
                    row["Amount"] = Math.Round(Convert.ToDecimal(detail.Qty) * Convert.ToDecimal(row["Cost"]), 2);
                    row["Remark"] = detail.RowRemark ?? string.Empty;
                    row["Packing"] = Convert.ToDecimal(detail.Packing);
                    row["BaseUnit"] = NormalizeBaseUnitValue(detail.BaseUnit);
                    table.Rows.Add(row);
                }
            }

            gridReport.Refresh();
        }

        private static Dictionary<string, string> ParseRemarks(string remarks)
        {
            Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(remarks))
                return values;

            string[] lines = remarks.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                int separatorIndex = line.IndexOf(':');
                if (separatorIndex <= 0)
                    continue;

                string key = line.Substring(0, separatorIndex).Trim();
                string value = line.Substring(separatorIndex + 1).Trim();
                values[key] = value;
            }

            return values;
        }

        private static string GetRemarkValue(Dictionary<string, string> remarks, string key, string fallback)
        {
            if (remarks != null && remarks.TryGetValue(key, out string value) && !string.IsNullOrWhiteSpace(value))
                return value;

            return fallback ?? string.Empty;
        }

        private static DateTime GetRemarkDate(Dictionary<string, string> remarks, string key, DateTime fallback)
        {
            if (remarks != null && remarks.TryGetValue(key, out string value))
            {
                DateTime parsedDate;
                if (DateTime.TryParse(value, out parsedDate))
                    return parsedDate.Date;
            }

            return fallback == DateTime.MinValue ? DateTime.Today : fallback.Date;
        }

        private static int ParseDocumentNumber(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            string numericPart = new string(text.Where(char.IsDigit).ToArray());
            int value;
            return int.TryParse(numericPart, out value) ? value : 0;
        }

        private static int ParseInt(string text)
        {
            int value;
            return int.TryParse(text, out value) ? value : 0;
        }

        private int GetCompanyId()
        {
            if (ModelClass.SessionContext.IsInitialized && ModelClass.SessionContext.CompanyId > 0)
                return ModelClass.SessionContext.CompanyId;

            return ParseInt(Convert.ToString(ModelClass.DataBase.CompanyId));
        }

        private int GetBranchId()
        {
            if (ModelClass.SessionContext.IsInitialized && ModelClass.SessionContext.BranchId > 0)
                return ModelClass.SessionContext.BranchId;

            return ParseInt(Convert.ToString(ModelClass.DataBase.BranchId));
        }

        private int GetFinYearId()
        {
            if (ModelClass.SessionContext.IsInitialized && ModelClass.SessionContext.FinYearId > 0)
                return ModelClass.SessionContext.FinYearId;

            return 0;
        }

        private int GetUserId()
        {
            if (ModelClass.SessionContext.IsInitialized && ModelClass.SessionContext.UserId > 0)
                return ModelClass.SessionContext.UserId;

            return 0;
        }

        private string GetUserName()
        {
            if (!string.IsNullOrWhiteSpace(ModelClass.SessionContext.UserName))
                return ModelClass.SessionContext.UserName;

            return string.Empty;
        }

        private PurchaseOrderRepository GetPurchaseOrderRepository()
        {
            if (_purchaseOrderRepository == null)
            {
                _purchaseOrderRepository = new PurchaseOrderRepository();
            }

            return _purchaseOrderRepository;
        }

        private bool IsDesignerHosted()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime || DesignMode;
        }

        private void ultraPictureBox8_Click(object sender, EventArgs e)
        {

        }


    }
}
