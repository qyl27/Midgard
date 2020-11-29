using System;
using System.Security.Cryptography;
using System.Text;

namespace Midgard.Utilities
{
    public class Uuid
    {
        public static Guid GetOfflinePlayerUuid(string name)
        {
            return GetUuidV1($"OfflinePlayer:{name}");
        }
        
        public static Guid GetUuidV1(string str)
        {
            return GetUuidV1(Encoding.UTF8.GetBytes(str));
        }
        
        public static Guid GetUuidV1(byte[] bytes)
        {
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(bytes);
            hash[6] &= 0x0f;
            hash[6] |= 0x30;
            hash[8] &= 0x3f;
            hash[8] |= 0x80;
            var uuid = BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
            return Guid.Parse(uuid);
        }
    }
}