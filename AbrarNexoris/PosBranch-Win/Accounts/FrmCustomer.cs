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
    public partial class FrmCustomer : Form
    {
        public int Ledgerid;
        Dropdowns drop = new Dropdowns();
        ClientOperations operation = new ClientOperations();
        private ClsCustomers originalValues = null;

        public FrmCustomer()
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
                ultraTextCustomer.MaxLength = 200;
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
            toolTip.SetToolTip(ultraTextCustomer, "Enter the customer's full name");
            toolTip.SetToolTip(ultraTextAliasName, "Enter a short name or alias for the customer");
            toolTip.SetToolTip(ultraTextEmail, "Enter the customer's email address");
            toolTip.SetToolTip(ultraTextPhone, "Enter the customer's phone number");
            toolTip.SetToolTip(ultraTextOpenDebit, "Enter opening debit balance (if any)");
            toolTip.SetToolTip(ultraTextOpenCredit, "Enter opening credit balance (if any)");
            toolTip.SetToolTip(ultraComboPriceLevel, "Select the customer's price level");
            toolTip.SetToolTip(ultraTextSSMNumber, "Enter the customer's SSM registration number");
            toolTip.SetToolTip(ultraTextTINNumber, "Enter the customer's TIN number");
            toolTip.SetToolTip(ultraTextCompanyName, "Enter the customer's company name");
            toolTip.SetToolTip(ultraTextCompanyTIN, "Enter the customer's company TIN number");
            toolTip.SetToolTip(ultraTextCompanyMSIC, "Enter the customer's company MSIC code");
            toolTip.SetToolTip(ultraTextCompanyEmail, "Enter the customer's company email address");
            toolTip.SetToolTip(button4, "Click to browse and select from existing customers");
        }



        // Set key navigation and selection properties

        private void LogAvailableColumns(Infragistics.Win.UltraWinGrid.UltraGridBand band)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== Available Grid Columns ===");
                foreach (var column in band.Columns.Cast<Infragistics.Win.UltraWinGrid.UltraGridColumn>())
                {
                    System.Diagnostics.Debug.WriteLine($"Column: {column.Key} - Hidden: {column.Hidden} - Width: {column.Width}");
                }
                System.Diagnostics.Debug.WriteLine("=============================");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error logging columns: {ex.Message}");
            }
        }

        private void LoadInitialData()
        {
            try
            {
                // Load branch data
                LoadBranchData();

                // Load price level data
                LoadPriceLevelData();
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
                BranchDDlGrid branchDDL = drop.getBanchDDl();
                ultraComboBranch.DataSource = branchDDL.List;
                ultraComboBranch.DisplayMember = "BranchName";
                ultraComboBranch.ValueMember = "Id";

                // Debug: Log branch data loading
                System.Diagnostics.Debug.WriteLine($"Branch data loaded: {branchDDL.List?.Count() ?? 0} branches");

                // Log first few branches for debugging
                if (branchDDL.List != null && branchDDL.List.Any())
                {
                    foreach (var branch in branchDDL.List.Take(3))
                    {
                        System.Diagnostics.Debug.WriteLine($"Branch: ID={branch.Id}, Name={branch.BranchName}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Warning: No branch data loaded or branch list is empty");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading branch data: {ex.Message}");
                MessageBox.Show($"Error loading branch data: {ex.Message}", "Branch Load Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadPriceLevelData()
        {
            try
            {
                PriceLevelDDlGrid priceLevelGrid = drop.GetPriceLevel();
                ultraComboPriceLevel.DataSource = priceLevelGrid.List;
                ultraComboPriceLevel.DisplayMember = "PriceLevel";
                ultraComboPriceLevel.ValueMember = "PriceLevelId";

                // Debug: Log price level data loading
                System.Diagnostics.Debug.WriteLine($"Price Level data loaded: {priceLevelGrid.List?.Count() ?? 0} price levels");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading price level data: {ex.Message}");
                MessageBox.Show($"Error loading price level data: {ex.Message}", "Price Level Load Error",
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

        private void FrmCustomer_Load(object sender, EventArgs e)
        {
            // Form is already initialized in constructor
        }

        private void FrmCustomer_KeyDown(object sender, KeyEventArgs e)
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
                                    BtnSave_Click_1(sender, e);
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
                            BtnSave_Click_1(sender, e);
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

        public void RefreshCustomerGrid()
        {
            try
            {
                // Reload data in any currently open customer dialogs
                foreach (Form form in Application.OpenForms)
                {
                    if (form is PosBranch_Win.DialogBox.frmCustomerDialog dlg)
                    {
                        dlg.RefreshData();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing customer grid: {ex.Message}");
            }
        }







        private void StoreOriginalValues(CustAddressDDLGrids customerDetails)
        {
            try
            {
                if (customerDetails?.ListCustomer != null && customerDetails.ListCustomer.Any())
                {
                    var customer = customerDetails.ListCustomer.First();
                    originalValues = new ClsCustomers
                    {
                        LedgerId = customer.LedgerId,
                        LedgerName = customer.LedgerName,
                        AliasName = customer.AliasName,
                        OpenDebit = customer.OpenDebit,
                        OpenCredit = customer.OpenCredit,
                        PriceLevel = customer.PriceLevel,
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

                return (originalValues.LedgerName != ultraTextCustomer.Text.Trim() ||
                        originalValues.AliasName != ultraTextAliasName.Text.Trim() ||
                        originalValues.OpenDebit != Convert.ToDecimal(ultraTextOpenDebit.Text) ||
                        originalValues.OpenCredit != Convert.ToDecimal(ultraTextOpenCredit.Text) ||
                        originalValues.PriceLevel != ultraComboPriceLevel.Text ||
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

                var customer = CreateCustomerObject();
                var customerAddress = CreateCustomerAddressObject();

                customer._Operation = "Update";

                CustomerRepositoty objRepo = new CustomerRepositoty();
                objRepo.UpdateCstomerAddress(customer, customerAddress);

                MessageBox.Show("Customer updated successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                ClearForm();
                RefreshCustomerGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating customer: {ex.Message}", "Error",
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

        private void BtnSave_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateForm())
                    return;

                var customer = CreateCustomerObject();
                var customerAddress = CreateCustomerAddressObject();

                customer._Operation = "GENERATELEDGER";

                CustomerRepositoty CustRepo = new CustomerRepositoty();
                CustRepo.SaveCustomer(customer, customerAddress);

                MessageBox.Show("Customer saved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                ClearForm();
                RefreshCustomerGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving customer: {ex.Message}", "Error",
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
                if (Ledgerid == 0)
                {
                    MessageBox.Show("Please select a customer to delete.", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var result = MessageBox.Show("Are you sure you want to delete this customer?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Implement delete functionality
                    MessageBox.Show("Delete functionality to be implemented.", "Info",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    ClearForm();
                    RefreshCustomerGrid();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting customer: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateForm()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ultraTextCustomer.Text))
                {
                    MessageBox.Show("Please enter Customer Name.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ultraTextCustomer.Focus();
                    return false;
                }

                if (ultraComboBranch.Value == null)
                {
                    MessageBox.Show("Please select a Branch.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ultraComboBranch.Focus();
                    return false;
                }

                if (ultraComboPriceLevel.Value == null && string.IsNullOrWhiteSpace(ultraComboPriceLevel.Text))
                {
                    MessageBox.Show("Please select a Price Level.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ultraComboPriceLevel.Focus();
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

        private ClsCustomers CreateCustomerObject()
        {
            return new ClsCustomers
            {
                CompanyId = SessionContext.CompanyId,
                BranchId = Convert.ToInt32(ultraComboBranch.Value ?? SessionContext.BranchId),
                LedgerId = Ledgerid,
                LedgerName = ultraTextCustomer.Text.Trim(),
                AliasName = ultraTextAliasName.Text.Trim(),
                PriceLevel = ultraComboPriceLevel.Text.Trim(),
                OpenDebit = Convert.ToDecimal(ultraTextOpenDebit.Text),
                OpenCredit = Convert.ToDecimal(ultraTextOpenCredit.Text),
                SSMNumber = ultraTextSSMNumber.Text.Trim(),
                TINNumber = ultraTextTINNumber.Text.Trim(),
                CompanyName = ultraTextCompanyName.Text.Trim(),
                CompanyTIN = ultraTextCompanyTIN.Text.Trim(),
                CompanyMSICCode = ultraTextCompanyMSIC.Text.Trim(),
                CompanyEmail = ultraTextCompanyEmail.Text.Trim(),
                Description = "Customer Description",
                Notes = "Customer Notes"
            };
        }

        private CustomerAddress CreateCustomerAddressObject()
        {
            return new CustomerAddress
            {
                Email = ultraTextEmail.Text.Trim(),
                Phone = ultraTextPhone.Text.Trim(),
                LedgerId = Ledgerid,
                Address = "Customer Address", // You might want to add an address field
                SSMNumber = ultraTextSSMNumber.Text.Trim(),
                TINNumber = ultraTextTINNumber.Text.Trim(),
                CompanyName = ultraTextCompanyName.Text.Trim(),
                CompanyTIN = ultraTextCompanyTIN.Text.Trim(),
                CompanyMSICCode = ultraTextCompanyMSIC.Text.Trim(),
                CompanyEmail = ultraTextCompanyEmail.Text.Trim()
            };
        }

        private void ClearForm()
        {
            try
            {
                ultraTextCustomer.Text = "";
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
                ultraComboPriceLevel.Value = null;
                ultraComboPriceLevel.Text = "";

                Ledgerid = 0;
                originalValues = null;

                SetButtonStates(false);

                ultraTextCustomer.Focus();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing form: {ex.Message}");
            }
        }

        private void btnOpenCustomerDialog_Click(object sender, EventArgs e)
        {
            try
            {
                using (PosBranch_Win.DialogBox.frmCustomerDialog customerDialog = new PosBranch_Win.DialogBox.frmCustomerDialog())
                {
                    if (customerDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Load the selected customer data into the form
                        LoadCustomerData(customerDialog.SelectedCustomerId);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening customer dialog: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                // Open customer list dialog for selection
                using (PosBranch_Win.DialogBox.frmCustomerDialog customerDialog = new PosBranch_Win.DialogBox.frmCustomerDialog())
                {
                    if (customerDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Load the selected customer data into the form
                        LoadCustomerData(customerDialog.SelectedCustomerId);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening customer list: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadCustomerData(int customerId)
        {
            try
            {
                CustomerRepositoty customerRepo = new CustomerRepositoty();
                var customer = customerRepo.GetCustomerById(customerId);
                if (customer != null)
                {
                    // Set Ledgerid for update operations
                    Ledgerid = customer.LedgerId;

                    ultraTextCustomer.Text = customer.LedgerName ?? "";
                    ultraTextAliasName.Text = customer.AliasName ?? "";
                    ultraTextOpenDebit.Text = customer.OpenDebit.ToString("F2");
                    ultraTextOpenCredit.Text = customer.OpenCredit.ToString("F2");

                    // Get customer address data for email, phone, and new company fields
                    try
                    {
                        var customerAddressData = customerRepo.getCustAddress(customerId);
                        if (customerAddressData?.ListCustAddress != null && customerAddressData.ListCustAddress.Any())
                        {
                            var address = customerAddressData.ListCustAddress.First();
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

                    // Set the branch if available
                    if (customer.BranchId > 0)
                    {
                        ultraComboBranch.Value = customer.BranchId;
                    }

                    // Set the price level if available
                    if (!string.IsNullOrEmpty(customer.PriceLevel))
                    {
                        ultraComboPriceLevel.Text = customer.PriceLevel;
                    }

                    // Store the original values for change detection
                    var customerDetails = new CustAddressDDLGrids();
                    customerDetails.ListCustomer = new List<ClsCustomers> { customer };
                    StoreOriginalValues(customerDetails);

                    // Set form to update mode
                    SetButtonStates(true);
                }
                else
                {
                    MessageBox.Show("Customer not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customer data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        // Test method to debug branch binding
        public void TestBranchBinding()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== TESTING BRANCH BINDING ===");

                // Test 1: Check if branch combo has data
                if (ultraComboBranch.DataSource != null)
                {
                    var branchDataSource = ultraComboBranch.DataSource;
                    System.Diagnostics.Debug.WriteLine($"Branch combo data source type: {branchDataSource.GetType().Name}");

                    if (branchDataSource is System.Collections.IList branchList)
                    {
                        System.Diagnostics.Debug.WriteLine($"Branch list count: {branchList.Count}");
                        if (branchList.Count > 0)
                        {
                            var firstBranch = branchList[0];
                            System.Diagnostics.Debug.WriteLine($"First branch type: {firstBranch.GetType().Name}");

                            // Try to get branch properties
                            var branchType = firstBranch.GetType();
                            foreach (var prop in branchType.GetProperties())
                            {
                                try
                                {
                                    var value = prop.GetValue(firstBranch);
                                    System.Diagnostics.Debug.WriteLine($"  Branch.{prop.Name}: {value}");
                                }
                                catch { }
                            }
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Branch combo has no data source!");
                }

                // Test 2: Check current branch value
                System.Diagnostics.Debug.WriteLine($"Current branch combo value: {ultraComboBranch.Value}");
                System.Diagnostics.Debug.WriteLine($"Current branch combo text: {ultraComboBranch.Text}");



                System.Diagnostics.Debug.WriteLine("=== END BRANCH BINDING TEST ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in TestBranchBinding: {ex.Message}");
            }
        }


    }
}