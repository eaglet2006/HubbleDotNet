using System;
using System.Collections.Generic;
using System.Text;

namespace Eaglet.Workroom.AspDotNetPager
{
    public class NextPageButton : PagerButton
    {
        private int _NextPageNo;

        public int NextPageNo
        {
            get
            {
                return _NextPageNo;
            }

            set
            {
                _NextPageNo = value;
            }
        }


        public NextPageButton()
        {
        }

        public override string GetUrl()
        {
            return "javascript:_doPost('" + NextPageNo.ToString() + "')";
        }
    }
}
