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
        Implements IConfiguration

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


        Private portalSettings As Abstractions.Portals.IPortalSettings
        Private portalController As IPortalController
        Private providerConfiguration As ProviderConfiguration
        Private objSecurity As PortalSecurity
        Private objEventLog As Abstractions.Logging.IEventLogger

        Private ReadOnly Property CacheKey As String
            Get
                Return $"{AUTHENTICATION_CONFIG_CACHE_PREFIX}.{CStr(Me.portalSettings.PortalId)}"
            End Get
        End Property
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
        Sub New(ByVal portalController As IPortalController,
                ByVal objEventLog As Abstractions.Logging.IEventLogger)

            Me.objEventLog = objEventLog
            Me.portalController = portalController
            Me.portalSettings = Me.portalController.GetCurrentSettings
            Me.providerConfiguration = ProviderConfiguration.GetProviderConfiguration(AUTHENTICATION_KEY)
            Me.objSecurity = New PortalSecurity

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
        Public Function GetConfig() As ConfigInfo Implements IConfiguration.GetConfig
            Dim config As ConfigInfo

            Try

                config = CType(DataCache.GetCache(CacheKey), ConfigInfo)

                If config Is Nothing Then
                    config = getConfigInfo()
                    DataCache.SetCache(CacheKey, config)
                End If

            Catch exc As Exception
                ' Problems reading AD config, just return nothing
                Return Nothing
            End Try

            Return config

        End Function
        Private Function getConfigInfo() As ConfigInfo

            Dim config As ConfigInfo = Nothing
            'Dim CambrianSettings As Dictionary(Of String, String)

            Try
                If providerConfiguration.DefaultProvider IsNot Nothing Then ' No provider specified, so disable authentication feature
                    'CambrianSettings = portalController.GetPortalSettings(portalSettings.PortalId)
                    config = New ConfigInfo(portalController.GetPortalSettings(portalSettings.PortalId), portalSettings.PortalId)
                    'With config
                    '    .PortalId = portalSettings.PortalId
                    '    If CambrianSettings.ContainsKey(AD_WINDOWSAUTHENTICATION) Then
                    '        .WindowsAuthentication = CType(Null.GetNull(CambrianSettings(AD_WINDOWSAUTHENTICATION), .WindowsAuthentication), Boolean)
                    '    End If
                    '    If CambrianSettings.ContainsKey(AD_HIDEWINDOWSLOGIN) Then
                    '        .HideWindowsLogin = CType(Null.GetNull(CambrianSettings(AD_HIDEWINDOWSLOGIN), .HideWindowsLogin), Boolean)
                    '    End If
                    '    If CambrianSettings.ContainsKey(AD_SYNCHRONIZEROLE) Then
                    '        .SynchronizeRole = CType(Null.GetNull(CambrianSettings(AD_SYNCHRONIZEROLE), .SynchronizeRole), Boolean)
                    '    End If
                    '    If CambrianSettings.ContainsKey(AD_SYNCHRONIZEPASSWORD) Then
                    '        .SynchronizePassword = CType(Null.GetNull(CambrianSettings(AD_SYNCHRONIZEPASSWORD), .SynchronizePassword), Boolean)
                    '    End If
                    '    If CambrianSettings.ContainsKey(AD_STRIPDOMAINNAME) Then
                    '        .StripDomainName = CType(Null.GetNull(CambrianSettings(AD_STRIPDOMAINNAME), .StripDomainName), Boolean)
                    '    End If
                    '    If CambrianSettings.ContainsKey(AD_ROOTDOMAIN) Then
                    '        .RootDomain = CType(Null.GetNull(CambrianSettings(AD_ROOTDOMAIN), .RootDomain), String)
                    '    End If
                    '    If CambrianSettings.ContainsKey(AD_EMAILDOMAIN) Then
                    '        .EmailDomain = CType(Null.GetNull(CambrianSettings(AD_EMAILDOMAIN), getDefaultEmailDomain()), String)
                    '    End If
                    '    If CambrianSettings.ContainsKey(AD_USERNAME) Then
                    '        .UserName = CType(Null.GetNull(CambrianSettings(AD_USERNAME), .UserName), String)
                    '    End If
                    '    If CambrianSettings.ContainsKey(AD_PROVIDERTYPENAME) Then
                    '        .ProviderTypeName = CType(Null.GetNull(CambrianSettings(AD_PROVIDERTYPENAME), ConfigInfo.DefaultProviderTypeName), String)
                    '    End If
                    '    If CambrianSettings.ContainsKey(AD_AUTHENTICATIONTYPE) Then
                    '        .AuthenticationType = CType(Null.GetNull(CambrianSettings(AD_AUTHENTICATIONTYPE), ConfigInfo.DefaultAuthenticationType), String)
                    '    End If
                    '    If CambrianSettings.ContainsKey(AD_AUTHENTICATIONPASSWORD) Then
                    '        .Password = objSecurity.Decrypt(AUTHENTICATION_KEY, CType(Null.GetNull(CambrianSettings(AD_AUTHENTICATIONPASSWORD), .Password.ToString), String))
                    '    End If
                    '    If CambrianSettings.ContainsKey(AD_SUBNET) Then
                    '        .AutoIP = CType(Null.GetNull(CambrianSettings(AD_SUBNET), .AutoIP), String)
                    '    End If
                    '    If CambrianSettings.ContainsKey(AD_AUTOCREATEUSERS) Then
                    '        .AutoCreateUsers = CType(Null.GetNull(CambrianSettings(AD_AUTOCREATEUSERS), .AutoCreateUsers), Boolean)
                    '    End If
                    '    If CambrianSettings.ContainsKey(AD_DEFAULTDOMAIN) Then
                    '        .DefaultDomain = CType(Null.GetNull(CambrianSettings(AD_DEFAULTDOMAIN), .DefaultDomain), String)
                    '    End If
                    '    If CambrianSettings.ContainsKey(AD_SEARCHBOTS) Then
                    '        .Bots = CType(Null.GetNull(CambrianSettings(AD_SEARCHBOTS), .Bots), String)
                    '    End If
                    '    If CambrianSettings.ContainsKey(AD_SYNCPHOTO) Then
                    '        .Photo = CType(Null.GetNull(CambrianSettings(AD_SYNCPHOTO), .Photo), Boolean)
                    '    End If
                    '    If CambrianSettings.ContainsKey(AD_ENABLEAUTOLOGIN) Then
                    '        .EnableAutoLogin = CType(Null.GetNull(CambrianSettings(AD_ENABLEAUTOLOGIN), .EnableAutoLogin), Boolean)
                    '    End If
                    '    If CambrianSettings.ContainsKey(AD_ENABLEDEBUGMODE) Then
                    '        .EnableDebugMode = CType(Null.GetNull(CambrianSettings(AD_ENABLEDEBUGMODE), .EnableDebugMode), Boolean)
                    '    End If
                    'End With
                End If
            Catch ex As Exception
                'Log the exception
                objEventLog.AddLog("Description", "There was a problem loading the settings for the AD Authentication Provider.  Error: " & ex.Message, portalSettings, -1, Abstractions.Logging.EventLogType.ADMIN_ALERT)
            End Try
            Return config
        End Function
        Private Function getDefaultEmailDomain() As String

            Dim _portalEmail As String = portalSettings.Email
            Dim sRet As String = ""
            If Not String.IsNullOrEmpty(_portalEmail) Then
                Dim nPos As Integer = _portalEmail.IndexOf("@")
                If nPos > 0 Then
                    sRet = _portalEmail.Substring(nPos)
                End If
            End If
            Return sRet
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
        Public Sub ResetConfig() Implements IConfiguration.ResetConfig
            Dim strkey As String = $"AuthenticationProvider{CStr(portalSettings.PortalId)}"
            DataCache.RemoveCache(CacheKey)
            DataCache.RemoveCache(strkey)
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
        Public Sub UpdateConfig(config As ConfigInfo) Implements IConfiguration.UpdateConfig
            With config
                portalController.UpdatePortalSetting(.PortalId, AD_WINDOWSAUTHENTICATION, .WindowsAuthentication.ToString, True, portalSettings.CultureCode, False)
                portalController.UpdatePortalSetting(.PortalId, AD_HIDEWINDOWSLOGIN, .HideWindowsLogin.ToString, True, portalSettings.CultureCode, False)
                portalController.UpdatePortalSetting(.PortalId, AD_SYNCHRONIZEROLE, .SynchronizeRole.ToString, True, portalSettings.CultureCode, False)
                portalController.UpdatePortalSetting(.PortalId, AD_SYNCHRONIZEPASSWORD, .SynchronizePassword.ToString, True, portalSettings.CultureCode, False)
                portalController.UpdatePortalSetting(.PortalId, AD_STRIPDOMAINNAME, .StripDomainName.ToString, True, portalSettings.CultureCode, False)
                portalController.UpdatePortalSetting(.PortalId, AD_ROOTDOMAIN, If(String.IsNullOrEmpty(.RootDomain), "", .RootDomain), True, portalSettings.CultureCode, False)
                portalController.UpdatePortalSetting(.PortalId, AD_EMAILDOMAIN, If(String.IsNullOrEmpty(.EmailDomain), "", .EmailDomain), True, portalSettings.CultureCode, False)
                portalController.UpdatePortalSetting(.PortalId, AD_USERNAME, If(String.IsNullOrEmpty(.UserName), "", .UserName), True, portalSettings.CultureCode, False)
                portalController.UpdatePortalSetting(.PortalId, AD_PROVIDERTYPENAME, If(String.IsNullOrEmpty(.ProviderTypeName), "", .ProviderTypeName), True, portalSettings.CultureCode, False)
                portalController.UpdatePortalSetting(.PortalId, AD_AUTHENTICATIONTYPE, If(String.IsNullOrEmpty(.AuthenticationType), "", .AuthenticationType), True, portalSettings.CultureCode, False)
                portalController.UpdatePortalSetting(.PortalId, AD_SUBNET, If(String.IsNullOrEmpty(.AutoIP), "127.0.0.1", .AutoIP), True, portalSettings.CultureCode, False)
                portalController.UpdatePortalSetting(.PortalId, AD_DEFAULTDOMAIN, If(String.IsNullOrEmpty(.DefaultDomain), "", .DefaultDomain), True, portalSettings.CultureCode, False)
                portalController.UpdatePortalSetting(.PortalId, AD_AUTOCREATEUSERS, .AutoCreateUsers.ToString, True, portalSettings.CultureCode, False)
                portalController.UpdatePortalSetting(.PortalId, AD_SEARCHBOTS, If(String.IsNullOrEmpty(.Bots), "", .Bots), True, portalSettings.CultureCode, False)
                portalController.UpdatePortalSetting(.PortalId, AD_SYNCPHOTO, .Photo.ToString, True, portalSettings.CultureCode, False)
                portalController.UpdatePortalSetting(.PortalId, AD_ENABLEAUTOLOGIN, .EnableAutoLogin.ToString, True, portalSettings.CultureCode, False)
                portalController.UpdatePortalSetting(.PortalId, AD_ENABLEDEBUGMODE, .EnableDebugMode.ToString, True, portalSettings.CultureCode, False)

                'Only update password if it has been changed
                If .Password.Length > 0 Then
                    portalController.UpdatePortalSetting(.PortalId, AD_AUTHENTICATIONPASSWORD, Convert.ToString(objSecurity.Encrypt(AUTHENTICATION_KEY, .Password)), True, portalSettings.CultureCode, False)
                End If
            End With
        End Sub

    End Class
End Namespace
