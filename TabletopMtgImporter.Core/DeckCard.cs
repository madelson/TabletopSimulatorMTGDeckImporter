﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabletopMtgImporter
{
    [DebuggerDisplay("{Name}")]
    internal class DeckCard
    {
        public DeckCard(string name, string? set, string? collectorNumber, bool isCommander)
        {
            if (set == null && collectorNumber != null)
            {
                throw new ArgumentException("Cannot specify collector number without set");
            }

            this.Name = name;
            // MA: for a while list cards would get various different set codes from Archidekt, but now Scryfall wants them as "plst"
            this.Set = set is "mb1" or "fmb1" or "phed" or "plist" ? "plst" : set;
            this.CollectorNumber = collectorNumber;
            this.IsCommander = isCommander;
        }

        public string Name { get; }
        public string? Set { get; }
        /// <summary>
        /// This is not strictly an int because cards with multiple art versions can have numbers like "60a"
        /// e. g. Soldevi Adnate
        /// </summary>
        public string? CollectorNumber { get; }
        public bool IsCommander { get; }
    }
}
