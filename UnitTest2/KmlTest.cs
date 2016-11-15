using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Data;
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
            var fullExtent = kml.GetExtents();
            Assert.IsNotNull(fullExtent);
            List<string> ids = kml.GetObjectIDsInViewForSList(fullExtent);
            Assert.AreEqual(ids[0], "ID_00000");
            FeatureDataRow dr = kml.GetFeature(ids[0]);
            Assert.IsNotNull(dr);
            Assert.IsNotNull(dr.ItemArray);
            Assert.AreEqual(dr.ItemArray[0], "ID_00000");
            Assert.AreEqual(dr.ItemArray[1], "PolyStyle00");

            SharpKml.Dom.Placemark placemark = (SharpKml.Dom.Placemark)dr.ItemArray[2];
            Assert.IsNotNull(placemark);
            Assert.AreEqual(placemark.Id, "ID_00000");
            Assert.AreEqual(placemark.Name, "a");
            Assert.AreEqual(placemark.Description.Text.Trim().Length, 1095);
            Assert.AreEqual(placemark.StyleUrl.ToString(), "#PolyStyle00");
            Assert.IsNull(placemark.Visibility);
            Assert.IsNull(placemark.Open);
            Assert.IsNull(placemark.Address);
            Assert.IsNotNull(placemark.Snippet);
            Assert.IsNotNull(dr.Geometry);

            Assert.IsInstanceOf(typeof(GeometryCollection), dr.Geometry);
            GeometryCollection gc = (GeometryCollection)dr.Geometry;
            Assert.AreEqual(gc.NumGeometries, 3);
            Func<List<Coordinate>, string> gwl = (list) =>
            {
                var result = "";
                foreach (var item in list)
                {
                    result += $"{item.X},{item.Y}  ";
                }

                return result;

            };

            for (int i = 0; i < gc.NumGeometries; i++)
            {
               IGeometry geo =  gc.GetGeometryN(i);
               Console.WriteLine($"{i}:{geo.GeometryType},{gwl(geo.Coordinates.ToList())}");
            }
            Assert.IsNotNull(dr.Table);
            Assert.AreEqual(dr.IsFeatureGeometryNull(), false);
            Assert.AreEqual(kml.GetFeatureCount(), 2);
            Console.WriteLine(string.Format("kml.GetFeatureCount():{0}", kml.GetFeatureCount()));
            Console.WriteLine(string.Format("kml.GetExtents():{0}", kml.GetExtents()));


        }

        [Test]
        public void KmlFileURL2Test()
        {
            GeoAPI.GeometryServiceProvider.Instance = NetTopologySuite.NtsGeometryServices.Instance;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(new Uri("http://112.74.67.213:6080/huayu/TestData/raw/master/online/Polygontwo_LayerToKML.kml"));
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            var kml = KmlProvider.FromKml(resp.GetResponseStream());
            Assert.IsNotNull(kml);
            var fullExtent = kml.GetExtents();
            Assert.IsNotNull(fullExtent);
            List<string> ids = kml.GetObjectIDsInViewForSList(fullExtent);
            Assert.AreEqual(ids[1], "ID_00001");
            FeatureDataRow dr = kml.GetFeature(ids[1]);
            Assert.IsNotNull(dr);
            Assert.IsNotNull(dr.ItemArray);
            Assert.AreEqual(dr.ItemArray[0], "ID_00001");
            Assert.AreEqual(dr.ItemArray[1], "PolyStyle00");

            SharpKml.Dom.Placemark placemark = (SharpKml.Dom.Placemark)dr.ItemArray[2];
            Assert.IsNotNull(placemark);
            Assert.AreEqual(placemark.Id, "ID_00001");
            Assert.AreEqual(placemark.Name, "b");
            Assert.AreEqual(placemark.Description.Text.Trim().Length, 1095);
            Assert.AreEqual(placemark.StyleUrl.ToString(), "#PolyStyle00");
            Assert.IsNull(placemark.Visibility);
            Assert.IsNull(placemark.Open);
            Assert.IsNull(placemark.Address);
            Assert.IsNotNull(placemark.Snippet);
            Assert.IsNotNull(dr.Geometry);

            Assert.IsInstanceOf(typeof(GeometryCollection), dr.Geometry);
            GeometryCollection gc = (GeometryCollection)dr.Geometry;
            Assert.AreEqual(gc.NumGeometries, 6);
            Func<List<Coordinate>, string> gwl = (list) =>
            {
                var result = "";
                foreach (var item in list)
                {
                    result += $"{item.X},{item.Y}  ";
                }

                return result;

            };

            for (int i = 0; i < gc.NumGeometries; i++)
            {
                IGeometry geo = gc.GetGeometryN(i);
                Console.WriteLine($"{i}:{geo.GeometryType},{gwl(geo.Coordinates.ToList())}");
            }
            Assert.IsNotNull(dr.Table);
            Assert.AreEqual(dr.IsFeatureGeometryNull(), false);
            Assert.AreEqual(kml.GetFeatureCount(), 2);
            Console.WriteLine(string.Format("kml.GetFeatureCount():{0}", kml.GetFeatureCount()));
            Console.WriteLine(string.Format("kml.GetExtents():{0}", kml.GetExtents()));


        }

        [Test]
        public void KmlFileTest()
        {
            DateTime dt1 = DateTime.Now;
            GeoAPI.GeometryServiceProvider.Instance = NetTopologySuite.NtsGeometryServices.Instance;
            //var kml = KmlProvider.FromKml("/WorkSpace/huas/Polygontwo_LayerToKML.kml");
            var kml = KmlProvider.FromKml(@"C:\Workspace\huas\2010上海50KM毅行.kml");
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

        [Test]
        public void KmzFileTest()
        {
            GeoAPI.GeometryServiceProvider.Instance = NetTopologySuite.NtsGeometryServices.Instance;
            GeoAPI.GeometryServiceProvider.Instance = NetTopologySuite.NtsGeometryServices.Instance;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(new Uri("http://112.74.67.213:6080/huayu/TestData/raw/master/snotelwithlabels.kmz"));
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            var kml = KmlProvider.FromKmz(resp.GetResponseStream());
            Assert.IsNotNull(kml);
            Assert.AreNotEqual(kml.GetFeatureCount(), 0);
            Console.WriteLine(string.Format("kml.GetFeatureCount():{0}", kml.GetFeatureCount()));
            Console.WriteLine(string.Format("kml.GetExtents():{0}", kml.GetExtents()));
        }




    }
}
