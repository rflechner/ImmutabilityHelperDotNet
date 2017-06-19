using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ImmutabilityHelperDotNet.Example.Domain;

namespace ImmutabilityHelperDotNet.Example.ConsoleApp
{
    public class ImmutabilityHelperVsManualCopy
    {
        DebtorModel d1 = new DebtorModel("Toto", 42);

        [Benchmark]
        public DebtorModel ManualCopy() => new DebtorModel(d1.Name + " older", d1.Age + 1);

        [Benchmark]
        public DebtorModel HelperCopy() => d1.With(d => d.Name, d1.Name + " older").With(d => d.Age, d1.Age + 1).Clone();
    }

    class Program
    {
        static void Main(string[] args)
        {
            var d1 = new DebtorModel("Toto", 42);



            for (int i = 0; i < 1000000; i++)
            {
                var d2 = d1
                    .With(d => d.Name, "tutu")
                    .With(d => d.Age, 45)
                    .Clone();
            }
            
            //var summary = BenchmarkRunner.Run<ImmutabilityHelperVsManualCopy>();

            Console.WriteLine("Press any key to quit ...");
            Console.ReadKey(true);
        }
    }
}
