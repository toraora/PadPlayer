# Puzzles and Dragons Automator
## Introduction
Puzzles and Dragons Automator is a system for performing actions in the popular mobile game Puzzles and Dragons. It uses any method (by default, beam search) to find good sequence of moves for a given player's team and board position. On the device side, MonkeyRunner is used to enable taking screenshots and emulating touch events on Android devices. The core is done in .NET / ASPNET5, while the device logic is done in Python.

## Usage
To use, define your team by creating a ``BoardScorer.Options`` object and filling out the properties as needed (example in ``BoardScorer.cs``). Configure the API endpoint 'solve2' in PadController.cs to use your team, then run ``pad.py`` under MonkeyRunner with USB debugging enabled on the connected Android device.

- - -
By Nathan Wong
nathan@berkeley.edu