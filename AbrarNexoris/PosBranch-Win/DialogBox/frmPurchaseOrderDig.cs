using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinGrid;
using ModelClass.TransactionModels;
using Repository.TransactionRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.DialogBox
{
    public partial class frmPurchaseOrderDig : Form
    {
        private static readonly Color FormBackColor = Color.FromArgb(214, 230, 240);
        private static readonly Color BorderBlue = Color.FromArgb(118, 154, 198);
        private static readonly Color GridHeaderBlue = Color.FromArgb(93, 151, 214);
        private static readonly Color GridHeaderBlueDark = Color.FromArgb(67, 118, 184);
        private static readonly Color GridSelectedBlue = Color.FromArgb(126, 126, 245);
        private static readonly Color GridRowLine = Color.FromArgb(197, 217, 241);
        private static readonly Color GridAltRow = Color.FromArgb(246, 250, 255);
        private static readonly Color ActionPanelBackColor = Color.FromArgb(206, 223, 238);
        private static readonly Color ButtonBlueTop = Color.FromArgb(232, 241, 252);
        private static readonly Color ButtonBlueBottom = Color.FromArgb(145, 181, 224);
        private static readonly Color ButtonBlueBorder = Color.FromArgb(62, 104, 166);
        private static readonly Color ButtonTextBlue = Color.FromArgb(14, 47, 108);
        private static readonly Color PanelHoverTopColor = Color.FromArgb(245, 250, 255);
        private static readonly Color PanelHoverBottomColor = Color.FromArgb(170, 206, 244);
        private static readonly Color PanelPressedTopColor = Color.FromArgb(205, 226, 248);
        private static readonly Color PanelPressedBottomColor = Color.FromArgb(128, 170, 224);

        private readonly PurchaseOrderRepository _purchaseOrderRepository;
        private DataTable _originalDataTable;
        private bool _handlingSelection;

        public int FilterLedgerId { get; set; }
        public int SelectedPurchaseOrderId { get; private set; }
        public int SelectedPurchaseOrderNo { get; private set; }

        public frmPurchaseOrderDig()
        {
            InitializeComponent();
            _purchaseOrderRepository = new PurchaseOrderRepository();

            Load += frmPurchaseOrderDig_Load;
            Shown += frmPurchaseOrderDig_Shown;
            Searchbx.TextChanged += Searchbx_TextChanged;
            comboBox1.SelectedIndexChanged += FilterControl_Changed;
            comboBox2.SelectedIndexChanged += FilterControl_Changed;
            textBox1.TextChanged += textBox1_TextChanged;
            ultraGrid1.InitializeLayout += ultraGrid1_InitializeLayout;
            ultraGrid1.DoubleClickRow += ultraGrid1_DoubleClickRow;
            ultraGrid1.KeyDown += ultraGrid1_KeyDown;

            InitializeDialog();
        }

        private void InitializeDialog()
        {
            Text = "Purchase Order Lookup";
            StartPosition = FormStartPosition.CenterParent;
            BackColor = FormBackColor;

            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(new object[] { "PO No", "Date", "Total" });
            comboBox1.SelectedIndex = 0;

            comboBox2.Items.Clear();
            comboBox2.Items.AddRange(new object[] { "Newest", "Oldest", "PO No", "Total" });
            comboBox2.SelectedIndex = 0;

            Searchbx.Text = string.Empty;
            textBox1.Text = "500";

            StylePanelButton(ultraPanel11);
            StylePanelButton(ultraPanel12);
            ApplyGridTheme();
            WireSelectionPanels();
        }

        private void frmPurchaseOrderDig_Load(object sender, EventArgs e)
        {
            LoadPurchaseOrders();
        }

        private void frmPurchaseOrderDig_Shown(object sender, EventArgs e)
        {
            Searchbx.Focus();
            Searchbx.SelectAll();
        }

        private void LoadPurchaseOrders()
        {
            try
            {
                List<PurchaseOrderLookupItem> items = _purchaseOrderRepository.GetPurchaseOrders(GetCompanyId(), GetBranchId(), 0, 5000);
                if (FilterLedgerId > 0)
                {
                    items = items.Where(IsPurchaseOrderForFilteredVendor).ToList();
                }

                DataTable table = BuildLookupTable(items);
                _originalDataTable = table.Copy();
                ApplyFilterAndSort();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load purchase orders.\r\n" + ex.Message, "Purchase Order", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsPurchaseOrderForFilteredVendor(PurchaseOrderLookupItem item)
        {
            if (item == null || FilterLedgerId <= 0)
                return true;

            try
            {
                PurchaseOrderLoadResult result = _purchaseOrderRepository.GetPurchaseOrderById(item.Poid);
                return result != null &&
                       result.Master != null &&
                       result.Master.LedgerID == FilterLedgerId;
            }
            catch
            {
                return false;
            }
        }

        private static DataTable BuildLookupTable(IEnumerable<PurchaseOrderLookupItem> items)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Poid", typeof(int));
            table.Columns.Add("PONo", typeof(int));
            table.Columns.Add("DisplayNo", typeof(string));
            table.Columns.Add("PODate", typeof(DateTime));
            table.Columns.Add("DisplayDate", typeof(string));
            table.Columns.Add("GrandTotal", typeof(decimal));

            if (items == null)
                return table;

            foreach (PurchaseOrderLookupItem item in items)
            {
                DataRow row = table.NewRow();
                row["Poid"] = item.Poid;
                row["PONo"] = item.PONo;
                row["DisplayNo"] = "PO-" + item.PONo;
                row["PODate"] = item.PODate == DateTime.MinValue ? DateTime.Today : item.PODate.Date;
                row["DisplayDate"] = item.PODate == DateTime.MinValue ? string.Empty : item.PODate.ToString("dd/MM/yyyy");
                row["GrandTotal"] = Convert.ToDecimal(item.GrandTotal);
                table.Rows.Add(row);
            }

            return table;
        }

        private void FilterControl_Changed(object sender, EventArgs e)
        {
            ApplyFilterAndSort();
        }

        private void Searchbx_TextChanged(object sender, EventArgs e)
        {
            ApplyFilterAndSort();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            ApplyFilterAndSort();
        }

        private void ApplyFilterAndSort()
        {
            if (_originalDataTable == null)
                return;

            IEnumerable<DataRow> rows = _originalDataTable.AsEnumerable();
            string searchText = (Searchbx.Text ?? string.Empty).Trim();
            string searchBy = Convert.ToString(comboBox1.SelectedItem);

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                string lowered = searchText.ToLowerInvariant();
                rows = rows.Where(row => MatchesSearch(row, searchBy, lowered));
            }

            switch (Convert.ToString(comboBox2.SelectedItem))
            {
                case "Oldest":
                    rows = rows.OrderBy(row => row.Field<DateTime>("PODate")).ThenBy(row => row.Field<int>("PONo"));
                    break;
                case "PO No":
                    rows = rows.OrderByDescending(row => row.Field<int>("PONo"));
                    break;
                case "Total":
                    rows = rows.OrderByDescending(row => row.Field<decimal>("GrandTotal")).ThenByDescending(row => row.Field<int>("PONo"));
                    break;
                default:
                    rows = rows.OrderByDescending(row => row.Field<DateTime>("PODate")).ThenByDescending(row => row.Field<int>("PONo"));
                    break;
            }

            int maxRows = ParseInt(textBox1.Text);
            if (maxRows > 0)
                rows = rows.Take(maxRows);

            DataTable filtered = _originalDataTable.Clone();
            foreach (DataRow row in rows)
                filtered.ImportRow(row);

            ultraGrid1.DataSource = filtered;

            if (ultraGrid1.Rows.Count > 0)
            {
                ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
            }
        }

        private static bool MatchesSearch(DataRow row, string searchBy, string loweredSearchText)
        {
            if (row == null)
                return false;

            string target;
            switch (searchBy)
            {
                case "Date":
                    target = Convert.ToString(row["DisplayDate"]);
                    break;
                case "Total":
                    target = Convert.ToDecimal(row["GrandTotal"]).ToString("0.00");
                    break;
                default:
                    target = Convert.ToString(row["DisplayNo"]);
                    break;
            }

            return !string.IsNullOrWhiteSpace(target) &&
                   target.ToLowerInvariant().Contains(loweredSearchText);
        }

        private void ApplyGridTheme()
        {
            ultraGrid1.DisplayLayout.Reset();
            ultraGrid1.UseAppStyling = false;
            ultraGrid1.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = ultraGrid1.DisplayLayout;
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.BorderStyle = UIElementBorderStyle.Solid;
            layout.GroupByBox.Hidden = true;

            layout.Override.AllowAddNew = AllowAddNew.No;
            layout.Override.AllowDelete = DefaultableBoolean.False;
            layout.Override.AllowUpdate = DefaultableBoolean.False;
            layout.Override.CellClickAction = CellClickAction.RowSelect;
            layout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            layout.Override.SelectTypeRow = SelectType.Single;
            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorWidth = 20;
            layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;

            layout.Appearance.BackColor = Color.White;
            layout.Appearance.BackColor2 = FormBackColor;
            layout.Appearance.BackGradientStyle = GradientStyle.None;
            layout.Appearance.BorderColor = BorderBlue;

            layout.Override.RowSelectorAppearance.BackColor = GridHeaderBlueDark;
            layout.Override.RowSelectorAppearance.BackColor2 = GridHeaderBlue;
            layout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.RowSelectorAppearance.BorderColor = BorderBlue;
            layout.Override.RowSelectorAppearance.ForeColor = Color.White;
            layout.Override.RowSelectorAppearance.TextHAlign = HAlign.Center;

            layout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            layout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor = Color.White;
            layout.Override.HeaderAppearance.BorderColor = BorderBlue;
            layout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;

            layout.Override.RowAppearance.BackColor = Color.White;
            layout.Override.RowAlternateAppearance.BackColor = GridAltRow;
            layout.Override.RowAppearance.BorderColor = GridRowLine;
            layout.Override.RowAlternateAppearance.BorderColor = GridRowLine;
            layout.Override.ActiveRowAppearance.BackColor = GridSelectedBlue;
            layout.Override.ActiveRowAppearance.ForeColor = Color.White;
            layout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
            layout.Override.SelectedRowAppearance.ForeColor = Color.White;
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
            layout.ScrollBounds = ScrollBounds.ScrollToFill;
            layout.ScrollStyle = ScrollStyle.Immediate;
            layout.ScrollBarLook.Appearance.BackColor = ActionPanelBackColor;
            layout.ScrollBarLook.Appearance.BorderColor = BorderBlue;
            layout.ScrollBarLook.TrackAppearance.BackColor = Color.FromArgb(225, 236, 246);
            layout.ScrollBarLook.ButtonAppearance.BackColor = GridHeaderBlue;
            layout.ScrollBarLook.ButtonAppearance.BackColor2 = GridHeaderBlueDark;
            layout.ScrollBarLook.ButtonAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.ScrollBarLook.ButtonAppearance.BorderColor = BorderBlue;

            ultraGrid1.BackColor = FormBackColor;
            ultraGrid1.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        }

        private void ultraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            if (e.Layout.Bands.Count == 0)
                return;

            UltraGridBand band = e.Layout.Bands[0];
            foreach (UltraGridColumn column in band.Columns)
                column.Hidden = true;

            ConfigureColumn(band, "DisplayNo", "PO No", 115, null, HAlign.Left, 0);
            ConfigureColumn(band, "DisplayDate", "Date", 110, null, HAlign.Center, 1);
            ConfigureColumn(band, "GrandTotal", "Total", 120, "#,##0.00", HAlign.Right, 2);

            e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
        }

        private static void ConfigureColumn(UltraGridBand band, string key, string header, int width, string format, HAlign alignment, int visiblePosition)
        {
            if (!band.Columns.Exists(key))
                return;

            UltraGridColumn column = band.Columns[key];
            column.Hidden = false;
            column.Header.Caption = header;
            column.Width = width;
            column.Header.VisiblePosition = visiblePosition;
            column.CellAppearance.TextHAlign = alignment;

            if (!string.IsNullOrWhiteSpace(format))
                column.Format = format;
        }

        private void WireSelectionPanels()
        {
            WirePanelClick(ultraPanel12, ConfirmSelection);
            WirePanelClick(ultraPanel11, CloseLookup);
        }

        private void WirePanelClick(UltraPanel panel, EventHandler handler)
        {
            if (panel == null)
                return;

            panel.Click += handler;
            panel.ClientArea.Click += handler;

            foreach (Control control in panel.ClientArea.Controls)
                control.Click += handler;
        }

        private void StylePanelButton(UltraPanel panel)
        {
            if (panel == null)
                return;

            panel.UseAppStyling = false;
            panel.Cursor = Cursors.Hand;
            panel.BorderStyle = UIElementBorderStyle.Rounded1;
            ApplyPanelButtonStyle(panel, false, false);

            panel.MouseEnter += Panel_MouseEnter;
            panel.MouseLeave += Panel_MouseLeave;
            panel.MouseDown += Panel_MouseDown;
            panel.MouseUp += Panel_MouseUp;
            panel.ClientArea.MouseEnter += Panel_MouseEnter;
            panel.ClientArea.MouseLeave += Panel_MouseLeave;
            panel.ClientArea.MouseDown += Panel_MouseDown;
            panel.ClientArea.MouseUp += Panel_MouseUp;

            foreach (Control control in panel.ClientArea.Controls)
            {
                control.Cursor = Cursors.Hand;
                control.MouseEnter += Panel_MouseEnter;
                control.MouseLeave += Panel_MouseLeave;
                control.MouseDown += Panel_MouseDown;
                control.MouseUp += Panel_MouseUp;
            }
        }

        private static void ApplyPanelButtonStyle(UltraPanel panel, bool isHover, bool isPressed)
        {
            AppearanceBase appearance = panel.Appearance;
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

        private void Panel_MouseEnter(object sender, EventArgs e)
        {
            UltraPanel panel = GetActionPanel(sender);
            if (panel != null)
                ApplyPanelButtonStyle(panel, true, false);
        }

        private void Panel_MouseLeave(object sender, EventArgs e)
        {
            UltraPanel panel = GetActionPanel(sender);
            if (panel == null)
                return;

            Point point = panel.PointToClient(Control.MousePosition);
            ApplyPanelButtonStyle(panel, panel.ClientRectangle.Contains(point), false);
        }

        private void Panel_MouseDown(object sender, MouseEventArgs e)
        {
            UltraPanel panel = GetActionPanel(sender);
            if (panel != null && e.Button == MouseButtons.Left)
                ApplyPanelButtonStyle(panel, true, true);
        }

        private void Panel_MouseUp(object sender, MouseEventArgs e)
        {
            UltraPanel panel = GetActionPanel(sender);
            if (panel == null)
                return;

            Point point = panel.PointToClient(Control.MousePosition);
            ApplyPanelButtonStyle(panel, panel.ClientRectangle.Contains(point), false);
        }

        private void ultraGrid1_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            ConfirmSelection(sender, EventArgs.Empty);
        }

        private void ultraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ConfirmSelection(sender, EventArgs.Empty);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                CloseLookup(sender, EventArgs.Empty);
                e.Handled = true;
            }
        }

        private void ConfirmSelection(object sender, EventArgs e)
        {
            if (_handlingSelection)
                return;

            _handlingSelection = true;
            try
            {
                if (ultraGrid1.ActiveRow == null)
                {
                    MessageBox.Show("Please select a purchase order.", "Purchase Order", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                SelectedPurchaseOrderId = Convert.ToInt32(ultraGrid1.ActiveRow.Cells["Poid"].Value);
                SelectedPurchaseOrderNo = Convert.ToInt32(ultraGrid1.ActiveRow.Cells["PONo"].Value);
                DialogResult = DialogResult.OK;
                Close();
            }
            finally
            {
                _handlingSelection = false;
            }
        }

        private void CloseLookup(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private static int ParseInt(string text)
        {
            int value;
            return int.TryParse(text, out value) ? value : 0;
        }

        private static int GetCompanyId()
        {
            if (ModelClass.SessionContext.IsInitialized && ModelClass.SessionContext.CompanyId > 0)
                return ModelClass.SessionContext.CompanyId;

            return ParseInt(Convert.ToString(ModelClass.DataBase.CompanyId));
        }

        private static int GetBranchId()
        {
            if (ModelClass.SessionContext.IsInitialized && ModelClass.SessionContext.BranchId > 0)
                return ModelClass.SessionContext.BranchId;

            return ParseInt(Convert.ToString(ModelClass.DataBase.BranchId));
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                CloseLookup(this, EventArgs.Empty);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
