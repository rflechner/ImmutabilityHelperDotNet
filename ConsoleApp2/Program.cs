using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Domain;

namespace ConsoleApp2
{
    class Program
    {
        private static string RemoveAccent(string txt)
        {
            byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(txt); //Tailspin uses Cyrillic (ISO-8859-5); others use Hebraw (ISO-8859-8)
            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        static void Main(string[] args)
        {
            var d1 = new DebtorModel("Toto", 42);

            var d2 = d1

                .With(d => d.Name, "tutu");
                //.With(d => d.Age, 45);

            //var d2 = With(d1, s => s.Name, "Tata");
            //var d3 = d1.With(d => d.Name, "Tutu");
            //var fieldAccessor = BaseModel<DebtorModel>.GetFieldAccessor<DebtorModel, string>("Name");
            //fieldAccessor("coucou");

            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            Console.WriteLine("codeBase:" + codeBase);

            Console.WriteLine(RemoveAccent("ô coucou mémé"));
            
            //var repository = new SellerStatsRepository();
            //foreach (var seller in repository.GetSellers())
            //{
            //    Console.WriteLine($"Seller {seller.Name} is {seller.Age} years old.");
            //}

            //foreach (var seller in repository.GetSellersHavingInvoiceCountOf(42))
            //{
            //    Console.WriteLine($"Seller {seller.Name} is {seller.Age} years old.");
            //}

            Console.ReadKey(true);
        }

        //public static T With<T, TP>(T source, Expression<Func<T, TP>> member, TP value) where T : ICloneable
        //{
        //    var target = (T)source.Clone();

            




        //    var newValue = Expression.Parameter(member.Body.Type);
        //    var assign = Expression.Lambda<Action<T, TP>>(
        //        Expression.Assign(member.Body, newValue),
        //        member.Parameters[0], newValue);

        //    var setter = assign.Compile();
        //    setter(target, value);

        //    return target;
        //}
    }

    public class SellerStatsRepository: RepositoryBase
    {
        public IList<SellerModel> GetSellersHavingInvoiceCountOf(int invoiceCount)
        {
            var param = new { invoiceCount };

            return ExecuteView(record => new SellerModel
            {
                Name = (string)record["Name"],
                Age = (int)record["Age"]
            }, param).ToList().AsReadOnly();
        }

        public IList<SellerModel> GetSellers()
        {
            return ExecuteView(record => new SellerModel
            {
                Name = (string) record["Name"],
                Age = (int) record["Age"]
            }).ToList().AsReadOnly();
        }
    }

    public abstract class RepositoryBase
    {

        public static IDictionary<string, object> ToDictionary(object o) 
            => o?.GetType()?.GetProperties()?.ToDictionary(member => member.Name, member => member.GetValue(o)) ?? new Dictionary<string, object>();

        [MethodImpl(MethodImplOptions.NoInlining)]
        //object is type of record set and builder contains logic of mapping
        //TODO: change Dictionary<string, object> type to ORM record type
        public IEnumerable<T> ExecuteView<T>(Func<Dictionary<string, object>, T> builder, object args = null)
        {
            var parameters = ToDictionary(args);
            var stackTrace = new StackTrace();
            var frame = stackTrace.GetFrame(1);
            var method = frame.GetMethod();
            var assembly = method.DeclaringType.Assembly;
            
            using (var stream = assembly.GetManifestResourceStream($"{method.DeclaringType.Namespace}.{method.Name}.sql"))
            {
                using (var reader = new StreamReader(stream))
                {
                    var sql = reader.ReadToEnd();
                    Console.WriteLine("Executing SQL {0}", sql);
                    Console.WriteLine("With params:");
                    foreach (var parameter in parameters)
                    {
                        Console.WriteLine($"- {parameter.Key}: {parameter.Value}");
                    }
                    // TODO: execute SQL passing parameters

                    var records = new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            { "Name", "nom 1" },
                            { "Age", 32 }
                        },
                        new Dictionary<string, object>
                        {
                            { "Name", "nom 2" },
                            { "Age", 35 }
                        }
                    };

                    return records.Select(builder);
                }
            }
        }
    }
}
