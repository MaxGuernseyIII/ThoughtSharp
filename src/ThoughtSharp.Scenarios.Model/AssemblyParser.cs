// MIT License
// 
// Copyright (c) 2025-2025 Hexagon Software LLC
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Reflection;

namespace ThoughtSharp.Scenarios.Model;

public class AssemblyParser
{
  public ScenariosModel Parse(Assembly LoadedAssembly)
  {
    return new([
      ..LoadedAssembly.GetExportedTypes().Select(ParseType)
    ]);
  }

  static ScenariosModelNode ParseType(Type Type)
  {
    return ParseDirectory(Type);
  }

  static DirectoryNode ParseDirectory(Type Type)
  {
    return new(Type.FullName!, ParseMembers(Type));
  }

  static IEnumerable<ScenariosModelNode> ParseMembers(Type Type)
  {
    return Type.GetNestedTypes().Select(ParseDirectoryMemberType);
  }

  static ScenariosModelNode ParseDirectoryMemberType(Type Type)
  {
    var Attributes = Type.GetCustomAttributes();

    if (Attributes.Any(A => A is CurriculumAttribute))
      return new CurriculumNode(Type, []);

    if (Attributes.Any(A => A is CapabilityAttribute))
      return new CapabilityNode(Type, ParseMembers(Type));

    return new DirectoryNode(Type.Name, ParseMembers(Type));
  }
}