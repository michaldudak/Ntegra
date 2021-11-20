namespace Ntegra;

public struct Checksum : IEquatable<Checksum>
{
	public byte High { get; private set; }

	public byte Low { get; private set; }

	private Checksum(ushort checksum)
	{
		High = Convert.ToByte((checksum >> 8) & 0xFF);
		Low = Convert.ToByte(checksum & 0xFF);
	}

	public Checksum(byte high, byte low)
	{
		High = high;
		Low = low;
	}

	public static Checksum Calculate(Command command, IEnumerable<byte> data)
	{
		var payload = (new byte[] { (byte)command }).Concat(data);
		return Calculate(payload);
	}

	public static Checksum Calculate(IEnumerable<byte> payload)
	{
		unchecked
		{
			ushort crc = 0x147a;
			foreach (var b in payload)
			{
				var firstBit = (crc & 0x8000) >> 15;
				crc = (ushort)(((crc << 1) & 0xFFFF) | firstBit);
				crc = (ushort)(crc ^ 0xFFFF);
				crc = (ushort)(crc + (crc >> 8) + b);
			}

			return new Checksum(crc);
		}
	}

	public override bool Equals(object? obj)
	{
		return obj is Checksum checksum && Equals(checksum);
	}

	public bool Equals(Checksum other)
	{
		return High == other.High &&
			   Low == other.Low;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(High, Low);
	}

	public static bool operator ==(Checksum left, Checksum right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Checksum left, Checksum right)
	{
		return !(left == right);
	}
}
