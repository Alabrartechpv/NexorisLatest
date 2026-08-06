using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Repository;
using Repository.MasterRepositry;
using ModelClass.Master;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;

namespace PosBranch_Win.DialogBox
{
    public partial class CurrencyDialog : Form
    {
        private List<CurrencyModel> originalList = new List<CurrencyModel>();
        private CurrencyRepository repository = new CurrencyRepository();
        private bool isAscendingSort = true;

        // Selected currency information
        public int SelectedCurrencyID { get; private set; }
        public string SelectedCurrencyName { get; private set; }
        public string SelectedCurrencyCode { get; private set; }
        public decimal SelectedExchangeRate { get; private set; }

        public CurrencyDialog()
        {
            InitializeComponent();

            this.Text = "Select Currency";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 244, 250);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(755, 508);

            this.Load += CurrencyDialog_Load;

            // Grid Layout and Key events
            ultraGridCurrency.InitializeLayout += UltraGridCurrency_InitializeLayout;
            ultraGridCurrency.DoubleClickRow += UltraGridCurrency_DoubleClickRow;
            ultraGridCurrency.KeyDown += UltraGridCurrency_KeyDown;

            if (textBoxsearch != null)
            {
                textBoxsearch.TextChanged += TextBoxsearch_TextChanged;
            }

            // Connect OK Button (ultraPanel5)
            if (ultraPanel5 != null) ultraPanel5.Click += (s, e) => SelectCurrency();
            if (label5 != null) label5.Click += (s, e) => SelectCurrency();
            if (ultraPictureBox1 != null) ultraPictureBox1.Click += (s, e) => SelectCurrency();

            // Connect Close Button (ultraPanel6)
            if (ultraPanel6 != null) ultraPanel6.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            if (label3 != null) label3.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            if (ultraPictureBox2 != null) ultraPictureBox2.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            // Connect New/Edit/Del Button (ultraPanel4)
            if (ultraPanel4 != null) ultraPanel4.Click += (s, e) => OpenItemCurrencyMaster();
            if (label4 != null) label4.Click += (s, e) => OpenItemCurrencyMaster();
            if (ultraPictureBox3 != null) ultraPictureBox3.Click += (s, e) => OpenItemCurrencyMaster();

            // Connect Up Button (ultraPanel3 / ultraPictureBox5)
            if (ultraPanel3 != null) ultraPanel3.Click += (s, e) => NavigateRowUp();
            if (ultraPictureBox5 != null) ultraPictureBox5.Click += (s, e) => NavigateRowUp();

            // Connect Down Button (ultraPanel7 / ultraPictureBox6)
            if (ultraPanel7 != null) ultraPanel7.Click += (s, e) => NavigateRowDown();
            if (ultraPictureBox6 != null) ultraPictureBox6.Click += (s, e) => NavigateRowDown();

            // Connect Sort Button (ultraPanel9 / ultraPictureBox4)
            if (ultraPanel9 != null) ultraPanel9.Click += (s, e) => ToggleSortOrder();
            if (ultraPictureBox4 != null) ultraPictureBox4.Click += (s, e) => ToggleSortOrder();

            SetupButtonHoverEffects();
        }

