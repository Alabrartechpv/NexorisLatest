using ModelClass;
using PosBranch_Win.Accounts;
using PosBranch_Win.Master;
using PosBranch_Win.Transaction;
using PosBranch_Win.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PosBranch_Win
{
    public partial class Home : Form
    {
        private int childFormNumber = 0;
        private bool isCtrlPressed = false;
        private bool isF2Pressed = false;
        private bool isShiftPressed = false;
        public Home()
        {
            InitializeComponent();
        }

        private void ShowNewForm(object sender, EventArgs e)
        {
            Form childForm = new Form();
            childForm.MdiParent = this;
            childForm.Text = "Window " + childFormNumber++;
            childForm.Show();
        }

        private void OpenFile(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            openFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string FileName = openFileDialog.FileName;
            }
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string FileName = saveFileDialog.FileName;
            }
        }

        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CutToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void ToolBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
           // toolStrip.Visible = toolBarToolStripMenuItem.Checked;
        }

        private void StatusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
           // statusStrip.Visible = statusBarToolStripMenuItem.Checked;
        }

        private void CascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        private void TileVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void TileHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void ArrangeIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.ArrangeIcons);
        }

        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
            {
                childForm.Close();
            }
        }

        private void Home_Load(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = DataBase.Branch;
            toolStripStatusUserValLabel3.Text = DataBase.UserName;
            ultraRadialMenu1.Show(this, new Point(Bounds.Right, Bounds.Top));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Connection childForm = new Connection();
            childForm.MdiParent = this;
            childForm.Text = "Window " + childFormNumber++;
            childForm.Show();
           // Connection cn = new Connection();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Test childForm = new Test();
            childForm.MdiParent = this;
            childForm.Text = "Window " + childFormNumber++;
            childForm.Show();
        }

        private void toolStripStatusLabel2_Click(object sender, EventArgs e)
        {

        }

        private void barcodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmBarcode br = new frmBarcode();
            br.Show();

        }

        private void posToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
        }

        private void pOSToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            frmPos frmPos = new frmPos();
            frmPos.MdiParent = this;
            frmPos.Text = "Window " + childFormNumber++;
            frmPos.Show();
        }

        private void ultraToolbarsManager1_ToolClick(object sender, Infragistics.Win.UltraWinToolbars.ToolClickEventArgs e)
        {
            frmPos frmPos = new frmPos();
            frmPos.MdiParent = this;
            frmPos.Text = "Window " + childFormNumber++;
            frmPos.Show();
        }

        private void ultraToolbarsManager1_ToolClick_1(object sender, Infragistics.Win.UltraWinToolbars.ToolClickEventArgs e)
        {
            if(e.Tool.Key == "Pos")
                {
                    frmPos frmPos = new frmPos();
                    frmPos.MdiParent = this;
                    frmPos.Text = "Pos";
                    frmPos.Show();
                }
        
            if(e.Tool.Key == "Sales")
            {
                frmSalesInvoice frmsales = new frmSalesInvoice();
                    frmsales.MdiParent = this;
                    frmsales.Text = "Sales Invoice";
                    frmsales.Show();
            }
            if (e.Tool.Key == "Sales Return")
            {
                frmSalesReturn frmsalesreturn = new frmSalesReturn();
                frmsalesreturn.MdiParent = this;
                frmsalesreturn.Text = "Sales Return";
                frmsalesreturn.Show();
            }
            if (e.Tool.Key == "Branch")
            {
                FrmBranch frmbranch = new FrmBranch();
                frmbranch.MdiParent = this;
                frmbranch.Text = "Branch";
                frmbranch.Show();
            }
            if (e.Tool.Key == "Category")
            {
                Master.FrmCategory frmcategory = new Master.FrmCategory();
                frmcategory.MdiParent = this;
                frmcategory.Text = "Category";
                frmcategory.Show();
            }
            if (e.Tool.Key == "Group")
            {
                Master.FrmGroup frmcategory = new Master.FrmGroup();
                frmcategory.MdiParent = this;
                frmcategory.Text = "Group";
                frmcategory.Show();
            }
            if(e.Tool.Key == "ItemMaster")
            {
                // FrmItemMaster ItemMaster = new FrmItemMaster();
                frmItemMasterNew ItemMaster = new frmItemMasterNew();
                ItemMaster.MdiParent = this;
                ItemMaster.Text = "ItemMaster";
                ItemMaster.Show();
            }
            if (e.Tool.Key == "Line")
            {
                frmLine line = new frmLine();
                line.MdiParent = this;
                line.Text = "ItemMaster";
                line.Show();
            }
            if (e.Tool.Key == "Rack")
            {
                frmRack rack = new frmRack();
                rack.MdiParent = this;
                rack.Text = "ItemMaster";
                rack.Show();
            }
            if (e.Tool.Key == "Row")
            {
                FrmRow Row = new FrmRow();
                Row.MdiParent = this;
                Row.Text = "ItemMaster";
                Row.Show();
            }
            if (e.Tool.Key == "Brand")
            {
                FrmBrand Brand = new FrmBrand();
                Brand.MdiParent = this;
                Brand.Text = "ItemMaster";
                Brand.Show();
            }
            if (e.Tool.Key == "Customer")
            {
                FrmCustomer Customer = new FrmCustomer();
                Customer.MdiParent = this;
                Customer.Text = "Customer";
                Customer.Show();
            }
            if(e.Tool.Key=="Vendor")
            {
                Accounts.FrmVendor vendor = new FrmVendor();
                vendor.MdiParent = this;
                vendor.Text = "Vednord";
                vendor.Show();
            }
            if (e.Tool.Key == "Users")
            {
                FrmUsers users = new FrmUsers();
                users.MdiParent = this;
                users.Text = "Vednord";
                users.Show();
            }
            if(e.Tool.Key == "Purchase")
            {
                FrmPurchase purchase = new FrmPurchase();
                purchase.MdiParent = this;
                purchase.Text = "Purchase";
                purchase.Show();
            }


        }

        private void Home_KeyPress(object sender, KeyPressEventArgs e)
        {
           
        }

        private void Home_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                //frmSalesInvoice frmsales = new frmSalesInvoice();
                //frmsales.MdiParent = this;
                //frmsales.Text = "Sales Invoice";
                //frmsales.Show();
            }
            if(e.KeyCode==Keys.F5)
            {
                //Accounts.FrmCustomer ObjCustomer = new FrmCustomer();
                //ObjCustomer.MdiParent = this;
                //ObjCustomer.Text = "Customer";
                //ObjCustomer.Show();
            }
            if(e.KeyCode==Keys.F6)
            {
                //Accounts.FrmVendor Objvendor = new FrmVendor();
                //Objvendor.MdiParent = this;
                //Objvendor.Text = "Vendor";
                //Objvendor.Show();
            }
            
            if(e.KeyCode == Keys.ControlKey)
            {
                isCtrlPressed = true;
            }
            if (e.KeyCode == Keys.F2)
            {
                isF2Pressed = true;
            }
            if(isCtrlPressed && isF2Pressed)
            {
                
            }

        }
    }
}
