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

Imports DotNetNuke.UI.Skins.Controls
Imports DotNetNuke.Authentication.ActiveDirectory.ADSI
Imports DotNetNuke.Services.Authentication
Imports DotNetNuke.Common.Utilities
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Services.Exceptions
Imports DotNetNuke.Framework.Providers


Namespace DotNetNuke.Authentication.ActiveDirectory
    Partial Class Settings
        Inherits AuthenticationSettingsBase

#Region "Private Members"

        Private _strError As String = Null.NullString

#End Region

#Region "Private Methods"

        Private Sub DisplayIpError(ByVal strInvalidIP As String)
            Dim _
                strError As String = strInvalidIP & " " &
                                     Localization.GetString("InValidIPAddress", Me.LocalResourceFile)
            tblSettings.Visible = True
            pnlError.Visible = True
            lblError.Text = strError
        End Sub

        Private Function GetUserDomainName(ByVal UserName As String) As String
            Dim strReturn As String = ""
            If UserName.IndexOf("\") > 0 Then
                strReturn = Left(UserName, (UserName.IndexOf("\")))
            End If
            Return strReturn
        End Function

        Private Function LocalizedStatus(ByVal InputText As String) As String
            'Return InputText
            Dim strReturn As String = InputText
            strReturn =
                strReturn.Replace("[Global Catalog Status]",
                                   Localization.GetString("[Global Catalog Status]", Me.LocalResourceFile))
            strReturn =
                strReturn.Replace("[Root Domain Status]",
                                   Localization.GetString("[Root Domain Status]", Me.LocalResourceFile))
            strReturn =
                strReturn.Replace("[LDAP Status]", Localization.GetString("[LDAP Status]", Me.LocalResourceFile))
            strReturn =
                strReturn.Replace("[Network Domains Status]",
                                   Localization.GetString("[Network Domains Status]", Me.LocalResourceFile))
            strReturn =
                strReturn.Replace("[LDAP Error Message]",
                                   Localization.GetString("[LDAP Error Message]", Me.LocalResourceFile))
            strReturn = strReturn.Replace("OK", Localization.GetString("OK", Me.LocalResourceFile))
            strReturn = strReturn.Replace("FAIL", Localization.GetString("FAIL", Me.LocalResourceFile))
            '
            Return strReturn

        End Function

        Private Function CheckEnteredIPAddr() As Boolean
            If Right(Me.txtAutoIP.Text, 1) = ";" Then
                Me.txtAutoIP.Text = Left(Me.txtAutoIP.Text, Me.txtAutoIP.Text.Length - 1)
            End If
            Dim arrIPArray As New ArrayList
            Dim arrAutoIP() = Me.txtAutoIP.Text.Split(";")
            For intCount As Integer = 0 To arrAutoIP.Length - 1
                Dim strAutoIP As String = arrAutoIP(intCount)
                Dim intFullIPAddr As Integer = 0
                If (InStr(strAutoIP, "-")) Then
                    Dim arrIPRange() = strAutoIP.Split("-")
                    For intIPCount As Integer = 0 To arrIPRange.Length - 1
                        intFullIPAddr = arrIPRange(intIPCount).ToString.Split(".").GetUpperBound(0)
                        Select Case intFullIPAddr
                            Case 3
                                arrIPArray.Add(arrIPRange(intIPCount).ToString)
                            Case Else
                                DisplayIpError(arrIPRange(intIPCount).ToString)
                                Return False
                                'Exit Function
                        End Select
                    Next
                Else
                    intFullIPAddr = arrAutoIP(intCount).ToString.Split(".").GetUpperBound(0)
                    Select Case intFullIPAddr
                        Case 3
                            arrIPArray.Add(arrAutoIP(intCount).ToString)
                        Case Else
                            DisplayIpError(arrAutoIP(intCount).ToString)
                            Return False
                            'Exit Function
                    End Select
                End If
            Next
            For intIPCheck As Integer = 0 To arrIPArray.Count - 1
                Try
                    Dim strIPAddr As String = Utilities.GetIP4Address(arrIPArray(intIPCheck))
                Catch ex As Exception
                    DisplayIpError(arrIPArray(intIPCheck))
                    Return False
                    'Exit Function
                End Try
            Next
            Return True
        End Function

#End Region

#Region "Public Methods"

        Public Overrides Sub UpdateSettings()
            Dim _portalSettings As PortalSettings = CType(HttpContext.Current.Items("PortalSettings"), PortalSettings)
            Try

                'Code Cleanup
                If Not chkAuthentication.Checked Then
                    Configuration.UpdateConfig(_portalSettings.PortalId, False, False, "", "", "", "", False, False,
                                                False, "", "", "", "", False, "", False)
                    Configuration.ResetConfig()
                Else
                    Dim providerTypeName As String = cboProviders.SelectedItem.Value
                    Dim authenticationType As String = cboAuthenticationType.SelectedItem.Value
                    If Not (txtAutoIP.Text = String.Empty) Then
                        If Not (CheckEnteredIPAddr()) Then
                            Exit Sub
                        End If
                    End If
                    'ACD-5585
                    'WorkItems 4766 and 4077
                    If chkAuthentication.Checked And Not chkHidden.Checked Then
                        Configuration.UpdateConfig(_portalSettings.PortalId, chkAuthentication.Checked,
                                                    chkHidden.Checked,
                                                    txtRootDomain.Text, txtEmailDomain.Text, txtUserName.Text,
                                                    txtPassword.Text, chkSynchronizeRole.Checked,
                                                    chkSynchronizePassword.Checked, chkStripDomainName.Checked,
                                                    providerTypeName, authenticationType, txtAutoIP.Text,
                                                    txtDefaultDomain.Text, chkAutoCreate.Checked, txtBots.Text, chkSynchronizePhoto.Checked)
                    Else
                        Configuration.UpdateConfig(_portalSettings.PortalId, False, chkHidden.Checked,
                                                    txtRootDomain.Text, txtEmailDomain.Text,
                                                    txtUserName.Text, txtPassword.Text, chkSynchronizeRole.Checked,
                                                    chkSynchronizePassword.Checked,
                                                    chkStripDomainName.Checked, providerTypeName, authenticationType,
                                                    txtAutoIP.Text, txtDefaultDomain.Text, chkAutoCreate.Checked, txtBots.Text, chkSynchronizePhoto.Checked)
                    End If
                    Configuration.ResetConfig()
                    Dim objAuthenticationController As New AuthenticationController
                    Dim statusMessage As String = objAuthenticationController.NetworkStatus
                    If statusMessage.ToLower.IndexOf("fail") > -1 Then
                        MessageCell.Controls.Add(Skins.Skin.GetModuleMessageControl("", LocalizedStatus(
                                                                                                                     statusMessage),
                                                                                                    ModuleMessage.
                                                                                                       ModuleMessageType _
                                                                                                       .RedError))
                    Else
                        MessageCell.Controls.Add(Skins.Skin.GetModuleMessageControl("", LocalizedStatus(
                                                                                                                     statusMessage),
                                                                                                    ModuleMessage.
                                                                                                       ModuleMessageType _
                                                                                                       .GreenSuccess))
                    End If
                End If
            Catch exc As Exception 'Module failed to load
                ProcessModuleLoadException(Me, exc)
            End Try
        End Sub

#End Region

#Region "Event Handlers"

        Private Sub Page_Init(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Init
            Dim objAuthenticationController As New AuthenticationController
            Dim _
                objProviderConfiguration As ProviderConfiguration =
                    ProviderConfiguration.GetProviderConfiguration(Configuration.AUTHENTICATION_KEY)
            Dim _Provider As Object
            ' Bind Authentication provider list, this allows each portal could use different provider for authentication
            For Each _Provider In objProviderConfiguration.Providers
                Dim objProvider As DictionaryEntry = CType(_Provider, DictionaryEntry)
                Dim ProviderName As String = CType(objProvider.Key, String)
                Dim ProviderType As String = CType(objProvider.Value, Provider).Type

                Me.cboProviders.Items.Add(New ListItem(ProviderName, ProviderType))
            Next

            ' Bind AuthenticationTypes list, on first configure, it could obtains only from default authentication provider
            Try
                Me.cboAuthenticationType.DataSource = objAuthenticationController.AuthenticationTypes
            Catch exc As TypeInitializationException
                _strError = Localization.GetString("AuthProviderError", Me.LocalResourceFile)
            End Try
            Me.cboAuthenticationType.DataBind()
        End Sub

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
            'Put user code to initialize the page here
            Try
                'ACD-8217
                'Test for Full Trust
                Dim permission As AspNetHostingPermissionLevel = Utilities.GetCurrentTrustLevel()
                If Not (permission = AspNetHostingPermissionLevel.Unrestricted) Then
                    Response.Redirect("~/DesktopModules/AuthenticationServices/ActiveDirectory/trusterror.htm", True)
                Else
                    ' Obtain PortalSettings from Current Context
                    Dim _portalSettings As PortalSettings = PortalController.Instance.GetCurrentPortalSettings

                    ' Reset config
                    Configuration.ResetConfig()
                    Dim config As Configuration = Configuration.GetConfig()

                    If UserInfo.Username.IndexOf("\") > 0 Then
                        Dim strDomain As String = GetUserDomainName(UserInfo.Username)
                        If strDomain.ToLower = Request.ServerVariables("SERVER_NAME").ToLower Then
                            _strError =
                                String.Format(Localization.GetString("SameDomainError", Me.LocalResourceFile),
                                               strDomain,
                                               HttpUtility.HtmlEncode(Request.ServerVariables("SERVER_NAME")))
                        End If
                    End If

                    If Not Page.IsPostBack Then

                        chkAuthentication.Checked = config.WindowsAuthentication
                        chkHidden.Checked = config.HideWindowsLogin
                        If chkHidden.Checked Then
                            chkAuthentication.Checked = True
                        End If
                        chkSynchronizeRole.Checked = config.SynchronizeRole
                        chkSynchronizePhoto.Checked = config.Photo
                        chkSynchronizePassword.Checked = config.SynchronizePassword
                        chkStripDomainName.Checked = config.StripDomainName
                        txtRootDomain.Text = config.RootDomain
                        txtUserName.Text = config.UserName
                        txtEmailDomain.Text = config.EmailDomain
                        txtAutoIP.Text = config.AutoIP
                        'ACD-5585
                        txtDefaultDomain.Text = config.DefaultDomain
                        'ACD-4259
                        chkAutoCreate.Checked = config.AutoCreateUsers
                        'WorkItems 4766 and 4077
                        txtBots.Text = config.Bots
                        If (txtBots.Text = "") Then
                            txtBots.Text = "gsa-crawler;MS Search 5.0 Robot"
                        End If

                        Me.cboAuthenticationType.Items.FindByText(config.AuthenticationType).Selected = True

                    End If

                    valConfirm.ErrorMessage = Localization.GetString("PasswordMatchFailure", Me.LocalResourceFile)

                    If String.IsNullOrEmpty(_strError) Then
                        tblSettings.Visible = True
                        pnlError.Visible = False
                    Else
                        tblSettings.Visible = False
                        pnlError.Visible = True
                        lblError.Text = _strError
                    End If
                End If
            Catch exc As Exception 'Module failed to load
                ProcessModuleLoadException(Me, exc)
            End Try
        End Sub

#End Region
    End Class
End Namespace
