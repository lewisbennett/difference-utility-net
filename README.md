# Difference Utility .NET

A simple library, similar to Android's DiffUtil, for .NET collections.

## The concept

Difference utility provides a means for gracefully applying collection changes when using `ObservableCollection`, by using `Add`, `Insert`, `Move`, and `Remove` methods. This allows you to not have to clear the collection first, then bulk add the existing data concatenated with any new data, potentially resulting in lower performance and/or worse user experience. Instead, only the required steps to get from collection A to collection B are taken.

## Samples

[.NET console project](https://github.com/lewisbennett/difference-utility-net/tree/develop/samples/Sample.Console)

[MvvmCross core project](https://github.com/lewisbennett/difference-utility-net/tree/develop/samples/Sample.MvvmCross.Core)

[MvvmCross Android project](https://github.com/lewisbennett/difference-utility-net/tree/develop/samples/Sample.MvvmCross.Droid)
