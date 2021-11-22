namespace Ntegra;

public class CommunicationModuleVersion
{
	public string FirmwareVersion { get; }

	public bool CanServe32ByteResponses { get; }

	public bool CanServeTroublesPart8 { get; }

	internal CommunicationModuleVersion(byte[] data)
	{
		if (data == null || data.Length != 13 || data[0] != 0x7C)
		{
			throw new ArgumentException("Invalid data supplied to CommunicationModuleVersion", nameof(data));
		}

		FirmwareVersion = FirmwareVersionParser.Parse(data.AsSpan(1, 11));
		CanServe32ByteResponses = (data[12] & 0x01) == 0x01;
		CanServeTroublesPart8 = (data[12] & 0x02) == 0x02;
	}
}
