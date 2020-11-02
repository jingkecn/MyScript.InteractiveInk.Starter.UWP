using System.ComponentModel;

namespace MyScript.InteractiveInk.UI.Enumerations
{
    public enum PartType
    {
        [DisplayName("Diagram")] Diagram,
        [DisplayName("Math")] Math,
        [DisplayName("Raw Content")] RawContent,
        [DisplayName("Text")] Text,
        [DisplayName("Text Document")] TextDocument
    }
}
