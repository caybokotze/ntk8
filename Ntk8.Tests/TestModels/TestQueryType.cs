using Ntk8.DatabaseServices;
using Ntk8.Models;

namespace Ntk8.Tests.TestModels;

public class TestQueryType : IAccountQueries
{
    public T? FetchUserById<T>(int id) where T : class, IUserEntity, new()
    {
        throw new System.NotImplementedException();
    }

    public T? FetchUserByEmailAddress<T>(string emailAddress) where T : class, IUserEntity, new()
    {
        throw new System.NotImplementedException();
    }

    public T? FetchUserByRefreshToken<T>(string refreshToken) where T : class, IUserEntity, new()
    {
        throw new System.NotImplementedException();
    }

    public T? FetchUserByResetToken<T>(string resetToken) where T : class, IUserEntity, new()
    {
        throw new System.NotImplementedException();
    }

    public T? FetchUserByVerificationToken<T>(string verificationToken) where T : class, IUserEntity, new()
    {
        throw new System.NotImplementedException();
    }
}