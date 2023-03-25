## Importing from mtg.wpf to Archidekt

* On the [sealed page of mtg.wpf](https://mtg.wtf/sealed), generate your box and click "Preview as Deck".
* Scroll down and copy the plain text content of the deck to a file in your downloads folder (e.g. "deck.txt"). **NOTE** save this file in case you want to reset later!
* Install the [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
* [Install LINQPad7](https://www.linqpad.net/Download.aspx).
* Right click [this link](https://github.com/madelson/TabletopSimulatorMTGDeckImporter/raw/master/scripts/MtgWpfToArchidekt.linq) -> "Save link as" and save it to your downloads folder. Be sure to replace any previous version!
* Open command prompt (Windows Key+R, type "cmd", hit "Ok")
* Run `cd %userprofile%\Downloads`
* Run `lprun7 MtgWpfToArchidekt.linq deck.txt`
    * If you get an error saying lprun7 is not recognized, try the following instead: `"c:\Program Files\LINQPad7\LPRun7.exe" MtgWpfToArchidekt.linq deck.txt`
* Open the generated cards.txt file and paste the content into the import panel in [Archidekt](https://archidekt.com/)
* **NOTE**: once you've picked your colors (e.g. black/white), you can run `lprun7 MtgWpfToArchidekt.linq deck.txt WB` to regenerate cards.txt filtered down to just the cards that fit that color identity.
    * If you get an error saying lprun7 is not recognized, try the following instead: `"c:\Program Files\LINQPad7\LPRun7.exe" MtgWpfToArchidekt.linq deck.txt WB`
