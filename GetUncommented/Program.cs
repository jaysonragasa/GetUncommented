using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.Json;

public class Program
{
	static void Main(string[] args)
	{
		// Initialize lists for folder paths
		List<string> folderPaths = new List<string>();
		List<string> excludedFolders = new List<string>();

		// Parse command line arguments
		ParseArguments(args, folderPaths, excludedFolders);

		// Prepare output file name with timestamp
		string outputFileName = $"nocomments_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json";
		string outputFilePath = Path.Combine(Directory.GetCurrentDirectory(), outputFileName);

		// Initialize a dictionary to hold the output data organized by file
		var outputData = new Dictionary<string, List<FileOutput>>();

		foreach (var folderPath in folderPaths)
		{
			if (Directory.Exists(folderPath))
			{
				ProcessDirectory(folderPath, excludedFolders, outputData);
			}
			else
			{
				Console.WriteLine($"Folder not found: {folderPath}");
			}
		}

		// Write the output data to the JSON file
		File.WriteAllText(outputFilePath, JsonSerializer.Serialize(outputData, new JsonSerializerOptions { WriteIndented = true }));

		Console.WriteLine($"Output saved to: {outputFilePath}");
	}

	static void ParseArguments(string[] args, List<string> folderPaths, List<string> excludedFolders)
	{
		foreach (var arg in args)
		{
			if (arg.StartsWith("/include:", StringComparison.OrdinalIgnoreCase))
				folderPaths.AddRange(arg.Substring("/include:".Length).Split(',').Select(p => p.Trim()));
			else if (arg.StartsWith("/exclude:", StringComparison.OrdinalIgnoreCase))
				excludedFolders.AddRange(arg.Substring("/exclude:".Length).Split(',').Select(p => p.Trim()));
		}
	}

	static void ProcessDirectory(string path, List<string> excludedFolders, Dictionary<string, List<FileOutput>> outputData)
	{
		if (IsExcludedDirectory(path, excludedFolders)) return;

		Console.WriteLine($"Processing directory: {path}");

		foreach (var file in Directory.GetFiles(path, "*.cs"))
		{
			Console.WriteLine($"Processing file: {file}");
			ProcessFile(file, outputData);
		}

		foreach (var directory in Directory.GetDirectories(path))
		{
			ProcessDirectory(directory, excludedFolders, outputData);
		}
	}

	static bool IsExcludedDirectory(string directory, List<string> excludedFolders)
	{
		return excludedFolders.Any(ex => string.Equals(Path.GetFileName(directory), ex, StringComparison.OrdinalIgnoreCase));
	}

	static void ProcessFile(string file, Dictionary<string, List<FileOutput>> outputData)
	{
		string code = File.ReadAllText(file);
		var root = CSharpSyntaxTree.ParseText(code).GetRoot();

		ProcessDeclarations(root, code, outputData, file);
	}

	static void ProcessDeclarations(SyntaxNode root, string code, Dictionary<string, List<FileOutput>> outputData, string file)
	{
		var declarations = root.DescendantNodes()
			.Where(node => node is ClassDeclarationSyntax || node is EnumDeclarationSyntax || node is InterfaceDeclarationSyntax);

		foreach (var declaration in declarations)
		{
			CheckAndAddToOutput(code, outputData, file, declaration);

			foreach (var member in GetPublicMembers(declaration))
			{
				CheckAndAddToOutput(code, outputData, file, member);
			}
		}
	}

	static IEnumerable<SyntaxNode> GetPublicMembers(SyntaxNode declaration)
	{
		return declaration switch
		{
			ClassDeclarationSyntax classDecl => classDecl.Members.Where(IsPublicMember),
			EnumDeclarationSyntax enumDecl => enumDecl.Members,
			InterfaceDeclarationSyntax interfaceDecl => interfaceDecl.Members.Where(IsPublicMember),
			_ => Enumerable.Empty<SyntaxNode>()
		};
	}

	static bool IsPublicMember(SyntaxNode member)
	{
		return member switch
		{
			MethodDeclarationSyntax method => method.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PublicKeyword)),
			PropertyDeclarationSyntax property => property.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PublicKeyword)),
			FieldDeclarationSyntax field => field.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PublicKeyword)),
			ConstructorDeclarationSyntax constructor => constructor.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PublicKeyword)),
			EventDeclarationSyntax eventDecl => eventDecl.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PublicKeyword)),
			_ => false
		};
	}

	static void CheckAndAddToOutput(string code, Dictionary<string, List<FileOutput>> outputData, string fileName, SyntaxNode node)
	{
		if (!node.GetLeadingTrivia().Any(trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
													 trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) ||
													 trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)))
		{
			var lineContent = code.Split('\n')[node.GetLocation().GetLineSpan().StartLinePosition.Line].Trim();
			AddToOutput(outputData, fileName, node is ClassDeclarationSyntax ? "Class" :
										 node is EnumDeclarationSyntax ? "Enum" :
										 node is InterfaceDeclarationSyntax ? "Interface" : "Member", lineContent);
		}
	}

	static void AddToOutput(Dictionary<string, List<FileOutput>> outputData, string fileName, string type, string name)
	{
		if (!outputData.ContainsKey(fileName))
		{
			outputData[fileName] = new List<FileOutput>();
		}
		outputData[fileName].Add(new FileOutput { Type = type, Name = name });
	}
}

public class FileOutput
{
	public string Type { get; set; }
	public string Name { get; set; }
}