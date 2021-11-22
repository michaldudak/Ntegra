using System.Text;

namespace Ntegra;

public static class FirmwareVersionParser
{
	public static string Parse(ReadOnlySpan<byte> bytes)
	{
		var versionString = Encoding.ASCII.GetString(bytes);
		return versionString.Insert(9, "-").Insert(7, "-").Insert(3, " ").Insert(1, ".");
	}
}
