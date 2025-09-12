namespace DesignPattern.Visitor;

public interface IVisitor
{
    void Visit(Book book);
    void Visit(Dvd dvd);
}