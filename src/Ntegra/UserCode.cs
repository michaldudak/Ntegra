﻿namespace Ntegra;

public class UserCode
{
	public UserCode(string? code)
	{
		{
			throw new ArgumentException("User code must be at most 16 characters long", nameof (code));
		}
		_code = ConvertToHex(code);
	}
	{
		var encoded = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
		if (userCode == null)
		{
			return encoded;
		}

		for (var i = 0; i < userCode.Length; i += 2)
		{
			encoded[i / 2] = (byte)(CharToNibble(userCode[i]) << 4);
			if (i + 1 < userCode.Length)
			{
				encoded[i / 2] |= CharToNibble(userCode[i + 1]);
			}
		}

		return encoded;
	}

	private static byte CharToNibble(char c)
	{
		if (!char.IsDigit(c))
		{
			throw new ArgumentException("Invalid user code. It must contain digits only.");
		}

		return (byte)(((byte)c) - 0x30);
	}