using EnvDTE;
using System;
using System.Collections.Generic;
using KLExtensions2022.Extensions;
using Microsoft.VisualStudio.Shell;
using System.Text.RegularExpressions;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using System.Linq;
using Microsoft.VisualStudio.Text;

namespace KLExtensions2022.Helpers
{
    public static class TextDocumentHelper
    {
        private static readonly DTE2 Dte2 = KLExtensions2022Package.DTE2 as DTE2;
        internal static string commentPreffix = "// ";
        internal const int StandardFindOptions = (int)(vsFindOptions.vsFindOptionsRegularExpression | vsFindOptions.vsFindOptionsMatchInHiddenText);

        internal static IEnumerable<EditPoint> FindMatches(TextDocument textDocument, string patternString)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var matches = new List<EditPoint>();
            EditPoint cursor = textDocument.StartPoint.CreateEditPoint();
            EditPoint end = null;
            TextRanges dummy = null;

            while (cursor != null && cursor.FindPattern(patternString, StandardFindOptions, ref end, ref dummy))
            {
                matches.Add(cursor.CreateEditPoint());
                cursor = end;
            }

            return matches;
        }

        internal static IEnumerable<EditPoint> FindMatches(TextSelection textSelection, string patternString)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var matches = new List<EditPoint>();
            var cursor = textSelection.TopPoint.CreateEditPoint();
            EditPoint end = null;
            TextRanges dummy = null;

            while (cursor != null && cursor.FindPattern(patternString, StandardFindOptions, ref end, ref dummy))
            {
                if (end.AbsoluteCharOffset > textSelection.BottomPoint.AbsoluteCharOffset)
                {
                    break;
                }

                matches.Add(cursor.CreateEditPoint());
                cursor = end;
            }

