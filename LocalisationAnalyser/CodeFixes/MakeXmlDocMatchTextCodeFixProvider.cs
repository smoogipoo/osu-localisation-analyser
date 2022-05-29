// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace LocalisationAnalyser.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakeXmlDocMatchTextCodeFixProvider)), Shared]
    [ExtensionOrder(After = nameof(MakeTextMatchXmlDocCodeFixProvider))]
    public class MakeXmlDocMatchTextCodeFixProvider : AbstractXmlDocCodeFixProvider
    {
        protected override bool PreferXmlDoc => false;
    }
}
