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
    bool _Showed = false;

    protected void Page_Load(object sender, EventArgs e)
    {
        AspNetPager.Visible = TextBoxSearch.Text != "";
    }

    protected override void Render(HtmlTextWriter writer)
    {
        if (!_Showed && IsPostBack)
        {
            AspNetPager.CurPage = 1;
            ShowTable(TextBoxSearch.Text);
        }

        base.Render(writer);
    }

    private string GetHerf(TNews news)
    {
        StringBuilder herf = new StringBuilder();
 
        herf.AppendFormat(@"<a href=""{0}"" target=""_blank"">", news.Url);
        herf.AppendFormat(@"{0}</a><br>", news.TitleHighLighter);
        herf.AppendFormat(@"<font size=-1>{0}</font>", news.Abstract);
        herf.AppendFormat(@"<br><font size=-1 color=#008000>{0} {1}K {2}</font>",
            news.Url, news.Content.Length / 1024, news.Time.ToString("yyyy-MM-dd"));
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
        string sql;

        List<TNews> newsList = Index.Search(indexDir, DropDownListSearchType.Text, text, pageSize, pageNo, DropDownListSort.Text,
            out recCount, out elapsedMilliseconds, out sql);
        LabelSql.Text = sql;
        LabelDuration.Text = "Duration:" + elapsedMilliseconds + "ms";
        AspNetPager.CssClass = "align-center";
        AspNetPager.RecordCount = recCount;

        AspNetPager.RecordCountButton.Text = "Count:";
        AspNetPager.RecordCountButton.TextFormat = "{0}&nbsp";

        AspNetPager.PageCountButton.Text = "Pages:";
        AspNetPager.PageCountButton.TextFormat = "{0}&nbsp";

        AspNetPager.NextPageButton.Text = "Next";
        AspNetPager.PageCountButton.TextFormat = "&nbsp{0}";

        AspNetPager.PrevPageButton.Text = "Prev";
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
        _Showed = true;
        if (TextBoxSearch.Text != "")
        {
            AspNetPager.CurPage = 1;
            ShowTable(TextBoxSearch.Text.Trim());
        }
    }

    protected void AspNetPager_PageChanged(object sender, Eaglet.Workroom.AspDotNetPager.AspNetPager.PageArgs e)
    {
        _Showed = true;

        if (TextBoxSearch.Text != "")
        {
            ShowTable(TextBoxSearch.Text);
        }
    }
}
