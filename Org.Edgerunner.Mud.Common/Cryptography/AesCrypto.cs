#region BSD 3-Clause License
// <copyright company="Edgerunner.org" file="AesCrypto.cs">
// Copyright (c)  2022
// </copyright>
//
// This is a modified form of some sample code shared on Stack
// I cannot take credit for it, other than the small changes.
//
// BSD 3-Clause License
//
// Copyright (c) 2022,
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice, this
//    list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
//
// 3. Neither the name of the copyright holder nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion

using System.Security.Cryptography;
using System.Text;

namespace Org.Edgerunner.Mud.Common.Cryptography;

/// <summary>
/// Class that performs AES encryption/decryption.
/// </summary>
public static class AesCrypto
{
    private static readonly byte[] _Salt = Encoding.ASCII.GetBytes("Q#$aGVw4Qfq4r234vsZ&K7dsVa2$");

    /// <summary>
    /// Encrypt the given string using AES.  The string can be decrypted using
    /// Decrypt().  The sharedSecret parameters must match.
    /// </summary>
    /// <param name="plainText">The text to encrypt.</param>
    /// <param name="sharedSecret">A password used to generate a key for encryption.</param>
    public static string Encrypt(string plainText, string sharedSecret)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentNullException(nameof(plainText));
        if (string.IsNullOrEmpty(sharedSecret))
            throw new ArgumentNullException(nameof(sharedSecret));

        string outStr;
        Aes? aesAlg = null;

        try
        {
            // generate the key from the shared secret and the salt
            var key = new Rfc2898DeriveBytes(sharedSecret, _Salt);

            // Create a Aes object
            aesAlg = Aes.Create();
            aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

            // Create a decryptor to perform the stream transform.
            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for encryption.
            using var msEncrypt = new MemoryStream();
            // prepend the IV
            msEncrypt.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int));
            msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                using var swEncrypt = new StreamWriter(csEncrypt);
                //Write all data to the stream.
                swEncrypt.Write(plainText);
            }
            outStr = Convert.ToBase64String(msEncrypt.ToArray());
        }
        finally
        {
            // Clear the Aes object.
            aesAlg?.Clear();
        }

        // Return the encrypted bytes from the memory stream.
        return outStr;
    }

    /// <summary>
    /// Decrypt the given string.  Assumes the string was encrypted using
    /// Encrypt(), using an identical sharedSecret.
    /// </summary>
    /// <param name="cipherText">The text to decrypt.</param>
    /// <param name="sharedSecret">A password used to generate a key for decryption.</param>
    public static string? Decrypt(string cipherText, string sharedSecret)
    {
        if (string.IsNullOrEmpty(cipherText))
            throw new ArgumentNullException(nameof(cipherText));
        if (string.IsNullOrEmpty(sharedSecret))
            throw new ArgumentNullException(nameof(sharedSecret));

        // Declare the Aes object
        // used to decrypt the data.
        Aes? aesAlg = null;

        // Declare the string used to hold
        // the decrypted text.
        string plaintext;

        try
        {
            // generate the key from the shared secret and the salt
            var key = new Rfc2898DeriveBytes(sharedSecret, _Salt);

            // Create the streams used for decryption.
            var bytes = Convert.FromBase64String(cipherText);
            using var msDecrypt = new MemoryStream(bytes);
            // Create a Aes object
            // with the specified key and IV.
            aesAlg = Aes.Create();
            aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
            // Get the initialization vector from the encrypted stream
            aesAlg.IV = ReadByteArray(msDecrypt);
            // Create a decryptor to perform the stream transform.
            var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            plaintext = srDecrypt.ReadToEnd();
        }
        catch (FormatException)
        {
            return null;
        }
        finally
        {
            // Clear the Aes object.
            aesAlg?.Clear();
        }

        return plaintext;
    }

    private static byte[] ReadByteArray(Stream s)
    {
        var rawLength = new byte[sizeof(int)];
        if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
            throw new SystemException("Stream did not contain properly formatted byte array");

        var buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
        if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
            throw new SystemException("Did not read byte array properly");

        return buffer;
    }
}