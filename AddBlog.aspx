<%@ Page Language="C#" AutoEventWireup="true" CodeFile="AddBlog.aspx.cs" Inherits="AddBlog" Async="true" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:DropDownList ID="parent_category" runat="server" AutoPostBack="True" Height="27px" Width="135px" OnSelectedIndexChanged="parent_category_SelectedIndexChanged"></asp:DropDownList>
        <asp:DropDownList ID="child_category" runat="server" Height="27px" Width="161px"></asp:DropDownList>
        <br />
        <br />
        <asp:CheckBoxList ID="choosen_users" runat="server">
            <asp:ListItem Value="54c8a200e4b0a0e2523d41f3">张崇园</asp:ListItem>
            <asp:ListItem Value="54c8b2f8e4b029264cb66142">Mr. Big</asp:ListItem>
            <asp:ListItem Value="54c9c8d4e4b0c6c6afb5f323">韩科技</asp:ListItem>
            <asp:ListItem Value="54d6f366e4b029bade939875">Nicolasguo</asp:ListItem>
        </asp:CheckBoxList>
        <br />
        <asp:FileUpload ID="file_upload" runat="server" Width="262px" AllowMultiple="True" />
        <asp:Button ID="btn_upload" runat="server" OnClick="btn_upload_Click" Text="开始上传" Width="116px" />
        <br />
        <br />
        <asp:Label ID="error_label" runat="server" Visible="False"></asp:Label>
    </div>
    </form>
</body>
</html>
