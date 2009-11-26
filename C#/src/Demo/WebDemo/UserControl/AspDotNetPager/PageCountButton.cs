using System;
using System.Collections.Generic;
using System.Text;

namespace Eaglet.Workroom.AspDotNetPager
{
    public class PageCountButton : PagerButton
    {
        private int _PageCount;

        public int PageCount
        {
            get
            {
                return _PageCount;
            }

            set
            {
                _PageCount = value;
            }
        }

        public PageCountButton()
        {
            Text = "Page Count:";
        }

        public override string GetText()
        {
            return Text + PageCount.ToString();
        }
    }
}
