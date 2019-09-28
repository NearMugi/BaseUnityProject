#!/usr/bin/env python

import sys
import io
import os
from pydub import AudioSegment

folderPath = str(sys.argv[1])
binaryString = str(sys.argv[2])
outFilePath = "/tmp.aac"

print(folderPath + outFilePath)
# print(binaryString)

tmp = binaryString.split(",")[:-1]
binary = [int(b).to_bytes(1, 'big') for b in tmp]
print(type(binary))
print(len(binary))

s = io.BytesIO(b"".join(binary))
print(s.read(2))
# バイナリデータからaacファイルを作成する
AudioSegment.from_file(s, format="aac")
#AudioSegment.from_file(s).export(folderPath + outFilePath, format="aac")
#song = AudioSegment.from_file(s, format="aac")
print("...get WavFile! %s" % (outFilePath))
