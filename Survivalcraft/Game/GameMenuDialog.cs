using Engine;
using Engine.Graphics;
using Engine.Media;
using GameEntitySystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Game
{
	public class GameMenuDialog : Dialog
	{
		public static bool m_increaseDetailDialogShown;

		public static bool m_decreaseDetailDialogShown;

		public bool m_adventureRestartExists;

		public StackPanelWidget m_statsPanel;

		public ComponentPlayer m_componentPlayer;

		public GameMenuDialog(ComponentPlayer componentPlayer)
		{
			XElement node = ContentManager.Get<XElement>("Dialogs/GameMenuDialog");
			LoadContents(this, node);
			m_statsPanel = Children.Find<StackPanelWidget>("StatsPanel");
			m_componentPlayer = componentPlayer;
			m_adventureRestartExists = WorldsManager.SnapshotExists(GameManager.WorldInfo.DirectoryName, "AdventureRestart");
			if (!m_increaseDetailDialogShown && PerformanceManager.LongTermAverageFrameTime.HasValue && PerformanceManager.LongTermAverageFrameTime.Value * 1000f < 25f && (SettingsManager.VisibilityRange <= 64 || SettingsManager.ResolutionMode == ResolutionMode.Low))
			{
				m_increaseDetailDialogShown = true;
				DialogsManager.ShowDialog(base.ParentWidget, new MessageDialog("你的设备性能充足", "考虑增加可视范围或分辨率以获得更好的图形效果。为此，请转到性能设置。", "确定", null, null));
				AnalyticsManager.LogEvent("[GameMenuScreen] IncreaseDetailDialog Shown");
			}
			if (!m_decreaseDetailDialogShown && PerformanceManager.LongTermAverageFrameTime.HasValue && PerformanceManager.LongTermAverageFrameTime.Value * 1000f > 50f && (SettingsManager.VisibilityRange >= 64 || SettingsManager.ResolutionMode == ResolutionMode.High))
			{
				m_decreaseDetailDialogShown = true;
				DialogsManager.ShowDialog(base.ParentWidget, new MessageDialog("你的设备性能充足", "考虑增加可视范围或分辨率以获得更好的图形效果。为此，请转到性能设置。", "确定", null, null));
				AnalyticsManager.LogEvent("[GameMenuScreen] DecreaseDetailDialog Shown");
			}
			m_statsPanel.Children.Clear();
			Project project = componentPlayer.Project;
			PlayerData playerData = componentPlayer.PlayerData;
			PlayerStats playerStats = componentPlayer.PlayerStats;
			SubsystemGameInfo subsystemGameInfo = project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
			SubsystemFurnitureBlockBehavior subsystemFurnitureBlockBehavior = project.FindSubsystem<SubsystemFurnitureBlockBehavior>(throwOnError: true);
			BitmapFont font = ContentManager.Get<BitmapFont>("Fonts/Pericles");
			BitmapFont font2 = ContentManager.Get<BitmapFont>("Fonts/Pericles");
			Color white = Color.White;
			StackPanelWidget stackPanelWidget = new StackPanelWidget
			{
				Direction = LayoutDirection.Vertical,
				HorizontalAlignment = WidgetAlignment.Center
			};
			m_statsPanel.Children.Add(stackPanelWidget);
			stackPanelWidget.Children.Add(new LabelWidget
			{
				Text = "游戏统计",
				Font = font,
				HorizontalAlignment = WidgetAlignment.Center,
				Margin = new Vector2(0f, 10f),
				Color = white
			});
			AddStat(stackPanelWidget, "游戏模式", LanguageControl.Get("GameMode",subsystemGameInfo.WorldSettings.GameMode.ToString()) + ", " +LanguageControl.Get("EnvironmentBehaviorMode", subsystemGameInfo.WorldSettings.EnvironmentBehaviorMode.ToString()));
			AddStat(stackPanelWidget, "地形类型", StringsManager.GetString("TerrainGenerationMode." + subsystemGameInfo.WorldSettings.TerrainGenerationMode.ToString() + ".Name"));
			string seed = subsystemGameInfo.WorldSettings.Seed;
			AddStat(stackPanelWidget, "世界种子", (!string.IsNullOrEmpty(seed)) ? seed : "(没有)");
			AddStat(stackPanelWidget, "海平面", WorldOptionsScreen.FormatOffset(subsystemGameInfo.WorldSettings.SeaLevelOffset));
			AddStat(stackPanelWidget, "温度", WorldOptionsScreen.FormatOffset(subsystemGameInfo.WorldSettings.TemperatureOffset));
			AddStat(stackPanelWidget, "湿度", WorldOptionsScreen.FormatOffset(subsystemGameInfo.WorldSettings.HumidityOffset));
			AddStat(stackPanelWidget, "生物群落大小", subsystemGameInfo.WorldSettings.BiomeSize.ToString() + "x");
			int num = 0;
			for (int i = 0; i < 1024; i++)
			{
				if (subsystemFurnitureBlockBehavior.GetDesign(i) != null)
				{
					num++;
				}
			}
			AddStat(stackPanelWidget, "家具使用数", $"{num}/{1024}");
			AddStat(stackPanelWidget, "创建的世界版本", string.IsNullOrEmpty(subsystemGameInfo.WorldSettings.OriginalSerializationVersion) ? "在1.22前" : subsystemGameInfo.WorldSettings.OriginalSerializationVersion);
			stackPanelWidget.Children.Add(new LabelWidget
			{
				Text = "玩家统计",
				Font = font,
				HorizontalAlignment = WidgetAlignment.Center,
				Margin = new Vector2(0f, 10f),
				Color = white
			});
			AddStat(stackPanelWidget, "名字", playerData.Name);
			AddStat(stackPanelWidget, "性别", playerData.PlayerClass.ToString());
			string value = (playerData.FirstSpawnTime >= 0.0) ? (((subsystemGameInfo.TotalElapsedGameTime - playerData.FirstSpawnTime) / 1200.0).ToString("N1") + " 天前") : "从未生成";
			AddStat(stackPanelWidget, "首次创建", value);
			string value2 = (playerData.LastSpawnTime >= 0.0) ? (((subsystemGameInfo.TotalElapsedGameTime - playerData.LastSpawnTime) / 1200.0).ToString("N1") + " 天") : "从未生成";
			AddStat(stackPanelWidget, "存活", value2);
			AddStat(stackPanelWidget, "重生", MathUtils.Max(playerData.SpawnsCount - 1, 0).ToString("N0") + " 次");
			AddStat(stackPanelWidget, "最高达到等级", "等级 " + ((int)MathUtils.Floor(playerStats.HighestLevel)).ToString("N0"));
			if (componentPlayer != null)
			{
				Vector3 position = componentPlayer.ComponentBody.Position;
				if (subsystemGameInfo.WorldSettings.GameMode == GameMode.Creative)
				{
					AddStat(stackPanelWidget, "位置", $"{position.X:0}, {position.Z:0} 海拔: {position.Y:0}");
				}
				else
				{
					AddStat(stackPanelWidget, "位置", "(在" + subsystemGameInfo.WorldSettings.GameMode.ToString() + "模式下不可用)");
				}
			}
			if (string.CompareOrdinal(subsystemGameInfo.WorldSettings.OriginalSerializationVersion, "1.29") > 0)
			{
				stackPanelWidget.Children.Add(new LabelWidget
				{
					Text = "战斗统计",
					Font = font,
					HorizontalAlignment = WidgetAlignment.Center,
					Margin = new Vector2(0f, 10f),
					Color = white
				});
				AddStat(stackPanelWidget, "被玩家击杀", playerStats.PlayerKills.ToString("N0"));
				AddStat(stackPanelWidget, "被陆地生物击杀", playerStats.LandCreatureKills.ToString("N0"));
				AddStat(stackPanelWidget, "被水生生物击杀", playerStats.WaterCreatureKills.ToString("N0"));
				AddStat(stackPanelWidget, "被空中生物击杀", playerStats.AirCreatureKills.ToString("N0"));
				AddStat(stackPanelWidget, "近战攻击次数", playerStats.MeleeAttacks.ToString("N0"));
				AddStat(stackPanelWidget, "近战命中次数", playerStats.MeleeHits.ToString("N0"), $"({((playerStats.MeleeHits == 0L) ? 0.0 : ((double)playerStats.MeleeHits / (double)playerStats.MeleeAttacks * 100.0)):0}%)");
				AddStat(stackPanelWidget, "远程攻击次数", playerStats.RangedAttacks.ToString("N0"));
				AddStat(stackPanelWidget, "远程命中次数", playerStats.RangedHits.ToString("N0"), $"({((playerStats.RangedHits == 0L) ? 0.0 : ((double)playerStats.RangedHits / (double)playerStats.RangedAttacks * 100.0)):0}%)");
				AddStat(stackPanelWidget, "收到击中数", playerStats.HitsReceived.ToString("N0"));
				stackPanelWidget.Children.Add(new LabelWidget
				{
					Text = "工作统计",
					Font = font,
					HorizontalAlignment = WidgetAlignment.Center,
					Margin = new Vector2(0f, 10f),
					Color = white
				});
				AddStat(stackPanelWidget, "挖掘次数", playerStats.BlocksDug.ToString("N0"));
				AddStat(stackPanelWidget, "放置次数", playerStats.BlocksPlaced.ToString("N0"));
				AddStat(stackPanelWidget, "交互次数", playerStats.BlocksInteracted.ToString("N0"));
				AddStat(stackPanelWidget, "制造次数", playerStats.ItemsCrafted.ToString("N0"));
				AddStat(stackPanelWidget, "制造家具个数", playerStats.FurnitureItemsMade.ToString("N0"));
				stackPanelWidget.Children.Add(new LabelWidget
				{
					Text = "移动统计",
					Font = font,
					HorizontalAlignment = WidgetAlignment.Center,
					Margin = new Vector2(0f, 10f),
					Color = white
				});
				AddStat(stackPanelWidget, "总旅行距离", FormatDistance(playerStats.DistanceTravelled));
				AddStat(stackPanelWidget, "步行距离", FormatDistance(playerStats.DistanceWalked), $"({((playerStats.DistanceTravelled > 0.0) ? (playerStats.DistanceWalked / playerStats.DistanceTravelled * 100.0) : 0.0):0.0}%)");
				AddStat(stackPanelWidget, "掉落距离", FormatDistance(playerStats.DistanceFallen), $"({((playerStats.DistanceTravelled > 0.0) ? (playerStats.DistanceFallen / playerStats.DistanceTravelled * 100.0) : 0.0):0.0}%)");
				AddStat(stackPanelWidget, "爬行距离", FormatDistance(playerStats.DistanceClimbed), $"({((playerStats.DistanceTravelled > 0.0) ? (playerStats.DistanceClimbed / playerStats.DistanceTravelled * 100.0) : 0.0):0.0}%)");
				AddStat(stackPanelWidget, "飞行距离", FormatDistance(playerStats.DistanceFlown), $"({((playerStats.DistanceTravelled > 0.0) ? (playerStats.DistanceFlown / playerStats.DistanceTravelled * 100.0) : 0.0):0.0}%)");
				AddStat(stackPanelWidget, "游泳距离", FormatDistance(playerStats.DistanceSwam), $"({((playerStats.DistanceTravelled > 0.0) ? (playerStats.DistanceSwam / playerStats.DistanceTravelled * 100.0) : 0.0):0.0}%)");
				AddStat(stackPanelWidget, "长途旅行距离", FormatDistance(playerStats.DistanceRidden), $"({((playerStats.DistanceTravelled > 0.0) ? (playerStats.DistanceRidden / playerStats.DistanceTravelled * 100.0) : 0.0):0.0}%)");
				AddStat(stackPanelWidget, "最低海拔", FormatDistance(playerStats.LowestAltitude));
				AddStat(stackPanelWidget, "最高海拔", FormatDistance(playerStats.HighestAltitude));
				AddStat(stackPanelWidget, "最深潜水", playerStats.DeepestDive.ToString("N1") + "m");
				AddStat(stackPanelWidget, "跳跃次数", playerStats.Jumps.ToString("N0"));
				stackPanelWidget.Children.Add(new LabelWidget
				{
					Text = "身体统计",
					Font = font,
					HorizontalAlignment = WidgetAlignment.Center,
					Margin = new Vector2(0f, 10f),
					Color = white
				});
				AddStat(stackPanelWidget, "总损失生命值", (playerStats.TotalHealthLost * 100.0).ToString("N0") + "%");
				AddStat(stackPanelWidget, "吃掉食物数", playerStats.FoodItemsEaten.ToString("N0") + " 次");
				AddStat(stackPanelWidget, "睡觉次数", playerStats.TimesWentToSleep.ToString("N0") + " 次");
				AddStat(stackPanelWidget, "睡觉累计时间", (playerStats.TimeSlept / 1200.0).ToString("N1") + " 天");
				AddStat(stackPanelWidget, "生病次数", playerStats.TimesWasSick.ToString("N0") + " 次");
				AddStat(stackPanelWidget, "呕吐次数", playerStats.TimesPuked.ToString("N0") + " 次");
				AddStat(stackPanelWidget, "感染流感次数", playerStats.TimesHadFlu.ToString("N0") + " 次");
				stackPanelWidget.Children.Add(new LabelWidget
				{
					Text = "其它统计",
					Font = font,
					HorizontalAlignment = WidgetAlignment.Center,
					Margin = new Vector2(0f, 10f),
					Color = white
				});
				AddStat(stackPanelWidget, "被闪电击中", playerStats.StruckByLightning.ToString("N0") + " 次");
				GameMode easiestModeUsed = playerStats.EasiestModeUsed;
				AddStat(stackPanelWidget, "使用最简单的游戏模式", easiestModeUsed.ToString());
				if (playerStats.DeathRecords.Count > 0)
				{
					stackPanelWidget.Children.Add(new LabelWidget
					{
						Text = "死亡记录",
						Font = font,
						HorizontalAlignment = WidgetAlignment.Center,
						Margin = new Vector2(0f, 10f),
						Color = white
					});
					foreach (PlayerStats.DeathRecord deathRecord in playerStats.DeathRecords)
					{
						float num2 = (float)MathUtils.Remainder(deathRecord.Day, 1.0);
						string arg = (!(num2 < 0.2f) && !(num2 >= 0.8f)) ? ((!(num2 >= 0.7f)) ? ((!(num2 >= 0.5f)) ? "早上的" : "中午的") : "晚上的") : "深夜的";
						AddStat(stackPanelWidget, string.Format("{1} 天 {0:0}", MathUtils.Floor(deathRecord.Day) + 1.0, arg), "", deathRecord.Cause);
					}
				}
			}
			else
			{
				stackPanelWidget.Children.Add(new LabelWidget
				{
					Text = "没有可用的玩家统计，因为这来自于一个很旧的游戏存档",
					WordWrap = true,
					Font = font2,
					HorizontalAlignment = WidgetAlignment.Center,
					TextAnchor = TextAnchor.HorizontalCenter,
					Margin = new Vector2(20f, 10f),
					Color = white
				});
			}
		}

		public override void Update()
		{
			if (Children.Find<ButtonWidget>("More").IsClicked)
			{
				List<Tuple<string, Action>> list = new List<Tuple<string, Action>>();
				if (m_adventureRestartExists && GameManager.WorldInfo.WorldSettings.GameMode == GameMode.Adventure)
				{
					list.Add(new Tuple<string, Action>("重置冒险", delegate
					{
						DialogsManager.ShowDialog(base.ParentWidget, new MessageDialog("重置冒险？", "冒险将重新开始", LanguageControl.Get("Usual","yes"), LanguageControl.Get("Usual","no"), delegate (MessageDialogButton result)
						{
							if (result == MessageDialogButton.Button1)
							{
								ScreensManager.SwitchScreen("GameLoading", GameManager.WorldInfo, "AdventureRestart");
							}
						}));
					}));
				}
				if (GetRateableItems().FirstOrDefault() != null && UserManager.ActiveUser != null)
				{
					list.Add(new Tuple<string, Action>("评分", delegate
					{
						DialogsManager.ShowDialog(base.ParentWidget, new ListSelectionDialog("选择要评分的内容", GetRateableItems(), 60f, (object o) => ((ActiveExternalContentInfo)o).DisplayName, delegate (object o)
						{
							ActiveExternalContentInfo activeExternalContentInfo = (ActiveExternalContentInfo)o;
							DialogsManager.ShowDialog(base.ParentWidget, new RateCommunityContentDialog(activeExternalContentInfo.Address, activeExternalContentInfo.DisplayName, UserManager.ActiveUser.UniqueId));
						}));
					}));
				}
				list.Add(new Tuple<string, Action>("编辑玩家", delegate
				{
					ScreensManager.SwitchScreen("Players", m_componentPlayer.Project.FindSubsystem<SubsystemPlayers>(throwOnError: true));
				}));
				list.Add(new Tuple<string, Action>("设置", delegate
				{
					ScreensManager.SwitchScreen("Settings");
				}));
				list.Add(new Tuple<string, Action>("帮助", delegate
				{
					ScreensManager.SwitchScreen("Help");
				}));
				if ((base.Input.Devices & (WidgetInputDevice.Keyboard | WidgetInputDevice.Mouse)) != 0)
				{
					list.Add(new Tuple<string, Action>("按键设置", delegate
					{
						DialogsManager.ShowDialog(base.ParentWidget, new KeyboardHelpDialog());
					}));
				}
				if ((base.Input.Devices & WidgetInputDevice.Gamepads) != 0)
				{
					list.Add(new Tuple<string, Action>("触摸板控制", delegate
					{
						DialogsManager.ShowDialog(base.ParentWidget, new GamepadHelpDialog());
					}));
				}
				ListSelectionDialog dialog = new ListSelectionDialog("更多选项", list, 60f, (object t) => ((Tuple<string, Action>)t).Item1, delegate (object t)
				{
					((Tuple<string, Action>)t).Item2();
				});
				DialogsManager.ShowDialog(base.ParentWidget, dialog);
			}
			if (base.Input.Back || base.Input.Cancel || Children.Find<ButtonWidget>("Resume").IsClicked)
			{
				DialogsManager.HideDialog(this);
			}
			if (Children.Find<ButtonWidget>("Quit").IsClicked)
			{
				DialogsManager.HideDialog(this);
				GameManager.SaveProject(waitForCompletion: true, showErrorDialog: true);
				GameManager.DisposeProject();
				ScreensManager.SwitchScreen("MainMenu");
			}
		}

		public IEnumerable<ActiveExternalContentInfo> GetRateableItems()
		{
			if (GameManager.Project != null && UserManager.ActiveUser != null)
			{
				SubsystemGameInfo subsystemGameInfo = GameManager.Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
				foreach (ActiveExternalContentInfo item in subsystemGameInfo.GetActiveExternalContent())
				{
					if (!CommunityContentManager.IsContentRated(item.Address, UserManager.ActiveUser.UniqueId))
					{
						yield return item;
					}
				}
			}
		}

		public static string FormatDistance(double value)
		{
			if (value < 1000.0)
			{
				return $"{value:0}m";
			}
			return $"{value / 1000.0:N2}km";
		}

		public void AddStat(ContainerWidget containerWidget, string title, string value1, string value2 = "")
		{
			BitmapFont font = ContentManager.Get<BitmapFont>("Fonts/Pericles");
			Color white = Color.White;
			Color gray = Color.Gray;
			containerWidget.Children.Add(new UniformSpacingPanelWidget
			{
				Direction = LayoutDirection.Horizontal,
				HorizontalAlignment = WidgetAlignment.Center,
				Children =
				{
					(Widget)new LabelWidget
					{
						Text = title + ":",
						HorizontalAlignment = WidgetAlignment.Far,
						Font = font,
						Color = gray,
						Margin = new Vector2(5f, 1f)
					},
					(Widget)new StackPanelWidget
					{
						Direction = LayoutDirection.Horizontal,
						HorizontalAlignment = WidgetAlignment.Near,
						Children =
						{
							(Widget)new LabelWidget
							{
								Text = value1,
								Font = font,
								Color = white,
								Margin = new Vector2(5f, 1f)
							},
							(Widget)new LabelWidget
							{
								Text = value2,
								Font = font,
								Color = gray,
								Margin = new Vector2(5f, 1f)
							}
						}
					}
				}
			});
		}
	}
}
