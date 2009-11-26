using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace Hubble.Framework.Security
{
    // 摘要:
    //     表示 System.Security.Cryptography.RSA 算法的标准参数。
    [Serializable]
    public struct InnerRSAParameters
    {
        // 摘要:
        //     表示 System.Security.Cryptography.RSA 算法的 D 参数。
        public byte[] D;
        //
        // 摘要:
        //     表示 System.Security.Cryptography.RSA 算法的 DP 参数。
        public byte[] DP;
        //
        // 摘要:
        //     表示 System.Security.Cryptography.RSA 算法的 DQ 参数。
        public byte[] DQ;
        //
        // 摘要:
        //     表示 System.Security.Cryptography.RSA 算法的 Exponent 参数。
        public byte[] Exponent;
        //
        // 摘要:
        //     表示 System.Security.Cryptography.RSA 算法的 InverseQ 参数。
        public byte[] InverseQ;
        //
        // 摘要:
        //     表示 System.Security.Cryptography.RSA 算法的 Modulus 参数。
        public byte[] Modulus;
        //
        // 摘要:
        //     表示 System.Security.Cryptography.RSA 算法的 P 参数。
        public byte[] P;
        //
        // 摘要:
        //     表示 System.Security.Cryptography.RSA 算法的 Q 参数。
        public byte[] Q;
    }


    /// <summary>
    /// RSA 加密类
    /// </summary>
    public class RSAEncryption
    {
        static public void CreatePairedKeys(out RSAParameters pubKey, out RSAParameters privateKey)
        {
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
            pubKey = RSA.ExportParameters(false);
            privateKey = RSA.ExportParameters(true);
        }

        /// <summary>
        /// 将密钥转换为字符串
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        static public string KeyToString(RSAParameters key)
        {
            InnerRSAParameters inner;
            inner.D = key.D;
            inner.DP = key.DP;
            inner.DQ = key.DQ;
            inner.Exponent = key.Exponent;
            inner.InverseQ = key.InverseQ;
            inner.Modulus = key.Modulus;
            inner.P = key.P;
            inner.Q = key.Q;

            Stream s = Hubble.Framework.Serialization.XmlSerialization.Serialize(inner);
  
            byte[] buf = new byte[s.Length];

            s.Position = 0;
            s.Read(buf, 0, (int)s.Length);

            return EncryptString.GetString(buf);
        }

        /// <summary>
        /// 从字符串获取密钥
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        static public RSAParameters KeyFromString(string str)
        {
            byte[] buf = EncryptString.GetBytes(str);

            MemoryStream s = new MemoryStream();
            s.Write(buf, 0, buf.Length);
            s.Position = 0;

            object obj;
            obj = Hubble.Framework.Serialization.XmlSerialization.Deserialize(s, typeof(InnerRSAParameters));

            InnerRSAParameters inner = (InnerRSAParameters)obj;

            RSAParameters key = new RSAParameters();

            key.D = inner.D;
            key.DP = inner.DP;
            key.DQ = inner.DQ;
            key.Exponent = inner.Exponent;
            key.InverseQ = inner.InverseQ;
            key.Modulus = inner.Modulus;
            key.P = inner.P;
            key.Q = inner.Q;

            return key;
        }

        /// <summary>
        /// 加密字符串
        /// </summary>
        /// <param name="str">要加密的字符串</param>
        /// <param name="RSAKeyInfo">公钥</param>
        /// <returns>加密后的字符串</returns>
        static public string RSAEncryptString(string str, RSAParameters RSAKeyInfo)
        {
            Stream s = Hubble.Framework.IO.Stream.WriteStringToStream(str, Encoding.UTF8);
            byte[] buf = new byte[s.Length];
            s.Position = 0;
            s.Read(buf, 0, buf.Length);
            buf = RSAEncrypt(buf, RSAKeyInfo);
            return EncryptString.GetString(buf);
        }


        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="str">要解密的字符串</param>
        /// <param name="RSAKeyInfo">私钥</param>
        /// <returns>加密后的字符串</returns>
        static public string RSADecryptString(string str, RSAParameters RSAKeyInfo)
        {
            byte[] buf = EncryptString.GetBytes(str);
            buf = RSADecrypt(buf, RSAKeyInfo);
            MemoryStream s = new MemoryStream();
            s.Write(buf, 0, buf.Length);

            string retString;
            s.Position = 0;
            Hubble.Framework.IO.Stream.ReadStreamToString(s, out retString, Encoding.UTF8);

            return retString;
        }


        /// <summary>
        /// Encrypt
        /// </summary>
        /// <param name="dataToEncrypt"></param>
        /// <param name="RSAKeyInfo"></param>
        /// <returns></returns>
        static public byte[] RSAEncrypt(byte[] dataToEncrypt, RSAParameters RSAKeyInfo)
        {
            return RSAEncrypt(dataToEncrypt, RSAKeyInfo, false);
        }

        /// <summary>
        /// Encrypt
        /// </summary>
        /// <param name="dataToEncrypt"></param>
        /// <param name="RSAKeyInfo"></param>
        /// <param name="doOAEPPadding"></param>
        /// <returns></returns>
        static public byte[] RSAEncrypt(byte[] dataToEncrypt, RSAParameters RSAKeyInfo, bool doOAEPPadding)
        {
            //Create a new instance of RSACryptoServiceProvider.
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();

            //Import the RSA Key information. This only needs
            //toinclude the public key information.
            RSA.ImportParameters(RSAKeyInfo);

            //Encrypt the passed byte array and specify OAEP padding.  
            //OAEP padding is only available on Microsoft Windows XP or
            //later.  
            return RSA.Encrypt(dataToEncrypt, doOAEPPadding);

        }

        /// <summary>
        /// Decrypt
        /// </summary>
        /// <param name="dataToDecrypt"></param>
        /// <param name="RSAKeyInfo"></param>
        /// <returns></returns>
        static public byte[] RSADecrypt(byte[] dataToDecrypt, RSAParameters RSAKeyInfo)
        {
            return RSADecrypt(dataToDecrypt, RSAKeyInfo, false);

        }

        /// <summary>
        /// Decrypt
        /// </summary>
        /// <param name="dataToDecrypt"></param>
        /// <param name="RSAKeyInfo"></param>
        /// <param name="doOAEPPadding"></param>
        /// <returns></returns>
        static public byte[] RSADecrypt(byte[] dataToDecrypt, RSAParameters RSAKeyInfo, bool doOAEPPadding)
        {
            //Create a new instance of RSACryptoServiceProvider.
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();

            //Import the RSA Key information. This needs
            //to include the private key information.
            RSA.ImportParameters(RSAKeyInfo);

            //Decrypt the passed byte array and specify OAEP padding.  
            //OAEP padding is only available on Microsoft Windows XP or
            //later.  
            return RSA.Decrypt(dataToDecrypt, doOAEPPadding);
        }
    }
}
