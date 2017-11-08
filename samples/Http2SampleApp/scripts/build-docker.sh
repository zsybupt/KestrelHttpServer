#!/usr/bin/env bash
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

dotnet publish --framework netcoreapp2.0 "$DIR/../Http2SampleApp.csproj"

# Check for default SSH key
if [ -z "$SSH_KEY_PATH" ]; then
    SSH_KEY_PATH="$HOME/.ssh/id_rsa.pub"
fi

# Check that the key path exists
if [ ! -f "$SSH_KEY_PATH" ]; then
    echo "Could not find SSH public key in ${SSH_KEY_PATH}. Use the 'SSH_KEY_PATH' environment variable to override the search path" 1>&2
    exit 1
fi

SSH_KEY=$(<$SSH_KEY_PATH)

docker build -t kestrel-http2-sample --build-arg "SSH_KEY=$SSH_KEY" "$DIR/.."
