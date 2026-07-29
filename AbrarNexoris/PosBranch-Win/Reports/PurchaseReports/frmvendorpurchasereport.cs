using ModelClass;
using PosBranch_Win.DialogBox;
using PosBranch_Win.Reports.FinancialReports;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinGrid;
using Infragistics.Win.UltraWinEditors;

namespace PosBranch_Win.Reports.PurchaseReports
{
    public partial class frmvendorpurchasereport : Form
    {
        private readonly Color pageBack = Color.FromArgb(232, 246, 255);
        private readonly Color cardBack = Color.FromArgb(250, 253, 255);
        private readonly Color border = Color.FromArgb(190, 226, 250);
        private readonly Color navy = Color.FromArgb(20, 55, 120);
        private readonly Color muted = Color.FromArgb(72, 98, 138);
        private readonly Color accent = Color.FromArgb(42, 121, 232);
        private readonly Color skyBlueOutline = Color.FromArgb(102, 190, 255);
        private readonly Color gridHeaderBlue = Color.FromArgb(93, 151, 214);
        private readonly Color gridHeaderBlueDark = Color.FromArgb(67, 118, 184);
        private readonly Color gridSelectedBlue = Color.FromArgb(126, 126, 245);
        private readonly Color gridRowLine = Color.FromArgb(197, 217, 241);
        private readonly Color gridAltRow = Color.FromArgb(246, 250, 255);
        private readonly Color gridFooterBorder = Color.FromArgb(144, 181, 223);
        private readonly Color buttonBlueTop = Color.FromArgb(232, 241, 252);
        private readonly Color buttonBlueBottom = Color.FromArgb(145, 181, 224);
        private readonly Color buttonLightOutline = Color.FromArgb(166, 183, 202);
        private readonly CultureInfo culture = new CultureInfo("en-IN");
        private readonly Dictionary<string, Label> footerLabels = new Dictionary<string, Label>();
        private readonly Dictionary<string, string> columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private int selectedVendorId;
        private string selectedVendorName = string.Empty;
        private int selectedItemId;
        private string selectedItemName = string.Empty;
        private bool suppressQuickDateChange;
        private DataTable currentData = new DataTable();
        private ReportMode activeMode = ReportMode.Overview;

        private enum ReportMode
        {
            Overview,
            Vendor,
            Item,
            Both
        }

        private Form columnChooserForm;
        private ListBox columnChooserListBox;
        private bool isDraggingHeaderToHide;
        private UltraGridColumn columnBeingDragged;
        private Point headerDragStartPoint;
        private readonly System.Windows.Forms.ToolTip headerToolTip = new System.Windows.Forms.ToolTip();
        private readonly HashSet<string> userHiddenColumnKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly Cursor blackXCursor = CreateBlackXCursor();

        public frmvendorpurchasereport()
        {
            InitializeComponent();
            gridReport.Resize += gridReport_Resize;
            gridReport.AfterColPosChanged += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterColRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridReport.AfterRowRegionScroll += (s, e) => UpdateFooterCellPositions();
            gridReport.Paint += (s, e) => UpdateFooterCellPositions();
            SetupHeaderDragToHideAndColumnChooser();
            ApplyRuntimeStyles();
        }

        private void frmvendorpurchasereport_Load(object sender, EventArgs e)
        {
            cmbQuickDate.Value = "Today";
            ApplyQuickDate();
            LoadReport();
        }

        /// <summary>
        /// Pre-selects a vendor by ID/name and loads the Vendor-mode report with ALL dates.
        /// Call this after the form is shown/loaded.
        /// </summary>
        public void OpenWithVendor(int vendorId, string vendorName)
        {
            if (vendorId <= 0) return;
            selectedVendorId = vendorId;
            selectedVendorName = string.IsNullOrWhiteSpace(vendorName) ? "Vendor " + vendorId : vendorName;
            txtVendor.Text = selectedVendorName;

            // Always show ALL dates when opening from Item Master so full purchase history is visible
            suppressQuickDateChange = true;
            cmbQuickDate.Value = "All";
            suppressQuickDateChange = false;
            ApplyQuickDate();

            SetActiveMode(ReportMode.Vendor, true);
        }

        private void ApplyRuntimeStyles()
        {
            Text = "Vendor Purchase Report";
            BackColor = pageBack;
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(900, 460);

            StyleFilterCombo(cmbQuickDate);
            StyleFilterDate(dtpFrom);
            StyleFilterDate(dtpTo);
            StyleFilterText(txtVendor);
            StyleFilterText(txtItem);

            cmbQuickDate.Items.Clear();
            cmbQuickDate.Items.Add("All");
            cmbQuickDate.Items.Add("Today");
            cmbQuickDate.Items.Add("Yesterday");
            cmbQuickDate.Items.Add("Last 7 Days");
            cmbQuickDate.Items.Add("This Month");
            cmbQuickDate.Items.Add("Custom");

            AttachCardPaint(panelFilters);
            AttachCardPaint(panelGrid);
            AttachCardPaint(panelRowsCard);
            AttachCardPaint(panelBillsCard);
            AttachCardPaint(panelQtyCard);
            AttachCardPaint(panelAmountCard);

            StyleButton(btnVendor, false);
            StyleButton(btnItem, false);
            StyleButton(btnBoth, false);
            StyleButton(btnClear, false);
            StyleClassicButton(btnExportGrid);
            StyleClassicButton(btnPreviewGrid);
            StyleClassicButton(btnPreviewReport);
            StyleButton(btnApply, true);

            ultraPanelGridFooter.Appearance.BackColor = gridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackColor2 = gridHeaderBlue;
            ultraPanelGridFooter.Appearance.BackGradientStyle = GradientStyle.None;
            ultraPanelGridFooter.Appearance.BorderColor = gridFooterBorder;
            ultraPanelGridFooter.BorderStyle = UIElementBorderStyle.Solid;

            StyleGrid();
        }

        private void AttachCardPaint(Panel panel)
        {
            if (panel != null)
                panel.Paint += Card_Paint;
        }

        private void StyleButton(Button button, bool primary)
        {
            if (button == null)
                return;

            button.FlatStyle = FlatStyle.Flat;
            button.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
            button.ForeColor = primary ? Color.White : navy;
            button.BackColor = primary ? accent : Color.FromArgb(236, 246, 255);
            button.UseVisualStyleBackColor = false;
            button.FlatAppearance.BorderColor = primary ? accent : skyBlueOutline;
            button.FlatAppearance.BorderSize = primary ? 0 : 1;
            button.FlatAppearance.MouseOverBackColor = primary ? accent : Color.FromArgb(225, 244, 255);
            button.FlatAppearance.MouseDownBackColor = primary ? Color.FromArgb(31, 96, 205) : Color.FromArgb(210, 235, 252);

            if (primary)
            {
                button.Paint -= ApplyButton_Paint;
                button.Paint += ApplyButton_Paint;
            }
        }

