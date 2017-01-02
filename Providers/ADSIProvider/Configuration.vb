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
Imports System.DirectoryServices
Imports System.Runtime.InteropServices
Imports DotNetNuke.Services.Exceptions
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Common.Utilities

Namespace DotNetNuke.Authentication.ActiveDirectory.ADSI

#Region "Enum"

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    '''     [tamttt]	08/01/2004	Created
    ''' </history>
    ''' -------------------------------------------------------------------
        Public Enum Path
        GC
        LDAP
        ADs
        WinNT
    End Enum

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    '''     [tamttt]	08/01/2004	Created
    ''' </history>
    ''' -------------------------------------------------------------------
        Public Enum CompareOperator As Integer
        [Is]
        [IsNot]
        [StartsWith]
        [EndsWith]
        [Present]
        [NotPresent]
    End Enum

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    '''     [tamttt]	08/01/2004	Created
    ''' </history>
    ''' -------------------------------------------------------------------
        Public Enum GroupType
        UNIVERSAL_GROUP = - 2147483640
        GLOBAL_GROUP = - 2147483646
        DOMAIN_LOCAL_GROUP = - 2147483644
    End Enum

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    '''     [tamttt]	08/01/2004	Created
    ''' </history>
    ''' -------------------------------------------------------------------
    Public Enum UserFlag
        ADS_UF_SCRIPTADS_UF_SCRIPT = 1
        '0x1 The logon script is executed. This flag does not work for the ADSI LDAP provider on either read or write operations. For the  ADSI WinNT provider, this flag is  read-only data, and it cannot be set for user objects. = 1    
        ADS_UF_ACCOUNTDISABLE = 2
        '0x2 user account is disabled.
        ADS_UF_HOMEDIR_REQUIRED = 8
        '0x8 The home directory is required.  
        ADS_UF_LOCKOUT = 16
        '0x10 The account is currently locked out.  
        ADS_UF_PASSWD_NOTREQD = 32
        '0x20 No password is required.
        ADS_UF_PASSWD_CANT_CHANGE = 64
        '0x40 The user cannot change the password. This flag can be read, but not set directly.  For more information and a code example that shows how to prevent a user from changing the password, see User Cannot Change Password. 
        ADS_UF_ENCRYPTED_TEXT_PASSWORD_ALLOWED = 128
        '0x80 The user can send an encrypted password. 
        ADS_UF_TEMP_DUPLICATE_ACCOUNT = 256
        '0x100 This is an account for users whose primary account is in another domain. This account provides user access to this domain, but not to any domain that trusts this domain. Also known as a  local user account. = 256,    
        ADS_UF_NORMAL_ACCOUNT = 512
        '0x200 This is a default account type that represents a typical user.
        ADS_UF_INTERDOMAIN_TRUST_ACCOUNT = 2048
        '0x800 This is a permit to trust account for a system domain that trusts other domains.
        ADS_UF_WORKSTATION_TRUST_ACCOUNT = 4096
        'This is a computer account for a Microsoft Windows NT Workstation/Windows 2000 Professional or Windows NT Server/Windows 2000 Server that is a member of this domain.  0x1000
        ADS_UF_SERVER_TRUST_ACCOUNT = 8192
        'This is a computer account for a system backup domain controller that is a member of this domain. 0x2000
        ADS_UF_DONT_EXPIRE_PASSWD = 65536
        '0x10000 When set, the password will not expire on this account.  
        ADS_UF_MNS_LOGON_ACCOUNT = 131072
        ' 0x20000 This is an MNS logon account. 
        ADS_UF_SMARTCARD_REQUIRED = 262144
        '0x40000 When set, this flag will force the user to log on using a smart card. 
        ADS_UF_TRUSTED_FOR_DELEGATION = 524288
        '0x80000 When set, the service account (user or computer account), under which a service runs, is trusted for Kerberos delegation. Any such service can impersonate a client requesting the service. To enable a service for Kerberos delegation, set this flag on the  userAccountControl property of the service account. 
        ADS_UF_NOT_DELEGATED = 1048576
        '0x100000 When set, the security context of the user will not be delegated to a service even if the service account is set as trusted for Kerberos delegation. 
        ADS_UF_USE_DES_KEY_ONLY = 2097152
        '0x200000 Restrict this principal to use only Data Encryption Standard (DES) encryption types for keys.Active Directory Client Extension:  Not supported.
        ADS_UF_DONT_REQUIRE_PREAUTH = 4194304
        '0x400000 This account does not require Kerberos preauthentication for logon.Active Directory Client Extension:  Not supported.
        ADS_UF_PASSWORD_EXPIRED = 8388608
        '0x800000 The user password has expired. This flag is created by the system using data from the  password last set attribute and the domain policy.  It is read-only and cannot be set. To manually set a user password as expired, use the NetUserSetInfo function with the USER_INFO_3 (usri3_password_expired member) or USER_INFO_4 (usri4_password_expired member) structure.Active Directory Client Extension:  Not supported.
        ADS_UF_TRUSTED_TO_AUTHENTICATE_FOR_DELEGATION = 16777216
        'The account is enabled for delegation. This is a security-sensitive setting; accounts with this option enabled should be strictly controlled. This setting enables a service running under the account to assume a client identity and authenticate as that user to other remote servers on the network.Active Directory Client Extension:  Not supported.
    End Enum

