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

Namespace DotNetNuke.Authentication.ActiveDirectory.ADSI
    Public Interface IUtilities
        Function CanonicalToNetBIOS(CanonicalName As String) As String
        Function GetADGroups(Name As String) As ArrayList
        Function GetAllGroupnames() As ArrayList
        Function GetDirectoryEntry(Path As String) As DirectoryEntry
        Function GetDomainByBIOSName(Name As String) As Domain
        Function GetGroupEntriesByName(GroupName As String) As ArrayList
        Function GetRootDomain() As Domain
        Function GetRootDomain(ADSIPath As Configuration.Path) As Domain
        Function GetRootEntry() As DirectoryEntry
        Function GetRootEntry(ADSIPath As Configuration.Path) As DirectoryEntry
        Function GetUserEntryByName(Name As String) As DirectoryEntry
        Function UPNToLogonName0(UserPrincipalName As String) As String
    End Interface
End Namespace
