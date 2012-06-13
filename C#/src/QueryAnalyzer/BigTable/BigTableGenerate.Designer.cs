namespace QueryAnalyzer.BigTable
{
    partial class BigTableGenerate
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageGlobal = new System.Windows.Forms.TabPage();
            this.checkBoxKeepDataIntegrity = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDownExecuteTimeout = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.labelLastUpdateTime = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxTableName = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.textBoxIndexFolder = new System.Windows.Forms.TextBox();
            this.tabPageServers = new System.Windows.Forms.TabPage();
            this.panelServers = new System.Windows.Forms.Panel();
            this.buttonUpdateServer = new System.Windows.Forms.Button();
            this.listViewServers = new System.Windows.Forms.ListView();
            this.buttonDeleteServer = new System.Windows.Forms.Button();
            this.buttonAddServer = new System.Windows.Forms.Button();
            this.tabPageTables = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBoxFailoverServers = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBoxFailoverServers = new System.Windows.Forms.ComboBox();
            this.buttonDelFailoverServers = new System.Windows.Forms.Button();
            this.listBoxFailoverServers = new System.Windows.Forms.ListBox();
            this.buttonAddFailoverServers = new System.Windows.Forms.Button();
            this.groupBoxBalanceServers = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBoxBalanceServers = new System.Windows.Forms.ComboBox();
            this.buttonDeleteBS = new System.Windows.Forms.Button();
            this.listBoxBalanceServers = new System.Windows.Forms.ListBox();
            this.buttonAddBS = new System.Windows.Forms.Button();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.listBoxTablets = new System.Windows.Forms.ListBox();
            this.labelBalanceEnabled = new System.Windows.Forms.Label();
            this.labelFailoverEnabled = new System.Windows.Forms.Label();
            this.buttonEnableBS = new System.Windows.Forms.Button();
            this.buttonEnableFS = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.tabPageGlobal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownExecuteTimeout)).BeginInit();
            this.tabPageServers.SuspendLayout();
            this.panelServers.SuspendLayout();
            this.tabPageTables.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBoxFailoverServers.SuspendLayout();
            this.groupBoxBalanceServers.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageGlobal);
            this.tabControl.Controls.Add(this.tabPageServers);
            this.tabControl.Controls.Add(this.tabPageTables);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(748, 501);
            this.tabControl.TabIndex = 0;
            // 
            // tabPageGlobal
            // 
            this.tabPageGlobal.Controls.Add(this.checkBoxKeepDataIntegrity);
            this.tabPageGlobal.Controls.Add(this.label3);
            this.tabPageGlobal.Controls.Add(this.numericUpDownExecuteTimeout);
            this.tabPageGlobal.Controls.Add(this.label2);
            this.tabPageGlobal.Controls.Add(this.labelLastUpdateTime);
            this.tabPageGlobal.Controls.Add(this.buttonCancel);
            this.tabPageGlobal.Controls.Add(this.buttonSave);
            this.tabPageGlobal.Controls.Add(this.label1);
            this.tabPageGlobal.Controls.Add(this.textBoxTableName);
            this.tabPageGlobal.Controls.Add(this.label8);
            this.tabPageGlobal.Controls.Add(this.textBoxIndexFolder);
            this.tabPageGlobal.Location = new System.Drawing.Point(4, 22);
            this.tabPageGlobal.Name = "tabPageGlobal";
            this.tabPageGlobal.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageGlobal.Size = new System.Drawing.Size(740, 475);
            this.tabPageGlobal.TabIndex = 0;
            this.tabPageGlobal.Text = "Global Setting";
            this.tabPageGlobal.UseVisualStyleBackColor = true;
            // 
            // checkBoxKeepDataIntegrity
            // 
            this.checkBoxKeepDataIntegrity.AutoSize = true;
            this.checkBoxKeepDataIntegrity.Checked = true;
            this.checkBoxKeepDataIntegrity.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxKeepDataIntegrity.Location = new System.Drawing.Point(19, 152);
            this.checkBoxKeepDataIntegrity.Name = "checkBoxKeepDataIntegrity";
            this.checkBoxKeepDataIntegrity.Size = new System.Drawing.Size(117, 17);
            this.checkBoxKeepDataIntegrity.TabIndex = 39;
            this.checkBoxKeepDataIntegrity.Text = "Keep Data Integrity";
            this.checkBoxKeepDataIntegrity.UseVisualStyleBackColor = true;
            this.checkBoxKeepDataIntegrity.CheckedChanged += new System.EventHandler(this.checkBoxKeepDataIntegrity_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(217, 108);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(20, 13);
            this.label3.TabIndex = 38;
            this.label3.Text = "ms";
            // 
            // numericUpDownExecuteTimeout
            // 
            this.numericUpDownExecuteTimeout.Location = new System.Drawing.Point(118, 105);
            this.numericUpDownExecuteTimeout.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.numericUpDownExecuteTimeout.Minimum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numericUpDownExecuteTimeout.Name = "numericUpDownExecuteTimeout";
            this.numericUpDownExecuteTimeout.Size = new System.Drawing.Size(80, 20);
            this.numericUpDownExecuteTimeout.TabIndex = 37;
            this.numericUpDownExecuteTimeout.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.numericUpDownExecuteTimeout.ValueChanged += new System.EventHandler(this.numericUpDownExecuteTimeout_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 108);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 13);
            this.label2.TabIndex = 36;
            this.label2.Text = "Execute Timeout";
            // 
            // labelLastUpdateTime
            // 
            this.labelLastUpdateTime.AutoSize = true;
            this.labelLastUpdateTime.Location = new System.Drawing.Point(394, 19);
            this.labelLastUpdateTime.Name = "labelLastUpdateTime";
            this.labelLastUpdateTime.Size = new System.Drawing.Size(94, 13);
            this.labelLastUpdateTime.TabIndex = 35;
            this.labelLastUpdateTime.Text = "Last Update Time:";
            this.labelLastUpdateTime.Visible = false;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(189, 354);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 34;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(19, 354);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(141, 23);
            this.buttonSave.TabIndex = 33;
            this.buttonSave.Text = "&Save Big Table Settings";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 32;
            this.label1.Text = "BigTable Name";
            // 
            // textBoxTableName
            // 
            this.textBoxTableName.Location = new System.Drawing.Point(102, 18);
            this.textBoxTableName.Name = "textBoxTableName";
            this.textBoxTableName.Size = new System.Drawing.Size(241, 20);
            this.textBoxTableName.TabIndex = 29;
            this.textBoxTableName.TextChanged += new System.EventHandler(this.textBoxTableName_TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(16, 59);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 13);
            this.label8.TabIndex = 31;
            this.label8.Text = "Index Folder";
            // 
            // textBoxIndexFolder
            // 
            this.textBoxIndexFolder.Location = new System.Drawing.Point(102, 56);
            this.textBoxIndexFolder.Name = "textBoxIndexFolder";
            this.textBoxIndexFolder.Size = new System.Drawing.Size(241, 20);
            this.textBoxIndexFolder.TabIndex = 30;
            // 
            // tabPageServers
            // 
            this.tabPageServers.Controls.Add(this.panelServers);
            this.tabPageServers.Location = new System.Drawing.Point(4, 22);
            this.tabPageServers.Name = "tabPageServers";
            this.tabPageServers.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageServers.Size = new System.Drawing.Size(740, 475);
            this.tabPageServers.TabIndex = 2;
            this.tabPageServers.Text = "Servers";
            this.tabPageServers.UseVisualStyleBackColor = true;
            // 
            // panelServers
            // 
            this.panelServers.Controls.Add(this.buttonUpdateServer);
            this.panelServers.Controls.Add(this.listViewServers);
            this.panelServers.Controls.Add(this.buttonDeleteServer);
            this.panelServers.Controls.Add(this.buttonAddServer);
            this.panelServers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelServers.Location = new System.Drawing.Point(3, 3);
            this.panelServers.Name = "panelServers";
            this.panelServers.Size = new System.Drawing.Size(734, 469);
            this.panelServers.TabIndex = 6;
            // 
            // buttonUpdateServer
            // 
            this.buttonUpdateServer.Location = new System.Drawing.Point(165, 355);
            this.buttonUpdateServer.Name = "buttonUpdateServer";
            this.buttonUpdateServer.Size = new System.Drawing.Size(75, 23);
            this.buttonUpdateServer.TabIndex = 6;
            this.buttonUpdateServer.Text = "Update";
            this.buttonUpdateServer.UseVisualStyleBackColor = true;
            this.buttonUpdateServer.Click += new System.EventHandler(this.buttonUpdateServer_Click);
            // 
            // listViewServers
            // 
            this.listViewServers.Dock = System.Windows.Forms.DockStyle.Top;
            this.listViewServers.Location = new System.Drawing.Point(0, 0);
            this.listViewServers.Name = "listViewServers";
            this.listViewServers.Size = new System.Drawing.Size(734, 349);
            this.listViewServers.TabIndex = 5;
            this.listViewServers.UseCompatibleStateImageBehavior = false;
            this.listViewServers.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listViewServers_MouseDoubleClick);
            // 
            // buttonDeleteServer
            // 
            this.buttonDeleteServer.Location = new System.Drawing.Point(84, 355);
            this.buttonDeleteServer.Name = "buttonDeleteServer";
            this.buttonDeleteServer.Size = new System.Drawing.Size(75, 23);
            this.buttonDeleteServer.TabIndex = 4;
            this.buttonDeleteServer.Text = "Delete";
            this.buttonDeleteServer.UseVisualStyleBackColor = true;
            this.buttonDeleteServer.Click += new System.EventHandler(this.buttonDeleteServer_Click);
            // 
            // buttonAddServer
            // 
            this.buttonAddServer.Location = new System.Drawing.Point(3, 355);
            this.buttonAddServer.Name = "buttonAddServer";
            this.buttonAddServer.Size = new System.Drawing.Size(75, 23);
            this.buttonAddServer.TabIndex = 3;
            this.buttonAddServer.Text = "Add";
            this.buttonAddServer.UseVisualStyleBackColor = true;
            this.buttonAddServer.Click += new System.EventHandler(this.buttonAddServer_Click);
            // 
            // tabPageTables
            // 
            this.tabPageTables.Controls.Add(this.panel1);
            this.tabPageTables.Controls.Add(this.splitter1);
            this.tabPageTables.Controls.Add(this.listBoxTablets);
            this.tabPageTables.Location = new System.Drawing.Point(4, 22);
            this.tabPageTables.Name = "tabPageTables";
            this.tabPageTables.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTables.Size = new System.Drawing.Size(740, 475);
            this.tabPageTables.TabIndex = 1;
            this.tabPageTables.Text = "Tablets";
            this.tabPageTables.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBoxFailoverServers);
            this.panel1.Controls.Add(this.groupBoxBalanceServers);
            this.panel1.Controls.Add(this.buttonDelete);
            this.panel1.Controls.Add(this.buttonAdd);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(202, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(535, 469);
            this.panel1.TabIndex = 2;
            // 
            // groupBoxFailoverServers
            // 
            this.groupBoxFailoverServers.Controls.Add(this.buttonEnableFS);
            this.groupBoxFailoverServers.Controls.Add(this.labelFailoverEnabled);
            this.groupBoxFailoverServers.Controls.Add(this.label7);
            this.groupBoxFailoverServers.Controls.Add(this.label5);
            this.groupBoxFailoverServers.Controls.Add(this.comboBoxFailoverServers);
            this.groupBoxFailoverServers.Controls.Add(this.buttonDelFailoverServers);
            this.groupBoxFailoverServers.Controls.Add(this.listBoxFailoverServers);
            this.groupBoxFailoverServers.Controls.Add(this.buttonAddFailoverServers);
            this.groupBoxFailoverServers.Location = new System.Drawing.Point(27, 274);
            this.groupBoxFailoverServers.Name = "groupBoxFailoverServers";
            this.groupBoxFailoverServers.Size = new System.Drawing.Size(410, 156);
            this.groupBoxFailoverServers.TabIndex = 5;
            this.groupBoxFailoverServers.TabStop = false;
            this.groupBoxFailoverServers.Text = "Failover Servers";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 53);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(81, 13);
            this.label7.TabIndex = 11;
            this.label7.Text = "Failover servers";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 25);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(84, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Choose a server";
            // 
            // comboBoxFailoverServers
            // 
            this.comboBoxFailoverServers.FormattingEnabled = true;
            this.comboBoxFailoverServers.Location = new System.Drawing.Point(97, 20);
            this.comboBoxFailoverServers.Name = "comboBoxFailoverServers";
            this.comboBoxFailoverServers.Size = new System.Drawing.Size(218, 21);
            this.comboBoxFailoverServers.TabIndex = 8;
            this.comboBoxFailoverServers.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.comboBoxServers_KeyPress);
            // 
            // buttonDelFailoverServers
            // 
            this.buttonDelFailoverServers.Location = new System.Drawing.Point(329, 102);
            this.buttonDelFailoverServers.Name = "buttonDelFailoverServers";
            this.buttonDelFailoverServers.Size = new System.Drawing.Size(75, 23);
            this.buttonDelFailoverServers.TabIndex = 7;
            this.buttonDelFailoverServers.Text = "Delete";
            this.buttonDelFailoverServers.UseVisualStyleBackColor = true;
            this.buttonDelFailoverServers.Click += new System.EventHandler(this.buttonDelFailoverServers_Click);
            // 
            // listBoxFailoverServers
            // 
            this.listBoxFailoverServers.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listBoxFailoverServers.FormattingEnabled = true;
            this.listBoxFailoverServers.Location = new System.Drawing.Point(6, 69);
            this.listBoxFailoverServers.Name = "listBoxFailoverServers";
            this.listBoxFailoverServers.Size = new System.Drawing.Size(309, 82);
            this.listBoxFailoverServers.TabIndex = 0;
            this.listBoxFailoverServers.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listBoxBalanceServer_DrawItem);
            this.listBoxFailoverServers.SelectedIndexChanged += new System.EventHandler(this.listBoxFailoverServers_SelectedIndexChanged);
            // 
            // buttonAddFailoverServers
            // 
            this.buttonAddFailoverServers.Location = new System.Drawing.Point(329, 69);
            this.buttonAddFailoverServers.Name = "buttonAddFailoverServers";
            this.buttonAddFailoverServers.Size = new System.Drawing.Size(75, 23);
            this.buttonAddFailoverServers.TabIndex = 5;
            this.buttonAddFailoverServers.Text = "Add";
            this.buttonAddFailoverServers.UseVisualStyleBackColor = true;
            this.buttonAddFailoverServers.Click += new System.EventHandler(this.buttonAddFailoverServers_Click);
            // 
            // groupBoxBalanceServers
            // 
            this.groupBoxBalanceServers.Controls.Add(this.buttonEnableBS);
            this.groupBoxBalanceServers.Controls.Add(this.labelBalanceEnabled);
            this.groupBoxBalanceServers.Controls.Add(this.label6);
            this.groupBoxBalanceServers.Controls.Add(this.label4);
            this.groupBoxBalanceServers.Controls.Add(this.comboBoxBalanceServers);
            this.groupBoxBalanceServers.Controls.Add(this.buttonDeleteBS);
            this.groupBoxBalanceServers.Controls.Add(this.listBoxBalanceServers);
            this.groupBoxBalanceServers.Controls.Add(this.buttonAddBS);
            this.groupBoxBalanceServers.Location = new System.Drawing.Point(27, 14);
            this.groupBoxBalanceServers.Name = "groupBoxBalanceServers";
            this.groupBoxBalanceServers.Size = new System.Drawing.Size(410, 254);
            this.groupBoxBalanceServers.TabIndex = 4;
            this.groupBoxBalanceServers.TabStop = false;
            this.groupBoxBalanceServers.Text = "Balance Servers";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 53);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(83, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Balance servers";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 24);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(84, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Choose a server";
            // 
            // comboBoxBalanceServers
            // 
            this.comboBoxBalanceServers.FormattingEnabled = true;
            this.comboBoxBalanceServers.Location = new System.Drawing.Point(97, 20);
            this.comboBoxBalanceServers.Name = "comboBoxBalanceServers";
            this.comboBoxBalanceServers.Size = new System.Drawing.Size(218, 21);
            this.comboBoxBalanceServers.TabIndex = 8;
            this.comboBoxBalanceServers.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.comboBoxServers_KeyPress);
            // 
            // buttonDeleteBS
            // 
            this.buttonDeleteBS.Location = new System.Drawing.Point(329, 107);
            this.buttonDeleteBS.Name = "buttonDeleteBS";
            this.buttonDeleteBS.Size = new System.Drawing.Size(75, 23);
            this.buttonDeleteBS.TabIndex = 7;
            this.buttonDeleteBS.Text = "Delete";
            this.buttonDeleteBS.UseVisualStyleBackColor = true;
            this.buttonDeleteBS.Click += new System.EventHandler(this.buttonDeleteBS_Click);
            // 
            // listBoxBalanceServers
            // 
            this.listBoxBalanceServers.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listBoxBalanceServers.FormattingEnabled = true;
            this.listBoxBalanceServers.Location = new System.Drawing.Point(6, 69);
            this.listBoxBalanceServers.Name = "listBoxBalanceServers";
            this.listBoxBalanceServers.Size = new System.Drawing.Size(309, 173);
            this.listBoxBalanceServers.TabIndex = 0;
            this.listBoxBalanceServers.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listBoxBalanceServer_DrawItem);
            this.listBoxBalanceServers.SelectedIndexChanged += new System.EventHandler(this.listBoxBalanceServer_SelectedIndexChanged);
            // 
            // buttonAddBS
            // 
            this.buttonAddBS.Location = new System.Drawing.Point(329, 69);
            this.buttonAddBS.Name = "buttonAddBS";
            this.buttonAddBS.Size = new System.Drawing.Size(75, 23);
            this.buttonAddBS.TabIndex = 5;
            this.buttonAddBS.Text = "Add";
            this.buttonAddBS.UseVisualStyleBackColor = true;
            this.buttonAddBS.Click += new System.EventHandler(this.buttonAddBS_Click);
            // 
            // buttonDelete
            // 
            this.buttonDelete.Location = new System.Drawing.Point(107, 436);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(75, 23);
            this.buttonDelete.TabIndex = 2;
            this.buttonDelete.Text = "Delete";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Location = new System.Drawing.Point(26, 436);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(75, 23);
            this.buttonAdd.TabIndex = 0;
            this.buttonAdd.Text = "Add";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(199, 3);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 469);
            this.splitter1.TabIndex = 1;
            this.splitter1.TabStop = false;
            // 
            // listBoxTablets
            // 
            this.listBoxTablets.Dock = System.Windows.Forms.DockStyle.Left;
            this.listBoxTablets.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listBoxTablets.FormattingEnabled = true;
            this.listBoxTablets.Location = new System.Drawing.Point(3, 3);
            this.listBoxTablets.Name = "listBoxTablets";
            this.listBoxTablets.Size = new System.Drawing.Size(196, 459);
            this.listBoxTablets.TabIndex = 0;
            this.listBoxTablets.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listBoxTablets_DrawItem);
            this.listBoxTablets.SelectedIndexChanged += new System.EventHandler(this.listBoxTablets_SelectedIndexChanged);
            // 
            // labelBalanceEnabled
            // 
            this.labelBalanceEnabled.AutoSize = true;
            this.labelBalanceEnabled.Location = new System.Drawing.Point(96, 53);
            this.labelBalanceEnabled.Name = "labelBalanceEnabled";
            this.labelBalanceEnabled.Size = new System.Drawing.Size(46, 13);
            this.labelBalanceEnabled.TabIndex = 11;
            this.labelBalanceEnabled.Text = "Enabled";
            // 
            // labelFailoverEnabled
            // 
            this.labelFailoverEnabled.AutoSize = true;
            this.labelFailoverEnabled.Location = new System.Drawing.Point(94, 53);
            this.labelFailoverEnabled.Name = "labelFailoverEnabled";
            this.labelFailoverEnabled.Size = new System.Drawing.Size(46, 13);
            this.labelFailoverEnabled.TabIndex = 12;
            this.labelFailoverEnabled.Text = "Enabled";
            // 
            // buttonEnableBS
            // 
            this.buttonEnableBS.Location = new System.Drawing.Point(329, 148);
            this.buttonEnableBS.Name = "buttonEnableBS";
            this.buttonEnableBS.Size = new System.Drawing.Size(75, 23);
            this.buttonEnableBS.TabIndex = 12;
            this.buttonEnableBS.Text = "Enable";
            this.buttonEnableBS.UseVisualStyleBackColor = true;
            this.buttonEnableBS.Click += new System.EventHandler(this.buttonEnableBS_Click);
            // 
            // buttonEnableFS
            // 
            this.buttonEnableFS.Location = new System.Drawing.Point(329, 131);
            this.buttonEnableFS.Name = "buttonEnableFS";
            this.buttonEnableFS.Size = new System.Drawing.Size(75, 23);
            this.buttonEnableFS.TabIndex = 13;
            this.buttonEnableFS.Text = "Enable";
            this.buttonEnableFS.UseVisualStyleBackColor = true;
            this.buttonEnableFS.Click += new System.EventHandler(this.buttonEnableFS_Click);
            // 
            // BigTableGenerate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl);
            this.Name = "BigTableGenerate";
            this.Size = new System.Drawing.Size(748, 501);
            this.Load += new System.EventHandler(this.BigTableGenerate_Load);
            this.tabControl.ResumeLayout(false);
            this.tabPageGlobal.ResumeLayout(false);
            this.tabPageGlobal.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownExecuteTimeout)).EndInit();
            this.tabPageServers.ResumeLayout(false);
            this.panelServers.ResumeLayout(false);
            this.tabPageTables.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.groupBoxFailoverServers.ResumeLayout(false);
            this.groupBoxFailoverServers.PerformLayout();
            this.groupBoxBalanceServers.ResumeLayout(false);
            this.groupBoxBalanceServers.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageGlobal;
        private System.Windows.Forms.TabPage tabPageTables;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.ListBox listBoxTablets;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Label label1;
        internal System.Windows.Forms.TextBox textBoxTableName;
        private System.Windows.Forms.Label label8;
        internal System.Windows.Forms.TextBox textBoxIndexFolder;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.GroupBox groupBoxBalanceServers;
        private System.Windows.Forms.ListBox listBoxBalanceServers;
        private System.Windows.Forms.Button buttonDeleteBS;
        private System.Windows.Forms.Button buttonAddBS;
        private System.Windows.Forms.TabPage tabPageServers;
        private System.Windows.Forms.ComboBox comboBoxBalanceServers;
        private System.Windows.Forms.Button buttonDeleteServer;
        private System.Windows.Forms.Button buttonAddServer;
        private System.Windows.Forms.ListView listViewServers;
        private System.Windows.Forms.Panel panelServers;
        private System.Windows.Forms.GroupBox groupBoxFailoverServers;
        private System.Windows.Forms.ComboBox comboBoxFailoverServers;
        private System.Windows.Forms.Button buttonDelFailoverServers;
        private System.Windows.Forms.ListBox listBoxFailoverServers;
        private System.Windows.Forms.Button buttonAddFailoverServers;
        private System.Windows.Forms.Label labelLastUpdateTime;
        private System.Windows.Forms.NumericUpDown numericUpDownExecuteTimeout;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkBoxKeepDataIntegrity;
        private System.Windows.Forms.Button buttonUpdateServer;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label labelFailoverEnabled;
        private System.Windows.Forms.Label labelBalanceEnabled;
        private System.Windows.Forms.Button buttonEnableBS;
        private System.Windows.Forms.Button buttonEnableFS;
    }
}
