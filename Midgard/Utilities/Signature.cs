using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.X509;

namespace Midgard.Utilities
{
    public class Signature
    {
        public static void Generate()
        {
            using (var rsa = RSA.Create(4096))
            {
                Save(rsa);
            }
        }

        public static void Load()
        {
            if (Program.RsaKey != null)
            {
                return;
            }

            Program.RsaKey = RSA.Create();
            Program.RsaKey.FromXmlString(InternalLoad());

            Program.RsaPublicKey =
                File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\PublicKey.key");
        }

        public static string Sign(string str)
        {
            if (Program.RsaKey == null)
            {
                Load();
            }

            if (Program.RsaKey == null)
            {
                return null;
            }

            var bytes = Encoding.UTF8.GetBytes(str);
            var signedData = Program.RsaKey.SignData(bytes, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(signedData);
            
        }

        private static string InternalLoad()
        {
            var file = AppDomain.CurrentDomain.BaseDirectory + @"\PrivateKey.xml";
            if (!File.Exists(file))
            {
                Generate();
            }

            return File.ReadAllText(file);
        }

        private static void Save(RSA rsa)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var privateKey = rsa.ToXmlString(true);
            File.WriteAllText(path + @"\PrivateKey.xml", privateKey);
            var publicKey = rsa.ToXmlString(false);
            var publicKeyJava = "-----BEGIN PUBLIC KEY-----" + PublicKeyToJavaFormat(publicKey) + "-----END PUBLIC KEY-----";
            File.WriteAllText(path + @"\PublicKey.key", publicKeyJava);
        }

        private static string PublicKeyToJavaFormat(string publicKey)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(publicKey);
            var m = new BigInteger(1, Convert.FromBase64String(xmlDocument.DocumentElement.GetElementsByTagName("Modulus")[0].InnerText));
            var p = new BigInteger(1, Convert.FromBase64String(xmlDocument.DocumentElement.GetElementsByTagName("Exponent")[0].InnerText));
            var pub = new RsaKeyParameters(false, m, p);

            var publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(pub);
            var serializedPublicBytes = publicKeyInfo.ToAsn1Object().GetDerEncoded();
            return Convert.ToBase64String(serializedPublicBytes);
        }
    }
}