rem @echo off
set folderPath=%1
set aac=%2

cd /d %folderPath%
python transcodingToWav.py %folderPath% %aac%
pause
