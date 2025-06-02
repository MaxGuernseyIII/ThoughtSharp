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
    Dictionary<string, List<ScenariosModelNode>> RootDirectories = [];

    foreach (var Type in LoadedAssembly.GetExportedTypes())
    {
      if (!IsThoughtSharpTrainingType(Type))
        continue;

      var NamespaceDirectoryName = Type.Namespace!;
      if (!RootDirectories.TryGetValue(NamespaceDirectoryName, out var List))
        RootDirectories[NamespaceDirectoryName] = List = new();

      List.Add(ParseType(Type));
    }

    return new([
      ..RootDirectories.Select(Pair => new DirectoryNode(Pair.Key, Pair.Value))
    ]);
  }

  bool IsThoughtSharpTrainingType(Type Type)
  {
    return IsCurriculumType(Type) || IsCapabilityType(Type) || Type.GetNestedTypes().Any(IsThoughtSharpTrainingType);
  }

  static IEnumerable<ScenariosModelNode> ParseTypes(Type Type)
  {
    return Type.GetNestedTypes().Select(ParseType);
  }

  static ScenariosModelNode ParseType(Type Type)
  {
    if (IsCurriculumType(Type))
      return new CurriculumNode(Type, []);

    if (IsCapabilityType(Type))
      return ParseCapabilityType(Type);

    if (IsMindPlaceType(Type))
      return new MindPlaceNode(Type);

    return new DirectoryNode(Type.Name, ParseTypes(Type));
  }

  static CapabilityNode ParseCapabilityType(Type Type)
  {
    return new(Type, 
      [
        ..ParseTypes(Type),
        ..ParseBehaviors(Type)
      ]);
  }

  static IEnumerable<ScenariosModelNode> ParseBehaviors(Type Type)
  {
    return Type.GetMethods().Where(IsValidBehaviorMethod).Select(T => new BehaviorNode(Type, T));
  }

  static bool IsValidBehaviorMethod(MethodInfo M)
  {
    return M is {IsStatic: false, IsPublic: true } && (M.ReturnType == typeof(void) || M.ReturnType == typeof(Task));
  }

  static bool IsMindPlaceType(Type Type)
  {
    return HasAttribute<MindPlaceAttribute>(Type);
  }

  static bool IsCapabilityType(Type Type)
  {
    return HasAttribute<CapabilityAttribute>(Type);
  }

  static bool IsCurriculumType(Type Type)
  {
    return HasAttribute<CurriculumAttribute>(Type);
  }

  static bool HasAttribute<T>(Type Type)
  {
    return Type.GetCustomAttributes().Any(A => A is T);
  }
}