        private void StyleFilterCombo(UltraComboEditor combo)
        {
            combo.UseAppStyling = false;
            combo.UseOsThemes = DefaultableBoolean.False;
            combo.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            combo.BorderStyle = UIElementBorderStyle.Solid;
            combo.DropDownStyle = Infragistics.Win.DropDownStyle.DropDownList;
            combo.Appearance.BackColor = Color.White;
            combo.Appearance.BorderColor = skyBlueOutline;
            combo.Appearance.ForeColor = navy;
            combo.Appearance.FontData.Name = "Segoe UI";
            combo.Appearance.FontData.SizeInPoints = 9F;
            combo.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
        }

        private void StyleFilterDate(UltraDateTimeEditor editor)
        {
            editor.UseAppStyling = false;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor = Color.White;
            editor.Appearance.BorderColor = skyBlueOutline;
            editor.Appearance.ForeColor = navy;
            editor.Appearance.FontData.Name = "Segoe UI";
            editor.Appearance.FontData.SizeInPoints = 9F;
            editor.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
            editor.DropDownButtonDisplayStyle = Infragistics.Win.ButtonDisplayStyle.Always;
            editor.FormatString = "dd-MMM-yyyy";
            editor.MaskInput = "{date}";
        }

        private void StyleFilterText(UltraTextEditor editor)
        {
            editor.UseAppStyling = false;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor = Color.White;
            editor.Appearance.BorderColor = skyBlueOutline;
            editor.Appearance.ForeColor = navy;
            editor.Appearance.FontData.Name = "Segoe UI";
            editor.Appearance.FontData.SizeInPoints = 9F;
        }

        private void ApplyButton_Paint(object sender, PaintEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(accent))
                e.Graphics.FillRectangle(brush, btnApply.ClientRectangle);

            TextRenderer.DrawText(e.Graphics, btnApply.Text, btnApply.Font, btnApply.ClientRectangle,
                Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }

        private void StyleClassicButton(UltraButton button)
        {
            if (button == null)
                return;

            button.UseAppStyling = false;
            button.UseOsThemes = DefaultableBoolean.False;
            button.ButtonStyle = UIElementButtonStyle.Flat;
            button.UseFlatMode = DefaultableBoolean.False;
            button.Appearance.BackColor = buttonBlueTop;
            button.Appearance.BackColor2 = buttonBlueBottom;
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.ForeColor = navy;
            button.Appearance.BorderColor = buttonLightOutline;
            button.Appearance.TextHAlign = HAlign.Center;
            button.Appearance.TextVAlign = VAlign.Middle;
            button.Appearance.FontData.SizeInPoints = 9;
            button.Font = new Font("Tahoma", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            button.HotTrackAppearance.BackColor = Color.FromArgb(241, 247, 254);
            button.HotTrackAppearance.BackColor2 = Color.FromArgb(166, 195, 231);
            button.HotTrackAppearance.BackGradientStyle = GradientStyle.Vertical;
            button.HotTrackAppearance.BorderColor = buttonLightOutline;
            button.HotTrackAppearance.ForeColor = navy;
            button.PressedAppearance.BackColor = Color.FromArgb(118, 161, 214);
            button.PressedAppearance.BackColor2 = Color.FromArgb(217, 231, 247);
            button.PressedAppearance.BackGradientStyle = GradientStyle.Vertical;
            button.PressedAppearance.BorderColor = Color.FromArgb(148, 163, 182);
            button.PressedAppearance.ForeColor = navy;
        }

        private void StyleGrid()
        {
            if (gridReport == null)
                return;

            gridReport.DisplayLayout.Reset();
            gridReport.UseAppStyling = false;
            gridReport.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = gridReport.DisplayLayout;
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.BorderStyle = UIElementBorderStyle.Solid;
            layout.GroupByBox.Hidden = false;
            layout.GroupByBox.BandLabelAppearance.BackColor = gridHeaderBlueDark;
            layout.GroupByBox.BandLabelAppearance.ForeColor = Color.White;
            layout.GroupByBox.BandLabelAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.GroupByBox.PromptAppearance.BackColor = gridHeaderBlue;
            layout.GroupByBox.PromptAppearance.BackColor2 = gridHeaderBlueDark;
            layout.GroupByBox.PromptAppearance.BackGradientStyle = GradientStyle.Horizontal;
            layout.GroupByBox.PromptAppearance.ForeColor = Color.White;
            layout.GroupByBox.Prompt = "Drag a column header here to group by that column";
            layout.GroupByBox.Appearance.BackColor = Color.FromArgb(109, 167, 226);
            layout.GroupByBox.Appearance.BackColor2 = Color.FromArgb(69, 125, 190);
            layout.GroupByBox.Appearance.BackGradientStyle = GradientStyle.Vertical;

            layout.Appearance.BackColor = pageBack;
            layout.Appearance.BorderColor = Color.FromArgb(118, 154, 198);
            layout.Appearance.BackColor2 = pageBack;
            layout.Appearance.BackGradientStyle = GradientStyle.None;

            layout.Override.AllowAddNew = AllowAddNew.No;
            layout.Override.AllowDelete = DefaultableBoolean.False;
            layout.Override.AllowUpdate = DefaultableBoolean.False;
            layout.Override.CellClickAction = CellClickAction.RowSelect;
            layout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            layout.Override.SelectTypeRow = SelectType.Single;
            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorWidth = 20;
            layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            layout.Override.AllowRowFiltering = DefaultableBoolean.False;
            layout.Override.FilterUIType = FilterUIType.Default;

            layout.Override.RowSelectorAppearance.BackColor = gridHeaderBlueDark;
            layout.Override.RowSelectorAppearance.BackColor2 = gridHeaderBlue;
            layout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.RowSelectorAppearance.BorderColor = Color.FromArgb(118, 154, 198);
            layout.Override.RowSelectorAppearance.ForeColor = Color.White;
            layout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.RowSelectorAppearance.TextHAlign = HAlign.Center;

            layout.Override.HeaderAppearance.BackColor = gridHeaderBlue;
            layout.Override.HeaderAppearance.BackColor2 = gridHeaderBlueDark;
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor = Color.White;
            layout.Override.HeaderAppearance.BorderColor = Color.FromArgb(118, 154, 198);
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
            layout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;

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
            layout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.CellAppearance.FontData.SizeInPoints = 8.25F;
            layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            layout.Override.MinRowHeight = 19;
            layout.Override.DefaultRowHeight = 19;
            layout.RowConnectorStyle = RowConnectorStyle.None;
            layout.AutoFitStyle = AutoFitStyle.None;
        }

        private void cmbQuickDate_ValueChanged(object sender, EventArgs e)
        {
            if (GetQuickDateText() != "Custom")
            {
                ApplyQuickDate();
                LoadReport();
            }
        }

        private void DatePicker_ValueChanged(object sender, EventArgs e)
        {
            SetCustomQuickDate();
        }

        private void btnVendor_Click(object sender, EventArgs e)
        {
            if (selectedVendorId > 0 && activeMode != ReportMode.Vendor)
            {
                SetActiveMode(ReportMode.Vendor, true);
                return;
            }

            SelectVendor();
        }

        private void btnItem_Click(object sender, EventArgs e)
        {
            if (selectedItemId > 0 && activeMode != ReportMode.Item)
            {
                SetActiveMode(ReportMode.Item, true);
                return;
            }

            SelectItem();
        }

        private void btnBoth_Click(object sender, EventArgs e)
        {
            if (selectedVendorId <= 0)
            {
                SelectVendor();
                if (selectedVendorId <= 0)
                    return;
            }

            if (selectedItemId <= 0)
            {
                SelectItem();
                if (selectedItemId <= 0)
                    return;
            }

            SetActiveMode(ReportMode.Both, true);
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ResetFilters();
        }

        private void btnExportGrid_Click(object sender, EventArgs e)
        {
            ExportCurrentData();
        }

        private void btnPreviewGrid_Click(object sender, EventArgs e)
        {
            if (currentData == null || currentData.Rows.Count == 0)
            {
                MessageBox.Show("No rows to preview.", "Vendor Purchase Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ShowReportPreview();
        }

        private void btnPreviewReport_Click(object sender, EventArgs e)
        {
            using (frmReportFormatDialog dialog = new frmReportFormatDialog(
                "VENDOR PURCHASE",
                new[]
                {
                    "VENDOR PURCHASE DETAILS",
                    "VENDOR PURCHASE DETAILS - GROUP BY BILL",
                    "VENDOR PURCHASE SUMMARY"
                }))
            {
                dialog.ShowDialog(this);
            }
        }

        private void SelectVendor()
        {
            using (frmVendorDig dialog = new frmVendorDig())
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    selectedVendorId = dialog.SelectedVendorId;
                    selectedVendorName = dialog.SelectedVendorName ?? string.Empty;
                    txtVendor.Text = selectedVendorName;
                    SetActiveMode(ReportMode.Vendor, true);
                }
            }
        }

        private void SelectItem()
        {
            using (frmdialForItemMaster dialog = new frmdialForItemMaster("frmvendorpurchasereport"))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    var data = dialog.GetSelectedItemData();
                    selectedItemId = GetDictionaryInt(data, "ItemId");
                    selectedItemName = GetDictionaryString(data, "Description");
                    if (string.IsNullOrWhiteSpace(selectedItemName))
                        selectedItemName = GetDictionaryString(data, "ItemName");
                    txtItem.Text = selectedItemName;
                    SetActiveMode(ReportMode.Item, true);
                }
            }
        }

