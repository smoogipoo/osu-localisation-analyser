// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using LocalisationAnalyser.Analysers;
using Xunit;

namespace LocalisationAnalyser.Tests.Analysers
{
    public class StringDoesNotMatchXmlDocAnalyserTests : AbstractAnalyserTests<StringDoesNotMatchXmlDocAnalyser>
    {
        [Theory]
        [InlineData("StringDoesNotMatchXmlDoc")]
        public Task RunTest(string name) => Check(name);
    }
}
