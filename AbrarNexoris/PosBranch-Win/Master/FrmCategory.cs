using ModelClass.Master;
using PosBranch_Win.DialogBox;
using Repository;
using Repository.MasterRepositry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Infragistics.Win.UltraWinGrid;

namespace PosBranch_Win.Master
{
    public partial class FrmCategory : Form
    {

        int Id;

        Dropdowns dd = new Dropdowns();
        Category categ = new Category();
        CategoryRepository clientop = new CategoryRepository();

        public FrmCategory()
        {
            InitializeComponent();
            btn_update.Visible = false;
        }

        private void btn_uploadfile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                picbx_uploadfile.Load(openFileDialog1.FileName);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (btn_save.Visible)
            {
                saveMaster();
                RefreshCategory();
            }
            else if (btn_update.Visible)
            {
                btn_update_Click(sender, e);
            }
        }

        private void btn_clear_Click(object sender, EventArgs e)
        {
            ultraTextCategoryName.Text = "";
            ultraComboGroupName.Value = null;
            picbx_uploadfile.Image = null;

            // Reset to new entry mode
            SetButtonMode(false);

            // Reset Id for new entry
            Id = 0;
        }

        private void FrmCategory_Load(object sender, EventArgs e)
        {
            this.FormatGrid();
            this.RefreshCategory();
            this.RefreshGroup();
            KeyPreview = true;
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

                    // CategoryName column
                    if (band.Columns.Exists("CategoryName"))
                    {
                        band.Columns["CategoryName"].Header.Caption = "Category Name";
                        band.Columns["CategoryName"].Width = 300;
                        band.Columns["CategoryName"].CellAppearance.TextHAlign = Infragistics.Win.HAlign.Left;
                        band.Columns["CategoryName"].CellActivation = Infragistics.Win.UltraWinGrid.Activation.NoEdit;
                        band.Columns["CategoryName"].MinWidth = 200;
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
                    ultraTextCategoryName.Text = row.Cells["CategoryName"].Value?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error displaying row values: {ex.Message}");
            }
        }

        public void RefreshCategory()
        {
            try
            {
                CategoryDDlGrid catgrid = dd.getCategoryDDl("");
                ultraGrid1.DataSource = catgrid.List;

                // Configure columns after data is loaded
                ConfigureGridColumns();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing category: {ex.Message}");
            }
        }

        private void saveMaster()
        {
            try
            {
                categ.CategoryName = ultraTextCategoryName.Text;
                categ.GroupId = Convert.ToInt32(ultraComboGroupName.Value);

                if (!string.IsNullOrEmpty(openFileDialog1.FileName) && File.Exists(openFileDialog1.FileName))
                {
                    byte[] bytes = File.ReadAllBytes(openFileDialog1.FileName);
                    categ.Photo = bytes;
                }

                categ._Operation = "CREATE";
                string message = clientop.SaveCategory(categ);

                // Show success dialog like other forms
                frmSuccesMsg msg = new frmSuccesMsg();
                msg.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving category: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void RefreshGroup()
        {
            try
            {
                GroupDDlGrid grid = dd.GroupDDl();
                ultraComboGroupName.DataSource = grid.List;
                ultraComboGroupName.DisplayMember = "GroupName";
                ultraComboGroupName.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing group: {ex.Message}");
            }
        }

        private void ultraGrid1_ClickCell(object sender, Infragistics.Win.UltraWinGrid.ClickCellEventArgs e)
        {
            try
            {
                if (e.Cell != null && e.Cell.Row != null)
                {
                    int selectedId = Convert.ToInt32(e.Cell.Row.Cells["Id"].Value);
                    Category categ = clientop.GetByIdCategory(selectedId);

                    if (categ != null)
                    {
                        Id = categ.Id;
                        ultraTextCategoryName.Text = categ.CategoryName;
                        ultraComboGroupName.Value = categ.GroupId;
                        Byte[] objpic = new byte[0];
                        objpic = categ.Photo;
                        if (categ.Photo != null)
                        {
                            if (categ.Photo.Length > 0)
                            {
                                picbx_uploadfile.Image = Image.FromStream(new MemoryStream(objpic));
                            }
                        }

                        // Switch to edit mode
                        SetButtonMode(true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting category: {ex.Message}\n\nPlease try again or contact system administrator.", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error in ultraGrid1_ClickCell: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }
        }

        private void btn_update_Click(object sender, EventArgs e)
        {
            try
            {
                categ.Id = Id;
                categ.CategoryName = ultraTextCategoryName.Text;
                categ.GroupId = Convert.ToInt32(ultraComboGroupName.Value);

                if (!string.IsNullOrEmpty(openFileDialog1.FileName) && File.Exists(openFileDialog1.FileName))
                {
                    byte[] bytes = File.ReadAllBytes(openFileDialog1.FileName);
                    categ.Photo = bytes;
                }

                categ._Operation = "UPDATE";
                Category message = clientop.UpdateCategory(categ);
                this.RefreshCategory();
                ultraTextCategoryName.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating category: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_delete_Click(object sender, EventArgs e)
        {
            try
            {
                if (Id == 0)
                {
                    MessageBox.Show("Please select a category to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Confirm deletion
                DialogResult result = MessageBox.Show("Are you sure you want to delete this category?", "Confirm Delete",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Call the repository to delete the category
                    Category deletedCategory = clientop.DeleteCategory(Id);

                    if (deletedCategory != null)
                    {
                        MessageBox.Show("Category deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Clear the form after successful deletion
                        btn_clear_Click(null, null);
                        btn_save.Visible = true;
                        btn_update.Visible = false;
                    }
                    else
                    {
                        MessageBox.Show("Error deleting category.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    this.RefreshCategory();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting category: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SetButtonMode(bool isEditMode)
        {
            if (isEditMode)
            {
                btn_save.Visible = false;
                btn_update.Visible = true;
            }
            else
            {
                btn_save.Visible = true;
                btn_update.Visible = false;
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                CategoryDDlGrid catgrid = dd.getCategoryDDl(ultraTextSearch.Text);
                ultraGrid1.DataSource = catgrid.List;

                // Configure columns after search results are loaded
                ConfigureGridColumns();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in search: {ex.Message}");
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
