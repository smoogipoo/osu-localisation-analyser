// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;

namespace LocalisationAnalyser.Localisation
{
    /// <summary>
    /// A localisation file.
    /// </summary>
    public partial class LocalisationFile : IEquatable<LocalisationFile>
    {
        /// <summary>
        /// The namespace of the localisation class in this file.
        /// </summary>
        public readonly string Namespace;

        /// <summary>
        /// The name of the localisation class stored in this file.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The localisation prefix of the localisation class in this file.
        /// </summary>
        public readonly string Prefix;

        /// <summary>
        /// All localisation members (properties and methodS) of the localisation class in this file.
        /// </summary>
        public readonly ImmutableArray<LocalisationMember> Members;

        public LocalisationFile(string @namespace, string name, string prefix, params LocalisationMember[] members)
        {
            Namespace = @namespace;
            Name = name;
            Prefix = prefix;
            Members = members.ToImmutableArray();
        }

        /// <summary>
        /// Creates a new <see cref="LocalisationFile"/> with a new set of members.
        /// </summary>
        /// <param name="members">The new localisation members.</param>
        /// <returns>The resultant <see cref="LocalisationFile"/>.</returns>
        public LocalisationFile WithMembers(params LocalisationMember[] members)
            => new LocalisationFile(Namespace, Name, Prefix, members);

        /// <summary>
        /// Writes this <see cref="LocalisationFile"/> to a stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="workspace">The workspace to format with.</param>
        /// <param name="options">The analyser options to apply to the document.</param>
        public async Task WriteAsync(Stream stream, Workspace workspace, AnalyzerConfigOptions? options = null)
        {
            using (var sw = new StreamWriter(stream))
                await sw.WriteAsync(Formatter.Format(SyntaxGenerators.GenerateClassSyntax(workspace, this, options), workspace).ToFullString());
        }

        /// <summary>
        /// Reads a <see cref="LocalisationFile"/> from a stream.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <returns>The <see cref="LocalisationFile"/>.</returns>
        /// <exception cref="MalformedLocalisationException">If the file doesn't contain a valid <see cref="LocalisationFile"/>.</exception>
        public static async Task<LocalisationFile> ReadAsync(Stream stream)
        {
            var result = await TryReadAsync(stream);

            if (!result.success)
                throw result.failureReason!;

            return result.file;
        }

        /// <summary>
        /// Attempts to read a <see cref="LocalisationFile"/> from a stream.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <returns>A tuple describing whether the <see cref="LocalisationFile"/> was successfully read, the resultant <see cref="LocalisationFile"/>, and an error
        /// in the form of an <see cref="Exception"/> if the read failed.</returns>
        public static async Task<(bool success, LocalisationFile file, Exception? failureReason)> TryReadAsync(Stream stream)
        {
            bool result = false;
            LocalisationFile? file = null;
            Exception? failureReason = null;

            try
            {
                using (var sr = new StreamReader(stream))
                {
                    var syntaxTree = CSharpSyntaxTree.ParseText(await sr.ReadToEndAsync());
                    var syntaxRoot = await syntaxTree.GetRootAsync();

                    var walker = new Walker();
                    walker.Visit(syntaxRoot);

                    if (string.IsNullOrEmpty(walker.Namespace))
                        failureReason = new MalformedLocalisationException("The localisation file contains no namespace.");
                    else if (string.IsNullOrEmpty(walker.Name))
                        failureReason = new MalformedLocalisationException("The localisation file contains no class.");
                    else if (string.IsNullOrEmpty(walker.Prefix))
                        failureReason = new MalformedLocalisationException("The localisation file contains no prefix identifier");
                    else
                    {
                        file = new LocalisationFile(walker.Namespace!, walker.Name!, walker.Prefix!, walker.Members.ToArray());
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                failureReason = ex;
            }

            file ??= new LocalisationFile(string.Empty, string.Empty, string.Empty, Array.Empty<LocalisationMember>());
            return (result, file, failureReason);
        }

        /// <summary>
        /// Reads a <see cref="LocalisationFile"/> from a stream.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <returns>The <see cref="LocalisationFile"/>.</returns>
        /// <exception cref="MalformedLocalisationException">If the file doesn't contain a valid <see cref="LocalisationFile"/>.</exception>
        public static LocalisationFile Read(Stream stream)
        {
            if (!TryRead(stream, out var file, out var ex))
                throw ex!;

            return file;
        }

        /// <summary>
        /// Attempts to read a <see cref="LocalisationFile"/> from a stream.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <param name="file">The resultant <see cref="LocalisationFile"/>.</param>
        /// <param name="failureReason">The reason why the <see cref="LocalisationFile"/> failed to be read.</param>
        /// <returns>Whether the <see cref="LocalisationFile"/> was successfully read.</returns>
        public static bool TryRead(Stream stream, out LocalisationFile file, out Exception? failureReason)
        {
            try
            {
                using (var sr = new StreamReader(stream))
                {
                    var syntaxTree = CSharpSyntaxTree.ParseText(sr.ReadToEnd());
                    var syntaxRoot = syntaxTree.GetRoot();

                    var walker = new Walker();
                    walker.Visit(syntaxRoot);

                    if (string.IsNullOrEmpty(walker.Namespace))
                        failureReason = new MalformedLocalisationException("The localisation file contains no namespace.");
                    else if (string.IsNullOrEmpty(walker.Name))
                        failureReason = new MalformedLocalisationException("The localisation file contains no class.");
                    else if (string.IsNullOrEmpty(walker.Prefix))
                        failureReason = new MalformedLocalisationException("The localisation file contains no prefix identifier");
                    else
                    {
                        file = new LocalisationFile(walker.Namespace!, walker.Name!, walker.Prefix!, walker.Members.ToArray());
                        failureReason = null;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                failureReason = ex;
            }

            file = new LocalisationFile(string.Empty, string.Empty, string.Empty, Array.Empty<LocalisationMember>());
            return false;
        }

        public bool Equals(LocalisationFile? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Namespace == other.Namespace && Name == other.Name && Prefix == other.Prefix && Members.Equals(other.Members);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Equals((LocalisationFile)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Namespace.GetHashCode();
                hashCode = (hashCode * 397) ^ Name.GetHashCode();
                hashCode = (hashCode * 397) ^ Prefix.GetHashCode();
                hashCode = (hashCode * 397) ^ Members.GetHashCode();
                return hashCode;
            }
        }
    }
}
