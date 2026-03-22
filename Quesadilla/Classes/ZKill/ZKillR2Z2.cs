using Quesadilla.Classes.Events;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Quesadilla.Classes.ZKill
{
	public class ZKillR2Z2
	{
		private ILogger log = Log.ForContext<ZKillR2Z2>();

		private BackgroundWorker Worker;

		private bool FeedPaused = false;
		private System.Timers.Timer t;

		private static long ZKillSequence = 0;

		private int ZkillErrorCount = 0;

		public event EventHandler<ZKillEventArgs> ZKillEventHandler;

		public ZKillR2Z2()
		{
			Worker = new BackgroundWorker();
			Worker.WorkerSupportsCancellation = true;
			Worker.WorkerReportsProgress = false;
			Worker.DoWork += this.DoWork;
			Worker.RunWorkerCompleted += this.RunWorkerCompleted;

			t = new System.Timers.Timer()
			{
				Interval = 6000,   // https://github.com/zKillboard/zKillboard/wiki/API-(R2Z2)
				AutoReset = true,
				Enabled = true,
			};
			t.Elapsed += this.Tick;
		}

		private void Tick(object sender, EventArgs e)
		{
			if (!Worker.IsBusy && !FeedPaused)
			{
				Worker.RunWorkerAsync();
			}
		}


		public void Pause()
		{
			Log.Error("ZKill R2Z2 processing PAUSED");
			FeedPaused = true;
		}
		public void Resume()
		{
			Log.Error("ZKill R2Z2 processing RESUMED");
			FeedPaused = false;
			ZkillErrorCount = 0;            // Reset error counter
		}


		private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			//            log.Information("zkill done");
		}

		private void GetLatestZKillCounter()
		{
			try
			{
				string URL = "https://r2z2.zkillboard.com/ephemeral/sequence.json";
				HttpClient hc = new HttpClient();
				hc.DefaultRequestHeaders.Add("User-Agent", Program.UserAgent);

				var response = hc.GetAsync(URL).Result;
				string strContent = string.Empty;

				if (response.IsSuccessStatusCode)
				{
					strContent = response.Content.ReadAsStringAsync().Result;
					ZKillSequence = JsonSerializer.Deserialize<ZKillSequence>(strContent).sequence;
					Log.Information($"R2Z2 Sequence: {ZKillSequence}");
				}
				else
				{
					strContent = "Error";
				}
			}
			catch
			{
				Log.Error("ZKill R2Z2 sequence request failed");
				ZkillErrorCount++;
				return;
			}
		}

		private void DoWork(object sender, DoWorkEventArgs e)
		{

			// Log.Information("ZKill R2Z2 tick");
			if (ZKillSequence == 0) GetLatestZKillCounter();
			if (ZKillSequence == 0) return; // Didn't manage to get initial sequence

			bool exitZKillLoop = false;
			while(exitZKillLoop != true)
			{
				string URL = $"https://r2z2.zkillboard.com/ephemeral/{ZKillSequence}.json";
				string strContent = string.Empty;
				// Log.Information($"R2Z2 Sequence: {ZKillSequence}");

				try
				{
					HttpClient hc = new HttpClient();
					hc.DefaultRequestHeaders.Add("User-Agent", Program.UserAgent);
					var response = hc.GetAsync(URL).Result;
					if (response.IsSuccessStatusCode)
					{
						//Log.Information("R2Z2 worked");
						strContent = response.Content.ReadAsStringAsync().Result;
						var zkill = JsonSerializer.Deserialize<ZKillR2Z2Data>(strContent);

						if (ZKillEventHandler != null)
						{
							var mainAttacker = zkill?.esi?.Attackers?.Where(o => o.FinalBlow).First();
							string isk = "";
							if (zkill.zkb.TotalValue < 1000000)
							{
								isk = Math.Truncate(zkill.zkb.TotalValue / 1000).ToString() + "k";
							}
							else if (zkill.zkb.TotalValue < 1000000000)
							{
								isk = Math.Truncate(zkill.zkb.TotalValue / 1000000).ToString() + "m";
							}
							else if (zkill.zkb.TotalValue < 1000000000000)
							{
								isk = Math.Round(zkill.zkb.TotalValue / 1000000000, 1).ToString() + "b";
							}
							else
							{
								isk = Math.Round(zkill.zkb.TotalValue / 1000000000000, 1).ToString() + "t";
							}
							try
							{
								ZKillKill z = new ZKillKill()
								{
									ID = zkill.esi.ID,
									SolarSystemId = zkill.esi.SolarSystemId,
									Timestamp = zkill.esi.Timestamp,
									TimeAdded = DateTime.Now,
									VictimAllianceId = zkill.esi.Victim.AllianceId,
									VictimCharacterId = zkill.esi.Victim.CharacterId,
									VictimCorporationId = zkill.esi.Victim.CorporationId,
									VictimShipTypeId = zkill.esi.Victim.ShipTypeId,

									KillerAllianceId = mainAttacker.AllianceId,
									KillerCharacterId = mainAttacker.CharacterId,
									KillerCorporationId = mainAttacker.CorporationId,
									KillerShipTypeId = mainAttacker.ShipTypeId,
									KillerWeaponTypeId = mainAttacker.WeaponTypeId,

									href = zkill.zkb.Href,

									NPC = zkill.zkb.Npc,
									TotalValue = isk,

									Expired = false,

								};
								ZKillEventHandler((object)this, new ZKillEventArgs(z));
							}
							catch (Exception ex)
							{
								Log.Error(ex.Message);
								if (ex.InnerException != null) Log.Error(ex.InnerException.Message);
							}
						}


						ZKillSequence++;
						Thread.Sleep(100); // https://github.com/zKillboard/zKillboard/wiki/API-(R2Z2)
					}
					else
					{
						// Log.Information("R2Z2 error returned");
						strContent = "Error";
						exitZKillLoop = true;
					}
				}
				catch
				{
					log.Error("ZKill R2Z2 request failed");
					ZkillErrorCount++;
					exitZKillLoop = true;
					return;
				}


			}
		}
	}
}
