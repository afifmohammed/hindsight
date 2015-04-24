namespace MediatR.Extras
{
    public interface ICorrelated
    {
        string CorrelationId { get; }
    }
}
