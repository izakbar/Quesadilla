using Quesadilla.Classes.Queues;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;

namespace Quesadilla.Classes.Alerts
{
	public class AlertQueueManager
	{
		private ConcurrentQueue<AlertQueueItem> _newAlerts = new ConcurrentQueue<AlertQueueItem>();
		private ConcurrentQueue<AlertQueueItem> _activeAlerts = new ConcurrentQueue<AlertQueueItem>();
		public int ActiveAlertCount()
		{
			return _activeAlerts.Count;
		}
		public bool HasActiveAlerts()
		{
			return _activeAlerts.Any();
		}
		public bool HasNewAlerts()
		{
			return _newAlerts.Any();
		}

		public int ExpireAlerts()
		{
			int Expired = 0;

			while (_activeAlerts.TryPeek(out var alert))
			{
				DateTime timeAdded = alert.Entry?.TimeAdded ?? alert.Kill.TimeAdded;

				double ageSeconds = (DateTime.Now - timeAdded).TotalSeconds;
				if (ageSeconds <= Program.MainConfiguration.ExpireAlertSeconds)
				{
					// stop processing when we hit an alert that is not expired, since the queue is ordered by time added
					break;
				}

				_activeAlerts.TryDequeue(out alert);
				Expired++;

				// expired so dequeue
				if (!alert.isFaction && !alert.isDecloak && alert.Kill == null && alert.Path != null)
				{
					// remove alert to solarsystem info
					Program.Universe.SolarSystems[alert.Path.ToSystemId].RemoveAlert(alert);
				}
			}
			return Expired;
		}
		public List<AlertQueueItem> GetLastActiveAlerts(int count)
		{
			return _activeAlerts.Reverse().Where(o => o.ShowAlert).Take(count).ToList();
		}

		public void EnqueueNewAlert(AlertQueueItem alert)
		{
			// only add to new alerts if not already in new alerts. This should prevent an alert for each character on same logfile entry
			try
			{
				bool zkillExists = false;
				// check if zkill already exists
				if (alert.Kill != null)
				{
					zkillExists =
						_activeAlerts.Any(o => o.Kill?.ID == alert.Kill?.ID) ||
						_newAlerts.Any(o => o.Kill?.ID == alert.Kill?.ID);
				}
				var foundAlerts = _newAlerts.Where(
					o => (
					(o.Path != null && alert.Path != null && o.Path.ToSystemId == alert.Path.ToSystemId) || (o.AlertText == alert.AlertText)
					|| (o.Kill != null && alert.Kill != null && o.Kill.ID == alert.Kill.ID)
					)).ToList();

				if ((!foundAlerts.Any()) && !zkillExists)
				{
					alert.RouteText = Program.Universe.PathToString(alert.Path);
					_newAlerts.Enqueue(alert);
				}
				if (foundAlerts.Any())
				{
					var existingAlert = foundAlerts.First();
					if (existingAlert.Path != null && alert.Path != null && alert.Path.TotalJumps < existingAlert.Path.TotalJumps)
					{
						existingAlert.Path = alert.Path;
						existingAlert.AlertText = alert.AlertText;
						existingAlert.Entry = alert.Entry;
						existingAlert.Context = alert.Context;
						existingAlert.RouteText = alert.RouteText;
						existingAlert.SoundFile = alert.SoundFile;
						existingAlert.ShowAlert = alert.ShowAlert;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Debug(ex.Message);
				if (ex.InnerException != null)
				{
					Log.Debug(ex.InnerException.Message);
				}
			}
		}

		public bool TryGetNextNewAlert(out AlertQueueItem alert)
		{
			if (_newAlerts.TryDequeue(out alert))
			{
				_activeAlerts.Enqueue(alert);

				if (alert.Path != null && alert.Path.ToSystemId != null)
				{
					Program.Universe.AddAlert(alert.Path.ToSystemId, alert);
				}
			}
			return true;
		}
	}
}