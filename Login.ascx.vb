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


Imports DotNetNuke.Services.Authentication
Imports DotNetNuke.Security.Membership
Imports DotNetNuke.Services.Log.EventLog
Imports System.Security.Permissions
Imports DNNUserInfo = DotNetNuke.Entities.Users.UserInfo


Namespace DotNetNuke.Authentication.ActiveDirectory
    Partial Class Login
        Inherits AuthenticationLoginBase


#Region "Private Members"

        Private memberProvider As MembershipProvider = MembershipProvider.Instance()

#End Region

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' AddEventLog runs when on login failure
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [mhorton]   10/12/2009  Added writing to the eventlog on login failure. Codeplex Work Item:3050
        ''' </history>
        ''' -----------------------------------------------------------------------------

            Private Shared Sub AddEventLog (ByVal portalId As Integer, ByVal username As String, ByVal userId As Integer, _
                                            ByVal portalName As String, ByVal Ip As String, _
                                            ByVal loginStatus As UserLoginStatus)

            Dim objEventLog As New EventLogController

            ' initialize log record
            Dim objEventLogInfo As New LogInfo
            Dim objSecurity As New PortalSecurity
            objEventLogInfo.AddProperty ("IP", Ip)
            objEventLogInfo.LogPortalID = portalId
            objEventLogInfo.LogPortalName = portalName
            objEventLogInfo.LogUserName = _
                objSecurity.InputFilter (username, _
                                         PortalSecurity.FilterFlag.NoScripting Or _
                                         PortalSecurity.FilterFlag.NoAngleBrackets Or PortalSecurity.FilterFlag.NoMarkup)
            objEventLogInfo.LogUserID = userId

            ' create log record
            objEventLogInfo.LogTypeKey = loginStatus.ToString
            objEventLog.AddLog (objEventLogInfo)

        End Sub

