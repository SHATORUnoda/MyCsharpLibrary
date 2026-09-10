using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

if (args.Length != 3)
{
    Console.WriteLine(
        "usage: Bundler <input.cs> <submit.cs> <libdir>");
    return;
}

string entry = Path.GetFullPath(args[0]);
string output = Path.GetFullPath(args[1]);
string libDir = Path.GetFullPath(args[2]);

if (!Directory.Exists(libDir))
{
    Console.WriteLine($"lib not found : {libDir}");
    return;
}

LibraryTrimmer trimmer = LibraryTrimmer.Create(
    entry,
    Directory.GetFiles(libDir, "*.cs"));

IReadOnlyList<TrimmedLibrary> libraries = trimmer.Trim();

HashSet<string> usings = new();

foreach (TrimmedLibrary library in libraries)
{
    foreach (var usingDirective in library.Usings)
        usings.Add(usingDirective.ToString());
}

foreach (var usingDirective in trimmer.GetEntryUsings())
    usings.Add(usingDirective.ToString());

StringBuilder body = new();

void AppendCode(string code)
{
    bool inDebugBlock = false;

    foreach (string line in code.Split('\n'))
    {
        string trimmed = line.Trim();

        if (trimmed.StartsWith("//require:"))
            continue;

        if (trimmed == "#if DEBUG")
        {
            inDebugBlock = true;
            continue;
        }

        if (inDebugBlock)
        {
            if (trimmed == "#endif")
                inDebugBlock = false;

            continue;
        }

        body.AppendLine(line);
    }
}

foreach (TrimmedLibrary library in libraries)
{
    body.AppendLine(
        $"// ===== {Path.GetFileNameWithoutExtension(library.Path)} =====");

    foreach (var member in library.Members)
        AppendCode(member.ToFullString());

    body.AppendLine();
}

body.AppendLine("// ===== Main =====");
AppendCode(trimmer.GetEntryCodeWithoutUsings());

StringBuilder submit = new();

foreach (string usingDirective in usings.OrderBy(value => value))
    submit.AppendLine(usingDirective);

submit.AppendLine();
submit.Append(body);
submit.AppendLine();
submit.AppendLine("// Generated using CsharpVersionBundler by SHATORUnoda");
submit.AppendLine("// https://github.com/SHATORUnoda/CsharpVersionBundler/blob/main/Program.cs");

File.WriteAllText(output, submit.ToString());

Console.WriteLine($"generated : {output}");

public sealed class LibraryTrimmer
{
    private readonly CSharpCompilation compilation;
    private readonly HashSet<SyntaxTree> libraryTrees;
    private readonly Dictionary<SyntaxTree, CompilationUnitSyntax> roots;
    private readonly Dictionary<SyntaxTree, string> paths;
    private readonly HashSet<ISymbol> selected =
        new(SymbolEqualityComparer.Default);
    private readonly Queue<ISymbol> pending = new();

    private LibraryTrimmer(
        CSharpCompilation compilation,
        SyntaxTree entryTree,
        IReadOnlyDictionary<SyntaxTree, string> libraryPaths)
    {
        this.compilation = compilation;
        EntryTree = entryTree;
        libraryTrees = libraryPaths.Keys.ToHashSet();
        paths = new(libraryPaths);
        roots = libraryPaths.Keys.ToDictionary(
            tree => tree,
            tree => (CompilationUnitSyntax)tree.GetRoot());
    }

    public SyntaxTree EntryTree { get; }

    public static LibraryTrimmer Create(
        string entryPath,
        IEnumerable<string> libraryPaths)
    {
        CSharpParseOptions options = CSharpParseOptions.Default
            .WithLanguageVersion(LanguageVersion.Preview);

        string fullEntryPath = Path.GetFullPath(entryPath);
        SyntaxTree entryTree = CSharpSyntaxTree.ParseText(
            File.ReadAllText(fullEntryPath),
            options,
            fullEntryPath);

        Dictionary<SyntaxTree, string> trees = new();

        foreach (string libraryPath in libraryPaths.OrderBy(path => path))
        {
            string fullPath = Path.GetFullPath(libraryPath);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(
                File.ReadAllText(fullPath),
                options,
                fullPath);
            trees.Add(tree, fullPath);
        }

        CSharpCompilation compilation = CSharpCompilation.Create(
            "LibraryAnalysis",
            trees.Keys.Prepend(entryTree),
            GetFrameworkReferences(),
            new CSharpCompilationOptions(
                OutputKind.ConsoleApplication,
                checkOverflow: true));

        return new LibraryTrimmer(compilation, entryTree, trees);
    }

