using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SharpKml.Dom;
using SharpKml.Dom.GX;
using SharpKml.Engine;

namespace UnitTests.Engine
{
    [TestFixture]
    public class TrackKmlTests
    {
        [Test]
        public void should_load_track_geometry()
        {
            using (var stream = SampleData.CreateStream("Engine.Data.Tracks.kml"))
            {

                KmlFile file = KmlFile.Load(stream);
                var placemark = file.FindObject("geoffrey") as Placemark;
                var track = placemark.Geometry as Track;
                Assert.That(track,Is.Not.Null);
                Assert.That(track.When.Count(), Is.EqualTo(track.Coordinates.Count()));
            }
        }
        [Test]
        public void track_from_ge()
        {
            using (var stream = new FileStream(@"C:\Dev\replay\Dev\Src\Plugins\KmlPlugin\TestData\TimeSpan_example\doc.kml", FileMode.Open))
            {
                var kml = KmlFile.Load(stream);
                
                Assert.That(kml.Root,Is.Not.Null);
            }
        }
    }
}