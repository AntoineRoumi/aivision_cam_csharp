# AI Vision Depth Camera library wrapper for C#

## Installation 

First of all, you need to run the following command to install the Python module :

```pip install -e aivision_cam```

Then, you can build the project using .NET :

```dotnet build .```

This will generate a DLL file that you can then use in your C# project.

## Usage warnings

The public interface uses only C# types, except for the DepthFinder.Update that takes a PyDict from the Python.NET library as an argument, to replicate the **kwargs argument of the Python library.

To create a PyDict with the desired named arguments, all you need to use is:

```csharp
// Top of the C# file
using Python.Runtime;

...
  // In a method
  depthFinder.Update(Py.kw(key1, value1, key2, value2, ...);
...
```

You will maybe need to install the Python.NET library, if so use the following command:

```dotnet add package pythonnet --version 3.0.3```
