

namespace System.IO.Packaging.Tests;

public static class Random
{
    [ThreadStatic]
    private static System.Random _instance;

    public static System.Random Shared => _instance ??= new System.Random();

}
