// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LocalisationAnalyser.Localisation;
using LocalisationAnalyser.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace LocalisationAnalyser.CodeFixes
{
    public abstract class AbstractXmlDocCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticRules.XMLDOC_DOES_NOT_MATCH_TEXT.Id);

        protected abstract bool PreferXmlDoc { get; }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var member = (MemberDeclarationSyntax)root!.FindToken(diagnosticSpan.Start).Parent;

            context.RegisterCodeFix(
                new LocaliseStringCodeAction(
                    "Update XMLDoc to match translation text",
                    (preview, cancellationToken) => updateDefinition(context.Document, member, preview, cancellationToken),
                    @"update-xmldoc"),
                diagnostic);
        }

        private async Task<Solution> updateDefinition(Document document, MemberDeclarationSyntax member, bool preview, CancellationToken cancellationToken)
        {
            LocalisationFile currentFile;

            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms, Encoding.UTF8, 1024, true))
                    await sw.WriteAsync((await document.GetTextAsync(cancellationToken)).ToString());

                ms.Seek(0, SeekOrigin.Begin);

                currentFile = await LocalisationFile.ReadAsync(ms);
            }

            LocalisationFile updatedFile = currentFile.WithMembers(currentFile.Members.Select(m =>
            {
                if (m.Name != ((PropertyDeclarationSyntax)member).Identifier.Text)
                    return m;

                if (PreferXmlDoc)
                    return new LocalisationMember(m.Name, m.Key, SyntaxGenerators.DecodeXmlDoc(m.XmlDoc), m.XmlDoc, m.Parameters.ToArray());

                return new LocalisationMember(m.Name, m.Key, m.EnglishText, m.EnglishText, m.Parameters.ToArray());
            }).ToArray());

            using (var ms = new MemoryStream())
            {
                var options = await document.GetAnalyserOptionsAsync(cancellationToken);
                await updatedFile.WriteAsync(ms, document.Project.Solution.Workspace, options, true);

                return document.WithText(SourceText.From(ms, Encoding.UTF8)).Project.Solution;
            }
        }
    }
}
