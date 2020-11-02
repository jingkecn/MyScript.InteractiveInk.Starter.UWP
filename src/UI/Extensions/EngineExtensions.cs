using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using MyScript.IInk;
using MyScript.InteractiveInk.Annotations;
using MyScript.InteractiveInk.UI.Constants;

namespace MyScript.InteractiveInk.UI.Extensions
{
    public static class EngineExtensions
    {
        #region Open

        public static ContentPackage Open([NotNull] this Engine source, [NotNull] string path)
        {
            return source.OpenPackage(path, PackageOpenOption.CREATE);
        }

        public static async Task<ContentPackage> OpenAsync([NotNull] this Engine source,
            [CanBeNull] StorageFile file = null)
        {
            if (file == null)
            {
                // Picks a file from the file picker.
                var picker = new FileOpenPicker {SuggestedStartLocation = PickerLocationId.DocumentsLibrary};
                picker.FileTypeFilter.Add(".iink");
                if (!(await picker.PickSingleFileAsync() is { } picked))
                {
                    return null;
                }

                file = picked;
            }

            var token = StorageApplicationPermissions.FutureAccessList.Add(file);
            // Creates a temporary file and open the content package from the temporary file.
            var temp = await file.CopyAsync(ApplicationData.Current.LocalCacheFolder, file.Name,
                NameCollisionOption.ReplaceExisting);
            // ReSharper disable once MethodHasAsyncOverload
            var package = source.Open(temp.Path);
            // Updates file access token.
            Debug.WriteLine($"{nameof(Editor)}.{nameof(OpenAsync)}: {token}");
            package.SetValue(Parameters.ParamFileToken, token);
            return package;
        }

        #endregion
    }
}
