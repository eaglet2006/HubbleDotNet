////////////////////////////////////////////////////////////////////
//以晁方的Pager.cs为原型修改
//Writer: Bo Xiao
//1-Oct 2008
///////////////////////////////////////////////////////////////////


using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.ComponentModel;
using System.Text;

namespace Eaglet.Workroom.AspDotNetPager
{
    [DefaultProperty("RecordCount")]
    [ToolboxData("<{0}:AspNetPager runat=server></{0}:AspNetPager>")]
    public class AspNetPager : WebControl, IPostBackEventHandler
    {

        #region Private fields

        private string _ActionForm = "form1";
        private int _MinDisplayPageNoButtons = 5;
        private int _MaxDisplayPageNoButtons = 10;

        private RecordCountButton _RecordCountButton = new RecordCountButton();
        private PageCountButton _PageCountButton = new PageCountButton();
        private PrevPageButton _PrevPageButton = new PrevPageButton();
        private NextPageButton _NextPageButton = new NextPageButton();
        private PageNoButton _PageNoButton = new PageNoButton();
        private PageNoButton _CurPageNoButton = new PageNoButton();

        #endregion

        #region EventArgs

        [Serializable]
        public class PageArgs : EventArgs
        {
            public PageArgs()
            {
            }

            private int _curPage;
            public int CurPage
            {
                get
                {
                    return _curPage;
                }
                set
                {
                    _curPage = value;
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Occur when page changed 
        /// </summary>
        public event EventHandler<PageArgs> PageChanged;  

        protected virtual void OnPageChange(Object sender, PageArgs e)
        {
            if (PageChanged != null)
            {
                PageChanged(this, e);
            }
        }

        public void RaisePostBackEvent(string eventArgument)
        {
            this.CurPage = Int32.Parse(System.Web.HttpContext.Current.Request.Form[this.UniqueID].ToString());
            PageArgs e = new PageArgs();
            e.CurPage = CurPage;
            OnPageChange(this, e);
        }
        #endregion Events

        #region Public properties

        /// <summary>
        /// Get or set the action form
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("form1")]
        [Localizable(true)]
        public string ActionForm
        {
            get
            {
                return _ActionForm;
            }

            set
            {
                _ActionForm = value;
            }
        }

        /// <summary>
        /// Min number of PageNoButtons that wiil be displayed
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue(5)]
        [Localizable(true)]
        public int MinDisplayPageNoButtons
        {
            get
            {
                return _MinDisplayPageNoButtons;
            }

            set
            {
                _MinDisplayPageNoButtons = value;
            }
        }

        /// <summary>
        /// Max number of PageNoButtons that wiil be displayed
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue(10)]
        [Localizable(true)]
        public int MaxDisplayPageNoButtons
        {
            get
            {
                return _MaxDisplayPageNoButtons;
            }

            set
            {
                _MaxDisplayPageNoButtons = value;
            }
        }


        /// <summary>
        /// Get or set current page
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue(1)]
        [Localizable(true)]
        public int CurPage
        {
            get
            {
                return (ViewState["CurPage"] == null) ? 1 : (int)ViewState["CurPage"];
            }

            set
            {
                ViewState["CurPage"] = value;
            }

        }

