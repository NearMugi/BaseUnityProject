folderPath = Wscript.Arguments(0)
bat = Wscript.Arguments(1) 
aac = Wscript.Arguments(2) 

'bat�t�@�C�����Ăяo���R�}���h�𐶐�����
cmd = "cmd /c " & bat 
cmd = cmd & " " & folderPath
cmd = cmd & " " & aac
'WScript.Echo cmd 

Set objWShell = CreateObject("Wscript.Shell") 
objWShell.CurrentDirectory = folderPath
objWShell.run cmd, vbHide
