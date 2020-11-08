using Engine.Audio;
using Engine.Graphics;
using Engine.Input;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES20;
using System;
using System.Drawing;
using System.Reflection;

namespace Engine
{
	public static class Window
	{
		private enum State
		{
			Uncreated,
			Inactive,
			Active
		}

		internal static GameWindow m_gameWindow;

		private static bool m_closing;

		private static float m_dpiScale;

		private static int? m_swapInterval;

		private static State m_state;

		public static Point2 ScreenSize
		{
			get
			{
				DisplayDevice @default = DisplayDevice.Default;
				if (Configuration.RunningOnMacOS)
				{
					return new Point2((int)MathUtils.Round((float)@default.Width * m_dpiScale), (int)MathUtils.Round((float)@default.Height * m_dpiScale));
				}
				return new Point2(@default.Width, @default.Height);
			}
		}

		public static WindowMode WindowMode
		{
			get
			{
				VerifyWindowOpened();
				if (m_gameWindow.WindowState == WindowState.Fullscreen)
				{
					return WindowMode.Fullscreen;
				}
				if (m_gameWindow.WindowBorder != 0)
				{
					return WindowMode.Fixed;
				}
				return WindowMode.Resizable;
			}
			set
			{
				VerifyWindowOpened();
				switch (value)
				{
				case WindowMode.Fixed:
					m_gameWindow.WindowBorder = WindowBorder.Fixed;
					if (m_gameWindow.WindowState == WindowState.Fullscreen)
					{
						m_gameWindow.WindowState = WindowState.Normal;
					}
					break;
				case WindowMode.Resizable:
					m_gameWindow.WindowBorder = WindowBorder.Resizable;
					if (m_gameWindow.WindowState == WindowState.Fullscreen)
					{
						m_gameWindow.WindowState = WindowState.Normal;
					}
					break;
				case WindowMode.Borderless:
					m_gameWindow.WindowBorder = WindowBorder.Hidden;
					if (m_gameWindow.WindowState == WindowState.Fullscreen)
					{
						m_gameWindow.WindowState = WindowState.Normal;
					}
					break;
				case WindowMode.Fullscreen:
					m_gameWindow.WindowState = WindowState.Fullscreen;
					break;
				}
			}
		}

		public static Point2 Position
		{
			get
			{
				VerifyWindowOpened();
				return new Point2(m_gameWindow.Location.X, m_gameWindow.Location.Y);
			}
			set
			{
				VerifyWindowOpened();
				m_gameWindow.Location = new Point(value.X, value.Y);
			}
		}

		public static Point2 Size
		{
			get
			{
				VerifyWindowOpened();
				return new Point2(m_gameWindow.ClientSize.Width, m_gameWindow.ClientSize.Height);
			}
			set
			{
				VerifyWindowOpened();
				m_gameWindow.ClientSize = new Size(value.X, value.Y);
			}
		}

		public static string Title
		{
			get
			{
				VerifyWindowOpened();
				return m_gameWindow.Title;
			}
			set
			{
				VerifyWindowOpened();
				m_gameWindow.Title = value;
			}
		}

		public static Icon Icon
		{
			get
			{
				VerifyWindowOpened();
				return m_gameWindow.Icon;
			}
			set
			{
				VerifyWindowOpened();
				m_gameWindow.Icon = value;
			}
		}

		public static int PresentationInterval
		{
			get
			{
				VerifyWindowOpened();
				if (!m_swapInterval.HasValue)
				{
					m_swapInterval = m_gameWindow.Context.SwapInterval;
				}
				return m_swapInterval.Value;
			}
			set
			{
				VerifyWindowOpened();
				value = MathUtils.Clamp(value, 0, 4);
				if (value != PresentationInterval)
				{
					m_gameWindow.Context.SwapInterval = value;
					m_swapInterval = value;
				}
			}
		}

		public static bool IsCreated => m_state != State.Uncreated;

