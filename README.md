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

```bash
GetUncommented.exe /include:C:\Projects\ProjectA,C:\Projects\ProjectB /exclude:bin,obj /membertype:public
```

## Output

The output will be saved in a JSON file named nocomments_yyyyMMdd_HHmmss.json, where yyyyMMdd_HHmmss is the current date and time. The output will contain the following structure:

```{
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


# Contributing

Contributions are welcome! Please feel free to submit a pull request or open an issue.
