#!/bin/bash

#install zip on debian OS, since microsoft/dotnet container doesn't have zip by default
if [ -f /etc/debian_version ]
then
  apt -qq update
  apt -qq -y install zip
fi

dotnet restore
dotnet tool install -g Amazon.Lambda.Tools --framework net6.0
dotnet lambda package --configuration "Release" --framework "net6.0" --output-package bin/Release/net6.0/hello.zip
rm -rf bin/Release/net6.0/hello
unzip bin/Release/net6.0/hello.zip -d bin/Release/net6.0/hello
rm -rf bin/Release/net6.0/hello.zip
cp doc1.docx bin/Release/net6.0/hello
zip -rj bin/Release/net6.0/hello.zip bin/Release/net6.0/hello 
rm -rf bin/Release/net6.0/hello

# sls deploy --aws-profile thivanka --stage dev

