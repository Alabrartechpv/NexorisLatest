using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Master;
using PosBranch_Win.Master;
using Repository;
using Repository.MasterRepositry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PosBranch_Win.Transaction;
using System.Data.SqlClient;

namespace PosBranch_Win.DialogBox
{
    public partial class frmReasonDialog : Form
    {
        private FrmStockAdjustment stockk; // Keep reference to parent
        private Dropdowns drop = new Dropdowns();
        private DataTable fullDataTable = null;
        private Label lblStatus;
        private bool isOriginalOrder = true;
        private Form columnChooserForm = null;
        private ListBox columnChooserListBox = null;
        private Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>();
        private ToolTip toolTip = new ToolTip();

        private Infragistics.Win.Misc.UltraPanel ultPanelPurchaseDisplay;
        private Label label1;
        private Infragistics.Win.Misc.UltraPanel ultraPanel3;
        private Infragistics.Win.UltraWinEditors.UltraPictureBox ultraPictureBox5;
        private Infragistics.Win.Misc.UltraPanel ultraPanel7;
        private Infragistics.Win.UltraWinEditors.UltraPictureBox ultraPictureBox6;
        private UltraGrid ultraGrid1;
        private Infragistics.Win.Misc.UltraPanel ultraPanel2;
        private Infragistics.Win.Misc.UltraPanel ultraPanel9;
        private Infragistics.Win.UltraWinEditors.UltraPictureBox ultraPictureBox4;
        private ComboBox comboBox2;
        private ComboBox comboBox1;
        private Label lblSearch;
        private Label label2;
        private TextBox textBoxsearch;
        private Infragistics.Win.Misc.UltraPanel ultraPanel4;
        private Label label4;
        private Infragistics.Win.UltraWinEditors.UltraPictureBox ultraPictureBox3;
        private Infragistics.Win.Misc.UltraPanel ultraPanel8;
        private Infragistics.Win.Misc.UltraPanel ultraPanel6;
        private Label label3;
        private Infragistics.Win.UltraWinEditors.UltraPictureBox ultraPictureBox2;
        private Infragistics.Win.Misc.UltraPanel ultraPanel5;
        private Label label5;
        private Infragistics.Win.UltraWinEditors.UltraPictureBox ultraPictureBox1;
        private TextBox textBox3;

        public frmReasonDialog()
        {
            InitializeComponent();
            InitializeStatusLabel();
            SetupUltraGridStyle();
            SetupUltraPanelStyle();
            InitializeSearchFilterComboBox();
            InitializeColumnOrderComboBox();
            EnsureSearchControlsInitialized();
            SetupPanelHoverEffects();
            ConnectClickEvents();
            SetupColumnChooserMenu();

            this.Load += new System.EventHandler(this.frmReasonDialog_Load);
            ultraGrid1.KeyPress += ultraGrid1_KeyPress;
            ultraGrid1.KeyDown += ultraGrid1_KeyDown;
            this.FormClosing += FrmReasonDialog_FormClosing;
            ultraGrid1.AfterRowsDeleted += UltraGrid1_AfterRowsDeleted;
            ultraGrid1.AfterSortChange += UltraGrid1_AfterSortChange;
            ultraGrid1.AfterColPosChanged += UltraGrid1_AfterColPosChanged;
            this.SizeChanged += FrmReasonDialog_SizeChanged;
            ultraGrid1.Resize += UltraGrid1_Resize;

            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 500;
            toolTip.ReshowDelay = 500;
            toolTip.ShowAlways = true;

            UpdateStatus("Form initialized");
        }

        private void InitializeStatusLabel()
        {
            lblStatus = new Label
            {
                Name = "lblStatus",
                AutoSize = false,
                Dock = DockStyle.Bottom,
                Height = 30,
                BackColor = Color.LightYellow,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "Ready"
            };
            this.Controls.Add(lblStatus);
        }

        private void UpdateStatus(string message)
        {
            if (lblStatus != null)
            {
                lblStatus.Text = message;
                lblStatus.Update();
            }
        }
        
        // --- BEGIN: Appearance Styling Helpers from frmdialForItemMaster ---
        private void SetupUltraPanelStyle()
        {
            StyleIconPanel(ultraPanel3);
            StyleIconPanel(ultraPanel4);
            StyleIconPanel(ultraPanel5);
            StyleIconPanel(ultraPanel6);
            StyleIconPanel(ultraPanel7);

            // Match ultraPanel3 and ultraPanel7 to ultraPanel4 for consistency
            ultraPanel3.Appearance.BackColor = ultraPanel4.Appearance.BackColor;
            ultraPanel3.Appearance.BackColor2 = ultraPanel4.Appearance.BackColor2;
            ultraPanel3.Appearance.BackGradientStyle = ultraPanel4.Appearance.BackGradientStyle;
            ultraPanel3.Appearance.BorderColor = ultraPanel4.Appearance.BorderColor;

            ultraPanel7.Appearance.BackColor = ultraPanel4.Appearance.BackColor;
            ultraPanel7.Appearance.BackColor2 = ultraPanel4.Appearance.BackColor2;
            ultraPanel7.Appearance.BackGradientStyle = ultraPanel4.Appearance.BackGradientStyle;
            ultraPanel7.Appearance.BorderColor = ultraPanel4.Appearance.BorderColor;

            // Main panel styling
            ultPanelPurchaseDisplay.Appearance.BackColor = System.Drawing.Color.White;
            ultPanelPurchaseDisplay.Appearance.BackColor2 = System.Drawing.Color.FromArgb(200, 230, 250);
            ultPanelPurchaseDisplay.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            ultPanelPurchaseDisplay.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
        }

        private void StyleIconPanel(Infragistics.Win.Misc.UltraPanel panel)
        {
            // Consistent blue gradient styling
            System.Drawing.Color lightBlue = System.Drawing.Color.FromArgb(127, 219, 255);
            System.Drawing.Color darkBlue = System.Drawing.Color.FromArgb(0, 116, 217);
            System.Drawing.Color borderBlue = System.Drawing.Color.FromArgb(0, 150, 220);
            System.Drawing.Color borderBase = System.Drawing.Color.FromArgb(0, 100, 180);

            panel.Appearance.BackColor = lightBlue;
            panel.Appearance.BackColor2 = darkBlue;
            panel.Appearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            panel.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded4;
            panel.Appearance.BorderColor = borderBlue;
            panel.Appearance.BorderColor3DBase = borderBase;

            foreach (System.Windows.Forms.Control control in panel.ClientArea.Controls)
            {
                if (control is Infragistics.Win.UltraWinEditors.UltraPictureBox pic)
                {
                    pic.BackColor = System.Drawing.Color.Transparent;
                    pic.BackColorInternal = System.Drawing.Color.Transparent;
                    pic.BorderShadowColor = System.Drawing.Color.Transparent;
                }
                else if (control is System.Windows.Forms.Label lbl)
                {
                    lbl.ForeColor = System.Drawing.Color.White;
                    lbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
                    lbl.BackColor = System.Drawing.Color.Transparent;
                }
            }

            // Hover effect
            panel.ClientArea.MouseEnter += (sender, e) => {
                panel.Appearance.BackColor = System.Drawing.Color.FromArgb(160, 230, 255);
                panel.Appearance.BackColor2 = System.Drawing.Color.FromArgb(30, 140, 230);
            };
            panel.ClientArea.MouseLeave += (sender, e) => {
                panel.Appearance.BackColor = lightBlue;
                panel.Appearance.BackColor2 = darkBlue;
            };
            panel.ClientArea.Cursor = System.Windows.Forms.Cursors.Hand;
        }

