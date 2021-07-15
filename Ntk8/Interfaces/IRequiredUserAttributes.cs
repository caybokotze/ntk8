namespace Ntk8.Interfaces
{
    public interface IRequiredUserAttributes
    {
        string Email { get; set; }
        string Name { get; set; }
        string Surname { get; set; }
        bool AcceptedTerms { get; set; }
    }
}