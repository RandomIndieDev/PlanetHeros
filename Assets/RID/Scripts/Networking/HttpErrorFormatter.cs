using System;
using UnityEngine;

namespace RID
{
    public static class HttpErrorFormatter
    {
        public static string Format(Exception e)
        {
            if (e == null)
                return "Unknown error";

            // unwrap aggregate exceptions
            if (e is AggregateException agg && agg.InnerException != null)
                e = agg.InnerException;

            // HttpRequestException
            if (e is System.Net.Http.HttpRequestException httpEx)
            {
                return $"Network error: {httpEx.Message}";
            }

            // WebException
            if (e is System.Net.WebException webEx)
            {
                if (webEx.Response != null)
                {
                    using var reader = new System.IO.StreamReader(webEx.Response.GetResponseStream());
                    string response = reader.ReadToEnd();

                    string parsed = TryExtractMessageFromJson(response);
                    if (!string.IsNullOrEmpty(parsed))
                        return parsed;

                    return response;
                }

                return webEx.Message;
            }

            // fallback
            return e.Message;
        }

        private static string TryExtractMessageFromJson(string json)
        {
            try
            {
                var obj = JsonUtility.FromJson<ErrorResponse>(json);
                return obj?.message;
            }
            catch
            {
                return null;
            }
        }

        [Serializable]
        private class ErrorResponse
        {
            public string message;
            public string error;
        }
    }
}