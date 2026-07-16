using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ModelClass;
using ModelClass.Master;
using Repository;
using Infragistics.Win.UltraWinGrid;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.Misc;
using PosBranch_Win.DialogBox;

namespace PosBranch_Win.Accounts
{
    public partial class FrmVendor : Form
    {
        // Static event to notify other forms when a vendor is saved/updated
        public static event Action OnVendorSaved;

        // Helper method to raise the vendor saved event safely
        private static void RaiseVendorSaved()
        {
            try
            {
                OnVendorSaved?.Invoke();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error raising vendor saved event: {ex.Message}");
            }
        }

        // Helper method to get dynamic CompanyId
        // Prefer SessionContext, fallback to DataBase for backward compatibility
        private int GetCompanyId()
        {
            try
            {
                if (SessionContext.IsInitialized && SessionContext.CompanyId > 0)
                {
                    return SessionContext.CompanyId;
                }
                else if (!string.IsNullOrEmpty(DataBase.CompanyId) && int.TryParse(DataBase.CompanyId, out int companyId) && companyId > 0)
                {
                    return companyId;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting CompanyId: {ex.Message}");
            }
            throw new InvalidOperationException("CompanyId is not set. Please ensure session is initialized.");
        }

        public int LedgerId;
        Dropdowns drop = new Dropdowns();
        VendorRepository vendorRepo = new VendorRepository();
        private ClsVendors originalValues = null;

        public FrmVendor()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            try
            {
                // Set up form properties
                this.KeyPreview = true;

                // Initialize controls
                InitializeControls();

                // Load initial data
                LoadInitialData();

                // Set initial button states
                SetButtonStates(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing form: {ex.Message}", "Initialization Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeControls()
        {
            try
            {
                // Set default values for numeric fields
                ultraTextOpenDebit.Text = "0.00";
                ultraTextOpenCredit.Text = "0.00";

                // Set up input masks and validation
                ultraTextPhone.MaxLength = 20;
                ultraTextEmail.MaxLength = 100;
                ultraTextVendor.MaxLength = 200;
                ultraTextAliasName.MaxLength = 100;
                ultraTextSSMNumber.MaxLength = 50;
                ultraTextTINNumber.MaxLength = 50;
                ultraTextCompanyName.MaxLength = 200;
                ultraTextCompanyTIN.MaxLength = 50;
                ultraTextCompanyMSIC.MaxLength = 50;
                ultraTextCompanyEmail.MaxLength = 100;

                // Set up tooltips
                SetupTooltips();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing controls: {ex.Message}");
            }
        }

        private void SetupTooltips()
        {
            // Add tooltips for better user experience
            var toolTip = new ToolTip();
            toolTip.SetToolTip(ultraTextVendor, "Enter the vendor's full business name");
            toolTip.SetToolTip(ultraTextAliasName, "Enter a short name or alias for the vendor");
            toolTip.SetToolTip(ultraTextEmail, "Enter the vendor's email address");
            toolTip.SetToolTip(ultraTextPhone, "Enter the vendor's phone number");
            toolTip.SetToolTip(ultraTextOpenDebit, "Enter opening debit balance (if any)");
            toolTip.SetToolTip(ultraTextOpenCredit, "Enter opening credit balance (if any)");
            toolTip.SetToolTip(ultraTextSSMNumber, "Enter the vendor's SSM registration number");
            toolTip.SetToolTip(ultraTextTINNumber, "Enter the vendor's TIN number");
            toolTip.SetToolTip(ultraTextCompanyName, "Enter the vendor's company name");
            toolTip.SetToolTip(ultraTextCompanyTIN, "Enter the vendor's company TIN number");
            toolTip.SetToolTip(ultraTextCompanyMSIC, "Enter the vendor's company MSIC code");
            toolTip.SetToolTip(ultraTextCompanyEmail, "Enter the vendor's company email address");
        }

        private void LoadInitialData()
        {
            try
            {
                // Load branch data
                LoadBranchData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading initial data: {ex.Message}", "Data Load Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadBranchData()
        {
            try
            {
                BranchDDlGrid branchDDLGrid = drop.getBanchDDl();
                ultraComboBranch.DataSource = branchDDLGrid.List;
                ultraComboBranch.DisplayMember = "BranchName";
                ultraComboBranch.ValueMember = "Id";

                // Debug: Log branch data loading
                System.Diagnostics.Debug.WriteLine($"Branch data loaded: {branchDDLGrid.List?.Count() ?? 0} branches");
                if (branchDDLGrid.List != null)
                {
                    foreach (var branch in branchDDLGrid.List.Take(3)) // Log first 3 branches
                    {
                        System.Diagnostics.Debug.WriteLine($"Branch: ID={branch.Id}, Name={branch.BranchName}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading branch data: {ex.Message}");
                MessageBox.Show($"Error loading branch data: {ex.Message}", "Branch Load Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void SetButtonStates(bool isEditMode)
        {
            try
            {
                btnSave.Visible = !isEditMode;
                btnUpdate.Visible = isEditMode;
                btnDelete.Visible = isEditMode;
                btnClear.Visible = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting button states: {ex.Message}");
            }
        }

        private void FrmVendor_Load(object sender, EventArgs e)
        {
            // Form is already initialized in constructor
        }

        private void FrmVendor_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.KeyCode)
                {
                    case Keys.Escape:
                        if (HasUnsavedChanges())
                        {
                            var result = MessageBox.Show("You have unsaved changes. Do you want to save them before closing?",
                                "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                            if (result == DialogResult.Yes)
                            {
                                if (btnUpdate.Visible)
                                    btnUpdate_Click(sender, e);
                                else
                                    btnSave_Click_1(sender, e);
                            }
                            else if (result == DialogResult.Cancel)
                            {
                                return;
                            }
                        }
                        this.Close();
                        break;
                    case Keys.F8:
                        if (btnSave.Visible)
                            btnSave_Click_1(sender, e);
                        else
                            btnUpdate_Click(sender, e);
                        break;
                    case Keys.F4:
                        this.Close();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error handling key press: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }










        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                using (var vendorDialog = new DialogBox.frmVendorDig())
                {
                    if (vendorDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Load the selected vendor data into the form
                        LoadVendorData(vendorDialog.SelectedVendorId);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening vendor dialog: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }





        private void StoreOriginalValues(VendorAddressDDLGrid vendorDetails)
        {
            try
            {
                if (vendorDetails?.ListVendor != null && vendorDetails.ListVendor.Any())
                {
                    var vendor = vendorDetails.ListVendor.First();
                    originalValues = new ClsVendors
                    {
                        LedgerId = vendor.LedgerId,
                        LedgerName = vendor.LedgerName,
                        Alias = vendor.Alias,
                        OpnDebit = vendor.OpnDebit,
                        OpnCredit = vendor.OpnCredit,
                        // Get new fields from current form values since they come from ContactDetails
                        SSMNumber = ultraTextSSMNumber.Text.Trim(),
                        TINNumber = ultraTextTINNumber.Text.Trim(),
                        CompanyName = ultraTextCompanyName.Text.Trim(),
                        CompanyTIN = ultraTextCompanyTIN.Text.Trim(),
                        CompanyMSICCode = ultraTextCompanyMSIC.Text.Trim(),
                        CompanyEmail = ultraTextCompanyEmail.Text.Trim()
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error storing original values: {ex.Message}");
            }
        }

        private bool HasUnsavedChanges()
        {
            try
            {
                if (originalValues == null)
                    return false;

                return (originalValues.LedgerName != ultraTextVendor.Text.Trim() ||
                        originalValues.Alias != ultraTextAliasName.Text.Trim() ||
                        originalValues.OpnDebit != Convert.ToDecimal(ultraTextOpenDebit.Text) ||
                        originalValues.OpnCredit != Convert.ToDecimal(ultraTextOpenCredit.Text) ||
                        originalValues.SSMNumber != ultraTextSSMNumber.Text.Trim() ||
                        originalValues.TINNumber != ultraTextTINNumber.Text.Trim() ||
                        originalValues.CompanyName != ultraTextCompanyName.Text.Trim() ||
                        originalValues.CompanyTIN != ultraTextCompanyTIN.Text.Trim() ||
                        originalValues.CompanyMSICCode != ultraTextCompanyMSIC.Text.Trim() ||
                        originalValues.CompanyEmail != ultraTextCompanyEmail.Text.Trim());
            }
            catch
            {
                return false;
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateForm())
                    return;

                var vendor = CreateVendorObject();
                var vendorAddress = CreateVendorAddressObject();

                vendor._Operation = "Update";
                vendorRepo.UpdateVendorAddress(vendor, vendorAddress);

                MessageBox.Show("Vendor updated successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Notify other forms that a vendor was updated
                RaiseVendorSaved();

                ClearForm();
                RefreshVendorGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating vendor: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Save()
        {
            // Delegate to the actual visible/enabled button — no hardcoded handler names
            if (btnSave.Visible && btnSave.Enabled)
                btnSave.PerformClick();
            else if (btnUpdate.Visible && btnUpdate.Enabled)
                btnUpdate.PerformClick();
        }

        private void btnSave_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateForm())
                    return;

                var vendor = CreateVendorObject();
                var vendorAddress = CreateVendorAddressObject();

                vendor._Operation = "GENERATELEDGER";
                vendorRepo.SaveVendor(vendor, vendorAddress);

                MessageBox.Show("Vendor saved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Notify other forms that a vendor was saved
                RaiseVendorSaved();

                ClearForm();
                RefreshVendorGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving vendor: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (LedgerId == 0)
                {
                    MessageBox.Show("Please select a vendor to delete.", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var result = MessageBox.Show("Are you sure you want to delete this vendor?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Implement delete functionality
                    MessageBox.Show("Delete functionality to be implemented.", "Info",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    ClearForm();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting vendor: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateForm()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ultraTextVendor.Text))
                {
                    MessageBox.Show("Please enter Vendor Name.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ultraTextVendor.Focus();
                    return false;
                }

                if (ultraComboBranch.Value == null)
                {
                    MessageBox.Show("Please select a Branch.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ultraComboBranch.Focus();
                    return false;
                }

                // Validate numeric fields
                if (!decimal.TryParse(ultraTextOpenDebit.Text, out _))
                {
                    MessageBox.Show("Please enter a valid Open Debit amount.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ultraTextOpenDebit.Focus();
                    return false;
                }

                if (!decimal.TryParse(ultraTextOpenCredit.Text, out _))
                {
                    MessageBox.Show("Please enter a valid Open Credit amount.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ultraTextOpenCredit.Focus();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Validation error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private ClsVendors CreateVendorObject()
        {
            return new ClsVendors
            {
                CompanyId = GetCompanyId(),
                BranchId = Convert.ToInt32(ultraComboBranch.Value),
                LedgerId = LedgerId,
                LedgerName = ultraTextVendor.Text.Trim(),
                Alias = ultraTextAliasName.Text.Trim(),
                OpnDebit = Convert.ToDecimal(ultraTextOpenDebit.Text),
                OpnCredit = Convert.ToDecimal(ultraTextOpenCredit.Text),
                SSMNumber = ultraTextSSMNumber.Text.Trim(),
                TINNumber = ultraTextTINNumber.Text.Trim(),
                CompanyName = ultraTextCompanyName.Text.Trim(),
                CompanyTIN = ultraTextCompanyTIN.Text.Trim(),
                CompanyMSICCode = ultraTextCompanyMSIC.Text.Trim(),
                CompanyEmail = ultraTextCompanyEmail.Text.Trim(),
                Description = "Vendor Description",
                Notes = "Vendor Notes"
            };
        }

        private VendorAddress CreateVendorAddressObject()
        {
            return new VendorAddress
            {
                Email = ultraTextEmail.Text.Trim(),
                Phone = ultraTextPhone.Text.Trim(),
                LedgerId = LedgerId,
                Address = "Vendor Address", // You might want to add an address field
                SSMNumber = ultraTextSSMNumber.Text.Trim(),
                TINNumber = ultraTextTINNumber.Text.Trim(),
                CompanyName = ultraTextCompanyName.Text.Trim(),
                CompanyTIN = ultraTextCompanyTIN.Text.Trim(),
                CompanyMSICCode = ultraTextCompanyMSIC.Text.Trim(),
                CompanyEmail = ultraTextCompanyEmail.Text.Trim()
            };
        }

        private void RefreshVendorGrid()
        {
            try
            {
                // Reload data in any currently open vendor dialogs
                foreach (Form form in Application.OpenForms)
                {
                    if (form is PosBranch_Win.DialogBox.frmVendorDig dlg)
                    {
                        dlg.RefreshData();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing vendor grid: {ex.Message}");
            }
        }

        private void ClearForm()
        {
            try
            {
                ultraTextVendor.Text = "";
                ultraTextAliasName.Text = "";
                ultraTextEmail.Text = "";
                ultraTextPhone.Text = "";
                ultraTextOpenDebit.Text = "0.00";
                ultraTextOpenCredit.Text = "0.00";
                ultraTextSSMNumber.Text = "";
                ultraTextTINNumber.Text = "";
                ultraTextCompanyName.Text = "";
                ultraTextCompanyTIN.Text = "";
                ultraTextCompanyMSIC.Text = "";
                ultraTextCompanyEmail.Text = "";
                ultraComboBranch.Value = null;

                LedgerId = 0;
                originalValues = null;

                SetButtonStates(false);

                ultraTextVendor.Focus();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing form: {ex.Message}");
            }
        }

        private void LoadVendorData(int vendorId)
        {
            try
            {
                var vendor = vendorRepo.GetVendorById(vendorId);
                if (vendor != null)
                {
                    // Set LedgerId for update operations
                    LedgerId = vendor.LedgerId;

                    ultraTextVendor.Text = vendor.LedgerName ?? "";
                    ultraTextAliasName.Text = vendor.Alias ?? "";
                    ultraTextOpenDebit.Text = vendor.OpnDebit.ToString("F2");
                    ultraTextOpenCredit.Text = vendor.OpnCredit.ToString("F2");

                    // Set the branch if available
                    if (vendor.BranchId > 0)
                    {
                        ultraComboBranch.Value = vendor.BranchId;
                    }

                    // Get vendor address data for email, phone, and new company fields
                    VendorAddressDDLGrid vendorAddressData = null;
                    try
                    {
                        vendorAddressData = vendorRepo.getVendorAddress(vendorId);
                        if (vendorAddressData?.ListVendorAddress != null && vendorAddressData.ListVendorAddress.Any())
                        {
                            var address = vendorAddressData.ListVendorAddress.First();
                            ultraTextEmail.Text = address.Email ?? "";
                            ultraTextPhone.Text = address.Phone ?? "";
                            ultraTextSSMNumber.Text = address.SSMNumber ?? "";
                            ultraTextTINNumber.Text = address.TINNumber ?? "";
                            ultraTextCompanyName.Text = address.CompanyName ?? "";
                            ultraTextCompanyTIN.Text = address.CompanyTIN ?? "";
                            ultraTextCompanyMSIC.Text = address.CompanyMSICCode ?? "";
                            ultraTextCompanyEmail.Text = address.CompanyEmail ?? "";
                        }
                        else
                        {
                            // Clear all fields if no address data found
                            ultraTextEmail.Text = "";
                            ultraTextPhone.Text = "";
                            ultraTextSSMNumber.Text = "";
                            ultraTextTINNumber.Text = "";
                            ultraTextCompanyName.Text = "";
                            ultraTextCompanyTIN.Text = "";
                            ultraTextCompanyMSIC.Text = "";
                            ultraTextCompanyEmail.Text = "";
                        }
                    }
                    catch (Exception addrEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading address data: {addrEx.Message}");
                        // Clear all fields if address loading fails
                        ultraTextEmail.Text = "";
                        ultraTextPhone.Text = "";
                        ultraTextSSMNumber.Text = "";
                        ultraTextTINNumber.Text = "";
                        ultraTextCompanyName.Text = "";
                        ultraTextCompanyTIN.Text = "";
                        ultraTextCompanyMSIC.Text = "";
                        ultraTextCompanyEmail.Text = "";
                    }

                    // Store the original values for change detection
                    StoreOriginalValues(vendorAddressData);

                    // Set form to update mode
                    SetButtonStates(true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading vendor data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}