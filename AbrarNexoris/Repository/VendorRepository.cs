using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using ModelClass;
using Dapper;

namespace Repository
{
    public class VendorRepository : BaseRepostitory
    {
        public string SaveVendor(ClsVendors objVendor, VendorAddress objVendorAddr)
        {
            DataConnection.Open();
            var trans = DataConnection.BeginTransaction();
            try
            {
                // Create a clean object with only the fields that POS_Vendor stored procedure expects
                var vendorForLedger = new
                {
                    CompanyId = objVendor.CompanyId,
                    BranchId = objVendor.BranchId,
                    LedgerId = objVendor.LedgerId,
                    LedgerName = objVendor.LedgerName,
                    Alias = objVendor.Alias,
                    Description = objVendor.Description,
                    Notes = objVendor.Notes,
                    OpnDebit = objVendor.OpnDebit,
                    OpnCredit = objVendor.OpnCredit,
                    _Operation = "GENERATELEDGER"
                };

                List<ClsVendors> GetGenLedgerNO = DataConnection.Query<ClsVendors>(STOREDPROCEDURE.POS_Vendor, vendorForLedger, trans, commandType: CommandType.StoredProcedure).ToList<ClsVendors>();

                if (GetGenLedgerNO.Count > 0)
                {
                    foreach (ClsVendors LedgerNo in GetGenLedgerNO)
                    {
                        objVendor.LedgerId = LedgerNo.LedgerId;
                        objVendorAddr.LedgerId = LedgerNo.LedgerId;
                        objVendorAddr.LedgerName = objVendor.LedgerName;
                        objVendorAddr.CompanyId = objVendor.CompanyId;
                        objVendorAddr.BranchId = objVendor.BranchId;
                    }
                }

                // Create a clean object for CREATE operation
                var vendorForCreate = new
                {
                    CompanyId = objVendor.CompanyId,
                    BranchId = objVendor.BranchId,
                    LedgerId = objVendor.LedgerId,
                    LedgerName = objVendor.LedgerName,
                    Alias = objVendor.Alias,
                    Description = objVendor.Description,
                    Notes = objVendor.Notes,
                    OpnDebit = objVendor.OpnDebit,
                    OpnCredit = objVendor.OpnCredit,
                    _Operation = "CREATE"
                };

                List<ClsVendors> ListVendor = DataConnection.Query<ClsVendors>(STOREDPROCEDURE.POS_Vendor, vendorForCreate, trans, commandType: CommandType.StoredProcedure).ToList<ClsVendors>();

                objVendorAddr._Operation = "CREATE";
                DataConnection.Query<VendorAddress>(STOREDPROCEDURE.POS_Vendor_ContactDetails, objVendorAddr, trans, commandType: CommandType.StoredProcedure).ToList<VendorAddress>();
                // Always commit — INSERTs don't return rows, so there's no count to check
                trans.Commit();
            }
            catch (Exception)
            {
                trans.Rollback();
                throw;
            }
            finally
            {
                if (DataConnection.State == ConnectionState.Open)
                    DataConnection.Close();
            }
            return "Success";
        }

        public VendorDDLGrid GetVendorDDL()
        {
            VendorDDLGrid objVendorDDLGrid = new VendorDDLGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Vendor, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@_Operation", "GETALL");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            objVendorDDLGrid.List = ds.Tables[0].ToListOfObject<VendorGridList>();
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

            return objVendorDDLGrid;
        }

