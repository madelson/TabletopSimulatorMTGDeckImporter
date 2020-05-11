using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabletopMtgImporter.Wpf
{
    internal class StringDeckInput : IDeckInput
    {
        private readonly string _deck;

        public StringDeckInput(string name, string deck)
        {
            this._deck = deck;
            this.Name = name;
        }

        public string Name { get; }

        public TextReader OpenReader() => new StringReader(this._deck);
    }
}
