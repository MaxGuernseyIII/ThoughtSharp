using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Loader;
using ThoughtSharp.Runtime;

namespace ThoughtSharp.Scenarios.Model;

public class ShapingAssemblyLoadContext(
  string DependencyPath,
  IReadOnlyList<Assembly> ExtraBindingOverrideAssemblies) : AssemblyLoadContext(isCollectible: false)
{
  readonly AssemblyDependencyResolver Resolver = new(DependencyPath);
  protected override Assembly? Load(AssemblyName AssemblyName)
  {
    var Name = AssemblyName.Name;

    ImmutableArray<Assembly> InterfaceAssemblies =
    [
      typeof(CurriculumAttribute).Assembly,
      typeof(MindAttribute).Assembly,
      ..ExtraBindingOverrideAssemblies
    ];

    var InterfaceAssembly = InterfaceAssemblies.FirstOrDefault(A => A.GetName().Name == Name);
    if (InterfaceAssembly is not null)
      return InterfaceAssembly;

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