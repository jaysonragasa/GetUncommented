using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using System.Text.Json;

class Program
{
	static void Main(string[] args)
	{
		// Parse the command line arguments
		var includeFolders = new List<string>();
		var excludeFolders = new List<string>();
		string memberType = "all"; // Default to 'all'

		foreach (var arg in args)
		{
			if (arg.StartsWith("/include:"))
				includeFolders.AddRange(arg.Substring("/include:".Length).Split(','));

			if (arg.StartsWith("/exclude:"))
				excludeFolders.AddRange(arg.Substring("/exclude:".Length).Split(','));

			if (arg.StartsWith("/membertype:"))
				memberType = arg.Substring("/membertype:".Length);
		}

		// Dictionary to store uncommented members by file
		var uncommentedMembers = new Dictionary<string, List<object>>();

		// Iterate through each include folder
		foreach (var folder in includeFolders)
		{
			if (Directory.Exists(folder))
			{
				foreach (var file in Directory.EnumerateFiles(folder, "*.cs", SearchOption.AllDirectories))
				{
					// Skip excluded folders
					if (excludeFolders.Any(exclude => file.Contains(exclude, StringComparison.OrdinalIgnoreCase)))
						continue;

					// Process the file
					ProcessFile(file, memberType, uncommentedMembers);
				}
			}
		}

		// Write output to a JSON file
		string outputFile = $"nocomments_{DateTime.Now:yyyyMMdd_HHmmss}.json";
		File.WriteAllText(outputFile, JsonSerializer.Serialize(uncommentedMembers, new JsonSerializerOptions { WriteIndented = true }));

		// Output the path of the JSON file
		Console.WriteLine($"Results saved to: {Path.GetFullPath(outputFile)}");
	}

	// Process a single .cs file
	static void ProcessFile(string filePath, string memberType, Dictionary<string, List<object>> uncommentedMembers)
	{
		Console.WriteLine($"Processing {filePath}");

		var code = File.ReadAllText(filePath);
		var tree = CSharpSyntaxTree.ParseText(code);
		var root = tree.GetRoot();

		// Process classes, interfaces, enums
		var classesAndStructs = root.DescendantNodes().OfType<TypeDeclarationSyntax>();
		foreach (var classOrStruct in classesAndStructs)
		{
			AddUncommentedMembers(filePath, classOrStruct, memberType, uncommentedMembers);
		}

		var interfaces = root.DescendantNodes().OfType<InterfaceDeclarationSyntax>();
		foreach (var interfaceNode in interfaces)
		{
			AddUncommentedMembers(filePath, interfaceNode, memberType, uncommentedMembers);
		}

		var enums = root.DescendantNodes().OfType<EnumDeclarationSyntax>();
		foreach (var enumNode in enums)
		{
			AddUncommentedMembers(filePath, enumNode, memberType, uncommentedMembers);
		}
	}

	// Add uncommented members (methods, properties, constructors, classes, enums, interfaces) to the result
	static void AddUncommentedMembers<T>(string filePath, T node, string memberType, Dictionary<string, List<object>> uncommentedMembers) where T : MemberDeclarationSyntax
	{
		var members = node.DescendantNodes().OfType<MemberDeclarationSyntax>()
			.Where(member => IsValidMember(member, memberType))
			.Where(member => !IsBackingField(member))  // Exclude private backing fields
			.Where(member => !HasComment(member));     // Ensure it doesn't have comments

		foreach (var member in members)
		{
			var lineSpan = member.SyntaxTree.GetLineSpan(member.Span);
			var lineNumber = lineSpan.StartLinePosition.Line + 1;

			if (!uncommentedMembers.ContainsKey(filePath))
				uncommentedMembers[filePath] = new List<object>();

			// Extract the member declaration based on its type
			string memberDeclaration = ExtractMemberDeclaration(member);

			uncommentedMembers[filePath].Add(new
			{
				Line = lineNumber,
				Member = memberDeclaration
			});
		}
	}

	// Extracts the member declaration (only the signature or declaration line) and excludes method bodies
	static string ExtractMemberDeclaration(MemberDeclarationSyntax member)
	{
		// Use a StringBuilder for optimized string construction
		var declaration = new StringBuilder();

		switch (member)
		{
			case MethodDeclarationSyntax method:
				// Build the method signature
				declaration.Append(string.Join(" ", method.Modifiers) + " ");
				declaration.Append(method.ReturnType + " ");
				declaration.Append(method.Identifier.Text + method.ParameterList.ToString());
				break;

			case PropertyDeclarationSyntax property:
				// Build the property declaration
				declaration.Append(string.Join(" ", property.Modifiers) + " ");
				declaration.Append(property.Type + " ");
				declaration.Append(property.Identifier.Text + " { get; set; }");
				break;

			case FieldDeclarationSyntax field:
				// Build the field declaration
				declaration.Append(string.Join(" ", field.Modifiers) + " ");
				declaration.Append(field.Declaration.Type + " ");
				declaration.Append(string.Join(", ", field.Declaration.Variables.Select(v => v.Identifier.Text)));
				break;

			case ConstructorDeclarationSyntax constructor:
				// Build the constructor signature
				declaration.Append(string.Join(" ", constructor.Modifiers) + " ");
				declaration.Append(constructor.Identifier.Text + constructor.ParameterList.ToString());
				break;

			case ClassDeclarationSyntax classDecl:
				// Build the class declaration
				declaration.Append(string.Join(" ", classDecl.Modifiers) + " class ");
				declaration.Append(classDecl.Identifier.Text);
				break;

			case InterfaceDeclarationSyntax interfaceDecl:
				// Build the interface declaration
				declaration.Append(string.Join(" ", interfaceDecl.Modifiers) + " interface ");
				declaration.Append(interfaceDecl.Identifier.Text);
				break;

			case EnumDeclarationSyntax enumDecl:
				// Build the enum declaration
				declaration.Append(string.Join(" ", enumDecl.Modifiers) + " enum ");
				declaration.Append(enumDecl.Identifier.Text);
				break;

			default:
				// For any other declaration, return the first line of the syntax
				return member.GetText().ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None)[0].Trim();
		}

		return declaration.ToString().Trim();
	}


	// Helper method to check if a member is a valid one based on its type (public/private/all)
	static bool IsValidMember(MemberDeclarationSyntax member, string memberType)
	{
		// Retrieve member's modifiers and compare with the provided memberType
		var modifiers = member.Modifiers.Select(m => m.ValueText.ToLower()).ToList();

		return memberType.ToLower() switch
		{
			"public" => modifiers.Contains("public"),
			"private" => modifiers.Contains("private"),
			_ => true // Default case for "all"
		};
	}

	// Helper method to check if a member is a backing field (must be private and likely used as a field)
	static bool IsBackingField(SyntaxNode member)
	{
		if (member is FieldDeclarationSyntax field)
		{
			// Check if the field is private and likely a backing field (you can enhance the logic here)
			return field.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword));
		}

		return false;
	}

	// Helper method to check if the member has XML-style comments (triple slashes)
	static bool HasComment(SyntaxNode member)
	{
		var trivia = member.GetLeadingTrivia();

		return trivia.Any(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
							   t.IsKind(SyntaxKind.MultiLineCommentTrivia) ||
							   t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia));
	}
}
