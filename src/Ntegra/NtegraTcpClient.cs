using System.Net.Sockets;

namespace Ntegra;

public class NtegraTcpClient : IDisposable
{
	private readonly string _address;
	private readonly int _port;
	private TcpClient _tcpClient;
	private NetworkStream _stream;

	public NtegraTcpClient(string address, int port)
	{
		_address = address;
		_port = port;
		_tcpClient = new TcpClient(address, port);
		_stream = _tcpClient.GetStream();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_stream.Dispose();
			_tcpClient.Dispose();
		}
	}

	private void Reconnect()
	{
		var oldStream = _stream;
		var oldClient = _tcpClient;

		_tcpClient = new TcpClient(_address, _port);
		_stream = _tcpClient.GetStream();

		oldStream.Dispose();
		oldClient.Dispose();
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public async Task<byte[]> SendCommand(Command command, params byte[] data)
	{
		var attempt = 0;
		do
		{
			attempt++;

			var checksum = Checksum.Calculate(command, data);

			var sendBuffer = new List<byte>(data.Length + 6)
			{
				0xFE,
				0xFE,
				(byte)command
			};

			foreach (var b in data)
			{
				sendBuffer.Add(b);
				if (b == 0xFE)
				{
					sendBuffer.Add(0xF0);
				}
			}

			sendBuffer.Add(checksum.High);
			sendBuffer.Add(checksum.Low);
			sendBuffer.Add(0xFE);
			sendBuffer.Add(0x0D);

			if (!_stream.CanWrite)
			{
				Reconnect();
				continue;
			}

			await _stream.WriteAsync(sendBuffer.ToArray().AsMemory(0, sendBuffer.Count));
			var response = await ReceiveResponse();
			if (response != null)
			{
				return response;
			}
			else
			{
				Reconnect();
			}
		} while (attempt < 3);

		throw new InvalidResponseException();
	}

	private async Task<byte[]?> ReceiveResponse()
	{
		var receiveBuffer = new Memory<byte>(new byte[100]);
		int readBytes;

		if (_stream is null || !_stream.CanRead)
		{
			return null;
		}

		readBytes = await _stream.ReadAsync(receiveBuffer);
		if (readBytes < 7)
		{
			return null;
		}

		if (receiveBuffer.Span[0] != 0xFE ||			receiveBuffer.Span[1] != 0xFE ||			receiveBuffer.Span[readBytes - 2] != 0xFE ||			receiveBuffer.Span[readBytes - 1] != 0x0D)
		{
			return null;
		}

		var response = new List<byte>();
		for (var i = 2; i < readBytes - 2; i++)
		{
			response.Add(receiveBuffer.Span[i]);
			if (receiveBuffer.Span[i] == 0xFE && receiveBuffer.Span[i + 1] == 0xF0)
			{
				i++;
			}
		}
		var receivedChecksum = new Checksum(response[^2], response[^1]);
		var payload = response.SkipLast(2);
		var calculatedChecksum = Checksum.Calculate(payload);

		if (receivedChecksum != calculatedChecksum)
		{
			throw new Exception("Invalid checksum");
		}

		return payload.ToArray();
	}
}

public class InvalidResponseException : Exception
{
}
