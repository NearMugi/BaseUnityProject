pyExe = Wscript.Arguments(0)
pyCode = Wscript.Arguments(1) 
folderPath = Wscript.Arguments(2)
aac = Wscript.Arguments(3) 

'python���Ăяo���R�}���h�𐶐�����
cmd = pyExe 
cmd = cmd & " " & pyCode
cmd = cmd & " " & folderPath
cmd = cmd & " " & aac
'WScript.Echo cmd 

Set objWShell = CreateObject("Wscript.Shell") 
objWShell.CurrentDirectory = folderPath
objWShell.run cmd, vbHide
