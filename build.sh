#!/bin/bash

#install zip on debian OS, since microsoft/dotnet container doesn't have zip by default
if [ -f /etc/debian_version ]
then
  apt -qq update
  apt -qq -y install zip
fi

dotnet restore
dotnet tool install -g Amazon.Lambda.Tools --framework net6.0
dotnet lambda package --configuration "Release" --framework "net6.0" --output-package bin/Release/net6.0/DocxGenerate.zip
rm -rf bin/Release/net6.0/DocxGenerate
unzip bin/Release/net6.0/DocxGenerate.zip -d bin/Release/net6.0/DocxGenerate
rm -rf bin/Release/net6.0/DocxGenerate.zip
cp doc1.docx bin/Release/net6.0/DocxGenerate
zip -rj bin/Release/net6.0/DocxGenerate.zip bin/Release/net6.0/DocxGenerate 
rm -rf bin/Release/net6.0/DocxGenerate

# sls deploy --aws-profile thivanka --stage dev

