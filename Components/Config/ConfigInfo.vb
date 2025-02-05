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

    End Class
End Namespace

