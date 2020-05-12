# Deck formats

In order to import a deck, you'll need to get it in a standard text format that the import tool can understand.

I recommend using a deck building tool like [Archidekt](https://archidekt.com/) to construct this, as this will ensure that you have no misspellings or typos. Currently, the import tool supports two formats.

### Simple format

The simple format is easy to construct by hand. Each line must be of the form:
```
<count> <cardname>
```
For example:
```
3 Forest
```

You can export this format from Archidekt by clicking "Export" on your deck page and choosing the "1 Card Name" format with "No Categories".

### Archidekt rich format

The "rich" format contains additional optional metadata like the set name, collector number, and category. In this format, each line takes the following form:
```
<count>x <card name> [(<set>)[ <collector number>]] [`<category>`]
``` 
For example:
```
29x Plains (mir) 334
1x Sol Ring `Ramp` 
1x Auriok Steelshaper (mrd) `Maybeboard` 
```

If the set/collector number are specified, the import tool will use this to determine which artwork is imported. If the category is "Maybeboard", the importer will not include the card in the deck.

You can export this format from Archidekt by clicking "Export" on your deck page and choosing the "1x Card Name (code)..." format. Alternatively, you can click "Edit" and copy-paste the content from there.

# Using the GUI

The left pane of the GUI app offers 2 tabs which provide different mechanisms for selecting a deck to import. Either you can type/paste in the deck content and a name, or you can select a file using the file selector.

Once the active tab has a deck selected, the "Import" button at the top of the right pane will become enabled. Click that button to being the import. 

As the import runs, logging information will appear in the right output pane. If your import fails or reports a warning, review the logging information to look for an explanation. If there is an unhandled error, you can find detailed error information by viewing the file %TEMP%\TabletopMTGImporterGUI.log.

# Using the Command Line App

To use the command line app, simply run it with a single argument which is the name of a file containing a deck in text format:
```
> TabletopMTGImporter.exe "My deck.txt"
```

# Debugging/Reporting Issues

If you encounter an issue, first review the output error information to see if there is an obvious problem such as an unrecognized card name. Also make sure that your computer is connected to the internet.

If you think there may be a bug in the importer, please [file an issue](https://github.com/madelson/TabletopSimulatorMTGDeckImporter/issues/new)! Please attach the exact deck file you tried to import.