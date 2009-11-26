using System;
using System.Collections.Generic;
using System.Text;

namespace Eaglet.Workroom.AspDotNetPager
{
    public class RecordCountButton : PagerButton
    {
        private int _RecordCount;

        public int RecordCount
        {
            get
            {
                return _RecordCount;
            }

            set
            {
                _RecordCount = value;
            }
        }

        public RecordCountButton()
        {
            Text = "Record Count:";
        }

        public override string GetText()
        {
            return Text + RecordCount.ToString();
        }
    }
}
