//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) Antoine Aubry and contributors

//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

using System;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.RepresentationModel
{
    public class LibYamlEventStream2 : IYamlLoader
    {
        private readonly TextWriter textWriter;

        public LibYamlEventStream2(TextWriter textWriter)
        {
            this.textWriter = textWriter;
        }

        public void OnAlias(AnchorName value, Mark start, Mark end)
        {
            textWriter.Write("=ALI *");
            textWriter.Write(value);
            textWriter.WriteLine();
        }

        public void OnComment(string value, bool isInline, Mark start, Mark end)
        {
        }

        public void OnDocumentEnd(bool isImplicit, Mark start, Mark end)
        {
            textWriter.Write("-DOC");
            if (!isImplicit) textWriter.Write(" ...");
            textWriter.WriteLine();
        }

        public void OnDocumentStart(Core.Tokens.VersionDirective? version, TagDirectiveCollection? tags, bool isImplicit, Mark start, Mark end)
        {
            textWriter.Write("+DOC");
            if (!isImplicit) textWriter.Write(" ---");
            textWriter.WriteLine();
        }

        public void OnMappingEnd(Mark start, Mark end)
        {
            textWriter.Write("-MAP");
            textWriter.WriteLine();
        }

        public void OnMappingStart(AnchorName anchor, TagName tag, MappingStyle style, Mark start, Mark end)
        {
            textWriter.Write("+MAP");
            WriteAnchorAndTag(textWriter, anchor, tag);
            textWriter.WriteLine();
        }

        public void OnScalar(AnchorName anchor, TagName tag, string value, ScalarStyle style, Mark start, Mark end)
        {
            textWriter.Write("=VAL");
            WriteAnchorAndTag(textWriter, anchor, tag, style);

            switch (style)
            {
                case ScalarStyle.DoubleQuoted: textWriter.Write(" \""); break;
                case ScalarStyle.SingleQuoted: textWriter.Write(" '"); break;
                case ScalarStyle.Folded: textWriter.Write(" >"); break;
                case ScalarStyle.Literal: textWriter.Write(" |"); break;
                default: textWriter.Write(" :"); break;
            }

            foreach (char character in value)
            {
                switch (character)
                {
                    case '\b': textWriter.Write("\\b"); break;
                    case '\t': textWriter.Write("\\t"); break;
                    case '\n': textWriter.Write("\\n"); break;
                    case '\r': textWriter.Write("\\r"); break;
                    case '\\': textWriter.Write("\\\\"); break;
                    default: textWriter.Write(character); break;
                }
            }
            textWriter.WriteLine();
        }

        public void OnSequenceEnd(Mark start, Mark end)
        {
            textWriter.Write("-SEQ");
            textWriter.WriteLine();
        }

        public void OnSequenceStart(AnchorName anchor, TagName tag, SequenceStyle style, Mark start, Mark end)
        {
            textWriter.Write("+SEQ");
            WriteAnchorAndTag(textWriter, anchor, tag);
            textWriter.WriteLine();
        }

        public void OnStreamEnd(Mark start, Mark end)
        {
            textWriter.Write("-STR");
            textWriter.WriteLine();
        }

        public void OnStreamStart(Mark start, Mark end)
        {
            textWriter.Write("+STR");
            textWriter.WriteLine();
        }

        private void WriteAnchorAndTag(TextWriter textWriter, AnchorName anchor, TagName tag, ScalarStyle scalarStyle = ScalarStyle.Any)
        {
            if (!anchor.IsEmpty)
            {
                textWriter.Write(" &");
                textWriter.Write(anchor);
            }

            var tagIsExplicit = !tag.IsNonSpecific;
            if (!tagIsExplicit && scalarStyle == ScalarStyle.Plain)
            {
                tagIsExplicit = !tag.IsEmpty;
            }

            if (tagIsExplicit)
            {
                textWriter.Write(" <");
                textWriter.Write(tag.Value);
                textWriter.Write(">");
            }
        }
    }

    /// <summary>
    /// Represents a LibYAML event stream.
    /// </summary>
    public class LibYamlEventStream
    {
        private readonly IParser parser;

        /// <summary>
        /// Initializes a new instance of the <see cref="LibYamlEventStream"/> class
        /// from the specified <see cref="IParser"/>.
        /// </summary>
        public LibYamlEventStream(IParser iParser)
        {
            parser = iParser ?? throw new ArgumentNullException(nameof(iParser));
        }

        public void WriteTo(TextWriter textWriter)
        {
            while (parser.MoveNext())
            {
                switch (parser.Current)
                {
                    case AnchorAlias anchorAlias:
                        textWriter.Write("=ALI *");
                        textWriter.Write(anchorAlias.Value);
                        break;
                    case DocumentEnd documentEnd:
                        textWriter.Write("-DOC");
                        if (!documentEnd.IsImplicit) textWriter.Write(" ...");
                        break;
                    case DocumentStart documentStart:
                        textWriter.Write("+DOC");
                        if (!documentStart.IsImplicit) textWriter.Write(" ---");
                        break;
                    case MappingEnd _:
                        textWriter.Write("-MAP");
                        break;
                    case MappingStart mappingStart:
                        textWriter.Write("+MAP");
                        WriteAnchorAndTag(textWriter, mappingStart);
                        break;
                    case Scalar scalar:
                        textWriter.Write("=VAL");
                        WriteAnchorAndTag(textWriter, scalar);

                        switch (scalar.Style)
                        {
                            case ScalarStyle.DoubleQuoted: textWriter.Write(" \""); break;
                            case ScalarStyle.SingleQuoted: textWriter.Write(" '"); break;
                            case ScalarStyle.Folded: textWriter.Write(" >"); break;
                            case ScalarStyle.Literal: textWriter.Write(" |"); break;
                            default: textWriter.Write(" :"); break;
                        }

                        foreach (char character in scalar.Value)
                        {
                            switch (character)
                            {
                                case '\b': textWriter.Write("\\b"); break;
                                case '\t': textWriter.Write("\\t"); break;
                                case '\n': textWriter.Write("\\n"); break;
                                case '\r': textWriter.Write("\\r"); break;
                                case '\\': textWriter.Write("\\\\"); break;
                                default: textWriter.Write(character); break;
                            }
                        }
                        break;
                    case SequenceEnd _:
                        textWriter.Write("-SEQ");
                        break;
                    case SequenceStart sequenceStart:
                        textWriter.Write("+SEQ");
                        WriteAnchorAndTag(textWriter, sequenceStart);
                        break;
                    case StreamEnd _:
                        textWriter.Write("-STR");
                        break;
                    case StreamStart _:
                        textWriter.Write("+STR");
                        break;
                }
                textWriter.WriteLine();
            }
        }

        private void WriteAnchorAndTag(TextWriter textWriter, NodeEvent nodeEvent)
        {
            if (!nodeEvent.Anchor.IsEmpty)
            {
                textWriter.Write(" &");
                textWriter.Write(nodeEvent.Anchor);
            }

            var tagIsExplicit = !nodeEvent.Tag.IsNonSpecific;
            if (!tagIsExplicit && nodeEvent is Scalar scalar && scalar.Style == ScalarStyle.Plain)
            {
                tagIsExplicit = !scalar.Tag.IsEmpty;
            }

            if (tagIsExplicit)
            {
                textWriter.Write(" <");
                textWriter.Write(nodeEvent.Tag.Value);
                textWriter.Write(">");
            }
        }
    }
}
