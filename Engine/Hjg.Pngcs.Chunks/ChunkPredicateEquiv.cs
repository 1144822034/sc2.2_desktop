namespace Hjg.Pngcs.Chunks
{
	internal class ChunkPredicateEquiv : ChunkPredicate
	{
		private readonly PngChunk chunk;

		public ChunkPredicateEquiv(PngChunk chunk)
		{
			this.chunk = chunk;
		}

		public bool Matches(PngChunk c)
		{
			return ChunkHelper.Equivalent(c, chunk);
		}
	}
}
