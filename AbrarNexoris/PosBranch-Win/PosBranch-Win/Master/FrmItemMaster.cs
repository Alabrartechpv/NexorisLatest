using ModelClass;
using ModelClass.Master;
using Repository;
using Repository.MasterRepositry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win.Master
{
    public partial class FrmItemMaster : Form
    {
        Dropdowns dd = new Dropdowns();
        Item itm = new Item();
        ItemMasterPriceSettings itmprice = new ItemMasterPriceSettings();
        ClientOperations clientop = new ClientOperations();
        UnitMasterRepository unitmasterrepo = new UnitMasterRepository();
        ItemMasterRepository itmrepos = new ItemMasterRepository();
        int Id;
        public FrmItemMaster()
        {
            InitializeComponent();
        }

        public void UOMAddFormatGrid()
        {
            DataGridViewTextBoxColumn colUnit = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colPacking = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colBarcode = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colReorder = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colOpeningStock = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colHideIn = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colId1 = new DataGridViewTextBoxColumn();

            dgv_uomtab.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dgv_uomtab.Columns.Clear();
            dgv_uomtab.AllowUserToOrderColumns = true;
            dgv_uomtab.AllowUserToDeleteRows = false;
            dgv_uomtab.AllowUserToAddRows = false;
            dgv_uomtab.AlternatingRowsDefaultCellStyle.BackColor = SystemColors.Menu;
            dgv_uomtab.AutoGenerateColumns = false;
            dgv_uomtab.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dgv_uomtab.ColumnHeadersHeight = 35;
            dgv_uomtab.MultiSelect = false;
            dgv_uomtab.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            //colUnit.DataPropertyName = "Unit";
            //colUnit.HeaderText = "Unit";
            //colUnit.Name = "Unit";
            //colUnit.ReadOnly = false;
            //colUnit.Visible = true;
            //colUnit.Width = 100;
            //colUnit.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            //colUnit.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            //dgv_uomtab.Columns.Add(colUnit);

            DataGridViewComboBoxColumn comboBoxColumn = new DataGridViewComboBoxColumn();
            comboBoxColumn.DataPropertyName = "Unit";
            comboBoxColumn.HeaderText = "Unit";
            comboBoxColumn.Name = "Unit";
            comboBoxColumn.ReadOnly = false;
            comboBoxColumn.Visible = true;
            comboBoxColumn.Width = 100;
            comboBoxColumn.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            comboBoxColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            
            UnitDDlGrid grid = dd.getUnitDDl();
            comboBoxColumn.DataSource = grid.List;
            comboBoxColumn.DisplayMember = "UnitName";
            comboBoxColumn.ValueMember = "UnitID";
            dgv_uomtab.Columns.Add(comboBoxColumn);

            colPacking.DataPropertyName = "Packing";
            colPacking.HeaderText = "Packing";
            colPacking.Name = "Packing";
            colPacking.ReadOnly = false;
            colPacking.Visible = true;
            colPacking.Width = 100;
            colPacking.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colPacking.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_uomtab.Columns.Add(colPacking);

            colBarcode.DataPropertyName = "Barcode";
            colBarcode.HeaderText = "Barcode";
            colBarcode.Name = "Barcode";
            colBarcode.ReadOnly = false;
            colBarcode.Visible = true;
            colBarcode.Width = 100;
            colBarcode.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colBarcode.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_uomtab.Columns.Add(colBarcode);

            colReorder.DataPropertyName = "Reorder";
            colReorder.HeaderText = "Reorder";
            colReorder.Name = "Reorder";
            colReorder.ReadOnly = false;
            colReorder.Visible = true;
            colReorder.Width = 100;
            colReorder.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colReorder.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_uomtab.Columns.Add(colReorder);

            colOpeningStock.DataPropertyName = "OpeningStock";
            colOpeningStock.HeaderText = "OpeningStock";
            colOpeningStock.Name = "OpeningStock";
            colOpeningStock.ReadOnly = false;
            colOpeningStock.Visible = true;
            colOpeningStock.Width = 100;
            colOpeningStock.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colOpeningStock.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_uomtab.Columns.Add(colOpeningStock);

            DataGridViewButtonColumn buttonColAdd = new DataGridViewButtonColumn();
            buttonColAdd.Name = "Add";
            buttonColAdd.Text = "Add";
            buttonColAdd.UseColumnTextForButtonValue = true;
            //DataGridViewButtonColumn buttonColEdit = new DataGridViewButtonColumn();
            //buttonColEdit.Name = "Edit";
            //buttonColEdit.Text = "Edit";
            //buttonColEdit.UseColumnTextForButtonValue = true;
            DataGridViewButtonColumn buttonColDelete = new DataGridViewButtonColumn();
            buttonColDelete.Name = "Delete";
            buttonColDelete.Text = "Delete";
            buttonColDelete.UseColumnTextForButtonValue = true;

            dgv_uomtab.Columns.Add(buttonColAdd);
            //dgv_uomtab.Columns.Add(buttonColEdit);
            dgv_uomtab.Columns.Add(buttonColDelete);
        }

        private void PriceAddFormatGrid()
        {
            DataGridViewTextBoxColumn colPId = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colPUnit = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colPPacking = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colPCost = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colPMargin = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colPMarginPer = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colPMrp = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colPWholesalePrice = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colPRetail = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colPCredit = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colPCard = new DataGridViewTextBoxColumn();

            dgv_pricetab.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dgv_pricetab.Columns.Clear();
            dgv_pricetab.AllowUserToOrderColumns = true;
            dgv_pricetab.AllowUserToDeleteRows = false;
            dgv_pricetab.AllowUserToAddRows = false;
            dgv_pricetab.AlternatingRowsDefaultCellStyle.BackColor = SystemColors.Menu;
            dgv_pricetab.AutoGenerateColumns = false;
            dgv_pricetab.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dgv_pricetab.ColumnHeadersHeight = 35;
            dgv_pricetab.MultiSelect = false;
            dgv_pricetab.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            colPUnit.DataPropertyName = "Unit";
            colPUnit.HeaderText = "Unit";
            colPUnit.Name = "Unit";
            colPUnit.ReadOnly = false;
            colPUnit.Visible = true;
            colPUnit.Width = 70;
            colPUnit.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colPUnit.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_pricetab.Columns.Add(colPUnit);

            //DataGridViewComboBoxColumn comboBoxColumn = new DataGridViewComboBoxColumn();
            //comboBoxColumn.DataPropertyName = "Unit";
            //comboBoxColumn.HeaderText = "Unit";
            //comboBoxColumn.Name = "Unit";
            //comboBoxColumn.ReadOnly = false;
            //comboBoxColumn.Visible = true;
            //comboBoxColumn.Width = 100;
            //comboBoxColumn.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            //comboBoxColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            ////DataRow dr;
            ////DataTable dt = new DataTable();
            //UnitDDlGrid grid = dd.getUnitDDl();
            //comboBoxColumn.DataSource = grid.List;
            //comboBoxColumn.DisplayMember = "UnitName";
            //comboBoxColumn.ValueMember = "UnitID";
            //dgv_pricetab.Columns.Add(comboBoxColumn);

            colPPacking.DataPropertyName = "Packing";
            colPPacking.HeaderText = "Packing";
            colPPacking.Name = "Packing";
            colPPacking.ReadOnly = false;
            colPPacking.Visible = true;
            colPPacking.Width = 70;
            colPPacking.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colPPacking.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_pricetab.Columns.Add(colPPacking);

            colPCost.DataPropertyName = "Cost";
            colPCost.HeaderText = "Cost";
            colPCost.Name = "Cost";
            colPCost.ReadOnly = false;
            colPCost.Visible = true;
            colPCost.Width = 70;
            colPCost.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colPCost.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_pricetab.Columns.Add(colPCost);

            colPMargin.DataPropertyName = "Margin";
            colPMargin.HeaderText = "Margin";
            colPMargin.Name = "Margin";
            colPMargin.ReadOnly = false;
            colPMargin.Visible = true;
            colPMargin.Width = 70;
            colPMargin.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colPMargin.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_pricetab.Columns.Add(colPMargin);

            colPMarginPer.DataPropertyName = "MarginPer";
            colPMarginPer.HeaderText = "Margin(%)";
            colPMarginPer.Name = "MarginPer";
            colPMarginPer.ReadOnly = false;
            colPMarginPer.Visible = true;
            colPMarginPer.Width = 70;
            colPMarginPer.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colPMarginPer.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_pricetab.Columns.Add(colPMarginPer);

            colPMrp.DataPropertyName = "MRP";
            colPMrp.HeaderText = "MRP";
            colPMrp.Name = "MRP";
            colPMrp.ReadOnly = false;
            colPMrp.Visible = true;
            colPMrp.Width = 70;
            colPMrp.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colPMrp.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_pricetab.Columns.Add(colPMrp);

            colPRetail.DataPropertyName = "RetailPrice";
            colPRetail.HeaderText = "RetailPrice";
            colPRetail.Name = "RetailPrice";
            colPRetail.ReadOnly = false;
            colPRetail.Visible = true;
            colPRetail.Width = 70;
            colPRetail.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colPRetail.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_pricetab.Columns.Add(colPRetail);

            colPWholesalePrice.DataPropertyName = "WholesalePrice";
            colPWholesalePrice.HeaderText = "WholesalePrice";
            colPWholesalePrice.Name = "WholesalePrice";
            colPWholesalePrice.ReadOnly = false;
            colPWholesalePrice.Visible = true;
            colPWholesalePrice.Width = 70;
            colPWholesalePrice.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colPWholesalePrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_pricetab.Columns.Add(colPWholesalePrice);

            colPCredit.DataPropertyName = "CreditPrice";
            colPCredit.HeaderText = "CreditPrice";
            colPCredit.Name = "CreditPrice";
            colPCredit.ReadOnly = false;
            colPCredit.Visible = true;
            colPCredit.Width = 70;
            colPCredit.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colPCredit.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_pricetab.Columns.Add(colPCredit);

            colPCard.DataPropertyName = "CardPrice";
            colPCard.HeaderText = "CardPrice";
            colPCard.Name = "CardPrice";
            colPCard.ReadOnly = false;
            colPCard.Visible = true;
            colPCard.Width = 70;
            colPCard.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colPCard.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_pricetab.Columns.Add(colPCard);

            //DataGridViewButtonColumn buttonColEdit = new DataGridViewButtonColumn();
            //buttonColEdit.Name = "Edit";
            //buttonColEdit.Text = "Edit";
            //buttonColEdit.UseColumnTextForButtonValue = true;
            //DataGridViewButtonColumn buttonColDelete = new DataGridViewButtonColumn();
            //buttonColDelete.Name = "Delete";
            //buttonColDelete.Text = "Delete";
            //buttonColDelete.UseColumnTextForButtonValue = true;

            //dgv_pricetab.Columns.Add(buttonColEdit);
            //dgv_pricetab.Columns.Add(buttonColDelete);
        }

        private void ImageAddFormatGrid()
        {
            DataGridViewTextBoxColumn colIId = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colIUnit = new DataGridViewTextBoxColumn();

            dgv_Imagetab.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dgv_Imagetab.Columns.Clear();
            dgv_Imagetab.AllowUserToOrderColumns = true;
            dgv_Imagetab.AllowUserToDeleteRows = false;
            dgv_Imagetab.AllowUserToAddRows = false;
            dgv_Imagetab.AlternatingRowsDefaultCellStyle.BackColor = SystemColors.Menu;
            dgv_Imagetab.AutoGenerateColumns = false;
            dgv_Imagetab.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dgv_Imagetab.ColumnHeadersHeight = 35;
            dgv_Imagetab.MultiSelect = false;
            dgv_Imagetab.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            colIUnit.DataPropertyName = "Unit";
            colIUnit.HeaderText = "Unit";
            colIUnit.Name = "Unit";
            colIUnit.ReadOnly = false;
            colIUnit.Visible = true;
            colIUnit.Width = 70;
            colIUnit.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colIUnit.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_Imagetab.Columns.Add(colIUnit);

            //DataGridViewComboBoxColumn comboBoxColumn = new DataGridViewComboBoxColumn();
            //comboBoxColumn.DataPropertyName = "Unit";
            //comboBoxColumn.HeaderText = "Unit";
            //comboBoxColumn.Name = "Unit";
            //comboBoxColumn.ReadOnly = false;
            //comboBoxColumn.Visible = true;
            //comboBoxColumn.Width = 100;
            //comboBoxColumn.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            //comboBoxColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            ////DataRow dr;
            ////DataTable dt = new DataTable();
            //UnitDDlGrid grid = dd.getUnitDDl();
            //comboBoxColumn.DataSource = grid.List;
            //comboBoxColumn.DisplayMember = "UnitName";
            //comboBoxColumn.ValueMember = "UnitID";
            //dgv_Imagetab.Columns.Add(comboBoxColumn);

            DataGridViewImageColumn img = new DataGridViewImageColumn();
            img.HeaderText = "Choose file";
            img.ImageLayout = DataGridViewImageCellLayout.Stretch;
            dgv_Imagetab.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv_Imagetab.RowTemplate.Height = 60;
            dgv_Imagetab.Columns.Add(img);
        }

        private void ListFormatGrid()
        {
            DataGridViewTextBoxColumn colLId = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colLName = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colLGroup = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colLCategory = new DataGridViewTextBoxColumn();

            dgv_list_itemmaster.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dgv_list_itemmaster.Columns.Clear();
            dgv_list_itemmaster.AllowUserToOrderColumns = true;
            dgv_list_itemmaster.AllowUserToDeleteRows = false;
            dgv_list_itemmaster.AllowUserToAddRows = false;
            dgv_list_itemmaster.AlternatingRowsDefaultCellStyle.BackColor = SystemColors.Menu;
            dgv_list_itemmaster.AutoGenerateColumns = false;
            dgv_list_itemmaster.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dgv_list_itemmaster.ColumnHeadersHeight = 35;
            dgv_list_itemmaster.MultiSelect = false;
            dgv_list_itemmaster.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            colLId.DataPropertyName = "ItemId";
            colLId.HeaderText = "ItemId";
            colLId.Name = "ItemId";
            colLId.ReadOnly = false;
            colLId.Visible = true;
            colLId.Width = 70;
            colLId.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colLId.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_list_itemmaster.Columns.Add(colLId);

            colLName.DataPropertyName = "Description";
            colLName.HeaderText = "Name";
            colLName.Name = "Description";
            colLName.ReadOnly = false;
            colLName.Visible = true;
            colLName.Width = 250;
            colLName.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colLName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_list_itemmaster.Columns.Add(colLName);

            colLGroup.DataPropertyName = "GroupName";
            colLGroup.HeaderText = "Group";
            colLGroup.Name = "GroupName";
            colLGroup.ReadOnly = false;
            colLGroup.Visible = true;
            colLGroup.Width = 200;
            colLGroup.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colLGroup.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_list_itemmaster.Columns.Add(colLGroup);

            colLCategory.DataPropertyName = "CategoryName";
            colLCategory.HeaderText = "Category";
            colLCategory.Name = "CategoryName";
            colLCategory.ReadOnly = false;
            colLCategory.Visible = true;
            colLCategory.Width = 200;
            colLCategory.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colLCategory.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_list_itemmaster.Columns.Add(colLCategory);
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == add_tab)
            {
                this.UOMAddFormatGrid();
            }
            else if (tabControl2.SelectedTab == list_tab)
            {
                this.ListFormatGrid();
            }
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            saveMaster();
            MessageBox.Show("Successfully saved!");
        }

        private void saveMaster()
        {
            itm.CompanyId = SessionContext.CompanyId;
            itm.BranchId = SessionContext.BranchId;
            itm.FinYearId = SessionContext.FinYearId;

            itm.Description = txt_itemname.Text;
            itm.NameInLocalLanguage = txt_localname.Text;
            itm.ItemTypeId = Convert.ToInt32(cb_itemtype.GetItemText(cb_itemtype.SelectedValue));
            itm.ForCustomerType = cb_custtype.GetItemText(cb_custtype.SelectedValue);
            itm.BaseUnitId = Convert.ToInt32(cb_baseunit.GetItemText(cb_baseunit.SelectedValue));
            itm.GroupId = Convert.ToInt32(cb_group.GetItemText(cb_group.SelectedValue));
            itm.CategoryId = Convert.ToInt32(cb_category.GetItemText(cb_category.SelectedValue));
            itm.BrandId = Convert.ToInt32(cb_brand.GetItemText(cb_brand.SelectedValue));

            itmprice.CompanyId = SessionContext.CompanyId;
            itmprice.BranchId = SessionContext.BranchId;
            itmprice.FinYearId = SessionContext.FinYearId;

            itmprice.Cost = float.Parse(txt_unitcost.Text);
            itmprice.OrderedStock = float.Parse(txt_orderedstock.Text);
            itmprice.StockValue = float.Parse(txt_availablestock.Text);
            for (int i = 0; i < dgv_uomtab.Rows.Count; i++)
            {
                itmprice.Unit = dgv_uomtab.Rows[i].Cells["Unit"].Value.ToString();
                itmprice.Packing = float.Parse(dgv_uomtab.Rows[i].Cells["Packing"].Value.ToString());
                itmprice.BarCode = dgv_uomtab.Rows[i].Cells["BarCode"].Value.ToString();
                itmprice.ReOrder = float.Parse(dgv_uomtab.Rows[i].Cells["ReOrder"].Value.ToString());
                itmprice.OpnStk = float.Parse(dgv_uomtab.Rows[i].Cells["OpeningStock"].Value.ToString());

                itmprice.MRP = float.Parse(dgv_pricetab.Rows[i].Cells["MRP"].Value.ToString());
                itmprice.MarginAmt = float.Parse(dgv_pricetab.Rows[i].Cells["Margin"].Value.ToString());
                itmprice.MarginPer = float.Parse(dgv_pricetab.Rows[i].Cells["MarginPer"].Value.ToString());
                itmprice.RetailPrice = float.Parse(dgv_pricetab.Rows[i].Cells["RetailPrice"].Value.ToString());
                itmprice.WholeSalePrice = float.Parse(dgv_pricetab.Rows[i].Cells["WholesalePrice"].Value.ToString());
                itmprice.CreditPrice = float.Parse(dgv_pricetab.Rows[i].Cells["CreditPrice"].Value.ToString());
                itmprice.CardPrice = float.Parse(dgv_pricetab.Rows[i].Cells["CardPrice"].Value.ToString());
            }
            byte[] bytes = File.ReadAllBytes(openFileDialog1.FileName);
            itmprice.PhotoByteArray = bytes;

            itm._Operation = "CREATE";
            itmprice._Operation = "CREATE";
            string message = clientop.saveItem(itm, itmprice, dgv_pricetab, dgv_uomtab);
            MessageBox.Show(message);
        }

        private void btn_clear_Click(object sender, EventArgs e)
        {
            txt_itemname.Clear();
            txt_localname.Clear();
            cb_itemtype.SelectedValue = false;
            cb_custtype.SelectedValue = false;
            cb_baseunit.SelectedValue = false;
            txt_unitcost.Clear();
            txt_orderedstock.Clear();
            txt_availablestock.Clear();
            cb_group.SelectedValue = false;
            cb_category.SelectedValue = false;
            cb_brand.SelectedValue = false;
            btn_save.Visible = true;
            btn_update.Visible = false;
        }
        public void getGroup()
        {
            DataRow dr;
            DataTable dt = new DataTable();
            GroupDDlGrid grp_grid = dd.GroupDDl();
            cb_group.DataSource = grp_grid.List;
            cb_group.DisplayMember = "GroupName";
            cb_group.ValueMember = "Id";
        }
        public void getCategory()
        {
            DataRow dr;
            DataTable dt = new DataTable();
            CategoryDDlGrid cat_grid = dd.getCategoryDDl("");
            cb_category.DataSource = cat_grid.List;
            cb_category.DisplayMember = "CategoryName";
            cb_category.ValueMember = "Id";
        }

        public void getBrand()
        {
            // FOR AUTOCOMPLETE

            //txt_brand.AutoCompleteMode = AutoCompleteMode.Suggest;
            //txt_brand.AutoCompleteSource = AutoCompleteSource.CustomSource;
            //AutoCompleteStringCollection suggestions = new AutoCompleteStringCollection();
            //DataRow dr;
            //DataTable dt = new DataTable();
            //CategoryDDlGrid cat_grid = dd.itemDDlGrid();
            //txt_brand.DataBindings.Add(cat_grid);
            //suggestions.AddRange(new string[] { "Option1", "Option2", "Option3" });
            //txt_brand.AutoCompleteCustomSource = suggestions;

            DataRow dr;
            DataTable dt = new DataTable();
            BrandDDLGrid brand_grid = dd.getBrandDDl();
            cb_brand.DataSource = brand_grid.List;
            cb_brand.DisplayMember = "BrandName";
            cb_brand.ValueMember = "Id";
        }

        public void getItemType()
        {
            DataRow dr;
            DataTable dt = new DataTable();
            ItemTypeDDlGrid grid = dd.getItemTypeDDl();
            cb_itemtype.DataSource = grid.List;
            cb_itemtype.DisplayMember = "ItemType";
            cb_itemtype.ValueMember = "Id";
        }

        public void getCustomerType()
        {
            DataRow dr;
            DataTable dt = new DataTable();
            CustomerTypeDDlGrid grid = dd.getCustomerTypeDDl();
            cb_custtype.DataSource = grid.List;
            cb_custtype.DisplayMember = "PriceLevel";
            cb_custtype.ValueMember = "Id";
        }

        public void getBaseUnitType()
        {
            DataRow dr;
            DataTable dt = new DataTable();
            UnitDDlGrid grid = dd.getUnitDDl();
            cb_baseunit.DataSource = grid.List;
            cb_baseunit.DisplayMember = "UnitName";
            cb_baseunit.ValueMember = "UnitID";
        }

        public void getPacking()
        {
            int selectedId = Convert.ToInt32(cb_baseunit.SelectedValue.ToString());
            DataRow dr;
            DataTable dt = new DataTable();
            int Packing = unitmasterrepo.GetByIdPacking(selectedId);
            dgv_uomtab.Rows[0].Cells["Packing"].Value = Packing;
            dgv_pricetab.Rows[0].Cells["Packing"].Value = Packing;
        }

        private void FrmItemMaster_Load(object sender, EventArgs e)
        {
            this.UOMAddFormatGrid();
            this.PriceAddFormatGrid();
            this.ImageAddFormatGrid();
            this.btn_update.Visible = false;
            this.getGroup();
            this.getCategory();
            this.getBrand();
            this.getItemType();
            this.getCustomerType();
            this.getBaseUnitType();
            cb_itemtype.SelectedValue = false;
            cb_custtype.SelectedValue = false;
            cb_group.SelectedValue = false;
            cb_category.SelectedValue = false;
            cb_brand.SelectedValue = false;
            cb_baseunit.SelectedValue = false;
            this.List();
        }

        public void List()
        {
            ItemlistDDlGrid itemlistgrid = dd.ItemlistgridDDl();
            itemlistgrid.List.ToString();
            DataGridTableStyle ts1 = new DataGridTableStyle();
            DataGridColumnStyle datagrid = new DataGridBoolColumn();
            ts1.GridColumnStyles.Add(datagrid);
            dgv_list_itemmaster.DataSource = itemlistgrid.List;
        }

        private int rowCount = 0;
        private void dgv_add_itemmaster_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            rowCount++;

            if (e.ColumnIndex == 7)
            {
                int SelectedRows = e.RowIndex;
            }
            else
            {
                if (e.ColumnIndex == 6)
                {
                    int SelectedRows = dgv_uomtab.CurrentCell.RowIndex;
                    dgv_uomtab.Rows.RemoveAt(SelectedRows);
                    rowCount--;

                    int SelectedRows1 = dgv_pricetab.CurrentCell.RowIndex;
                    dgv_pricetab.Rows.RemoveAt(SelectedRows1);
                    rowCount--;

                    int SelectedRows2 = dgv_Imagetab.CurrentCell.RowIndex;
                    dgv_Imagetab.Rows.RemoveAt(SelectedRows2);
                    rowCount--;
                }
                else
                {
                    if (e.ColumnIndex == 5)
                    {
                        DataGridViewRow row = new DataGridViewRow();
                        row.CreateCells(dgv_uomtab);
                        UnitDDL unit = cb_baseunit.SelectedItem as UnitDDL;
                        dgv_uomtab.Rows[e.RowIndex].Cells["Unit"].Value = unit.UnitID;
                        dgv_uomtab.Rows.Add(row);

                        DataGridViewRow row1 = new DataGridViewRow();
                        row1.CreateCells(dgv_pricetab);
                        UnitDDL unit1 = cb_baseunit.SelectedItem as UnitDDL;
                        dgv_pricetab.Rows[e.RowIndex].Cells["Unit"].Value = unit1.UnitName;
                        dgv_pricetab.Rows.Add(row1);

                        DataGridViewRow row2 = new DataGridViewRow();
                        row2.CreateCells(dgv_Imagetab);
                        UnitDDL unit2 = cb_baseunit.SelectedItem as UnitDDL;
                        dgv_Imagetab.Rows[e.RowIndex].Cells["Unit"].Value = unit2.UnitName;
                        dgv_Imagetab.Rows.Add(row2);
                    }
                }
            }
        }

        private void txt_searchDescription_TextChanged(object sender, EventArgs e)
        {
            ItemlistDDl objItemddl = new ItemlistDDl();
            ItemlistDDlGrid grdListt = new ItemlistDDlGrid();
            objItemddl.Description = txt_searchDescription.Text;
            grdListt = dd.SearchItem(objItemddl);
            dgv_list_itemmaster.DataSource = grdListt.List;
        }

        private void dgv_list_itemmaster_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > 0)
            {
                int selectedId = (int)dgv_list_itemmaster.Rows[e.RowIndex].Cells["ItemId"].Value;
                Item item = clientop.GetByIdItem(selectedId);
                ItemMasterPriceSettings itemprice = clientop.GetByIdItemPrice(selectedId);
                if (item != null)
                {
                    Id = item.ItemId;
                    txt_itemname.Text = item.Description;
                    txt_localname.Text = item.NameInLocalLanguage;
                    cb_itemtype.SelectedValue = item.ItemTypeId;
                    cb_custtype.SelectedValue = Convert.ToInt32(item.ForCustomerType);
                    cb_baseunit.SelectedValue = item.BaseUnitId;
                    cb_group.SelectedValue = item.GroupId;
                    cb_category.SelectedValue = item.CategoryId;
                    cb_brand.SelectedValue = item.BrandId;

                    txt_unitcost.Text = Convert.ToString(itemprice.Cost);
                    txt_orderedstock.Text = Convert.ToString(itemprice.OrderedStock);
                    txt_availablestock.Text = Convert.ToString(itemprice.StockValue);

                    this.tabControl2.SelectedTab = add_tab;
                    this.btn_save.Visible = false;
                    btn_update.Visible = true;

                    dgv_uomtab.Rows.Add();
                    UnitDDL unit = cb_baseunit.SelectedItem as UnitDDL;
                    dgv_uomtab.Rows[0].Cells["Unit"].Value = unit.UnitID;
                    //dgv_uomtab.Rows[0].Cells["Unit"].Value = cb_baseunit.GetItemText(cb_baseunit.SelectedItem);
                    dgv_uomtab.Rows[0].Cells["Packing"].Value = itemprice.Packing;
                    dgv_uomtab.Rows[0].Cells["Barcode"].Value = itemprice.BarCode;
                    dgv_uomtab.Rows[0].Cells["ReOrder"].Value = itemprice.ReOrder;
                    dgv_uomtab.Rows[0].Cells["OpeningStock"].Value = itemprice.OpnStk;

                    dgv_pricetab.Rows.Add();
                    UnitDDL unit1 = cb_baseunit.SelectedItem as UnitDDL;
                    dgv_pricetab.Rows[0].Cells["Unit"].Value = unit1.UnitName;
                    //dgv_pricetab.Rows[0].Cells["Unit"].Value = cb_baseunit.GetItemText(cb_baseunit.SelectedItem);
                    dgv_pricetab.Rows[0].Cells["Packing"].Value = itemprice.Packing;
                    dgv_pricetab.Rows[0].Cells["Cost"].Value = itemprice.Cost;
                    dgv_pricetab.Rows[0].Cells["Margin"].Value = itemprice.MarginAmt;
                    dgv_pricetab.Rows[0].Cells["MarginPer"].Value = itemprice.MarginPer;
                    dgv_pricetab.Rows[0].Cells["MRP"].Value = itemprice.MRP;
                    dgv_pricetab.Rows[0].Cells["RetailPrice"].Value = itemprice.RetailPrice;
                    dgv_pricetab.Rows[0].Cells["WholesalePrice"].Value = itemprice.WholeSalePrice;
                    dgv_pricetab.Rows[0].Cells["CreditPrice"].Value = itemprice.CreditPrice;
                    dgv_pricetab.Rows[0].Cells["CardPrice"].Value = itemprice.CardPrice;

                    dgv_Imagetab.Rows.Add();
                    UnitDDL unit2 = cb_baseunit.SelectedItem as UnitDDL;
                    dgv_Imagetab.Rows[0].Cells["Unit"].Value = unit2.UnitName;
                    //dgv_Imagetab.Rows[0].Cells["Unit"].Value = cb_baseunit.GetItemText(cb_baseunit.SelectedItem);
                    dgv_Imagetab.Rows[0].Cells[""].Value = itemprice.Photo;
                }
            }
        }

        private void btn_update_Click(object sender, EventArgs e)
        {
            #region CODE FOR UPDATE MASTER
            ItemMasterPriceSettings itmp = new ItemMasterPriceSettings();
            DataBase.Operations = "UPDATE";
            Item itmm = new Item();

            itmm.ItemId = Id;
            itmm.Description = txt_itemname.Text;
            itmm.NameInLocalLanguage = txt_localname.Text;
            itmm.ItemTypeId = Convert.ToInt32(cb_itemtype.GetItemText(cb_itemtype.SelectedValue));
            itmm.ForCustomerType = cb_custtype.GetItemText(cb_custtype.SelectedValue);
            itmm.BaseUnitId = Convert.ToInt32(cb_baseunit.GetItemText(cb_baseunit.SelectedValue));
            itmm.GroupId = Convert.ToInt32(cb_group.GetItemText(cb_group.SelectedValue));
            itmm.CategoryId = Convert.ToInt32(cb_category.GetItemText(cb_category.SelectedValue));
            itmm.BrandId = Convert.ToInt32(cb_brand.GetItemText(cb_brand.SelectedValue));

            itmprice.CompanyId = SessionContext.CompanyId;
            itmprice.BranchId = SessionContext.BranchId;
            itmprice.FinYearId = SessionContext.FinYearId;

            //Item message = clientop.UpdateItem(itmm);
            //string message = clientop.UpdateFunc(itmm, itmp, dgv_uomtab, dgv_pricetab);

            #endregion


            #region DELETE PREVIOS RECORD FROM PRICE SETTINGS
            DataBase.Operations = "DELETE";

            //ItemMasterPriceSettings itmp = new ItemMasterPriceSettings();
            //string message1 = clientop.UpdateFunc(itmm, itmp, dgv_uomtab, dgv_pricetab);

            #endregion


            #region INSERT NEW RECORD TO PRICESETTINGS

            for (int i = 0; i < dgv_uomtab.Rows.Count; i++)
            {
                itmp.ItemId = Id;
                itmp.Unit = dgv_uomtab.Rows[i].Cells["Unit"].Value.ToString();
                itmp.UnitId = Convert.ToInt32(cb_baseunit.GetItemText(cb_baseunit.SelectedValue));
                itmp.Packing = float.Parse(dgv_uomtab.Rows[i].Cells["Packing"].Value.ToString());
                itmp.BarCode = dgv_uomtab.Rows[i].Cells["BarCode"].Value.ToString();
                itmp.ReOrder = float.Parse(dgv_uomtab.Rows[i].Cells["ReOrder"].Value.ToString());
                itmp.OpnStk = float.Parse(dgv_uomtab.Rows[i].Cells["OpeningStock"].Value.ToString());

                itmp.MarginAmt = float.Parse(dgv_pricetab.Rows[i].Cells["Margin"].Value.ToString());
                itmp.MarginPer = float.Parse(dgv_pricetab.Rows[i].Cells["MarginPer"].Value.ToString());
                itmp.MRP = float.Parse(dgv_pricetab.Rows[i].Cells["MRP"].Value.ToString());
                itmp.RetailPrice = float.Parse(dgv_pricetab.Rows[i].Cells["RetailPrice"].Value.ToString());
                itmp.WholeSalePrice = float.Parse(dgv_pricetab.Rows[i].Cells["WholesalePrice"].Value.ToString());
                itmp.CreditPrice = float.Parse(dgv_pricetab.Rows[i].Cells["CreditPrice"].Value.ToString());
                itmp.CardPrice = float.Parse(dgv_pricetab.Rows[i].Cells["CardPrice"].Value.ToString());

                itmp.Cost = float.Parse(txt_unitcost.Text);
                itmp.OrderedStock = double.Parse(txt_orderedstock.Text);
                itmp.StockValue = float.Parse(txt_availablestock.Text);

                //itmp.Photo = Encoding.UTF8.GetString(dgv_Imagetab.Rows[i].Cells[""].Value.ToString());

                //byte[] bytes1 =   Convert.FromBase64String(dgv_Imagetab.Rows[0].Cells[""].Value.ToString());
                //byte[] bytes = File.ReadAllBytes(openFileDialog1.FileName);
                //itmp.Photo = bytes; //Encoding.UTF8.GetBytes(dgv_Imagetab.Rows[0].Cells[""].Value.ToString()); 

                byte[] bytes = File.ReadAllBytes(openFileDialog1.FileName);
                itmp.PhotoByteArray = bytes;

                DataBase.Operations = "CREATE";
                //string message3 = clientop.UpdateFunc(itmm, itmp, dgv_uomtab, dgv_pricetab);
            }
            #endregion
            string message2 = clientop.UpdateFunc(itmm, itmp, dgv_uomtab, dgv_pricetab);
            MessageBox.Show("done");
            this.List();
        }

        private void dgv_Imagetab_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            rowCount++;

            if (e.ColumnIndex == 1)
            {
                int SelectedRows = e.RowIndex;
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        dgv_Imagetab.AllowUserToAddRows = false;
                        MemoryStream ms = new MemoryStream();
                        Image img;
                        img = Image.FromFile(openFileDialog1.FileName);
                        dgv_Imagetab.Rows[0].Cells[""].Value = img;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
            else
            {
                if (e.ColumnIndex == 3)
                {
                    int SelectedRows = dgv_Imagetab.CurrentCell.RowIndex;
                    dgv_Imagetab.Rows.RemoveAt(SelectedRows);
                    rowCount--;
                }
                else
                {

                    if (e.ColumnIndex == 2)
                    {

                        //DataGridViewRow row = new DataGridViewRow();
                        //row.CreateCells(dgv_Imagetab);
                        //row.Cells[0].Value = $"{rowCount}";
                        //dgv_Imagetab.Rows.Add(row);

                        MessageBox.Show("7th");
                    }
                }
            }
        }

        private void dgv_Imagetab_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                dgv_Imagetab.Rows.Add();
            }
        }

        private void dgv_pricetab_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            rowCount++;

            if (e.ColumnIndex == 7)
            {
                int SelectedRows = e.RowIndex;
            }
            else
            {
                if (e.ColumnIndex == 10)
                {
                    int SelectedRows = dgv_pricetab.CurrentCell.RowIndex;
                    dgv_pricetab.Rows.RemoveAt(SelectedRows);
                    rowCount--;
                }
                else
                {
                    if (e.ColumnIndex == 6)
                    {
                        DataGridViewRow row = new DataGridViewRow();
                        row.CreateCells(dgv_pricetab);
                        row.Cells[0].Value = $"{rowCount}";
                    }
                }
            }
        }

        private void cb_baseunit_SelectedIndexChanged(object sender, EventArgs e)
        {
            //string abcd =  cb_baseunit.GetItemText(cb_baseunit.SelectedItem);
            //if(cb_baseunit.SelectedIndex > 0)
            //{
            //    //DataGridViewRow row = new DataGridViewRow();
            //    //dgv_uomtab.Rows.Add(row);
            //    //DataGridViewRow row1 = new DataGridViewRow();
            //    //dgv_pricetab.Rows.Add(row1);
            //}
        }

        private void cb_baseunit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                DataGridViewRow row = new DataGridViewRow();
                dgv_uomtab.Rows.Add(row);
                DataGridViewRow row1 = new DataGridViewRow();
                dgv_pricetab.Rows.Add(row1);
            }
        }

        private void cb_baseunit_Click(object sender, EventArgs e)
        {
            // MessageBox.Show("sdfs");
        }

        private void cb_baseunit_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (cb_baseunit.SelectionLength >= 0)
            {
                this.UOMAddFormatGrid();
                DataGridViewRow row = new DataGridViewRow();
                dgv_uomtab.Rows.Add(row);
                if (dgv_uomtab.Rows.Count > 0)
                {
                    UnitDDL unit = cb_baseunit.SelectedItem as UnitDDL;
                    DataGridViewComboBoxColumn comboBoxColumn = new DataGridViewComboBoxColumn();
                    comboBoxColumn.DataPropertyName = "Unit";
                    comboBoxColumn.HeaderText = "Unit";
                    comboBoxColumn.Name = "Unit";
                    comboBoxColumn.ReadOnly = false;
                    comboBoxColumn.Visible = true;
                    comboBoxColumn.Width = 100;
                    comboBoxColumn.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    comboBoxColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    //DataRow dr;
                    //DataTable dt = new DataTable();
                    UnitDDlGrid grid = dd.getUnitDDl();
                    comboBoxColumn.DataSource = grid.List;
                    comboBoxColumn.DisplayMember = "UnitName";
                    comboBoxColumn.ValueMember = "UnitID";
                    
                    dgv_uomtab.Rows[0].Cells["Unit"].Value = unit.UnitID;

                }

                DataGridViewRow row1 = new DataGridViewRow();
                dgv_pricetab.Rows.Add(row1);
                if (dgv_pricetab.Rows.Count > 0)
                {
                    UnitDDL unit = cb_baseunit.SelectedItem as UnitDDL;
                    DataGridViewComboBoxColumn comboBoxColumn = new DataGridViewComboBoxColumn();
                    comboBoxColumn.DataPropertyName = "Unit";
                    comboBoxColumn.HeaderText = "Unit";
                    comboBoxColumn.Name = "Unit";
                    comboBoxColumn.ReadOnly = false;
                    comboBoxColumn.Visible = true;
                    comboBoxColumn.Width = 100;
                    comboBoxColumn.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    comboBoxColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    //DataRow dr;
                    //DataTable dt = new DataTable();
                    UnitDDlGrid grid = dd.getUnitDDl();
                    comboBoxColumn.DataSource = grid.List;
                    comboBoxColumn.DisplayMember = "UnitName";
                    comboBoxColumn.ValueMember = "UnitID";

                    dgv_pricetab.Rows[0].Cells["Unit"].Value = unit.UnitID;
                }

                DataGridViewRow row2 = new DataGridViewRow();
                dgv_Imagetab.Rows.Add(row2);
                if (dgv_Imagetab.Rows.Count > 0)
                {
                    UnitDDL unit = cb_baseunit.SelectedItem as UnitDDL;
                    DataGridViewComboBoxColumn comboBoxColumn = new DataGridViewComboBoxColumn();
                    comboBoxColumn.DataPropertyName = "Unit";
                    comboBoxColumn.HeaderText = "Unit";
                    comboBoxColumn.Name = "Unit";
                    comboBoxColumn.ReadOnly = false;
                    comboBoxColumn.Visible = true;
                    comboBoxColumn.Width = 100;
                    comboBoxColumn.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    comboBoxColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                    //DataRow dr;
                    //DataTable dt = new DataTable();
                    UnitDDlGrid grid = dd.getUnitDDl();
                    comboBoxColumn.DataSource = grid.List;
                    comboBoxColumn.DisplayMember = "UnitName";
                    comboBoxColumn.ValueMember = "UnitID";

                    dgv_Imagetab.Rows[0].Cells["Unit"].Value = unit.UnitID;
                }

                //DataGridViewRow row1 = new DataGridViewRow();
                //dgv_pricetab.Rows.Add(row1);
                //dgv_pricetab.Rows[0].Cells["Unit"].Value = cb_baseunit.GetItemText(cb_baseunit.SelectedItem);
                //this.getPacking();

                //DataGridViewRow row2 = new DataGridViewRow();
                //dgv_Imagetab.Rows.Add(row2);
                //dgv_Imagetab.Rows[0].Cells["Unit"].Value = cb_baseunit.GetItemText(cb_baseunit.SelectedItem);
            }
        }

        private void txt_unitcost_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string selectedcost = txt_unitcost.Text.ToString();
                int selectedId = Convert.ToInt32(cb_baseunit.SelectedValue.ToString());
                DataRow dr;
                DataTable dt = new DataTable();
                string cost = selectedcost.ToString();
                int Packing = unitmasterrepo.GetByIdPacking(selectedId);
                dgv_pricetab.Rows[0].Cells["Cost"].Value = Convert.ToInt32(cost) * Packing; 
                dgv_pricetab.Rows[0].Cells["Margin"].Value = 0;
                dgv_pricetab.Rows[0].Cells["MRP"].Value = 0;
                dgv_pricetab.Rows[0].Cells["RetailPrice"].Value = 0;
                dgv_pricetab.Rows[0].Cells["WholesalePrice"].Value = 0;
                dgv_pricetab.Rows[0].Cells["CreditPrice"].Value = 0;
                dgv_pricetab.Rows[0].Cells["CardPrice"].Value = 0;

                //float MRP = float.Parse(dgv_pricetab.Rows[0].Cells["MRP"].Value.ToString());
                //float MarginAmt = (Convert.ToInt32(cost) * Packing) - MRP;
                //dgv_pricetab.Rows[0].Cells["Margin"].Value = MarginAmt;
            }
        }

        private void dgv_pricetab_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (dgv_pricetab.Columns[e.ColumnIndex].Name == "MRP")
            {
                string selectedcost = txt_unitcost.Text.ToString();
                int selectedId = Convert.ToInt32(cb_baseunit.SelectedValue.ToString());
                string cost = selectedcost.ToString();
                int Packing = unitmasterrepo.GetByIdPacking(selectedId);
                float MRP = float.Parse(dgv_pricetab.Rows[0].Cells["MRP"].Value.ToString());
                float MarginAmt =  MRP - (Convert.ToInt32(cost) * Packing);
                dgv_pricetab.Rows[0].Cells["Margin"].Value = MarginAmt;

                float M1 = MRP - (Convert.ToInt32(cost) * Packing);
                float M2 = M1 / MRP;
                float MarPer = M2 * 100;
                dgv_pricetab.Rows[0].Cells["MarginPer"].Value = MarPer;

                dgv_pricetab.Rows[0].Cells["RetailPrice"].Value = MRP;
                dgv_pricetab.Rows[0].Cells["WholesalePrice"].Value = MRP;
                dgv_pricetab.Rows[0].Cells["CreditPrice"].Value = MRP;
                dgv_pricetab.Rows[0].Cells["CardPrice"].Value = MRP;
            }
            if (dgv_pricetab.Columns[e.ColumnIndex].Name == "MRP")
            {
                string selectedcost = txt_unitcost.Text.ToString();
                int selectedId = Convert.ToInt32(cb_baseunit.SelectedValue.ToString());
                string cost = selectedcost.ToString();
                int Packing = unitmasterrepo.GetByIdPacking(selectedId);
                float MRP = float.Parse(dgv_pricetab.Rows[0].Cells["MRP"].Value.ToString());
                float MarginAmt = MRP - (Convert.ToInt32(cost) * Packing);
                dgv_pricetab.Rows[0].Cells["Margin"].Value = MarginAmt;

                float M1 = MRP - (Convert.ToInt32(cost) * Packing);
                float M2 = M1 / MRP;
                float MarPer = M2 * 100;
                dgv_pricetab.Rows[0].Cells["MarginPer"].Value = MarPer;

                dgv_pricetab.Rows[0].Cells["RetailPrice"].Value = MRP;
                dgv_pricetab.Rows[0].Cells["WholesalePrice"].Value = MRP;
                dgv_pricetab.Rows[0].Cells["CreditPrice"].Value = MRP;
                dgv_pricetab.Rows[0].Cells["CardPrice"].Value = MRP;
            }
        }

        private void dgv_uomtab_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int selectedId = Convert.ToInt32(dgv_uomtab.Rows[e.RowIndex].Cells["Unit"].Value.ToString());
            UnitMaster unitDetails = unitmasterrepo.GetByIdUnit(selectedId);
            if (dgv_uomtab.Columns[e.ColumnIndex].Name == "Unit")
            {
                dgv_uomtab.Rows[e.RowIndex].Cells["Packing"].Value = unitDetails.Packing;
                if (e.RowIndex < dgv_pricetab.Rows.Count)
                {
                    dgv_pricetab.Rows[e.RowIndex].Cells["Unit"].Value = unitDetails.UnitName;
                    dgv_pricetab.Rows[e.RowIndex].Cells["Packing"].Value = unitDetails.Packing;
                }
                if (e.RowIndex < dgv_Imagetab.Rows.Count)
                {
                    dgv_Imagetab.Rows[e.RowIndex].Cells["Unit"].Value = unitDetails.UnitName;
                }
            }
        }
    }
}
//22-08-2024 - 03:31 By Sibi
