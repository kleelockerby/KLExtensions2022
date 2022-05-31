using EnvDTE;
using System;
using System.Collections.Generic;
using KLExtensions2022.Extensions;
using Microsoft.VisualStudio.Shell;

namespace KLExtensions2022.Helpers
{
    public static class TextDocumentHelper
    {
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
            ThreadHelper.ThrowIfNotOnUIThread();
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
            ThreadHelper.ThrowIfNotOnUIThread();
            TextRanges dummy = null;
            int lastCount = -1;
            while (textSelection.ReplacePattern(patternString, replacementString, StandardFindOptions, ref dummy))
            {
                if (lastCount == dummy.Count)
                {
                    //OutputWindowHelper.WarningWriteLine("Forced a break out of TextDocumentHelper's SubstituteAllStringMatches for a selection.");
                    break;
                }
                lastCount = dummy.Count;
            }
        }

        internal static void SubstituteAllStringMatches(EditPoint startPoint, EditPoint endPoint, string patternString, string replacementString)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
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
    }
}
