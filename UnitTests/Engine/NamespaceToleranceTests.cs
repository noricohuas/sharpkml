using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SharpKml.Dom;
using SharpKml.Dom.GX;
using SharpKml.Engine;

namespace UnitTests.Engine
{
    [TestFixture]
    public class NamespaceToleranceTests
    {
        [Test]
        public void should_load_kmls_even_when_the_namespace_is_wrong()
        {
            using (var stream = SampleData.CreateStream("Engine.Data.OverlayBadNamespace.kml"))
            {

                KmlFile file = KmlFile.Load(stream);
                Assert.That(file.Root, Is.Not.Null);

                var featureCount = ((Container) ((Kml) file.Root).Feature).Features.Count();
                Assert.That(featureCount, Is.EqualTo(12));



            }
        }
    }
}