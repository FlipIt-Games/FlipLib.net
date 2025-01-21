using System.Numerics;

namespace FES.Tests;

public class PoolTests
{
    [Fact]    
    public void Ctor_Initialization() 
    {
        var pool = new Pool<Entity>(1000);
        Assert.Equal(pool.Size, 0);
        Assert.Equal(pool.Capacity, 1000);
    }

    [Fact]
    public void Add_Increases_Size_And_Preserve_Capacity() 
    {
        var pool = new Pool<int>(10);
        pool.Add(2);
        pool.Add(5);

        Assert.Equal(pool.Size, 2);
        Assert.Equal(pool.Capacity, 10);
    }

    [Fact]
    public void Remove_Decrease_Size_And_Preserve_Capacity() 
    {

        var pool = new Pool<int>(10);
        pool.Add(2);
        pool.Add(5);

        pool.Remove((Id<int>)(1));

        Assert.Equal(pool.Size, 1);
        Assert.Equal(pool.Capacity, 10);
    }

    [Fact]
    public void Get_Retrieve_Data()
    {
        var pool = new Pool<int>(10);
        pool.Add(2);
        pool.Add(5);

        var data = pool.Get((Id<int>)1);
        Assert.Equal(data, 5);
    }

    [Fact] 
    public void Foreach_Yield_Correct_Datas()
    {
        var pool = new Pool<int>(10);
        pool.Add(2);
        pool.Add(5);
        pool.Add(7);

        var res = new HashSet<int>();

        foreach (var i in pool) 
        {
            res.Add(i);
            Console.WriteLine(i);
        }

        Assert.True(res.Count == 3);
        Assert.True(res.Contains(2));
        Assert.True(res.Contains(5));
        Assert.True(res.Contains(7));
    }

    [Fact]
    public void Iterating_More_Than_Once() 
    {
        var pool = new Pool<int>(10);
        pool.Add(2);
        pool.Add(5);
        pool.Add(7);

        var res = new HashSet<int>();

        foreach (var i in pool) 
        {
            res.Add(i);
            Console.WriteLine(i);
        }

        Assert.True(res.Count == 3);
        Assert.True(res.Contains(2));
        Assert.True(res.Contains(5));
        Assert.True(res.Contains(7));

        res = new HashSet<int>(10);
        foreach (var i in pool) 
        {
            res.Add(i);
            Console.WriteLine(i);
        }

        Assert.True(res.Count == 3);
        Assert.True(res.Contains(2));
        Assert.True(res.Contains(5));
        Assert.True(res.Contains(7));
    }

    [Fact] 
    public void Foreach_Can_Add_While_Iterating()
    {
        var pool = new Pool<int>(10);
        pool.Add(2);
        pool.Add(5);

        var res = new HashSet<int>();

        foreach (var i in pool) 
        {
            res.Add(i);
            if (pool.Size == 2) 
            {
                pool.Add(7);
            }
        }

        Assert.True(res.Count == 3);
        Assert.True(res.Contains(2));
        Assert.True(res.Contains(5));
        Assert.True(res.Contains(7));
    }

    [Fact] 
    public void Foreach_Can_Remove_While_Iterating()
    {
        var pool = new Pool<int>(10);
        pool.Add(2);
        pool.Add(5);
        pool.Add(7);

        var res = new HashSet<int>();

        foreach (var i in pool) 
        {
            res.Add(i);
            if (pool.Size == 0) 
            {
                pool.Remove((Id<int>)0);
            }
        }

        Assert.True(res.Count == 3);
        Assert.True(res.Contains(2));
        Assert.True(res.Contains(5));
        Assert.True(res.Contains(7));
    }
}