        private void SetupUltraGridStyle()
        {
            try
            {
                ultraGrid1.DisplayLayout.Reset();
                ultraGrid1.DisplayLayout.Override.AllowAddNew = Infragistics.Win.UltraWinGrid.AllowAddNew.No;
                ultraGrid1.DisplayLayout.Override.AllowDelete = Infragistics.Win.DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.RowSelectors = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = Infragistics.Win.UltraWinGrid.SelectType.Single;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortSingle;
                ultraGrid1.DisplayLayout.Override.AllowColMoving = Infragistics.Win.UltraWinGrid.AllowColMoving.WithinBand;
                ultraGrid1.DisplayLayout.Override.AllowColSizing = Infragistics.Win.UltraWinGrid.AllowColSizing.Free;
                ultraGrid1.DisplayLayout.Override.AllowColSwapping = Infragistics.Win.UltraWinGrid.AllowColSwapping.WithinBand;
                ultraGrid1.DisplayLayout.Override.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.RowSelect;
                ultraGrid1.DisplayLayout.GroupByBox.Hidden = true;
                ultraGrid1.DisplayLayout.GroupByBox.Prompt = string.Empty;
                ultraGrid1.DisplayLayout.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleRow = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleCell = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleHeader = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.BorderStyleRowSelector = Infragistics.Win.UIElementBorderStyle.Solid;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BorderAlpha = Infragistics.Win.Alpha.Opaque;
                ultraGrid1.DisplayLayout.Override.CellAppearance.BorderAlpha = Infragistics.Win.Alpha.Opaque;
                ultraGrid1.DisplayLayout.Override.CellPadding = 0;
                ultraGrid1.DisplayLayout.Override.RowSpacingBefore = 0;
                ultraGrid1.DisplayLayout.Override.RowSpacingAfter = 0;
                ultraGrid1.DisplayLayout.Override.CellSpacing = 0;
                ultraGrid1.DisplayLayout.InterBandSpacing = 0;
                System.Drawing.Color lightBlue = System.Drawing.Color.FromArgb(173, 216, 230);
                System.Drawing.Color headerBlue = System.Drawing.Color.FromArgb(0, 123, 255);
                ultraGrid1.DisplayLayout.Override.CellAppearance.BorderColor = lightBlue;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BorderColor = lightBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BorderColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BorderColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.MinRowHeight = 30;
                ultraGrid1.DisplayLayout.Override.DefaultRowHeight = 30;
                ultraGrid1.DisplayLayout.Override.HeaderStyle = Infragistics.Win.HeaderStyle.WindowsXPCommand;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = headerBlue;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = System.Drawing.Color.White;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextVAlign = Infragistics.Win.VAlign.Middle;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackColor2 = headerBlue;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.ForeColor = System.Drawing.Color.White;
                ultraGrid1.DisplayLayout.Override.RowSelectorHeaderStyle = Infragistics.Win.UltraWinGrid.RowSelectorHeaderStyle.Default;
                ultraGrid1.DisplayLayout.Override.RowSelectorNumberStyle = Infragistics.Win.UltraWinGrid.RowSelectorNumberStyle.None;
                ultraGrid1.DisplayLayout.Override.RowSelectorWidth = 15;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.Image = null;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.Image = null;
                ultraGrid1.DisplayLayout.Override.RowSelectorAppearance.Image = null;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = System.Drawing.Color.White;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor2 = System.Drawing.Color.White;
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor = System.Drawing.Color.White;
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackColor2 = System.Drawing.Color.White;
                ultraGrid1.DisplayLayout.Override.RowAlternateAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackColor2 = System.Drawing.Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.ForeColor = System.Drawing.Color.White;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor2 = System.Drawing.Color.FromArgb(0, 120, 215);
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = System.Drawing.Color.White;
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.SizeInPoints = 10;
                ultraGrid1.DisplayLayout.Override.RowAppearance.FontData.SizeInPoints = 10;
                ultraGrid1.DisplayLayout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
                ultraGrid1.DisplayLayout.Override.RowAppearance.FontData.Name = "Microsoft Sans Serif";
                ultraGrid1.DisplayLayout.ScrollBounds = Infragistics.Win.UltraWinGrid.ScrollBounds.ScrollToFill;
                ultraGrid1.DisplayLayout.ScrollStyle = Infragistics.Win.UltraWinGrid.ScrollStyle.Immediate;
                if (ultraGrid1.DisplayLayout.ScrollBarLook != null)
                {
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackColor2 = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ButtonAppearance.BorderColor = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor = System.Drawing.Color.White;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackColor2 = System.Drawing.Color.White;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                    ultraGrid1.DisplayLayout.ScrollBarLook.TrackAppearance.BorderColor = lightBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackColor2 = headerBlue;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                    ultraGrid1.DisplayLayout.ScrollBarLook.ThumbAppearance.BorderColor = headerBlue;
                }
                ultraGrid1.DisplayLayout.Override.CellAppearance.TextVAlign = Infragistics.Win.VAlign.Middle;
                ultraGrid1.DisplayLayout.Override.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                ultraGrid1.DisplayLayout.AutoFitStyle = Infragistics.Win.UltraWinGrid.AutoFitStyle.None;
                ultraGrid1.DisplayLayout.Override.ColumnAutoSizeMode = Infragistics.Win.UltraWinGrid.ColumnAutoSizeMode.None;
                // Hide all columns first
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                    {
                        col.Hidden = true;
                    }
                    // Show and configure only the relevant columns
                    string[] columnsToShow = new string[] { "LedgerID", "ReasonName" };
                    for (int i = 0; i < columnsToShow.Length; i++)
                    {
                        string colKey = columnsToShow[i];
                        if (ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(colKey))
                        {
                            UltraGridColumn col = ultraGrid1.DisplayLayout.Bands[0].Columns[colKey];
                            col.Hidden = false;
                            col.Header.VisiblePosition = i;
                            switch (colKey)
                            {
                                case "LedgerID":
                                    col.Header.Caption = "Ledger ID";
                                    col.Width = 120;
                                    col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                                    break;
                                case "ReasonName":
                                    col.Header.Caption = "Reason Name";
                                    col.Width = 250;
                                    col.CellAppearance.TextHAlign = Infragistics.Win.HAlign.Left;
                                    break;
                            }
                        }
                    }
                }
                ultraGrid1.Refresh();
                ultraGrid1.DisplayLayout.Override.AllowRowFiltering = Infragistics.Win.DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.FilterUIType = Infragistics.Win.UltraWinGrid.FilterUIType.FilterRow;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortSingle;
                ultraGrid1.DisplayLayout.Override.WrapHeaderText = Infragistics.Win.DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextTrimming = Infragistics.Win.TextTrimming.None;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting up grid style: {ex.Message}");
            }
        }
        // --- END: Appearance Styling Helpers ---
        
