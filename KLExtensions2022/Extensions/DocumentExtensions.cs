using EnvDTE;

namespace KLExtensions2022.Extensions
{
    internal static class DocumentExtensions
    {
        internal static TextDocument GetTextDocument(this Document document)
        {
            return document.Object("TextDocument") as TextDocument;
        }
    }
}
