using Dapper.CQRS;
using Ntk8.Data.Queries;
using Ntk8.Exceptions;
using Ntk8.Models;

namespace Ntk8.DatabaseServices;

public interface IUserQueries
{
    public T? FetchUserById<T>(int id) where T : class, IUserEntity, new();
    public T? FetchUserByEmailAddress<T>(string emailAddress) where T : class, IUserEntity, new();
    public T? FetchUserByRefreshToken<T>(string refreshToken) where T : class, IUserEntity, new();
    public T? FetchUserByResetToken<T>(string resetToken) where T : class, IUserEntity, new();
    public T? FetchUserByVerificationToken<T>(string verificationToken) where T : class, IUserEntity, new();
}

public class UserQueries : IUserQueries
{
    private readonly IQueryExecutor _queryExecutor;

    public UserQueries(IQueryExecutor queryExecutor)
    {
        _queryExecutor = queryExecutor;
    }
    
    public T? FetchUserById<T>(int id) where T : class, IUserEntity, new()
    {
        return _queryExecutor.Execute(new FetchUserById<T>(id));
    }

    public T? FetchUserByEmailAddress<T>(string emailAddress) where T : class, IUserEntity, new()
    {
        if (string.IsNullOrEmpty(emailAddress))
        {
            throw new InvalidEmailAddressException();
        }
        
        return _queryExecutor.Execute(new FetchUserByEmailAddress<T>(emailAddress));
    }

    public T? FetchUserByRefreshToken<T>(string refreshToken) where T : class, IUserEntity, new()
    {
        return _queryExecutor.Execute(new FetchUserByRefreshToken<T>(refreshToken));
    }

    public T? FetchUserByResetToken<T>(string resetToken) where T : class, IUserEntity, new()
    {
        return _queryExecutor.Execute(new FetchUserByResetToken<T>(resetToken));
    }

    public T? FetchUserByVerificationToken<T>(string verificationToken) where T : class, IUserEntity, new()
    {
        return _queryExecutor.Execute(new FetchUserByVerificationToken<T>(verificationToken));
    }
}