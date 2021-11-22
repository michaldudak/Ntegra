using System;
using System.Net.Sockets;

namespace Ntegra;

public class NtegraTcpClient : IDisposable
{
	private readonly TcpClient _tcpClient;
	private readonly NetworkStream _stream;

	public NtegraTcpClient(string address, int port)
	{
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
				break;
			}

			await _stream.WriteAsync(sendBuffer.ToArray().AsMemory(0, sendBuffer.Count));
			var response = await ReceiveResponse();
			if (response != null)
			{
				return response;
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
		if (readBytes == 0)
		{
			return null;
		}

		if ((receiveBuffer.Span[0] != receiveBuffer.Span[1]) || receiveBuffer.Span[0] != 0xFE)
		{
			return null;
		}

		var response = new List<byte>();
		for (var i = 2; i < readBytes - 4; i++)
		{
			response.Add(receiveBuffer.Span[i]);
			if (receiveBuffer.Span[i] == 0xFE && receiveBuffer.Span[i + 1] == 0xF0)
			{
				i++;
			}
		}

		var sentChecksum = new Checksum(receiveBuffer.Span[readBytes - 4], receiveBuffer.Span[readBytes - 3]);
		var calculatedChecksum = Checksum.Calculate(response);

		if (sentChecksum != calculatedChecksum)
		{
			throw new Exception("Invalid checksum");
		}

		return response.ToArray();
	}
}

public class InvalidResponseException : Exception
{
}
