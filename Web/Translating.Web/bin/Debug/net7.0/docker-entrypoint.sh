#!/bin/sh

# exit when any command fails
#set -e

# trust dev root CA
cp /https/ca.crt /usr/local/share/ca-certificates/
chmod -R 644 /usr/local/share/ca-certificates/

update-ca-certificates --fresh

dotnet Translating.Web.dll
#need for visual studio to run proj
tail -f /dev/null