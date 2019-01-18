using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Api.Gateway.Auth
{
    public class SecurityKeysProvider
    {
        private static volatile List<SecurityKey> _listKeys;
        private static readonly object SyncRoot = new object();

        public static List<SecurityKey> GetSecurityKeys(string jwksUri)
        {
            if (_listKeys != null && _listKeys.Any())
            {
                return _listKeys;
            }

            lock (SyncRoot)
            {
                if (_listKeys != null && _listKeys.Any())
                {
                    return _listKeys;
                }

                using (var cl = new HttpClient())
                {
                    var data = cl.GetStringAsync(jwksUri).Result;

                    var keys = new List<SecurityKey>();

                    var jwks = JsonConvert.DeserializeObject<List<JsonWebKey>>(data);

                    foreach (var webKey in jwks)
                    {
                        var unescapeDataString = Uri.UnescapeDataString(webKey.E);
                        var e = Base64UrlEncoder.DecodeBytes(unescapeDataString);
                        unescapeDataString = Uri.UnescapeDataString(webKey.N);
                        var n = Base64UrlEncoder.DecodeBytes(unescapeDataString);
                        var key = new RsaSecurityKey(new RSAParameters { Exponent = e, Modulus = n });
                        keys.Add(key);
                    }

                    _listKeys = keys;

                }
            }

            return _listKeys;
        }
    }
}
