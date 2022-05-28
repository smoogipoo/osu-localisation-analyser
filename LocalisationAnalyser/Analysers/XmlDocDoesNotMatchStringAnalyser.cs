// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LocalisationAnalyser.Analysers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class XmlDocDoesNotMatchStringAnalyser : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DiagnosticRules.XMLDOC_DOES_NOT_MATCH_STRING);

        public override void Initialize(AnalysisContext context)
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            // context.RegisterSyntaxNodeAction(analyseString, SyntaxKind.StringLiteralExpression, SyntaxKind.InterpolatedStringExpression);
        }
    }
}
