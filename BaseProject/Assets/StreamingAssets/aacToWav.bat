rem @echo off
set folderPath=%1
set aac=%2

python transcodingToWav.py %folderPath% %aac%
pause
