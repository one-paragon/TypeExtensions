using OneParagon;
using FluentAssertions;
using System;
using Xunit;
using static OneParagon.TypeExtensions;

public class TypeExtensionTests
{
    public record Type1();

    public record Type2();

    public TypeExtensionTests()
    {

    }
    public string TestMethod<T>()
    {
        return "Generic";
    }

    public string TestMethod<T, U>()
    {
        return "2Generic";
    }
    public string TestMethod()
    {
        return "NonGeneric";
    }

    public string TestMethod<T>(int testInt, bool testBool, Type1 t1)
    {
        return "GenericWith3Params_t1";

    }
    public string TestMethod<T, U>(int testInt, bool? testBool = null)
    {
        return "2GenericWith2Params";
    }
    public string TestMethod<T>(int testInt, bool? testBool = null)
    {
        return "GenericWith2Params";
    }
    public string TestMethod<T>(int testInt, bool testBool, Type2 t2)
    {
        return "GenericWith3Params_t2";
    }

    [Fact]
    public void FindMatchingGenericMethod_numberOFArgs_numberofGenericTypes()
    {
        Type t = typeof(TypeExtensionTests);
        var args = new object[] { 1, true };
        var test = OneParagon.TypeExtensions.FindMatchingGenericMethod(t, "TestMethod", args, 2);
        if (test == null) throw new InvalidOperationException("Matching method not found");
        var genericMethod = test.MakeGenericMethod(new Type[] { typeof(Type1), typeof(Type1) });
        var result = genericMethod.Invoke(this, args);
      //  result.ToString().Should().Be("2GenericWith2Params");

    }


    [Fact]
    public void FindMatchingGenericMethod_numberOFArgs()
    {
        Type t = typeof(TypeExtensionTests);
        var args = new object[] { 1, true };
        var test = FindMatchingGenericMethod(t, "TestMethod", args);
        if (test == null) throw new InvalidOperationException("Matching method not found");
        var genericMethod = test.MakeGenericMethod(new Type[] { typeof(Type1) });
        var result = genericMethod.Invoke(this, args);
        result.ToString().Should().Be("GenericWith2Params");

    }
    [Fact]
    public void FindMatchingGenericMethod_nullableArg()
    {
        Type t = typeof(TypeExtensionTests);
        var args = new object[] { 1, Type.Missing };
        Action act = () => { FindMatchingGenericMethod(t, "TestMethod", args); };
        act.Should().Throw<NotSupportedException>();

    }

    [Fact]
    public void FindMatchingGenericMethod_argTypes()
    {
        Type t = typeof(TypeExtensionTests);
        var args = new object[] { 1, true, new Type1() };
        var test = FindMatchingGenericMethod(t, "TestMethod", args);
        if (test == null) throw new InvalidOperationException("Matching method not found");
        var genericMethod = test.MakeGenericMethod(new Type[] { typeof(Type1) });
        var result = genericMethod.Invoke(this, args);
        result.ToString().Should().Be("GenericWith3Params_t1");
    }

    [Fact]
    public void FindMatchingGenericMethod_argTypesTypo()
    {
        Type t = typeof(TypeExtensionTests);
        var args = new object[] { 1, true, new Type1()};
        var test = FindMatchingGenericMethod(t, "TestMethodxxx", args);
        test.Should().BeNull();
    }

    [Fact]
    public void FindMatchingGenericMethod_argTypesInherited()
    {
        Type t = typeof(TypeExtensionTests);
        Type2 testModel = new Type2();
        var args = new object[] { 1, true, testModel };
        var test = FindMatchingGenericMethod(t, "TestMethod", args);
        if (test == null) throw new InvalidOperationException("Matching method not found");
        var genericMethod = test.MakeGenericMethod(new Type[] { typeof(Type1) });
        var result = genericMethod.Invoke(this, args);
        result.ToString().Should().Be("GenericWith3Params_t2");
    }

}