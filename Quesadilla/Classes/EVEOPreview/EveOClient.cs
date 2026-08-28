using Quesadilla.Classes.Queues;
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
		public static async Task UpdateSystem(string clientName, string systemName)
		{
			NamedPipeClientStream _pipe = new NamedPipeClientStream(".", _pipename, PipeDirection.InOut);
			try
			{
				await _pipe.ConnectAsync(500);
				StreamReader _reader = new StreamReader(_pipe);
				StreamWriter _writer = new StreamWriter(_pipe)
				{
					AutoFlush = true
				};

				var systemCommand = new EveOEnvelope(
					Guid.NewGuid(),
					EveOSystemUpdate.MessageType,
					JsonSerializer.SerializeToElement(
						new EveOSystemUpdate
						{
							Client = clientName,
							SystemName = systemName
						}));

				await _writer.WriteLineAsync(JsonSerializer.Serialize(systemCommand));

				var response = await _reader.ReadLineAsync();
			}
			catch (Exception ex)
			{
			}
			finally
			{
				if (_pipe.IsConnected)
				{
					_pipe.Close();
					_pipe.Dispose();
				}
			}
		}
		public static async Task AlertClient(AlertQueueItem alert)
		{
			NamedPipeClientStream _pipe = new NamedPipeClientStream(".", _pipename, PipeDirection.InOut);
			try
			{
				await _pipe.ConnectAsync(500);
				StreamReader _reader = new StreamReader(_pipe);
				StreamWriter _writer = new StreamWriter(_pipe)
				{
					AutoFlush = true
				};

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

				await _writer.WriteLineAsync(JsonSerializer.Serialize(alertCommand));

				var response = await _reader.ReadLineAsync();
			}
			catch (Exception ex)
			{
			}
			finally
			{
				if (_pipe.IsConnected)
				{
					_pipe.Close();
					_pipe.Dispose();
				}
			}
		}
		public static async Task Agression(string clientName, bool agression)
		{
			NamedPipeClientStream _pipe = new NamedPipeClientStream(".", _pipename, PipeDirection.InOut);
			try
			{
				await _pipe.ConnectAsync(500);
				StreamReader _reader = new StreamReader(_pipe);
				StreamWriter _writer = new StreamWriter(_pipe)
				{
					AutoFlush = true
				};

				var agressionCommand = new EveOEnvelope(
					Guid.NewGuid(),
					EveOSystemUpdate.MessageType,
					JsonSerializer.SerializeToElement(
						new EveOAgression
						{
							Client = clientName,
							Agression = agression
						}));

				await _writer.WriteLineAsync(JsonSerializer.Serialize(agressionCommand));

				var response = await _reader.ReadLineAsync();
			}
			catch (Exception ex)
			{
			}
			finally
			{
				if (_pipe.IsConnected)
				{
					_pipe.Close();
					_pipe.Dispose();
				}
			}
		}
	}
}