        private void CurrencyDialog_Load(object sender, EventArgs e)
        {
            try
            {
                originalList = repository.GetAllCurrencies();
                if (originalList == null || originalList.Count == 0)
                {
                    Dropdowns drop = new Dropdowns();
                    CurrencyDDLGRID curObj = drop.getCurrency();
                    originalList = curObj?.List?.ToList() ?? new List<CurrencyModel>();
                }

                // Sanitize records
                foreach (var item in originalList)
                {
                    if (item.ExchangeRate <= 0) item.ExchangeRate = 1.0000m;

                    // Clean / Normalize Currency Code (e.g., INR, USD, EUR)
                    if (string.IsNullOrWhiteSpace(item.CurrencyCode) || item.CurrencyCode == "$" || item.CurrencyCode == "₹" || item.CurrencyCode == "?")
                    {
                        if (!string.IsNullOrWhiteSpace(item.CurrencyName))
                        {
                            string nameUpper = item.CurrencyName.ToUpper();
                            if (nameUpper.Contains("RUPEE") || nameUpper.Contains("INR") || nameUpper.Contains("INDIA"))
                                item.CurrencyCode = "INR";
                            else if (nameUpper.Contains("DOLLAR") || nameUpper.Contains("USD") || nameUpper.Contains("US"))
                                item.CurrencyCode = "USD";
                            else if (nameUpper.Contains("EURO") || nameUpper.Contains("EUR"))
                                item.CurrencyCode = "EUR";
                            else if (nameUpper.Contains("POUND") || nameUpper.Contains("GBP"))
                                item.CurrencyCode = "GBP";
                            else if (nameUpper.Contains("DIRHAM") || nameUpper.Contains("AED"))
                                item.CurrencyCode = "AED";
                            else if (nameUpper.Contains("RIYAL") || nameUpper.Contains("SAR"))
                                item.CurrencyCode = "SAR";
                        }
                    }

                    // Clean / Normalize Currency Symbol (e.g., ₹, $)
                    if (string.IsNullOrWhiteSpace(item.CurrencySymbol) || item.CurrencySymbol == "?")
                    {
                        if (string.Equals(item.CurrencyCode, "INR", StringComparison.OrdinalIgnoreCase) ||
                            (item.CurrencyName != null && item.CurrencyName.ToUpper().Contains("RUPEE")))
                        {
                            item.CurrencySymbol = "₹";
                        }
                        else if (string.Equals(item.CurrencyCode, "USD", StringComparison.OrdinalIgnoreCase) ||
                                 (item.CurrencyName != null && item.CurrencyName.ToUpper().Contains("DOLLAR")))
                        {
                            item.CurrencySymbol = "$";
                        }
                        else if (string.Equals(item.CurrencyCode, "EUR", StringComparison.OrdinalIgnoreCase) ||
                                 (item.CurrencyName != null && item.CurrencyName.ToUpper().Contains("EURO")))
                        {
                            item.CurrencySymbol = "€";
                        }
                        else if (string.Equals(item.CurrencyCode, "GBP", StringComparison.OrdinalIgnoreCase) ||
                                 (item.CurrencyName != null && item.CurrencyName.ToUpper().Contains("POUND")))
                        {
                            item.CurrencySymbol = "£";
                        }
                        else
                        {
                            item.CurrencySymbol = !string.IsNullOrWhiteSpace(item.CurrencyCode) ? item.CurrencyCode : "$";
                        }
                    }
                }

                BindGrid(originalList);
                StyleDialogControls();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CurrencyDialog Load error: " + ex.Message);
            }
        }

        private void BindGrid(List<CurrencyModel> data)
        {
            ultraGridCurrency.DataSource = null;
            ultraGridCurrency.DataSource = data;

            if (label1 != null)
            {
                label1.Text = $"Total Currencies Available: {data.Count}";
                label1.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                label1.ForeColor = Color.White;
            }
        }

        private void UltraGridCurrency_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                var layout = e.Layout;
                var band = layout.Bands[0];

                // AutoFit columns across 100% of grid width
                layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;

                // Disable AppStyling / OS Themes for accurate custom rendering
                ultraGridCurrency.UseAppStyling = false;
                ultraGridCurrency.UseOsThemes = DefaultableBoolean.False;

                layout.GroupByBox.Hidden = true;
                layout.Override.DefaultRowHeight = 32;

                // Outer Grid & Container Borders
                layout.BorderStyle = UIElementBorderStyle.Solid;
                layout.Appearance.BorderColor = Color.FromArgb(160, 185, 215);

                // Header styling matching Item Master dialog (Steel blue vertical gradient with white text)
                layout.Override.HeaderStyle = HeaderStyle.WindowsXPCommand;
                layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
                layout.Override.HeaderAppearance.BackColor = Color.FromArgb(46, 93, 144);
                layout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(20, 60, 105);
                layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
                layout.Override.HeaderAppearance.BorderColor = Color.FromArgb(20, 60, 105);
                layout.Override.HeaderAppearance.ForeColor = Color.White;
                layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                layout.Override.HeaderAppearance.FontData.Name = "Segoe UI";
                layout.Override.HeaderAppearance.FontData.SizeInPoints = 9.5F;

