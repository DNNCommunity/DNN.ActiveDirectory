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

Namespace DotNetNuke.Authentication.ActiveDirectory.ADSI
    Public Class CrossReferenceCollection
        Inherits CollectionBase

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Class CrossReference
            Friend mDomainPath As String
            Friend mCanonicalName As String
            Friend mNetBIOSName As String

            Friend Sub New(ByVal Path As String, ByVal NetBIOS As String, ByVal Canonical As String)
                mDomainPath = Path
                mCanonicalName = Canonical
                mNetBIOSName = NetBIOS
            End Sub

            Public ReadOnly Property DomainPath() As String
                Get
                    Return mDomainPath
                End Get
            End Property

            Public ReadOnly Property CanonicalName() As String
                Get
                    Return mCanonicalName
                End Get
            End Property

            Public ReadOnly Property NetBIOSName() As String
                Get
                    Return mNetBIOSName
                End Get
            End Property
        End Class

        ' Allows access to items by both NetBiosName or CanonicalName
        Private mNetBIOSLookup As Hashtable = New Hashtable
        Private mCanonicalLookup As Hashtable = New Hashtable
        Private mProcessLog As String = ""

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Public Sub New (ByVal UserName As String, ByVal Password As String, ByVal AuthType As AuthenticationTypes)
            MyBase.New()
            Try
                ' Obtain NETBIOS only if LDAP accessible to prevent error
                Dim rootLDAP As New DirectoryEntry ("LDAP://rootDSE", UserName, Password, AuthType)
                Dim _
                    crossRefPath As String = "LDAP://CN=Partitions," & _
                                             rootLDAP.Properties ("configurationNamingContext").Value.ToString
                Dim objCrossRefContainer As DirectoryEntry

                If (UserName.Length > 0) AndAlso (Password.Length > 0) Then
                    objCrossRefContainer = New DirectoryEntry (crossRefPath, UserName, Password, AuthType)
                Else
                    objCrossRefContainer = New DirectoryEntry (crossRefPath)
                End If

                Dim objCrossRef As DirectoryEntry
                For Each objCrossRef In objCrossRefContainer.Children
                    If Not objCrossRef.Properties ("nETBIOSName").Value Is Nothing Then
                        Dim netBIOSName As String = CType (objCrossRef.Properties ("nETBIOSName").Value, String)
                        Dim canonicalName As String = CType (objCrossRef.Properties ("dnsRoot").Value, String)
                        Dim domainPath As String = CType (objCrossRef.Properties ("nCName").Value, String)
                        Dim crossRef As CrossReference = New CrossReference (domainPath, netBIOSName, canonicalName)
                        Me.Add (crossRef)
                    End If
                Next
            Catch ex As COMException
                mProcessLog += ex.Message & "<br>"
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
        Friend Shadows Sub Clear()
            mNetBIOSLookup.Clear()
            mCanonicalLookup.Clear()
            MyBase.Clear()
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
        Friend Sub Add (ByVal RefObject As CrossReference)
            Dim index As Integer
            Try
                index = MyBase.List.Add (RefObject)
                mCanonicalLookup.Add (RefObject.CanonicalName, index)
                mNetBIOSLookup.Add (RefObject.NetBIOSName, index)
            Catch ex As COMException
                mProcessLog += ex.Message
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
        Public Function Item (ByVal index As Integer) As CrossReference
            Try
                Dim obj As Object
                obj = MyBase.List.Item (index)
                Return CType (obj, CrossReference)
            Catch Exc As Exception
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
        Public Function Item (ByVal Name As String) As CrossReference
            Dim index As Integer
            Dim obj As Object

            ' Do validation first
            Try
                If mCanonicalLookup.Item (Name) Is Nothing Then
                    Return Nothing
                End If
            Catch ex As Exception
                Return Nothing
            End Try

            index = CInt (mCanonicalLookup.Item (Name))
            obj = MyBase.List.Item (index)

            Return CType (obj, CrossReference)
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
        Public Function ItemByNetBIOS (ByVal Name As String) As CrossReference
            Dim index As Integer
            Dim obj As Object

            ' Do validation first
            Try
                If mNetBIOSLookup.Item (Name) Is Nothing Then
                    Return Nothing
                End If
            Catch ex As Exception
                Return Nothing
            End Try

            index = CInt (mNetBIOSLookup.Item (Name))
            obj = MyBase.List.Item (index)

            Return CType (obj, CrossReference)
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
        Public ReadOnly Property ProcesssLog() As String
            Get
                Return mProcessLog
            End Get
        End Property
    End Class
End Namespace
