Imports System.IO
Namespace DotNetNuke.Authentication.ActiveDirectory.ADSI
    Public Class ConfigInfo
        Public PortalId As Integer
        Public SettingModuleId As Integer
        Public ADSINetwork As Boolean
        Public LDAPAccesible As Boolean
        Public ConfigDomainPath As String
        Public DefaultEmailDomain As String
        Public RootDomainPath As String
        Public ConfigurationPath As String
        Public AuthenticationType As DirectoryServices.AuthenticationTypes = DirectoryServices.AuthenticationTypes.Delegation
        Public UserName As String
        Public Password As String
        Public SearchPageSize As Integer = 1000
        Public ADSIPath As Configuration.Path = Configuration.Path.GC
        Public ProcessLog As String
        Public RefCollection As CrossReferenceCollection
    End Class
End Namespace

