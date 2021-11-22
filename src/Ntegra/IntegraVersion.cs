namespace Ntegra;

public enum IntegraType : byte
{
	Integra24 = 0,
	Integra32 = 1,
	Integra64 = 2,
	Integra128 = 3,
	Integra128WrlSim300 = 4,
	Integra128WrlLeon = 132,
	Integra64Plus = 66,
	Integra128Plus = 67,
	Integra256Plus = 72
}

public class IntegraVersion
{
	public IntegraType IntegraType { get; }

	public string FirmwareVersion { get; }

	public Language Language { get; }

	public bool SettingsStoredInFlash { get; }

	internal IntegraVersion(byte[] data)
	{
		if (data == null || data.Length != 15 || data[0] != 0x7E)
		{
			throw new ArgumentException("Invalid data supplied to IntegraVersion", nameof(data));
		}

		IntegraType = (IntegraType)data[1];
		FirmwareVersion = FirmwareVersionParser.Parse(data.AsSpan(2, 11));
		Language = (Language)data[13];
		SettingsStoredInFlash = data[14] == 0xFF;
	}
}
