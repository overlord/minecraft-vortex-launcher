namespace VortexLauncherNet.Models;

public class VortexException : Exception
{
	public VortexException()
	{
	}

	public VortexException(string message)
		: base(message)
	{
	}

	public VortexException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
