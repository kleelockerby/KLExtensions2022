using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.LanguageServices;
using KLExtensions2022.Helpers;
using KLExtensions2022.Extensions;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Task = System.Threading.Tasks.Task;
using System.Text.RegularExpressions;
using KLExtensions2022.Commands;
using System.IO.Packaging;
using System.Linq;
using System.Collections.Generic;
using Document = Microsoft.CodeAnalysis.Document;
using CompilationUnitSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax;
using LocalDeclarationStatementSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax;
using VariableDeclaratorSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax;
using IdentifierNameSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace KLExtensions2022
{
    [Command(PackageGuids.guidPackageCreateContextCmdSetString, PackageIds.RemoveVarsCommandId)]
    internal sealed class RemoveVarsCommand : BaseCommand<RemoveVarsCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            try
            {
                await KLExtensions2022Package.JoinTaskFactory.SwitchToMainThreadAsync();              
                IServiceProvider serviceProvideer = Package as System.IServiceProvider;
                IWpfTextView textView = GetTextView();
                SnapshotPoint caretPosition = textView.Caret.Position.BufferPosition;
                Document document = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();
                await GetLocalDeclarationsAsync(document);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private IWpfTextView GetTextView()
        {
            IComponentModel compService = Package.GetService<SComponentModel, IComponentModel>();
            Assumes.Present(compService);

            IVsTextManager textManager = Package.GetService<SVsTextManager, IVsTextManager>();
            IVsTextView textView;
            textManager.GetActiveView(1, null, out textView);
            IVsEditorAdaptersFactoryService editorAdapter = compService.GetService<IVsEditorAdaptersFactoryService>();
            return editorAdapter.GetWpfTextView(textView);
        }

        private async Task GetLocalDeclarationsAsync(Document document)
        {
            SyntaxNode syntaxRoot = document.GetSyntaxRootAsync().Result;
            CompilationUnitSyntax root = (CompilationUnitSyntax)syntaxRoot;
            DocumentEditor editor = DocumentEditor.CreateAsync(document).Result;
            List<LocalDeclarationStatementSyntax> localDeclarations = root?.DescendantNodes().OfType<LocalDeclarationStatementSyntax>().ToList();

            foreach (LocalDeclarationStatementSyntax localNode in localDeclarations)
            {
                foreach (VariableDeclaratorSyntax variableNode in localNode.Declaration.Variables)
                {
                    SyntaxKind varKind = variableNode.Initializer.Value.Kind();
                    if (localNode.Declaration.Type.IsVar)
                    {
                        IdentifierNameSyntax varTypeName = localNode.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();
                        IdentifierNameSyntax strongTypeName = await ReplaceVarWithTypeAsync(document, localNode);
                        editor.ReplaceNode(varTypeName, strongTypeName);
                    }
                }
            }
            Document newDocument = editor.GetChangedDocument();
            string text = newDocument.GetTextAsync().Result.ToString();
            Console.WriteLine(text);
        }

        private async Task<IdentifierNameSyntax> ReplaceVarWithTypeAsync(Document document, LocalDeclarationStatementSyntax varDeclaration)
        {
            SyntaxNode root = document.GetSyntaxRootAsync().Result;
            SemanticModel semanticModel = await document.GetSemanticModelAsync();
            SymbolInfo typeSymbol = semanticModel.GetSymbolInfo(varDeclaration.Declaration.Type);
            IdentifierNameSyntax newIdentifier = SyntaxFactory.IdentifierName(typeSymbol.Symbol.ToDisplayString());
            newIdentifier.NormalizeWhitespace();
            newIdentifier = newIdentifier.WithLeadingTrivia(varDeclaration.GetLeadingTrivia());
            newIdentifier = newIdentifier.WithTrailingTrivia(varDeclaration.GetTrailingTrivia());
            return newIdentifier;
        }
    }
}