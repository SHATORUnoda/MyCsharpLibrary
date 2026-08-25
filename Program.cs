using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


// ============================================================
// CsharpVersionBundler
//
// usage:
//
//     Bundler <input.cs> <submit.cs> <libdir>
//
// 例:
//
//     Bundler Program.cs submit.cs lib
//
// ============================================================


// ============================================================
// Arguments
// ============================================================

if (args.Length != 3)
{
    Console.WriteLine(
        "usage: Bundler <input.cs> <submit.cs> <libdir>");

    return;
}

string entryPath =
    Path.GetFullPath(args[0]);

string outputPath =
    Path.GetFullPath(args[1]);

string libDir =
    Path.GetFullPath(args[2]);


// ============================================================
// Validate
// ============================================================

if (!File.Exists(entryPath))
{
    Console.WriteLine(
        $"input not found : {entryPath}");

    return;
}

if (!Directory.Exists(libDir))
{
    Console.WriteLine(
        $"lib not found : {libDir}");

    return;
}

string libraryRoot =
    Path.GetFullPath(libDir)
    + Path.DirectorySeparatorChar;


// ============================================================
// DEBUG block removal
// ============================================================
//
// #if DEBUG
// ...
// #endif
//
// を Roslyn に渡す前に完全に削除する。
//
// したがって:
//
//   - DEBUG内のコードは解析されない
//   - DEBUG内で使用されるメソッドも必要扱いされない
//   - DEBUG専用クラスも必要扱いされない
//   - submit.csにもDEBUGブロックは出力されない
//
//
//
// 注意:
//
//   #if !DEBUG
//
// はDEBUGブロックではないので残す。
//
// また:
//
//   #if DEBUG && SOME_SYMBOL
//
// のように DEBUG が AND 条件の一部になっているものは
// DEBUG未定義時に全体が無効になるため削除する。
//
//   #if DEBUG || SOME_SYMBOL
//
// はDEBUG以外でも有効になり得るため、単純削除しない。
//
// ============================================================

bool IsIdentifierCharacter(char c)
{
    return
        char.IsLetterOrDigit(c) ||
        c == '_';
}

bool ContainsPositiveDebugIdentifier(
    string expression)
{
    string trimmed =
        expression.Trim();

    if (trimmed.Length == 0)
        return false;

    // DEBUGが否定されている場合は除外しない。
    if (ContainsIdentifier(
            trimmed,
            "!DEBUG"))
    {
        return false;
    }

    // OR条件がある場合、
    // DEBUGがfalseでも別条件で成立する可能性がある。
    if (trimmed.Contains("||"))
    {
        return false;
    }

    return ContainsIdentifier(
        trimmed,
        "DEBUG");
}

bool ContainsIdentifier(
    string text,
    string identifier)
{
    for (int i = 0;
         i + identifier.Length <= text.Length;
         i++)
    {
        if (!text.AsSpan(
                i,
                identifier.Length)
            .SequenceEqual(
                identifier.AsSpan()))
        {
            continue;
        }

        bool leftOk =
            i == 0 ||
            !IsIdentifierCharacter(
                text[i - 1]);

        int right =
            i + identifier.Length;

        bool rightOk =
            right >= text.Length ||
            !IsIdentifierCharacter(
                text[right]);

        if (leftOk && rightOk)
        {
            return true;
        }
    }

    return false;
}

bool IsIfDirective(string text)
{
    string trimmed =
        text.TrimStart();

    return
        trimmed.StartsWith(
            "#if ",
            StringComparison.Ordinal) ||
        trimmed.StartsWith(
            "#if\t",
            StringComparison.Ordinal);
}

bool IsEndIfDirective(string text)
{
    return text
        .TrimStart()
        .StartsWith(
            "#endif",
            StringComparison.Ordinal);
}

bool IsDebugIfDirective(string text)
{
    if (!IsIfDirective(text))
        return false;

    string trimmed =
        text.TrimStart();

    string expression =
        trimmed[3..].Trim();

    return ContainsPositiveDebugIdentifier(
        expression);
}

string RemoveDebugBlocks(
    string source)
{
    using StringReader reader =
        new(source);

    StringBuilder result =
        new();

    int debugDepth = 0;

    string? line;

    while ((line = reader.ReadLine()) != null)
    {
        string trimmed =
            line.TrimStart();

        // ----------------------------------------------------
        // Outside DEBUG block
        // ----------------------------------------------------

        if (debugDepth == 0)
        {
            if (IsDebugIfDirective(trimmed))
            {
                debugDepth = 1;
                continue;
            }

            result.AppendLine(line);

            continue;
        }

        // ----------------------------------------------------
        // Inside DEBUG block
        // ----------------------------------------------------

        if (IsIfDirective(trimmed))
        {
            debugDepth++;
            continue;
        }

        if (IsEndIfDirective(trimmed))
        {
            debugDepth--;
            continue;
        }

        // DEBUGブロック内部は全部削除
    }

    return result.ToString();
}


