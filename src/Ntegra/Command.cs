namespace Ntegra;

public enum Command : byte
{
	ZonesViolation = 0x00,
	OutputsState = 0x17,
	ReadZoneTemperature = 0x7D,
	CommunicationModuleVersion = 0x7C,
	IntegraVersion = 0x7E,
	OutputsOn = 0x88,
	OutputsOff = 0x89,
	ReadDeviceName = 0xEE,
}
