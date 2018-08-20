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
Imports System.Security.Principal
Imports System.Runtime.InteropServices
Imports DotNetNuke.Services.Exceptions
Imports System.Net
Imports SecurityException = System.Security.SecurityException
Imports DotNetNuke.Services.Log.EventLog
Imports DotNetNuke.Services.FileSystem
Imports DotNetNuke.Entities.Users

Namespace DotNetNuke.Authentication.ActiveDirectory.ADSI
    Public Class Utilities
        Public Shared objEventLog As New EventLogController
        Public Const AD_IMAGE_FOLDER_PATH As String = "Images/AD Photos"
        Sub New()
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
        Public Overloads Shared Function GetRootDomain(ByVal ADSIPath As Path) As Domain
            Try
                Dim adsiConfig As Configuration = Configuration.GetConfig()

                Dim rootDomainFullPath As String = AddADSIPath(adsiConfig.RootDomainPath, ADSIPath)
                Dim _
                    rootDomainEntry As Domain =
                        Domain.GetDomain(rootDomainFullPath, adsiConfig.UserName, adsiConfig.Password,
                                          adsiConfig.AuthenticationType)
                Return rootDomainEntry
            Catch exc As COMException
                LogException(exc)
                Return Nothing
            End Try

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Overloads Shared Function GetRootDomain() As Domain
            Try
                Dim adsiConfig As Configuration = Configuration.GetConfig()

                Dim rootDomainFullPath As String = AddADSIPath(adsiConfig.RootDomainPath)
                Dim _
                    rootDomainEntry As Domain =
                        Domain.GetDomain(rootDomainFullPath, adsiConfig.UserName, adsiConfig.Password,
                                          adsiConfig.AuthenticationType)
                Return rootDomainEntry
            Catch exc As COMException
                LogException(exc)
                Return Nothing
            End Try

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
        Public Shared Function GetDomainByBIOSName(ByVal Name As String) As Domain
            Dim adsiConfig As Configuration = Configuration.GetConfig()

            ' Only access CrossRefCollection if LDAP is accessible
            If Not adsiConfig.RefCollection Is Nothing AndAlso adsiConfig.RefCollection.Count > 0 Then
                Dim refObject As CrossReferenceCollection.CrossReference = adsiConfig.RefCollection.ItemByNetBIOS(Name)
                Dim path As String = AddADSIPath(refObject.DomainPath)
                Dim _
                    domain As Domain =
                        Domain.GetDomain(path, adsiConfig.UserName, adsiConfig.Password, adsiConfig.AuthenticationType)

                Return domain
            Else
                Return Nothing
            End If

        End Function

        Public Overloads Shared Function GetRootEntry() As DirectoryEntry
            Return GetRootEntry(Path.GC)
        End Function

        Public Overloads Shared Function GetRootEntry(ByVal ADSIPath As Path) As DirectoryEntry
            Try
                Dim adsiConfig As Configuration = Configuration.GetConfig()
                Dim entry As DirectoryEntry = Nothing
                If Not adsiConfig Is Nothing Then
                    Dim rootDomainFullPath As String = AddADSIPath(adsiConfig.RootDomainPath, ADSIPath)
                    If Not rootDomainFullPath Is Nothing Then
                        entry = GetDirectoryEntry(rootDomainFullPath)
                    End If
                End If
                If Not entry Is Nothing AndAlso entry.Name.Length > 0 Then
                    Return entry
                Else
                    Return Nothing
                End If
            Catch exc As COMException
                LogException(exc)
                Return Nothing
            End Try

        End Function


        ''' -------------------------------------------------------------------
        ''' <summary>
        '''     Depends on how User/Password specified, 2 different method to obtain directory entry
        ''' </summary>
        ''' <remarks>
        '''     Admin might not enter User/Password to access AD in web.config
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Shared Function GetDirectoryEntry(ByVal Path As String) As DirectoryEntry
            Dim adsiConfig As Configuration = Configuration.GetConfig()
            Dim returnEntry As DirectoryEntry

            If (adsiConfig.UserName.Length > 0) AndAlso (adsiConfig.Password.Length > 0) Then
                returnEntry =
                    New DirectoryEntry(Path, adsiConfig.UserName, adsiConfig.Password, AuthenticationTypes.Delegation)
            Else
                returnEntry = New DirectoryEntry(Path)
            End If

            Return returnEntry

        End Function

        '''-------------------------------------------------------------------
        '''<summary>
        '''    Obtain the path to access top level domain entry in Windows Active Directory
        '''</summary>
        '''<remarks>For better performance and avoid error, Global Catalog is preferer accessing method
        '''</remarks>
        '''<history>
        '''    [tamttt]	08/01/2004	Created
        '''</history>
        '''-------------------------------------------------------------------
        Public Shared Function GetRootForestPath(Optional ByVal ADSIPath As Path = Path.GC) As String
            Try
                Dim strADSIPath As String = ADSIPath.ToString & "://"
                Dim ADsRoot As New DirectoryEntry(strADSIPath & "rootDSE")
                Dim _
                    strRootDomain As String = strADSIPath &
                                              CType(
                                                  ADsRoot.Properties(Configuration.ADSI_ROOTDOMAINNAMIMGCONTEXT).Value,
                                                  String)

                Return strRootDomain
            Catch ex As COMException
                LogException(ex)
                Return Nothing
            End Try
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        '''     Obtain location of the domain contains this entry, 
        ''' </summary>
        ''' <remarks>
        '''     Return string is in canonical format (ttt.com.vn)
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Shared Function GetEntryLocation(ByVal Entry As DirectoryEntry) As String
            Dim strReturn As String = ""
            If Not Entry Is Nothing Then
                Dim entryPath As String = CheckNullString(Entry.Path)

                If entryPath.Length > 0 Then
                    strReturn = Right(entryPath, entryPath.Length - entryPath.IndexOf("DC="))
                    strReturn = ConvertToCanonical(strReturn, False)
                End If
            End If

            Return strReturn
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [mhorton]	10/05/2009	Created - WorkItem:2943
        ''' </history>
        ''' -------------------------------------------------------------------

        Public Shared Function GetAllGroupnames() As ArrayList
            Dim RootDomain As Domain = GetRootDomain()
            Dim objSearch As New Search(RootDomain)

            objSearch.AddFilter(Configuration.ADSI_CLASS, CompareOperator.Is, ObjectClass.group.ToString)
            objSearch.PropertiesToLoad.Add(Configuration.ADSI_CNAME)

            Return objSearch.GetPropertyEntries(Configuration.ADSI_CNAME)

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        '''     Obtain user from Windows Active Directory using LogonName format - NETBIOSNAME\USERNAME
        ''' </summary>
        ''' <remarks>
        '''     -In multiple domains network, search result might return more than one user with the same name
        '''     -Additional steps to check by domain name to get correct user
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Shared Function GetUserEntryByName(ByVal Name As String) As DirectoryEntry
            ' Create search object then assign required params to get user entry in Active Directory
            Dim objSearch As New Search(GetRootDomain)
            Dim userEntries As ArrayList
            Dim userEntry As DirectoryEntry
            Dim userDomain As Domain

            With objSearch
                .AddFilter(Configuration.ADSI_CLASS, CompareOperator.Is, ObjectClass.person.ToString)
                .AddFilter(Configuration.ADSI_ACCOUNTNAME, CompareOperator.Is, TrimUserDomainName(Name))

                userEntries = .GetEntries
                Select Case userEntries.Count
                    Case 0
                        'Found no entry, return nothing
                        Return Nothing
                    Case 1
                        ' Find only one entry, return it
                        Return CType(userEntries.Item(0), DirectoryEntry)
                    Case Else
                        ' Find more than one entry, so we have to check to obtain correct user
                        ' Get user domain
                        userDomain = GetDomainByBIOSName(GetUserDomainName(Name))
                        If Not userDomain Is Nothing Then
                            For Each userEntry In userEntries
                                Dim entryPath As String = userEntry.Path
                                Dim _
                                    entryLocation As String =
                                        Right(entryPath, entryPath.Length - entryPath.IndexOf("DC="))
                                If entryLocation.ToLower = userDomain.DistinguishedName.ToLower Then
                                    Return userEntry
                                End If
                            Next
                        Else
                            ' If an error occurs while accessing LDAP (i.e double-hop issue), we return the first entry
                            ' This method not very accurately, however it would be OK for ALMOST network
                            Return CType(userEntries.Item(0), DirectoryEntry)
                        End If

                End Select

            End With

            Return Nothing
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <remarks>
        ''' This function's reserved for simple network which have single domain and logon username in simple format
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Shared Function CanonicalToNetBIOS(ByVal CanonicalName As String) As String
            Dim config As Configuration = Configuration.GetConfig()

            ' Only access CrossRefCollection if LDAP is accessible
            If Not config.RefCollection Is Nothing AndAlso config.RefCollection.Count > 0 Then
                Dim refObject As CrossReferenceCollection.CrossReference = config.RefCollection.Item(CanonicalName)
                If Not refObject Is Nothing Then
                    Return refObject.mNetBIOSName
                Else
                    Return ""
                End If
            Else
                Return ""
            End If
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        '''    Convert input string USERNAME@DOMAIN into NETBIOSNAME\USERNAME
        ''' </summary>
        ''' <remarks>
        '''    - We could do it only if LDAP is accessible to obtain NetBIOSName
        '''    - If LDAP is unaccessible, return original user name (UPN format)
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Shared Function UPNToLogonName0(ByVal UserPrincipalName As String) As String
            Dim config As Configuration = Configuration.GetConfig()
            Dim userName As String = UserPrincipalName

            If config.LDAPAccesible Then
                Dim _
                    userDomain As String =
                        Right(UserPrincipalName, UserPrincipalName.Length - (UserPrincipalName.IndexOf("@") + 1))
                Dim userNetBIOS As String = CanonicalToNetBIOS(userDomain)
                If Not userNetBIOS.Length = 0 Then
                    userName = userNetBIOS & "\" & TrimUserDomainName(UserPrincipalName)
                End If
            End If

            Return userName

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Get domain name (NETBIOS) from user logon name
        ''' </summary>
        ''' <remarks>
        ''' Input string must be LogonName format (NETBIOSNAME\USERNAME)
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Shared Function GetUserDomainName(ByVal UserName As String) As String
            Dim strReturn As String = ""
            If UserName.IndexOf("\") > 0 Then
                strReturn = Left(UserName, (UserName.IndexOf("\")))
            End If
            Return strReturn
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Trim user logon string to get simple user name
        ''' </summary>
        ''' <remarks>
        ''' Accept 3 different formats :
        ''' - LogonName format (NETBIOSNAME\USERNAME)
        ''' - UPN format (USERNAME@DOMAINNAME)
        ''' - Simple format (USERNAME only)
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Shared Function TrimUserDomainName(ByVal UserName As String) As String
            Dim strReturn As String
            If UserName.IndexOf("\") > -1 Then
                strReturn = Right(UserName, UserName.Length - (UserName.IndexOf("\") + 1))
            ElseIf UserName.IndexOf("@") > -1 Then
                strReturn = Left(UserName, UserName.IndexOf("@"))
            Else
                strReturn = UserName
            End If

            Return strReturn
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
        Public Shared Function AddADSIPath(ByVal Path As String, Optional ByVal ADSIPath As Path = ADSI.Path.GC) _
            As String
            If Path.IndexOf("LDAP://") <> -1 Then
                Return Path
            ElseIf Path.IndexOf("://") <> -1 Then
                'Clean existing ADs path first
                Path = Right(Path, Path.Length - (Path.IndexOf("://") + 3))
            End If
            Return ADSIPath.ToString & "://" & Path
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Shared Function ValidateDomainPath(ByVal Path As String, Optional ByVal ADSIPath As Path = ADSI.Path.GC) _
            As String
            ' If root domain is not specified in site settings, we start from top root forest
            If Path.Length = 0 Then
                Return GetRootForestPath()
            ElseIf (Path.IndexOf("DC=") <> -1) And (Path.IndexOf("://") <> -1) Then
                Return Path
            ElseIf (Path.IndexOf("LDAP://") <> -1) And (Path.IndexOf("://") <> -1) Then
                Return Path
            ElseIf (Path.IndexOf(".") <> -1) Then
                ' "ttt.com.vn" format,  it's possible for "LDAP://ttt.com.vn" format to access Authentication, however GC:// gives better performance
                Return ConvertToDistinguished(Path)
            Else
                ' Invalid path, so we get root path from Active Directory
                Return GetRootForestPath()
            End If
            'End If
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
        Public Shared Function ConvertToDistinguished(ByVal Canonical As String,
                                                       Optional ByVal ADSIPath As Path = Path.GC) As String
            Dim strDistinguished As String

            ' Clean up ADSI.Path to make sure we get a proper path
            If Canonical.IndexOf("://") <> -1 Then
                strDistinguished = Right(Canonical, Canonical.Length - (Canonical.IndexOf("://") + 3))
            Else
                strDistinguished = Canonical
            End If

            strDistinguished = Replace(strDistinguished, ".", ",DC=")
            strDistinguished = "DC=" & strDistinguished

            If Canonical.IndexOf("://") <> -1 Then
                strDistinguished = AddADSIPath(strDistinguished, ADSIPath)
            End If

            Return strDistinguished

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
        Public Shared Function ConvertToCanonical(ByVal Distinguished As String, ByVal IncludeADSIPath As Boolean) _
            As String
            Dim strCanonical As String = Distinguished

            If Not IncludeADSIPath AndAlso Distinguished.IndexOf("://") <> -1 Then
                strCanonical = Right(Distinguished, Distinguished.Length - (Distinguished.IndexOf("://") + 3))
            End If

            strCanonical = Replace(strCanonical, "DC=", "")
            strCanonical = Replace(strCanonical, "dc=", "")
            strCanonical = Replace(strCanonical, "CN=", "")
            strCanonical = Replace(strCanonical, "cn=", "")
            strCanonical = Replace(strCanonical, ",", ".")

            Return strCanonical

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Shared Function CheckNullString(ByVal value As Object) As String
            If value Is Nothing Then
                Return ""
            Else
                Return value.ToString
            End If
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
        Public Shared Function GetRandomPassword() As String
            Dim rd As New Random
            Return Convert.ToString(rd.Next)
        End Function

        ' See http://www.aspalliance.com/bbilbro/viewarticle.aspx?paged_article_id=4
        Public Shared Function ReplaceCaseInsensitive(ByVal text As String, ByVal oldValue As String,
                                                       ByVal newValue As String) As String
            oldValue = GetCaseInsensitiveSearch(oldValue)

            Return Regex.Replace([text], oldValue, newValue)

        End Function

        'ReplaceCaseInsensitive

        Shared Function GetCaseInsensitiveSearch(ByVal search As String) As String
            Dim result As String = String.Empty

            Dim index As Integer

            For index = 0 To search.Length - 1
                Dim character As Char = search.Chars(index)
                Dim characterLower As Char = Char.ToLower(character)
                Dim characterUpper As Char = Char.ToUpper(character)

                If characterUpper = characterLower Then
                    result = result + character
                Else
                    result = result + "[" + characterLower + characterUpper + "]"
                End If

            Next index
            Return result
        End Function

        'GetCaseInsensitiveSearch

        'ACD-7422 - Role Synchronization Not Working On W2K Domain Controllers
        'By using TokenGroups it should work with W2K.
        Public Shared Function GetADGroups(ByVal Name As String) As ArrayList
            Dim user As DirectoryEntry = GetUserEntryByName(Name)
            Dim irc As IdentityReferenceCollection = ExpandTokenGroups(user).Translate(GetType(NTAccount))
            Dim arrAccounts As New ArrayList

            For Each account As IdentityReference In irc
                If (TypeOf account Is NTAccount) Then
#If DEBUG Then
                    Debug.Print("Account=" + account.Value)
#End If
                    'arrAccounts.Add (account.Value)
                    'Trim the leading Group Name off the group (i.e. Remove DOMAIN\ from DOMAIN\Group)
                    If InStr(account.Value, "\") Then
                        If Not (arrAccounts.Contains(account.Value.Substring(InStr(account.Value, "\")))) Then
                            arrAccounts.Add(account.Value.Substring(InStr(account.Value, "\")))
                        End If
                    Else
                        arrAccounts.Add(account.Value)
                    End If
                End If
            Next

            Return arrAccounts
        End Function

        Private Shared Function ExpandTokenGroups(ByVal user As DirectoryEntry) As IdentityReferenceCollection
            user.RefreshCache(New String() {"tokenGroups"})

            Dim irc As New IdentityReferenceCollection()

            For Each sidBytes As Byte() In user.Properties("tokenGroups")
                irc.Add(New SecurityIdentifier(sidBytes, 0))
            Next
            Return irc
        End Function

        Public Shared Function GetIP4Address(ByVal strPassedIP As String) As String
            Dim IP4Address As String = String.Empty

            For Each IPA As IPAddress In Dns.GetHostAddresses(strPassedIP)
                If IPA.AddressFamily.ToString() = "InterNetwork" Then
                    IP4Address = IPA.ToString()
                    Exit For
                End If
            Next

            If IP4Address <> String.Empty Then
                Return IP4Address
            End If

            For Each IPA As IPAddress In Dns.GetHostAddresses(Dns.GetHostName())
                If IPA.AddressFamily.ToString() = "InterNetwork" Then
                    IP4Address = IPA.ToString()
                    Exit For
                End If
            Next

            Return IP4Address
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' Checks the trust level of the portal.
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [mhorton]	08/10/2008	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Shared Function GetCurrentTrustLevel() As AspNetHostingPermissionLevel
            For Each trustLevel As AspNetHostingPermissionLevel In
                New AspNetHostingPermissionLevel() _
                    {AspNetHostingPermissionLevel.Unrestricted, AspNetHostingPermissionLevel.High,
                     AspNetHostingPermissionLevel.Medium, AspNetHostingPermissionLevel.Low,
                     AspNetHostingPermissionLevel.Minimal}
                Try
                    Dim perm As New AspNetHostingPermission(trustLevel)
                    perm.Demand()
                Catch generatedExceptionName As SecurityException
                    Continue For
                End Try

                Return trustLevel
            Next

            Return AspNetHostingPermissionLevel.None
        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        '''     in multiple domains network that search result return more than one group with the same name (i.e Administrators)
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Shared Function GetGroupEntriesByName(ByVal GroupName As String) As ArrayList
            Dim RootDomain As ADSI.Domain = GetRootDomain()
            Dim objSearch As New Search(RootDomain)

            objSearch.AddFilter(Configuration.ADSI_CLASS, ADSI.CompareOperator.Is, ObjectClass.group.ToString)
            objSearch.AddFilter(Configuration.ADSI_ACCOUNTNAME, ADSI.CompareOperator.Is, GroupName)

            Dim groupEntries As ArrayList = objSearch.GetEntries

            If Not groupEntries Is Nothing Then
                Return groupEntries
            Else
                Return Nothing
            End If

        End Function

        Public Shared Function AddEventLog(portalsettings As Portals.PortalSettings, description As String) As Boolean
            objEventLog.AddLog("Description", description, portalsettings, -1, DotNetNuke.Services.Log.EventLog.EventLogController.EventLogType.ADMIN_ALERT)
        End Function

        ''' <summary>
        ''' Uses the byte data from active directory and writes an image file to the specified folder.  Returns the FileID 
        ''' </summary>
        ''' <param name="objUserInfo">User object</param>
        ''' <param name="photo">Byte array containing photo bytes</param>
        ''' <history>
        '''     [sawest]    12/16/2016     Created
        ''' </history>
        Public Shared Function WritePhoto(ByVal objUserInfo As ADUserInfo, ByVal photo() As Byte) As String

            Dim _folderinfo As IFolderInfo
            Dim _fileinfo As IFileInfo

            _folderinfo = FolderManager.Instance.GetUserFolder(objUserInfo)

            If Not _folderinfo Is Nothing Then
                Using stream As New System.IO.MemoryStream(photo)
                    Dim fname As String = Replace(objUserInfo.Username, "\", "_") & "_profile_photo.jpg"
                    _fileinfo = FileManager.Instance.AddFile(_folderinfo, fname, stream)
                    stream.Close()
                    _folderinfo = Nothing
                    If Not _fileinfo Is Nothing Then
                        Return _fileinfo.FileId
                    Else
                        Return ""
                    End If
                End Using
            Else
                Return ""
            End If
        End Function
    End Class
End Namespace
