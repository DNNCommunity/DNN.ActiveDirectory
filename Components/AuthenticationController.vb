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
Imports DotNetNuke.Authentication.ActiveDirectory.ADSI
Imports DotNetNuke.Common
Imports DotNetNuke.Entities.Modules
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Security.Membership
Imports DotNetNuke.Entities.Users
Imports DotNetNuke.Common.Utilities
Imports DotNetNuke.Services.Log.EventLog
Imports System.Xml.XPath
Imports System.Xml

Imports DNNUserController = DotNetNuke.Entities.Users.UserController

Namespace DotNetNuke.Authentication.ActiveDirectory
    Public Class AuthenticationController
        Inherits UserUserControlBase
        Implements IAuthenticationController

        Private ReadOnly Property mProviderTypeName As String
            Get
                Return config.ProviderTypeName
            End Get
        End Property

        Private Shadows ReadOnly portalSettings As PortalSettings
        Private config As ConfigInfo
        Private objAuthUserController As IUserController
        Private objEventLog As Abstractions.Logging.IEventLogger
        Private Const COOKIE_PERSIST_TIMEOUT_SETTING As String = "PersistentCookieTimeout"
        Private objEventLogInfo As Abstractions.Logging.ILogInfo
        Private navigationManager As Abstractions.INavigationManager
        Private portalController As IPortalController
        Private adsiProvider As IAuthenticationProvider
        Private userController As IUserController

        Sub New(ByVal configuration As IConfiguration,
                ByVal portalController As IPortalController,
                ByVal userController As IUserController,
                ByVal eventLogger As Abstractions.Logging.IEventLogger,
                ByVal navigationManager As Abstractions.INavigationManager,
                ByVal adsiProvider As IAuthenticationProvider)
            Me.portalController = portalController
            Me.config = configuration.GetConfig()
            Me.portalSettings = Me.portalController.GetCurrentSettings
            Me.objAuthUserController = userController
            Me.objEventLog = eventLogger
            Me.navigationManager = navigationManager
            Me.adsiProvider = adsiProvider
            Me.userController = userController
        End Sub

        Public Sub AuthenticationLogon() Implements IAuthenticationController.AuthenticationLogon
            Dim objReturnUser As UserInfo
            Dim loggedOnUserName As String = HttpContext.Current.Request.ServerVariables(Configuration.LOGON_USER_VARIABLE)
            Dim loginStatus As UserLoginStatus = UserLoginStatus.LOGIN_FAILURE

            ' Get ipAddress for eventLog
            Dim ipAddress As String = ""
            If Not HttpContext.Current.Request.UserHostAddress Is Nothing Then
                ipAddress = HttpContext.Current.Request.UserHostAddress
            End If

            If (loggedOnUserName.Length > 0) Then
                Dim objUser As UserInfo
                Dim objAuthUser As ADUserInfo
                objAuthUser = objAuthUserController.GetUser(loggedOnUserName)
                objUser = DNNUserController.GetUserByName(portalSettings.PortalId, loggedOnUserName)

                objReturnUser = AuthenticateUser(objUser, objAuthUser, loginStatus)


                If Not (objReturnUser Is Nothing) Then

                    objAuthUser.LastIPAddress = ipAddress
                    UpdateDNNUser(objReturnUser, objAuthUser)

                    FormsAuthentication.SetAuthCookie(Convert.ToString(loggedOnUserName), True)

                    SetStatus(portalSettings.PortalId, AuthenticationStatus.WinLogon)

                    'check if user has supplied custom value for expiration
                    Dim persistentCookieTimeout As Integer
                    If Not Common.Utilities.Config.GetSetting(COOKIE_PERSIST_TIMEOUT_SETTING) Is Nothing Then
                        persistentCookieTimeout = Integer.Parse(Common.Utilities.Config.GetSetting(COOKIE_PERSIST_TIMEOUT_SETTING))
                        'only use if non-zero, otherwise leave as asp.net value
                        If persistentCookieTimeout <> 0 Then
                            'locate and update cookie
                            Dim authCookie As String = FormsAuthentication.FormsCookieName
                            For Each cookie As String In HttpContext.Current.Response.Cookies
                                If cookie.Equals(authCookie) Then
                                    HttpContext.Current.Response.Cookies(cookie).Expires =
                                        DateTime.Now.AddMinutes(persistentCookieTimeout)
                                End If
                            Next
                        End If
                    End If

                    objEventLogInfo = New LogInfo
                    objEventLogInfo.AddProperty("IP", ipAddress)
                    objEventLogInfo.LogPortalId = portalSettings.PortalId
                    objEventLogInfo.LogPortalName = portalSettings.PortalName
                    objEventLogInfo.LogUserId = objReturnUser.UserID
                    objEventLogInfo.LogUserName = loggedOnUserName
                    objEventLogInfo.AddProperty("WindowsAuthentication", "True")
                    objEventLogInfo.LogTypeKey = "LOGIN_SUCCESS"
                    objEventLog.AddLog(objEventLogInfo)

                    'Updated to redirect to querrystring passed in prior to authentication
                    Dim querystringparams As String = "logon=" & DateTime.Now.Ticks.ToString()
                    Dim strUrl As String = navigationManager.NavigateURL(portalSettings.ActiveTab.TabID, String.Empty, querystringparams)

                    If Not HttpContext.Current.Request.Cookies("DNNReturnTo") Is Nothing Then
                        querystringparams = HttpContext.Current.Request.Cookies("DNNReturnTo").Value
                        'ACD-8445
                        If querystringparams <> String.Empty Then
                            querystringparams = querystringparams.ToLower
                            If querystringparams.IndexOf("windowssignin.aspx") < 0 Then
                                strUrl = querystringparams
                            End If
                        End If
                    End If
                    'debug logging issue #54 steven west 2/6/2019
                    If config.EnableDebugMode Then
                        objEventLog.AddLog("Description", $"@AUTHENTICATIONLOGON:Dumping redirect url: URL:{strUrl}", portalSettings, -1, Abstractions.Logging.EventLogType.ADMIN_ALERT)
                    End If
                    HttpContext.Current.Response.Redirect(strUrl, True)
                Else
                    'added error issue #54 steven west 2/6/2019
                    LogException(New Exception($"There was an error trying to create user account: {loggedOnUserName}"))
                End If
            Else
                ' Not Windows Authentication
            End If

        End Sub

        Public Function ManualLogon(ByVal userName As String, ByVal strPassword As String,
                                     ByRef loginStatus As UserLoginStatus, ByVal ipAddress As String) As UserInfo Implements IAuthenticationController.ManualLogon
            Dim objAuthUser As ADUserInfo = ProcessFormAuthentication(userName, strPassword)
            Dim objUser As UserInfo
            Dim objReturnUser As UserInfo = Nothing

            If (userName.Length > 0) And (objAuthUser IsNot Nothing) Then
                If config.StripDomainName Then
                    userName = Utilities.TrimUserDomainName(userName)
                End If

                objAuthUser.Username = userName
                objUser = DNNUserController.GetUserByName(portalSettings.PortalId, userName)
                objReturnUser = AuthenticateUser(objUser, objAuthUser, loginStatus)

                If Not (objReturnUser Is Nothing) Then
                    objAuthUser.LastIPAddress = ipAddress
                    UpdateDNNUser(objReturnUser, objAuthUser)
                End If

            End If

            Return objReturnUser

        End Function

        'Public Function UPNManualLogon(ByVal userName As String, ByVal strPassword As String,
        '                             ByRef loginStatus As UserLoginStatus, ByVal ipAddress As String) As UserInfo

        '    Dim objAuthUser As ADUserInfo = ProcessFormAuthentication(userName, strPassword)
        '    Dim objUser As UserInfo
        '    Dim objReturnUser As New UserInfo

        '    If (userName.Length > 0) And (objAuthUser IsNot Nothing) Then
        '        If config.StripDomainName Then
        '            userName = Utilities.TrimUserDomainName(userName)
        '        End If

        '        objAuthUser.Username = userName
        '        objUser = DNNUserController.GetUserByName(portalSettings.PortalId, userName)
        '        objReturnUser = AuthenticateUser(objUser, objAuthUser, loginStatus, ipAddress)

        '        If Not (objReturnUser Is Nothing) Then
        '            objAuthUser.LastIPAddress = ipAddress
        '            UpdateDNNUser(objReturnUser, objAuthUser)
        '        End If
        '    End If

        '    Return objReturnUser

        'End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        '''    Process the authentication of the user whether they've logged in 
        '''    manually or automatically
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [mhorton]	02/19/2012	Created
        '''     [sawest]    12/16/2016  Added enable password retrieval check before getting aspnetuser password
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Function AuthenticateUser(ByVal objUser As UserInfo, ByVal objAuthUser As ADUserInfo,
                                     ByRef loginStatus As UserLoginStatus) As UserInfo Implements IAuthenticationController.AuthenticateUser
            Dim objReturnUser As UserInfo = Nothing
            Dim userExists As Boolean = objUser IsNot Nothing
            'Dim objDnnUserInfo As UserInfo



            If userExists Then
                UpdateUser(objUser, loginStatus)
            Else
                CreateUser(objAuthUser, loginStatus)
            End If

            If loginStatus = UserLoginStatus.LOGIN_SUCCESS Then
                objReturnUser = DNNUserController.GetUserByName(portalSettings.PortalId, objAuthUser.Username)
                If config.SynchronizeRole Then
                    SynchronizeRoles(objReturnUser)
                End If
            End If

            Return objReturnUser

        End Function
        Private Sub CreateUser(ByVal objAuthUser As ADUserInfo, ByRef loginstatus As UserLoginStatus)

            Dim objUser As UserInfo

            If Not config.AutoCreateUsers Then  'Only create user if Allowed to

                'User doesn't exist in this portal. Make sure user doesn't exist on any other portal
                objUser = DNNUserController.GetUserByName(objAuthUser.Username)

                If objUser Is Nothing Then 'User doesn't exist in any portal
                    objAuthUser.Membership.Password = DNNUserController.GeneratePassword()
                    'objDnnUserInfo = New UserInfo
                    'objDnnUserInfo.AffiliateID = objAuthUser.AffiliateID
                    'objDnnUserInfo.DisplayName = objAuthUser.DisplayName
                    'objDnnUserInfo.Email = objAuthUser.Email
                    'objDnnUserInfo.FirstName = objAuthUser.FirstName
                    'objDnnUserInfo.IsDeleted = objAuthUser.IsDeleted
                    'objDnnUserInfo.IsSuperUser = objAuthUser.IsSuperUser
                    'objDnnUserInfo.LastIPAddress = ipAddress
                    'objDnnUserInfo.LastName = objAuthUser.LastName
                    'objDnnUserInfo.Membership = objAuthUser.Membership
                    'objDnnUserInfo.PortalID = objAuthUser.PortalID
                    'objDnnUserInfo.Profile = objAuthUser.Profile
                    'objDnnUserInfo.Roles = objAuthUser.Roles
                    'objDnnUserInfo.Username = objAuthUser.Username
                    'CreateUser(objDnnUserInfo, loginStatus)

                Else 'user exists in another portal
                    'Issue: 36 8/16/18 Steven A West
                    'If user exists in other portal, password must match to create in different portal
                    'This will reset all instances of this user's password to the same random password across all portals
                    objAuthUser.Membership.Password = RandomizeAndNormalizePassword(objUser, objAuthUser)
                    objAuthUser.UserID = objUser.UserID
                    ' CreateUser(CType(objAuthUser, UserInfo), loginStatus)
                End If

                CreateUser(CType(objAuthUser, UserInfo), loginstatus)
            End If

        End Sub
        Private Sub UpdateUser(ByVal objUser As UserInfo, ByRef loginStatus As UserLoginStatus)

            If Not objUser.IsDeleted Then
                loginStatus = UserLoginStatus.LOGIN_SUCCESS
            Else
                If Not config.AutoCreateUsers Then 'Only create user if configured to
                    objUser.IsDeleted = False
                    objUser.Membership.IsDeleted = False
                    objUser.Membership.Password = DNNUserController.GeneratePassword()
                    DNNUserController.UpdateUser(portalSettings.PortalId, objUser)
                    CreateUser(objUser, loginStatus)
                End If
            End If

        End Sub
        ''' -------------------------------------------------------------------
        ''' <summary>
        '''      Updates the DNN profile with information pulled from the Active Directory
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [mhorton]	02/19/2012	Created
        '''     [mhorton]	02/19/2012	Fixed Item 7739 Only updates the profile if information is pulled from the Active Directory.
        '''     [sawest]    12/16/2016  Added photo item
        ''' </history>
        ''' -------------------------------------------------------------------
        Private Sub UpdateDNNUser(ByVal objReturnUser As UserInfo, ByVal objAuthUser As ADUserInfo)

            With objReturnUser
                If Not (objAuthUser.DisplayName = "") Then
                    .DisplayName = objAuthUser.DisplayName
                End If
                If Not (objAuthUser.Email = "") Then
                    .Email = objAuthUser.Email
                End If
                If Not (objAuthUser.FirstName = "") Then
                    .FirstName = objAuthUser.FirstName
                End If
                If Not (objAuthUser.LastIPAddress = "") Then
                    .LastIPAddress = objAuthUser.LastIPAddress
                End If
                If Not (objAuthUser.LastName = "") Then
                    .LastName = objAuthUser.LastName
                End If
                If Not (objAuthUser.Profile.FirstName = "") Then
                    .Profile.FirstName = objAuthUser.Profile.FirstName
                End If
                If Not (objAuthUser.Profile.LastName Is Nothing) Then
                    .Profile.LastName = objAuthUser.Profile.LastName
                End If
                If Not (objAuthUser.Profile.Street = "") Then
                    .Profile.Street = objAuthUser.Profile.Street
                End If
                If Not (objAuthUser.Profile.City = "") Then
                    .Profile.City = objAuthUser.Profile.City
                End If
                If Not (objAuthUser.Profile.Region = "") Then
                    .Profile.Region = objAuthUser.Profile.Region
                End If
                If Not (objAuthUser.Profile.PostalCode = "") Then
                    .Profile.PostalCode = objAuthUser.Profile.PostalCode
                End If
                If Not (objAuthUser.Profile.Country = "") Then
                    .Profile.Country = objAuthUser.Profile.Country
                End If
                If Not (objAuthUser.Profile.Telephone = "") Then
                    .Profile.Telephone = objAuthUser.Profile.Telephone
                End If
                If Not (objAuthUser.Profile.Fax = "") Then
                    .Profile.Fax = objAuthUser.Profile.Fax
                End If
                If Not (objAuthUser.Profile.Cell = "") Then
                    .Profile.Cell = objAuthUser.Profile.Cell
                End If
                If Not (objAuthUser.Profile.Fax = "") Then
                    .Profile.Fax = objAuthUser.Profile.Fax
                End If
                If Not (objAuthUser.Profile.Website = "") Then
                    .Profile.Website = objAuthUser.Profile.Website
                End If
                If Not (objAuthUser.Profile.Photo = "") Then
                    .Profile.Photo = objAuthUser.Profile.Photo
                End If
            End With
            objAuthUserController.UpdateDnnUser(objReturnUser)
        End Sub

        Private Sub CreateUser(ByVal objUser As UserInfo, ByRef loginStatus As UserLoginStatus)
            UpdateDisplayName(objUser)
            objUser.Membership.Approved = True

            Dim createStatus As UserCreateStatus = DNNUserController.CreateUser(objUser)

            Dim args As UserCreatedEventArgs
            If createStatus = UserCreateStatus.Success Then
                args = New UserCreatedEventArgs(objUser)
            Else ' registration error
                args = New UserCreatedEventArgs(Nothing)
            End If
            args.CreateStatus = createStatus
            OnUserCreated(args)
            OnUserCreateCompleted(args)

            'debug logging issue #54 steven west 2/6/2019
            If config.EnableDebugMode Then
                objEventLog.AddLog("Description", $"@CREATEUSER:Dumping create status: CreateStatus: {createStatus.ToString}", portalSettings, -1, Abstractions.Logging.EventLogType.ADMIN_ALERT)
            End If

            If createStatus = UserCreateStatus.Success Or createStatus = UserCreateStatus.UserAlreadyRegistered Then
                loginStatus = UserLoginStatus.LOGIN_SUCCESS
            Else
                loginStatus = UserLoginStatus.LOGIN_FAILURE
            End If

        End Sub


        'Private Function RandomizePassword(ByVal objUser As UserInfo, ByRef strPassword As String) As String

        '    Dim aspNetUser As MembershipUser = Web.Security.Membership.GetUser(objUser.Username)
        '    Dim strStoredPassword As String = ""
        '    If Web.Security.Membership.Provider.EnablePasswordRetrieval And Web.Security.Membership.Provider.PasswordFormat <> MembershipPasswordFormat.Hashed Then
        '        strStoredPassword = aspNetUser.GetPassword()
        '    End If

        '    If strStoredPassword = strPassword Or String.IsNullOrEmpty(strStoredPassword) Then
        '        Dim strRandomPassword As String = DNNUserController.GeneratePassword()
        '        DNNUserController.ChangePassword(objUser, DNNUserController.ResetPassword(objUser, "").ToString(), strRandomPassword)
        '        Return strRandomPassword
        '    Else
        '        Return strStoredPassword
        '    End If
        'End Function
        ''' -----------------------------------------------------------------------------
        ''' <summary>
        ''' RandomizeAndNormalizePassword = Creates a random password and sets all existing userobj in all portals to that password.
        ''' </summary>
        ''' <param name="objUser">DNN User Object</param>
        ''' <history>
        '''     [sawest]   8/14/2018 - Created
        ''' </history>
        ''' -----------------------------------------------------------------------------
        Private Function RandomizeAndNormalizePassword(ByVal objUser As UserInfo, ByVal objAuthUser As ADUserInfo) As String
            Dim myPortals As ArrayList = portalController.GetPortals
            Dim strRandomPassword As String = DNNUserController.GeneratePassword()
            'Dim strRandomPassword As String = Utilities.GetRandomPassword() 'switched to DNN generate password so it follows any complexity rules issue #75
            For Each prtl As Abstractions.Portals.IPortalInfo In myPortals
                objUser = DNNUserController.GetUserByName(prtl.PortalId, objAuthUser.Username)
                If objUser IsNot Nothing Then
                    DNNUserController.ChangePassword(objUser, DNNUserController.ResetPassword(objUser, "").ToString(), strRandomPassword)
                End If
            Next
            Return strRandomPassword
        End Function

        'Private Function RandomizePassword(ByVal aspNetUser As MembershipUser, ByVal objUser As UserInfo, ByRef strPassword As String) As String

        '    Dim strStoredPassword As String = ""
        '    If Web.Security.Membership.Provider.EnablePasswordRetrieval And Web.Security.Membership.Provider.PasswordFormat <> MembershipPasswordFormat.Hashed Then
        '        strStoredPassword = aspNetUser.GetPassword()
        '    End If

        '    If strStoredPassword = strPassword Or String.IsNullOrEmpty(strStoredPassword) Then
        '        Dim strRandomPassword As String = DNNUserController.GeneratePassword()
        '        DNNUserController.ResetPasswordToken(objUser, 2)
        '        DNNUserController.ChangePasswordByToken(portalSettings.PortalId, objUser.Username, strRandomPassword, objUser.PasswordResetToken.ToString)
        '        Return strRandomPassword
        '    Else
        '        Return strStoredPassword
        '    End If
        'End Function





        Public Function ProcessFormAuthentication(ByVal loggedOnUserName As String, ByVal loggedOnPassword As String) As ADUserInfo Implements IAuthenticationController.ProcessFormAuthentication
            Dim username As String
            Dim objAuthUser As ADUserInfo

            If config.WindowsAuthentication Then
                username = loggedOnUserName
                objAuthUser = objAuthUserController.GetUser(username, loggedOnPassword)
                Return objAuthUser
            End If

            Return Nothing

        End Function


        'Public Function GetDnnUser(ByVal portalId As Integer, ByVal loggedOnUserName As String) As UserInfo

        '    Dim objUser As UserInfo
        '    Dim userName As String = loggedOnUserName

        '    If config.StripDomainName Then
        '        userName = Utilities.TrimUserDomainName(userName)
        '    End If

        '    objUser = DNNUserController.GetUserByName(Null.NullInteger, userName)
        '    If Not objUser Is Nothing Then
        '        ' Check if user exists in this portal
        '        If DNNUserController.GetUserByName(portalId, userName) Is Nothing Then
        '            ' The user does not exist in this portal - add them
        '            objUser.PortalID = portalId
        '            DNNUserController.CreateUser(objUser)
        '        End If
        '        Return objUser
        '    Else
        '        ' the user does not exist
        '        Return Nothing
        '    End If

        'End Function


        Public Function AuthenticationTypes() As Array Implements IAuthenticationController.AuthenticationTypes
            Return adsiProvider.GetAuthenticationTypes
        End Function


        Public Function NetworkStatus() As String Implements IAuthenticationController.NetworkStatus
            Return adsiProvider.GetNetworkStatus()
        End Function

        Private Shared Function GetAuthCookieStr(ByVal portalID As Integer) As String
            Return $"{Configuration.AUTHENTICATION_STATUS_KEY}.{portalID.ToString}"
        End Function
        Public Shared Function GetStatus(ByVal portalId As Integer) As AuthenticationStatus
            Dim authCookies As String = GetAuthCookieStr(portalId)
            Try
                If Not HttpContext.Current.Request.Cookies(authCookies) Is Nothing Then
                    ' get Authentication from cookie
                    Dim _
                        authenticationTicket As FormsAuthenticationTicket =
                            FormsAuthentication.Decrypt(HttpContext.Current.Request.Cookies(authCookies).Value)
                    Return _
                        CType([Enum].Parse(GetType(AuthenticationStatus), authenticationTicket.UserData),
                            AuthenticationStatus)
                Else
                    Return AuthenticationStatus.Undefined
                End If
            Catch ex As Exception
            End Try
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        '''     [mhorton]	02/10/2012	Get the forms cookie timeout from the web.config - WorkItem:7620
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Shared Sub SetStatus(ByVal portalId As Integer, ByVal status As AuthenticationStatus)
            Dim authCookies As String = GetAuthCookieStr(portalId)
            Dim request As HttpRequest = HttpContext.Current.Request
            Dim response As HttpResponse = HttpContext.Current.Response
            Dim nTimeOut As Integer = GetAuthCookieTimeout()

            If nTimeOut = 0 Then
                nTimeOut = 60
            End If
            Dim _
                authenticationTicket As _
                    New FormsAuthenticationTicket(1, authCookies, DateTime.Now, DateTime.Now.AddMinutes(nTimeOut), False,
                                                   status.ToString)
            ' encrypt the ticket
            Dim strAuthentication As String = FormsAuthentication.Encrypt(authenticationTicket)

            If Not request.Cookies(authCookies) Is Nothing Then
                ' expire
                request.Cookies(authCookies).Value = Nothing
                request.Cookies(authCookies).Path = "/"
                request.Cookies(authCookies).Expires = DateTime.Now.AddYears(-1)
            End If

            response.Cookies(authCookies).Value = strAuthentication
            response.Cookies(authCookies).Path = "/"
            response.Cookies(authCookies).Expires = DateTime.Now.AddMinutes(nTimeOut)

        End Sub

        '<Obsolete("procedure obsoleted in 5.0.3 - user SynchronizeRoles(ByVal objUser As UserInfo) instead")>
        'Public Sub SynchronizeRoles(ByVal loggedOnUserName As String, ByVal intUserId As Integer)
        '    Dim objAuthUserController As New UserController
        '    Dim objAuthUser As ADUserInfo

        '    objAuthUser = objAuthUserController.GetUser(loggedOnUserName)

        '    ' user object might be in simple version in none active directory network
        '    If objAuthUser.IsNotSimplyUser Then
        '        objAuthUser.UserID = intUserId
        '        UserController.AddUserRoles(portalSettings.PortalId, objAuthUser)
        '        'User exists updating user profile
        '        objAuthUserController.UpdateDnnUser(objAuthUser)
        '    End If
        'End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <remarks>
        '''		[mhorton] Created to prevent duplicate code on role synchronization.
        ''' </remarks>
        ''' <history>
        '''     [mhorton]	12/18/2006	Created
        '''     [mhorton]	10/05/2009	Changed to use IsNotSimplyUser instead of GUID - WorkItem:2943
        '''     [mhorton]	02/09/2012	AD User losing host permissions when logging in - WorkItem:7424
        '''     [mhorton]   02/17/2012 User's profile was getting blanked when getting updated - Item 7739
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Sub SynchronizeRoles(ByVal objUser As UserInfo) Implements IAuthenticationController.SynchronizeRoles
            Dim objAuthUser As ADUserInfo

            objAuthUser = objAuthUserController.GetUser(objUser.Username)
            objAuthUser.IsSuperUser = objUser.IsSuperUser
            ' user object might be in simple version in none active directory network
            If objAuthUser.IsNotSimplyUser Then
                objAuthUser.UserID = objUser.UserID
                userController.AddUserRoles(portalSettings.PortalId, objAuthUser)
            End If
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' This functions updates the display name so that it conforms to 
        ''' Portal rules
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [mhorton]	02/05/2008	Created 
        ''' </history>
        ''' -------------------------------------------------------------------
        Private Sub UpdateDisplayName(ByVal objDnnUser As UserInfo)
            'Update DisplayName to conform to Format
            Dim setting As Object = GetSetting(portalSettings.PortalId, "Security_DisplayNameFormat")
            If (Not setting Is Nothing) AndAlso (Not String.IsNullOrEmpty(Convert.ToString(setting))) Then
                objDnnUser.UpdateDisplayName(Convert.ToString(setting))
            End If
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' This function pulls the cookie timeout from the web.config.
        ''' </summary>
        ''' <remarks>
        '''		[mhorton] Use only until core version 6.1.0 is the minimum supported version 
        ''' and then call GetAuthCookieTimeout from the core code.
        ''' </remarks>
        ''' <history>
        '''     [mhorton]	02/10/2012	Created in response to WorkItem:7620
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Shared Function GetAuthCookieTimeout() As Integer

            'First check that the script module is installed
            Dim appStatusInfo As Abstractions.Application.IApplicationStatusInfo = New Application.ApplicationStatusInfo(New Application.Application())

            Dim configDoc As XmlDocument = Common.Utilities.Config.Load(appStatusInfo)
            Dim formsNav As XPathNavigator = configDoc.CreateNavigator.SelectSingleNode("configuration/system.web/authentication/forms")

            If formsNav Is Nothing Then
                ' Check the new XPath for a wrapped system.web
                formsNav = configDoc.CreateNavigator.SelectSingleNode("configuration/location/system.web/authentication/forms")
            End If
            Return If((formsNav IsNot Nothing), XmlUtils.GetAttributeValueAsInteger(formsNav, "timeout", 30), 0)

        End Function

    End Class
End Namespace
