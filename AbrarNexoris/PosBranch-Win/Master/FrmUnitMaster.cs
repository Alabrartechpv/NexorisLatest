using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.Misc;
using ModelClass.Master;
using Repository.MasterRepositry;

namespace PosBranch_Win.Master
{
    public partial class FrmUnitMaster : Form
    {
        private UnitMasterRepository _unitRepository;
        private List<UnitMaster> _unitsList;
        private UnitMaster _currentUnit;
        private bool _isEditMode = false;

        public FrmUnitMaster()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            try
            {
                _unitRepository = new UnitMasterRepository();
                _unitsList = new List<UnitMaster>();
                _currentUnit = new UnitMaster();

                // Configure grid appearance
                ConfigureGrid();

                // Configure button states
                ConfigureButtonStates();

                // Load initial data
                LoadUnits();

                // Wire up events
                WireUpEvents();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureGrid()
        {
            try
            {
                // Grid appearance is now configured in Designer file
                // Only configure runtime behavior here
                ultraGridUnits.DisplayLayout.Override.AllowRowFiltering = DefaultableBoolean.True;

                // Configure grid columns
                ConfigureGridColumns();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error configuring grid: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureGridColumns()
        {
            try
            {
                // Try immediate column hiding first
                HideTechnicalColumns();

                // Also use a timer as backup to ensure columns are configured after data binding is complete
                System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
                timer.Interval = 200; // Increased delay to 200ms
                timer.Tick += (sender, e) =>
                {
                    try
                    {
                        timer.Stop();
                        timer.Dispose();

                        // Hide columns again after timer expires
                        HideTechnicalColumns();

                        // Try multiple times with increasing delays
                        System.Windows.Forms.Timer retryTimer = new System.Windows.Forms.Timer();
                        retryTimer.Interval = 500; // 500ms delay
                        retryTimer.Tick += (retrySender, retryE) =>
                        {
                            retryTimer.Stop();
                            retryTimer.Dispose();
                            HideTechnicalColumns();
                        };
                        retryTimer.Start();

                        System.Diagnostics.Debug.WriteLine("Grid columns configured with retry mechanism");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in timer-based column configuration: {ex.Message}");
                    }
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting up grid column configuration: {ex.Message}");
            }
        }

        private void HideTechnicalColumns()
        {
            try
            {
                if (ultraGridUnits.DisplayLayout.Bands.Count > 0)
                {
                    var band = ultraGridUnits.DisplayLayout.Bands[0];

                    // List of columns to hide (only show UnitID and UnitName)
                    string[] columnsToHide = { "IsDelete", "_Operation", "Operation", "IsD", "_Ope",
                        "UnitSymbol", "UnitQuantityCode", "Packing", "NoOfDecimalPlaces", "UnitNameInBill" };

                    foreach (string columnName in columnsToHide)
                    {
                        if (band.Columns.Contains(columnName))
                        {
                            band.Columns[columnName].Hidden = true;
                            System.Diagnostics.Debug.WriteLine($"Hidden column: {columnName}");
                        }
                    }

                    System.Diagnostics.Debug.WriteLine("Technical columns hidden successfully");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error hiding technical columns: {ex.Message}");
            }
        }

        private void UltraGridUnits_InitializeLayout(object sender, Infragistics.Win.UltraWinGrid.InitializeLayoutEventArgs e)
        {
            try
            {
                // Hide technical columns when grid layout is initialized
                HideTechnicalColumns();
                System.Diagnostics.Debug.WriteLine("Grid layout initialized, technical columns hidden");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in InitializeLayout: {ex.Message}");

            }
        }

        private void ConfigureButtonStates()
        {
            // Ribbon toolbar integration active
        }

        private void EnableEditButtons()
        {
            // Edit mode active for selected unit
        }

        private void WireUpEvents()
        {
            // Grid events
            ultraGridUnits.ClickCell += UltraGridUnits_ClickCell;
            ultraGridUnits.DoubleClickRow += UltraGridUnits_DoubleClickRow;
            ultraGridUnits.InitializeLayout += UltraGridUnits_InitializeLayout;
            ultraGridUnits.KeyDown += UltraGridUnits_KeyDown;

            // Search events
            ultraTextSearch.TextChanged += UltraTextSearch_TextChanged;
            ultraTextSearch.KeyDown += UltraTextSearch_KeyDown;

            // Text editor events
            ultraTextUnitName.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { PerformSaveOrUpdate(); e.Handled = true; } };

            // Form events
            this.Load += FrmUnitMaster_Load;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F8)
            {
                PerformSaveOrUpdate();
                return true;
            }
            if (keyData == Keys.F1 || keyData == (Keys.Control | Keys.N))
            {
                PerformClear();
                return true;
            }
            if (keyData == (Keys.Control | Keys.B))
            {
                PerformDelete();
                return true;
            }
            if (keyData == Keys.F4 || keyData == Keys.Escape)
            {
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void UltraTextSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Down)
            {
                if (ultraGridUnits != null && ultraGridUnits.Rows.Count > 0)
                {
                    ultraGridUnits.ActiveRow = ultraGridUnits.Rows[0];
                    ultraGridUnits.Focus();
                    e.Handled = true;
                }
            }
        }

        private void UltraGridUnits_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (ultraGridUnits.ActiveRow != null)
                {
                    LoadUnitToForm(ultraGridUnits.ActiveRow);
                    ultraTextUnitName.Focus();
                    e.Handled = true;
                }
            }
        }

        private void FrmUnitMaster_Load(object sender, EventArgs e)
        {
            try
            {
                LoadUnits();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadUnits()
        {
            try
            {
                // Load units for display (excludes technical fields)
                var displayUnits = _unitRepository.GetUnitsForDisplay();
                if (displayUnits != null && displayUnits.Count > 0)
                {
                    // Store the full units list for operations, but bind display units to grid
                    _unitsList = _unitRepository.GetAllUnits();
                    ultraGridUnits.DataSource = displayUnits;

                    // Immediately try to hide columns
                    HideTechnicalColumns();
                }
                else
                {
                    // Fallback to sample data if no units found
                    _unitsList = new List<UnitMaster>
                    {
                        new UnitMaster { UnitID = 1, UnitName = "Pieces", IsDelete = false },
                        new UnitMaster { UnitID = 2, UnitName = "Kilograms", IsDelete = false },
                        new UnitMaster { UnitID = 3, UnitName = "Liters", IsDelete = false },
                        new UnitMaster { UnitID = 4, UnitName = "Meters", IsDelete = false }
                    };

                    // Create display units from fallback data
                    var fallbackDisplayUnits = new List<ModelClass.Master.UnitMasterDisplay>();
                    foreach (var unit in _unitsList)
                    {
                        fallbackDisplayUnits.Add(new ModelClass.Master.UnitMasterDisplay
                        {
                            UnitID = unit.UnitID,
                            UnitName = unit.UnitName
                        });
                    }

                    ultraGridUnits.DataSource = fallbackDisplayUnits;

                    // Immediately try to hide columns
                    HideTechnicalColumns();
                }

                // Also configure grid columns with timer-based approach
                ConfigureGridColumns();
            }
            catch (Exception ex)
            {
                // Use fallback data if database is not accessible
                _unitsList = new List<UnitMaster>
                {
                    new UnitMaster { UnitID = 1, UnitName = "Pieces", IsDelete = false },
                    new UnitMaster { UnitID = 2, UnitName = "Kilograms", IsDelete = false },
                    new UnitMaster { UnitID = 3, UnitName = "Liters", IsDelete = false },
                    new UnitMaster { UnitID = 4, UnitName = "Meters", IsDelete = false }
                };

                // Create display units from fallback data
                var fallbackDisplayUnits = new List<ModelClass.Master.UnitMasterDisplay>();
                foreach (var unit in _unitsList)
                {
                    fallbackDisplayUnits.Add(new ModelClass.Master.UnitMasterDisplay
                    {
                        UnitID = unit.UnitID,
                        UnitName = unit.UnitName
                    });
                }

                ultraGridUnits.DataSource = fallbackDisplayUnits;

                // No need to configure grid columns since display models don't have technical fields
                ConfigureGridColumns();

                // Show warning but allow form to continue working
                MessageBox.Show($"Warning: Could not connect to database. Using default unit data.\n\nError: {ex.Message}",
                    "Database Connection Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Refreshes the grid data and ensures columns are properly configured
        /// </summary>
        private void RefreshGridData()
        {
            LoadUnits();

            // Force hide columns after data refresh
            System.Windows.Forms.Timer forceHideTimer = new System.Windows.Forms.Timer();
            forceHideTimer.Interval = 300; // 300ms delay
            forceHideTimer.Tick += (sender, e) =>
            {
                forceHideTimer.Stop();
                forceHideTimer.Dispose();
                ForceHideTechnicalColumns();
            };
            forceHideTimer.Start();
        }

        private void ForceHideTechnicalColumns()
        {
            try
            {
                // Hide technical columns
                HideTechnicalColumns();

                // Force the grid to redraw
                ultraGridUnits.Refresh();

                System.Diagnostics.Debug.WriteLine("Force hide technical columns completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in force hide: {ex.Message}");
            }
        }

        #region Ribbon Actions & Public Methods

        public void Save() => PerformSaveOrUpdate();
        public void SaveRecord() => PerformSaveOrUpdate();
        public void SaveData() => PerformSaveOrUpdate();
        public void RibbonSave() => PerformSaveOrUpdate();
        public void btnSave_Click(object sender, EventArgs e) => PerformSaveOrUpdate();

        public void New() => PerformNew();
        public void NewRecord() => PerformNew();
        public void BtnNew_Click(object sender, EventArgs e) => PerformNew();

        public new void Update() => PerformUpdate();
        public void UpdateRecord() => PerformUpdate();
        public void UpdateData() => PerformUpdate();
        public void BtnUpdate_Click(object sender, EventArgs e) => PerformUpdate();

        public void Delete() => PerformDelete();
        public void DeleteRecord() => PerformDelete();
        public void RibbonDeleteInvoice() => PerformDelete();
        public void BtnDelete_Click(object sender, EventArgs e) => PerformDelete();

        public void Clear() => ClearForm();
        public void ClearFields() => ClearForm();
        public void ClearRecord() => ClearForm();
        public void RibbonClear() => ClearForm();
        public void BtnClear_Click(object sender, EventArgs e) => ClearForm();

        public void CloseForm() => this.Close();
        public void BtnClose_Click(object sender, EventArgs e) => this.Close();

        public void PerformSaveOrUpdate()
        {
            if (_isEditMode || (_currentUnit != null && _currentUnit.UnitID > 0))
            {
                PerformUpdate();
            }
            else
            {
                PerformSave();
            }
        }

        public void PerformNew()
        {
            try
            {
                ClearForm();
                _isEditMode = false;
                _currentUnit = new UnitMaster();
                ultraTextUnitName.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating new unit: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void PerformSave()
        {
            try
            {
                if (ValidateForm())
                {
                    _currentUnit = GetFormData();
                    _currentUnit._Operation = "Create";

                    string result = _unitRepository.SaveUnit(_currentUnit);

                    if (result == "Success")
                    {
                        MessageBox.Show("Unit saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        RefreshGridData();
                        ClearForm();
                    }
                    else
                    {
                        MessageBox.Show($"Error saving unit: {result}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving unit: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void PerformUpdate()
        {
            try
            {
                if (ValidateForm())
                {
                    _currentUnit = GetFormData();
                    _currentUnit._Operation = "Update";

                    UnitMaster result = _unitRepository.UpdateUnit(_currentUnit);

                    if (result != null)
                    {
                        MessageBox.Show("Unit updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        RefreshGridData();
                        ClearForm();
                    }
                    else
                    {
                        MessageBox.Show("Error updating unit", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating unit: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void PerformDelete()
        {
            try
            {
                if (_currentUnit != null && _currentUnit.UnitID > 0)
                {
                    if (MessageBox.Show("Are you sure you want to delete this unit?", "Confirm Delete",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        UnitMaster result = _unitRepository.DeleteUnit(_currentUnit.UnitID);

                        if (result != null)
                        {
                            MessageBox.Show("Unit deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            RefreshGridData();
                            ClearForm();
                        }
                        else
                        {
                            MessageBox.Show("Error deleting unit", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("No unit selected for deletion. Please select a unit first.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting unit: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void PerformClear()
        {
            ClearForm();
        }

        #endregion

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ultraTextUnitName.Text))
                {
                    MessageBox.Show("Please enter Unit Name to search!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ultraTextUnitName.Focus();
                    return;
                }

                // Search for units by name
                var searchResults = _unitRepository.GetSearchResultsForDisplay(ultraTextUnitName.Text.Trim());
                if (searchResults != null && searchResults.Count > 0)
                {
                    ultraGridUnits.DataSource = searchResults;

                    // Immediately try to hide columns
                    HideTechnicalColumns();

                    // Also configure grid columns with timer-based approach
                    ConfigureGridColumns();

                    MessageBox.Show($"Found {searchResults.Count} unit(s) matching '{ultraTextUnitName.Text.Trim()}'", "Search Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"No units found matching '{ultraTextUnitName.Text.Trim()}'", "Search Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RefreshGridData(); // Reload all units
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching units: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UltraGridUnits_ClickCell(object sender, Infragistics.Win.UltraWinGrid.ClickCellEventArgs e)
        {
            try
            {
                if (e.Cell != null && e.Cell.Row != null)
                {
                    LoadUnitToForm(e.Cell.Row);

                    // Enable Update and Delete buttons when a unit is selected
                    if (_currentUnit != null && _currentUnit.UnitID > 0)
                    {
                        _isEditMode = true;
                        EnableEditButtons();
                        System.Diagnostics.Debug.WriteLine($"Edit mode enabled for unit: {_currentUnit.UnitID}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading unit to form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UltraGridUnits_DoubleClickRow(object sender, Infragistics.Win.UltraWinGrid.DoubleClickRowEventArgs e)
        {
            try
            {
                if (e.Row != null)
                {
                    LoadUnitToForm(e.Row);
                    _isEditMode = true;
                    EnableEditButtons();
                    System.Diagnostics.Debug.WriteLine($"Double-click edit mode enabled for unit: {_currentUnit?.UnitID}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing unit: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UltraTextSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string searchText = ultraTextSearch.Text.ToLower();

                if (string.IsNullOrEmpty(searchText))
                {
                    // Reload display units
                    var displayUnits = _unitRepository.GetUnitsForDisplay();
                    if (displayUnits != null && displayUnits.Count > 0)
                    {
                        ultraGridUnits.DataSource = displayUnits;
                        // Immediately try to hide columns
                        HideTechnicalColumns();
                    }
                }
                else
                {
                    // Use the repository search method for display
                    var searchResults = _unitRepository.GetSearchResultsForDisplay(searchText);
                    if (searchResults != null && searchResults.Count > 0)
                    {
                        ultraGridUnits.DataSource = searchResults;
                        // Immediately try to hide columns
                        HideTechnicalColumns();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching units: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadUnitToForm(UltraGridRow row)
        {
            try
            {
                // Get the UnitID from the display model
                int unitId = Convert.ToInt32(row.Cells["UnitID"].Value);

                // Find the corresponding full unit from the stored list
                var fullUnit = _unitsList.FirstOrDefault(u => u.UnitID == unitId);

                if (fullUnit != null)
                {
                    _currentUnit = fullUnit;
                    System.Diagnostics.Debug.WriteLine($"Loaded unit to form: ID={_currentUnit.UnitID}, Name={_currentUnit.UnitName}");

                    SetFormData(_currentUnit);

                    // Update form title to show current unit
                    this.Text = $"Unit Master - {_currentUnit.UnitName} (ID: {_currentUnit.UnitID})";
                }
                else
                {
                    // Fallback: create a basic unit with available data
                    _currentUnit = new UnitMaster
                    {
                        UnitID = unitId,
                        UnitName = row.Cells["UnitName"].Value?.ToString() ?? "",
                        IsDelete = false
                    };

                    System.Diagnostics.Debug.WriteLine($"Created fallback unit: ID={_currentUnit.UnitID}, Name={_currentUnit.UnitName}");

                    SetFormData(_currentUnit);
                    this.Text = $"Unit Master - {_currentUnit.UnitName} (ID: {_currentUnit.UnitID})";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading unit data: {ex.Message}");
                MessageBox.Show($"Error loading unit data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetFormData(UnitMaster unit)
        {
            try
            {
                ultraTextUnitName.Text = unit.UnitName;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting form data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private UnitMaster GetFormData()
        {
            try
            {
                return new UnitMaster
                {
                    UnitID = _currentUnit?.UnitID ?? 0,
                    UnitName = ultraTextUnitName.Text.Trim(),
                    // Set default values for fields not shown in UI
                    UnitSymbol = "",
                    UnitQuantityCode = 1,
                    Packing = 1.0,
                    NoOfDecimalPlaces = 0,
                    UnitNameInBill = ultraTextUnitName.Text.Trim(), // Same as UnitName
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting form data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new UnitMaster();
            }
        }

        private bool ValidateForm()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ultraTextUnitName.Text))
                {
                    MessageBox.Show("Please enter Unit Name!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ultraTextUnitName.Focus();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Validation error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public void ClearForm()
        {
            try
            {
                ultraTextUnitName.Text = "";
                _currentUnit = new UnitMaster();
                _isEditMode = false;
                this.Text = "Unit Master";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error clearing form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
