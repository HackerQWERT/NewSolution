public abstract class Animal
{
    public virtual string Name { get; set; }
}

public class Dog : Animal
{
    public override string Name
    {
        get { return "Woof!"; }
        set { }
    }
}