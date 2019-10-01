folderPath = Wscript.Arguments(0)
bat = Wscript.Arguments(1) 
aac = Wscript.Arguments(2) 

'batファイルを呼び出すコマンドを生成する
cmd = "cmd /c " & folderPath & bat 
cmd = cmd & " " & folderPath
cmd = cmd & " " & aac
'WScript.Echo cmd 

Set objWShell = CreateObject("Wscript.Shell") 
objWShell.run cmd, vbHide
'objWShell.run "cmd /c \Assets\StreamingAssets\aacToWav.bat", vbHide
