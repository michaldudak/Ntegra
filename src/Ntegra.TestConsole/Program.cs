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

		var outputs = await controller.GetOutputsState();
		PrintOutputs(outputs);

		while (true)
		{
			switch (Console.ReadKey().KeyChar)
			{
				case '1':
					await controller.SetOutputState(10, !await controller.GetOutputState(10));
					break;
				case '2':
					await controller.SetOutputState(11, !await controller.GetOutputState(11));
					break;
			}
		}
	}

	private static IConfiguration SetUpConfiguration(string[] args)
	{
		var configurationBuilder = new ConfigurationBuilder();
		configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
		configurationBuilder.AddJsonFile("appsettings.json");
		configurationBuilder.AddUserSecrets(Assembly.GetExecutingAssembly());
		configurationBuilder.AddCommandLine(args);

		return configurationBuilder.Build();
	}

	private static void PrintOutputs(BitArray outputs)
	{
		var currentColor = Console.ForegroundColor;
		for (var i = 0; i < outputs.Count; ++i)
		{
			Console.ForegroundColor = outputs[i] ? ConsoleColor.Green : ConsoleColor.Red;
			Console.Write(i.ToString().PadLeft(4, ' '));
		}

		Console.ForegroundColor = currentColor;
		Console.WriteLine();
		Console.WriteLine();
	}
}
