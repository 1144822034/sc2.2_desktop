using Engine.Media;
using OpenTK.Graphics.ES20;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Engine.Graphics
{
	public class Texture2D : GraphicsResource
	{
		internal int m_texture;

		private All m_pixelFormat;

		private All m_pixelType;

		public IntPtr NativeHandle => (IntPtr)m_texture;

		public string DebugName
		{
			get
			{
				return string.Empty;
			}
			set
			{
			}
		}

		public int Width
		{
			get;
			private set;
		}

		public int Height
		{
			get;
			private set;
		}

		public ColorFormat ColorFormat
		{
			get;
			private set;
		}

		public int MipLevelsCount
		{
			get;
			private set;
		}

		public object Tag
		{
			get;
			set;
		}

		public Texture2D(int width, int height, int mipLevelsCount, ColorFormat colorFormat)
		{
			InitializeTexture2D(width, height, mipLevelsCount, colorFormat);
			switch (ColorFormat)
			{
			case ColorFormat.Rgba8888:
				m_pixelFormat = All.Rgba;
				m_pixelType = All.UnsignedByte;
				break;
			case ColorFormat.Rgb565:
				m_pixelFormat = All.Rgb;
				m_pixelType = All.UnsignedShort565;
				break;
			case ColorFormat.Rgba5551:
				m_pixelFormat = All.Rgba;
				m_pixelType = All.UnsignedShort5551;
				break;
			case ColorFormat.R8:
				m_pixelFormat = All.Luminance;
				m_pixelType = All.UnsignedByte;
				break;
			default:
				throw new InvalidOperationException("Unsupported surface format.");
			}
			AllocateTexture();
		}

		public override void Dispose()
		{
			base.Dispose();
			DeleteTexture();
		}

		public void SetData<T>(int mipLevel, T[] source, int sourceStartIndex = 0) where T : struct
		{
			VerifyParametersSetData(mipLevel, source, sourceStartIndex);
			GCHandle gCHandle = GCHandle.Alloc(source, GCHandleType.Pinned);
			try
			{
				int width = MathUtils.Max(Width >> mipLevel, 1);
				int height = MathUtils.Max(Height >> mipLevel, 1);
				IntPtr pixels = gCHandle.AddrOfPinnedObject() + sourceStartIndex * Utilities.SizeOf<T>();
				GLWrapper.BindTexture(All.Texture2D, m_texture, forceBind: false);
				GL.TexImage2D(All.Texture2D, mipLevel, m_pixelFormat, width, height, 0, m_pixelFormat, m_pixelType, pixels);
			}
			finally
			{
				gCHandle.Free();
			}
		}

		internal override void HandleDeviceLost()
		{
			DeleteTexture();
		}

		internal override void HandleDeviceReset()
		{
			AllocateTexture();
		}

		private void AllocateTexture()
		{
			GL.GenTextures(1, out m_texture);
			GLWrapper.BindTexture(All.Texture2D, m_texture, forceBind: false);
			for (int i = 0; i < MipLevelsCount; i++)
			{
				int width = MathUtils.Max(Width >> i, 1);
				int height = MathUtils.Max(Height >> i, 1);
				GL.TexImage2D(All.Texture2D, i, m_pixelFormat, width, height, 0, m_pixelFormat, m_pixelType, IntPtr.Zero);
			}
		}

		private void DeleteTexture()
		{
			if (m_texture != 0)
			{
				GLWrapper.DeleteTexture(m_texture);
				m_texture = 0;
			}
		}

		public override int GetGpuMemoryUsage()
		{
			int num = 0;
			for (int i = 0; i < MipLevelsCount; i++)
			{
				int num2 = MathUtils.Max(Width >> i, 1);
				int num3 = MathUtils.Max(Height >> i, 1);
				num += ColorFormat.GetSize() * num2 * num3;
			}
			return num;
		}

		public static Texture2D Load(Image image, int mipLevelsCount = 1)
		{
			Texture2D texture2D = new Texture2D(image.Width, image.Height, mipLevelsCount, ColorFormat.Rgba8888);
			if (mipLevelsCount > 1)
			{
				Image[] array = Image.GenerateMipmaps(image, mipLevelsCount).ToArray();
				for (int i = 0; i < array.Length; i++)
				{
					texture2D.SetData(i, array[i].Pixels);
				}
			}
			else
			{
				texture2D.SetData(0, image.Pixels);
			}
			return texture2D;
		}

		public static Texture2D Load(Stream stream, bool premultiplyAlpha = false, int mipLevelsCount = 1)
		{
			Image image = Image.Load(stream);
			if (premultiplyAlpha)
			{
				Image.PremultiplyAlpha(image);
			}
			return Load(image, mipLevelsCount);
		}

		public static Texture2D Load(string fileName, bool premultiplyAlpha = false, int mipLevelsCount = 1)
		{
			using (Stream stream = Storage.OpenFile(fileName, OpenFileMode.Read))
			{
				return Load(stream, premultiplyAlpha, mipLevelsCount);
			}
		}

		internal void InitializeTexture2D(int width, int height, int mipLevelsCount, ColorFormat colorFormat)
		{
			if (width < 1)
			{
				throw new ArgumentOutOfRangeException("width");
			}
			if (height < 1)
			{
				throw new ArgumentOutOfRangeException("height");
			}
			if (mipLevelsCount < 1)
			{
				throw new ArgumentOutOfRangeException("mipLevelsCount");
			}
			Width = width;
			Height = height;
			ColorFormat = colorFormat;
			if (mipLevelsCount > 1)
			{
				int num = 0;
				for (int num2 = MathUtils.Max(width, height); num2 >= 1; num2 /= 2)
				{
					num++;
				}
				MipLevelsCount = MathUtils.Min(num, mipLevelsCount);
			}
			else
			{
				MipLevelsCount = 1;
			}
		}

		private void VerifyParametersSetData<T>(int mipLevel, T[] source, int sourceStartIndex = 0) where T : struct
		{
			VerifyNotDisposed();
			int num = Utilities.SizeOf<T>();
			int size = ColorFormat.GetSize();
			int num2 = MathUtils.Max(Width >> mipLevel, 1);
			int num3 = MathUtils.Max(Height >> mipLevel, 1);
			int num4 = size * num2 * num3;
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (mipLevel < 0 || mipLevel >= MipLevelsCount)
			{
				throw new ArgumentOutOfRangeException("mipLevel");
			}
			if (num > size)
			{
				throw new ArgumentNullException("Source array element size is larger than pixel size.");
			}
			if (size % num != 0)
			{
				throw new ArgumentNullException("Pixel size is not an integer multiple of source array element size.");
			}
			if (sourceStartIndex < 0 || (source.Length - sourceStartIndex) * num < num4)
			{
				throw new InvalidOperationException("Not enough data in source array.");
			}
		}
	}
}
