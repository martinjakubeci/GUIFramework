using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUIFramework.Core
{
    public static class GenericExtensions
    {
        public static T Copy<T>(this T @this)
            => PrepareCopy(@this).Copy();

        public static CopyPrototype<T> PrepareCopy<T>(this T @this)
            => new CopyPrototype<T>(@this);

    }

    public class CopyPrototype<T>
    {
        //use json serialize and deserialize to create a deep copy of an object

        private string _serialized;
        private JsonSerializerSettings _serializeSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
        private JsonSerializerSettings _deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace, TypeNameHandling = TypeNameHandling.All };

        public string Serialized => _serialized;

        public CopyPrototype(T instance)
            => _serialized = JsonConvert.SerializeObject(instance, _serializeSettings);

        public T Copy()
            => JsonConvert.DeserializeObject<T>(_serialized, _deserializeSettings);
    }
}
