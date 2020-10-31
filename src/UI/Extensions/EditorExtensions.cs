using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Input;
using MyScript.IInk;
using MyScript.InteractiveInk.Annotations;

namespace MyScript.InteractiveInk.UI.Extensions
{
    /// <summary>
    ///     Sends pointer events.
    ///     Please be careful with the timestamp: the editor accepts the pointer timestamp in milliseconds, whereas the UWP
    ///     pointer timestamp is in microseconds, therefore, a conversion between these two units is a must.
    ///     Otherwise, you would encounter the following error when handling pointer events on the text document part:
    ///     - ink rejected: stroke is too long.
    ///     This is because, on the text document part, the interactive-ink SDK checks the interval time between the first
    ///     point (pointer down) and the last point (pointer up / cancel) of a single stroke, and raises the error if the
    ///     interval time is too high (> 15s).
    /// </summary>
    public static partial class EditorExtensions
    {
        /// <summary>
        ///     <inheritdoc cref="Editor.PointerCancel" />
        /// </summary>
        /// <param name="source">The source <see cref="Editor" />.</param>
        /// <param name="point">The <see cref="PointerPoint" />.</param>
        public static void PointerCancel([NotNull] this Editor source, PointerPoint point)
        {
            if (!point.Properties.IsPrimary)
            {
                return;
            }

            var id = point.PointerId;
            source.PointerCancel((int)id);
        }

        /// <summary>
        ///     <inheritdoc cref="Editor.PointerDown" />
        /// </summary>
        /// <param name="source">The source <see cref="Editor" />.</param>
        /// <param name="point">The <see cref="PointerPoint" />.</param>
        /// <param name="predominance">The predominant <see cref="PointerType" />.</param>
        public static void PointerDown([NotNull] this Editor source, PointerPoint point,
            [CanBeNull] PointerType? predominance = null)
        {
            if (!point.Properties.IsPrimary)
            {
                return;
            }

            var x = point.Position.X;
            var y = point.Position.Y;
            var timestamp = point.Timestamp.FromMicrosecondsToMilliseconds();
            var pressure = point.Properties.Pressure;
            var type = point.PointerDevice.PointerDeviceType.ToNative(predominance);
            var id = point.PointerId;
            source.PointerDown((float)x, (float)y, (long)timestamp, pressure, type, (int)id);
        }

        /// <summary>
        ///     <inheritdoc cref="Editor.PointerMove" />
        /// </summary>
        /// <param name="source">The source <see cref="Editor" />.</param>
        /// <param name="point">The <see cref="PointerPoint" />.</param>
        /// <param name="predominance">The predominant <see cref="PointerType" />.</param>
        public static void PointerMove([NotNull] this Editor source, PointerPoint point,
            [CanBeNull] PointerType? predominance = null)
        {
            if (!point.IsInContact || !point.Properties.IsPrimary)
            {
                return;
            }

            var x = point.Position.X;
            var y = point.Position.Y;
            var timestamp = point.Timestamp.FromMicrosecondsToMilliseconds();
            var pressure = point.Properties.Pressure;
            var type = point.PointerDevice.PointerDeviceType.ToNative(predominance);
            var id = point.PointerId;
            source.PointerMove((float)x, (float)y, (long)timestamp, pressure, type, (int)id);
        }

        /// <summary>
        ///     <inheritdoc cref="Editor.PointerUp" />
        /// </summary>
        /// <param name="source">The source <see cref="Editor" />.</param>
        /// <param name="point">The <see cref="PointerPoint" />.</param>
        /// <param name="predominance">The predominant <see cref="PointerType" />.</param>
        public static void PointerUp([NotNull] this Editor source, PointerPoint point,
            [CanBeNull] PointerType? predominance = null)
        {
            if (!point.Properties.IsPrimary)
            {
                return;
            }

            var x = point.Position.X;
            var y = point.Position.Y;
            var timestamp = point.Timestamp.FromMicrosecondsToMilliseconds();
            var pressure = point.Properties.Pressure;
            var type = point.PointerDevice.PointerDeviceType.ToNative(predominance);
            var id = point.PointerId;
            source.PointerUp((float)x, (float)y, (long)timestamp, pressure, type, (int)id);
        }
    }

    /// <summary>
    ///     Handles file I/O commands.
    /// </summary>
    public static partial class EditorExtensions
    {
        private const string ParamFileToken = "FileToken";

        #region Open

        public static ContentPackage Open([NotNull] this Editor source, [NotNull] string path)
        {
            return source.Engine?.OpenPackage(path, PackageOpenOption.EXISTING);
        }

        public static async Task<ContentPackage> OpenAsync([NotNull] this Editor source)
        {
            // Picks a file from the file picker.
            var picker = new FileOpenPicker {SuggestedStartLocation = PickerLocationId.DocumentsLibrary};
            picker.FileTypeFilter.Add(".iink");
            if (!(await picker.PickSingleFileAsync() is { } file))
            {
                return null;
            }

            return await source.OpenAsync(file.Path);
        }

