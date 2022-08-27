# Fluent Feeds

[![CI](https://github.com/hannesschulze/fluent-feeds/actions/workflows/ci.yml/badge.svg)](https://github.com/hannesschulze/fluent-feeds/actions/workflows/ci.yml)
[![Coverage](https://img.shields.io/codecov/c/github/hannesschulze/fluent-feeds)](https://codecov.io/gh/hannesschulze/fluent-feeds)
[![License](https://img.shields.io/github/license/hannesschulze/fluent-feeds)](license.txt)
[![Release](https://img.shields.io/github/v/release/hannesschulze/fluent-feeds?sort=semver)](https://github.com/hannesschulze/fluent-feeds/releases)

> Simple feed reader for Windows 11.

![Screenshot](doc/img/screenshot.png)

## Goals

Fluent Feeds aims to be a simple, modern and native feed reader app for Windows 11 that fits into the rest of the
platform.

It is designed to be modular by providing abstractions for feeds and items and using standard "feed providers" which
can load these feeds and items.

## Supported feeds

Right now, the app includes the following feed providers:
 * The **Syndication** feed provider which can load items from RSS or Atom feeds using the .NET
   `System.ServiceModel.Syndication` APIs.
 * The **Hacker News** feed provider which uses both the official API and the Algolia API to fetch stories and comments
   from Hacker News.

Support for dynamically loading custom feed providers using a plugin system is also planned. 

## License

This project is licensed under the MIT License - see the [license.txt](license.txt) file for details.
