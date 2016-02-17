using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SharpKml.Dom;
using SharpKml.Engine;

namespace NUintTests.Engine
{
    [TestFixture]
    public class ExtendedDataTest
    {
        [Test]
        public void CanSaveAndRestoreHiddenData()
        {
            var placemark = new Placemark { Name = "TestPlacemark" };

            placemark.ExtendedData = new ExtendedData();
            placemark.ExtendedData.AddHiddenData(new HiddenData { Name = "HIDDEN_KEY", Value = "A VALUE" });

            var buffer = new byte[2048];
            using (var ms = new MemoryStream(buffer))
            {
                KmlFile.Create(placemark, false).Save(ms);
                ms.Flush();

                using (var reader = new StreamReader(new MemoryStream(buffer)))
                {
                    Console.WriteLine(reader.ReadToEnd());
                }

                using (var ms2 = new MemoryStream(buffer))
                {
                    var kmlFile = KmlFile.Load(ms2);

                    Assert.That(kmlFile.Root, Is.InstanceOf<Placemark>());
                    var placemarkCopy = kmlFile.Root as Placemark;

                    Assert.That(placemarkCopy.ExtendedData.HiddenData.Any(), Is.True);

                    var hidden = placemarkCopy.ExtendedData.HiddenData.First(h => h.Name == "HIDDEN_KEY");

                    Assert.That(hidden, Is.Not.Null);
                }

            }
        }
    }
}
