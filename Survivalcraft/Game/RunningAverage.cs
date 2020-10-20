using System.Diagnostics;

namespace Game
{
	public class RunningAverage
	{
		private long m_startTicks;

		private long m_period;

		private float m_sumValues;

		private int m_countValues;

		private float m_value;

		public float Value => m_value;

		public RunningAverage(float period)
		{
			m_period = (long)(period * (float)Stopwatch.Frequency);
		}

		public void AddSample(float sample)
		{
			m_sumValues += sample;
			m_countValues++;
			long timestamp = Stopwatch.GetTimestamp();
			if (timestamp >= m_startTicks + m_period)
			{
				m_value = m_sumValues / (float)m_countValues;
				m_sumValues = 0f;
				m_countValues = 0;
				m_startTicks = timestamp;
			}
		}
	}
}
