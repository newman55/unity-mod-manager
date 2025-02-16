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
            this.components = new System.ComponentModel.Container();
            this.panelMain = new System.Windows.Forms.Panel();
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.additionallyGroupBox = new System.Windows.Forms.GroupBox();
            this.notesTextBox = new System.Windows.Forms.RichTextBox();
            this.extraFilesGroupBox = new System.Windows.Forms.GroupBox();
            this.extraFilesManualButton = new System.Windows.Forms.Button();
            this.extraFilesAutoButton = new System.Windows.Forms.Button();
            this.extraFilesTextBox = new System.Windows.Forms.RichTextBox();
            this.labelFolder = new System.Windows.Forms.Label();
            this.labelGame = new System.Windows.Forms.Label();
            this.installTypeGroup = new System.Windows.Forms.GroupBox();
            this.btnRestore = new System.Windows.Forms.Button();
            this.btnDownloadUpdate = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnOpenFolder = new System.Windows.Forms.Button();
            this.gameList = new System.Windows.Forms.ComboBox();
            this.btnInstall = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.currentVersion = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.installedVersion = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.splitContainerMods = new System.Windows.Forms.SplitContainer();
            this.listMods = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ModcontextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.installToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.revertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uninstallToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wwwToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.openFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainerModsInstall = new System.Windows.Forms.SplitContainer();
            this.btnCheckUpdates = new System.Windows.Forms.Button();
            this.btnModInstall = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.inputLog = new System.Windows.Forms.TextBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.resetFirewallGroup = new System.Windows.Forms.GroupBox();
            this.btnRemFirewallGame = new System.Windows.Forms.Button();
            this.btnRemFirewallInstaller = new System.Windows.Forms.Button();
            this.updateCheckingModeGroup = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnGetApiKey = new System.Windows.Forms.Button();
            this.textBoxApiKey = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.modInstallFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.btnSetFolder = new System.Windows.Forms.Button();
            this.panelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.additionallyGroupBox.SuspendLayout();
            this.extraFilesGroupBox.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMods)).BeginInit();
            this.splitContainerMods.Panel1.SuspendLayout();
            this.splitContainerMods.Panel2.SuspendLayout();
            this.splitContainerMods.SuspendLayout();
            this.ModcontextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerModsInstall)).BeginInit();
            this.splitContainerModsInstall.Panel1.SuspendLayout();
            this.splitContainerModsInstall.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.resetFirewallGroup.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.splitContainerMain);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(358, 469);
            this.panelMain.TabIndex = 3;
            // 
            // splitContainerMain
            // 
            this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainerMain.IsSplitterFixed = true;
            this.splitContainerMain.Location = new System.Drawing.Point(0, 0);
            this.splitContainerMain.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerMain.Name = "splitContainerMain";
            this.splitContainerMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerMain.Panel1
            // 
            this.splitContainerMain.Panel1.Controls.Add(this.tabControl);
            this.splitContainerMain.Panel1MinSize = 20;
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.statusStrip1);
            this.splitContainerMain.Panel2MinSize = 20;
            this.splitContainerMain.Size = new System.Drawing.Size(358, 469);
            this.splitContainerMain.SplitterDistance = 440;
            this.splitContainerMain.TabIndex = 11;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPage1);
            this.tabControl.Controls.Add(this.tabPage2);
            this.tabControl.Controls.Add(this.tabPage3);
            this.tabControl.Controls.Add(this.tabPage4);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tabControl.ItemSize = new System.Drawing.Size(80, 24);
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl.Name = "tabControl";
            this.tabControl.Padding = new System.Drawing.Point(0, 4);
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(358, 440);
            this.tabControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControl.TabIndex = 10;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabs_Changed);
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.tabPage1.Controls.Add(this.btnSetFolder);
            this.tabPage1.Controls.Add(this.panel1);
            this.tabPage1.Controls.Add(this.labelFolder);
            this.tabPage1.Controls.Add(this.labelGame);
            this.tabPage1.Controls.Add(this.installTypeGroup);
            this.tabPage1.Controls.Add(this.btnRestore);
            this.tabPage1.Controls.Add(this.btnDownloadUpdate);
            this.tabPage1.Controls.Add(this.btnRemove);
            this.tabPage1.Controls.Add(this.btnOpenFolder);
            this.tabPage1.Controls.Add(this.gameList);
            this.tabPage1.Controls.Add(this.btnInstall);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.currentVersion);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.installedVersion);
            this.tabPage1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tabPage1.Location = new System.Drawing.Point(4, 28);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(350, 408);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Install";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.additionallyGroupBox);
            this.panel1.Controls.Add(this.extraFilesGroupBox);
            this.panel1.Location = new System.Drawing.Point(4, 259);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(343, 151);
            this.panel1.TabIndex = 23;
            // 
            // additionallyGroupBox
            // 
            this.additionallyGroupBox.Controls.Add(this.notesTextBox);
            this.additionallyGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.additionallyGroupBox.Location = new System.Drawing.Point(0, 74);
            this.additionallyGroupBox.Name = "additionallyGroupBox";
            this.additionallyGroupBox.Padding = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.additionallyGroupBox.Size = new System.Drawing.Size(343, 74);
            this.additionallyGroupBox.TabIndex = 20;
            this.additionallyGroupBox.TabStop = false;
            this.additionallyGroupBox.Text = "Comment";
            this.additionallyGroupBox.Visible = false;
            // 
            // notesTextBox
            // 
            this.notesTextBox.AcceptsTab = true;
            this.notesTextBox.AutoWordSelection = true;
            this.notesTextBox.BackColor = System.Drawing.Color.WhiteSmoke;
            this.notesTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.notesTextBox.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.notesTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.notesTextBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.notesTextBox.Location = new System.Drawing.Point(6, 16);
            this.notesTextBox.Margin = new System.Windows.Forms.Padding(0);
            this.notesTextBox.Name = "notesTextBox";
            this.notesTextBox.ReadOnly = true;
            this.notesTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.notesTextBox.ShortcutsEnabled = false;
            this.notesTextBox.Size = new System.Drawing.Size(331, 55);
            this.notesTextBox.TabIndex = 19;
            this.notesTextBox.TabStop = false;
            this.notesTextBox.Text = "";
            this.notesTextBox.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.notesTextBox_LinkClicked);
            // 
            // extraFilesGroupBox
            // 
            this.extraFilesGroupBox.Controls.Add(this.extraFilesManualButton);
            this.extraFilesGroupBox.Controls.Add(this.extraFilesAutoButton);
            this.extraFilesGroupBox.Controls.Add(this.extraFilesTextBox);
            this.extraFilesGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.extraFilesGroupBox.Location = new System.Drawing.Point(0, 0);
            this.extraFilesGroupBox.Name = "extraFilesGroupBox";
            this.extraFilesGroupBox.Padding = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.extraFilesGroupBox.Size = new System.Drawing.Size(343, 74);
            this.extraFilesGroupBox.TabIndex = 21;
            this.extraFilesGroupBox.TabStop = false;
            this.extraFilesGroupBox.Text = "Extra Files";
            // 
            // extraFilesManualButton
            // 
            this.extraFilesManualButton.BackColor = System.Drawing.Color.Transparent;
            this.extraFilesManualButton.Location = new System.Drawing.Point(215, 45);
            this.extraFilesManualButton.Name = "extraFilesManualButton";
            this.extraFilesManualButton.Size = new System.Drawing.Size(60, 23);
            this.extraFilesManualButton.TabIndex = 21;
            this.extraFilesManualButton.Text = "Manual";
            this.extraFilesManualButton.UseVisualStyleBackColor = false;
            this.extraFilesManualButton.Click += new System.EventHandler(this.extraFilesManualButton_Click);
            // 
            // extraFilesAutoButton
            // 
            this.extraFilesAutoButton.BackColor = System.Drawing.Color.PaleGreen;
            this.extraFilesAutoButton.Location = new System.Drawing.Point(277, 45);
            this.extraFilesAutoButton.Name = "extraFilesAutoButton";
            this.extraFilesAutoButton.Size = new System.Drawing.Size(60, 23);
            this.extraFilesAutoButton.TabIndex = 20;
            this.extraFilesAutoButton.Text = "Auto";
            this.extraFilesAutoButton.UseVisualStyleBackColor = false;
            this.extraFilesAutoButton.Click += new System.EventHandler(this.extraFilesAutoButton_Click);
            // 
            // extraFilesTextBox
            // 
            this.extraFilesTextBox.AcceptsTab = true;
            this.extraFilesTextBox.AutoWordSelection = true;
            this.extraFilesTextBox.BackColor = System.Drawing.Color.WhiteSmoke;
            this.extraFilesTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.extraFilesTextBox.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.extraFilesTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.extraFilesTextBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.extraFilesTextBox.Location = new System.Drawing.Point(6, 16);
            this.extraFilesTextBox.Margin = new System.Windows.Forms.Padding(0);
            this.extraFilesTextBox.Name = "extraFilesTextBox";
            this.extraFilesTextBox.ReadOnly = true;
            this.extraFilesTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.extraFilesTextBox.ShortcutsEnabled = false;
            this.extraFilesTextBox.Size = new System.Drawing.Size(331, 55);
            this.extraFilesTextBox.TabIndex = 19;
            this.extraFilesTextBox.TabStop = false;
            this.extraFilesTextBox.Text = "";
            // 
            // labelFolder
            // 
            this.labelFolder.AutoSize = true;
            this.labelFolder.Location = new System.Drawing.Point(3, 185);
            this.labelFolder.Name = "labelFolder";
            this.labelFolder.Size = new System.Drawing.Size(36, 13);
            this.labelFolder.TabIndex = 22;
            this.labelFolder.Text = "Folder";
            // 
            // labelGame
            // 
            this.labelGame.AutoSize = true;
            this.labelGame.Location = new System.Drawing.Point(3, 151);
            this.labelGame.Name = "labelGame";
            this.labelGame.Size = new System.Drawing.Size(35, 13);
            this.labelGame.TabIndex = 21;
            this.labelGame.Text = "Game";
            // 
            // installTypeGroup
            // 
            this.installTypeGroup.Location = new System.Drawing.Point(4, 213);
            this.installTypeGroup.Name = "installTypeGroup";
            this.installTypeGroup.Padding = new System.Windows.Forms.Padding(10, 3, 10, 5);
            this.installTypeGroup.Size = new System.Drawing.Size(343, 46);
            this.installTypeGroup.TabIndex = 18;
            this.installTypeGroup.TabStop = false;
            this.installTypeGroup.Text = "Installation method";
            // 
            // btnRestore
            // 
            this.btnRestore.AutoSize = true;
            this.btnRestore.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnRestore.Enabled = false;
            this.btnRestore.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnRestore.Location = new System.Drawing.Point(3, 93);
            this.btnRestore.Name = "btnRestore";
            this.btnRestore.Size = new System.Drawing.Size(344, 45);
            this.btnRestore.TabIndex = 13;
            this.btnRestore.Text = "Restore original files";
            this.btnRestore.UseMnemonic = false;
            this.btnRestore.UseVisualStyleBackColor = true;
            // 
            // btnDownloadUpdate
            // 
            this.btnDownloadUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDownloadUpdate.AutoSize = true;
            this.btnDownloadUpdate.BackColor = System.Drawing.Color.PaleGreen;
            this.btnDownloadUpdate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnDownloadUpdate.Location = new System.Drawing.Point(222, 178);
            this.btnDownloadUpdate.Name = "btnDownloadUpdate";
            this.btnDownloadUpdate.Size = new System.Drawing.Size(122, 26);
            this.btnDownloadUpdate.TabIndex = 12;
            this.btnDownloadUpdate.Text = "Home Page";
            this.btnDownloadUpdate.UseMnemonic = false;
            this.btnDownloadUpdate.UseVisualStyleBackColor = false;
            // 
            // btnRemove
            // 
            this.btnRemove.AutoSize = true;
            this.btnRemove.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnRemove.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnRemove.Location = new System.Drawing.Point(3, 48);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(344, 45);
            this.btnRemove.TabIndex = 2;
            this.btnRemove.Text = "Uninstall";
            this.btnRemove.UseMnemonic = false;
            this.btnRemove.UseVisualStyleBackColor = true;
            // 
            // btnOpenFolder
            // 
            this.btnOpenFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenFolder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnOpenFolder.Location = new System.Drawing.Point(40, 178);
            this.btnOpenFolder.Name = "btnOpenFolder";
            this.btnOpenFolder.Size = new System.Drawing.Size(142, 26);
            this.btnOpenFolder.TabIndex = 9;
            this.btnOpenFolder.Text = "Select";
            this.btnOpenFolder.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOpenFolder.UseVisualStyleBackColor = true;
            // 
            // gameList
            // 
            this.gameList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gameList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.gameList.FormattingEnabled = true;
            this.gameList.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.gameList.Location = new System.Drawing.Point(41, 147);
            this.gameList.Name = "gameList";
            this.gameList.Size = new System.Drawing.Size(169, 21);
            this.gameList.Sorted = true;
            this.gameList.TabIndex = 8;
            // 
            // btnInstall
            // 
            this.btnInstall.AutoSize = true;
            this.btnInstall.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnInstall.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnInstall.Location = new System.Drawing.Point(3, 3);
            this.btnInstall.Margin = new System.Windows.Forms.Padding(5);
            this.btnInstall.Name = "btnInstall";
            this.btnInstall.Size = new System.Drawing.Size(344, 45);
            this.btnInstall.TabIndex = 1;
            this.btnInstall.Text = "Install";
            this.btnInstall.UseMnemonic = false;
            this.btnInstall.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(219, 160);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Ingame Version:";
            // 
            // currentVersion
            // 
            this.currentVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.currentVersion.AutoSize = true;
            this.currentVersion.Location = new System.Drawing.Point(299, 144);
            this.currentVersion.Name = "currentVersion";
            this.currentVersion.Size = new System.Drawing.Size(31, 13);
            this.currentVersion.TabIndex = 4;
            this.currentVersion.Text = "1.0.0";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(219, 144);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Current Version:";
            // 
            // installedVersion
            // 
            this.installedVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.installedVersion.AutoSize = true;
            this.installedVersion.Location = new System.Drawing.Point(299, 160);
            this.installedVersion.Name = "installedVersion";
            this.installedVersion.Size = new System.Drawing.Size(10, 13);
            this.installedVersion.TabIndex = 5;
            this.installedVersion.Text = "-";
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.Color.WhiteSmoke;
            this.tabPage2.Controls.Add(this.splitContainerMods);
            this.tabPage2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.tabPage2.Location = new System.Drawing.Point(4, 28);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(350, 408);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Mods";
            // 
            // splitContainerMods
            // 
            this.splitContainerMods.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMods.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainerMods.IsSplitterFixed = true;
            this.splitContainerMods.Location = new System.Drawing.Point(3, 3);
            this.splitContainerMods.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerMods.Name = "splitContainerMods";
            this.splitContainerMods.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerMods.Panel1
            // 
            this.splitContainerMods.Panel1.Controls.Add(this.listMods);
            // 
            // splitContainerMods.Panel2
            // 
            this.splitContainerMods.Panel2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.splitContainerMods.Panel2.Controls.Add(this.splitContainerModsInstall);
            this.splitContainerMods.Size = new System.Drawing.Size(344, 402);
            this.splitContainerMods.SplitterDistance = 256;
            this.splitContainerMods.TabIndex = 0;
            // 
            // listMods
            // 
            this.listMods.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.listMods.ContextMenuStrip = this.ModcontextMenuStrip1;
            this.listMods.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listMods.FullRowSelect = true;
            this.listMods.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listMods.HideSelection = false;
            this.listMods.Location = new System.Drawing.Point(0, 0);
            this.listMods.MultiSelect = false;
            this.listMods.Name = "listMods";
            this.listMods.Size = new System.Drawing.Size(344, 256);
            this.listMods.TabIndex = 0;
            this.listMods.UseCompatibleStateImageBehavior = false;
            this.listMods.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 120;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Version";
            this.columnHeader2.Width = 50;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Manager Version";
            this.columnHeader3.Width = 70;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Status";
            this.columnHeader4.Width = 100;
            // 
            // ModcontextMenuStrip1
            // 
            this.ModcontextMenuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.ModcontextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.installToolStripMenuItem,
            this.updateToolStripMenuItem,
            this.revertToolStripMenuItem,
            this.uninstallToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.checkToolStripMenuItem,
            this.wwwToolStripMenuItem1,
            this.openFolderToolStripMenuItem});
            this.ModcontextMenuStrip1.Name = "ModcontextMenuStrip1";
            this.ModcontextMenuStrip1.Size = new System.Drawing.Size(149, 180);
            this.ModcontextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.ModcontextMenuStrip1_Opening);
            // 
            // installToolStripMenuItem
            // 
            this.installToolStripMenuItem.Name = "installToolStripMenuItem";
            this.installToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.installToolStripMenuItem.Text = "Install";
            this.installToolStripMenuItem.Click += new System.EventHandler(this.installToolStripMenuItem_Click);
            // 
            // updateToolStripMenuItem
            // 
            this.updateToolStripMenuItem.Name = "updateToolStripMenuItem";
            this.updateToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.updateToolStripMenuItem.Text = "Update";
            this.updateToolStripMenuItem.Click += new System.EventHandler(this.updateToolStripMenuItem_Click);
            // 
            // revertToolStripMenuItem
            // 
            this.revertToolStripMenuItem.Name = "revertToolStripMenuItem";
            this.revertToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.revertToolStripMenuItem.Text = "Revert";
            this.revertToolStripMenuItem.Click += new System.EventHandler(this.revertToolStripMenuItem_Click);
            // 
            // uninstallToolStripMenuItem
            // 
            this.uninstallToolStripMenuItem.Name = "uninstallToolStripMenuItem";
            this.uninstallToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.uninstallToolStripMenuItem.Text = "Uninstall";
            this.uninstallToolStripMenuItem.Click += new System.EventHandler(this.uninstallToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.deleteToolStripMenuItem.Text = "Remove";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // checkToolStripMenuItem
            // 
            this.checkToolStripMenuItem.Name = "checkToolStripMenuItem";
            this.checkToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.checkToolStripMenuItem.Text = "Check Update";
            this.checkToolStripMenuItem.Click += new System.EventHandler(this.checkToolStripMenuItem_Click);
            // 
            // wwwToolStripMenuItem1
            // 
            this.wwwToolStripMenuItem1.Name = "wwwToolStripMenuItem1";
            this.wwwToolStripMenuItem1.Size = new System.Drawing.Size(148, 22);
            this.wwwToolStripMenuItem1.Text = "Home Page";
            this.wwwToolStripMenuItem1.Click += new System.EventHandler(this.wwwToolStripMenuItem1_Click);
            // 
            // openFolderToolStripMenuItem
            // 
            this.openFolderToolStripMenuItem.Name = "openFolderToolStripMenuItem";
            this.openFolderToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.openFolderToolStripMenuItem.Text = "Open Folder";
            this.openFolderToolStripMenuItem.Click += new System.EventHandler(this.openFolderToolStripMenuItem_Click);
            // 
            // splitContainerModsInstall
            // 
            this.splitContainerModsInstall.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerModsInstall.Location = new System.Drawing.Point(0, 0);
            this.splitContainerModsInstall.Name = "splitContainerModsInstall";
            this.splitContainerModsInstall.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerModsInstall.Panel1
            // 
            this.splitContainerModsInstall.Panel1.Controls.Add(this.btnCheckUpdates);
            this.splitContainerModsInstall.Panel1.Controls.Add(this.btnModInstall);
            // 
            // splitContainerModsInstall.Panel2
            // 
            this.splitContainerModsInstall.Panel2.BackgroundImage = global::UnityModManagerNet.Installer.Properties.Resources.dragdropfiles;
            this.splitContainerModsInstall.Panel2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.splitContainerModsInstall.Size = new System.Drawing.Size(344, 142);
            this.splitContainerModsInstall.SplitterDistance = 45;
            this.splitContainerModsInstall.TabIndex = 0;
            // 
            // btnCheckUpdates
            // 
            this.btnCheckUpdates.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnCheckUpdates.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.25F);
            this.btnCheckUpdates.Location = new System.Drawing.Point(174, 0);
            this.btnCheckUpdates.Name = "btnCheckUpdates";
            this.btnCheckUpdates.Size = new System.Drawing.Size(170, 45);
            this.btnCheckUpdates.TabIndex = 1;
            this.btnCheckUpdates.Text = "Check Updates";
            this.btnCheckUpdates.UseVisualStyleBackColor = true;
            this.btnCheckUpdates.Click += new System.EventHandler(this.btnCheckUpdates_Click);
            // 
            // btnModInstall
            // 
            this.btnModInstall.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnModInstall.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.25F);
            this.btnModInstall.Location = new System.Drawing.Point(0, 0);
            this.btnModInstall.Name = "btnModInstall";
            this.btnModInstall.Size = new System.Drawing.Size(174, 45);
            this.btnModInstall.TabIndex = 0;
            this.btnModInstall.Text = "Install Mod";
            this.btnModInstall.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.BackColor = System.Drawing.Color.WhiteSmoke;
            this.tabPage3.Controls.Add(this.inputLog);
            this.tabPage3.Location = new System.Drawing.Point(4, 28);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(350, 408);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Log";
            // 
            // inputLog
            // 
            this.inputLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputLog.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.25F);
            this.inputLog.Location = new System.Drawing.Point(3, 3);
            this.inputLog.Multiline = true;
            this.inputLog.Name = "inputLog";
            this.inputLog.ReadOnly = true;
            this.inputLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.inputLog.Size = new System.Drawing.Size(344, 402);
            this.inputLog.TabIndex = 10;
            // 
            // tabPage4
            // 
            this.tabPage4.BackColor = System.Drawing.Color.WhiteSmoke;
            this.tabPage4.Controls.Add(this.resetFirewallGroup);
            this.tabPage4.Controls.Add(this.updateCheckingModeGroup);
            this.tabPage4.Controls.Add(this.label4);
            this.tabPage4.Controls.Add(this.btnGetApiKey);
            this.tabPage4.Controls.Add(this.textBoxApiKey);
            this.tabPage4.Controls.Add(this.label1);
            this.tabPage4.Location = new System.Drawing.Point(4, 28);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(350, 408);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Settings";
            // 
            // resetFirewallGroup
            // 
            this.resetFirewallGroup.Controls.Add(this.btnRemFirewallGame);
            this.resetFirewallGroup.Controls.Add(this.btnRemFirewallInstaller);
            this.resetFirewallGroup.Location = new System.Drawing.Point(9, 201);
            this.resetFirewallGroup.Name = "resetFirewallGroup";
            this.resetFirewallGroup.Size = new System.Drawing.Size(329, 94);
            this.resetFirewallGroup.TabIndex = 9;
            this.resetFirewallGroup.TabStop = false;
            this.resetFirewallGroup.Text = "Reset firewall rules";
            // 
            // btnRemFirewallGame
            // 
            this.btnRemFirewallGame.Location = new System.Drawing.Point(5, 57);
            this.btnRemFirewallGame.Name = "btnRemFirewallGame";
            this.btnRemFirewallGame.Size = new System.Drawing.Size(319, 31);
            this.btnRemFirewallGame.TabIndex = 7;
            this.btnRemFirewallGame.Text = "For the Game";
            this.btnRemFirewallGame.UseVisualStyleBackColor = true;
            this.btnRemFirewallGame.Click += new System.EventHandler(this.btnRemFirewallGame_Click);
            // 
            // btnRemFirewallInstaller
            // 
            this.btnRemFirewallInstaller.Location = new System.Drawing.Point(5, 22);
            this.btnRemFirewallInstaller.Name = "btnRemFirewallInstaller";
            this.btnRemFirewallInstaller.Size = new System.Drawing.Size(319, 31);
            this.btnRemFirewallInstaller.TabIndex = 8;
            this.btnRemFirewallInstaller.Text = "For the Installer";
            this.btnRemFirewallInstaller.UseVisualStyleBackColor = true;
            this.btnRemFirewallInstaller.Click += new System.EventHandler(this.btnRemFirewallInstaller_Click);
            // 
            // updateCheckingModeGroup
            // 
            this.updateCheckingModeGroup.Location = new System.Drawing.Point(9, 137);
            this.updateCheckingModeGroup.Name = "updateCheckingModeGroup";
            this.updateCheckingModeGroup.Padding = new System.Windows.Forms.Padding(10, 0, 3, 3);
            this.updateCheckingModeGroup.Size = new System.Drawing.Size(329, 57);
            this.updateCheckingModeGroup.TabIndex = 6;
            this.updateCheckingModeGroup.TabStop = false;
            this.updateCheckingModeGroup.Text = "Check updates";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(60, 72);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(278, 58);
            this.label4.TabIndex = 3;
            this.label4.Text = "Get API key on nexusmods, this will allow you to receive notifications about new " +
    "versions of mods.";
            // 
            // btnGetApiKey
            // 
            this.btnGetApiKey.Location = new System.Drawing.Point(63, 37);
            this.btnGetApiKey.Name = "btnGetApiKey";
            this.btnGetApiKey.Size = new System.Drawing.Size(101, 30);
            this.btnGetApiKey.TabIndex = 2;
            this.btnGetApiKey.Text = "Get API key";
            this.btnGetApiKey.UseVisualStyleBackColor = true;
            this.btnGetApiKey.Click += new System.EventHandler(this.btnGetApiKey_Click);
            // 
            // textBoxApiKey
            // 
            this.textBoxApiKey.Location = new System.Drawing.Point(64, 5);
            this.textBoxApiKey.Name = "textBoxApiKey";
            this.textBoxApiKey.Size = new System.Drawing.Size(270, 23);
            this.textBoxApiKey.TabIndex = 1;
            this.textBoxApiKey.TextChanged += new System.EventHandler(this.textBoxApiKey_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(6, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "API key";
            // 
            // statusStrip1
            // 
            this.statusStrip1.AutoSize = false;
            this.statusStrip1.Dock = System.Windows.Forms.DockStyle.Top;
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.statusStrip1.Location = new System.Drawing.Point(0, 0);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.ShowItemToolTips = true;
            this.statusStrip1.Size = new System.Drawing.Size(358, 20);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.statusLabel.MergeAction = System.Windows.Forms.MergeAction.Replace;
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.statusLabel.Size = new System.Drawing.Size(39, 15);
            this.statusLabel.Text = "Ready";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // folderBrowserDialog
            // 
            this.folderBrowserDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
            this.folderBrowserDialog.HelpRequest += new System.EventHandler(this.folderBrowserDialog_HelpRequest);
            // 
            // modInstallFileDialog
            // 
            this.modInstallFileDialog.Filter = "ZIP|*.zip";
            this.modInstallFileDialog.Multiselect = true;
            // 
            // btnSetFolder
            // 
            this.btnSetFolder.Location = new System.Drawing.Point(185, 178);
            this.btnSetFolder.Name = "btnSetFolder";
            this.btnSetFolder.Size = new System.Drawing.Size(26, 26);
            this.btnSetFolder.TabIndex = 24;
            this.btnSetFolder.Text = "...";
            this.btnSetFolder.UseVisualStyleBackColor = true;
            this.btnSetFolder.Click += new System.EventHandler(this.btnSetFolder_Click);
            // 
            // UnityModManagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(358, 469);
            this.Controls.Add(this.panelMain);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(374, 1000);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(374, 508);
            this.Name = "UnityModManagerForm";
            this.ShowIcon = false;
            this.Text = "UnityModManager Installer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UnityModLoaderForm_FormClosing);
            this.panelMain.ResumeLayout(false);
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.additionallyGroupBox.ResumeLayout(false);
            this.extraFilesGroupBox.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.splitContainerMods.Panel1.ResumeLayout(false);
            this.splitContainerMods.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMods)).EndInit();
            this.splitContainerMods.ResumeLayout(false);
            this.ModcontextMenuStrip1.ResumeLayout(false);
            this.splitContainerModsInstall.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerModsInstall)).EndInit();
            this.splitContainerModsInstall.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.resetFirewallGroup.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Label currentVersion;
        private System.Windows.Forms.Label installedVersion;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox gameList;
        private System.Windows.Forms.Button btnOpenFolder;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnInstall;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.SplitContainer splitContainerMods;
        private System.Windows.Forms.ListView listMods;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.TabPage tabPage3;
        public System.Windows.Forms.TextBox inputLog;
        public System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ContextMenuStrip ModcontextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem updateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem installToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uninstallToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem revertToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wwwToolStripMenuItem1;
        private System.Windows.Forms.SplitContainer splitContainerModsInstall;
        private System.Windows.Forms.Button btnModInstall;
        private System.Windows.Forms.OpenFileDialog modInstallFileDialog;
        private System.Windows.Forms.Button btnDownloadUpdate;
        private System.Windows.Forms.Button btnRestore;
        private System.Windows.Forms.GroupBox installTypeGroup;
        private System.Windows.Forms.RichTextBox notesTextBox;
        private System.Windows.Forms.GroupBox additionallyGroupBox;
        private System.Windows.Forms.Label labelGame;
        private System.Windows.Forms.Label labelFolder;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox extraFilesGroupBox;
        private System.Windows.Forms.RichTextBox extraFilesTextBox;
        private System.Windows.Forms.Button extraFilesAutoButton;
        private System.Windows.Forms.Button extraFilesManualButton;
        private System.Windows.Forms.ToolStripMenuItem openFolderToolStripMenuItem;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxApiKey;
        private System.Windows.Forms.ToolStripMenuItem checkToolStripMenuItem;
        private System.Windows.Forms.Button btnGetApiKey;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox updateCheckingModeGroup;
        private System.Windows.Forms.Button btnCheckUpdates;
        private System.Windows.Forms.Button btnRemFirewallGame;
        private System.Windows.Forms.GroupBox resetFirewallGroup;
        private System.Windows.Forms.Button btnRemFirewallInstaller;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.Button btnSetFolder;
    }
}

