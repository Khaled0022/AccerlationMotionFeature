using System.Reflection;

namespace TestProject;

internal class PrivateObject
{
    private readonly object _obj;
    private readonly Type _type;

    public PrivateObject(object obj)
    {
        _obj = obj;
        _type = obj.GetType();
    }

    public object? Invoke(string methodName, params object[] args)
    {
        MethodInfo? info = _type.GetMethod(methodName, BindingFlags.NonPublic);

        if (info == null)
        {
            throw new ArgumentException($"Method '{methodName}' not found in class '{_type}'");
        }
        return info.Invoke(_obj, args);
    }

    public object? InvokeStatic(string methodName, params object[] args)
    {
        MethodInfo? info = _type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);

        if (info == null)
        {
            throw new ArgumentException($"Method '{methodName}' not found in class '{_type}'");
        }
        return info.Invoke(null, args);
    }

    public object? GetFieldOrProperty(string propertyName)
    {
        FieldInfo? field = _type.GetField(propertyName, BindingFlags.NonPublic);
        if (field != null) return field.GetValue(_obj);

        PropertyInfo? prop = _type.GetProperty(propertyName, BindingFlags.NonPublic);
        if (prop != null) return prop.GetValue(_obj);

        throw new ArgumentException($"No field or property named '{propertyName}' found in class '{_type}'");
    }

    public void SetFieldOrProperty(string propertyName, object value)
    {
        FieldInfo? field = _type.GetField(propertyName, BindingFlags.NonPublic);

        if (field != null)
        {
            field.SetValue(_obj, value);
            return;
        }
        PropertyInfo? prop = _type.GetProperty(propertyName, BindingFlags.NonPublic);

        if (prop != null)
        {
            prop.SetValue(_obj, value);
            return;
        }
        throw new ArgumentException($"No field or property named '{propertyName}' found in class '{_type}'");
    }
}