#Region "Protected Properties"

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' Gets whether the Captcha control is used to validate the login
        ''' </summary>
        ''' <history>
        ''' 	[cnurse]	03/17/2006  Created
        '''     [cnurse]    07/03/2007  Moved from Sign.ascx.vb
        ''' </history>
        ''' -----------------------------------------------------------------------------
        Protected ReadOnly Property UseCaptcha() As Boolean
            Get
                Dim setting As Object = GetSetting (PortalId, "Security_CaptchaLogin")
                Return CType (setting, Boolean)
            End Get
        End Property

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' Returns the username entered into a standard format (DOMAIN\User) if the 
        ''' default domain set.
        ''' </summary>
        ''' <history>
        '''     [mhorton]   27/04/2009 - Created
        '''     [mhorton]   27/04/2009 - ACD-5585
        '''     [mhorton]   22/03/2011 - item-5950
        ''' </history>
        ''' -----------------------------------------------------------------------------

            Protected Property UserName() As String
            Get
                Dim config As Configuration = Configuration.GetConfig()
                'Check to see if ADAuth config has a "default domain prefix"
                Dim sDefaultDomain As String = config.DefaultDomain

                Dim theUser As String = String.Empty
                Dim strDomain As String = String.Empty

                If Not String.IsNullOrEmpty (txtUsername.Text) Then
                    If txtUsername.Text.IndexOf ("@") > - 1 Then
                        theUser = Left (txtUsername.Text, txtUsername.Text.IndexOf ("@"))
                        strDomain = Right (txtUsername.Text, Len (txtUsername.Text) - (Len (theUser) + 1)).ToUpper
                        If strDomain.Contains (sDefaultDomain) Then
                            theUser = Trim (sDefaultDomain).Replace ("\", "") & "\" & theUser
                        Else
                            theUser = txtUsername.Text
                        End If
                    Else
                        'If username doesn't contain the DOMAIN\ already and config uses Default Domain
                        'Then append default domain as prefix
                        If (Not txtUsername.Text.Contains("\")) And (sDefaultDomain <> "") Then
                            theUser = Trim(sDefaultDomain).Replace("\", "") & "\" & txtUsername.Text
                        Else
                            If txtUsername.Text.IndexOf("\") > -1 Then
                                strDomain = Left(txtUsername.Text, txtUsername.Text.IndexOf("\")).ToUpper
                                theUser = Right(txtUsername.Text, Len(txtUsername.Text) - (Len(strDomain) + 1))
                                theUser = strDomain & "\" & theUser
                            Else
                                theUser = txtUsername.Text
                            End If
                        End If
                    End If
                End If

                Return theUser
            End Get
            Set (ByVal value As String)
                txtUsername.Text = value
            End Set
        End Property

#End Region

#Region "Public Properties"

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' Check if the Auth System is Enabled (for the Portal)
        ''' </summary>
        ''' <remarks></remarks>
        ''' <history>
        ''' 	[cnurse]	07/04/2007	Created
        ''' </history>
        ''' -----------------------------------------------------------------------------
        Public Overrides ReadOnly Property Enabled() As Boolean
            Get
                Try
                    'Make sure app is running at full trust
                    Dim HostingPermissions As New AspNetHostingPermission (PermissionState.Unrestricted)
                    HostingPermissions.Demand()

                    'Check if Windows Auth is enabled for the portal
                    Return Configuration.GetConfig().WindowsAuthentication
                Catch ex As Exception
                    Return False
                End Try
            End Get
        End Property

#End Region

#Region "Event Handlers"

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' Page_Load runs when the control is loaded
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        ''' 	[cnurse]	9/8/2004	Updated to reflect design changes for Help, 508 support
        '''                       and localisation
        '''     [mhorton]   07/30/2007  Cleaned out unneeded legacy code
        ''' </history>
        ''' -----------------------------------------------------------------------------
        Private Sub Page_Load (ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
            If Not Request.IsAuthenticated Then
                If Page.IsPostBack = False Then
                    Try
                        If Not Request.QueryString ("username") Is Nothing Then
                            txtUsername.Text = Request.QueryString ("username")
                        End If
                    Catch
                        'control not there 
                    End Try
                End If

                txtPassword.Attributes.Add ("value", txtPassword.Text)

                Try
                    If String.IsNullOrEmpty (txtUsername.Text) Then
                        SetFormFocus (txtUsername)
                    Else
                        SetFormFocus (txtPassword)
                    End If
                Catch
                    'Not sure why this Try/Catch may be necessary, logic was there in old setFormFocus location stating the following
                    'control not there or error setting focus
                End Try
            End If

            divCaptcha1.Visible = UseCaptcha
            divCaptcha2.Visible = UseCaptcha

            If UseCaptcha Then
                ctlCaptcha.ErrorMessage = Localization.GetString ("InvalidCaptcha", Localization.SharedResourceFile)
                ctlCaptcha.Text = Localization.GetString ("CaptchaText", Localization.SharedResourceFile)
            End If

        End Sub

        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' cmdLogin_Click runs when the login button is clicked
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        ''' 	[cnurse]	9/24/2004	Updated to reflect design changes for Help, 508 support
        '''                       and localisation
        '''     [cnurse]    12/11/2005  Updated to reflect abstraction of Membership
        '''     [cnurse]    07/03/2007  Moved from Sign.ascx.vb
        '''     [mhorton]   07/30/2007  Cleaned out unneeded legacy code
        '''     [mhorton]   10/12/2009  Added writing to the eventlog on login failure. Codeplex Work Item:3050
        ''' </history>
        ''' -----------------------------------------------------------------------------
        Private Sub cmdLogin_Click (ByVal sender As Object, ByVal e As EventArgs) Handles cmdLogin.Click
            If (UseCaptcha And ctlCaptcha.IsValid) OrElse (Not UseCaptcha) Then

                Dim loginStatus As UserLoginStatus = UserLoginStatus.LOGIN_FAILURE
                Dim objAuthentication As New AuthenticationController
                Dim objUser As DNNUserInfo = Nothing
                If UserName.Contains("\") Then
                    objUser = objAuthentication.ManualLogon(UserName, txtPassword.Text, loginStatus, IPAddress)
                End If
                Dim authenticated As Boolean = Null.NullBoolean
                Dim message As String = Null.NullString
                authenticated = (loginStatus <> UserLoginStatus.LOGIN_FAILURE)

                'If objUser is nothing then there must've been a problem logging in. Write to the eventlog.
                If objUser Is Nothing Then
                    AddEventLog(PortalId, UserName, Null.NullInteger, PortalSettings.PortalName, IPAddress, loginStatus)
                End If
                'Raise UserAuthenticated Event
                Dim _
                    eventArgs As UserAuthenticatedEventArgs = _
                        New UserAuthenticatedEventArgs(objUser, txtUsername.Text, loginStatus, "Active Directory")
                eventArgs.Authenticated = authenticated
                eventArgs.Message = message
                OnUserAuthenticated(eventArgs)
            End If

        End Sub

#End Region
    End Class
End Namespace
