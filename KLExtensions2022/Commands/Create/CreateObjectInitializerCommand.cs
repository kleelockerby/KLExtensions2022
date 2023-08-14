using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
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

namespace KLExtensions2022
{
    [Command(PackageGuids.guidPackageCreateContextCmdSetString, PackageIds.CreateObjectInitializerCommandId)]
    internal sealed class CreateObjectInitializerCommand : BaseCommand<CreateObjectInitializerCommand>
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

                IdentifierNameSyntax objectType = (await document.GetSyntaxRootAsync()).FindToken(caretPosition).Parent.AncestorsAndSelf().OfType<IdentifierNameSyntax>().FirstOrDefault();

                if (objectType != null)
                {
                    SemanticModel semanticModel = document.GetSemanticModelAsync().Result;
                    ITypeSymbol typeInfo = semanticModel.GetSymbolInfo(objectType).Symbol as ITypeSymbol;

                    List<Member> members = GetMembersForInitialization(typeInfo);
                    string initializer = CreateInitializer(members);
                    
                    TextSelection ts = DTE.ActiveDocument.Selection as EnvDTE.TextSelection;
                    ts.Text = System.Environment.NewLine + initializer;
                }
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

        private string CreateInitializer(List<Member> members)
        {
            string result = "{" + System.Environment.NewLine;
            foreach (Member m in members)
            {
                result += m.name + " = " + TypeDefaultValue(m.type) + "," + System.Environment.NewLine;
            }
            result += "};" + System.Environment.NewLine;
            return result;
        }

        List<Member> GetMembersForInitialization(ITypeSymbol type)
        {
            List<Member> result = new List<Member>();
            foreach (ISymbol m in type.GetMembers())
            {
                if (m.DeclaredAccessibility == Accessibility.Public)
                {
                    if (m.Kind == SymbolKind.Property)
                    {
                        IPropertySymbol property = m as Microsoft.CodeAnalysis.IPropertySymbol;
                        if (!property.IsReadOnly)
                        {
                            result.Add(new Member(m.Name, property.Type));
                        }
                    }
                    if (m.Kind == SymbolKind.Field)
                    {
                        IFieldSymbol field = m as IFieldSymbol;
                        if (!field.IsReadOnly && !field.IsConst)
                        {
                            result.Add(new Member(m.Name, field.Type));
                        }
                    }
                }
            }
            return result;
        }

        private static string TypeDefaultValue(ITypeSymbol type)
        {
            switch (type.SpecialType)
            {
                case SpecialType.System_SByte:
                case SpecialType.System_Int16:
                case SpecialType.System_Int32:
                case SpecialType.System_Int64:
                case SpecialType.System_Byte:
                case SpecialType.System_UInt16:
                case SpecialType.System_UInt32:
                case SpecialType.System_UInt64:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                    return "0";
                case SpecialType.System_String:
                    return "\"\"";
                case SpecialType.System_Boolean:
                    return "false";
            }
            switch (type.TypeKind)
            {
                case TypeKind.Class:
                    return "null";
            }
            return "";
        }

    }
}
