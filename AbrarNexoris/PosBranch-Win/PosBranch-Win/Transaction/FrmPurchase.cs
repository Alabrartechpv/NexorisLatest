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
using Repository.TransactionRepository;
using PosBranch_Win.DialogBox;
using ModelClass.TransactionModels;


namespace PosBranch_Win.Transaction
{
    public partial class FrmPurchase : Form
    {
        Dropdowns drop = new Dropdowns();
        public float Amount, TotalAmount,SubTotal,NetTotal;
        public bool CheckExists;
        public PurchaseMaster ObjPurchaseMaster = new PurchaseMaster();
        public PurchaseDetails ObjPurchaseDetails = new PurchaseDetails();
        
        public FrmPurchase()
        {
            InitializeComponent();
        }

        private void FrmPurchase_Load(object sender, EventArgs e)
        {
            PurchaseInvoiceRepository ObjPurchaseRepo = new PurchaseInvoiceRepository();

            BranchDDlGrid branchDDl = drop.getBanchDDl();
            CmboBranch.DataSource = branchDDl.List;
            CmboBranch.DisplayMember = "BranchName";
            CmboBranch.ValueMember = "Id";

            PaymodeDDlGrid ObjPayModeDDL = drop.GetPaymode();
            CmboPayment.DataSource = ObjPayModeDDL.List;
            CmboPayment.DisplayMember = "PayModeName";
            CmboPayment.ValueMember = "PayModeID";

            VendorDDLGrids VendorDDLGrids = drop.VendorDDL();
            CmboVendor.DataSource = VendorDDLGrids.List;
            CmboVendor.DisplayMember = "LedgerName";
            CmboVendor.ValueMember = "LedgerID";

            int Purchasnos = ObjPurchaseRepo.GeneratePurchaseNO();
            txtPurchaseNo.Text = Purchasnos.ToString();

            AddProductItems();
            this.barcodeFocus();
        }