                // Row selectors with numbers
                layout.Override.RowSelectors = DefaultableBoolean.True;
                layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.VisibleIndex;
                layout.Override.RowSelectorWidth = 35;
                layout.Override.BorderStyleRowSelector = UIElementBorderStyle.Solid;
                layout.Override.RowSelectorAppearance.BackColor = Color.FromArgb(46, 93, 144);
                layout.Override.RowSelectorAppearance.BackColor2 = Color.FromArgb(20, 60, 105);
                layout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
                layout.Override.RowSelectorAppearance.BorderColor = Color.FromArgb(20, 60, 105);
                layout.Override.RowSelectorAppearance.ForeColor = Color.White;
                layout.Override.RowSelectorAppearance.TextHAlign = HAlign.Center;
                layout.Override.RowSelectorAppearance.FontData.Name = "Segoe UI";
                layout.Override.RowSelectorAppearance.FontData.SizeInPoints = 9F;

                // Cell & Row Borders
                layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
                layout.Override.CellAppearance.BorderColor = Color.FromArgb(208, 223, 238);
                layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
                layout.Override.RowAppearance.BorderColor = Color.FromArgb(208, 223, 238);

                // Row typography and alternating colors
                layout.Override.RowAppearance.FontData.Name = "Segoe UI";
                layout.Override.RowAppearance.FontData.SizeInPoints = 9.5F;
                layout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(245, 248, 252);

                layout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(212, 228, 247);
                layout.Override.ActiveRowAppearance.ForeColor = Color.FromArgb(10, 35, 80);

                layout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(195, 218, 245);
                layout.Override.SelectedRowAppearance.ForeColor = Color.FromArgb(10, 35, 80);

                layout.Override.SelectTypeRow = SelectType.Single;
                layout.Override.CellClickAction = CellClickAction.RowSelect;
                layout.Override.AllowUpdate = DefaultableBoolean.False;

                // Hide all columns first
                foreach (UltraGridColumn col in band.Columns)
                {
                    col.Hidden = true;
                }

                // Show only required columns with clean headers and proportional widths
                if (band.Columns.Exists("CurrencyCode"))
                {
                    var col = band.Columns["CurrencyCode"];
                    col.Hidden = false;
                    col.Header.Caption = "Code";
                    col.Width = 90;
                    col.Header.VisiblePosition = 0;
                    col.CellAppearance.TextHAlign = HAlign.Left;
                }

                if (band.Columns.Exists("CurrencyName"))
                {
                    var col = band.Columns["CurrencyName"];
                    col.Hidden = false;
                    col.Header.Caption = "Currency Name";
                    col.Width = 280;
                    col.Header.VisiblePosition = 1;
                    col.CellAppearance.TextHAlign = HAlign.Left;
                }

                if (band.Columns.Exists("ExchangeRate"))
                {
                    var col = band.Columns["ExchangeRate"];
                    col.Hidden = false;
                    col.Header.Caption = "Exchange Rate";
                    col.Width = 150;
                    col.Format = "#,#0.0000";
                    col.CellAppearance.TextHAlign = HAlign.Right;
                    col.Header.Appearance.TextHAlign = HAlign.Right;
                    col.Header.VisiblePosition = 2;
                }

