using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Input;
using Microsoft.Win32.SafeHandles;
using MyScript.IInk;
using MyScript.InteractiveInk.Annotations;
using MyScript.InteractiveInk.Common.Constants;
using MyScript.InteractiveInk.Common.Enumerations;

namespace MyScript.InteractiveInk.Extensions
{
    public static partial class EditorExtensions
    {
        public static void AddBlockAt([NotNull] this Editor source, Point position, ContentType type)
        {
            source.AddBlock((float)position.X, (float)position.Y, type.ToNative());
        }

        /// <summary>
        ///     Append a <see cref="ContentBlock" /> to the end of a document.
        /// </summary>
        /// <param name="source">The source <see cref="Editor" />.</param>
        /// <param name="type">
        ///     <inheritdoc cref="ContentType" />
        /// </param>
        /// <param name="autoScroll">
        ///     Set <code>true</code> to auto scroll to the new <see cref="ContentBlock" />, otherwise
        ///     <code>false</code>. The default value is <code>true</code>.
        /// </param>
        /// <param name="target">
        ///     <inheritdoc cref="IRenderTarget" />
        /// </param>
        public static void AppendBlock([NotNull] this Editor source, ContentType type, bool autoScroll = true,
            [CanBeNull] IRenderTarget target = null)
        {
            if (!source.CanAddBlock(type) || !(source.Renderer is {} renderer))
            {
                return;
            }

            var block = source.GetRootBlock();
            var dpi = renderer.GetDpi();
            var box = block.Box.ToPlatform().FromMillimeterToPixel(dpi);
            var lineHeight = default(float);
            var styles = source.ListStyleClasses(_ => true);
            if (styles.TryGetValue("guide", out var style))
            {
                lineHeight = style.FontLineHeight.FromMillimeterToPixel(dpi.Y);
            }

            var (x, y) = (box.Left, box.Bottom);
            source.AddBlock((float)x, (float)y + lineHeight, type.ToNative());
            if (!autoScroll)
            {
                return;
            }

            renderer.ScrollTo(new IInk.Graphics.Point((float)x, (float)y), target, source.ClampViewOffset);
        }

        public static async Task AddImageAtAsync([NotNull] this Editor source, Point position)
        {
            // Picks a file from the file picker.
            var picker = new FileOpenPicker {SuggestedStartLocation = PickerLocationId.PicturesLibrary};
            picker.FileTypeFilter.Add(MimeType.GIF.ToFileType());
            picker.FileTypeFilter.Add(MimeType.JPEG.ToFileType());
            picker.FileTypeFilter.Add(MimeType.PNG.ToFileType());
            picker.FileTypeFilter.Add(MimeType.SVG.ToFileType());
            if (!(await picker.PickSingleFileAsync() is { } picked))
            {
                return;
            }

            var folder = ApplicationData.Current.LocalCacheFolder;
            var file = await picked.CopyAsync(folder, picked.Name, NameCollisionOption.GenerateUniqueName);
            var (x, y) = ((float)position.X, (float)position.Y);
            source.AddImage(x, y, file.Path, file.FileType.ToMimeType());
        }

        public static async Task AppendImageAsync([NotNull] this Editor source, bool autoScroll = true,
            [CanBeNull] IRenderTarget target = null)
        {
            if (!(source.Renderer is { } renderer))
            {
                return;
            }

            var block = source.GetRootBlock();
            var dpi = renderer.GetDpi();
            var box = block.Box.ToPlatform().FromMillimeterToPixel(dpi);
            var lineHeight = default(float);
            var styles = source.ListStyleClasses(_ => true);
            if (styles.TryGetValue("guide", out var style))
            {
                lineHeight = style.FontLineHeight.FromMillimeterToPixel(dpi.Y);
            }

            var (x, y) = ((float)box.Left, (float)box.Bottom);
            await source.AddImageAtAsync(new Point(x, y + lineHeight));
            if (!autoScroll)
            {
                return;
            }

            renderer.ScrollTo(new IInk.Graphics.Point(x, y), target, source.ClampViewOffset);
        }

