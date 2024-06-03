using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TabletopMtgImporter
{
    public interface IDeckInput
    {
        string Name { get; }
        Regex UwcSetRegex { get; }
        TextReader OpenReader();
    }
}
