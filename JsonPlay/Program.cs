using System;
using System.Reflection;


// 获取 MyClass 的类型信息
Type myClassType = typeof(MyClass);

// 创建 MyClass 的对象实例
object myClassInstance = Activator.CreateInstance(myClassType);

// 获取 Add 方法信息
MethodInfo addMethod = myClassType.GetMethod("Add");

// 调用 Add 方法并传递参数
int result = (int)addMethod.Invoke(myClassInstance, new object[] { 5, 3 });
Console.WriteLine("Add 方法结果：" + result);

// 获取 SayHello 方法信息
MethodInfo sayHelloMethod = myClassType.GetMethod("SayHello");

// 调用 SayHello 方法并传递参数
string helloMessage = (string)sayHelloMethod.Invoke(myClassInstance, new object[] { "Alice" });
Console.WriteLine("SayHello 方法结果：" + helloMessage);

// 获取 SayHello 方法的参数信息
ParameterInfo[] sayHelloParameters = sayHelloMethod.GetParameters();
Console.WriteLine("SayHello 方法的参数个数：" + sayHelloParameters.Length);
foreach (var parameter in sayHelloParameters)
{
    Console.WriteLine($"参数名：{parameter.Name}，参数类型：{parameter.ParameterType}");
}


public class MyClass
{
    public int Add(int a, int b)
    {
        return a + b;
    }

    public string SayHello(string name)
    {
        return $"Hello, {name}!";
    }
}