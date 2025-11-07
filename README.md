# Grade Calculator

## Introduction

The Grade Calculator is a command-line tool for managing school grades. It allows adding, removing, and displaying grades, as well as calculating statistics.

This program is intended to be used for the swiss school system, which uses grades from 1.0 (worst) to 6.0 (best). A grade of 4.0 or higher is considered passing.

This program has been tested on Windows 11 Home + Enterprise.

## Setup

1. Install [dotnet (9.0)](https://dotnet.microsoft.com/en-us/download)
1. Clone this repository
1. In PowerShell, navigate to the repository root
1. `dotnet run -- [arguments]`

## Arguments

```txt
Usage:
    list (Subject)               Shows list of stored marks, optionally filtered by subject
    add <Name> <Mark> <Subject>  Adds a new mark
    remove <Name>                Removes a mark by name
    stats                        Shows average per subject
    log                          Shows the log
    help                         Shows this help
<> - Required
() - Optional
```

### Examples

```ps1
dotnet run -- list
dotnet run -- list Math # Filter by subject

dotnet run -- add "FP Grade Calculator Project M323" 6.0 "Computer Science"

dotnet run -- remove "FP Grade Calculator Project M323"

dotnet run -- stats

dotnet run -- help
```

### Note

To reset all saved data, delete the `data` folder.
