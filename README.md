<div align="center">

  <img src="assets/logo.png" width="50%" height="50%">
  
  [![License: Apache](https://img.shields.io/badge/License-Apache-blue.svg)](https://opensource.org/licenses/Apache-2.0)
  [![GitHub forks](https://img.shields.io/nuget/dt/DifferenceUtility.Net.svg)](https://www.nuget.org/packages/DifferenceUtility.Net/)
  [![lewisbennett](https://circleci.com/gh/lewisbennett/difference-utility-net.svg?style=svg)](https://circleci.com/gh/lewisbennett/difference-utility-net)
  
</div>

# DifferenceUtility.Net

DifferenceUtility.Net is a library for .NET handles the calculation and dispatch of the shortest possible path to convert one collection to another, resulting in a quick and smooth transition.

The library uses Eugene W. Myers' diff algorithm to calculate the difference between two collections ([see docs](docs)). It also has an optional extra layer for calculating moves for items that are persistant between the two collections, but might be in different positions.

## Getting Started

### Diff Callback

Start by creating the diff callback for your chosen data type by implementing [`IDiffCallback`](src/DifferenceUtility.Net/Base/IDiffCallback.cs). Alternatively, you can extend [`BaseDiffCallback`](src/DifferenceUtility.Net/Base/BaseDiffCallback.cs) depending on the use case. Examples of various implementations can be found within the [sample projects](samples).

### Calculate Diff

Call `DiffUtil.CalculateDiff` to calculate the difference between your two collections. The source collection, destination collection, and diff callback for the same data type must be provided. You can optionally disable move detection via the `detectMoves` parameter. This is recommended if your data is sorted by the same constraint (for example: date/time order), or whenever you know there won't be any moves that need to be made.

Depending on the size of your collections, it is recommended to calculate the difference on a background thread, then dispatch them on the main thread. This is especially recommended for UI applictions so not to block the main thread.


### Dispatching the Changes

Calling `DiffUtil.CalculateDiff` will return a `DiffResult` object which contains the necessary instructions to convert the source collection into the destination collection. Call `DiffResult.DispatchUpdatesTo` and provide either an `ObservableCollection` or an [`ICollectionUpdateCallback`](src/DifferenceUtility.Net/Base/ICollectionUpdateCallback.cs) to receive the changes.

## Sample Projects

* [MvvmCross core project](samples/Sample.MvvmCross.Core)
* [MvvmCross Android project](samples/Sample.MvvmCross.Droid)
* [Console characters project](samples/Sample.NetConsole.Characters): A console project that converts one string to another.
* [Console people project](samples/Sample.NetConsole.People): A console project that uses simple user/people objects.
* [Console reliability project](samples/Sample.NetConsole.Reliability): A console project that generates random strings, along with pre-made ones, to stress test the system.
* [Console path deconstruction project](samples/Sample.NetConsole.PathDeconstruction): A console project that deconstructs the calculated diff path to visually show the insert, remove, and update steps.

## Benchmarking

The project can be benchmarked using simple data within the [benchmarking project](src/DifferenceUtility.Net.Benchmarks).
