<%@ Control Language="vb" Inherits="DotNetNuke.Authentication.ActiveDirectory.Login, DotNetNuke.Authentication.ActiveDirectory" AutoEventWireup="false" Explicit="True" CodeBehind="Login.ascx.vb" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
    <div class="dnnForm dnnLoginService dnnClear" style="margin-top: 15px;">
        <div class="dnnFormItem">
            <asp:Label ID="plUsername" AssociatedControlID="txtUsername" runat="server" resourcekey="Username" CssClass="dnnFormLabel" />
            <asp:TextBox ID="txtUsername" runat="server" onkeydown="return checkEnter(event)" />
        </div>
        <div class="dnnFormItem">
            <asp:Label ID="plPassword" AssociatedControlID="txtPassword" runat="server" resourcekey="Password" CssClass="dnnFormLabel" />
            <asp:TextBox ID="txtPassword" TextMode="Password" runat="server" onkeydown="return checkEnter(event)" />
        </div>

        <div class="dnnFormItem" id="divCaptcha1" runat="server" visible="false">
            <asp:Label ID="plCaptcha" AssociatedControlID="ctlCaptcha" runat="server" resourcekey="Captcha" CssClass="dnnFormLabel" />
        </div>
        <div class="dnnFormItem" id="divCaptcha2" runat="server" visible="false">
            <dnn:captchacontrol id="ctlCaptcha" captchawidth="130" captchaheight="40" runat="server" errorstyle-cssclass="dnnFormMessage dnnFormError" />
        </div>
         <div class="dnnFormItem">
            <label class="dnnFormLabel" ></label> 
            <asp:LinkButton ID="cmdLogin" resourcekey="cmdLogin" CssClass="dnnPrimaryAction" Text="Login" runat="server" ClientIDMode="Static" />
        </div>
    </div>

<script>
    function checkEnter(e) {
        if (e.keyCode == 13) {
            $("#cmdLogin")[0].click();
        }
    }
</script>