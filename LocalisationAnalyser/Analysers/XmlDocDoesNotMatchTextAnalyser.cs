// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Immutable;
using System.Linq;
using LocalisationAnalyser.Localisation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LocalisationAnalyser.Analysers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class XmlDocDoesNotMatchTextAnalyser : AbstractMemberAnalyser
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DiagnosticRules.XMLDOC_DOES_NOT_MATCH_TEXT);

        protected override void AnalyseProperty(SyntaxTreeAnalysisContext context, LocalisationMember member, PropertyDeclarationSyntax property)
        {
            base.AnalyseProperty(context, member, property);

            if (member.EnglishText == member.XmlDoc)
                return;

            var xmlDocTrivia = property.Modifiers.First().LeadingTrivia.FirstOrDefault(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia));

            context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.XMLDOC_DOES_NOT_MATCH_TEXT, xmlDocTrivia.GetLocation(), property));
        }

        protected override void AnalyseMethod(SyntaxTreeAnalysisContext context, LocalisationMember member, MethodDeclarationSyntax method)
        {
            base.AnalyseMethod(context, member, method);

            if (member.EnglishText == member.XmlDoc)
                return;

            var xmlDocTrivia = method.Modifiers.First().LeadingTrivia.FirstOrDefault(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia));

            context.ReportDiagnostic(Diagnostic.Create(DiagnosticRules.XMLDOC_DOES_NOT_MATCH_TEXT, xmlDocTrivia.GetLocation(), method));
        }
    }
}
