using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TabletopMtgImporter
{
    public sealed class DeckFileInput : IDeckInput
    {
        private readonly string _path;
        
        public DeckFileInput(string path, bool useUwcCards)
        {
            this._path = Path.GetFullPath(path ?? throw new ArgumentNullException(nameof(path)));
            this.UwcSetRegex = new(useUwcCards ? "." : "$^", RegexOptions.IgnoreCase);
        }

        public string Name => Path.GetFileNameWithoutExtension(this._path);

        public Regex UwcSetRegex { get; }

        public TextReader OpenReader() => new StreamReader(this._path);
    }
}
