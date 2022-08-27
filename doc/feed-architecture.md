# Feed Architecture

At the heart of Fluent Feeds is its modular feed provider architecture.

## Feed Providers

The entry point is the abstract `FeedProvider` class which creates all other data structure. When a feed provider is
first loaded, the feed provider describes the initial tree structure shown in the application's sidebar. It also
defines which feeds and groups can be edited by the user and how new feeds can be created (for example, new RSS feeds
can be created using a URL factory).

Finally, Fluent Feeds stores the tree structure into a local database. To implement this behavior, the feed provider
needs to be able to store custom feeds and items into strings and then load them again. This string is then stored in
the database.

## Feeds

The initial tree structure is defined in a tree of `FeedDescriptor` nodes. These objects are merely descriptions of the
feed and not complete loader implementations or database models. The feed provider has no knowledge of the logic used
to store feeds in the database, cache items or group feeds together. These classes are part of the app project and
created based on the descriptors returned by the feed provider. This is done to keep the base class library's API small
and stable.

There are two types of feed descriptors:
 * `GroupFeedDescriptor` which is used to create group feeds combining the items of all their child feeds (as long as 
   `IsExcludedFromGroup` is set to false on the child feed's descriptor object).
 * `CachedFeedDescriptor` which contains an `IFeedContentLoader` implementation capable of loading a set of items and
   the feed's metadata when requested. The feeds created from cached feed descriptors store the metadata and items in the local database.

## Items

Similar to feed descriptors, items created by the feed loader are also returned as `ItemDescriptor`s which are used to
create the database objects in the app. Item descriptors contain some common data such as the title, author or a short
summary. Additionally, they contain an `IItemContentLoader` implementation which can load the content of the item when
requested. Note that app-specific data like the `IsRead` flag indicating if the user has already read the article are
not present in the descriptor object.

There are multiple item content types that can be returned by a content loader, for example:
 * `ArticleItemContent` which contains a rich-text body.
 * `CommentItemContent` which contains a comment tree.
