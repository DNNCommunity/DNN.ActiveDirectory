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
Imports DotNetNuke.Framework

Namespace DotNetNuke.Authentication.ActiveDirectory
    Public MustInherit Class AuthenticationProvider

#Region "Shared/Static Methods"

        ' singleton reference to the instantiated object 
        Private Shared objProvider As AuthenticationProvider = Nothing

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Shared Sub New()
            Dim _portalSettings As PortalSettings = PortalController.Instance.GetCurrentPortalSettings
            Dim _config As Configuration = Configuration.GetConfig()
            Dim strKey As String = "AuthenticationProvider" & _portalSettings.PortalId.ToString

            objProvider = CType (Reflection.CreateObject (_config.ProviderTypeName, strKey), AuthenticationProvider)

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
        Public Shared Shadows Function Instance (ByVal AuthenticationTypeName As String) As AuthenticationProvider
            'CreateProvider()
            Dim _portalSettings As PortalSettings = PortalController.Instance.GetCurrentPortalSettings
            Dim strKey As String = "AuthenticationProvider" & _portalSettings.PortalId.ToString
            objProvider = CType (Reflection.CreateObject (AuthenticationTypeName, strKey), AuthenticationProvider)
            Return objProvider
        End Function

#End Region

#Region "Abstract Methods"

        Public MustOverride Overloads Function GetUser (ByVal LoggedOnUserName As String, _
                                                        ByVal LoggedOnPassword As String) As ADUserInfo

        Public MustOverride Overloads Function GetUser (ByVal LoggedOnUserName As String) As ADUserInfo
        Public MustOverride Function GetGroups() As ArrayList
        Public MustOverride Function GetGroups(ByVal arrUserPortalRoles As ArrayList) As ArrayList
        Public MustOverride Function GetAuthenticationTypes() As Array
        Public MustOverride Function GetNetworkStatus() As String

#End Region
    End Class
End Namespace