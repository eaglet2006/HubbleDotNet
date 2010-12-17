using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TaskManage
{
    public partial class FrmCreateSchema : Form
    {
        private int _SchemaId = -1;

        public int SchemaId
        {
            get
            {
                return _SchemaId;
            }
            set
            {
                _SchemaId = value;
            }
        }

        public FrmCreateSchema()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if(txtSchemaName.Text.Trim() == "")
            {
                MessageBox.Show("Name can't be empty!");
                txtSchemaName.Focus();
                return;
            }

            if (textBoxDatabase.Text.Trim() == "")
            {
                MessageBox.Show("Databaase can't be empty!");
                txtSchemaName.Focus();
                return;
            }

            if (textBoxSql.Text.Trim() == "")
            {
                MessageBox.Show("Sql can't be empty!");
                txtSchemaName.Focus();
                return;
            }

            Schema schema = new Schema();
            schema.SchemaId = SchemaId;
            schema.Name = txtSchemaName.Text.Trim();
            schema.UserName = textBoxUserName.Text.Trim();
            schema.Password = Hubble.Framework.Security.DesEncryption.Encrypt(new byte[]{ 0x14, 0x0A, 0x0C, 0x0E, 0x0A, 0x11, 0x42, 0x58 }, textBoxPassword.Text.Trim());
            schema.Database = textBoxDatabase.Text.Trim();
            schema.Sql = textBoxSql.Text.Trim();
            schema.Type = (cobSchemaType.SelectedIndex == 0) ? SchemaType.RunOnce : SchemaType.RunRepeat;
            schema.State = (cbState.Checked == true) ? SchemaState.Enable : SchemaState.Disable;
            

            //Content
            SchemaInfo schemaInfo = new SchemaInfo();

            //Frequency
            Frequency frequency = new Frequency();
            EveryDay everyDay = new EveryDay();
            EveryWeek everyWeek = new EveryWeek();
            EveryMonth everyMonth = new EveryMonth();

            //Daily
            DayFrequency dayFrequency = new DayFrequency();

            //Available time
            AvailableTime runTime = new AvailableTime();

            if (schema.Type == SchemaType.RunOnce)
            {
                schemaInfo.RunOnceTime = Convert.ToDateTime(dtRunOnceDate.Text + " " + dtRunOnceTime.Text);
            }
            else
            {
                FrequencyType frequencyType;
                switch(cobFrequencyType.SelectedIndex)
                {
                    case 0 :
                        frequencyType = FrequencyType.Day;
                        everyDay.RunInterval = (int)(numEveryDayRunInterval.Value);
                        break;
                    case 1:
                        frequencyType = FrequencyType.Week;
                        everyWeek.RunInterval = (int)(numEveryWeekRunInterval.Value);
                        DayOfWeek[] dayOfWeek = new DayOfWeek[7];
                        int i = 0;
                        if(cbMonday.Checked == true)
                            dayOfWeek[i++] = DayOfWeek.Monday;
                        if(cbTuesday.Checked == true)
                            dayOfWeek[i++] = DayOfWeek.Tuesday;
                        if (cbWednesday.Checked == true)
                            dayOfWeek[i++] = DayOfWeek.Wednesday;
                        if(cbThursday.Checked == true)
                            dayOfWeek[i++] = DayOfWeek.Thursday;
                        if(cbFriday.Checked == true)
                            dayOfWeek[i++] = DayOfWeek.Friday; 
                        if(cbSaturday.Checked == true)
                            dayOfWeek[i++] = DayOfWeek.Saturday;
                        if(cbSunday.Checked == true)
                            dayOfWeek[i++] = DayOfWeek.Sunday;
                        //
                        everyWeek.DaysOfWeek = dayOfWeek;
                        break;
                    case 2:
                        frequencyType = FrequencyType.Month;
                        if(rdoOptionWhichDay.Checked == true)
                        {
                            everyMonth.Option = 1;
                            everyMonth.RunInterval = (int)(numEveryMonthRunInterval1.Value);
                            everyMonth.WhichDay = (int)(numWhichDay.Value);
                        }
                        else
                        {
                            everyMonth.Option = 2;
                            everyMonth.RunInterval = (int)(numEveryMonthRunInterval2.Value);
                            switch (cobWhichWeek.SelectedIndex)
                            {
                                case 0:
                                case 1:
                                case 2:
                                case 3:
                                    everyMonth.WhichWeek = cobWhichWeek.SelectedIndex + 1;
                                    break;
                                case 4:
                                    everyMonth.WhichWeek = 9;
                                    break;
                            }
                            
                            switch(cobDaysOfWeek.SelectedIndex)
                            {
                                case 0:
                                case 1:
                                case 2:
                                case 3:
                                case 4:
                                case 5:
                                case 6:
                                    everyMonth.DayOfWeek = (DaysOfWeek)(cobDaysOfWeek.SelectedIndex);
                                    break;
                                case 7:
                                    everyMonth.DayOfWeek  = DaysOfWeek.Day;
                                    break;
                            }
                        }
                        break;
                    default:
                        frequencyType = FrequencyType.Day;
                        everyDay.RunInterval = (int)(numEveryDayRunInterval.Value);
                        break;
                }

                frequency.FrequencyType = frequencyType;
                frequency.EveryDay = everyDay;
                frequency.EveryWeek = everyWeek;
                frequency.EveryMonth = everyMonth;

                //Daily
                if (rdoRunOnce.Checked == true)//One time only
                {
                    string[] timeString = dtDayFrequencyRunOnceTime.Text.Split(':');
                    int hour = int.Parse(timeString[0]);
                    int minute = int.Parse(timeString[1]);
                    int second = int.Parse(timeString[2]);
                    dayFrequency.Option = 1;
                    dayFrequency.RunOnceTime = new TimeSpan(hour, minute, second);
                }
                else
                {
                    dayFrequency.Option = 2;
                    dayFrequency.RunInterval = (int)(numRunInterval.Value);
                    if (cobIntervalUnit.SelectedIndex == 0)//hour
                    {
                        dayFrequency.TimeUnit = TimeUnit.Hour;
                    }
                    else//minute
                    {
                        dayFrequency.TimeUnit = TimeUnit.Minute;
                    }

                    string[] startTime = dtStartTime.Text.Split(':');
                    int hour = int.Parse(startTime[0]);
                    int minute = int.Parse(startTime[1]);
                    int second = int.Parse(startTime[2]);
                    dayFrequency.StartTime = new TimeSpan(hour, minute, second);

                    string[] endTime = dtEndTime.Text.Split(':');
                    hour = int.Parse(endTime[0]);
                    minute = int.Parse(endTime[1]);
                    second = int.Parse(endTime[2]);
                    dayFrequency.EndTime = new TimeSpan(hour, minute, second);
                }
                //Duration

                runTime.IsInfinity = rdoInfinity.Checked;
                runTime.StartDate = Convert.ToDateTime(dtStartDate.Text);
                runTime.EndDate = Convert.ToDateTime(dtEndDate.Text);

                schemaInfo.RunTime = runTime;
                schemaInfo.DayFrequency = dayFrequency;
                schemaInfo.Frequency = frequency;
            }

            schema.SchemaInfo = schemaInfo;



            Close();
        }

        private void FrmCreateSchema_Load(object sender, EventArgs e)
        {
            txtSchemaName.Focus();
            cobSchemaType.SelectedIndex = 1;//Cycle
            cbState.Checked = true;

            groupBox1.Enabled = false;
            groupBox2.Enabled = true;
            groupBox3.Enabled = true;
            groupBox4.Enabled = true;

            cobFrequencyType.SelectedIndex = 1;//Weekly
            panelEveryDay.Visible = false;
            panelEveryWeek.Visible = true;
            panelEveryMonth.Visible = false;
            
            //Weekly
            cbSunday.Checked = true;

            //Monthly
            cobWhichWeek.SelectedIndex = 0;
            cobDaysOfWeek.SelectedIndex = 0;
            cobWhichWeek.Enabled = false;
            cobDaysOfWeek.Enabled = false;
            numEveryMonthRunInterval2.Enabled = false;

            //Daily
            cobIntervalUnit.SelectedIndex = 0;
            numRunInterval.Enabled = false;
            cobIntervalUnit.Enabled = false;
            dtStartTime.Enabled = false;
            dtEndTime.Enabled = false;

            //Duation
            dtEndDate.Enabled = false;

        }

        private void cobSchemaType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cobSchemaType.SelectedIndex == 0)//One time only
            {
                groupBox1.Enabled = true;
                groupBox2.Enabled = false;
                groupBox3.Enabled = false;
                groupBox4.Enabled = false;
            }
            if(cobSchemaType.SelectedIndex == 1)//Repeating
            {
                groupBox1.Enabled = false;
                groupBox2.Enabled = true;
                groupBox3.Enabled = true;
                groupBox4.Enabled = true;

                cobFrequencyType.SelectedIndex = 0;//Daily
                panelEveryDay.Visible = true;
                panelEveryMonth.Visible = false;
                panelEveryWeek.Visible = false;
            }
        }

        private void cobFrequencyType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cobFrequencyType.SelectedIndex == 0)//Daily
            {
                panelEveryDay.Visible = true;
                panelEveryMonth.Visible = false;
                panelEveryWeek.Visible = false;
            }
            if (cobFrequencyType.SelectedIndex == 1)//Weekly
            {
                panelEveryDay.Visible = false;
                panelEveryMonth.Visible = false;
                panelEveryWeek.Visible = true;
                cbSunday.Checked = true;
            }
            if (cobFrequencyType.SelectedIndex == 2)//Monthly
            {
                panelEveryDay.Visible = false;
                panelEveryMonth.Visible = true;
                panelEveryWeek.Visible = false;
            }
        }

        private void rdoRunOnce_CheckedChanged(object sender, EventArgs e)//One time only daily
        {
            if (rdoRunOnce.Checked == true)
            {
                dtDayFrequencyRunOnceTime.Enabled = true;
            }
            else
            {
                dtDayFrequencyRunOnceTime.Enabled = false;
            }
        }

        private void rdoOptionWhichDay_CheckedChanged(object sender, EventArgs e)//Monthly
        {
            if (rdoOptionWhichDay.Checked == true)
            {
                numWhichDay.Enabled = true;
                numEveryMonthRunInterval1.Enabled = true;
            }
            else
            {
                numWhichDay.Enabled = false;
                numEveryMonthRunInterval1.Enabled = false;
            }
        }

        private void rdoOptionWhichDaysOfWeek_CheckedChanged(object sender, EventArgs e)//Monthly
        {
            if (rdoOptionWhichDaysOfWeek.Checked == true)
            {
                cobWhichWeek.Enabled = true;
                cobDaysOfWeek.Enabled = true;
                numEveryMonthRunInterval2.Enabled = true;
            }
            else
            {
                cobWhichWeek.Enabled = false;
                cobDaysOfWeek.Enabled = false;
                numEveryMonthRunInterval2.Enabled = false;
            }
        }

        private void rdoRunInterval_CheckedChanged(object sender, EventArgs e)//Daily 
        {
            if(rdoRunInterval.Checked == true)
            {
                numRunInterval.Enabled = true;
                cobIntervalUnit.Enabled = true;
                dtStartTime.Enabled = true;
                dtEndTime.Enabled = true;
            }
            else
            {
                numRunInterval.Enabled = false;
                cobIntervalUnit.Enabled = false;
                dtStartTime.Enabled = false;
                dtEndTime.Enabled = false;
            }
        }

        private void cobIntervalUnit_SelectedIndexChanged(object sender, EventArgs e)//Unit
        {
            if(cobIntervalUnit.SelectedIndex == 0)//Hour
            {
                numRunInterval.Maximum = 23;
            }

            if (cobIntervalUnit.SelectedIndex == 1)//Minute
            {
                numRunInterval.Maximum = 59;
            }
        }

        private void rdoEndDate_CheckedChanged(object sender, EventArgs e)//
        {
            if (rdoEndDate.Checked == true)
            {
                dtEndDate.Enabled = true;
            }
            else
            {
                dtEndDate.Enabled = false;
            }
        }
    }
}