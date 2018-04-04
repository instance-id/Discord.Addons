﻿using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Discord.Addons.MpGame.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BufferStratagySetterAnalyzer : DiagnosticAnalyzer
    {
        private const string DiagnosticId = "MPG0001";
        private const string Title = "Restrict BufferStrategy setting";
        private const string MessageFormat = "Do not set the BufferStrategy outside of the constructor.";
        private const string Description = "";
        private const string Category = "API Usage";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleAssignmentExpression);
        }

        private void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is AssignmentExpressionSyntax assignment))
                return; //technically never false, but let's not make assumptions

            var ctor = assignment.FirstAncestorOrSelf<ConstructorDeclarationSyntax>();
            if (ctor != null)
                return; //we inside a ctor, this analyzer doesn't care anymore

            var classDecl = ctor.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDecl == null)
                return; //we're entirely irrelevant to structs

            var classSymbolInfo = context.SemanticModel.GetSymbolInfo(classDecl);
            if (classSymbolInfo.Symbol is ITypeSymbol classSymbol && !classSymbol.DerivesFromPile())
                return; //trigger was outside of a Pile-derived class, bail out

            var symbolInfo = context.SemanticModel.GetSymbolInfo(assignment.Left);
            if (!(symbolInfo.Symbol is IPropertySymbol symbol))
                return; //assignment wasn't a property, get outta here

            if (symbol.Name == "BufferStrategy")
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
        }
    }
}
