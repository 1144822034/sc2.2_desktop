using System;

namespace FluxJpeg.Core.Filtering
{
	internal abstract class Filter
	{
		protected int _newWidth;

		protected int _newHeight;

		protected byte[][,] _sourceData;

		protected byte[][,] _destinationData;

		protected bool _color;

		private FilterProgressEventArgs progressArgs = new FilterProgressEventArgs();

		public event EventHandler<FilterProgressEventArgs> ProgressChanged;

		protected void UpdateProgress(double progress)
		{
			progressArgs.Progress = progress;
			if (this.ProgressChanged != null)
			{
				this.ProgressChanged(this, progressArgs);
			}
		}

		public byte[][,] Apply(byte[][,] imageData, int newWidth, int newHeight)
		{
			_newHeight = newHeight;
			_newWidth = newWidth;
			_color = (imageData.Length != 1);
			_destinationData = Image.CreateRaster(newWidth, newHeight, imageData.Length);
			_sourceData = imageData;
			ApplyFilter();
			return _destinationData;
		}

		protected abstract void ApplyFilter();
	}
}
