<%@ Control Language="vb" AutoEventWireup="false" Explicit="True" Inherits="DotNetNuke.Authentication.ActiveDirectory.Settings, DotNetNuke.Authentication.ActiveDirectory"
    Codebehind="Settings.ascx.vb" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<asp:Panel ID="pnlError" runat="server" Visible="false">
    <asp:Image ID="Image1" runat="server" ImageUrl="~/images/red-error.gif" ImageAlign="Left"  /><asp:Label ID="lblError" runat="server" CssClass="Normal" />
</asp:Panel>
<table id="tblSettings" runat="server" cellspacing="2" cellpadding="2" width="500" summary="Authentication Settings Design Table" border="0">
    <tr valign="top" height="*">
        <td id="MessageCell" align="center" runat="server"></td>
    </tr>
    <tr>
        <td valign="top" width="500">
            <table id="tblADSettings" cellspacing="0" cellpadding="1" border="0">
                <tr>
                    <td class="SubHead" width="200"><dnn:label id="plAuthentication" runat="server" controlname="chkAuthentication" text="Windows Authentication?" /></td>
                    <td valign="top"><asp:CheckBox ID="chkAuthentication" runat="server" CssClass="NormalTextBox"></asp:CheckBox></td>
                </tr>
                <tr>
                    <td class="SubHead" width="200"><dnn:label id="plHidden" runat="server" controlname="chkHidden" text="Hide Login Controls?" /></td>
                    <td valign="top"><asp:CheckBox ID="chkHidden" runat="server" CssClass="NormalTextBox"></asp:CheckBox></td>
                </tr>
                <tr>
                    <td class="SubHead" width="200"><dnn:Label id="plSynchronizeRole" runat="server" controlname="chkSynchronizeRole" text="Synchronize Role?" /></td>
                    <td valign="top"><asp:CheckBox ID="chkSynchronizeRole" runat="server" CssClass="NormalTextBox" /></td>
                </tr>
                <tr>
                    <td class="SubHead" width="200"><dnn:Label id="plSynchronizePhoto" runat="server" controlname="chkSynchronizePhoto" text="Synchronize Photo?" /></td>
                    <td valign="top"><asp:CheckBox ID="chkSynchronizePhoto" runat="server" CssClass="NormalTextBox" /></td>
                </tr>
                    <tr>
                    <td class="SubHead" width="200"><dnn:Label id="plAutoLogin" runat="server" controlname="chkAutoLogin" text="Enable Auto-Login?" /></td>
                    <td valign="top"><asp:CheckBox ID="chkAutoLogin" runat="server" CssClass="NormalTextBox" /></td>
                </tr>
                <tr id="rowSynchornizePassword" runat="server" visible="False">
                    <td class="SubHead" width="200"><dnn:label id="plSynchornizePassword" runat="server" controlname="chkSynchronizePassword" text="Synchronize Password?" /></td>
                    <td valign="top"><asp:CheckBox ID="chkSynchronizePassword" runat="server" CssClass="NormalTextBox" /></td>
                </tr>
                <tr id="rowAutoCreate" runat="server">
                    <td class="SubHead" width="200"><dnn:label id="plAutoCreate" runat="server" controlname="chkAutoCreate" text="Automatically Create Users?" /></td>
                    <td valign="top" nowrap="nowrap"><asp:CheckBox ID="chkAutoCreate" runat="server" CssClass="NormalTextBox" /></td>
                </tr>
                 <tr id="Tr1" runat="server">
                    <td class="SubHead" width="200"><dnn:label id="plDebugMode" runat="server" controlname="chkDebugMode" text="Enable Debug Mode?" /></td>
                    <td valign="top" nowrap="nowrap"><asp:CheckBox ID="chkDebugMode" runat="server" CssClass="NormalTextBox" /></td>
                </tr>
                <tr>
                    <td class="SubHead" width="200"><dnn:label id="plStripDomainName" runat="server" controlname="chkStripDomainName" text="Strip Domain?" visible="false" /></td>
                    <td valign="top"><asp:CheckBox ID="chkStripDomainName" runat="server" CssClass="NormalTextBox" Visible="false" /></td>
                </tr>
                <tr>
                    <td class="SubHead" width="200"><dnn:label id="plProvider" runat="server" controlname="cboProviders" text="Provider" /></td>
                    <td valign="top"><asp:DropDownList ID="cboProviders" runat="server" CssClass="NormalTextBox" Width="300" /></td>
                </tr>
                <tr>
                    <td class="SubHead" width="200"><dnn:label id="plAuthenticationType" runat="server" controlname="cboAuthenticationType" text="Authentication Type" /></td>
                    <td valign="top"><asp:DropDownList ID="cboAuthenticationType" runat="server" CssClass="NormalTextBox" Width="300" /></td>
                </tr>
                <tr id="rowRootDomain" runat="server">
                    <td class="SubHead" width="200"><dnn:label id="plRootDomain" runat="server" controlname="txtRootDomain" text="Root Domain:" /></td>
                    <td valign="top" nowrap="nowrap"><asp:TextBox ID="txtRootDomain" runat="server" CssClass="NormalTextBox" Width="300px" /></td>
                </tr>
                <tr id="rowUserName" runat="server">
                    <td class="SubHead" width="200"><dnn:label id="plUserName" runat="server" controlname="txtUserName" text="User Name:" /></td>
                    <td valign="top" nowrap="nowrap"><asp:TextBox ID="txtUserName" runat="server" CssClass="NormalTextBox" Width="300px" /></td>
                </tr>
                <tr id="rowPassword" runat="server">
                    <td class="SubHead" width="200"><dnn:label id="plPassword" runat="server" controlname="txtPassword" text="Password:" /></td>
                    <td valign="top" nowrap="nowrap"><asp:TextBox ID="txtPassword" runat="server" CssClass="NormalTextBox" Width="300px" TextMode="Password" /></td>
                </tr>
                <tr id="rowConfirm" runat="server">
                    <td class="SubHead" width="200"><dnn:label id="plConfirm" runat="server" controlname="chkAuthentication" text="Confirm Password:" /></td>
                    <td valign="top" nowrap="nowrap">
                        <asp:TextBox ID="txtConfirm" runat="server" CssClass="NormalTextBox" Width="300px" TextMode="Password" />
                        <asp:CompareValidator ID="valConfirm" runat="server" ControlToCompare="txtPassword" ControlToValidate="txtConfirm" 
                            ErrorMessage="<br>Password Values Entered Do Not Match." Display="Dynamic" CssClass="NormalRed" />
                    </td>
                </tr>
                <tr id="rowEmailDomain" runat="server">
                    <td class="SubHead" width="200"><dnn:label id="plEmailDomain" runat="server" controlname="txtEmailDomain" text="Email Domain:" /></td>
                    <td valign="top" nowrap="nowrap"><asp:TextBox ID="txtEmailDomain" runat="server" CssClass="NormalTextBox" Width="300px" /></td>
                </tr>
                <tr id="rowDefaultDomain" runat="server">
                    <td class="SubHead" width="200"><dnn:label id="plDefaultDomain" runat="server" controlname="txtDefaultDomain" text="Default Domain Prefix:" /></td>
                    <td valign="top" nowrap="nowrap"><asp:TextBox ID="txtDefaultDomain" runat="server" CssClass="NormalTextBox" Width="300px" /></td>
                </tr> 
                <tr id="rowSubNet" runat="server">
                    <td class="SubHead" width="200"><dnn:label id="plAutoIP" runat="server" controlname="txtAutoIP" text="Auto-login IP Address:" /></td>
                    <td valign="top" nowrap="nowrap"><asp:TextBox ID="txtAutoIP" runat="server" CssClass="NormalTextBox" Width="300px" Height="72px" TextMode="MultiLine" /></td>
                </tr> 
                <tr id="rowBots" runat="server">
                    <td class="SubHead" width="200"><dnn:label id="plBots" runat="server" controlname="txtBots" text="Search Bots:" /></td>
                    <td valign="top" nowrap="nowrap"><asp:TextBox ID="txtBots" runat="server" CssClass="NormalTextBox" Width="300px" Height="72px" TextMode="MultiLine" /></td>
                </tr>
            </table>
        </td>
    </tr>
</table>
