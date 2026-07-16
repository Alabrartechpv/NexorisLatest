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
using PosBranch_Win.DialogBox;
using Repository;
using System.IO;
using ModelClass.Master;
using Infragistics.Win.UltraWinGrid;

namespace PosBranch_Win.Master
{
    public partial class FrmBrand : Form
    {
        Brand brnd = new Brand();
        Dropdowns drp = new Dropdowns();
        ClientOperations operations = new ClientOperations();
        int Id;
        public int SelectedId;
        public FrmBrand()
        {
            InitializeComponent();
            btnUpdate.Visible = false;
        }


        private void FormatGrid()
        {
            try
            {
                // Configure main grid appearance with modern styling
                ultraGrid1.DisplayLayout.Override.AllowAddNew = Infragistics.Win.UltraWinGrid.AllowAddNew.No;
                ultraGrid1.DisplayLayout.Override.AllowDelete = Infragistics.Win.DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.AllowUpdate = Infragistics.Win.DefaultableBoolean.False;
                ultraGrid1.DisplayLayout.Override.RowSelectors = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = Infragistics.Win.UltraWinGrid.SelectType.Single;
                ultraGrid1.DisplayLayout.Override.HeaderClickAction = Infragistics.Win.UltraWinGrid.HeaderClickAction.SortSingle;
                ultraGrid1.DisplayLayout.Override.CellClickAction = Infragistics.Win.UltraWinGrid.CellClickAction.RowSelect;

                // Configure header appearance with modern gradient look
                ultraGrid1.DisplayLayout.Override.HeaderStyle = Infragistics.Win.HeaderStyle.WindowsXPCommand;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackColor2 = System.Drawing.Color.FromArgb(0, 102, 184);
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ForeColor = System.Drawing.Color.White;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.Bold = Infragistics.Win.DefaultableBoolean.True;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.FontData.SizeInPoints = 9;
                ultraGrid1.DisplayLayout.Override.HeaderAppearance.ThemedElementAlpha = Infragistics.Win.Alpha.Transparent;

                // Reset appearance settings
                ultraGrid1.DisplayLayout.Override.CellAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.SelectedCellAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.RowAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.SelectedRowAppearance.Reset();
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.Reset();

                // Set key navigation and selection properties
                ultraGrid1.DisplayLayout.Override.SelectTypeRow = Infragistics.Win.UltraWinGrid.SelectType.Single;
                ultraGrid1.DisplayLayout.Override.SelectTypeCell = Infragistics.Win.UltraWinGrid.SelectType.Single;
                ultraGrid1.DisplayLayout.Override.SelectTypeCol = Infragistics.Win.UltraWinGrid.SelectType.None;
                ultraGrid1.DisplayLayout.TabNavigation = Infragistics.Win.UltraWinGrid.TabNavigation.NextCell;

                // Set basic row appearance
                ultraGrid1.DisplayLayout.Override.RowAppearance.BackColor = System.Drawing.Color.White;

                // Configure active row highlighting
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackColor = System.Drawing.Color.LightSkyBlue;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.BackGradientStyle = Infragistics.Win.GradientStyle.None;
                ultraGrid1.DisplayLayout.Override.ActiveRowAppearance.ForeColor = System.Drawing.Color.Black;

                // Remove active cell highlighting
                ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.BackColor = System.Drawing.Color.Empty;
                ultraGrid1.DisplayLayout.Override.ActiveCellAppearance.ForeColor = System.Drawing.Color.Black;

                // Configure scrolling and layout
                ultraGrid1.DisplayLayout.ScrollBounds = Infragistics.Win.UltraWinGrid.ScrollBounds.ScrollToFill;
                ultraGrid1.DisplayLayout.AutoFitStyle = Infragistics.Win.UltraWinGrid.AutoFitStyle.ResizeAllColumns;

                // Configure spacing and expansion behavior
                ultraGrid1.DisplayLayout.InterBandSpacing = 10;
                ultraGrid1.DisplayLayout.Override.ExpansionIndicator = Infragistics.Win.UltraWinGrid.ShowExpansionIndicator.Never;
                ultraGrid1.DisplayLayout.Override.RowSpacingBefore = 0;

                // Set row sizing to fixed height for compact appearance
                ultraGrid1.DisplayLayout.Override.RowSizing = Infragistics.Win.UltraWinGrid.RowSizing.Fixed;
                ultraGrid1.DisplayLayout.Override.DefaultRowHeight = 25;
                ultraGrid1.DisplayLayout.Override.CellPadding = 2;

                // Configure columns after data is loaded
                ConfigureGridColumns();

                // Connect grid events
                ConnectGridEvents();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error formatting grid: {ex.Message}");
            }
        }

