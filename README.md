## smartive - core

A small but useful .net core library with some helpers / additions / extensions for your everyday work with
.net core.

[![pipeline status](https://gitlab.com/smartive/open-source/smartive-core/badges/master/pipeline.svg)](https://gitlab.com/smartive/open-source/smartive-core/commits/master)
[![nuget](https://img.shields.io/nuget/v/Smartive.Core.svg)](https://www.nuget.org/packages?q=smartive.core)
[![branch coverage](https://gitlab.com/smartive/open-source/smartive-core/-/jobs/artifacts/master/raw/coverage/report/badge_branchcoverage.svg?job=test)](https://gitlab.com/smartive/open-source/smartive-core/-/jobs/artifacts/master/file/coverage/report/summary.htm?job=test)
[![line coverage](https://gitlab.com/smartive/open-source/smartive-core/-/jobs/artifacts/master/raw/coverage/report/badge_linecoverage.svg?job=test)](https://gitlab.com/smartive/open-source/smartive-core/-/jobs/artifacts/master/file/coverage/report/summary.htm?job=test)

### Libs

All libraries are bundled in the `Smartive.Core` library. All parts can be installed on their own
with the corresponding nuget packages.

### Smartive.Core.Database

Contains various helpers and repository files for entity framework core.
There is a repository pattern that adds some functionality to fast prototype repositories
with default crud operations.

### Smartive.Core.Debug

Small debug library for asp.net core applications. Adds various debugging routes and
helpers to debug a web application.

### Smartive.Core.Wpf

Extensions for wpf applications (like xamarin forms).
Contains a base view model that implements `INotifyPropertyChanged`.
