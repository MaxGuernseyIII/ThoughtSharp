using System.Reflection;
using System.Runtime.Loader;
using ThoughtSharp.Runtime;

namespace ThoughtSharp.Scenarios.Model;

public class ShapingAssemblyLoadContext(string DependencyPath) : AssemblyLoadContext(isCollectible: false)
{
  readonly AssemblyDependencyResolver Resolver = new(DependencyPath);
  protected override Assembly? Load(AssemblyName AssemblyName)
  {
    var Name = AssemblyName.Name;
    if (
      Name == typeof(CurriculumAttribute).Assembly.GetName().Name ||
      Name == typeof(MindAttribute).Assembly.GetName().Name)
      return null;

    //Console.WriteLine($"Attempting to resolve: {AssemblyName.FullName}");
    var Path = Resolver.ResolveAssemblyToPath(AssemblyName);

    return Path is not null ? LoadFromAssemblyPath(Path) : null;
  }

  protected override IntPtr LoadUnmanagedDll(string UnmanagedDllName)
  {
    //Console.WriteLine($"Attempting to resolve unmanaged: {UnmanagedDllName}");
    var DllPath = Resolver.ResolveUnmanagedDllToPath(UnmanagedDllName);

    if (DllPath is not null)
      return LoadUnmanagedDllFromPath(DllPath);

    return base.LoadUnmanagedDll(UnmanagedDllName);
  }
}