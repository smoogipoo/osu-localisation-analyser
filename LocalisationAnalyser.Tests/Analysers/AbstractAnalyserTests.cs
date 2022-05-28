// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LocalisationAnalyser.Tests.Analysers
{
    public abstract class AbstractAnalyserTests<TAnalyzer>
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        private const string resources_namespace = "LocalisationAnalyser.Tests.Resources";

        public abstract Task RunTest(string name);

        protected async Task Check(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();

            var sourceFiles = new List<string>
            {
                $"{resources_namespace}.LocalisableString.txt",
                $"{resources_namespace}.TranslatableString.txt",
            };

            foreach (var f in resourceNames.Where(n => n.StartsWith($"{resources_namespace}.Analysers.{name}")))
                sourceFiles.Add(f);

            await Verifiers.CSharpAnalyzerVerifier<TAnalyzer>.VerifyAnalyzerAsync(sourceFiles.Select(f => readResourceStream(assembly, f)).ToArray());
        }

        private string readResourceStream(Assembly asm, string resource)
        {
            using (var stream = asm.GetManifestResourceStream(resource)!)
            using (var sr = new StreamReader(stream))
                return sr.ReadToEnd();
        }
    }
}
