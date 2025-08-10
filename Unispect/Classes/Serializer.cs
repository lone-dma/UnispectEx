﻿using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

namespace Unispect
{
    public static class Serializer
    {
        public static void Save(string filePath, object objectToSerialize)
        {
            try
            {
                using (Stream stream = File.Open(filePath, FileMode.Create))
                {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                    var bin = new BinaryFormatter();
#pragma warning restore SYSLIB0011 // Type or member is obsolete
                    bin.Serialize(stream, objectToSerialize);
                }
            }
            catch (IOException)
            {
            }
        }

        public static void SaveCompressed(string filePath, object objectToSerialize)
        {
            // Todo: maybe implement a progress indicator by wrapping the stream
            try
            {
                using (Stream fileStream = File.Open(filePath, FileMode.Create))
                using (var compressedStream = new GZipStream(fileStream, CompressionMode.Compress))
                {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                    var bin = new BinaryFormatter();
#pragma warning restore SYSLIB0011 // Type or member is obsolete
                    bin.Serialize(compressedStream, objectToSerialize);
                }
            }
            catch (IOException)
            {
            }
        }

        public static T Load<T>(string filePath) where T : new()
        {
            var result = new T();

            try
            {
                using (Stream stream = File.Open(filePath, FileMode.Open))
                {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                    var bin = new BinaryFormatter();
#pragma warning restore SYSLIB0011 // Type or member is obsolete
                    result = (T)bin.Deserialize(stream);
                }
            }
            catch (IOException)
            {
            }

            return result;
        }
        public static T LoadCompressed<T>(string filePath) where T : new()
        {
            var result = new T();

            try
            {
                using (Stream fileStream = File.Open(filePath, FileMode.Open)) 
                using (var decompressStream = new GZipStream(fileStream, CompressionMode.Decompress))
                {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                    var bin = new BinaryFormatter();
#pragma warning restore SYSLIB0011 // Type or member is obsolete
                    result = (T)bin.Deserialize(decompressStream);
                }
            }
            catch (IOException)
            {
            }

            return result;
        }
    }
}