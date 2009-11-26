using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text;
using System.Diagnostics;

using Hubble.WebDemo;

public partial class _Default : System.Web.UI.Page 
{
    protected void Page_Load(object sender, EventArgs e)
    {
        AspNetPager.Visible = TextBoxSearch.Text != "";
    }

    private string GetHerf(TNews news)
    {
        StringBuilder herf = new StringBuilder();
 
        herf.AppendFormat(@"<a href=""{0}"" target=""_blank"">", news.Url);
        herf.AppendFormat(@"{0}</a><br>", news.TitleHighLighter);
        herf.AppendFormat(@"<font size=-1>{0}</font>", news.Abstract);
        herf.AppendFormat(@"<br><font size=-1 color=#008000>{0}</font>", news.Url);
        return herf.ToString();
    }

    private string GetNavigate(TNews news)
    {
        StringBuilder herf = new StringBuilder();

        herf.AppendFormat(@"<a href=""{0}"" target=""_blank"">", news.Url);
        herf.AppendFormat(@"{0}</a><br>", news.TitleHighLighter);
        herf.AppendFormat(@"<font size=-1>{0}</font>", news.Abstract);
        herf.AppendFormat(@"<br><font size=-1 color=#008000>{0}</font>", news.Url);
        return herf.ToString();
    }


    private void ShowTable(string text)
    {
        string homeDir = Page.MapPath("./");
        System.Environment.CurrentDirectory = homeDir;
        int recCount;
        int pageNo = AspNetPager.CurPage;
        int pageSize = AspNetPager.PageSize;

        string indexDir = "";

        long elapsedMilliseconds;

        List<TNews> newsList = Index.Search(indexDir, text, pageSize, pageNo, out recCount, out elapsedMilliseconds);
        LabelDuration.Text = "用时：" + elapsedMilliseconds + "毫秒";
        AspNetPager.CssClass = "align-center";
        AspNetPager.RecordCount = recCount;

        AspNetPager.RecordCountButton.Text = "记录数:";
        AspNetPager.RecordCountButton.TextFormat = "{0}&nbsp";

        AspNetPager.PageCountButton.Text = "总页数:";
        AspNetPager.PageCountButton.TextFormat = "{0}&nbsp";

        AspNetPager.NextPageButton.Text = "下一页";
        AspNetPager.PageCountButton.TextFormat = "&nbsp{0}";

        AspNetPager.PrevPageButton.Text = "上一页";
        AspNetPager.PageCountButton.TextFormat = "{0}&nbsp";

        AspNetPager.PageNoButton.TextFormat = "&nbsp[{0}]&nbsp";



        foreach (TNews news in newsList)
        {
            TableList.Style.Add("TABLE-LAYOUT", "fixed");
            TableList.Style.Add("width", "100%");
            TableList.Width = 30;
            TableRow row = new TableRow();
            TableCell cell = new TableCell();
            cell.Style.Add("TABLE-LAYOUT", "fixed");
            cell.Style.Add("WORD-WRAP", "break-word");
            cell.Style.Add("width", "80%");
            cell.CssClass = "f";
            cell.Text = GetHerf(news);
            row.Cells.Add(cell);

            cell = new TableCell();
            cell.Style.Add("TABLE-LAYOUT", "fixed");
            cell.Style.Add("WORD-WRAP", "break-word");
            cell.Style.Add("width", "20%");
            row.Cells.Add(cell);

            TableList.Rows.Add(row);
        }
    }

    protected void ButtonSearch_Click(object sender, EventArgs e)
    {
        if (TextBoxSearch.Text != "")
        {
            AspNetPager.CurPage = 1;
            ShowTable(TextBoxSearch.Text);
        }

        GC.Collect();
    }
    protected void AspNetPager_PageChanged(object sender, Eaglet.Workroom.AspDotNetPager.AspNetPager.PageArgs e)
    {
        if (TextBoxSearch.Text != "")
        {
            ShowTable(TextBoxSearch.Text);
        }
    }
}
