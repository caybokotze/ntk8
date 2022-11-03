using Ntk8.DatabaseServices;
using Ntk8.Tests.TestHelpers;

namespace Ntk8.Tests.TestModels;

public class TestQueryType : IUserQueries
{
    public TestUserEntity? FetchUserById(int id)
    {
        throw new System.NotImplementedException();
    }

    public TestUserEntity? FetchUserByEmailAddress(string emailAddress)
    {
        throw new System.NotImplementedException();
    }

    public TestUserEntity? FetchUserByRefreshToken(string refreshToken)
    {
        throw new System.NotImplementedException();
    }

    public TestUserEntity? FetchUserByResetToken(string resetToken)
    {
        throw new System.NotImplementedException();
    }

    public TestUserEntity? FetchUserByVerificationToken(string verificationToken)
    {
        throw new System.NotImplementedException();
    }
}