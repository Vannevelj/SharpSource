using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using SharpSource.Utilities;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SharpSource.Diagnostics;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class LoggerMessageAttributeCodeFix : CodeFixProvider
{
    private static readonly Regex NonAlphaRegex = new(@"[^a-zA-Z\s]", RegexOptions.Compiled);
    private static readonly Regex PlaceholderRegex = new(@"\{([^}:]+)(?::[^}]*)?\}", RegexOptions.Compiled);

    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(LoggerMessageAttributeAnalyzer.Rule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;


    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var invocation = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault();
        if (invocation is null)
        {
            return;
        }

        // Get the containing type - needed for adding the partial method
        var containingType = invocation.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
        if (containingType is null)
        {
            return;
        }

        diagnostic.Properties.TryGetValue("logLevel", out var logLevel);
        diagnostic.Properties.TryGetValue("message", out var message);

        // Only offer code fix if we have enough info
        if (logLevel is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use LoggerMessage attribute",
                _ => UseLoggerMessageAttribute(context.Document, root, invocation, containingType, logLevel, message),
                LoggerMessageAttributeAnalyzer.Rule.Id),
            diagnostic);
    }

    private static Task<Document> UseLoggerMessageAttribute(Document document, SyntaxNode root, InvocationExpressionSyntax invocation, TypeDeclarationSyntax containingType, string logLevel, string? message)
    {
        // Generate a method name based on the log message or log level
        var methodName = GenerateMethodName(logLevel, message);

        // Extract arguments that should become method parameters
        var (parameters, arguments, templateArgs) = ExtractParametersAndArguments(invocation, message);

        // Create the new partial method
        var newMethod = CreateLoggerMessageMethod(methodName, logLevel, message, parameters);

        // Create the new invocation
        var newInvocation = CreateInvocation(invocation, methodName, arguments);

        // First, replace the invocation within the containing type
        var newContainingType = containingType.ReplaceNode(invocation, newInvocation);

        // Add partial modifier if not present
        if (!newContainingType.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            newContainingType = newContainingType.AddModifiers(Token(SyntaxKind.PartialKeyword));
        }

        // Add the new method
        newContainingType = newContainingType.AddMembers(newMethod);

        // Replace the containing type in the root
        var newRoot = root.ReplaceNode(containingType, newContainingType);

        var newDocument = document.WithSyntaxRoot(newRoot);
        return Task.FromResult(newDocument);
    }


    private static string GenerateMethodName(string logLevel, string? message)
    {
        if (message is not null)
        {
            // Extract meaningful words from the message
            var words = NonAlphaRegex.Replace(message, " ")
                .Split([' '], System.StringSplitOptions.RemoveEmptyEntries)
                .Take(4);

            if (words.Any())
            {
                var methodName = string.Join("", words.Select(w => char.ToUpperInvariant(w[0]) + w.Substring(1).ToLowerInvariant()));
                return "Log" + methodName;
            }
        }

        return "Log" + logLevel;
    }

    private static (ParameterListSyntax parameters, ArgumentListSyntax arguments, string[] templateArgs) ExtractParametersAndArguments(InvocationExpressionSyntax invocation, string? message)
    {
        var parameters = new System.Collections.Generic.List<ParameterSyntax>();
        var arguments = new System.Collections.Generic.List<ArgumentSyntax>();
        var templateArgs = Array.Empty<string>();

        // First, add the logger parameter
        ExpressionSyntax? loggerExpression = null;

        // Find the logger expression from the invocation
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            loggerExpression = memberAccess.Expression;
        }

        // Extract template placeholders from the message
        if (message is not null)
        {
            var placeholders = PlaceholderRegex.Matches(message);
            templateArgs = placeholders.Cast<Match>().Select(m => m.Groups[1].Value).ToArray();
        }

        // Find format arguments from the invocation
        var invocationArgs = invocation.ArgumentList.Arguments.ToList();

        // Skip logger (for extension methods), log level (for Log method), and message arguments
        var formatArgsStart = invocationArgs.FindIndex(a =>
        {
            var paramName = a.NameColon?.Name.Identifier.Text;
            return paramName == "args" || ( a.Expression is not LiteralExpressionSyntax && !IsLogLevelExpression(a.Expression) && paramName != "message" && paramName != "eventId" && paramName != "exception" );
        });

        if (formatArgsStart >= 0)
        {
            for (var i = formatArgsStart; i < invocationArgs.Count; i++)
            {
                var arg = invocationArgs[i];
                var paramName = i - formatArgsStart < templateArgs.Length
                    ? ToCamelCase(templateArgs[i - formatArgsStart])
                    : $"arg{i - formatArgsStart}";

                // Create parameter
                var paramType = ParseTypeName("object"); // Default to object, ideally we'd infer the type
                parameters.Add(Parameter(Identifier(paramName)).WithType(paramType));

                // Create argument
                arguments.Add(Argument(arg.Expression));
            }
        }

        var parameterList = ParameterList(SeparatedList(parameters));
        var argumentList = ArgumentList(SeparatedList(arguments));

        return (parameterList, argumentList, templateArgs);
    }

    private static bool IsLogLevelExpression(ExpressionSyntax expression) =>
        expression is MemberAccessExpressionSyntax memberAccess &&
        memberAccess.Expression.ToString().Contains("LogLevel");

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }

    private static MethodDeclarationSyntax CreateLoggerMessageMethod(string methodName, string logLevel, string? message, ParameterListSyntax parameters)
    {
        // Build the attribute
        var attributeArguments = new List<AttributeArgumentSyntax>
        {
            AttributeArgument(
                NameEquals("Level"),
                null,
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("LogLevel"),
                    IdentifierName(logLevel)))
        };

        // Generate message template - either from the original message or from parameter names
        var messageTemplate = message;
        if (messageTemplate is null && parameters.Parameters.Count > 0)
        {
            // Generate a message template from the parameter names
            var placeholders = parameters.Parameters.Select(p => $"{{{p.Identifier.Text}}}");
            messageTemplate = string.Join(" ", placeholders);
        }

        if (messageTemplate is not null)
        {
            attributeArguments.Add(
                AttributeArgument(
                    NameEquals("Message"),
                    null,
                    LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(messageTemplate))));
        }

        var attribute = Attribute(IdentifierName("LoggerMessage"))
            .WithArgumentList(AttributeArgumentList(SeparatedList(attributeArguments)));

        var attributeList = AttributeList(SingletonSeparatedList(attribute));

        // Create the method
        var method = MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), methodName)
            .WithAttributeLists(SingletonList(attributeList))
            .AddModifiers(
                Token(SyntaxKind.PrivateKeyword),
                Token(SyntaxKind.PartialKeyword))
            .WithParameterList(parameters)
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
            .WithAdditionalAnnotations(Formatter.Annotation, Simplifier.AddImportsAnnotation);

        return method;
    }

    private static InvocationExpressionSyntax CreateInvocation(InvocationExpressionSyntax originalInvocation, string methodName, ArgumentListSyntax arguments) =>
        InvocationExpression(IdentifierName(methodName))
            .WithArgumentList(arguments)
            .WithAdditionalAnnotations(Formatter.Annotation);
}