using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using Infragistics.Win.UltraWinEditors;
using ModelClass;
using PosBranch_Win.Accounts;
using Repository.Accounts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PosBranch_Win.DialogBox
{
    /// <summary>
    /// Dialog for listing and selecting Debit Notes with professional UI features.
    /// </summary>
    public partial class frmDebitNoteList : Form
    {
        private DebitNoteRepository debitNoteRepo = new DebitNoteRepository();
        private DataTable fullDataTable = null;
        private Label lblStatus;
        private Form columnChooserForm = null;
        private ListBox columnChooserListBox = null;
        private Dictionary<string, int> savedColumnWidths = new Dictionary<string, int>();
        private bool isOriginalOrder = true;
        private System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();

        // Event for when a Debit Note is selected
        public delegate void DebitNoteSelectedHandler(int voucherId);
        public event DebitNoteSelectedHandler OnDebitNoteSelected;

        public frmDebitNoteList()
        {
            InitializeComponent();
            this.KeyPreview = true;

            InitializeStatusLabel();
            SetupUltraGridStyle();
            SetupColumnChooserMenu();
            InitializeSearchFilterComboBox();
            InitializeColumnOrderComboBox();
            SetupPanelHoverEffects();
            ConnectNavigationPanelEvents();

            // Event handlers for grid
            this.ultraGrid1.InitializeLayout += ultraGrid1_InitializeLayout;
            this.ultraGrid1.KeyDown += ultraGrid1_KeyDown;
            this.ultraGrid1.DoubleClickRow += ultraGrid1_DoubleClickRow;
            this.ultraGrid1.AfterSelectChange += ultraGrid1_AfterSelectChange;

            // Form key handler
            this.KeyDown += frmDebitNoteList_KeyDown;

            // Search placeholder logic
            this.textBoxsearch.GotFocus += textBoxsearch_GotFocus;
            this.textBoxsearch.LostFocus += textBoxsearch_LostFocus;
            this.textBoxsearch.Text = "Search debit notes...";
            this.textBoxsearch.ForeColor = Color.Gray;

            // Resize handle for column chooser
            this.SizeChanged += FrmDebitNoteList_SizeChanged;
            this.LocationChanged += (s, e) => PositionColumnChooserAtBottomRight();
            this.Activated += (s, e) => PositionColumnChooserAtBottomRight();
            this.FormClosing += FrmDebitNoteList_FormClosing;
            this.ultraGrid1.Resize += UltraGrid1_Resize;
        }

        private void frmDebitNoteList_Load(object sender, EventArgs e)
        {
            LoadDebitNotes();
            this.BeginInvoke(new Action(() =>
            {
                textBoxsearch.Focus();
                textBoxsearch.Select();
            }));
        }

        private void LoadDebitNotes()
        {
            try
            {
                UpdateStatus("Loading debit notes...");
                fullDataTable = debitNoteRepo.GetAllDebitNotes(Convert.ToInt32(DataBase.BranchId), SessionContext.FinYearId, 0, 1000);
                PreserveOriginalRowOrder(fullDataTable);

                ultraGrid1.DataSource = fullDataTable;

                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                }

                InitializeSavedColumnWidths();
                UpdateRecordCountLabel();
                UpdateStatus($"Found {fullDataTable.Rows.Count} Debit Notes.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading debit notes: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Error loading data.");
            }
        }

        private void PreserveOriginalRowOrder(DataTable table)
        {
            if (table == null) return;
            if (!table.Columns.Contains("OriginalRowOrder"))
            {
                DataColumn orderColumn = new DataColumn("OriginalRowOrder", typeof(int));
                table.Columns.Add(orderColumn);
                int rowIndex = 0;
                foreach (DataRow row in table.Rows)
                {
                    row["OriginalRowOrder"] = rowIndex++;
                }
            }
        }

        private void ultraGrid1_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
            try
            {
                // Hide all columns first
                foreach (UltraGridColumn col in e.Layout.Bands[0].Columns)
                {
                    col.Hidden = true;
                }

                // Show columns matching IRS POS format
                ShowColumn(e, "PH_ACCTCODE", "Account Code", 100);
                ShowColumn(e, "VENDOR_NAME", "Vendor Name", 250);
                ShowColumn(e, "PH_DOCNO", "Doc No", 110);
                ShowColumn(e, "PH_UPDDATE", "Date", 100, "dd/MMM/yyyy");
                ShowColumn(e, "PH_DOCAMT", "Amount", 110, "N4", HAlign.Right);
                ShowColumn(e, "PH_MODE", "Mode", 80);
                ShowColumn(e, "PH_REMARK", "Remark", 200);
                ShowColumn(e, "PH_STATUS", "Status", 100);

                if (e.Layout.Bands[0].Columns.Exists("OriginalRowOrder"))
                {
                    e.Layout.Bands[0].Columns["OriginalRowOrder"].Hidden = true;
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error initializing layout: {ex.Message}");
            }
        }

        private void ShowColumn(InitializeLayoutEventArgs e, string key, string caption, int width, string format = null, HAlign align = HAlign.Left)
        {
            if (e.Layout.Bands[0].Columns.Exists(key))
            {
                var col = e.Layout.Bands[0].Columns[key];
                col.Hidden = false;
                col.Header.Caption = caption;
                col.Width = width;
                if (format != null) col.Format = format;
                col.CellAppearance.TextHAlign = align;
            }
        }

        private void SelectDebitNote()
        {
            if (ultraGrid1.ActiveRow == null) return;

            try
            {
                int voucherId = 0;

                // Priority columns for ID
                string[] idCols = { "VoucherId", "VoucherID", "PH_ID", "ID" };
                foreach (string col in idCols)
                {
                    if (ultraGrid1.ActiveRow.Cells.Exists(col) && ultraGrid1.ActiveRow.Cells[col].Value != DBNull.Value)
                    {
                        voucherId = Convert.ToInt32(ultraGrid1.ActiveRow.Cells[col].Value);
                        if (voucherId > 0) break;
                    }
                }

                if (voucherId == 0)
                {
                    // Fallback search
                    foreach (var cell in ultraGrid1.ActiveRow.Cells)
                    {
                        if (cell.Column.Key.ToLower().Contains("id") && int.TryParse(cell.Value.ToString(), out int val))
                        {
                            if (val > 0) { voucherId = val; break; }
                        }
                    }
                }

                if (voucherId > 0)
                {
                    OnDebitNoteSelected?.Invoke(voucherId);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Could not identify the selected Debit Note ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting debit note: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterData()
        {
            try
            {
                if (fullDataTable == null) return;

                string searchText = textBoxsearch.Text.Trim();
                if (searchText == "Search debit notes...") searchText = "";

                DataView dv = fullDataTable.DefaultView;
                if (!string.IsNullOrEmpty(searchText))
                {
                    string selectedField = comboBox1.SelectedItem?.ToString() ?? "All";
                    string filter = "";
                    string escapedText = searchText.Replace("'", "''");

                    // Map display names to columns
                    var fieldMap = new Dictionary<string, string>
                    {
                        { "Doc No", "PH_DOCNO" },
                        { "Vendor Name", "VENDOR_NAME" },
                        { "Account Code", "PH_ACCTCODE" },
                        { "Amount", "PH_DOCAMT" }
                    };

                    if (selectedField != "All" && fieldMap.ContainsKey(selectedField))
                    {
                        string col = fieldMap[selectedField];
                        if (col == "PH_DOCAMT")
                            filter = $"CONVERT({col}, 'System.String') LIKE '%{escapedText}%'";
                        else
                            filter = $"{col} LIKE '%{escapedText}%'";
                    }
                    else
                    {
                        filter = $"PH_DOCNO LIKE '%{escapedText}%' OR VENDOR_NAME LIKE '%{escapedText}%' OR PH_ACCTCODE LIKE '%{escapedText}%'";
                    }

                    dv.RowFilter = filter;
                }
                else
                {
                    dv.RowFilter = string.Empty;
                }

                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.Selected.Rows.Clear();
                    ultraGrid1.Selected.Rows.Add(ultraGrid1.Rows[0]);
                }
                UpdateRecordCountLabel();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Filter error: {ex.Message}");
            }
        }

        private void InitializeSearchFilterComboBox()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.Add("All");
            comboBox1.Items.Add("Doc No");
            comboBox1.Items.Add("Vendor Name");
            comboBox1.Items.Add("Account Code");
            comboBox1.Items.Add("Amount");
            comboBox1.SelectedIndex = 0;
            comboBox1.SelectedIndexChanged += (s, e) => FilterData();
            textBoxsearch.TextChanged += (s, e) => FilterData();
        }

        private void InitializeColumnOrderComboBox()
        {
            comboBox2.Items.Clear();
            comboBox2.Items.Add("Date");
            comboBox2.Items.Add("Doc No");
            comboBox2.Items.Add("Vendor Name");
            comboBox2.SelectedIndex = 0;
            comboBox2.SelectedIndexChanged += (s, e) =>
            {
                // Sorting logic if needed
            };
        }

        private void ultraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectDebitNote();
                e.Handled = true;
            }
        }

        private void ultraGrid1_DoubleClickRow(object sender, DoubleClickRowEventArgs e)
        {
            SelectDebitNote();
        }

        private void frmDebitNoteList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void textBoxsearch_GotFocus(object sender, EventArgs e)
        {
            if (textBoxsearch.Text == "Search debit notes...")
            {
                textBoxsearch.Text = "";
                textBoxsearch.ForeColor = Color.Black;
            }
        }

        private void textBoxsearch_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxsearch.Text))
            {
                textBoxsearch.Text = "Search debit notes...";
                textBoxsearch.ForeColor = Color.Gray;
            }
        }

        private void ultraGrid1_AfterSelectChange(object sender, AfterSelectChangeEventArgs e)
        {
            if (ultraGrid1.ActiveRow != null)
            {
                var cell = ultraGrid1.ActiveRow.Cells.Exists("PH_DOCNO") ? ultraGrid1.ActiveRow.Cells["PH_DOCNO"] : null;
                if (cell != null && cell.Value != null)
                {
                    UpdateStatus($"Selected: {cell.Value}");
                }
            }
        }

        // --- Status Label ---
        private void InitializeStatusLabel()
        {
            lblStatus = new Label();
            lblStatus.Name = "lblStatus";
            lblStatus.Dock = DockStyle.Bottom;
            lblStatus.Height = 25;
            lblStatus.BackColor = Color.LightYellow;
            lblStatus.BorderStyle = BorderStyle.FixedSingle;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            lblStatus.Text = "Ready";
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

        private void UpdateRecordCountLabel()
        {
            int current = ultraGrid1.Rows.Count;
            int total = (fullDataTable != null) ? fullDataTable.Rows.Count : 0;
            label1.Text = $"Showing {current} of {total} records";
            if (textBox3 != null) textBox3.Text = current.ToString();
        }

        // --- Grid Style ---
        private void SetupUltraGridStyle()
        {
            try
            {
                ultraGrid1.DisplayLayout.Reset();
                var ov = ultraGrid1.DisplayLayout.Override;

                ov.AllowAddNew = AllowAddNew.No;
                ov.AllowDelete = DefaultableBoolean.False;
                ov.AllowUpdate = DefaultableBoolean.False;
                ov.RowSelectors = DefaultableBoolean.True;
                ov.SelectTypeRow = SelectType.Single;
                ov.HeaderClickAction = HeaderClickAction.SortSingle;
                ov.CellClickAction = CellClickAction.RowSelect;
                ov.AllowColMoving = AllowColMoving.WithinBand;
                ov.AllowColSizing = AllowColSizing.Free;

                ultraGrid1.DisplayLayout.GroupByBox.Hidden = true;
                ultraGrid1.DisplayLayout.BorderStyle = UIElementBorderStyle.Solid;

                Color headerBlue = Color.FromArgb(0, 123, 255);
                Color lightBlue = Color.FromArgb(173, 216, 230);

                ov.HeaderAppearance.BackColor = headerBlue;
                ov.HeaderAppearance.ForeColor = Color.White;
                ov.HeaderAppearance.FontData.Bold = DefaultableBoolean.True;
                ov.HeaderAppearance.TextHAlign = HAlign.Center;
                ov.HeaderStyle = HeaderStyle.WindowsXPCommand;

                ov.SelectedRowAppearance.BackColor = Color.FromArgb(0, 120, 215);
                ov.SelectedRowAppearance.ForeColor = Color.White;
                ov.ActiveRowAppearance.BackColor = Color.FromArgb(0, 120, 215);
                ov.ActiveRowAppearance.ForeColor = Color.White;

                ov.RowAppearance.BackColor = Color.White;
                ov.RowAlternateAppearance.BackColor = Color.FromArgb(245, 250, 255);

                ov.CellAppearance.BorderColor = lightBlue;
                ov.RowAppearance.BorderColor = lightBlue;

                ov.MinRowHeight = 30;
                ov.DefaultRowHeight = 30;

                ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.None;
                ultraGrid1.DisplayLayout.ScrollBounds = ScrollBounds.ScrollToFill;
                ultraGrid1.DisplayLayout.ScrollStyle = ScrollStyle.Immediate;
            }
            catch (Exception ex)
            {
                UpdateStatus($"Style error: {ex.Message}");
            }
        }

        // --- Column Chooser ---
        private void SetupColumnChooserMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripMenuItem item = new ToolStripMenuItem("Field/Column Chooser");
            item.Click += (s, e) => ShowColumnChooser();
            menu.Items.Add(item);
            ultraGrid1.ContextMenuStrip = menu;

            ultraGrid1.AllowDrop = true;
            CreateColumnChooserForm();
        }

        private void CreateColumnChooserForm()
        {
            columnChooserForm = new Form
            {
                Text = "Customization",
                Size = new Size(220, 280),
                FormBorderStyle = FormBorderStyle.FixedSingle,
                StartPosition = FormStartPosition.Manual,
                TopMost = true,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowIcon = false,
                ShowInTaskbar = false,
                BackColor = Color.FromArgb(240, 240, 240)
            };
            columnChooserForm.FormClosing += (s, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing) { e.Cancel = true; columnChooserForm.Hide(); }
            };

            columnChooserListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                AllowDrop = true,
                DrawMode = DrawMode.OwnerDrawFixed,
                BorderStyle = BorderStyle.None,
                ItemHeight = 30
            };

            columnChooserForm.Controls.Add(columnChooserListBox);
        }

        private void ShowColumnChooser()
        {
            if (columnChooserForm == null || columnChooserForm.IsDisposed) CreateColumnChooserForm();
            PopulateColumnChooserListBox();
            columnChooserForm.Show(this);
            PositionColumnChooserAtBottomRight();
        }

        private void PopulateColumnChooserListBox()
        {
            if (columnChooserListBox == null) return;
            columnChooserListBox.Items.Clear();
            if (ultraGrid1.DisplayLayout.Bands.Count == 0) return;

            foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
            {
                if (col.Hidden && !col.Key.Contains("Id") && col.Key != "OriginalRowOrder")
                {
                    string caption = string.IsNullOrEmpty(col.Header.Caption) ? col.Key : col.Header.Caption;
                    columnChooserListBox.Items.Add(caption);
                }
            }
        }

        private void PositionColumnChooserAtBottomRight()
        {
            if (columnChooserForm != null && columnChooserForm.Visible)
            {
                columnChooserForm.Location = new Point(this.Right - columnChooserForm.Width - 20, this.Bottom - columnChooserForm.Height - 20);
            }
        }

        // --- Buttons & Hover ---
        private void ConnectNavigationPanelEvents()
        {
            ultraPanel3.Click += (s, e) => MoveRow(-1);
            ultraPictureBox5.Click += (s, e) => MoveRow(-1);

            ultraPanel7.Click += (s, e) => MoveRow(1);
            ultraPictureBox6.Click += (s, e) => MoveRow(1);

            ultraPanel5.Click += (s, e) => SelectDebitNote();
            ultraPictureBox1.Click += (s, e) => SelectDebitNote();
            label5.Click += (s, e) => SelectDebitNote();

            ultraPanel6.Click += (s, e) => this.Close();
            ultraPictureBox2.Click += (s, e) => this.Close();
            label3.Click += (s, e) => this.Close();
        }

        private void MoveRow(int direction)
        {
            if (ultraGrid1.Rows.Count == 0) return;
            int idx = (ultraGrid1.ActiveRow != null) ? ultraGrid1.ActiveRow.Index + direction : 0;
            if (idx >= 0 && idx < ultraGrid1.Rows.Count)
            {
                ultraGrid1.ActiveRow = ultraGrid1.Rows[idx];
                ultraGrid1.Selected.Rows.Clear();
                ultraGrid1.Selected.Rows.Add(ultraGrid1.ActiveRow);
                ultraGrid1.ActiveRowScrollRegion.ScrollRowIntoView(ultraGrid1.ActiveRow);
            }
        }

        private void SetupPanelHoverEffects()
        {
            SetupHover(ultraPanel5, label5, ultraPictureBox1);
            SetupHover(ultraPanel6, label3, ultraPictureBox2);
            SetupHover(ultraPanel3, null, ultraPictureBox5);
            SetupHover(ultraPanel7, null, ultraPictureBox6);
        }

        private void SetupHover(Infragistics.Win.Misc.UltraPanel p, Label l, Infragistics.Win.UltraWinEditors.UltraPictureBox pb)
        {
            Color baseColor = p.Appearance.BackColor;
            Color hoverColor = Color.FromArgb(Math.Min(baseColor.R + 40, 255), Math.Min(baseColor.G + 40, 255), Math.Min(baseColor.B + 40, 255));

            p.MouseEnter += (s, e) => { p.Appearance.BackColor = hoverColor; p.Cursor = Cursors.Hand; };
            p.MouseLeave += (s, e) => { p.Appearance.BackColor = baseColor; p.Cursor = Cursors.Default; };
            if (l != null) { l.MouseEnter += (s, e) => { p.Appearance.BackColor = hoverColor; l.Cursor = Cursors.Hand; }; l.MouseLeave += (s, e) => { p.Appearance.BackColor = baseColor; }; }
            if (pb != null) { pb.MouseEnter += (s, e) => { p.Appearance.BackColor = hoverColor; pb.Cursor = Cursors.Hand; }; pb.MouseLeave += (s, e) => { p.Appearance.BackColor = baseColor; }; }
        }

        private void InitializeSavedColumnWidths()
        {
            savedColumnWidths.Clear();
            foreach (UltraGridColumn col in ultraGrid1.DisplayLayout.Bands[0].Columns)
            {
                if (!col.Hidden) savedColumnWidths[col.Key] = col.Width;
            }
        }

        private void FrmDebitNoteList_SizeChanged(object sender, EventArgs e) => PositionColumnChooserAtBottomRight();
        private void FrmDebitNoteList_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (columnChooserForm != null && !columnChooserForm.IsDisposed) { columnChooserForm.Dispose(); columnChooserForm = null; }
        }
        private void UltraGrid1_Resize(object sender, EventArgs e) { }
    }
}
