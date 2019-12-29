using System;
using System.Reflection;

namespace Sample.Core
{
    public static class AssemblyMetadata
    {
        private static Lazy<string> _fileVersion = new Lazy<string>(() =>
        {
            var assembly = typeof(AssemblyMetadata).Assembly;
            var attribute = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
            return attribute?.Version;
        });

        private static Lazy<string> _assemblyVersion = new Lazy<string>(() =>
        {
            var assembly = typeof(AssemblyMetadata).Assembly;
            var attribute = assembly.GetCustomAttribute<AssemblyVersionAttribute>();
            return attribute?.Version;
        });

        private static Lazy<string> _informationVersion = new Lazy<string>(() =>
        {
            var assembly = typeof(AssemblyMetadata).Assembly;
            var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            return attribute?.InformationalVersion;
        });

        public static string FileVersion => _fileVersion.Value;

        public static string AssemblyVersion => _assemblyVersion.Value;

        public static string InformationalVersion => _informationVersion.Value;
    }
}
