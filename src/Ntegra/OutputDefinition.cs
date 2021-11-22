using System.Text;

namespace Ntegra;

public class OutputDefinition
{
	public byte OutputFunction { get; }

	public string Name { get; }

	public ushort Number { get; }

	internal OutputDefinition(byte[] data)
	{
		if (data == null || data.Length != 20 || data[0] != 0xEE)
		{
			throw new ArgumentException("Invalid data provided to OutputDefinition constructor", nameof(data));
		}

		Number = data[2] == 0 ? (ushort)256 : data[2];
		OutputFunction = data[3];
		Name = Encoding.GetEncoding(1250).GetString(data.AsSpan(4, 16));
	}
}
