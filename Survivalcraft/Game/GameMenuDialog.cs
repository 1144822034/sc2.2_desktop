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
				DialogsManager.ShowDialog(base.ParentWidget, new MessageDialog("����豸���ܳ���", "�������ӿ��ӷ�Χ��ֱ����Ի�ø��õ�ͼ��Ч����Ϊ�ˣ���ת���������á�", "ȷ��", null, null));
				AnalyticsManager.LogEvent("[GameMenuScreen] IncreaseDetailDialog Shown");
			}
			if (!m_decreaseDetailDialogShown && PerformanceManager.LongTermAverageFrameTime.HasValue && PerformanceManager.LongTermAverageFrameTime.Value * 1000f > 50f && (SettingsManager.VisibilityRange >= 64 || SettingsManager.ResolutionMode == ResolutionMode.High))
			{
				m_decreaseDetailDialogShown = true;
				DialogsManager.ShowDialog(base.ParentWidget, new MessageDialog("����豸���ܳ���", "�������ӿ��ӷ�Χ��ֱ����Ի�ø��õ�ͼ��Ч����Ϊ�ˣ���ת���������á�", "ȷ��", null, null));
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
				Text = "��Ϸͳ��",
				Font = font,
				HorizontalAlignment = WidgetAlignment.Center,
				Margin = new Vector2(0f, 10f),
				Color = white
			});
			AddStat(stackPanelWidget, "��Ϸģʽ", LanguageControl.Get("GameMode",subsystemGameInfo.WorldSettings.GameMode.ToString()) + ", " +LanguageControl.Get("EnvironmentBehaviorMode", subsystemGameInfo.WorldSettings.EnvironmentBehaviorMode.ToString()));
			AddStat(stackPanelWidget, "��������", StringsManager.GetString("TerrainGenerationMode." + subsystemGameInfo.WorldSettings.TerrainGenerationMode.ToString() + ".Name"));
			string seed = subsystemGameInfo.WorldSettings.Seed;
			AddStat(stackPanelWidget, "��������", (!string.IsNullOrEmpty(seed)) ? seed : "(û��)");
			AddStat(stackPanelWidget, "��ƽ��", WorldOptionsScreen.FormatOffset(subsystemGameInfo.WorldSettings.SeaLevelOffset));
			AddStat(stackPanelWidget, "�¶�", WorldOptionsScreen.FormatOffset(subsystemGameInfo.WorldSettings.TemperatureOffset));
			AddStat(stackPanelWidget, "ʪ��", WorldOptionsScreen.FormatOffset(subsystemGameInfo.WorldSettings.HumidityOffset));
			AddStat(stackPanelWidget, "����Ⱥ���С", subsystemGameInfo.WorldSettings.BiomeSize.ToString() + "x");
			int num = 0;
			for (int i = 0; i < 1024; i++)
			{
				if (subsystemFurnitureBlockBehavior.GetDesign(i) != null)
				{
					num++;
				}
			}
			AddStat(stackPanelWidget, "�Ҿ�ʹ����", $"{num}/{1024}");
			AddStat(stackPanelWidget, "����������汾", string.IsNullOrEmpty(subsystemGameInfo.WorldSettings.OriginalSerializationVersion) ? "��1.22ǰ" : subsystemGameInfo.WorldSettings.OriginalSerializationVersion);
			stackPanelWidget.Children.Add(new LabelWidget
			{
				Text = "���ͳ��",
				Font = font,
				HorizontalAlignment = WidgetAlignment.Center,
				Margin = new Vector2(0f, 10f),
				Color = white
			});
			AddStat(stackPanelWidget, "����", playerData.Name);
			AddStat(stackPanelWidget, "�Ա�", playerData.PlayerClass.ToString());
			string value = (playerData.FirstSpawnTime >= 0.0) ? (((subsystemGameInfo.TotalElapsedGameTime - playerData.FirstSpawnTime) / 1200.0).ToString("N1") + " ��ǰ") : "��δ����";
			AddStat(stackPanelWidget, "�״δ���", value);
			string value2 = (playerData.LastSpawnTime >= 0.0) ? (((subsystemGameInfo.TotalElapsedGameTime - playerData.LastSpawnTime) / 1200.0).ToString("N1") + " ��") : "��δ����";
			AddStat(stackPanelWidget, "���", value2);
			AddStat(stackPanelWidget, "����", MathUtils.Max(playerData.SpawnsCount - 1, 0).ToString("N0") + " ��");
			AddStat(stackPanelWidget, "��ߴﵽ�ȼ�", "�ȼ� " + ((int)MathUtils.Floor(playerStats.HighestLevel)).ToString("N0"));
			if (componentPlayer != null)
			{
				Vector3 position = componentPlayer.ComponentBody.Position;
				if (subsystemGameInfo.WorldSettings.GameMode == GameMode.Creative)
				{
					AddStat(stackPanelWidget, "λ��", $"{position.X:0}, {position.Z:0} ����: {position.Y:0}");
				}
				else
				{
					AddStat(stackPanelWidget, "λ��", "(��" + subsystemGameInfo.WorldSettings.GameMode.ToString() + "ģʽ�²�����)");
				}
			}
			if (string.CompareOrdinal(subsystemGameInfo.WorldSettings.OriginalSerializationVersion, "1.29") > 0)
			{
				stackPanelWidget.Children.Add(new LabelWidget
				{
					Text = "ս��ͳ��",
					Font = font,
					HorizontalAlignment = WidgetAlignment.Center,
					Margin = new Vector2(0f, 10f),
					Color = white
				});
				AddStat(stackPanelWidget, "����һ�ɱ", playerStats.PlayerKills.ToString("N0"));
				AddStat(stackPanelWidget, "��½�������ɱ", playerStats.LandCreatureKills.ToString("N0"));
				AddStat(stackPanelWidget, "��ˮ�������ɱ", playerStats.WaterCreatureKills.ToString("N0"));
				AddStat(stackPanelWidget, "�����������ɱ", playerStats.AirCreatureKills.ToString("N0"));
				AddStat(stackPanelWidget, "��ս��������", playerStats.MeleeAttacks.ToString("N0"));
				AddStat(stackPanelWidget, "��ս���д���", playerStats.MeleeHits.ToString("N0"), $"({((playerStats.MeleeHits == 0L) ? 0.0 : ((double)playerStats.MeleeHits / (double)playerStats.MeleeAttacks * 100.0)):0}%)");
				AddStat(stackPanelWidget, "Զ�̹�������", playerStats.RangedAttacks.ToString("N0"));
				AddStat(stackPanelWidget, "Զ�����д���", playerStats.RangedHits.ToString("N0"), $"({((playerStats.RangedHits == 0L) ? 0.0 : ((double)playerStats.RangedHits / (double)playerStats.RangedAttacks * 100.0)):0}%)");
				AddStat(stackPanelWidget, "�յ�������", playerStats.HitsReceived.ToString("N0"));
				stackPanelWidget.Children.Add(new LabelWidget
				{
					Text = "����ͳ��",
					Font = font,
					HorizontalAlignment = WidgetAlignment.Center,
					Margin = new Vector2(0f, 10f),
					Color = white
				});
				AddStat(stackPanelWidget, "�ھ����", playerStats.BlocksDug.ToString("N0"));
				AddStat(stackPanelWidget, "���ô���", playerStats.BlocksPlaced.ToString("N0"));
				AddStat(stackPanelWidget, "��������", playerStats.BlocksInteracted.ToString("N0"));
				AddStat(stackPanelWidget, "�������", playerStats.ItemsCrafted.ToString("N0"));
				AddStat(stackPanelWidget, "����Ҿ߸���", playerStats.FurnitureItemsMade.ToString("N0"));
				stackPanelWidget.Children.Add(new LabelWidget
				{
					Text = "�ƶ�ͳ��",
					Font = font,
					HorizontalAlignment = WidgetAlignment.Center,
					Margin = new Vector2(0f, 10f),
					Color = white
				});
				AddStat(stackPanelWidget, "�����о���", FormatDistance(playerStats.DistanceTravelled));
				AddStat(stackPanelWidget, "���о���", FormatDistance(playerStats.DistanceWalked), $"({((playerStats.DistanceTravelled > 0.0) ? (playerStats.DistanceWalked / playerStats.DistanceTravelled * 100.0) : 0.0):0.0}%)");
				AddStat(stackPanelWidget, "�������", FormatDistance(playerStats.DistanceFallen), $"({((playerStats.DistanceTravelled > 0.0) ? (playerStats.DistanceFallen / playerStats.DistanceTravelled * 100.0) : 0.0):0.0}%)");
				AddStat(stackPanelWidget, "���о���", FormatDistance(playerStats.DistanceClimbed), $"({((playerStats.DistanceTravelled > 0.0) ? (playerStats.DistanceClimbed / playerStats.DistanceTravelled * 100.0) : 0.0):0.0}%)");
				AddStat(stackPanelWidget, "���о���", FormatDistance(playerStats.DistanceFlown), $"({((playerStats.DistanceTravelled > 0.0) ? (playerStats.DistanceFlown / playerStats.DistanceTravelled * 100.0) : 0.0):0.0}%)");
				AddStat(stackPanelWidget, "��Ӿ����", FormatDistance(playerStats.DistanceSwam), $"({((playerStats.DistanceTravelled > 0.0) ? (playerStats.DistanceSwam / playerStats.DistanceTravelled * 100.0) : 0.0):0.0}%)");
				AddStat(stackPanelWidget, "��;���о���", FormatDistance(playerStats.DistanceRidden), $"({((playerStats.DistanceTravelled > 0.0) ? (playerStats.DistanceRidden / playerStats.DistanceTravelled * 100.0) : 0.0):0.0}%)");
				AddStat(stackPanelWidget, "��ͺ���", FormatDistance(playerStats.LowestAltitude));
				AddStat(stackPanelWidget, "��ߺ���", FormatDistance(playerStats.HighestAltitude));
				AddStat(stackPanelWidget, "����Ǳˮ", playerStats.DeepestDive.ToString("N1") + "m");
				AddStat(stackPanelWidget, "��Ծ����", playerStats.Jumps.ToString("N0"));
				stackPanelWidget.Children.Add(new LabelWidget
				{
					Text = "����ͳ��",
					Font = font,
					HorizontalAlignment = WidgetAlignment.Center,
					Margin = new Vector2(0f, 10f),
					Color = white
				});
				AddStat(stackPanelWidget, "����ʧ����ֵ", (playerStats.TotalHealthLost * 100.0).ToString("N0") + "%");
				AddStat(stackPanelWidget, "�Ե�ʳ����", playerStats.FoodItemsEaten.ToString("N0") + " ��");
				AddStat(stackPanelWidget, "˯������", playerStats.TimesWentToSleep.ToString("N0") + " ��");
				AddStat(stackPanelWidget, "˯���ۼ�ʱ��", (playerStats.TimeSlept / 1200.0).ToString("N1") + " ��");
				AddStat(stackPanelWidget, "��������", playerStats.TimesWasSick.ToString("N0") + " ��");
				AddStat(stackPanelWidget, "Ż�´���", playerStats.TimesPuked.ToString("N0") + " ��");
				AddStat(stackPanelWidget, "��Ⱦ���д���", playerStats.TimesHadFlu.ToString("N0") + " ��");
				stackPanelWidget.Children.Add(new LabelWidget
				{
					Text = "����ͳ��",
					Font = font,
					HorizontalAlignment = WidgetAlignment.Center,
					Margin = new Vector2(0f, 10f),
					Color = white
				});
				AddStat(stackPanelWidget, "���������", playerStats.StruckByLightning.ToString("N0") + " ��");
				GameMode easiestModeUsed = playerStats.EasiestModeUsed;
				AddStat(stackPanelWidget, "ʹ����򵥵���Ϸģʽ", easiestModeUsed.ToString());
				if (playerStats.DeathRecords.Count > 0)
				{
					stackPanelWidget.Children.Add(new LabelWidget
					{
						Text = "������¼",
						Font = font,
						HorizontalAlignment = WidgetAlignment.Center,
						Margin = new Vector2(0f, 10f),
						Color = white
					});
					foreach (PlayerStats.DeathRecord deathRecord in playerStats.DeathRecords)
					{
						float num2 = (float)MathUtils.Remainder(deathRecord.Day, 1.0);
						string arg = (!(num2 < 0.2f) && !(num2 >= 0.8f)) ? ((!(num2 >= 0.7f)) ? ((!(num2 >= 0.5f)) ? "���ϵ�" : "�����") : "���ϵ�") : "��ҹ��";
						AddStat(stackPanelWidget, string.Format("{1} �� {0:0}", MathUtils.Floor(deathRecord.Day) + 1.0, arg), "", deathRecord.Cause);
					}
				}
			}
			else
			{
				stackPanelWidget.Children.Add(new LabelWidget
				{
					Text = "û�п��õ����ͳ�ƣ���Ϊ��������һ���ܾɵ���Ϸ�浵",
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
					list.Add(new Tuple<string, Action>("����ð��", delegate
					{
						DialogsManager.ShowDialog(base.ParentWidget, new MessageDialog("����ð�գ�", "ð�ս����¿�ʼ", LanguageControl.Get("Usual","yes"), LanguageControl.Get("Usual","no"), delegate (MessageDialogButton result)
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
					list.Add(new Tuple<string, Action>("����", delegate
					{
						DialogsManager.ShowDialog(base.ParentWidget, new ListSelectionDialog("ѡ��Ҫ���ֵ�����", GetRateableItems(), 60f, (object o) => ((ActiveExternalContentInfo)o).DisplayName, delegate (object o)
						{
							ActiveExternalContentInfo activeExternalContentInfo = (ActiveExternalContentInfo)o;
							DialogsManager.ShowDialog(base.ParentWidget, new RateCommunityContentDialog(activeExternalContentInfo.Address, activeExternalContentInfo.DisplayName, UserManager.ActiveUser.UniqueId));
						}));
					}));
				}
				list.Add(new Tuple<string, Action>("�༭���", delegate
				{
					ScreensManager.SwitchScreen("Players", m_componentPlayer.Project.FindSubsystem<SubsystemPlayers>(throwOnError: true));
				}));
				list.Add(new Tuple<string, Action>("����", delegate
				{
					ScreensManager.SwitchScreen("Settings");
				}));
				list.Add(new Tuple<string, Action>("����", delegate
				{
					ScreensManager.SwitchScreen("Help");
				}));
				if ((base.Input.Devices & (WidgetInputDevice.Keyboard | WidgetInputDevice.Mouse)) != 0)
				{
					list.Add(new Tuple<string, Action>("��������", delegate
					{
						DialogsManager.ShowDialog(base.ParentWidget, new KeyboardHelpDialog());
					}));
				}
				if ((base.Input.Devices & WidgetInputDevice.Gamepads) != 0)
				{
					list.Add(new Tuple<string, Action>("���������", delegate
					{
						DialogsManager.ShowDialog(base.ParentWidget, new GamepadHelpDialog());
					}));
				}
				ListSelectionDialog dialog = new ListSelectionDialog("����ѡ��", list, 60f, (object t) => ((Tuple<string, Action>)t).Item1, delegate (object t)
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
