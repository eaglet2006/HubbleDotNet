using System;
using System.Collections.Generic;
using System.Text;

namespace Eaglet.Workroom.AspDotNetPager
{
    public class PrevPageButton : PagerButton
    {
        private int _PrevPageNo;

        public int PrevPageNo
        {
            get
            {
                return _PrevPageNo;
            }

            set
            {
                _PrevPageNo = value;
            }
        }

        public PrevPageButton()
        {
            Text = "Prev";
        }

        public override string GetUrl()
        {
            return "javascript:_doPost('" + PrevPageNo.ToString() + "')";
        }
    }
}
