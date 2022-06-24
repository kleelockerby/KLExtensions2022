using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text.Formatting;

namespace KLExtensions2022
{
    public class SelectionHelper
    {
        private readonly IWpfTextView view;
        private SnapshotSpan selection;
        private bool shouldSelectionBeApplied;
        private bool isSelectionReversed;

        public SelectionHelper(IWpfTextView view)
        {
            this.view = view;
        }

        public void TakeSelectionSnapshot()
        {
            this.selection = this.view.Selection.SelectedSpans.FirstOrDefault();

            // Only apply the selection if something is selected and the selection ends at the beginning of the line or at the end of the document
            if (this.selection.Length > 0)
            {
                var currentLine = this.view.GetTextViewLineContainingBufferPosition(this.selection.End);

                if (this.selection.End.Position == this.view.TextSnapshot.Length || currentLine.Start.Position == this.selection.End.Position)
                {
                    this.shouldSelectionBeApplied = true;
                    this.isSelectionReversed = this.view.Selection.IsReversed;
                }
            }
        }

        public void ApplySelection(int offset)
        {
            if (this.shouldSelectionBeApplied)
            {
                var updatedSelectionSnapshot = new SnapshotSpan(this.view.TextSnapshot, this.selection.Start + offset, this.selection.Length);
                this.view.Selection.Select(updatedSelectionSnapshot, this.isSelectionReversed);
                var caretPosition = this.isSelectionReversed ? updatedSelectionSnapshot.Start : updatedSelectionSnapshot.End;
                this.view.Caret.MoveTo(new SnapshotPoint(this.view.TextSnapshot, caretPosition));
            }
        }

        public SnapshotPoint GetLineStartPoint()
        {
            var startPosition = this.view.Selection.Start.Position;
            var startLine = this.view.GetTextViewLineContainingBufferPosition(startPosition);
            return startLine.Start;
        }

        public SnapshotPoint GetLineEndIncludingLineBreak()
        {
            var endLine = this.GetEndLine();
            return this.view.Selection.IsEmpty || this.view.Selection.End.Position != endLine.Start ? endLine.EndIncludingLineBreak : new SnapshotPoint(this.view.TextSnapshot, endLine.Start);
        }

        public SnapshotPoint GetLineEndPoint()
        {
            var endLine = this.GetEndLine();
            return this.view.Selection.IsEmpty || this.view.Selection.End.Position != endLine.Start ? endLine.End : new SnapshotPoint(this.view.TextSnapshot, endLine.Start - 1);
        }

        private IWpfTextViewLine GetEndLine()
        {
            var endPosition = this.view.Selection.End.Position;
            var endLine = this.view.GetTextViewLineContainingBufferPosition(endPosition);
            return endLine;
        }
    }
}
