using System.Runtime.CompilerServices;

namespace DupeBuster.Tests;

public static class Util
{
    public static string GetMethodName([CallerMemberName] string name = "")
        => name + Random.Shared.Next();
}