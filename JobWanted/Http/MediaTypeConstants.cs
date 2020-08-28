using JobWanted.Extensions;
using System.Net.Http.Headers;

namespace JobWanted.Http
{
    public static class MediaTypeConstants
    {
        private static readonly MediaTypeHeaderValue DefaultApplicationXmlMediaType = new MediaTypeHeaderValue(MediaTypes.ApplicationXmlMediaType);
        private static readonly MediaTypeHeaderValue DefaultTextXmlMediaType = new MediaTypeHeaderValue(MediaTypes.TextXmlMediaType);
        private static readonly MediaTypeHeaderValue DefaultApplicationJsonMediaType = new MediaTypeHeaderValue(MediaTypes.ApplicationJsonMediaType);
        private static readonly MediaTypeHeaderValue DefaultTextJsonMediaType = new MediaTypeHeaderValue(MediaTypes.TextJsonMediaType);
        private static readonly MediaTypeHeaderValue DefaultApplicationOctetStreamMediaType = new MediaTypeHeaderValue(MediaTypes.ApplicationOctetStreamMediaType);
        private static readonly MediaTypeHeaderValue DefaultApplicationFormUrlEncodedMediaType = new MediaTypeHeaderValue(MediaTypes.ApplicationFormUrlEncodedMediaType);
        private static readonly MediaTypeHeaderValue DefaultApplicationBsonMediaType = new MediaTypeHeaderValue(MediaTypes.ApplicationBsonMediaType);

        public static MediaTypeHeaderValue ApplicationOctetStreamMediaType => DefaultApplicationOctetStreamMediaType.Clone();

        public static MediaTypeHeaderValue ApplicationXmlMediaType => DefaultApplicationXmlMediaType.Clone();

        public static MediaTypeHeaderValue ApplicationJsonMediaType => DefaultApplicationJsonMediaType.Clone();

        public static MediaTypeHeaderValue TextXmlMediaType => DefaultTextXmlMediaType.Clone();

        public static MediaTypeHeaderValue TextJsonMediaType => DefaultTextJsonMediaType.Clone();

        public static MediaTypeHeaderValue ApplicationFormUrlEncodedMediaType => DefaultApplicationFormUrlEncodedMediaType.Clone();

        public static MediaTypeHeaderValue ApplicationBsonMediaType => DefaultApplicationBsonMediaType.Clone();
    }
}
