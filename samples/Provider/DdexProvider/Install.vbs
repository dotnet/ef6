Main

Sub Main()

   Dim WshShell
   Set WshShell = WScript.CreateObject("WScript.Shell")

   Dim fso
   Set fso = WScript.CreateObject("Scripting.FileSystemObject")

   Dim myDir
   myDir = fso.GetParentFolderName(WScript.ScriptFullName)

   Dim ranu
   ranu = WScript.Arguments(0)

   Dim regRoot
   regRoot = WScript.Arguments(1)
   If Right(regRoot, 1) = "\" Then
      regRoot = Left(regRoot, Len(regRoot) - 1)
   End If
   If (ranu = "No") Then
      regRoot = "HKEY_LOCAL_MACHINE\" & regRoot
   Else
      regRoot = "HKEY_CURRENT_USER\" & regRoot & "\Configuration"
   End If

   Dim codebase
   codebase = WScript.Arguments(2)

   Dim regFile
   Dim genRegFile
   Dim regFileContents
   Set regFile = fso.OpenTextFile(myDir & "\ExtendedProvider.reg", 1)
   Set genRegFile = fso.CreateTextFile(myDir & "\ExtendedProvider.gen.reg", true)
   regFileContents = regFile.ReadAll()
   regFileContents = Replace(regFileContents, "%REGROOT%", regRoot)
   regFileContents = Replace(regFileContents, "%PROVIDERGUID%", "{E46DFA5C-80A2-4a7a-B6FC-C5D2F67D8690}")
   regFileContents = Replace(regFileContents, "%CODEBASE%", Replace(codebase, "\", "\\"))
   genRegFile.Write(regFileContents)
   genRegFile.Close()
   regFile.Close()

   Dim oExec
   Set oExec = WshShell.Exec(WScript.Arguments(3) & " /s """ & myDir & "\ExtendedProvider.gen.reg""")
   Do While oExec.Status = 0
      WScript.Sleep(100)
   Loop

   fso.DeleteFile(myDir & "\ExtendedProvider.gen.reg")

End Sub
