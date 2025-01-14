﻿using System;
using Newtonsoft.Json;
using SoftAPIClient.Core.Interfaces;

namespace SoftAPIClient.RestSharpNewtonsoft
{
    public class RestSharpJsonResponseDeserializer : IResponseDeserializer
    {
        public T Convert<T>(string response)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(response, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
            }
            catch (Exception)
            {
                return Activator.CreateInstance<T>();
            }
        }
    }
}
