namespace Evan.Dynamic
{
    public interface IObjectProxy<out T>
    {
        public T Object { get; }
    }
}