using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TabletopMtgImporter
{
    internal static class DeckParser
    {
        public static bool TryParse(TextReader reader, out List<DeckCard> cards, out List<string> errorLines)
        {
            cards = new List<DeckCard>();
            errorLines = new List<string>();

            var format = Format.Unknown;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    DeckCard[]? parsed;
                    switch (format)
                    {
                        case Format.Unknown:
                            if ((parsed = TryParseLineArchidekt1CardName(line)) != null)
                            {
                                format = Format.Archidekt1CardName;
                            }
                            else if ((parsed = TryParseLineArchidekt1xCardNameCodeCategoryLabel(line)) != null)
                            {
                                format = Format.Archidekt1xCardNameCodeCategoryLabel;
                            }
                            else
                            {
                                parsed = null;
                            }
                            break;
                        case Format.Archidekt1CardName:
                            parsed = TryParseLineArchidekt1CardName(line);
                            break;
                        case Format.Archidekt1xCardNameCodeCategoryLabel:
                            parsed = TryParseLineArchidekt1xCardNameCodeCategoryLabel(line);
                            break;
                        default:
                            throw new InvalidOperationException("Should never get here");
                    }

                    if (parsed == null)
                    {
                        errorLines.Add(line);
                    }
                    else
                    {
                        cards.AddRange(parsed);
                    }
                }
            }

            return errorLines.Count == 0;
        }

        private static DeckCard[]? TryParseLineArchidekt1CardName(string line)
        {
            var match = Regex.Match(line, @"^(?<count>\d+)\s+(?<name>\S+(\s+\S+)*)\s*$");
            if (match.Success 
                && int.TryParse(match.Groups["count"].Value, out var count))
            {
                var result = new DeckCard[count];
                for (var i = 0; i < count; ++i)
                {
                    // in this format, we don't know the set or whether the card is a commander
                    result[i] = new DeckCard(match.Groups["name"].Value, set: null, collectorNumber: null, isCommander: false);
                }

                return result;
            }

            return null;
        }

        private static DeckCard[]? TryParseLineArchidekt1xCardNameCodeCategoryLabel(string line)
        {
            var match = Regex.Match(
                line,
                @"^
                    # card frequency
                    (?<count>\d+)x
                    # card name. Disallow characters used to delimit the following sections as well as trailing whitespace
                    \s+(?<name>[^\s\(`^]+(\s+[^\s\(`^]+)*)
                    # set and optionally collector number
                    (\s+\((?<set>\w+)\)(\s(?<collectorNumber>\d+[a-zA-Z]?))?)?
                    # optional foil marker
                    (\s+\*F\*)?
                    # category [x{a}{b}...] (new format) or `x` (old format)
                    (\s+[\[`](?<category>[^`\{\]]+)(\{.*?\})*[\]`])?
                    # label
                    (\s+\^(?<label>[^\^]+)\^)?\s*
                $",
                RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace
            );

            if (match.Success
                && int.TryParse(match.Groups["count"].Value, out var count))
            {
                const string CommanderCategory = "Commander",
                    MaybeboardCategory = "Maybeboard";

                var categoryGroup = match.Groups["category"];
                var category = categoryGroup.Success ? categoryGroup.Value : null;
                if (StringComparer.OrdinalIgnoreCase.Equals(category, MaybeboardCategory))
                {
                    return Array.Empty<DeckCard>(); // skip
                }
                var isCommander = StringComparer.OrdinalIgnoreCase.Equals(category, CommanderCategory);

                var setGroup = match.Groups["set"];
                var set = setGroup.Success ? setGroup.Value : null;

                var collectorNumberGroup = match.Groups["collectorNumber"];
                var collectorNumber = collectorNumberGroup.Success ? collectorNumberGroup.Value : null;

                var result = new DeckCard[count];
                for (var i = 0; i < count; ++i)
                {

                    // in this format, we don't know the set or whether the card is a commander
                    result[i] = new DeckCard(match.Groups["name"].Value, set: set, collectorNumber: collectorNumber, isCommander: isCommander);
                }

                return result;
            }

            return null;
        }

        private enum Format
        {
            Unknown,
            Archidekt1CardName,
            Archidekt1xCardNameCodeCategoryLabel,
        }
    }
}