    public IReadOnlyList<TrimmedLibrary> Trim()
    {
        AnalyzeEntry();

        while (pending.Count > 0)
            AnalyzeSymbol(pending.Dequeue());

        return paths
            .OrderBy(pair => pair.Value)
            .Select(pair => CreateTrimmedLibrary(pair.Key, pair.Value))
            .Where(library => library.Members.Count > 0)
            .ToArray();
    }

    public IEnumerable<UsingDirectiveSyntax> GetEntryUsings()
    {
        SemanticModel model = compilation.GetSemanticModel(EntryTree);
        CompilationUnitSyntax root =
            (CompilationUnitSyntax)EntryTree.GetRoot();

        return root.Usings.Where(usingDirective =>
            ShouldKeepUsing(usingDirective, model, root, false));
    }

    public string GetEntryCodeWithoutUsings()
    {
        SourceText text = EntryTree.GetText();
        CompilationUnitSyntax root =
            (CompilationUnitSyntax)EntryTree.GetRoot();
        StringBuilder result = new();
        int position = 0;

        foreach (UsingDirectiveSyntax usingDirective in root.Usings)
        {
            result.Append(text.ToString(
                TextSpan.FromBounds(position, usingDirective.FullSpan.Start)));
            position = usingDirective.FullSpan.End;
        }

        result.Append(text.ToString(
            TextSpan.FromBounds(position, text.Length)));

        return result.ToString();
    }

