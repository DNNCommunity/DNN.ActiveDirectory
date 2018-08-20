<%@ Page Language="vb" AutoEventWireup="false" Inherits="DotNetNuke.Authentication.ActiveDirectory.WindowsSignin, DotNetNuke.Authentication.ActiveDirectory" Codebehind="WindowsSignin.aspx.vb" %>
<p align=center>
<label id="plSetIIS" visible="true" runat="server">Error while processing Windows Authentication<br/>Check your IIS settings. DesktopModules/AuthenticationServices/ActiveDirectory/WindowsSignin.aspx should <b>NOT</b> allow anonymous access.</label>
<label id="plNoAuthentication" visible="false" runat="server">Windows Authentication Disabled.<br />Please contact your administrator.<br /></label>
</p>
