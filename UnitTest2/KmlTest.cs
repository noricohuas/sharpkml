using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpMap.Data.Providers;

namespace UnitTests
{
    [TestFixture]
    public class KmlTest
    {

        [Test]
        public void KmlFileURLTest()
        {
            GeoAPI.GeometryServiceProvider.Instance = NetTopologySuite.NtsGeometryServices.Instance;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(new Uri("http://112.74.67.213:6080/huayu/TestData/raw/master/online/Polygontwo_LayerToKML.kml"));
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            var kml = KmlProvider.FromKml(resp.GetResponseStream());
            Assert.IsNotNull(kml);
            Assert.AreNotEqual(kml.GetFeatureCount(), 0);
            Console.WriteLine(string.Format("kml.GetFeatureCount():{0}", kml.GetFeatureCount()));
            Console.WriteLine(string.Format("kml.GetExtents():{0}", kml.GetExtents()));


        }

        [Test]
        public void KmlFileTest()
        {
            DateTime dt1 = DateTime.Now;
            GeoAPI.GeometryServiceProvider.Instance = NetTopologySuite.NtsGeometryServices.Instance;
            //var kml = KmlProvider.FromKml("/WorkSpace/huas/Polygontwo_LayerToKML.kml");
            var kml = KmlProvider.FromKml("/WorkSpace/huas/2010上海50KM毅行.kml");
            DateTime dt2 = DateTime.Now;
            Assert.IsNotNull(kml);
            Assert.AreNotEqual(kml.GetFeatureCount(), 0);
            Console.WriteLine(string.Format("kml.GetFeatureCount():{0}", kml.GetFeatureCount()));
            Console.WriteLine(string.Format("kml.GetExtents():{0}", kml.GetExtents()));
            DateTime dt3 = DateTime.Now;
            TimeSpan span1 = dt2 - dt1;
            TimeSpan span2 = dt3 - dt1;
            Console.WriteLine($"dt2 - dt1:{span1.Seconds},dt3 - dt1:{span2.Seconds}");

        }

    }
}
