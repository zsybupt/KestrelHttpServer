#!/usr/bin/env bash

# Symlink libssl in place
ln -s /usr/lib/x86_64-linux-gnu/libssl.so.1.1 /usr/lib/x86_64-linux-gnu/libssl.so

# Start the app
/usr/bin/dotnet /app/Http2SampleApp.dll