		public static bool IsActive => m_state == State.Active;

		public static event Action Created;

		public static event Action Resized;

		public static event Action Activated;

		public static event Action Deactivated;

		public static event Action Closed;

		public static event Action Frame;

		public static event Action<UnhandledExceptionInfo> UnhandledException;

		public static event Action<Uri> HandleUri;

		static Window()
		{
			m_dpiScale = 1f;
			Toolkit.Init();
		}

		public static void Run(int width = 0, int height = 0, WindowMode windowMode = WindowMode.Fixed, string title = "")
		{
			if (m_gameWindow != null)
			{
				throw new InvalidOperationException("Window is already opened.");
			}
			if ((width != 0 || height != 0) && (width <= 0 || height <= 0))
			{
				throw new ArgumentOutOfRangeException("size");
			}
			AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs args)
			{
				Exception ex = args.ExceptionObject as Exception;
				if (ex == null)
				{
					ex = new Exception($"Unknown exception. Additional information: {args.ExceptionObject}");
				}
				UnhandledExceptionInfo unhandledExceptionInfo = new UnhandledExceptionInfo(ex);
				if (Window.UnhandledException != null)
				{
					Window.UnhandledException(unhandledExceptionInfo);
				}
				if (!unhandledExceptionInfo.IsHandled)
				{
					Log.Error("Application terminating due to unhandled exception {0}", unhandledExceptionInfo.Exception);
					Environment.Exit(1);
				}
			};
			GraphicsMode mode = new GraphicsMode(new OpenTK.Graphics.ColorFormat(24), 16, 0, 0, OpenTK.Graphics.ColorFormat.Empty, 2);
			m_gameWindow = new GameWindow(400, 300, mode, title, GameWindowFlags.Default, DisplayDevice.Default, 2, 0, GraphicsContextFlags.Default);
			m_gameWindow.Icon = new Icon(typeof(Window).GetTypeInfo().Assembly.GetManifestResourceStream("Engine.Resources.icon.ico"),new Size(32,32));
			m_dpiScale = (float)m_gameWindow.ClientSize.Width / 400f;
			width = ((width == 0) ? (ScreenSize.X * 3 / 4) : width);
			height = ((height == 0) ? (ScreenSize.Y * 3 / 4) : height);
			m_gameWindow.ClientSize = new Size(width, height);
			if (Configuration.RunningOnMacOS)
			{
				Point2 point = new Point2((int)MathUtils.Round((float)ScreenSize.X / m_dpiScale), (int)MathUtils.Round((float)ScreenSize.Y / m_dpiScale));
				m_gameWindow.Location = new Point(MathUtils.Max((point.X - m_gameWindow.Size.Width) / 2, 0), MathUtils.Max((point.Y - m_gameWindow.Size.Height) / 2, 0));
			}
			else
			{
				m_gameWindow.Location = new Point(MathUtils.Max((ScreenSize.X - m_gameWindow.Size.Width) / 2, 0), MathUtils.Max((ScreenSize.Y - m_gameWindow.Size.Height) / 2, 0));
			}
			WindowMode = windowMode;
			m_gameWindow.Load += LoadHandler;
			GL.GetInteger(GetPName.RedBits, out int data);
			GL.GetInteger(GetPName.RedBits, out data);
			GL.GetInteger(GetPName.GreenBits, out int data2);
			GL.GetInteger(GetPName.BlueBits, out int data3);
			GL.GetInteger(GetPName.AlphaBits, out int data4);
			GL.GetInteger(GetPName.DepthBits, out int data5);
			GL.GetInteger(GetPName.StencilBits, out int data6);
			Log.Information("OpenGL framebuffer created, R={0} G={1} B={2} A={3}, D={4} S={5}", data, data2, data3, data4, data5, data6);
			m_gameWindow.Run();
		}

		public static void Close()
		{
			VerifyWindowOpened();
			m_closing = true;
		}