        #region Event Handlers for UI
        private void FrmReasonDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                columnChooserForm.Close();
                columnChooserForm = null;
            }
        }

        private void UltraGrid1_Resize(object sender, EventArgs e) => PreserveColumnWidths();
        private void FrmReasonDialog_SizeChanged(object sender, EventArgs e) => PreserveColumnWidths();
        private void UltraGrid1_AfterColPosChanged(object sender, AfterColPosChangedEventArgs e) => PreserveColumnWidths();
        private void UltraGrid1_AfterSortChange(object sender, BandEventArgs e) => PreserveColumnWidths();
        private void UltraGrid1_AfterRowsDeleted(object sender, EventArgs e) => PreserveColumnWidths();
        
        private void ultraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && ultraGrid1.ActiveRow != null)
            {
                SendReasonToParentForm();
                e.Handled = true;
            }
        }

        private void ultraGrid1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true; 
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter && ultraGrid1.Focused && ultraGrid1.ActiveRow != null)
            {
                SendReasonToParentForm();
                return true;
            }
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            if (keyData == Keys.Up && ultraGrid1.Focused)
            {
                if (ultraGrid1.ActiveRow != null && ultraGrid1.ActiveRow.Index > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[ultraGrid1.ActiveRow.Index - 1];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                    ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
                }
                return true;
            }
            if (keyData == Keys.Down && ultraGrid1.Focused)
            {
                if (ultraGrid1.ActiveRow != null && ultraGrid1.ActiveRow.Index < ultraGrid1.Rows.Count - 1)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[ultraGrid1.ActiveRow.Index + 1];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                    ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
                }
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ultraGrid1_DoubleClickCell(object sender, DoubleClickCellEventArgs e)
        {
            SendReasonToParentForm();
        }

        private void textBoxsearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilter(textBoxsearch.Text);
        }
        #endregion

        #region Data Handling
        private void LoadAllData()
        {
            try
            {
                UpdateStatus("Loading reasons...");
                DataBase.Operations = "Reason";
                ModelClass.TransactionModels.ReasonDDLGrid reas = drop.getReasonDDl();

                if (reas.List != null && reas.List.Any())
                {
                    fullDataTable = new DataTable();
                    fullDataTable.Columns.Add("LedgerID", typeof(int));
                    fullDataTable.Columns.Add("ReasonName", typeof(string));

                    foreach (var item in reas.List)
                    {
                        fullDataTable.Rows.Add(item.LedgerID, item.ReasonName);
                    }
                    PreserveOriginalRowOrder(fullDataTable);
                    ultraGrid1.DataSource = fullDataTable;
                    UpdateStatus($"{fullDataTable.Rows.Count} reasons loaded.");
                }
                else
                {
                    fullDataTable = new DataTable(); // Ensure not null
                    UpdateStatus("No reasons found.");
                }
                UpdateRecordCountLabel();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error loading data: {ex.Message}");
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
        }
        
        private void ApplyFilter(string searchText)
        {
            try
            {
                if (fullDataTable == null) return;

                DataView dv = fullDataTable.DefaultView;
                if (!string.IsNullOrEmpty(searchText) && searchText != "Search...")
                {
                    string filterOption = comboBox1.SelectedItem?.ToString() ?? "Select all";
                    dv.RowFilter = BuildFilterString(searchText, filterOption);
                }
                else
                {
                    dv.RowFilter = string.Empty;
                }
                ultraGrid1.DataSource = dv;
                UpdateRecordCountLabel();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Filter error: {ex.Message}");
            }
        }

        private string BuildFilterString(string searchText, string filterOption)
        {
            string escapedSearchText = searchText.Replace("'", "''");
            switch (filterOption)
            {
                case "Reason Name":
                    return $"ReasonName LIKE '%{escapedSearchText}%'";
                case "Ledger ID":
                    return $"CONVERT(LedgerID, 'System.String') LIKE '%{escapedSearchText}%'";
                default: // "Select all"
                    return $"ReasonName LIKE '%{escapedSearchText}%' OR CONVERT(LedgerID, 'System.String') LIKE '%{escapedSearchText}%'";
            }
        }
        
        private void SendReasonToParentForm()
        {
            try
            {
                if (ultraGrid1.ActiveRow != null)
                {
                    UltraGridCell ledgIdCell = this.ultraGrid1.ActiveRow.Cells["LedgerID"];
                    UltraGridCell reasNameCell = this.ultraGrid1.ActiveRow.Cells["ReasonName"];

                    string selectedReasonName = reasNameCell.Value != null ? reasNameCell.Value.ToString() : "";

                    stockk = Application.OpenForms.OfType<FrmStockAdjustment>().FirstOrDefault();
                    if (stockk != null)
                    {
                        stockk.txtb_reason.Text = selectedReasonName;
                        stockk.ultlbl_ledgerid.Text = ledgIdCell.Value != null ? ledgIdCell.Value.ToString() : "";
                    }

                    var reasonMasterForm = Application.OpenForms.OfType<Master.FrmReason>().FirstOrDefault();
                    if (reasonMasterForm != null)
                    {
                        reasonMasterForm.SelectReasonByName(selectedReasonName);
                    }

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending reason back: " + ex.Message);
            }
        }
        #endregion

        #region UI Setup
        private void InitializeSearchFilterComboBox()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.Add("Select all");
            comboBox1.Items.Add("Reason Name");
            comboBox1.Items.Add("Ledger ID");
            comboBox1.SelectedIndex = 0;
            comboBox1.SelectedIndexChanged += (s, e) => ApplyFilter(textBoxsearch.Text);
        }

        private void InitializeColumnOrderComboBox()
        {
            comboBox2.Items.Clear();
            comboBox2.Items.Add("Default Order");
            comboBox2.Items.Add("Reason Name");
            comboBox2.Items.Add("Ledger ID");
            comboBox2.SelectedIndex = 0;
            comboBox2.SelectedIndexChanged += (s, e) => ReorderColumns(comboBox2.SelectedItem.ToString());
        }

        private void EnsureSearchControlsInitialized()
        {
            if (textBoxsearch != null)
            {
                textBoxsearch.Text = "Search...";
                textBoxsearch.GotFocus += (s, e) => { if (textBoxsearch.Text == "Search...") textBoxsearch.Text = ""; };
                textBoxsearch.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(textBoxsearch.Text)) textBoxsearch.Text = "Search..."; };
                textBoxsearch.TextChanged += textBoxsearch_TextChanged;
            }
        }

        private void ConnectClickEvents()
        {
            // OK Button
            ultraPanel5.Click += (s, e) => SendReasonToParentForm();
            ultraPictureBox1.Click += (s, e) => SendReasonToParentForm();
            label5.Click += (s, e) => SendReasonToParentForm();

            // Close Button
            ultraPanel6.Click += (s, e) => this.Close();
            ultraPictureBox2.Click += (s, e) => this.Close();
            label3.Click += (s, e) => this.Close();

            // New/Edit/Del Button
            ultraPanel4.Click += (s, e) => OpenLedgerForm();
            ultraPictureBox3.Click += (s, e) => OpenLedgerForm();
            label4.Click += (s, e) => OpenLedgerForm();

            // Navigation
            ConnectNavigationPanelEvents(ultraPanel3, ultraPictureBox5, MoveItemHighlighterUp); // Up
            ConnectNavigationPanelEvents(ultraPanel7, ultraPictureBox6, MoveItemHighlighterDown); // Down
            
            // Toggle sort
            ultraPictureBox4.Click += ToggleSortOrder;
        }

        private void OpenLedgerForm()
        {
            try
            {
                frmReasonMaster master = new frmReasonMaster();
                if (master.ShowDialog() == DialogResult.OK)
                {
                    LoadAllData();
                    if (master.ReasonModel != null && !string.IsNullOrWhiteSpace(master.ReasonModel.ReasonName))
                    {
                        ApplyFilter(master.ReasonModel.ReasonName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Reason Master: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void SetupPanelHoverEffects()
        {
            SetupPanelGroupHoverEffects(ultraPanel5, label5, ultraPictureBox1);
            SetupPanelGroupHoverEffects(ultraPanel6, label3, ultraPictureBox2);
            SetupPanelGroupHoverEffects(ultraPanel4, label4, ultraPictureBox3);
            SetupPanelGroupHoverEffects(ultraPanel3, null, ultraPictureBox5);
            SetupPanelGroupHoverEffects(ultraPanel7, null, ultraPictureBox6);
        }

        private void SetupPanelGroupHoverEffects(Infragistics.Win.Misc.UltraPanel panel, Label label, Infragistics.Win.UltraWinEditors.UltraPictureBox pictureBox)
        {
            if (panel == null) return;
            Color originalBackColor = panel.Appearance.BackColor;
            Color originalBackColor2 = panel.Appearance.BackColor2;
            Color hoverBackColor = BrightenColor(originalBackColor, 30);
            Color hoverBackColor2 = BrightenColor(originalBackColor2, 30);
            Action applyHover = () => {
                panel.Appearance.BackColor = hoverBackColor;
                panel.Appearance.BackColor2 = hoverBackColor2;
                panel.ClientArea.Cursor = Cursors.Hand;
            };
            Action removeHover = () => {
                panel.Appearance.BackColor = originalBackColor;
                panel.Appearance.BackColor2 = originalBackColor2;
                panel.ClientArea.Cursor = Cursors.Default;
            };
            panel.MouseEnter += (s, e) => applyHover();
            panel.MouseLeave += (s, e) => removeHover();
            if (pictureBox != null)
            {
                pictureBox.MouseEnter += (s, e) => applyHover();
                pictureBox.MouseLeave += (s, e) => { if (!panel.ClientRectangle.Contains(panel.PointToClient(Control.MousePosition))) removeHover(); };
            }
            if (label != null)
            {
                label.MouseEnter += (s, e) => applyHover();
                label.MouseLeave += (s, e) => { if (!panel.ClientRectangle.Contains(panel.PointToClient(Control.MousePosition))) removeHover(); };
            }
        }

        private void ConnectNavigationPanelEvents(Infragistics.Win.Misc.UltraPanel panel, Infragistics.Win.UltraWinEditors.UltraPictureBox pictureBox, EventHandler handler)
        {
            panel.Click += handler;
            panel.ClientArea.Click += handler;
            if (pictureBox != null) pictureBox.Click += handler;
        }

        private void MoveItemHighlighterUp(object sender, EventArgs e)
        {
            if (ultraGrid1.Rows.Count == 0) return;
            int currentIndex = ultraGrid1.ActiveRow?.Index ?? 0;
            if (currentIndex > 0)
            {
                ultraGrid1.ActiveRow = ultraGrid1.Rows[currentIndex - 1];
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
            }
        }

        private void MoveItemHighlighterDown(object sender, EventArgs e)
        {
            if (ultraGrid1.Rows.Count == 0) return;
            int currentIndex = ultraGrid1.ActiveRow?.Index ?? -1;
            if (currentIndex < ultraGrid1.Rows.Count - 1)
            {
                ultraGrid1.ActiveRow = ultraGrid1.Rows[currentIndex + 1];
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
            }
        }

        private void ToggleSortOrder(object sender, EventArgs e)
        {
            if (ultraGrid1.DataSource is DataView dataView)
            {
                isOriginalOrder = !isOriginalOrder;
                dataView.Sort = isOriginalOrder ? "OriginalRowOrder ASC" : "OriginalRowOrder DESC";
                ultraGrid1.Refresh();
                UpdateStatus(isOriginalOrder ? "Sorted in original order." : "Sorted in reverse order.");
            }
        }
        #endregion

        #region Column Chooser
        // --- BEGIN: Column Chooser Feature (from frmdialForItemMaster.cs) ---
        private void SetupColumnChooserMenu()
        {
            var gridContextMenu = new ContextMenuStrip();
            var columnChooserMenuItem = new ToolStripMenuItem("Field/Column Chooser");
            columnChooserMenuItem.Click += (s, e) => ShowColumnChooser();
            gridContextMenu.Items.Add(columnChooserMenuItem);
            ultraGrid1.ContextMenuStrip = gridContextMenu;
            
            // Add direct header drag-drop functionality
            SetupDirectHeaderDragDrop();
        }

        // Track mouse position and column for drag
        private Point startPoint;
        private UltraGridColumn columnToMove = null;
        private bool isDraggingColumn = false;

        // Set up direct header drag and drop functionality
        private void SetupDirectHeaderDragDrop()
        {
            // Enable the grid as a drop target
            ultraGrid1.AllowDrop = true;

            // Add drag event handlers for headers
            ultraGrid1.MouseDown += UltraGrid1_MouseDown;
            ultraGrid1.MouseMove += UltraGrid1_MouseMove;
            ultraGrid1.MouseUp += UltraGrid1_MouseUp;
            ultraGrid1.DragOver += UltraGrid1_DragOver;
            ultraGrid1.DragDrop += UltraGrid1_DragDrop;
            
            // Create column chooser form but don't show it yet
            CreateColumnChooserForm();
        }

        // Create the column chooser form without showing it
        private void CreateColumnChooserForm()
        {
            columnChooserForm = new Form {
                Text = "Customization", 
                Size = new Size(220, 280), 
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
                StartPosition = FormStartPosition.Manual, // Manual for positioning
                TopMost = true,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false
            };
            
            columnChooserListBox = new ListBox { 
                Dock = DockStyle.Fill, 
                AllowDrop = true 
            };
            
            // Add the event handlers for the ListBox
            columnChooserListBox.MouseDown += ColumnChooserListBox_MouseDown;
            columnChooserListBox.DragOver += ColumnChooserListBox_DragOver;
            columnChooserListBox.DragDrop += ColumnChooserListBox_DragDrop;
            
            columnChooserForm.Controls.Add(columnChooserListBox);
            
            // Add hidden columns to the ListBox
            PopulateColumnChooserListBox();
            
            // Handle form closing to save position
            columnChooserForm.FormClosing += ColumnChooserForm_FormClosing;
        }

        // Position the column chooser at the bottom right of the grid
        private void PositionColumnChooserAtBottomRight()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                // Convert grid's bottom-right to screen coordinates
                Point gridBottomRight = ultraGrid1.PointToScreen(
                    new Point(ultraGrid1.Width, ultraGrid1.Height));
                
                // Position the column chooser form just to the right of the grid
                columnChooserForm.Location = new Point(
                    gridBottomRight.X - columnChooserForm.Width,
                    gridBottomRight.Y - columnChooserForm.Height);
                
                // Make sure it's visible on the screen
                Rectangle screenBounds = Screen.GetWorkingArea(this);
                if (columnChooserForm.Right > screenBounds.Right)
                    columnChooserForm.Left = screenBounds.Right - columnChooserForm.Width;
                if (columnChooserForm.Bottom > screenBounds.Bottom)
                    columnChooserForm.Top = screenBounds.Bottom - columnChooserForm.Height;
            }
        }

        // Handle mouse down on grid to initiate drag
        private void UltraGrid1_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                // Reset drag state
                isDraggingColumn = false;
                columnToMove = null;

                // Store the mouse down position
                startPoint = new Point(e.X, e.Y);

                // If we're in the header area, try to determine which column
                if (e.Y < 40) // Assuming header is in the top 40 pixels
                {
                    // Calculate horizontal position of each column
                    int xPos = 0;

                    // Account for row selector width if present
                    if (ultraGrid1.DisplayLayout.Override.RowSelectors == Infragistics.Win.DefaultableBoolean.True)
                    {
                        xPos += ultraGrid1.DisplayLayout.Override.RowSelectorWidth;
                    }

                    // Find which column contains the x position
                    foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                    {
                        if (!col.Hidden)
                        {
                            // Check if click is within this column's width
                            if (e.X >= xPos && e.X < xPos + col.Width)
                            {
                                columnToMove = col;
                                isDraggingColumn = true;
                                break;
                            }

                            xPos += col.Width;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error in mouse down: {ex.Message}");
                isDraggingColumn = false;
                columnToMove = null;
            }
        }

        // Method to directly hide a column and add it to the column chooser
        private void HideColumn(UltraGridColumn column)
        {
            try
            {
                if (column != null && !column.Hidden)
                {
                    // Get column name before hiding it
                    string columnName = !string.IsNullOrEmpty(column.Header.Caption) ?
                                    column.Header.Caption : column.Key;

                    // Store the column width before hiding it
                    savedColumnWidths[column.Key] = column.Width;

                    // Temporarily disable layout updates to prevent visual flicker
                    ultraGrid1.SuspendLayout();

                    // Hide the specific column that was dragged
                    column.Hidden = true;

                    // Ensure other columns don't resize by explicitly setting their widths
                    foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                    {
                        if (!col.Hidden && savedColumnWidths.ContainsKey(col.Key))
                        {
                            col.Width = savedColumnWidths[col.Key];
                        }
                    }

                    // Resume layout updates
                    ultraGrid1.ResumeLayout();

                    // Make sure the column chooser exists but don't show it automatically
                    if (columnChooserForm == null || columnChooserForm.IsDisposed)
                    {
                        CreateColumnChooserForm();
                    }

                    // Add to column chooser listbox without showing it automatically
                    if (columnChooserListBox != null)
                    {
                        // Check if this column is already in the list to prevent duplicates
                        bool alreadyExists = false;
                        foreach (object item in columnChooserListBox.Items)
                        {
                            if (item is ColumnItem columnItem && columnItem.ColumnKey == column.Key)
                            {
                                alreadyExists = true;
                                break;
                            }
                        }

                        // Only add if it doesn't already exist
                        if (!alreadyExists)
                        {
                            columnChooserListBox.Items.Add(new ColumnItem(column.Key, columnName));
                        }
                    }
                    
                    // Show the column chooser if it's not already visible
                    if (!columnChooserForm.Visible)
                    {
                        ShowColumnChooser();
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error hiding column: {ex.Message}");
            }
        }

        // Handle mouse move to initiate drag if needed
        private void UltraGrid1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                // Only track movement if we're dragging a column
                if (e.Button == MouseButtons.Left && columnToMove != null && isDraggingColumn)
                {
                    // Calculate how far the mouse has moved
                    int deltaX = Math.Abs(e.X - startPoint.X);
                    int deltaY = Math.Abs(e.Y - startPoint.Y);

                    // Only start drag if moved beyond threshold
                    if (deltaX > SystemInformation.DragSize.Width || deltaY > SystemInformation.DragSize.Height)
                    {
                        // Check if moving primarily downward (column to chooser)
                        bool isDraggingDown = (e.Y > startPoint.Y && deltaY > deltaX);

                        if (isDraggingDown)
                        {
                            // Change cursor to indicate a drag operation
                            ultraGrid1.Cursor = Cursors.No;

                            // Show tooltip with hint
                            string columnName = !string.IsNullOrEmpty(columnToMove.Header.Caption) ?
                                    columnToMove.Header.Caption : columnToMove.Key;
                            toolTip.SetToolTip(ultraGrid1, $"Drag down to hide '{columnName}' column");

                            // If dragged downward more than 50 pixels, hide the column
                            if (e.Y - startPoint.Y > 50)
                            {
                                // Hide the column and add to customization
                                HideColumn(columnToMove);

                                // Reset drag state
                                columnToMove = null;
                                isDraggingColumn = false;
                                ultraGrid1.Cursor = Cursors.Default;
                                toolTip.SetToolTip(ultraGrid1, "");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error in mouse move: {ex.Message}");
                ultraGrid1.Cursor = Cursors.Default;
                toolTip.SetToolTip(ultraGrid1, "");
            }
        }

        // Event handler for mouse up on the grid
        private void UltraGrid1_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                // Reset cursor
                ultraGrid1.Cursor = Cursors.Default;
                toolTip.SetToolTip(ultraGrid1, "");

                // Reset drag state
                isDraggingColumn = false;
                columnToMove = null;
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error in mouse up: {ex.Message}");
            }
        }

        private void ShowColumnChooser()
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed)
            {
                // Position and show the existing form
                PositionColumnChooserAtBottomRight();
                columnChooserForm.Show();
                columnChooserForm.BringToFront();
                HighlightColumnChooserForm();
                return;
            }
            
            // Create a new form if needed
            CreateColumnChooserForm();
            PositionColumnChooserAtBottomRight();
            columnChooserForm.Show(this);
            HighlightColumnChooserForm();
        }

        private void HighlightColumnChooserForm()
        {
            // Flash the column chooser form to draw attention to it
            try
            {
                if (columnChooserForm != null && columnChooserForm.Visible && !columnChooserForm.IsDisposed)
                {
                    // Store original background color
                    Color originalColor = columnChooserForm.BackColor;
                    
                    // Flash effect with a timer
                    System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
                    timer.Interval = 100;
                    int flashCount = 0;
                    
                    timer.Tick += (s, e) => {
                        flashCount++;
                        
                        if (flashCount % 2 == 1)
                            columnChooserForm.BackColor = Color.LightBlue;
                        else
                            columnChooserForm.BackColor = originalColor;
                        
                        if (flashCount >= 6)
                        {
                            timer.Stop();
                            columnChooserForm.BackColor = originalColor;
                            timer.Dispose();
                        }
                    };
                    
                    timer.Start();
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error highlighting column chooser: {ex.Message}");
            }
        }

        private void ColumnChooserForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Save the state of the column chooser when it's closed
            // This could include its position, size, and current selection
            try
            {
                // Just hide the form instead of closing it
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    columnChooserForm.Hide();
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error in column chooser form closing: {ex.Message}");
            }
        }

        private void PopulateColumnChooserListBox()
        {
            try
            {
                if (columnChooserListBox == null) return;
                
                columnChooserListBox.Items.Clear();
                
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                    {
                        if (col.Hidden)
                        {
                            string columnName = !string.IsNullOrEmpty(col.Header.Caption) ? 
                                          col.Header.Caption : col.Key;
                            
                            columnChooserListBox.Items.Add(new ColumnItem(col.Key, columnName));
                        }
                    }
                }
                
                // Update the status to show how many columns are hidden
                UpdateStatus($"{columnChooserListBox.Items.Count} columns are hidden.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error populating column chooser: {ex.Message}");
            }
        }

        private void ColumnChooserListBox_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                // Get the index of the item under the mouse
                int index = columnChooserListBox.IndexFromPoint(e.Location);
                
                // If a valid item was clicked, start a drag operation
                if (index != ListBox.NoMatches && e.Button == MouseButtons.Left)
                {
                    // Use DoDragDrop to start the drag operation
                    columnChooserListBox.DoDragDrop(columnChooserListBox.Items[index], DragDropEffects.Move);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error in column chooser mouse down: {ex.Message}");
            }
        }

        private void ColumnChooserListBox_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                // Only accept specific drag data formats
                if (e.Data.GetDataPresent(typeof(string)) || 
                    e.Data.GetDataPresent(typeof(ColumnItem)))
                {
                    e.Effect = DragDropEffects.Move;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error in column chooser drag over: {ex.Message}");
                e.Effect = DragDropEffects.None;
            }
        }

        private void ColumnChooserListBox_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                // Handle drops from the grid (column headers)
                if (e.Data.GetDataPresent(typeof(UltraGridColumn)))
                {
                    UltraGridColumn column = (UltraGridColumn)e.Data.GetData(typeof(UltraGridColumn));
                    if (column != null && !column.Hidden)
                    {
                        // Store the column width before hiding it
                        savedColumnWidths[column.Key] = column.Width;
                        
                        // Hide the column
                        column.Hidden = true;
                        
                        // Add to the listbox
                        string columnName = !string.IsNullOrEmpty(column.Header.Caption) ? 
                                          column.Header.Caption : column.Key;
                        
                        columnChooserListBox.Items.Add(new ColumnItem(column.Key, columnName));
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error in column chooser drag drop: {ex.Message}");
            }
        }

        private void UltraGrid1_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                // Accept ColumnItem objects from the column chooser
                if (e.Data.GetDataPresent(typeof(ColumnItem)))
                {
                    e.Effect = DragDropEffects.Move;
                    
                    // Optional: Show a tooltip or visual indicator of where the column will be shown
                    ColumnItem item = (ColumnItem)e.Data.GetData(typeof(ColumnItem));
                    if (item != null)
                    {
                        toolTip.SetToolTip(ultraGrid1, $"Drop to show '{item.DisplayText}' column");
                    }
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error in grid drag over: {ex.Message}");
                e.Effect = DragDropEffects.None;
            }
        }

        private void UltraGrid1_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                // Check if the dragged data is a ColumnItem
                if (e.Data.GetDataPresent(typeof(ColumnItem)))
                {
                    ColumnItem item = (ColumnItem)e.Data.GetData(typeof(ColumnItem));
                    if (item != null)
                    {
                        // Find the column by its key
                        UltraGridColumn column = ultraGrid1.DisplayLayout.Bands[0].Columns[item.ColumnKey];
                        if (column != null)
                        {
                            // Show the column
                            column.Hidden = false;
                            
                            // Restore the original width if available
                            if (savedColumnWidths.ContainsKey(column.Key))
                            {
                                column.Width = savedColumnWidths[column.Key];
                            }
                            
                            // Remove the item from the column chooser listbox
                            for (int i = 0; i < columnChooserListBox.Items.Count; i++)
                            {
                                if (columnChooserListBox.Items[i] is ColumnItem listItem && 
                                    listItem.ColumnKey == item.ColumnKey)
                                {
                                    columnChooserListBox.Items.RemoveAt(i);
                                    break;
                                }
                            }
                            
                            // Update status
                            UpdateStatus($"Column '{item.DisplayText}' shown.");
                        }
                    }
                }
                
                // Clear any tooltips
                toolTip.SetToolTip(ultraGrid1, "");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error in grid drag drop: {ex.Message}");
            }
        }

        private class ColumnItem
        {
            public string ColumnKey { get; }
            public string DisplayText { get; }
            
            public ColumnItem(string key, string text)
            {
                ColumnKey = key;
                DisplayText = text;
            }
            
            public override string ToString()
            {
                return DisplayText;
            }
        }
        // --- END: Column Chooser Feature ---
        #endregion

        #region Helpers
        private void PreserveColumnWidths()
        {
            try
            {
                ultraGrid1.SuspendLayout();
                foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    if (!col.Hidden && savedColumnWidths.ContainsKey(col.Key))
                        col.Width = savedColumnWidths[col.Key];
                }
                ultraGrid1.ResumeLayout();
            }
            catch (Exception ex) { UpdateStatus($"Width preservation error: {ex.Message}"); }
        }

        private void InitializeSavedColumnWidths()
        {
            savedColumnWidths.Clear();
            if (ultraGrid1.DisplayLayout.Bands.Count > 0)
            {
                foreach (UltraGridColumn column in ultraGrid1.DisplayLayout.Bands[0].Columns)
                {
                    if (!column.Hidden) savedColumnWidths[column.Key] = column.Width;
                }
            }
        }
        
        private void PreserveOriginalRowOrder(DataTable table)
        {
            if (!table.Columns.Contains("OriginalRowOrder"))
            {
                table.Columns.Add("OriginalRowOrder", typeof(int));
                for (int i = 0; i < table.Rows.Count; i++)
                    table.Rows[i]["OriginalRowOrder"] = i;
            }
        }

        private void UpdateRecordCountLabel()
        {
            if (label1 != null && fullDataTable != null)
            {
                int displayedCount = ultraGrid1.Rows.Count;
                int totalCount = fullDataTable.Rows.Count;
                label1.Text = $"Showing {displayedCount} of {totalCount} records";
            }
        }
        
        private void ReorderColumns(string selectedColumn)
        {
            if (ultraGrid1.DisplayLayout.Bands.Count == 0) return;

            string columnKey = GetColumnKeyFromDisplayName(selectedColumn);
            if (string.IsNullOrEmpty(columnKey) || !ultraGrid1.DisplayLayout.Bands[0].Columns.Exists(columnKey)) return;
            
            ultraGrid1.SuspendLayout();
            var otherCols = ultraGrid1.DisplayLayout.Bands[0].Columns.Cast<UltraGridColumn>()
                .Where(c => c.Key != columnKey).ToList();
            
            ultraGrid1.DisplayLayout.Bands[0].Columns[columnKey].Header.VisiblePosition = 0;
            for (int i = 0; i < otherCols.Count; i++)
            {
                otherCols[i].Header.VisiblePosition = i + 1;
            }
            ultraGrid1.ResumeLayout();
        }

        private string GetColumnKeyFromDisplayName(string displayName)
        {
            switch (displayName)
            {
                case "Reason Name": return "ReasonName";
                case "Ledger ID": return "LedgerID";
                default: return string.Empty;
            }
        }

        private Color BrightenColor(Color color, int amount)
        {
            return Color.FromArgb(color.A, Math.Min(color.R + amount, 255), Math.Min(color.G + amount, 255), Math.Min(color.B + amount, 255));
        }

        private bool IsMouseOverControl(Control control)
        {
            Point mousePos = control.PointToClient(Control.MousePosition);
            return control.ClientRectangle.Contains(mousePos);
        }
        #endregion

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmReasonDialog));
            Infragistics.Win.Appearance appearance1 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance2 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance3 = new Infragistics.Win.Appearance();
            this.ultPanelPurchaseDisplay = new Infragistics.Win.Misc.UltraPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.ultraPanel3 = new Infragistics.Win.Misc.UltraPanel();
            this.ultraPictureBox5 = new Infragistics.Win.UltraWinEditors.UltraPictureBox();
            this.ultraPanel7 = new Infragistics.Win.Misc.UltraPanel();
            this.ultraPictureBox6 = new Infragistics.Win.UltraWinEditors.UltraPictureBox();
            this.ultraGrid1 = new Infragistics.Win.UltraWinGrid.UltraGrid();
            this.ultraPanel2 = new Infragistics.Win.Misc.UltraPanel();
            this.ultraPanel9 = new Infragistics.Win.Misc.UltraPanel();
            this.ultraPictureBox4 = new Infragistics.Win.UltraWinEditors.UltraPictureBox();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxsearch = new System.Windows.Forms.TextBox();
            this.ultraPanel4 = new Infragistics.Win.Misc.UltraPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.ultraPictureBox3 = new Infragistics.Win.UltraWinEditors.UltraPictureBox();
            this.ultraPanel8 = new Infragistics.Win.Misc.UltraPanel();
            this.ultraPanel6 = new Infragistics.Win.Misc.UltraPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.ultraPictureBox2 = new Infragistics.Win.UltraWinEditors.UltraPictureBox();
            this.ultraPanel5 = new Infragistics.Win.Misc.UltraPanel();
            this.label5 = new System.Windows.Forms.Label();
            this.ultraPictureBox1 = new Infragistics.Win.UltraWinEditors.UltraPictureBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.ultPanelPurchaseDisplay.ClientArea.SuspendLayout();
            this.ultPanelPurchaseDisplay.SuspendLayout();
            this.ultraPanel3.ClientArea.SuspendLayout();
            this.ultraPanel3.SuspendLayout();
            this.ultraPanel7.ClientArea.SuspendLayout();
            this.ultraPanel7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGrid1)).BeginInit();
            this.ultraPanel2.ClientArea.SuspendLayout();
            this.ultraPanel2.SuspendLayout();
            this.ultraPanel9.ClientArea.SuspendLayout();
            this.ultraPanel9.SuspendLayout();
            this.ultraPanel4.ClientArea.SuspendLayout();
            this.ultraPanel4.SuspendLayout();
            this.ultraPanel8.SuspendLayout();
            this.ultraPanel6.ClientArea.SuspendLayout();
            this.ultraPanel6.SuspendLayout();
            this.ultraPanel5.ClientArea.SuspendLayout();
            this.ultraPanel5.SuspendLayout();
            this.SuspendLayout();
            // 
            // ultPanelPurchaseDisplay
            // 
            // 
            // ultPanelPurchaseDisplay.ClientArea
            // 
            this.ultPanelPurchaseDisplay.ClientArea.Controls.Add(this.label1);
            this.ultPanelPurchaseDisplay.ClientArea.Controls.Add(this.ultraPanel3);
            this.ultPanelPurchaseDisplay.ClientArea.Controls.Add(this.ultraPanel7);
            this.ultPanelPurchaseDisplay.ClientArea.Controls.Add(this.ultraGrid1);
            this.ultPanelPurchaseDisplay.ClientArea.Controls.Add(this.ultraPanel2);
            this.ultPanelPurchaseDisplay.ClientArea.Controls.Add(this.textBoxsearch);
            this.ultPanelPurchaseDisplay.ClientArea.Controls.Add(this.ultraPanel4);
            this.ultPanelPurchaseDisplay.ClientArea.Controls.Add(this.ultraPanel8);
            this.ultPanelPurchaseDisplay.ClientArea.Controls.Add(this.ultraPanel6);
            this.ultPanelPurchaseDisplay.ClientArea.Controls.Add(this.ultraPanel5);
            this.ultPanelPurchaseDisplay.ClientArea.Controls.Add(this.textBox3);
            this.ultPanelPurchaseDisplay.Location = new System.Drawing.Point(-1, -1);
            this.ultPanelPurchaseDisplay.Name = "ultPanelPurchaseDisplay";
            this.ultPanelPurchaseDisplay.Size = new System.Drawing.Size(789, 577);
            this.ultPanelPurchaseDisplay.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(6, 414);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(164, 16);
            this.label1.TabIndex = 54;
            this.label1.Text = "Showing 0 of 0 records";
            // 
            // ultraPanel3
            // 
            // 
            // ultraPanel3.ClientArea
            // 
            this.ultraPanel3.ClientArea.Controls.Add(this.ultraPictureBox5);
            this.ultraPanel3.Location = new System.Drawing.Point(684, 71);
            this.ultraPanel3.Name = "ultraPanel3";
            this.ultraPanel3.Size = new System.Drawing.Size(61, 54);
            this.ultraPanel3.TabIndex = 44;
            // 
            // ultraPictureBox5
            // 
            this.ultraPictureBox5.BackColor = System.Drawing.Color.Transparent;
            this.ultraPictureBox5.BackColorInternal = System.Drawing.Color.Transparent;
            this.ultraPictureBox5.BorderShadowColor = System.Drawing.Color.Transparent;
            this.ultraPictureBox5.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ultraPictureBox5.Image = ((object)(resources.GetObject("ultraPictureBox5.Image")));
            this.ultraPictureBox5.Location = new System.Drawing.Point(5, 8);
            this.ultraPictureBox5.Name = "ultraPictureBox5";
            this.ultraPictureBox5.Size = new System.Drawing.Size(46, 31);
            this.ultraPictureBox5.TabIndex = 40;
            // 
            // ultraPanel7
            // 
            // 
            // ultraPanel7.ClientArea
            // 
            this.ultraPanel7.ClientArea.Controls.Add(this.ultraPictureBox6);
            this.ultraPanel7.Location = new System.Drawing.Point(684, 135);
            this.ultraPanel7.Name = "ultraPanel7";
            this.ultraPanel7.Size = new System.Drawing.Size(61, 54);
            this.ultraPanel7.TabIndex = 45;
            // 
            // ultraPictureBox6
            // 
            this.ultraPictureBox6.BackColor = System.Drawing.Color.Transparent;
            this.ultraPictureBox6.BackColorInternal = System.Drawing.Color.Transparent;
            this.ultraPictureBox6.BorderShadowColor = System.Drawing.Color.Transparent;
            this.ultraPictureBox6.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ultraPictureBox6.Image = ((object)(resources.GetObject("ultraPictureBox6.Image")));
            this.ultraPictureBox6.Location = new System.Drawing.Point(5, 10);
            this.ultraPictureBox6.Name = "ultraPictureBox6";
            this.ultraPictureBox6.Size = new System.Drawing.Size(46, 31);
            this.ultraPictureBox6.TabIndex = 41;
            // 
            // ultraGrid1
            // 
            this.ultraGrid1.Location = new System.Drawing.Point(5, 69);
            this.ultraGrid1.Name = "ultraGrid1";
            this.ultraGrid1.Size = new System.Drawing.Size(673, 322);
            this.ultraGrid1.TabIndex = 0;
            this.ultraGrid1.DoubleClickCell += new Infragistics.Win.UltraWinGrid.DoubleClickCellEventHandler(this.ultraGrid1_DoubleClickCell);
            // 
            // ultraPanel2
            // 
            appearance1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(93)))), ((int)(((byte)(144)))));
            this.ultraPanel2.Appearance = appearance1;
            // 
            // ultraPanel2.ClientArea
            // 
            this.ultraPanel2.ClientArea.Controls.Add(this.ultraPanel9);
            this.ultraPanel2.ClientArea.Controls.Add(this.comboBox2);
            this.ultraPanel2.ClientArea.Controls.Add(this.comboBox1);
            this.ultraPanel2.ClientArea.Controls.Add(this.lblSearch);
            this.ultraPanel2.ClientArea.Controls.Add(this.label2);
            this.ultraPanel2.Location = new System.Drawing.Point(-1, 0);
            this.ultraPanel2.Name = "ultraPanel2";
            this.ultraPanel2.Size = new System.Drawing.Size(802, 37);
            this.ultraPanel2.TabIndex = 43;
            // 
            // ultraPanel9
            // 
            appearance2.BackColor = System.Drawing.Color.White;
            this.ultraPanel9.Appearance = appearance2;
            this.ultraPanel9.BorderStyle = Infragistics.Win.UIElementBorderStyle.WindowsVista;
            // 
            // ultraPanel9.ClientArea
            // 
            this.ultraPanel9.ClientArea.Controls.Add(this.ultraPictureBox4);
            this.ultraPanel9.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraPanel9.Location = new System.Drawing.Point(685, 2);
            this.ultraPanel9.Name = "ultraPanel9";
            this.ultraPanel9.Size = new System.Drawing.Size(90, 32);
            this.ultraPanel9.TabIndex = 6;
            // 
            // ultraPictureBox4
            // 
            this.ultraPictureBox4.BackColor = System.Drawing.Color.Transparent;
            this.ultraPictureBox4.BackColorInternal = System.Drawing.Color.Transparent;
            this.ultraPictureBox4.BorderShadowColor = System.Drawing.Color.Transparent;
            this.ultraPictureBox4.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ultraPictureBox4.Image = ((object)(resources.GetObject("ultraPictureBox4.Image")));
            this.ultraPictureBox4.Location = new System.Drawing.Point(-1, 0);
            this.ultraPictureBox4.Name = "ultraPictureBox4";
            this.ultraPictureBox4.Size = new System.Drawing.Size(88, 32);
            this.ultraPictureBox4.TabIndex = 41;
            // 
            // comboBox2
            // 
            this.comboBox2.Font = new System.Drawing.Font("Microsoft Tai Le", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Location = new System.Drawing.Point(429, 2);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(250, 34);
            this.comboBox2.TabIndex = 5;
            // 
            // comboBox1
            // 
            this.comboBox1.Font = new System.Drawing.Font("Microsoft Tai Le", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(72, 2);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(262, 34);
            this.comboBox1.TabIndex = 3;
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.BackColor = System.Drawing.Color.Transparent;
            this.lblSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSearch.ForeColor = System.Drawing.Color.White;
            this.lblSearch.Location = new System.Drawing.Point(7, 11);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(59, 17);
            this.lblSearch.TabIndex = 0;
            this.lblSearch.Text = "Search";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(362, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "Sort By";
            // 
            // textBoxsearch
            // 
            this.textBoxsearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxsearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxsearch.Location = new System.Drawing.Point(5, 41);
            this.textBoxsearch.Name = "textBoxsearch";
            this.textBoxsearch.Size = new System.Drawing.Size(673, 24);
            this.textBoxsearch.TabIndex = 1;
            // 
            // ultraPanel4
            // 
            // 
            // ultraPanel4.ClientArea
            // 
            this.ultraPanel4.ClientArea.Controls.Add(this.label4);
            this.ultraPanel4.ClientArea.Controls.Add(this.ultraPictureBox3);
            this.ultraPanel4.Location = new System.Drawing.Point(261, 440);
            this.ultraPanel4.Name = "ultraPanel4";
            this.ultraPanel4.Size = new System.Drawing.Size(184, 51);
            this.ultraPanel4.TabIndex = 46;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Font = new System.Drawing.Font("Microsoft PhagsPa", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(49, 11);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(122, 24);
            this.label4.TabIndex = 7;
            this.label4.Text = "New/Edit/Del";
            // 
            // ultraPictureBox3
            // 
            this.ultraPictureBox3.BackColor = System.Drawing.Color.Transparent;
            this.ultraPictureBox3.BackColorInternal = System.Drawing.Color.Transparent;
            this.ultraPictureBox3.BorderShadowColor = System.Drawing.Color.Transparent;
            this.ultraPictureBox3.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ultraPictureBox3.Image = ((object)(resources.GetObject("ultraPictureBox3.Image")));
            this.ultraPictureBox3.Location = new System.Drawing.Point(-5, 6);
            this.ultraPictureBox3.Name = "ultraPictureBox3";
            this.ultraPictureBox3.Size = new System.Drawing.Size(57, 35);
            this.ultraPictureBox3.TabIndex = 42;
            // 
            // ultraPanel8
            // 
            appearance3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(116)))), ((int)(((byte)(217)))));
            appearance3.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(219)))), ((int)(((byte)(255)))));
            appearance3.BackGradientStyle = Infragistics.Win.GradientStyle.GlassTop37;
            this.ultraPanel8.Appearance = appearance3;
            this.ultraPanel8.BorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded1;
            this.ultraPanel8.ForeColor = System.Drawing.Color.White;
            this.ultraPanel8.Location = new System.Drawing.Point(5, 388);
            this.ultraPanel8.Name = "ultraPanel8";
            this.ultraPanel8.Size = new System.Drawing.Size(673, 23);
            this.ultraPanel8.TabIndex = 47;
            // 
            // ultraPanel6
            // 
            // 
            // ultraPanel6.ClientArea
            // 
            this.ultraPanel6.ClientArea.Controls.Add(this.label3);
            this.ultraPanel6.ClientArea.Controls.Add(this.ultraPictureBox2);
            this.ultraPanel6.Location = new System.Drawing.Point(135, 440);
            this.ultraPanel6.Name = "ultraPanel6";
            this.ultraPanel6.Size = new System.Drawing.Size(116, 51);
            this.ultraPanel6.TabIndex = 45;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("Microsoft PhagsPa", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(50, 11);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 24);
            this.label3.TabIndex = 7;
            this.label3.Text = "Close";
            // 
            // ultraPictureBox2
            // 
            this.ultraPictureBox2.BackColor = System.Drawing.Color.Transparent;
            this.ultraPictureBox2.BackColorInternal = System.Drawing.Color.Transparent;
            this.ultraPictureBox2.BorderShadowColor = System.Drawing.Color.Transparent;
            this.ultraPictureBox2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ultraPictureBox2.Image = ((object)(resources.GetObject("ultraPictureBox2.Image")));
            this.ultraPictureBox2.Location = new System.Drawing.Point(-3, 3);
            this.ultraPictureBox2.Name = "ultraPictureBox2";
            this.ultraPictureBox2.Size = new System.Drawing.Size(60, 42);
            this.ultraPictureBox2.TabIndex = 42;
            // 
            // ultraPanel5
            // 
            // 
            // ultraPanel5.ClientArea
            // 
            this.ultraPanel5.ClientArea.Controls.Add(this.label5);
            this.ultraPanel5.ClientArea.Controls.Add(this.ultraPictureBox1);
            this.ultraPanel5.Location = new System.Drawing.Point(5, 440);
            this.ultraPanel5.Name = "ultraPanel5";
            this.ultraPanel5.Size = new System.Drawing.Size(116, 51);
            this.ultraPanel5.TabIndex = 44;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Font = new System.Drawing.Font("Microsoft PhagsPa", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(64, 11);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 24);
            this.label5.TabIndex = 8;
            this.label5.Text = "OK";
            // 
            // ultraPictureBox1
            // 
            this.ultraPictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.ultraPictureBox1.BackColorInternal = System.Drawing.Color.Transparent;
            this.ultraPictureBox1.BorderShadowColor = System.Drawing.Color.Transparent;
            this.ultraPictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ultraPictureBox1.Image = ((object)(resources.GetObject("ultraPictureBox1.Image")));
            this.ultraPictureBox1.Location = new System.Drawing.Point(-2, 3);
            this.ultraPictureBox1.Name = "ultraPictureBox1";
            this.ultraPictureBox1.Size = new System.Drawing.Size(60, 42);
            this.ultraPictureBox1.TabIndex = 42;
            // 
            // textBox3
            // 
            this.textBox3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox3.Location = new System.Drawing.Point(684, 41);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(90, 24);
            this.textBox3.TabIndex = 2;
            // 
            // frmReasonDialog
            // 
            this.ClientSize = new System.Drawing.Size(786, 505);
            this.Controls.Add(this.ultPanelPurchaseDisplay);
            this.Name = "frmReasonDialog";
            this.Text = "          ";
            this.ultPanelPurchaseDisplay.ClientArea.ResumeLayout(false);
            this.ultPanelPurchaseDisplay.ClientArea.PerformLayout();
            this.ultPanelPurchaseDisplay.ResumeLayout(false);
            this.ultraPanel3.ClientArea.ResumeLayout(false);
            this.ultraPanel3.ResumeLayout(false);
            this.ultraPanel7.ClientArea.ResumeLayout(false);
            this.ultraPanel7.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGrid1)).EndInit();
            this.ultraPanel2.ClientArea.ResumeLayout(false);
            this.ultraPanel2.ClientArea.PerformLayout();
            this.ultraPanel2.ResumeLayout(false);
            this.ultraPanel9.ClientArea.ResumeLayout(false);
            this.ultraPanel9.ResumeLayout(false);
            this.ultraPanel4.ClientArea.ResumeLayout(false);
            this.ultraPanel4.ClientArea.PerformLayout();
            this.ultraPanel4.ResumeLayout(false);
            this.ultraPanel8.ResumeLayout(false);
            this.ultraPanel6.ClientArea.ResumeLayout(false);
            this.ultraPanel6.ClientArea.PerformLayout();
            this.ultraPanel6.ResumeLayout(false);
            this.ultraPanel5.ClientArea.ResumeLayout(false);
            this.ultraPanel5.ClientArea.PerformLayout();
            this.ultraPanel5.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private void frmReasonDialog_Load(object sender, EventArgs e)
        {
            try
            {
                UpdateStatus("Form loading...");
                LoadAllData();
                ApplyFilter(string.Empty);
                textBoxsearch.Focus();
                InitializeSavedColumnWidths();
                UpdateRecordCountLabel();
                UpdateStatus("Form loaded successfully.");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error loading form: {ex.Message}");
            }
        }
    }
}
