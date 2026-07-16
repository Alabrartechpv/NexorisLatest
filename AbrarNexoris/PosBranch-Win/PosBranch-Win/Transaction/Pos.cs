using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Master;
using Repository;
using System;
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
    
    public partial class Pos : Form
    {


        DataGridViewTextBoxColumn colSlNo = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn Barcode = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn ItemName = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn Unit = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn UnitPrice = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn Qty = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn Amount = new DataGridViewTextBoxColumn();
        DataGridViewTextBoxColumn Total = new DataGridViewTextBoxColumn();
        Dropdowns dp = new Dropdowns();
        public Pos()
        {
            InitializeComponent();
        }

        private void Pos_Load(object sender, EventArgs e)
        {
            listvBranch.Visible = false;
            listvCustomer.Visible = false;
            //dvgBarcode.Visible = false;
           // ultraGridItem.Visible = false;
            PaymodeDDlGrid pay = dp.GetPaymode();
            cmbPaymode.DataSource = pay.List;

            cmbPaymode.DisplayMember = "PayModeName";
            cmbPaymode.ValueMember = "PayModeID";
            this.desing();

            FormatGrid();


        }
        void desing() {
            //this.txtBarcode.Size = new System.Drawing.Size(250, 100);

        }


        private void txtBranch_TextChanged(object sender, EventArgs e)
        {
            // here selecting the branch ddl
            //
            BranchDDlGrid bd =  dp.getBanchDDl();
            if(bd.List.Count<BranchDDl>() >  0)
            {
                txtBranch.Text = bd.List.First().BranchName;

            }
            else
            {

            }

            //here selecting the customer ddl

            CustomerDDlGrid cs = dp.CustomerDDl();
            cs.List.ToString();
            if(cs.List.Count<CustomerDDl>() > 0)
            {              
                var dt = cs.List.AsEnumerable<CustomerDDl>();
              var customer =  cs.List.Where(i => i.LedgerName.Contains("DEFAULT CUSTOMER"));
               if(customer.First().LedgerName == "DEFAULT CUSTOMER")
                {
                    txtCustomer.Text = "DEFAULT CUSTOMER";
                }
               else
                {
                    listvCustomer.Visible = true;
                    listvCustomer.DataSource = cs.List;
                    listvCustomer.DisplayMember = "LedgerName";
                }
                
            }

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void pnlposbody_Paint(object sender, PaintEventArgs e)
        {

        }

        private void txtItemName_TextChanged(object sender, EventArgs e)
        {
            //ultraGridItem.Visible = true;
            //DataBase.Operations = "GETITEM";
            //ItemDDlGrid item = dp.itemDDlGrid("", txtItemName.Text);
            //DataGridTableStyle ts1 = new DataGridTableStyle();
            //DataGridColumnStyle datagrid = new DataGridBoolColumn();
            //datagrid.Width = 400;
            //ts1.GridColumnStyles.Add(datagrid);
            //ultraGridItem.DataSource = item.List;
           // ultraGridItem.Focus();


        }

        private void txtBarcode_TextChanged(object sender, EventArgs e)
        {
          
              
         
        }

       

        private void txtItemName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == (char)Keys.Enter)
            {
                //ultraGridItem.Focus();
            }
            if(e.KeyChar == (char)Keys.Space)
            {
               // ultraGridItem.Visible = true;
                DataBase.Operations = "GETITEM";
                ItemDDlGrid item = dp.itemDDlGrid("", txtItemName.Text);
                DataGridTableStyle ts1 = new DataGridTableStyle();
                DataGridColumnStyle datagrid = new DataGridBoolColumn();
                datagrid.Width = 400;
                ts1.GridColumnStyles.Add(datagrid);
              //  ultraGridItem.DataSource = item.List;
            }
        }

        private bool CheckValueExistsInAnotherGrid(string selectedValue)
        {
            // Loop through the rows of the destination DataGridView (destinationDataGridView)
            //foreach (DataGridViewRow row in dgvPosBody.Rows)
            //{
            //    // Get the value from the column named "DesiredColumnName" (replace with the actual column name)
            //    string valueInAnotherGrid = row.Cells[1].Value.ToString();

            //    // Compare the values
            //    if (valueInAnotherGrid == selectedValue)
            //    {
            //     //   int rowIndex = dgvPosBody.CurrentCell.RowIndex;
            //    float qty = float.Parse(dgvPosBody.Rows[rowIndex].Cells[5].Value.ToString());
            //    float cQty =   qty + 1;               
            //    dgvPosBody.Rows[rowIndex].Cells[5].Value = cQty;
            //    return true; // Value exists in the destination DataGridView
            //    }
            //}

            return false; // Value does not exist in the destination DataGridView
        }

        private void dvgItemName_CellClick(object sender, DataGridViewCellEventArgs e)
        {
           
        }

        private void Pos_MouseClick(object sender, MouseEventArgs e)
        {
           // ultraGridItem.Visible = false;
        }

        private void FormatGrid()
        {

           

            //dgvPosBody.CellBorderStyle = DataGridViewCellBorderStyle.None;
            //dgvPosBody.Columns.Clear();
            //dgvPosBody.AllowUserToOrderColumns = true;
            //dgvPosBody.AllowUserToDeleteRows = false;
            //dgvPosBody.AllowUserToAddRows = false;
            //dgvPosBody.AlternatingRowsDefaultCellStyle.BackColor = SystemColors.Menu;
            //dgvPosBody.AutoGenerateColumns = false;
            //dgvPosBody.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            //dgvPosBody.ColumnHeadersHeight = 35;
            //dgvPosBody.MultiSelect = false;
            //dgvPosBody.SelectionMode = DataGridViewSelectionMode.FullRowSelect;


            //  colSlNo.DataPropertyName = "SlNo";
            //  colSlNo.HeaderText = "SlNo";
            //  colSlNo.Name = "SlNo";
            //  colSlNo.ReadOnly = false;
            //  colSlNo.Visible = true;
            //  colSlNo.Width = 50;
            //  colSlNo.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            //  colSlNo.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            ////  dgvPosBody.Columns.Add(colSlNo);

            //  Barcode.DataPropertyName = "Barcode";
            //  Barcode.HeaderText = "Barcode";
            //  Barcode.Name = "Barcode";
            //  Barcode.ReadOnly = false;
            //  Barcode.Visible = true;
            //  Barcode.Width = 150;
            //  Barcode.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            //  Barcode.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            // // dgvPosBody.Columns.Add(Barcode);

            //  ItemName.DataPropertyName = "ItemName";
            //  ItemName.HeaderText = "ItemName";
            //  ItemName.Name = "ItemName";
            //  ItemName.ReadOnly = false;
            //  ItemName.Visible = true;
            //  ItemName.Width = 300;
            //  ItemName.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            //  ItemName.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            ////  dgvPosBody.Columns.Add(ItemName);

            //  Unit.DataPropertyName = "Unit";
            //  Unit.HeaderText = "Unit";
            //  Unit.Name = "Unit";
            //  Unit.ReadOnly = false;
            //  Unit.Visible = true;
            //  Unit.Width = 70;
            //  Unit.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            //  Unit.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            ////  dgvPosBody.Columns.Add(Unit);

            //  UnitPrice.DataPropertyName = "UnitPrice";
            //  UnitPrice.HeaderText = "UnitPrice";
            //  UnitPrice.Name = "UnitPrice";
            //  UnitPrice.ReadOnly = false;
            //  UnitPrice.Visible = true;
            //  UnitPrice.Width = 70;
            //  UnitPrice.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            //  UnitPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            // // dgvPosBody.Columns.Add(UnitPrice);

            //  Qty.DataPropertyName = "Qty";
            //  Qty.HeaderText = "Qty";
            //  Qty.Name = "Qty";
            //  Qty.ReadOnly = false;
            //  Qty.Visible = true;
            //  Qty.Width = 70;
            //  Qty.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            //  Qty.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            // // dgvPosBody.Columns.Add(Qty);

            //  Amount.DataPropertyName = "Amount";
            //  Amount.HeaderText = "Amount";
            //  Amount.Name = "Amount";
            //  Amount.ReadOnly = false;
            //  Amount.Visible = true;
            //  Amount.Width = 70;
            //  Amount.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            //  Amount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            ////  dgvPosBody.Columns.Add(Amount);

            //  Total.DataPropertyName = "Total";
            //  Total.HeaderText = "Total";
            //  Total.Name = "Total";
            //  Total.ReadOnly = false;
            //  Total.Visible = true;
            //  Total.Width = 120;
            //  Total.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            //  Total.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            // // dgvPosBody.Columns.Add(Total);

        }

        private void txtItemName_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {            
                //ultraGridItem.Visible = true;
                //DataBase.Operations = "GETITEM";
                //ItemDDlGrid item = dp.itemDDlGrid("", txtItemName.Text);
                //DataGridTableStyle ts1 = new DataGridTableStyle();
                //DataGridColumnStyle datagrid = new DataGridBoolColumn();
                //datagrid.Width = 400;
                //ts1.GridColumnStyles.Add(datagrid);
                //ultraGridItem.DataSource = item.List;
                //ultraGridItem.Focus();              

            }


        }

        private void dvgItemName_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //ItemDDl item = new ItemDDl();

            //if (e.RowIndex > 0)
            //{
            //    for (int i = 0; i < dgvPosBody.Rows.Count; i++)
            //    {
            //        dgvPosBody.Rows[i].Cells["SlNo"].Value = i + 1;
            //    }


            //    dgvPosBody.Rows.Add(
            //      dvgItemName.Rows[e.RowIndex].Cells[0].Value.ToString(),
            //      dvgItemName.Rows[e.RowIndex].Cells[1].Value.ToString(),
            //      dvgItemName.Rows[e.RowIndex].Cells[2].Value.ToString(),
            //      dvgItemName.Rows[e.RowIndex].Cells[3].Value.ToString(),
            //      dvgItemName.Rows[e.RowIndex].Cells[4].Value.ToString(),
            //      dvgItemName.Rows[e.RowIndex].Cells[5].Value.ToString()
            //      );
            //}
        }

        private void dgvPosBody_KeyUp(object sender, KeyEventArgs e)
        {
           
        }

        private void dvgItemName_KeyUp(object sender, KeyEventArgs e)
        {
            
        }

        private void ultraGridItem_KeyPress(object sender, KeyPressEventArgs e)
        {
           if(e.KeyChar == (char)Keys.Enter)
            {
                ItemDDl item = new ItemDDl();
                DataTable dt = new DataTable();
                // item.ItemId = this.ultraGridItem..ToString();
                for (int i = 0; i < dgvPosBody.Rows.Count; i++)
                        {
                            dgvPosBody.Rows[i].Cells["SlNo"].Value = i + 1;
                        }
               
                //dt.Rows.Add(SlNo, item.BarCode.ToString(), item.Description.ToString(),
                //    item.Unit.ToString(),item.);
                // dgvPosBody.Rows.Add(SlNo,item.BarCode.ToString(),item.Description.ToString(),
                // item.Unit.ToString());
               // this.dgvPosBody.DataSource = item;
              //  UltraGridCell cell = this.ultraGridItem.Rows[0].Cells[0];
                // int col = this.ultraGridItem.Selected;
            }
        }
    }
}
