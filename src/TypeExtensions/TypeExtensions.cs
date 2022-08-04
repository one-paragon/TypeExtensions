using System.Reflection;

namespace OneParagon;

public static class TypeExtensions
{

    public static bool IsGenericOfCore(this Type type, Type genericType)
        => type.IsGenericType && type.GetGenericTypeDefinition() == genericType;

    public static bool IsGenericOf(this Type type, Type genericType)
        => type.IsGenericOfCore(genericType) ||
            type.BaseType!.IsGenericOf(genericType) ||
            type.GetInterfaces().Any(x => x.IsGenericOfCore(genericType));

    public static bool ImplementsInterface(this Type type, Type interfaceType)
        => type.GetInterface(interfaceType.Name) != null;

    public static Type? GetCommonInterface(this Type t1, Type t2)
        => t2.GetInterfaces().FirstOrDefault(i =>
            !t1.BaseType!.GetInterfaces().Contains(i) && t1.GetInterfaces().Contains(i));

    public static bool IsCollectionProperty(this PropertyInfo property) =>
        property.PropertyType.IsGenericOf(typeof(ICollection<>));

    public static HashSet<Type> simpleTypes = new HashSet<Type>
        {
            typeof(Enum),
            typeof(String),
            typeof(Decimal),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(Guid),
        };

    public static HashSet<Type> nonSimpleTypes = new HashSet<Type>();


    public static bool IsSimpleType(this Type type)
    {
        if (type.IsPrimitive || simpleTypes.Contains(type))
        {
            return true;
        }

        if (nonSimpleTypes.Contains(type))
        {
            return false;
        }

        var res = Convert.GetTypeCode(type) != TypeCode.Object ||
                  (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                   IsSimpleType(type.GetGenericArguments()[0]));

        if (res)
        {
            simpleTypes.Add(type);
        }
        else
        {
            nonSimpleTypes.Add(type);
        }

        return res;
    }

    /// <summary>
    /// Method (referenced by methodName) must be public
    /// </summary>
    public static Object InvokeGeneric<T>(string methodName, Type type, object[] args)
    {
        var method = typeof(T).GetMethod(methodName);
        var genericMethod = method.MakeGenericMethod(new Type[] { type });
        return genericMethod.Invoke(null, args);
    }

    public static Object InvokeGeneric(this object src, string methodName, Type[] genericTypes,
        params object[] args)
    {
        var t = src.GetType();
        var method = FindMatchingGenericMethod(t, methodName, args, genericTypes.Length);
        if (method == null) throw new InvalidOperationException("Matching method not found");
        var genericMethod = method.MakeGenericMethod(genericTypes);
        return genericMethod.Invoke(src, args);
    }

    public static MethodInfo FindMatchingGenericMethod(Type t, string methodName, object[] args,
        int? genericCount = 1)
    {
        if (args.Any(arg => (arg is Missing)))
            throw new NotSupportedException("Missing optional parameters not supported.");

        return t.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
            .SingleOrDefault(methodInfo => methodInfo.Name == methodName &&
                                           methodInfo.IsGenericMethod &&
                                           methodInfo.GetGenericArguments().Count() == genericCount &&
                                           ParamsMatch(methodInfo, args));
    }

    private static bool ParamsMatch(MethodInfo info, object[] args)
    {
        var @params = info.GetParameters();
        if (@params.Length != args.Length) return false;
        return @params.Zip(args).All((a) => a.Item1.ParameterType.IsAssignableFrom(a.Item2.GetType()));
    }

    public static async Task<object> InvokeGenericAsync(this object src, string methodName, Type[] genericTypes,
        params object[] args)
    {
        var task = (Task)InvokeGeneric(src, methodName, genericTypes, args);
        await task;
        var prop = task.GetType().GetProperty("Result");
        return prop.GetValue(task);
    }
}