		private static void LoadHandler(object sender, EventArgs args)
		{
			InitializeAll();
			SubscribeToEvents();
			m_state = State.Inactive;
			if (Window.Created != null)
			{
				Window.Created();
			}
			if (m_state == State.Inactive)
			{
				m_state = State.Active;
				if (Window.Activated != null)
				{
					Window.Activated();
				}
			}
		}

		private static void FocusedChangedHandler(object sender, EventArgs args)
		{
			if (m_gameWindow.Focused)
			{
				if (m_state == State.Inactive)
				{
					m_state = State.Active;
					if (Window.Activated != null)
					{
						Window.Activated();
					}
				}
				return;
			}
			if (m_state == State.Active)
			{
				m_state = State.Inactive;
				if (Window.Deactivated != null)
				{
					Window.Deactivated();
				}
			}
			Keyboard.Clear();
			Mouse.Clear();
			Touch.Clear();
		}

		private static void ClosedHandler(object sender, EventArgs args)
		{
			if (m_state == State.Active)
			{
				m_state = State.Inactive;
				if (Window.Deactivated != null)
				{
					Window.Deactivated();
				}
			}
			if (m_state == State.Inactive)
			{
				m_state = State.Uncreated;
				if (Window.Closed != null)
				{
					Window.Closed();
				}
			}
			UnsubscribeFromEvents();
			DisposeAll();
			m_gameWindow.Dispose();
			m_gameWindow = null;
		}

		private static void ResizeHandler(object sender, EventArgs args)
		{
			Display.Resize();
			if (Window.Resized != null)
			{
				Window.Resized();
			}
		}

		private static void RenderFrameHandler(object sender, EventArgs args)
		{
			BeforeFrameAll();
			if (Window.Frame != null)
			{
				Window.Frame();
			}
			AfterFrameAll();
			if (!m_closing)
			{
				m_gameWindow.Context.SwapBuffers();
			}
			else
			{
				m_gameWindow.Close();
			}
		}

		private static void VerifyWindowOpened()
		{
			if (m_gameWindow == null)
			{
				throw new InvalidOperationException("Window is not opened.");
			}
		}

		private static void SubscribeToEvents()
		{
			m_gameWindow.FocusedChanged += FocusedChangedHandler;
			m_gameWindow.Closed += ClosedHandler;
			m_gameWindow.Resize += ResizeHandler;
			m_gameWindow.RenderFrame += RenderFrameHandler;
		}

		private static void UnsubscribeFromEvents()
		{
			m_gameWindow.FocusedChanged -= FocusedChangedHandler;
			m_gameWindow.Closed -= ClosedHandler;
			m_gameWindow.Resize -= ResizeHandler;
			m_gameWindow.RenderFrame -= RenderFrameHandler;
		}

		private static void InitializeAll()
		{
			Dispatcher.Initialize();
			Display.Initialize();
			Keyboard.Initialize();
			Mouse.Initialize();
			Touch.Initialize();
			GamePad.Initialize();
			Mixer.Initialize();
		}

		private static void DisposeAll()
		{
			Dispatcher.Dispose();
			Display.Dispose();
			Keyboard.Dispose();
			Mouse.Dispose();
			Touch.Dispose();
			GamePad.Dispose();
			Mixer.Dispose();
		}

		private static void BeforeFrameAll()
		{
			Time.BeforeFrame();
			Dispatcher.BeforeFrame();
			Display.BeforeFrame();
			Keyboard.BeforeFrame();
			Mouse.BeforeFrame();
			Touch.BeforeFrame();
			GamePad.BeforeFrame();
			Mixer.BeforeFrame();
		}

		private static void AfterFrameAll()
		{
			Time.AfterFrame();
			Dispatcher.AfterFrame();
			Display.AfterFrame();
			Keyboard.AfterFrame();
			Mouse.AfterFrame();
			Touch.AfterFrame();
			GamePad.AfterFrame();
			Mixer.AfterFrame();
		}
	}
}
