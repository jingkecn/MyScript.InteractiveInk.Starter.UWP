using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using MyScript.IInk;
using MyScript.InteractiveInk.Annotations;
using MyScript.InteractiveInk.Common.Constants;

namespace MyScript.InteractiveInk.Extensions
{
    public static partial class ContentPackageExtensions
    {
        public static IEnumerable<ContentPart> GetPages([NotNull] this ContentPackage source)
        {
            var count = source.PartCount;
            for (var index = 0; index < count; index++)
            {
                yield return source.GetPart(index);
            }
        }
    }

    public static partial class ContentPackageExtensions
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

    public static partial class ContentPackageExtensions
    {
        [CanBeNull]
        public static async Task<StorageFile> GetAssociatedFile([NotNull] this ContentPackage source)
        {
            var token = source.GetValue(Parameters.ParamFileToken, string.Empty);
            Debug.WriteLine($"{nameof(ContentPackage)}.{nameof(GetAssociatedFile)}: {token}");
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            return await StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);
        }

        #region Save

        public static void Save([NotNull] this ContentPackage source, [CanBeNull] string path = null)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    source.Save();
                    return;
                }

                source.SaveAs(path);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        public static async Task SaveAsync([NotNull] this ContentPackage source, bool saveAsNew = false,
            [NotNull] StorageFile file = null)
        {
            if (saveAsNew || file == null)
            {
                // Picks a file from the file picker.
                var picker = new FileSavePicker
                {
                    SuggestedFileName = "New Document", SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                };
                picker.FileTypeChoices.Add("MyScript Interactive Ink File", new[] {".iink"});
                if (!(await picker.PickSaveFileAsync() is { } picked))
                {
                    return;
                }

                file = picked;
            }

            var token = StorageApplicationPermissions.FutureAccessList.Add(file);
            Debug.WriteLine($"{nameof(ContentPackage)}.{nameof(SaveAsync)}: {token}");
            source.SetValue(Parameters.ParamFileToken, token);
            var tempPath = Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, file.Name);
            source.Save(tempPath);
            var temp = await StorageFile.GetFileFromPathAsync(tempPath);
            await temp.CopyAndReplaceAsync(file);
            await temp.DeleteAsync(StorageDeleteOption.PermanentDelete);
        }

        #endregion
    }
}