        public void AddProductItems()
        {
            DataGridViewTextBoxColumn Slno = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn ItemId = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn BarCode = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn Description = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn Cost = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn UnitId = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn Unit = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn Qty = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn UnitPrize = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn Packing = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn MarginPer = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn MarginAmt = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn TaxPer = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn TaxAmt = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn RetailPrice = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn WholeSalePrice = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn CreditPrice = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn CardPrice = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn Amount = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn TotalAmount = new DataGridViewTextBoxColumn();



            Slno.HeaderText = "SLNO";
            Slno.Name = "SLNO";
            Slno.DataPropertyName = "SLNO";
            Slno.Width = 50;
            Slno.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            Slno.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            Slno.Visible = true;
            Slno.ReadOnly = false;

            dgvItem.Columns.Add(Slno);

            dgvItem.CellBorderStyle = DataGridViewCellBorderStyle.None;
            ItemId.HeaderText = "ItemId";
            ItemId.Name = "ItemId";
            ItemId.DataPropertyName = "ItemId";
            ItemId.Width = 50;
            ItemId.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            ItemId.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            ItemId.Visible = true;
            ItemId.ReadOnly = false;
            dgvItem.Columns.Add(ItemId);

            BarCode.HeaderText = "BarCode";
            BarCode.Name = "BarCode";
            BarCode.DataPropertyName = "BarCode";
            BarCode.Width = 100;
            BarCode.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            BarCode.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            BarCode.Visible = true;
            BarCode.ReadOnly = false;
            dgvItem.Columns.Add(BarCode);

            Description.HeaderText = "Description";
            Description.Name = "Description";
            Description.DataPropertyName = "Description";
            Description.Width = 200;
            Description.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            Description.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            Description.Visible = true;
            Description.ReadOnly = false;
           // ItemName.AutoComplete = false;
            
            
            
            

            //DataBase.Operations = "GETALL";
            //ItemDDlGrid itemGrid = drop.itemDDlGrid(null, null);
            //ItemName.DataSource = itemGrid.List;
            //ItemName.DisplayMember = "Description";
            //ItemName.ValueMember = "ItemId";
            dgvItem.Columns.Add(Description);

            Cost.HeaderText = "Cost";
            Cost.Name = "Cost";
            Cost.DataPropertyName = "Cost";
            Cost.Width = 100;
            Cost.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            Cost.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            Cost.Visible = true;
            Cost.ReadOnly = false;
            dgvItem.Columns.Add(Cost);

            UnitId.HeaderText = "UnitId";
            UnitId.Name = "UnitId";
            UnitId.DataPropertyName = "UnitId";
            UnitId.Width = 50;
            UnitId.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            UnitId.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            UnitId.Visible = true;
            UnitId.ReadOnly = false;
            dgvItem.Columns.Add(UnitId);

            Unit.HeaderText = "Unit";
            Unit.Name = "Unit";
            Unit.DataPropertyName = "Unit";
            Unit.Width = 50;
            Unit.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            Unit.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            Unit.Visible = true;
            Unit.ReadOnly = false;
            dgvItem.Columns.Add(Unit);


            Qty.HeaderText = "Qty";
            Qty.Name = "Qty";
            Qty.DataPropertyName = "Qty";
            Qty.Width = 50;
            Qty.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            Qty.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            Qty.Visible = true;
            Qty.ReadOnly = false;
            dgvItem.Columns.Add(Qty);

            UnitPrize.HeaderText = "UnitPrize";
            UnitPrize.Name = "UnitPrize";
            UnitPrize.DataPropertyName = "UnitPrize";
            UnitPrize.Width = 50;
            UnitPrize.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            UnitPrize.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            UnitPrize.Visible = true;
            UnitPrize.ReadOnly = false;
            dgvItem.Columns.Add(UnitPrize);



            Packing.HeaderText = "Packing";
            Packing.Name = "Packing";
            Packing.DataPropertyName = "Packing";
            Packing.Width = 50;
            Packing.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            Packing.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            Packing.Visible = true;
            Packing.ReadOnly = false;
            dgvItem.Columns.Add(Packing);

            MarginPer.HeaderText = "MarginPer";
            MarginPer.Name = "MarginPer";
            MarginPer.DataPropertyName = "MarginPer";
            MarginPer.Width = 50;
            MarginPer.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            MarginPer.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            MarginPer.Visible = true;
            MarginPer.ReadOnly = false;
            dgvItem.Columns.Add(MarginPer);

            MarginAmt.HeaderText = "MarginAmt";
            MarginAmt.Name = "MarginAmt";
            MarginAmt.DataPropertyName = "MarginAmt";
            MarginAmt.Width = 50;
            MarginAmt.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            MarginAmt.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            MarginAmt.Visible = true;
            MarginAmt.ReadOnly = false;
            dgvItem.Columns.Add(MarginAmt);

            TaxPer.HeaderText = "TaxPer";
            TaxPer.Name = "TaxPer";
            TaxPer.DataPropertyName = "TaxPer";
            TaxPer.Width = 50;
            TaxPer.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            TaxPer.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            TaxPer.Visible = true;
            TaxPer.ReadOnly = false;
            dgvItem.Columns.Add(TaxPer);

            TaxAmt.HeaderText = "TaxAmt";
            TaxAmt.Name = "TaxAmt";
            TaxAmt.DataPropertyName = "TaxAmt";
            TaxAmt.Width = 50;
            TaxAmt.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            TaxAmt.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            TaxAmt.Visible = true;
            TaxAmt.ReadOnly = false;
            dgvItem.Columns.Add(TaxAmt);

            RetailPrice.HeaderText = "RetailPrice";
            RetailPrice.Name = "RetailPrice";
            RetailPrice.DataPropertyName = "RetailPrice";
            RetailPrice.Width = 50;
            RetailPrice.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            RetailPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            RetailPrice.Visible = true;
            RetailPrice.ReadOnly = false;
            dgvItem.Columns.Add(RetailPrice);

            WholeSalePrice.HeaderText = "WholeSalePrice";
            WholeSalePrice.Name = "WholeSalePrice";
            WholeSalePrice.DataPropertyName = "WholeSalePrice";
            WholeSalePrice.Width = 50;
            WholeSalePrice.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            WholeSalePrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            WholeSalePrice.Visible = true;
            WholeSalePrice.ReadOnly = false;
            dgvItem.Columns.Add(WholeSalePrice);

            CreditPrice.HeaderText = "CreditPrice";
            CreditPrice.Name = "CreditPrice";
            CreditPrice.DataPropertyName = "CreditPrice";
            CreditPrice.Width = 50;
            CreditPrice.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            CreditPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            CreditPrice.Visible = true;
            CreditPrice.ReadOnly = false;
            dgvItem.Columns.Add(CreditPrice);

            CardPrice.HeaderText = "CardPrice";
            CardPrice.Name = "CardPrice";
            CardPrice.DataPropertyName = "CardPrice";
            CardPrice.Width = 50;
            CardPrice.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            CardPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            CardPrice.Visible = true;
            CardPrice.ReadOnly = false;
            dgvItem.Columns.Add(CardPrice);

            Amount.HeaderText = "Amount";
            Amount.Name = "Amount";
            Amount.DataPropertyName = "Amount";
            Amount.Width = 50;
            Amount.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            Amount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            Amount.Visible = true;
            Amount.ReadOnly = false;
            dgvItem.Columns.Add(Amount);

            TotalAmount.HeaderText = "TotalAmount";
            TotalAmount.Name = "TotalAmount";
            TotalAmount.DataPropertyName = "TotalAmount";
            TotalAmount.Width = 50;
            TotalAmount.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            TotalAmount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            TotalAmount.Visible = true;
            TotalAmount.ReadOnly = false;
            dgvItem.Columns.Add(TotalAmount);



        }

