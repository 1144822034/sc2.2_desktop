using System;
using System.Threading;

namespace Game
{
	public class CancellableProgress : Progress
	{
		public readonly CancellationToken CancellationToken;

		private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

		public event Action Cancelled;

		public CancellableProgress()
		{
			CancellationToken = CancellationTokenSource.Token;
		}

		public void Cancel()
		{
			CancellationTokenSource.Cancel();
			this.Cancelled?.Invoke();
		}
	}
}