        [CanBeNull]
        [SuppressMessage("ReSharper", "MethodHasAsyncOverload")]
        public static async Task<ContentPackage> OpenAsync([NotNull] this Editor source, [NotNull] string path)
        {
            var file = await StorageFile.GetFileFromPathAsync(path);
            var token = StorageApplicationPermissions.FutureAccessList.Add(file);
            // Creates a temporary file and open the content package from the temporary file.
            var temp = await file.CopyAsync(ApplicationData.Current.LocalCacheFolder, file.Name,
                NameCollisionOption.ReplaceExisting);
            var package = source.Open(temp.Path);
            // Updates file access token.
            Debug.WriteLine($"{nameof(Editor)}.{nameof(OpenAsync)}: {token}");
            package.SetValue(ParamFileToken, token);
            // Reads the first content part from the content package.
            if (package.PartCount == 0)
            {
                return package;
            }

            source.Part = package.GetPart(0);
            return package;
        }

        #endregion

        #region Save

        public static void Save([NotNull] this Editor source, [CanBeNull] string path = null)
        {
            try
            {
                var package = source.Part?.Package;
                if (string.IsNullOrEmpty(path))
                {
                    package?.Save();
                    return;
                }

                package?.SaveAs(path);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        public static async Task SaveAsync([NotNull] this Editor source)
        {
            if (!(source.Part?.Package is { } package))
            {
                return;
            }

            var token = package.GetValue(ParamFileToken, string.Empty);
            Debug.WriteLine($"{nameof(Editor)}.{nameof(SaveAsync)}: {token}");
            if (string.IsNullOrEmpty(token))
            {
                await source.SaveAsAsync();
                return;
            }

            var file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);
            await source.SaveAsync(file.Path);
        }

        public static async Task SaveAsync([NotNull] this Editor source, [NotNull] string path)
        {
            if (!(source.Part?.Package is { } package))
            {
                return;
            }

            var file = await StorageFile.GetFileFromPathAsync(path);
            var token = StorageApplicationPermissions.FutureAccessList.Add(file);
            Debug.WriteLine($"{nameof(Editor)}.{nameof(SaveAsAsync)}: {token}");
            package.SetValue(ParamFileToken, token);
            var tempPath = Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, file.Name);
            source.Save(tempPath);
            var temp = await StorageFile.GetFileFromPathAsync(tempPath);
            await temp.CopyAndReplaceAsync(file);
            await temp.DeleteAsync(StorageDeleteOption.PermanentDelete);
            await source.OpenAsync(path);
        }

        public static async Task SaveAsAsync([NotNull] this Editor source)
        {
            var picker = new FileSavePicker
            {
                SuggestedFileName = "New Document", SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            picker.FileTypeChoices.Add("MyScript Interactive Ink", new[] {".iink"});
            if (!(await picker.PickSaveFileAsync() is { } file))
            {
                return;
            }

            await source.SaveAsync(file.Path);
        }

        #endregion
    }

    /// <summary>
    ///     Handles typeset commands.
    /// </summary>
    public static partial class EditorExtensions
    {
        public static void Typeset([NotNull] this Editor source, [CanBeNull] ContentBlock block = null)
        {
            var states = source.GetSupportedTargetConversionStates(block);
            if (!states.Any())
            {
                return;
            }

            source.Convert(block, states.First());
        }

        public static void Typeset([NotNull] this Editor source, float x, float y)
        {
            source.Typeset(source.HitBlock(x, y));
        }

        public static void Typeset([NotNull] this Editor source, Point position)
        {
            source.Typeset((float)position.X, (float)position.Y);
        }
    }

    /// <summary>
    ///     Waits for idle and continues other commands.
    /// </summary>
    public static partial class EditorExtensions
    {
        public static void WaitForIdleAndClear([NotNull] this Editor source)
        {
            if (!source.IsIdle())
            {
                source.WaitForIdle();
            }

            source.Clear();
        }

        public static async Task WaitForIdleAndOpenAsync([NotNull] this Editor source)
        {
            if (!source.IsIdle())
            {
                source.WaitForIdle();
            }

            await source.OpenAsync();
        }

        public static void WaitForIdleAndRedo([NotNull] this Editor source)
        {
            if (!source.IsIdle())
            {
                source.WaitForIdle();
            }

            source.Redo();
        }

        public static async Task WaitForIdleAndSaveAsync([NotNull] this Editor source)
        {
            if (!source.IsIdle())
            {
                source.WaitForIdle();
            }

            await source.SaveAsync();
        }

        public static async Task WaitForIdleAndSaveAsAsync([NotNull] this Editor source)
        {
            if (!source.IsIdle())
            {
                source.WaitForIdle();
            }

            await source.SaveAsAsync();
        }

        public static void WaitForIdleAndTypeset([NotNull] this Editor source, [CanBeNull] ContentBlock block = null)
        {
            if (!source.IsIdle())
            {
                source.WaitForIdle();
            }

            source.Typeset(block);
        }

        public static void WaitForIdleAndTypeset([NotNull] this Editor source, float x, float y)
        {
            if (!source.IsIdle())
            {
                source.WaitForIdle();
            }

            source.Typeset(x, y);
        }

        public static void WaitForIdleAndTypeset([NotNull] this Editor source, Point position)
        {
            if (!source.IsIdle())
            {
                source.WaitForIdle();
            }

            source.Typeset(position);
        }

        public static void WaitForIdleAndUndo([NotNull] this Editor source)
        {
            if (!source.IsIdle())
            {
                source.WaitForIdle();
            }

            source.Undo();
        }
    }
}