        /// <summary>
        /// Get page count
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue(0)]
        [Localizable(true)]
        public int PageCount
        {
            get
            {
                int _pageCount = 0;
                if ((RecordCount % PageSize) == 0)
                {
                    _pageCount = RecordCount / PageSize;
                }
                else
                {
                    _pageCount = RecordCount / PageSize + 1;
                }
                return _pageCount;
            }
        }

        /// <summary>
        /// Get or set record count
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue(0)]
        [Localizable(true)]
        public int RecordCount
        {
            get
            {
                return (ViewState["RecordCount"] == null) ? 0 : (int)(ViewState["RecordCount"]);
            }

            set
            {
                ViewState["RecordCount"] = value;
            }
        }

        /// <summary>
        /// Set or get the max record count inside one page
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue(10)]
        [Localizable(true)]
        public int PageSize
        {
            get
            {
                return (ViewState["PageSize"] == null) ? 1 : (int)(ViewState["PageSize"]);
            }

            set
            {
                //EnsureChildControls();
                ViewState["PageSize"] = value;
            }
        }

        #region Buttons

        /// <summary>
        /// Set or get the record count button
        /// </summary>
        public RecordCountButton RecordCountButton
        {
            get
            {
                return _RecordCountButton;
            }
        }

        /// <summary>
        /// Set or get the page count button
        /// </summary>
        public PageCountButton PageCountButton
        {
            get
            {
                return _PageCountButton;
            }
        }

        /// <summary>
        /// Set or get the prev page button
        /// </summary>
        public PrevPageButton PrevPageButton
        {
            get
            {
                return _PrevPageButton;
            }
        }

        /// <summary>
        /// Set or get the next page button
        /// </summary>
        public NextPageButton NextPageButton
        {
            get
            {
                return _NextPageButton;
            }
        }

        /// <summary>
        /// Set or get the pageno button
        /// </summary>
        public PageNoButton PageNoButton
        {
            get
            {
                return _PageNoButton;
            }
        }

        /// <summary>
        /// Set or get the pageno button
        /// </summary>
        public PageNoButton CurPageNoButton
        {
            get
            {
                return _CurPageNoButton;
            }
        }


        #endregion


        #endregion Public properties

        #region Protected override methods

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            StringBuilder doPost = new StringBuilder();

            doPost.AppendLine(  "<div>");
            doPost.AppendLine(  "   <input type=\"hidden\" name=\"" + this.UniqueID + "\" id=\"__EVENTTARGET\" value=\"\" />");
            doPost.AppendLine(  "</div>");
            doPost.AppendLine(  "<script type=text/javascript>");
            doPost.AppendLine(  "      function _doPost(cutomArg){");
            doPost.AppendFormat("          document.forms['{0}'].{1}.value = cutomArg;\r\n", ActionForm, this.UniqueID);
            doPost.AppendFormat("          document.forms['{0}'].submit();\r\n", ActionForm);
            doPost.AppendLine(  "      }");
            doPost.AppendLine(  "</script>");

            if (!Page.ClientScript.IsClientScriptIncludeRegistered(this.GetType(), "_doPost"))
            {
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "_doPost", doPost.ToString());
            }
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            RecordCountButton.RecordCount = RecordCount;
            PageCountButton.PageCount = PageCount;

            writer.Write(string.Format("<div class=\"{0}\" style=\"{1}\">\r\n", this.CssClass, Style.Value));
            writer.Write(RecordCountButton.Value);
            writer.Write(PageCountButton.Value);

            PrevPageButton.Visible = CurPage > 1;
            PrevPageButton.PrevPageNo = CurPage - 1;
            writer.Write(PrevPageButton.Value);

            int fst = 1;
            int lst = PageCount;

            if (CurPage + MinDisplayPageNoButtons >= MaxDisplayPageNoButtons)
            {
                fst = Math.Max(1, CurPage - (MaxDisplayPageNoButtons / 2));
                lst = Math.Min(PageCount, fst + MaxDisplayPageNoButtons -1);
            }
            else
            {
                lst = Math.Min(PageCount, CurPage + MinDisplayPageNoButtons -1);
            }

            for (int i = fst; i <= lst; i++)
            {
                PageNoButton pageNoButton;

                if (i != CurPage)
                {
                    pageNoButton = PageNoButton;
                }
                else
                {
                    pageNoButton = CurPageNoButton;
                }

                pageNoButton.PageNo = i;
                pageNoButton.CurPageNo = CurPage;
                writer.Write(pageNoButton.Value);
            }

            NextPageButton.Visible = CurPage < PageCount;
            NextPageButton.NextPageNo = CurPage + 1;
            writer.Write(NextPageButton.Value);

            writer.Write("</div>\r\n");
        }

        #endregion Protected override methods
    }
}