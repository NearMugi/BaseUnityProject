
cmd = "python.exe transcodingToWav.py D:\#WorkSpace\#PersonalDevelop\BaseUnityProject\BaseProject\Assets\StreamingAssets /hoge.aac"
'WScript.Echo cmd 

Set objWShell = CreateObject("Wscript.Shell") 
objWShell.run cmd, 0
