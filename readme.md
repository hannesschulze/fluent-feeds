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

## Credits

Fluent Feeds depends on the following projects:

 * **[.NET](https://dotnet.microsoft.com/en-us/)** which provides the platform the app is implemented on.
 * **[WinUI 3](https://microsoft.github.io/microsoft-ui-xaml/)** which is used to implement the UI.
 * **[CsWin32](https://github.com/microsoft/CsWin32)** which is used to generate bindings for the Win32 methods used by
   the app.
 * **[MVVM Toolkit](https://github.com/CommunityToolkit/dotnet)** which is used to implement the MVVM architecture.
 * **[EF Core](https://github.com/dotnet/efcore)** which is used to interact with the local database.
 * **[SQLite](https://sqlite.org/index.html)** which is used for the local database.
 * **[AngleSharp](https://anglesharp.github.io/)** which is used for parsing HTML content and transforming it into
   Fluent Feed's rich text object model.
 * **[xUnit](https://xunit.net/)** which is used as the test framework for all unit tests.
 * **[Fluent System Icons](https://github.com/microsoft/fluentui-system-icons)** which is the icon set used by the app.
 * **[Next.js](https://nextjs.org)** which is used for the project website.

## License

This project is licensed under the MIT License - see the [license.txt](license.txt) file for details.