        private void ResetFilters()
        {
            selectedVendorId = 0;
            selectedItemId = 0;
            selectedVendorName = string.Empty;
            selectedItemName = string.Empty;
            activeMode = ReportMode.Overview;
            txtVendor.Clear();
            txtItem.Clear();
            cmbQuickDate.Value = "Today";
            ApplyQuickDate();
            UpdateModeButtons();
            LoadReport();
        }

        private void LoadReport()
        {
            try
            {
                using (VendorPurchaseReportRepository repo = new VendorPurchaseReportRepository())
                {
                    currentData = activeMode == ReportMode.Both && selectedVendorId > 0 && selectedItemId > 0
                        ? repo.GetVendorItemPurchases(GetDateValue(dtpFrom), GetDateValue(dtpTo), selectedVendorId, selectedItemId, GetCompanyId(), GetBranchId(), GetFinYearId())
                        : activeMode == ReportMode.Item && selectedItemId > 0
                            ? repo.GetItemVendorPurchases(GetDateValue(dtpFrom), GetDateValue(dtpTo), selectedItemId, GetCompanyId(), GetBranchId(), GetFinYearId())
                            : repo.GetVendorPurchases(GetDateValue(dtpFrom), GetDateValue(dtpTo),
                            activeMode == ReportMode.Vendor ? selectedVendorId : 0,
                            0, GetCompanyId(), GetBranchId(), GetFinYearId());
                }

                if ((activeMode == ReportMode.Overview || activeMode == ReportMode.Vendor) && currentData != null)
                {
                    currentData = GroupVendorDataByBill(currentData);
                }

                if (currentData != null && (activeMode == ReportMode.Overview || activeMode == ReportMode.Vendor || activeMode == ReportMode.Both))
                {
                    PopulateVendorBillOutstanding();
                }

                gridReport.DataSource = currentData;
                ConfigureGridColumns();
                ApplyUserHiddenColumns();
                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
                UpdateSummary();
                UpdateModeButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load vendor purchase report: " + ex.Message,
                    "Vendor Purchase Report", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataTable GroupVendorDataByBill(DataTable inputTable)
        {
            if (inputTable == null || inputTable.Rows.Count == 0)
                return inputTable;

            DataTable billTable = new DataTable();
            foreach (DataColumn col in inputTable.Columns)
            {
                billTable.Columns.Add(col.ColumnName, col.DataType);
            }

            var groupedRows = inputTable.AsEnumerable()
                .GroupBy(r => {
                    string pNo = inputTable.Columns.Contains("PurchaseNo") && r["PurchaseNo"] != DBNull.Value ? Convert.ToString(r["PurchaseNo"]).Trim() : "";
                    string gNo = inputTable.Columns.Contains("GRNNumber") && r["GRNNumber"] != DBNull.Value ? Convert.ToString(r["GRNNumber"]).Trim() : "";
                    string iNo = inputTable.Columns.Contains("InvoiceNo") && r["InvoiceNo"] != DBNull.Value ? Convert.ToString(r["InvoiceNo"]).Trim() : "";
                    return !string.IsNullOrEmpty(pNo) ? pNo : (!string.IsNullOrEmpty(gNo) ? gNo : iNo);
                });

            int rRank = 1;
            foreach (var group in groupedRows)
            {
                DataRow firstRow = group.First();
                DataRow newRow = billTable.NewRow();

                foreach (DataColumn col in inputTable.Columns)
                {
                    newRow[col.ColumnName] = firstRow[col.ColumnName];
                }

                if (billTable.Columns.Contains("Rank"))
                {
                    newRow["Rank"] = rRank++;
                }

                if (inputTable.Columns.Contains("TotalAmount"))
                {
                    decimal totalAmt = group.Sum(r => r["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(r["TotalAmount"]) : 0m);
                    newRow["TotalAmount"] = totalAmt;
                }
                else if (inputTable.Columns.Contains("Amount"))
                {
                    decimal totalAmt = group.Sum(r => r["Amount"] != DBNull.Value ? Convert.ToDecimal(r["Amount"]) : 0m);
                    newRow["Amount"] = totalAmt;
                }

                if (inputTable.Columns.Contains("Qty"))
                {
                    decimal totalQty = group.Sum(r => r["Qty"] != DBNull.Value ? Convert.ToDecimal(r["Qty"]) : 0m);
                    newRow["Qty"] = totalQty;
                }

                billTable.Rows.Add(newRow);
            }

            return billTable;
        }

        private void PopulateVendorBillOutstanding()
        {
            if (currentData == null || currentData.Rows.Count == 0) return;

            var grnOutstandingMap = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using (var paymentRepo = new Repository.Accounts.VendorPaymentRepository())
                {
                    List<int> vendorIdsToFetch = new List<int>();
                    if (selectedVendorId > 0)
                    {
                        vendorIdsToFetch.Add(selectedVendorId);
                    }
                    else
                    {
                        foreach (DataRow row in currentData.Rows)
                        {
                            int vId = 0;
                            if (currentData.Columns.Contains("VendorLedgerId") && row["VendorLedgerId"] != DBNull.Value)
                                int.TryParse(row["VendorLedgerId"].ToString(), out vId);
                            else if (currentData.Columns.Contains("LedgerID") && row["LedgerID"] != DBNull.Value)
                                int.TryParse(row["LedgerID"].ToString(), out vId);
                            else if (currentData.Columns.Contains("LedgerId") && row["LedgerId"] != DBNull.Value)
                                int.TryParse(row["LedgerId"].ToString(), out vId);
                            else if (currentData.Columns.Contains("VendorId") && row["VendorId"] != DBNull.Value)
                                int.TryParse(row["VendorId"].ToString(), out vId);

                            if (vId > 0 && !vendorIdsToFetch.Contains(vId))
                                vendorIdsToFetch.Add(vId);
                        }
                    }

                    foreach (int vId in vendorIdsToFetch)
                    {
                        DataTable invoices = paymentRepo.GetAllInvoices(vId);
                        if (invoices != null)
                        {
                            foreach (DataRow invRow in invoices.Rows)
                            {
                                string billNo = Convert.ToString(invRow["BillNo"]).Trim();
                                decimal balance = invRow.Table.Columns.Contains("Balance") && invRow["Balance"] != DBNull.Value
                                    ? Convert.ToDecimal(invRow["Balance"])
                                    : 0m;
                                if (!string.IsNullOrEmpty(billNo))
                                {
                                    grnOutstandingMap[billNo] = balance;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error getting vendor invoices for outstanding calculation: " + ex.Message);
            }

            if (!currentData.Columns.Contains("Outstanding"))
            {
                currentData.Columns.Add("Outstanding", typeof(decimal));
            }

            foreach (DataRow row in currentData.Rows)
            {
                string purchaseNo = string.Empty;
                if (currentData.Columns.Contains("PurchaseNo") && row["PurchaseNo"] != DBNull.Value)
                {
                    purchaseNo = Convert.ToString(row["PurchaseNo"]).Trim();
                }
                else if (currentData.Columns.Contains("GRNNumber") && row["GRNNumber"] != DBNull.Value)
                {
                    purchaseNo = Convert.ToString(row["GRNNumber"]).Trim();
                }

                if (!string.IsNullOrEmpty(purchaseNo) && grnOutstandingMap.TryGetValue(purchaseNo, out decimal balVal))
                {
                    row["Outstanding"] = balVal;
                }
                else if (currentData.Columns.Contains("Balance") && row["Balance"] != DBNull.Value)
                {
                    row["Outstanding"] = Convert.ToDecimal(row["Balance"]);
                }
                else
                {
                    row["Outstanding"] = 0m;
                }
            }
        }

        private void ConfigureGridColumns()
        {
            if (gridReport.DisplayLayout.Bands.Count == 0)
                return;

            if (activeMode == ReportMode.Item && selectedItemId > 0)
            {
                ConfigureItemVendorGridColumns();
                return;
            }

            if ((activeMode == ReportMode.Vendor && selectedVendorId > 0)
                || (activeMode == ReportMode.Both && selectedVendorId > 0 && selectedItemId > 0))
            {
                ConfigureVendorDetailGridColumns(activeMode == ReportMode.Both);
                return;
            }

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            foreach (UltraGridColumn column in band.Columns)
            {
                column.Hidden = true;
            }

            int displayIndex = 0;
            ShowColumn("Rank", "#", 45, displayIndex++);
            ShowColumn("PurchaseDate", "Purchase Date", 115, displayIndex++, "dd-MMM-yyyy");
            ShowColumn("InvoiceDate", "Invoice Date", 115, displayIndex++, "dd-MMM-yyyy");
            ShowColumn("PurchaseNo", "Purchase No", 100, displayIndex++);
            ShowColumn("GRNNumber", "GRN No", 105, displayIndex++);

            if (ColumnExists("InvoiceNo"))
                ShowColumn("InvoiceNo", "Invoice No", 105, displayIndex++);

            ShowColumn("Vendor", "Vendor", 200, displayIndex++);

            if (ColumnExists("TotalAmount"))
                ShowColumn("TotalAmount", "Total Amount", 130, displayIndex++, "N2", true);
            else if (ColumnExists("Amount"))
                ShowColumn("Amount", "Total Amount", 130, displayIndex++, "N2", true);

            if (ColumnExists("Outstanding"))
                ShowColumn("Outstanding", "Outstanding", 130, displayIndex++, "N2", true);

            foreach (UltraGridColumn column in band.Columns)
            {
                if (column.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                    column.Hidden = true;
            }
        }

        private void ConfigureVendorDetailGridColumns(bool showVendor)
        {
            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            foreach (UltraGridColumn column in band.Columns)
            {
                column.Hidden = true;
            }

            int displayIndex = 0;
            if (showVendor)
                ShowColumn("Vendor", "Vendor", 200, displayIndex++);

            ShowColumn("PurchaseDate", "Purchase Date", 115, displayIndex++, "dd-MMM-yyyy");
            ShowColumn("InvoiceDate", "Invoice Date", 115, displayIndex++, "dd-MMM-yyyy");
            ShowColumn("PurchaseNo", "Purchase No", 100, displayIndex++);
            ShowColumn("GRNNumber", "GRN No", 105, displayIndex++);

            if (ColumnExists("InvoiceNo"))
                ShowColumn("InvoiceNo", "Invoice No", 105, displayIndex++);

            if (activeMode != ReportMode.Vendor)
            {
                ShowColumn("ItemName", "Item Name", 240, displayIndex++);
                ShowColumn("Qty", "Qty", 85, displayIndex++, "N2", true);
                ShowColumn("Price", "Price", 95, displayIndex++, "N2", true);
            }

            if (ColumnExists("TotalAmount"))
                ShowColumn("TotalAmount", "Total Amount", 130, displayIndex++, "N2", true);
            else if (ColumnExists("Amount"))
                ShowColumn("Amount", "Total Amount", 130, displayIndex++, "N2", true);

            if (ColumnExists("Outstanding"))
                ShowColumn("Outstanding", "Outstanding", 130, displayIndex++, "N2", true);
        }

        private void ConfigureItemVendorGridColumns()
        {
            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            foreach (UltraGridColumn column in band.Columns)
            {
                column.Hidden = true;
            }

            int displayIndex = 0;
            ShowColumn("Vendor", "Vendor", 200, displayIndex++);
            ShowColumn("PurchaseDate", "Purchase Date", 115, displayIndex++, "dd-MMM-yyyy");
            ShowColumn("InvoiceDate", "Invoice Date", 115, displayIndex++, "dd-MMM-yyyy");
            ShowColumn("PurchaseNo", "Purchase No", 100, displayIndex++);
            ShowColumn("GRNNumber", "GRN No", 105, displayIndex++);

            if (ColumnExists("InvoiceNo"))
                ShowColumn("InvoiceNo", "Invoice No", 105, displayIndex++);

            ShowColumn("Qty", "Qty", 85, displayIndex++, "N2", true);
            ShowColumn("Price", "Price", 95, displayIndex++, "N2", true);

            if (ColumnExists("Amount"))
                ShowColumn("Amount", "Amount", 130, displayIndex++, "N2", true);
            else if (ColumnExists("TotalAmount"))
                ShowColumn("TotalAmount", "Amount", 130, displayIndex++, "N2", true);

            // Outstanding column is explicitly hidden in Item mode
        }

        private void SetColumn(string name, string caption, int width, string format = null, bool alignRight = false)
        {
            if (!ColumnExists(name))
                return;

            UltraGridColumn column = gridReport.DisplayLayout.Bands[0].Columns[name];
            column.Header.Caption = caption;
            column.Width = width;
            if (!string.IsNullOrWhiteSpace(format))
                column.Format = format;
            if (alignRight)
                column.CellAppearance.TextHAlign = HAlign.Right;
        }

        private void ShowColumn(string name, string caption, int width, int displayIndex, string format = null, bool alignRight = false)
        {
            if (!ColumnExists(name))
                return;

            SetColumn(name, caption, width, format, alignRight);
            UltraGridColumn column = gridReport.DisplayLayout.Bands[0].Columns[name];
            column.Hidden = false;
            column.Header.VisiblePosition = displayIndex;
        }

        private bool ColumnExists(string name)
        {
            return gridReport.DisplayLayout.Bands.Count > 0
                && gridReport.DisplayLayout.Bands[0].Columns.Exists(name);
        }

        private void SetActiveMode(ReportMode mode, bool reload)
        {
            activeMode = CanUseMode(mode) ? mode : ReportMode.Overview;
            UpdateModeButtons();

            if (reload)
                LoadReport();
        }

        private bool CanUseMode(ReportMode mode)
        {
            if (mode == ReportMode.Vendor)
                return selectedVendorId > 0;
            if (mode == ReportMode.Item)
                return selectedItemId > 0;
            if (mode == ReportMode.Both)
                return selectedVendorId > 0 && selectedItemId > 0;
            return true;
        }

        private void UpdateModeButtons()
        {
            SetModeButtonState(btnVendor, activeMode == ReportMode.Vendor && selectedVendorId > 0);
            SetModeButtonState(btnItem, activeMode == ReportMode.Item && selectedItemId > 0);
            SetModeButtonState(btnBoth, activeMode == ReportMode.Both && selectedVendorId > 0 && selectedItemId > 0);
        }

        private void SetModeButtonState(Button button, bool active)
        {
            if (button == null)
                return;

            button.BackColor = active ? Color.FromArgb(218, 239, 255) : Color.FromArgb(236, 246, 255);
            button.FlatAppearance.BorderColor = active ? accent : skyBlueOutline;
            button.ForeColor = active ? accent : navy;
        }

        private void UpdateSummary()
        {
            int rows = currentData == null ? 0 : currentData.Rows.Count;
            lblTotalRows.Text = rows.ToString("N0", culture);
            lblShowing.Text = "Showing " + rows.ToString("N0", culture) + " record(s)";

            decimal qty = 0;
            decimal amount = 0;
            DataView view = currentData == null ? null : currentData.DefaultView;
            if (view != null)
            {
                foreach (DataRowView rowView in view)
                {
                    qty += ToDecimal(rowView.Row, "Qty");
                    amount += ToDecimal(rowView.Row, currentData.Columns.Contains("TotalAmount") ? "TotalAmount" : "Amount");
                }
            }

            lblTotalQty.Text = qty.ToString("N2", culture);
            lblTotalAmount.Text = Money(amount);

            if (currentData != null && currentData.Columns.Contains("PurchaseNo"))
            {
                DataView distinct = new DataView(currentData);
                DataTable bills = distinct.ToTable(true, "PurchaseNo");
                lblTotalPurchases.Text = bills.Rows.Count.ToString("N0", culture);
            }
            else
            {
                lblTotalPurchases.Text = "0";
            }
        }

        private void ApplyQuickDate()
        {
            suppressQuickDateChange = true;
            DateTime today = DateTime.Today;
            string selected = GetQuickDateText();
            DateTime from = today;
            DateTime to = today;

            if (selected == "All")
            {
                from = new DateTime(2000, 1, 1);
                to = new DateTime(2099, 12, 31);
                if (panelFrom != null) panelFrom.Visible = false;
                if (panelTo != null) panelTo.Visible = false;
            }
            else
            {
                if (panelFrom != null) panelFrom.Visible = true;
                if (panelTo != null) panelTo.Visible = true;

                if (selected == "Yesterday")
                {
                    from = today.AddDays(-1);
                    to = from;
                }
                else if (selected == "Last 7 Days")
                {
                    from = today.AddDays(-6);
                }
                else if (selected == "This Month")
                {
                    from = new DateTime(today.Year, today.Month, 1);
                }
            }

            dtpFrom.Value = from;
            dtpTo.Value = to;
            suppressQuickDateChange = false;
        }

        private void SetCustomQuickDate()
        {
            if (suppressQuickDateChange)
                return;

            if (cmbQuickDate != null && GetQuickDateText() != "Custom")
                cmbQuickDate.Value = "Custom";
        }

        private DateTime GetDateValue(UltraDateTimeEditor picker)
        {
            if (picker.Value == null || picker.Value == DBNull.Value)
                return DateTime.Today;
            return Convert.ToDateTime(picker.Value).Date;
        }

        private string GetQuickDateText()
        {
            return Convert.ToString(cmbQuickDate.Value ?? cmbQuickDate.Text);
        }

        private int GetCompanyId()
        {
            if (SessionContext.IsInitialized && SessionContext.CompanyId > 0)
                return SessionContext.CompanyId;
            int value;
            return int.TryParse(DataBase.CompanyId, out value) && value > 0 ? value : 0;
        }

        private int GetBranchId()
        {
            if (SessionContext.IsInitialized && SessionContext.BranchId > 0)
                return SessionContext.BranchId;
            int value;
            return int.TryParse(DataBase.BranchId, out value) && value > 0 ? value : 0;
        }

        private int GetFinYearId()
        {
            if (SessionContext.IsInitialized && SessionContext.FinYearId > 0)
                return SessionContext.FinYearId;
            int value;
            return int.TryParse(DataBase.FinyearId, out value) && value > 0 ? value : 0;
        }

        private int GetDictionaryInt(System.Collections.Generic.Dictionary<string, object> data, string key)
        {
            object rawValue;
            if (!TryGetDictionaryValue(data, key, out rawValue) || rawValue == null)
                return 0;
            int value;
            return int.TryParse(Convert.ToString(rawValue), out value) ? value : 0;
        }

        private string GetDictionaryString(System.Collections.Generic.Dictionary<string, object> data, string key)
        {
            object rawValue;
            if (!TryGetDictionaryValue(data, key, out rawValue) || rawValue == null)
                return string.Empty;
            return Convert.ToString(rawValue);
        }

        private bool TryGetDictionaryValue(System.Collections.Generic.Dictionary<string, object> data, string key, out object value)
        {
            value = null;
            if (data == null || string.IsNullOrWhiteSpace(key))
                return false;

            if (data.TryGetValue(key, out value))
                return true;

            foreach (System.Collections.Generic.KeyValuePair<string, object> pair in data)
            {
                if (string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    value = pair.Value;
                    return true;
                }
            }

            return false;
        }

        private decimal ToDecimal(DataRow row, string column)
        {
            if (row == null || !row.Table.Columns.Contains(column) || row[column] == DBNull.Value)
                return 0;
            decimal value;
            return decimal.TryParse(Convert.ToString(row[column]), NumberStyles.Any, culture, out value) ? value : 0;
        }

        private string Money(decimal value)
        {
            return "Rs " + value.ToString("N2", culture);
        }

        private void gridReport_Resize(object sender, EventArgs e)
        {
            UpdateFooterCellPositions();
        }

        private void CreateFooterCells()
        {
            ultraPanelGridFooter.ClientArea.Controls.Clear();
            footerLabels.Clear();

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
                footerLabel.BackColor = gridHeaderBlue;
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
                footerLabels[column.Key] = footerLabel;

                if (!columnAggregations.ContainsKey(column.Key))
                {
                    if (column.Key.Equals("TotalAmount", StringComparison.OrdinalIgnoreCase) ||
                        column.Key.Equals("Amount", StringComparison.OrdinalIgnoreCase) ||
                        column.Key.Equals("Qty", StringComparison.OrdinalIgnoreCase) ||
                        column.Key.Equals("Outstanding", StringComparison.OrdinalIgnoreCase))
                    {
                        columnAggregations[column.Key] = "Sum";
                    }
                    else
                    {
                        columnAggregations[column.Key] = "None";
                    }
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

        private void UpdateFooterValues()
        {
            if (footerLabels.Count == 0)
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

        private void UpdateFooterCellPositions()
        {
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0 || footerLabels.Count == 0)
                return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            int rowSelectorWidth = gridReport.DisplayLayout.Override.RowSelectorWidth;
            int scrollOffset = 0;
            if (gridReport.ActiveColScrollRegion != null)
            {
                scrollOffset = gridReport.ActiveColScrollRegion.Position;
            }

            int calculatedX = rowSelectorWidth - scrollOffset;

            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden || !footerLabels.ContainsKey(column.Key))
                    continue;

                Label footerLabel = footerLabels[column.Key];
                var headerUI = column.Header.GetUIElement();
                int left, width;

                if (headerUI != null)
                {
                    left = headerUI.Rect.Left;
                    width = headerUI.Rect.Width;
                }
                else
                {
                    left = calculatedX;
                    width = column.Width;
                }

                calculatedX += column.Width;

                footerLabel.Left = left;
                footerLabel.Width = width;
                footerLabel.Top = 0;
                footerLabel.Height = ultraPanelGridFooter.Height;
                footerLabel.Visible = (left + width > 0 && left < ultraPanelGridFooter.Width);
                footerLabel.Invalidate();
            }
        }

        private IEnumerable<UltraGridRow> GetVisibleDataRows()
        {
            foreach (UltraGridRow row in gridReport.Rows)
            {
                if (row != null && row.IsDataRow && !row.IsFilteredOut)
                    yield return row;
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

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Fill background matching header blue
            Rectangle rect = new Rectangle(0, 0, footerLabel.Width, footerLabel.Height);
            using (SolidBrush bgBrush = new SolidBrush(gridHeaderBlue))
            {
                g.FillRectangle(bgBrush, rect);
            }

            // Draw right and top border grid lines
            using (Pen borderPen = new Pen(Color.FromArgb(118, 154, 198), 1))
            {
                g.DrawLine(borderPen, footerLabel.Width - 1, 0, footerLabel.Width - 1, footerLabel.Height);
                g.DrawLine(borderPen, 0, 0, footerLabel.Width, 0);
            }

            if (string.IsNullOrWhiteSpace(displayText))
            {
                footerLabel.Text = string.Empty;
                return;
            }

            if (columnAggregations.ContainsKey(columnKey) &&
                string.Equals(columnAggregations[columnKey], "None", StringComparison.OrdinalIgnoreCase))
            {
                footerLabel.Text = string.Empty;
                return;
            }

            bool isNumeric = gridReport.DisplayLayout.Bands.Count > 0 &&
                             gridReport.DisplayLayout.Bands[0].Columns.Exists(columnKey) &&
                             IsSummableColumn(gridReport.DisplayLayout.Bands[0].Columns[columnKey]);

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

            footerLabel.Text = string.Empty;
        }

        private void ShowReportPreview()
        {
            using (Form preview = new Form())
            using (Panel header = new Panel())
            using (Panel footer = new Panel())
            using (UltraGrid previewGrid = new UltraGrid())
            {
                preview.Text = "Vendor Purchase Report - Report Preview";
                preview.StartPosition = FormStartPosition.CenterParent;
                preview.WindowState = FormWindowState.Maximized;
                preview.MinimumSize = new Size(1024, 600);
                preview.BackColor = pageBack;
                preview.Padding = new Padding(10);

                header.Dock = DockStyle.Top;
                header.Height = 72;
                header.BackColor = gridHeaderBlueDark;
                header.Padding = new Padding(18, 10, 18, 8);

                Label titleLabel = new Label
                {
                    Dock = DockStyle.Top,
                    Height = 28,
                    Text = "VENDOR PURCHASE REPORT",
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                Label subtitleLabel = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = BuildPreviewSubtitle(),
                    ForeColor = Color.FromArgb(224, 238, 252),
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                header.Controls.Add(subtitleLabel);
                header.Controls.Add(titleLabel);

                previewGrid.Dock = DockStyle.Fill;
                previewGrid.BackColor = Color.White;
                previewGrid.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
                previewGrid.UseAppStyling = false;
                previewGrid.UseOsThemes = DefaultableBoolean.False;
                previewGrid.InitializeLayout += PreviewGrid_InitializeLayout;
                previewGrid.DataSource = currentData.Copy();

                footer.Dock = DockStyle.Bottom;
                footer.Height = 38;
                footer.BackColor = gridHeaderBlue;
                footer.Padding = new Padding(16, 0, 16, 0);

                Label footerLabel = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = BuildPreviewFooterText(),
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleRight
                };
                footer.Controls.Add(footerLabel);

                preview.Controls.Add(previewGrid);
                preview.Controls.Add(footer);
                preview.Controls.Add(header);
                preview.ShowDialog(this);
            }
        }

        private string BuildPreviewSubtitle()
        {
            return string.Format("Quick: {0}    |    Date: {1:dd-MMM-yyyy} to {2:dd-MMM-yyyy}    |    Vendor: {3}    |    Item: {4}",
                GetQuickDateText(),
                GetDateValue(dtpFrom),
                GetDateValue(dtpTo),
                string.IsNullOrWhiteSpace(selectedVendorName) ? "All" : selectedVendorName,
                string.IsNullOrWhiteSpace(selectedItemName) ? "All" : selectedItemName);
        }

        private string BuildPreviewFooterText()
        {
            int rows = currentData == null ? 0 : currentData.Rows.Count;
            string bills = lblTotalPurchases.Text;
            string qty = lblTotalQty.Text;
            string amount = lblTotalAmount.Text;
            return string.Format("Rows: {0:N0}    |    Purchase Bills: {1}    |    Quantity: {2}    |    Amount: {3}",
                rows, bills, qty, amount);
        }

        private void PreviewGrid_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            StylePreviewGridLayout(e.Layout);

            if (e.Layout.Bands.Count == 0 || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand previewBand = e.Layout.Bands[0];
            UltraGridBand sourceBand = gridReport.DisplayLayout.Bands[0];
            foreach (UltraGridColumn column in previewBand.Columns)
            {
                column.Hidden = true;
                if (!sourceBand.Columns.Exists(column.Key))
                    continue;

                UltraGridColumn sourceColumn = sourceBand.Columns[column.Key];
                column.Hidden = sourceColumn.Hidden;
                column.Header.Caption = sourceColumn.Header.Caption;
                column.Header.VisiblePosition = sourceColumn.Header.VisiblePosition;
                column.Width = sourceColumn.Width;
                column.Format = sourceColumn.Format;
                column.CellAppearance.TextHAlign = sourceColumn.CellAppearance.TextHAlign;
            }
        }

        private void StylePreviewGridLayout(UltraGridLayout layout)
        {
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.BorderStyle = UIElementBorderStyle.Solid;
            layout.GroupByBox.Hidden = true;
            layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;

            layout.Override.AllowAddNew = AllowAddNew.No;
            layout.Override.AllowDelete = DefaultableBoolean.False;
            layout.Override.AllowUpdate = DefaultableBoolean.False;
            layout.Override.CellClickAction = CellClickAction.RowSelect;
            layout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            layout.Override.SelectTypeRow = SelectType.Single;
            layout.Override.RowSelectors = DefaultableBoolean.False;
            layout.Override.AllowRowFiltering = DefaultableBoolean.False;

            layout.Appearance.BackColor = pageBack;
            layout.Appearance.BorderColor = Color.FromArgb(118, 154, 198);
            layout.Override.HeaderAppearance.BackColor = gridHeaderBlue;
            layout.Override.HeaderAppearance.BackColor2 = gridHeaderBlueDark;
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor = Color.White;
            layout.Override.HeaderAppearance.BorderColor = Color.FromArgb(118, 154, 198);
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
            layout.Override.DefaultRowHeight = 23;
            layout.Override.MinRowHeight = 23;
        }

        private void ExportCurrentData()
        {
            if (currentData == null || currentData.Rows.Count == 0)
            {
                MessageBox.Show("No rows to export.", "Vendor Purchase Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files (*.csv)|*.csv";
                dialog.FileName = "VendorPurchaseReport_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                File.WriteAllText(dialog.FileName, BuildCsv(currentData), Encoding.UTF8);
                MessageBox.Show("Report exported successfully.", "Vendor Purchase Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private string BuildCsv(DataTable table)
        {
            StringBuilder csv = new StringBuilder();
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (i > 0) csv.Append(',');
                csv.Append(EscapeCsv(table.Columns[i].ColumnName));
            }
            csv.AppendLine();

            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    if (i > 0) csv.Append(',');
                    csv.Append(EscapeCsv(Convert.ToString(row[i])));
                }
                csv.AppendLine();
            }

            return csv.ToString();
        }

        private string EscapeCsv(string value)
        {
            value = value ?? string.Empty;
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private void Card_Paint(object sender, PaintEventArgs e)
        {
            Panel panel = sender as Panel;
            if (panel == null)
                return;

            if (panel.Width < 4 || panel.Height < 4)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
            using (GraphicsPath path = RoundedRect(rect, Math.Min(7, Math.Min(rect.Width, rect.Height) / 2)))
            using (SolidBrush brush = new SolidBrush(panel.BackColor))
            using (Pen pen = new Pen(border))
            {
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(pen, path);
            }
        }

        private GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return new GraphicsPath();

            radius = Math.Max(1, Math.Min(radius, Math.Min(bounds.Width, bounds.Height) / 2));
            int diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        #region Column Chooser & Drag-Down to Hide

        private static Cursor CreateBlackXCursor()
        {
            try
            {
                using (Bitmap bmp = new Bitmap(32, 32))
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);

                    using (SolidBrush bgBrush = new SolidBrush(Color.Black))
                    {
                        g.FillEllipse(bgBrush, 4, 4, 24, 24);
                    }

                    using (Pen whitePen = new Pen(Color.White, 3.5f))
                    {
                        whitePen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                        whitePen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                        g.DrawLine(whitePen, 11, 11, 21, 21);
                        g.DrawLine(whitePen, 21, 11, 11, 21);
                    }

                    IntPtr hIcon = bmp.GetHicon();
                    return new Cursor(hIcon);
                }
            }
            catch
            {
                return Cursors.No;
            }
        }

        private void ApplyUserHiddenColumns()
        {
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
            foreach (UltraGridColumn col in band.Columns)
            {
                if (userHiddenColumnKeys.Contains(col.Key))
                {
                    col.Hidden = true;
                }
            }
        }

        private void SetupHeaderDragToHideAndColumnChooser()
        {
            gridReport.AllowDrop = true;
            gridReport.MouseDown += GridReport_MouseDown;
            gridReport.MouseMove += GridReport_MouseMove;
            gridReport.MouseUp += GridReport_MouseUp;
            gridReport.DragOver += GridReport_DragOver;
            gridReport.DragDrop += GridReport_DragDrop;

            ContextMenuStrip headerMenu = new ContextMenuStrip { Font = new Font("Segoe UI", 9F) };
            ToolStripMenuItem chooserItem = new ToolStripMenuItem("📋 Field / Column Chooser...", null, (s, e) => ShowColumnChooserForm());
            chooserItem.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            headerMenu.Items.Add(chooserItem);

            ToolStripMenuItem showAllItem = new ToolStripMenuItem("🔓 Show / Unhide All Columns", null, (s, e) => UnhideAllColumns());
            headerMenu.Items.Add(showAllItem);

            gridReport.ContextMenuStrip = headerMenu;
        }

        private void GridReport_MouseDown(object sender, MouseEventArgs e)
        {
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            UIElement element = gridReport.DisplayLayout.UIElement?.ElementFromPoint(new Point(e.X, e.Y));
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
        }

        private void GridReport_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDraggingHeaderToHide || columnBeingDragged == null || e.Button != MouseButtons.Left)
                return;

            int deltaX = Math.Abs(e.X - headerDragStartPoint.X);
            int deltaY = e.Y - headerDragStartPoint.Y;

            if (deltaY > 25 && deltaY > deltaX)
            {
                gridReport.Cursor = blackXCursor;
                string colName = !string.IsNullOrEmpty(columnBeingDragged.Header.Caption) ? columnBeingDragged.Header.Caption : columnBeingDragged.Key;
                headerToolTip.SetToolTip(gridReport, $"✖ Drag down to hide '{colName}' column");

                if (deltaY > 50)
                {
                    HideColumn(columnBeingDragged);
                    isDraggingHeaderToHide = false;
                    columnBeingDragged = null;
                    gridReport.Cursor = Cursors.Default;
                    headerToolTip.SetToolTip(gridReport, string.Empty);
                }
            }
        }

        private void GridReport_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDraggingHeaderToHide)
            {
                if (columnBeingDragged != null && (e.Y - headerDragStartPoint.Y) > 40)
                {
                    HideColumn(columnBeingDragged);
                }
                isDraggingHeaderToHide = false;
                columnBeingDragged = null;
                gridReport.Cursor = Cursors.Default;
                headerToolTip.SetToolTip(gridReport, string.Empty);
            }
        }

        private void GridReport_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ColumnChooserItem)))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void GridReport_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(ColumnChooserItem)) is ColumnChooserItem item)
            {
                Point clientPt = gridReport.PointToClient(new Point(e.X, e.Y));
                int dropPosition = GetTargetColumnPositionFromPoint(clientPt);
                UnhideColumn(item.ColumnKey, dropPosition);
            }
        }

