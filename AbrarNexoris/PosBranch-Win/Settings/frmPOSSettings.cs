using ModelClass;
using ModelClass.Settings;
using Repository.SettingsRepo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win.Settings
{
    public partial class frmPOSSettings : Form
    {
        private POSSettingsRepository _settingsRepository;
        private bool _hasUnsavedChanges = false;

        public frmPOSSettings()
        {
            InitializeComponent();
            _settingsRepository = new POSSettingsRepository();
        }

        #region Form Events

        private void frmPOSSettings_Load(object sender, EventArgs e)
        {
            try
            {
                InitializeControls();
                LoadCurrentSettings();
                _hasUnsavedChanges = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void frmPOSSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_hasUnsavedChanges)
            {
                var result = MessageBox.Show("You have unsaved changes. Do you want to save before closing?",
                    "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    SaveSettings();
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void frmPOSSettings_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
            else if (e.Control && e.KeyCode == Keys.S)
            {
                SaveSettings();
            }
        }

        #endregion

        #region Initialize Controls

        private void InitializeControls()
        {
            // Note: FormBorderStyle is set to None in Designer for proper tab embedding
            // Only set KeyPreview here - other form properties are controlled by the host (Home.cs OpenFormInTab)
            this.KeyPreview = true;

            // Initialize Duplicate Item Behavior combo
            cmbDuplicateItemBehavior.Items.Clear();
            cmbDuplicateItemBehavior.Items.Add("MergeQuantity", "Merge Quantity");
            cmbDuplicateItemBehavior.Items.Add("SeparateRows", "Separate Rows");

            // Initialize Rounding Method combo
            cmbRoundingMethod.Items.Clear();
            cmbRoundingMethod.Items.Add("None", "None");
            cmbRoundingMethod.Items.Add("Nearest5", "Nearest 5");
            cmbRoundingMethod.Items.Add("Nearest10", "Nearest 10");
            cmbRoundingMethod.Items.Add("Up", "Round Up");
            cmbRoundingMethod.Items.Add("Down", "Round Down");

            // Initialize Default Sale Type combo
            cmbDefaultSaleType.Items.Clear();
            cmbDefaultSaleType.Items.Add("Cash", "Cash Sale");
            cmbDefaultSaleType.Items.Add("Credit", "Credit Sale");

            // Initialize Default Price Level combo
            cmbDefaultPriceLevel.Items.Clear();
            cmbDefaultPriceLevel.Items.Add("RetailPrice", "Retail Price");
            cmbDefaultPriceLevel.Items.Add("WholesalePrice", "Wholesale Price");
            cmbDefaultPriceLevel.Items.Add("CreditPrice", "Credit Price");
            cmbDefaultPriceLevel.Items.Add("CardPrice", "Card Price");
            cmbDefaultPriceLevel.Items.Add("MRP", "MRP");
            cmbDefaultPriceLevel.Items.Add("StaffPrice", "Staff Price");
            cmbDefaultPriceLevel.Items.Add("MinPrice", "Min Price");

            // Initialize Default Credit Days combo
            cmbDefaultCreditDays.Items.Clear();
            cmbDefaultCreditDays.Items.Add("0", "0 DAY");
            cmbDefaultCreditDays.Items.Add("15", "15 DAYS");
            cmbDefaultCreditDays.Items.Add("30", "30 DAYS");
            cmbDefaultCreditDays.Items.Add("60", "60 DAYS");
            cmbDefaultCreditDays.Items.Add("90", "90 DAYS");

            // Set up change tracking
            cmbDuplicateItemBehavior.ValueChanged += OnSettingChanged;
            cmbRoundingMethod.ValueChanged += OnSettingChanged;
            cmbDefaultSaleType.ValueChanged += OnSettingChanged;
            cmbDefaultPriceLevel.ValueChanged += OnSettingChanged;
            cmbDefaultCreditDays.ValueChanged += OnSettingChanged;
            chkAllowNegativeStock.CheckedChanged += OnSettingChanged;
            chkAllowSaleBelowCost.CheckedChanged += OnSettingChanged;
            chkAutoPrintAfterSave.CheckedChanged += OnSettingChanged;
            chkShowCostToUser.CheckedChanged += OnSettingChanged;
            chkShowMarginColumn.CheckedChanged += OnSettingChanged;
            numMaxDiscountPercent.ValueChanged += OnSettingChanged;
            numPrintCopies.ValueChanged += OnSettingChanged;

            // Set default values for combos if empty
            if (cmbDuplicateItemBehavior.Items.Count > 0) cmbDuplicateItemBehavior.SelectedIndex = 0;
            if (cmbRoundingMethod.Items.Count > 0) cmbRoundingMethod.SelectedIndex = 0;
            if (cmbDefaultSaleType.Items.Count > 0) cmbDefaultSaleType.SelectedIndex = 0;
            if (cmbDefaultPriceLevel.Items.Count > 0) cmbDefaultPriceLevel.SelectedIndex = 0;
            SetComboValue(cmbDefaultCreditDays, "30");

            // Enforce integer mode for numeric editors
            numMaxDiscountPercent.NumericType = Infragistics.Win.UltraWinEditors.NumericType.Integer;
            numPrintCopies.NumericType = Infragistics.Win.UltraWinEditors.NumericType.Integer;
        }

        private void OnSettingChanged(object sender, EventArgs e)
        {
            _hasUnsavedChanges = true;
        }

        #endregion

        #region Load Settings

        private void LoadCurrentSettings()
        {
            try
            {
                var settings = _settingsRepository.GetSettings(SessionContext.CompanyId, SessionContext.BranchId);

                foreach (var setting in settings)
                {
                    ApplySettingToControl(setting);
                }

                // Update display label
                lblCurrentBranch.Text = $"Branch: {SessionContext.BranchName ?? SessionContext.BranchId.ToString()}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings from database: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LoadDefaultValues();
            }
        }

        private void ApplySettingToControl(POSSetting setting)
        {
            switch (setting.SettingKey)
            {
                case SettingKeys.DuplicateItemBehavior:
                    SetComboValue(cmbDuplicateItemBehavior, setting.SettingValue ?? "MergeQuantity");
                    break;

                case SettingKeys.AllowNegativeStock:
                    chkAllowNegativeStock.Checked = setting.GetBoolValue(false);
                    break;

                case SettingKeys.AllowSaleBelowCost:
                    chkAllowSaleBelowCost.Checked = setting.GetBoolValue(false);
                    break;

                case SettingKeys.AutoPrintAfterSave:
                    chkAutoPrintAfterSave.Checked = setting.GetBoolValue(true);
                    break;

                case SettingKeys.DefaultSaleType:
                    SetComboValue(cmbDefaultSaleType, setting.SettingValue ?? "Cash");
                    break;

                case SettingKeys.DefaultPriceLevel:
                    SetComboValue(cmbDefaultPriceLevel, setting.SettingValue ?? "RetailPrice");
                    break;

                case SettingKeys.DefaultCreditDays:
                    SetComboValue(cmbDefaultCreditDays, setting.SettingValue ?? "30");
                    break;

                case SettingKeys.RoundingMethod:
                    SetComboValue(cmbRoundingMethod, setting.SettingValue ?? "None");
                    break;

                case SettingKeys.MaxDiscountPercent:
                    numMaxDiscountPercent.Value = Math.Min(100, Math.Max(0, setting.GetIntValue(100)));
                    break;

                case SettingKeys.ShowCostToUser:
                    chkShowCostToUser.Checked = setting.GetBoolValue(false);
                    break;

                case SettingKeys.ShowMarginColumn:
                    chkShowMarginColumn.Checked = setting.GetBoolValue(false);
                    break;

                case SettingKeys.PrintCopies:
                    numPrintCopies.Value = Math.Min(10, Math.Max(1, setting.GetIntValue(1)));
                    break;
            }
        }

        private void SetComboValue(Infragistics.Win.UltraWinEditors.UltraComboEditor combo, string value)
        {
            foreach (Infragistics.Win.ValueListItem item in combo.Items)
            {
                if (item.DataValue.ToString() == value)
                {
                    combo.SelectedItem = item;
                    return;
                }
            }
            if (combo.Items.Count > 0) combo.SelectedIndex = 0;
        }

        private void LoadDefaultValues()
        {
            if (cmbDuplicateItemBehavior.Items.Count > 0) cmbDuplicateItemBehavior.SelectedIndex = 0; // MergeQuantity
            chkAllowNegativeStock.Checked = false;
            chkAllowSaleBelowCost.Checked = false;
            chkAutoPrintAfterSave.Checked = true;
            if (cmbDefaultSaleType.Items.Count > 0) cmbDefaultSaleType.SelectedIndex = 0; // Cash
            if (cmbDefaultPriceLevel.Items.Count > 0) cmbDefaultPriceLevel.SelectedIndex = 0; // RetailPrice
            SetComboValue(cmbDefaultCreditDays, "30");
            if (cmbRoundingMethod.Items.Count > 0) cmbRoundingMethod.SelectedIndex = 0; // None
            numMaxDiscountPercent.Value = 100;
            chkShowCostToUser.Checked = false;
            chkShowMarginColumn.Checked = false;
            numPrintCopies.Value = 1;
        }

        #endregion

        #region Save Settings

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            try
            {
                int companyId = SessionContext.CompanyId;
                int branchId = SessionContext.BranchId;

                // Save all settings
                _settingsRepository.SaveSetting(companyId, branchId,
                    SettingKeys.DuplicateItemBehavior,
                    cmbDuplicateItemBehavior.Value?.ToString() ?? "MergeQuantity",
                    "String", "How to handle scanning same item: MergeQuantity or SeparateRows", "Sales");

                _settingsRepository.SaveSetting(companyId, branchId,
                    SettingKeys.AllowNegativeStock,
                    chkAllowNegativeStock.Checked.ToString().ToLower(),
                    "Boolean", "Allow selling items with negative stock", "Inventory");

                _settingsRepository.SaveSetting(companyId, branchId,
                    SettingKeys.AllowSaleBelowCost,
                    chkAllowSaleBelowCost.Checked.ToString().ToLower(),
                    "Boolean", "Allow selling items below cost price", "Sales");

                _settingsRepository.SaveSetting(companyId, branchId,
                    SettingKeys.AutoPrintAfterSave,
                    chkAutoPrintAfterSave.Checked.ToString().ToLower(),
                    "Boolean", "Automatically print receipt after saving sale", "Printing");

                _settingsRepository.SaveSetting(companyId, branchId,
                    SettingKeys.DefaultSaleType,
                    cmbDefaultSaleType.Value?.ToString() ?? "Cash",
                    "String", "Default sale mode for new sales: Cash or Credit", "Sales");

                _settingsRepository.SaveSetting(companyId, branchId,
                    SettingKeys.DefaultPriceLevel,
                    cmbDefaultPriceLevel.Value?.ToString() ?? "RetailPrice",
                    "String", "Default price level for new sales", "Sales");

                _settingsRepository.SaveSetting(companyId, branchId,
                    SettingKeys.DefaultCreditDays,
                    cmbDefaultCreditDays.Value?.ToString() ?? "30",
                    "Integer", "Default credit term (days) for credit sales", "Sales");

                _settingsRepository.SaveSetting(companyId, branchId,
                    SettingKeys.RoundingMethod,
                    cmbRoundingMethod.Value?.ToString() ?? "None",
                    "String", "Rounding: None, Nearest5, Nearest10, Up, Down", "Sales");

                _settingsRepository.SaveSetting(companyId, branchId,
                    SettingKeys.MaxDiscountPercent,
                    Convert.ToInt32(numMaxDiscountPercent.Value).ToString(),
                    "Integer", "Maximum allowed discount percentage", "Sales");

                _settingsRepository.SaveSetting(companyId, branchId,
                    SettingKeys.ShowCostToUser,
                    chkShowCostToUser.Checked.ToString().ToLower(),
                    "Boolean", "Show cost column to regular users", "Display");

                _settingsRepository.SaveSetting(companyId, branchId,
                    SettingKeys.ShowMarginColumn,
                    chkShowMarginColumn.Checked.ToString().ToLower(),
                    "Boolean", "Show margin percentage column", "Display");

                _settingsRepository.SaveSetting(companyId, branchId,
                    SettingKeys.PrintCopies,
                    Convert.ToInt32(numPrintCopies.Value).ToString(),
                    "Integer", "Number of receipt copies to print", "Printing");

                // Reload settings into SessionContext
                var updatedSettings = _settingsRepository.GetSettings(companyId, branchId);
                SessionContext.LoadSettings(updatedSettings);

                _hasUnsavedChanges = false;
                MessageBox.Show("Settings saved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Button Events

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnResetDefaults_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to reset all settings to their default values?",
                "Reset to Defaults", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                LoadDefaultValues();
                _hasUnsavedChanges = true;
            }
        }

        #endregion
    }
}
