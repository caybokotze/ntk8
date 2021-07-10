namespace Dispatch.K8.Dto
{
    public interface IRequiredUserAttributes
    {
        string Email { get; set; }
        string Name { get; set; }
        string Surname { get; set; }
        bool AcceptedTerms { get; set; }
    }
}