            return matches;
        }

        internal static EditPoint FirstOrDefaultMatch(TextDocument textDocument, string patternString)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var cursor = textDocument.StartPoint.CreateEditPoint();
            EditPoint end = null;
            TextRanges dummy = null;

            if (cursor != null && cursor.FindPattern(patternString, StandardFindOptions, ref end, ref dummy))
            {
                return cursor.CreateEditPoint();
            }

            return null;
        }

        internal static string GetTextToFirstMatch(TextPoint startPoint, string matchString)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var startEditPoint = startPoint.CreateEditPoint();
            var endEditPoint = startEditPoint.CreateEditPoint();
            TextRanges subGroupMatches = null;

            if (endEditPoint.FindPattern(matchString, StandardFindOptions, ref endEditPoint, ref subGroupMatches))
            {
                return startEditPoint.GetText(endEditPoint);
            }

            return null;
        }

        internal static void InsertBlankLineBeforePoint(EditPoint point)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (point.Line <= 1) return;

            point.LineUp(1);
            point.StartOfLine();

            string text = point.GetLine();
            if (RegexNullSafe.IsMatch(text, @"^\s*[^\s\{]"))
            {
                point.EndOfLine();
                point.Insert(Environment.NewLine);
            }
        }

        internal static void InsertBlankLineAfterPoint(EditPoint point)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (point.AtEndOfDocument) return;

            point.LineDown(1);
            point.StartOfLine();

            string text = point.GetLine();
            if (RegexNullSafe.IsMatch(text, @"^\s*[^\s\}]"))
            {
                point.Insert(Environment.NewLine);
            }
        }

        internal static void SubstituteAllStringMatches(TextDocument textDocument, string patternString, string replacementString)
        {
            //ThreadHelper.ThrowIfNotOnUIThread();
            TextRanges dummy = null;
            int lastCount = -1;
            while (textDocument.ReplacePattern(patternString, replacementString, StandardFindOptions, ref dummy))
            {
                if (lastCount == dummy.Count)
                {
                    //OutputWindowHelper.WarningWriteLine("Forced a break out of TextDocumentHelper's SubstituteAllStringMatches for a document.");
                    break;
                }
                lastCount = dummy.Count;
            }
        }

        internal static void SubstituteAllStringMatches(TextSelection textSelection, string patternString, string replacementString)
        {
            //ThreadHelper.ThrowIfNotOnUIThread();
            TextRanges dummy = null;
            int lastCount = -1;

            while (textSelection.ReplacePattern(patternString, replacementString, StandardFindOptions, ref dummy))
            {
                if (lastCount == dummy.Count)
                {
                    Console.WriteLine("Forced a break out of TextDocumentHelper's SubstituteAllStringMatches for a selection.");
                    break;
                }
                lastCount = dummy.Count;
            }
        }

        internal static void SubstituteAllStringMatches(EditPoint startPoint, EditPoint endPoint, string patternString, string replacementString)
        {
            //ThreadHelper.ThrowIfNotOnUIThread();
            TextRanges dummy = null;
            int lastCount = -1;
            while (startPoint.ReplacePattern(endPoint, patternString, replacementString, StandardFindOptions, ref dummy))
            {
                if (lastCount == dummy.Count)
                {
                    //OutputWindowHelper.WarningWriteLine("Forced a break out of TextDocumentHelper's SubstituteAllStringMatches for a pair of points.");
                    break;
                }
                lastCount = dummy.Count;
            }
        }

        internal static string NormalizeLineEndings(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return content;
            }
            return Regex.Replace(content, @"\r\n|\n\r|\n|\r", "\r\n");
        }

        internal static void RefreshComment()
        {
            var textDocument = Dte2.ActiveDocument.Object("TextDocument") as TextDocument;
            commentPreffix = "# ";
        }

        internal static void Swap<T>(ref T a, ref T b)
        {
            T temp = b;
            b = a;
            a = temp;
        }

        internal static int GetCaretPosition(this IWpfTextView obj)
        {
            return obj.Caret.Position.BufferPosition;
        }

        public static string Comment(this string text)
        {
            var textLines = text.Replace(Environment.NewLine, "\n").Split('\n')
                                .Select(x => new
                                {
                                    Text = x,
                                    TextStart = x.IndexOfNonWhitespace(),
                                    IsEmpty = (x == "")
                                });

            int indent = textLines.Where(x => !x.IsEmpty).Min(x => x.TextStart);
            string[] replacementLines = textLines.Select(x =>
            {
                if(x.IsEmpty)
                    return x.Text;
                else
                    return x.Text.CommentText(indent, true);
            }).ToArray();

            var commentedText = string.Join(Environment.NewLine, replacementLines);
            return commentedText;
        }

        public static int IndexOfNonWhitespace(this string text)
        {
            return text.TakeWhile(c => char.IsWhiteSpace(c)).Count();
        }

        public static string CommentText(this string text, int indent, bool doComment)
        {
            int textStart = text.IndexOfNonWhitespace();

            if(doComment)
            {
                if(textStart < indent)
                    return new string(' ', indent - textStart) + commentPreffix + text.Substring(textStart);
                else
                    return text.Substring(0, indent) + commentPreffix + text.Substring(indent);
            }
            else
            {
                return text.Substring(0, textStart) + text.Substring(textStart).TrimCommentPreffix();
            }
        }

        static public string TrimCommentPreffix(this string text)
        {
            if(text.StartsWith(commentPreffix))
                return text.Substring(commentPreffix.Length);
            else
                return text;
        }

        public static ITextSnapshotLine GetLine(this IWpfTextView obj, int lineNumber)
        {
            return obj.TextSnapshot.GetLineFromLineNumber(lineNumber);
        }

        public static void SetSelection(this IWpfTextView obj, int start, int length)
        {
            SnapshotPoint selectionStart = new SnapshotPoint(obj.TextSnapshot, start);
            var selectionSpan = new SnapshotSpan(selectionStart, length);

            obj.Selection.Select(selectionSpan, false);
        }

        public static void MoveCaretTo(this IWpfTextView obj, int position)
        {
            obj.Caret.MoveTo(new SnapshotPoint(obj.TextSnapshot, position));
        }

        public static string GetText(this IWpfTextView obj)
        {
            return obj.TextSnapshot.GetText();
        }
    }
}
