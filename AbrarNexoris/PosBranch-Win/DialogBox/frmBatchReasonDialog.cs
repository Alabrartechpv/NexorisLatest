using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;

namespace PosBranch_Win.DialogBox
{
    public class BatchReasonItemInfo
    {
        public int RowIndex { get; set; }
        public bool IsSelected { get; set; } = true;
        public string SlNo { get; set; }
        public string Description { get; set; }
        public string Barcode { get; set; }
        public string CurrentReason { get; set; }
    }

    public class frmBatchReasonDialog : Form
    {
        public string SelectedReason { get; private set; } = "";
        public List<int> SelectedRowIndices { get; private set; } = new List<int>();

        private ComboBox cmbReason;
        private UltraGrid ultraGridItems;
        private CheckBox chkSelectAll;
        private Button btnApply;
        private Button btnCancel;

        public frmBatchReasonDialog(List<BatchReasonItemInfo> items, List<string> availableReasons)
        {
            InitializeComponentUI(items, availableReasons);
        }

        private void InitializeComponentUI(List<BatchReasonItemInfo> items, List<string> availableReasons)
        {
            this.Text = "Batch Reason Selection";
            this.Size = new Size(600, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(232, 246, 255); // Matching frmPurchaseReturn
            this.Font = new Font("Microsoft Sans Serif", 8.25F);

            // Top Label
            Label lblReason = new Label
            {
                Text = "Select or type Batch Reason:",
                Location = new Point(18, 16),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(10, 31, 79)
            };
            this.Controls.Add(lblReason);

            // Reason ComboBox (Editable DropDown)
            cmbReason = new ComboBox
            {
                Location = new Point(20, 42),
                Size = new Size(545, 26),
                DropDownStyle = ComboBoxStyle.DropDown,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(10, 31, 79)
            };

            if (availableReasons != null)
            {
                foreach (var r in availableReasons)
                {
                    cmbReason.Items.Add(r);
                }
            }
            if (cmbReason.Items.Count > 0)
            {
                cmbReason.SelectedIndex = 0;
            }
            this.Controls.Add(cmbReason);

            // Items List Header Label & Select All Checkbox
            Label lblItems = new Label
            {
                Text = "Select items to apply batch reason to:",
                Location = new Point(18, 83),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(10, 31, 79)
            };
            this.Controls.Add(lblItems);

            chkSelectAll = new CheckBox
            {
                Text = "Select All",
                Location = new Point(475, 81),
                AutoSize = true,
                Checked = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(10, 31, 79)
            };
            chkSelectAll.CheckedChanged += (s, e) =>
            {
                bool isChecked = chkSelectAll.Checked;
                if (ultraGridItems != null && ultraGridItems.Rows != null)
                {
                    foreach (UltraGridRow row in ultraGridItems.Rows)
                    {
                        if (row.Cells.Exists("SELECT"))
                        {
                            row.Cells["SELECT"].Value = isChecked;
                        }
                    }
                }
            };
            this.Controls.Add(chkSelectAll);

            // Create DataTable for UltraGrid DataSource
            DataTable dt = new DataTable();
            dt.Columns.Add("SELECT", typeof(bool));
            dt.Columns.Add("RowIdx", typeof(int));
            dt.Columns.Add("SlNo", typeof(string));
            dt.Columns.Add("Item Name", typeof(string));
            dt.Columns.Add("Barcode", typeof(string));
            dt.Columns.Add("Current Reason", typeof(string));

            if (items != null)
            {
                foreach (var item in items)
                {
                    dt.Rows.Add(item.IsSelected, item.RowIndex, item.SlNo, item.Description, item.Barcode, item.CurrentReason);
                }
            }

            // Create UltraGrid instance
            ultraGridItems = new UltraGrid
            {
                Location = new Point(20, 110),
                Size = new Size(545, 305),
                DataSource = dt
            };

            // Apply the EXACT same UltraGrid theme as frmPurchaseReturn.cs!
            ApplyUnifiedGridTheme(ultraGridItems);

            // Additional Column Layout customizations
            if (ultraGridItems.DisplayLayout.Bands.Count > 0)
            {
                UltraGridBand band = ultraGridItems.DisplayLayout.Bands[0];
                if (band.Columns.Exists("RowIdx")) band.Columns["RowIdx"].Hidden = true;

                if (band.Columns.Exists("SELECT"))
                {
                    UltraGridColumn colSel = band.Columns["SELECT"];
                    colSel.Width = 45;
                    colSel.Style = Infragistics.Win.UltraWinGrid.ColumnStyle.CheckBox;
                    colSel.Header.Caption = "";
                    colSel.Header.CheckBoxVisibility = HeaderCheckBoxVisibility.Always;
                    colSel.Header.CheckBoxAlignment = HeaderCheckBoxAlignment.Center;
                    colSel.CellAppearance.TextHAlign = HAlign.Center;
                }
                if (band.Columns.Exists("SlNo"))
                {
                    UltraGridColumn colSl = band.Columns["SlNo"];
                    colSl.Width = 55;
                    colSl.Header.Caption = "SlNo";
                    colSl.CellActivation = Activation.NoEdit;
                    colSl.CellAppearance.TextHAlign = HAlign.Center;
                }
                if (band.Columns.Exists("Item Name"))
                {
                    UltraGridColumn colDesc = band.Columns["Item Name"];
                    colDesc.Width = 220;
                    colDesc.Header.Caption = "Item Name";
                    colDesc.CellActivation = Activation.NoEdit;
                }
                if (band.Columns.Exists("Barcode"))
                {
                    UltraGridColumn colBC = band.Columns["Barcode"];
                    colBC.Width = 95;
                    colBC.Header.Caption = "Barcode";
                    colBC.CellActivation = Activation.NoEdit;
                    colBC.CellAppearance.TextHAlign = HAlign.Center;
                }
                if (band.Columns.Exists("Current Reason"))
                {
                    UltraGridColumn colReas = band.Columns["Current Reason"];
                    colReas.Width = 115;
                    colReas.Header.Caption = "Current Reason";
                    colReas.CellActivation = Activation.NoEdit;
                }
            }

            this.Controls.Add(ultraGridItems);

            // Bottom Buttons (Matching frmPurchaseReturn look)
            btnApply = new Button
            {
                Text = "Apply Reason",
                Location = new Point(325, 430),
                Size = new Size(120, 34),
                BackColor = Color.FromArgb(67, 118, 184),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                DialogResult = DialogResult.OK,
                Cursor = Cursors.Hand
            };
            btnApply.FlatAppearance.BorderSize = 0;
            btnApply.Click += BtnApply_Click;
            this.Controls.Add(btnApply);

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(455, 430),
                Size = new Size(110, 34),
                BackColor = Color.FromArgb(220, 230, 245),
                ForeColor = Color.FromArgb(10, 31, 79),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                DialogResult = DialogResult.Cancel,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(170, 195, 225);
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnApply;
            this.CancelButton = btnCancel;
        }

        private void ApplyUnifiedGridTheme(UltraGrid grid)
        {
            if (grid == null) return;

            grid.UseAppStyling = false;
            grid.UseOsThemes = DefaultableBoolean.False;

            UltraGridLayout layout = grid.DisplayLayout;

            // Background matching Image 3 empty grid space
            layout.Appearance.BackColor = Color.FromArgb(232, 246, 255);
            layout.Appearance.BackColor2 = Color.FromArgb(232, 246, 255);
            layout.Appearance.BackGradientStyle = GradientStyle.None;
            layout.Appearance.BorderColor = Color.FromArgb(197, 217, 241);
            layout.BorderStyle = UIElementBorderStyle.Solid;

            // Header style matching Image 3 (flat blue header theme)
            layout.Override.HeaderStyle = HeaderStyle.Standard;
            layout.Override.HeaderAppearance.BackColor = Color.FromArgb(93, 151, 214);
            layout.Override.HeaderAppearance.BackColor2 = Color.FromArgb(67, 118, 184);
            layout.Override.HeaderAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.HeaderAppearance.ForeColor = Color.White;
            layout.Override.HeaderAppearance.BorderColor = Color.FromArgb(118, 154, 198);
            layout.Override.HeaderAppearance.TextHAlign = HAlign.Center;
            layout.Override.HeaderAppearance.TextVAlign = VAlign.Middle;
            layout.Override.HeaderAppearance.FontData.Bold = DefaultableBoolean.False;
            layout.Override.HeaderAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.HeaderAppearance.FontData.SizeInPoints = 8.25F;
            layout.Override.HeaderAppearance.ThemedElementAlpha = Alpha.Transparent;

            // Row selector styling matching headers
            layout.Override.RowSelectors = DefaultableBoolean.True;
            layout.Override.RowSelectorWidth = 20;
            layout.Override.RowSelectorNumberStyle = RowSelectorNumberStyle.RowIndex;
            layout.Override.RowSelectorAppearance.BackColor = Color.FromArgb(67, 118, 184);
            layout.Override.RowSelectorAppearance.BackColor2 = Color.FromArgb(93, 151, 214);
            layout.Override.RowSelectorAppearance.BackGradientStyle = GradientStyle.Vertical;
            layout.Override.RowSelectorAppearance.BorderColor = Color.FromArgb(118, 154, 198);
            layout.Override.RowSelectorAppearance.ForeColor = Color.White;
            layout.Override.RowSelectorAppearance.FontData.Bold = DefaultableBoolean.True;
            layout.Override.RowSelectorAppearance.TextHAlign = HAlign.Center;

            // Row & Cell appearance matching Image 3
            layout.Override.RowAppearance.BackColor = Color.White;
            layout.Override.RowAppearance.ForeColor = Color.FromArgb(10, 31, 79);
            layout.Override.RowAppearance.BorderColor = Color.FromArgb(197, 217, 241);
            layout.Override.RowAlternateAppearance.BackColor = Color.FromArgb(245, 250, 255);
            layout.Override.RowAlternateAppearance.BorderColor = Color.FromArgb(197, 217, 241);

            // Selected / Active Row
            layout.Override.SelectedRowAppearance.BackColor = Color.FromArgb(173, 216, 255);
            layout.Override.SelectedRowAppearance.ForeColor = Color.FromArgb(10, 31, 79);
            layout.Override.SelectedRowAppearance.FontData.Bold = DefaultableBoolean.False;
            layout.Override.ActiveRowAppearance.BackColor = Color.FromArgb(173, 216, 255);
            layout.Override.ActiveRowAppearance.ForeColor = Color.FromArgb(10, 31, 79);
            layout.Override.ActiveRowAppearance.FontData.Bold = DefaultableBoolean.False;

            // Borders
            layout.Override.BorderStyleHeader = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleCell = UIElementBorderStyle.Solid;
            layout.Override.BorderStyleRow = UIElementBorderStyle.Solid;
            layout.Override.CellAppearance.BorderColor = Color.FromArgb(197, 217, 241);
            layout.Override.CellAppearance.ForeColor = Color.FromArgb(10, 31, 79);
            layout.Override.CellAppearance.FontData.Name = "Microsoft Sans Serif";
            layout.Override.CellAppearance.FontData.SizeInPoints = 8.25F;
            layout.Override.CellAppearance.TextVAlign = VAlign.Middle;

            // Compact spacing
            layout.Override.RowSizing = RowSizing.AutoFree;
            layout.Override.DefaultRowHeight = 22;
            layout.Override.RowSpacingBefore = 0;
            layout.Override.RowSpacingAfter = 0;
            layout.Override.CellPadding = 2;
            layout.Override.CellSpacing = 0;
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            string reason = cmbReason.Text.Trim();
            if (string.IsNullOrWhiteSpace(reason) || reason == "Select Reason")
            {
                MessageBox.Show("Please select or type a valid reason.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            SelectedReason = reason;
            SelectedRowIndices.Clear();

            if (ultraGridItems != null && ultraGridItems.Rows != null)
            {
                foreach (UltraGridRow row in ultraGridItems.Rows)
                {
                    bool isChecked = row.Cells.Exists("SELECT") && row.Cells["SELECT"].Value != null && Convert.ToBoolean(row.Cells["SELECT"].Value);
                    if (isChecked)
                    {
                        if (row.Cells.Exists("RowIdx") && row.Cells["RowIdx"].Value != null)
                        {
                            int rowIdx = Convert.ToInt32(row.Cells["RowIdx"].Value);
                            SelectedRowIndices.Add(rowIdx);
                        }
                    }
                }
            }

            if (SelectedRowIndices.Count == 0)
            {
                MessageBox.Show("Please select at least one item to apply the batch reason.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            this.DialogResult = DialogResult.OK;
        }
    }
}
