using ModelClass.TransactionModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ModelClass;
using Repository;

namespace Repository.TransactionRepository
{
    public class PurchaseReturnRepository : BaseRepostitory
    {
        public PReturnMaster GetById(Int64 Id)
        {
            PReturnMaster item = new PReturnMaster();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_PurchaseReturn, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", Id);
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            item = ds.Tables[0].Rows[0].ToNullableObject<PReturnMaster>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return item;
        }

        public PReturnDetailsGrid GetByIdPRD(Int64 Id)
        {
            PReturnDetailsGrid item = new PReturnDetailsGrid();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_PurchaseReturn, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", Id);
                    cmd.Parameters.AddWithValue("@_Operation", "GETBYID");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 1) && (ds.Tables[1] != null) && (ds.Tables[1].Rows.Count > 0))
                        {
                            item.List = ds.Tables[1].ToListOfObject<PReturnDetails>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return item;
        }

        public int PReturnNo = 0;
        public int GeneratePReturnNo(SqlTransaction trans = null)
        {
            int PReturnNo = 0;
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_PurchaseReturn, (SqlConnection)DataConnection, trans))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                    cmd.Parameters.AddWithValue("@_Operation", "GENERATENUMBER");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            PReturnNo = Convert.ToInt32(ds.Tables[0].Rows[0].ItemArray[0].ToString());
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
            return PReturnNo;
        }

        public string savePR(PReturnMaster pr, PReturnDetails details, DataGridView dgvInvoice)
        {
            DataConnection.Open();
            using (SqlTransaction trans = (SqlTransaction)DataConnection.BeginTransaction())
            {
                try
                {
                    pr.PReturnNo = this.GeneratePReturnNo(trans);
                    using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_PurchaseReturn, (SqlConnection)DataConnection, trans))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                        cmd.Parameters.AddWithValue("@FinYearId", SessionContext.FinYearId);
                        cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                        cmd.Parameters.AddWithValue("@PReturnNo", pr.PReturnNo);
                        cmd.Parameters.AddWithValue("@PReturnDate", pr.PReturnDate);
                        cmd.Parameters.AddWithValue("@PInvoice", pr.PInvoice);
                        cmd.Parameters.AddWithValue("@InvoiceNo", pr.InvoiceNo);
                        cmd.Parameters.AddWithValue("@InvoiceDate", pr.InvoiceDate);
                        cmd.Parameters.AddWithValue("@LedgerID", pr.LedgerID);
                        cmd.Parameters.AddWithValue("@VendorName", pr.VendorName);
                        cmd.Parameters.AddWithValue("@PaymodeID", pr.PaymodeID);
                        cmd.Parameters.AddWithValue("@Paymode", pr.Paymode);
                        cmd.Parameters.AddWithValue("@PaymodeLedgerID", pr.PaymodeLedgerID);
                        cmd.Parameters.AddWithValue("@CreditPeriod", pr.CreditPeriod);
                        cmd.Parameters.AddWithValue("@SubTotal", pr.SubTotal);
                        cmd.Parameters.AddWithValue("@SpDisPer", pr.SpDisPer);
                        cmd.Parameters.AddWithValue("@SpDsiAmt", pr.SpDsiAmt);
                        cmd.Parameters.AddWithValue("@BillDiscountPer", pr.BillDiscountPer);
                        cmd.Parameters.AddWithValue("@BillDiscountAmt", pr.BillDiscountAmt);
                        cmd.Parameters.AddWithValue("@TaxPer", pr.TaxPer);
                        cmd.Parameters.AddWithValue("@TaxAmt", pr.TaxAmt);
                        cmd.Parameters.AddWithValue("@Frieght", pr.Frieght);
                        cmd.Parameters.AddWithValue("@ExpenseAmt", pr.ExpenseAmt);
                        cmd.Parameters.AddWithValue("@OtherExpAmt", pr.OtherExpAmt);
                        cmd.Parameters.AddWithValue("@GrandTotal", pr.GrandTotal);
                        cmd.Parameters.AddWithValue("@CancelFlag", pr.CancelFlag);
                        cmd.Parameters.AddWithValue("@UserID", pr.UserID);
                        cmd.Parameters.AddWithValue("@UserName", pr.UserName);
                        cmd.Parameters.AddWithValue("@TaxType", pr.TaxType);
                        cmd.Parameters.AddWithValue("@Remarks", pr.Remarks);
                        cmd.Parameters.AddWithValue("@RoundOff", pr.RoundOff);
                        cmd.Parameters.AddWithValue("@CessPer", pr.CessPer);
                        cmd.Parameters.AddWithValue("@CessAmt", pr.CessAmt);
                        cmd.Parameters.AddWithValue("@CalAfterTax", pr.CalAfterTax);
                        cmd.Parameters.AddWithValue("@CurrencyID", pr.CurrencyID);
                        cmd.Parameters.AddWithValue("@CurSymbol", pr.CurSymbol);
                        cmd.Parameters.AddWithValue("@SeriesID", pr.SeriesID);
                        cmd.Parameters.AddWithValue("@VoucherID", pr.VoucherID);
                        cmd.Parameters.AddWithValue("@_Operation", "CREATE");

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                    }

                    // Iterate over DataGridView and insert PReturnDetails for each row
                    for (int i = 0; i < dgvInvoice.Rows.Count; i++)
                    {
                        details.CompanyId = SessionContext.CompanyId;
                        details.FinYearId = SessionContext.FinYearId;
                        details.BranchID = SessionContext.BranchId;
                        details.PReturnNo = pr.PReturnNo;
                        details.PReturnDate = pr.PReturnDate;
                        details.InvoiceNo = pr.InvoiceNo;
                        details.SlNo = i + 1;
                        details.ItemID = Convert.ToInt64(dgvInvoice.Rows[i].Cells["ItemID"].Value);
                        details.Description = dgvInvoice.Rows[i].Cells["Description"].Value.ToString();
                        details.UnitId = Convert.ToInt32(dgvInvoice.Rows[i].Cells["UnitId"].Value);
                        details.BaseUnit = Convert.ToBoolean(dgvInvoice.Rows[i].Cells["BaseUnit"].Value);
                        details.Packing = Convert.ToDouble(dgvInvoice.Rows[i].Cells["Packing"].Value);
                        details.IsExpiry = Convert.ToBoolean(dgvInvoice.Rows[i].Cells["IsExpiry"].Value);
                        details.BatchNo = dgvInvoice.Rows[i].Cells["BatchNo"].Value.ToString();

                        if (dgvInvoice.Rows[i].Cells["Expiry"].Value != DBNull.Value)
                            details.Expiry = Convert.ToDateTime(dgvInvoice.Rows[i].Cells["Expiry"].Value);

                        details.Qty = Convert.ToDouble(dgvInvoice.Rows[i].Cells["Qty"].Value);
                        details.TaxPer = Convert.ToDouble(dgvInvoice.Rows[i].Cells["TaxPer"].Value);
                        details.TaxAmt = Convert.ToDouble(dgvInvoice.Rows[i].Cells["TaxAmt"].Value);
                        details.Reason = dgvInvoice.Rows[i].Cells["Reason"].Value.ToString();
                        details.Free = Convert.ToDouble(dgvInvoice.Rows[i].Cells["Free"].Value);
                        details.Cost = Convert.ToDouble(dgvInvoice.Rows[i].Cells["Cost"].Value);
                        details.DisPer = Convert.ToDouble(dgvInvoice.Rows[i].Cells["DisPer"].Value);
                        details.DisAmt = Convert.ToDouble(dgvInvoice.Rows[i].Cells["DisAmt"].Value);
                        details.SalesPrice = Convert.ToDouble(dgvInvoice.Rows[i].Cells["SalesPrice"].Value);
                        details.OriginalCost = Convert.ToDouble(dgvInvoice.Rows[i].Cells["OriginalCost"].Value);
                        details.TotalSP = Convert.ToDouble(dgvInvoice.Rows[i].Cells["TotalSP"].Value);
                        details.TotalAmount = Convert.ToDouble(dgvInvoice.Rows[i].Cells["TotalAmount"].Value);
                        details.CessAmt = Convert.ToDouble(dgvInvoice.Rows[i].Cells["CessAmt"].Value);
                        details.CessPer = Convert.ToDouble(dgvInvoice.Rows[i].Cells["CessPer"].Value);

                        // Insert PReturnDetails for each item
                        using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_PReturnDetails, (SqlConnection)DataConnection, trans))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@CompanyId", details.CompanyId);
                            cmd.Parameters.AddWithValue("@FinYearId", details.FinYearId);
                            cmd.Parameters.AddWithValue("@BranchID", details.BranchID);
                            cmd.Parameters.AddWithValue("@PReturnNo", details.PReturnNo);
                            cmd.Parameters.AddWithValue("@PReturnDate", details.PReturnDate);
                            cmd.Parameters.AddWithValue("@InvoiceNo", details.InvoiceNo);
                            cmd.Parameters.AddWithValue("@SlNo", details.SlNo);
                            cmd.Parameters.AddWithValue("@ItemID", details.ItemID);
                            cmd.Parameters.AddWithValue("@Description", details.Description);
                            cmd.Parameters.AddWithValue("@UnitId", details.UnitId);
                            cmd.Parameters.AddWithValue("@BaseUnit", details.BaseUnit ? "Y" : "N");
                            cmd.Parameters.AddWithValue("@Packing", details.Packing);
                            cmd.Parameters.AddWithValue("@IsExpiry", details.IsExpiry);
                            cmd.Parameters.AddWithValue("@BatchNo", details.BatchNo);
                            cmd.Parameters.AddWithValue("@Expiry", details.Expiry == null ? DBNull.Value : (object)details.Expiry);
                            cmd.Parameters.AddWithValue("@Qty", details.Qty);
                            cmd.Parameters.AddWithValue("@TaxPer", details.TaxPer);
                            cmd.Parameters.AddWithValue("@TaxAmt", details.TaxAmt);
                            cmd.Parameters.AddWithValue("@Reason", details.Reason);
                            cmd.Parameters.AddWithValue("@Free", details.Free);
                            cmd.Parameters.AddWithValue("@Cost", details.Cost);
                            cmd.Parameters.AddWithValue("@DisPer", details.DisPer);
                            cmd.Parameters.AddWithValue("@DisAmt", details.DisAmt);
                            cmd.Parameters.AddWithValue("@SalesPrice", details.SalesPrice);
                            cmd.Parameters.AddWithValue("@OriginalCost", details.OriginalCost);
                            cmd.Parameters.AddWithValue("@TotalSP", details.TotalSP);
                            cmd.Parameters.AddWithValue("@TotalAmount", details.TotalAmount);
                            cmd.Parameters.AddWithValue("@CessAmt", details.CessAmt);
                            cmd.Parameters.AddWithValue("@CessPer", details.CessPer);
                            cmd.Parameters.AddWithValue("@_Operation", "CREATE");

                            SqlDataAdapter da = new SqlDataAdapter(cmd);
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                        }
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
                finally
                {
                    if (DataConnection.State == ConnectionState.Open)
                    {
                        DataConnection.Close();
                    }
                }
            }

            return "success";
        }

        public List<PReturnGetAll> GetAll()
        {
            List<PReturnGetAll> prList = new List<PReturnGetAll>();
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_PurchaseReturn, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");
                    cmd.Parameters.AddWithValue("@PageIndex", 0);
                    cmd.Parameters.AddWithValue("@PageSize", 100);

                    using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                    {
                        DataSet dt = new DataSet();
                        adp.Fill(dt);
                        if (dt.Tables[0].Rows.Count > 0)
                        {
                            prList = dt.Tables[0].ToListOfObject<PReturnGetAll>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            return prList;
        }

        public string UpdatePR(PReturnMaster pr)
        {
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_PurchaseReturn, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", pr.Id);
                    cmd.Parameters.AddWithValue("@CompanyId", pr.CompanyId);
                    cmd.Parameters.AddWithValue("@FinYearId", pr.FinYearId);
                    cmd.Parameters.AddWithValue("@BranchId", pr.BranchId);
                    cmd.Parameters.AddWithValue("@PReturnNo", pr.PReturnNo);
                    cmd.Parameters.AddWithValue("@PReturnDate", pr.PReturnDate);
                    cmd.Parameters.AddWithValue("@InvoiceNo", pr.InvoiceNo);
                    cmd.Parameters.AddWithValue("@InvoiceDate", pr.InvoiceDate);
                    cmd.Parameters.AddWithValue("@LedgerID", pr.LedgerID);
                    cmd.Parameters.AddWithValue("@PaymodeID", pr.PaymodeID);
                    cmd.Parameters.AddWithValue("@PaymodeLedgerID", pr.PaymodeLedgerID);
                    cmd.Parameters.AddWithValue("@CreditPeriod", pr.CreditPeriod);
                    cmd.Parameters.AddWithValue("@SubTotal", pr.SubTotal);
                    cmd.Parameters.AddWithValue("@SpDisPer", pr.SpDisPer);
                    cmd.Parameters.AddWithValue("@SpDsiAmt", pr.SpDsiAmt);
                    cmd.Parameters.AddWithValue("@BillDiscountPer", pr.BillDiscountPer);
                    cmd.Parameters.AddWithValue("@BillDiscountAmt", pr.BillDiscountAmt);
                    cmd.Parameters.AddWithValue("@TaxPer", pr.TaxPer);
                    cmd.Parameters.AddWithValue("@TaxAmt", pr.TaxAmt);
                    cmd.Parameters.AddWithValue("@Frieght", pr.Frieght);
                    cmd.Parameters.AddWithValue("@ExpenseAmt", pr.ExpenseAmt);
                    cmd.Parameters.AddWithValue("@OtherExpAmt", pr.OtherExpAmt);
                    cmd.Parameters.AddWithValue("@GrandTotal", pr.GrandTotal);
                    cmd.Parameters.AddWithValue("@CancelFlag", pr.CancelFlag);
                    cmd.Parameters.AddWithValue("@UserID", pr.UserID);
                    cmd.Parameters.AddWithValue("@TaxType", pr.TaxType);
                    cmd.Parameters.AddWithValue("@Remarks", pr.Remarks);
                    cmd.Parameters.AddWithValue("@RoundOff", pr.RoundOff);
                    cmd.Parameters.AddWithValue("@CessPer", pr.CessPer);
                    cmd.Parameters.AddWithValue("@CessAmt", pr.CessAmt);
                    cmd.Parameters.AddWithValue("@CalAfterTax", pr.CalAfterTax);
                    cmd.Parameters.AddWithValue("@_Operation", "UPDATE");

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            return "success";
        }

        public string DeletePR(int Id, int CompanyId, int FinYearId, int BranchId, int VoucherId, string VoucherType)
        {
            DataConnection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_PurchaseReturn, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", Id);
                    cmd.Parameters.AddWithValue("@CompanyId", CompanyId);
                    cmd.Parameters.AddWithValue("@FinYearId", FinYearId);
                    cmd.Parameters.AddWithValue("@BranchId", BranchId);
                    cmd.Parameters.AddWithValue("@VoucherID", VoucherId);
                    cmd.Parameters.AddWithValue("@VoucherType", VoucherType);
                    cmd.Parameters.AddWithValue("@_Operation", "DELETE");

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                {
                    DataConnection.Close();
                }
            }

            return "success";
        }
    }
}
