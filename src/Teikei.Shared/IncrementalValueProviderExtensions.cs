using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

public static class IncrementalValueProviderExtensions
{
	public static IncrementalValuesProvider<TSource> WithValue<TSource>(
		this IncrementalValuesProvider<TSource?> source
	)
	{
		return source.SelectMany(
			(TSource? item, CancellationToken _) =>
				(item is null) ? ImmutableArray<TSource>.Empty : ImmutableArray.Create(item)
		);
	}
}
