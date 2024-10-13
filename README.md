# C# Comment Checker

This C# console application scans specified directories for `.cs` files and checks for uncommented members, classes, interfaces, and enums. The results are output in JSON format, making it easy to identify areas of your code that lack documentation.

## Features

- Recursively scans through specified directories for C# files.
- Checks for public, private, or all member types.
- Excludes specified folders from scanning.
- Excludes private backing fields from the results.
- Outputs results in JSON format.

## Prerequisites

- .NET SDK installed on your machine (for building).
- The compiled executable (`CommentChecker.exe` or similar).

## Usage

To run the application, use the following command line syntax in your terminal or command prompt:

GetUncommented.exe [options]


### Accepted Arguments

1. **`/include:`**  
   Specifies the directories to include in the scan. Multiple directories can be separated by commas.
   - **Example:** `/include:C:\Projects\ProjectA,C:\Projects\ProjectB`

2. **`/exclude:`**  
   Specifies the directories to exclude from the scan. This can help you skip folders that you do not want to check.
   - **Example:** `/exclude:bin,obj`

3. **`/membertype:`**  
   Specifies the type of members to check for comments. Accepted values are:
   - **`public`**: Checks only public members.
   - **`private`**: Checks only private members.
   - **`all`**: Checks all members (default behavior if not specified).
   - **Example:** `/membertype:public`

## Example Command

```batch
GetUncommented.exe /include:C:\Projects\ProjectA,C:\Projects\ProjectB /exclude:bin,obj /membertype:public
```

## Output

The output will be saved in a JSON file named nocomments_yyyyMMdd_HHmmss.json, where yyyyMMdd_HHmmss is the current date and time. The output will contain the following structure:

```json
{
  "C:\\Projects\\ProjectA\\ClassA.cs": [
    {
      "Line": 10,
      "Member": "public void MethodA()"
    },
    {
      "Line": 15,
      "Member": "public class ClassA"
    }
  ],
  "C:\\Projects\\ProjectB\\ClassB.cs": [
    {
      "Line": 8,
      "Member": "private int fieldB"
    }
  ]
}
```

## Adding Comments

You can go check CoPilot and add comments for you. Here's a sample from our sample output json file
```csharp
/// <summary>
/// Represents an easing function that combines BounceIn and BounceOut.
/// </summary>
public static readonly Easing BounceInOut;

/// <summary>
/// Represents an easing function that accelerates from zero velocity.
/// </summary>
public static readonly Easing QuadIn;

/// <summary>
/// Represents an easing function that decelerates to zero velocity.
/// </summary>
public static readonly Easing QuadOut;

/// <summary>
/// Represents an easing function that accelerates and then decelerates.
/// </summary>
public static readonly Easing QuadInOut;

/// <summary>
/// Represents an easing function that accelerates from zero velocity more sharply.
/// </summary>
public static readonly Easing QuartIn;

/// <summary>
/// Represents an easing function that decelerates to zero velocity more sharply.
/// </summary>
public static readonly Easing QuartOut;

/// <summary>
/// Represents an easing function that accelerates and then decelerates more sharply.
/// </summary>
public static readonly Easing QuartInOut;

/// <summary>
/// Represents an easing function that accelerates exponentially.
/// </summary>
public static readonly Easing ExpoIn;

/// <summary>
/// Represents an easing function that decelerates exponentially.
/// </summary>
public static readonly Easing ExpoOut;

/// <summary>
/// Represents an easing function that accelerates and then decelerates exponentially.
/// </summary>
public static readonly Easing ExpoInOut;

/// <summary>
/// Represents an easing function that accelerates with a backtracking effect.
/// </summary>
public static readonly Easing BackIn;

/// <summary>
/// Represents an easing function that decelerates with a backtracking effect.
/// </summary>
public static readonly Easing BackOut;

/// <summary>
/// Represents an easing function that accelerates and then decelerates with a backtracking effect.
/// </summary>
public static readonly Easing BackInOut;

/// <summary>
/// Represents an easing function that accelerates very sharply.
/// </summary>
public static readonly Easing QuintIn;

/// <summary>
/// Represents an easing function that decelerates very sharply.
/// </summary>
public static readonly Easing QuintOut;

/// <summary>
/// Represents an easing function that accelerates and then decelerates very sharply.
/// </summary>
public static readonly Easing QuintInOut;

/// <summary>
/// Represents an easing function that accelerates in a circular motion.
/// </summary>
public static readonly Easing CircIn;

/// <summary>
/// Represents an easing function that decelerates in a circular motion.
/// </summary>
public static readonly Easing CircOut;

/// <summary>
/// Represents an easing function that accelerates and then decelerates in a circular motion.
/// </summary>
public static readonly Easing CircInOut;

/// <summary>
/// Represents an easing function that accelerates with an elastic effect.
/// </summary>
public static readonly Easing ElasticIn;

/// <summary>
/// Represents an easing function that decelerates with an elastic effect.
/// </summary>
public static readonly Easing ElasticOut;

/// <summary>
/// Represents an easing function that accelerates and then decelerates with an elastic effect.
/// </summary>
public static readonly Easing ElasticInOut;

```

Just take one array from our json and ask CoPilot to add comments in XML documentation comment. Here's a portion from our sample JSON output file.
```JSON
"C:\\github\\maui-samples\\8.0\\Animations\\Animations\\Extensions\\ExtensionMethods.cs": [
    {
      "Line": 6,
      "Member": "public static readonly Easing BounceInOut"
    },
    {
      "Line": 13,
      "Member": "public static readonly Easing QuadIn"
    },
    {
      "Line": 18,
      "Member": "public static readonly Easing QuadOut"
    },
    {
      "Line": 23,
      "Member": "public static readonly Easing QuadInOut"
    },
    {
      "Line": 30,
      "Member": "public static readonly Easing QuartIn"
    },
    {
      "Line": 35,
      "Member": "public static readonly Easing QuartOut"
    },
    {
      "Line": 40,
      "Member": "public static readonly Easing QuartInOut"
    },
    {
      "Line": 47,
      "Member": "public static readonly Easing ExpoIn"
    },
    {
      "Line": 54,
      "Member": "public static readonly Easing ExpoOut"
    },
    {
      "Line": 61,
      "Member": "public static readonly Easing ExpoInOut"
    },
    {
      "Line": 77,
      "Member": "public static readonly Easing BackIn"
    },
    {
      "Line": 82,
      "Member": "public static readonly Easing BackOut"
    },
    {
      "Line": 87,
      "Member": "public static readonly Easing BackInOut"
    },
    {
      "Line": 95,
      "Member": "public static readonly Easing QuintIn"
    },
    {
      "Line": 100,
      "Member": "public static readonly Easing QuintOut"
    },
    {
      "Line": 105,
      "Member": "public static readonly Easing QuintInOut"
    },
    {
      "Line": 113,
      "Member": "public static readonly Easing CircIn"
    },
    {
      "Line": 118,
      "Member": "public static readonly Easing CircOut"
    },
    {
      "Line": 123,
      "Member": "public static readonly Easing CircInOut"
    },
    {
      "Line": 131,
      "Member": "public static readonly Easing ElasticIn"
    },
    {
      "Line": 140,
      "Member": "public static readonly Easing ElasticOut"
    },
    {
      "Line": 149,
      "Member": "public static readonly Easing ElasticInOut"
    }
  ],
```

And this is the actual code

[ExtensionMethods.cs](https://github.com/dotnet/maui-samples/blob/main/8.0/Animations/Animations/Extensions/ExtensionMethods.cs)


# Contributing

Contributions are welcome! Please feel free to submit a pull request or open an issue.
