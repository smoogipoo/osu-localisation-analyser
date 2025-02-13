// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LocalisationAnalyser.Abstractions.IO.Default
{
    internal class DefaultFile : IFile
    {
        public Task<string> ReadAllTextAsync(string fullname, CancellationToken cancellationToken) => File.ReadAllTextAsync(fullname, cancellationToken);
    }
}
