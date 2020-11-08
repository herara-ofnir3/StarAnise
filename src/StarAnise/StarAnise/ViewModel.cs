using System;
using System.Reactive.Disposables;

namespace StarAnise
{
	public abstract class ViewModel : IDisposable
	{
		protected CompositeDisposable DisposeBag = new CompositeDisposable();
		private bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					DisposeBag?.Dispose();
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			// このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
