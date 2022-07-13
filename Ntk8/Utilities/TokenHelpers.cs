using System;
using System.Security.Cryptography;

namespace Ntk8.Utilities;

public static class TokenHelpers
{
    public static string GenerateCryptoRandomToken(int length = 40)
    {
        var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
        var randomBytes = new byte[length];
        rngCryptoServiceProvider.GetBytes(randomBytes);
        return BitConverter.ToString(randomBytes).Replace("-", "");
    }
}