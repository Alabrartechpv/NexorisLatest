using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Report;
using Repository;
using Repository.ReportRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win.Reports.InventoryReport
{
    public partial class frmItemReport : Form
    {
        // ─── Theme Palette (matches FrmSmartReorderDashboard / frmVendorOutstandingReport) ────────
        private static readonly Color FormBackColor        = Color.FromArgb(232, 246, 255);
        private static readonly Color FilterPanelBackColor = Color.FromArgb(232, 246, 255);
        private static readonly Color ActionPanelBackColor = Color.FromArgb(206, 223, 238);
        private static readonly Color BorderBlue           = Color.FromArgb(118, 154, 198);
        private static readonly Color ControlBackColor     = Color.White;
        private static readonly Color ControlTextColor     = Color.FromArgb(18, 49, 102);
        private static readonly Color GridHeaderBlue       = Color.FromArgb(93, 151, 214);
        private static readonly Color GridHeaderBlueDark   = Color.FromArgb(67, 118, 184);
        private static readonly Color GridSelectedBlue     = Color.FromArgb(173, 216, 255);
        private static readonly Color GridRowLine          = Color.FromArgb(197, 217, 241);
        private static readonly Color GridAltRow           = Color.FromArgb(246, 250, 255);
        private static readonly Color GridFooterBorder     = Color.FromArgb(144, 181, 223);
        private static readonly Color SkyBlueOutline       = Color.FromArgb(160, 210, 255);

        private static readonly Color ButtonTopColor       = Color.FromArgb(234, 244, 255);
        private static readonly Color ButtonBottomColor    = Color.FromArgb(152, 188, 235);
        private static readonly Color ButtonBorderColor    = Color.FromArgb(73, 119, 184);
        private static readonly Color ButtonTextBlue       = Color.FromArgb(14, 47, 108);

        private static readonly Color PanelHoverTopColor   = Color.FromArgb(245, 250, 255);
        private static readonly Color PanelHoverBottomColor= Color.FromArgb(170, 206, 244);

        private static readonly Color PanelPressedTopColor = Color.FromArgb(205, 226, 248);
        private static readonly Color PanelPressedBottomColor = Color.FromArgb(128, 170, 224);

        private ItemReportRepo itemReportRepo;
        private BaseRepostitory baseRepo;
        private int selectedItemId = 0;
        private string selectedItemName = "";

        private Dictionary<string, Label> summaryLabels = new Dictionary<string, Label>();
        private Dictionary<string, string> columnAggregations = new Dictionary<string, string>
        {
            { "Qty", "None" },
            { "Cost", "Average" },
            { "UnitPrice", "Average" },
            { "Balance", "None" }
        };
        private readonly string[] summaryTypes = new string[] { "None", "Sum", "Average", "Min", "Max", "Count" };

        public frmItemReport()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            try
            {
                itemReportRepo = new ItemReportRepo();
                baseRepo = new BaseRepostitory();

                // Apply unified theme appearance
                InitializeRuntimeAppearance();

                // Load branches dropdown
                LoadBranches();

                // Configure Grid & Buttons
                ConfigureTransactionGrid();
                StyleButtons();
                SetupSearchIcon();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeRuntimeAppearance()
        {
            BackColor = FormBackColor;

            // Panels
            if (ultraPanelControls != null)
            {
                ultraPanelControls.Appearance.BackColor = FilterPanelBackColor;
                ultraPanelControls.Appearance.BorderColor = BorderBlue;
                ultraPanelControls.BorderStyle = UIElementBorderStyle.Solid;
            }

            if (ultraPanelActionBar != null)
            {
                ultraPanelActionBar.Appearance.BackColor = ActionPanelBackColor;
                ultraPanelActionBar.Appearance.BorderColor = BorderBlue;
                ultraPanelActionBar.BorderStyle = UIElementBorderStyle.Solid;
                ultraPanelActionBar.Size = new Size(ultraPanelActionBar.Width, 38);
            }

            if (ultraPanelGrid != null)
            {
                ultraPanelGrid.Appearance.BackColor = FormBackColor;
                ultraPanelGrid.Appearance.BorderColor = BorderBlue;
                ultraPanelGrid.BorderStyle = UIElementBorderStyle.Solid;
            }

            if (gridFooterPanel != null)
            {
                gridFooterPanel.Appearance.BackColor = GridHeaderBlue;
                gridFooterPanel.Appearance.BackColor2 = GridHeaderBlue;
                gridFooterPanel.Appearance.BackGradientStyle = GradientStyle.None;
                gridFooterPanel.Appearance.BorderColor = GridFooterBorder;
                gridFooterPanel.BorderStyle = UIElementBorderStyle.Solid;
                gridFooterPanel.Height = 28;
            }

            // Labels
            StyleLabel(ultraLabelBranch);
            StyleLabel(ultraLabelItem);

            // Controls
            StyleFilterCombo(ultraComboBranch);
            StyleTextEditor(txtItemName);
        }

        private static void StyleLabel(Infragistics.Win.Misc.UltraLabel lbl)
        {
            if (lbl == null) return;
            lbl.Appearance.BackColor = Color.Transparent;
            lbl.Appearance.ForeColor = Color.FromArgb(18, 47, 95);
            lbl.Appearance.FontData.Bold = DefaultableBoolean.False;
            lbl.Appearance.FontData.Name = "Microsoft Sans Serif";
            lbl.Appearance.FontData.SizeInPoints = 9F;
        }

        private static void StyleFilterCombo(Infragistics.Win.UltraWinEditors.UltraComboEditor combo)
        {
            if (combo == null) return;
            combo.UseAppStyling = false;
            combo.UseOsThemes = DefaultableBoolean.False;
            combo.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            combo.BorderStyle = UIElementBorderStyle.Solid;
            combo.Appearance.BackColor = ControlBackColor;
            combo.Appearance.BorderColor = SkyBlueOutline;
            combo.Appearance.ForeColor = ControlTextColor;
            combo.Appearance.FontData.Name = "Microsoft Sans Serif";
            combo.Appearance.FontData.SizeInPoints = 9F;
            combo.ButtonStyle = UIElementButtonStyle.Office2003ToolbarButton;
        }

        private static void StyleTextEditor(Infragistics.Win.UltraWinEditors.UltraTextEditor editor)
        {
            if (editor == null) return;
            editor.UseAppStyling = false;
            editor.UseOsThemes = DefaultableBoolean.False;
            editor.DisplayStyle = EmbeddableElementDisplayStyle.Office2013;
            editor.BorderStyle = UIElementBorderStyle.Solid;
            editor.Appearance.BackColor = ControlBackColor;
            editor.Appearance.BorderColor = SkyBlueOutline;
            editor.Appearance.ForeColor = ControlTextColor;
            editor.Appearance.FontData.Name = "Microsoft Sans Serif";
            editor.Appearance.FontData.SizeInPoints = 9F;
        }

        private void SetupSearchIcon()
        {
            try
            {
                picItemSearch.UseAppStyling = false;
                picItemSearch.UseOsThemes = DefaultableBoolean.False;
                picItemSearch.Appearance.BackColor = Color.FromArgb(72, 122, 214);
                picItemSearch.Appearance.BackColor2 = Color.FromArgb(48, 90, 175);
                picItemSearch.Appearance.BackGradientStyle = GradientStyle.Vertical;
                picItemSearch.Appearance.BorderColor = Color.FromArgb(40, 80, 160);
                picItemSearch.BorderStyle = UIElementBorderStyle.Solid;
                picItemSearch.Cursor = Cursors.Hand;

                Bitmap bmp = new Bitmap(28, 25);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);
                    using (Pen p = new Pen(Color.White, 2.4f))
                    {
                        g.DrawEllipse(p, 6, 5, 10, 10);
                        g.DrawLine(p, 14, 13, 21, 19);
                    }
                }
                picItemSearch.Image = bmp;

                picItemSearch.MouseEnter += (s, e) =>
                {
                    picItemSearch.Appearance.BackColor = Color.FromArgb(95, 145, 230);
                    picItemSearch.Appearance.BackColor2 = Color.FromArgb(72, 122, 214);
                };
                picItemSearch.MouseLeave += (s, e) =>
                {
                    picItemSearch.Appearance.BackColor = Color.FromArgb(72, 122, 214);
                    picItemSearch.Appearance.BackColor2 = Color.FromArgb(48, 90, 175);
                };
            }
            catch { }
        }

        private void LoadBranches()
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Branch, (SqlConnection)baseRepo.DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("_Operation", "GETALL");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapt.Fill(dt);

                        DataRow dr = dt.NewRow();
                        dr["Id"] = 0;
                        dr["BranchName"] = "--Select Branch--";
                        dt.Rows.InsertAt(dr, 0);

                        ultraComboBranch.ValueMember = "Id";
                        ultraComboBranch.DisplayMember = "BranchName";
                        ultraComboBranch.DataSource = dt;

                        if (!string.IsNullOrEmpty(DataBase.BranchId))
                        {
                            if (int.TryParse(DataBase.BranchId, out int currentBranchId))
                            {
                                ultraComboBranch.Value = currentBranchId;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading branches: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void picItemSearch_Click(object sender, EventArgs e)
        {
            OpenItemSearchDialog();
        }

        private void txtItemName_Click(object sender, EventArgs e)
        {
            OpenItemSearchDialog();
        }

        private void txtItemName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F7 || e.KeyCode == Keys.Enter)
            {
                OpenItemSearchDialog();
                e.Handled = true;
            }
        }

        private void frmItemReport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F7)
            {
                OpenItemSearchDialog();
                e.Handled = true;
            }
        }

        private void OpenItemSearchDialog()
        {
            try
            {
                using (var itemDialog = new PosBranch_Win.DialogBox.frmdialForItemMaster("frmItemReport"))
                {
                    itemDialog.StartPosition = FormStartPosition.CenterParent;
                    if (itemDialog.ShowDialog(this) == DialogResult.OK)
                    {
                        Dictionary<string, object> selectedData = itemDialog.GetSelectedItemData();
                        if (selectedData != null)
                        {
                            int itemId = 0;
                            if (selectedData.ContainsKey("ItemId") && selectedData["ItemId"] != null)
                                int.TryParse(selectedData["ItemId"].ToString(), out itemId);
                            else if (selectedData.ContainsKey("ItemID") && selectedData["ItemID"] != null)
                                int.TryParse(selectedData["ItemID"].ToString(), out itemId);
                            else if (selectedData.ContainsKey("Id") && selectedData["Id"] != null)
                                int.TryParse(selectedData["Id"].ToString(), out itemId);

                            string desc = "";
                            if (selectedData.ContainsKey("Description") && selectedData["Description"] != null)
                                desc = selectedData["Description"].ToString();
                            else if (selectedData.ContainsKey("ItemName") && selectedData["ItemName"] != null)
                                desc = selectedData["ItemName"].ToString();

                            if (itemId == 0 && itemDialog.SelectedItemId > 0)
                            {
                                itemId = (int)itemDialog.SelectedItemId;
                            }
                            if (string.IsNullOrEmpty(desc) && !string.IsNullOrEmpty(itemDialog.SelectedItemName))
                            {
                                desc = itemDialog.SelectedItemName;
                            }

                            if (itemId > 0)
                            {
                                selectedItemId = itemId;
                                selectedItemName = desc;
                                txtItemName.Text = desc;
                                btnSearch.Focus();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting item: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureTransactionGrid()
        {
            ApplyGridStyling(ultraGridTransactions);
            ultraGridTransactions.InitializeLayout += UltraGridTransactions_InitializeLayout;
            ultraGridTransactions.InitializeRow += UltraGridTransactions_InitializeRow;
            InitializeGridFooterPanel();
        }

        private void InitializeGridFooterPanel()
        {
            if (gridFooterPanel == null)
                return;

            gridFooterPanel.Paint += (s, e) => { AlignSummaryLabels(); };
            gridFooterPanel.Resize += (s, e) => { AlignSummaryLabels(); };
            ultraGridTransactions.AfterColPosChanged += (s, e) => AlignSummaryLabels();
            ultraGridTransactions.AfterSortChange += (s, e) => AlignSummaryLabels();
            ultraGridTransactions.AfterRowFilterChanged += (s, e) => AlignSummaryLabels();
            ultraGridTransactions.InitializeLayout += (s, e) => { AlignSummaryLabels(); UpdateSummaryFooter(); };
            ultraGridTransactions.SizeChanged += (s, e) => AlignSummaryLabels();

            var panelMenu = new ContextMenuStrip();
            foreach (var type in summaryTypes)
            {
                var item = new ToolStripMenuItem(type, null, OnPanelSummaryTypeSelected) { Tag = type };
                panelMenu.Items.Add(item);
            }
            gridFooterPanel.ClientArea.ContextMenuStrip = panelMenu;
            gridFooterPanel.ClientArea.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    var ctrl = gridFooterPanel.ClientArea.GetChildAtPoint(e.Location);
                    if (ctrl == null || !(ctrl is Label))
                        panelMenu.Show(gridFooterPanel.ClientArea, e.Location);
                }
            };

            UpdateSummaryFooter();
        }

        private void OnPanelSummaryTypeSelected(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is string type)
            {
                if (ultraGridTransactions.DisplayLayout.Bands.Count > 0)
                {
                    foreach (var col in ultraGridTransactions.DisplayLayout.Bands[0].Columns.Cast<UltraGridColumn>())
                    {
                        if (!col.Hidden && IsNumericColumn(col))
                            columnAggregations[col.Key] = type;
                    }
                }
                UpdateSummaryFooter();
            }
        }

        private bool IsNumericColumn(UltraGridColumn col)
        {
            if (col == null) return false;
            Type DataType = col.DataType;
            return DataType == typeof(int) || DataType == typeof(decimal) || DataType == typeof(double) || DataType == typeof(float) || DataType == typeof(long);
        }

        private ContextMenuStrip CreateFooterLabelMenu(string columnKey)
        {
            var menu = new ContextMenuStrip();
            foreach (var type in summaryTypes)
            {
                var item = new ToolStripMenuItem(type) { Tag = type };
                item.Click += (s, e) =>
                {
                    columnAggregations[columnKey] = type;
                    UpdateFooterValues();
                };
                menu.Items.Add(item);
            }
            menu.Opening += (s, e) =>
            {
                foreach (ToolStripMenuItem item in menu.Items)
                {
                    item.Checked = columnAggregations.ContainsKey(columnKey) && columnAggregations[columnKey] == (string)item.Tag;
                }
            };
            return menu;
        }

        private void UpdateSummaryFooter()
        {
            if (gridFooterPanel == null || gridFooterPanel.ClientArea == null || ultraGridTransactions == null || ultraGridTransactions.DisplayLayout == null || ultraGridTransactions.DisplayLayout.Bands.Count == 0)
                return;

            gridFooterPanel.ClientArea.SuspendLayout();
            gridFooterPanel.ClientArea.Controls.Clear();
            summaryLabels.Clear();

            var band = ultraGridTransactions.DisplayLayout.Bands[0];
            foreach (var col in band.Columns.Cast<UltraGridColumn>())
            {
                if (col.Hidden) continue;
                if (!IsNumericColumn(col)) continue;
                if (!columnAggregations.ContainsKey(col.Key) || columnAggregations[col.Key] == "None") continue;

                var lbl = new Label
                {
                    Name = $"lblSummary_{col.Key}",
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleRight,
                    ForeColor = Color.FromArgb(17, 52, 102),
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Height = gridFooterPanel.Height - 4,
                    ContextMenuStrip = CreateFooterLabelMenu(col.Key)
                };
                gridFooterPanel.ClientArea.Controls.Add(lbl);
                summaryLabels[col.Key] = lbl;
            }
            UpdateFooterValues();
            AlignSummaryLabels();
            gridFooterPanel.ClientArea.ResumeLayout();
        }

        private void UpdateFooterValues()
        {
            if (ultraGridTransactions == null || ultraGridTransactions.DataSource == null) return;
            if (ultraGridTransactions.DataSource is List<ItemTransactionModel> list)
            {
                foreach (var kvp in summaryLabels)
                {
                    string colKey = kvp.Key;
                    Label lbl = kvp.Value;
                    string agg = columnAggregations.ContainsKey(colKey) ? columnAggregations[colKey] : "None";

                    var values = new List<double>();
                    foreach (var item in list)
                    {
                        var prop = item.GetType().GetProperty(colKey);
                        if (prop != null)
                        {
                            var val = prop.GetValue(item);
                            if (val != null) values.Add(Convert.ToDouble(val));
                        }
                    }

                    string text = "";
                    switch (agg)
                    {
                        case "Sum":
                            text = values.Count > 0 ? values.Sum().ToString("N2") : "0.00";
                            break;
                        case "Min":
                            text = values.Count > 0 ? values.Min().ToString("N2") : "-";
                            break;
                        case "Max":
                            text = values.Count > 0 ? values.Max().ToString("N2") : "-";
                            break;
                        case "Average":
                            text = values.Count > 0 ? values.Average().ToString("N2") : "-";
                            break;
                        case "Count":
                            text = values.Count.ToString();
                            break;
                    }
                    lbl.Text = text;
                }
            }
        }

        private void AlignSummaryLabels()
        {
            if (gridFooterPanel == null || gridFooterPanel.ClientArea == null || ultraGridTransactions == null || ultraGridTransactions.DisplayLayout == null || ultraGridTransactions.DisplayLayout.Bands.Count == 0)
                return;

            var band = ultraGridTransactions.DisplayLayout.Bands[0];
            foreach (var col in band.Columns.Cast<UltraGridColumn>())
            {
                if (col.Hidden) continue;
                if (!summaryLabels.TryGetValue(col.Key, out var lbl)) continue;

                var headerUI = ultraGridTransactions.DisplayLayout.Bands[0].Columns[col.Key].Header?.GetUIElement();
                if (headerUI != null)
                {
                    var headerPoint = headerUI.Control.PointToScreen(headerUI.Rect.Location);
                    int colLeft = headerPoint.X - gridFooterPanel.PointToScreen(Point.Empty).X;
                    int colWidth = headerUI.Rect.Width;

                    lbl.Left = colLeft;
                    lbl.Width = colWidth;
                    lbl.Visible = true;
                }
                else
                {
                    lbl.Visible = false;
                }
            }
        }

        private void UltraGridTransactions_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            UltraGridBand band = e.Layout.Bands[0];

            // Add or configure "Sl No" Column at VisiblePosition = 0
            if (!band.Columns.Exists("SlNo"))
            {
                UltraGridColumn slCol = band.Columns.Add("SlNo", "Sl No");
                slCol.DataType = typeof(int);
                slCol.Header.Caption = "Sl No";
                slCol.Header.VisiblePosition = 0;
                slCol.Width = 55;
                slCol.CellAppearance.TextHAlign = HAlign.Center;
            }
            else
            {
                band.Columns["SlNo"].Header.Caption = "Sl No";
                band.Columns["SlNo"].Header.VisiblePosition = 0;
                band.Columns["SlNo"].Width = 55;
                band.Columns["SlNo"].CellAppearance.TextHAlign = HAlign.Center;
            }

            string[] colsToHide = new string[] { "BranchId", "UnitId", "RefId", "IsBaseUnit" };
            foreach (string col in colsToHide)
            {
                if (band.Columns.Exists(col))
                    band.Columns[col].Hidden = true;
            }

            if (band.Columns.Exists("DT"))
            {
                band.Columns["DT"].Header.Caption = "Date";
                band.Columns["DT"].Format = "dd-MM-yyyy";
                band.Columns["DT"].CellAppearance.TextHAlign = HAlign.Center;
                band.Columns["DT"].Header.VisiblePosition = 1;
                band.Columns["DT"].Width = 90;
            }

            if (band.Columns.Exists("Operation"))
            {
                band.Columns["Operation"].Header.Caption = "Voucher Type";
                band.Columns["Operation"].CellAppearance.TextHAlign = HAlign.Left;
                band.Columns["Operation"].Header.VisiblePosition = 2;
                band.Columns["Operation"].Width = 110;
            }

            if (band.Columns.Exists("RefNo"))
            {
                band.Columns["RefNo"].Header.Caption = "Ref / Bill No";
                band.Columns["RefNo"].CellAppearance.TextHAlign = HAlign.Center;
                band.Columns["RefNo"].Header.VisiblePosition = 3;
                band.Columns["RefNo"].Width = 95;
            }

            if (band.Columns.Exists("Account"))
            {
                band.Columns["Account"].Header.Caption = "Party / Ledger Account";
                band.Columns["Account"].CellAppearance.TextHAlign = HAlign.Left;
                band.Columns["Account"].Header.VisiblePosition = 4;
                band.Columns["Account"].Width = 160;
            }

            if (band.Columns.Exists("Way"))
            {
                band.Columns["Way"].Header.Caption = "Way";
                band.Columns["Way"].CellAppearance.TextHAlign = HAlign.Center;
                band.Columns["Way"].Header.VisiblePosition = 5;
                band.Columns["Way"].Width = 60;
            }

            if (band.Columns.Exists("Qty"))
            {
                band.Columns["Qty"].Header.Caption = "Qty";
                band.Columns["Qty"].Format = "#,##0.00";
                band.Columns["Qty"].CellAppearance.TextHAlign = HAlign.Right;
                band.Columns["Qty"].Header.VisiblePosition = 6;
                band.Columns["Qty"].Width = 80;
            }

            if (band.Columns.Exists("UnitName"))
            {
                band.Columns["UnitName"].Header.Caption = "Unit";
                band.Columns["UnitName"].CellAppearance.TextHAlign = HAlign.Center;
                band.Columns["UnitName"].Header.VisiblePosition = 7;
                band.Columns["UnitName"].Width = 70;
            }

            if (band.Columns.Exists("Packing"))
            {
                band.Columns["Packing"].Header.Caption = "Packing";
                band.Columns["Packing"].Format = "#,##0.##";
                band.Columns["Packing"].CellAppearance.TextHAlign = HAlign.Center;
                band.Columns["Packing"].Header.VisiblePosition = 8;
                band.Columns["Packing"].Width = 70;
            }

            if (band.Columns.Exists("Cost"))
            {
                band.Columns["Cost"].Header.Caption = "Cost Price";
                band.Columns["Cost"].Format = "₹ #,##0.00";
                band.Columns["Cost"].CellAppearance.TextHAlign = HAlign.Right;
                band.Columns["Cost"].Header.VisiblePosition = 9;
                band.Columns["Cost"].Width = 90;
            }

            if (band.Columns.Exists("UnitPrice"))
            {
                band.Columns["UnitPrice"].Header.Caption = "Sales Price";
                band.Columns["UnitPrice"].Format = "₹ #,##0.00";
                band.Columns["UnitPrice"].CellAppearance.TextHAlign = HAlign.Right;
                band.Columns["UnitPrice"].Header.VisiblePosition = 10;
                band.Columns["UnitPrice"].Width = 90;
            }

            if (band.Columns.Exists("Balance"))
            {
                band.Columns["Balance"].Header.Caption = "Stock Balance";
                band.Columns["Balance"].Format = "#,##0.00";
                band.Columns["Balance"].CellAppearance.TextHAlign = HAlign.Right;
                band.Columns["Balance"].Header.VisiblePosition = 11;
                band.Columns["Balance"].Width = 100;
            }

            if (band.Columns.Exists("BranchName"))
            {
                band.Columns["BranchName"].Header.Caption = "Branch";
                band.Columns["BranchName"].CellAppearance.TextHAlign = HAlign.Left;
                band.Columns["BranchName"].Header.VisiblePosition = 12;
                band.Columns["BranchName"].Width = 110;
            }

            e.Layout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
        }

        private void UltraGridTransactions_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            if (e.Row.Cells.Exists("SlNo"))
            {
                e.Row.Cells["SlNo"].Value = e.Row.Index + 1;
            }

            if (e.Row.Cells.Exists("Way"))
            {
                string way = e.Row.Cells["Way"].Value?.ToString();
                if (string.Equals(way, "IN", StringComparison.OrdinalIgnoreCase))
                {
                    e.Row.Cells["Way"].Appearance.ForeColor = Color.FromArgb(46, 125, 50); // Green
                    e.Row.Cells["Way"].Appearance.FontData.Bold = DefaultableBoolean.True;
                }
                else if (string.Equals(way, "OUT", StringComparison.OrdinalIgnoreCase))
                {
                    e.Row.Cells["Way"].Appearance.ForeColor = Color.FromArgb(198, 40, 40); // Red
                    e.Row.Cells["Way"].Appearance.FontData.Bold = DefaultableBoolean.True;
                }
            }
        }

        private void ApplyGridStyling(UltraGrid targetGrid)
        {
            if (targetGrid == null) return;

            targetGrid.UseAppStyling = false;
            targetGrid.UseOsThemes = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Appearance.BackColor = FormBackColor;
            targetGrid.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            targetGrid.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.CaptionVisible = DefaultableBoolean.False;
            targetGrid.DisplayLayout.GroupByBox.Hidden = true;
            targetGrid.DisplayLayout.GroupByBox.BorderStyle = UIElementBorderStyle.None;

            targetGrid.DisplayLayout.Override.HeaderStyle = HeaderStyle.Standard;
            targetGrid.DisplayLayout.Override.HeaderClickAction = HeaderClickAction.SortSingle;
            targetGrid.DisplayLayout.Override.AllowAddNew = AllowAddNew.No;
            targetGrid.DisplayLayout.Override.AllowDelete = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Override.AllowUpdate = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Override.AllowColMoving = AllowColMoving.WithinBand;
            targetGrid.DisplayLayout.Override.AllowColSizing = AllowColSizing.Free;
            targetGrid.DisplayLayout.Override.AllowRowFiltering = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Override.CellClickAction = CellClickAction.RowSelect;

            targetGrid.DisplayLayout.Override.RowSelectors = DefaultableBoolean.True;
            targetGrid.DisplayLayout.Override.RowSelectorHeaderStyle = RowSelectorHeaderStyle.ColumnChooserButton;
            targetGrid.DisplayLayout.Override.RowSelectorWidth = 25;
            targetGrid.DisplayLayout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.BackColor = GridHeaderBlueDark;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = GridHeaderBlue;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.BorderColor = BorderBlue;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.ForeColor = Color.White;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            targetGrid.DisplayLayout.Override.RowSelectorAppearance.TextHAlign = HAlign.Center;

            targetGrid.DisplayLayout.Override.MinRowHeight = 24;
            targetGrid.DisplayLayout.Override.DefaultRowHeight = 24;
            targetGrid.DisplayLayout.Override.RowAppearance.BackColor = Color.White;
            targetGrid.DisplayLayout.Override.RowAppearance.ForeColor = ControlTextColor;
            targetGrid.DisplayLayout.Override.RowAppearance.BorderColor = GridRowLine;
            targetGrid.DisplayLayout.Override.RowAlternateAppearance.BackColor = GridAltRow;
            targetGrid.DisplayLayout.Override.RowAlternateAppearance.BorderColor = GridRowLine;
            targetGrid.DisplayLayout.Override.ActiveRowAppearance.BackColor = GridSelectedBlue;
            targetGrid.DisplayLayout.Override.ActiveRowAppearance.ForeColor = ControlTextColor;
            targetGrid.DisplayLayout.Override.SelectedRowAppearance.BackColor = GridSelectedBlue;
            targetGrid.DisplayLayout.Override.SelectedRowAppearance.ForeColor = ControlTextColor;

            targetGrid.DisplayLayout.Override.HeaderAppearance.BackColor = GridHeaderBlue;
            targetGrid.DisplayLayout.Override.HeaderAppearance.BackColor2 = GridHeaderBlueDark;
            targetGrid.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            targetGrid.DisplayLayout.Override.HeaderAppearance.ForeColor = Color.White;
            targetGrid.DisplayLayout.Override.HeaderAppearance.BorderColor = BorderBlue;
            targetGrid.DisplayLayout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
            targetGrid.DisplayLayout.Override.HeaderAppearance.TextVAlign = VAlign.Middle;
            targetGrid.DisplayLayout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
            targetGrid.DisplayLayout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            targetGrid.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;
            targetGrid.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

            targetGrid.DisplayLayout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            targetGrid.DisplayLayout.Override.CellAppearance.BorderColor = GridRowLine;
            targetGrid.DisplayLayout.Override.CellAppearance.ForeColor = ControlTextColor;
            targetGrid.DisplayLayout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            targetGrid.DisplayLayout.Override.CellAppearance.FontData.SizeInPoints = 8.25F;
            targetGrid.DisplayLayout.Override.RowSizing = RowSizing.AutoFree;
        }

        private void StyleButtons()
        {
            StyleButton(btnSearch);
            StyleButton(btnExport);
            StyleButton(btnPrint);
            StyleButton(btnClose);
            StyleButton(btnHideSelection);

            SetupEnhancedSummaryPanel();
        }

        private static void StyleButton(Infragistics.Win.Misc.UltraButton button)
        {
            if (button == null) return;
            button.UseAppStyling = false;
            button.UseOsThemes = DefaultableBoolean.False;
            button.ButtonStyle = UIElementButtonStyle.Office2013Button;
            button.Appearance.BackColor = ButtonTopColor;
            button.Appearance.BackColor2 = ButtonBottomColor;
            button.Appearance.BackGradientStyle = GradientStyle.Vertical;
            button.Appearance.BorderColor = ButtonBorderColor;
            button.Appearance.ForeColor = ButtonTextBlue;
            button.Appearance.FontData.Name = "Microsoft Sans Serif";
            button.Appearance.FontData.SizeInPoints = 9F;
            button.Appearance.FontData.Bold = DefaultableBoolean.False;

            button.HotTrackAppearance.BackColor = PanelHoverTopColor;
            button.HotTrackAppearance.BackColor2 = PanelHoverBottomColor;
            button.HotTrackAppearance.BorderColor = ButtonBorderColor;
            button.HotTrackAppearance.ForeColor = ButtonTextBlue;

            button.PressedAppearance.BackColor = PanelPressedTopColor;
            button.PressedAppearance.BackColor2 = PanelPressedBottomColor;
            button.PressedAppearance.BorderColor = ButtonBorderColor;
            button.PressedAppearance.ForeColor = ButtonTextBlue;
        }

        private void btnHideSelection_Click(object sender, EventArgs e)
        {
            ultraPanelControls.Visible = !ultraPanelControls.Visible;
            btnHideSelection.Text = ultraPanelControls.Visible ? "Hide Selection" : "Show Selection";
            LayoutPanels();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutPanels();
        }

        private void LayoutPanels()
        {
            if (ultraPanelActionBar == null || ultraPanelGrid == null || ultraPanelSummary == null || ultraPanelControls == null) return;

            if (TopLevel == false)
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Normal;
                Dock = DockStyle.Fill;
            }

            SuspendLayout();

            ultraPanelControls.Dock = DockStyle.Top;
            ultraPanelActionBar.Dock = DockStyle.Top;
            ultraPanelSummary.Dock = DockStyle.Bottom;
            ultraPanelSummary.Height = 72;
            ultraPanelSummary.Visible = true;
            ultraPanelGrid.Dock = DockStyle.Fill;

            if (!Controls.Contains(ultraPanelControls)) Controls.Add(ultraPanelControls);
            if (!Controls.Contains(ultraPanelActionBar)) Controls.Add(ultraPanelActionBar);
            if (!Controls.Contains(ultraPanelSummary)) Controls.Add(ultraPanelSummary);
            if (!Controls.Contains(ultraPanelGrid)) Controls.Add(ultraPanelGrid);

            Controls.SetChildIndex(ultraPanelControls, 3);
            Controls.SetChildIndex(ultraPanelActionBar, 2);
            Controls.SetChildIndex(ultraPanelSummary, 1);
            Controls.SetChildIndex(ultraPanelGrid, 0);

            if (ultraPanelGrid.ClientArea != null)
            {
                ultraPanelGrid.ClientArea.SuspendLayout();
                gridFooterPanel.Dock = DockStyle.Bottom;
                gridFooterPanel.Height = 26;
                ultraGridTransactions.Dock = DockStyle.Fill;

                if (!ultraPanelGrid.ClientArea.Controls.Contains(gridFooterPanel))
                    ultraPanelGrid.ClientArea.Controls.Add(gridFooterPanel);
                if (!ultraPanelGrid.ClientArea.Controls.Contains(ultraGridTransactions))
                    ultraPanelGrid.ClientArea.Controls.Add(ultraGridTransactions);

                ultraPanelGrid.ClientArea.Controls.SetChildIndex(gridFooterPanel, 1);
                ultraPanelGrid.ClientArea.Controls.SetChildIndex(ultraGridTransactions, 0);

                ultraPanelGrid.ClientArea.ResumeLayout(true);
                ultraPanelGrid.ClientArea.PerformLayout();
            }

            ResumeLayout(true);
            PerformLayout();

            AlignSummaryCards();
            AlignSummaryLabels();
        }

        private void AlignSummaryCards()
        {
            if (ultraPanelSummary == null || ultraPanelSummary.ClientArea == null) return;
            int totalWidth = ultraPanelSummary.ClientArea.Width;
            if (totalWidth <= 0) return;

            UltraLabel[] captions = new UltraLabel[]
            {
                ultraLabelSalesCaption, ultraLabelPurchaseCaption, ultraLabelReturnCaption, ultraLabelAdjustCaption,
                ultraLabelTotalInCaption, ultraLabelTotalOutCaption, ultraLabelCurrentStockCaption, ultraLabelStockValueCaption
            };

            UltraLabel[] values = new UltraLabel[]
            {
                ultraLabelSalesValue, ultraLabelPurchaseValue, ultraLabelReturnValue, ultraLabelAdjustValue,
                ultraLabelTotalInValue, ultraLabelTotalOutValue, ultraLabelCurrentStockValue, ultraLabelStockValueValue
            };

            int count = 8;
            int padding = 12;
            int baseCardWidth = 140;

            int availableWidth = totalWidth - (padding * 2);
            if (availableWidth <= 0) return;

            int gap = 10;
            int computedWidth = (availableWidth - (gap * (count - 1))) / count;
            int cardWidth = Math.Max(baseCardWidth, Math.Min(220, computedWidth));

            int remainingForGaps = availableWidth - (count * cardWidth);
            if (count > 1)
            {
                gap = Math.Max(4, remainingForGaps / (count - 1));
            }

            int currentX = padding;
            for (int i = 0; i < count; i++)
            {
                if (captions[i] != null)
                {
                    captions[i].Location = new Point(currentX, 2);
                    captions[i].Size = new Size(cardWidth, 16);
                }
                if (values[i] != null)
                {
                    values[i].Location = new Point(currentX, 18);
                    values[i].Size = new Size(cardWidth, 48);
                }
                currentX += cardWidth + gap;
            }
        }

        private void SetupEnhancedSummaryPanel()
        {
            if (ultraPanelSummary == null) return;

            ultraPanelSummary.Dock = DockStyle.Bottom;
            ultraPanelSummary.Height = 72;
            ultraPanelSummary.Visible = true;

            gridFooterPanel.Dock = DockStyle.Bottom;
            gridFooterPanel.Height = 26;
            ultraGridTransactions.Dock = DockStyle.Fill;

            ultraPanelSummary.ClientArea.AutoScroll = false;
            ultraPanelSummary.Resize += (s, e) => AlignSummaryCards();
        }

        private void frmItemReport_Load(object sender, EventArgs e)
        {
            LayoutPanels();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (selectedItemId <= 0)
            {
                MessageBox.Show("Please select an item first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                OpenItemSearchDialog();
                return;
            }

            this.Cursor = Cursors.WaitCursor;

            try
            {
                int itemId = selectedItemId;
                int branchId = ultraComboBranch.Value != null ? Convert.ToInt32(ultraComboBranch.Value) : (int.TryParse(DataBase.BranchId, out int bId) ? bId : 0);

                int finYearId = !string.IsNullOrEmpty(DataBase.FinyearId) ? Convert.ToInt32(DataBase.FinyearId) : 1;
                int companyId = !string.IsNullOrEmpty(DataBase.CompanyId) ? Convert.ToInt32(DataBase.CompanyId) : 1;

                var reportData = itemReportRepo.GetItemReport(finYearId, companyId, branchId, itemId);

                if (reportData.Transactions != null && reportData.Transactions.Count > 0)
                {
                    string defaultUnit = reportData.PriceSettings?.FirstOrDefault(x => !string.IsNullOrEmpty(x.UnitName))?.UnitName;
                    if (string.IsNullOrWhiteSpace(defaultUnit)) defaultUnit = "UNIT";

                    decimal runningBalance = 0;
                    foreach (var t in reportData.Transactions)
                    {
                        if (string.IsNullOrWhiteSpace(t.UnitName))
                        {
                            t.UnitName = defaultUnit;
                        }

                        decimal packing = t.Packing > 0 ? t.Packing : 1;
                        decimal baseQty = t.Qty * packing;

                        if (string.Equals(t.Way, "IN", StringComparison.OrdinalIgnoreCase))
                        {
                            runningBalance += baseQty;
                        }
                        else if (string.Equals(t.Way, "OUT", StringComparison.OrdinalIgnoreCase))
                        {
                            runningBalance -= baseQty;
                        }

                        t.Balance = runningBalance;
                    }
                }

                ultraGridTransactions.DataSource = reportData.Transactions;
                UpdateSummaryFooter();

                if (reportData.Transactions != null && reportData.Transactions.Count > 0)
                {
                    Func<ItemTransactionModel, decimal> getBaseQty = x => x.Qty * (x.Packing > 0 ? x.Packing : 1);
                    Func<ItemTransactionModel, decimal> getUnitCost = x => x.Packing > 1 ? (x.Cost / x.Packing) : x.Cost;

                    var salesTxns = reportData.Transactions.Where(x => x.Operation.Equals("Sales", StringComparison.OrdinalIgnoreCase));
                    decimal salesQty = salesTxns.Sum(getBaseQty);
                    decimal salesAmt = salesTxns.Sum(x => x.Qty * x.UnitPrice);

                    var purchaseTxns = reportData.Transactions.Where(x => x.Operation.Equals("Purchase", StringComparison.OrdinalIgnoreCase));
                    decimal purchaseQty = purchaseTxns.Sum(getBaseQty);
                    decimal purchaseAmt = purchaseTxns.Sum(x => x.Qty * x.Cost);

                    var returnTxns = reportData.Transactions.Where(x => x.Operation.StartsWith("Sales Return", StringComparison.OrdinalIgnoreCase) || x.Operation.StartsWith("Return", StringComparison.OrdinalIgnoreCase));
                    decimal returnQty = returnTxns.Sum(getBaseQty);
                    decimal returnAmt = returnTxns.Sum(x => x.Qty * (x.UnitPrice > 0 ? x.UnitPrice : x.Cost));

                    var adjustTxns = reportData.Transactions.Where(x => x.Operation.StartsWith("Stock Adjust", StringComparison.OrdinalIgnoreCase) || x.Operation.StartsWith("Adjustment", StringComparison.OrdinalIgnoreCase));
                    decimal adjustQty = adjustTxns.Sum(getBaseQty);

                    decimal totalIn = reportData.Transactions.Where(x => x.Way.Equals("IN", StringComparison.OrdinalIgnoreCase)).Sum(getBaseQty);
                    decimal totalOut = reportData.Transactions.Where(x => x.Way.Equals("OUT", StringComparison.OrdinalIgnoreCase)).Sum(getBaseQty);

                    decimal currentStock = reportData.StockSummary != null && reportData.StockSummary.Count > 0
                        ? reportData.StockSummary.Sum(x => x.Stock)
                        : (totalIn - totalOut);

                    decimal latestCost = reportData.Transactions
                        .Where(x => x.Cost > 0)
                        .Select(getUnitCost)
                        .LastOrDefault();

                    if (latestCost == 0 && reportData.PriceSettings != null && reportData.PriceSettings.Count > 0)
                    {
                        var ps = reportData.PriceSettings.FirstOrDefault(x => x.Cost > 0);
                        if (ps != null)
                        {
                            latestCost = ps.Cost;
                        }
                    }

                    decimal stockValue = currentStock * latestCost;

                    if (ultraLabelSalesValue != null) ultraLabelSalesValue.Text = $"{salesQty:N2} Qty\n₹ {salesAmt:N2}";
                    if (ultraLabelPurchaseValue != null) ultraLabelPurchaseValue.Text = $"{purchaseQty:N2} Qty\n₹ {purchaseAmt:N2}";
                    if (ultraLabelReturnValue != null) ultraLabelReturnValue.Text = $"{returnQty:N2} Qty\n₹ {returnAmt:N2}";
                    if (ultraLabelAdjustValue != null) ultraLabelAdjustValue.Text = $"{adjustQty:N2} Qty";

                    if (ultraLabelTotalInValue != null) ultraLabelTotalInValue.Text = $"{totalIn:N2} Qty";
                    if (ultraLabelTotalOutValue != null) ultraLabelTotalOutValue.Text = $"{totalOut:N2} Qty";
                    if (ultraLabelCurrentStockValue != null) ultraLabelCurrentStockValue.Text = $"{currentStock:N2} Qty";
                    if (ultraLabelStockValueValue != null) ultraLabelStockValueValue.Text = $"₹ {stockValue:N2}";
                }
                else
                {
                    if (ultraLabelSalesValue != null) ultraLabelSalesValue.Text = "0.00 Qty\n₹ 0.00";
                    if (ultraLabelPurchaseValue != null) ultraLabelPurchaseValue.Text = "0.00 Qty\n₹ 0.00";
                    if (ultraLabelReturnValue != null) ultraLabelReturnValue.Text = "0.00 Qty\n₹ 0.00";
                    if (ultraLabelAdjustValue != null) ultraLabelAdjustValue.Text = "0.00 Qty";

                    if (ultraLabelTotalInValue != null) ultraLabelTotalInValue.Text = "0.00 Qty";
                    if (ultraLabelTotalOutValue != null) ultraLabelTotalOutValue.Text = "0.00 Qty";
                    if (ultraLabelCurrentStockValue != null) ultraLabelCurrentStockValue.Text = "0.00 Qty";
                    if (ultraLabelStockValueValue != null) ultraLabelStockValueValue.Text = "₹ 0.00";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV Files|*.csv",
                    Title = "Save Report"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (ultraGridTransactions.Rows.Count > 0)
                    {
                        ExportToCSV(ultraGridTransactions, saveFileDialog.FileName);
                        MessageBox.Show("Export successful.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("No data to export.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToCSV(UltraGrid grid, string fileName)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var col in grid.DisplayLayout.Bands[0].Columns)
            {
                if (!col.Hidden)
                    sb.Append(col.Header.Caption + ",");
            }
            if (sb.Length > 0) sb.Length--;
            sb.AppendLine();

            foreach (var row in grid.Rows)
            {
                foreach (var col in grid.DisplayLayout.Bands[0].Columns)
                {
                    if (!col.Hidden)
                    {
                        string value = row.Cells[col].Value?.ToString() ?? "";
                        if (value.Contains(",")) value = "\"" + value + "\"";
                        sb.Append(value + ",");
                    }
                }
                if (sb.Length > 0) sb.Length--;
                sb.AppendLine();
            }

            File.WriteAllText(fileName, sb.ToString());
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            ultraGridTransactions.PrintPreview();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
