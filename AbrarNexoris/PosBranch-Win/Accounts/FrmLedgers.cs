using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Diagnostics;
using Repository;
using Repository.Accounts;
using ModelClass;
using ModelClass.Accounts;
using ModelClass.Master;

namespace PosBranch_Win.Accounts
{
    public partial class FrmLedgers : Form
    {
        // Repository instances for database operations
        private Dropdowns drop = new Dropdowns();
        private AccountGroupRepository accountGroupRepo;
        private LedgerRepository ledgerRepo;
        private int? preselectedGroupId = null;

        public void SetPreselectedGroupId(int groupId)
        {
            preselectedGroupId = groupId;
        }
        public FrmLedgers()
        {
            try
            {
                InitializeComponent();
                tblMain.Paint += TblMain_Paint;

                // Initialize repositories
                accountGroupRepo = new AccountGroupRepository();
                ledgerRepo = new LedgerRepository();

                // Register event handlers
                this.Load += FrmLedgers_Load;
                btnSearchLedger.Click += BtnSearch_Click;

                // Set up numeric validation for amount fields
                ultratxtOpnBalance.KeyPress += NumericTextBox_KeyPress;
                ultratxtBalance.KeyPress += NumericTextBox_KeyPress;

                // Auto-calculate Balance when Debit or Credit changes
                ultratxtOpnBalance.TextChanged += UpdateBalance;
                ultraComboDrCr.ValueChanged += UpdateBalance;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing form: " + ex.Message, "Initialization Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FrmLedgers_Load(object sender, EventArgs e)
        {
            try
            {
                Debug.WriteLine("Form load started");

                // Initialize the repositories if not already done
                if (accountGroupRepo == null)
                    accountGroupRepo = new AccountGroupRepository();

                if (ledgerRepo == null)
                    ledgerRepo = new LedgerRepository();

                // Load dropdowns
                LoadBranches();
                LoadAccountGroups();

                // Set up UI elements FIRST
                SetupUI();

                // Generate next ledger ID
                GenerateNextLedgerID();

                if (preselectedGroupId.HasValue)
                {
                    ultraDrpParentGroup.Value = preselectedGroupId.Value;
                }

                Debug.WriteLine("Form load completed");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing form: " + ex.Message +
                    "\nStack trace: " + ex.StackTrace, "Critical Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Data Loading Methods

        private void LoadBranches()
        {
            try
            {
                // Load branches using the Dropdowns repository
                BranchDDlGrid branchDDl = drop.getBanchDDl();

                if (branchDDl != null && branchDDl.List != null)
                {
                    ultraDropDownBranch.DataSource = branchDDl.List;
                    ultraDropDownBranch.DisplayMember = "BranchName";
                    ultraDropDownBranch.ValueMember = "Id";
                    ultraDropDownBranch.Visible = true;

                    // Pre-select the logged-in branch from session
                    ultraDropDownBranch.Value = SessionContext.BranchId;

                    // Fallback: select first item if session branch not found
                    if (ultraDropDownBranch.SelectedIndex == -1 && ultraDropDownBranch.Items.Count > 0)
                        ultraDropDownBranch.SelectedIndex = 0;
                }
                else
                {
                    // Fallback to a default branch if no data is returned
                    DataTable dtBranches = new DataTable();
                    dtBranches.Columns.Add("Id", typeof(int));
                    dtBranches.Columns.Add("BranchName", typeof(string));
                    dtBranches.Rows.Add(1, "Main Branch");

                    ultraDropDownBranch.DataSource = dtBranches;
                    ultraDropDownBranch.DisplayMember = "BranchName";
                    ultraDropDownBranch.ValueMember = "Id";
                    ultraDropDownBranch.Visible = true;
                    ultraDropDownBranch.SelectedIndex = 0;

                    MessageBox.Show("Could not load branches. Using default branch.",
                                  "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading branches: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Create a fallback branch in case of error
                DataTable dtBranches = new DataTable();
                dtBranches.Columns.Add("Id", typeof(int));
                dtBranches.Columns.Add("BranchName", typeof(string));
                dtBranches.Rows.Add(1, "Main Branch");

                ultraDropDownBranch.DataSource = dtBranches;
                ultraDropDownBranch.DisplayMember = "BranchName";
                ultraDropDownBranch.ValueMember = "Id";
                ultraDropDownBranch.Visible = true;
                ultraDropDownBranch.SelectedIndex = 0;
            }
        }

        private void LoadAccountGroups()
        {
            try
            {
                // Try to load account groups from repository
                AccountGroupHeadDDL accountGroups = accountGroupRepo.GetAllParentGroups();

                if (accountGroups != null && accountGroups.List != null && accountGroups.List.Any())
                {
                    // Create a datatable from the list
                    DataTable dtAccountGroups = new DataTable();
                    dtAccountGroups.Columns.Add("GroupID", typeof(int));
                    dtAccountGroups.Columns.Add("GroupName", typeof(string));

                    foreach (var group in accountGroups.List)
                    {
                        dtAccountGroups.Rows.Add(group.GroupID, group.GroupName);
                    }

                    ultraDrpParentGroup.DataSource = dtAccountGroups;
                    ultraDrpParentGroup.DisplayMember = "GroupName";
                    ultraDrpParentGroup.ValueMember = "GroupID";
                    ultraDrpParentGroup.Visible = true;

                    // Select the first group by default if available
                    if (ultraDrpParentGroup.Items.Count > 0)
                        ultraDrpParentGroup.SelectedIndex = 0;
                }
                else
                {
                    // Create an empty datatable with the expected structure
                    DataTable dtEmptyGroups = new DataTable();
                    dtEmptyGroups.Columns.Add("GroupID", typeof(int));
                    dtEmptyGroups.Columns.Add("GroupName", typeof(string));

                    ultraDrpParentGroup.DataSource = dtEmptyGroups;
                    ultraDrpParentGroup.DisplayMember = "GroupName";
                    ultraDrpParentGroup.ValueMember = "GroupID";
                    ultraDrpParentGroup.Visible = true;

                    MessageBox.Show("No account groups found. Please create some account groups first.",
                                  "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading account groups: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Create a default empty data table
                DataTable dtAccountGroups = new DataTable();
                dtAccountGroups.Columns.Add("GroupID", typeof(int));
                dtAccountGroups.Columns.Add("GroupName", typeof(string));

                ultraDrpParentGroup.DataSource = dtAccountGroups;
                ultraDrpParentGroup.DisplayMember = "GroupName";
                ultraDrpParentGroup.ValueMember = "GroupID";
                ultraDrpParentGroup.Visible = true;
            }
        }

        #endregion

        #region Event Handlers

        public void Save()
        {
            UltraBtnSave_Click(this, EventArgs.Empty);
        }

        public void Clear()
        {
            UltraBtnClear_Click(this, EventArgs.Empty);
        }

        public void Delete()
        {
            UltraBtnDelete_Click(this, EventArgs.Empty);
        }

        private void UltraBtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate input
                if (!ValidateInput())
                    return;

                // Get form data
                int ledgerId = Convert.ToInt32(ultraTxtLedgerId.Text.Trim());
                string ledgerName = ultraTxtLedgerName.Text.Trim();
                string aliasName = ultratxtAliasName.Text.Trim();
                string description = ultratxtDescription.Text.Trim();
                string notes = ultratxtNotes.Text.Trim();

                // Get selected values from dropdowns
                int branchId = 0;
                if (ultraDropDownBranch.Value != null)
                    branchId = Convert.ToInt32(ultraDropDownBranch.Value);
                if (branchId == 0)
                    branchId = SessionContext.BranchId;

                int groupId = 0;
                if (ultraDrpParentGroup.Value != null)
                    groupId = Convert.ToInt32(ultraDrpParentGroup.Value);

                // Parse decimal values
                decimal openDebit = 0;
                decimal openCredit = 0;
                decimal openBalance = 0;

                if (!string.IsNullOrEmpty(ultratxtOpnBalance.Text))
                    decimal.TryParse(ultratxtOpnBalance.Text, out openBalance);

                if (ultraComboDrCr.Value != null && ultraComboDrCr.Value.ToString() == "Dr")
                {
                    openDebit = openBalance;
                }
                else if (ultraComboDrCr.Value != null && ultraComboDrCr.Value.ToString() == "Cr")
                {
                    openCredit = openBalance;
                }

                // Create ledger object
                ModelClass.Accounts.Ledger ledger = new ModelClass.Accounts.Ledger
                {
                    LedgerID = ledgerId,
                    LedgerName = ledgerName,
                    Alias = aliasName,
                    Description = description,
                    Notes = notes,
                    GroupID = groupId,
                    BranchID = branchId,
                    OpnDebit = openDebit,
                    OpnCredit = openCredit,
                    // Default values for other properties
                    ProvideBankDetails = false,
                    GstApplicable = false,
                    VatApplicable = false,
                    InventoryValuesAffected = false,
                    MaintainBillWiseDetails = false,
                    PriceLevelApplicable = false,
                    ActivateInterestCalculations = false,
                    CompanyID = SessionContext.CompanyId
                };

                // Check if we're updating or creating
                if (ultraTxtLedgerId.Tag != null)
                {
                    // Confirm update
                    DialogResult result = MessageBox.Show("Are you sure you want to update this ledger?", "Confirm Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.No)
                        return;

                    // Update existing ledger
                    bool success = ledgerRepo.UpdateLedger(ledger);

                    if (success)
                        MessageBox.Show("Ledger updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                        MessageBox.Show("Failed to update ledger.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    // Confirm save
                    DialogResult result = MessageBox.Show("Are you sure you want to save this new ledger?", "Confirm Save", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.No)
                        return;

                    // Create new ledger
                    bool success = ledgerRepo.CreateLedger(ledger);

                    if (success)
                        MessageBox.Show("Ledger created successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                        MessageBox.Show("Failed to create ledger.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // Clear the form
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving ledger: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UltraBtnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void UltraBtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if an item is selected for deletion
                if (ultraTxtLedgerId.Tag == null)
                {
                    MessageBox.Show("Please select a ledger to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int ledgerId = Convert.ToInt32(ultraTxtLedgerId.Text);

                // Confirm deletion
                DialogResult result = MessageBox.Show("Are you sure you want to delete this ledger?",
                                                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                    return;

                // Delete the ledger
                bool success = ledgerRepo.DeleteLedger(ledgerId);

                if (success)
                {
                    MessageBox.Show("Ledger deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Clear the form
                    ClearForm();
                }
                else
                {
                    MessageBox.Show("Failed to delete ledger. It may be in use by other records.",
                                  "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting ledger: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void NumericTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only digits, decimal point, and control characters
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // Allow only one decimal point
            if (e.KeyChar == '.' && (sender as Infragistics.Win.UltraWinEditors.UltraTextEditor).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }

        #endregion

        #region Helper Methods

        private void SetupUI()
        {
            try
            {
                ApplyModernTheme();

                // Balance is always read-only (auto-calculated)
                ultratxtBalance.ReadOnly = true;
                ultratxtBalance.TabStop = false;

                // Force ALL CAPS for text entry consistency
                ultraTxtLedgerName.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
                ultratxtAliasName.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
                ultratxtDescription.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
                ultratxtNotes.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;

                // Hover effects for the Search button
                btnSearchLedger.ButtonStyle = Infragistics.Win.UIElementButtonStyle.Flat;
                btnSearchLedger.HotTrackAppearance.BackColor = Color.FromArgb(206, 231, 247);
                btnSearchLedger.HotTrackAppearance.ForeColor = Color.FromArgb(24, 92, 143);
                btnSearchLedger.PressedAppearance.BackColor = Color.FromArgb(185, 218, 239);
                btnSearchLedger.PressedAppearance.ForeColor = Color.FromArgb(24, 92, 143);

                ultraComboDrCr.Items.Clear();
                ultraComboDrCr.Items.Add("Dr", "Debit (Dr)");
                ultraComboDrCr.Items.Add("Cr", "Credit (Cr)");
                ultraComboDrCr.SelectedIndex = 0;
                StyleCombo(ultraComboDrCr);

                UpdateFormModeCaption(false);
                ultraTxtLedgerName.Focus();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in SetupUI: " + ex.Message);
            }
        }

        private void ApplyModernTheme()
        {
            BackColor = Color.FromArgb(222, 240, 250);
            pnlMain.Appearance.BackColor = Color.FromArgb(222, 240, 250);
            tblMain.BackColor = Color.FromArgb(238, 248, 253);

            var sectionBackColor = Color.FromArgb(199, 225, 242);
            var sectionForeColor = Color.FromArgb(15, 77, 128);

            StyleCombo(ultraDrpParentGroup);

            StyleEditor(ultraTxtLedgerName);
            StyleEditor(ultratxtAliasName);
            StyleEditor(ultratxtDescription);
            StyleEditor(ultratxtOpnBalance);
            StyleEditor(ultratxtNotes);
            StyleEditor(ultraTxtLedgerId, true);
            StyleEditor(ultratxtBalance, true);

            StyleSectionLabel(lblSectionLedger, sectionBackColor, sectionForeColor);
            StyleSectionLabel(lblSectionFinancial, sectionBackColor, sectionForeColor);
            StyleSectionLabel(lblSectionNotes, sectionBackColor, sectionForeColor);

            StyleFieldLabel(ultraLblLedgerId);
            StyleFieldLabel(ultralblLedgerName);
            StyleFieldLabel(ultraLblAccGroup);
            StyleFieldLabel(ultralblAlias);
            StyleFieldLabel(ultraLblDescription);
            StyleFieldLabel(ultraLblOpnBalance);
            StyleFieldLabel(ultraLblBalace);
        }

        private void TblMain_Paint(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(Color.FromArgb(170, 208, 232)))
            {
                var rect = tblMain.ClientRectangle;
                rect.Width -= 1;
                rect.Height -= 1;
                e.Graphics.DrawRectangle(pen, rect);
            }
        }

        private void StyleSectionLabel(Infragistics.Win.Misc.UltraLabel label, Color backColor, Color foreColor)
        {
            label.Appearance = new Infragistics.Win.Appearance();
            label.Appearance.BackColor = backColor;
            label.Appearance.ForeColor = foreColor;
            label.Appearance.TextVAlignAsString = "Middle";
            label.Padding = new Size(12, 0);
        }

        private void StyleFieldLabel(Infragistics.Win.Misc.UltraLabel label)
        {
            label.Appearance = new Infragistics.Win.Appearance();
            label.Appearance.BackColor = tblMain.BackColor;
            label.Appearance.ForeColor = Color.FromArgb(40, 62, 89);
            label.Appearance.TextVAlignAsString = "Middle";
        }

        private void StyleCombo(Infragistics.Win.UltraWinEditors.UltraComboEditor combo)
        {
            combo.BackColor = Color.White;
            combo.ForeColor = Color.FromArgb(40, 62, 89);
            combo.Appearance.BackColor = Color.White;
            combo.Appearance.ForeColor = combo.ForeColor;
            combo.Appearance.BorderColor = Color.FromArgb(110, 170, 210);
            combo.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            combo.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2010;
            combo.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
        }

        private void StyleEditor(Infragistics.Win.UltraWinEditors.UltraTextEditor editor, bool readOnly = false)
        {
            Color editorBackColor = readOnly
                ? Color.FromArgb(239, 245, 250)
                : Color.White;

            editor.BackColor = editorBackColor;
            editor.ForeColor = Color.FromArgb(40, 62, 89);
            editor.Appearance.BackColor = editorBackColor;
            editor.Appearance.ForeColor = editor.ForeColor;
            editor.Appearance.BorderColor = Color.FromArgb(110, 170, 210);
            editor.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            editor.DisplayStyle = Infragistics.Win.EmbeddableElementDisplayStyle.Office2010;
            editor.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
        }



        private void BtnSearch_Click(object sender, EventArgs e)
        {
            using (var searchForm = new PosBranch_Win.DialogBox.FrmLedgerSearch())
            {
                if (searchForm.ShowDialog() == DialogResult.OK && searchForm.SelectedLedgerId > 0)
                {
                    LoadLedgerById(searchForm.SelectedLedgerId);
                }
            }
        }

        private bool ValidateInput()
        {
            // Check branch selection
            if (false) // Branch validation removed
            {
                MessageBox.Show("Please select a branch.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ultraDropDownBranch.Focus();
                return false;
            }

            // LedgerID is auto-generated — no manual entry check needed.
            // If it is somehow empty (e.g. GetNextLedgerID failed), block save with a clear message.
            if (string.IsNullOrWhiteSpace(ultraTxtLedgerId.Text))
            {
                MessageBox.Show("Ledger ID could not be generated. Please refresh the form or contact support.",
                    "ID Generation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Check ledger name
            if (string.IsNullOrWhiteSpace(ultraTxtLedgerName.Text))
            {
                MessageBox.Show("Please enter a ledger name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ultraTxtLedgerName.Focus();
                return false;
            }

            // Check account group
            if (ultraDrpParentGroup.SelectedIndex == -1 || 
                ultraDrpParentGroup.Value == null || 
                Convert.ToInt32(ultraDrpParentGroup.Value) <= 0)
            {
                MessageBox.Show("Please select a valid Account Group.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ultraDrpParentGroup.Focus();
                return false;
            }

            // Check duplicate ledger name
            string ledgerName = ultraTxtLedgerName.Text.Trim();
            int branchId = SessionContext.BranchId;
            int excludeId = ultraTxtLedgerId.Tag != null ? Convert.ToInt32(ultraTxtLedgerId.Tag) : 0;

            if (ledgerRepo.IsLedgerNameExists(ledgerName, branchId, excludeId))
            {
                MessageBox.Show($"A Ledger with the name '{ledgerName}' already exists. Please choose a different name.", "Duplicate Name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ultraTxtLedgerName.Focus();
                return false;
            }

            // Check duplicate alias name
            string aliasName = ultratxtAliasName.Text.Trim();
            if (!string.IsNullOrWhiteSpace(aliasName) && ledgerRepo.IsLedgerAliasExists(aliasName, branchId, excludeId))
            {
                MessageBox.Show($"A Ledger with the alias '{aliasName}' already exists. Please choose a different alias.", "Duplicate Alias", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ultratxtAliasName.Focus();
                return false;
            }

            return true;
        }

        private void ClearForm()
        {
            // Clear all textboxes
            ultraTxtLedgerName.Text = string.Empty;
            ultratxtAliasName.Text = string.Empty;
            ultratxtDescription.Text = string.Empty;
            ultratxtNotes.Text = string.Empty;
            ultratxtOpnBalance.Text = "0.00";
            if (ultraComboDrCr.Items.Count > 0) ultraComboDrCr.SelectedIndex = 0;
            ultratxtBalance.Text = "0.00";

            // Reset dropdowns to first item if available
            if (ultraDropDownBranch.Items.Count > 0)
                ultraDropDownBranch.SelectedIndex = 0;

            if (ultraDrpParentGroup.Items.Count > 0)
                ultraDrpParentGroup.SelectedIndex = 0;

            // Clear the tag and reset the form mode
            ultraTxtLedgerId.Tag = null;
            UpdateFormModeCaption(false);

            // GenerateNextLedgerID will set ReadOnly = true after populating the ID
            GenerateNextLedgerID();

            // Set focus to the ledger name field
            ultraTxtLedgerName.Focus();
        }

        private void GenerateNextLedgerID()
        {
            try
            {
                // Get the next available ID from the repository
                int nextId = ledgerRepo.GetNextLedgerID();

                if (nextId > 0)
                {
                    ultraTxtLedgerId.Text = nextId.ToString();
                    ultraTxtLedgerId.ReadOnly = true;
                }
                else
                {
                    // Fallback: leave field empty and editable so the user is aware
                    ultraTxtLedgerId.Text = string.Empty;
                    ultraTxtLedgerId.ReadOnly = false;
                    MessageBox.Show("Could not auto-generate a Ledger ID. Please check the database connection.",
                        "ID Generation Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                // On failure: clear and unlock so the problem is visible
                ultraTxtLedgerId.Text = string.Empty;
                ultraTxtLedgerId.ReadOnly = false;
                MessageBox.Show("Error generating next ledger ID: " + ex.Message +
                    "\nYou may enter the ID manually or refresh the form.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Auto-calculates Balance = OpnDebit - OpnCredit whenever either field changes.
        /// </summary>
        private void UpdateBalance(object sender, EventArgs e)
        {
            try
            {
                decimal balance = 0;
                decimal.TryParse(ultratxtOpnBalance.Text, out balance);
                
                if (ultraComboDrCr.Value != null && ultraComboDrCr.Value.ToString() == "Cr")
                {
                    ultratxtBalance.Text = (-balance).ToString("N2");
                }
                else
                {
                    ultratxtBalance.Text = balance.ToString("N2");
                }
            }
            catch
            {
                // Ignore parse errors during typing
            }
        }

        /// <summary>
        /// Sets the selected value on an UltraComboEditor by its ValueMember.
        /// </summary>
        private void SelectUltraDropDownByValue(Infragistics.Win.UltraWinEditors.UltraComboEditor control, object value)
        {
            try
            {
                if (control == null || value == null)
                    return;
                control.Value = value;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in SelectUltraDropDownByValue: " + ex.Message);
            }
        }

        /// <summary>
        /// Loads ledger data by ID using repository
        /// </summary>
        /// <param name="ledgerId">The ID of the ledger to load</param>
        public void LoadLedgerById(int ledgerId)
        {
            try
            {
                // Get the ledger data using repository's direct method
                DataRow ledgerRow = ledgerRepo.GetLedgerById(ledgerId);

                if (ledgerRow != null)
                {
                    // Populate form controls
                    ultraTxtLedgerId.Text = ledgerId.ToString();
                    ultraTxtLedgerName.Text = ledgerRow["LedgerName"].ToString();

                    if (ledgerRow["Alias"] != DBNull.Value)
                        ultratxtAliasName.Text = ledgerRow["Alias"].ToString();

                    if (ledgerRow["Description"] != DBNull.Value)
                        ultratxtDescription.Text = ledgerRow["Description"].ToString();

                    if (ledgerRow["Notes"] != DBNull.Value)
                        ultratxtNotes.Text = ledgerRow["Notes"].ToString();

                    decimal opnDebit = 0;
                    decimal opnCredit = 0;

                    if (ledgerRow["OpnDebit"] != DBNull.Value)
                        opnDebit = Convert.ToDecimal(ledgerRow["OpnDebit"]);

                    if (ledgerRow["OpnCredit"] != DBNull.Value)
                        opnCredit = Convert.ToDecimal(ledgerRow["OpnCredit"]);

                    if (opnDebit > 0)
                    {
                        ultratxtOpnBalance.Text = opnDebit.ToString("N2");
                        SelectUltraDropDownByValue(ultraComboDrCr, "Dr");
                    }
                    else if (opnCredit > 0)
                    {
                        ultratxtOpnBalance.Text = opnCredit.ToString("N2");
                        SelectUltraDropDownByValue(ultraComboDrCr, "Cr");
                    }
                    else
                    {
                        ultratxtOpnBalance.Text = "0.00";
                        if (ultraComboDrCr.Items.Count > 0) SelectUltraDropDownByValue(ultraComboDrCr, "Dr");
                    }

                    if (ledgerRow["Balance"] != DBNull.Value)
                        ultratxtBalance.Text = Convert.ToDecimal(ledgerRow["Balance"]).ToString("N2");

                    // Set group dropdown if needed
                    if (ledgerRow["GroupID"] != DBNull.Value)
                    {
                        int groupId = Convert.ToInt32(ledgerRow["GroupID"]);
                        SelectUltraDropDownByValue(ultraDrpParentGroup, groupId);
                    }

                    // Set branch dropdown if needed
                    if (ledgerRow["BranchID"] != DBNull.Value)
                    {
                        int branchId = Convert.ToInt32(ledgerRow["BranchID"]);
                        SelectUltraDropDownByValue(ultraDropDownBranch, branchId);
                    }

                    // Make the ledger ID read-only
                    ultraTxtLedgerId.ReadOnly = true;

                    // Store the ledger ID in the tag property for update
                    ultraTxtLedgerId.Tag = ledgerId;

                    UpdateFormModeCaption(true);
                }
                else
                {
                    MessageBox.Show($"Ledger with ID {ledgerId} not found.", "Record Not Found",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading ledger: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void UpdateFormModeCaption(bool isEditMode)
        {
            Text = isEditMode ? "Ledgers - Edit" : "Ledgers";
        }

        #endregion
    }
}
