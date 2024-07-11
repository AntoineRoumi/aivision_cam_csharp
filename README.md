# AI Vision Depth Camera library wrapper for C#

## Build 

To setup all the dependencies, simply run the `setup.sh`. It will download the [Python package](https://github.com/AntoineRoumi/object-detection), install it with pip and copy the training dataset for color recognition in the current directory. It will also install Python.NET for C#.

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
