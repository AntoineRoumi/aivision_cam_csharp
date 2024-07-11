#!/bin/bash

rm ./object-detection -rf
git clone https://github.com/AntoineRoumi/object-detection.git --depth 1
pip install ./object-detection
cp ./object-detection/training_dataset -r .

dotnet add package pythonnet --version 3.0.3
