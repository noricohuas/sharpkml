﻿// Copyright 2014 -      Robert Smart (www.cnl-software.com)
//
// This file is part of SharpMap.Data.Providers.Kml.
// SharpMap.Data.Providers.Kml is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap.Data.Providers.Kml is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using GeoAPI.Geometries;
using SharpKml.Dom;
using SharpKml.Engine;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;
using LinearRing = SharpKml.Dom.LinearRing;
using LineString = SharpKml.Dom.LineString;
using Point = SharpKml.Dom.Point;
using Polygon = SharpKml.Dom.Polygon;
using Style = SharpKml.Dom.Style;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Kml/Kmz provider
    /// </summary>
    public class KmlProvider : IProvider
    {
        #region Static factory methods

        public KmzFile kmzFile;
        /// <summary>
        /// Creates a KmlProvider from a file
        /// </summary>
        /// <param name="filename">The path to the file</param>
        /// <returns>A Kml provider</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public static KmlProvider FromKml(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentNullException("filename");
            if (!File.Exists(filename))
                throw new FileNotFoundException("File not found", "filename");

            using (var s = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var kml = FromKml(s);
                //kml.InitIds();
                return kml;
            }
        }

        /// <summary>
        /// Creates a KmlProvider from a Kml stream
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static KmlProvider FromKml(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            var kml = new KmlProvider(KmlFile.Load(stream));
            //kml.InitIds();
            return kml;
        }

        /// <summary>
        /// Creates a KmlProvider from a file
        /// </summary>
        /// <param name="filename">The path to the file</param>
        /// <param name="internalFile">The internal file to read</param>
        /// <returns>A Kml provider</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public static KmlProvider FromKmz(string filename, string internalFile = null)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentNullException("filename");
            if (!File.Exists(filename))
                throw new FileNotFoundException("File not found", "filename"); 
            
            using (var s = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var kml = FromKmz(s, internalFile);
                //kml.InitIds();
                return kml;
            }
        }

        /// <summary>
        /// Creates a KmlProvider from a Kmz stream
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <param name="internalFile">The internal file to read</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static KmlProvider FromKmz(Stream stream, string internalFile = null)
        {
            var kmz = KmzFile.Open(stream);
            if (string.IsNullOrEmpty(internalFile))
            {
                var kml = new KmlProvider(kmz.GetDefaultKmlFile());
                kml.kmzFile = kmz;
                //kml.InitIds();
                return kml;
            }
        
            //NOTE:DON'T KNOW IF THIS IS CORRECT!
            using (var ms = new MemoryStream(kmz.ReadFile(internalFile)))
            {
                var kml = new KmlProvider(KmlFile.Load(ms));
                //kml.InitIds();
                return kml;
            }
        }
        #endregion

        #region static constructor and fields

        private static readonly FeatureDataTable _schemaTable;

        static KmlProvider()
        {
            _schemaTable = new FeatureDataTable();
            AddColumnsToFeatureDataTable(_schemaTable);
        }

        #endregion

        #region private fields and constants
        private IGeometryFactory _geometryFactory;
        private Dictionary<Placemark, List<IGeometry>> _geometrys;
        private Dictionary<string, VectorStyle> _kmlStyles;
        private List<StyleMap> _styleMaps;

        private const string DefaultStyleId = "{6787C5B3-6482-4B96-9C2D-2C6236D2AC50}";
        private const string DefaultPointStyleId = "{E2892545-7CF4-48A1-B8F0-5A0BF06EF0E1}";

        #endregion

        /// <summary>
        /// Method to create a theme for the Layer
        /// </summary>
        /// <returns>A theme</returns>
        /// <example language="C#">
        /// <code>
        /// </code>
        /// </example>
        public ITheme GetKmlTheme()
        {
            //todo layer will need to do this
            //Layer.Theme = assetTheme;
            return new CustomTheme(GetKmlStyle);
        }

        /// <summary>
        /// Gets or sets a value indicating that <see cref="SharpKml.Dom.LinearRing"/> are to be treated as polygons
        /// </summary>
        public bool RingsArePolygons { get; set; }

        /// <summary>
        /// Creates an instance of this class using the provided KmlFile
        /// </summary>
        /// <param name="kmlFile">The KmlFile</param>
        public KmlProvider(KmlFile kmlFile)
        {
            ParseKml(kmlFile);
        }


        /// <summary>
        /// Method to parse the KmlFile
        /// </summary>
        /// <param name="kmlFile">The file to parse</param>
        private void ParseKml(KmlFile kmlFile)
        {
            var kml = kmlFile.Root as Kml;
            if (kml == null)
            {
                throw new Exception("Kml file is null! Please check that the file conforms to http://www.opengis.net/kml/2.2 standards");
            }

            var doc = kml.Feature as Document;
            if (doc == null)
            {
                throw new Exception("Kml file does not have a document node! please check that the file conforms to http://www.opengis.net/kml/2.2 standards");
            }

            _geometryFactory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(4326);
            ConnectionID = doc.Name;
            if (doc.Description != null && !string.IsNullOrEmpty(doc.Description.Text))
                ConnectionID += " (" + doc.Description.Text + ")";

            ExtractStyles(kml);
            ExtractStyleMaps(kml);
            ExtractGeometries(kml);

        }

        private void ExtractStyleMaps(Element kml)
        {
            _styleMaps = new List<StyleMap>();
            foreach (var style in kml.Flatten().OfType<StyleMapCollection>())
            {
                var styleMap = new StyleMap { Id = style.Id };
                _styleMaps.Add(styleMap);
                style.ToList().ForEach(x =>
                {
                    if (x.State != null)

                        switch (x.State.Value)
                        {
                            case StyleState.Normal:
                                styleMap.NormalStyleUrl = x.StyleUrl.ToString().Replace("#", "");
                                break;
                            case StyleState.Highlight:
                                styleMap.HighlightStyleUrl = x.StyleUrl.ToString().Replace("#", "");
                                break;
                        }

                });
            }
        }

        /// <summary>
        /// Style map class
        /// </summary>
        private class StyleMap
        {
            public string Id { get; set; }
            public string NormalStyleUrl { get; set; }
            public string HighlightStyleUrl { get; set; }
        }

        //todo needs buffing up
        private void ExtractStyles(Element kml)
        {
            _kmlStyles = new Dictionary<string, VectorStyle>();

            _kmlStyles.Add(DefaultStyleId, DefaultVectorStyle());
            _kmlStyles.Add(DefaultPointStyleId, DefaultPointStyle());

            var symbolDict = new Dictionary<string, Image>();

            foreach (var style in kml.Flatten().OfType<Style>())
            {
                if (string.IsNullOrEmpty(style.Id))
                    continue;

                var vectorStyle = new VectorStyle();
                vectorStyle.Enabled = true;

                if (style.Polygon != null)
                {

                    if (style.Polygon.Fill != null)
                    {
                        if (style.Polygon.Fill.Value)
                        {
                            var color = new SolidBrush(Color.FromArgb(style.Polygon.Color.Value.Argb));
                            //fill the polygon
                            vectorStyle.Fill = color;
                            vectorStyle.PointColor = color; //Color.FromArgb(100, color.R, color.G, color.B)
                        }
                        else
                        {
                            //don't fill it 
                            var color = new SolidBrush(Color.Transparent);
                            vectorStyle.Fill = color; //Color.FromArgb(100, color.R, color.G, color.B)
                            vectorStyle.PointColor = color;
                        }
                    }
                    else if (style.Polygon.Color.HasValue)
                    {
                        var color = new SolidBrush(Color.FromArgb(style.Polygon.Color.Value.Argb));
                        //fill the polygon
                        vectorStyle.Fill = color;
                        vectorStyle.PointColor = color; //Color.FromArgb(100, color.R, color.G, color.B)
                    }

                    vectorStyle.EnableOutline = true;
                }

                if (style.Line != null)
                {
                    if (style.Line.Width != null)
                    {
                        var linePen = new Pen(
                            Color.FromArgb(style.Line.Color != null
                                ? style.Line.Color.Value.Argb
                                : Color.Black.ToArgb()), (float)style.Line.Width);

                        vectorStyle.Line = linePen;
                        vectorStyle.Outline = linePen;
                    }
                }

                try
                {

                    if (style.Icon != null && style.Icon.Icon != null && style.Icon.Icon.Href != null)
                    {
                        if (symbolDict.ContainsKey(style.Icon.Icon.Href.ToString()))
                        {
                            vectorStyle.Symbol = symbolDict[style.Icon.Icon.Href.ToString()];
                        }
                        else
                        {
                            //Image newSymbol = Properties.Resources.red_pushpin;
                            ////Image newSymbol = GetImageFromUrl(style.Icon.Icon.Href);                            
                            //symbolDict.Add(style.Icon.Icon.Href.ToString(), newSymbol);
                            //vectorStyle.Symbol = newSymbol;

                            SetIconUrl(style.Id, style.Icon.Icon.Href.ToString());
                        }

                        vectorStyle.SymbolScale = 1f;
                    }
                }
                catch (Exception ex)
                {

                    Trace.WriteLine(ex.Message);
                }

                try
                {
                    _kmlStyles.Add(style.Id, vectorStyle);
                }
                catch (ArgumentException)
                {
                    // we ignore duplicates -> bad kml
                }
            }
        }

        Dictionary<string, string> _IconUrlDic = new Dictionary<string, string>();

        private void SetIconUrl(string styleID, string url)
        {
            if (!_IconUrlDic.ContainsKey(styleID))
            {
                _IconUrlDic.Add(styleID, url);
            }
        }

        public string GetIconUrl(string styleID)
        {
            string result = string.Empty;
            if (_IconUrlDic.ContainsKey(styleID))
            {
                _IconUrlDic.TryGetValue(styleID, out result);
            }

            if (string.IsNullOrEmpty(result))
            {
                foreach (var stl in _styleMaps)
                {
                    if (stl.Id == styleID)
                    {
                        _IconUrlDic.TryGetValue(stl.NormalStyleUrl, out result);
                        return result;
                    }
                }
            }
            return result;
        }

        public VectorStyle GetKmlStyle(FeatureDataRow row)
        {
            //get styleID from row
            var styleId = (string)row["StyleUrl"];

            if (_kmlStyles.ContainsKey(styleId))
            {
                return _kmlStyles[styleId];
            }

            //look in style maps
            foreach (var stl in _styleMaps)
            {

                if (stl.Id == styleId)
                {
                    return _kmlStyles[stl.NormalStyleUrl];
                }
            }

            //if (row.Geometry.OgcGeometryType == OgcGeometryType.Point ||
            //    row.Geometry.OgcGeometryType == OgcGeometryType.MultiPoint)
            //{
            //    return DefaultPointStyle();
            //}

            //return DefaultVectorStyle();

            return null;


        }

        private static VectorStyle DefaultPointStyle()
        {
            var vectorStyle = new VectorStyle();
            vectorStyle.Enabled = true;
            try
            {
                //var defaultIcon = GetImageFromUrl(new Uri(@"http://www.google.com/intl/en_us/mapfiles/ms/icons/red-pushpin.png"));
                var defaultIcon =  Properties.Resources.red_pushpin;
                //use this as default symbol
                vectorStyle.Symbol = defaultIcon;
                vectorStyle.SymbolScale = 1f;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            vectorStyle.PointSize = 2f;
            vectorStyle.PointColor = Brushes.DarkGray;

            return vectorStyle;
        }

        private static VectorStyle DefaultVectorStyle()
        {
            var vectorStyle = new VectorStyle();
            vectorStyle.Enabled = true;

            vectorStyle.Line = new Pen(Brushes.DarkGray, 2f);
            vectorStyle.Outline = new Pen(Brushes.DarkGray, 2f);
            vectorStyle.Fill = new SolidBrush(Color.LightGray);
            vectorStyle.EnableOutline = true;

            return vectorStyle;
        }

        public void ExtractGeometries(Element kml)
        {
            _geometrys = new Dictionary<Placemark, List<IGeometry>>();
            int count = 0;

            //todo handle other geom types
            foreach (var f in kml.Flatten().OfType<MultipleGeometry>())
            {
                f.Id = $"_Geometry_1ids1_{count++}";
                if (!IsProcessed(f))
                    ProcessMuiltipleGeometry(f);
            }

            foreach (var f in kml.Flatten().OfType<Polygon>())
            {
                f.Id = $"_Geometry_1ids1_{count++}";
                if (!IsProcessed(f))
                    ProcessPolygonGeometry(f);
            }

            //foreach (var f in kml.Flatten().OfType<LinearRing>())
            //{
            //    f.Id = $"_Geometry_1ids1_{count++}";
            //    if (!IsProcessed(f))
            //        ProcessLinearRingGeometry(f);
            //}

            foreach (var f in kml.Flatten().OfType<LineString>())
            {
                f.Id = $"_Geometry_1ids1_{count++}";
                if (!IsProcessed(f))
                    ProcessLineStringGeometry(f);
            }

            foreach (var f in kml.Flatten().OfType<Point>())
            {
                f.Id = $"_Geometry_1ids1_{count++}";
                if (!IsProcessed(f))
                    ProcessPointGeometry(f);
            }

        }

        private bool IsProcessed(Geometry e)
        {
            if (e == null) return false;
            if (string.IsNullOrEmpty(e.Id) && e.Id.Contains("_Geometry_1ids1_"))
                return true;
            while (true)
            {
                if (!(e.Parent is Geometry)) break;
                Geometry e2 = e.Parent as Geometry;;
                if (!string.IsNullOrEmpty(e2.Id) && e2.Id.Contains("_Geometry_1ids1_"))
                    return true;
            }
            return false;
        }

        private void ProcessMuiltipleGeometry(MultipleGeometry f)
        {
            f.Geometry.ToList().ForEach(g =>
            {
                if (g is Polygon)
                {
                    ProcessPolygonGeometry((Polygon)g);
                }
                if (g is LineString)
                {
                    ProcessLineStringGeometry((LineString)g);
                }
                if (g is Point)
                {
                    ProcessPointGeometry((Point)g);
                }
                if (g is LinearRing)
                {
                    ProcessLinearRingGeometry((LinearRing) g);
                }
                if (g is MultipleGeometry)
                {
                    ProcessMuiltipleGeometry((MultipleGeometry)g);
                }
            });
        }

        private void ProcessPolygonGeometry(Polygon f)
        {
            var outerRing = _geometryFactory.CreateLinearRing(
                f.OuterBoundary.LinearRing.Coordinates.Select(crd => new Coordinate(crd.Longitude, crd.Latitude)).ToArray());

            var innerHoles = new List<ILinearRing>();

            foreach (var hole in f.InnerBoundary)
            {
                innerHoles.Add(_geometryFactory.CreateLinearRing(
                        hole.LinearRing.Coordinates.Select(crd => new Coordinate(crd.Longitude, crd.Latitude)).ToArray()));
            }

            var pGeom = _geometryFactory.CreatePolygon(outerRing, innerHoles.ToArray());
            AddGeometryToCollection(f.GetParent<Placemark>(), pGeom);
        }

        private void ProcessLineStringGeometry(LineString f)
        {
            IGeometry pGeom;
            if (f.Coordinates.Count == 1)
            {
                var coord = f.Coordinates.First();
                var coords = new Coordinate(coord.Longitude, coord.Latitude);

                pGeom = _geometryFactory.CreatePoint(coords);
            }
            else
            {
                pGeom = _geometryFactory.CreateLineString(
                        f.Coordinates.Select(crd => new Coordinate(crd.Longitude, crd.Latitude)).ToArray());
            }
            AddGeometryToCollection(f.GetParent<Placemark>(), pGeom);
        }

        private void ProcessPointGeometry(Point f)
        {
            var coords = new Coordinate(f.Coordinate.Longitude, f.Coordinate.Latitude);

            var pGeom = _geometryFactory.CreatePoint(coords);
            AddGeometryToCollection(f.GetParent<Placemark>(), pGeom);
        }

        private void ProcessLinearRingGeometry(LinearRing f)
        {
            var ring = _geometryFactory.CreateLinearRing(
                    f.Coordinates.Select(crd => new Coordinate(crd.Longitude, crd.Latitude)).ToArray());

            var geom = RingsArePolygons ? (IGeometry)_geometryFactory.CreatePolygon(ring) : ring;
            AddGeometryToCollection(f.GetParent<Placemark>(), geom);
        }

        private void AddGeometryToCollection(Placemark parent, IGeometry geom)
        {
            List<IGeometry> placeMarkGeoms;
            if (_geometrys.TryGetValue(parent, out placeMarkGeoms) == false)
            {
                placeMarkGeoms = new List<IGeometry>();
                _geometrys.Add(parent, placeMarkGeoms);
            }

            placeMarkGeoms.Add(geom);
        }

        private static object[] GetAssetProperties(Feature f)
        {
            return new object[]
            {
                f.Id,
                f.StyleUrl != null ? f.StyleUrl.ToString().Replace("#", "") : "",
                f
            };
        }

        /*
        private static FeatureDataRow GetAssetFeatureDataRow(FeatureDataTable fdt, string id, string styleId, Placemark geom)
        {
            FeatureDataRow newRow = fdt.NewRow();
            newRow["Id"] = id;
            newRow["StyleUrl"] = styleId;
            newRow["Object"] = geom;
            //newRow["Label"] = obj.Style.Label;

            return newRow;
        }
         */
        private static void AddColumnsToFeatureDataTable(FeatureDataTable fdt)
        {
            if (!fdt.Columns.Contains("Id"))
                fdt.Columns.Add("Id", typeof(string));

            if (!fdt.Columns.Contains("StyleUrl"))
                fdt.Columns.Add("StyleUrl", typeof(string));

            if (!fdt.Columns.Contains("Object"))
                fdt.Columns.Add("Object", typeof(Placemark));

            //if (!fdt.Columns.Contains("Label"))
            //    fdt.Columns.Add("Label", typeof(string));
        }

        void IDisposable.Dispose()
        {
            // throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets the features within the specified <see cref="GeoAPI.Geometries.Envelope"/>
        /// </summary>
        /// <param name="bbox"></param>
        /// <returns>Features within the specified <see cref="GeoAPI.Geometries.Envelope"/></returns>
        public Collection<IGeometry> GetGeometriesInView(Envelope bbox)
        {
            var box = _geometryFactory.ToGeometry(bbox);
            var retCollection = new Collection<IGeometry>();

            foreach (var geometryList in _geometrys.Values)
            {
                geometryList.Where(box.Intersects).ToList().ForEach(retCollection.Add);
            }

            return retCollection;

        }

        /// <summary>
        /// Returns all objects whose <see cref="GeoAPI.Geometries.Envelope"/> intersects 'bbox'.
        /// </summary>
        /// <remarks>
        /// This method is usually much faster than the QueryFeatures method, because intersection tests
        /// are performed on objects simplified by their <see cref="GeoAPI.Geometries.Envelope"/>, and using the Spatial Index
        /// </remarks>
        /// <param name="bbox">Box that objects should intersect</param>
        /// <returns></returns>
        public Collection<uint> GetObjectIDsInView(Envelope bbox)
        {
            var box = _geometryFactory.ToGeometry(bbox);
            var res = new Collection<uint>();
            _geometrys.Where(x => box.Intersects(_geometryFactory.BuildGeometry(x.Value))).ToList().ForEach(x =>
            {
                res.Add(Convert.ToUInt32(x.Key.Id));
            });
            return res;
        }

        public List<string> GetObjectIDsInViewForSList(Envelope bbox)
        {
            var box = _geometryFactory.ToGeometry(bbox);
            var res = new List<string>();
            _geometrys.Where(x => box.Intersects(_geometryFactory.BuildGeometry(x.Value))).ToList().ForEach(x =>
            {
                res.Add(x.Key.Id);
            });
            return res;
        }

        /// <summary>
        /// Returns the geometry corresponding to the Object ID
        /// </summary>
        /// <param name="oid">Object ID</param>
        /// <returns>geometry</returns>
        public IGeometry GetGeometryByID(uint oid)
        {
            var sid = oid.ToString(NumberFormatInfo.InvariantInfo);
            var tmp = _geometrys.FirstOrDefault(x => x.Key.Id == sid);
            
            return tmp.Value != null ?
                _geometryFactory.BuildGeometry(tmp.Value) : null;
        }

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="geom">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(IGeometry geom, FeatureDataSet ds)
        {
            var fdt = (FeatureDataTable)_schemaTable.Copy();

            /* // NOTE WHY IS THIS, No other provider behaves like that?
            if (ds.Tables.Count > 0)
            {
                fdt = ds.Tables[0];
            }
            else
            {
                fdt = new FeatureDataTable();
            }*/

            var pGeom = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(geom);

            fdt.BeginLoadData();
            foreach (var feature in _geometrys)
            {
                feature.Value.Where(pGeom.Intersects).ToList()
                    .ForEach(v =>
                    {
                        var newRow = (FeatureDataRow) fdt.LoadDataRow(GetAssetProperties(feature.Key), true);
                        newRow.Geometry = v;
                    }
                    );
            }
            fdt.EndLoadData();

            ds.Tables.Add(fdt);
        }

        /// <summary>
        /// Returns the data associated with all the geometries that are intersected by 'geom'
        /// </summary>
        /// <param name="box">Geometry to intersect with</param>
        /// <param name="ds">FeatureDataSet to fill data into</param>
        public void ExecuteIntersectionQuery(Envelope box, FeatureDataSet ds)
        {
            ExecuteIntersectionQuery(_geometryFactory.ToGeometry(box), ds);
        }

        /// <summary>
        /// Returns the number of features in the dataset
        /// </summary>
        /// <returns>number of features</returns>
        public int GetFeatureCount()
        {
            return _geometrys.Count;
        }

        /// <summary>
        /// Returns a <see cref="SharpMap.Data.FeatureDataRow"/> based on a RowID
        /// </summary>
        /// <param name="oid">The id of the row.</param>
        /// <returns>datarow</returns>
        public FeatureDataRow GetFeature(uint oid)
        {
            var sid = oid.ToString(NumberFormatInfo.InvariantInfo);
            var tmp = _geometrys.FirstOrDefault(x => x.Key.Id == sid);

            if (tmp.Value != null)
            {
                var res = (FeatureDataRow) _schemaTable.NewRow();
                res.ItemArray = GetAssetProperties(tmp.Key);
                res.Geometry = _geometryFactory.BuildGeometry(tmp.Value);
                //res.AcceptChanges();
                return res;
            }
            return null;
        }

        public FeatureDataRow GetFeature(string oid)
        {
            var tmp = _geometrys.FirstOrDefault(x => x.Key.Id == oid);

            if (tmp.Value != null)
            {
                var res = (FeatureDataRow)_schemaTable.NewRow();
                res.ItemArray = GetAssetProperties(tmp.Key);
                res.Geometry = _geometryFactory.BuildGeometry(tmp.Value);
                //res.AcceptChanges();
                return res;
            }
            return null;
        }

        /// <summary>
        /// <see cref="Envelope"/> of dataset
        /// </summary>
        /// <returns>The 2d extent of the layer</returns>
        public Envelope GetExtents()
        {
            var retEnv = new Envelope();

            _geometrys.Values.ToList().ForEach(x => x.ForEach(v => retEnv.ExpandToInclude(v.EnvelopeInternal)));
            return retEnv;
        }

        public Document GetRoot()
        {
            if (_geometrys.Keys.Count > 0)
            {
                var place = _geometrys.Keys.ToArray()[0];
                if (place.Parent != null)
                {
                    if (place.Parent is Document) return place.Parent as Document;
                    if (place.Parent is Folder)
                    {
                        var f = (Folder)place.Parent;
                        return GetRoot(f);
                    }
                   
                }
            }
            

            return null;
        }

        private Document GetRoot(Folder f)
        {
            Document result = null;
            bool conti = true;
            while (conti)
            {
                if (f != null && f.Parent != null && f.Parent is Folder)
                {
                    f = (Folder) f.Parent;
                }else if (f != null && f.Parent != null && f.Parent is Document)
                {
                    result = (Document)f.Parent;
                }

                if (f == null || f.Parent == null || f.Parent is Document) conti = false;
            }

            return result;
        }

        private Dictionary<string,string> _placeMarkGeometryTypeDic = new Dictionary<string, string>();

        public string GetGeometryType(string id)
        {
            string result = "unkown";
            if (_placeMarkGeometryTypeDic.ContainsKey(id))
            {
                _placeMarkGeometryTypeDic.TryGetValue(id, out result);
            }
            return result;
        }

        protected void InitIds()
        {
            int count = 1;
            var root = GetRoot();
            if (root != null)
            {
                root.Id = count++.ToString();

                Dictionary<string,string> dic = new Dictionary<string, string>();
                _geometrys.Keys.ToList().ForEach(x =>
                {
                    if (x.Parent is Folder)
                    {
                        if ((x.Parent as Folder).Id == null) (x.Parent as Folder).Id = count++.ToString();
                        if (!dic.ContainsKey((x.Parent as Folder).Id))
                        {
                            (x.Parent as Folder).Id = count++.ToString();
                            dic.Add((x.Parent as Folder).Id, (x.Parent as Folder).Id);
                        }
                        
                    }
                    x.Id = count++.ToString();
                });

                
                _geometrys.ToList().ForEach(x =>
                {
                    if (x.Value?.Count > 0)
                    {
                        var first = x.Value.ToArray()[0];
                        if (!_placeMarkGeometryTypeDic.ContainsKey(x.Key.Id))
                            _placeMarkGeometryTypeDic.Add(x.Key.Id, GetEsriGeometryType(first?.GeometryType));
                    }

                });
            }
        }

        private string GetEsriGeometryType(string geometryType)
        {
            string GeometryType = "esriGeometryUnkown";
            switch (geometryType)
            {
                case "Point":
                    GeometryType = "esriGeometryPoint";
                    break;
                case "MultiPoint":
                    GeometryType = "esriGeometryMultipoint";
                    break;
                case "LineString":
                case "MultiLineString":
                    GeometryType = "esriGeometryPolyline";
                    break;
                case "Polygon":
                case "MultiPolygon":
                case "GeometryCollection":
                    GeometryType = "esriGeometryPolygon";
                    break;
                default:
                    GeometryType = "esriGeometryUnkown";
                    break;
            }

            return GeometryType;
        }

        public Image GetImageFromKmz(String fileName)
        {
            if (kmzFile != null)
            {
               byte[] bytes = kmzFile.ReadFile(fileName);
                MemoryStream ms = new MemoryStream(bytes);
                Image image = Image.FromStream(ms);
                return image;
            }
            return Properties.Resources.red_pushpin;
        }

        public List<Folder> GetFolders()
        {
            var result = new List<Folder>();
            Dictionary<string,string> dic = new Dictionary<string, string>();
            _geometrys.Keys.ToList().ForEach(x =>
            {
                if (x.Parent is Folder &&!dic.ContainsKey((x.Parent as Folder).Id))
                {
                    result.Add(x.Parent as Folder);
                    dic.Add((x.Parent as Folder).Id, (x.Parent as Folder).Id);
                }
            });

            return result;
        }

        public Dictionary<Placemark, List<IGeometry>> GetAllGeometrys()
        {
            return _geometrys;
        }

        /// <summary>
        /// Opens the datasource
        /// </summary>
        public void Open()
        {
            IsOpen = true;
        }

        /// <summary>
        /// Closes the datasource
        /// </summary>
        public void Close()
        {
            IsOpen = false;
        }

        /// <summary>
        /// Gets the connection ID of the datasource
        /// </summary>
        /// <remarks>
        /// <para>The ConnectionID should be unique to the datasource (for instance the filename or the
        /// connectionstring), and is meant to be used for connection pooling.</para>
        /// <para>If connection pooling doesn't apply to this datasource, the ConnectionID should return String.Empty</para>
        /// </remarks>
        public string ConnectionID { get; private set; }

        /// <summary>
        /// Returns true if the datasource is currently open
        /// </summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// The spatial reference ID (CRS)
        /// </summary>
        public int SRID { get { return 4326; } set { }}

        #region private helper methods
        private static Image GetImageFromUrl(Uri url)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            using (var httpWebReponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (var stream = httpWebReponse.GetResponseStream())
                {
                    if (stream != null)
                        return Image.FromStream(stream);
                }
            }
            return VectorStyle.DefaultSymbol;
        }
        #endregion

    }
}
