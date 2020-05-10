using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabletopMtgImporter
{
    public interface IDeckInput
    {
        string Name { get; }
        TextReader OpenReader();
    }
}
