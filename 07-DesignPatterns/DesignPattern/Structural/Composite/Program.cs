using System;
using System.Collections.Generic;

// Componente
public interface IComponent
{
    void Display(int depth);
}

// Folha (Arquivo)
public class File : IComponent
{
    private string _name;

    public File(string name)
    {
        _name = name;
    }

    public void Display(int depth)
    {
        Console.WriteLine(new String('-', depth) + _name);
    }
}

// Composto (Pasta)
public class Folder : IComponent
{
    private string _name;
    private List<IComponent> _children = new List<IComponent>();

    public Folder(string name)
    {
        _name = name;
    }

    public void Add(IComponent component)
    {
        _children.Add(component);
    }

    public void Remove(IComponent component)
    {
        _children.Remove(component);
    }

    public void Display(int depth)
    {
        Console.WriteLine(new String('-', depth) + _name);

        foreach (var child in _children)
        {
            child.Display(depth + 2);
        }
    }
}

// Teste
class Program
{
    static void Main()
    {
        Folder root = new Folder("Root");
        root.Add(new File("File A"));
        root.Add(new File("File B"));

        Folder folder1 = new Folder("Folder 1");
        folder1.Add(new File("File C"));
        folder1.Add(new File("File D"));

        root.Add(folder1);

        Folder folder2 = new Folder("Folder 2");
        folder2.Add(new File("File E"));

        folder1.Add(folder2);

        root.Display(1);
    }
}
