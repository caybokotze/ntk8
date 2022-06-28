using Ntk8.DatabaseServices;
using Ntk8.Tests.TestHelpers;

namespace Ntk8.Tests.TestModels;

public class TestQueryType : INtk8Queries<TestUser>
{
    public TestUser? FetchUserById(int id)
    {
        throw new System.NotImplementedException();
    }

    public TestUser? FetchUserByEmailAddress(string emailAddress)
    {
        throw new System.NotImplementedException();
    }

    public TestUser? FetchUserByRefreshToken(string refreshToken)
    {
        throw new System.NotImplementedException();
    }

    public TestUser? FetchUserByResetToken(string resetToken)
    {
        throw new System.NotImplementedException();
    }

    public TestUser? FetchUserByVerificationToken(string verificationToken)
    {
        throw new System.NotImplementedException();
    }
}