// ============================================================
// Read source files
// ============================================================

Dictionary<string, string> sourceFiles =
    new(StringComparer.OrdinalIgnoreCase);


// Entry
sourceFiles[entryPath] =
    RemoveDebugBlocks(
        File.ReadAllText(entryPath));


// Libraries
foreach (string file in
         Directory.GetFiles(
             libDir,
             "*.cs",
             SearchOption.AllDirectories))
{
    string path =
        Path.GetFullPath(file);

    sourceFiles[path] =
        RemoveDebugBlocks(
            File.ReadAllText(file));
}


// ============================================================
// Parse
// ============================================================

CSharpParseOptions parseOptions =
    CSharpParseOptions.Default
        .WithLanguageVersion(
            LanguageVersion.Latest);


Dictionary<string, SyntaxTree> trees =
    new(StringComparer.OrdinalIgnoreCase);

foreach (var pair in sourceFiles)
{
    trees[pair.Key] =
        CSharpSyntaxTree.ParseText(
            pair.Value,
            parseOptions,
            pair.Key);
}


// ============================================================
// Metadata references
// ============================================================

List<MetadataReference> references =
    new();

HashSet<string> referencePaths =
    new(StringComparer.OrdinalIgnoreCase);

void AddReference(
    string path)
{
    if (!File.Exists(path))
        return;

    string fullPath =
        Path.GetFullPath(path);

    if (!referencePaths.Add(fullPath))
        return;

    try
    {
        references.Add(
            MetadataReference.CreateFromFile(
                fullPath));
    }
    catch
    {
        // 読み込めないDLLは無視
    }
}


// ============================================================
// .NET runtime assemblies
// ============================================================

string? trustedAssemblies =
    AppContext.GetData(
        "TRUSTED_PLATFORM_ASSEMBLIES") as string;

if (trustedAssemblies != null)
{
    foreach (string path in
             trustedAssemblies.Split(
                 Path.PathSeparator,
                 StringSplitOptions.RemoveEmptyEntries))
    {
        AddReference(path);
    }
}


// ============================================================
// project.assets.json
// ============================================================
//
// entry.csから親ディレクトリを辿って
//
//     obj/project.assets.json
//
// または
//
//     .dotnet/obj/project.assets.json
//
// を探す。
//
// これによって ac-library-csharp 等のPackageReferenceを
// RoslynのCompilationにも可能な範囲で追加する。
//
// ============================================================

string? FindAssetsFile()
{
    DirectoryInfo? directory =
        new(
            Path.GetDirectoryName(
                entryPath)!);

    while (directory != null)
    {
        string normal =
            Path.Combine(
                directory.FullName,
                "obj",
                "project.assets.json");

        if (File.Exists(normal))
            return normal;

        string dotnet =
            Path.Combine(
                directory.FullName,
                ".dotnet",
                "obj",
                "project.assets.json");

        if (File.Exists(dotnet))
            return dotnet;

        directory =
            directory.Parent;
    }

    return null;
}

