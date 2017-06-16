using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }

    //abstract class RepositoryBase
    //{
    //    [MethodImpl(MethodImplOptions.NoInlining)]
    //    public T ExecuteView<T>()
    //    {
    //        StackTrace stackTrace = new StackTrace();
    //        MethodBase methodBase = stackTrace.GetFrame(1).GetMethod();

    //        var assembly = GetType().GetTypeInfo().Assembly;
    //        assembly.GetManifestResourceStream(memberName);
    //    }
    //}


}
