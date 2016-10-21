#!/usr/bin/env bash

##########################################################################
# This is the .NET Core SDK bootstrapper script for OS X.
##########################################################################

# Define directories.
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
TOOLS_DIR=$SCRIPT_DIR/tools
NETCORE_SDK_INSTALLER_PKG=$TOOLS_DIR/dotnet-dev.pkg

# Make sure the tools folder exist.
if [ ! -d "$TOOLS_DIR" ]; then
  mkdir "$TOOLS_DIR"
fi

# Download .NET Core SDK if it does not exist.
if [ ! -f "$NETCORE_SDK_INSTALLER_PKG" ]; then
    echo "Downloading .NET Core SDK..."
    curl -Lsfo "$NETCORE_SDK_INSTALLER_PKG" https://dotnetcli.blob.core.windows.net/dotnet/Sdk/rel-1.0.0/dotnet-dev-osx-x64.latest.pkg
    # https://go.microsoft.com/fwlink/?LinkID=831679
    if [ $? -ne 0 ]; then
        echo "An error occured while downloading .NET Core SDK."
        exit 1
    fi
fi

sudo installer -verboseR -pkg "$NETCORE_SDK_INSTALLER_PKG" -target /
