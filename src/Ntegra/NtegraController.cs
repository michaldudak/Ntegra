using System.Collections;
using System.Text;

namespace Ntegra;

public class NtegraController : IDisposable
{
	private readonly NtegraTcpClient _client;
	private readonly UserCode _userCode;
	private bool disposedValue;

	public NtegraController(NtegraTcpClient tcpClient, string? userCode = null)
	{
		_client = tcpClient;
		_userCode = new UserCode(userCode);
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
	}

	public async Task<BitArray> GetZonesViolations()
	{
		var result = await _client.SendCommand(Command.ZonesViolation);
		return new BitArray(result.Skip(1).ToArray());
	}

	public async Task<BitArray> GetOutputsState()
	{
		var result = await _client.SendCommand(Command.OutputsState);
		return new BitArray(result.Skip(1).ToArray());
	}

	public async Task<bool> GetOutputState(ushort outputNumber)
	{
		if (outputNumber == 0)
		{
			throw new ArgumentException("Output number cannot be 0");
		}

		var allOutputs = await GetOutputsState();
		return allOutputs[outputNumber - 1];
	}
	public Task SetOutputState(ushort outputNumber, bool state)
	{
		if (state)
		{
			return SetOutputsOn(new[] { outputNumber });
		}

		return SetOutputsOff(new[] { outputNumber });
	}

	public async Task SetOutputsOn(IEnumerable<ushort> outputNumbers)
	{
		if (outputNumbers.Any(i => i == 0))
		{
			throw new ArgumentException("Output number cannot be 0");
		}

		var bitArray = new BitArray(128, false);
		foreach (var outputNumber in outputNumbers)
		{
			bitArray[outputNumber - 1] = true;
		}

		var bytes = new byte[16];
		bitArray.CopyTo(bytes, 0);
		bytes = PrependUserCode(bytes);
		await _client.SendCommand(Command.OutputsOn, bytes);
	}

	public async Task SetOutputsOff(IEnumerable<ushort> outputNumbers)
	{
		if (outputNumbers.Any(i => i == 0))
		{
			throw new ArgumentException("Output number cannot be 0");
		}

		var bitArray = new BitArray(128, false);
		foreach (var outputNumber in outputNumbers)
		{
			bitArray[outputNumber - 1] = true;
		}

		var bytes = new byte[16];
		bitArray.CopyTo(bytes, 0);
		bytes = PrependUserCode(bytes);
		await _client.SendCommand(Command.OutputsOff, bytes);
	}

	public async Task<CommunicationModuleVersion> GetCommunicationDeviceVersion()
	{
		var result = await _client.SendCommand(Command.CommunicationModuleVersion);
		return new CommunicationModuleVersion(result);
	}

	public async Task<IntegraVersion> GetIntegraVersion()
	{
		var result = await _client.SendCommand(Command.IntegraVersion);
		return new IntegraVersion(result);
	}

	public async Task<decimal?> GetZoneTemperature(byte zoneIndex)
	{
		var result = await _client.SendCommand(Command.ReadZoneTemperature, zoneIndex);
		if (result[0] != 0x7D || result[1] != zoneIndex || (result[2] == 0xFF && result[3] == 0xFF))
		{
			return null;
		}

		var reading = result[2] << 8 | result[3];
		return reading / 2M - 55M;
	}

	public async Task<OutputDefinition> GetOutputDefinition(byte outputIndex)
	{
		var result = await _client.SendCommand(Command.ReadDeviceName, 4, outputIndex);
		return new OutputDefinition(result);
	}

	private byte[] PrependUserCode(byte[] payload)
	{
		return _userCode.AsBytes.Concat(payload).ToArray();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				_client.Dispose();
			}

			disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
