namespace ThoughtSharp.Runtime;

public class Thought
{

    public static Thought<T> ForProduct<T>(T Product)
    {
        return new(Product);
    }
}

public class Thought<T>(T Product) : Thought
{
    public T Unwrap()
    {
        return Product;
    }
}