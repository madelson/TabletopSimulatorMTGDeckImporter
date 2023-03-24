## Importing from mtg.wpf to Archidekt

* On the [sealed page of mtg.wpf](https://mtg.wtf/sealed), generate your box and click "Preview as Deck".
* Scroll down and copy the plain text content of the deck to a file in your downloads folder (e.g. "deck.txt").
* [Install LINQPad7](https://www.linqpad.net/Download.aspx).
* Right click [this link](https://github.com/madelson/TabletopSimulatorMTGDeckImporter/raw/master/scripts/MtgWpfToArchidekt.linq) -> "Save link as" and save it to your downloads folder.
* Open command prompt (Windows Key+R, type "cmd", hit "Ok")
* Run `cd %userprofile%\Downloads`
* Run `lprun7 MtgWpfToArchidekt.linq deck.txt`
* Open the generated cards.txt file and paste the content into the import panel in [Archidekt](https://archidekt.com/)
