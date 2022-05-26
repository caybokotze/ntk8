using Ntk8.Models;

namespace Ntk8.DatabaseServices;

public interface INtk8Commands
{
    void DeleteRolesForUserById(int id);
    void DeleteUserById(int id);
    long InsertRefreshToken(RefreshToken refreshToken);
    int InsertRole(Role role);
    int InsertUser(IBaseUser user);
    void InsertUserAndRole(IBaseUser user, Role role);
    int InsertUserRole(UserRole userRole);
    void InvalidateRefreshToken(RefreshToken refreshToken);
    void UpdateRefreshToken(RefreshToken refreshToken);
    void UpdateUser(IBaseUser user);
}

public class Ntk8Commands : INtk8Commands
{
    public void DeleteRolesForUserById(int id)
    {
        throw new System.NotImplementedException();
    }

    public void DeleteUserById(int id)
    {
        throw new System.NotImplementedException();
    }

    public long InsertRefreshToken(RefreshToken refreshToken)
    {
        throw new System.NotImplementedException();
    }

    public int InsertRole(Role role)
    {
        throw new System.NotImplementedException();
    }

    public int InsertUser(IBaseUser user)
    {
        throw new System.NotImplementedException();
    }

    public void InsertUserAndRole(IBaseUser user, Role role)
    {
        throw new System.NotImplementedException();
    }

    public int InsertUserRole(UserRole userRole)
    {
        throw new System.NotImplementedException();
    }

    public void InvalidateRefreshToken(RefreshToken refreshToken)
    {
        throw new System.NotImplementedException();
    }

    public void UpdateRefreshToken(RefreshToken refreshToken)
    {
        throw new System.NotImplementedException();
    }

    public void UpdateUser(IBaseUser user)
    {
        throw new System.NotImplementedException();
    }
}