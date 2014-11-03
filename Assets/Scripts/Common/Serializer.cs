using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Assets.Scripts.Common
{
    public static class Serializer
    {
        private static void SetEnvironmentVariable()
        {
            Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
        }

        public static string Serialize(object input)
        {
            SetEnvironmentVariable();

            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter
                {
                    Binder = new VersionDeserializationBinder()
                };

                formatter.Serialize(stream, input);

                return Convert.ToBase64String(stream.ToArray());
            }
        }

        public static T Deserialize<T>(string input)
        {
            SetEnvironmentVariable();

            using (var stream = new MemoryStream(Convert.FromBase64String(input)))
            {
                var formatter = new BinaryFormatter
                {
                    Binder = new VersionDeserializationBinder()
                };

                return (T) formatter.Deserialize(stream);
            }
        }
    }

    public sealed class VersionDeserializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (string.IsNullOrEmpty(assemblyName) || string.IsNullOrEmpty(typeName)) return null;

            assemblyName = Assembly.GetExecutingAssembly().FullName;

            var typeToDeserialize = Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));

            return typeToDeserialize;
        }
    }
}