﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SoftAPIClient.Core.Interfaces;
using SoftAPIClient.MetaData;
using Newtonsoft.Json;
using RestSharp;

namespace SoftAPIClient.Implementations.RestSharpImpl
{
    public class RestSharpResponseConverter : IResponseConverter
    {
        public virtual Response Convert(Func<Request> request)
        {
            var requestObject = request.Invoke();
            var settings = requestObject.Settings;
            IRestClient client = new RestClient(requestObject.Url)
            {
                FollowRedirects = settings?.FollowRedirects ?? true
            };

            if (settings?.Encoder != null)
            {
                client.UseUrlEncoder(settings.Encoder);
            }

            foreach (var (key, value) in requestObject.PathParameters)
            {
                client.AddOrUpdateDefaultParameter(new Parameter(key, value, ParameterType.UrlSegment));
            }

            foreach (var (key, value) in requestObject.QueryParameters)
            {
                client.AddOrUpdateDefaultParameter(new Parameter(key, value, ParameterType.QueryString));
            }

            var restRequest = new RestRequest(Enum.Parse<Method>(requestObject.Method));
            requestObject.Headers.ForEach(h => restRequest.AddHeader(h.Key, h.Value));

            if (requestObject.FormDataParameters.Count != 0)
            {
                foreach (var (key, value) in requestObject.FormDataParameters)
                {
                    restRequest.AddParameter(key, value);
                }
            }

            HandleBody(requestObject, restRequest);

            IResponseDeserializer deserializer = requestObject.Deserializer ?? new RestSharpJsonResponseDeserializer();

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            IRestResponse response = client.Execute(restRequest);
            stopWatch.Stop();

            var responseHeaders = response.Headers
                .Select(p => new KeyValuePair<string, string>(p.Name, p.Value?.ToString())).ToList();
            var responseCookies = response.Cookies
                .Select(p => new KeyValuePair<string, string>(p.Name, p.Value?.ToString())).ToList();

            var result = new Response
            {
                HttpStatusCode = response.StatusCode,
                ResponseUri = response.ResponseUri,
                Headers = responseHeaders,
                Cookies = responseCookies,
                ContentType = response.ContentType,
                OriginalRequest = requestObject,
                OriginalResponse = response,
                ResponseBodyString = response.Content,
                ElapsedTime = stopWatch.ElapsedMilliseconds,
                Deserializer = deserializer
            };
            return result;
        }

        private void HandleBody(Request request, IRestRequest restRequest)
        {
            var requestBody = request.Body.Value;
            if (requestBody == null)
            {
                return;
            }
            switch (request.Body.Key)
            {
                case BodyType.Json:
                    var serializedBody = JsonConvert.SerializeObject(requestBody);
                    restRequest.AddParameter("application/json", serializedBody, ParameterType.RequestBody);
                    restRequest.AddJsonBody(requestBody);
                    break;
                case BodyType.Xml:
                    restRequest.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                    restRequest.RequestFormat = DataFormat.Xml;
                    restRequest.AddXmlBody(requestBody);
                    break;
                default:
                    restRequest.AddBody(requestBody);
                    break;
            }
        }
    }
}