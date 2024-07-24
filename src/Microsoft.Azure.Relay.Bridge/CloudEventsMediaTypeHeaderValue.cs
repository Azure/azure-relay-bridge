using System.Net.Http.Headers;

namespace Microsoft.Azure.Relay.Bridge
{
    internal class CloudEventsMediaTypeHeaderValue : MediaTypeHeaderValue
    {
        public CloudEventsMediaTypeHeaderValue(string mediaType) : base(mediaType)
        {
        }

        public CloudEventsMediaTypeHeaderValue(string mediaType, string charset) : base(mediaType)
        {
            Parameters.Add(new NameValueHeaderValue("charset", charset));
        }

        public static bool TryParse(string input, out CloudEventsMediaTypeHeaderValue parsedValue)
        {
            parsedValue = null;

            if (!MediaTypeHeaderValue.TryParse(input, out var mediaType))
            {
                return false;
            }

            if (mediaType.MediaType != "application/cloudevents+json")
            {
                return false;
            }

            var charset = mediaType.CharSet ?? "utf-8";
            parsedValue = new CloudEventsMediaTypeHeaderValue("application/cloudevents+json", charset);

            return true;
        }
    }
}
