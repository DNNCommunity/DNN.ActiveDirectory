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
    Public Class Search
        Inherits DirectorySearcher

        Private mSearchFilters As New ArrayList
        Private mFilterString As String

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        ''' </history>
        ''' -------------------------------------------------------------------
        Sub New()
            MyBase.New()
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
        Sub New (ByVal rearchRoot As DirectoryEntry)
            MyBase.New (rearchRoot)
            PopulateDefaultProperties()
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
        Sub New (ByVal rearchRoot As DirectoryEntry, ByVal Filter As String, _
                 Optional ByVal SortProperty As String = Configuration.ADSI_CNAME)
            MyBase.New (rearchRoot, Filter)
            PopulateDefaultProperties()

            Sort.PropertyName = SortProperty
        End Sub

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        '''     [mhorton]   10/05/2009  Added PropertyNamesOnly - WorkItem:2943
        ''' </history>
        ''' -------------------------------------------------------------------
        Private Sub PopulateDefaultProperties()
            CacheResults = True
            ' default is True
            ReferralChasing = ReferralChasingOption.All
            'default is External
            SearchScope = DirectoryServices.SearchScope.Subtree
            'default is Subtree
            PropertyNamesOnly = False
            PageSize = 1000
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
        Public Overloads Function GetEntry() As DirectoryEntry
            Dim result As SearchResult

            Try
                Filter = FilterString
                result = FindOne()

                If Not result Is Nothing Then
                    Return result.GetDirectoryEntry
                Else
                    Return Nothing
                End If
            Catch ex As COMException
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
        Public Overloads Function GetEntries() As ArrayList
            Dim resultCollection As SearchResultCollection
            Dim result As SearchResult
            Dim entries As New ArrayList
            Try
                Filter = FilterString
                resultCollection = FindAll()
                For Each result In resultCollection
                    entries.Add (result.GetDirectoryEntry)
                Next

                'Item 4230 - Explicit call of Dispose() is required, according to 
                'http://msdn.microsoft.com/library/system.directoryservices.directorysearcher.findall.aspx
                resultCollection.Dispose()

            Catch ex As COMException
            End Try

            Return entries

        End Function

        ''' -------------------------------------------------------------------
        ''' <summary>
        ''' </summary>
        ''' <remarks>
        ''' </remarks>
        ''' <history>
        '''     [tamttt]	08/01/2004	Created
        '''     [mhorton]   10/05/2009  Added PropertyNamesOnly - WorkItem:2943
        ''' </history>
        ''' -------------------------------------------------------------------

            Public Overloads Function GetPropertyEntries (ByVal Propertyname As String) As ArrayList
            Dim resultCollection As SearchResultCollection
            Dim entries As New ArrayList
            Try
                Filter = FilterString
                resultCollection = FindAll()
                For Each result As SearchResult In resultCollection
                    entries.Add (result.GetDirectoryEntry.Properties (Propertyname) (0))
                Next

                'Explicit call of Dispose() is required, according to 
                'http://msdn.microsoft.com/library/system.directoryservices.directorysearcher.findall.aspx
                resultCollection.Dispose()

            Catch ex As COMException
            End Try

            Return entries

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
        Public Sub AddFilter (ByVal Name As String, ByVal [Operator] As CompareOperator, _
                              Optional ByVal Value As String = "*")
            Dim filter As New SearchFilter

            filter.SetFilter (Name, [Operator], Value)
            mSearchFilters.Add (filter)

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
        Public Property SearchFilters() As ArrayList
            Get
                Return mSearchFilters
            End Get
            Set (ByVal Value As ArrayList)
                mSearchFilters = Value
            End Set
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
        Public ReadOnly Property FilterString() As String
            Get
                Dim filter As SearchFilter
                Dim sb As New StringBuilder

                sb.Append ("(&")
                For Each filter In Me.SearchFilters
                    sb.Append (AppendFilter (filter))
                Next
                sb.Append (")")
                Return sb.ToString
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
        Private Function AppendFilter (ByVal Filter As SearchFilter) As String
            Dim sb As New StringBuilder
            With Filter
                Select Case Filter.ADSICompareOperator
                    Case CompareOperator.Is
                        sb.Append ("(")
                        sb.Append (.Name)
                        sb.Append ("=")
                        sb.Append (.Value)
                        sb.Append (")")
                    Case CompareOperator.[IsNot]
                        sb.Append ("(!")
                        sb.Append (.Name)
                        sb.Append ("=")
                        sb.Append (.Value)
                        sb.Append (")")
                    Case CompareOperator.StartsWith
                        sb.Append ("(")
                        sb.Append (.Name)
                        sb.Append ("=")
                        sb.Append (.Value)
                        sb.Append ("*)")
                    Case CompareOperator.EndsWith
                        sb.Append ("(")
                        sb.Append (.Name)
                        sb.Append ("=*")
                        sb.Append (.Value)
                        sb.Append (")")
                    Case CompareOperator.Present
                        sb.Append ("(")
                        sb.Append (.Name)
                        sb.Append ("=")
                        sb.Append ("*)")
                    Case CompareOperator.NotPresent
                        sb.Append ("(!")
                        sb.Append (.Name)
                        sb.Append ("=")
                        sb.Append ("*)")
                End Select
            End With

            Return sb.ToString

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
            Public Structure SearchFilter
            Friend mName As String
            Friend mValue As String
            Friend mCompareOperator As CompareOperator

            Friend Sub SetFilter (ByVal Name As String, ByVal [Operator] As CompareOperator, ByVal Value As String)
                mName = Name
                mValue = Value
                mCompareOperator = [Operator]
            End Sub

            Public ReadOnly Property Name() As String
                Get
                    Return mName
                End Get
            End Property

            Public ReadOnly Property Value() As String
                Get
                    Return mValue
                End Get
            End Property

            Public ReadOnly Property ADSICompareOperator() As CompareOperator
                Get
                    Return mCompareOperator
                End Get
            End Property
        End Structure
    End Class
End Namespace
