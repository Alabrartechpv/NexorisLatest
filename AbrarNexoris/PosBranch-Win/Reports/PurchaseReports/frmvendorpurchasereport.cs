using ModelClass;
using PosBranch_Win.DialogBox;
using Repository.ReportRepository;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Infragistics.Win;
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
        private readonly CultureInfo culture = new CultureInfo("en-IN");

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
            Item
        }

        public frmvendorpurchasereport()
        {
            InitializeComponent();
            ApplyRuntimeStyles();
        }

        private void frmvendorpurchasereport_Load(object sender, EventArgs e)
        {
            cmbQuickDate.Value = "Today";
            ApplyQuickDate();
            LoadReport();
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
            StyleButton(btnClear, false);
            StyleButton(btnExport, false);
            StyleButton(btnApply, true);

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
            editor.DropDownButtonDisplayStyle = ButtonDisplayStyle.Always;
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

        private void StyleGrid()
        {
            if (gridReport == null)
                return;

            gridReport.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(226, 239, 252);
            gridReport.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            gridReport.ColumnHeadersDefaultCellStyle.ForeColor = navy;
            gridReport.ColumnHeadersHeight = 32;
            gridReport.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            gridReport.DefaultCellStyle.ForeColor = Color.FromArgb(36, 64, 105);
            gridReport.DefaultCellStyle.SelectionBackColor = Color.FromArgb(211, 229, 248);
            gridReport.DefaultCellStyle.SelectionForeColor = navy;
            gridReport.RowTemplate.Height = 28;
        }

        private void cmbQuickDate_ValueChanged(object sender, EventArgs e)
        {
            if (GetQuickDateText() != "Custom")
                ApplyQuickDate();
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

        private void btnApply_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ResetFilters();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportCurrentData();
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
                    currentData = activeMode == ReportMode.Item && selectedItemId > 0
                        ? repo.GetItemVendorPurchases(GetDateValue(dtpFrom), GetDateValue(dtpTo), selectedItemId, GetCompanyId(), GetBranchId(), GetFinYearId())
                        : repo.GetVendorPurchases(GetDateValue(dtpFrom), GetDateValue(dtpTo),
                            activeMode == ReportMode.Vendor ? selectedVendorId : 0,
                            0, GetCompanyId(), GetBranchId(), GetFinYearId());
                }

                gridReport.DataSource = currentData;
                ConfigureGridColumns();
                UpdateSummary();
                UpdateModeButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load vendor purchase report: " + ex.Message,
                    "Vendor Purchase Report", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureGridColumns()
        {
            if (gridReport.Columns.Count == 0)
                return;

            if (activeMode == ReportMode.Item && selectedItemId > 0)
            {
                ConfigureItemVendorGridColumns();
                return;
            }

            if (activeMode == ReportMode.Vendor && selectedVendorId > 0)
            {
                ConfigureVendorDetailGridColumns();
                return;
            }

            SetColumn("Rank", "#", 55);
            SetColumn("Vendor", "Vendor", 190);
            SetColumn("PurchaseDate", "Purchase Date", 115, "dd-MMM-yyyy");
            SetColumn("InvoiceDate", "Invoice Date", 115, "dd-MMM-yyyy");
            SetColumn("PurchaseNo", "Purchase No", 90);
            SetColumn("GRNNumber", "GRN No", 90);
            SetColumn("InvoiceNo", "Invoice No", 105);
            SetColumn("ItemName", "Item Name", 230);
            SetColumn("Qty", "Qty", 80, "N2", true);
            SetColumn("Amount", "Amount", 110, "N2", true);
            SetColumn("TotalAmount", "Total Amount", 120, "N2", true);

            foreach (DataGridViewColumn column in gridReport.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.Automatic;
                if (column.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                    column.Visible = false;
            }
        }

        private void ConfigureVendorDetailGridColumns()
        {
            foreach (DataGridViewColumn column in gridReport.Columns)
            {
                column.Visible = false;
                column.SortMode = DataGridViewColumnSortMode.Automatic;
            }

            int displayIndex = 0;
            ShowColumn("PurchaseDate", "Purchase Date", 140, displayIndex++, "dd-MMM-yyyy");
            ShowColumn("InvoiceDate", "Invoice Date", 140, displayIndex++, "dd-MMM-yyyy");
            ShowColumn("PurchaseNo", "Purchase No", 120, displayIndex++);
            ShowColumn("GRNNumber", "GRN No", 120, displayIndex++);
            ShowColumn("ItemName", "Item Name", 320, displayIndex++);

            if (gridReport.Columns.Contains("TotalAmount"))
                ShowColumn("TotalAmount", "Total Amount", 150, displayIndex++, "N2", true);
            else
                ShowColumn("Amount", "Total Amount", 150, displayIndex++, "N2", true);
        }

        private void ConfigureItemVendorGridColumns()
        {
            foreach (DataGridViewColumn column in gridReport.Columns)
            {
                column.Visible = false;
                column.SortMode = DataGridViewColumnSortMode.Automatic;
            }

            int displayIndex = 0;
            ShowColumn("Vendor", "Vendor", 260, displayIndex++);
            ShowColumn("PurchaseDate", "Purchase Date", 140, displayIndex++, "dd-MMM-yyyy");
            ShowColumn("InvoiceDate", "Invoice Date", 140, displayIndex++, "dd-MMM-yyyy");

            if (gridReport.Columns.Contains("Amount"))
                ShowColumn("Amount", "Amount", 140, displayIndex++, "N2", true);
            else
                ShowColumn("TotalAmount", "Amount", 140, displayIndex++, "N2", true);
        }

        private void SetColumn(string name, string caption, int width, string format = null, bool alignRight = false)
        {
            if (!gridReport.Columns.Contains(name))
                return;

            DataGridViewColumn column = gridReport.Columns[name];
            column.HeaderText = caption;
            column.Width = width;
            column.MinimumWidth = Math.Min(width, 80);
            column.FillWeight = Math.Max(50, width);
            if (!string.IsNullOrWhiteSpace(format))
                column.DefaultCellStyle.Format = format;
            if (alignRight)
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        private void ShowColumn(string name, string caption, int width, int displayIndex, string format = null, bool alignRight = false)
        {
            if (!gridReport.Columns.Contains(name))
                return;

            SetColumn(name, caption, width, format, alignRight);
            DataGridViewColumn column = gridReport.Columns[name];
            column.Visible = true;
            column.DisplayIndex = displayIndex;
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
            return true;
        }

        private void UpdateModeButtons()
        {
            SetModeButtonState(btnVendor, activeMode == ReportMode.Vendor && selectedVendorId > 0);
            SetModeButtonState(btnItem, activeMode == ReportMode.Item && selectedItemId > 0);
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
    }
}
