FROM microsoft/aspnetcore:2.0.0-stretch

ARG SSH_KEY

WORKDIR /app

RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    libssl-dev \
    openssh-server && \
    rm -rf /var/lib/apt/lists/*

# Configure OpenSSH
COPY ./scripts/configure-sshd.sh /app
RUN mkdir /var/run/sshd && \
    SSH_KEY="${SSH_KEY}" /app/configure-sshd.sh

ARG CONFIGURATION=Debug

COPY ./bin/${CONFIGURATION}/netcoreapp2.0/publish/ /app
COPY ./scripts/container-start.sh /app

ENTRYPOINT [ "/app/container-start.sh" ]
