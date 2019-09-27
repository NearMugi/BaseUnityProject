#!/usr/bin/env python

import sys
import os
from pydub import AudioSegment

filePath = str(sys.argv[1])
folderPath = str(sys.argv[2])
outFilePath = "/output.wav"

if os.path.exists(filePath):
    aacFile = AudioSegment.from_file(filePath, "aac")
    aacFile.export(folderPath + outFilePath, format="wav")
    print("...get WavFile! %s -> %s" % (filePath, outFilePath))
else:
    print("...file nothing " + filePath)
