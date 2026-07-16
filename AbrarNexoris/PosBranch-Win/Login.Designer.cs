namespace PosBranch_Win
{
    partial class Login
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

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Login));
            Infragistics.Win.Appearance appearance1 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance2 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance3 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance4 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance5 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance6 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance7 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance8 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance9 = new Infragistics.Win.Appearance();
            Infragistics.Win.Appearance appearance10 = new Infragistics.Win.Appearance();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.lblCurrentDate = new System.Windows.Forms.Label();
            this.lblCurrentTime = new System.Windows.Forms.Label();
            this.lblCompanyName = new System.Windows.Forms.Label();
            this.ultraPictureBox1 = new Infragistics.Win.UltraWinEditors.UltraPictureBox();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.ultraButton8 = new Infragistics.Win.Misc.UltraButton();
            this.ultraButton9 = new Infragistics.Win.Misc.UltraButton();
            this.ultraButton10 = new Infragistics.Win.Misc.UltraButton();
            this.ultraButton1 = new Infragistics.Win.Misc.UltraButton();
            this.ultraButton2 = new Infragistics.Win.Misc.UltraButton();
            this.ultraButton3 = new Infragistics.Win.Misc.UltraButton();
            this.ultraButton5 = new Infragistics.Win.Misc.UltraButton();
            this.ultraButton6 = new Infragistics.Win.Misc.UltraButton();
            this.ultraButton7 = new Infragistics.Win.Misc.UltraButton();
            this.ultraButton4 = new Infragistics.Win.Misc.UltraButton();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.lblUserId = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.lblDate = new System.Windows.Forms.Label();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtDate = new System.Windows.Forms.TextBox();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.pnlContainer = new System.Windows.Forms.Panel();
            this.pnlLeft.SuspendLayout();
            this.pnlRight.SuspendLayout();
            this.pnlContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlLeft
            // 
            this.pnlLeft.BackColor = System.Drawing.Color.LavenderBlush;
            this.pnlLeft.Controls.Add(this.lblCurrentDate);
            this.pnlLeft.Controls.Add(this.lblCurrentTime);
            this.pnlLeft.Controls.Add(this.lblCompanyName);
            this.pnlLeft.Controls.Add(this.ultraPictureBox1);
            this.pnlLeft.Location = new System.Drawing.Point(0, 0);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Size = new System.Drawing.Size(277, 414);
            this.pnlLeft.TabIndex = 0;
            // 
            // lblCurrentDate
            // 
            this.lblCurrentDate.AutoSize = true;
            this.lblCurrentDate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblCurrentDate.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurrentDate.ForeColor = System.Drawing.Color.MidnightBlue;
            this.lblCurrentDate.Location = new System.Drawing.Point(69, 292);
            this.lblCurrentDate.Name = "lblCurrentDate";
            this.lblCurrentDate.Size = new System.Drawing.Size(148, 18);
            this.lblCurrentDate.TabIndex = 3;
            this.lblCurrentDate.Text = "Thursday 19-Mar-26";
            // 
            // lblCurrentTime
            // 
            this.lblCurrentTime.AutoSize = true;
            this.lblCurrentTime.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblCurrentTime.Font = new System.Drawing.Font("Arial", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurrentTime.ForeColor = System.Drawing.Color.Firebrick;
            this.lblCurrentTime.Location = new System.Drawing.Point(38, 319);
            this.lblCurrentTime.Name = "lblCurrentTime";
            this.lblCurrentTime.Size = new System.Drawing.Size(212, 55);
            this.lblCurrentTime.TabIndex = 4;
            this.lblCurrentTime.Text = "00:00:00";
            // 
            // lblCompanyName
            // 
            this.lblCompanyName.AutoSize = true;
            this.lblCompanyName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblCompanyName.Font = new System.Drawing.Font("Arial Narrow", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCompanyName.ForeColor = System.Drawing.Color.Black;
            this.lblCompanyName.Location = new System.Drawing.Point(42, 19);
            this.lblCompanyName.Name = "lblCompanyName";
            this.lblCompanyName.Size = new System.Drawing.Size(203, 31);
            this.lblCompanyName.TabIndex = 0;
            this.lblCompanyName.Text = "Nexoris Pos  Login ";
            // 
            // ultraPictureBox1
            // 
            this.ultraPictureBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ultraPictureBox1.BackColorInternal = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ultraPictureBox1.BorderShadowColor = System.Drawing.Color.Empty;
            this.ultraPictureBox1.Image = ((object)(resources.GetObject("ultraPictureBox1.Image")));
            this.ultraPictureBox1.Location = new System.Drawing.Point(-101, -211);
            this.ultraPictureBox1.Name = "ultraPictureBox1";
            this.ultraPictureBox1.Size = new System.Drawing.Size(482, 771);
            this.ultraPictureBox1.TabIndex = 5;
            // 
            // pnlRight
            // 
            this.pnlRight.BackColor = System.Drawing.Color.Gainsboro;
            this.pnlRight.Controls.Add(this.ultraButton8);
            this.pnlRight.Controls.Add(this.ultraButton9);
            this.pnlRight.Controls.Add(this.ultraButton10);
            this.pnlRight.Controls.Add(this.ultraButton1);
            this.pnlRight.Controls.Add(this.ultraButton2);
            this.pnlRight.Controls.Add(this.ultraButton3);
            this.pnlRight.Controls.Add(this.ultraButton5);
            this.pnlRight.Controls.Add(this.ultraButton6);
            this.pnlRight.Controls.Add(this.ultraButton7);
            this.pnlRight.Controls.Add(this.ultraButton4);
            this.pnlRight.Controls.Add(this.label1);
            this.pnlRight.Controls.Add(this.comboBox1);
            this.pnlRight.Controls.Add(this.lblUserId);
            this.pnlRight.Controls.Add(this.lblPassword);
            this.pnlRight.Controls.Add(this.lblDate);
            this.pnlRight.Controls.Add(this.txtUserName);
            this.pnlRight.Controls.Add(this.txtPassword);
            this.pnlRight.Controls.Add(this.txtDate);
            this.pnlRight.Controls.Add(this.btnClear);
            this.pnlRight.Controls.Add(this.btnCancel);
            this.pnlRight.Controls.Add(this.btnOK);
            this.pnlRight.Location = new System.Drawing.Point(277, 0);
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Size = new System.Drawing.Size(305, 414);
            this.pnlRight.TabIndex = 1;
            // 
            // pnlContainer
            // 
            this.pnlContainer.BackColor = System.Drawing.Color.Transparent;
            this.pnlContainer.Controls.Add(this.pnlRight);
            this.pnlContainer.Controls.Add(this.pnlLeft);
            this.pnlContainer.Location = new System.Drawing.Point(3, 3);
            this.pnlContainer.Name = "pnlContainer";
            this.pnlContainer.Size = new System.Drawing.Size(582, 414);
            this.pnlContainer.TabIndex = 2;
            // 
            // ultraButton8
            // 
            appearance1.BackColor = System.Drawing.Color.DeepSkyBlue;
            appearance1.BackColor2 = System.Drawing.Color.SkyBlue;
            appearance1.BackGradientStyle = Infragistics.Win.GradientStyle.GlassBottom37;
            this.ultraButton8.Appearance = appearance1;
            this.ultraButton8.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraButton8.Location = new System.Drawing.Point(128, 175);
            this.ultraButton8.Name = "ultraButton8";
            this.ultraButton8.Size = new System.Drawing.Size(61, 55);
            this.ultraButton8.TabIndex = 44;
            this.ultraButton8.Text = "9";
            this.ultraButton8.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraButton9
            // 
            appearance2.BackColor = System.Drawing.Color.DeepSkyBlue;
            appearance2.BackColor2 = System.Drawing.Color.SkyBlue;
            appearance2.BackGradientStyle = Infragistics.Win.GradientStyle.GlassBottom37;
            this.ultraButton9.Appearance = appearance2;
            this.ultraButton9.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraButton9.Location = new System.Drawing.Point(66, 175);
            this.ultraButton9.Name = "ultraButton9";
            this.ultraButton9.Size = new System.Drawing.Size(61, 55);
            this.ultraButton9.TabIndex = 43;
            this.ultraButton9.Text = "8";
            this.ultraButton9.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraButton10
            // 
            appearance3.BackColor = System.Drawing.Color.DeepSkyBlue;
            appearance3.BackColor2 = System.Drawing.Color.SkyBlue;
            appearance3.BackGradientStyle = Infragistics.Win.GradientStyle.GlassBottom37;
            this.ultraButton10.Appearance = appearance3;
            this.ultraButton10.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraButton10.Location = new System.Drawing.Point(4, 175);
            this.ultraButton10.Name = "ultraButton10";
            this.ultraButton10.Size = new System.Drawing.Size(61, 55);
            this.ultraButton10.TabIndex = 42;
            this.ultraButton10.Text = "7";
            this.ultraButton10.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraButton1
            // 
            appearance4.BackColor = System.Drawing.Color.DeepSkyBlue;
            appearance4.BackColor2 = System.Drawing.Color.SkyBlue;
            appearance4.BackGradientStyle = Infragistics.Win.GradientStyle.GlassBottom37;
            this.ultraButton1.Appearance = appearance4;
            this.ultraButton1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraButton1.Location = new System.Drawing.Point(128, 287);
            this.ultraButton1.Name = "ultraButton1";
            this.ultraButton1.Size = new System.Drawing.Size(61, 55);
            this.ultraButton1.TabIndex = 41;
            this.ultraButton1.Text = "3";
            this.ultraButton1.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraButton2
            // 
            appearance5.BackColor = System.Drawing.Color.DeepSkyBlue;
            appearance5.BackColor2 = System.Drawing.Color.SkyBlue;
            appearance5.BackGradientStyle = Infragistics.Win.GradientStyle.GlassBottom37;
            this.ultraButton2.Appearance = appearance5;
            this.ultraButton2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraButton2.Location = new System.Drawing.Point(66, 287);
            this.ultraButton2.Name = "ultraButton2";
            this.ultraButton2.Size = new System.Drawing.Size(61, 55);
            this.ultraButton2.TabIndex = 40;
            this.ultraButton2.Text = "2";
            this.ultraButton2.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraButton3
            // 
            appearance6.BackColor = System.Drawing.Color.DeepSkyBlue;
            appearance6.BackColor2 = System.Drawing.Color.SkyBlue;
            appearance6.BackGradientStyle = Infragistics.Win.GradientStyle.GlassBottom37;
            this.ultraButton3.Appearance = appearance6;
            this.ultraButton3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraButton3.Location = new System.Drawing.Point(4, 287);
            this.ultraButton3.Name = "ultraButton3";
            this.ultraButton3.Size = new System.Drawing.Size(61, 55);
            this.ultraButton3.TabIndex = 39;
            this.ultraButton3.Text = "1";
            this.ultraButton3.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraButton5
            // 
            appearance7.BackColor = System.Drawing.Color.DeepSkyBlue;
            appearance7.BackColor2 = System.Drawing.Color.SkyBlue;
            appearance7.BackGradientStyle = Infragistics.Win.GradientStyle.GlassBottom37;
            this.ultraButton5.Appearance = appearance7;
            this.ultraButton5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraButton5.Location = new System.Drawing.Point(128, 231);
            this.ultraButton5.Name = "ultraButton5";
            this.ultraButton5.Size = new System.Drawing.Size(61, 55);
            this.ultraButton5.TabIndex = 38;
            this.ultraButton5.Text = "6";
            this.ultraButton5.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraButton6
            // 
            appearance8.BackColor = System.Drawing.Color.DeepSkyBlue;
            appearance8.BackColor2 = System.Drawing.Color.SkyBlue;
            appearance8.BackGradientStyle = Infragistics.Win.GradientStyle.GlassBottom37;
            this.ultraButton6.Appearance = appearance8;
            this.ultraButton6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraButton6.Location = new System.Drawing.Point(66, 231);
            this.ultraButton6.Name = "ultraButton6";
            this.ultraButton6.Size = new System.Drawing.Size(61, 55);
            this.ultraButton6.TabIndex = 37;
            this.ultraButton6.Text = "5";
            this.ultraButton6.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraButton7
            // 
            appearance9.BackColor = System.Drawing.Color.DeepSkyBlue;
            appearance9.BackColor2 = System.Drawing.Color.SkyBlue;
            appearance9.BackGradientStyle = Infragistics.Win.GradientStyle.GlassBottom37;
            this.ultraButton7.Appearance = appearance9;
            this.ultraButton7.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraButton7.Location = new System.Drawing.Point(4, 231);
            this.ultraButton7.Name = "ultraButton7";
            this.ultraButton7.Size = new System.Drawing.Size(61, 55);
            this.ultraButton7.TabIndex = 36;
            this.ultraButton7.Text = "4";
            this.ultraButton7.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // ultraButton4
            // 
            appearance10.BackColor = System.Drawing.Color.DeepSkyBlue;
            appearance10.BackColor2 = System.Drawing.Color.SkyBlue;
            appearance10.BackGradientStyle = Infragistics.Win.GradientStyle.GlassBottom37;
            this.ultraButton4.Appearance = appearance10;
            this.ultraButton4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ultraButton4.Location = new System.Drawing.Point(4, 344);
            this.ultraButton4.Name = "ultraButton4";
            this.ultraButton4.Size = new System.Drawing.Size(185, 69);
            this.ultraButton4.TabIndex = 35;
            this.ultraButton4.Text = "0";
            this.ultraButton4.UseOsThemes = Infragistics.Win.DefaultableBoolean.False;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.DarkBlue;
            this.label1.Location = new System.Drawing.Point(32, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 17);
            this.label1.TabIndex = 30;
            this.label1.Text = "Branch";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(95, 13);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(151, 21);
            this.comboBox1.TabIndex = 29;
            // 
            // lblUserId
            // 
            this.lblUserId.AutoSize = true;
            this.lblUserId.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUserId.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblUserId.Location = new System.Drawing.Point(36, 49);
            this.lblUserId.Name = "lblUserId";
            this.lblUserId.Size = new System.Drawing.Size(57, 17);
            this.lblUserId.TabIndex = 10;
            this.lblUserId.Text = "User ID";
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPassword.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblPassword.Location = new System.Drawing.Point(21, 84);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(74, 17);
            this.lblPassword.TabIndex = 12;
            this.lblPassword.Text = "Password";
            // 
            // lblDate
            // 
            this.lblDate.AutoSize = true;
            this.lblDate.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDate.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblDate.Location = new System.Drawing.Point(52, 119);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(39, 17);
            this.lblDate.TabIndex = 14;
            this.lblDate.Text = "Date";
            // 
            // txtUserName
            // 
            this.txtUserName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtUserName.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUserName.Location = new System.Drawing.Point(95, 46);
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(151, 26);
            this.txtUserName.TabIndex = 11;
            this.txtUserName.MouseClick += new System.Windows.Forms.MouseEventHandler(this.txtUserName_MouseClick);
            this.txtUserName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtUserName_KeyDown);
            // 
            // txtPassword
            // 
            this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPassword.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPassword.Location = new System.Drawing.Point(95, 81);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(151, 26);
            this.txtPassword.TabIndex = 13;
            this.txtPassword.MouseClick += new System.Windows.Forms.MouseEventHandler(this.txtPassword_MouseClick);
            this.txtPassword.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtPassword_KeyDown);
            // 
            // txtDate
            // 
            this.txtDate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDate.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDate.Location = new System.Drawing.Point(95, 116);
            this.txtDate.Name = "txtDate";
            this.txtDate.ReadOnly = true;
            this.txtDate.Size = new System.Drawing.Size(151, 26);
            this.txtDate.TabIndex = 15;
            // 
            // btnClear
            // 
            this.btnClear.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnClear.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClear.ForeColor = System.Drawing.Color.Black;
            this.btnClear.Location = new System.Drawing.Point(190, 175);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(110, 55);
            this.btnClear.TabIndex = 19;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.ForeColor = System.Drawing.Color.Black;
            this.btnCancel.Location = new System.Drawing.Point(190, 231);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(110, 55);
            this.btnCancel.TabIndex = 23;
            this.btnCancel.Text = " Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.BackColor = System.Drawing.Color.Cyan;
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOK.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.ForeColor = System.Drawing.Color.Black;
            this.btnOK.Location = new System.Drawing.Point(190, 287);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(110, 126);
            this.btnOK.TabIndex = 27;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = false;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Login
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(588, 420);
            this.Controls.Add(this.pnlContainer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Login";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login";
            this.Load += new System.EventHandler(this.Login_Load);
            this.pnlContainer.ResumeLayout(false);
            this.pnlLeft.ResumeLayout(false);
            this.pnlLeft.PerformLayout();
            this.pnlRight.ResumeLayout(false);
            this.pnlRight.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.Panel pnlContainer;
        private System.Windows.Forms.Label lblCompanyName;
        private System.Windows.Forms.Label lblCurrentDate;
        private System.Windows.Forms.Label lblCurrentTime;

        private System.Windows.Forms.Label lblUserId;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Label lblDate;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtDate;

        private System.Windows.Forms.Timer timer1;
        private Infragistics.Win.UltraWinEditors.UltraPictureBox ultraPictureBox1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label1;
        private Infragistics.Win.Misc.UltraButton ultraButton8;
        private Infragistics.Win.Misc.UltraButton ultraButton9;
        private Infragistics.Win.Misc.UltraButton ultraButton10;
        private Infragistics.Win.Misc.UltraButton ultraButton1;
        private Infragistics.Win.Misc.UltraButton ultraButton2;
        private Infragistics.Win.Misc.UltraButton ultraButton3;
        private Infragistics.Win.Misc.UltraButton ultraButton5;
        private Infragistics.Win.Misc.UltraButton ultraButton6;
        private Infragistics.Win.Misc.UltraButton ultraButton7;
        private Infragistics.Win.Misc.UltraButton ultraButton4;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
    }
}
