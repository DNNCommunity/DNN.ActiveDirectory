<%@ Control Language="vb" AutoEventWireup="false" Explicit="True" Inherits="DotNetNuke.Authentication.ActiveDirectory.Settings, DotNetNuke.Authentication.ActiveDirectory"
    CodeBehind="Settings.ascx.vb" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls.Internal" Assembly="DotNetNuke.Web" %>
<%@ Register TagPrefix="dnncl" Namespace="DotNetNuke.Web.Client.ClientResourceManagement" Assembly="DotNetNuke.Web.Client" %>


<div class="dnnFormMessage dnnFormWarning" id="MessageCell" runat="server">
</div>
<div class="dnnForm dnnHTMLSettings dnnClear">
    <div class="dnnFormItem">
        <dnn:label id="plReplaceTokens" controlname="chkAuthentication" runat="server" text="Windows authentication?" />
        <asp:CheckBox ID="chkAuthentication" runat="server" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plHidden" runat="server" controlname="chkHidden" text="Hide Login Controls?" />
        <asp:CheckBox ID="chkHidden" runat="server" />
    </div>
    <div class="dnnFormItem">
        <dnn:Label id="plSynchronizeRole" runat="server" controlname="chkSynchronizeRole" text="Synchronize Role?" />
        <asp:CheckBox ID="chkSynchronizeRole" runat="server" />
    </div>
    <div class="dnnFormItem">
        <dnn:Label id="plSynchronizePhoto" runat="server" controlname="chkSynchronizePhoto" text="Synchronize Photo?" />
        <asp:CheckBox ID="chkSynchronizePhoto" runat="server" />
    </div>
    <div class="dnnFormItem">
        <dnn:Label id="plAutoLogin" runat="server" controlname="chkAutoLogin" text="Enable Auto-Login?" />
        <asp:CheckBox ID="chkAutoLogin" runat="server" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plSynchornizePassword" runat="server" controlname="chkSynchronizePassword" text="Synchronize Password?" />
        <asp:CheckBox ID="chkSynchronizePassword" runat="server" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plAutoCreate" runat="server" controlname="chkAutoCreate" text="Automatically Create Users?" />
        <asp:CheckBox ID="chkAutoCreate" runat="server" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plDebugMode" runat="server" controlname="chkDebugMode" text="Enable Debug Mode?" />
        <asp:CheckBox ID="chkDebugMode" runat="server" />
    </div>
    <div class="dnnFormItem" visible="false" runat="server">
        <dnn:label id="plStripDomainName" runat="server" controlname="chkStripDomainName" text="Strip Domain?" visible="false" />
        <asp:CheckBox ID="chkStripDomainName" runat="server" Visible="false" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plProvider" runat="server" controlname="cboProviders" text="Provider" />
        <asp:DropDownList ID="cboProviders" runat="server" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plAuthenticationType" runat="server" controlname="cboAuthenticationType" text="Authentication Type" />
        <asp:DropDownList ID="cboAuthenticationType" runat="server" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plRootDomain" runat="server" controlname="txtRootDomain" text="Root Domain:" />
        <asp:TextBox ID="txtRootDomain" runat="server" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plUserName" runat="server" controlname="txtUserName" text="User Name:" />
        <asp:TextBox ID="txtUserName" runat="server" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plPassword" runat="server" controlname="txtPassword" text="Password:" />
        <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plConfirm" runat="server" controlname="chkAuthentication" text="Confirm Password:" />
        <asp:TextBox ID="txtConfirm" runat="server" TextMode="Password" />
        <asp:CompareValidator ID="valConfirm" runat="server" ControlToCompare="txtPassword" ControlToValidate="txtConfirm"
            ErrorMessage="<br>Password Values Entered Do Not Match." Display="Dynamic" CssClass="NormalRed" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plEmailDomain" runat="server" controlname="txtEmailDomain" text="Email Domain:" />
        <asp:TextBox ID="txtEmailDomain" runat="server" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plDefaultDomain" runat="server" controlname="txtDefaultDomain" text="Default Domain Prefix:" />
        <asp:TextBox ID="txtDefaultDomain" runat="server" />
    </div>

    <div class="dnnFormItem">
        <dnn:label id="plGroupAllow" runat="server" controlname="cboGroupAllow" text="Use Group Membership" />
        <asp:DropDownList ID="cboGroupAllow" runat="server">
            <asp:ListItem id="plGroupAllow_none" controlname="liGroupAllow_none" Value="0">No, do not use group membership.</asp:ListItem>
            <asp:ListItem id="plGroupAllow_allow" controlname="liGroupAllow_allow" Value ="1">Allow only members of the listed groups to log in.</asp:ListItem>
            <asp:ListItem id="plGroupAllow_reject" controlname="liGroupAllow_reject" Value="2">Do not allow members of the listed groups to log in.</asp:ListItem>
        </asp:DropDownList>
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plGroups" runat="server" controlname="txtGroups" text="Group List:" />
        <asp:TextBox ID="txtGroups" runat="server" Height="72px" TextMode="MultiLine" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plAutoIP" runat="server" controlname="txtAutoIP" text="Auto-login IP Address:" />
        <asp:TextBox ID="txtAutoIP" runat="server" Height="72px" TextMode="MultiLine" />
    </div>
    <div class="dnnFormItem">
        <dnn:label id="plBots" runat="server" controlname="txtBots" text="Search Bots:" />
        <asp:TextBox ID="txtBots" runat="server" Height="72px" TextMode="MultiLine" />
    </div>
</div>



