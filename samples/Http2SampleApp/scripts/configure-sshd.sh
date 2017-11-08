#!/usr/bin/env bash

SSH_KEY="$1"

if [ -z $SSH_KEY ]; then
    echo "You must provide the SSH_KEY build-arg to Docker Build in order to configure SSH" 1>&2
    echo "That argument must contain the text of your SSH public key (not just the file path)" 1>&2
    exit 1
fi

echo "=> Adding SSH key to /root"
mkdir -p /root/.ssh
chmod go-rwx /root/.ssh
echo "${SSH_KEY}" > /root/.ssh/authorized_keys
chmod go-rw /root/.ssh/authorized_keys
echo "=> Done!"
