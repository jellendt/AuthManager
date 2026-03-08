using System.Security.Cryptography;

class Program
{
    static void Main()
    {
        // 16 bytes * 8 bits/byte = 128 bits
        int keySize = 64; // 128 bits = 16 bytes
        byte[] key = new byte[keySize];

        using (RNGCryptoServiceProvider rng = new())
        {
            rng.GetBytes(key);
        }

        // Optional: Ausgabe des Schlüssels in Hex-Format
        Console.WriteLine("Generated 128-bit key:");
        Console.WriteLine(BitConverter.ToString(key).Replace("-", ""));

        // Optional: Speicherung des Schlüssels als Base64-String
        string base64Key = Convert.ToBase64String(key);
        Console.WriteLine("Base64 encoded key:");
        Console.WriteLine(base64Key);
    }
}
