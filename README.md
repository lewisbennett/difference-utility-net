<div align="center">

  <img src="assets/logo.png" width="50%" height="50%">
  
  [![License: Apache](https://img.shields.io/badge/License-Apache-blue.svg)](https://opensource.org/licenses/Apache-2.0)
  [![GitHub forks](https://img.shields.io/nuget/dt/DifferenceUtility.Net.svg)](https://www.nuget.org/packages/DifferenceUtility.Net/)
  [![lewisbennett](https://circleci.com/gh/lewisbennett/difference-utility-net.svg?style=svg)](https://circleci.com/gh/lewisbennett/difference-utility-net)
  
</div>

# DifferenceUtility.Net

DifferenceUtility.Net is a library for .NET handles the calculation and dispatch of the shortest possible path to convert one collection to another, resulting in a quick and smooth transition.

The library uses Eugene W. Myers' diff algorithm to calculate the difference between two collections ([see docs](docs)). It also has an optional extra layer for calculating moves for items that are persistant between the two collections, but might be in different positions.
