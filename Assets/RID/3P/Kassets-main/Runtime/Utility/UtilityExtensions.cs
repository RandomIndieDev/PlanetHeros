using System;
using System.Collections.Generic;
using System.Linq;

namespace Kadinche.Kassets
{
    public static class UtilityExtensions
    {
        /// <summary>
        /// Determine whether a type is simple (String, Decimal, DateTime, etc) 
        /// or complex (i.e. custom class with public properties and methods).
        /// </summary>
        /// <see cref="http://stackoverflow.com/questions/2442534/how-to-test-if-type-is-primitive"/>
        public static bool IsSimpleType(this Type type)
        {
            return
                type.IsPrimitive ||
                new Type[] { 
                    typeof(String),
                    typeof(Decimal),
                    typeof(DateTime),
                    typeof(DateTimeOffset),
                    typeof(TimeSpan),
                    typeof(Guid)
                }.Contains(type);
        }
    }
    
    public static class DisposableExtensions
    {
        public static void DisposeAll(this List<IDisposable> disposables)
        {
            if (disposables == null)
                return;
        
            for (int i = 0; i < disposables.Count; i++)
            {
                disposables[i]?.Dispose();
            }
            disposables.Clear();
        }
    }

}