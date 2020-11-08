using Engine;
using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Game
{
	public static class Program
	{
		private static double m_frameBeginTime;

		private static double m_cpuEndTime;

		private static List<Uri> m_urisToHandle = new List<Uri>();

		public static float LastFrameTime
		{
			get;
			set;
		}

		public static float LastCpuFrameTime
		{
			get;
			set;
		}

		public static event Action<Uri> HandleUri;

		[STAThread]
		public static void Main()
		{
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
			CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
			Log.RemoveAllLogSinks();
			Log.AddLogSink(new GameLogSink());
			Window.HandleUri += HandleUriHandler;
			Window.Deactivated += DeactivatedHandler;
			Window.Frame += FrameHandler;
			Window.UnhandledException += delegate(UnhandledExceptionInfo e)
			{
				ExceptionManager.ReportExceptionToUser("Unhandled exception.", e.Exception);
				e.IsHandled = true;
			};
			Window.Run(1920, 1080, Engine.WindowMode.Resizable, "生存战争2.2插件版V1.13b4");
		}

		private static void HandleUriHandler(Uri uri)
		{
			m_urisToHandle.Add(uri);
		}

		private static void DeactivatedHandler()
		{
			GC.Collect();
			ModsManager.SaveSettings();
		}

		private static void FrameHandler()
		{
			if (Time.FrameIndex < 0)
			{
				Display.Clear(Vector4.Zero, 1f);
			}
			else if (Time.FrameIndex == 0)
			{
				Initialize();
			}
			else
			{
				Run();
			}
		}

		private static void Initialize()
		{
			Log.Information($"Survivalcraft starting up at {DateTime.Now}, Version={VersionsManager.Version}, BuildConfiguration={VersionsManager.BuildConfiguration}, Platform={VersionsManager.Platform}, Storage.AvailableFreeSpace={Storage.FreeSpace / 1024 / 1024}MB, ApproximateScreenDpi={ScreenResolutionManager.ApproximateScreenDpi:0.0}, ApproxScreenInches={ScreenResolutionManager.ApproximateScreenInches:0.0}, ScreenResolution={Window.Size}, ProcessorsCount={Environment.ProcessorCount}, RAM={Utilities.GetTotalAvailableMemory() / 1024 / 1024}MB, 64bit={Marshal.SizeOf<IntPtr>() == 8}");
			MarketplaceManager.Initialize();
			SettingsManager.Initialize();
			AnalyticsManager.Initialize();
			VersionsManager.Initialize();
			ExternalContentManager.Initialize();
			ContentManager.Initialize();
			ScreensManager.Initialize();
		}

		private static void Run()
		{
			VrManager.WaitGetPoses();
			double realTime = Time.RealTime;
			LastFrameTime = (float)(realTime - m_frameBeginTime);
			LastCpuFrameTime = (float)(m_cpuEndTime - m_frameBeginTime);
			m_frameBeginTime = realTime;
			Window.PresentationInterval = ((!VrManager.IsVrStarted) ? SettingsManager.PresentationInterval : 0);
			try
			{
				if (ExceptionManager.Error == null)
				{
					while (m_urisToHandle.Count > 0)
					{
						Uri obj = m_urisToHandle[0];
						m_urisToHandle.RemoveAt(0);
						Program.HandleUri?.Invoke(obj);
					}
					PerformanceManager.Update();
					MotdManager.Update();
					MusicManager.Update();
					ScreensManager.Update();
					DialogsManager.Update();
				}
				else
				{
					ExceptionManager.UpdateExceptionScreen();
				}
			}
			catch (Exception e)
			{
				ExceptionManager.ReportExceptionToUser(null, e);
				ScreensManager.SwitchScreen("MainMenu");
			}
			try
			{
				Display.RenderTarget = null;
				if (ExceptionManager.Error == null)
				{
					ScreensManager.Draw();
					PerformanceManager.Draw();
					ScreenCaptureManager.Run();
				}
				else
				{
					ExceptionManager.DrawExceptionScreen();
				}
				m_cpuEndTime = Time.RealTime;
			}
			catch (Exception e2)
			{
				ExceptionManager.ReportExceptionToUser(null, e2);
				ScreensManager.SwitchScreen("MainMenu");
			}
		}
	}
}
