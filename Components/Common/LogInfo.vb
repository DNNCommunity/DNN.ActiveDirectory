Imports DotNetNuke.Abstractions.Logging
Namespace DotNetNuke.Authentication.ActiveDirectory
    Public Class LogInfo
        Implements ILogInfo

        Public Property BypassBuffering As Boolean Implements ILogInfo.BypassBuffering

        Public Property Exception As IExceptionInfo Implements ILogInfo.Exception

        Public Property LogConfigId As String Implements ILogInfo.LogConfigId


        Public Property LogCreateDate As Date Implements ILogInfo.LogCreateDate


        Public Property LogCreateDateNum As Long Implements ILogInfo.LogCreateDateNum


        Public Property LogEventId As Integer Implements ILogInfo.LogEventId


        Public Property LogFileId As String Implements ILogInfo.LogFileId


        Public Property LogGuid As String Implements ILogInfo.LogGuid


        Public Property LogPortalId As Integer Implements ILogInfo.LogPortalId


        Public Property LogPortalName As String Implements ILogInfo.LogPortalName


        Public Property LogProperties As ILogProperties Implements ILogInfo.LogProperties


        Public Property LogServerName As String Implements ILogInfo.LogServerName


        Public Property LogTypeKey As String Implements ILogInfo.LogTypeKey


        Public Property LogUserId As Integer Implements ILogInfo.LogUserId


        Public Property LogUserName As String Implements ILogInfo.LogUserName


        Public Sub AddProperty(name As String, value As String) Implements ILogInfo.AddProperty
            Throw New NotImplementedException()
        End Sub
    End Class
End Namespace