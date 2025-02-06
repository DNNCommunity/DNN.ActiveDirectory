Namespace DotNetNuke.Authentication.ActiveDirectory
    Public Class ConfigInfo

        Public PortalId As Integer
        Public WindowsAuthentication As Boolean
        Public HideWindowsLogin As Boolean
        Public RootDomain As String
        Public UserName As String
        Public Password As String
        Public SynchronizeRole As Boolean 'Role membership to be synchronized (Authentication/DNN) when user logon
        Public ProviderTypeName As String
        Public AuthenticationType As String
        Public EmailDomain As String
        Public AutoIP As String
        Public AutoCreateUsers As Boolean
        Public DefaultDomain As String
        Public Bots As String
        Public Photo As Boolean
        Public EnableAutoLogin As Boolean
        Public EnableDebugMode As Boolean


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
        Public SynchronizePassword As Boolean
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
        Public StripDomainName As Boolean
        Private objSecurity As New PortalSecurity
        Sub New()

        End Sub
        Public Shared ReadOnly Property DefaultProviderTypeName() As String
            Get
                Return _
                    "DotNetNuke.Authentication.ActiveDirectory.ADSI.ADSIProvider, DotNetNuke.Authentication.ActiveDirectory"
            End Get
        End Property
        Public Shared ReadOnly Property DefaultAuthenticationType() As String
            Get
                Return "Delegation"
            End Get
        End Property

        Sub New(Cambriansettings As Dictionary(Of String, String), portalID As Integer)
            If Cambriansettings IsNot Nothing Then
                With Me
                    .PortalId = portalID
                    If Cambriansettings.ContainsKey(Configuration.AD_WINDOWSAUTHENTICATION) Then
                        .WindowsAuthentication = CType(Null.GetNull(Cambriansettings(Configuration.AD_WINDOWSAUTHENTICATION), .WindowsAuthentication), Boolean)
                    End If
                    If Cambriansettings.ContainsKey(Configuration.AD_HIDEWINDOWSLOGIN) Then
                        .HideWindowsLogin = CType(Null.GetNull(Cambriansettings(Configuration.AD_HIDEWINDOWSLOGIN), .HideWindowsLogin), Boolean)
                    End If
                    If Cambriansettings.ContainsKey(Configuration.AD_SYNCHRONIZEROLE) Then
                        .SynchronizeRole = CType(Null.GetNull(Cambriansettings(Configuration.AD_SYNCHRONIZEROLE), .SynchronizeRole), Boolean)
                    End If
                    If Cambriansettings.ContainsKey(Configuration.AD_SYNCHRONIZEPASSWORD) Then
                        .SynchronizePassword = CType(Null.GetNull(Cambriansettings(Configuration.AD_SYNCHRONIZEPASSWORD), .SynchronizePassword), Boolean)
                    End If
                    If Cambriansettings.ContainsKey(Configuration.AD_STRIPDOMAINNAME) Then
                        .StripDomainName = CType(Null.GetNull(Cambriansettings(Configuration.AD_STRIPDOMAINNAME), .StripDomainName), Boolean)
                    End If
                    If Cambriansettings.ContainsKey(Configuration.AD_ROOTDOMAIN) Then
                        .RootDomain = CType(Null.GetNull(Cambriansettings(Configuration.AD_ROOTDOMAIN), .RootDomain), String)
                    End If
                    If Cambriansettings.ContainsKey(Configuration.AD_EMAILDOMAIN) Then
                        .EmailDomain = CType(Null.GetNull(Cambriansettings(Configuration.AD_EMAILDOMAIN), .EmailDomain), String)
                    End If
                    If Cambriansettings.ContainsKey(Configuration.AD_USERNAME) Then
                        .UserName = CType(Null.GetNull(Cambriansettings(Configuration.AD_USERNAME), .UserName), String)
                    End If
                    If Cambriansettings.ContainsKey(Configuration.AD_PROVIDERTYPENAME) Then
                        .ProviderTypeName = CType(Null.GetNull(Cambriansettings(Configuration.AD_PROVIDERTYPENAME), ConfigInfo.DefaultProviderTypeName), String)
                    End If
                    If Cambriansettings.ContainsKey(Configuration.AD_AUTHENTICATIONTYPE) Then
                        .AuthenticationType = CType(Null.GetNull(Cambriansettings(Configuration.AD_AUTHENTICATIONTYPE), ConfigInfo.DefaultAuthenticationType), String)
                    End If
                    If Cambriansettings.ContainsKey(Configuration.AD_AUTHENTICATIONPASSWORD) Then
                        .Password = objSecurity.Decrypt(Configuration.AUTHENTICATION_KEY, CType(Null.GetNull(Cambriansettings(Configuration.AD_AUTHENTICATIONPASSWORD), .Password), String))
                    End If
                    If Cambriansettings.ContainsKey(Configuration.AD_SUBNET) Then
                        .AutoIP = CType(Null.GetNull(Cambriansettings(Configuration.AD_SUBNET), .AutoIP), String)
                    End If
                    If Cambriansettings.ContainsKey(Configuration.AD_AUTOCREATEUSERS) Then
                        .AutoCreateUsers = CType(Null.GetNull(Cambriansettings(Configuration.AD_AUTOCREATEUSERS), .AutoCreateUsers), Boolean)
                    End If
                    If Cambriansettings.ContainsKey(Configuration.AD_DEFAULTDOMAIN) Then
                        .DefaultDomain = CType(Null.GetNull(Cambriansettings(Configuration.AD_DEFAULTDOMAIN), .DefaultDomain), String)
                    End If
                    If Cambriansettings.ContainsKey(Configuration.AD_SEARCHBOTS) Then
                        .Bots = CType(Null.GetNull(Cambriansettings(Configuration.AD_SEARCHBOTS), .Bots), String)
                    End If
                    If Cambriansettings.ContainsKey(Configuration.AD_SYNCPHOTO) Then
                        .Photo = CType(Null.GetNull(Cambriansettings(Configuration.AD_SYNCPHOTO), .Photo), Boolean)
                    End If
                    If Cambriansettings.ContainsKey(Configuration.AD_ENABLEAUTOLOGIN) Then
                        .EnableAutoLogin = CType(Null.GetNull(Cambriansettings(Configuration.AD_ENABLEAUTOLOGIN), .EnableAutoLogin), Boolean)
                    End If
                    If Cambriansettings.ContainsKey(Configuration.AD_ENABLEDEBUGMODE) Then
                        .EnableDebugMode = CType(Null.GetNull(Cambriansettings(Configuration.AD_ENABLEDEBUGMODE), .EnableDebugMode), Boolean)
                    End If
                End With
            End If
        End Sub
    End Class
End Namespace

