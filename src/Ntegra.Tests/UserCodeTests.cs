using NUnit.Framework;
using Shouldly;namespace Ntegra.Tests;

public class UserCodeTests
{
	[Test]
	public void ValidUserCodeOfEvenLengthShouldBeCovertedToHex()
	{		var code = new UserCode("12345678");		code.AsBytes.ShouldBe(new byte[] { 0x12, 0x34, 0x56, 0x78, 0xFF, 0xFF, 0xFF, 0xFF });
	}	[Test]
	public void ValidUserCodeOfOddLengthShouldBeCovertedToHex()
	{		var code = new UserCode("123456789");		code.AsBytes.ShouldBe(new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9F, 0xFF, 0xFF, 0xFF });
	}
}