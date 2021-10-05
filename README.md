# CryptoDNS

## Table of contents

* [General info](#general-info)
* [How it works](#how-it-works)
* [Technologies](#technologies)
* [Setup](#setup)
* [Release](#release)
* [DECENOMY](#decenomy)
* [License](#license)

## General info

This project is a multi-cryptocurrency DNS server for cryptocurrency's core wallets compatible with the DECENOMY Standard Wallet (DSW).

## How it works

For each configured DSW wallet, the service will call the corresponding `-cli` executable to fetch its peers and current masternode list at each minute, building a list with these fetched peers.

The service then serves those IP addresses on port 53 in a random way using the DNS protocol.
## Technologies

Project is created with:
* .NET 5.0 (net core)

## Setup

To develop on this project, install the .NET 5.0 SDK:

[.NET 5](https://github.com/dotnet/core/blob/main/release-notes/5.0/README.md)

With the SDK installed execute this command to test it or debug it:

```
$ sudo dotnet run
```

The `sudo` command is needed to allow the binding of port 53

## Release

To release the binaries for production compile and pack it with this command:

```
$ dotnet publish -c Release
```

For Windows or other architectures you need to specify the correct RID like described here:

[Publish .NET apps with the .NET CLI](https://docs.microsoft.com/en-us/dotnet/core/deploying/deploy-with-cli)
[.NET RID Catalog](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog)

example for: (Linux distributions running on ARM like Raspbian on Raspberry Pi Model 2+)

```
$ dotnet publish -c Release - r linux-arm
```

or for (Linux distributions running on 64-bit ARM like Ubuntu Server 64-bit on Raspberry Pi Model 3+)

```
$ dotnet publish -c Release - r linux-arm64
```

## DECENOMY

The CryptoDNS project is part of DECENOMY.

## License

Distributed under the MIT License. See `LICENSE` for more information.

