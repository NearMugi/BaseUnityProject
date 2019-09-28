#!/usr/bin/env python

import sys
import os
from pydub import AudioSegment

filePath = str(sys.argv[1])
folderPath = str(sys.argv[2])
outFilePath = "/output.wav"


if os.path.exists(filePath):
    print("...File exist")
    try:
        aacFile = AudioSegment.from_file(filePath, "aac")
        aacFile.export(folderPath + outFilePath, format="wav")
        print("...get WavFile! %s -> %s" % (filePath, outFilePath))
    except Exception as e:
        print("...Err \n" + e.args)
else:
    print("...file nothing " + filePath)
