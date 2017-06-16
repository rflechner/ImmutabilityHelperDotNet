using System;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ImmutabilityHelperDotNet.Example.Domain;

namespace ImmutabilityHelperDotNet.Example.ConsoleApp
{
    public class Md5VsSha256
    {
        private const int N = 10000;
        private readonly byte[] data;

        private readonly SHA256 sha256 = SHA256.Create();
        private readonly MD5 md5 = MD5.Create();

        public Md5VsSha256()
        {
            data = new byte[N];
            new Random(42).NextBytes(data);
        }

        [Benchmark]
        public byte[] Sha256() => sha256.ComputeHash(data);

        [Benchmark]
        public byte[] Md5() => md5.ComputeHash(data);
    }

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



            for (int i = 0; i < 100; i++)
            {
                var d2 = d1
                    .With(d => d.Name, "tutu")
                    .With(d => d.Age, 45)
                    .Clone();
            }


            //var summary = BenchmarkRunner.Run<Md5VsSha256>();
            //var summary = BenchmarkRunner.Run<ImmutabilityHelperVsManualCopy>();

            Console.WriteLine("Press any key to quit ...");
            Console.ReadKey(true);
        }
    }
}
