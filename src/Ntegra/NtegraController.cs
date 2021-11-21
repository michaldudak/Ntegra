using System.Collections;

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
	}

	public async Task<BitArray> GetOutputsState()
	{
		var result = await _client.SendCommand(Command.OutputsState);
		return new BitArray(result);
	}

	public async Task<bool> GetOutputState(byte outputIndex)
	{
		var allOutputs = await GetOutputsState();
		return allOutputs[outputIndex];
	}
	public Task SetOutputState(int outputIndex, bool state)
	{
		if (state)
		{
			return SetOutputsOn(new[] { outputIndex });
		}

		return SetOutputsOff(new[] { outputIndex });
	}

	public async Task SetOutputsOn(IEnumerable<int> outputIndexes)
	{
		var bitArray = new BitArray(128, false);
		foreach (var outputIndex in outputIndexes)
		{
			bitArray[outputIndex] = true;
		}

		var bytes = new byte[16];
		bitArray.CopyTo(bytes, 0);
		bytes = PrependUserCode(bytes);
		await _client.SendCommand(Command.OutputsOn, bytes);
	}

	public async Task SetOutputsOff(IEnumerable<int> outputIndexes)
	{
		var bitArray = new BitArray(128, false);
		foreach (var outputIndex in outputIndexes)
		{
			bitArray[outputIndex] = true;
		}

		var bytes = new byte[16];
		bitArray.CopyTo(bytes, 0);
		bytes = PrependUserCode(bytes);
		await _client.SendCommand(Command.OutputsOff, bytes);
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
