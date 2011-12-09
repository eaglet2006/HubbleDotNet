namespace TaskManage
{
    partial class FrmCreateSchema
    {
        /// <summary>
        /// 
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows  

        /// <summary>
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmCreateSchema));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSchemaName = new System.Windows.Forms.TextBox();
            this.cobSchemaType = new System.Windows.Forms.ComboBox();
            this.cbState = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dtRunOnceTime = new System.Windows.Forms.DateTimePicker();
            this.dtRunOnceDate = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.panelEveryMonth = new System.Windows.Forms.Panel();
            this.numEveryMonthRunInterval2 = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.cobDaysOfWeek = new System.Windows.Forms.ComboBox();
            this.cobWhichWeek = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.numEveryMonthRunInterval1 = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.numWhichDay = new System.Windows.Forms.NumericUpDown();
            this.rdoOptionWhichDaysOfWeek = new System.Windows.Forms.RadioButton();
            this.rdoOptionWhichDay = new System.Windows.Forms.RadioButton();
            this.panelEveryWeek = new System.Windows.Forms.Panel();
            this.cbSunday = new System.Windows.Forms.CheckBox();
            this.cbThursday = new System.Windows.Forms.CheckBox();
            this.cbTuesday = new System.Windows.Forms.CheckBox();
            this.cbSaturday = new System.Windows.Forms.CheckBox();
            this.cbFriday = new System.Windows.Forms.CheckBox();
            this.cbWednesday = new System.Windows.Forms.CheckBox();
            this.cbMonday = new System.Windows.Forms.CheckBox();
            this.label14 = new System.Windows.Forms.Label();
            this.numEveryWeekRunInterval = new System.Windows.Forms.NumericUpDown();
            this.label13 = new System.Windows.Forms.Label();
            this.panelEveryDay = new System.Windows.Forms.Panel();
            this.label16 = new System.Windows.Forms.Label();
            this.numEveryDayRunInterval = new System.Windows.Forms.NumericUpDown();
            this.label15 = new System.Windows.Forms.Label();
            this.cobFrequencyType = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.dtEndTime = new System.Windows.Forms.DateTimePicker();
            this.dtStartTime = new System.Windows.Forms.DateTimePicker();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.numRunInterval = new System.Windows.Forms.NumericUpDown();
            this.cobIntervalUnit = new System.Windows.Forms.ComboBox();
            this.dtDayFrequencyRunOnceTime = new System.Windows.Forms.DateTimePicker();
            this.rdoRunOnce = new System.Windows.Forms.RadioButton();
            this.rdoRunInterval = new System.Windows.Forms.RadioButton();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.dtEndDate = new System.Windows.Forms.DateTimePicker();
            this.rdoInfinity = new System.Windows.Forms.RadioButton();
            this.rdoEndDate = new System.Windows.Forms.RadioButton();
            this.dtStartDate = new System.Windows.Forms.DateTimePicker();
            this.label5 = new System.Windows.Forms.Label();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.txtSummary = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.textBoxUserName = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.textBoxDatabase = new System.Windows.Forms.TextBox();
            this.label20 = new System.Windows.Forms.Label();
            this.textBoxSql = new System.Windows.Forms.TextBox();
            this.label21 = new System.Windows.Forms.Label();
            this.buttonTest = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.panelEveryMonth.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numEveryMonthRunInterval2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numEveryMonthRunInterval1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWhichDay)).BeginInit();
            this.panelEveryWeek.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numEveryWeekRunInterval)).BeginInit();
            this.panelEveryDay.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numEveryDayRunInterval)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRunInterval)).BeginInit();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 120);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Type：";
            // 
            // txtSchemaName
            // 
            this.txtSchemaName.Location = new System.Drawing.Point(118, 11);
            this.txtSchemaName.Name = "txtSchemaName";
            this.txtSchemaName.Size = new System.Drawing.Size(84, 20);
            this.txtSchemaName.TabIndex = 0;
            // 
            // cobSchemaType
            // 
            this.cobSchemaType.FormattingEnabled = true;
            this.cobSchemaType.Items.AddRange(new object[] {
            "One time only",
            "Repeating"});
            this.cobSchemaType.Location = new System.Drawing.Point(116, 117);
            this.cobSchemaType.Name = "cobSchemaType";
            this.cobSchemaType.Size = new System.Drawing.Size(398, 21);
            this.cobSchemaType.TabIndex = 5;
            this.cobSchemaType.SelectedIndexChanged += new System.EventHandler(this.cobSchemaType_SelectedIndexChanged);
            this.cobSchemaType.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cobFrequencyType_KeyPress);
            // 
            // cbState
            // 
            this.cbState.AutoSize = true;
            this.cbState.Checked = true;
            this.cbState.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbState.Location = new System.Drawing.Point(537, 120);
            this.cbState.Name = "cbState";
            this.cbState.Size = new System.Drawing.Size(65, 17);
            this.cbState.TabIndex = 4;
            this.cbState.Text = "Enabled";
            this.cbState.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.dtRunOnceTime);
            this.groupBox1.Controls.Add(this.dtRunOnceDate);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(12, 144);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(706, 61);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "One time only";
            // 
            // dtRunOnceTime
            // 
            this.dtRunOnceTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtRunOnceTime.Location = new System.Drawing.Point(394, 26);
            this.dtRunOnceTime.Name = "dtRunOnceTime";
            this.dtRunOnceTime.ShowUpDown = true;
            this.dtRunOnceTime.Size = new System.Drawing.Size(80, 20);
            this.dtRunOnceTime.TabIndex = 8;
            // 
            // dtRunOnceDate
            // 
            this.dtRunOnceDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtRunOnceDate.Location = new System.Drawing.Point(143, 26);
            this.dtRunOnceDate.Name = "dtRunOnceDate";
            this.dtRunOnceDate.Size = new System.Drawing.Size(120, 20);
            this.dtRunOnceDate.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(341, 30);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(33, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Time:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(38, 30);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Date:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.panelEveryMonth);
            this.groupBox2.Controls.Add(this.panelEveryWeek);
            this.groupBox2.Controls.Add(this.panelEveryDay);
            this.groupBox2.Controls.Add(this.cobFrequencyType);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Location = new System.Drawing.Point(12, 211);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(706, 154);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Frequency";
            // 
            // panelEveryMonth
            // 
            this.panelEveryMonth.Controls.Add(this.numEveryMonthRunInterval2);
            this.panelEveryMonth.Controls.Add(this.label12);
            this.panelEveryMonth.Controls.Add(this.label11);
            this.panelEveryMonth.Controls.Add(this.cobDaysOfWeek);
            this.panelEveryMonth.Controls.Add(this.cobWhichWeek);
            this.panelEveryMonth.Controls.Add(this.label10);
            this.panelEveryMonth.Controls.Add(this.numEveryMonthRunInterval1);
            this.panelEveryMonth.Controls.Add(this.label9);
            this.panelEveryMonth.Controls.Add(this.numWhichDay);
            this.panelEveryMonth.Controls.Add(this.rdoOptionWhichDaysOfWeek);
            this.panelEveryMonth.Controls.Add(this.rdoOptionWhichDay);
            this.panelEveryMonth.Location = new System.Drawing.Point(38, 50);
            this.panelEveryMonth.Name = "panelEveryMonth";
            this.panelEveryMonth.Size = new System.Drawing.Size(554, 68);
            this.panelEveryMonth.TabIndex = 2;
            this.panelEveryMonth.Visible = false;
            // 
            // numEveryMonthRunInterval2
            // 
            this.numEveryMonthRunInterval2.Location = new System.Drawing.Point(254, 36);
            this.numEveryMonthRunInterval2.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numEveryMonthRunInterval2.Name = "numEveryMonthRunInterval2";
            this.numEveryMonthRunInterval2.Size = new System.Drawing.Size(70, 20);
            this.numEveryMonthRunInterval2.TabIndex = 15;
            this.numEveryMonthRunInterval2.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(330, 40);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(37, 13);
            this.label12.TabIndex = 10;
            this.label12.Text = "Month";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(215, 40);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(29, 13);
            this.label11.TabIndex = 8;
            this.label11.Text = "- Per";
            // 
            // cobDaysOfWeek
            // 
            this.cobDaysOfWeek.FormattingEnabled = true;
            this.cobDaysOfWeek.Items.AddRange(new object[] {
            "Sun",
            "Mon",
            "Tus",
            "Wed",
            "Thu",
            "Fri",
            "Sat",
            "Day"});
            this.cobDaysOfWeek.Location = new System.Drawing.Point(139, 37);
            this.cobDaysOfWeek.Name = "cobDaysOfWeek";
            this.cobDaysOfWeek.Size = new System.Drawing.Size(70, 21);
            this.cobDaysOfWeek.TabIndex = 14;
            this.cobDaysOfWeek.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cobFrequencyType_KeyPress);
            // 
            // cobWhichWeek
            // 
            this.cobWhichWeek.FormattingEnabled = true;
            this.cobWhichWeek.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "Last"});
            this.cobWhichWeek.Location = new System.Drawing.Point(63, 37);
            this.cobWhichWeek.Name = "cobWhichWeek";
            this.cobWhichWeek.Size = new System.Drawing.Size(70, 21);
            this.cobWhichWeek.TabIndex = 13;
            this.cobWhichWeek.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cobFrequencyType_KeyPress);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(272, 14);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(37, 13);
            this.label10.TabIndex = 5;
            this.label10.Text = "Month";
            // 
            // numEveryMonthRunInterval1
            // 
            this.numEveryMonthRunInterval1.Location = new System.Drawing.Point(193, 9);
            this.numEveryMonthRunInterval1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numEveryMonthRunInterval1.Name = "numEveryMonthRunInterval1";
            this.numEveryMonthRunInterval1.Size = new System.Drawing.Size(70, 20);
            this.numEveryMonthRunInterval1.TabIndex = 12;
            this.numEveryMonthRunInterval1.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(141, 14);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(45, 13);
            this.label9.TabIndex = 3;
            this.label9.Text = "Day-Per";
            // 
            // numWhichDay
            // 
            this.numWhichDay.Location = new System.Drawing.Point(63, 9);
            this.numWhichDay.Maximum = new decimal(new int[] {
            31,
            0,
            0,
            0});
            this.numWhichDay.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numWhichDay.Name = "numWhichDay";
            this.numWhichDay.Size = new System.Drawing.Size(70, 20);
            this.numWhichDay.TabIndex = 11;
            this.numWhichDay.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // rdoOptionWhichDaysOfWeek
            // 
            this.rdoOptionWhichDaysOfWeek.AutoSize = true;
            this.rdoOptionWhichDaysOfWeek.Location = new System.Drawing.Point(3, 38);
            this.rdoOptionWhichDaysOfWeek.Name = "rdoOptionWhichDaysOfWeek";
            this.rdoOptionWhichDaysOfWeek.Size = new System.Drawing.Size(34, 17);
            this.rdoOptionWhichDaysOfWeek.TabIndex = 1;
            this.rdoOptionWhichDaysOfWeek.Text = "at";
            this.rdoOptionWhichDaysOfWeek.UseVisualStyleBackColor = true;
            this.rdoOptionWhichDaysOfWeek.CheckedChanged += new System.EventHandler(this.rdoOptionWhichDaysOfWeek_CheckedChanged);
            // 
            // rdoOptionWhichDay
            // 
            this.rdoOptionWhichDay.AutoSize = true;
            this.rdoOptionWhichDay.Checked = true;
            this.rdoOptionWhichDay.Location = new System.Drawing.Point(3, 14);
            this.rdoOptionWhichDay.Name = "rdoOptionWhichDay";
            this.rdoOptionWhichDay.Size = new System.Drawing.Size(45, 17);
            this.rdoOptionWhichDay.TabIndex = 0;
            this.rdoOptionWhichDay.TabStop = true;
            this.rdoOptionWhichDay.Text = "from";
            this.rdoOptionWhichDay.UseVisualStyleBackColor = true;
            this.rdoOptionWhichDay.CheckedChanged += new System.EventHandler(this.rdoOptionWhichDay_CheckedChanged);
            // 
            // panelEveryWeek
            // 
            this.panelEveryWeek.Controls.Add(this.cbSunday);
            this.panelEveryWeek.Controls.Add(this.cbThursday);
            this.panelEveryWeek.Controls.Add(this.cbTuesday);
            this.panelEveryWeek.Controls.Add(this.cbSaturday);
            this.panelEveryWeek.Controls.Add(this.cbFriday);
            this.panelEveryWeek.Controls.Add(this.cbWednesday);
            this.panelEveryWeek.Controls.Add(this.cbMonday);
            this.panelEveryWeek.Controls.Add(this.label14);
            this.panelEveryWeek.Controls.Add(this.numEveryWeekRunInterval);
            this.panelEveryWeek.Controls.Add(this.label13);
            this.panelEveryWeek.Location = new System.Drawing.Point(38, 50);
            this.panelEveryWeek.Name = "panelEveryWeek";
            this.panelEveryWeek.Size = new System.Drawing.Size(586, 99);
            this.panelEveryWeek.TabIndex = 11;
            this.panelEveryWeek.Visible = false;
            // 
            // cbSunday
            // 
            this.cbSunday.AutoSize = true;
            this.cbSunday.Location = new System.Drawing.Point(482, 70);
            this.cbSunday.Name = "cbSunday";
            this.cbSunday.Size = new System.Drawing.Size(45, 17);
            this.cbSunday.TabIndex = 18;
            this.cbSunday.Text = "Sun";
            this.cbSunday.UseVisualStyleBackColor = true;
            // 
            // cbThursday
            // 
            this.cbThursday.AutoSize = true;
            this.cbThursday.Location = new System.Drawing.Point(241, 70);
            this.cbThursday.Name = "cbThursday";
            this.cbThursday.Size = new System.Drawing.Size(45, 17);
            this.cbThursday.TabIndex = 17;
            this.cbThursday.Text = "Thu";
            this.cbThursday.UseVisualStyleBackColor = true;
            // 
            // cbTuesday
            // 
            this.cbTuesday.AutoSize = true;
            this.cbTuesday.Location = new System.Drawing.Point(105, 70);
            this.cbTuesday.Name = "cbTuesday";
            this.cbTuesday.Size = new System.Drawing.Size(45, 17);
            this.cbTuesday.TabIndex = 16;
            this.cbTuesday.Text = "Tue";
            this.cbTuesday.UseVisualStyleBackColor = true;
            // 
            // cbSaturday
            // 
            this.cbSaturday.AutoSize = true;
            this.cbSaturday.Location = new System.Drawing.Point(482, 47);
            this.cbSaturday.Name = "cbSaturday";
            this.cbSaturday.Size = new System.Drawing.Size(42, 17);
            this.cbSaturday.TabIndex = 6;
            this.cbSaturday.Text = "Sat";
            this.cbSaturday.UseVisualStyleBackColor = true;
            // 
            // cbFriday
            // 
            this.cbFriday.AutoSize = true;
            this.cbFriday.Location = new System.Drawing.Point(366, 47);
            this.cbFriday.Name = "cbFriday";
            this.cbFriday.Size = new System.Drawing.Size(37, 17);
            this.cbFriday.TabIndex = 5;
            this.cbFriday.Text = "Fri";
            this.cbFriday.UseVisualStyleBackColor = true;
            // 
            // cbWednesday
            // 
            this.cbWednesday.AutoSize = true;
            this.cbWednesday.Location = new System.Drawing.Point(241, 47);
            this.cbWednesday.Name = "cbWednesday";
            this.cbWednesday.Size = new System.Drawing.Size(49, 17);
            this.cbWednesday.TabIndex = 4;
            this.cbWednesday.Text = "Wed";
            this.cbWednesday.UseVisualStyleBackColor = true;
            // 
            // cbMonday
            // 
            this.cbMonday.AutoSize = true;
            this.cbMonday.Location = new System.Drawing.Point(105, 47);
            this.cbMonday.Name = "cbMonday";
            this.cbMonday.Size = new System.Drawing.Size(47, 17);
            this.cbMonday.TabIndex = 3;
            this.cbMonday.Text = "Mon";
            this.cbMonday.UseVisualStyleBackColor = true;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(184, 14);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(48, 13);
            this.label14.TabIndex = 2;
            this.label14.Text = "Week,at";
            // 
            // numEveryWeekRunInterval
            // 
            this.numEveryWeekRunInterval.Location = new System.Drawing.Point(105, 9);
            this.numEveryWeekRunInterval.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numEveryWeekRunInterval.Name = "numEveryWeekRunInterval";
            this.numEveryWeekRunInterval.Size = new System.Drawing.Size(60, 20);
            this.numEveryWeekRunInterval.TabIndex = 1;
            this.numEveryWeekRunInterval.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(0, 16);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(45, 13);
            this.label13.TabIndex = 0;
            this.label13.Text = "Interval:";
            // 
            // panelEveryDay
            // 
            this.panelEveryDay.Controls.Add(this.label16);
            this.panelEveryDay.Controls.Add(this.numEveryDayRunInterval);
            this.panelEveryDay.Controls.Add(this.label15);
            this.panelEveryDay.Location = new System.Drawing.Point(38, 50);
            this.panelEveryDay.Name = "panelEveryDay";
            this.panelEveryDay.Size = new System.Drawing.Size(266, 47);
            this.panelEveryDay.TabIndex = 10;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(165, 17);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(26, 13);
            this.label16.TabIndex = 2;
            this.label16.Text = "Day";
            // 
            // numEveryDayRunInterval
            // 
            this.numEveryDayRunInterval.Location = new System.Drawing.Point(102, 13);
            this.numEveryDayRunInterval.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numEveryDayRunInterval.Name = "numEveryDayRunInterval";
            this.numEveryDayRunInterval.Size = new System.Drawing.Size(50, 20);
            this.numEveryDayRunInterval.TabIndex = 1;
            this.numEveryDayRunInterval.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(-2, 16);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(86, 13);
            this.label15.TabIndex = 0;
            this.label15.Text = "Execute interval:";
            // 
            // cobFrequencyType
            // 
            this.cobFrequencyType.FormattingEnabled = true;
            this.cobFrequencyType.Items.AddRange(new object[] {
            "Daily",
            "Weekly",
            "Monthly"});
            this.cobFrequencyType.Location = new System.Drawing.Point(142, 22);
            this.cobFrequencyType.Name = "cobFrequencyType";
            this.cobFrequencyType.Size = new System.Drawing.Size(120, 21);
            this.cobFrequencyType.TabIndex = 9;
            this.cobFrequencyType.SelectedIndexChanged += new System.EventHandler(this.cobFrequencyType_SelectedIndexChanged);
            this.cobFrequencyType.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cobFrequencyType_KeyPress);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(38, 24);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(49, 13);
            this.label8.TabIndex = 0;
            this.label8.Text = "Execute:";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.dtEndTime);
            this.groupBox3.Controls.Add(this.dtStartTime);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.numRunInterval);
            this.groupBox3.Controls.Add(this.cobIntervalUnit);
            this.groupBox3.Controls.Add(this.dtDayFrequencyRunOnceTime);
            this.groupBox3.Controls.Add(this.rdoRunOnce);
            this.groupBox3.Controls.Add(this.rdoRunInterval);
            this.groupBox3.Location = new System.Drawing.Point(12, 371);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(706, 77);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Frequency daily";
            // 
            // dtEndTime
            // 
            this.dtEndTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtEndTime.Location = new System.Drawing.Point(584, 50);
            this.dtEndTime.Name = "dtEndTime";
            this.dtEndTime.ShowUpDown = true;
            this.dtEndTime.Size = new System.Drawing.Size(80, 20);
            this.dtEndTime.TabIndex = 23;
            this.dtEndTime.Value = new System.DateTime(2006, 12, 19, 23, 59, 59, 0);
            // 
            // dtStartTime
            // 
            this.dtStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtStartTime.Location = new System.Drawing.Point(420, 50);
            this.dtStartTime.Name = "dtStartTime";
            this.dtStartTime.ShowUpDown = true;
            this.dtStartTime.Size = new System.Drawing.Size(80, 20);
            this.dtStartTime.TabIndex = 22;
            this.dtStartTime.Value = new System.DateTime(2006, 12, 19, 0, 0, 0, 0);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(549, 54);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(29, 13);
            this.label7.TabIndex = 6;
            this.label7.Text = "End:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(382, 54);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(32, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "Start:";
            // 
            // numRunInterval
            // 
            this.numRunInterval.Location = new System.Drawing.Point(143, 49);
            this.numRunInterval.Maximum = new decimal(new int[] {
            23,
            0,
            0,
            0});
            this.numRunInterval.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numRunInterval.Name = "numRunInterval";
            this.numRunInterval.Size = new System.Drawing.Size(80, 20);
            this.numRunInterval.TabIndex = 20;
            this.numRunInterval.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // cobIntervalUnit
            // 
            this.cobIntervalUnit.FormattingEnabled = true;
            this.cobIntervalUnit.Items.AddRange(new object[] {
            "Hour",
            "Minute"});
            this.cobIntervalUnit.Location = new System.Drawing.Point(231, 50);
            this.cobIntervalUnit.Name = "cobIntervalUnit";
            this.cobIntervalUnit.Size = new System.Drawing.Size(80, 21);
            this.cobIntervalUnit.TabIndex = 21;
            this.cobIntervalUnit.SelectedIndexChanged += new System.EventHandler(this.cobIntervalUnit_SelectedIndexChanged);
            this.cobIntervalUnit.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cobFrequencyType_KeyPress);
            // 
            // dtDayFrequencyRunOnceTime
            // 
            this.dtDayFrequencyRunOnceTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtDayFrequencyRunOnceTime.Location = new System.Drawing.Point(143, 20);
            this.dtDayFrequencyRunOnceTime.Name = "dtDayFrequencyRunOnceTime";
            this.dtDayFrequencyRunOnceTime.ShowUpDown = true;
            this.dtDayFrequencyRunOnceTime.Size = new System.Drawing.Size(80, 20);
            this.dtDayFrequencyRunOnceTime.TabIndex = 19;
            // 
            // rdoRunOnce
            // 
            this.rdoRunOnce.AutoSize = true;
            this.rdoRunOnce.Checked = true;
            this.rdoRunOnce.Location = new System.Drawing.Point(38, 22);
            this.rdoRunOnce.Name = "rdoRunOnce";
            this.rdoRunOnce.Size = new System.Drawing.Size(79, 17);
            this.rdoRunOnce.TabIndex = 19;
            this.rdoRunOnce.TabStop = true;
            this.rdoRunOnce.Text = "Execute at:";
            this.rdoRunOnce.UseVisualStyleBackColor = true;
            this.rdoRunOnce.CheckedChanged += new System.EventHandler(this.rdoRunOnce_CheckedChanged);
            // 
            // rdoRunInterval
            // 
            this.rdoRunInterval.AutoSize = true;
            this.rdoRunInterval.Location = new System.Drawing.Point(38, 52);
            this.rdoRunInterval.Name = "rdoRunInterval";
            this.rdoRunInterval.Size = new System.Drawing.Size(101, 17);
            this.rdoRunInterval.TabIndex = 20;
            this.rdoRunInterval.Text = "Execute interval";
            this.rdoRunInterval.UseVisualStyleBackColor = true;
            this.rdoRunInterval.CheckedChanged += new System.EventHandler(this.rdoRunInterval_CheckedChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.dtEndDate);
            this.groupBox4.Controls.Add(this.rdoInfinity);
            this.groupBox4.Controls.Add(this.rdoEndDate);
            this.groupBox4.Controls.Add(this.dtStartDate);
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Location = new System.Drawing.Point(12, 454);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(708, 89);
            this.groupBox4.TabIndex = 8;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Duration";
            // 
            // dtEndDate
            // 
            this.dtEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtEndDate.Location = new System.Drawing.Point(482, 25);
            this.dtEndDate.Name = "dtEndDate";
            this.dtEndDate.Size = new System.Drawing.Size(120, 20);
            this.dtEndDate.TabIndex = 25;
            // 
            // rdoInfinity
            // 
            this.rdoInfinity.AutoSize = true;
            this.rdoInfinity.Checked = true;
            this.rdoInfinity.Location = new System.Drawing.Point(386, 61);
            this.rdoInfinity.Name = "rdoInfinity";
            this.rdoInfinity.Size = new System.Drawing.Size(55, 17);
            this.rdoInfinity.TabIndex = 26;
            this.rdoInfinity.TabStop = true;
            this.rdoInfinity.Text = "Infinity";
            this.rdoInfinity.UseVisualStyleBackColor = true;
            // 
            // rdoEndDate
            // 
            this.rdoEndDate.AutoSize = true;
            this.rdoEndDate.Location = new System.Drawing.Point(386, 27);
            this.rdoEndDate.Name = "rdoEndDate";
            this.rdoEndDate.Size = new System.Drawing.Size(73, 17);
            this.rdoEndDate.TabIndex = 25;
            this.rdoEndDate.Text = "End Date:";
            this.rdoEndDate.UseVisualStyleBackColor = true;
            this.rdoEndDate.CheckedChanged += new System.EventHandler(this.rdoEndDate_CheckedChanged);
            // 
            // dtStartDate
            // 
            this.dtStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtStartDate.Location = new System.Drawing.Point(144, 25);
            this.dtStartDate.Name = "dtStartDate";
            this.dtStartDate.Size = new System.Drawing.Size(120, 20);
            this.dtStartDate.TabIndex = 24;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(38, 29);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Start date:";
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(660, 14);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(75, 25);
            this.btnConfirm.TabIndex = 9;
            this.btnConfirm.Text = "Save";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(660, 87);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 25);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.txtSummary);
            this.groupBox5.Controls.Add(this.label17);
            this.groupBox5.Location = new System.Drawing.Point(12, 549);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(708, 83);
            this.groupBox5.TabIndex = 11;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Abstract";
            // 
            // txtSummary
            // 
            this.txtSummary.Location = new System.Drawing.Point(145, 18);
            this.txtSummary.Multiline = true;
            this.txtSummary.Name = "txtSummary";
            this.txtSummary.Size = new System.Drawing.Size(550, 58);
            this.txtSummary.TabIndex = 27;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(41, 18);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(60, 13);
            this.label17.TabIndex = 0;
            this.label17.Text = "Description";
            // 
            // textBoxUserName
            // 
            this.textBoxUserName.Location = new System.Drawing.Point(118, 35);
            this.textBoxUserName.Name = "textBoxUserName";
            this.textBoxUserName.Size = new System.Drawing.Size(84, 20);
            this.textBoxUserName.TabIndex = 1;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(14, 37);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(60, 13);
            this.label18.TabIndex = 12;
            this.label18.Text = "UserName:";
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new System.Drawing.Point(118, 61);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(84, 20);
            this.textBoxPassword.TabIndex = 2;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(14, 63);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(56, 13);
            this.label19.TabIndex = 14;
            this.label19.Text = "Password:";
            // 
            // textBoxDatabase
            // 
            this.textBoxDatabase.Location = new System.Drawing.Point(118, 87);
            this.textBoxDatabase.Name = "textBoxDatabase";
            this.textBoxDatabase.Size = new System.Drawing.Size(84, 20);
            this.textBoxDatabase.TabIndex = 3;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(14, 89);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(56, 13);
            this.label20.TabIndex = 16;
            this.label20.Text = "Database:";
            // 
            // textBoxSql
            // 
            this.textBoxSql.Location = new System.Drawing.Point(217, 30);
            this.textBoxSql.Multiline = true;
            this.textBoxSql.Name = "textBoxSql";
            this.textBoxSql.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxSql.Size = new System.Drawing.Size(380, 77);
            this.textBoxSql.TabIndex = 4;
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(214, 11);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(78, 13);
            this.label21.TabIndex = 19;
            this.label21.Text = "Sql to execute:";
            // 
            // buttonTest
            // 
            this.buttonTest.Location = new System.Drawing.Point(660, 53);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(75, 23);
            this.buttonTest.TabIndex = 20;
            this.buttonTest.Text = "Test";
            this.buttonTest.UseVisualStyleBackColor = true;
            this.buttonTest.Click += new System.EventHandler(this.buttonTest_Click);
            // 
            // FrmCreateSchema
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(752, 640);
            this.Controls.Add(this.buttonTest);
            this.Controls.Add(this.label21);
            this.Controls.Add(this.textBoxSql);
            this.Controls.Add(this.textBoxDatabase);
            this.Controls.Add(this.label20);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.textBoxUserName);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cbState);
            this.Controls.Add(this.cobSchemaType);
            this.Controls.Add(this.txtSchemaName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FrmCreateSchema";
            this.Text = "New schema";
            this.Load += new System.EventHandler(this.FrmCreateSchema_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.panelEveryMonth.ResumeLayout(false);
            this.panelEveryMonth.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numEveryMonthRunInterval2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numEveryMonthRunInterval1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWhichDay)).EndInit();
            this.panelEveryWeek.ResumeLayout(false);
            this.panelEveryWeek.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numEveryWeekRunInterval)).EndInit();
            this.panelEveryDay.ResumeLayout(false);
            this.panelEveryDay.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numEveryDayRunInterval)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRunInterval)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSchemaName;
        private System.Windows.Forms.ComboBox cobSchemaType;
        private System.Windows.Forms.CheckBox cbState;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DateTimePicker dtRunOnceDate;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.RadioButton rdoRunOnce;
        private System.Windows.Forms.RadioButton rdoRunInterval;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DateTimePicker dtEndDate;
        private System.Windows.Forms.RadioButton rdoInfinity;
        private System.Windows.Forms.RadioButton rdoEndDate;
        private System.Windows.Forms.DateTimePicker dtStartDate;
        private System.Windows.Forms.DateTimePicker dtDayFrequencyRunOnceTime;
        private System.Windows.Forms.DateTimePicker dtEndTime;
        private System.Windows.Forms.DateTimePicker dtStartTime;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numRunInterval;
        private System.Windows.Forms.ComboBox cobIntervalUnit;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Panel panelEveryMonth;
        private System.Windows.Forms.RadioButton rdoOptionWhichDaysOfWeek;
        private System.Windows.Forms.RadioButton rdoOptionWhichDay;
        private System.Windows.Forms.ComboBox cobFrequencyType;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox cobDaysOfWeek;
        private System.Windows.Forms.ComboBox cobWhichWeek;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown numEveryMonthRunInterval1;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown numWhichDay;
        private System.Windows.Forms.Panel panelEveryWeek;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.CheckBox cbWednesday;
        private System.Windows.Forms.CheckBox cbMonday;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.NumericUpDown numEveryWeekRunInterval;
        private System.Windows.Forms.CheckBox cbSunday;
        private System.Windows.Forms.CheckBox cbThursday;
        private System.Windows.Forms.CheckBox cbTuesday;
        private System.Windows.Forms.CheckBox cbSaturday;
        private System.Windows.Forms.CheckBox cbFriday;
        private System.Windows.Forms.Panel panelEveryDay;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.NumericUpDown numEveryDayRunInterval;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.NumericUpDown numEveryMonthRunInterval2;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.DateTimePicker dtRunOnceTime;
        private System.Windows.Forms.TextBox txtSummary;
        private System.Windows.Forms.TextBox textBoxUserName;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox textBoxDatabase;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.TextBox textBoxSql;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Button buttonTest;
    }
}