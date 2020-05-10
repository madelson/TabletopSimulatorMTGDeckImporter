using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabletopMtgImporter
{
    internal class DeckCard
    {
        public DeckCard(string name, string? set, int? collectorNumber, bool isCommander)
        {
            if (set == null && collectorNumber.HasValue)
            {
                throw new ArgumentException("Cannot specify collector number without set");
            }

            this.Name = name;
            this.Set = set;
            this.CollectorNumber = collectorNumber;
            this.IsCommander = isCommander;
        }

        public string Name { get; }
        public string? Set { get; }
        public int? CollectorNumber { get; }
        public bool IsCommander { get; }
    }
}
