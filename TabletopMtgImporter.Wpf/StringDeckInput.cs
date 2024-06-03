using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TabletopMtgImporter.Wpf
{
    internal class StringDeckInput : IDeckInput
    {
        private readonly string _deck;

        public StringDeckInput(string name, string deck, bool useUwcCards)
        {
            this._deck = deck;
            this.Name = name;
            this.UwcSetRegex = new Regex(useUwcCards ? "." : "$^", RegexOptions.IgnoreCase);
        }

        public string Name { get; }

        public Regex UwcSetRegex { get; }

        public TextReader OpenReader() => new StringReader(this._deck);
    }
}