void AddPackageReferences(
    string assetsPath)
{
    try
    {
        using FileStream stream =
            File.OpenRead(assetsPath);

        using JsonDocument document =
            JsonDocument.Parse(stream);

        JsonElement root =
            document.RootElement;

        if (!root.TryGetProperty(
                "targets",
                out JsonElement targets))
        {
            return;
        }

        if (!root.TryGetProperty(
                "libraries",
                out JsonElement libraries))
        {
            return;
        }

        string packageRoot =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.UserProfile),
                ".nuget",
                "packages");

        foreach (JsonProperty targetProperty
                 in targets.EnumerateObject())
        {
            JsonElement target =
                targetProperty.Value;

            foreach (JsonProperty libraryProperty
                     in target.EnumerateObject())
            {
                string libraryName =
                    libraryProperty.Name;

                if (!libraries.TryGetProperty(
                        libraryName,
                        out JsonElement libraryInfo))
                {
                    continue;
                }

                if (!libraryInfo.TryGetProperty(
                        "type",
                        out JsonElement typeElement))
                {
                    continue;
                }

                string type =
                    typeElement.GetString() ?? "";

                if (!string.Equals(
                        type,
                        "package",
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!libraryInfo.TryGetProperty(
                        "path",
                        out JsonElement pathElement))
                {
                    continue;
                }

                string? packagePath =
                    pathElement.GetString();

                if (string.IsNullOrEmpty(packagePath))
                    continue;

                string packageDirectory =
                    Path.Combine(
                        packageRoot,
                        packagePath.Replace(
                            '/',
                            Path.DirectorySeparatorChar));

                // ------------------------------------------------
                // compile assets
                // ------------------------------------------------

                if (libraryProperty.Value.TryGetProperty(
                        "compile",
                        out JsonElement compileAssets))
                {
                    foreach (JsonProperty asset
                             in compileAssets.EnumerateObject())
                    {
                        string relativePath =
                            asset.Name;

                        string dll =
                            Path.Combine(
                                packageDirectory,
                                relativePath.Replace(
                                    '/',
                                    Path.DirectorySeparatorChar));

                        AddReference(dll);
                    }
                }

                // ------------------------------------------------
                // runtime assets
                // ------------------------------------------------

                if (libraryProperty.Value.TryGetProperty(
                        "runtime",
                        out JsonElement runtimeAssets))
                {
                    foreach (JsonProperty asset
                             in runtimeAssets.EnumerateObject())
                    {
                        string relativePath =
                            asset.Name;

                        string dll =
                            Path.Combine(
                                packageDirectory,
                                relativePath.Replace(
                                    '/',
                                    Path.DirectorySeparatorChar));

                        AddReference(dll);
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $"warning: could not read project.assets.json: {ex.Message}");
    }
}

string? assetsFile =
    FindAssetsFile();

if (assetsFile != null)
{
    AddPackageReferences(
        assetsFile);
}


// ============================================================
// Compilation
// ============================================================

CSharpCompilation compilation =
    CSharpCompilation.Create(
        assemblyName:
            "CsharpVersionBundler_Analysis",

        syntaxTrees:
            trees.Values,

        references:
            references,

        options:
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel:
                    OptimizationLevel.Release)
    );


// ============================================================
// Diagnostics
// ============================================================
//
// Roslynで解決できない外部ライブラリ等があっても
// Bundler自体は可能な限り処理する。
//
// ============================================================

List<Diagnostic> errors =
    compilation
        .GetDiagnostics()
        .Where(
            x =>
                x.Severity ==
                DiagnosticSeverity.Error)
        .ToList();

if (errors.Count > 0)
{
    Console.WriteLine();
    Console.WriteLine(
        "warning: Roslyn compilation contains errors:");

    foreach (Diagnostic error
             in errors.Take(20))
    {
        Console.WriteLine(
            "  " + error);
    }

    if (errors.Count > 20)
    {
        Console.WriteLine(
            $"  ... {errors.Count - 20} more");
    }

    Console.WriteLine();
}


// ============================================================
// Symbol -> source files
// ============================================================

bool IsLibraryFile(
    string path)
{
    if (string.IsNullOrEmpty(path))
        return false;

    string fullPath =
        Path.GetFullPath(path);

    return fullPath.StartsWith(
        libraryRoot,
        StringComparison.OrdinalIgnoreCase);
}

IEnumerable<string> GetSourceFiles(
    ISymbol symbol)
{
    symbol =
        symbol.OriginalDefinition;

    HashSet<string> result =
        new(StringComparer.OrdinalIgnoreCase);

    foreach (Location location
             in symbol.Locations)
    {
        if (!location.IsInSource)
            continue;

        string? path =
            location.SourceTree?.FilePath;

        if (string.IsNullOrEmpty(path))
            continue;

        path =
            Path.GetFullPath(path);

        if (IsLibraryFile(path))
        {
            result.Add(path);
        }
    }

    foreach (SyntaxReference reference
             in symbol.DeclaringSyntaxReferences)
    {
        string path =
            Path.GetFullPath(
                reference.SyntaxTree.FilePath);

        if (IsLibraryFile(path))
        {
            result.Add(path);
        }
    }

    return result;
}


// ============================================================
// Required symbols
// ============================================================

HashSet<ISymbol> requiredMembers =
    new(SymbolEqualityComparer.Default);

HashSet<INamedTypeSymbol> requiredTypes =
    new(SymbolEqualityComparer.Default);

Queue<ISymbol> analysisQueue =
    new();


// ============================================================
// Add required type
// ============================================================

void AddRequiredType(
    INamedTypeSymbol type)
{
    type =
        type.OriginalDefinition;

    if (!GetSourceFiles(type).Any())
        return;

    if (!requiredTypes.Add(type))
        return;

    Console.WriteLine(
        $"type   : {type.ToDisplayString()}");

    analysisQueue.Enqueue(type);
}


// ============================================================
// Add required member
// ============================================================

void AddRequiredMember(
    ISymbol member)
{
    member =
        member.OriginalDefinition;

    if (!GetSourceFiles(member).Any())
        return;

    switch (member)
    {
        case IMethodSymbol:
        case IPropertySymbol:
        case IFieldSymbol:
        case IEventSymbol:

            if (requiredMembers.Add(member))
            {
                Console.WriteLine(
                    $"member : {member.ToDisplayString()}");

                analysisQueue.Enqueue(member);
            }

            if (member.ContainingType != null)
            {
                AddRequiredType(
                    member.ContainingType);
            }

            break;

        case INamedTypeSymbol namedType:

            AddRequiredType(
                namedType);

            break;
    }
}


// ============================================================
// Add arbitrary symbol
// ============================================================

void AddRequiredSymbol(
    ISymbol? symbol)
{
    if (symbol == null)
        return;

    symbol =
        symbol.OriginalDefinition;

    switch (symbol)
    {
        case INamedTypeSymbol type:

            AddRequiredType(type);

            break;

        case IMethodSymbol:
        case IPropertySymbol:
        case IFieldSymbol:
        case IEventSymbol:

            AddRequiredMember(symbol);

            break;
    }
}


// ============================================================
// Analyze references
// ============================================================
//
// ここが最重要部分。
//
// using static Radix;
//
// のRadixは解析しない。
//
// 一方:
//
// ReadCharArray();
//
// は解析され、Reader.ReadCharArray()を取得する。
//
// ============================================================

void AnalyzeReferences(
    SyntaxNode root,
    SemanticModel model)
{
    foreach (SyntaxNode node
             in root.DescendantNodesAndSelf())
    {
        // ----------------------------------------------------
        // using directive本体と内部を完全に無視
        // ----------------------------------------------------

        if (node is UsingDirectiveSyntax)
            continue;

        if (node.Ancestors()
                .Any(
                    x =>
                        x is UsingDirectiveSyntax))
        {
            continue;
        }

        // ----------------------------------------------------
        // declaration node itself
        //
        // 宣言そのものを「参照」とは扱わない。
        // ただし宣言内部の子ノードは解析する。
        // ----------------------------------------------------

        if (node is MethodDeclarationSyntax)
            continue;

        if (node is ConstructorDeclarationSyntax)
            continue;

        if (node is DestructorDeclarationSyntax)
            continue;

        if (node is PropertyDeclarationSyntax)
            continue;

        if (node is IndexerDeclarationSyntax)
            continue;

        if (node is FieldDeclarationSyntax)
            continue;

        if (node is EventDeclarationSyntax)
            continue;

        if (node is EventFieldDeclarationSyntax)
            continue;

        if (node is TypeDeclarationSyntax)
            continue;

        if (node is EnumDeclarationSyntax)
            continue;

        if (node is DelegateDeclarationSyntax)
            continue;

        // ----------------------------------------------------
        // SymbolInfo
        // ----------------------------------------------------

        SymbolInfo symbolInfo =
            model.GetSymbolInfo(
                node);

        AddRequiredSymbol(
            symbolInfo.Symbol);

        foreach (ISymbol candidate
                 in symbolInfo.CandidateSymbols)
        {
            AddRequiredSymbol(
                candidate);
        }

        // ----------------------------------------------------
        // TypeInfo
        // ----------------------------------------------------

        TypeInfo typeInfo =
            model.GetTypeInfo(
                node);

        AddRequiredSymbol(
            typeInfo.Type);

        AddRequiredSymbol(
            typeInfo.ConvertedType);
    }
}


// ============================================================
// Analyze type header
// ============================================================
//
// クラス全体ではなく、
//
//   - attributes
//   - base class
//   - interfaces
//   - generic constraints
//
// だけを解析する。
//
// これによりReader型を使っただけでReader内の全メソッドが
// 依存扱いされることを防ぐ。
//
// ============================================================

void AnalyzeTypeHeader(
    INamedTypeSymbol type,
    SemanticModel model)
{
    foreach (SyntaxReference reference
             in type.DeclaringSyntaxReferences)
    {
        SyntaxNode declaration =
            reference.GetSyntax();

        if (declaration
            is not BaseTypeDeclarationSyntax baseDeclaration)
        {
            continue;
        }

        // Attributes
        foreach (AttributeListSyntax attributes
                 in baseDeclaration.AttributeLists)
        {
            AnalyzeReferences(
                attributes,
                model);
        }

        // Base class / interfaces
        if (baseDeclaration.BaseList != null)
        {
            AnalyzeReferences(
                baseDeclaration.BaseList,
                model);
        }

        // Generic constraints
        if (baseDeclaration
            is TypeDeclarationSyntax typeDeclaration)
        {
            foreach (
                TypeParameterConstraintClauseSyntax constraint
                in typeDeclaration.ConstraintClauses)
            {
                AnalyzeReferences(
                    constraint,
                    model);
            }
        }
    }
}


// ============================================================
// Analyze entry
// ============================================================

{
    SyntaxTree programTree =
        trees[entryPath];

    SemanticModel programModel =
        compilation.GetSemanticModel(
            programTree);

    AnalyzeReferences(
        programTree.GetRoot(),
        programModel);
}


// ============================================================
// Recursive dependency analysis
// ============================================================

while (analysisQueue.Count > 0)
{
    ISymbol symbol =
        analysisQueue.Dequeue();

    // --------------------------------------------------------
    // Type
    // --------------------------------------------------------

    if (symbol is INamedTypeSymbol namedType)
    {
        foreach (SyntaxReference reference
                 in namedType.DeclaringSyntaxReferences)
        {
            SemanticModel model =
                compilation.GetSemanticModel(
                    reference.SyntaxTree);

            AnalyzeTypeHeader(
                namedType,
                model);
        }

        continue;
    }

    // --------------------------------------------------------
    // Member
    // --------------------------------------------------------

    foreach (SyntaxReference reference
             in symbol.DeclaringSyntaxReferences)
    {
        SyntaxNode declaration =
            reference.GetSyntax();

        SemanticModel model =
            compilation.GetSemanticModel(
                declaration.SyntaxTree);

        AnalyzeReferences(
            declaration,
            model);
    }
}


// ============================================================
// Interface implementation dependencies
// ============================================================
//
// 例えば:
//
//     class Foo : IBar
//     {
//         public void X() { }
//     }
//
// でFoo型だけが必要な場合、X()を削るとFooがコンパイルできなく
// なるので、interfaceの実装に必要なメソッド等は残す。
//
// ============================================================

void AddInterfaceImplementations(
    INamedTypeSymbol type)
{
    foreach (INamedTypeSymbol interfaceType
             in type.AllInterfaces)
    {
        foreach (ISymbol interfaceMember
                 in interfaceType.GetMembers())
        {
            ISymbol? implementation =
                type.FindImplementationForInterfaceMember(
                    interfaceMember);

            if (implementation == null)
                continue;

            string? sourceFile =
                GetSourceFiles(
                    implementation)
                .FirstOrDefault();

            if (sourceFile == null)
                continue;

            AddRequiredMember(
                implementation);
        }
    }
}


// ============================================================
// Add interface implementations
// ============================================================

foreach (INamedTypeSymbol type
         in requiredTypes.ToArray())
{
    if (type.TypeKind ==
        TypeKind.Class)
    {
        AddInterfaceImplementations(
            type);
    }
}


// Newly discovered members
while (analysisQueue.Count > 0)
{
    ISymbol symbol =
        analysisQueue.Dequeue();

    if (symbol is INamedTypeSymbol namedType)
    {
        foreach (SyntaxReference reference
                 in namedType.DeclaringSyntaxReferences)
        {
            SemanticModel model =
                compilation.GetSemanticModel(
                    reference.SyntaxTree);

            AnalyzeTypeHeader(
                namedType,
                model);
        }

        continue;
    }

    foreach (SyntaxReference reference
             in symbol.DeclaringSyntaxReferences)
    {
        SyntaxNode declaration =
            reference.GetSyntax();

        SemanticModel model =
            compilation.GetSemanticModel(
                declaration.SyntaxTree);

        AnalyzeReferences(
            declaration,
            model);
    }
}


// ============================================================
// Runtime-important members
// ============================================================
//
//   - static constructor
//   - destructor
//   - override
//   - explicit interface implementation
//
// ============================================================

bool IsRuntimeRequiredMember(
    ISymbol symbol)
{
    if (symbol is not IMethodSymbol method)
        return false;

    if (method.MethodKind ==
        MethodKind.StaticConstructor)
    {
        return true;
    }

    if (method.MethodKind ==
        MethodKind.Destructor)
    {
        return true;
    }

    if (method.IsOverride)
    {
        return true;
    }

    if (method.ExplicitInterfaceImplementations.Length > 0)
    {
        return true;
    }

    return false;
}


// ============================================================
// Runtime-important member closure
// ============================================================

bool runtimeChanged = true;

while (runtimeChanged)
{
    runtimeChanged = false;

    foreach (INamedTypeSymbol type
             in requiredTypes.ToArray())
    {
        foreach (ISymbol member
                 in type.GetMembers())
        {
            if (!IsRuntimeRequiredMember(member))
                continue;

            ISymbol normalized =
                member.OriginalDefinition;

            if (requiredMembers.Contains(
                    normalized))
            {
                continue;
            }

            AddRequiredMember(
                normalized);

            runtimeChanged = true;
        }
    }

    while (analysisQueue.Count > 0)
    {
        ISymbol symbol =
            analysisQueue.Dequeue();

        if (symbol is INamedTypeSymbol namedType)
        {
            foreach (
                SyntaxReference reference
                in namedType.DeclaringSyntaxReferences)
            {
                SemanticModel model =
                    compilation.GetSemanticModel(
                        reference.SyntaxTree);

                AnalyzeTypeHeader(
                    namedType,
                    model);
            }

            continue;
        }

        foreach (
            SyntaxReference reference
            in symbol.DeclaringSyntaxReferences)
        {
            SyntaxNode declaration =
                reference.GetSyntax();

            SemanticModel model =
                compilation.GetSemanticModel(
                    declaration.SyntaxTree);

            AnalyzeReferences(
                declaration,
                model);
        }
    }
}


// ============================================================
// Required files
// ============================================================

HashSet<string> requiredFiles =
    new(StringComparer.OrdinalIgnoreCase);

foreach (INamedTypeSymbol type
         in requiredTypes)
{
    foreach (string path
             in GetSourceFiles(type))
    {
        requiredFiles.Add(path);
    }
}

foreach (ISymbol member
         in requiredMembers)
{
    foreach (string path
             in GetSourceFiles(member))
    {
        requiredFiles.Add(path);
    }
}


// ============================================================
// Required members grouped by type
// ============================================================

Dictionary<
    INamedTypeSymbol,
    HashSet<ISymbol>
> membersByType =
    new(SymbolEqualityComparer.Default);

foreach (ISymbol member
         in requiredMembers)
{
    if (member.ContainingType == null)
        continue;

    INamedTypeSymbol type =
        member.ContainingType.OriginalDefinition;

    if (!membersByType.TryGetValue(
            type,
            out HashSet<ISymbol>? members))
    {
        members =
            new(
                SymbolEqualityComparer.Default);

        membersByType[type] =
            members;
    }

    members.Add(
        member.OriginalDefinition);
}


// ============================================================
// using filter
// ============================================================
//
// normal using:
//   安全のためそのまま残す。
//
// using static:
//   libの型をターゲットとしていて、それが不要なら削除。
//   System.Console等の外部型は残す。
//
// ============================================================

bool ShouldKeepUsing(
    UsingDirectiveSyntax usingDirective,
    SemanticModel model)
{
    // --------------------------------------------------------
    // using static
    // --------------------------------------------------------

    if (usingDirective.StaticKeyword != default)
    {
        NameSyntax name =
            usingDirective.Name!;

        SymbolInfo info =
            model.GetSymbolInfo(
                (SyntaxNode)name);

        ISymbol? target =
            info.Symbol;

        // 解決できなければ安全側で残す
        if (target == null)
            return true;

        string? sourcePath =
            GetSourceFiles(target)
                .FirstOrDefault();

        // System.Console等
        if (sourcePath == null)
            return true;

        return requiredFiles.Contains(
            sourcePath);
    }

    // --------------------------------------------------------
    // alias
    // --------------------------------------------------------

    if (usingDirective.Alias != null)
    {
        NameSyntax name =
            usingDirective.Name!;

        SymbolInfo info =
            model.GetSymbolInfo(
                (SyntaxNode)name);

        ISymbol? target =
            info.Symbol;

        if (target == null)
            return true;

        string? sourcePath =
            GetSourceFiles(target)
                .FirstOrDefault();

        if (sourcePath == null)
            return true;

        return requiredFiles.Contains(
            sourcePath);
    }

    // --------------------------------------------------------
    // normal using
    // --------------------------------------------------------
    //
    // System.Linqなどを誤って消すと、
    // ライブラリ側の残ったメソッドがコンパイルできなくなる可能性
    // があるので、通常のusingは保持する。
    //
    // --------------------------------------------------------

    return true;
}


// ============================================================
// Collect using directives
// ============================================================

HashSet<string> collectedUsings =
    new(StringComparer.Ordinal);

void CollectUsings(
    string path)
{
    SyntaxTree tree =
        trees[path];

    SemanticModel model =
        compilation.GetSemanticModel(
            tree);

    CompilationUnitSyntax unit =
        (CompilationUnitSyntax)
            tree.GetRoot();

    foreach (UsingDirectiveSyntax usingDirective
             in unit.Usings)
    {
        if (!ShouldKeepUsing(
                usingDirective,
                model))
        {
            continue;
        }

        string text =
            usingDirective
                .ToFullString()
                .Trim();

        if (text.Length == 0)
            continue;

        collectedUsings.Add(text);
    }
}

CollectUsings(entryPath);

foreach (string path
         in requiredFiles)
{
    CollectUsings(path);
}


// ============================================================
// Filter type declaration
// ============================================================

SyntaxNode FilterTypeDeclaration(
    TypeDeclarationSyntax typeDeclaration,
    SemanticModel model)
{
    INamedTypeSymbol? type =
        model.GetDeclaredSymbol(
            typeDeclaration);

    if (type == null)
        return typeDeclaration;

    type =
        type.OriginalDefinition;


    // --------------------------------------------------------
    // Interface
    // --------------------------------------------------------
    //
    // interfaceのメンバーは契約なので削らない。
    //
    // --------------------------------------------------------

    if (type.TypeKind ==
        TypeKind.Interface)
    {
        return typeDeclaration;
    }


    membersByType.TryGetValue(
        type,
        out HashSet<ISymbol>? required);

    required ??=
        new(
            SymbolEqualityComparer.Default);


    List<MemberDeclarationSyntax> kept =
        new();


    // structはフィールドを勝手に削るとレイアウトや初期化が
    // 変わるため、フィールドを保持する。
    bool preserveStructFields =
        type.TypeKind ==
        TypeKind.Struct;


    foreach (MemberDeclarationSyntax member
             in typeDeclaration.Members)
    {
        ISymbol? memberSymbol =
            model.GetDeclaredSymbol(
                member);

        if (memberSymbol == null)
            continue;

        memberSymbol =
            memberSymbol.OriginalDefinition;


        // ----------------------------------------------------
        // Nested type
        // ----------------------------------------------------

        if (memberSymbol
            is INamedTypeSymbol nestedType)
        {
            if (!requiredTypes.Contains(
                    nestedType))
            {
                continue;
            }

            if (member
                is TypeDeclarationSyntax nestedDeclaration)
            {
                SyntaxNode filteredNested =
                    FilterTypeDeclaration(
                        nestedDeclaration,
                        model);

                kept.Add(
                    (MemberDeclarationSyntax)
                        filteredNested);
            }
            else
            {
                kept.Add(member);
            }

            continue;
        }


        // ----------------------------------------------------
        // Struct fields
        // ----------------------------------------------------

        if (preserveStructFields &&
            member is FieldDeclarationSyntax)
        {
            kept.Add(member);
            continue;
        }


        // ----------------------------------------------------
        // Required member
        // ----------------------------------------------------

        if (required.Contains(
                memberSymbol))
        {
            kept.Add(member);
        }
    }


    return typeDeclaration.WithMembers(
        new SyntaxList<MemberDeclarationSyntax>(
            kept));
}


// ============================================================
// Filter namespace
// ============================================================

SyntaxNode FilterNamespaceDeclaration(
    BaseNamespaceDeclarationSyntax namespaceDeclaration,
    SemanticModel model)
{
    List<MemberDeclarationSyntax> kept =
        new();

    foreach (MemberDeclarationSyntax member
             in namespaceDeclaration.Members)
    {
        // ----------------------------------------------------
        // Nested namespace
        // ----------------------------------------------------

        if (member
            is BaseNamespaceDeclarationSyntax nestedNamespace)
        {
            SyntaxNode filteredNested =
                FilterNamespaceDeclaration(
                    nestedNamespace,
                    model);

            if (filteredNested
                is BaseNamespaceDeclarationSyntax filteredBase &&
                filteredBase.Members.Count == 0)
            {
                continue;
            }

            kept.Add(
                (MemberDeclarationSyntax)
                    filteredNested);

            continue;
        }


        // ----------------------------------------------------
        // Class / struct / record / interface
        // ----------------------------------------------------

        if (member
            is TypeDeclarationSyntax typeDeclaration)
        {
            INamedTypeSymbol? type =
                model.GetDeclaredSymbol(
                    typeDeclaration);

            if (type == null)
                continue;

            type =
                type.OriginalDefinition;

            if (!requiredTypes.Contains(type))
                continue;

            SyntaxNode filteredType =
                FilterTypeDeclaration(
                    typeDeclaration,
                    model);

            kept.Add(
                (MemberDeclarationSyntax)
                    filteredType);

            continue;
        }


        // ----------------------------------------------------
        // Enum
        // ----------------------------------------------------

        if (member
            is EnumDeclarationSyntax enumDeclaration)
        {
            INamedTypeSymbol? type =
                model.GetDeclaredSymbol(
                    enumDeclaration);

            if (type == null)
                continue;

            type =
                type.OriginalDefinition;

            if (requiredTypes.Contains(type))
            {
                kept.Add(member);
            }

            continue;
        }


        // ----------------------------------------------------
        // Delegate
        // ----------------------------------------------------

        if (member
            is DelegateDeclarationSyntax delegateDeclaration)
        {
            INamedTypeSymbol? type =
                model.GetDeclaredSymbol(
                    delegateDeclaration);

            if (type == null)
                continue;

            type =
                type.OriginalDefinition;

            if (requiredTypes.Contains(type))
            {
                kept.Add(member);
            }
        }
    }


    // --------------------------------------------------------
    // file-scoped namespace
    // --------------------------------------------------------
    //
    // 複数の.csを1つに結合するため、
    //
    // namespace Foo;
    //
    // を
    //
    // namespace Foo
    // {
    //     ...
    // }
    //
    // に変換する。
    //
    // --------------------------------------------------------

    if (namespaceDeclaration
        is FileScopedNamespaceDeclarationSyntax fileScoped)
    {
        NamespaceDeclarationSyntax converted =
            SyntaxFactory
                .NamespaceDeclaration(
                    fileScoped.Name)
                .WithExterns(
                    fileScoped.Externs)
                .WithUsings(
                    fileScoped.Usings)
                .WithMembers(
                    new SyntaxList<MemberDeclarationSyntax>(
                        kept))
                .WithLeadingTrivia(
                    fileScoped.GetLeadingTrivia())
                .WithTrailingTrivia(
                    fileScoped.GetTrailingTrivia());

        return converted;
    }


    // block namespace
    return namespaceDeclaration.WithMembers(
        new SyntaxList<MemberDeclarationSyntax>(
            kept));
}


// ============================================================
// Append library file
// ============================================================

void AppendLibraryFile(
    StringBuilder output,
    string path)
{
    SyntaxTree tree =
        trees[path];

    SemanticModel model =
        compilation.GetSemanticModel(
            tree);

    CompilationUnitSyntax unit =
        (CompilationUnitSyntax)
            tree.GetRoot();

    output.AppendLine(
        $"// ===== {Path.GetFileName(path)} =====");


    foreach (MemberDeclarationSyntax member
             in unit.Members)
    {
        // ----------------------------------------------------
        // Namespace
        // ----------------------------------------------------

        if (member
            is BaseNamespaceDeclarationSyntax namespaceDeclaration)
        {
            SyntaxNode filtered =
                FilterNamespaceDeclaration(
                    namespaceDeclaration,
                    model);

            if (filtered
                is BaseNamespaceDeclarationSyntax filteredBase &&
                filteredBase.Members.Count == 0)
            {
                continue;
            }

            output.AppendLine(
                filtered.ToFullString());

            continue;
        }


        // ----------------------------------------------------
        // Type
        // ----------------------------------------------------

        if (member
            is TypeDeclarationSyntax typeDeclaration)
        {
            INamedTypeSymbol? type =
                model.GetDeclaredSymbol(
                    typeDeclaration);

            if (type == null)
                continue;

            type =
                type.OriginalDefinition;

            if (!requiredTypes.Contains(type))
                continue;

            SyntaxNode filtered =
                FilterTypeDeclaration(
                    typeDeclaration,
                    model);

            output.AppendLine(
                filtered.ToFullString());

            continue;
        }


        // ----------------------------------------------------
        // Enum
        // ----------------------------------------------------

        if (member
            is EnumDeclarationSyntax enumDeclaration)
        {
            INamedTypeSymbol? type =
                model.GetDeclaredSymbol(
                    enumDeclaration);

            if (type == null)
                continue;

            type =
                type.OriginalDefinition;

            if (requiredTypes.Contains(type))
            {
                output.AppendLine(
                    enumDeclaration.ToFullString());
            }

            continue;
        }


        // ----------------------------------------------------
        // Delegate
        // ----------------------------------------------------

        if (member
            is DelegateDeclarationSyntax delegateDeclaration)
        {
            INamedTypeSymbol? type =
                model.GetDeclaredSymbol(
                    delegateDeclaration);

            if (type == null)
                continue;

            type =
                type.OriginalDefinition;

            if (requiredTypes.Contains(type))
            {
                output.AppendLine(
                    delegateDeclaration.ToFullString());
            }
        }
    }

    output.AppendLine();
}


// ============================================================
// Generate submit.cs
// ============================================================

StringBuilder result =
    new();


// ============================================================
// Usings
// ============================================================

foreach (string usingText
         in collectedUsings.OrderBy(x => x))
{
    result.AppendLine(
        usingText);
}

result.AppendLine();


// ============================================================
// Libraries
// ============================================================

foreach (string path
         in requiredFiles.OrderBy(x => x))
{
    AppendLibraryFile(
        result,
        path);
}


// ============================================================
// Main
// ============================================================

result.AppendLine(
    "// ===== Main =====");

CompilationUnitSyntax mainRoot =
    (CompilationUnitSyntax)
        trees[entryPath].GetRoot();

CompilationUnitSyntax mainWithoutUsings =
    mainRoot.WithUsings(
        new SyntaxList<UsingDirectiveSyntax>());

result.Append(
    mainWithoutUsings.ToFullString());

result.AppendLine();


// ============================================================
// Footer
// ============================================================

result.AppendLine(
    "// Generated using CsharpVersionBundler by SHATORUnoda");

result.AppendLine(
    "// https://github.com/SHATORUnoda/MyCsharpLibrary/blob/main/Program.cs");


// ============================================================
// Write submit.cs
// ============================================================

File.WriteAllText(
    outputPath,
    result.ToString());


// ============================================================
// Result
// ============================================================

Console.WriteLine();

Console.WriteLine(
    "========== Bundler Result ==========");

Console.WriteLine(
    $"files   : {requiredFiles.Count}");

Console.WriteLine(
    $"types   : {requiredTypes.Count}");

Console.WriteLine(
    $"members : {requiredMembers.Count}");

Console.WriteLine();

foreach (string path
         in requiredFiles.OrderBy(x => x))
{
    Console.WriteLine(
        $"  {Path.GetFileName(path)}");
}

Console.WriteLine();

Console.WriteLine(
    $"generated : {outputPath}");
