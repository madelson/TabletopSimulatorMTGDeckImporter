<Query Kind="Program">
  <NuGetReference Version="13.0.3">Newtonsoft.Json</NuGetReference>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>Newtonsoft.Json.Linq</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main(string[] args)
{
	ParseArgs(args, out var inputFile, out var colorIdentityFilter);
	
	var inputFileCards = ParseInputFile(inputFile);	
	
	var allCards = await GetAllCardsAsync();	
	var allCardsByName = allCards.ToLookup(c => c.name);
		
	var mappedInputFileCards = inputFileCards.Select(t => (
			t.Name, 
			t.Set,
			t.Count,
			Card: (allCardsByName.Contains(t.Name) ? allCardsByName[t.Name] : allCardsByName.Where(g => g.Key.StartsWith($"{t.Name} //")).SelectMany(t => t))
				.First(c => c.set == t.Set)
		))
		.ToArray();
		
	var primaryColorIdentities = mappedInputFileCards.Select(t => t.Card.ColorIdentity)
		.GroupBy(ci => ci)
		.Where(g => g.Count() >= 10)
		.Select(g => g.Key)
		.ToHashSet();

	var outputContents = string.Join(
		Environment.NewLine,
		mappedInputFileCards
			.Where(t => !colorIdentityFilter.HasValue || (t.Card.ColorIdentity & colorIdentityFilter.Value) == t.Card.ColorIdentity)
			.Select(t => $"{t.Count}x {t.Card.name} ({t.Set}) {t.Card.collector_number} [{GetCategory(t.Card)}]")
	);
	var outputPath = Path.GetFullPath("./cards.txt");
	File.WriteAllText(outputPath, outputContents);
	Console.WriteLine($"Cards written to {outputPath}");
	
	string GetCategory(Card card) =>
		colorIdentityFilter.HasValue ? "Maybeboard"
			: primaryColorIdentities.Contains(card.ColorIdentity) ? ToString(card.ColorIdentity)
			: "Other";
}

static (string Name, string Set, int Count)[] ParseInputFile(string inputFile)
{
	return File.ReadLines(inputFile)
		.Where(l => l.Trim().Length > 0)
		.Select(l => Regex.Match(l, @"^\s*(?<count>\d+)\s+\[(?<set>\w+):.*?]\s+(?<name>.*?)\s*(\[.*?\]\s*)?$"))
		.Where(m => m.Success ? true : throw new FormatException("Malformed line"))
		.Select(m => (Name: m.Groups["name"].Value, Set: m.Groups["set"].Value.ToLowerInvariant(), Count: int.Parse(m.Groups["count"].Value)))
		.GroupBy(c => c.Name)
		.Select(g => (g.Key, g.First().Set, g.Sum(c => c.Count)))
		.ToArray();
}

static void ParseArgs(string[] args, out string inputFile, out ColorIdentity? colorIdentityFilter)
{
	if (args.Length is not (1 or 2))
	{
		Console.Error.WriteLine($"Usage: lprun7 {Path.GetFileName(Util.CurrentQueryPath)} <inputFile> [colorIdentityFilter]");
		Environment.Exit(-1);
	}
	
	inputFile = Path.GetFullPath(args[0]);
	if (!File.Exists(inputFile))
	{
		Console.Error.WriteLine($"File '{inputFile}' does not exist");
		Environment.Exit(-2);
	}
	
	if (args.Length == 1)
	{
		colorIdentityFilter = null;
		return;
	}
	
	var colorIdentityFilterArg = args[1].ToUpperInvariant();
	colorIdentityFilter = colorIdentityFilterArg == "C"
		? ColorIdentity.C
		: GetColorIdentity(colorIdentityFilterArg.ToCharArray());
}

static ColorIdentity GetColorIdentity(char[] array)
{
	var result = ColorIdentity.C;
	foreach (var ch in array)
	{
		result |= ch switch {
			'W' => ColorIdentity.W,
			'U' => ColorIdentity.U,
			'B' => ColorIdentity.B,
			'R' => ColorIdentity.R,
			'G' => ColorIdentity.G,
			_ => throw new FormatException($"Unexpected color identity '{ch}'")
		};
	}
	return result;
}

static string ToString(ColorIdentity colorIdentity) => colorIdentity.ToString().Replace(", ", string.Empty);

static async Task<Card[]> GetAllCardsAsync()
{
	var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "scryfall-default-cards.json");
	if (!File.Exists(path) || (DateTime.UtcNow - File.GetLastWriteTimeUtc(path)) >= TimeSpan.FromDays(7))
	{
		await DownloadCardsAsync(path);
	}

	using var reader = new JsonTextReader(new StreamReader(path));
	return new JsonSerializer().Deserialize<IEnumerable<Card>>(reader)
		.Where(c => c.layout is not ("planar" or "scheme" or "vanguard" or "token" or "double_faced_token" or "emblem" or "art_series"))
		.ToArray();
}

static async Task DownloadCardsAsync(string path)
{
	Console.WriteLine("Downloading all cards from Scryfall. This may take a bit...");
	
	using HttpClient client = new();
	using var bulkDataResponse = await client.GetAsync("https://api.scryfall.com/bulk-data");
	if (bulkDataResponse.StatusCode != HttpStatusCode.OK) 
	{ 
		throw new WebException($"Bulk data info request failed with status code {bulkDataResponse.StatusCode}"); 
	}

	var bulkDataInfoJson = await bulkDataResponse.Content.ReadAsStringAsync();
	var bulkDataInfo = JsonConvert.DeserializeAnonymousType(
		bulkDataInfoJson, 
		new { data = new[] { new { type = default(string)!, download_uri = default(string)! } } });
	var bulkDataUri = bulkDataInfo.data.Single(d => d.type == "default_cards").download_uri;

	using var defaultCardsResponse = await client.GetAsync(bulkDataUri);
	if (defaultCardsResponse.StatusCode != HttpStatusCode.OK)
	{
		throw new WebException($"Bulk data download failed with status code {defaultCardsResponse.StatusCode}");
	}
	
	await using var fileStream = File.OpenWrite(path);
	await (await defaultCardsResponse.Content.ReadAsStreamAsync()).CopyToAsync(fileStream);
}

class Card
{
	public string name;
	public string set;
	public string layout;
	public char[] color_identity;
	public string collector_number;
	
	private ColorIdentity? _colorIdentity;
	
	public ColorIdentity ColorIdentity => this._colorIdentity ??= GetColorIdentity(this.color_identity);
}

[Flags]
enum ColorIdentity
{
	C = 0,
	W = 1 << 0,
	U = 1 << 1,
	B = 1 << 2,
	R = 1 << 3,
	G = 1 << 4,
}
