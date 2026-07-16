using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.TransactionModels;
using PosBranch_Win.DialogBox;
using Repository;
using Repository.TransactionRepository;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace PosBranch_Win.Transaction
{
    public partial class frmSalesInvoice : Form
    {
        Dropdowns dp = new Dropdowns();
        SalesMaster sales = new SalesMaster();
        SalesDetails salesDetails = new SalesDetails();
        SalesRepository operations = new SalesRepository();
        private bool isCtrlPressed = false;
        private bool isf2Pressed = false;

        float SubTotal;
        float NetTotal;
        float DisTotal;
        bool CheckExists;
      

        DataGridViewTextBoxColumn colSlNo = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colItemId = new DataGridViewTextBoxColumn();

        DataGridViewTextBoxColumn colBarCode = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colDescriptionId = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colDescription = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colUnitId = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colUnit = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colIsBaseUnit = new DataGridViewTextBoxColumn();


        DataGridViewTextBoxColumn colBatchNo = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colExpiry = new DataGridViewTextBoxColumn();

        DataGridViewTextBoxColumn colQty = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colPacking = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colUnitPrice = new DataGridViewTextBoxColumn();

        DataGridViewTextBoxColumn colDiscPer = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colDiscAmt = new DataGridViewTextBoxColumn();

        DataGridViewTextBoxColumn colTaxPer = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colTaxAmt = new DataGridViewTextBoxColumn();

        DataGridViewTextBoxColumn colSalePrice = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colSSalePriceWithTax = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colAmountWithTax = new DataGridViewTextBoxColumn();

        DataGridViewTextBoxColumn colOldQty = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colBatchId = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colTaxType = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colCost = new DataGridViewTextBoxColumn();

        DataGridViewTextBoxColumn colMarginper = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colMarginAmt = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn colRetailPrice = new DataGridViewTextBoxColumn();



        public frmSalesInvoice()
        {
            InitializeComponent();
        }

        private void ultraPanel1_PaintClient(object sender, PaintEventArgs e)
        {

        }

        private void frmSalesInvoice_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        private void frmSalesInvoice_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void frmSalesInvoice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                isCtrlPressed = true;
            }
            if(e.KeyCode == Keys.F2)
            {
                isf2Pressed = true;
            }
            if(e.KeyCode == Keys.F11)
            {

            }
            else if (e.KeyCode == Keys.F5)
            {
                frmCustomerDialog cust = new frmCustomerDialog();
                cust.ShowDialog();
            }
            else if (e.KeyCode == Keys.F4)
            {

            } else if (e.KeyCode == Keys.F5)
            {

            } else if (e.KeyCode == Keys.F6)
            {
                bool isAdmin = string.Equals(SessionContext.UserLevel, "Administrator", StringComparison.OrdinalIgnoreCase);
                if (isAdmin)
                {
                    frmSalesPersonDial salesperson = new frmSalesPersonDial();
                    salesperson.ShowDialog();
                }
            }
            else if (e.KeyCode == Keys.F7)
            {
                frmBarcodeDialog barcode = new frmBarcodeDialog();
                barcode.ShowDialog();
            }
            else if (e.KeyCode == Keys.F11)
            {

            }
            else if (e.KeyCode == Keys.F4)
            {
                this.Close();
            }
            else if (e.KeyCode == Keys.F8)
            {                                 
                   
                this.HoldBill();

                //SaveMaster("");
            }
            else if (e.KeyCode == Keys.F1)
            {

            }
            else if (e.KeyCode == Keys.F12)
            {

            }
            else if(e.KeyCode == Keys.Oem3)
            {
                if(ChkSearch.Checked == true)
                {
                    ChkSearch.Checked = false;

                }
                else
                {
                    ChkSearch.Checked = true;
                }
            }
            else if (e.KeyCode == Keys.ControlKey && e.Modifiers == Keys.L)
            {
            }
            else if(isCtrlPressed == true && isf2Pressed == true)
            {
                if(lblBillNo.Text == "Billno")
                {
                    SaveMaster("");
                }
                else
                {
                    CompleteSale("");
                }
            }
            else if(e.KeyCode == Keys.Oem3)
            {
                txtBarcode.Clear();
                this.BarcodeFocuse();
                this.ChkSearch.Checked = true;               
            }
        }

        private void frmSalesInvoice_Load(object sender, EventArgs e)
        {
            KeyPreview = true;
            CustomerDDlGrid cs = dp.CustomerDDl();
            cs.List.ToString();
            var led = cs.List.Where(f => f.LedgerName == "DEFAULT CUSTOMER").FirstOrDefault();
            txtCustomer.Text = led.LedgerName;
            sales.LedgerID = led.LedgerID;
            lblledger.Text = led.LedgerID.ToString();
            txtSalesPerson.Text = DataBase.UserName;
            bool isAdmin = string.Equals(SessionContext.UserLevel, "Administrator", StringComparison.OrdinalIgnoreCase);
            txtSalesPerson.ReadOnly = !isAdmin;
            if (button2 != null)
            {
                button2.Enabled = isAdmin;
            }
            this.FormatGrid();
            DataTable dt = this.getPriceLevel();
           
            this.cmpPrice.DataSource = dt;
            this.cmpPrice.DisplayMember = "Name";
            this.cmpPrice.ValueMember = "ID";
            this.cmpPrice.SelectedIndex = 0;
            txtNetTotal.Text = Convert.ToString( 0);
            txtSubtotal.Text = Convert.ToString(0);
            PaymodeDDlGrid pm = dp.PaymodeDDl();
            pm.List.ToString();
            IEnumerable result = pm.List.AsEnumerable();
            cmbPaymt.DataSource = pm.List;
            cmbPaymt.DisplayMember = "PayModeName";  // Column to display
            cmbPaymt.ValueMember = "PayModeId";
            cmbPaymt.SelectedIndex = 1;
          
            pnlItem.Visible = false;
            //this.ActiveControl = txtBarcode;
            //txtBarcode.Focus();
            lblledger.Visible = false;
            this.BarcodeFocuse();



        }

        private void FormatGrid()
        {
            dgvItems.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dgvItems.Columns.Clear();
            dgvItems.AllowUserToOrderColumns = true;
            dgvItems.AllowUserToDeleteRows = false;
            dgvItems.AllowUserToAddRows = false;
            dgvItems.AlternatingRowsDefaultCellStyle.BackColor = SystemColors.Menu;
            dgvItems.AutoGenerateColumns = false;
            dgvItems.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dgvItems.ColumnHeadersHeight = 35;
            dgvItems.MultiSelect = false;
            dgvItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            colSlNo.DataPropertyName = "SlNO";
            colSlNo.HeaderText = "SlNO";
            colSlNo.Name = "SlNO";
            colSlNo.ReadOnly = false;
            colSlNo.Visible = true;
            colSlNo.Width = 50;
            colSlNo.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colSlNo.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItems.Columns.Add(colSlNo);

            colItemId.DataPropertyName = "ItemId";
            colItemId.HeaderText = "ItemId";
            colItemId.Name = "ItemId";
            colItemId.ReadOnly = false;
            colItemId.Visible = false;
            colItemId.Width = 50;
            colItemId.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colItemId.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItems.Columns.Add(colItemId);

            colBarCode.DataPropertyName = "BarCode";
            colBarCode.HeaderText = "BarCode";
            colBarCode.Name = "BarCode";
            colBarCode.ReadOnly = false;
            colBarCode.Visible = true;
            colBarCode.Width = 120;
            colBarCode.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colBarCode.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItems.Columns.Add(colBarCode);

            colDescriptionId.DataPropertyName = "DescriptionId";
            colDescriptionId.HeaderText = "DescriptionId";
            colDescriptionId.Name = "DescriptionId";
            colDescriptionId.ReadOnly = false;
            colDescriptionId.Visible = false;
            colDescriptionId.Width = 0;
            colDescriptionId.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colDescriptionId.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItems.Columns.Add(colDescriptionId);

            colDescription.DataPropertyName = "ItemName";
            colDescription.HeaderText = "ItemName";
            colDescription.Name = "ItemName";
            colDescription.ReadOnly = false;
            colDescription.Visible = true;
            colDescription.Width = 300;//250;//250 ;
            colDescription.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colDescription.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItems.Columns.Add(colDescription);

            colUnitId.DataPropertyName = "UnitId";
            colUnitId.HeaderText = "UnitId";
            colUnitId.Name = "UnitId";
            colUnitId.ReadOnly = false;
            colUnitId.Visible = false;
            colUnitId.Width = 0;
            colUnitId.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colUnitId.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItems.Columns.Add(colUnitId);

            colUnit.DataPropertyName = "Unit";
            colUnit.HeaderText = "Unit";
            colUnit.Name = "Unit";
            colUnit.ReadOnly = false;
            colUnit.Visible = true;
            colUnit.Width = 100;//50;
            colUnit.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colUnit.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItems.Columns.Add(colUnit);


            colIsBaseUnit.DataPropertyName = "IsBaseUnit";
            colIsBaseUnit.HeaderText = "IsBaseUnit";
            colIsBaseUnit.Name = "IsBaseUnit";
            colIsBaseUnit.ReadOnly = false;
            colIsBaseUnit.Visible = false;
            colIsBaseUnit.Width = 0;
            colIsBaseUnit.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colIsBaseUnit.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItems.Columns.Add(colIsBaseUnit);


            colBatchNo.DataPropertyName = "BatchNo";
            colBatchNo.HeaderText = "BatchNo";
            colBatchNo.Name = "BatchNo";
            colBatchNo.Visible = false;
            colBatchNo.Width = 0;
            colBatchNo.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colBatchNo.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItems.Columns.Add(colBatchNo);

            colExpiry.DataPropertyName = "Expiry";
            colExpiry.HeaderText = "Expiry";
            colExpiry.Name = "Expiry";
            colExpiry.Visible = false;
            colExpiry.Width = 0;//50;
            colExpiry.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colExpiry.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItems.Columns.Add(colExpiry);

            colQty.DataPropertyName = "Qty";
            colQty.HeaderText = "Qty";
            colQty.Name = "Qty";
            colQty.ReadOnly = false;
            colQty.Visible = true;
            colQty.Width = 50;//50;
            colQty.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colQty.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvItems.Columns.Add(colQty);

            colPacking.DataPropertyName = "Packing";
            colPacking.HeaderText = "Packing";
            colPacking.Name = "Packing";
            colPacking.ReadOnly = false;
            colPacking.Visible = false;
            colPacking.Width = 0;
            colPacking.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colPacking.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvItems.Columns.Add(colPacking);
            


            ////////////////////////////////////////////////////////////////
            colCost.DataPropertyName = "Cost";
            colCost.HeaderText = "Cost";
            colCost.Name = "Cost";
            colCost.Visible = true;
            colCost.Width = 40;
            colCost.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colCost.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItems.Columns.Add(colCost);
            ////////////////////////////////////////////////////////////////


            colUnitPrice.DataPropertyName = "UnitPrice";
            colUnitPrice.HeaderText = "UnitPrice";
            colUnitPrice.Name = "UnitPrice";
            colUnitPrice.ReadOnly = false;
            colUnitPrice.Visible = true;
            colUnitPrice.Width = 70;//80;
            colUnitPrice.DefaultCellStyle.Format = Actioncls.FormattedAmount(0, Actioncls.gNoOfDecimals);
            colUnitPrice.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            colUnitPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvItems.Columns.Add(colUnitPrice);

            colDiscPer.DataPropertyName = "DiscPer";
            colDiscPer.HeaderText = "DiscPer";
            colDiscPer.Name = "DiscPer";
            colDiscPer.ReadOnly = false;
            colDiscPer.Visible = true;
            colDiscPer.Width = 50;//80;
            colDiscPer.DefaultCellStyle.Format = Actioncls.FormattedAmount(0, Actioncls.gNoOfDecimals);
            colDiscPer.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            colDiscPer.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvItems.Columns.Add(colDiscPer);

            colDiscAmt.DataPropertyName = "DiscAmt";
            colDiscAmt.HeaderText = "DiscAmt";
            colDiscAmt.Name = "DiscAmt";
            colDiscAmt.ReadOnly = false;
            colDiscAmt.Visible = true;
            colDiscAmt.Width = 60;//65;//80;
            colDiscAmt.DefaultCellStyle.Format = Actioncls.FormattedAmount(0, Actioncls.gNoOfDecimals);
            colDiscAmt.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            colDiscAmt.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvItems.Columns.Add(colDiscAmt);

            colSalePrice.DataPropertyName = "Amount";
            colSalePrice.HeaderText = "S/Price";
            colSalePrice.Name = "Amount";
            colSalePrice.ReadOnly = false;
            colSalePrice.Visible = true;
            colSalePrice.Width = 80;//100;
            colSalePrice.DefaultCellStyle.Format = Actioncls.FormattedAmount(0, Actioncls.gNoOfDecimals);
            colSalePrice.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            colSalePrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colSalePrice.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvItems.Columns.Add(colSalePrice);

            colTaxPer.DataPropertyName = "TaxPer";
            colTaxPer.HeaderText = "TaxPer";
            colTaxPer.Name = "%";
            colTaxPer.ReadOnly = false;
            colTaxPer.Visible = false;
            colTaxPer.Width = 0;//50;//80;
            colTaxPer.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            colTaxPer.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvItems.Columns.Add(colTaxPer);

            colTaxAmt.DataPropertyName = "TaxAmt";
            colTaxAmt.HeaderText = "TaxAmt";
            colTaxAmt.Name = "TaxAmt";
            colTaxAmt.ReadOnly = false;
            colTaxAmt.Visible = false;
            colTaxAmt.Width = 0;//65;//80;
            colTaxAmt.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            colTaxAmt.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvItems.Columns.Add(colTaxAmt);

            colSSalePriceWithTax.DataPropertyName = "S/Price(Tax)";
            colSSalePriceWithTax.HeaderText = "S/Price(Tax)";
            colSSalePriceWithTax.Name = "S/Price(Tax)";
            colSSalePriceWithTax.ReadOnly = false;
            colSSalePriceWithTax.Visible = false;
            colSSalePriceWithTax.Width = 150;//80;
            colSSalePriceWithTax.DefaultCellStyle.Format = Actioncls.FormattedAmount(0, Actioncls.gNoOfDecimals);
            colSSalePriceWithTax.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            colSSalePriceWithTax.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvItems.Columns.Add(colSSalePriceWithTax);

            colAmountWithTax.DataPropertyName = "TotalAmount";
            colAmountWithTax.HeaderText = "NetAmount";// "Amount";
            colAmountWithTax.Name = "TotalAmount";
            colAmountWithTax.ReadOnly = false;
            colAmountWithTax.Visible = true;//false;
            colAmountWithTax.Width = 80;
            //colAmountWithTax.DefaultCellStyle.Format = MyClass.FormattedAmount(0, MyClass.gNoOfDecimals);
            colAmountWithTax.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            colAmountWithTax.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            colAmountWithTax.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvItems.Columns.Add(colAmountWithTax);

            colOldQty.DataPropertyName = "OldQty";
            colOldQty.HeaderText = "OldQty";
            colOldQty.Name = "OldQty";
            colOldQty.Visible = false;
            colOldQty.Width = 0;
            colOldQty.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colOldQty.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItems.Columns.Add(colOldQty);

            colBatchId.DataPropertyName = "BatchId";
            colBatchId.HeaderText = "BatchId";
            colBatchId.Name = "BatchId";
            colBatchId.Visible = false;
            colBatchId.Width = 0;
            colBatchId.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colBatchId.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItems.Columns.Add(colBatchId);


            colTaxType.DataPropertyName = "TaxType";
            colTaxType.HeaderText = "TaxType";
            colTaxType.Name = "TaxType";
            colTaxType.Visible = false;
            colTaxType.Width = 0;
            colTaxType.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colTaxType.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItems.Columns.Add(colTaxType);


            colMarginper.DataPropertyName = "Marginper";
            colMarginper.HeaderText = "Marginper";
            colMarginper.Name = "Marginper";
            colMarginper.Visible = false;
            colMarginper.Width = 0;
            colMarginper.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colMarginper.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItems.Columns.Add(colMarginper);


            colMarginAmt.DataPropertyName = "MarginAmt";
            colMarginAmt.HeaderText = "MarginAmt";
            colMarginAmt.Name = "MarginAmt";
            colMarginAmt.Visible = false;
            colMarginAmt.Width = 0;
            colMarginAmt.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colMarginAmt.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItems.Columns.Add(colMarginAmt);


           



        }

        public string mycontrols{
            get { return txtCustomer.Text; }
            }

        private void txtBarcode_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if(txtBarcode.Text.Length > 9)
                {
                    MessageBox.Show("ghsddf");
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            
         
        }

        private void txtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            //if(e.KeyCode == Keys.Enter && (txtBarcode.Text != string.Empty))
            //{
            //    MessageBox.Show("hdfjsdhf");
            //}
            //else
            //{
            //    MessageBox.Show("dfsdfdss");
            //}
        }

        private void dgvitemlist_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {

                ItemDDl item = new ItemDDl();
                UltraGridCell ItemId = this.dgvitemlist.ActiveRow.Cells["ItemId"];
                UltraGridCell BarCode = this.dgvitemlist.ActiveRow.Cells["BarCode"];
                UltraGridCell ItemName = this.dgvitemlist.ActiveRow.Cells["ItemName"];
                UltraGridCell Cost = this.dgvitemlist.ActiveRow.Cells["Cost"];
                UltraGridCell UnitId = this.dgvitemlist.ActiveRow.Cells["UnitId"];
                UltraGridCell Unit = this.dgvitemlist.ActiveRow.Cells["Unit"];
                UltraGridCell Packing = this.dgvitemlist.ActiveRow.Cells["Packing"];
                UltraGridCell Marginper = this.dgvitemlist.ActiveRow.Cells["Marginper"];
                UltraGridCell MarginAmt = this.dgvitemlist.ActiveRow.Cells["MarginAmt"];
                UltraGridCell TaxPer = this.dgvitemlist.ActiveRow.Cells["TaxPer"];
                UltraGridCell TaxAmt = this.dgvitemlist.ActiveRow.Cells["TaxAmt"];
                UltraGridCell RetailPrice = this.dgvitemlist.ActiveRow.Cells["RetailPrice"];
                UltraGridCell WholeSalePrice = this.dgvitemlist.ActiveRow.Cells["WholeSalePrice"];
                UltraGridCell CreditPrice = this.dgvitemlist.ActiveRow.Cells["CreditPrice"];
                UltraGridCell CardPrice = this.dgvitemlist.ActiveRow.Cells["CardPrice"];

                dgvItems.Focus();
                this.CheckData(BarCode.Value.ToString());
                int count;
                if (CheckExists == false)
                {
                    count = dgvItems.Rows.Add();
                    dgvItems.Rows[count].Cells["SlNO"].Value = dgvItems.Rows.Count;
                    dgvItems.Rows[count].Cells["BarCode"].Value = BarCode.Value.ToString();
                    dgvItems.Rows[count].Cells["ItemName"].Value = ItemName.Value.ToString();
                    dgvItems.Rows[count].Cells["Cost"].Value = Cost.Value.ToString();
                    dgvItems.Rows[count].Cells["UnitId"].Value = UnitId.Value.ToString();
                    dgvItems.Rows[count].Cells["Qty"].Value = 1;
                    dgvItems.Rows[count].Cells["Unit"].Value = Unit.Value.ToString();
                    if (cmpPrice.SelectedItem.ToString() == "RetailPrice")
                    {
                        dgvItems.Rows[count].Cells["UnitPrice"].Value = RetailPrice.Value.ToString();

                    }
                    else if (cmpPrice.SelectedItem.ToString() == "WholesalePrice")
                    {
                        dgvItems.Rows[count].Cells["UnitPrice"].Value = WholeSalePrice.Value.ToString();

                    }
                    else if (cmpPrice.SelectedItem.ToString() == "CreditPrice")
                    {
                        dgvItems.Rows[count].Cells["UnitPrice"].Value = CreditPrice.Value.ToString();

                    }
                    else if (cmpPrice.SelectedItem.ToString() == "CardPrice")
                    {
                        dgvItems.Rows[count].Cells["UnitPrice"].Value = CardPrice.Value.ToString();

                    }

                    dgvItems.Rows[count].Cells["DiscPer"].Value = 0;
                    dgvItems.Rows[count].Cells["DiscAmt"].Value = 0;
                    float qty = float.Parse(dgvItems.Rows[count].Cells["Qty"].Value.ToString());
                    float UnitPrice = float.Parse(dgvItems.Rows[count].Cells["UnitPrice"].Value.ToString());
                    dgvItems.Rows[count].Cells["S/Price"].Value = (qty * UnitPrice);
                    dgvItems.Rows[count].Cells["Amount"].Value = (qty * UnitPrice);

                    this.CalculateTotal();
                    this.BarcodeFocuse();
                    pnlItem.Visible = false;

                }
                else
                {
                    this.BarcodeFocuse();
                    this.CheckExists = false;


                }
            }
        }

        public void Print(Int64 BillNo)
        {
            billPara pr = new billPara();
            pr.BranchId =SessionContext.BranchId;
            ReportViewer rp = new ReportViewer();                                      
            rp.PrintBill(BillNo);
            this.Clear();
            

           // rp.Load("");
            
        }
        private void CalculateTotal()
        {
            SubTotal = 0;
            NetTotal = 0;

            for (int i = 0; i < dgvItems.Rows.Count; i++)
            {
                SubTotal += float.Parse(dgvItems.Rows[i].Cells["Amount"].Value.ToString());
                txtSubtotal.Text = SubTotal.ToString();
                NetTotal += float.Parse(dgvItems.Rows[i].Cells["Amount"].Value.ToString());
                txtNetTotal.Text = SubTotal.ToString();
                dgvItems.Rows[i].Cells[0].Selected = true;
                this.ActiveControl = txtBarcode;
                txtBarcode.Focus();
            }
        }
        private void pbxSave_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you Want to  Hold Bill", "Hold Bill",MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                // this.SaveMaster("");         
                //FrmSalesCmpt cmpt = new FrmSalesCmpt(txtNetTotal.Text);
                //cmpt.ShowDialog();
                this.HoldBill();
            }

                        
        }

        private void pbxSave_MouseHover(object sender, EventArgs e)
        {
            toolTip1.Show("Save", pbxSave);
        }

        private void ultraPictureBox1_MouseHover(object sender, EventArgs e)
        {
            toolTip1.Show("Clear", pbxSave);
        }

        private void ultraPictureBox2_MouseHover(object sender, EventArgs e)
        {
            toolTip1.Show("Delete", pbxSave);
        }

        private void pbxExit_MouseHover(object sender, EventArgs e)
        {
            toolTip1.Show("Exit", pbxSave);
        }

        private void ultraPictureBox3_MouseHover(object sender, EventArgs e)
        {
            toolTip1.Show("Last Bill", pbxSave);
        }

        private void pbxExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        public int SelectedIndex { get; set; }
        private DataTable getPriceLevel()
        {
            DataTable dt = new DataTable("PriceLevel");
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Name", typeof(string));

            dt.Rows.Add(new object[] { 1, "RetailPrice"});
            dt.Rows.Add(new object[] { 2, "WholesalePrice" });
            dt.Rows.Add(new object[] { 3, "CreditPrice"});
            dt.Rows.Add(new object[] { 4, "CardPrice"});
           
            return dt;

        }

        private void HoldBill()
        {
            DialogResult result = MessageBox.Show("Do you Want to  Hold Bill", "Hold Bill", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                sales.BranchId = SessionContext.BranchId;
                sales.CompanyId = SessionContext.CompanyId;
                sales.FinYearId = SessionContext.FinYearId;
                sales.BillDate = DateTime.Now;
                sales.CustomerName = txtCustomer.Text;
                sales.UserId = SessionContext.UserId;
                sales.EmpID = SessionContext.UserId;
                sales._Operation = "CREATE";


                sales.StateId = 1;
                sales.PaymodeName = cmbPaymt.GetItemText(cmbPaymt.SelectedItem);
                sales.PaymodeId = Convert.ToInt32(cmbPaymt.Value);
                sales.CustomerName = txtCustomer.Text;
                // sales.LedgerID = Convert.ToInt32(lblledger.Text);
                sales.NetAmount = double.Parse(txtNetTotal.Text);
                sales.SubTotal = double.Parse(txtSubtotal.Text);
                sales.SavedVia = "DESKTOP";
                sales.Status = "Hold";

                //  List<SalesDetails> list = dgvItems.ToString();
                for (int i = 0; i < dgvItems.Rows.Count; i++)
                {
                    salesDetails.ItemId = Convert.ToInt32(dgvItems.Rows[i].Cells["ItemId"].Value.ToString());
                    salesDetails.Barcode = dgvItems.Rows[i].Cells["BarCode"].Value.ToString();
                    salesDetails.CompanyId = SessionContext.CompanyId;
                    salesDetails.BranchID = SessionContext.BranchId;
                    salesDetails.FinYearId = SessionContext.FinYearId;
                    salesDetails.BillDate = DateTime.Now;
                    salesDetails.ItemName = dgvItems.Rows[i].Cells["ItemName"].Value.ToString();
                    salesDetails.SlNO = Convert.ToInt32(dgvItems.Rows[i].Cells["SlNO"].Value.ToString());
                    salesDetails.Unit = dgvItems.Rows[i].Cells["Unit"].Value.ToString();
                    salesDetails.UnitPrice = float.Parse(dgvItems.Rows[i].Cells["UnitPrice"].Value.ToString());
                    salesDetails.Cost = float.Parse(dgvItems.Rows[i].Cells["Cost"].Value.ToString());
                    salesDetails.DiscountAmount = float.Parse(dgvItems.Rows[i].Cells["DiscAmt"].Value.ToString());
                    salesDetails.DiscountPer = float.Parse(dgvItems.Rows[i].Cells["DiscPer"].Value.ToString());
                    salesDetails.Expiry = DateTime.Now;
                    salesDetails.MarginAmt = float.Parse(dgvItems.Rows[i].Cells["ItemId"].Value.ToString());
                    salesDetails.MarginPer = float.Parse(dgvItems.Rows[i].Cells["ItemId"].Value.ToString());
                    salesDetails.MRP = 0;
                    salesDetails.Packing = 1;
                    salesDetails.Qty = Convert.ToInt32(dgvItems.Rows[i].Cells["Qty"].Value.ToString());
                    salesDetails.TotalAmount = float.Parse(dgvItems.Rows[i].Cells["Amount"].Value.ToString());
                    salesDetails.BaseUnit = "";
                    salesDetails.Amount = 0;
                    salesDetails.VoucherId = 0;
                    salesDetails.CounterId = 0;
                    salesDetails.UnitId = 1;
                    salesDetails.TotalAmount = 0;





                    // salesDetails.ItemName = dgvItems.Rows[i].Cells["ItemName"].Value.ToString();
                    // salesDetails.RetailPrice =float.Parse( dgvItems.Rows[i].Cells["S/Price"].Value.ToString());

                }

                string message = operations.HoldSales(sales, salesDetails, dgvItems);
            }
            else
            {
                this.Clear();
            }

        }

        private void SaveMaster(string My)
        {
          
                sales.BranchId = SessionContext.BranchId;
                sales.CompanyId = SessionContext.CompanyId;
                sales.FinYearId = SessionContext.FinYearId;
                sales.BillDate = DateTime.Now;
                sales.CustomerName = txtCustomer.Text;
                sales.UserId = SessionContext.UserId;
                sales.EmpID = SessionContext.UserId;
                sales._Operation = "CREATE";


                sales.StateId = 1;
                sales.PaymodeName = cmbPaymt.GetItemText(cmbPaymt.SelectedItem);
                sales.PaymodeId = Convert.ToInt32(cmbPaymt.Value);
                sales.CustomerName = txtCustomer.Text;
                sales.LedgerID = Convert.ToInt32(lblledger.Text);
                sales.NetAmount = double.Parse(txtNetTotal.Text);
                sales.SubTotal = double.Parse(txtSubtotal.Text);
                sales.Status = "Complete";
                sales.SavedVia = "DESKTOP";
                //  List<SalesDetails> list = dgvItems.ToString();
                for (int i = 0; i < dgvItems.Rows.Count; i++)
                {
                    salesDetails.ItemId = Convert.ToInt32(dgvItems.Rows[i].Cells["ItemId"].Value.ToString());
                    salesDetails.Barcode = dgvItems.Rows[i].Cells["BarCode"].Value.ToString();
                    salesDetails.CompanyId = SessionContext.CompanyId;
                    salesDetails.BranchID = SessionContext.BranchId;
                    salesDetails.FinYearId = SessionContext.FinYearId;
                    salesDetails.BillDate = DateTime.Now;
                    salesDetails.ItemName = dgvItems.Rows[i].Cells["ItemName"].Value.ToString();
                    salesDetails.SlNO = Convert.ToInt32(dgvItems.Rows[i].Cells["SlNO"].Value.ToString());
                    salesDetails.Unit = dgvItems.Rows[i].Cells["Unit"].Value.ToString();
                    salesDetails.UnitPrice = float.Parse(dgvItems.Rows[i].Cells["UnitPrice"].Value.ToString());
                    salesDetails.Cost = float.Parse(dgvItems.Rows[i].Cells["Cost"].Value.ToString());
                    salesDetails.DiscountAmount = float.Parse(dgvItems.Rows[i].Cells["DiscAmt"].Value.ToString());
                    salesDetails.DiscountPer = float.Parse(dgvItems.Rows[i].Cells["DiscPer"].Value.ToString());
                    salesDetails.Expiry = DateTime.Now;
                    salesDetails.MarginAmt = float.Parse(dgvItems.Rows[i].Cells["ItemId"].Value.ToString());
                    salesDetails.MarginPer = float.Parse(dgvItems.Rows[i].Cells["ItemId"].Value.ToString());
                    salesDetails.MRP = 0;
                    salesDetails.Packing = 1;
                    salesDetails.Qty = Convert.ToInt32(dgvItems.Rows[i].Cells["Qty"].Value.ToString());
                    salesDetails.TotalAmount = float.Parse(dgvItems.Rows[i].Cells["Amount"].Value.ToString());
                    salesDetails.BaseUnit = "";
                    salesDetails.Amount = float.Parse(dgvItems.Rows[i].Cells["Amount"].Value.ToString());
                    salesDetails.VoucherId = 0;
                    salesDetails.CounterId = 0;
                    salesDetails.UnitId = 1;
                    salesDetails.TotalAmount = 0;
                    // salesDetails.ItemName = dgvItems.Rows[i].Cells["ItemName"].Value.ToString();
                    // salesDetails.RetailPrice =float.Parse( dgvItems.Rows[i].Cells["S/Price"].Value.ToString());

                }

                string message = operations.SaveSales(sales, salesDetails, dgvItems);

                if (message != null)
                {
                    DialogResult result = MessageBox.Show("Do you Want Print", "Print", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        this.Print(Convert.ToInt64(message));
                    }
                    else
                    {
                        // Do something
                    }
                
            }

            //MessageBox.Show(My);
        }

        private void CompleteSale(string status)
        {
            SalesMaster sales = new SalesMaster();
            sales.BillNo =  Convert.ToInt64(lblBillNo.Text.ToString());
            string message = operations.CompleteSale(sales);
        }
        public void BarcodeFocuse()
        {
            this.ActiveControl = txtBarcode;
            txtBarcode.Clear();
            txtBarcode.Text = "";
            txtBarcode.Focus();
        }

        public void  CheckData(string Barcode)
        {
            if(dgvItems.Rows.Count > 0)
            {
                for(int i =0; i < dgvItems.Rows.Count; i++)
                {
                    if(dgvItems.Rows[i].Cells["BarCode"].Value.ToString() == Barcode)
                    {
                        CheckExists = true;

                        int qty = Convert.ToInt32(dgvItems.Rows[i].Cells["Qty"].Value.ToString());
                        dgvItems.Rows[i].Cells["Qty"].Value = qty + 1;
                        // float qty = float.Parse(dgvItems.Rows[count].Cells["Qty"].Value.ToString());
                        float UnitPrice = float.Parse(dgvItems.Rows[i].Cells["UnitPrice"].Value.ToString());
                        dgvItems.Rows[i].Cells["Amount"].Value = (Convert.ToInt32(dgvItems.Rows[i].Cells["Qty"].Value) * UnitPrice);
                        dgvItems.Rows[i].Cells["TotalAmount"].Value = (Convert.ToInt32(dgvItems.Rows[i].Cells["Qty"].Value) * UnitPrice);
                        this.CalculateTotal();
                        this.BarcodeFocuse();
                        pnlItem.Visible = false;
                    }

                }
            }
            
        }

        public void Clear()
        {
            if (dgvItems.Rows.Count > 0)
            {
                dgvItems.Rows.Clear();
                txtNetTotal.Clear();
                txtSubtotal.Clear();
            }
        }

        private void txtBarcode_KeyUp(object sender, KeyEventArgs e)
        {
            if (ChkSearch.Checked == true)
            {
                if (txtBarcode.Text.Length > 3)
                { 
                    if (e.KeyCode == Keys.Enter)
                    {
                        pnlItem.Visible = true;
                        DataBase.Operations = "GETITEM";
                        var item = dp.itemDDlGrid("", txtBarcode.Text);
                        dgvitemlist.DataSource = item.List;
                    }
                       
                }
                    
            }
            else
            {
                if(e.KeyCode == Keys.Enter)
                {
                    
                }
            }
        }

        private void txtBarcode_KeyPress(object sender, KeyPressEventArgs e)
        {
              
            
        }

        private void cmbPaymt_ValueChanged(object sender, EventArgs e)
        {
            sales.PaymodeName = cmbPaymt.GetItemText(cmbPaymt.SelectedItem);
            sales.PaymodeId = Convert.ToInt32( cmbPaymt.Value);

        }

        private void dgvItems_KeyUp(object sender, KeyEventArgs e)
        {

        }


        private void dgvItems_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
           

                if (dgvItems.Rows.Count > 0) 
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0) // e.ColumnIndex == 1 for second column
                {
                    if(dgvItems.Rows[e.RowIndex].Cells["Qty"].Value != null && dgvItems.Rows[e.RowIndex].Cells["UnitPrice"].Value != null)
                    {
                        float Qty = float.Parse(dgvItems.Rows[e.RowIndex].Cells["Qty"].Value.ToString());
                        float Price = float.Parse(dgvItems.Rows[e.RowIndex].Cells["UnitPrice"].Value.ToString());
                        float Total = Qty * Price;
                        dgvItems.Rows[e.RowIndex].Cells["Amount"].Value = Total;
                        dgvItems.Rows[e.RowIndex].Cells["TotalAmount"].Value = Total;

                        txtSubtotal.Text = Total.ToString();
                        txtNetTotal.Text = Total.ToString();

                        this.CalculateTotal();
                    }
                    

                   
                }
            }
            
        }

        private void ultraPictureBox1_Click(object sender, EventArgs e)
        {
            dgvItems.Rows.Clear();

        }

        private void dgvItems_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void dgvItems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void dgvItems_Enter(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            FrmDialogSHold hold = new FrmDialogSHold();
            hold.ShowDialog();
        }

        private void ultraPictureBox3_Click(object sender, EventArgs e)
        {
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)   
        {
            int index = 0;
            if (e.KeyCode == Keys.Enter && txtBarcode.Text != string.Empty)
            {
                if (txtBarcode.Text.Contains('*'))
                {
                    string[] values = txtBarcode.Text.Split('*');
                    index = Convert.ToInt32(values[1].ToString());
                    dgvItems.Rows[index - 1].Selected = true;
                    dgvItems.CurrentCell = dgvItems.Rows[index - 1].Cells["Qty"];

                    // Optionally begin editing
                    dgvItems.BeginEdit(true);
                    txtBarcode.Text = "";

                } else if (txtBarcode.Text.Contains('-'))
                {
                    string[] values = txtBarcode.Text.Split('-');
                    index = Convert.ToInt32(values[1].ToString());
                    dgvItems.Rows[index - 1].Selected = true;
                    dgvItems.Rows.RemoveAt(index - 1);
                    txtBarcode.Text = "";
                    this.CalculateTotal();
                }
                else
                {
                    DataBase.Operations = "GETITEMBYBARCODE";
                    ItemDDlGrid item = dp.itemDDlGrid(txtBarcode.Text, "");
                    if (item.List.Count() == 1)
                    {
                        this.CheckData(txtBarcode.Text);
                        if (CheckExists == false)
                        {
                            int count;
                            count = dgvItems.Rows.Add();
                            dgvItems.Rows[count].Cells["SlNO"].Value = dgvItems.Rows.Count;

                            foreach (var Ite in item.List)
                            {
                                dgvItems.Rows[count].Cells["ItemId"].Value = Ite.ItemId.ToString();
                                dgvItems.Rows[count].Cells["BarCode"].Value = Ite.BarCode.ToString();
                                dgvItems.Rows[count].Cells["ItemName"].Value = Ite.Description.ToString();
                                dgvItems.Rows[count].Cells["Cost"].Value = Ite.Cost.ToString();
                                dgvItems.Rows[count].Cells["UnitId"].Value = Ite.UnitId.ToString();
                                dgvItems.Rows[count].Cells["Qty"].Value = 1;
                                dgvItems.Rows[count].Cells["Unit"].Value = Ite.Unit.ToString();
                                if (cmpPrice.SelectedItem.ToString() == "RetailPrice")
                                {
                                    dgvItems.Rows[count].Cells["UnitPrice"].Value = Ite.RetailPrice.ToString(); ;

                                }
                                else if (cmpPrice.SelectedItem.ToString() == "WholesalePrice")
                                {
                                    dgvItems.Rows[count].Cells["UnitPrice"].Value = Ite.WholeSalePrice.ToString(); ;

                                }
                                else if (cmpPrice.SelectedItem.ToString() == "CreditPrice")
                                {
                                    dgvItems.Rows[count].Cells["UnitPrice"].Value = Ite.CreditPrice.ToString(); ;

                                }
                                else if (cmpPrice.SelectedItem.ToString() == "CardPrice")
                                {
                                    dgvItems.Rows[count].Cells["UnitPrice"].Value = Ite.CardPrice.ToString(); ;

                                }
                                dgvItems.Rows[count].Cells["DiscPer"].Value = 0;
                                dgvItems.Rows[count].Cells["DiscAmt"].Value = 0;
                                float qty1 = float.Parse(dgvItems.Rows[count].Cells["Qty"].Value.ToString());
                                float UnitPrice = float.Parse(dgvItems.Rows[count].Cells["UnitPrice"].Value.ToString());
                                dgvItems.Rows[count].Cells["Amount"].Value = (qty1 * UnitPrice);
                                dgvItems.Rows[count].Cells["TotalAmount"].Value = (qty1 * UnitPrice);
                            }

                        }
                        CheckExists = false;
                        txtBarcode.Clear();
                        this.BarcodeFocuse();
                        dgvItems.Rows[dgvItems.Rows.Count - 1].Selected = true;
                    }
                }
                
            }
            else if (e.KeyCode == Keys.Space)
            {
                //DataBase.Operations = "GETITEMBYBARCODENAME";
                //ItemDDlGrid item = dp.itemDDlGrid(txtBarcode.Text, "");
                txtItemNameSearch.Focus();
                DataBase.Operations = "GETALL";
                ItemDDlGrid item = dp.itemDDlGrid();
                if (item.List != null)
                {
                    pnlItem.Visible = true;
                    txtItemNameSearch.Focus();
                    dgvitemlist.DataSource = item.List;
                    this.HideColumn();
                }

            } 
                
        }
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            

        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void txtItemNameSearch_TextChanged(object sender, EventArgs e)
        {
            if(ChkSearch.Checked == false)
            {
                DataBase.Operations = "GETITEMBYBARCODENAME";
                ItemDDlGrid item = dp.itemDDlGrid(txtItemNameSearch.Text, txtItemNameSearch.Text);
                if (item.List != null)
                {
                    dgvitemlist.DataSource = item.List;
                   // this.AddingColumToUltraGrid();
                    this.HideColumn();
                }
            }
            else
            {
                
                DataBase.Operations = "GETITEM";
                ItemDDlGrid item = dp.itemDDlGrid(txtItemNameSearch.Text, txtItemNameSearch.Text);
                if (item.List != null)
                {
                    dgvitemlist.DataSource = item.List;
                    //this.AddingColumToUltraGrid();
                    this.HideColumn();

                }

            }
           
        }

        private void txtItemNameSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Down)
            {
                dgvitemlist.Rows[0].Selected = true;
                dgvitemlist.Focus();
            }
        }

        private void dgvitemlist_InitializeLayout(object sender, InitializeLayoutEventArgs e)
        {
        }
        private void AddingColumToUltraGrid()
        {
            var band = dgvitemlist.DisplayLayout.Bands[0];
            UltraGridColumn newColumn = band.Columns.Add("BarCode", "Barcode");

            // Set properties for the new column (optional)
            newColumn.Width = 100;
            newColumn.Header.Caption = "BarCode";
           // newColumn.Format = "N2"; // Numeric format, if applicable

            // Optionally, set other properties
            newColumn.CellActivation = Activation.NoEdit; // Make the cell read-only
            newColumn.CellAppearance.BackColor = System.Drawing.Color.LightGray;

         
        }
        private void HideColumn()
        {
            UltraGridBand band = dgvitemlist.DisplayLayout.Bands[0];
            foreach (UltraGridColumn column in band.Columns)
            {
                // Example condition: Hide columns with names starting with "Temp"
                if (column.Key.StartsWith("ItemId"))
                {
                    column.Hidden = true;
                }
                if (column.Key.StartsWith("UnitId"))
                {
                    column.Hidden = true;
                }
                if (column.Key.StartsWith("TaxPer"))
                {
                    column.Hidden = true;
                }
                if (column.Key.StartsWith("TaxAmt"))
                {
                    column.Hidden = true;
                }
                if (column.Key.StartsWith("CardPrice"))
                {
                    column.Hidden = true;
                }
                if (column.Key.StartsWith("WholeSalePrice"))
                {
                    column.Hidden = true;
                }
                if (column.Key.StartsWith("ItemName"))
                {
                    column.Width = 200;
                }
                if (column.Key.StartsWith("MarginAmt"))
                {
                    column.Hidden = true;
                }
                if (column.Key.StartsWith("MarginPer"))
                {
                    column.Hidden = true;
                }
            }

        }

        private void ChkSearch_CheckedChanged(object sender, EventArgs e)
        {

        }

        public void GetMyBill(SalesMaster master)
        {
            txtNetTotal.Text = master.NetAmount.ToString();
            txtSubtotal.Text = master.SubTotal.ToString();

        }

        private void textBox5_KeyUp(object sender, KeyEventArgs e)
        {
           
            if(e.KeyCode == Keys.Enter)
            {
                Int64 BillNo = Convert.ToInt64(textBox5.Text);
                salesGrid sales = operations.GetById(BillNo);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            frmSalesListDialog List = new frmSalesListDialog();
            List.ShowDialog();
        }
    }
}
