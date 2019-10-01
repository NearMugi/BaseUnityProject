pyExe = Wscript.Arguments(0)
pyCode = Wscript.Arguments(1) 
folderPath = Wscript.Arguments(2)
aac = Wscript.Arguments(3) 


Set FS = CreateObject("Scripting.FileSystemObject")
ret = FS.FileExists(folderPath & aac)
'WScript.Echo ret

'python���Ăяo���R�}���h�𐶐�����
cmd = pyExe 
cmd = cmd & " " & pyCode
cmd = cmd & " " & folderPath
cmd = cmd & " " & aac
'WScript.Echo cmd 

Set objWShell = CreateObject("Wscript.Shell") 
objWShell.CurrentDirectory = folderPath
objWShell.run cmd, vbHide
