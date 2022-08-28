---
title: 'Fluent Feeds v0.1.0'
publishedTimestamp: '2022-08-28T17:15Z'
author: 'Hannes Schulze'
summary: 'Find out more about the first public release of Fluent Feeds.'
---

Today marks the first public release of Fluent Feeds, a simple feed reader application designed for Windows 11.

This release includes support for RSS and Atom feeds, as well as experimental support for the "Hacker News" feed
provider which can be activated/deactivated in the app settings – in the future, this feed provider will become an
optional plugin which can be loaded dynamically.

Please note that this is the first public preview release of the app which may still contain bugs. There may still be
substantial changes and bug fixes coming in the next releases. Known issues include:

 * The app sometimes crashing when switching between Hacker News items
   ([#42](https://github.com/hannesschulze/fluent-feeds/issues/42)).
 * The title bar becoming unclickable after resizing the window
   ([upstream bug](https://github.com/microsoft/WindowsAppSDK/issues/2574)).
 * Drag and drop is currently not supported in the sidebar
   (delayed because of an [upstream bug](https://github.com/microsoft/microsoft-ui-xaml/issues/3290)).

The full list of bug reports and feature requests can be found on
[GitHub](https://github.com/hannesschulze/fluent-feeds/issues). If you have found a new issue or have a suggestion that
is not on that list, please make sure to submit a new issue.
