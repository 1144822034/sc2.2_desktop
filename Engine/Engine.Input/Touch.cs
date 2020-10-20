using System;
using System.Collections.Generic;

namespace Engine.Input
{
	public static class Touch
	{
		private static List<TouchLocation> m_touchLocations = new List<TouchLocation>();

		public static ReadOnlyList<TouchLocation> TouchLocations => new ReadOnlyList<TouchLocation>(m_touchLocations);

		public static event Action<TouchLocation> TouchPressed;

		public static event Action<TouchLocation> TouchReleased;

		public static event Action<TouchLocation> TouchMoved;

		internal static void Initialize()
		{
		}

		internal static void Dispose()
		{
		}

		public static void Clear()
		{
			m_touchLocations.Clear();
		}

		internal static void BeforeFrame()
		{
		}

		internal static void AfterFrame()
		{
			int num = 0;
			while (num < m_touchLocations.Count)
			{
				if (m_touchLocations[num].State == TouchLocationState.Released)
				{
					m_touchLocations.RemoveAt(num);
					continue;
				}
				TouchLocation value;
				if (m_touchLocations[num].ReleaseQueued)
				{
					List<TouchLocation> touchLocations = m_touchLocations;
					int index = num;
					value = new TouchLocation
					{
						Id = m_touchLocations[num].Id,
						Position = m_touchLocations[num].Position,
						State = TouchLocationState.Released
					};
					touchLocations[index] = value;
				}
				else if (m_touchLocations[num].State == TouchLocationState.Pressed)
				{
					List<TouchLocation> touchLocations2 = m_touchLocations;
					int index2 = num;
					value = new TouchLocation
					{
						Id = m_touchLocations[num].Id,
						Position = m_touchLocations[num].Position,
						State = TouchLocationState.Moved
					};
					touchLocations2[index2] = value;
				}
				num++;
			}
		}

		private static int FindTouchLocationIndex(int id)
		{
			for (int i = 0; i < m_touchLocations.Count; i++)
			{
				if (m_touchLocations[i].Id == id)
				{
					return i;
				}
			}
			return -1;
		}

		private static void ProcessTouchPressed(int id, Vector2 position)
		{
			ProcessTouchMoved(id, position);
		}

		private static void ProcessTouchMoved(int id, Vector2 position)
		{
			if (!Window.IsActive || Keyboard.IsKeyboardVisible)
			{
				return;
			}
			int num = FindTouchLocationIndex(id);
			TouchLocation touchLocation;
			if (num >= 0)
			{
				if (m_touchLocations[num].State == TouchLocationState.Moved)
				{
					List<TouchLocation> touchLocations = m_touchLocations;
					touchLocation = new TouchLocation
					{
						Id = id,
						Position = position,
						State = TouchLocationState.Moved
					};
					touchLocations[num] = touchLocation;
				}
				if (Touch.TouchMoved != null)
				{
					Touch.TouchMoved(m_touchLocations[num]);
				}
			}
			else
			{
				List<TouchLocation> touchLocations2 = m_touchLocations;
				touchLocation = new TouchLocation
				{
					Id = id,
					Position = position,
					State = TouchLocationState.Pressed
				};
				touchLocations2.Add(touchLocation);
				if (Touch.TouchPressed != null)
				{
					Touch.TouchPressed(m_touchLocations[m_touchLocations.Count - 1]);
				}
			}
		}

		private static void ProcessTouchReleased(int id, Vector2 position)
		{
			if (!Window.IsActive || Keyboard.IsKeyboardVisible)
			{
				return;
			}
			int num = FindTouchLocationIndex(id);
			if (num >= 0)
			{
				TouchLocation value;
				if (m_touchLocations[num].State == TouchLocationState.Pressed)
				{
					List<TouchLocation> touchLocations = m_touchLocations;
					value = new TouchLocation
					{
						Id = id,
						Position = position,
						State = TouchLocationState.Pressed,
						ReleaseQueued = true
					};
					touchLocations[num] = value;
				}
				else
				{
					List<TouchLocation> touchLocations2 = m_touchLocations;
					value = new TouchLocation
					{
						Id = id,
						Position = position,
						State = TouchLocationState.Released
					};
					touchLocations2[num] = value;
				}
				if (Touch.TouchReleased != null)
				{
					Touch.TouchReleased(m_touchLocations[num]);
				}
			}
		}
	}
}
