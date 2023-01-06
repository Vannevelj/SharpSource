namespace Microsoft.CodeAnalysis.Simplification
{
    /// <summary>
    /// An annotation that holds onto information about a type or namespace symbol.
    /// Taken from https://github.com/dotnet/roslyn/blob/main/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/Core/Simplification/SymbolAnnotation.cs
    /// </summary>
    internal static class SymbolAnnotation
    {
        private const string Kind = "SymbolId";

        public static SyntaxAnnotation Create(ISymbol symbol)
            => Create(DocumentationCommentId.CreateReferenceId(symbol));

        public static SyntaxAnnotation Create(string fullyQualifiedTypeName)
            => new(Kind, fullyQualifiedTypeName);
    }
}