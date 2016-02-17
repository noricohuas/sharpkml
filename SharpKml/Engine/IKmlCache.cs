using System;

namespace SharpKml.Engine
{
    

    public interface IKmlCache : IDisposable
    {
        KmlFile Load(Uri uri, Func<Uri, KmlFile> loadCallback);
    }
}