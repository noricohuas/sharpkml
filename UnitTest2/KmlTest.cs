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
using SharpKml.Dom;
using SharpMap.Data;
using SharpMap.Data.Providers;
using SharpMap.Styles;
using TimeSpan = System.TimeSpan;
using System.Drawing;

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

       private Func<List<Coordinate>, string> gwl = (list) =>
        {
            var result = "";
            foreach (var item in list)
            {
                result += $"{item.X},{item.Y}  ";
            }

            return result;

        };

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
            List<string> ids = kml.GetObjectIDsInViewForSList(kml.GetExtents());
            int polylineCount = 0;
            int pointCount = 0;
            foreach (var id in ids)
            {
                var dr = kml.GetFeature(id);
                var place = (Placemark)dr.ItemArray[2];
                if (dr.Geometry.GeometryType.Equals("MultiLineString") || dr.Geometry.GeometryType.Equals("LineString"))
                {
                    polylineCount++;
                   
                    Console.WriteLine($"LineString:{place.Id},{place.Name},{kml.GetGeometryType(place.Id)}" );
                }
                if (dr.Geometry.GeometryType.Equals("Point") || dr.Geometry.GeometryType.Equals("MultiPoint"))
                {
                    pointCount++;
                    Console.WriteLine($"Point:{place.Id},{place.Name}");
                }

             
             
                //Console.WriteLine(gwl(dr.Geometry.Coordinates.ToList()));
                //Console.WriteLine("------------------------------------------------------");
            }

            kml.GetFolders().ForEach(folder =>
            {
                Console.WriteLine($"folder:{folder.Id},{folder.Name},{folder.Features.Count()}");
            });

            Assert.AreEqual(kml.GetFolders().Count, 6);
            Assert.AreEqual(polylineCount,3);
            Console.WriteLine($"polylineCount:{polylineCount}");
            Assert.AreEqual(pointCount, 24);
            Console.WriteLine($"pointCount:{pointCount}");

        }

        [Test]
        public void KmlFileTest2()
        {
            GeoAPI.GeometryServiceProvider.Instance = NetTopologySuite.NtsGeometryServices.Instance;
            var kml = KmlProvider.FromKml(@"C:\Workspace\huas\2010上海50KM毅行.kml");
            //var kml = KmlProvider.FromKmz(@"C:\Workspace\huas\test2_MapToKML.kmz");
            //var kml = KmlProvider.FromKmz(@"C:\Workspace\huas\Polygontwo_LayerToKML.kmz");
            Assert.IsNotNull(kml);
            var folder =  kml.GetRoot();
            Dictionary<string, TypeAndStyle> dic = new Dictionary<string, TypeAndStyle>();
            kml.GetObjectIDsInViewForSList(kml.GetExtents()).ForEach(x =>
            {
                var dr = kml.GetFeature(x);
                var style = kml.GetKmlStyle(dr);
                var styleid = (string) dr["StyleUrl"];
                var place = (Placemark)dr.ItemArray[2];
                //Console.WriteLine($"styleid:{styleid},{place.StyleUrl.ToString()}");
                if (style != null && !string.IsNullOrEmpty(styleid) && !dic.ContainsKey(styleid))
                {
                    dic.Add(styleid, new TypeAndStyle(kml.GetGeometryType(place?.Id), style));
                }
            });

            Console.WriteLine($"styleCount:{dic.Count}");
            dic.ToList().ForEach(x =>
            {
                VectorStyle style = x.Value.Style;
                Console.WriteLine($"styleid:{x.Key},type:{x.Value.Type}");
                if (x.Value.Type.Equals("esriGeometryPolygon"))
                {
                    var fill = (SolidBrush)style.Fill;
                    var outline = style.Outline;
                    Console.WriteLine($"color:{fill.Color.R},{fill.Color.G},{fill.Color.B},{fill.Color.A},  " +
                                      $"outline:{outline.Color.R},{outline.Color.G},{outline.Color.B},{outline.Color.A}, width:{outline.Width}");
                }else if (x.Value.Type.Equals("esriGeometryPolyline"))
                {
                    var line = style.Line;
                    Console.WriteLine($"linecolor:{line.Color.R},{line.Color.G},{line.Color.B},{line.Color.A}, width:{line.Width}, offset:{style.LineOffset}");
                }
                else if (x.Value.Type.Equals("esriGeometryPoint"))
                {
                    var url = kml.GetIconUrl(x.Key);
                    Console.WriteLine($"url:{url}");
                    if (!string.IsNullOrEmpty(url) && !url.Contains("http"))
                    {
                        var image = kml.GetImageFromKmz(url);
                        if (image != null)
                        {
                            Console.WriteLine($"image height:{image.Height} width:{image.Width} ");
                        }
                    }
                }
            });


        }

        public class TypeAndStyle
        {
            public string Type { get; set; }
            public VectorStyle Style { get; set; }
            public TypeAndStyle(string type, VectorStyle v)
            {
                Type = type;
                Style = v;
            }
        }


        [Test]
        public void KmlFileTest3()
        {
            GeoAPI.GeometryServiceProvider.Instance = NetTopologySuite.NtsGeometryServices.Instance;
            //var kml = KmlProvider.FromKml(@"C:\Workspace\huas\2010上海50KM毅行.kml");
            var kml = KmlProvider.FromKmz(@"C:\Workspace\huas\test2_MapToKML.kmz");
            //var kml = KmlProvider.FromKmz(@"C:\Workspace\huas\Polygontwo_LayerToKML.kmz");
            Assert.IsNotNull(kml);

            List<string> ids = kml.GetObjectIDsInViewForSList(kml.GetExtents());
            int polylineCount = 0;
            int pointCount = 0;
            int polygoncount = 0;
            int styleCount = 0;
            foreach (var id in ids)
            {
                var dr = kml.GetFeature(id);
                var style = kml.GetKmlStyle(dr);
                Console.WriteLine($"styleid:{(string)dr["StyleUrl"]},");
                var place = (Placemark)dr.ItemArray[2];
                if (style != null)
                {
                    styleCount++;
                }
                if (dr.Geometry.GeometryType.Equals("MultiLineString") || dr.Geometry.GeometryType.Equals("LineString"))
                {
                    polylineCount++;

                    Console.WriteLine($"LineString:{place.Id},{place.Name},{kml.GetGeometryType(place.Id)}");
                }
                if (dr.Geometry.GeometryType.Equals("Point") || dr.Geometry.GeometryType.Equals("MultiPoint"))
                {
                    pointCount++;
                    Console.WriteLine($"Point:{place.Id},{place.Name},{kml.GetGeometryType(place.Id)}");
                }

                if (dr.Geometry.GeometryType.Equals("Polygon") || dr.Geometry.GeometryType.Equals("MultiPolygon") || dr.Geometry.GeometryType.Equals("GeometryCollection"))
                {
                    polygoncount++;
                    Console.WriteLine($"Point:{place.Id},{place.Name},{kml.GetGeometryType(place.Id)}");
                }

            }

            Console.WriteLine($"pointCount:{pointCount},polylineCount:{polylineCount},polygoncount:{polygoncount},");
            Console.WriteLine($"ids:{ids.Count},styleCount:{styleCount}");

            kml.GetFolders().ForEach(folder =>
            {
                Console.WriteLine($"folder:{folder.Id},{folder.Name},{folder.Features.Count()}");
            });

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
