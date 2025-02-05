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
Namespace DotNetNuke.Authentication.ActiveDirectory
    Public Interface IConfiguration
        ReadOnly Property AuthenticationType As String
        ReadOnly Property AutoCreateUsers As String
        ReadOnly Property AutoIP As String
        ReadOnly Property Bots As String
        ReadOnly Property DefaultDomain As String
        ReadOnly Property EmailDomain As String
        ReadOnly Property EnableAutoLogin As String
        ReadOnly Property EnableDebugMode As String
        ReadOnly Property HideWindowsLogin As Boolean
        ReadOnly Property Password As String
        ReadOnly Property Photo As String
        ReadOnly Property PortalId As Integer
        ReadOnly Property ProviderTypeName As String
        ReadOnly Property RootDomain As String
        ReadOnly Property StripDomainName As Boolean
        ReadOnly Property SynchronizePassword As Boolean
        ReadOnly Property SynchronizeRole As Boolean
        ReadOnly Property UserName As String
        ReadOnly Property WindowsAuthentication As Boolean
    End Interface
End Namespace
