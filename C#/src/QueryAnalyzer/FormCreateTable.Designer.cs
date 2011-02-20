namespace QueryAnalyzer
{
    partial class FormCreateTable
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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.buttonTestConnectionString = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxTableName = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.textBoxExample = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxConnectionString = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBoxDBAdapter = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxIndexFolder = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.buttonMirrorTable = new System.Windows.Forms.Button();
            this.groupBoxIncrementalMode = new System.Windows.Forms.GroupBox();
            this.radioButtonAll = new System.Windows.Forms.RadioButton();
            this.radioButtonAppendOnly = new System.Windows.Forms.RadioButton();
            this.textBoxDBTableName = new System.Windows.Forms.TextBox();
            this.labelDBTableName = new System.Windows.Forms.Label();
            this.groupBoxMode = new System.Windows.Forms.GroupBox();
            this.radioButtonCreateTableFromExistTable = new System.Windows.Forms.RadioButton();
            this.radioButtonCreateNewTable = new System.Windows.Forms.RadioButton();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.panelDocIdReplaceField = new System.Windows.Forms.Panel();
            this.textBoxDocIdReplaceField = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.buttonAddField = new System.Windows.Forms.Button();
            this.panelFields = new System.Windows.Forms.Panel();
            this.panelHead = new System.Windows.Forms.Panel();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.textBoxScript = new System.Windows.Forms.TextBox();
            this.buttonBack = new System.Windows.Forms.Button();
            this.buttonNext = new System.Windows.Forms.Button();
            this.buttonFinish = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBoxIncrementalMode.SuspendLayout();
            this.groupBoxMode.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.panelDocIdReplaceField.SuspendLayout();
            this.panelFields.SuspendLayout();
            this.panelHead.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPage1);
            this.tabControl.Controls.Add(this.tabPage2);
            this.tabControl.Controls.Add(this.tabPage3);
            this.tabControl.Controls.Add(this.tabPage4);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(846, 479);
            this.tabControl.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.buttonTestConnectionString);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.textBoxTableName);
            this.tabPage1.Controls.Add(this.label8);
            this.tabPage1.Controls.Add(this.textBoxExample);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.textBoxConnectionString);
            this.tabPage1.Controls.Add(this.label6);
            this.tabPage1.Controls.Add(this.comboBoxDBAdapter);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.textBoxIndexFolder);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(838, 453);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Database attributes";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // buttonTestConnectionString
            // 
            this.buttonTestConnectionString.Location = new System.Drawing.Point(31, 288);
            this.buttonTestConnectionString.Name = "buttonTestConnectionString";
            this.buttonTestConnectionString.Size = new System.Drawing.Size(169, 23);
            this.buttonTestConnectionString.TabIndex = 4;
            this.buttonTestConnectionString.Text = "Test DB Connection String";
            this.buttonTestConnectionString.UseVisualStyleBackColor = true;
            this.buttonTestConnectionString.Click += new System.EventHandler(this.buttonTestConnectionString_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(28, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 28;
            this.label1.Text = "Table Name";
            // 
            // textBoxTableName
            // 
            this.textBoxTableName.Location = new System.Drawing.Point(181, 22);
            this.textBoxTableName.Name = "textBoxTableName";
            this.textBoxTableName.Size = new System.Drawing.Size(241, 20);
            this.textBoxTableName.TabIndex = 0;
            this.textBoxTableName.TextChanged += new System.EventHandler(this.textBoxTableName_TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(28, 63);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 13);
            this.label8.TabIndex = 26;
            this.label8.Text = "Index Folder";
            // 
            // textBoxExample
            // 
            this.textBoxExample.BackColor = System.Drawing.SystemColors.Control;
            this.textBoxExample.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxExample.Location = new System.Drawing.Point(181, 207);
            this.textBoxExample.Name = "textBoxExample";
            this.textBoxExample.ReadOnly = true;
            this.textBoxExample.Size = new System.Drawing.Size(346, 13);
            this.textBoxExample.TabIndex = 25;
            this.textBoxExample.Text = "E.g. Data Source=(local);Initial Catalog=xxx;Integrated Security=True";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(178, 188);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(244, 23);
            this.label5.TabIndex = 24;
            this.label5.Text = "Connection string to Database.";
            // 
            // textBoxConnectionString
            // 
            this.textBoxConnectionString.Location = new System.Drawing.Point(181, 165);
            this.textBoxConnectionString.Name = "textBoxConnectionString";
            this.textBoxConnectionString.Size = new System.Drawing.Size(383, 20);
            this.textBoxConnectionString.TabIndex = 3;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(28, 169);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(109, 13);
            this.label6.TabIndex = 22;
            this.label6.Text = "DB Connection String";
            // 
            // comboBoxDBAdapter
            // 
            this.comboBoxDBAdapter.FormattingEnabled = true;
            this.comboBoxDBAdapter.Location = new System.Drawing.Point(181, 113);
            this.comboBoxDBAdapter.Name = "comboBoxDBAdapter";
            this.comboBoxDBAdapter.Size = new System.Drawing.Size(241, 21);
            this.comboBoxDBAdapter.TabIndex = 2;
            this.comboBoxDBAdapter.SelectedIndexChanged += new System.EventHandler(this.comboBoxDBAdapter_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(178, 139);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(110, 13);
            this.label3.TabIndex = 20;
            this.label3.Text = "Choose a DB Adapter";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(28, 115);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 13);
            this.label4.TabIndex = 19;
            this.label4.Text = "DB Adapter";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(178, 83);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(166, 13);
            this.label2.TabIndex = 18;
            this.label2.Text = "Folder path in server. E.g. d:\\Test";
            // 
            // textBoxIndexFolder
            // 
            this.textBoxIndexFolder.Location = new System.Drawing.Point(181, 60);
            this.textBoxIndexFolder.Name = "textBoxIndexFolder";
            this.textBoxIndexFolder.Size = new System.Drawing.Size(241, 20);
            this.textBoxIndexFolder.TabIndex = 1;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.buttonMirrorTable);
            this.tabPage2.Controls.Add(this.groupBoxIncrementalMode);
            this.tabPage2.Controls.Add(this.textBoxDBTableName);
            this.tabPage2.Controls.Add(this.labelDBTableName);
            this.tabPage2.Controls.Add(this.groupBoxMode);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(838, 453);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Index Mode";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // buttonMirrorTable
            // 
            this.buttonMirrorTable.Enabled = false;
            this.buttonMirrorTable.Location = new System.Drawing.Point(30, 380);
            this.buttonMirrorTable.Name = "buttonMirrorTable";
            this.buttonMirrorTable.Size = new System.Drawing.Size(75, 23);
            this.buttonMirrorTable.TabIndex = 35;
            this.buttonMirrorTable.Text = "Mirror table";
            this.buttonMirrorTable.UseVisualStyleBackColor = true;
            this.buttonMirrorTable.Click += new System.EventHandler(this.buttonMirrorTable_Click);
            // 
            // groupBoxIncrementalMode
            // 
            this.groupBoxIncrementalMode.Controls.Add(this.radioButtonAll);
            this.groupBoxIncrementalMode.Controls.Add(this.radioButtonAppendOnly);
            this.groupBoxIncrementalMode.Location = new System.Drawing.Point(30, 248);
            this.groupBoxIncrementalMode.Name = "groupBoxIncrementalMode";
            this.groupBoxIncrementalMode.Size = new System.Drawing.Size(363, 103);
            this.groupBoxIncrementalMode.TabIndex = 34;
            this.groupBoxIncrementalMode.TabStop = false;
            this.groupBoxIncrementalMode.Text = "Incremental Mode";
            // 
            // radioButtonAll
            // 
            this.radioButtonAll.AutoSize = true;
            this.radioButtonAll.Location = new System.Drawing.Point(21, 61);
            this.radioButtonAll.Name = "radioButtonAll";
            this.radioButtonAll.Size = new System.Drawing.Size(74, 17);
            this.radioButtonAll.TabIndex = 1;
            this.radioButtonAll.Text = "Updatable";
            this.radioButtonAll.UseVisualStyleBackColor = true;
            // 
            // radioButtonAppendOnly
            // 
            this.radioButtonAppendOnly.AutoSize = true;
            this.radioButtonAppendOnly.Checked = true;
            this.radioButtonAppendOnly.Location = new System.Drawing.Point(21, 28);
            this.radioButtonAppendOnly.Name = "radioButtonAppendOnly";
            this.radioButtonAppendOnly.Size = new System.Drawing.Size(86, 17);
            this.radioButtonAppendOnly.TabIndex = 0;
            this.radioButtonAppendOnly.TabStop = true;
            this.radioButtonAppendOnly.Text = "Append Only";
            this.radioButtonAppendOnly.UseVisualStyleBackColor = true;
            this.radioButtonAppendOnly.CheckedChanged += new System.EventHandler(this.radioButtonAppendOnly_CheckedChanged);
            // 
            // textBoxDBTableName
            // 
            this.textBoxDBTableName.Location = new System.Drawing.Point(30, 195);
            this.textBoxDBTableName.Name = "textBoxDBTableName";
            this.textBoxDBTableName.Size = new System.Drawing.Size(241, 20);
            this.textBoxDBTableName.TabIndex = 32;
            // 
            // labelDBTableName
            // 
            this.labelDBTableName.AutoSize = true;
            this.labelDBTableName.Location = new System.Drawing.Point(27, 164);
            this.labelDBTableName.Name = "labelDBTableName";
            this.labelDBTableName.Size = new System.Drawing.Size(162, 13);
            this.labelDBTableName.TabIndex = 33;
            this.labelDBTableName.Text = "Exist Table Name or View Name ";
            // 
            // groupBoxMode
            // 
            this.groupBoxMode.Controls.Add(this.radioButtonCreateTableFromExistTable);
            this.groupBoxMode.Controls.Add(this.radioButtonCreateNewTable);
            this.groupBoxMode.Location = new System.Drawing.Point(30, 40);
            this.groupBoxMode.Name = "groupBoxMode";
            this.groupBoxMode.Size = new System.Drawing.Size(363, 103);
            this.groupBoxMode.TabIndex = 0;
            this.groupBoxMode.TabStop = false;
            this.groupBoxMode.Text = "Index Mode";
            // 
            // radioButtonCreateTableFromExistTable
            // 
            this.radioButtonCreateTableFromExistTable.AutoSize = true;
            this.radioButtonCreateTableFromExistTable.Checked = true;
            this.radioButtonCreateTableFromExistTable.Location = new System.Drawing.Point(21, 61);
            this.radioButtonCreateTableFromExistTable.Name = "radioButtonCreateTableFromExistTable";
            this.radioButtonCreateTableFromExistTable.Size = new System.Drawing.Size(150, 17);
            this.radioButtonCreateTableFromExistTable.TabIndex = 1;
            this.radioButtonCreateTableFromExistTable.TabStop = true;
            this.radioButtonCreateTableFromExistTable.Text = "Build Index from exist table";
            this.radioButtonCreateTableFromExistTable.UseVisualStyleBackColor = true;
            this.radioButtonCreateTableFromExistTable.CheckedChanged += new System.EventHandler(this.radioButtonIndexMode_CheckedChanged);
            // 
            // radioButtonCreateNewTable
            // 
            this.radioButtonCreateNewTable.AutoSize = true;
            this.radioButtonCreateNewTable.Location = new System.Drawing.Point(21, 28);
            this.radioButtonCreateNewTable.Name = "radioButtonCreateNewTable";
            this.radioButtonCreateNewTable.Size = new System.Drawing.Size(140, 17);
            this.radioButtonCreateNewTable.TabIndex = 0;
            this.radioButtonCreateNewTable.Text = "Build Index in new table ";
            this.radioButtonCreateNewTable.UseVisualStyleBackColor = true;
            this.radioButtonCreateNewTable.CheckedChanged += new System.EventHandler(this.radioButtonIndexMode_CheckedChanged);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.panelDocIdReplaceField);
            this.tabPage3.Controls.Add(this.buttonDelete);
            this.tabPage3.Controls.Add(this.buttonAddField);
            this.tabPage3.Controls.Add(this.panelFields);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(838, 453);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Fields";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // panelDocIdReplaceField
            // 
            this.panelDocIdReplaceField.Controls.Add(this.textBoxDocIdReplaceField);
            this.panelDocIdReplaceField.Controls.Add(this.label7);
            this.panelDocIdReplaceField.Location = new System.Drawing.Point(461, 419);
            this.panelDocIdReplaceField.Name = "panelDocIdReplaceField";
            this.panelDocIdReplaceField.Size = new System.Drawing.Size(200, 30);
            this.panelDocIdReplaceField.TabIndex = 3;
            this.panelDocIdReplaceField.Visible = false;
            // 
            // textBoxDocIdReplaceField
            // 
            this.textBoxDocIdReplaceField.Location = new System.Drawing.Point(63, 5);
            this.textBoxDocIdReplaceField.Name = "textBoxDocIdReplaceField";
            this.textBoxDocIdReplaceField.Size = new System.Drawing.Size(126, 20);
            this.textBoxDocIdReplaceField.TabIndex = 1;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(10, 10);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(43, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "ID Field";
            // 
            // buttonDelete
            // 
            this.buttonDelete.Location = new System.Drawing.Point(100, 421);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(75, 23);
            this.buttonDelete.TabIndex = 2;
            this.buttonDelete.Text = "Delete";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // buttonAddField
            // 
            this.buttonAddField.Location = new System.Drawing.Point(9, 421);
            this.buttonAddField.Name = "buttonAddField";
            this.buttonAddField.Size = new System.Drawing.Size(75, 23);
            this.buttonAddField.TabIndex = 1;
            this.buttonAddField.Text = "Add";
            this.buttonAddField.UseVisualStyleBackColor = true;
            this.buttonAddField.Click += new System.EventHandler(this.buttonAddField_Click);
            // 
            // panelFields
            // 
            this.panelFields.AutoScroll = true;
            this.panelFields.Controls.Add(this.panelHead);
            this.panelFields.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFields.Location = new System.Drawing.Point(3, 3);
            this.panelFields.Name = "panelFields";
            this.panelFields.Size = new System.Drawing.Size(832, 411);
            this.panelFields.TabIndex = 0;
            // 
            // panelHead
            // 
            this.panelHead.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelHead.Controls.Add(this.label9);
            this.panelHead.Controls.Add(this.label10);
            this.panelHead.Controls.Add(this.label11);
            this.panelHead.Controls.Add(this.label12);
            this.panelHead.Controls.Add(this.label13);
            this.panelHead.Controls.Add(this.label14);
            this.panelHead.Controls.Add(this.label15);
            this.panelHead.Controls.Add(this.label16);
            this.panelHead.Location = new System.Drawing.Point(15, 10);
            this.panelHead.Name = "panelHead";
            this.panelHead.Size = new System.Drawing.Size(800, 34);
            this.panelHead.TabIndex = 9;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(654, 10);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(48, 13);
            this.label9.TabIndex = 7;
            this.label9.Text = "Default";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(46, 10);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(70, 13);
            this.label10.TabIndex = 0;
            this.label10.Text = "Field Name";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(608, 10);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(23, 13);
            this.label11.TabIndex = 6;
            this.label11.Text = "PK";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(156, 10);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(66, 13);
            this.label12.TabIndex = 1;
            this.label12.Text = "Data Type";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(548, 10);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(39, 13);
            this.label13.TabIndex = 5;
            this.label13.Text = "NULL";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(263, 10);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(77, 13);
            this.label14.TabIndex = 2;
            this.label14.Text = "Data Length";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(345, 10);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(70, 13);
            this.label15.TabIndex = 4;
            this.label15.Text = "Index Type";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.Location = new System.Drawing.Point(439, 10);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(55, 13);
            this.label16.TabIndex = 3;
            this.label16.Text = "Analyzer";
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.textBoxScript);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(838, 453);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Script";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // textBoxScript
            // 
            this.textBoxScript.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxScript.Location = new System.Drawing.Point(3, 3);
            this.textBoxScript.Multiline = true;
            this.textBoxScript.Name = "textBoxScript";
            this.textBoxScript.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxScript.Size = new System.Drawing.Size(832, 447);
            this.textBoxScript.TabIndex = 0;
            // 
            // buttonBack
            // 
            this.buttonBack.Location = new System.Drawing.Point(4, 497);
            this.buttonBack.Name = "buttonBack";
            this.buttonBack.Size = new System.Drawing.Size(75, 23);
            this.buttonBack.TabIndex = 1;
            this.buttonBack.Text = "Back";
            this.buttonBack.UseVisualStyleBackColor = true;
            this.buttonBack.Click += new System.EventHandler(this.buttonBack_Click);
            // 
            // buttonNext
            // 
            this.buttonNext.Location = new System.Drawing.Point(113, 497);
            this.buttonNext.Name = "buttonNext";
            this.buttonNext.Size = new System.Drawing.Size(75, 23);
            this.buttonNext.TabIndex = 2;
            this.buttonNext.Text = "Next";
            this.buttonNext.UseVisualStyleBackColor = true;
            this.buttonNext.Click += new System.EventHandler(this.buttonNext_Click);
            // 
            // buttonFinish
            // 
            this.buttonFinish.Enabled = false;
            this.buttonFinish.Location = new System.Drawing.Point(227, 497);
            this.buttonFinish.Name = "buttonFinish";
            this.buttonFinish.Size = new System.Drawing.Size(75, 23);
            this.buttonFinish.TabIndex = 3;
            this.buttonFinish.Text = "Finish";
            this.buttonFinish.UseVisualStyleBackColor = true;
            this.buttonFinish.Click += new System.EventHandler(this.buttonFinish_Click);
            // 
            // FormCreateTable
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(846, 532);
            this.Controls.Add(this.buttonFinish);
            this.Controls.Add(this.buttonNext);
            this.Controls.Add(this.buttonBack);
            this.Controls.Add(this.tabControl);
            this.Name = "FormCreateTable";
            this.Text = "Create Table";
            this.Load += new System.EventHandler(this.FormCreateTable_Load);
            this.tabControl.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.groupBoxIncrementalMode.ResumeLayout(false);
            this.groupBoxIncrementalMode.PerformLayout();
            this.groupBoxMode.ResumeLayout(false);
            this.groupBoxMode.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.panelDocIdReplaceField.ResumeLayout(false);
            this.panelDocIdReplaceField.PerformLayout();
            this.panelFields.ResumeLayout(false);
            this.panelHead.ResumeLayout(false);
            this.panelHead.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button buttonBack;
        private System.Windows.Forms.Button buttonNext;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBoxExample;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        internal System.Windows.Forms.TextBox textBoxConnectionString;
        internal System.Windows.Forms.ComboBox comboBoxDBAdapter;
        internal System.Windows.Forms.TextBox textBoxIndexFolder;
        private System.Windows.Forms.GroupBox groupBoxMode;
        private System.Windows.Forms.Label label1;
        internal System.Windows.Forms.TextBox textBoxTableName;
        private System.Windows.Forms.Panel panelFields;
        private System.Windows.Forms.Panel panelHead;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Button buttonAddField;
        internal System.Windows.Forms.TextBox textBoxDBTableName;
        private System.Windows.Forms.Label labelDBTableName;
        private System.Windows.Forms.TabPage tabPage4;
        internal System.Windows.Forms.TextBox textBoxScript;
        internal System.Windows.Forms.RadioButton radioButtonCreateTableFromExistTable;
        internal System.Windows.Forms.RadioButton radioButtonCreateNewTable;
        private System.Windows.Forms.Button buttonTestConnectionString;
        private System.Windows.Forms.GroupBox groupBoxIncrementalMode;
        internal System.Windows.Forms.RadioButton radioButtonAll;
        internal System.Windows.Forms.RadioButton radioButtonAppendOnly;
        private System.Windows.Forms.Panel panelDocIdReplaceField;
        private System.Windows.Forms.TextBox textBoxDocIdReplaceField;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button buttonFinish;
        private System.Windows.Forms.Button buttonMirrorTable;
    }
}