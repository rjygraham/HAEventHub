using CommandLine;
using EventHubProducer.Models;
using Microsoft.Azure.EventHubs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EventHubProducer
{
	class Program
	{
		private static IDictionary<string, TaskState> sensorTasks = new Dictionary<string, TaskState>();
		private static TaskFactory taskFactory;
		private static Random random = new Random();
		private static EventHubClient client;
		private static CancellationTokenSource taskFactoryCts;
		private static bool shouldExit = false;

		static async Task Main(string[] args)
		{
			await Parser.Default.ParseArguments<CommandLineOptions>(args)
				.WithParsedAsync(ExecuteAsync);
		}

		private static async Task ExecuteAsync(CommandLineOptions options)
		{
			// Create EventHubClient using ConnectionString (-c) passed in via command line arguments.
			client = EventHubClient.CreateFromConnectionString(options.ConnectionString);

			// Verify sensor ID file actually exists.
			if (!File.Exists(options.SensorsFile))
			{
				Console.WriteLine("Specified file does not exits.");
				return;
			}

			// Read sensor IDs
			var sensorIds = await File.ReadAllLinesAsync(options.SensorsFile);
			if (sensorIds.Length == 0)
			{
				Console.WriteLine("Specified file does not contain and sensor IDs.");
				return;
			}

			// Watch file for changes.
			var filter = $"*{Path.GetExtension(options.SensorsFile)}";
			var fsw = new FileSystemWatcher(Path.GetDirectoryName(options.SensorsFile), filter);
			fsw.EnableRaisingEvents = true;
			fsw.NotifyFilter = NotifyFilters.LastWrite;
			fsw.Changed += OnSensorsFileChanged;

			// Kick everything off.
			taskFactoryCts = new CancellationTokenSource();
			taskFactory = new TaskFactory(taskFactoryCts.Token);
			await AddSensorsAsync(sensorIds);

			// Keep running while sensor tasks are executing.
			while (!shouldExit)
			{
				await taskFactory.ContinueWhenAll(sensorTasks.Values.Select(s => s.Task).ToArray(), (results) => { });
			}

			Console.WriteLine("Completed simulation.");
		}

		private static async Task SimulateSensorMeasurementAsync(object state)
		{
			var typedState = (Tuple<string, CancellationToken>)state;

			while (!typedState.Item2.IsCancellationRequested)
			{
				var eventToSend = new SampleEvent
				{
					Id = typedState.Item1,
					Timestamp = DateTime.UtcNow,
					Measurement = random.Next(0, 1000)
				};

				Console.WriteLine($"Sensor ID: {eventToSend.Id} | Measurement: {eventToSend.Measurement}");

				var json = JsonSerializer.Serialize(eventToSend);
				var data = new EventData(Encoding.UTF8.GetBytes(json));

				await client.SendAsync(data);

				await Task.Delay(500);
			}
		}

		private static async void OnSensorsFileChanged(object sender, FileSystemEventArgs e)
		{
			var shouldReadFileAgain = false;

			while (!shouldReadFileAgain)
			{
				try
				{
					var updatedSensorIds = new List<string>(await File.ReadAllLinesAsync(e.FullPath));

					var newSensors = updatedSensorIds.Except(sensorTasks.Keys);
					var removedSensors = sensorTasks.Keys.Except(updatedSensorIds);
					Console.WriteLine($"Sensors file changed, updating registered sensors.\nAdded sensor(s): {string.Join(',', newSensors)}\nRemoved sensor(s): {string.Join(',', removedSensors)}");

					await AddSensorsAsync(newSensors);
					RemoveSensors(removedSensors);

					if (updatedSensorIds.Count == 0)
					{
						shouldExit = true;
					}

					shouldReadFileAgain = true;
				}
				catch (IOException ioex)
				{
					await Task.Delay(100);
				}
				catch (Exception)
				{
					shouldReadFileAgain = true;
				}
			}
		}

		private static async Task AddSensorsAsync(IEnumerable<string> sensorIds)
		{
			foreach (var sensorId in sensorIds)
			{
				if (!string.IsNullOrWhiteSpace(sensorId))
				{
					var cts = new CancellationTokenSource();
					var innerState = new Tuple<string, CancellationToken>(sensorId, cts.Token);
					var outterState = new TaskState(await taskFactory.StartNew(SimulateSensorMeasurementAsync, innerState, taskFactoryCts.Token), cts);
					sensorTasks.Add(sensorId, outterState);
				}
			}
		}

		private static void RemoveSensors(IEnumerable<string> sensorIds)
		{
			foreach (var sensorId in sensorIds)
			{
				sensorTasks[sensorId].CancellationTokenSource.Cancel();
				sensorTasks.Remove(sensorId);
			}
		}
	}
}
