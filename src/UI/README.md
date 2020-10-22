MyScript Interactive Ink SDK | UI
=================================

This project implements most of the UI elements:

- [`InteractiveInkCanvas`](Xaml/Controls/InteractiveInkCanvas.xaml) implements [`IRenderTarget`](https://developer.myscript.com/refguides/interactive-ink/android/1.4/com/myscript/iink/IRenderTarget.html), and is the main render target for interactive ink, whereas:
  - [`Canvas`](Commands/Canvas.cs) implements [`ICanvas`](https://developer.myscript.com/refguides/interactive-ink/android/1.4/index.html?com/myscript/iink/graphics/ICanvas.html), receives drawing commands and realizes the concrete drawings.
  - [`Path`](Commands/Path.cs) implements [`IPath`](https://developer.myscript.com/refguides/interactive-ink/android/1.4/index.html?com/myscript/iink/graphics/IPath.html), receives SVG path drawing commands and realizes the concrete drawings.
- [`InteractiveInkToolbar`](Xaml/Controls/InteractiveInkToolbar.xaml) contains the Windows [`InkToolbar`](https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.inktoolbar) for ink-related features in an [associated](../App/Views/MainPage.xaml#L25) [`InteractiveInkCanvas`](Xaml/Controls/InteractiveInkCanvas.xaml).
- [`FontMetricsService`](Services/FontMetricsService.cs) implements [`IFontMetricsProvider`](https://developer.myscript.com/refguides/interactive-ink/android/1.4/index.html?com/myscript/iink/text/IFontMetricsProvider.html), provides digital text typesetting operations, see [implementation](Services/FontMetricsService.cs).
