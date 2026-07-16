using Infragistics.Win.UltraWinGrid;
using ModelClass;
using ModelClass.Master;
using PosBranch_Win.Master;
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

namespace PosBranch_Win.DialogBox
{
    public partial class frmUnitDialog : Form
    {
        Dropdowns drop = new Dropdowns();
        frmItemMasterNew ItemMaster = new frmItemMasterNew();
        string FormName;
        bool checke = false;
        public frmUnitDialog(string Params)
        {

            InitializeComponent();
            FormName = Params;
        }

        private void frmUnitDialog_Load(object sender, EventArgs e)
        {
            DataBase.Operations = "Unit";
            UnitDDlGrid Unit = drop.getUnitDDl();
            Unit.List.ToString();
            DataGridTableStyle ts1 = new DataGridTableStyle();
            DataGridColumnStyle datagrid = new DataGridBoolColumn();
            //datagrid.Width = 400;
            this.ultraGrid1.DisplayLayout.AutoFitStyle = AutoFitStyle.ResizeAllColumns;
            ts1.GridColumnStyles.Add(datagrid);
            ultraGrid1.DataSource = Unit.List;
        }

        private void ultraGrid1_KeyPress(object sender, KeyPressEventArgs e)
        {
            
                if (FormName == "ItemMasterGrid")
                {

                    ItemMaster = (frmItemMasterNew)Application.OpenForms["frmItemMasterNew"];
                    UltraGridCell UnitID = this.ultraGrid1.ActiveRow.Cells["UnitID"];
                    UltraGridCell UnitName = this.ultraGrid1.ActiveRow.Cells["UnitName"];
                    UltraGridCell Packing = this.ultraGrid1.ActiveRow.Cells["Packing"];
                    this.checkUnitExist(UnitName.Value.ToString());
                    if (this.checke == true)
                    {
                        this.Close();
                        MessageBox.Show("Unit Already Added");
                    }
                    else
                    {
                        int count;
                        count = ItemMaster.dgv_Uom.Rows.Add();
                        ItemMaster.dgv_Uom.Rows[count].Cells["Unit"].Value = UnitName.Value.ToString();
                        ItemMaster.dgv_Uom.Rows[count].Cells["UnitId"].Value = UnitID.Value.ToString();

                        ItemMaster.dgv_Uom.Rows[count].Cells["Packing"].Value = Packing.Value.ToString();
                        ItemMaster.dgv_Uom.Rows[count].Cells["Reorder"].Value = 5;
                        ItemMaster.dgv_Uom.Rows[count].Cells["BarCode"].Value = 0;
                        ItemMaster.dgv_Uom.Rows[count].Cells["OpnStk"].Value = 0;


                        count = ItemMaster.dgv_Price.Rows.Add();
                        //getting values from the first row
                        float cost1 = float.Parse(ItemMaster.dgv_Price.Rows[0].Cells["Cost"].Value.ToString());

                        ItemMaster.dgv_Price.Rows[count].Cells["Unit"].Value = UnitName.Value.ToString();
                        ItemMaster.dgv_Price.Rows[count].Cells["Packing"].Value = Packing.Value.ToString();
                        ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value = cost1 * float.Parse(Packing.Value.ToString()); //ItemMaster.Txt_UnitCost.Text;
                        ItemMaster.dgv_Price.Rows[count].Cells["MRP"].Value = float.Parse(ItemMaster.dgv_Price.Rows[0].Cells["MRP"].Value.ToString()) * float.Parse(Packing.Value.ToString()); //ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value;
                        ItemMaster.dgv_Price.Rows[count].Cells["RetailPrice"].Value = float.Parse(ItemMaster.dgv_Price.Rows[0].Cells["RetailPrice"].Value.ToString()) * float.Parse(Packing.Value.ToString()); //ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value;
                        ItemMaster.dgv_Price.Rows[count].Cells["WholeSalePrice"].Value = float.Parse(ItemMaster.dgv_Price.Rows[0].Cells["WholeSalePrice"].Value.ToString()) * float.Parse(Packing.Value.ToString()); //ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value;
                        ItemMaster.dgv_Price.Rows[count].Cells["CreditPrice"].Value = float.Parse(ItemMaster.dgv_Price.Rows[0].Cells["CreditPrice"].Value.ToString()) * float.Parse(Packing.Value.ToString());  //ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value;
                        ItemMaster.dgv_Price.Rows[count].Cells["CardPrice"].Value = float.Parse(ItemMaster.dgv_Price.Rows[0].Cells["CardPrice"].Value.ToString()) * float.Parse(Packing.Value.ToString()); //ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value;
                                                                                                                                                                                                           // float margin = float.Parse(ItemMaster.dgv_Price.Rows[count].Cells["MRP"].Value.ToString()) - float.Parse(ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value.ToString());
                        ItemMaster.dgv_Price.Rows[count].Cells["MarginAmt"].Value = 0; //margin.ToString();
                                                                                       // float marginper = (float.Parse(ItemMaster.dgv_Price.Rows[count].Cells["MRP"].Value.ToString()) - float.Parse(ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value.ToString()));
                                                                                       //  marginper = margin / float.Parse(ItemMaster.dgv_Price.Rows[count].Cells["MRP"].Value.ToString()) * 100;
                        ItemMaster.dgv_Price.Rows[count].Cells["MarginPer"].Value = 0;
                        if (ItemMaster.txt_TaxPer.Text != "")
                        {
                            float taxPer = float.Parse(ItemMaster.txt_TaxPer.Text);
                            ItemMaster.dgv_Price.Rows[count].Cells["TaxPer"].Value = taxPer.ToString();

                        }
                        else
                        {
                            ItemMaster.dgv_Price.Rows[count].Cells["TaxPer"].Value = 0;

                        }
                        if (ItemMaster.txt_TaxAmount.Text != "")
                        {
                            ItemMaster.dgv_Price.Rows[count].Cells["TaxAmt"].Value = 0;

                        }
                        else
                        {
                        ItemMaster.dgv_Price.Rows[count].Cells["TaxAmt"].Value = ItemMaster.txt_TaxAmount.Text;
                        }
                    }



                    this.Close();

                }
                else if (FormName == "ItemMasterMaster")
                {

                if (ItemMaster.dgv_Uom.Rows.Count == 0)
                {
                    ItemMaster = (frmItemMasterNew)Application.OpenForms["frmItemMasterNew"];
                    UltraGridCell UnitID = this.ultraGrid1.ActiveRow.Cells["UnitID"];
                    UltraGridCell UnitName = this.ultraGrid1.ActiveRow.Cells["UnitName"];
                    UltraGridCell Packing = this.ultraGrid1.ActiveRow.Cells["Packing"];
                    this.checkUnitExist(UnitName.Value.ToString());
                    if (checke == false)
                    {
                        ItemMaster.txt_BaseUnit.Text = UnitName.Value.ToString();
                        ItemMaster.lblBaseUnitId.Text = UnitID.Value.ToString();

                        int count; //here values for uom 
                        count = ItemMaster.dgv_Uom.Rows.Add();
                        ItemMaster.dgv_Uom.Rows[count].Cells["Unit"].Value = UnitName.Value.ToString();
                        ItemMaster.dgv_Uom.Rows[count].Cells["UnitId"].Value = UnitID.Value.ToString();
                        ItemMaster.dgv_Uom.Rows[count].Cells["Packing"].Value = Packing.Value.ToString();
                        ItemMaster.dgv_Uom.Rows[count].Cells["Reorder"].Value = 5;
                        ItemMaster.dgv_Uom.Rows[count].Cells["BarCode"].Value = 0;
                        ItemMaster.dgv_Uom.Rows[count].Cells["OpnStk"].Value = 0;


                        count = ItemMaster.dgv_Price.Rows.Add();
                        ItemMaster.dgv_Price.Rows[count].Cells["Unit"].Value = UnitName.Value.ToString();
                        ItemMaster.dgv_Price.Rows[count].Cells["Packing"].Value = Packing.Value.ToString();
                        ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value = 0; //ItemMaster.Txt_UnitCost.Text;
                        ItemMaster.dgv_Price.Rows[count].Cells["MRP"].Value = 0; //ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value;
                        ItemMaster.dgv_Price.Rows[count].Cells["RetailPrice"].Value = 0; //ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value;
                        ItemMaster.dgv_Price.Rows[count].Cells["WholeSalePrice"].Value = 0; //ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value;
                        ItemMaster.dgv_Price.Rows[count].Cells["CreditPrice"].Value = 0; //ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value;
                        ItemMaster.dgv_Price.Rows[count].Cells["CardPrice"].Value = 0;//ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value;
                                                                                      // float margin = float.Parse(ItemMaster.dgv_Price.Rows[count].Cells["MRP"].Value.ToString()) - float.Parse(ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value.ToString());
                        ItemMaster.dgv_Price.Rows[count].Cells["MarginAmt"].Value = 0; //margin.ToString();
                                                                                       // float marginper = (float.Parse(ItemMaster.dgv_Price.Rows[count].Cells["MRP"].Value.ToString()) - float.Parse(ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value.ToString()));
                                                                                       //  marginper = margin / float.Parse(ItemMaster.dgv_Price.Rows[count].Cells["MRP"].Value.ToString()) * 100;
                        ItemMaster.dgv_Price.Rows[count].Cells["MarginPer"].Value = 0; //marginper.ToString();
                        if (ItemMaster.txt_TaxPer.Text != "")
                        {
                            float taxPer = float.Parse(ItemMaster.txt_TaxPer.Text);
                            ItemMaster.dgv_Price.Rows[count].Cells["TaxPer"].Value = taxPer.ToString();

                        }
                        else
                        {
                            ItemMaster.dgv_Price.Rows[count].Cells["TaxPer"].Value = 0;

                        }
                        if (ItemMaster.txt_TaxAmount.Text != "")
                        {
                            ItemMaster.dgv_Price.Rows[count].Cells["TaxAmt"].Value = 0;

                        }

                        this.Close();
                    }
                
                    
                    else
                    {
                        MessageBox.Show("Unit Already Added");
                    }
                }
                else
                {
                    ItemMaster.dgv_Uom.Rows.RemoveAt(0);
                    ItemMaster.dgv_Price.Rows.RemoveAt(0);

                    ItemMaster = (frmItemMasterNew)Application.OpenForms["frmItemMasterNew"];
                    UltraGridCell UnitID = this.ultraGrid1.ActiveRow.Cells["UnitID"];
                    UltraGridCell UnitName = this.ultraGrid1.ActiveRow.Cells["UnitName"];
                    UltraGridCell Packing = this.ultraGrid1.ActiveRow.Cells["Packing"];
                    this.checkUnitExist(UnitName.Value.ToString());
                    if (checke == false)
                    {
                        ItemMaster.txt_BaseUnit.Text = UnitName.Value.ToString();
                        ItemMaster.lblBaseUnitId.Text = UnitID.Value.ToString();

                        int count; //here values for uom 
                        count = ItemMaster.dgv_Uom.Rows.Add();
                        ItemMaster.dgv_Uom.Rows[count].Cells["Unit"].Value = UnitName.Value.ToString();
                        ItemMaster.dgv_Uom.Rows[count].Cells["UnitId"].Value = UnitID.Value.ToString();
                        ItemMaster.dgv_Uom.Rows[count].Cells["Packing"].Value = Packing.Value.ToString();
                        ItemMaster.dgv_Uom.Rows[count].Cells["Reorder"].Value = 5;
                        ItemMaster.dgv_Uom.Rows[count].Cells["BarCode"].Value = 0;
                        ItemMaster.dgv_Uom.Rows[count].Cells["OpnStk"].Value = 0;


                        count = ItemMaster.dgv_Price.Rows.Add();
                        ItemMaster.dgv_Price.Rows[count].Cells["Unit"].Value = UnitName.Value.ToString();
                        ItemMaster.dgv_Price.Rows[count].Cells["Packing"].Value = Packing.Value.ToString();
                        ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value = 0; //ItemMaster.Txt_UnitCost.Text;
                        ItemMaster.dgv_Price.Rows[count].Cells["MRP"].Value = 0; //ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value;
                        ItemMaster.dgv_Price.Rows[count].Cells["RetailPrice"].Value = 0; //ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value;
                        ItemMaster.dgv_Price.Rows[count].Cells["WholeSalePrice"].Value = 0; //ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value;
                        ItemMaster.dgv_Price.Rows[count].Cells["CreditPrice"].Value = 0; //ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value;
                        ItemMaster.dgv_Price.Rows[count].Cells["CardPrice"].Value = 0;//ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value;
                                                                                      // float margin = float.Parse(ItemMaster.dgv_Price.Rows[count].Cells["MRP"].Value.ToString()) - float.Parse(ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value.ToString());
                        ItemMaster.dgv_Price.Rows[count].Cells["MarginAmt"].Value = 0; //margin.ToString();
                                                                                       // float marginper = (float.Parse(ItemMaster.dgv_Price.Rows[count].Cells["MRP"].Value.ToString()) - float.Parse(ItemMaster.dgv_Price.Rows[count].Cells["Cost"].Value.ToString()));
                                                                                       //  marginper = margin / float.Parse(ItemMaster.dgv_Price.Rows[count].Cells["MRP"].Value.ToString()) * 100;
                        ItemMaster.dgv_Price.Rows[count].Cells["MarginPer"].Value = 0; //marginper.ToString();
                        if (ItemMaster.txt_TaxPer.Text != "")
                        {
                            float taxPer = float.Parse(ItemMaster.txt_TaxPer.Text);
                            ItemMaster.dgv_Price.Rows[count].Cells["TaxPer"].Value = taxPer.ToString();

                        }
                        else
                        {
                            ItemMaster.dgv_Price.Rows[count].Cells["TaxPer"].Value = 0;

                        }
                        if (ItemMaster.txt_TaxAmount.Text != "")
                        {
                            ItemMaster.dgv_Price.Rows[count].Cells["TaxAmt"].Value = 0;

                        }

                        this.Close();
                    }


                    else
                    {
                        MessageBox.Show("Unit Already Added");
                    }


                }
            }
     
        }

        public bool checkUnitExist(string Unit)
        {
            this.checke = false;
            if (ItemMaster.dgv_Uom.Rows.Count>0)
            {
                for(int i =0;  ItemMaster.dgv_Uom.Rows.Count > i; i++)
                {
                    if (ItemMaster.dgv_Uom.Rows[i].Cells["Unit"].Value.ToString() == Unit)
                    {
                        this.checke = true;
                    }
                    else
                    {
                        this.checke = false;
                    }
                }
            }
            return checke;
        }
    }
}
