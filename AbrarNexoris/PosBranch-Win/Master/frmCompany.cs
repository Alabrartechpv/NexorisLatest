using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using ModelClass.Master;
using Repository.MasterRepositry;
using PosBranch_Win.DialogBox;
using Repository;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win;

namespace PosBranch_Win.Master
{
    public partial class frmCompany : Form
    {
        // Repository instance
        private CompanyRepo _companyRepo;

        // Current company model
        private CompanyModel _currentCompany;

        // Flag to track if we're in edit mode
        private bool _isEditMode = false;

        public frmCompany()
        {
            InitializeComponent();
            _companyRepo = new CompanyRepo();

            // Initialize with a new empty company model
            _currentCompany = new CompanyModel();

            // Apply modern UI styles consistently
            ApplyModernStyles();

            // Wire up form events
            this.Load += FrmCompany_Load;
            this.Resize += FrmCompany_Resize;
            this.KeyPreview = true;
            this.KeyDown += FrmCompany_KeyDown;
        }

        private void ApplyModernStyles()
        {
            try
            {
                // Base font
                this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

                // Style buttons with flat modern look
                if (btnBrowse != null)
                {
                    btnBrowse.FlatStyle = FlatStyle.Flat;
                    btnBrowse.FlatAppearance.BorderSize = 1;
                }

                // Style text editors
                UltraTextEditor[] editors =
                {
                    txtCompanyName, txtCompanyCaption,
                    txtAddress1, txtAddress2, txtAddress3, txtAddress4,
                    txtZipcode, txtBusinessType, txtBackupPath,
                    txtEmail, txtWebsite, txtPhone, txtMobile,
                    txtTaxNo, txtLicenseNo, txtDLNo1, txtDLNo2, txtFSSAINo,
                    ultraTextEditor1, ultraTextEditor2, ultraTextEditor3, ultraTextEditor4
                };

                foreach (var editor in editors)
                {
                    if (editor == null) continue;
                    editor.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
                    editor.UseOsThemes = DefaultableBoolean.False;
                    editor.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
                }

                // Hide technical ID labels if visible (safety; also set in designer)
                ultraLblStateId.Visible = false;
                ultralblCountryId.Visible = false;
                ultralblTaxId.Visible = false;
                ultraLblCurrencyId.Visible = false;

                // Ensure logo presentation
                picLogo.BackColor = Color.White;
                picLogo.BorderStyle = BorderStyle.FixedSingle;
                picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            }
            catch { /* Styling should not break runtime */ }
        }

        private void FrmCompany_Load(object sender, EventArgs e)
        {
            PerformResponsiveLayout();
            try
            {
                int targetCompanyId = 0;
                if (AppSession.CompanyID > 0)
                {
                    targetCompanyId = AppSession.CompanyID;
                }
                else
                {
                    var companies = _companyRepo.GetCompanyDropdownList();
                    if (companies != null && companies.Any())
                    {
                        targetCompanyId = companies.Max(c => c.CompanyID);
                    }
                }

                if (targetCompanyId > 0)
                {
                    LoadCompanyData(targetCompanyId);
                    _isEditMode = true;
                }
                else
                {
                    // If no companies exist, set up for a new one
                    ClearControls();
                    _isEditMode = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading company data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Save()
        {
            SaveRecord();
        }

        public void SaveRecord()
        {
            btnSave_Click(this, EventArgs.Empty);
        }

        public void Clear()
        {
            ClearRecord();
        }

        public void ClearRecord()
        {
            BtnNew_Click(this, EventArgs.Empty);
        }

        public void Delete()
        {
            DeleteRecord();
        }

        public void DeleteRecord()
        {
            BtnDelete_Click(this, EventArgs.Empty);
        }

        public void New()
        {
            BtnNew_Click(this, EventArgs.Empty);
        }

        private void FrmCompany_Resize(object sender, EventArgs e)
        {
            PerformResponsiveLayout();
        }

        private void FrmCompany_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.KeyCode)
                {
                    case Keys.F8:
                        SaveRecord();
                        break;
                    case Keys.F1:
                        ClearRecord();
                        break;
                    case Keys.F4:
                        this.Close();
                        break;
                }
            }
            catch { }
        }

