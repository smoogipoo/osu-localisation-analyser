// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using LocalisationAnalyser.Localisation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LocalisationAnalyser.Analysers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class XmlDocDoesNotMatchStringAnalyser : DiagnosticAnalyzer
    {
        private readonly ConcurrentDictionary<string, LocalisationFile> validFiles = new ConcurrentDictionary<string, LocalisationFile>();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DiagnosticRules.XMLDOC_DOES_NOT_MATCH_STRING);

        public override void Initialize(AnalysisContext context)
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxTreeAction(analyseSyntaxTree);
            context.RegisterSyntaxNodeAction(analyseProperty, SyntaxKind.PropertyDeclaration);
        }

        private void analyseSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            string path = context.Tree.FilePath;

            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms, Encoding.UTF8, 1024, true))
                    sw.Write(context.Tree.ToString());

                ms.Position = 0;

                if (LocalisationFile.TryRead(ms, out var file, out _))
                    validFiles[path] = file;
            }
        }

        private void analyseProperty(SyntaxNodeAnalysisContext context)
        {
            if (!validFiles.TryGetValue(context.Node.SyntaxTree.FilePath, out var file))
                return;

            string? name = ((PropertyDeclarationSyntax)context.Node).Identifier.Text;
            if (name == null)
                return;

            LocalisationMember member = file.Members.Single(m => m.Name == name && m.Parameters.Length == 0);

            if (member.EnglishText == member.XmlDoc)
                return;

            context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.XMLDOC_DOES_NOT_MATCH_STRING, context.Node.GetLocation(), context.Node));
        }
    }
}
