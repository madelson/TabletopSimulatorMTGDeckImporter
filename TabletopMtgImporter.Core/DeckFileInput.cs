using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabletopMtgImporter
{
    public sealed class DeckFileInput : IDeckInput
    {
        private readonly string _path;

        public DeckFileInput(string path)
        {
            this._path = Path.GetFullPath(path ?? throw new ArgumentNullException(nameof(path)));
        }

        public string Name => Path.GetFileNameWithoutExtension(this._path);
        public TextReader OpenReader() => new StreamReader(this._path);
    }
}