    private static IEnumerable<MetadataReference> GetFrameworkReferences()
    {
        string? assemblies = AppContext.GetData(
            "TRUSTED_PLATFORM_ASSEMBLIES") as string;

        if (string.IsNullOrEmpty(assemblies))
            throw new InvalidOperationException(
                "framework assemblies were not found");

        return assemblies.Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path));
    }

    private void AnalyzeEntry()
    {
        SemanticModel model = compilation.GetSemanticModel(EntryTree);
        CompilationUnitSyntax root =
            (CompilationUnitSyntax)EntryTree.GetRoot();

        foreach (MemberDeclarationSyntax member in root.Members)
            AddReferencedSymbols(member, model);
    }

    private void AnalyzeSymbol(ISymbol symbol)
    {
        if (symbol is INamedTypeSymbol type)
        {
            AnalyzeTypeHeader(type);
            return;
        }

        foreach (SyntaxReference reference in
                 symbol.DeclaringSyntaxReferences)
        {
            SyntaxNode node = reference.GetSyntax();
            AddReferencedSymbols(
                node,
                compilation.GetSemanticModel(node.SyntaxTree));
        }
    }

    private void AnalyzeTypeHeader(INamedTypeSymbol type)
    {
        foreach (SyntaxReference reference in
                 type.DeclaringSyntaxReferences)
        {
            SyntaxNode node = reference.GetSyntax();
            SemanticModel model =
                compilation.GetSemanticModel(node.SyntaxTree);

            switch (node)
            {
                case TypeDeclarationSyntax declaration:
                    foreach (AttributeListSyntax attributes in
                             declaration.AttributeLists)
                    {
                        AddReferencedSymbols(attributes, model);
                    }

                    if (declaration.BaseList is not null)
                        AddReferencedSymbols(declaration.BaseList, model);

                    foreach (TypeParameterConstraintClauseSyntax clause in
                             declaration.ConstraintClauses)
                    {
                        AddReferencedSymbols(clause, model);
                    }

                    if (declaration is RecordDeclarationSyntax record &&
                        record.ParameterList is not null)
                    {
                        AddReferencedSymbols(record.ParameterList, model);
                    }

                    break;

                case EnumDeclarationSyntax declaration:
                    foreach (AttributeListSyntax attributes in
                             declaration.AttributeLists)
                    {
                        AddReferencedSymbols(attributes, model);
                    }

                    if (declaration.BaseList is not null)
                        AddReferencedSymbols(declaration.BaseList, model);

                    break;
            }
        }
    }

    private void AddReferencedSymbols(
        SyntaxNode node,
        SemanticModel model)
    {
        foreach (SyntaxNode descendant in node.DescendantNodesAndSelf())
        {
            SymbolInfo symbolInfo = model.GetSymbolInfo(descendant);
            AddSymbol(symbolInfo.Symbol);

            foreach (ISymbol candidate in symbolInfo.CandidateSymbols)
                AddSymbol(candidate);

            TypeInfo typeInfo = model.GetTypeInfo(descendant);
            AddSymbol(typeInfo.Type);
            AddSymbol(typeInfo.ConvertedType);
        }
    }

    private void AddSymbol(ISymbol? symbol)
    {
        symbol = Normalize(symbol);

        if (symbol is null || !IsLibrarySymbol(symbol) ||
            !selected.Add(symbol))
        {
            return;
        }

        pending.Enqueue(symbol);

        switch (symbol)
        {
            case INamedTypeSymbol type:
                if (type.ContainingType is not null)
                    AddSymbol(type.ContainingType);

                if (type.TypeKind == TypeKind.Interface)
                    AddInterfaceImplementations(type);
                else
                    AddSelectedInterfaceImplementations(type);
                break;

            case IMethodSymbol method:
                AddSymbol(method.ContainingType);
                break;

            case IPropertySymbol property:
                AddSymbol(property.ContainingType);
                break;

            case IFieldSymbol field:
                AddSymbol(field.ContainingType);
                break;

            case IEventSymbol eventSymbol:
                AddSymbol(eventSymbol.ContainingType);
                break;
        }
    }

    private void AddInterfaceImplementations(INamedTypeSymbol type)
    {
        if (type.TypeKind != TypeKind.Interface)
            return;

        foreach (ISymbol member in type.GetMembers())
            AddSymbol(member);

        foreach (INamedTypeSymbol implementation in selected
            .OfType<INamedTypeSymbol>()
            .Where(candidate => candidate.TypeKind != TypeKind.Interface)
            .ToArray())
        {
            AddInterfaceImplementations(implementation, type);
        }
    }

    private void AddSelectedInterfaceImplementations(INamedTypeSymbol type)
    {
        foreach (INamedTypeSymbol interfaceType in selected
            .OfType<INamedTypeSymbol>()
            .Where(candidate => candidate.TypeKind == TypeKind.Interface)
            .ToArray())
        {
            if (type.AllInterfaces.Any(candidate =>
                SymbolEqualityComparer.Default.Equals(
                    candidate.OriginalDefinition,
                    interfaceType.OriginalDefinition)))
            {
                AddInterfaceImplementations(type, interfaceType);
            }
        }
    }

    private void AddInterfaceImplementations(
        INamedTypeSymbol implementation,
        INamedTypeSymbol interfaceType)
    {
        foreach (ISymbol interfaceMember in interfaceType.GetMembers())
            AddSymbol(implementation.FindImplementationForInterfaceMember(
                interfaceMember));
    }

    private static ISymbol? Normalize(ISymbol? symbol)
    {
        if (symbol is null)
            return null;

        if (symbol is IAliasSymbol alias)
            symbol = alias.Target;

        if (symbol is IMethodSymbol method)
        {
            if (method.MethodKind == MethodKind.LocalFunction)
                return Normalize(method.ContainingSymbol);

            if (method.AssociatedSymbol is not null)
                return Normalize(method.AssociatedSymbol);
        }

        if (symbol is IParameterSymbol ||
            symbol is ITypeParameterSymbol)
        {
            return Normalize(symbol.ContainingSymbol);
        }

        if (symbol is ILocalSymbol ||
            symbol is ILabelSymbol ||
            symbol is IRangeVariableSymbol ||
            symbol is INamespaceSymbol)
        {
            return null;
        }

        return symbol.OriginalDefinition;
    }

    private bool IsLibrarySymbol(ISymbol symbol)
    {
        return symbol.Locations.Any(location =>
            location.IsInSource &&
            location.SourceTree is not null &&
            libraryTrees.Contains(location.SourceTree));
    }

    private bool IsSelected(ISymbol? symbol)
    {
        symbol = Normalize(symbol);
        return symbol is not null && selected.Contains(symbol);
    }

    private TrimmedLibrary CreateTrimmedLibrary(
        SyntaxTree tree,
        string path)
    {
        SemanticModel model = compilation.GetSemanticModel(tree);
        List<MemberDeclarationSyntax> members = new();

        foreach (MemberDeclarationSyntax member in roots[tree].Members)
        {
            MemberDeclarationSyntax? trimmed =
                TrimMember(member, model);

            if (trimmed is not null)
                members.Add(trimmed);
        }

        CompilationUnitSyntax trimmedRoot = roots[tree]
            .WithMembers(SyntaxFactory.List(members));
        SyntaxTree trimmedTree = CSharpSyntaxTree.ParseText(
            trimmedRoot.ToFullString(),
            (CSharpParseOptions)tree.Options,
            path);
        trimmedRoot = (CompilationUnitSyntax)trimmedTree.GetRoot();
        SemanticModel trimmedModel = compilation
            .ReplaceSyntaxTree(tree, trimmedTree)
            .GetSemanticModel(trimmedTree);

        IReadOnlyList<UsingDirectiveSyntax> usings = trimmedRoot.Usings
            .Where(usingDirective =>
                ShouldKeepUsing(usingDirective, trimmedModel, trimmedRoot, false))
            .ToArray();

        return new TrimmedLibrary(path, members, usings);
    }

    private MemberDeclarationSyntax? TrimMember(
        MemberDeclarationSyntax member,
        SemanticModel model)
    {
        if (member is TypeDeclarationSyntax type)
        {
            if (!IsSelected(model.GetDeclaredSymbol(type)))
                return null;

            List<MemberDeclarationSyntax> members = new();

            foreach (MemberDeclarationSyntax child in type.Members)
            {
                MemberDeclarationSyntax? trimmed =
                    TrimMember(child, model);

                if (trimmed is not null)
                    members.Add(trimmed);
            }

            BaseListSyntax? baseList = type.BaseList;

            if (baseList is not null)
            {
                SeparatedSyntaxList<BaseTypeSyntax> baseTypes =
                    SyntaxFactory.SeparatedList(
                        baseList.Types.Where(baseType =>
                            IsSelected(model.GetSymbolInfo(baseType.Type).Symbol) ||
                            IsSelected(model.GetTypeInfo(baseType.Type).Type)));

                baseList = baseTypes.Count == 0
                    ? null
                    : baseList.WithTypes(baseTypes);
            }

            return type
                .WithBaseList(baseList)
                .WithMembers(SyntaxFactory.List(members));
        }

        if (member is EnumDeclarationSyntax enumDeclaration)
        {
            if (!IsSelected(model.GetDeclaredSymbol(enumDeclaration)))
                return null;

            return enumDeclaration.WithMembers(
                SyntaxFactory.SeparatedList(
                    enumDeclaration.Members.Where(enumMember =>
                        IsSelected(model.GetDeclaredSymbol(enumMember)))));
        }

        if (member is FieldDeclarationSyntax field)
        {
            SeparatedSyntaxList<VariableDeclaratorSyntax> variables =
                SyntaxFactory.SeparatedList(
                    field.Declaration.Variables.Where(variable =>
                        IsSelected(model.GetDeclaredSymbol(variable))));

            return variables.Count == 0
                ? null
                : field.WithDeclaration(
                    field.Declaration.WithVariables(variables));
        }

        if (member is EventFieldDeclarationSyntax eventField)
        {
            SeparatedSyntaxList<VariableDeclaratorSyntax> variables =
                SyntaxFactory.SeparatedList(
                    eventField.Declaration.Variables.Where(variable =>
                        IsSelected(model.GetDeclaredSymbol(variable))));

            return variables.Count == 0
                ? null
                : eventField.WithDeclaration(
                    eventField.Declaration.WithVariables(variables));
        }

        if (member is ConstructorDeclarationSyntax constructor &&
            constructor.ParameterList.Parameters.Count == 0 &&
            constructor.Body is not null &&
            constructor.Body.Statements.Count == 0 &&
            constructor.Initializer is null)
        {
            return null;
        }

        return IsSelected(model.GetDeclaredSymbol(member))
            ? member
            : null;
    }

    private bool ShouldKeepUsing(
        UsingDirectiveSyntax usingDirective,
        SemanticModel model,
        CompilationUnitSyntax root,
        bool selectedMembersOnly)
    {
        if (usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
        {
            INamedTypeSymbol? type =
                model.GetTypeInfo(usingDirective.Name!).Type as INamedTypeSymbol;

            if (type is null)
                return false;

            INamedTypeSymbol target = type.OriginalDefinition;

            return ReferencedMemberTypes(root, model, selectedMembersOnly).Any(referenced =>
                SymbolEqualityComparer.Default.Equals(
                    referenced.OriginalDefinition, target));
        }

        ISymbol? symbol = model.GetSymbolInfo(usingDirective.Name!).Symbol;

        if (symbol is not INamespaceSymbol targetNamespace)
            return true;

        return ReferencedSymbols(root, model, selectedMembersOnly).Any(referenced =>
            IsWithinNamespace(referenced.ContainingNamespace, targetNamespace));
    }

    private IEnumerable<ISymbol> ReferencedSymbols(
        CompilationUnitSyntax root,
        SemanticModel model,
        bool selectedMembersOnly)
    {
        IEnumerable<SyntaxNode> nodes = root.DescendantNodes()
            .Where(node => !node.AncestorsAndSelf()
                .OfType<UsingDirectiveSyntax>().Any())
            .Where(node => !selectedMembersOnly ||
                IsInSelectedMember(node, model));

        foreach (SyntaxNode node in nodes)
        {
            SymbolInfo symbolInfo = model.GetSymbolInfo(node);

            if (symbolInfo.Symbol is not null)
                yield return symbolInfo.Symbol;

            foreach (ISymbol candidate in symbolInfo.CandidateSymbols)
                yield return candidate;

            TypeInfo typeInfo = model.GetTypeInfo(node);

            if (typeInfo.Type is not null)
                yield return typeInfo.Type;

            if (typeInfo.ConvertedType is not null)
                yield return typeInfo.ConvertedType;
        }
    }

    private IEnumerable<INamedTypeSymbol> ReferencedMemberTypes(
        CompilationUnitSyntax root,
        SemanticModel model,
        bool selectedMembersOnly)
    {
        IEnumerable<SyntaxNode> nodes = root.DescendantNodes()
            .Where(node => !node.AncestorsAndSelf()
                .OfType<UsingDirectiveSyntax>().Any())
            .Where(node => !selectedMembersOnly ||
                IsInSelectedMember(node, model));

        foreach (SyntaxNode node in nodes)
        {
            SymbolInfo symbolInfo = model.GetSymbolInfo(node);

            if (ContainingType(symbolInfo.Symbol) is INamedTypeSymbol type)
                yield return type;

            foreach (ISymbol candidate in symbolInfo.CandidateSymbols)
            {
                if (ContainingType(candidate) is INamedTypeSymbol candidateType)
                    yield return candidateType;
            }
        }
    }

    private static INamedTypeSymbol? ContainingType(ISymbol? symbol)
    {
        return symbol switch
        {
            IMethodSymbol member => member.ContainingType,
            IPropertySymbol member => member.ContainingType,
            IFieldSymbol member => member.ContainingType,
            IEventSymbol member => member.ContainingType,
            _ => null
        };
    }

    private bool IsInSelectedMember(
        SyntaxNode node,
        SemanticModel model)
    {
        MemberDeclarationSyntax? member = node.AncestorsAndSelf()
            .OfType<MemberDeclarationSyntax>()
            .FirstOrDefault();

        return member is null || IsSelected(model.GetDeclaredSymbol(member));
    }

    private static bool IsWithinNamespace(
        INamespaceSymbol? namespaceSymbol,
        INamespaceSymbol target)
    {
        string targetName = target.ToDisplayString();

        while (namespaceSymbol is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(namespaceSymbol, target) ||
                namespaceSymbol.ToDisplayString() == targetName)
                return true;

            namespaceSymbol = namespaceSymbol.ContainingNamespace;
        }

        return false;
    }
}

public sealed record TrimmedLibrary(
    string Path,
    IReadOnlyList<MemberDeclarationSyntax> Members,
    IReadOnlyList<UsingDirectiveSyntax> Usings);
