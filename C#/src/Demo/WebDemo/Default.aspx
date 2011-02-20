<%@ Register Assembly="AspDotNetPager" Namespace="Eaglet.Workroom.AspDotNetPager" TagPrefix="cc1" %>
<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Hubble Web Demo</title>
    
<style type="text/css"><!--
body,td,.p1,.p2,.i{font-family:arial}
body{margin:6px 0 0 0;background-color:#fff;color:#000;}
table{border:0}
TD{FONT-SIZE:9pt;LINE-HEIGHT:18px;}
.f14{FONT-SIZE:14px}
.f10{font-size:10.5pt}
.f16{font-size:16px;font-family:Arial}
.c{color:#7777CC;}
.p1{LINE-HEIGHT:120%;margin-left:-12pt}
.p2{width:100%;LINE-HEIGHT:120%;margin-left:-12pt}
.i{font-size:16px}
.t{COLOR:#0000cc;TEXT-DECORATION:none}
a.t:hover{TEXT-DECORATION:underline}
.p{padding-left:18px;font-size:14px;word-spacing:4px;}
.f{line-height:120%;font-size:100%;width:32em;padding-left:15px;word-break:break-all;word-wrap:break-word;}
.h{margin-left:8px;width:100%}
.s{width:8%;padding-left:10px; height:25px;}
.m,a.m:link{COLOR:#666666;font-size:100%;}
a.m:visited{COLOR:#660066;}
.g{color:#008000; font-size:12px;}
.r{ word-break:break-all;cursor:hand;width:225px;}
.bi {background-color:#D9E1F7;height:20px;margin-bottom:12px}
.pl{padding-left:3px;height:8px;padding-right:2px;font-size:14px;}
.Tit{height:21px; font-size:14px;}
.fB{ font-weight:bold;}
.mo,a.mo:link,a.mo:visited{COLOR:#666666;font-size:100%;line-height:10px;}
.htb{margin-bottom:5px;}
#ft{clear:both;line-height:20px;background:#E6E6E6;text-align:center}
#ft,#ft *{color:#77C;font-size:12px;font-family:Arial}
#ft span{color:#666}
.align-center{   
    margin:0 auto;      
    text-align:center;  }
       
--></style>
    
</head>
<body>
    <p>
        <br />
    </p>
    <form id="form1" runat="server">
    <div class = "align-center">
        <br />
        HubbleDotNet&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
        <asp:TextBox ID="TextBoxSearch" runat="server" Width="308px" ></asp:TextBox>&nbsp;&nbsp;
        <asp:Label ID="LabelDuration" runat="server" Text="Duration:0ms"></asp:Label>
        <br />
        <asp:DropDownList ID="DropDownListSearchType" runat="server">
            <asp:ListItem Value="Precise"></asp:ListItem>
            <asp:ListItem Value="Fuzzy"></asp:ListItem>
            <asp:ListItem Value="Like"></asp:ListItem>
        </asp:DropDownList>
        &nbsp;<asp:DropDownList 
            ID="DropDownListSort" runat="server">
            <asp:ListItem Value="score desc">Sort by score</asp:ListItem>
            <asp:ListItem Value="time desc">Sort by time</asp:ListItem>
            <asp:ListItem Value="time desc, score desc">Sort by time and score</asp:ListItem>
        </asp:DropDownList>
        <asp:Button ID="ButtonSearch" runat="server" Text="Search" OnClick="ButtonSearch_Click" />
            <p>
        <asp:Label ID="LabelSql" runat="server" Text="SQL:"></asp:Label>
    </p>

            </div>

        <div>
        <br />
        <asp:Table ID="TableList" runat="server">
        </asp:Table>
        <cc1:AspNetPager id="AspNetPager" runat="server" OnPageChanged="AspNetPager_PageChanged" PageSize="10">
        </cc1:AspNetPager>
        </div>
    </form>

</body>
</html>
