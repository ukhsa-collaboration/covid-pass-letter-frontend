using System;
using System.Security.Cryptography;

namespace CovidLetter.Frontend.Queue.Utils;

public static class Checksum
{
    public static string Sha256(byte[] content)
    {
        return Convert.ToBase64String(SHA256.HashData(content));
    }
}
