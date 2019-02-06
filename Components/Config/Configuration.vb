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
Imports DotNetNuke.Framework.Providers
Imports DotNetNuke.Security
Imports DotNetNuke.Common.Utilities

Namespace DotNetNuke.Authentication.ActiveDirectory
    Public Class Configuration

        Public Const AUTHENTICATION_PATH As String = "/DesktopModules/AuthenticationServices/ActiveDirectory/"
        Public Const AUTHENTICATION_LOGON_PAGE As String = "WindowsSignin.aspx"
        Public Const AUTHENTICATION_LOGOFF_PAGE As String = "Logoff.aspx"
        Public Const AUTHENTICATION_KEY As String = "authentication"
        Public Const AUTHENTICATION_STATUS_KEY As String = "authentication.status"
        Public Const LOGON_USER_VARIABLE As String = "LOGON_USER"
        Private Const AUTHENTICATION_CONFIG_CACHE_PREFIX As String = "Authentication.Configuration"

        'Setting Name Constants
        Public Const AD_WINDOWSAUTHENTICATION As String = "AD_WindowsAuthentication"
        Public Const AD_HIDEWINDOWSLOGIN As String = "AD_HideWindowsLogin"
        Public Const AD_SYNCHRONIZEROLE As String = "AD_SynchronizeRole"
        Public Const AD_SYNCHRONIZEPASSWORD As String = "AD_SynchronizePassword"
        Public Const AD_STRIPDOMAINNAME As String = "AD_StripDomainName"
        Public Const AD_ROOTDOMAIN As String = "AD_RootDomain"
        Public Const AD_EMAILDOMAIN As String = "AD_EmailDomain"
        Public Const AD_USERNAME As String = "AD_UserName"
        Public Const AD_PROVIDERTYPENAME As String = "AD_ProviderTypeName"
        Public Const AD_AUTHENTICATIONTYPE As String = "AD_AuthenticationType"
        Public Const AD_AUTHENTICATIONPASSWORD As String = "AD_AuthenticationPassword"
        Public Const AD_SUBNET As String = "AD_SubNet"
        Public Const AD_AUTOCREATEUSERS As String = "AD_AutoCreateUsers"
        Public Const AD_DEFAULTDOMAIN As String = "AD_DefaultDomain"
        Public Const AD_SEARCHBOTS As String = "AD_SearchBots"
        Public Const AD_SYNCPHOTO As String = "AD_SyncPhoto"
        Public Const AD_ENABLEAUTOLOGIN As String = "AD_ENABLEAUTOLOGIN"
        Public Const AD_ENABLEDEBUGMODE As String = "AD_ENABLEDEBUGMODE"


        Private mPortalId As Integer
        Private mWindowsAuthentication As Boolean = False
        Private mHideWindowsLogin As Boolean = False
        Private mRootDomain As String = ""
        Private mUserName As String = ""
        Private mPassword As String = ""
        Private mSynchronizeRole As Boolean = False
        Private mSynchronizePassword As Boolean = False
        ' reserve for future feature
        Private mStripDomainName As Boolean = False
        Private mProviderTypeName As String = DefaultProviderTypeName
        Private mAuthenticationType As String = DefaultAuthenticationType
        Private mEmailDomain As String = DefaultEmailDomain
        Private mAutoIP As String = ""
        'ACD-4259
        Private mAutoCreateUsers As Boolean = False
        'ACD-5585
        Private mDefaultDomain As String = ""
        'WorkItems 4766 and 4077
        Private mBots As String = ""
        Private mPhoto As Boolean = False
        Private mEnableAutoLogin As Boolean = False
        Private mEnableDebugMode As Boolean = False

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' - Obtain Authentication settings from database
        ''' </summary>
        ''' <remarks>
        ''' - Setting records are stored in ModuleSettings table, separately for each portal,
        ''' this method allows each portal could have different accessing method to Windows Active Directory
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        '''     [cnurse]    07/04/2007  Moved settings to Authentication Module from Site Settings
        '''     [mhorton]     01/02/2008  Move mPortalID inside of the Try/Catch in case PortalSettings hasn't been initialized.
        '''     [mhorton]     06/15/2008  ACD-7913
        '''     [sawest]    12/16/2016  Added if contain statements.  If a key was missing, an error was thrown in the try block and the rest of the settings did not load.  
        '''     [sawest]    12/16/2016  Switched to using constants for setting names
        '''     [sawest]    01/02/2017  Added photo setting and constant
        '''     [sawest]    02/06/2019  Added debug mode setting and constant
        ''' </history>
        ''' -------------------------------------------------------------------        
        Sub New()
            Dim _portalSettings As PortalSettings = PortalController.Instance.GetCurrentPortalSettings
            Dim _providerConfiguration As ProviderConfiguration = ProviderConfiguration.GetProviderConfiguration(AUTHENTICATION_KEY)
            Dim objSecurity As New PortalSecurity

            Try
                If _providerConfiguration.DefaultProvider Is Nothing Then
                    ' No provider specified, so disable authentication feature
                    Return
                Else
                    mPortalId = _portalSettings.PortalId
                    Dim CambrianSettings As Dictionary(Of String, String) = PortalController.Instance.GetPortalSettings(PortalId)
                    If CambrianSettings.ContainsKey(AD_WINDOWSAUTHENTICATION) Then
                        mWindowsAuthentication = CType(Null.GetNull(CambrianSettings(AD_WINDOWSAUTHENTICATION), mWindowsAuthentication), Boolean)
                    End If
                    If CambrianSettings.ContainsKey(AD_HIDEWINDOWSLOGIN) Then
                        mHideWindowsLogin = CType(Null.GetNull(CambrianSettings(AD_HIDEWINDOWSLOGIN), mHideWindowsLogin), Boolean)
                    End If
                    If CambrianSettings.ContainsKey(AD_SYNCHRONIZEROLE) Then
                        mSynchronizeRole = CType(Null.GetNull(CambrianSettings(AD_SYNCHRONIZEROLE), mSynchronizeRole), Boolean)
                    End If
                    If CambrianSettings.ContainsKey(AD_SYNCHRONIZEPASSWORD) Then
                        mSynchronizePassword = CType(Null.GetNull(CambrianSettings(AD_SYNCHRONIZEPASSWORD), mSynchronizePassword), Boolean)
                    End If
                    If CambrianSettings.ContainsKey(AD_STRIPDOMAINNAME) Then
                        mStripDomainName = CType(Null.GetNull(CambrianSettings(AD_STRIPDOMAINNAME), mStripDomainName), Boolean)
                    End If
                    If CambrianSettings.ContainsKey(AD_ROOTDOMAIN) Then
                        mRootDomain = CType(Null.GetNull(CambrianSettings(AD_ROOTDOMAIN), mRootDomain), String)
                    End If
                    If CambrianSettings.ContainsKey(AD_EMAILDOMAIN) Then
                        mEmailDomain = CType(Null.GetNull(CambrianSettings(AD_EMAILDOMAIN), mEmailDomain), String)
                    End If
                    If CambrianSettings.ContainsKey(AD_USERNAME) Then
                        mUserName = CType(Null.GetNull(CambrianSettings(AD_USERNAME), mUserName), String)
                    End If
                    If CambrianSettings.ContainsKey(AD_PROVIDERTYPENAME) Then
                        mProviderTypeName = CType(Null.GetNull(CambrianSettings(AD_PROVIDERTYPENAME), mProviderTypeName), String)
                    End If
                    If CambrianSettings.ContainsKey(AD_AUTHENTICATIONTYPE) Then
                        mAuthenticationType = CType(Null.GetNull(CambrianSettings(AD_AUTHENTICATIONTYPE), mAuthenticationType), String)
                    End If
                    If CambrianSettings.ContainsKey(AD_AUTHENTICATIONPASSWORD) Then
                        mPassword = objSecurity.Decrypt(AUTHENTICATION_KEY, CType(Null.GetNull(CambrianSettings(AD_AUTHENTICATIONPASSWORD), mPassword.ToString), String))
                    End If
                    If CambrianSettings.ContainsKey(AD_SUBNET) Then
                        mAutoIP = CType(Null.GetNull(CambrianSettings(AD_SUBNET), mAutoIP), String)
                    End If
                    If CambrianSettings.ContainsKey(AD_AUTOCREATEUSERS) Then
                        'ACD-4259
                        mAutoCreateUsers = CType(Null.GetNull(CambrianSettings(AD_AUTOCREATEUSERS), mAutoCreateUsers), Boolean)
                    End If
                    If CambrianSettings.ContainsKey(AD_DEFAULTDOMAIN) Then
                        'ACD-5585
                        mDefaultDomain = CType(Null.GetNull(CambrianSettings(AD_DEFAULTDOMAIN), mDefaultDomain), String)
                    End If
                    If CambrianSettings.ContainsKey(AD_SEARCHBOTS) Then
                        'WorkItems 4766 and 4077
                        mBots = CType(Null.GetNull(CambrianSettings(AD_SEARCHBOTS), mBots), String)
                    End If
                    If CambrianSettings.ContainsKey(AD_SYNCPHOTO) Then
                        mPhoto = CType(Null.GetNull(CambrianSettings(AD_SYNCPHOTO), mPhoto), Boolean)
                    End If
                    If CambrianSettings.ContainsKey(AD_ENABLEAUTOLOGIN) Then
                        mEnableAutoLogin = CType(Null.GetNull(CambrianSettings(AD_ENABLEAUTOLOGIN), mEnableAutoLogin), Boolean)
                    End If
                    If CambrianSettings.ContainsKey(AD_ENABLEDEBUGMODE) Then
                        mEnableDebugMode = CType(Null.GetNull(CambrianSettings(AD_ENABLEDEBUGMODE), mEnableDebugMode), Boolean)
                    End If
                End If
            Catch ex As Exception
                'Log the exception
                ADSI.Utilities.AddEventLog(_portalSettings, "There was a problem loading the settings for the AD Authentication Provider.  Error: " & ex.Message)
            End Try

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Obtain Authentication Configuration
        ''' </summary>
        ''' <remarks>
        ''' Accessing Active Directory also costs resource, 
        ''' so we only do it once then save into cache for later use
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        '''     [mhorton] 01/02/2008 Added Try/Catch in case PortalsSettings haven't 
        '''             been initialized yet.
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Shared Function GetConfig() As Configuration
            Dim config As Configuration = Nothing
            Try
                Dim _portalSettings As PortalSettings = PortalController.Instance.GetCurrentPortalSettings
                Dim strKey As String = AUTHENTICATION_CONFIG_CACHE_PREFIX & "." & CStr(_portalSettings.PortalId)

                config = CType(DataCache.GetCache(strKey), Configuration)

                If config Is Nothing Then
                    config = New Configuration
                    DataCache.SetCache(strKey, config)
                End If

            Catch exc As Exception
                ' Problems reading AD config, just return nothing
            End Try

            Return config

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        '''     [mhorton]     15/10/2007  -Fixed ACD-3084
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Shared Sub ResetConfig()
            Dim _portalSettings As PortalSettings = PortalController.Instance.GetCurrentPortalSettings
            Dim strKey As String = AUTHENTICATION_CONFIG_CACHE_PREFIX & "." & CStr(_portalSettings.PortalId)
            DataCache.RemoveCache(strKey)
            strKey = "AuthenticationProvider" & CStr(_portalSettings.PortalId)
            DataCache.RemoveCache(strKey)
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        '''     [mhorton]     06/15/2008  ACD-7913
        '''     [mhorton]     04/14/2013 Item 8473 Null values not saved in DNN7. Use blank string
        '''     [mhorton]     04/18/2013 Item 8512 Null values not saved in DNN7. Use blank string
        '''     [sawest]    12/16/2016 Condensed some code.  Switched to using constants for setting names
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Shared Sub UpdateConfig(ByVal PortalID As Integer,
                                        ByVal WindowsAuthentication As Boolean,
                                        ByVal Hidden As Boolean,
                                        ByVal RootDomain As String,
                                        ByVal EmailDomain As String,
                                        ByVal AuthenticationUserName As String,
                                        ByVal AuthenticationPassword As String,
                                        ByVal SynchronizeRole As Boolean,
                                        ByVal SynchronizePassword As Boolean,
                                        ByVal StripDomainName As Boolean,
                                        ByVal ProviderTypeName As String,
                                        ByVal AuthenticationType As String,
                                        ByVal SubNet As String,
                                        ByVal DefaultDomain As String,
                                        ByVal AutoCreateUsers As Boolean,
                                        ByVal Bots As String,
                                       ByVal Photo As Boolean,
                                       ByVal EnableAutoLogin As Boolean,
                                       ByVal EnableDebugMode As Boolean)

            Dim objSecurity As New PortalSecurity
            'Item 8512
            PortalController.UpdatePortalSetting(PortalID, AD_WINDOWSAUTHENTICATION, WindowsAuthentication.ToString)
            PortalController.UpdatePortalSetting(PortalID, AD_HIDEWINDOWSLOGIN, Hidden.ToString)
            PortalController.UpdatePortalSetting(PortalID, AD_SYNCHRONIZEROLE, SynchronizeRole.ToString)
            PortalController.UpdatePortalSetting(PortalID, AD_SYNCHRONIZEPASSWORD, SynchronizePassword.ToString)
            PortalController.UpdatePortalSetting(PortalID, AD_STRIPDOMAINNAME, StripDomainName.ToString)
            PortalController.UpdatePortalSetting(PortalID, AD_ROOTDOMAIN, If(String.IsNullOrEmpty(RootDomain), "", RootDomain))
            PortalController.UpdatePortalSetting(PortalID, AD_EMAILDOMAIN, If(String.IsNullOrEmpty(EmailDomain), "", EmailDomain))
            PortalController.UpdatePortalSetting(PortalID, AD_USERNAME, If(String.IsNullOrEmpty(AuthenticationUserName), "", AuthenticationUserName))
            PortalController.UpdatePortalSetting(PortalID, AD_PROVIDERTYPENAME, If(String.IsNullOrEmpty(ProviderTypeName), "", ProviderTypeName))
            PortalController.UpdatePortalSetting(PortalID, AD_AUTHENTICATIONTYPE, If(String.IsNullOrEmpty(AuthenticationType), "", AuthenticationType))
            PortalController.UpdatePortalSetting(PortalID, AD_SUBNET, If(String.IsNullOrEmpty(SubNet), "127.0.0.1", SubNet))
            'ACD-5585
            PortalController.UpdatePortalSetting(PortalID, AD_DEFAULTDOMAIN, If(String.IsNullOrEmpty(DefaultDomain), "", DefaultDomain))
            'ACD-4259
            PortalController.UpdatePortalSetting(PortalID, AD_AUTOCREATEUSERS, AutoCreateUsers.ToString)
            'WorkItems 4766 and 4077
            PortalController.UpdatePortalSetting(PortalID, AD_SEARCHBOTS, If(String.IsNullOrEmpty(Bots), "", Bots))
            PortalController.UpdatePortalSetting(PortalID, AD_SYNCPHOTO, Photo.ToString)
            PortalController.UpdatePortalSetting(PortalID, AD_ENABLEAUTOLOGIN, EnableAutoLogin.ToString)
            PortalController.UpdatePortalSetting(PortalID, AD_ENABLEDEBUGMODE, EnableDebugMode.ToString)
            'Only update password if it has been changed
            If AuthenticationPassword.Length > 0 Then
                PortalController.UpdatePortalSetting(PortalID, AD_AUTHENTICATIONPASSWORD, Convert.ToString(objSecurity.Encrypt(AUTHENTICATION_KEY, AuthenticationPassword)))
            End If

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Shared ReadOnly Property DefaultProviderTypeName() As String
            Get
                Return _
                    "DotNetNuke.Authentication.ActiveDirectory.ADSI.ADSIProvider, DotNetNuke.Authentication.ActiveDirectory"
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Shared ReadOnly Property DefaultAuthenticationType() As String
            Get
                Return "Delegation"
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Shared ReadOnly Property DefaultEmailDomain() As String
            Get
                Dim _portalSettings As PortalSettings = PortalController.Instance.GetCurrentPortalSettings
                Dim _portalEmail As String = _portalSettings.Email
                Dim sRet As String = ""
                If Not String.IsNullOrEmpty(_portalEmail) Then
                    Dim nPos As Integer = _portalEmail.IndexOf("@")
                    If nPos > 0 Then
                        sRet = _portalEmail.Substring(nPos)
                    End If
                End If
                Return sRet
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property WindowsAuthentication() As Boolean
            Get
                Return mWindowsAuthentication
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [mhorton]	12/10/2007	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property HideWindowsLogin() As Boolean
            Get
                Return mHideWindowsLogin
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property RootDomain() As String
            Get
                Return mRootDomain
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property UserName() As String
            Get
                Return mUserName
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Password() As String
            Get
                Return mPassword
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Role membership to be synchronized (Authentication/DNN) when user logon
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property SynchronizeRole() As Boolean
            Get
                Return mSynchronizeRole
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        '''     Process checking DNN password against Windows password
        '''     update DNN password if not match
        '''     requires modified signin page for functionality
        ''' </summary>
        ''' <remarks>
        '''     This process quite expensive in terms of performance
        '''     Reserve for future implementation
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2005	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property SynchronizePassword() As Boolean
            Get
                Return mSynchronizePassword
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        '''     Determin if Domain Name should be stripped from UserName
        ''' </summary>
        ''' <remarks>
        '''     In an environment with single domain or unique username across domains
        ''' </remarks>
        ''' <history>
        '''     [jhoskins]	10/10/2007	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property StripDomainName() As Boolean
            Get
                Return mStripDomainName
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property PortalId() As Integer
            Get
                Return mPortalId
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property ProviderTypeName() As String
            Get
                Return mProviderTypeName
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        '''     It was configured in web.config, move to site settings is more flexible
        '''     When configure first time, only default provider (ADs) available to provide list of type to select 
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2005	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property AuthenticationType() As String
            Get
                Return mAuthenticationType
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property EmailDomain() As String
            Get
                Return mEmailDomain
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Used to determine if a valid input is provided, if not, return default value
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        '''     [tamttt]    08/20/2005  Replace by core Null.GetNull function
        ''' </history>
        ''' -------------------------------------------------------------------
        Private Function GetValue(ByVal Input As Object, ByVal DefaultValue As String) As String
            If Input Is Nothing Then
                Return DefaultValue
            Else
                Return CStr(Input)
            End If
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Returns list of search crawlers/bots that can index the site - WorkItems 4766 and 4077
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [mhorton]	17/04/2011	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Bots() As String
            Get
                Return mBots
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [mhorton]	17/10/2007	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property AutoIP() As String
            Get
                Return mAutoIP
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [mhorton]	4/05/2009   created based on code supplied by
        '''                  Nathan Truhan - ACD-4259
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property AutoCreateUsers() As String
            Get
                Return mAutoCreateUsers
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [mhorton]	27/04/2009	Created ACD-5585
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property DefaultDomain() As String
            Get
                Return mDefaultDomain
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [sawest]	01/02/2017	Created 
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property Photo() As String
            Get
                Return mPhoto
            End Get
        End Property
        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [sawest]	01/02/2017	Created 
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property EnableAutoLogin() As String
            Get
                Return mEnableAutoLogin
            End Get
        End Property
        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [sawest]	02/06/2019	Created 
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property EnableDebugMode() As String
            Get
                Return mEnableDebugMode
            End Get
        End Property




    End Class


End Namespace
