using System;

namespace JobWanted.Extensions
{
    public static class CloneableExtensions
    {
        internal static T Clone<T>(this T value) where T : ICloneable
        {
            return (T)value.Clone();
        }
    }
}
