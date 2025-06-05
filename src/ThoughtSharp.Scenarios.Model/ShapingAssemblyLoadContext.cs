using System.Reflection;
using System.Runtime.Loader;
using ThoughtSharp.Runtime;

namespace ThoughtSharp.Scenarios.Model;

public class ShapingAssemblyLoadContext(string DependencyPath) : AssemblyLoadContext(isCollectible: false)
{
  protected override Assembly? Load(AssemblyName AssemblyName)
  {
    Console.WriteLine($"Attempting to resolve: {AssemblyName.FullName}");
    var Name = AssemblyName.Name;
    if (
      Name == typeof(CurriculumAttribute).Assembly.GetName().Name ||
      Name == typeof(MindAttribute).Assembly.GetName().Name)
      return null;

    var AssemblyPath = Path.Combine(DependencyPath, $"{Name}.dll");
    if (File.Exists(AssemblyPath))
      return LoadFromAssemblyPath(AssemblyPath);

    Console.WriteLine($"Couldn't find the file at {DependencyPath}");

    return null;
  }

  protected override IntPtr LoadUnmanagedDll(string UnmanagedDllName)
  {
    Console.WriteLine($"Attempting to resolve unmanaged: {UnmanagedDllName}");
    var DllPath = Path.Combine(DependencyPath, $"{UnmanagedDllName}.dll");
    
    if (File.Exists(DllPath))
      return LoadUnmanagedDllFromPath(DllPath);

    Console.WriteLine($"Couldn't find the file at {DllPath}");

    return base.LoadUnmanagedDll(UnmanagedDllName);
  }
}