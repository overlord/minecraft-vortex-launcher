using VortexLauncherNet.Models;

namespace VortexLauncherNet.Helpers;

internal static class Validator
{
	public static void ValidatePlayerName(string? playerName)
	{
		if (string.IsNullOrEmpty(playerName))
		{
			throw new VortexException("Enter your desired name.");
		}

		if (playerName.Length < 3)
		{
			throw new VortexException("Name is too short! Minimum length is 3.");
		}
	}

	public static void ValidateRam(int? ramAmount)
	{
		if ((ramAmount ?? 0) == 0)
		{
			throw new VortexException("Enter RAM amount.");
		}

		if (ramAmount < 350)
		{
			throw new VortexException("You allocated too low amount of memory!\nAllocate at least 350 MB to prevent crashes.");
		}
	}
}
