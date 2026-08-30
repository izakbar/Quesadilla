using Quesadilla.Classes.Tick;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Quesadilla.Classes
{
	public sealed class TickManager : IDisposable
	{
		private readonly List<TickItem> _registeredTicks = new List<TickItem>();

		private Thread _workerThread;
		private bool _running;

		private readonly int TICK_INTERVAL_MS;

		public TickManager(int timer = 200)
		{
			TICK_INTERVAL_MS = timer;
		}

		public void Start()
		{
			if (_running)
				return;

			_running = true;

			_workerThread = new Thread(WorkerLoop)
			{
				IsBackground = true,
				Name = "TickManager",
				Priority = ThreadPriority.AboveNormal
			};

			_workerThread.Start();
		}

		public void Stop()
		{
			_running = false;

			if (_workerThread != null &&
				_workerThread.IsAlive)
			{
				_workerThread.Join();
			}
		}

		public void RegisterTick(
			string name,
			int initialValue,
			int maxValue,
			Action callback)
		{
			lock (_registeredTicks)
			{
				if (_registeredTicks.Any(t => t._name == name))
					return;

				_registeredTicks.Add(
					new TickItem(
						name,
						initialValue,
						maxValue,
						callback));
			}
		}

		public void RemoveTick(string name)
		{
			lock (_registeredTicks)
			{
				var tick = _registeredTicks
					.FirstOrDefault(t => t._name == name);

				if (tick != null)
				{
					_registeredTicks.Remove(tick);
				}
			}
		}

		private void WorkerLoop()
		{
			DateTime nextTick = DateTime.UtcNow;

			while (_running)
			{
				nextTick = nextTick.AddMilliseconds(TICK_INTERVAL_MS);

				lock (_registeredTicks)
				{
					foreach (var tick in _registeredTicks)
					{
						tick.Increment();

						if (tick.IsTriggered())
						{
							try
							{
								if (tick.TryBeginExecution())
								{
									try
									{
										tick.Execute();
									}
									catch (Exception ex)
									{
										Console.WriteLine(ex);
									}
									finally
									{
										tick.EndExecution();
									}
								}
							}
							catch (Exception ex)
							{
								Console.WriteLine(ex);
							}
						}
					}
				}

				TimeSpan sleepTime = nextTick - DateTime.UtcNow;

				if (sleepTime.TotalMilliseconds > 0)
				{
					Thread.Sleep(sleepTime);
				}
			}
		}

		public void Dispose()
		{
			Stop();
		}
	}
}