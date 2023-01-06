using System.Collections.Immutable;
using System.Linq;

namespace Microsoft.CodeAnalysis.Simplification
{
    /// <summary>
    /// An annotation that holds onto information about a type or namespace symbol.
    /// </summary>
    internal static class SymbolAnnotation
    {
        private const string Kind = "SymbolId";

        public static SyntaxAnnotation Create(ISymbol symbol)
            => Create(DocumentationCommentId.CreateReferenceId(symbol));

        public static SyntaxAnnotation Create(string referenceId)
            => new(Kind, referenceId);
    }
}