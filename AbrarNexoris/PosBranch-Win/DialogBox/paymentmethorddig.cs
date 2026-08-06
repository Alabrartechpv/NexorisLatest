using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinGrid;
using ModelClass.Master;
using PosBranch_Win.Master;
using Repository.MasterRepositry;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.DialogBox
{
    public partial class paymentmethorddig : Form
    {
        private readonly PaymodeRepository repo = new PaymodeRepository();
        private readonly Color pageBack = Color.FromArgb(232, 246, 255);
        private readonly Color midPearlBlue = Color.FromArgb(198, 222, 248);
        private readonly Color gridHeaderBlue = Color.FromArgb(93, 151, 214);
        private readonly Color gridHeaderBlueDark = Color.FromArgb(67, 118, 184);
        private readonly Color gridSelectedBlue = Color.FromArgb(126, 126, 245);
        private readonly Color gridRowLine = Color.FromArgb(197, 217, 241);
        private readonly Color gridAltRow = Color.FromArgb(246, 250, 255);
        private readonly Color gridFooterBorder = Color.FromArgb(144, 181, 223);

        private readonly Dictionary<string, Label> footerLabels = new Dictionary<string, Label>();
        private readonly Dictionary<string, string> columnAggregations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private List<PaymodeModel> allPaymodesList = new List<PaymodeModel>();
        private bool isInternalTextBox3Change = false;

        public int SelectedPaymodeId { get; private set; }
        public string SelectedPaymodeName { get; private set; }
        public PaymodeModel SelectedPaymodeModel { get; private set; }

        public paymentmethorddig()
        {
            InitializeComponent();
            RegisterGridEvents();
            ApplyRuntimeStyles();
            InitSearchControls();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            StyleAllUltraPanels();
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            StyleAllUltraPanels();
        }

        private void paymentmethorddig_Load(object sender, EventArgs e)
        {
            LoadPaymodeData();
        }

        private void panelGridContainer_Paint(object sender, PaintEventArgs e)
        {
        }

        private void InitSearchControls()
        {
            if (comboBox1 != null)
            {
                comboBox1.SelectedIndexChanged -= SearchControl_Changed;
                comboBox1.Items.Clear();
                comboBox1.Items.Add("ALL");
                comboBox1.Items.Add("PM_CODE");
                comboBox1.Items.Add("PM_DESCRIPTION");
                comboBox1.Items.Add("PM_SHORTKEY");
                comboBox1.Items.Add("PM_ACCOUNT");
                comboBox1.SelectedIndex = 0;
                comboBox1.SelectedIndexChanged += SearchControl_Changed;
            }

            if (comboBox2 != null)
            {
                comboBox2.SelectedIndexChanged -= SearchControl_Changed;
                comboBox2.Items.Clear();
                comboBox2.Items.Add("PM_CODE");
                comboBox2.Items.Add("PM_DESCRIPTION");
                comboBox2.Items.Add("PM_SHORTKEY");
                comboBox2.Items.Add("PM_ACCOUNT");
                comboBox2.SelectedIndex = 0;
                comboBox2.SelectedIndexChanged += SearchControl_Changed;
            }

            if (textBoxsearch != null)
            {
                textBoxsearch.TextChanged -= SearchControl_Changed;
                textBoxsearch.TextChanged += SearchControl_Changed;
            }

            if (textBox3 != null)
            {
                textBox3.TextChanged -= TextBox3_TextChanged;
                textBox3.TextChanged += TextBox3_TextChanged;
            }
        }

        private void SearchControl_Changed(object sender, EventArgs e)
        {
            ApplyFilterAndPagination();
        }

        private void TextBox3_TextChanged(object sender, EventArgs e)
        {
            if (isInternalTextBox3Change) return;
            ApplyFilterAndPagination();
        }

        private void ApplyRuntimeStyles()
        {
            BackColor = midPearlBlue;

            StyleAllUltraPanels();
            StyleGrid();

            if (ultraPanelGridFooter != null)
            {
                ultraPanelGridFooter.Appearance.BackColor = gridHeaderBlue;
                ultraPanelGridFooter.Appearance.BackColor2 = gridHeaderBlue;
                ultraPanelGridFooter.Appearance.BackGradientStyle = GradientStyle.None;
                ultraPanelGridFooter.Appearance.BorderColor = gridFooterBorder;
                ultraPanelGridFooter.BorderStyle = UIElementBorderStyle.Solid;
            }
        }

        private void RegisterGridEvents()
        {
            Load += paymentmethorddig_Load;

            ultraGrid1.InitializeLayout += (s, e) =>
            {
                ConfigureGridColumns();
                CreateFooterCells();
                UpdateFooterCellPositions();
                UpdateFooterValues();
            };

            ultraGrid1.Resize += (s, e) => UpdateFooterCellPositions();
            ultraGrid1.AfterColPosChanged += (s, e) => UpdateFooterCellPositions();
            ultraGrid1.AfterColRegionScroll += (s, e) => UpdateFooterCellPositions();
            ultraGrid1.AfterRowRegionScroll += (s, e) => UpdateFooterCellPositions();
            ultraGrid1.Paint += (s, e) => UpdateFooterCellPositions();
            ultraGrid1.DoubleClickRow += ultraGrid1_DoubleClickRow;
            ultraGrid1.KeyDown += ultraGrid1_KeyDown;

            ConnectPanelClickEvents();
        }

        private void StyleAllUltraPanels()
        {
            UltraPanel[] panels = { ultraPanel5, ultraPanel6, ultraPanel7, ultraPanel3 };
            foreach (var panel in panels)
            {
                if (panel != null)
                {
                    StyleIconPanel(panel);
                }
            }
        }

        private void StyleIconPanel(UltraPanel panel)
        {
            if (panel == null) return;

            panel.UseAppStyling = false;

            Color topColor = Color.FromArgb(234, 244, 255);       // #EAF4FF
            Color bottomColor = Color.FromArgb(152, 188, 235);    // #98BCEB
            Color borderColor = Color.FromArgb(73, 119, 184);     // #4977B8
            Color textColor = Color.FromArgb(0, 46, 127);         // #002E7F dark blue

            Color hoverTop = Color.FromArgb(245, 250, 255);
            Color hoverBottom = Color.FromArgb(170, 206, 244);

            Color pressedTop = Color.FromArgb(205, 226, 248);
            Color pressedBottom = Color.FromArgb(128, 170, 224);

            panel.Appearance.BackColor = topColor;
            panel.Appearance.BackColor2 = bottomColor;
            panel.Appearance.BackGradientStyle = GradientStyle.Vertical;

            panel.BorderStyle = UIElementBorderStyle.Rounded1;
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

            panel.MouseEnter -= (s, e) => setHoverState();
            panel.MouseLeave -= (s, e) => setNormalState();
            panel.MouseDown -= (s, e) => setPressedState();
            panel.MouseUp -= (s, e) => setHoverState();

            panel.MouseEnter += (s, e) => setHoverState();
            panel.MouseLeave += (s, e) => setNormalState();
            panel.MouseDown += (s, e) => setPressedState();
            panel.MouseUp += (s, e) => setHoverState();

            panel.ClientArea.MouseEnter -= (s, e) => setHoverState();
            panel.ClientArea.MouseLeave -= (s, e) => setNormalState();
            panel.ClientArea.MouseDown -= (s, e) => setPressedState();
            panel.ClientArea.MouseUp -= (s, e) => setHoverState();

            panel.ClientArea.MouseEnter += (s, e) => setHoverState();
            panel.ClientArea.MouseLeave += (s, e) => setNormalState();
            panel.ClientArea.MouseDown += (s, e) => setPressedState();
            panel.ClientArea.MouseUp += (s, e) => setHoverState();

            foreach (Control control in panel.ClientArea.Controls)
            {
                control.Cursor = Cursors.Hand;

                if (control is Label lbl)
                {
                    lbl.BackColor = Color.Transparent;
                    lbl.ForeColor = textColor;
                    lbl.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
                }
                else if (control is Infragistics.Win.UltraWinEditors.UltraPictureBox pic)
                {
                    pic.BackColor = Color.Transparent;
                }

                control.MouseEnter -= (s, e) => setHoverState();
                control.MouseLeave -= (s, e) => setNormalState();
                control.MouseDown -= (s, e) => setPressedState();
                control.MouseUp -= (s, e) => setHoverState();

                control.MouseEnter += (s, e) => setHoverState();
                control.MouseLeave += (s, e) => setNormalState();
                control.MouseDown += (s, e) => setPressedState();
                control.MouseUp += (s, e) => setHoverState();
            }

            panel.ClientArea.Cursor = Cursors.Hand;
        }

        private void ConnectPanelClickEvents()
        {
            ConnectClick(ultraPanel3, btnUpArrow_Click);
            ConnectClick(ultraPanel7, btnDownArrow_Click);
            ConnectClick(ultraPanel5, btnOk_Click);
            ConnectClick(ultraPanel6, btnCloseDialog_Click);
        }

        private void ConnectClick(Control ctrl, EventHandler handler)
        {
            if (ctrl == null) return;
            ctrl.Click -= handler;
            ctrl.Click += handler;

            if (ctrl is UltraPanel p)
            {
                p.ClientArea.Click -= handler;
                p.ClientArea.Click += handler;
                foreach (Control c in p.ClientArea.Controls)
                {
                    c.Click -= handler;
                    c.Click += handler;
                }
            }
        }

        private void btnUpArrow_Click(object sender, EventArgs e)
        {
            if (ultraGrid1.Rows == null || ultraGrid1.Rows.Count == 0) return;
            int currentIndex = ultraGrid1.ActiveRow != null ? ultraGrid1.ActiveRow.Index : 0;
            if (currentIndex > 0)
            {
                ultraGrid1.ActiveRow = ultraGrid1.Rows[currentIndex - 1];
                ultraGrid1.ActiveRow.Selected = true;
            }
        }

        private void btnDownArrow_Click(object sender, EventArgs e)
        {
            if (ultraGrid1.Rows == null || ultraGrid1.Rows.Count == 0) return;
            int currentIndex = ultraGrid1.ActiveRow != null ? ultraGrid1.ActiveRow.Index : -1;
            if (currentIndex < ultraGrid1.Rows.Count - 1)
            {
                ultraGrid1.ActiveRow = ultraGrid1.Rows[currentIndex + 1];
                ultraGrid1.ActiveRow.Selected = true;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (ultraGrid1.ActiveRow != null && ultraGrid1.ActiveRow.IsDataRow)
            {
                SelectAndClose(ultraGrid1.ActiveRow);
            }
            else if (ultraGrid1.Rows != null && ultraGrid1.Rows.Count > 0)
            {
                SelectAndClose(ultraGrid1.Rows[0]);
            }
        }

        private void btnCloseDialog_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void StyleGrid()
        {
            if (ultraGrid1 == null) return;

            ultraGrid1.DisplayLayout.Reset();
            ultraGrid1.UseAppStyling = false;
            ultraGrid1.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = ultraGrid1.DisplayLayout;
            layout.CaptionVisible = DefaultableBoolean.False;
            layout.BorderStyle = UIElementBorderStyle.Solid;

            layout.GroupByBox.Hidden = true;

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

            layout.Override.RowSelectorAppearance.BackColor = gridHeaderBlueDark;
            layout.Override.RowSelectorAppearance.BackColor2 = gridHeaderBlue;
            layout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.RowSelectorAppearance.BorderColor = Color.FromArgb(118, 154, 198);
            layout.Override.RowSelectorAppearance.ForeColor = Color.White;

            layout.Override.HeaderAppearance.BackColor = gridHeaderBlue;
            layout.Override.HeaderAppearance.BackColor2 = gridHeaderBlueDark;
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor = Color.White;
            layout.Override.HeaderAppearance.BorderColor = Color.FromArgb(118, 154, 198);
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.HeaderAppearance.FontData.Name = "Segoe UI";
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 9F;

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
            layout.Override.CellAppearance.FontData.Name = "Segoe UI";
            layout.Override.CellAppearance.FontData.SizeInPoints = 9F;
            layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            layout.Override.MinRowHeight = 22;
            layout.Override.DefaultRowHeight = 22;
            layout.RowConnectorStyle = RowConnectorStyle.None;

            // Make cell positions free without stretching or forcing auto-fit
            layout.AutoFitStyle = AutoFitStyle.None;
        }

        public void LoadPaymodeData()
        {
            try
            {
                allPaymodesList = repo.GetAllPaymodes() ?? new List<PaymodeModel>();

                isInternalTextBox3Change = true;
                if (textBox3 != null)
                {
                    textBox3.Text = allPaymodesList.Count.ToString();
                }
                isInternalTextBox3Change = false;

                ApplyFilterAndPagination();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading paymodes: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyFilterAndPagination()
        {
            if (allPaymodesList == null) return;

            string searchText = textBoxsearch != null ? textBoxsearch.Text.Trim() : string.Empty;
            string searchField = comboBox1 != null && comboBox1.SelectedItem != null ? comboBox1.SelectedItem.ToString() : "ALL";
            string sortField = comboBox2 != null && comboBox2.SelectedItem != null ? comboBox2.SelectedItem.ToString() : "PM_CODE";

            IEnumerable<PaymodeModel> filtered = allPaymodesList.AsEnumerable();

            if (!string.IsNullOrEmpty(searchText))
            {
                if (searchField.Equals("PM_CODE", StringComparison.OrdinalIgnoreCase))
                {
                    filtered = filtered.Where(p => p.PayModeName != null && p.PayModeName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
                }
                else if (searchField.Equals("PM_DESCRIPTION", StringComparison.OrdinalIgnoreCase))
                {
                    filtered = filtered.Where(p => p.Description != null && p.Description.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
                }
                else if (searchField.Equals("PM_SHORTKEY", StringComparison.OrdinalIgnoreCase))
                {
                    filtered = filtered.Where(p => p.FunctionKey != null && p.FunctionKey.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
                }
                else if (searchField.Equals("PM_ACCOUNT", StringComparison.OrdinalIgnoreCase))
                {
                    filtered = filtered.Where(p => p.LedgerName != null && p.LedgerName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
                }
                else // ALL
                {
                    filtered = filtered.Where(p =>
                        (p.PayModeName != null && p.PayModeName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (p.Description != null && p.Description.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (p.FunctionKey != null && p.FunctionKey.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (p.LedgerName != null && p.LedgerName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    );
                }
            }

            if (sortField.Equals("PM_DESCRIPTION", StringComparison.OrdinalIgnoreCase))
            {
                filtered = filtered.OrderBy(p => p.Description);
            }
            else if (sortField.Equals("PM_SHORTKEY", StringComparison.OrdinalIgnoreCase))
            {
                filtered = filtered.OrderBy(p => p.FunctionKey);
            }
            else if (sortField.Equals("PM_ACCOUNT", StringComparison.OrdinalIgnoreCase))
            {
                filtered = filtered.OrderBy(p => p.LedgerName);
            }
            else
            {
                filtered = filtered.OrderBy(p => p.PayModeName);
            }

            List<PaymodeModel> resultList = filtered.ToList();

            if (textBox3 != null && int.TryParse(textBox3.Text.Trim(), out int maxRows) && maxRows > 0)
            {
                resultList = resultList.Take(maxRows).ToList();
            }

            ultraGrid1.DataSource = resultList;

            ConfigureGridColumns();
            CreateFooterCells();
            UpdateFooterCellPositions();
            UpdateFooterValues();
        }

        private void ConfigureGridColumns()
        {
            if (ultraGrid1.DisplayLayout.Bands.Count == 0) return;
            UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];

            foreach (UltraGridColumn col in band.Columns)
            {
                col.Hidden = true;
            }

            if (band.Columns.Exists("PayModeName"))
            {
                band.Columns["PayModeName"].Hidden = false;
                band.Columns["PayModeName"].Header.Caption = "PM_CODE";
                band.Columns["PayModeName"].Header.VisiblePosition = 0;
                band.Columns["PayModeName"].Width = 140;
            }

            if (band.Columns.Exists("Description"))
            {
                band.Columns["Description"].Hidden = false;
                band.Columns["Description"].Header.Caption = "PM_DESCRIPTION";
                band.Columns["Description"].Header.VisiblePosition = 1;
                band.Columns["Description"].Width = 260;
            }

            if (band.Columns.Exists("FunctionKey"))
            {
                band.Columns["FunctionKey"].Hidden = false;
                band.Columns["FunctionKey"].Header.Caption = "PM_SHORTKEY";
                band.Columns["FunctionKey"].Header.VisiblePosition = 2;
                band.Columns["FunctionKey"].Width = 110;
            }

            if (band.Columns.Exists("LedgerName"))
            {
                band.Columns["LedgerName"].Hidden = false;
                band.Columns["LedgerName"].Header.Caption = "PM_ACCOUNT";
                band.Columns["LedgerName"].Header.VisiblePosition = 3;
                band.Columns["LedgerName"].Width = 160;
            }
        }

        private void CreateFooterCells()
        {
            if (ultraPanelGridFooter == null) return;
            ultraPanelGridFooter.ClientArea.Controls.Clear();
            footerLabels.Clear();

            if (ultraGrid1.DisplayLayout == null || ultraGrid1.DisplayLayout.Bands.Count == 0) return;

            UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
            int xOffset = ultraGrid1.DisplayLayout.Override.RowSelectorWidth;

            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden) continue;

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
                footerLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                footerLabel.ContextMenuStrip = CreateFooterContextMenu(column.Key);

                ultraPanelGridFooter.ClientArea.Controls.Add(footerLabel);
                footerLabels[column.Key] = footerLabel;

                // Make ultraPanelGridFooter on default none for all columns
                if (!columnAggregations.ContainsKey(column.Key))
                {
                    columnAggregations[column.Key] = "None";
                }

                xOffset += column.Width;
            }
        }

        private ContextMenuStrip CreateFooterContextMenu(string columnKey)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Tag = columnKey;

            string currentAgg = columnAggregations.ContainsKey(columnKey) ? columnAggregations[columnKey] : "None";

            string[] options = { "None", "Sum", "Count", "Average", "Min", "Max" };
            foreach (string opt in options)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(opt);
                item.Checked = (opt.Equals(currentAgg, StringComparison.OrdinalIgnoreCase));
                item.Click += (s, e) =>
                {
                    columnAggregations[columnKey] = opt;
                    UpdateFooterValues();
                };
                menu.Items.Add(item);
            }
            return menu;
        }

        private void UpdateFooterCellPositions()
        {
            if (ultraPanelGridFooter == null || ultraGrid1.DisplayLayout == null || ultraGrid1.DisplayLayout.Bands.Count == 0) return;

            UltraGridBand band = ultraGrid1.DisplayLayout.Bands[0];
            int xOffset = ultraGrid1.DisplayLayout.Override.RowSelectorWidth;

            foreach (UltraGridColumn column in band.Columns.Cast<UltraGridColumn>().OrderBy(c => c.Header.VisiblePosition))
            {
                if (column.Hidden) continue;

                if (footerLabels.TryGetValue(column.Key, out Label label))
                {
                    label.Left = xOffset;
                    label.Width = column.Width;
                    label.Height = Math.Max(ultraPanelGridFooter.Height - 2, 20);
                }
                xOffset += column.Width;
            }
        }

        private void UpdateFooterValues()
        {
            if (ultraGrid1.Rows == null || ultraGrid1.Rows.Count == 0)
            {
                foreach (var kvp in footerLabels) kvp.Value.Text = string.Empty;
                return;
            }

            int count = ultraGrid1.Rows.Count;

            foreach (var kvp in footerLabels)
            {
                string colKey = kvp.Key;
                Label lbl = kvp.Value;

                string agg = columnAggregations.ContainsKey(colKey) ? columnAggregations[colKey] : "None";
                if (agg.Equals("None", StringComparison.OrdinalIgnoreCase))
                {
                    lbl.Text = string.Empty;
                    continue;
                }

                if (agg.Equals("Count", StringComparison.OrdinalIgnoreCase))
                {
                    lbl.Text = $"Count: {count}";
                }
            }
        }

        private void ultraGrid1_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            if (e.Row != null && e.Row.IsDataRow)
            {
                SelectAndClose(e.Row);
            }
        }

        private void ultraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && ultraGrid1.ActiveRow != null && ultraGrid1.ActiveRow.IsDataRow)
            {
                SelectAndClose(ultraGrid1.ActiveRow);
                e.Handled = true;
            }
        }

        private void SelectAndClose(UltraGridRow row)
        {
            if (row == null || !row.IsDataRow) return;
            SelectedPaymodeId = Convert.ToInt32(row.Cells["PayModeID"].Value);
            SelectedPaymodeName = Convert.ToString(row.Cells["PayModeName"].Value);

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
