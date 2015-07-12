<%@ Control language="vb" Inherits="DotNetNuke.Authentication.ActiveDirectory.Login, DotNetNuke.Authentication.ActiveDirectory" AutoEventWireup="false" Explicit="True" Codebehind="Login.ascx.vb" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls"%>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="dnnForm dnnLoginService dnnClear">
    <div class="dnnFormItem">
        <asp:label id="plUsername" AssociatedControlID="txtUsername" runat="server" resourcekey="Username" CssClass="dnnFormLabel" />
        <asp:textbox id="txtUsername" runat="server" />
    </div>
    <div class="dnnFormItem">
        <asp:label id="plPassword" AssociatedControlID="txtPassword" runat="server" resourcekey="Password" CssClass="dnnFormLabel" />
        <asp:textbox id="txtPassword" textmode="Password" runat="server" />
    </div>
    <div class="dnnFormItem" id="divCaptcha1" runat="server" visible="false">
        <asp:label id="plCaptcha" AssociatedControlID="ctlCaptcha" runat="server" resourcekey="Captcha" CssClass="dnnFormLabel" />
    </div>
    <div class="dnnFormItem" id="divCaptcha2" runat="server" visible="false">
        <dnn:captchacontrol id="ctlCaptcha" captchawidth="130" captchaheight="40" runat="server" errorstyle-cssclass="dnnFormMessage dnnFormError" />
    </div>
    <p><asp:LinkButton id="cmdLogin" resourcekey="cmdLogin" cssclass="dnnPrimaryAction" text="Login" runat="server" /></p>
</div>
