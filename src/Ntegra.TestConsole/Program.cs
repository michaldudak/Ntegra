using System.Collections;
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
			var key = Console.ReadKey(true);
			if (key.Key == ConsoleKey.Escape)
			{
				break;
			}

			switch (key.KeyChar)
			{
				case '1':
					await controller.SetOutputState(11, !await controller.GetOutputState(11));
					break;
				case '2':
					await controller.SetOutputState(12, !await controller.GetOutputState(12));
					break;
				case '3':
					await controller.SetOutputState(13, !await controller.GetOutputState(13));
					break;
				case '5':
					await controller.SetOutputState(25, !await controller.GetOutputState(25));
					break;
				case '6':
					await controller.SetOutputState(26, !await controller.GetOutputState(26));
					break;
				case 's':
					var outputs = await controller.GetOutputsState();
					PrintOutputs(outputs);
					break;
				case 'z':
					var zones = await controller.GetZonesViolations();
					PrintOutputs(zones);
					break;
				case 't':
					var temp = await controller.GetZoneTemperature(51);
					if (!temp.HasValue)
					{
						Console.WriteLine("Temperature undetermined");
						break;
					}

					Console.WriteLine($"Temperature: {temp.Value:F1}°C");
					break;
				case 'v':
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
				case 'o':
					for (byte i = 1; i < 128; ++i)
					{
						var output = await controller.GetOutputDefinition(i);
						Console.WriteLine($"{output.Number}: {output.Name} (fn {output.OutputFunction})");
					}
					break;
			}
		}
	}

	private static IConfiguration SetUpConfiguration(string[] args)
	{
		var configurationBuilder = new ConfigurationBuilder();
		configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
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
