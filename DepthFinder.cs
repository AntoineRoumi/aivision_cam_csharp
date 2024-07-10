using Python.Runtime;

class Coords3D 
{
    public float X { get; }
    public float Y { get; }
    public float Z { get; }

    public Coords3D(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}

class BoundingBox
{
    public float X0 { get; }
    public float Y0 { get; }
    public float X1 { get; }
    public float Y1 { get; }

    public BoundingBox(float x0, float y0, float x1, float y1)
    {
        X0 = x0;
        Y0 = y0;
        X1 = x1;
        Y1 = y1;
    }
}

class Result
{
    public Coords3D? Coords { get; }
    public float? Distance { get; }
    public BoundingBox Bbox { get; } = default!;
    public string ClassName { get; } = default!;
    public string ColorName { get; } = default!;

    public Result(Coords3D? coords, float? distance, BoundingBox bbox, string className, string colorName)
    {
        Coords = coords;
        Distance = distance;
        Bbox = bbox;
        ClassName = className;
        ColorName = colorName;
    }
}

class DepthFinder
{
    private int Width, Height, Fps;
    private string Weights = string.Empty;
    private Py.GILState GIL = default!;
    private dynamic DepthFinderModule = null!;
    private dynamic DepthFinderInstance = null!;

    public DepthFinder(int width = 1280, int height = 720, int fps = 30, string weights = "yolov8s.pt")
    {
        Width = width;
        Height = height;
        Fps = fps;
        Weights = weights;
        InitPython();
        InitObject();
    }

    public List<Result> GetVisibleObjects()
    {
        try {
            dynamic obj = DepthFinderInstance.visible_objects;
            PyList visibleObjects = PyList.AsList(obj);
            List<Result> results = new List<Result>();
            foreach (dynamic result in visibleObjects)
            {
                Coords3D? coords;
                float? distance;
                if (result.coords != null) {
                    PyTuple pycoords = PyTuple.AsTuple(result.coords);
                    coords = new Coords3D(pycoords.GetItem(0).As<float>(), pycoords.GetItem(1).As<float>(), pycoords.GetItem(2).As<float>());
                    PyFloat pydistance = PyFloat.AsFloat(result.distance);
                    distance = pydistance.As<float>();
                } else {
                    coords = null;
                    distance = null;
                }

                PyTuple pybbox = PyTuple.AsTuple(result.bbox);
                Console.WriteLine(pybbox.GetItem(0).GetPythonType());
                BoundingBox bbox = new BoundingBox(pybbox.GetItem(0).As<int>(), pybbox.GetItem(1).As<int>(),
                        pybbox.GetItem(2).As<int>(), pybbox.GetItem(3).As<int>());

                string className = result.class_name.ToString();
                string colorName = result.color.ToString();

                results.Add(new Result(coords, distance, bbox, className, colorName));
            }
            return results;
        } catch (PythonException e) {
            Console.WriteLine("Error in GetResults: {0}", e.Message);
            return new List<Result>();
        }
    }

    // Initialization and termination of Python

    private void InitPython()
    {
        Runtime.PythonDLL = "/usr/lib/x86_64-linux-gnu/libpython3.10.so";
        PythonEngine.Initialize();
        GIL = Py.GIL();
        Console.WriteLine("Initializing");
    }

    public void Terminate()
    {
        try {
            DepthFinderInstance.terminate();
        } catch (PythonException e) {
            Console.WriteLine("Error in Terminate: {0}", e.Message);
        }
        Console.WriteLine("Terminating");
        GIL.Dispose();
        AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", true);
        PythonEngine.Shutdown();
        AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", false);
    }

    // __init__ wrapper

    private void InitObject()
    {
        DepthFinderModule = Py.Import("aivision_cam.depth_finder");
        try {
            DepthFinderInstance = DepthFinderModule.DepthFinder(Width, Height, Fps, Weights);
        } catch (PythonException e) {
            Console.WriteLine("Error in Init: {0}", e.Message);
        }
    }

    // Public API wrapper
    
