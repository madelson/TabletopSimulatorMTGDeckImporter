using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabletopMtgImporter
{
    public interface ICache
    {
        Task<string?> GetValueOrDefaultAsync(string key);
        Task SetValueAsync(string key, string value);
    }
}