#End Region
    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    ''' <history>
    '''     [sawest]    12/16/2016  Added photo constant
    ''' </history>
    ''' -------------------------------------------------------------------
    Public Class Configuration
        Public Const ADSI_CONFIGURATIONNAMIMGCONTEXT As String = "configurationNamingContext"
        Public Const ADSI_DEFAULTNAMIMGCONTEXT As String = "defaultNamingContext"
        Public Const ADSI_ROOTDOMAINNAMIMGCONTEXT As String = "rootDomainNamingContext"
        Public Const ADSI_USERACCOUNTCONTROL As String = "userAccountControl"
        Public Const ADSI_CLASS As String = "objectClass"
        Public Const ADSI_CATEGORY As String = "objectCategory"
        Public Const ADSI_DC As String = "dc"
        Public Const ADSI_NCNAME As String = "nCName"
        Public Const ADSI_DNSROOT As String = "dnsRoot"
        Public Const ADSI_GROUPTYPE As String = "groupType"
        Public Const ADSI_MEMBER As String = "member"
        Public Const ADSI_CNAME As String = "cn"
        Public Const ADSI_ACCOUNTNAME As String = "sAMAccountName"
        Public Const ADSI_DISTINGUISHEDNAME As String = "distinguishedName"
        Public Const ADSI_CANONICALNAME As String = "canonicalName"
        Public Const ADSI_UPN As String = "userPrincipalName"
        Public Const ADSI_DISPLAYNAME As String = "displayName"
        Public Const ADSI_FIRSTNAME As String = "givenName"
        Public Const ADSI_LASTNAME As String = "sn"
        Public Const ADSI_STREET As String = "streetAddress"
        Public Const ADSI_CITY As String = "l"
        Public Const ADSI_POSTALCODE As String = "postalCode"
        Public Const ADSI_REGION As String = "st"
        Public Const ADSI_COUNTRY As String = "co"
        Public Const ADSI_TELEPHONE As String = "telephoneNumber"
        Public Const ADSI_FAX As String = "facsimileTelephoneNumber"
        Public Const ADSI_CELL As String = "mobile"
        Public Const ADSI_HOMEPHONE As String = "homePhone"
        Public Const ADSI_EMAIL As String = "mail"
        Public Const ADSI_WEBSITE As String = "url"
        Public Const ADSI_DESCRIPTION As String = "description"
        Public Const ADSI_EMPLOYEEID As String = "employeeID"
        Public Const ADSI_COMPANY As String = "company"
        Public Const ADSI_MANAGER As String = "manager"
        Public Const ADSI_DEPARTMENT As String = "department"
        Public Const ADSI_ASSISTANT As String = "assistant"
        Public Const ADSI_PHOTO As String = "thumbnailPhoto"

        Private Const ADSI_CONFIG_CACHE_PREFIX As String = "ADSI.Configuration"

        Private mPortalId As Integer
        Private mSettingModuleId As Integer

        ' mRootDomainPath will be stored in DC=ttt,DC=com,DC=vn format (without ADSIPath)
        ' ADSIPath to be added depends on Authentication accessing method
        Private mADSINetwork As Boolean = False
        Private mLDAPAccesible As Boolean = False
        Private mConfigDomainPath As String = ""
        ' Row value user input in site settings
        Private mDefaultEmailDomain As String = ""
        ' Row value user input in site settings - without @
        Private mRootDomainPath As String = ""
        Private mConfigurationPath As String = ""
        Private mAuthenticationType As AuthenticationTypes = AuthenticationTypes.Delegation
        Private mUserName As String = ""
        Private mPassword As String = ""
        Private mSearchPageSize As Integer = 1000
        Private mADSIPath As Path = Path.GC
        Private mProcessLog As String = ""

        ' For Domain Reference Configuration
        Private mRefCollection As CrossReferenceCollection

        ''' -------------------------------------------------------------------
        ''' <summary>
        '''     Obtain Authentication settings from database
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Sub New()
            Dim authConfig As ActiveDirectory.Configuration = ActiveDirectory.Configuration.GetConfig()
            mPortalId = authConfig.PortalId

            Try
                'Temporary fix this setting as TRUE for design, to be removed when release
                mConfigDomainPath = authConfig.RootDomain
                mDefaultEmailDomain = authConfig.EmailDomain
                mUserName = authConfig.UserName
                mPassword = authConfig.Password
                mAuthenticationType = _
                    CType ([Enum].Parse (GetType (AuthenticationTypes), authConfig.AuthenticationType), _
                        AuthenticationTypes)
                ' IMPORTANT: Remove ADSIPath, to be added later depends on accessing method

                mRootDomainPath = Utilities.ValidateDomainPath (mConfigDomainPath)
                mRootDomainPath = Right (mRootDomainPath, mRootDomainPath.Length - mRootDomainPath.IndexOf ("DC="))

            Catch exc As Exception
                mProcessLog += exc.Message & "<br>"
            End Try

            ' Also check if Authentication implemented in this Windows Network
            Dim gc As New DirectoryEntry
            Try
                If DirectoryEntry.Exists ("GC://rootDSE") Then
                    Dim rootGC As DirectoryEntry
                    'If (mUserName.Length > 0) AndAlso (mPassword.Length > 0) Then
                    'rootGC = New DirectoryEntry("GC://rootDSE", mUserName, mPassword, mAuthenticationType)
                    'Else
                        rootGC = New DirectoryEntry ("GC://rootDSE")
                    'End If
                    mConfigurationPath = rootGC.Properties (ADSI_CONFIGURATIONNAMIMGCONTEXT).Value.ToString
                    mADSINetwork = True
                End If
            Catch exc As COMException
                mADSINetwork = False
                mLDAPAccesible = False
                mProcessLog += exc.Message & "<br>"
                LogException (exc)
                ' Nothing to do if we could not access Global Catalog, so return
                'Return
            End Try

            ' Also check if LDAP fully accessible
            Dim ldap As New DirectoryEntry
            Try
                If DirectoryEntry.Exists ("LDAP://rootDSE") Then
                    mLDAPAccesible = True
                    mRefCollection = New CrossReferenceCollection (mUserName, mPassword, mAuthenticationType)
                End If
            Catch exc As COMException
                mLDAPAccesible = False
                mProcessLog += exc.Message & "<br>"
                LogException (exc)
            End Try

        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Obtain Authentication Configuration
        ''' </summary>
        ''' <remarks>
        ''' Accessing Active Directory also cost lots of resource, 
        ''' so we only do it once then save into application cache for later use
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Shared Function GetConfig() As Configuration
            Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
            Dim strKey As String = ADSI_CONFIG_CACHE_PREFIX & "." & CStr (_portalSettings.PortalId)

            Dim config As Configuration = CType (DataCache.GetCache (strKey), Configuration)
            If config Is Nothing Then
                config = New Configuration
                DataCache.SetCache (strKey, config)
            End If

            Return config

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Shared Sub ResetConfig()
            Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
            Dim strKey As String = ADSI_CONFIG_CACHE_PREFIX & "." & CStr (_portalSettings.PortalId)
            DataCache.RemoveCache (strKey)

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
        Public Sub SetSecurity (ByVal Entry As DirectoryEntry)
            Try
                Entry.AuthenticationType = mAuthenticationType
                If (mUserName.Length > 0) AndAlso (mPassword.Length > 0) Then
                    Entry.Username = mUserName
                    Entry.Password = mPassword
                End If

            Catch ex As COMException
                LogException (ex)
            End Try
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
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public ReadOnly Property RefCollection() As CrossReferenceCollection
            Get
                Return mRefCollection
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
        Public ReadOnly Property AuthenticationType() As AuthenticationTypes
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
        Public ReadOnly Property RootDomainPath() As String
            Get
                Return mRootDomainPath
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
        Public ReadOnly Property ConfigDomainPath() As String
            Get
                Return mConfigDomainPath
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
        Public ReadOnly Property ConfigurationPath() As String
            Get
                Return mConfigurationPath
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
        Public ReadOnly Property DefaultEmailDomain() As String
            Get
                Return mDefaultEmailDomain
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
        Public ReadOnly Property ADSINetwork() As Boolean
            Get
                Return mADSINetwork
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
        Public ReadOnly Property LDAPAccesible() As Boolean
            Get
                Return mLDAPAccesible
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
        Public ReadOnly Property ProcessLog() As String
            Get
                Return mProcessLog
            End Get
        End Property

        ''' -------------------------------------------------------------------
        ''' <summary>
        '''     Used to determine if a valid input is provided, if not, return default value
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Private Function GetValue (ByVal Input As Object, ByVal DefaultValue As String) As String
            If Input Is Nothing Then
                Return DefaultValue
            Else
                Return CStr (Input)
            End If
        End Function
    End Class
End Namespace
