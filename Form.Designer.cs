namespace UnityModManagerNet.Installer
{
    partial class UnityModManagerForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnInstall = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.inputLog = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnOpenFolder = new System.Windows.Forms.Button();
            this.gameList = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.installedVersion = new System.Windows.Forms.Label();
            this.currentVersion = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnInstall
            // 
            this.btnInstall.AutoSize = true;
            this.btnInstall.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnInstall.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnInstall.Location = new System.Drawing.Point(5, 5);
            this.btnInstall.Margin = new System.Windows.Forms.Padding(5);
            this.btnInstall.Name = "btnInstall";
            this.btnInstall.Size = new System.Drawing.Size(316, 45);
            this.btnInstall.TabIndex = 0;
            this.btnInstall.Text = "Install";
            this.btnInstall.UseMnemonic = false;
            this.btnInstall.UseVisualStyleBackColor = true;
            this.btnInstall.Click += new System.EventHandler(this.btnInstall_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.AutoSize = true;
            this.btnRemove.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnRemove.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnRemove.Location = new System.Drawing.Point(5, 50);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(316, 45);
            this.btnRemove.TabIndex = 1;
            this.btnRemove.Text = "Uninstall";
            this.btnRemove.UseMnemonic = false;
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // inputLog
            // 
            this.inputLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.inputLog.Location = new System.Drawing.Point(5, 179);
            this.inputLog.Multiline = true;
            this.inputLog.Name = "inputLog";
            this.inputLog.ReadOnly = true;
            this.inputLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.inputLog.Size = new System.Drawing.Size(316, 156);
            this.inputLog.TabIndex = 2;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnOpenFolder);
            this.panel1.Controls.Add(this.gameList);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.installedVersion);
            this.panel1.Controls.Add(this.currentVersion);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.inputLog);
            this.panel1.Controls.Add(this.btnRemove);
            this.panel1.Controls.Add(this.btnInstall);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(5);
            this.panel1.Size = new System.Drawing.Size(326, 340);
            this.panel1.TabIndex = 3;
            // 
            // btnOpenFolder
            // 
            this.btnOpenFolder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnOpenFolder.Location = new System.Drawing.Point(139, 130);
            this.btnOpenFolder.Name = "btnOpenFolder";
            this.btnOpenFolder.Size = new System.Drawing.Size(182, 23);
            this.btnOpenFolder.TabIndex = 9;
            this.btnOpenFolder.Text = "Select Game Folder";
            this.btnOpenFolder.UseVisualStyleBackColor = true;
            this.btnOpenFolder.Click += new System.EventHandler(this.btnOpenFolder_Click);
            // 
            // gameList
            // 
            this.gameList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.gameList.FormattingEnabled = true;
            this.gameList.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.gameList.Location = new System.Drawing.Point(140, 103);
            this.gameList.Name = "gameList";
            this.gameList.Size = new System.Drawing.Size(180, 21);
            this.gameList.Sorted = true;
            this.gameList.TabIndex = 8;
            this.gameList.SelectedIndexChanged += new System.EventHandler(this.gameList_Changed);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 122);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(87, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Installed Version:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 102);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Current Version:";
            // 
            // installedVersion
            // 
            this.installedVersion.AutoSize = true;
            this.installedVersion.Location = new System.Drawing.Point(88, 122);
            this.installedVersion.Name = "installedVersion";
            this.installedVersion.Size = new System.Drawing.Size(10, 13);
            this.installedVersion.TabIndex = 5;
            this.installedVersion.Text = "-";
            // 
            // currentVersion
            // 
            this.currentVersion.AutoSize = true;
            this.currentVersion.Location = new System.Drawing.Point(83, 102);
            this.currentVersion.Name = "currentVersion";
            this.currentVersion.Size = new System.Drawing.Size(31, 13);
            this.currentVersion.TabIndex = 4;
            this.currentVersion.Text = "1.0.0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(146, 156);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "Log";
            // 
            // folderBrowserDialog
            // 
            this.folderBrowserDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
            this.folderBrowserDialog.HelpRequest += new System.EventHandler(this.folderBrowserDialog_HelpRequest);
            // 
            // UnityModLoaderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(326, 340);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UnityModLoaderForm";
            this.ShowIcon = false;
            this.Text = "UnityModManager Installer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UnityModLoaderForm_FormClosing);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnInstall;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label currentVersion;
        public System.Windows.Forms.TextBox inputLog;
        private System.Windows.Forms.Label installedVersion;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox gameList;
        private System.Windows.Forms.Button btnOpenFolder;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
    }
}

