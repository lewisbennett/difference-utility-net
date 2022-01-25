namespace System.Runtime.CompilerServices
{
    // Dummy class, required because this project uses C# 9 features that are only included with .NET 5.0.
    // Since Xamarin cannot reference .NET 5.0 projects, we have to continue using .NET Standard 2.1,
    // meaning that this class isn't included.

    // Link to bug report:
    // https://developercommunity.visualstudio.com/content/problem/1244809/error-cs0518-predefined-type-systemruntimecompiler.html

    internal static class IsExternalInit
    {
    }
}