        private void ConfigureGridColumns()
        {
            try
            {
                if (ultraGrid1.DisplayLayout.Bands.Count > 0)
                {
                    var band = ultraGrid1.DisplayLayout.Bands[0];

                    // Id column
                    if (band.Columns.Exists("Id"))
                    {
                        band.Columns["Id"].Header.Caption = "ID";
                        band.Columns["Id"].Width = 80;
                        band.Columns["Id"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Center;
                        band.Columns["Id"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                        band.Columns["Id"].MinWidth = 60;
                    }

                    // BrandName column
                    if (band.Columns.Exists("BrandName"))
                    {
                        band.Columns["BrandName"].Header.Caption = "Brand Name";
                        band.Columns["BrandName"].Width = 300;
                        band.Columns["BrandName"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Left;
                        band.Columns["BrandName"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                        band.Columns["BrandName"].MinWidth = 200;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error configuring grid columns: {ex.Message}");
            }
        }

        private void ConnectGridEvents()
        {
            try
            {
                // Add keyboard event handler for navigation
                ultraGrid1.KeyDown += UltraGrid1_KeyDown;

                // Add row selection event
                ultraGrid1.AfterRowActivate += UltraGrid1_AfterRowActivate;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error connecting grid events: {ex.Message}");
            }
        }

        private void UltraGrid1_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter && ultraGrid1.ActiveRow != null)
                {
                    // Handle Enter key press
                    DisplayRowValues(ultraGrid1.ActiveRow);
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in grid key handler: {ex.Message}");
            }
        }

        private void UltraGrid1_AfterRowActivate(object sender, EventArgs e)
        {
            try
            {
                if (ultraGrid1.ActiveRow != null)
                {
                    // Handle row activation for selection
                    DisplayRowValues(ultraGrid1.ActiveRow);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in row activate handler: {ex.Message}");
            }
        }

        private void DisplayRowValues(UltraGridRow row)
        {
            try
            {
                if (row != null && row.Cells.Count > 0)
                {
                    ultraTextBrandName.Text = row.Cells["BrandName"].Value?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error displaying row values: {ex.Message}");
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                brnd.BrandName = ultraTextBrandName.Text;

                if (!string.IsNullOrEmpty(openFileDialog1.FileName) && File.Exists(openFileDialog1.FileName))
                {
                    Byte[] brandPhoto = File.ReadAllBytes(openFileDialog1.FileName);
                    brnd.Photo = brandPhoto;
                }

                brnd._Operation = "CREATE";
                string message = operations.SaveBrand(brnd);

                // Show success dialog like other forms
                frmSuccesMsg msg = new frmSuccesMsg();
                msg.ShowDialog();

                this.RefreshBrand();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving brand: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FrmBrand_Load(object sender, EventArgs e)
        {
            this.FormatGrid();
            this.RefreshBrand();
            KeyPreview = true;
        }

        public void RefreshBrand()
        {
            try
            {
                BrandDDLGrid brndGrid = drp.getBrandDDL();
                ultraGrid1.DataSource = brndGrid.List;

                // Configure columns after data is loaded
                ConfigureGridColumns();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing brand: {ex.Message}");
            }
        }

        private void ultraGrid1_ClickCell(object sender, Infragistics.Win.UltraWinGrid.ClickCellEventArgs e)
        {
            try
            {
                if (e.Cell != null && e.Cell.Row != null)
                {
                    Brand brd = new Brand();
                    brd.Id = Convert.ToInt32(e.Cell.Row.Cells["Id"].Value);
                    Brand brnd = operations.GetBrandId(brd);
                    Byte[] brandPhoto = new Byte[0];
                    if (brnd != null)
                    {
                        Id = brnd.Id;
                        ultraTextBrandName.Text = brnd.BrandName;
                        brandPhoto = brnd.Photo;
                        if (brnd.Photo.Length > 0)
                        {
                            MemoryStream ms = new MemoryStream(brandPhoto);
                            pictureBoxBrand.Image = Image.FromStream(ms);
                        }

                        // Switch to edit mode
                        SetButtonMode(true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting brand: {ex.Message}\n\nPlease try again or contact system administrator.", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error in ultraGrid1_ClickCell: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                brnd.Id = Id;
                brnd.BrandName = ultraTextBrandName.Text;
                if (!string.IsNullOrEmpty(openFileDialog1.FileName) && File.Exists(openFileDialog1.FileName))
                {
                    Byte[] brdPhoto = File.ReadAllBytes(openFileDialog1.FileName);
                    brnd.Photo = brdPhoto;
                }
                brnd._Operation = "Update";
                Brand message = operations.UpdateBrand(brnd);

                MessageBox.Show("Brand updated successfully!", "Update Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.RefreshBrand();

                // Clear form and reset to new entry mode
                ultraTextBrandName.Text = "";
                pictureBoxBrand.Image = null;
                SetButtonMode(false);
                Id = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating brand: {ex.Message}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error in btnUpdate_Click: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }
        }

        private void brnClear_Click(object sender, EventArgs e)
        {
            ultraTextBrandName.Text = "";
            pictureBoxBrand.Image = null;

            // Reset to new entry mode
            SetButtonMode(false);

            // Reset Id for new entry
            Id = 0;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (ultraGrid1.ActiveRow != null)
                {
                    int selectedId = Convert.ToInt32(ultraGrid1.ActiveRow.Cells["Id"].Value);
                    string brandName = ultraGrid1.ActiveRow.Cells["BrandName"].Value?.ToString() ?? "Unknown";

                    DialogResult result = MessageBox.Show(
                        $"Are you sure you want to delete the brand '{brandName}'?",
                        "Confirm Delete",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        brnd.Id = selectedId;
                        operations.DeleteBrand(brnd);

                        MessageBox.Show("Brand deleted successfully!", "Delete Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        this.RefreshBrand();

                        // Clear form and reset to new entry mode
                        ultraTextBrandName.Text = "";
                        pictureBoxBrand.Image = null;
                        SetButtonMode(false);
                        Id = 0;
                    }
                }
                else
                {
                    MessageBox.Show("Please select a brand to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting brand: {ex.Message}", "Delete Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error in btnDelete_Click: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }
        }



        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Brand brd = new Brand();
                brd.BrandName = ultraTextSearch.Text;

                BrandDDLGrid bddlg = new BrandDDLGrid();
                bddlg = operations.BrandSearch(brd);
                ultraGrid1.DataSource = bddlg.List;

                // Configure columns after search results are loaded
                ConfigureGridColumns();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in search: {ex.Message}");
            }
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBoxBrand.Load(openFileDialog1.FileName);
                string s = openFileDialog1.FileName;

                Byte[] bphoto = File.ReadAllBytes(openFileDialog1.FileName);
                string sphoto = Convert.ToBase64String(bphoto);

            }

        }

        public Byte[] imageToByteArray(Image imagein)
        {
            MemoryStream ms = new MemoryStream();
            imagein.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            return ms.ToArray();

        }

        private void FrmBrand_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void SetButtonMode(bool isEditMode)
        {
            if (isEditMode)
            {
                btnSave.Visible = false;
                btnUpdate.Visible = true;
            }
            else
            {
                btnSave.Visible = true;
                btnUpdate.Visible = false;
            }
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.ActiveCell = ultraGrid1.Rows[0].Cells[0];
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (ultraGrid1.Rows.Count > 0)
                {
                    ultraGrid1.ActiveRow = ultraGrid1.Rows[0];
                    ultraGrid1.ActiveCell = ultraGrid1.Rows[0].Cells[0];
                    ultraGrid1.Focus();
                }
            }
        }
    }
}