    public void Update()
    {
        try {
            DepthFinderInstance.update();
        } catch (PythonException e) {
            Console.WriteLine("Error in Update: {0}", e.Message);
        }
    }

    // Use Py.kw(key1, value1, key2, value2, ...) to pass arguments
    public void Update(Py.KeywordArguments kwargs) 
    {
        try {
            DepthFinderInstance.update(kwargs);
        } catch (PythonException e) {
            Console.WriteLine("Error in Update: {0}", e.Message);
        }
    }

    public List<string> GetClassesNames()
    {
        List<string> classesNamesList = new List<string>();
        try {
            PyList classesNames = DepthFinderInstance.get_classes_names();
            foreach (var className in classesNames)
            {
                if (PyString.IsStringType(className)) {
                    classesNamesList.Add(className.ToString()!);
                } else {
                    classesNamesList.Add("");
                }
            }
        } catch (PythonException e) {
            Console.WriteLine("Error in GetClassesNames: {0}", e.Message);
        }
        return classesNamesList;
    }

    public int? GetIdFromClassName(string className) {
        try {
            PyInt? id = DepthFinderInstance.get_id_from_class_name(className);
            return id?.ToInt32();
        } catch (PythonException e) {
            Console.WriteLine("Error in GetIdFromClassName: {0}", e.Message);
            return null;
        }
    }
    
    public Coords3D? FindObjectByNameAndColor(string className, string color)
    {
        try {
            PyObject? obj = DepthFinderInstance.find_object_by_name_and_color(className, color);
            if (obj == null)
                return null;
            PyTuple coords = PyTuple.AsTuple(obj);
            return new Coords3D(coords.GetItem(0).As<float>(), coords.GetItem(1).As<float>(), coords.GetItem(2).As<float>());
        } catch (PythonException e) {
            Console.WriteLine("Error in FindObjectByNameAndColor: {0}", e.Message);
            return null;
        }
    }

    public Coords3D? FindObjectByIdAndColor(int id, string color)
    {
        try {
            PyObject? obj = DepthFinderInstance.find_object_by_id_and_color(id, color);
            if (obj == null)
                return null;
            PyTuple coords = PyTuple.AsTuple(obj);
            return new Coords3D(coords.GetItem(0).As<float>(), coords.GetItem(1).As<float>(), coords.GetItem(2).As<float>());
        } catch (PythonException e) {
            Console.WriteLine("Error in FindObjectByIdAndColor: {0}", e.Message);
            return null;
        }
    }
    
    public Coords3D? FindObjectByName(string className)
    {
        try {
            PyObject? obj = DepthFinderInstance.find_object_by_name(className);
            if (obj == null)
                return null;
            PyTuple coords = PyTuple.AsTuple(obj);
            return new Coords3D(coords.GetItem(0).As<float>(), coords.GetItem(1).As<float>(), coords.GetItem(2).As<float>());
        } catch (PythonException e) {
            Console.WriteLine("Error in FindObjectByName: {0}", e.Message);
            return null;
        }
    }

    public Coords3D? FindObjectById(int id)
    {
        try {
            PyObject? obj = DepthFinderInstance.find_object_by_id(id);
            if (obj == null)
                return null;
            PyTuple coords = PyTuple.AsTuple(obj);
            return new Coords3D(coords.GetItem(0).As<float>(), coords.GetItem(1).As<float>(), coords.GetItem(2).As<float>());
        } catch (PythonException e) {
            Console.WriteLine("Error in FindObjectById: {0}", e.Message);
            return null;
        }
    }

    public string GetColorOfBox(BoundingBox bbox)
    {
        try {
            PyObject obj = DepthFinderInstance.get_color_of_box_xyxy(bbox.X0, bbox.Y0, bbox.X1, bbox.Y1);
            return obj.As<string>().ToString();
        } catch (PythonException e) {
            Console.WriteLine("Error in GetColorOfBox: {0}", e.Message);
            return string.Empty;
        }
    }

    public string GetColorOfBoxXYXY(int x0, int y0, int x1, int y1)
    {
        return GetColorOfBox(new BoundingBox(x0, y0, x1, y1));
    }

    
}
