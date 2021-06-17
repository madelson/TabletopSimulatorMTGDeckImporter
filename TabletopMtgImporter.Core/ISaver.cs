using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabletopMtgImporter
{
    public interface ISaver
    {
        Task SaveAsync(string name, string contents);
    }
}
