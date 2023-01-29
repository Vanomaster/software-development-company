using System.IO;

namespace Core;

public static class Constants
{
    public static readonly string ProgramExePath = Directory.GetCurrentDirectory();
    public static readonly string UserDataPath = @$"{ProgramExePath}\UserData.txt";
}