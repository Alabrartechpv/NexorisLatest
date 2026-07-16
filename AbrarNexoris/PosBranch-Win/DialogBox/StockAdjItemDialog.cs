using PosBranch_Win.Transaction;
using System;
using System.Windows.Forms;

namespace PosBranch_Win.DialogBox
{
    public partial class StockAdjItemDialog : Form
    {
        FrmStockAdjustment frmStock = new FrmStockAdjustment();
        public StockAdjItemDialog()
        {
            InitializeComponent();
        }

        private void dgv_Item_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int index = 0;
                if (dgv_Item.Rows.Count > 0)
                {
                    e.SuppressKeyPress = true;
                    int currentRowIndex = dgv_Item.CurrentRow.Index;

                    frmStock = (FrmStockAdjustment)Application.OpenForms["FrmStockAdjustment"];

                    if (index >= 0 && index < dgv_Item.Rows.Count)
                    {
                        // Use the new AddItemToGrid method instead of directly accessing dgv_stockadjustment
                        string itemId = dgv_Item.Rows[currentRowIndex].Cells["ItemId"].Value.ToString();
                        string barcode = dgv_Item.Rows[currentRowIndex].Cells["BarCode"].Value.ToString();
                        string description = dgv_Item.Rows[currentRowIndex].Cells["Description"].Value.ToString();
                        string unit = dgv_Item.Rows[currentRowIndex].Cells["Unit"].Value.ToString();
                        string stock = dgv_Item.Rows[currentRowIndex].Cells["Stock"].Value.ToString();

                        // Add the item to the grid
                        frmStock.AddItemToGrid(
                            itemId,       // Item ID
                            barcode,      // Barcode
                            description,  // Description
                            unit,         // UOM
                            stock,        // Qty On Hand
                            1             // Default Adj Qty to 1
                        );

                        frmStock.SetUpdateMode();

                        // Close the dialog after adding the item
                        this.Close();
                    }
                }
            }
        }
    }
}
