using ModelClass.Master;
using PosBranch_Win.DialogBox;
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
    public partial class frmItemMasterNew : Form
    {
        /// <summary>
        /// Here column for dgv_Uom grid
        /// </summary>
        DataGridViewTextBoxColumn colUnit = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colUnitId = new DataGridViewTextBoxColumn();

        DataGridViewTextBoxColumn colPacking = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colBarcode = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colReorder = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colOpenStock = new DataGridViewTextBoxColumn();

        /// <summary>
        /// Here column for dgv_price
        /// </summary>
        DataGridViewTextBoxColumn colUnit4Price = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colPacking4Price = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colCost = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colMargin = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colMarginPer = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colMrp = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colWalking = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colWholeSale = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colCredit = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colCard = new DataGridViewTextBoxColumn();
       

        /// <summary>
        /// here column for dgv_tax
        /// </summary>
        /// 
        DataGridViewTextBoxColumn colTaxType = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colTaxPer = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colTaxAmt = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colUnitTax = new DataGridViewTextBoxColumn();


        Item ItemMaster = new Item();
        ItemMasterPriceSettings ItemPriceSettings = new ItemMasterPriceSettings();
        ItemMasterRepository ItemRepository = new ItemMasterRepository();
        public frmItemMasterNew()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeComponent();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            string Params = "ItemMasterGrid";
            frmUnitDialog unitDialog = new frmUnitDialog(Params);
            unitDialog.ShowDialog();
        }

        private void btn_ItemLoad_Click(object sender, EventArgs e)
        {
            string Params = "FromItemMaster";
            frmdialForItemMaster item = new frmdialForItemMaster(Params);
            item.ShowDialog();

        }

        private void frmItemMasterNew_Load(object sender, EventArgs e)
        {
            this.GetUOMdesing();
            this.GetPriceDesing();
          //  this.GetTaxDesing();
            this.GetImagesDesing();
            this.ActiveControl = txt_description;
        }
        private void GetUOMdesing() {
            dgv_Uom.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dgv_Uom.Columns.Clear();
            dgv_Uom.AllowUserToOrderColumns = true;
            dgv_Uom.AllowUserToDeleteRows = false;
            dgv_Uom.AllowUserToAddRows = false;
            dgv_Uom.AlternatingRowsDefaultCellStyle.BackColor = SystemColors.Menu;
            dgv_Uom.AutoGenerateColumns = false;
            dgv_Uom.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dgv_Uom.ColumnHeadersHeight = 70;
            dgv_Uom.MultiSelect = false;
            dgv_Uom.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            colUnit.DataPropertyName = "Unit";
            colUnit.HeaderText = "Unit";
            colUnit.Name = "Unit";
            colUnit.ReadOnly = false;
            colUnit.Visible = true;
            colUnit.Width = 120;
            colUnit.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colUnit.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_Uom.Columns.Add(colUnit);

            colUnitId.DataPropertyName = "UnitId";
            colUnitId.HeaderText = "UnitId";
            colUnitId.Name = "UnitId";
            colUnitId.ReadOnly = false;
            colUnitId.Visible = false;
            colUnitId.Width = 120;
            colUnitId.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colUnitId.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_Uom.Columns.Add(colUnitId);

            colPacking.DataPropertyName = "Packing";
            colPacking.HeaderText = "Packing";
            colPacking.Name = "Packing";
            colPacking.ReadOnly = false;
            colPacking.Visible = true;
            colPacking.Width = 120;
            colPacking.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colPacking.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_Uom.Columns.Add(colPacking);

            colBarcode.DataPropertyName = "BarCode";
            colBarcode.HeaderText = "Barcode";
            colBarcode.Name = "BarCode";
            colBarcode.ReadOnly = false;
            colBarcode.Visible = true;
            colBarcode.Width =180;
            colBarcode.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colBarcode.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_Uom.Columns.Add(colBarcode);

            colReorder.DataPropertyName = "Reorder";
            colReorder.HeaderText = "Reorder";
            colReorder.Name = "Reorder";
            colReorder.ReadOnly = false;
            colReorder.Visible = true;
            colReorder.Width = 110;
            colReorder.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colReorder.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_Uom.Columns.Add(colReorder);

            colOpenStock.DataPropertyName = "OpnStk";
            colOpenStock.HeaderText = "OpeningStock";
            colOpenStock.Name = "OpnStk";
            colOpenStock.ReadOnly = false;
            colOpenStock.Visible = true;
            colOpenStock.Width = 110;
            colOpenStock.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colOpenStock.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_Uom.Columns.Add(colOpenStock);

        }
        private void GetPriceDesing()
        {
            dgv_Price.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dgv_Price.Columns.Clear();
            dgv_Price.AllowUserToOrderColumns = true;
            dgv_Price.AllowUserToDeleteRows = false;
            dgv_Price.AllowUserToAddRows = false;
            dgv_Price.AlternatingRowsDefaultCellStyle.BackColor = SystemColors.Menu;
            dgv_Price.AutoGenerateColumns = false;
            dgv_Price.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dgv_Price.ColumnHeadersHeight = 70;
            dgv_Price.MultiSelect = false;
            dgv_Price.SelectionMode = DataGridViewSelectionMode.FullRowSelect;


            colUnit4Price.DataPropertyName = "Unit";
            colUnit4Price.HeaderText = "UnitName";
            colUnit4Price.Name = "Unit";
            colUnit4Price.ReadOnly = false;
            colUnit4Price.Visible = true;
            colUnit4Price.Width = 110;
            colUnit4Price.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colUnit4Price.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_Price.Columns.Add(colUnit4Price);

            colPacking4Price.DataPropertyName = "Packing";
            colPacking4Price.HeaderText = "Packing";
            colPacking4Price.Name = "Packing";
            colPacking4Price.ReadOnly = false;
            colPacking4Price.Visible = true;
            colPacking4Price.Width = 110;
            colPacking4Price.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colPacking4Price.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_Price.Columns.Add(colPacking4Price);

            colCost.DataPropertyName = "Cost";
            colCost.HeaderText = "Cost";
            colCost.Name = "Cost";
            colCost.ReadOnly = false;
            colCost.Visible = true;
            colCost.Width = 110;
            colCost.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colCost.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_Price.Columns.Add(colCost);

            colMargin.DataPropertyName = "MarginAmt";
            colMargin.HeaderText = "MarginAmt";
            colMargin.Name = "MarginAmt";
            colMargin.ReadOnly = false;
            colMargin.Visible = true;
            colMargin.Width = 110;
            colMargin.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colMargin.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_Price.Columns.Add(colMargin);

            colMarginPer.DataPropertyName = "MarginPer";
            colMarginPer.HeaderText = "MarginPer";
            colMarginPer.Name = "MarginPer";
            colMarginPer.ReadOnly = false;
            colMarginPer.Visible = true;
            colMarginPer.Width = 110;
            colMarginPer.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colMarginPer.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_Price.Columns.Add(colMarginPer);

            colTaxPer.DataPropertyName = "TaxPer";
            colTaxPer.HeaderText = "TaxPer";
            colTaxPer.Name = "TaxPer";
            colTaxPer.ReadOnly = false;
            colTaxPer.Visible = true;
            colTaxPer.Width = 110;
            colTaxPer.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colTaxPer.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_Price.Columns.Add(colTaxPer);

            colTaxAmt.DataPropertyName = "TaxAmt";
            colTaxAmt.HeaderText = "TaxAmt";
            colTaxAmt.Name = "TaxAmt";
            colTaxAmt.ReadOnly = false;
            colTaxAmt.Visible = true;
            colTaxAmt.Width = 110;
            colTaxAmt.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colTaxAmt.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_Price.Columns.Add(colTaxAmt);

            colMrp.DataPropertyName = "MRP";
            colMrp.HeaderText = "MRP";
            colMrp.Name = "MRP";
            colMrp.ReadOnly = false;
            colMrp.Visible = true;
            colMrp.Width = 110;
            colMrp.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colMrp.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_Price.Columns.Add(colMrp);

            colWalking.DataPropertyName = "RetailPrice";
            colWalking.HeaderText = "WalkingPrice";
            colWalking.Name = "RetailPrice";
            colWalking.ReadOnly = false;
            colWalking.Visible = true;
            colWalking.Width = 110;
            colWalking.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colWalking.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_Price.Columns.Add(colWalking);

            colWholeSale.DataPropertyName = "WholeSalePrice";
            colWholeSale.HeaderText = "RetailPrice";
            colWholeSale.Name = "WholeSalePrice";
            colWholeSale.ReadOnly = false;
            colWholeSale.Visible = true;
            colWholeSale.Width = 110;
            colWholeSale.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colWholeSale.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_Price.Columns.Add(colWholeSale);

            colCredit.DataPropertyName = "CreditPrice";
            colCredit.HeaderText = "CreditPrice";
            colCredit.Name = "CreditPrice";
            colCredit.ReadOnly = false;
            colCredit.Visible = true;
            colCredit.Width = 110;
            colCredit.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colCredit.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_Price.Columns.Add(colCredit);

            colCard.DataPropertyName = "CardPrice";
            colCard.HeaderText = "CardPrice";
            colCard.Name = "CardPrice";
            colCard.ReadOnly = false;
            colCard.Visible = true;
            colCard.Width = 110;
            colCard.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colCard.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv_Price.Columns.Add(colCard);

          


        }
        private void GetImagesDesing()
        {

        }

        private void btn_Add_ItemIype_Click(object sender, EventArgs e)
        {

            frmItemTypeDialog itemTypeDialog = new frmItemTypeDialog();
            itemTypeDialog.ShowDialog();
        }

        private void btn_Add_Cate_Click(object sender, EventArgs e)
        {
            frmCategoryDialog category = new frmCategoryDialog();
            category.ShowDialog();
        }

        private void btn_Add_Grup_Click(object sender, EventArgs e)
        {
            frmGroupDialog groupDialog = new frmGroupDialog();
            groupDialog.ShowDialog();
        }

        private void btn_Add_Brand_Click(object sender, EventArgs e)
        {
            frmBrandDialog brandDialog = new frmBrandDialog();
            brandDialog.ShowDialog();
        }

        private void btn_Add_Custm_Click(object sender, EventArgs e)
        {
            frmCustomerTypeDDl customTypedialog = new frmCustomerTypeDDl();
            customTypedialog.ShowDialog();
        }

        private void btn_BaseUnit_Click(object sender, EventArgs e)
        {
            string Params = "ItemMasterMaster";
            frmUnitDialog unitDialog = new frmUnitDialog(Params);
            unitDialog.ShowDialog();
        }

        private void txt_description_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void txt_description_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }

        private void txt_description_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                this.ActiveControl = txt_LocalLanguage;
            }
        }

        private void txt_LocalLanguage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.ActiveControl = btn_Add_ItemIype;
            }
        }

        private void txt_ItemType_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if(txt_ItemType.Text != "")
                {
                    this.ActiveControl = btn_Add_Cate;

                }
            }
        }

        private void txt_Category_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (txt_Category.Text != "")
                { 
                    this.ActiveControl = btn_Add_Grup;

                }
            }
        }

        private void txt_Group_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (txt_Group.Text != "")
                {
                    this.ActiveControl = btn_Add_Brand;

                }
            }
        }

        private void txt_Brand_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (txt_Brand.Text != "")
                {
                    this.ActiveControl = btn_Add_Custm;
                }
                   
            }
        }

        private void txt_BaseUnit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (txt_BaseUnit.Text != "")
                {
                    this.ActiveControl = btn_Add_UnitIm;
                }
                    
            }
        }

        private void Txt_UnitCost_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
               
            }
        }

        private void txt_CustomerType_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (txt_CustomerType.Text != "")
                {
                    this.ActiveControl = txt_BaseUnit;

                }

            }
        }

        private void Txt_UnitCost_TextChanged(object sender, EventArgs e)
        {
            //int count;
            //count = dgv_Price.Rows.Add();
         if(dgv_Price.Rows.Count > 0 && Txt_UnitCost.Text != "")
            {
                dgv_Price.Rows[0].Cells["Cost"].Value = Txt_UnitCost.Text; //ItemMaster.Txt_UnitCost.Text;
                dgv_Price.Rows[0].Cells["MRP"].Value = Txt_UnitCost.Text; //ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value;
                dgv_Price.Rows[0].Cells["RetailPrice"].Value = Txt_UnitCost.Text; //ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value;
                dgv_Price.Rows[0].Cells["WholeSalePrice"].Value = Txt_UnitCost.Text; //ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value;
                dgv_Price.Rows[0].Cells["CreditPrice"].Value = Txt_UnitCost.Text; //ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value;
                dgv_Price.Rows[0].Cells["CardPrice"].Value = Txt_UnitCost.Text;//ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value;                                                                         // float margin = float.Parse(ItemMaster.dgv_Price.Rows[count].Cells["MRP"].Value.ToString()) - float.Parse(ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value.ToString());
                dgv_Price.Rows[0].Cells["MarginAmt"].Value = 0; //margin.ToString();
                float marginAmt = float.Parse(dgv_Price.Rows[0].Cells["MRP"].Value.ToString()) - float.Parse(Txt_UnitCost.Text);  // float marginper = (float.Parse(ItemMaster.dgv_Price.Rows[count].Cells["MRP"].Value.ToString()) - float.Parse(ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value.ToString()));                                                                           //  marginper = margin / float.Parse(ItemMaster.dgv_Price.Rows[count].Cells["MRP"].Value.ToString()) * 100;
                float MarginPer = float.Parse(dgv_Price.Rows[0].Cells["MRP"].Value.ToString()) - float.Parse(Txt_UnitCost.Text);
                MarginPer = MarginPer / float.Parse(dgv_Price.Rows[0].Cells["MRP"].Value.ToString()) * 100;
                dgv_Price.Rows[0].Cells["MarginPer"].Value = MarginPer;
            }
            
        }

        private void dgv_Price_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if(dgv_Price.Rows.Count > 0)
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0) // e.ColumnIndex == 1 for second column
                {
                    if (Txt_UnitCost.Text != "")
                    {
                        if(dgv_Price.Rows[e.RowIndex].Cells["RetailPrice"].Value != null)
                        {
                            float Wlkin = float.Parse(dgv_Price.Rows[e.RowIndex].Cells["RetailPrice"].Value.ToString());
                            float Mrp = float.Parse(dgv_Price.Rows[e.RowIndex].Cells["MRP"].Value.ToString());
                            int packing = Convert.ToInt32(dgv_Price.Rows[0].Cells["Packing"].Value.ToString());
                            float Cost = float.Parse(dgv_Price.Rows[e.RowIndex].Cells["Cost"].Value.ToString());
                            float Margin = (Wlkin * packing) - Cost;
                            float marginPer = (Wlkin * packing);
                            marginPer = Margin / Mrp * 100;
                            marginPer = marginPer / Mrp;
                            if(dgv_Price.Rows[e.RowIndex].Cells["TaxPer"].Value == null)
                            {
                                dgv_Price.Rows[e.RowIndex].Cells["TaxPer"].Value = txt_TaxPer.Text;
                            }
                            float taxPer = float.Parse(dgv_Price.Rows[e.RowIndex].Cells["TaxPer"].Value.ToString());
                            float TaxAmt = Wlkin * taxPer / 100;
                            TaxAmt = TaxAmt + Wlkin;
                            dgv_Price.Rows[e.RowIndex].Cells["MarginAmt"].Value = Margin.ToString();
                            dgv_Price.Rows[e.RowIndex].Cells["MarginPer"].Value = marginPer.ToString();
                            dgv_Price.Rows[e.RowIndex].Cells["TaxAmt"].Value = TaxAmt.ToString();
                            txt_TaxAmount.Text = TaxAmt.ToString();

                        }
                        else if(dgv_Price.Rows[e.RowIndex].Cells["RetailPrice"].Value != null)
                        {
                            float Wlkin = float.Parse(dgv_Price.Rows[e.RowIndex].Cells["RetailPrice"].Value.ToString());
                            float Mrp = float.Parse(dgv_Price.Rows[e.RowIndex].Cells["MRP"].Value.ToString());
                            int packing = Convert.ToInt32(dgv_Price.Rows[0].Cells["Packing"].Value.ToString());
                            float Cost = float.Parse(dgv_Price.Rows[e.RowIndex].Cells["Cost"].Value.ToString());
                            float taxPer = float.Parse(dgv_Price.Rows[e.RowIndex].Cells["TaxPer"].Value.ToString());
                            float TaxAmt = Wlkin * taxPer / 100;
                            TaxAmt = TaxAmt + Wlkin;
                            float Margin = (Wlkin * packing) - Cost;
                            float marginPer = (Wlkin * packing);
                            marginPer = Margin / Mrp * 100;
                            marginPer = marginPer / Mrp;
                            dgv_Price.Rows[e.RowIndex].Cells["MarginAmt"].Value = Margin.ToString();
                            dgv_Price.Rows[e.RowIndex].Cells["MarginPer"].Value = marginPer.ToString();
                            dgv_Price.Rows[e.RowIndex].Cells["TaxAmt"].Value = TaxAmt.ToString();

                        }

                    }
                }


            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //using (OpenFileDialog openFileDialog = new OpenFileDialog())
            //{
                // Set filter for image files
                openFileDialog1.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                openFileDialog1.Title = "Select an Image";

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    // Get the path of the selected file
                    string filePath = openFileDialog1.FileName;

                    pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
                    // Load the image into the PictureBox
                    pictureBox1.Image = Image.FromFile(filePath);
                   
                    
                }
            //}
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.SaveMaster();
        }
        public void SaveMaster()
        {
            ItemMaster.Description = txt_description.Text;
            ItemMaster.NameInLocalLanguage = txt_LocalLanguage.Text;
            ItemMaster.ItemTypeId = Convert.ToInt32(lblItemTypeId.Text);
            ItemMaster.CategoryId = Convert.ToInt32(lblCategoryId.Text);
            ItemMaster.GroupId = Convert.ToInt32(lblGroupId.Text);
            ItemMaster.BrandId = Convert.ToInt32(lblBrandId.Text);
            ItemMaster.ForCustomerType = lblCustomerTypeID.Text;
            ItemMaster.BaseUnitId = Convert.ToInt32(lblBaseUnitId.Text);
            ItemMaster.VendorId = 0;
            ItemPriceSettings.TaxType = txt_TaxType.Text;
            ItemPriceSettings.TaxPer = float.Parse(txt_TaxPer.Text);
            ItemPriceSettings.TaxAmt = float.Parse(txt_TaxAmount.Text);

            
            ItemPriceSettings.PhotoByteArray = File.ReadAllBytes(openFileDialog1.FileName); ;

            string Message = ItemRepository.saveItemMaster(ItemMaster, ItemPriceSettings, dgv_Uom, dgv_Price);
            if(Message == "success")
            {
                frmSuccesMsg success = new frmSuccesMsg();
                success.ShowDialog();
                this.clear();
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            frmTaxTypeDialog frmTaxType = new frmTaxTypeDialog();
            frmTaxType.StartPosition = FormStartPosition.CenterScreen;
            frmTaxType.ShowDialog();
        }

        private void btn_Add_ItemIype_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (txt_CustomerType.Text != "")
            {
                this.ActiveControl = btn_Add_Cate;

            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            frmTaxPerDialog taxPer = new frmTaxPerDialog();
            taxPer.StartPosition = FormStartPosition.CenterScreen;
            taxPer.ShowDialog();
        }

        private void txt_TaxPer_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void txt_TaxPer_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }

        private void txt_TaxPer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if(dgv_Uom.Rows.Count > 0)
                {
                    for(int i=0;  dgv_Uom.Rows.Count > i; i++)
                    {
                        float taxPer = float.Parse(txt_TaxPer.Text);
                        float packing = float.Parse(dgv_Price.Rows[i].Cells["Packing"].Value.ToString());
                        dgv_Price.Rows[i].Cells["TaxPer"].Value = taxPer.ToString();

                    }
                }
            }
        }

        private void txt_TaxAmount_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (dgv_Uom.Rows.Count > 0)
                {
                    for (int i = 0; dgv_Uom.Rows.Count > i; i++)
                    {
                        float taxAmt = float.Parse(txt_TaxAmount.Text);
                        float packing = float.Parse(dgv_Price.Rows[i].Cells["Packing"].Value.ToString());
                        dgv_Price.Rows[i].Cells["TaxAmt"].Value = taxAmt.ToString();

                    }
                }
            }
        }

        public void clear()
        {
            txt_description.Clear();
            txt_LocalLanguage.Clear();
            txt_ItemType.Clear();
            txt_Group.Clear();
            txt_TaxAmount.Clear();
            txt_TaxPer.Clear();
            txt_TaxType.Clear();
            Txt_UnitCost.Clear();
            txt_BaseUnit.Clear();
            txt_Brand.Clear();
            txt_Category.Clear();
            txt_CustomerType.Clear();

            dgv_Price.Columns.Clear();
            dgv_Uom.Columns.Clear();
           
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.clear();
        }
    }
}
