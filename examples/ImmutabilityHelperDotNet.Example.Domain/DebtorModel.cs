namespace ImmutabilityHelperDotNet.Example.Domain
{
    public class DebtorModel : BaseModel<DebtorModel>
    {
        public DebtorModel(string name, int age)
        {
            Name = name;
            Age = age;
        }

        public string Name { get; }
        public int Age { get; }
    }
}