// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LocalisationAnalyser.Analysers
{
    /// <summary>
    /// Discovers all non-verbatim strings (literal and interpolated) and reports <see cref="DiagnosticRules.STRING_CAN_BE_LOCALISED"/>.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class StringCanBeLocalisedAnalyser : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DiagnosticRules.STRING_CAN_BE_LOCALISED);

        public override void Initialize(AnalysisContext context)
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(analyseString, SyntaxKind.StringLiteralExpression, SyntaxKind.InterpolatedStringExpression);
        }

        private void analyseString(SyntaxNodeAnalysisContext context)
        {
            switch (context.Node)
            {
                case LiteralExpressionSyntax literal:
                    if (literal.Token.IsVerbatimStringLiteral())
                        break;

                    if (literal.Token.ValueText.Where(char.IsLetter).Any())
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.STRING_CAN_BE_LOCALISED, context.Node.GetLocation(), context.Node));
                    break;

                case InterpolatedStringExpressionSyntax interpolated:
                    if (interpolated.StringStartToken.Kind() == SyntaxKind.InterpolatedVerbatimStringStartToken)
                        break;

                    if (interpolated.Contents.Any(c => c is InterpolatedStringTextSyntax text && text.TextToken.ValueText.Where(char.IsLetter).Any()))
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.STRING_CAN_BE_LOCALISED, context.Node.GetLocation(), context.Node));
                    break;
            }
        }
    }
}