        private void FrmPurchase_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode==Keys.F7)
            {
                FrmPurchaseItemDialog FrmPurDialog = new FrmPurchaseItemDialog();
                //FrmPurDialog.ShowDialog();
            }
            else if(e.KeyCode==Keys.Escape)
            {
                this.Close();
            }
        }

        private void txtBarcode_TextChanged(object sender, EventArgs e)
        {

            //if(txtBarcode.Text.Length>=13)
            //{
                
            //    FrmPurchaseItemDialog purItemD = new FrmPurchaseItemDialog();
            //    purItemD.ShowDialog();
            //}

            
        }

        private void txtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if((txtBarcode.Text!=null)&&(e.KeyCode==Keys.Enter))
            {
                if(txtBarcode.Text.Contains('*'))
                {
                    string[] StrRow = txtBarcode.Text.Split('*');

                    int RowIndex = Convert.ToInt32(StrRow[1].ToString());
                    dgvItem.Rows[RowIndex - 1].Selected = true;
                    dgvItem.CurrentCell = dgvItem.Rows[RowIndex-1].Cells["Qty"];
                    dgvItem.BeginEdit(true);
                }
                {

                    string barcode = txtBarcode.Text;
                    DataBase.Operations = "BARCODEPURCHASE";
                    ItemDDlGrid itemDDLG = drop.itemDDlGrid(barcode, null);


                    BindingSource bindingDs = new BindingSource();
                    bindingDs.DataSource = itemDDLG.List;
                    int i = bindingDs.Count;
                    if (i == 1)
                    {

                        //dgvItem.DataSource = itemDDLG.List;
                        this.CheckBarcode(txtBarcode.Text);
                        if(CheckExists == false)
                        {
                            int index = 0;
                            if (index >= 0 && index < bindingDs.Count)
                            {
                                int cnt = dgvItem.Rows.Add();

                                foreach (var itm in itemDDLG.List)
                                {
                                    dgvItem.Rows[cnt].Cells["SLNO"].Value = dgvItem.Rows.Count;
                                    dgvItem.Rows[cnt].Cells["ItemId"].Value = itm.ItemId;
                                    dgvItem.Rows[cnt].Cells["BarCode"].Value = itm.BarCode;
                                    dgvItem.Rows[cnt].Cells["Description"].Value = itm.Description;
                                    dgvItem.Rows[cnt].Cells["Cost"].Value = itm.Cost;
                                    dgvItem.Rows[cnt].Cells["UnitId"].Value = itm.UnitId;
                                    dgvItem.Rows[cnt].Cells["Unit"].Value = itm.Unit;
                                    dgvItem.Rows[cnt].Cells["Qty"].Value = 1;
                                    dgvItem.Rows[cnt].Cells["UnitPrize"].Value = itm.RetailPrice;
                                    dgvItem.Rows[cnt].Cells["Packing"].Value = itm.Packing;
                                    dgvItem.Rows[cnt].Cells["MarginPer"].Value = itm.MarginPer;
                                    dgvItem.Rows[cnt].Cells["MarginAmt"].Value = itm.MarginAmt;
                                    dgvItem.Rows[cnt].Cells["TaxPer"].Value = itm.TaxPer;
                                    dgvItem.Rows[cnt].Cells["TaxAmt"].Value = itm.TaxAmt;
                                    dgvItem.Rows[cnt].Cells["RetailPrice"].Value = itm.RetailPrice;
                                    dgvItem.Rows[cnt].Cells["WholeSalePrice"].Value = itm.WholeSalePrice;
                                    dgvItem.Rows[cnt].Cells["CreditPrice"].Value = itm.CreditPrice;
                                    dgvItem.Rows[cnt].Cells["CardPrice"].Value = itm.CardPrice;

                                }
                                this.CaluateTotals();
                                this.barcodeFocus();

                            }
                        }
                        CheckExists = false;



                    }
                    else
                    {
                        FrmPurchaseItemDialog itemDia = new FrmPurchaseItemDialog();
                        itemDia.DgvItem.DataSource = bindingDs;
                        itemDia.ShowDialog();
                    }
                }
              
            }
        }

        private void dgvItem_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if(dgvItem.Rows.Count>0)
            {
                if(e.RowIndex>=0 && e.ColumnIndex>=0)
                {
                   if((dgvItem.Rows[e.RowIndex].Cells["Qty"].Value!=null)&&(dgvItem.Rows[e.RowIndex].Cells["UnitPrize"].Value!=null))
                    {
                        float Qty = float.Parse(dgvItem.Rows[e.RowIndex].Cells["Qty"].Value.ToString());
                        float Prize = float.Parse(dgvItem.Rows[e.RowIndex].Cells["UnitPrize"].Value.ToString());
                        float Total = Qty * Prize;
                        dgvItem.Rows[e.RowIndex].Cells["Amount"].Value = Total;
                        dgvItem.Rows[e.RowIndex].Cells["TotalAmount"].Value = Total;
                        this.CaluateTotals();

                    }
                }
            }
             
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.SavePurchase();
        }

        public void SavePurchase()
        {
            ObjPurchaseMaster.BranchId = SessionContext.BranchId;
            ObjPurchaseMaster.CompanyId = SessionContext.CompanyId;
            ObjPurchaseMaster.FinYearId = SessionContext.FinYearId;
            ObjPurchaseMaster.BranchName = CmboBranch.DisplayMember;
            ObjPurchaseMaster.PurchaseNo = Convert.ToInt32(txtPurchaseNo.Text);
            ObjPurchaseMaster.PurchaseDate = dtpPurchaseDate.Value;
            ObjPurchaseMaster.InvoiceNo = txtInvoiceNo.Text;
            ObjPurchaseMaster.InvoiceDate = DtpInoviceDate.Value;
            ObjPurchaseMaster.LedgerID = Convert.ToInt32( CmboVendor.ValueMember);
            ObjPurchaseMaster.VendorName = CmboVendor.DisplayMember;
            ObjPurchaseMaster.PaymodeID = Convert.ToInt32(CmboPayment.ValueMember);
            ObjPurchaseMaster.Paymode = CmboPayment.DisplayMember;
            ObjPurchaseMaster.PaymodeLedgerID = 0;
            ObjPurchaseMaster.CreditPeriod = 0;
            ObjPurchaseMaster.SubTotal = Convert.ToInt32(lblSubTotal.Text);
            ObjPurchaseMaster.SpDisPer = 0;
            ObjPurchaseMaster.SpDsiAmt = 0;
            ObjPurchaseMaster.BillDiscountPer = 0;
            ObjPurchaseMaster.BillDiscountAmt = 0;
            ObjPurchaseMaster.TaxPer = 0;
            ObjPurchaseMaster.TaxAmt=0;
            ObjPurchaseMaster.Frieght = 0;
            ObjPurchaseMaster.ExpenseAmt = 0;
            ObjPurchaseMaster.OtherExpAmt = 0;
            ObjPurchaseMaster.GrandTotal = Convert.ToInt32(lblGrandTotal.Text) ;
            ObjPurchaseMaster.CancelFlag =false;
            ObjPurchaseMaster.UserID = SessionContext.UserId;
            ObjPurchaseMaster.UserName = DataBase.UserName;
            ObjPurchaseMaster.TaxType = "I";
            ObjPurchaseMaster.Remarks = "";
            ObjPurchaseMaster.RoundOff = 0;
            ObjPurchaseMaster.CessPer = 0;
            ObjPurchaseMaster.CessAmt = 0;
            ObjPurchaseMaster.CalAfterTax = 0;
            ObjPurchaseMaster.CurrencyID = 8;
            ObjPurchaseMaster.CurSymbol = "RM";
            ObjPurchaseMaster.SeriesID = 0;
            ObjPurchaseMaster.VoucherID = 1;
            ObjPurchaseMaster.IsSyncd = false;
            ObjPurchaseMaster.Paid = false;
            ObjPurchaseMaster.Pid = 0;
            ObjPurchaseMaster.POrderMasterId = 0;
            ObjPurchaseMaster.PayedAmount = Convert.ToInt32(txtPayedAmt.Text);
            ObjPurchaseMaster.BilledBy = txtBilledBy.Text;

            for(int i=0;i<dgvItem.Rows.Count;i++)
            {
                ObjPurchaseDetails.CompanyId = SessionContext.CompanyId;
                ObjPurchaseDetails.BranchID = SessionContext.BranchId;
                ObjPurchaseDetails.FinYearId = SessionContext.FinYearId;
                ObjPurchaseDetails.BranchName = CmboBranch.DisplayMember;
                ObjPurchaseDetails.PurchaseNo = Convert.ToInt32(txtPurchaseNo.Text);
                ObjPurchaseDetails.PurchaseDate = dtpPurchaseDate.Value;
                ObjPurchaseDetails.InvoiceNo = txtInvoiceNo.Text;
                ObjPurchaseDetails.SlNo = Convert.ToInt32(dgvItem.Rows[i].Cells["SLNO"].Value.ToString());
                ObjPurchaseDetails.ItemID = Convert.ToInt32(dgvItem.Rows[i].Cells["ItemId"].ToString());
                ObjPurchaseDetails.ItemName = dgvItem.Rows[i].Cells["Description"].Value.ToString();
                ObjPurchaseDetails.UnitId = Convert.ToInt32(dgvItem.Rows[i].Cells["UnitId"].Value.ToString());
                ObjPurchaseDetails.Unit = dgvItem.Rows[i].Cells["Unit"].Value.ToString();
                ObjPurchaseDetails.BaseUnit = "Y";
                ObjPurchaseDetails.Packing = float.Parse(dgvItem.Rows[i].Cells["Packing"].Value.ToString());
                ObjPurchaseDetails.Cost = float.Parse(dgvItem.Rows[i].Cells["Qty"].Value.ToString());
              

            }


        }

        public void CaluateTotals()
        {
            SubTotal = 0;
            NetTotal = 0;
            for(int i=0;i<dgvItem.Rows.Count;i++)
            {
                SubTotal += float.Parse(dgvItem.Rows[i].Cells["Amount"].Value.ToString());
                lblSubTotal.Text = SubTotal.ToString();
                NetTotal += float.Parse(dgvItem.Rows[i].Cells["Amount"].Value.ToString());
                lblGrandTotal.Text = NetTotal.ToString();
                dgvItem.Rows[i].Cells[0].Selected = true;
                this.ActiveControl = txtBarcode;
                txtBarcode.Focus();
            }


        }

        public void CheckBarcode(string barcode)
        {
            if(dgvItem.Rows.Count>0)
            {
                for (int i = 0; i < dgvItem.Rows.Count; i++)
                {
                    if (dgvItem.Rows[i].Cells["BarCode"].Value.ToString() == barcode)
                    {
                        CheckExists = true;
                        int qty = Convert.ToInt32(dgvItem.Rows[i].Cells["Qty"].Value.ToString());
                        dgvItem.Rows[i].Cells["Qty"].Value = qty + 1;
                        float unitPirce = float.Parse(dgvItem.Rows[i].Cells["UnitPrize"].Value.ToString());
                        dgvItem.Rows[i].Cells["Amount"].Value = (Convert.ToInt32(dgvItem.Rows[i].Cells["Qty"].Value) * unitPirce);
                        dgvItem.Rows[i].Cells["TotalAmount"].Value = (Convert.ToInt32(dgvItem.Rows[i].Cells["Qty"].Value) * unitPirce);
                        this.CaluateTotals();
                        this.barcodeFocus();
                    }
                }
            }
        }

        public void barcodeFocus()
        {
            this.ActiveControl = txtBarcode;
            txtBarcode.Clear();
            txtBarcode.Text = "";
            txtBarcode.Focus();
        }
    }
}
