
# Immutability in C#

Here is a POC trying use a syntax like F#:

```fsharp

type Guy = { Name:string; Age:int }

let jean = {Name="Jean"; Age=20}
let jeanOlder = { jean with Age=21 }

```
In C#:

```csharp

public class Guy
{
  public Guy(string name, int age)
  {
    Name = name;
    Age = age
  }
  
  public string Name { get; }
  public int Age { get; }
}

var jean = new Guy("Jean", 20);
var jeanOlder = jean.With(d => d.Age, 45).Clone();

```

