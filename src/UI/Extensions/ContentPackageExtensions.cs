using MyScript.IInk;
using MyScript.InteractiveInk.Annotations;

namespace MyScript.InteractiveInk.UI.Extensions
{
    public static class ContentPackageExtensions
    {
        #region Getters

        public static bool GetValue([NotNull] this ContentPackage source, [NotNull] string key,
            bool defaultValue = default)
        {
            return source.Metadata?.GetBoolean(key, defaultValue) ?? defaultValue;
        }

        public static double GetValue([NotNull] this ContentPackage source, [NotNull] string key,
            double defaultValue = default)
        {
            return source.Metadata?.GetNumber(key, defaultValue) ?? defaultValue;
        }

        public static string GetValue([NotNull] this ContentPackage source, [NotNull] string key,
            string defaultValue = default)
        {
            return source.Metadata?.GetString(key, defaultValue) ?? defaultValue;
        }

        public static string[] GetValue([NotNull] this ContentPackage source, [NotNull] string key,
            string[] defaultValue = default)
        {
            return source.Metadata?.GetStringArray(key, defaultValue) ?? defaultValue;
        }

        #endregion

        #region Setters

        public static void SetValue([NotNull] this ContentPackage source, [NotNull] string key, bool value)
        {
            var parameters = source.Metadata;
            parameters.SetBoolean(key, value);
            source.Metadata = parameters;
        }

        public static void SetValue([NotNull] this ContentPackage source, [NotNull] string key, double value)
        {
            var parameters = source.Metadata;
            parameters.SetNumber(key, value);
            source.Metadata = parameters;
        }

        public static void SetValue([NotNull] this ContentPackage source, [NotNull] string key, string value)
        {
            var parameters = source.Metadata;
            parameters.SetString(key, value);
            source.Metadata = parameters;
        }

        public static void SetValue([NotNull] this ContentPackage source, [NotNull] string key, string[] value)
        {
            var parameters = source.Metadata;
            parameters.SetStringArray(key, value);
            source.Metadata = parameters;
        }

        #endregion
    }
}
