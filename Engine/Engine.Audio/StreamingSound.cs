using Engine.Media;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Engine.Audio
{
	public sealed class StreamingSound : BaseSound
	{
		private Task m_task;

		private ManualResetEvent m_stopTaskEvent = new ManualResetEvent(initialState: false);

		private bool m_noMoreData;

		private float m_bufferDuration;

		public StreamingSource StreamingSource
		{
			get;
			private set;
		}

		private int ReadStreamingSource(byte[] buffer, int count)
		{
			int num = 0;
			if (StreamingSource.BytesCount > 0)
			{
				while (count > 0)
				{
					int num2 = StreamingSource.Read(buffer, num, count);
					if (num2 > 0)
					{
						num += num2;
						count -= num2;
						continue;
					}
					if (!m_isLooped)
					{
						break;
					}
					StreamingSource.Position = 0L;
				}
			}
			return num;
		}

		private void VerifyStreamingSource(StreamingSource streamingSource)
		{
			if (streamingSource == null)
			{
				throw new ArgumentNullException("streamingSource");
			}
			if (streamingSource.ChannelsCount < 1 || streamingSource.ChannelsCount > 2)
			{
				throw new InvalidOperationException("Unsupported channels count.");
			}
			if (streamingSource.SamplingFrequency < 8000 || streamingSource.SamplingFrequency > 48000)
			{
				throw new InvalidOperationException("Unsupported frequency.");
			}
		}

		public StreamingSound(StreamingSource streamingSource, float volume = 1f, float pitch = 1f, float pan = 0f, bool isLooped = false, bool disposeOnStop = false, float bufferDuration = 0.3f)
		{
			VerifyStreamingSource(streamingSource);
			StreamingSource = streamingSource;
			base.ChannelsCount = streamingSource.ChannelsCount;
			base.SamplingFrequency = streamingSource.SamplingFrequency;
			base.Volume = volume;
			base.Pitch = pitch;
			base.Pan = pan;
			base.IsLooped = isLooped;
			base.DisposeOnStop = disposeOnStop;
			m_bufferDuration = MathUtils.Clamp(bufferDuration, 0.01f, 10f);
			m_task = Task.Run(delegate
			{
				try
				{
					StreamingThreadFunction();
				}
				catch (Exception message)
				{
					Log.Error(message);
				}
			});
		}

		internal override void InternalPlay()
		{
			AL.SourcePlay(m_source);
			Mixer.CheckALError();
		}

		internal override void InternalPause()
		{
			AL.SourcePause(m_source);
			Mixer.CheckALError();
		}

		internal override void InternalStop()
		{
			AL.SourceStop(m_source);
			Mixer.CheckALError();
			StreamingSource.Position = 0L;
			m_noMoreData = false;
		}

		internal override void InternalDispose()
		{
			if (m_stopTaskEvent != null && m_task != null)
			{
				m_stopTaskEvent.Set();
				m_task.Wait();
				m_task = null;
				m_stopTaskEvent.Dispose();
				m_stopTaskEvent = null;
			}
			if (StreamingSource != null)
			{
				StreamingSource.Dispose();
				StreamingSource = null;
			}
			base.InternalDispose();
		}

		private void StreamingThreadFunction()
		{
			int[] array = new int[3];
			List<int> list = new List<int>();
			int millisecondsTimeout = MathUtils.Clamp((int)(0.5f * m_bufferDuration / (float)array.Length * 1000f), 1, 100);
			byte[] array2 = new byte[2 * base.ChannelsCount * (int)((float)base.SamplingFrequency * m_bufferDuration / (float)array.Length)];
			for (int i = 0; i < array.Length; i++)
			{
				int num = AL.GenBuffer();
				Mixer.CheckALError();
				array[i] = num;
				list.Add(num);
			}
			do
			{
				lock (m_stateSync)
				{
					if (!m_noMoreData)
					{
						AL.GetSource(m_source, ALGetSourcei.BuffersProcessed, out int value);
						Mixer.CheckALError();
						for (int j = 0; j < value; j++)
						{
							int item = AL.SourceUnqueueBuffer(m_source);
							Mixer.CheckALError();
							list.Add(item);
						}
						if (list.Count > 0 && !m_noMoreData && base.State == SoundState.Playing)
						{
							int num2 = ReadStreamingSource(array2, array2.Length);
							m_noMoreData = (num2 < array2.Length);
							if (num2 > 0)
							{
								int num3 = list[list.Count - 1];
								AL.BufferData(num3, (base.ChannelsCount == 1) ? ALFormat.Mono16 : ALFormat.Stereo16, array2, num2, base.SamplingFrequency);
								Mixer.CheckALError();
								AL.SourceQueueBuffer(m_source, num3);
								Mixer.CheckALError();
								list.RemoveAt(list.Count - 1);
								ALSourceState sourceState = AL.GetSourceState(m_source);
								Mixer.CheckALError();
								if (sourceState != ALSourceState.Playing)
								{
									AL.SourcePlay(m_source);
									Mixer.CheckALError();
								}
							}
						}
					}
					else if (AL.GetSourceState(m_source) == ALSourceState.Stopped)
					{
						Dispatcher.Dispatch(delegate
						{
							Stop();
						});
					}
				}
			}
			while (!m_stopTaskEvent.WaitOne(millisecondsTimeout));
			AL.SourceStop(m_source);
			Mixer.CheckALError();
			AL.Source(m_source, ALSourcei.Buffer, 0);
			Mixer.CheckALError();
			for (int k = 0; k < array.Length; k++)
			{
				if (array[k] != 0)
				{
					AL.DeleteBuffer(array[k]);
					Mixer.CheckALError();
					array[k] = 0;
				}
			}
		}
	}
}
