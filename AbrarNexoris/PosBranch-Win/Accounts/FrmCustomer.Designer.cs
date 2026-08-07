using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.Misc;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win.UltraWinGrid;
using Infragistics.Win.UltraWinToolbars;

namespace PosBranch_Win.Accounts
{
    partial class FrmCustomer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Infragistics.Win.Appearance appearance1 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance2 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance3 = new Infragistics.Win.Appearance();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmCustomer));
            Infragistics.Win.Appearance appearance4 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance5 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance6 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance7 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance8 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance9 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance10 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance11 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance12 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance13 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance14 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance15 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance16 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance17 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance18 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance19 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance20 = new Infragistics.Win.Appearance();
            this.ultraPanel1 = new Infragistics.Win.Misc.UltraPanel();
            this.ultraLabelTitle = new Infragistics.Win.Misc.UltraLabel();
            this.ultraGroupBoxBasicInfo = new Infragistics.Win.Misc.UltraGroupBox();
            this.button4 = new System.Windows.Forms.Button();
            this.ultraTextCustomer = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.ultraLabelCustomer = new Infragistics.Win.Misc.UltraLabel();
            this.ultraTextAliasName = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.ultraLabelAlias = new Infragistics.Win.Misc.UltraLabel();
            this.ultraComboPriceLevel = new Infragistics.Win.UltraWinEditors.UltraComboEditor();
            this.ultraLabelPriceLevel = new Infragistics.Win.Misc.UltraLabel();
            this.ultraGroupBoxContact = new Infragistics.Win.Misc.UltraGroupBox();
            this.ultraTextEmail = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.ultraLabelEmail = new Infragistics.Win.Misc.UltraLabel();
            this.ultraTextPhone = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.ultraLabelPhone = new Infragistics.Win.Misc.UltraLabel();
            this.ultraGroupBoxFinancial = new Infragistics.Win.Misc.UltraGroupBox();
            this.ultraTextOpenDebit = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.ultraLabelOpenDebit = new Infragistics.Win.Misc.UltraLabel();
            this.ultraTextOpenCredit = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.ultraLabelOpenCredit = new Infragistics.Win.Misc.UltraLabel();
            this.ultraTextSSMNumber = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.ultraLabelSSMNumber = new Infragistics.Win.Misc.UltraLabel();
            this.ultraTextTINNumber = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.ultraLabelTINNumber = new Infragistics.Win.Misc.UltraLabel();
            this.ultraGroupBoxCompany = new Infragistics.Win.Misc.UltraGroupBox();
            this.ultraTextCompanyName = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.ultraLabelCompanyName = new Infragistics.Win.Misc.UltraLabel();
            this.ultraTextCompanyTIN = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.ultraLabelCompanyTIN = new Infragistics.Win.Misc.UltraLabel();
            this.ultraTextCompanyMSIC = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.ultraLabelCompanyMSIC = new Infragistics.Win.Misc.UltraLabel();
            this.ultraTextCompanyEmail = new Infragistics.Win.UltraWinEditors.UltraTextEditor();
            this.ultraLabelCompanyEmail = new Infragistics.Win.Misc.UltraLabel();

            this.ultraPanel1.ClientArea.SuspendLayout();
            this.ultraPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxBasicInfo)).BeginInit();
            this.ultraGroupBoxBasicInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextCustomer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextAliasName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboPriceLevel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxContact)).BeginInit();
            this.ultraGroupBoxContact.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextEmail)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextPhone)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxFinancial)).BeginInit();
            this.ultraGroupBoxFinancial.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextOpenDebit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextOpenCredit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextSSMNumber)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextTINNumber)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxCompany)).BeginInit();
            this.ultraGroupBoxCompany.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextCompanyName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextCompanyTIN)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextCompanyMSIC)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextCompanyEmail)).BeginInit();

            this.SuspendLayout();
            // 
            // ultraPanel1
            // 
            appearance1.BackColor = System.Drawing.Color.SkyBlue;
            appearance1.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(233)))), ((int)(((byte)(236)))), ((int)(((byte)(239)))));
            appearance1.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            this.ultraPanel1.Appearance = appearance1;
            // 
            // ultraPanel1.ClientArea
            // 
            this.ultraPanel1.ClientArea.Controls.Add(this.ultraLabelTitle);
            this.ultraPanel1.ClientArea.Controls.Add(this.ultraGroupBoxBasicInfo);
            this.ultraPanel1.ClientArea.Controls.Add(this.ultraGroupBoxContact);
            this.ultraPanel1.ClientArea.Controls.Add(this.ultraGroupBoxFinancial);
            this.ultraPanel1.ClientArea.Controls.Add(this.ultraGroupBoxCompany);
            this.ultraPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ultraPanel1.Location = new System.Drawing.Point(0, 0);
            this.ultraPanel1.Name = "ultraPanel1";
            this.ultraPanel1.Size = new System.Drawing.Size(1349, 520);
            this.ultraPanel1.TabIndex = 1;
            // 
            // ultraLabelTitle
            // 
            appearance2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            appearance2.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(184)))));
            appearance2.BackGradientStyle = Infragistics.Win.GradientStyle.Vertical;
            appearance2.FontData.BoldAsString = "True";
            appearance2.FontData.Name = "Segoe UI";
            appearance2.FontData.SizeInPoints = 20F;
            appearance2.ForeColor = System.Drawing.Color.White;
            appearance2.TextHAlignAsString = "Center";
            this.ultraLabelTitle.Appearance = appearance2;
            this.ultraLabelTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.ultraLabelTitle.Location = new System.Drawing.Point(0, 0);
            this.ultraLabelTitle.Name = "ultraLabelTitle";
            this.ultraLabelTitle.Size = new System.Drawing.Size(1349, 42);
            this.ultraLabelTitle.TabIndex = 0;
            this.ultraLabelTitle.Text = "👥 Customer Management System";
            // 
            // ultraGroupBoxBasicInfo
            // 
            appearance3.BackColor = System.Drawing.Color.Transparent;
            this.ultraGroupBoxBasicInfo.Appearance = appearance3;
            this.ultraGroupBoxBasicInfo.Controls.Add(this.button4);
            this.ultraGroupBoxBasicInfo.Controls.Add(this.ultraTextCustomer);
            this.ultraGroupBoxBasicInfo.Controls.Add(this.ultraLabelCustomer);
            this.ultraGroupBoxBasicInfo.Controls.Add(this.ultraTextAliasName);
            this.ultraGroupBoxBasicInfo.Controls.Add(this.ultraLabelAlias);
            this.ultraGroupBoxBasicInfo.Controls.Add(this.ultraComboPriceLevel);
            this.ultraGroupBoxBasicInfo.Controls.Add(this.ultraLabelPriceLevel);
            this.ultraGroupBoxBasicInfo.HeaderBorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded1;
            this.ultraGroupBoxBasicInfo.Location = new System.Drawing.Point(11, 60);
            this.ultraGroupBoxBasicInfo.Name = "ultraGroupBoxBasicInfo";
            this.ultraGroupBoxBasicInfo.Size = new System.Drawing.Size(440, 145);
            this.ultraGroupBoxBasicInfo.TabIndex = 1;
            this.ultraGroupBoxBasicInfo.Text = "Basic Information";
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.Color.Transparent;
            this.button4.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("button4.BackgroundImage")));
            this.button4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button4.Location = new System.Drawing.Point(396, 32);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(22, 22);
            this.button4.TabIndex = 12;
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // ultraTextCustomer
            // 
            appearance5.BackColor = System.Drawing.Color.White;
            appearance5.BorderColor = System.Drawing.SystemColors.Highlight;
            this.ultraTextCustomer.Appearance = appearance5;
            this.ultraTextCustomer.BackColor = System.Drawing.Color.White;
            this.ultraTextCustomer.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraTextCustomer.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraTextCustomer.Location = new System.Drawing.Point(100, 30);
            this.ultraTextCustomer.Name = "ultraTextCustomer";
            this.ultraTextCustomer.Size = new System.Drawing.Size(320, 27);
            this.ultraTextCustomer.TabIndex = 3;
            this.ultraTextCustomer.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraLabelCustomer
            // 
            this.ultraLabelCustomer.AutoSize = true;
            this.ultraLabelCustomer.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraLabelCustomer.ForeColor = System.Drawing.Color.Black;
            this.ultraLabelCustomer.Location = new System.Drawing.Point(20, 33);
            this.ultraLabelCustomer.Name = "ultraLabelCustomer";
            this.ultraLabelCustomer.Size = new System.Drawing.Size(78, 22);
            this.ultraLabelCustomer.TabIndex = 2;
            this.ultraLabelCustomer.Text = "Customer:";
            // 
            // ultraTextAliasName
            // 
            appearance6.BackColor = System.Drawing.Color.White;
            appearance6.BorderColor = System.Drawing.SystemColors.Highlight;
            this.ultraTextAliasName.Appearance = appearance6;
            this.ultraTextAliasName.BackColor = System.Drawing.Color.White;
            this.ultraTextAliasName.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraTextAliasName.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraTextAliasName.Location = new System.Drawing.Point(100, 65);
            this.ultraTextAliasName.Name = "ultraTextAliasName";
            this.ultraTextAliasName.Size = new System.Drawing.Size(320, 27);
            this.ultraTextAliasName.TabIndex = 5;
            this.ultraTextAliasName.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraLabelAlias
            // 
            this.ultraLabelAlias.AutoSize = true;
            this.ultraLabelAlias.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraLabelAlias.ForeColor = System.Drawing.Color.Black;
            this.ultraLabelAlias.Location = new System.Drawing.Point(20, 68);
            this.ultraLabelAlias.Name = "ultraLabelAlias";
            this.ultraLabelAlias.Size = new System.Drawing.Size(43, 22);
            this.ultraLabelAlias.TabIndex = 4;
            this.ultraLabelAlias.Text = "Alias:";
            // 
            // ultraComboPriceLevel
            // 
            appearance7.BackColor = System.Drawing.Color.White;
            appearance7.BorderColor = System.Drawing.SystemColors.Highlight;
            this.ultraComboPriceLevel.Appearance = appearance7;
            this.ultraComboPriceLevel.BackColor = System.Drawing.Color.White;
            this.ultraComboPriceLevel.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraComboPriceLevel.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraComboPriceLevel.Location = new System.Drawing.Point(100, 100);
            this.ultraComboPriceLevel.Name = "ultraComboPriceLevel";
            this.ultraComboPriceLevel.Size = new System.Drawing.Size(320, 27);
            this.ultraComboPriceLevel.TabIndex = 11;
            this.ultraComboPriceLevel.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraLabelPriceLevel
            // 
            this.ultraLabelPriceLevel.AutoSize = true;
            this.ultraLabelPriceLevel.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraLabelPriceLevel.ForeColor = System.Drawing.Color.Black;
            this.ultraLabelPriceLevel.Location = new System.Drawing.Point(20, 103);
            this.ultraLabelPriceLevel.Name = "ultraLabelPriceLevel";
            this.ultraLabelPriceLevel.Size = new System.Drawing.Size(85, 22);
            this.ultraLabelPriceLevel.TabIndex = 10;
            this.ultraLabelPriceLevel.Text = "Price Level:";
            // 
            // ultraGroupBoxContact
            // 
            this.ultraGroupBoxContact.Appearance = appearance3;
            this.ultraGroupBoxContact.Controls.Add(this.ultraTextEmail);
            this.ultraGroupBoxContact.Controls.Add(this.ultraLabelEmail);
            this.ultraGroupBoxContact.Controls.Add(this.ultraTextPhone);
            this.ultraGroupBoxContact.Controls.Add(this.ultraLabelPhone);
            this.ultraGroupBoxContact.HeaderBorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded1;
            this.ultraGroupBoxContact.Location = new System.Drawing.Point(460, 60);
            this.ultraGroupBoxContact.Name = "ultraGroupBoxContact";
            this.ultraGroupBoxContact.Size = new System.Drawing.Size(440, 120);
            this.ultraGroupBoxContact.TabIndex = 2;
            this.ultraGroupBoxContact.Text = "Contact Information";
            // 
            // ultraTextEmail
            // 
            appearance8.BackColor = System.Drawing.Color.White;
            appearance8.BorderColor = System.Drawing.SystemColors.Highlight;
            this.ultraTextEmail.Appearance = appearance8;
            this.ultraTextEmail.BackColor = System.Drawing.Color.White;
            this.ultraTextEmail.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraTextEmail.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraTextEmail.Location = new System.Drawing.Point(80, 30);
            this.ultraTextEmail.Name = "ultraTextEmail";
            this.ultraTextEmail.Size = new System.Drawing.Size(340, 27);
            this.ultraTextEmail.TabIndex = 7;
            this.ultraTextEmail.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraLabelEmail
            // 
            this.ultraLabelEmail.AutoSize = true;
            this.ultraLabelEmail.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraLabelEmail.ForeColor = System.Drawing.Color.Black;
            this.ultraLabelEmail.Location = new System.Drawing.Point(20, 33);
            this.ultraLabelEmail.Name = "ultraLabelEmail";
            this.ultraLabelEmail.Size = new System.Drawing.Size(48, 22);
            this.ultraLabelEmail.TabIndex = 6;
            this.ultraLabelEmail.Text = "Email:";
            // 
            // ultraTextPhone
            // 
            appearance9.BackColor = System.Drawing.Color.White;
            appearance9.BorderColor = System.Drawing.SystemColors.Highlight;
            this.ultraTextPhone.Appearance = appearance9;
            this.ultraTextPhone.BackColor = System.Drawing.Color.White;
            this.ultraTextPhone.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraTextPhone.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraTextPhone.Location = new System.Drawing.Point(80, 65);
            this.ultraTextPhone.Name = "ultraTextPhone";
            this.ultraTextPhone.Size = new System.Drawing.Size(340, 27);
            this.ultraTextPhone.TabIndex = 9;
            this.ultraTextPhone.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraLabelPhone
            // 
            this.ultraLabelPhone.AutoSize = true;
            this.ultraLabelPhone.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraLabelPhone.ForeColor = System.Drawing.Color.Black;
            this.ultraLabelPhone.Location = new System.Drawing.Point(20, 68);
            this.ultraLabelPhone.Name = "ultraLabelPhone";
            this.ultraLabelPhone.Size = new System.Drawing.Size(54, 22);
            this.ultraLabelPhone.TabIndex = 8;
            this.ultraLabelPhone.Text = "Phone:";
            // 
            // ultraGroupBoxFinancial
            // 
            this.ultraGroupBoxFinancial.Appearance = appearance3;
            this.ultraGroupBoxFinancial.Controls.Add(this.ultraTextOpenDebit);
            this.ultraGroupBoxFinancial.Controls.Add(this.ultraLabelOpenDebit);
            this.ultraGroupBoxFinancial.Controls.Add(this.ultraTextOpenCredit);
            this.ultraGroupBoxFinancial.Controls.Add(this.ultraLabelOpenCredit);
            this.ultraGroupBoxFinancial.Controls.Add(this.ultraTextSSMNumber);
            this.ultraGroupBoxFinancial.Controls.Add(this.ultraLabelSSMNumber);
            this.ultraGroupBoxFinancial.Controls.Add(this.ultraTextTINNumber);
            this.ultraGroupBoxFinancial.Controls.Add(this.ultraLabelTINNumber);
            this.ultraGroupBoxFinancial.HeaderBorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded1;
            this.ultraGroupBoxFinancial.Location = new System.Drawing.Point(914, 60);
            this.ultraGroupBoxFinancial.Name = "ultraGroupBoxFinancial";
            this.ultraGroupBoxFinancial.Size = new System.Drawing.Size(432, 180);
            this.ultraGroupBoxFinancial.TabIndex = 3;
            this.ultraGroupBoxFinancial.Text = "Financial & Legal Information";
            // 
            // ultraTextOpenDebit
            // 
            appearance10.BackColor = System.Drawing.Color.White;
            appearance10.BorderColor = System.Drawing.SystemColors.Highlight;
            this.ultraTextOpenDebit.Appearance = appearance10;
            this.ultraTextOpenDebit.BackColor = System.Drawing.Color.White;
            this.ultraTextOpenDebit.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraTextOpenDebit.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraTextOpenDebit.Location = new System.Drawing.Point(115, 30);
            this.ultraTextOpenDebit.Name = "ultraTextOpenDebit";
            this.ultraTextOpenDebit.Size = new System.Drawing.Size(308, 27);
            this.ultraTextOpenDebit.TabIndex = 13;
            this.ultraTextOpenDebit.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraLabelOpenDebit
            // 
            this.ultraLabelOpenDebit.AutoSize = true;
            this.ultraLabelOpenDebit.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraLabelOpenDebit.ForeColor = System.Drawing.Color.Black;
            this.ultraLabelOpenDebit.Location = new System.Drawing.Point(14, 33);
            this.ultraLabelOpenDebit.Name = "ultraLabelOpenDebit";
            this.ultraLabelOpenDebit.Size = new System.Drawing.Size(91, 22);
            this.ultraLabelOpenDebit.TabIndex = 12;
            this.ultraLabelOpenDebit.Text = "Open Debit:";
            // 
            // ultraTextOpenCredit
            // 
            appearance11.BackColor = System.Drawing.Color.White;
            appearance11.BorderColor = System.Drawing.SystemColors.Highlight;
            this.ultraTextOpenCredit.Appearance = appearance11;
            this.ultraTextOpenCredit.BackColor = System.Drawing.Color.White;
            this.ultraTextOpenCredit.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraTextOpenCredit.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraTextOpenCredit.Location = new System.Drawing.Point(115, 101);
            this.ultraTextOpenCredit.Name = "ultraTextOpenCredit";
            this.ultraTextOpenCredit.Size = new System.Drawing.Size(308, 27);
            this.ultraTextOpenCredit.TabIndex = 15;
            this.ultraTextOpenCredit.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraLabelOpenCredit
            // 
            this.ultraLabelOpenCredit.AutoSize = true;
            this.ultraLabelOpenCredit.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraLabelOpenCredit.ForeColor = System.Drawing.Color.Black;
            this.ultraLabelOpenCredit.Location = new System.Drawing.Point(11, 104);
            this.ultraLabelOpenCredit.Name = "ultraLabelOpenCredit";
            this.ultraLabelOpenCredit.Size = new System.Drawing.Size(95, 22);
            this.ultraLabelOpenCredit.TabIndex = 14;
            this.ultraLabelOpenCredit.Text = "Open Credit:";
            // 
            // ultraTextSSMNumber
            // 
            appearance12.BackColor = System.Drawing.Color.White;
            appearance12.BorderColor = System.Drawing.SystemColors.Highlight;
            this.ultraTextSSMNumber.Appearance = appearance12;
            this.ultraTextSSMNumber.BackColor = System.Drawing.Color.White;
            this.ultraTextSSMNumber.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraTextSSMNumber.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraTextSSMNumber.Location = new System.Drawing.Point(115, 65);
            this.ultraTextSSMNumber.Name = "ultraTextSSMNumber";
            this.ultraTextSSMNumber.Size = new System.Drawing.Size(308, 27);
            this.ultraTextSSMNumber.TabIndex = 14;
            this.ultraTextSSMNumber.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraLabelSSMNumber
            // 
            this.ultraLabelSSMNumber.AutoSize = true;
            this.ultraLabelSSMNumber.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraLabelSSMNumber.ForeColor = System.Drawing.Color.Black;
            this.ultraLabelSSMNumber.Location = new System.Drawing.Point(4, 68);
            this.ultraLabelSSMNumber.Name = "ultraLabelSSMNumber";
            this.ultraLabelSSMNumber.Size = new System.Drawing.Size(104, 22);
            this.ultraLabelSSMNumber.TabIndex = 13;
            this.ultraLabelSSMNumber.Text = "SSM Number:";
            // 
            // ultraTextTINNumber
            // 
            appearance13.BackColor = System.Drawing.Color.White;
            appearance13.BorderColor = System.Drawing.SystemColors.Highlight;
            this.ultraTextTINNumber.Appearance = appearance13;
            this.ultraTextTINNumber.BackColor = System.Drawing.Color.White;
            this.ultraTextTINNumber.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraTextTINNumber.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraTextTINNumber.Location = new System.Drawing.Point(115, 133);
            this.ultraTextTINNumber.Name = "ultraTextTINNumber";
            this.ultraTextTINNumber.Size = new System.Drawing.Size(308, 27);
            this.ultraTextTINNumber.TabIndex = 15;
            this.ultraTextTINNumber.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraLabelTINNumber
            // 
            this.ultraLabelTINNumber.AutoSize = true;
            this.ultraLabelTINNumber.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraLabelTINNumber.ForeColor = System.Drawing.Color.Black;
            this.ultraLabelTINNumber.Location = new System.Drawing.Point(11, 137);
            this.ultraLabelTINNumber.Name = "ultraLabelTINNumber";
            this.ultraLabelTINNumber.Size = new System.Drawing.Size(98, 22);
            this.ultraLabelTINNumber.TabIndex = 14;
            this.ultraLabelTINNumber.Text = "TIN Number:";
            // 
            // ultraGroupBoxCompany
            // 
            this.ultraGroupBoxCompany.Appearance = appearance3;
            this.ultraGroupBoxCompany.Controls.Add(this.ultraTextCompanyName);
            this.ultraGroupBoxCompany.Controls.Add(this.ultraLabelCompanyName);
            this.ultraGroupBoxCompany.Controls.Add(this.ultraTextCompanyTIN);
            this.ultraGroupBoxCompany.Controls.Add(this.ultraLabelCompanyTIN);
            this.ultraGroupBoxCompany.Controls.Add(this.ultraTextCompanyMSIC);
            this.ultraGroupBoxCompany.Controls.Add(this.ultraLabelCompanyMSIC);
            this.ultraGroupBoxCompany.Controls.Add(this.ultraTextCompanyEmail);
            this.ultraGroupBoxCompany.Controls.Add(this.ultraLabelCompanyEmail);
            this.ultraGroupBoxCompany.HeaderBorderStyle = Infragistics.Win.UIElementBorderStyle.Rounded1;
            this.ultraGroupBoxCompany.Location = new System.Drawing.Point(11, 250);
            this.ultraGroupBoxCompany.Name = "ultraGroupBoxCompany";
            this.ultraGroupBoxCompany.Size = new System.Drawing.Size(1335, 120);
            this.ultraGroupBoxCompany.TabIndex = 4;
            this.ultraGroupBoxCompany.Text = "Company Information";
            // 
            // ultraTextCompanyName
            // 
            appearance14.BackColor = System.Drawing.Color.White;
            appearance14.BorderColor = System.Drawing.SystemColors.Highlight;
            this.ultraTextCompanyName.Appearance = appearance14;
            this.ultraTextCompanyName.BackColor = System.Drawing.Color.White;
            this.ultraTextCompanyName.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraTextCompanyName.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraTextCompanyName.Location = new System.Drawing.Point(132, 30);
            this.ultraTextCompanyName.Name = "ultraTextCompanyName";
            this.ultraTextCompanyName.Size = new System.Drawing.Size(300, 27);
            this.ultraTextCompanyName.TabIndex = 16;
            this.ultraTextCompanyName.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraLabelCompanyName
            // 
            this.ultraLabelCompanyName.AutoSize = true;
            this.ultraLabelCompanyName.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraLabelCompanyName.ForeColor = System.Drawing.Color.Black;
            this.ultraLabelCompanyName.Location = new System.Drawing.Point(23, 33);
            this.ultraLabelCompanyName.Name = "ultraLabelCompanyName";
            this.ultraLabelCompanyName.Size = new System.Drawing.Size(77, 22);
            this.ultraLabelCompanyName.TabIndex = 15;
            this.ultraLabelCompanyName.Text = "Company:";
            // 
            // ultraTextCompanyTIN
            // 
            appearance15.BackColor = System.Drawing.Color.White;
            appearance15.BorderColor = System.Drawing.SystemColors.Highlight;
            this.ultraTextCompanyTIN.Appearance = appearance15;
            this.ultraTextCompanyTIN.BackColor = System.Drawing.Color.White;
            this.ultraTextCompanyTIN.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraTextCompanyTIN.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraTextCompanyTIN.Location = new System.Drawing.Point(132, 65);
            this.ultraTextCompanyTIN.Name = "ultraTextCompanyTIN";
            this.ultraTextCompanyTIN.Size = new System.Drawing.Size(300, 27);
            this.ultraTextCompanyTIN.TabIndex = 17;
            this.ultraTextCompanyTIN.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraLabelCompanyTIN
            // 
            this.ultraLabelCompanyTIN.AutoSize = true;
            this.ultraLabelCompanyTIN.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraLabelCompanyTIN.ForeColor = System.Drawing.Color.Black;
            this.ultraLabelCompanyTIN.Location = new System.Drawing.Point(23, 68);
            this.ultraLabelCompanyTIN.Name = "ultraLabelCompanyTIN";
            this.ultraLabelCompanyTIN.Size = new System.Drawing.Size(106, 22);
            this.ultraLabelCompanyTIN.TabIndex = 16;
            this.ultraLabelCompanyTIN.Text = "Company TIN:";
            // 
            // ultraTextCompanyMSIC
            // 
            appearance16.BackColor = System.Drawing.Color.White;
            appearance16.BorderColor = System.Drawing.SystemColors.Highlight;
            this.ultraTextCompanyMSIC.Appearance = appearance16;
            this.ultraTextCompanyMSIC.BackColor = System.Drawing.Color.White;
            this.ultraTextCompanyMSIC.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraTextCompanyMSIC.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraTextCompanyMSIC.Location = new System.Drawing.Point(570, 65);
            this.ultraTextCompanyMSIC.Name = "ultraTextCompanyMSIC";
            this.ultraTextCompanyMSIC.Size = new System.Drawing.Size(300, 27);
            this.ultraTextCompanyMSIC.TabIndex = 18;
            this.ultraTextCompanyMSIC.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraLabelCompanyMSIC
            // 
            this.ultraLabelCompanyMSIC.AutoSize = true;
            this.ultraLabelCompanyMSIC.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraLabelCompanyMSIC.ForeColor = System.Drawing.Color.Black;
            this.ultraLabelCompanyMSIC.Location = new System.Drawing.Point(452, 68);
            this.ultraLabelCompanyMSIC.Name = "ultraLabelCompanyMSIC";
            this.ultraLabelCompanyMSIC.Size = new System.Drawing.Size(118, 22);
            this.ultraLabelCompanyMSIC.TabIndex = 17;
            this.ultraLabelCompanyMSIC.Text = "Company MSIC:";
            // 
            // ultraTextCompanyEmail
            // 
            appearance17.BackColor = System.Drawing.Color.White;
            appearance17.BorderColor = System.Drawing.SystemColors.Highlight;
            this.ultraTextCompanyEmail.Appearance = appearance17;
            this.ultraTextCompanyEmail.BackColor = System.Drawing.Color.White;
            this.ultraTextCompanyEmail.BorderStyle = Infragistics.Win.UIElementBorderStyle.Solid;
            this.ultraTextCompanyEmail.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraTextCompanyEmail.Location = new System.Drawing.Point(962, 65);
            this.ultraTextCompanyEmail.Name = "ultraTextCompanyEmail";
            this.ultraTextCompanyEmail.Size = new System.Drawing.Size(300, 27);
            this.ultraTextCompanyEmail.TabIndex = 19;
            this.ultraTextCompanyEmail.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraLabelCompanyEmail
            // 
            this.ultraLabelCompanyEmail.AutoSize = true;
            this.ultraLabelCompanyEmail.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraLabelCompanyEmail.ForeColor = System.Drawing.Color.Black;
            this.ultraLabelCompanyEmail.Location = new System.Drawing.Point(897, 68);
            this.ultraLabelCompanyEmail.Name = "ultraLabelCompanyEmail";
            this.ultraLabelCompanyEmail.Size = new System.Drawing.Size(48, 22);
            this.ultraLabelCompanyEmail.TabIndex = 18;
            this.ultraLabelCompanyEmail.Text = "Email:";

            // 
            // FrmCustomer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1349, 520);
            this.Controls.Add(this.ultraPanel1);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.Name = "FrmCustomer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Customer Management - Modern UI";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FrmCustomer_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrmCustomer_KeyDown);
            this.ultraPanel1.ClientArea.ResumeLayout(false);
            this.ultraPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxBasicInfo)).EndInit();
            this.ultraGroupBoxBasicInfo.ResumeLayout(false);
            this.ultraGroupBoxBasicInfo.PerformLayout();

            ((System.ComponentModel.ISupportInitialize)(this.ultraTextCustomer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextAliasName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraComboPriceLevel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxContact)).EndInit();
            this.ultraGroupBoxContact.ResumeLayout(false);
            this.ultraGroupBoxContact.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextEmail)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextPhone)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxFinancial)).EndInit();
            this.ultraGroupBoxFinancial.ResumeLayout(false);
            this.ultraGroupBoxFinancial.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextOpenDebit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextOpenCredit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextSSMNumber)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextTINNumber)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraGroupBoxCompany)).EndInit();
            this.ultraGroupBoxCompany.ResumeLayout(false);
            this.ultraGroupBoxCompany.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextCompanyName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextCompanyTIN)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextCompanyMSIC)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ultraTextCompanyEmail)).EndInit();

            this.ResumeLayout(false);

        }

        #endregion
        private Infragistics.Win.Misc.UltraPanel ultraPanel1;
        private Infragistics.Win.Misc.UltraLabel ultraLabelTitle;
        private Infragistics.Win.Misc.UltraGroupBox ultraGroupBoxBasicInfo;
        private Infragistics.Win.Misc.UltraGroupBox ultraGroupBoxContact;
        private Infragistics.Win.Misc.UltraGroupBox ultraGroupBoxFinancial;
        private Infragistics.Win.Misc.UltraGroupBox ultraGroupBoxCompany;


        private Infragistics.Win.Misc.UltraLabel ultraLabelCustomer;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor ultraTextCustomer;
        private Infragistics.Win.Misc.UltraLabel ultraLabelAlias;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor ultraTextAliasName;
        private Infragistics.Win.Misc.UltraLabel ultraLabelEmail;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor ultraTextEmail;
        private Infragistics.Win.Misc.UltraLabel ultraLabelPhone;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor ultraTextPhone;
        private Infragistics.Win.Misc.UltraLabel ultraLabelOpenDebit;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor ultraTextOpenDebit;
        private Infragistics.Win.Misc.UltraLabel ultraLabelOpenCredit;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor ultraTextOpenCredit;
        private Infragistics.Win.Misc.UltraLabel ultraLabelPriceLevel;
        private Infragistics.Win.UltraWinEditors.UltraComboEditor ultraComboPriceLevel;
        private Infragistics.Win.Misc.UltraLabel ultraLabelSSMNumber;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor ultraTextSSMNumber;
        private Infragistics.Win.Misc.UltraLabel ultraLabelTINNumber;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor ultraTextTINNumber;
        private Infragistics.Win.Misc.UltraLabel ultraLabelCompanyName;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor ultraTextCompanyName;
        private Infragistics.Win.Misc.UltraLabel ultraLabelCompanyTIN;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor ultraTextCompanyTIN;
        private Infragistics.Win.Misc.UltraLabel ultraLabelCompanyMSIC;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor ultraTextCompanyMSIC;
        private Infragistics.Win.Misc.UltraLabel ultraLabelCompanyEmail;
        private Infragistics.Win.UltraWinEditors.UltraTextEditor ultraTextCompanyEmail;
        private Button button4;
    }
}