        public VendorAddressDDLGrid getVendorAddress(int LedgerId)
        {
            VendorAddressDDLGrid objVendorAddress = new VendorAddressDDLGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Vendor, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@LedgerId", LedgerId);
                    cmd.Parameters.AddWithValue("@_Operation", "GetById");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            objVendorAddress.ListVendor = ds.Tables[0].ToListOfObject<ClsVendors>();
                        }

                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[1] != null) && (ds.Tables[1].Rows.Count > 0))
                        {
                            objVendorAddress.ListVendorAddress = ds.Tables[1].ToListOfObject<VendorAddress>();
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

            return objVendorAddress;
        }

        public string UpdateVendorAddress(ClsVendors objVendor, VendorAddress objVendorAddress)
        {
            DataConnection.Open();
            var trans = DataConnection.BeginTransaction();
            try
            {
                // Create a clean object with only the fields that POS_Vendor stored procedure expects
                var vendorForUpdate = new
                {
                    CompanyId = objVendor.CompanyId,
                    BranchId = objVendor.BranchId,
                    LedgerId = objVendor.LedgerId,
                    LedgerName = objVendor.LedgerName,
                    Alias = objVendor.Alias,
                    Description = objVendor.Description,
                    Notes = objVendor.Notes,
                    OpnDebit = objVendor.OpnDebit,
                    OpnCredit = objVendor.OpnCredit,
                    _Operation = "Update"
                };

                List<ClsVendors> ListObjVendor = DataConnection.Query<ClsVendors>(STOREDPROCEDURE.POS_Vendor, vendorForUpdate, trans, commandType: CommandType.StoredProcedure).ToList<ClsVendors>();

                objVendorAddress._Operation = "Update";
                List<VendorAddress> ListObjVendorAddress = DataConnection.Query<VendorAddress>(STOREDPROCEDURE.POS_Vendor_ContactDetails, objVendorAddress, trans, commandType: CommandType.StoredProcedure).ToList<VendorAddress>();

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
                    DataConnection.Close();
            }

            return "SUCCESS";
        }

        public VendorDDLGrid SearchVendors(string searchText)
        {
            VendorDDLGrid objVendorDDLGrid = new VendorDDLGrid();
            DataConnection.Open();
            try
            {
                using (SqlCommand cmd = new SqlCommand(STOREDPROCEDURE.POS_Vendor, (SqlConnection)DataConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyId", SessionContext.CompanyId);
                    cmd.Parameters.AddWithValue("@BranchId", SessionContext.BranchId);
                    cmd.Parameters.AddWithValue("@LedgerName", searchText);
                    cmd.Parameters.AddWithValue("@_Operation", "Search");

                    using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                    {
                        DataSet ds = new DataSet();
                        adapt.Fill(ds);
                        if ((ds != null) && (ds.Tables.Count > 0) && (ds.Tables[0] != null) && (ds.Tables[0].Rows.Count > 0))
                        {
                            objVendorDDLGrid.List = ds.Tables[0].ToListOfObject<VendorGridList>();
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

            return objVendorDDLGrid;
        }

        public ClsVendors GetVendorById(int vendorId)
        {
            DataConnection.Open();
            try
            {
                string query = @"
                    SELECT l.LedgerID, l.LedgerName, l.Alias, l.Description, l.Notes, 
                           l.OpnDebit, l.OpnCredit, 
                           l.CompanyID as CompanyId, l.BranchID as BranchId,
                           ISNULL(cd.SSMNumber, '') as SSMNumber, 
                           ISNULL(cd.TINNumber, '') as TINNumber, 
                           ISNULL(cd.CompanyName, '') as CompanyName, 
                           ISNULL(cd.CompanyTIN, '') as CompanyTIN, 
                           ISNULL(cd.CompanyMSICCode, '') as CompanyMSICCode, 
                           ISNULL(cd.CompanyEmail, '') as CompanyEmail
                    FROM LedgerMaster l 
                    LEFT JOIN ContactDetails cd ON l.LedgerID = cd.LedgerID
                    WHERE l.LedgerID = @VendorId AND l.GroupID = 17";

                var vendor = DataConnection.Query<ClsVendors>(query, new { VendorId = vendorId }).FirstOrDefault();
                return vendor;
            }
            finally
            {
                DataConnection.Close();
            }
        }
    }
}
