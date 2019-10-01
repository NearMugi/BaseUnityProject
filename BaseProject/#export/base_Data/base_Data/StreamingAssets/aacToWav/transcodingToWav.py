#!/usr/bin/env python

import sys
import os
from pydub import AudioSegment

folderPath = str(sys.argv[1])
aacFile = str(sys.argv[2])
filePath = folderPath + aacFile
outFilePath = folderPath + "/" + aacFile.split(".")[0] + ".wav"


if os.path.exists(filePath):
    print("...File exist")
    try:
        aacFile = AudioSegment.from_file(filePath, "aac")
        aacFile.export(outFilePath, format="wav")
        print("...get WavFile! %s -> %s" % (filePath, outFilePath))
    except Exception as e:
        print("...Err \n" + e.args)
else:
    print("...file nothing " + filePath)
