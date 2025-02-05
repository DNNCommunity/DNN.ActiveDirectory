'
' DotNetNuke® - http://www.dotnetnuke.com
' Copyright (c) 2002-2013
' by DotNetNuke Corporation
'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
'
' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
' of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
' DEALINGS IN THE SOFTWARE.
'

Imports DotNetNuke.Entities.Portals

Namespace DotNetNuke.Authentication.ActiveDirectory
    Partial Class WindowsSignin
        Inherits Page

        Private objAuthentication As IAuthenticationController
        Private configuration As IConfiguration

        Sub New(authenticationController As IAuthenticationController,
                ByVal configuration As IConfiguration)
            Me.objAuthentication = authenticationController
            Me.configuration = configuration
        End Sub
#Region " Web Form Designer Generated Code "

        'This call is required by the Web Form Designer.
        <DebuggerStepThrough()> _
        Private Sub InitializeComponent()

        End Sub

        'NOTE: The following placeholder declaration is required by the Web Form Designer.
        'Do not delete or move it.
        Private designerPlaceholderDeclaration As Object

        Private Sub Page_Init (ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Init
            'CODEGEN: This method call is required by the Web Form Designer
            'Do not modify it using the code editor.
            InitializeComponent()
            Dim config As ConfigInfo
            If Request.ServerVariables("LOGON_USER").Length > 0 Then
                ' Reset config
                Configuration.ResetConfig()
                config = configuration.GetConfig()
                If (config.WindowsAuthentication Or config.HideWindowsLogin) Then
                    objAuthentication.AuthenticationLogon()
                Else
                    Me.plNoAuthentication.Visible = True
                    Me.plSetIIS.Visible = False
                End If
            End If

        End Sub

#End Region

        Private Sub Page_Load (ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
            'Put user code to initialize the page here        
        End Sub
    End Class
End Namespace