                if (band.Columns.Exists("CurrencySymbol"))
                {
                    var col = band.Columns["CurrencySymbol"];
                    col.Hidden = false;
                    col.Header.Caption = "Symbol";
                    col.Width = 90;
                    col.CellAppearance.TextHAlign = HAlign.Center;
                    col.Header.Appearance.TextHAlign = HAlign.Center;
                    col.Header.VisiblePosition = 3;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("InitializeLayout error: " + ex.Message);
            }
        }

        private void TextBoxsearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilterAndSort();
        }

        private void ToggleSortOrder()
        {
            isAscendingSort = !isAscendingSort;
            ApplyFilterAndSort();
        }

        private void ApplyFilterAndSort()
        {
            try
            {
                string search = textBoxsearch.Text.Trim().ToLower();
                IEnumerable<CurrencyModel> query = originalList;

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(c =>
                        (!string.IsNullOrEmpty(c.CurrencyCode) && c.CurrencyCode.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(c.CurrencyName) && c.CurrencyName.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(c.CurrencySymbol) && c.CurrencySymbol.ToLower().Contains(search))
                    );
                }

                query = isAscendingSort
                    ? query.OrderBy(c => c.CurrencyName)
                    : query.OrderByDescending(c => c.CurrencyName);

                BindGrid(query.ToList());
            }
            catch { }
        }

        private void StyleDialogControls()
        {
            try
            {
                if (lblSearch != null)
                {
                    lblSearch.Text = "Search Currency:";
                    lblSearch.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                    lblSearch.ForeColor = Color.White;
                }

                if (label2 != null) label2.Visible = false;
                if (comboBox1 != null) comboBox1.Visible = false;
                if (comboBox2 != null) comboBox2.Visible = false;

                if (textBoxsearch != null)
                {
                    textBoxsearch.Font = new Font("Segoe UI", 10F);
                    textBoxsearch.Focus();
                }

                // Apply vibrant GlassTop50 cyan-to-royal-blue button panel styling matching Item Master dialog
                ApplyButtonPanelStyle(ultraPanel5, label5, "OK");
                ApplyButtonPanelStyle(ultraPanel6, label3, "Close");
                ApplyButtonPanelStyle(ultraPanel4, label4, "New / Edit / Del");

                // Style Up/Down Scroll Panels
                ApplyScrollButtonStyle(ultraPanel3);
                ApplyScrollButtonStyle(ultraPanel7);

                // Style Status Bar (ultraPanel8)
                if (ultraPanel8 != null)
                {
                    ultraPanel8.UseAppStyling = false;
                    ultraPanel8.UseOsThemes = DefaultableBoolean.False;
                    ultraPanel8.Appearance.BackColor = Color.FromArgb(0, 116, 217);
                    ultraPanel8.Appearance.BackColor2 = Color.FromArgb(127, 219, 255);
                    ultraPanel8.Appearance.BackGradientStyle = GradientStyle.GlassTop37;
                    ultraPanel8.BorderStyle = UIElementBorderStyle.Rounded1;
                }
            }
            catch { }
        }

        private void ApplyButtonPanelStyle(Infragistics.Win.Misc.UltraPanel panel, Label label, string text)
        {
            if (panel == null) return;
            panel.UseAppStyling = false;
            panel.UseOsThemes = DefaultableBoolean.False;
            panel.Appearance.BackColor = Color.FromArgb(0, 174, 239);
            panel.Appearance.BackColor2 = Color.FromArgb(0, 116, 217);
            panel.Appearance.BackGradientStyle = GradientStyle.GlassTop50;
            panel.BorderStyle = UIElementBorderStyle.Rounded1;

            if (label != null)
            {
                label.Text = text;
                label.ForeColor = Color.White;
                label.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
                label.BackColor = Color.Transparent;
            }
        }

        private void ApplyScrollButtonStyle(Infragistics.Win.Misc.UltraPanel panel)
        {
            if (panel == null) return;
            panel.UseAppStyling = false;
            panel.UseOsThemes = DefaultableBoolean.False;
            panel.Appearance.BackColor = Color.FromArgb(0, 174, 239);
            panel.Appearance.BackColor2 = Color.FromArgb(0, 116, 217);
            panel.Appearance.BackGradientStyle = GradientStyle.GlassTop50;
            panel.BorderStyle = UIElementBorderStyle.Rounded1;
        }

        private void SetupButtonHoverEffects()
        {
            AddHoverEffect(ultraPanel5);
            AddHoverEffect(ultraPanel6);
            AddHoverEffect(ultraPanel4);
            AddHoverEffect(ultraPanel3);
            AddHoverEffect(ultraPanel7);
            AddHoverEffect(ultraPanel9);
        }

        private void AddHoverEffect(Infragistics.Win.Misc.UltraPanel panel)
        {
            if (panel == null) return;
            panel.MouseEnter += (s, e) =>
            {
                panel.Appearance.BackColor = Color.FromArgb(20, 194, 255);
                panel.Appearance.BackColor2 = Color.FromArgb(10, 136, 237);
            };
            panel.MouseLeave += (s, e) =>
            {
                if (panel == ultraPanel9)
                {
                    panel.Appearance.BackColor = Color.White;
                    panel.Appearance.BackColor2 = Color.Empty;
                }
                else
                {
                    panel.Appearance.BackColor = Color.FromArgb(0, 174, 239);
                    panel.Appearance.BackColor2 = Color.FromArgb(0, 116, 217);
                }
            };
        }

        private void NavigateRowUp()
        {
            try
            {
                if (ultraGridCurrency.Rows.Count == 0) return;
                if (ultraGridCurrency.ActiveRow == null)
                {
                    ultraGridCurrency.Rows[0].Activate();
                    ultraGridCurrency.Rows[0].Selected = true;
                    return;
                }
                var prev = ultraGridCurrency.ActiveRow.GetSibling(SiblingRow.Previous);
                if (prev != null && prev.IsDataRow)
                {
                    prev.Activate();
                    prev.Selected = true;
                    ultraGridCurrency.ActiveRowScrollRegion.ScrollRowIntoView(prev);
                }
            }
            catch { }
        }

        private void NavigateRowDown()
        {
            try
            {
                if (ultraGridCurrency.Rows.Count == 0) return;
                if (ultraGridCurrency.ActiveRow == null)
                {
                    ultraGridCurrency.Rows[0].Activate();
                    ultraGridCurrency.Rows[0].Selected = true;
                    return;
                }
                var next = ultraGridCurrency.ActiveRow.GetSibling(SiblingRow.Next);
                if (next != null && next.IsDataRow)
                {
                    next.Activate();
                    next.Selected = true;
                    ultraGridCurrency.ActiveRowScrollRegion.ScrollRowIntoView(next);
                }
            }
            catch { }
        }

        private void OpenItemCurrencyMaster()
        {
            try
            {
                using (Master.FrmCurrency frm = new Master.FrmCurrency())
                {
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        originalList = repository.GetAllCurrencies();
                        BindGrid(originalList);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Currency Master: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UltraGridCurrency_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            SelectCurrency();
        }

        private void UltraGridCurrency_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectCurrency();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                e.Handled = true;
            }
        }

        private void SelectCurrency()
        {
            if (ultraGridCurrency.ActiveRow != null && ultraGridCurrency.ActiveRow.IsDataRow)
            {
                try
                {
                    var row = ultraGridCurrency.ActiveRow;
                    if (row.Cells.Exists("CurrencyID") && row.Cells["CurrencyID"].Value != null)
                    {
                        SelectedCurrencyID = Convert.ToInt32(row.Cells["CurrencyID"].Value);
                    }

                    if (row.Cells.Exists("CurrencyName") && row.Cells["CurrencyName"].Value != null)
                    {
                        SelectedCurrencyName = Convert.ToString(row.Cells["CurrencyName"].Value);
                    }

                    if (row.Cells.Exists("CurrencyCode") && row.Cells["CurrencyCode"].Value != null)
                    {
                        SelectedCurrencyCode = Convert.ToString(row.Cells["CurrencyCode"].Value);
                    }

                    if (row.Cells.Exists("ExchangeRate") && row.Cells["ExchangeRate"].Value != null)
                    {
                        decimal.TryParse(Convert.ToString(row.Cells["ExchangeRate"].Value), out decimal rate);
                        SelectedExchangeRate = rate > 0 ? rate : 1.0000m;
                    }

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error selecting currency: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
