namespace FluentFeeds.Shared.Models;

public class Feed
{
	public Feed(string name, Symbol symbol)
	{
		Name = name;
		Symbol = symbol;
	}

	public string Name { get; }

	public Symbol Symbol { get; }
}