        private void PerformResponsiveLayout()
        {
            try
            {
                if (ultraPanel1 == null || ultraPanel1.ClientArea == null)
                    return;

                // Disable scrollbars when fitting screen cleanly
                ultraPanel1.AutoScroll = false;

                int clientWidth = ultraPanel1.ClientArea.Width;
                int clientHeight = ultraPanel1.ClientArea.Height;

                if (clientWidth <= 300 || clientHeight <= 300)
                    return;

                int padding = 12;
                int gap = 12;
                int topY = ultraLabelTitle != null ? ultraLabelTitle.Height + padding : 54;

                int availableHeight = clientHeight - topY - padding;
                if (availableHeight < 400)
                {
                    ultraPanel1.AutoScroll = true;
                    availableHeight = 460;
                }

                // Calculate row heights to fill 100% of available screen height
                int topRowHeight = Math.Max(220, (availableHeight - gap) * 52 / 100);
                int row2Height = Math.Max(210, availableHeight - topRowHeight - gap);

                // --- TOP ROW: 3 Columns ---
                int colWidth = (clientWidth - (padding * 2) - (gap * 2)) / 3;
                if (colWidth < 280) colWidth = 280;

                // 1. Basic Information
                if (ultraGroupBoxBasicInfo != null)
                {
                    ultraGroupBoxBasicInfo.SetBounds(padding, topY, colWidth, topRowHeight);
                    int innerW = colWidth - 140;
                    if (innerW > 60)
                    {
                        if (txtCompanyName != null)
                        {
                            txtCompanyName.Width = Math.Max(60, innerW - 30);
                            if (button4 != null)
                                button4.Location = new Point(txtCompanyName.Right + 6, txtCompanyName.Top + 2);
                        }
                        if (txtCompanyCaption != null) txtCompanyCaption.Width = innerW;
                        if (txtBusinessType != null) txtBusinessType.Width = innerW;
                    }
                }

                // 2. Address Information (Scaling all address textboxes & state/country line)
                if (ultraGroupBoxAddress != null)
                {
                    int left2 = padding + colWidth + gap;
                    ultraGroupBoxAddress.SetBounds(left2, topY, colWidth, topRowHeight);
                    int innerW = colWidth - 110;
                    if (innerW > 80)
                    {
                        if (txtAddress1 != null) txtAddress1.Width = innerW;
                        if (txtAddress2 != null) txtAddress2.Width = innerW;
                        if (txtAddress3 != null) txtAddress3.Width = innerW;
                        if (txtAddress4 != null) txtAddress4.Width = innerW;
                        if (txtZipcode != null) txtZipcode.Width = Math.Max(70, innerW / 2);

                        // State & Country line layout - fit without overlap
                        int stateLabelW = label6 != null ? label6.Width : 42;
                        int stateInputW = Math.Max(50, (innerW - 135) / 2);

                        if (ultraTextEditor1 != null)
                        {
                            ultraTextEditor1.Location = new Point(label6 != null ? label6.Left + stateLabelW + 4 : 60, 180);
                            ultraTextEditor1.Width = stateInputW;
                        }
                        if (btn_Add_ItemIype != null && ultraTextEditor1 != null)
                            btn_Add_ItemIype.Location = new Point(ultraTextEditor1.Right + 3, ultraTextEditor1.Top + 2);

                        int countryLabelX = btn_Add_ItemIype != null ? btn_Add_ItemIype.Right + 8 : (ultraTextEditor1 != null ? ultraTextEditor1.Right + 8 : 200);
                        if (label7 != null) label7.Location = new Point(countryLabelX, 183);

                        int countryInputX = label7 != null ? label7.Right + 4 : countryLabelX + 58;
                        if (ultraTextEditor2 != null)
                        {
                            ultraTextEditor2.Location = new Point(countryInputX, 180);
                            ultraTextEditor2.Width = stateInputW;
                            if (button1 != null)
                                button1.Location = new Point(ultraTextEditor2.Right + 3, ultraTextEditor2.Top + 2);
                        }
                    }
                }

                // 3. Contact Information
                if (ultraGroupBoxContact != null)
                {
                    int left3 = padding + (colWidth + gap) * 2;
                    ultraGroupBoxContact.SetBounds(left3, topY, colWidth, topRowHeight);
                    int innerW = colWidth - 90;
                    if (innerW > 80)
                    {
                        if (txtEmail != null) txtEmail.Width = innerW;
                        if (txtWebsite != null) txtWebsite.Width = innerW;

                        // Phone & Mobile in 2 sub-columns
                        int phoneW = Math.Max(60, (innerW - 70) / 2);
                        if (txtPhone != null) txtPhone.Width = phoneW;
                        int mobLabelX = txtPhone != null ? txtPhone.Right + 8 : 220;
                        if (ultraLabel6 != null) ultraLabel6.Location = new Point(mobLabelX, 33);
                        int mobInputX = ultraLabel6 != null ? ultraLabel6.Right + 4 : mobLabelX + 55;
                        if (txtMobile != null)
                        {
                            txtMobile.Location = new Point(mobInputX, 30);
                            txtMobile.Width = phoneW;
                        }
                    }
                }

                // --- BOTTOM ROW ---
                int row2Y = topY + topRowHeight + gap;

                // 4. Legal & Tax Information (~64% width)
                int legalWidth = (clientWidth - (padding * 2) - gap) * 64 / 100;
                if (legalWidth < 480) legalWidth = 480;

                if (ultraGroupBoxLegal != null)
                {
                    ultraGroupBoxLegal.SetBounds(padding, row2Y, legalWidth, row2Height);

                    int col1InputW = Math.Max(80, (legalWidth - 360) / 3);

                    // Line 1: Tax Sys | Currency | Phone No
                    if (ultraTextEditor3 != null)
                    {
                        ultraTextEditor3.Width = col1InputW;
                        if (button2 != null) button2.Location = new Point(ultraTextEditor3.Right + 3, ultraTextEditor3.Top + 2);
                    }

                    int curLabelX = button2 != null ? button2.Right + 10 : 220;
                    if (ultraLabel1 != null) ultraLabel1.Location = new Point(curLabelX, 33);
                    int curInputX = ultraLabel1 != null ? ultraLabel1.Right + 4 : curLabelX + 70;
                    if (ultraTextEditor4 != null)
                    {
                        ultraTextEditor4.Location = new Point(curInputX, 30);
                        ultraTextEditor4.Width = col1InputW;
                        if (button3 != null) button3.Location = new Point(ultraTextEditor4.Right + 3, ultraTextEditor4.Top + 2);
                    }

                    int phoneLabelX = button3 != null ? button3.Right + 10 : 450;
                    if (label10 != null) label10.Location = new Point(phoneLabelX, 33);
                    int phoneInputX = label10 != null ? label10.Right + 4 : phoneLabelX + 75;
                    if (txtTaxNo != null)
                    {
                        txtTaxNo.Location = new Point(phoneInputX, 30);
                        txtTaxNo.Width = col1InputW;
                    }

                    // Line 2: GST NO | Reg. No | Company Code (Ensure no truncation on label13)
                    if (txtLicenseNo != null) txtLicenseNo.Width = col1InputW;

                    if (label12 != null) label12.Location = new Point(curLabelX, 68);
                    if (txtDLNo1 != null)
                    {
                        txtDLNo1.Location = new Point(curInputX, 65);
                        txtDLNo1.Width = col1InputW;
                    }

                    if (label13 != null)
                    {
                        label13.Text = "Company Code:";
                        label13.AutoSize = true;
                        label13.Location = new Point(phoneLabelX, 68);
                    }
                    if (txtDLNo2 != null)
                    {
                        int codeInputX = label13 != null ? label13.Right + 4 : phoneInputX;
                        txtDLNo2.Location = new Point(codeInputX, 65);
                        txtDLNo2.Width = col1InputW;
                    }

                    // Line 3: FSSAI No
                    if (txtFSSAINo != null) txtFSSAINo.Width = col1InputW;
                }

                // Right Column (~36% width): Dates box + Logo box
                int rightX = padding + legalWidth + gap;
                int rightWidth = clientWidth - padding - rightX;
                if (rightWidth < 280) rightWidth = 280;

                int datesHeight = (row2Height - gap) / 2;
                if (datesHeight < 105) datesHeight = 105;

                // 5. Financial Year & Book Period
                if (ultraGroupBoxDates != null)
                {
                    ultraGroupBoxDates.SetBounds(rightX, row2Y, rightWidth, datesHeight);
                    int dtpW = Math.Max(75, (rightWidth - 180) / 2);
                    if (dtpFinYearFrom != null) dtpFinYearFrom.Width = dtpW;
                    int to1LabelX = dtpFinYearFrom != null ? dtpFinYearFrom.Right + 6 : 240;
                    if (FinYearTo != null) FinYearTo.Location = new Point(to1LabelX, 33);
                    int to1InputX = FinYearTo != null ? FinYearTo.Right + 4 : to1LabelX + 30;
                    if (dtpFinYearTo != null)
                    {
                        dtpFinYearTo.Location = new Point(to1InputX, 30);
                        dtpFinYearTo.Width = dtpW;
                    }

                    if (dtpBookFrom != null) dtpBookFrom.Width = dtpW;
                    if (ultraLabel5 != null) ultraLabel5.Location = new Point(to1LabelX, 68);
                    if (dtpBookTo != null)
                    {
                        dtpBookTo.Location = new Point(to1InputX, 65);
                        dtpBookTo.Width = dtpW;
                    }
                }

                // 6. Logo & Backup Settings (Fills bottom right area cleanly)
                int logoY = row2Y + datesHeight + gap;
                int logoHeight = row2Height - datesHeight - gap;
                if (logoHeight < 110) logoHeight = 110;

                if (ultraGroupBoxLogo != null)
                {
                    ultraGroupBoxLogo.SetBounds(rightX, logoY, rightWidth, logoHeight);
                    int backupW = Math.Max(90, rightWidth - 260);
                    if (txtBackupPath != null) txtBackupPath.Width = backupW;
                    if (picLogo != null)
                    {
                        picLogo.Location = new Point(rightWidth - 145, 20);
                        picLogo.Size = new Size(135, Math.Max(60, logoHeight - 32));
                    }
                    if (btnBrowse != null)
                    {
                        int browseX = txtBackupPath != null ? Math.Max(105, txtBackupPath.Left + (backupW - 120) / 2) : 105;
                        btnBrowse.Location = new Point(browseX, logoHeight - 42);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in PerformResponsiveLayout: {ex.Message}");
            }
        }

        private void LoadCompanyData(int companyId)
        {
            try
            {
                // Fetch the company data from the repository
                _currentCompany = _companyRepo.GetCompanyById(companyId);

                if (_currentCompany != null)
                {
                    // Populate form controls with company data
                    txtCompanyName.Text = _currentCompany.CompanyName ?? "";
                    txtCompanyCaption.Text = _currentCompany.CompanyCaption ?? "";
                    txtAddress1.Text = _currentCompany.Address1 ?? "";
                    txtAddress2.Text = _currentCompany.Address2 ?? "";
                    txtAddress3.Text = _currentCompany.Address3 ?? "";
                    txtAddress4.Text = _currentCompany.Address4 ?? "";
                    txtZipcode.Text = _currentCompany.Zipcode ?? "";
                    txtPhone.Text = _currentCompany.Phone ?? "";
                    txtMobile.Text = _currentCompany.Mobile ?? "";
                    txtEmail.Text = _currentCompany.Email ?? "";
                    txtWebsite.Text = _currentCompany.Website ?? "";
                    txtBusinessType.Text = _currentCompany.BusinessType ?? "";
                    txtBackupPath.Text = _currentCompany.BackupPath ?? "";

                    // Load state, country, tax system, and currency values
                    if (_currentCompany.State.HasValue)
                    {
                        // Store state ID
                        ultraLblStateId.Text = _currentCompany.State.Value.ToString();

                        try
                        {
                            // Fetch state name from repository and show in text editor
                            Dropdowns dp = new Dropdowns();
                            var states = dp.getStateDDl();

                            // Safer approach without relying on exact property names through reflection
                            if (states?.List != null)
                            {
                                foreach (var item in states.List)
                                {
                                    try
                                    {
                                        // Try to get the ID property using reflection
                                        var properties = item.GetType().GetProperties();
                                        var idProperty = properties.FirstOrDefault(p =>
                                            p.Name == "StateID" || p.Name == "ID" ||
                                            p.Name.EndsWith("ID") || p.Name.EndsWith("Id"));

                                        if (idProperty != null)
                                        {
                                            var idValue = Convert.ToInt32(idProperty.GetValue(item));
                                            if (idValue == _currentCompany.State.Value)
                                            {
                                                // Found matching state, get the name
                                                var nameProperty = properties.FirstOrDefault(p =>
                                                    p.Name == "StateName" || p.Name == "Name" ||
                                                    p.Name.EndsWith("Name"));

                                                if (nameProperty != null)
                                                {
                                                    ultraTextEditor1.Text = nameProperty.GetValue(item)?.ToString() ?? "";
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        // Skip this item if we can't process it properly
                                        continue;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Just log the error but continue loading
                            Console.WriteLine($"Error loading state: {ex.Message}");
                        }
                    }
                    else
                    {
                        ultraLblStateId.Text = "0";
                        ultraTextEditor1.Text = string.Empty;
                    }

                    if (_currentCompany.Country.HasValue)
                    {
                        // Store country ID
                        ultralblCountryId.Text = _currentCompany.Country.Value.ToString();

                        try
                        {
                            // Fetch country name from repository and show in text editor
                            Dropdowns dp = new Dropdowns();
                            var countries = dp.CountryDDl();

                            // Safer approach without relying on exact property names through reflection
                            if (countries?.List != null)
                            {
                                foreach (var item in countries.List)
                                {
                                    try
                                    {
                                        // Try to get the ID property using reflection
                                        var properties = item.GetType().GetProperties();
                                        var idProperty = properties.FirstOrDefault(p =>
                                            p.Name == "CountryID" || p.Name == "ID" ||
                                            p.Name.EndsWith("ID") || p.Name.EndsWith("Id"));

                                        if (idProperty != null)
                                        {
                                            var idValue = Convert.ToInt32(idProperty.GetValue(item));
                                            if (idValue == _currentCompany.Country.Value)
                                            {
                                                // Found matching country, get the name
                                                var nameProperty = properties.FirstOrDefault(p =>
                                                    p.Name == "CountryName" || p.Name == "Name" ||
                                                    p.Name.EndsWith("Name"));

                                                if (nameProperty != null)
                                                {
                                                    ultraTextEditor2.Text = nameProperty.GetValue(item)?.ToString() ?? "";
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        // Skip this item if we can't process it properly
                                        continue;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Just log the error but continue loading
                            Console.WriteLine($"Error loading country: {ex.Message}");
                        }
                    }
                    else
                    {
                        ultralblCountryId.Text = "0";
                        ultraTextEditor2.Text = string.Empty;
                    }

                    if (_currentCompany.TaxSystem.HasValue)
                    {
                        // Store tax system ID
                        ultralblTaxId.Text = _currentCompany.TaxSystem.Value.ToString();

                        try
                        {
                            // Fetch tax system name from repository and show in text editor
                            Dropdowns dp = new Dropdowns();
                            var taxSystems = dp.TaxTypeDDL();

                            // Safer approach without relying on exact property names through reflection
                            if (taxSystems?.List != null)
                            {
                                foreach (var item in taxSystems.List)
                                {
                                    try
                                    {
                                        // Try to get the ID property using reflection
                                        var properties = item.GetType().GetProperties();
                                        var idProperty = properties.FirstOrDefault(p =>
                                            p.Name == "ID" || p.Name == "TaxTypeID" || p.Name == "TaxID" ||
                                            p.Name.EndsWith("ID") || p.Name.EndsWith("Id"));

                                        if (idProperty != null)
                                        {
                                            var idValue = Convert.ToInt32(idProperty.GetValue(item));
                                            if (idValue == _currentCompany.TaxSystem.Value)
                                            {
                                                // Found matching tax system, get the name/type
                                                var nameProperty = properties.FirstOrDefault(p =>
                                                    p.Name == "TaxType" || p.Name == "TaxName" ||
                                                    p.Name == "Name" || p.Name.EndsWith("Name"));

                                                if (nameProperty != null)
                                                {
                                                    ultraTextEditor3.Text = nameProperty.GetValue(item)?.ToString() ?? "";
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        // Skip this item if we can't process it properly
                                        continue;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Just log the error but continue loading
                            Console.WriteLine($"Error loading tax system: {ex.Message}");
                        }
                    }
                    else
                    {
                        ultralblTaxId.Text = "0";
                        ultraTextEditor3.Text = string.Empty;
                    }

                    if (_currentCompany.Currency.HasValue)
                    {
                        // Store currency ID
                        ultraLblCurrencyId.Text = _currentCompany.Currency.Value.ToString();

                        try
                        {
                            // Fetch currency name from repository and show in text editor
                            Dropdowns dp = new Dropdowns();
                            var currencies = dp.getCurrency();

                            // Safer approach without relying on exact property names through reflection
                            if (currencies?.List != null)
                            {
                                foreach (var item in currencies.List)
                                {
                                    try
                                    {
                                        // Try to get the ID property using reflection
                                        var properties = item.GetType().GetProperties();
                                        var idProperty = properties.FirstOrDefault(p =>
                                            p.Name == "CurrencyID" || p.Name == "ID" ||
                                            p.Name.EndsWith("ID") || p.Name.EndsWith("Id"));

                                        if (idProperty != null)
                                        {
                                            var idValue = Convert.ToInt32(idProperty.GetValue(item));
                                            if (idValue == _currentCompany.Currency.Value)
                                            {
                                                // Found matching currency, get the name
                                                var nameProperty = properties.FirstOrDefault(p =>
                                                    p.Name == "CurrencyName" || p.Name == "Name" ||
                                                    p.Name.EndsWith("Name"));

                                                if (nameProperty != null)
                                                {
                                                    ultraTextEditor4.Text = nameProperty.GetValue(item)?.ToString() ?? "";
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        // Skip this item if we can't process it properly
                                        continue;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Just log the error but continue loading
                            Console.WriteLine($"Error loading currency: {ex.Message}");
                        }
                    }
                    else
                    {
                        ultraLblCurrencyId.Text = "0";
                        ultraTextEditor4.Text = string.Empty;
                    }

                    // Set date pickers
                    if (_currentCompany.FinYearFrom.HasValue)
                        dtpFinYearFrom.Value = _currentCompany.FinYearFrom.Value;
                    else
                        dtpFinYearFrom.Value = DateTime.Today;

                    if (_currentCompany.FinYearTo.HasValue)
                        dtpFinYearTo.Value = _currentCompany.FinYearTo.Value;
                    else
                        dtpFinYearTo.Value = DateTime.Today.AddYears(1);

                    if (_currentCompany.BookFrom.HasValue)
                        dtpBookFrom.Value = _currentCompany.BookFrom.Value;
                    else
                        dtpBookFrom.Value = DateTime.Today;

                    if (_currentCompany.BookTo.HasValue)
                        dtpBookTo.Value = _currentCompany.BookTo.Value;
                    else
                        dtpBookTo.Value = DateTime.Today.AddYears(1);

                    // Tax and license information
                    txtTaxNo.Text = _currentCompany.TaxNo ?? "";
                    txtLicenseNo.Text = _currentCompany.LicenseNo ?? "";
                    txtDLNo1.Text = _currentCompany.DLNO1 ?? "";
                    txtDLNo2.Text = _currentCompany.DLNO2 ?? "";
                    txtFSSAINo.Text = _currentCompany.FSSAINo ?? "";

                    // Load logo if available with safe image loading
                    if (_currentCompany.Logo != null && _currentCompany.Logo.Length > 0)
                    {
                        try
                        {
                            using (MemoryStream ms = new MemoryStream(_currentCompany.Logo))
                            {
                                // Create a copy of the image to avoid GDI+ issues
                                using (Image originalImage = Image.FromStream(ms))
                                {
                                    picLogo.Image = ValidateAndPrepareImage(originalImage);
                                }
                            }
                        }
                        catch (Exception logoEx)
                        {
                            Console.WriteLine($"Warning: Could not load company logo: {logoEx.Message}");
                            picLogo.Image = null;
                        }
                    }
                    else
                    {
                        picLogo.Image = null;
                    }

                    // Set edit mode flag
                    _isEditMode = true;

                    // Store company info in the global session
                    AppSession.LoadCompanyInfo(_currentCompany);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading company data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate required fields
                if (!ValidateCompanyData())
                    return;

                // Get data from controls
                _currentCompany.CompanyName = txtCompanyName.Text.Trim();
                _currentCompany.CompanyCaption = txtCompanyCaption.Text.Trim();
                _currentCompany.Address1 = txtAddress1.Text.Trim();
                _currentCompany.Address2 = txtAddress2.Text.Trim();
                _currentCompany.Address3 = txtAddress3.Text.Trim();
                _currentCompany.Address4 = txtAddress4.Text.Trim();
                _currentCompany.Zipcode = txtZipcode.Text.Trim();
                _currentCompany.Phone = txtPhone.Text.Trim();
                _currentCompany.Mobile = txtMobile.Text.Trim();
                _currentCompany.Email = txtEmail.Text.Trim();
                _currentCompany.Website = txtWebsite.Text.Trim();
                _currentCompany.BusinessType = txtBusinessType.Text.Trim();
                _currentCompany.BackupPath = txtBackupPath.Text.Trim();

                // Get IDs from label controls
                // Get the state ID
                if (int.TryParse(ultraLblStateId.Text, out int stateId))
                {
                    _currentCompany.State = stateId;
                }

                // Get the country ID
                if (int.TryParse(ultralblCountryId.Text, out int countryId))
                {
                    _currentCompany.Country = countryId;
                }

                // Get the tax system ID
                if (int.TryParse(ultralblTaxId.Text, out int taxId))
                {
                    _currentCompany.TaxSystem = taxId;
                }

                // Get the currency ID
                if (int.TryParse(ultraLblCurrencyId.Text, out int currencyId))
                {
                    _currentCompany.Currency = currencyId;
                }

                // Get date picker values
                _currentCompany.FinYearFrom = dtpFinYearFrom.Value;
                _currentCompany.FinYearTo = dtpFinYearTo.Value;
                _currentCompany.BookFrom = dtpBookFrom.Value;
                _currentCompany.BookTo = dtpBookTo.Value;

                // Get tax and license information
                _currentCompany.TaxNo = txtTaxNo.Text.Trim();
                _currentCompany.LicenseNo = txtLicenseNo.Text.Trim();
                _currentCompany.DLNO1 = txtDLNo1.Text.Trim();
                _currentCompany.DLNO2 = txtDLNo2.Text.Trim();
                _currentCompany.FSSAINo = txtFSSAINo.Text.Trim();

                // Get logo from picture box with better error handling
                if (picLogo.Image != null)
                {
                    try
                    {
                        // Validate and prepare the image to prevent GDI+ errors
                        Image safeImage = ValidateAndPrepareImage(picLogo.Image);

                        if (safeImage != null)
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                // Try to save as JPEG first, fallback to PNG if that fails
                                try
                                {
                                    safeImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                                }
                                catch
                                {
                                    // If JPEG fails, try PNG
                                    ms.SetLength(0); // Reset stream
                                    safeImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                }
                                _currentCompany.Logo = ms.ToArray();
                            }

                            // Dispose the safe copy
                            safeImage.Dispose();
                        }
                        else
                        {
                            _currentCompany.Logo = null;
                        }
                    }
                    catch (Exception logoEx)
                    {
                        // Log the logo error but continue with saving other company data
                        Console.WriteLine($"Warning: Could not save logo image: {logoEx.Message}");
                        _currentCompany.Logo = null; // Set logo to null if it fails

                        // Show a warning to the user
                        DialogResult logoResult = MessageBox.Show(
                            "Warning: Could not save the company logo due to image format issues. " +
                            "Do you want to continue saving the company information without the logo?",
                            "Logo Save Warning",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);

                        if (logoResult == DialogResult.No)
                        {
                            return; // User chose not to continue
                        }
                    }
                }

                string result;
                if (_isEditMode)
                {
                    // Update existing company
                    result = _companyRepo.UpdateCompany(_currentCompany);
                }
                else
                {
                    // Create new company
                    result = _companyRepo.CreateCompany(_currentCompany);
                }

                // Check result and show appropriate message
                if (result == "SUCCESS" || int.TryParse(result, out _))
                {
                    MessageBox.Show("Company information saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Update the global session
                    AppSession.LoadCompanyInfo(_currentCompany);

                    // Retain loaded company in edit mode rather than wiping form controls
                    if (int.TryParse(result, out int newCompanyId))
                    {
                        _currentCompany.CompanyID = newCompanyId;
                    }
                    _isEditMode = true;
                    if (_currentCompany.CompanyID > 0)
                    {
                        LoadCompanyData(_currentCompany.CompanyID);
                    }
                }
                else if (result == "NAME EXISTS")
                {
                    MessageBox.Show("A company with this name already exists. Please use a different name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show($"Failed to save company information. Result: {result}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving company information: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            // Clear all controls and prepare for a new company
            ClearControls();
            _currentCompany = new CompanyModel();
            _isEditMode = false;
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                // Confirm deletion
                if (MessageBox.Show("Are you sure you want to delete this company?", "Confirm Delete",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    if (_currentCompany != null && _currentCompany.CompanyID > 0)
                    {
                        string result = _companyRepo.DeleteCompany(_currentCompany.CompanyID);

                        if (result == "SUCCESS")
                        {
                            MessageBox.Show("Company deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Clear controls and create a new company
                            BtnNew_Click(sender, e);
                        }
                        else
                        {
                            MessageBox.Show($"Failed to delete company. Result: {result}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("No company selected to delete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting company: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            try
            {
                // Show file open dialog to select a logo image
                using (OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Title = "Select Company Logo";
                    dlg.Filter = "Image Files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp";

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            // Load image safely to prevent GDI+ errors
                            using (Image originalImage = Image.FromFile(dlg.FileName))
                            {
                                Image safeImage = ValidateAndPrepareImage(originalImage);
                                if (safeImage != null)
                                {
                                    // Dispose the old image if it exists
                                    if (picLogo.Image != null)
                                    {
                                        picLogo.Image.Dispose();
                                    }
                                    picLogo.Image = safeImage;
                                }
                                else
                                {
                                    MessageBox.Show("The selected image could not be loaded. Please try a different image.", "Image Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }
                        catch (Exception imageEx)
                        {
                            MessageBox.Show($"Error loading the selected image: {imageEx.Message}\n\nPlease ensure the image file is not corrupted and try again.", "Image Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearControls()
        {
            // Clear all text boxes
            txtCompanyName.Text = string.Empty;
            txtCompanyCaption.Text = string.Empty;
            txtAddress1.Text = string.Empty;
            txtAddress2.Text = string.Empty;
            txtAddress3.Text = string.Empty;
            txtAddress4.Text = string.Empty;
            txtZipcode.Text = string.Empty;
            txtPhone.Text = string.Empty;
            txtMobile.Text = string.Empty;
            txtEmail.Text = string.Empty;
            txtWebsite.Text = string.Empty;
            txtBusinessType.Text = string.Empty;
            txtBackupPath.Text = string.Empty;
            txtTaxNo.Text = string.Empty;
            txtLicenseNo.Text = string.Empty;
            txtDLNo1.Text = string.Empty;
            txtDLNo2.Text = string.Empty;
            txtFSSAINo.Text = string.Empty;

            // Clear state, country, tax, and currency fields
            ultraTextEditor1.Text = string.Empty;
            ultraTextEditor2.Text = string.Empty;
            ultraTextEditor3.Text = string.Empty;
            ultraTextEditor4.Text = string.Empty;

            ultraLblStateId.Text = "0";
            ultralblCountryId.Text = "0";
            ultralblTaxId.Text = "0";
            ultraLblCurrencyId.Text = "0";

            // Reset date pickers to current date
            dtpFinYearFrom.Value = DateTime.Today;
            dtpFinYearTo.Value = DateTime.Today.AddYears(1);
            dtpBookFrom.Value = DateTime.Today;
            dtpBookTo.Value = DateTime.Today.AddYears(1);

            // Clear logo safely
            if (picLogo.Image != null)
            {
                picLogo.Image.Dispose();
                picLogo.Image = null;
            }
        }

        private bool ValidateCompanyData()
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(txtCompanyName.Text))
            {
                MessageBox.Show("Company name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCompanyName.Focus();
                return false;
            }

            // Validate email format if provided
            if (!string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                if (!Regex.IsMatch(txtEmail.Text, emailPattern))
                {
                    MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return false;
                }
            }

            // Validate phone number if provided - allow more flexible formats
            if (!string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                string phonePattern = @"^[\d\s\-\+\(\)]{7,15}$"; // Allow 7-15 digits with spaces, dashes, plus, parentheses
                if (!Regex.IsMatch(txtPhone.Text, phonePattern))
                {
                    MessageBox.Show("Please enter a valid phone number (7-15 digits, can include spaces, dashes, plus, parentheses).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPhone.Focus();
                    return false;
                }
            }

            // Validate mobile number if provided - allow more flexible formats
            if (!string.IsNullOrWhiteSpace(txtMobile.Text))
            {
                string mobilePattern = @"^[\d\s\-\+\(\)]{7,15}$"; // Allow 7-15 digits with spaces, dashes, plus, parentheses
                if (!Regex.IsMatch(txtMobile.Text, mobilePattern))
                {
                    MessageBox.Show("Please enter a valid mobile number (7-15 digits, can include spaces, dashes, plus, parentheses).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtMobile.Focus();
                    return false;
                }
            }

            // Validate date ranges
            if (dtpFinYearTo.Value <= dtpFinYearFrom.Value)
            {
                MessageBox.Show("Financial year end date must be after the start date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtpFinYearTo.Focus();
                return false;
            }

            if (dtpBookTo.Value <= dtpBookFrom.Value)
            {
                MessageBox.Show("Book end date must be after the start date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtpBookTo.Focus();
                return false;
            }

            return true;
        }

        private void btn_Add_ItemIype_Click(object sender, EventArgs e)
        {
            // Create a new instance of StateDialog
            using (StateDialog stateDialog = new StateDialog())
            {
                // Show it as a modal dialog
                DialogResult result = stateDialog.ShowDialog(this);

                if (result == DialogResult.OK)
                {
                    // If dialog result is OK, get the selected state information
                    string stateName = stateDialog.SelectedStateName;
                    int stateId = stateDialog.SelectedStateID;

                    // Display the selected state information in ultraTextEditor1
                    ultraTextEditor1.Text = stateName;

                    // Store the state ID in ultraLblStateId for company saving purpose
                    ultraLblStateId.Text = stateId.ToString();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (StateDialog stateDialog = new StateDialog())
            {
                DialogResult result = stateDialog.ShowDialog(this);

                if (result == DialogResult.OK)
                {
                    // If dialog result is OK, get the selected state information
                    string stateName = stateDialog.SelectedStateName;
                    int stateId = stateDialog.SelectedStateID;

                    // Display the selected state information in ultraTextEditor1
                    ultraTextEditor1.Text = stateName;

                    // Store the state ID in ultraLblStateId for company saving purpose
                    ultraLblStateId.Text = stateId.ToString();
                }
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            using (CountryDialog countryDialog = new CountryDialog())
            {
                DialogResult result = countryDialog.ShowDialog(this);

                if (result == DialogResult.OK)
                {
                    // If dialog result is OK, get the selected country information
                    string countryName = countryDialog.SelectedCountryName;
                    int countryId = countryDialog.SelectedCountryID;

                    // Display the selected country information in ultraTextEditor2
                    ultraTextEditor2.Text = countryName;

                    // Store the country ID in ultralblCountryId for company saving purpose
                    ultralblCountryId.Text = countryId.ToString();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (TaxDialog taxDialog = new TaxDialog())
            {
                DialogResult result = taxDialog.ShowDialog(this);

                if (result == DialogResult.OK)
                {
                    // If dialog result is OK, get the selected tax information
                    string taxName = taxDialog.SelectedTaxName;
                    int taxId = taxDialog.SelectedTaxID;

                    // Display the selected tax information in ultraTextEditor3
                    ultraTextEditor3.Text = taxName;

                    // Store the tax ID in ultralblTaxId for company saving purpose
                    ultralblTaxId.Text = taxId.ToString();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (CurrencyDialog currencyDialog = new CurrencyDialog())
            {
                DialogResult result = currencyDialog.ShowDialog(this);

                if (result == DialogResult.OK)
                {
                    // If dialog result is OK, get the selected currency information
                    string currencyName = currencyDialog.SelectedCurrencyName;
                    int currencyId = currencyDialog.SelectedCurrencyID;

                    // Display the selected currency information in ultraTextEditor4
                    ultraTextEditor4.Text = currencyName;

                    // Store the currency ID in ultraLblCurrencyId for company saving purpose
                    ultraLblCurrencyId.Text = currencyId.ToString();
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                // Create a new instance of CompanyListDialog
                using (CompanyListDialog companyListDialog = new CompanyListDialog())
                {
                    // Show it as a modal dialog
                    DialogResult result = companyListDialog.ShowDialog(this);

                    if (result == DialogResult.OK)
                    {
                        // If dialog result is OK, get the selected company ID
                        int companyId = companyListDialog.SelectedCompanyID;

                        // Load the selected company data
                        LoadCompanyData(companyId);

                        // Set edit mode flag to true since we're loading an existing company
                        _isEditMode = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading selected company: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void ultraLabel2_Click(object sender, EventArgs e)
        {

        }

        private void dtpFinYearFrom_ValueChanged(object sender, EventArgs e)
        {

        }

        private void FinYearTo_Click(object sender, EventArgs e)
        {

        }

        private void dtpFinYearTo_ValueChanged(object sender, EventArgs e)
        {

        }

        private void ultraLabel4_Click(object sender, EventArgs e)
        {

        }

        private void dtpBookFrom_ValueChanged(object sender, EventArgs e)
        {

        }

        private void ultraLabel5_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Validates and prepares an image for saving to prevent GDI+ errors
        /// </summary>
        /// <param name="image">The image to validate</param>
        /// <returns>A safe copy of the image or null if validation fails</returns>
        private Image ValidateAndPrepareImage(Image image)
        {
            try
            {
                if (image == null) return null;

                // Create a copy of the image to avoid GDI+ issues
                using (Bitmap originalBitmap = new Bitmap(image))
                {
                    // Create a new bitmap with the same dimensions
                    Bitmap safeBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height);

                    using (Graphics g = Graphics.FromImage(safeBitmap))
                    {
                        // Set high quality settings
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                        // Draw the original image onto the new bitmap
                        g.DrawImage(originalBitmap, 0, 0, originalBitmap.Width, originalBitmap.Height);
                    }

                    return safeBitmap;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating image: {ex.Message}");
                return null;
            }
        }
    }
}