        private int GetTargetColumnPositionFromPoint(Point pt)
        {
            if (gridReport.DisplayLayout == null || gridReport.DisplayLayout.Bands.Count == 0)
                return 0;

            UIElement element = gridReport.DisplayLayout.UIElement?.ElementFromPoint(pt);
            HeaderUIElement headerUI = element as HeaderUIElement ?? element?.GetAncestor(typeof(HeaderUIElement)) as HeaderUIElement;

            if (headerUI != null && headerUI.Header?.Column != null)
            {
                return headerUI.Header.Column.Header.VisiblePosition;
            }

            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
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

            ToolStripMenuItem hideItem = new ToolStripMenuItem($"🙈 Hide Column '{colName}'", null, (s, e) => HideColumn(col));
            hideItem.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            menu.Items.Add(hideItem);

            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem chooserItem = new ToolStripMenuItem("📋 Field / Column Chooser...", null, (s, e) => ShowColumnChooserForm());
            menu.Items.Add(chooserItem);

            ToolStripMenuItem showAllItem = new ToolStripMenuItem("🔓 Show / Unhide All Columns", null, (s, e) => UnhideAllColumns());
            menu.Items.Add(showAllItem);

            menu.Show(gridReport, location);
        }

        private void ShowColumnChooserForm()
        {
            if (columnChooserForm == null || columnChooserForm.IsDisposed)
            {
                CreateColumnChooserForm();
            }

            PopulateColumnChooserListBox();
            columnChooserForm.Show(this);
            PositionColumnChooser();
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
            if (columnChooserListBox == null || gridReport.DisplayLayout.Bands.Count == 0)
                return;

            columnChooserListBox.Items.Clear();
            UltraGridBand band = gridReport.DisplayLayout.Bands[0];

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
            if (gridReport.DisplayLayout.Bands.Count > 0 && gridReport.DisplayLayout.Bands[0].Columns.Exists(columnKey))
            {
                UltraGridColumn col = gridReport.DisplayLayout.Bands[0].Columns[columnKey];
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
            if (gridReport.DisplayLayout.Bands.Count == 0) return;
            UltraGridBand band = gridReport.DisplayLayout.Bands[0];
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
            if (columnChooserForm != null && !columnChooserForm.IsDisposed && columnChooserForm.Visible)
            {
                columnChooserForm.Location = new Point(
                    Right - columnChooserForm.Width - 30,
                    Bottom - columnChooserForm.Height - 30);
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

        #endregion
    }
}
