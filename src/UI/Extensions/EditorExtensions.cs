using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Input;
using MyScript.IInk;
using MyScript.InteractiveInk.Annotations;
using MyScript.InteractiveInk.UI.Enumerations;

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
        #region Save

        public static async Task SaveAsync([NotNull] this Editor source, bool saveAsNew = false)
        {
            if (!(source.Part?.Package is { } package))
            {
                return;
            }

            await package.SaveAsync(saveAsNew, await package.GetAssociatedFile());
            await source.OpenAsync(await package.GetAssociatedFile());
        }

        #endregion

        #region Open

        public static ContentPackage Open([NotNull] this Editor source, [NotNull] string path,
            [CanBeNull] PartType? type = null)
        {
            var package = source.Engine.Open(path);
            source.Part = type.HasValue ? package.CreatePart(type.Value.ToNative()) :
                package.PartCount != 0 ? package.GetPart(0) : source.Part;
            return package;
        }

        [CanBeNull]
        public static async Task<ContentPackage> OpenAsync([NotNull] this Editor source,
            [CanBeNull] StorageFile file = null, [CanBeNull] PartType? type = null)
        {
            var engine = source.Engine;
            if (!(await engine.OpenAsync(file) is { } package))
            {
                return null;
            }

            source.Part = type.HasValue ? package.CreatePart(type.Value.ToNative()) :
                package.PartCount != 0 ? package.GetPart(0) : source.Part;
            return package;
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
