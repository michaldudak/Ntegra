﻿namespace Ntegra;

public enum Command : byte
{
	OutputsState = 0x17,	ReadZoneTemperature = 0x7D,	IntegraVersion = 0x7E,
	OutputsOn = 0x88,
	OutputsOff = 0x89,
}
