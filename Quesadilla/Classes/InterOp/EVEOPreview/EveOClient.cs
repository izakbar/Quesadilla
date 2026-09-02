using Quesadilla.Classes.ESIData;
using Quesadilla.Classes.Queues;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Windows.Media.Protection.PlayReady;

namespace Quesadilla.Classes.EVEOPreview
{

	public static class EveOClient
	{
		private static string _pipename = "EVE-O-Preview-Pipe";
		private static async Task SendCommand(object command)
		{
			using (var pipe = new NamedPipeClientStream(".", _pipename, PipeDirection.InOut))
			{
				await pipe.ConnectAsync(500);

				using (var reader = new StreamReader(pipe))
				using (var writer = new StreamWriter(pipe)
				{
					AutoFlush = true
				})
				{
					await writer.WriteLineAsync(JsonSerializer.Serialize(command));
					await reader.ReadLineAsync();
				}
			}
		}

		public static async Task UpdateSystem(string clientName, string systemName)
		{
			var systemCommand = new EveOEnvelope(
				Guid.NewGuid(),
				EveOSystemUpdate.MessageType,
				JsonSerializer.SerializeToElement(
					new EveOSystemUpdate
					{
						Client = clientName,
						SystemName = systemName
					}));

			await SendCommand(systemCommand);
		}
		public static async Task AlertClient(AlertQueueItem alert)
		{
			var eveoAlert = new EveOAlertClient
			{
				Client = alert.Context,
				SystemName = alert.Path?.ToSystemName,
				AlertJumps = alert.Path == null ? 0 : alert.Path.TotalJumps,
				AlertText = alert.Entry?.LineContent,
				AlertType = EveOAlertClient.AlertTypeIntel,
			};

			if (alert.isDecloak)
			{
				eveoAlert.AlertType = EveOAlertClient.AlertTypeDecloak;
				eveoAlert.Client = alert.Entry.CharacterName;
			}

			if (alert.isFaction)
			{
				eveoAlert.AlertType = EveOAlertClient.AlertTypeFaction;
				eveoAlert.Client = alert.Entry.CharacterName;
				eveoAlert.AlertText = "Faction Spawn";
			}

			if (alert.isMiningOver)
			{
				eveoAlert.AlertType = EveOAlertClient.AlertTypeMiningOver;
				eveoAlert.AlertText = "Mining over";
				eveoAlert.Client = alert.Entry.CharacterName;
			}

			if (alert.Kill != null)
			{
				eveoAlert.AlertType = EveOAlertClient.AlertTypeKill;
				eveoAlert.AlertText = alert.Kill.ID.ToString();
			}

			var alertCommand = new EveOEnvelope(
				Guid.NewGuid(),
				EveOAlertClient.MessageType,
				JsonSerializer.SerializeToElement(eveoAlert));

			await SendCommand(alertCommand);

		}
		public static async Task Agression(string clientName, bool agression)
		{
			var agressionCommand = new EveOEnvelope(
				Guid.NewGuid(),
				EveOAgression.MessageType,
				JsonSerializer.SerializeToElement(
					new EveOAgression
					{
						Client = clientName,
						Agression = agression
					}));

			await SendCommand(agressionCommand);
		}
	}
}