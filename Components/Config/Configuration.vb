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
        '
        Private Const AUTHENTICATION_CONFIG_CACHE_PREFIX As String = "Authentication.Configuration"

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
        ''' </history>
        ''' -------------------------------------------------------------------        
        Sub New()
            Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
            Dim _
                _providerConfiguration As ProviderConfiguration = _
                    ProviderConfiguration.GetProviderConfiguration (AUTHENTICATION_KEY)
            Dim objSecurity As New PortalSecurity

            Try
                If _providerConfiguration.DefaultProvider Is Nothing Then
                    ' No provider specified, so disable authentication feature
                    Return
                Else
                    mPortalId = _portalSettings.PortalId

                    Dim _
                        CambrianSettings As Dictionary(Of String, String) = _
                            PortalController.GetPortalSettingsDictionary(PortalId)
                    mWindowsAuthentication = _
                        CType(Null.GetNull(CambrianSettings("AD_WindowsAuthentication"), mWindowsAuthentication), _
                            Boolean)
                    mHideWindowsLogin = _
                        CType(Null.GetNull(CambrianSettings("AD_HideWindowsLogin"), mHideWindowsLogin), Boolean)
                    mSynchronizeRole = _
                        CType(Null.GetNull(CambrianSettings("AD_SynchronizeRole"), mSynchronizeRole), Boolean)
                    mSynchronizePassword = _
                        CType(Null.GetNull(CambrianSettings("AD_SynchronizePassword"), mSynchronizePassword), Boolean)
                    mStripDomainName = _
                        CType(Null.GetNull(CambrianSettings("AD_StripDomainName"), mStripDomainName), Boolean)
                    mRootDomain = CType(Null.GetNull(CambrianSettings("AD_RootDomain"), mRootDomain), String)
                    mEmailDomain = CType(Null.GetNull(CambrianSettings("AD_EmailDomain"), mEmailDomain), String)
                    mUserName = CType(Null.GetNull(CambrianSettings("AD_UserName"), mUserName), String)
                    mProviderTypeName = _
                        CType(Null.GetNull(CambrianSettings("AD_ProviderTypeName"), mProviderTypeName), String)
                    mAuthenticationType = _
                        CType(Null.GetNull(CambrianSettings("AD_AuthenticationType"), mAuthenticationType), String)
                    mPassword = _
                        objSecurity.Decrypt(AUTHENTICATION_KEY, _
                                             CType( _
                                                Null.GetNull(CambrianSettings("AD_AuthenticationPassword"), _
                                                              mPassword.ToString), String))
                    mAutoIP = CType(Null.GetNull(CambrianSettings("AD_SubNet"), mAutoIP), String)
                    'ACD-4259
                    mAutoCreateUsers = _
                        CType(Null.GetNull(CambrianSettings("AD_AutoCreateUsers"), mAutoCreateUsers), Boolean)
                    'ACD-5585
                    mDefaultDomain = _
                        CType(Null.GetNull(CambrianSettings("AD_DefaultDomain"), mDefaultDomain), String)
                    'WorkItems 4766 and 4077
                    mBots = _
                        CType(Null.GetNull(CambrianSettings("AD_SearchBots"), mBots), String)

                End If
            Catch ex As Exception
                ' Do nothing: we cannot access data at this time
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
                Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
                Dim strKey As String = AUTHENTICATION_CONFIG_CACHE_PREFIX & "." & CStr (_portalSettings.PortalId)

                config = CType (DataCache.GetCache (strKey), Configuration)

                If config Is Nothing Then
                    config = New Configuration
                    DataCache.SetCache (strKey, config)
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
            Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
            Dim strKey As String = AUTHENTICATION_CONFIG_CACHE_PREFIX & "." & CStr (_portalSettings.PortalId)
            DataCache.RemoveCache (strKey)
            strKey = "AuthenticationProvider" & CStr (_portalSettings.PortalId)
            DataCache.RemoveCache (strKey)
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
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Shared Sub UpdateConfig(ByVal PortalID As Integer, _
                                        ByVal WindowsAuthentication As Boolean, _
                                        ByVal Hidden As Boolean, _
                                        ByVal RootDomain As String, _
                                        ByVal EmailDomain As String, _
                                        ByVal AuthenticationUserName As String, _
                                        ByVal AuthenticationPassword As String, _
                                        ByVal SynchronizeRole As Boolean, _
                                        ByVal SynchronizePassword As Boolean, _
                                        ByVal StripDomainName As Boolean, _
                                        ByVal ProviderTypeName As String, _
                                        ByVal AuthenticationType As String, _
                                        ByVal SubNet As String, _
                                        ByVal DefaultDomain As String, _
                                        ByVal AutoCreateUsers As Boolean, _
                                        ByVal Bots As String)

            Dim objSecurity As New PortalSecurity
            'Item 8512
            PortalController.UpdatePortalSetting(PortalID, "AD_WindowsAuthentication", WindowsAuthentication.ToString)
            PortalController.UpdatePortalSetting(PortalID, "AD_HideWindowsLogin", Hidden.ToString)
            PortalController.UpdatePortalSetting(PortalID, "AD_SynchronizeRole", SynchronizeRole.ToString)
            PortalController.UpdatePortalSetting(PortalID, "AD_SynchronizePassword", SynchronizePassword.ToString)
            PortalController.UpdatePortalSetting(PortalID, "AD_StripDomainName", StripDomainName.ToString)
            If String.IsNullOrEmpty(RootDomain) Then
                RootDomain = ""
            End If
            PortalController.UpdatePortalSetting(PortalID, "AD_RootDomain", RootDomain)
            If String.IsNullOrEmpty(EmailDomain) Then
                EmailDomain = ""
            End If
            PortalController.UpdatePortalSetting(PortalID, "AD_EmailDomain", EmailDomain)
            If String.IsNullOrEmpty(AuthenticationUserName) Then
                AuthenticationUserName = ""
            End If
            PortalController.UpdatePortalSetting(PortalID, "AD_UserName", AuthenticationUserName)
            If String.IsNullOrEmpty(ProviderTypeName) Then
                ProviderTypeName = ""
            End If
            PortalController.UpdatePortalSetting(PortalID, "AD_ProviderTypeName", ProviderTypeName)
            If String.IsNullOrEmpty(AuthenticationType) Then
                AuthenticationType = ""
            End If
            PortalController.UpdatePortalSetting(PortalID, "AD_AuthenticationType", AuthenticationType)
            If String.IsNullOrEmpty(SubNet) Then
                SubNet = ""
                PortalController.UpdatePortalSetting(PortalID, "AD_SubNet", "127.0.0.1")
            End If
            PortalController.UpdatePortalSetting(PortalID, "AD_SubNet", SubNet)
            'ACD-5585
            If String.IsNullOrEmpty(DefaultDomain) Then
                DefaultDomain = ""
            End If
            PortalController.UpdatePortalSetting(PortalID, "AD_DefaultDomain", DefaultDomain)
            'ACD-4259
            PortalController.UpdatePortalSetting(PortalID, "AD_AutoCreateUsers", AutoCreateUsers.ToString)
            'WorkItems 4766 and 4077
            If String.IsNullOrEmpty(Bots) Then
                Bots = ""
            End If
            PortalController.UpdatePortalSetting(PortalID, "AD_SearchBots", Bots)

            'Only update password if it has been changed
            If AuthenticationPassword.Length > 0 Then
                PortalController.UpdatePortalSetting(PortalID, "AD_AuthenticationPassword", _
                                                      Convert.ToString( _
                                                                        objSecurity.Encrypt(AUTHENTICATION_KEY, _
                                                                                             AuthenticationPassword)))
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
                Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
                Dim _portalEmail As String = _portalSettings.Email
                Dim sRet As String = ""
                If Not String.IsNullOrEmpty (_portalEmail) Then
                    Dim nPos As Integer = _portalEmail.IndexOf ("@")
                    If nPos > 0 Then
                        sRet = _portalEmail.Substring (nPos)
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
        Private Function GetValue (ByVal Input As Object, ByVal DefaultValue As String) As String
            If Input Is Nothing Then
                Return DefaultValue
            Else
                Return CStr (Input)
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
    End Class
End Namespace
