# SFSEd: Kerbal Space Program save-game editor
Copyright (C) Oliver 'kfsone' Smith, April 2020

Small tool for viewing/editing the '.sfs' files Kerbal Space Program uses to save games.

![.NET Core](https://github.com/kfsone/sfsed/workflows/.NET%20Core/badge.svg)

Version 0.4: Apr 17 2020
- Configured dotnet workflow for github.

Version 0.3: Apr 17 2020
- Converted to .net core for portability,
- UI improvements,
 - Show changes in red,
 - Improved status bar,
 - Don't leave "Loaded" or "Saved" in the status bar for a long time (was confusing)
- Minor memory/cpu performance improvements,
- General code cleanup,
- Terminology changes (Property for key/value pairs, Domain for collections)
TODO: Track changes up the treeview hierarchy too

Version 0.2: Apr 15 2020
- Added minimal, basic editing support,

Version 0.1: Apr 15 2020
- Reads and Writes SFS files,
- If a save would overwrite an existing file, is backed up as <original name>.saved
	e.g. foo.sfs => foo.sfs.saved
		 foo.sfs.saved => foo.sfs.saved.saved
