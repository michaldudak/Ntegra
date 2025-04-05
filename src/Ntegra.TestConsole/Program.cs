using System.Collections;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Ntegra.TestConsole;

public static class Program
{
	public static async Task Main(string[] args)
	{
		var config = SetUpConfiguration(args);

		var address = config["Host"];
		var port = int.Parse(config["Port"]);
		var userCode = config["UserCode"];

		using var client = new NtegraTcpClient(address, port);
		using var controller = new NtegraController(client, userCode);

		Console.WriteLine("Connected.");

		while (true)
		{
			switch (Console.ReadKey(true).Key)
			{
				case ConsoleKey.D1:
					Console.WriteLine("Output toggle: Office lights");
					await controller.SetOutputState(11, !await controller.GetOutputState(11));
					break;
				case ConsoleKey.D2:
					Console.WriteLine("Output toggle: Office ambient light");
					await controller.SetOutputState(12, !await controller.GetOutputState(12));
					break;
				case ConsoleKey.D3:
					Console.WriteLine("Output toggle: Corridor light");
					await controller.SetOutputState(13, !await controller.GetOutputState(13));
					break;
				case ConsoleKey.PageUp:
					Console.WriteLine("Output toggle: Office blinds up");
					await controller.SetOutputState(25, !await controller.GetOutputState(25));
					await controller.SetOutputState(27, !await controller.GetOutputState(27));
					break;
				case ConsoleKey.PageDown:
					Console.WriteLine("Output toggle: Office blinds down");
					await controller.SetOutputState(26, !await controller.GetOutputState(26));
					await controller.SetOutputState(28, !await controller.GetOutputState(28));
					break;
				case ConsoleKey.Insert:
					Console.WriteLine("Output toggle: Office left blinds up");
					await controller.SetOutputState(27, !await controller.GetOutputState(27));
					break;
				case ConsoleKey.Delete:
					Console.WriteLine("Output toggle: Office left blinds down");
					await controller.SetOutputState(28, !await controller.GetOutputState(28));
					break;
				case ConsoleKey.Home:
					Console.WriteLine("Output toggle: Office right blinds up");
					await controller.SetOutputState(25, !await controller.GetOutputState(25));
					break;
				case ConsoleKey.End:
					Console.WriteLine("Output toggle: Office right blinds down");
					await controller.SetOutputState(26, !await controller.GetOutputState(26));
					break;
				case ConsoleKey.S:
					Console.WriteLine("Outputs state");
					var outputs = await controller.GetOutputsState();
					PrintOutputs(outputs);
					break;
				case ConsoleKey.Z:
					Console.WriteLine("Inputs state");
					var zones = await controller.GetZonesViolations();
					PrintOutputs(zones);
					break;
				case ConsoleKey.T:
					Console.WriteLine("Staircase temperature");
					var temp = await controller.GetZoneTemperature(51);
					if (!temp.HasValue)
					{
						Console.WriteLine("Temperature undetermined");
						break;
					}

					Console.WriteLine($"Temperature: {temp.Value:F1}°C");
					break;
				case ConsoleKey.V:
					var version = await controller.GetIntegraVersion();
					Console.WriteLine(version.IntegraType.ToString());
					Console.WriteLine(version.FirmwareVersion);
					Console.WriteLine(version.Language.ToString());
					Console.WriteLine("Settings stored in flash: " + version.SettingsStoredInFlash);

					var commVersion = await controller.GetCommunicationDeviceVersion();
					Console.WriteLine(commVersion.FirmwareVersion);
					Console.WriteLine("Serves 32 byte data: " + commVersion.CanServe32ByteResponses);
					Console.WriteLine("Serves 8 part troubles: " + commVersion.CanServeTroublesPart8);

					break;
				case ConsoleKey.O:
					for (byte i = 1; i < 128; ++i)
					{
						var output = await controller.GetOutputDefinition(i);
						Console.WriteLine($"{output.Number,3}: {output.Name} (fn {output.OutputFunction})");
					}
					break;
				case ConsoleKey.Escape:
					return;
			}
		}
	}

	private static IConfiguration SetUpConfiguration(string[] args)
	{
		var configurationBuilder = new ConfigurationBuilder();
		configurationBuilder.SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
		configurationBuilder.AddJsonFile("appsettings.json", optional: true);
		configurationBuilder.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true);
		configurationBuilder.AddCommandLine(args);

		return configurationBuilder.Build();
	}

	private static void PrintOutputs(BitArray outputs)
	{
		var currentColor = Console.ForegroundColor;
		for (var i = 0; i < outputs.Count; ++i)
		{
			Console.ForegroundColor = outputs[i] ? ConsoleColor.Green : ConsoleColor.Red;
			Console.Write((i + 1).ToString().PadLeft(4, ' '));
		}

		Console.ForegroundColor = currentColor;
		Console.WriteLine();
		Console.WriteLine();
	}
}