        public static bool CanAddBlock([NotNull] this Editor source, ContentType type, bool defaultValue = default)
        {
            return source.SupportedAddBlockTypes?.Contains(type.ToNative()) ?? defaultValue;
        }

        [CanBeNull]
        public static ContentBlock GetBlockAt([NotNull] this Editor source, Point position)
        {
            return source.HitBlock((float)position.X, (float)position.Y);
        }

        public static void RemoveBlockAt([NotNull] this Editor source, Point position)
        {
            var block = source.HitBlock((float)position.X, (float)position.Y);
            if (block?.IsContainer() ?? true)
            {
                return;
            }

            source.RemoveBlock(block);
            block.Dispose();
        }
    }

    public static partial class EditorExtensions
    {
        public static bool HasNextPage([NotNull] this Editor source)
        {
            return source.Part?.HasNext() ?? false;
        }

        public static bool HasPreviousPage([NotNull] this Editor source)
        {
            return source.Part?.HasPrevious() ?? false;
        }
    }

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
        private static Dictionary<string, SafeFileHandle> SafeFileHandles { get; } =
            new Dictionary<string, SafeFileHandle>();

        #region Save

        public static async Task SaveAsync([NotNull] this Editor source, bool saveAsNew = false)
        {
            if (!(source.Part?.Package is { } package))
            {
                return;
            }

            // Unlock the target file if any.
            var token = package.GetValue(Parameters.ParamFileToken, string.Empty);
            if (SafeFileHandles.TryGetValue(token, out var handle))
            {
                handle.Close();
            }

            await package.SaveAsync(saveAsNew, await package.GetAssociatedFile());
            await source.OpenAsync(await package.GetAssociatedFile());
        }

        #endregion

        #region Open

        public static ContentPackage Open([NotNull] this Editor source, [NotNull] string path,
            [CanBeNull] ContentType? type = null)
        {
            var package = source.Engine.Open(path);
            source.Part = type.HasValue ? package.CreatePart(type.Value.ToNative()) :
                package.PartCount != 0 ? package.GetPart(0) : source.Part;
            return package;
        }

        [CanBeNull]
        public static async Task<ContentPackage> OpenAsync([NotNull] this Editor source,
            [CanBeNull] StorageFile file = null, [CanBeNull] ContentType? type = null)
        {
            // Unlock the target file if any.
            file?.CreateSafeFileHandle()?.Close();
            var engine = source.Engine;
            if (!(await engine.OpenAsync(file) is { } package))
            {
                return null;
            }

            source.Part = type.HasValue ? package.CreatePart(type.Value.ToNative()) :
                package.PartCount != 0 ? package.GetPart(0) : source.Part;
            file = await package.GetAssociatedFile();
            // Unlock the previous file if any.
            var token = package.GetValue(Parameters.ParamFileToken, string.Empty);
            if (SafeFileHandles.TryGetValue(token, out var handle))
            {
                handle.Close();
            }

            // Re-lock the target file.
            SafeFileHandles[token] = file.CreateSafeFileHandle(share: FileShare.None);
            return package;
        }

        #endregion
    }

    public static partial class EditorExtensions
    {
        public static void GoToNextPage([NotNull] this Editor source)
        {
            source.Part = source.Part?.GetNext() ?? source.Part;
        }

        public static void GoToPreviousPage([NotNull] this Editor source)
        {
            source.Part = source.Part?.GetPrevious() ?? source.Part;
        }
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

        public static async Task<ContentPackage> WaitForIdleAndOpenAsync([NotNull] this Editor source)
        {
            if (!source.IsIdle())
            {
                source.WaitForIdle();
            }

            return await source.OpenAsync();
        }

        public static void WaitForIdleAndRedo([NotNull] this Editor source)
        {
            if (!source.IsIdle())
            {
                source.WaitForIdle();
            }

            source.Redo();
        }

        public static async Task WaitForIdleAndSaveAsync([NotNull] this Editor source, bool saveAsNew = false)
        {
            if (!source.IsIdle())
            {
                source.WaitForIdle();
            }

            await source.SaveAsync(saveAsNew);
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
