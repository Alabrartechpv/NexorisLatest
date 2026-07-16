using ModelClass;
using ModelClass.Master;
using ModelClass.TransactionModels;
using PosBranch_Win.DialogBox;
using Repository;
using Repository.TransactionRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win.Transaction
{
    public partial class frmSalesReturn : Form
    {
        BaseRepostitory con = new BaseRepostitory();
        SalesReturnDetails SRDetails = new SalesReturnDetails();
        SalesReturn SReturn = new SalesReturn();
        //SMaster Sm = new SMaster();
        SalesReturnRepository operations = new SalesReturnRepository();
        string No;
        Dropdowns dp = new Dropdowns();
        public frmSalesReturn()
        {
            InitializeComponent();
        }
        private void FormatGrid()
        {
            DataGridViewTextBoxColumn colItemName = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colUnit = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colRate = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colQty = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colReason = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colTotalAmount = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colBarcode = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colId = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colSlno= new DataGridViewTextBoxColumn();

            dgvInvoice.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dgvInvoice.Columns.Clear();
            dgvInvoice.AllowUserToOrderColumns = true;
            dgvInvoice.AllowUserToDeleteRows = false;
            dgvInvoice.AllowUserToAddRows = false;
            dgvInvoice.AlternatingRowsDefaultCellStyle.BackColor = SystemColors.Menu;
            dgvInvoice.AutoGenerateColumns = false;
            dgvInvoice.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dgvInvoice.ColumnHeadersHeight = 35;
            dgvInvoice.MultiSelect = false;
            dgvInvoice.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            colSlno.DataPropertyName = "SlNo";
            colSlno.HeaderText = "SlNo";
            colSlno.Name = "SlNo";
            colSlno.ReadOnly = false;
            colSlno.Visible = true;
            colSlno.Width = 100;
            colSlno.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colSlno.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvInvoice.Columns.Add(colSlno);

            colId.DataPropertyName = "ItemId";
            colId.HeaderText = "ItemId";
            colId.Name = "ItemId";
            colId.ReadOnly = false;
            colId.Visible = false;
            colId.Width = 100;
            colId.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colId.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvInvoice.Columns.Add(colId);

            colItemName.DataPropertyName = "ItemName";
            colItemName.HeaderText = "ItemName";
            colItemName.Name = "ItemName";
            colItemName.ReadOnly = false;
            colItemName.Visible = true;
            colItemName.Width = 255;
            colItemName.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colItemName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvInvoice.Columns.Add(colItemName);
           
            colBarcode.DataPropertyName = "Barcode";
            colBarcode.HeaderText = "Barcode";
            colBarcode.Name = "Barcode";
            colBarcode.ReadOnly = false;
            colBarcode.Visible = true;
            colBarcode.Width = 100;
            colBarcode.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colBarcode.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvInvoice.Columns.Add(colBarcode);
            
            colUnit.DataPropertyName = "Unit";
            colUnit.HeaderText = "Unit";
            colUnit.Name = "Unit";
            colUnit.ReadOnly = false;
            colUnit.Visible = true;
            colUnit.Width = 100;
            colUnit.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colUnit.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvInvoice.Columns.Add(colUnit);

            colRate.DataPropertyName = "UnitPrice";
            colRate.HeaderText = "UnitPrice";
            colRate.Name = "UnitPrice";
            colRate.ReadOnly = false;
            colRate.Visible = true;
            colRate.Width = 100;
            colRate.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colRate.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvInvoice.Columns.Add(colRate);

            colQty.DataPropertyName = "Qty";
            colQty.HeaderText = "Qty";
            colQty.Name = "Qty";
            colQty.ReadOnly = false;
            colQty.Visible = true;
            colQty.Width = 100;
            colQty.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colQty.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvInvoice.Columns.Add(colQty);

            colReason.DataPropertyName = "Reason";
            colReason.HeaderText = "Reason";
            colReason.Name = "Reason";
            colReason.ReadOnly = false;
            colReason.Visible = true;
            colReason.Width = 200;
            colReason.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colReason.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvInvoice.Columns.Add(colReason);

            colTotalAmount.DataPropertyName = "Amount";
            colTotalAmount.HeaderText = "TotalAmount";
            colTotalAmount.Name = "Amount";
            colTotalAmount.ReadOnly = false;
            colTotalAmount.Visible = true;
            colTotalAmount.Width = 100;
            colTotalAmount.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            colTotalAmount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvInvoice.Columns.Add(colTotalAmount);


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

            //dgvInvoice.Columns.Add(buttonColAdd);
            ////dgv_uomtab.Columns.Add(buttonColEdit);
            //dgvInvoice.Columns.Add(buttonColDelete);
        }
        private void frmSalesReturn_Load(object sender, EventArgs e)
        {
            KeyPreview = true;
            //CustomerDDlGrid cs = dp.CustomerDDl();
            //cs.List.ToString();
            //var led = cs.List.Where(f => f.LedgerName == "DEFAULT CUSTOMER").FirstOrDefault();
            //TxtCustmrName.Text = led.LedgerName;
            //SReturn.LedgerID = led.LedgerID;

            this.RefreshBranch();
            this.RefreshPaymode();
            this.RefreshInvoice();
            this.RefreshCustomer();
            this.FormatGrid();

            int SReturnNo = operations.GenerateSReturnNo();
            TxtSRNO.Text = SReturnNo.ToString();

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }
        private int rowCount = 0;
        private void dgvInvoice_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            rowCount++;

            if (e.ColumnIndex == 8)
            {
                DataGridViewRow row = new DataGridViewRow();
                dgvInvoice.Rows.Add(row);
            }
            else if(e.ColumnIndex == 9)
            {
                int SelectedRows = dgvInvoice.CurrentCell.RowIndex;
                dgvInvoice.Rows.RemoveAt(SelectedRows);
                rowCount--;
            }
        }
        public void RefreshBranch()
        {
            System.Data.DataRow dr;


            using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Branch, (SqlConnection)con.DataConnection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                {
                    cmd.Parameters.AddWithValue("_Operation", "GETALL");
                    DataTable dt = new DataTable();//fdf
                    adapt.Fill(dt);
                    dr = dt.NewRow();
                    //dr.ItemArray = new object[] { 0, "--Select Branch--" };
                    //dt.Rows.InsertAt(dr, 0);
                    cmbBranch.ValueMember = "Id";
                    cmbBranch.DisplayMember = "BranchName";
                    cmbBranch.DataSource = dt;

                }
            }
        }
        public void RefreshPaymode()
        {
            System.Data.DataRow dr;
            DataTable dt = new DataTable();
            PaymodeDDlGrid grid = dp.GetPaymode();
            cmbPaymntMethod.DataSource = grid.List;
            cmbPaymntMethod.DisplayMember = "PayModeName";
            cmbPaymntMethod.ValueMember = "PayModeId";

        }
        public void RefreshCustomer()
        {
            System.Data.DataRow dr;
            DataTable dt = new DataTable();
            CustomerDDlGrid cs = dp.CustomerDDl();
            cmbCustomer.DataSource = cs.List;
            var led = cs.List.Where(f => f.LedgerName == "DEFAULT CUSTOMER").FirstOrDefault();
            cmbCustomer.DisplayMember = "LedgerName";
            cmbCustomer.ValueMember = "LedgerID";

        }
        public void RefreshInvoice()
        {
            System.Data.DataRow dr;
            DataTable dt = new DataTable();
            SalesDDlGrid grid = dp.SalesDDl();
            //cmbInvoiceNo.DataSource = grid.List;
            //cmbInvoiceNo.DisplayMember = "BillNo";
            //cmbInvoiceNo.ValueMember = "BillNo";

            //cmbInvoiceNo.SelectedIndex = 0;
            //cmbInvoiceNo.SelectedValue = "N/A";


        }

        private void cmbInvoiceNo_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (cmbInvoiceNo.SelectedIndex > 0)
            //{
               
            //    Int64 Number = Convert.ToInt64(cmbInvoiceNo.GetItemText(cmbInvoiceNo.SelectedValue));
            //    SalesReturn SR = operations.GetById(Number);
               

            //    if (SR != null)
            //    {
            //       // No = SR.InvoiceNo;
            //        dtInvoiceDate.Value = SR.BillDate;
            //        TxtInvoiceAmnt.Text = SR.NetAmount.ToString();
                  
            //    }

            //    else
            //    {
                    
            //       // dtInvoiceDate.Value = null;
            //        TxtInvoiceAmnt.Text = null;
            //    }
            //}
           
        }

        private void cmbInvoiceNo_Click(object sender, EventArgs e)
        {
            
            
           
        }

        private void cmbInvoiceNo_KeyPress(object sender, KeyPressEventArgs e)
        {
          
        }

        private void cmbInvoiceNo_KeyDown(object sender, KeyEventArgs e)
        {
            
        }


        private void SaveSalesReturn()
        {
            SReturn.BranchId = SessionContext.BranchId;
            SReturn.CompanyId = SessionContext.CompanyId;
            SReturn.FinYearId = SessionContext.FinYearId;

            SReturn.CustomerName = cmbCustomer.GetItemText(cmbCustomer.SelectedValue);
            SReturn.Paymode = cmbPaymntMethod.GetItemText(cmbPaymntMethod.SelectedItem);
            SReturn.PaymodeID = Convert.ToInt32(cmbPaymntMethod.SelectedValue);
            SReturn.SReturnNo = 0;
            SReturn.InvoiceNo = textBox1.Text;
            SReturn.SpDisPer = 0;
            SReturn.SpDsiAmt = 0;
            SReturn.BillDiscountPer = 0;
            SReturn.BillDiscountAmt = 0;
            SReturn.TaxPer = 0;
            SReturn.TaxAmt = 0;
            SReturn.Frieght = 0;
            SReturn.GrandTotal = 0;
            SReturn.UserID = 0;
            SReturn.UserName = "";
            SReturn.TaxType = "";
            SReturn.RoundOff = 0;
            SReturn.CessAmt = 0;
            SReturn.CessPer = 0;
            SReturn.VoucherID = 0;
            SReturn.BranchName = "";
            SReturn.CalAfterTax = 0;
            SReturn.CurSymbol = "";
            SReturn.SReturnDate = dtSReturnDate.Value;//DateTime.ParseExact( formattedDate , "yyyy/MM/dd HH:mm:ss", null);  //DateTime(formattedDate);//Convert.ToDateTime(formattedDate);
            SReturn.InvoiceDate = dtInvoiceDate.Value;
            SRDetails.BranchName = DataBase.Branch;
            SRDetails.SReturnDate=dtSReturnDate.Value;

            SReturn._Operation = "CREATE";
            string message = operations.saveSR(SReturn, SRDetails,dgvInvoice);
            MessageBox.Show(message);

        }

        private void pbxSave_Click(object sender, EventArgs e)
        {
            SaveSalesReturn();
            MessageBox.Show("Successfully saved!");
        }

        private void TxtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if ((TxtBarcode.Text != null) && (e.KeyCode == Keys.Enter))
            {
                string barcode = TxtBarcode.Text;
                DataBase.Operations = "BARCODESReturn";
                ItemDDlGrid itemgrid= dp.itemDDlGrid(barcode, null);
                BindingSource bindingDs = new BindingSource();
                bindingDs.DataSource = itemgrid.List;
                int i = bindingDs.Count;
                if (i == 1)
                {
                    int index = 0;
                    if (index >= 0 && index < bindingDs.Count)
                    {
                        int cnt = dgvInvoice.Rows.Add();
                        foreach (var itm in itemgrid.List)
                        {
                            dgvInvoice.Rows[cnt].Cells["SLNO"].Value = dgvInvoice.Rows.Count;
                            dgvInvoice.Rows[cnt].Cells["ItemId"].Value = itm.ItemId;
                            dgvInvoice.Rows[cnt].Cells["ItemName"].Value = itm.Description;
                            dgvInvoice.Rows[cnt].Cells["Barcode"].Value = itm.BarCode;
                            dgvInvoice.Rows[cnt].Cells["Unit"].Value = itm.Unit;
                            dgvInvoice.Rows[cnt].Cells["UnitId"].Value = itm.UnitId;
                            dgvInvoice.Rows[cnt].Cells["UnitPrice"].Value = itm.Cost;
                            dgvInvoice.Rows[cnt].Cells["Qty"].Value = 1;
                           // dgvInvoice.Rows[cnt].Cells["Amount"].Value = itm.;
                            
                        }

                    }



                }
                else
                {
                    FrmSReturnDialog SRDialog = new FrmSReturnDialog();
                    SRDialog.dgvSReturnDial.DataSource = bindingDs;
                    SRDialog.ShowDialog();
                }

            }
        }

     
        private void frmSalesReturn_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F7)
            {
                
                FrmSReturnDialog FrmSRDialog = new FrmSReturnDialog();
                FrmSRDialog.ShowDialog();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                frmBillsDialog bills = new frmBillsDialog();
                bills.ShowDialog();
            }
        }

        private void btn_Add_Custm_Click(object sender, EventArgs e)
        {
            frmBillsDialog bills = new frmBillsDialog();
            bills.ShowDialog();
        }
    }
}
//21-08-24 03: 30
