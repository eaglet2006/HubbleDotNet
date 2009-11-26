using System;
using System.Collections.Generic;
using System.Text;

namespace Eaglet.Workroom.AspDotNetPager
{
    public class PageNoButton : PagerButton
    {
        private int _PageNo;
        private int _CurPageNo;

        public int PageNo
        {
            get
            {
                return _PageNo;
            }

            set
            {
                _PageNo = value;
            }
        }

        public int CurPageNo
        {
            get
            {
                return _CurPageNo;
            }

            set
            {
                _CurPageNo = value;
            }
        }

        public PageNoButton()
        {
        }

        public PageNoButton(int pageNo, int curPageNo)
        {
            PageNo = pageNo;
            CurPageNo = curPageNo;
        }


        public override string GetText()
        {
            return PageNo.ToString();
        }

        public override string GetUrl()
        {
            if (PageNo == CurPageNo)
            {
                return "";
            }
            else
            {
                return "javascript:_doPost('" + PageNo + "')";
            }
        }
    }
}
