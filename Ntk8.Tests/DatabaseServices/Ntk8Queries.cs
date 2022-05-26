using Dapper.CQRS;
using Ntk8.Data.Queries;
using Ntk8.Models;

namespace Ntk8.Tests.DatabaseServices;

public interface INtk8Queries<out T> where T : class, IBaseUser, new()
{
    public T? FetchUserById(int id);
    public T? FetchUserByEmailAddress(string emailAddress);
    public T? FetchUserByRefreshToken(string refreshToken);
    public T? FetchUserByResetToken(string resetToken);
    public T? FetchUserByVerificationToken(string verificationToken);
}

public class Ntk8Queries<T> : INtk8Queries<T> where T : class, IBaseUser, new()
{
    private readonly IQueryExecutor _queryExecutor;

    public Ntk8Queries(IQueryExecutor queryExecutor)
    {
        _queryExecutor = queryExecutor;
    }
    
    public T? FetchUserById(int id)
    {
        return _queryExecutor.Execute(new FetchUserById<T>(id));
    }

    public T? FetchUserByEmailAddress(string emailAddress)
    {
        return _queryExecutor.Execute(new FetchUserByEmailAddress<T>(emailAddress));
    }

    public T? FetchUserByRefreshToken(string refreshToken)
    {
        return _queryExecutor.Execute(new FetchUserByRefreshToken<T>(refreshToken));
    }

    public T? FetchUserByResetToken(string resetToken)
    {
        return _queryExecutor.Execute(new FetchUserByResetToken<T>(resetToken));
    }

    public T? FetchUserByVerificationToken(string verificationToken)
    {
        return _queryExecutor.Execute(new FetchUserByVerificationToken<T>(verificationToken));
    }
}