using System;
using System.Net.Sockets;

namespace Ntegra;

public class NtegraTcpClient : IDisposable
{
	private readonly TcpClient _tcpClient;
	private Stream? _stream;

	public NtegraTcpClient(string address, int port)
	{
		_tcpClient = new TcpClient(address, port);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_tcpClient.Dispose();
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void OpenConnection()
	{
		_stream = _tcpClient.GetStream();
	}


	public async Task<byte[]> SendCommand(Command command, params byte[] data)
	{
		var receiveBuffer = new byte[100];
		var attempt = 0;
		int readBytes;
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

			if (_stream == null || !_tcpClient.Connected)
			{
				OpenConnection();
			}

			await _stream.WriteAsync(sendBuffer.ToArray().AsMemory(0, sendBuffer.Count));

			readBytes = _stream.Read(receiveBuffer, 0, 100);
		} while ((receiveBuffer[0] != receiveBuffer[1] || receiveBuffer[0] != 0xFE) && attempt < 3);

		var response = new List<byte>();
		for (var i = 3; i < readBytes - 4; i++)
		{
			response.Add(receiveBuffer[i]);
			if (receiveBuffer[i] == 0xFE && receiveBuffer[i + 1] == 0xF0)
			{
				i++;
			}
		}

		var sentChecksum = new Checksum(receiveBuffer[readBytes - 4], receiveBuffer[readBytes - 3]);
		var calculatedChecksum = Checksum.Calculate((Command)receiveBuffer[2], response);

		if (sentChecksum != calculatedChecksum)
		{
			throw new Exception("Invalid checksum");
		}

		return response.ToArray();
	}
}
