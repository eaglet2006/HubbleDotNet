using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.WebDemo
{
    public class SearchPage
    {
        private String Style
        {
            get
            {
                return "<STYLE><!--" +
                        "body,td,.p1,.p2,.i{font-family:arial}" +
                        "body{margin:6px 0 0 0;background-color:#fff;color:#000;}" +
                        "table{border:0}" +
                        "TD{FONT-SIZE:9pt;LINE-HEIGHT:18px;}" +
                        ".f14{FONT-SIZE:14px}" +
                        ".f10{font-size:10.5pt}" +
                        ".f16{font-size:16px;font-family:Arial}" +
                        ".c{color:#7777CC;}" +
                        ".p1{LINE-HEIGHT:120%;margin-left:-12pt}" +
                        ".p2{width:100%;LINE-HEIGHT:120%;margin-left:-12pt}" +
                        ".i{font-size:16px}" +
                        ".t{COLOR:#0000cc;TEXT-DECORATION:none}" +
                        "a.t:hover{TEXT-DECORATION:underline}" +
                        ".p{padding-left:18px;font-size:14px;word-spacing:4px;}" +
                        ".f{line-height:120%;font-size:100%;width:32em;padding-left:15px;word-break:break-all;word-wrap:break-word;}" +
                        ".h{margin-left:8px;width:100%}" +
                        ".s{width:8%;padding-left:10px; height:25px;}" +
                        ".m,a.m:link{COLOR:#666666;font-size:100%;}" +
                        "a.m:visited{COLOR:#660066;}" +
                        ".g{color:#008000; font-size:12px;}" +
                        ".r{ word-break:break-all;cursor:hand;width:225px;}" +
                        ".bi {background-color:#D9E1F7;height:20px;margin-bottom:12px}" +
                        ".pl{padding-left:3px;height:8px;padding-right:2px;font-size:14px;}" +
                        ".Tit{height:21px; font-size:14px;}" +
                        ".fB{ font-weight:bold;}" +
                        ".mo,a.mo:link,a.mo:visited{COLOR:#666666;font-size:100%;line-height:10px;}" +
                        ".htb{margin-bottom:5px;}" +
                        "#ft{clear:both;line-height:20px;background:#E6E6E6;text-align:center}" +
                        "#ft,#ft *{color:#77C;font-size:12px;font-family:Arial}" +
                        "#ft span{color:#666}" +
                        "--></STYLE>";
            }
        }

        public String GetStartPage()
        {
            String html = ""; 
            html += "<html>" + Style + "<body>";
            html += "<input id=\"Keywords\" type=text >";
            html += "&nbsp&nbsp&nbsp";
            html += "<input id=\"Search\" type=submit value=\"ËÑË÷\">";
            html += "</body></html>";
            return html;
        }

        internal String GetResultPage(List<TNews> newsList, String keyWord, int pageCount, int pageNo)
        {
            StringBuilder html = new StringBuilder();

            html.Append("<html>" + Style + "<body>");
            html.AppendFormat("<input id=\"Keywords\" type=text Value={0}>", keyWord);
            html.Append("&nbsp&nbsp&nbsp");
            html.Append("<input id=\"Search\" type=submit value=\"ËÑË÷\">");

            foreach (TNews news in newsList)
            {
                html.Append("<br>");
                try
                {
                    GetLine(news, html);
                }
                catch
                {
                }
            }

            html.Append("<br>");

            for (int i = 1; i <= pageCount; i++)
            {
                if (i == pageNo)
                {
                    html.AppendFormat("<a id=Page{0} href=#><font color=\"red\">{0}</font></a>&nbsp&nbsp&nbsp", i);
                }
                else
                {
                    html.AppendFormat("<a id=Page{0} href=#>{0}</a>&nbsp&nbsp&nbsp", i);
                }
            }

            html.Append("</body></html>");
            return html.ToString();
        }

        private void GetLine(TNews news, StringBuilder html)
        {
            html.Append("<div>");
            html.Append("<a id=dfs6 href='" + news.Url + "' target='_blank'>");
            html.Append("<font size=\"3\">" + news.Title + "</font>");
            html.Append("</a><br>");

            Uri uri = new Uri(news.Url);
            int size = news.Content.Length;
            html.AppendFormat(news.Abstract);
            html.Append("<br>");
            html.AppendFormat(@"<font color=#008000>{0}/ {1} {2}</font>", uri.Host, size, news.Time.ToString("yyyy-M-d"));

        }
    }
}
