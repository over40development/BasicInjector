namespace Launcher
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.userPanel = new System.Windows.Forms.Panel();
            this.lnkEpsilon = new System.Windows.Forms.LinkLabel();
            this.btnInjectMain = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbClientTypeMain = new System.Windows.Forms.ComboBox();
            this.adminPanel = new System.Windows.Forms.Panel();
            this.chkCloseAfterInject = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbClientType = new System.Windows.Forms.ComboBox();
            this.btnInject = new System.Windows.Forms.Button();
            this.lblProcess = new System.Windows.Forms.Label();
            this.btnBrowseDll = new System.Windows.Forms.Button();
            this.txtProcess = new System.Windows.Forms.TextBox();
            this.btnSelect = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtDllPath = new System.Windows.Forms.TextBox();
            this.userPanel.SuspendLayout();
            this.adminPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "All files|*.*";
            this.openFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog1_FileOk);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // userPanel
            // 
            this.userPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.userPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(236)))), ((int)(((byte)(239)))));
            this.userPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.userPanel.Controls.Add(this.lnkEpsilon);
            this.userPanel.Controls.Add(this.btnInjectMain);
            this.userPanel.Controls.Add(this.label3);
            this.userPanel.Controls.Add(this.cmbClientTypeMain);
            this.userPanel.Location = new System.Drawing.Point(0, 0);
            this.userPanel.Margin = new System.Windows.Forms.Padding(0);
            this.userPanel.Name = "userPanel";
            this.userPanel.Size = new System.Drawing.Size(382, 150);
            this.userPanel.TabIndex = 9;
            // 
            // lnkEpsilon
            // 
            this.lnkEpsilon.ActiveLinkColor = System.Drawing.Color.Blue;
            this.lnkEpsilon.AutoSize = true;
            this.lnkEpsilon.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lnkEpsilon.Location = new System.Drawing.Point(7, 34);
            this.lnkEpsilon.Name = "lnkEpsilon";
            this.lnkEpsilon.Size = new System.Drawing.Size(46, 13);
            this.lnkEpsilon.TabIndex = 27;
            this.lnkEpsilon.TabStop = true;
            this.lnkEpsilon.Text = "Website";
            this.lnkEpsilon.VisitedLinkColor = System.Drawing.Color.Blue;
            // 
            // btnInjectMain
            // 
            this.btnInjectMain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInjectMain.Location = new System.Drawing.Point(7, 102);
            this.btnInjectMain.Name = "btnInjectMain";
            this.btnInjectMain.Size = new System.Drawing.Size(368, 41);
            this.btnInjectMain.TabIndex = 26;
            this.btnInjectMain.Text = "&Inject";
            this.btnInjectMain.UseVisualStyleBackColor = true;
            this.btnInjectMain.Click += new System.EventHandler(this.btnInject_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 13);
            this.label3.TabIndex = 25;
            this.label3.Text = "Client Type:";
            // 
            // cmbClientTypeMain
            // 
            this.cmbClientTypeMain.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbClientTypeMain.FormattingEnabled = true;
            this.cmbClientTypeMain.Location = new System.Drawing.Point(111, 4);
            this.cmbClientTypeMain.MaxDropDownItems = 2;
            this.cmbClientTypeMain.Name = "cmbClientTypeMain";
            this.cmbClientTypeMain.Size = new System.Drawing.Size(139, 21);
            this.cmbClientTypeMain.TabIndex = 24;
            this.cmbClientTypeMain.Text = "Select Type";
            this.cmbClientTypeMain.SelectedIndexChanged += new System.EventHandler(this.cmbClientType_SelectedValueChanged);
            // 
            // adminPanel
            // 
            this.adminPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.adminPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.adminPanel.Controls.Add(this.chkCloseAfterInject);
            this.adminPanel.Controls.Add(this.label2);
            this.adminPanel.Controls.Add(this.cmbClientType);
            this.adminPanel.Controls.Add(this.btnInject);
            this.adminPanel.Controls.Add(this.lblProcess);
            this.adminPanel.Controls.Add(this.btnBrowseDll);
            this.adminPanel.Controls.Add(this.txtProcess);
            this.adminPanel.Controls.Add(this.btnSelect);
            this.adminPanel.Controls.Add(this.label1);
            this.adminPanel.Controls.Add(this.txtDllPath);
            this.adminPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.adminPanel.Location = new System.Drawing.Point(0, 0);
            this.adminPanel.Margin = new System.Windows.Forms.Padding(0);
            this.adminPanel.Name = "adminPanel";
            this.adminPanel.Size = new System.Drawing.Size(368, 150);
            this.adminPanel.TabIndex = 10;
            // 
            // chkCloseAfterInject
            // 
            this.chkCloseAfterInject.AutoSize = true;
            this.chkCloseAfterInject.Location = new System.Drawing.Point(256, 7);
            this.chkCloseAfterInject.Name = "chkCloseAfterInject";
            this.chkCloseAfterInject.Size = new System.Drawing.Size(105, 17);
            this.chkCloseAfterInject.TabIndex = 30;
            this.chkCloseAfterInject.Text = "Close after Inject";
            this.chkCloseAfterInject.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 29;
            this.label2.Text = "Client Type:";
            // 
            // cmbClientType
            // 
            this.cmbClientType.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbClientType.FormattingEnabled = true;
            this.cmbClientType.Location = new System.Drawing.Point(111, 5);
            this.cmbClientType.MaxDropDownItems = 2;
            this.cmbClientType.Name = "cmbClientType";
            this.cmbClientType.Size = new System.Drawing.Size(139, 21);
            this.cmbClientType.TabIndex = 28;
            this.cmbClientType.Text = "Select Type";
            this.cmbClientType.SelectedIndexChanged += new System.EventHandler(this.cmbClientType_SelectedValueChanged);
            // 
            // btnInject
            // 
            this.btnInject.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInject.Location = new System.Drawing.Point(8, 112);
            this.btnInject.Name = "btnInject";
            this.btnInject.Size = new System.Drawing.Size(352, 32);
            this.btnInject.TabIndex = 27;
            this.btnInject.Text = "&Inject";
            this.btnInject.UseVisualStyleBackColor = true;
            this.btnInject.Click += new System.EventHandler(this.btnInject_Click);
            // 
            // lblProcess
            // 
            this.lblProcess.AutoSize = true;
            this.lblProcess.Location = new System.Drawing.Point(8, 29);
            this.lblProcess.Name = "lblProcess";
            this.lblProcess.Size = new System.Drawing.Size(115, 13);
            this.lblProcess.TabIndex = 21;
            this.lblProcess.Text = "Select Target Process:";
            // 
            // btnBrowseDll
            // 
            this.btnBrowseDll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseDll.Location = new System.Drawing.Point(289, 86);
            this.btnBrowseDll.Name = "btnBrowseDll";
            this.btnBrowseDll.Size = new System.Drawing.Size(71, 20);
            this.btnBrowseDll.TabIndex = 26;
            this.btnBrowseDll.Text = "Browse";
            this.btnBrowseDll.UseVisualStyleBackColor = true;
            this.btnBrowseDll.Click += new System.EventHandler(this.btnBrowseDll_Click);
            // 
            // txtProcess
            // 
            this.txtProcess.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtProcess.Location = new System.Drawing.Point(8, 45);
            this.txtProcess.Name = "txtProcess";
            this.txtProcess.ReadOnly = true;
            this.txtProcess.Size = new System.Drawing.Size(277, 20);
            this.txtProcess.TabIndex = 22;
            // 
            // btnSelect
            // 
            this.btnSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelect.Location = new System.Drawing.Point(288, 45);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(72, 20);
            this.btnSelect.TabIndex = 25;
            this.btnSelect.Text = "Select";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 69);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 13);
            this.label1.TabIndex = 23;
            this.label1.Text = "Select DLL to Inject:";
            // 
            // txtDllPath
            // 
            this.txtDllPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDllPath.Location = new System.Drawing.Point(8, 86);
            this.txtDllPath.Name = "txtDllPath";
            this.txtDllPath.ReadOnly = true;
            this.txtDllPath.Size = new System.Drawing.Size(277, 20);
            this.txtDllPath.TabIndex = 24;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(368, 150);
            this.Controls.Add(this.userPanel);
            this.Controls.Add(this.adminPanel);
            this.MaximizeBox = false;
            this.Name = "frmMain";
            this.Text = "Injector";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.userPanel.ResumeLayout(false);
            this.userPanel.PerformLayout();
            this.adminPanel.ResumeLayout(false);
            this.adminPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Panel userPanel;
        private System.Windows.Forms.LinkLabel lnkEpsilon;
        private System.Windows.Forms.Button btnInjectMain;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbClientTypeMain;
        private System.Windows.Forms.Panel adminPanel;
        private System.Windows.Forms.CheckBox chkCloseAfterInject;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbClientType;
        private System.Windows.Forms.Button btnInject;
        private System.Windows.Forms.Label lblProcess;
        private System.Windows.Forms.Button btnBrowseDll;
        private System.Windows.Forms.TextBox txtProcess;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtDllPath;
    }
}

