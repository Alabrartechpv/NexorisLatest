using System;
using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace Repository.SettingsRepo
{
    public sealed class PurchasePrintBranding
    {
        public string ShopName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public byte[] Logo { get; set; }
        public bool HideLogo { get; set; }
        public string BranchName { get; set; }
        public string DefaultShopName { get; set; }
        public string DefaultAddress1 { get; set; }
        public string DefaultAddress2 { get; set; }
        public string DefaultPhone { get; set; }
        public string DefaultEmail { get; set; }
        public byte[] CompanyLogo { get; set; }
    }

    public sealed class PurchasePrintBrandingRepository : BaseRepostitory
    {
        public PurchasePrintBranding Get(int companyId, int branchId)
        {
            const string defaults = @"
SELECT b.BranchName, c.CompanyName AS DefaultShopName, c.Address1 AS DefaultAddress1,
       c.Address2 AS DefaultAddress2,
       COALESCE(NULLIF(LTRIM(RTRIM(c.Phone)), ''), b.Phone) AS DefaultPhone,
       c.Email AS DefaultEmail, c.Logo AS CompanyLogo";
            const string source = @"
FROM dbo.Branches b
JOIN dbo.CompanyInfo c ON c.CompanyID = b.CompanyId";
            const string scope = " WHERE b.CompanyId = @companyId AND b.Id = @branchId";
            var keys = new { companyId, branchId };
            try
            {
                return DataConnection.QuerySingleOrDefault<PurchasePrintBranding>(
                    defaults + @", p.ShopName, p.Address1, p.Address2, p.Phone, p.Email, p.Logo,
                        ISNULL(p.HideLogo, 0) AS HideLogo" + source + @"
                    LEFT JOIN dbo.PurchasePrintBranding p ON p.CompanyId = b.CompanyId AND p.BranchId = b.Id" + scope,
                    keys);
            }
            catch (SqlException ex) when (ex.Number == 208)
            {
                return DataConnection.QuerySingleOrDefault<PurchasePrintBranding>(defaults + source + scope, keys);
            }
        }

        public void Save(int companyId, int branchId, PurchasePrintBranding branding)
        {
            const string sql = @"
IF NOT EXISTS (SELECT 1 FROM dbo.Branches WHERE CompanyId = @CompanyId AND Id = @BranchId)
    THROW 50000, 'The selected branch is not available for this company.', 1;

UPDATE dbo.PurchasePrintBranding WITH (UPDLOCK, SERIALIZABLE)
SET ShopName = @ShopName, Address1 = @Address1, Address2 = @Address2, Phone = @Phone,
    Email = @Email, Logo = @Logo, HideLogo = @HideLogo
WHERE CompanyId = @CompanyId AND BranchId = @BranchId;

IF @@ROWCOUNT = 0
    INSERT dbo.PurchasePrintBranding
        (CompanyId, BranchId, ShopName, Address1, Address2, Phone, Email, Logo, HideLogo)
    VALUES (@CompanyId, @BranchId, @ShopName, @Address1, @Address2, @Phone, @Email, @Logo, @HideLogo);";
            DataConnection.Open();
            try
            {
                using (var transaction = DataConnection.BeginTransaction(IsolationLevel.Serializable))
                {
                    DataConnection.Execute(sql, new
                    {
                        CompanyId = companyId,
                        BranchId = branchId,
                        branding.ShopName,
                        branding.Address1,
                        branding.Address2,
                        branding.Phone,
                        branding.Email,
                        branding.Logo,
                        branding.HideLogo
                    }, transaction);
                    transaction.Commit();
                }
            }
            finally
            {
                DataConnection.Close();
            }
        }
    }
}
