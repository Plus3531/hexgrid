//using GeoLibrary.IO.GeoJson;
//using GeoLibrary.IO.Wkt;
//using GeoLibrary.Model;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.CoordinateSystems;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Point = NetTopologySuite.Geometries.Point;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using System.Diagnostics;

namespace hexgrid
{
    public partial class Form1 : Form
    {
        private string fcHexGrid;
        public Form1()
        {
            InitializeComponent();
            fcHexGrid = getHexGrid();
        }
        public static string getHexGrid()
        {
            string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = Path.Combine(directoryName, "mijnraster.geojson");

            // This text is added only once to the file.
            if (!File.Exists(path))
            {
                // Create a file to write to.
                string createText = "Hello and Welcome" + Environment.NewLine;
                File.WriteAllText(path, createText);
            }

            // This text is always added, making the file longer over time
            // if it is not deleted.
            string appendText = "This is extra text" + Environment.NewLine;
            File.AppendAllText(path, appendText);

            // Open the file to read from.
            return File.ReadAllText(path);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Collection<IFeature> features = new Collection<IFeature> { };
            FeatureCollection target = new FeatureCollection();
            FeatureCollection fcPoints;
            string geoJsonFcPoints = "{'type': 'FeatureCollection', 'features': [{'type': 'Feature', 'properties': { 'id': 2, 'compententies': ['eten', 'drinken'], naam: 'Hendrik Slinger' }, 'geometry': { 'type': 'Point', 'coordinates': [82083, 436726] }}, " +
                "{'type': 'Feature', 'properties': { 'id': 12, 'compententies': ['lopen', 'zwemmen'], naam: 'Malle Jan' }, 'geometry': { 'type': 'Point', 'coordinates': [82083, 436727] }}, {'type': 'Feature', 'properties': { 'id': 13, 'compententies': ['snacken', 'buizen'], naam: 'Dikke Dries' }, 'geometry': { 'type': 'Point', 'coordinates': [82083, 446727] }}]}";
            FeatureCollection featureCollection;
            var serializer = GeoJsonSerializer.Create();
            using (var stringReader = new StringReader(geoJsonFcPoints))
            using (var jsonReader = new JsonTextReader(stringReader))
            {
                fcPoints = serializer.Deserialize<FeatureCollection>(jsonReader);
            }

            using (var stringReader = new StringReader(fcHexGrid))
            using (var jsonReader = new JsonTextReader(stringReader))
            {
                featureCollection = serializer.Deserialize<FeatureCollection>(jsonReader);
            }
            var cnt = 1;
            foreach (var pf in fcPoints)
            {
                var foundPoly = featureCollection.FirstOrDefault(f => f.Geometry.Contains(pf.Geometry));

                if (foundPoly != null)
                {
                    var fit = target.FirstOrDefault(tf => tf.Attributes["id"] == foundPoly.Attributes["id"]);
                    if (fit != null)
                    {
                        foundPoly = fit;
                        cnt++;
                    }
                    else
                    {
                        cnt = 1;
                    }
                    var attrNames = pf.Attributes.GetNames();
                    foreach (var an in attrNames)
                    {
                        foundPoly.Attributes.Add(an + cnt.ToString(), pf.Attributes[an]);
                    };
                    if (fit == null)
                    {
                        target.Add(foundPoly);
                    }
                }
            };

            // id wordt uit properties gehaald en niveau hoger gezet (?)
            string geoJson2;

            var serializer2 = GeoJsonSerializer.Create();
            using (var stringWriter = new StringWriter())
            using (var jsonWriter = new JsonTextWriter(stringWriter))
            {
                serializer2.Serialize(jsonWriter, target);
                geoJson2 = stringWriter.ToString();
            }
            MessageBox.Show(geoJson2.ToString());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //var coord = new Coordinate(16800, 7274);
            //var mijPoint = new Point(coord);
            //var theCoords = GetCoords();
            //var cwd = theCoords.Select(c => new CoordDist { Distance = c.Point.Distance(mijPoint), Point = c.Point });
            //var ww = cwd.FirstOrDefault(c => c.Distance == cwd.Min(cc => cc.Distance));
            DoIt();
        }

        private IEnumerable<CoordDist> GetCoords()
        {
            //yield return new CoordDist { Point = new Point(new Coordinate(77948.1823056314, 447157.40190000006)), Distance = 0 };
            //yield return new CoordDist { Point = new Point(new Coordinate(79391.557978605473, 449657.40190000006)), Distance = 0 };
            //yield return new CoordDist { Point = new Point(new Coordinate(82278.3093245536, 449657.40190000006)), Distance = 0 };
            //yield return new CoordDist { Point = new Point(new Coordinate(83721.684997527671, 447157.40190000006)), Distance = 0 };
            //yield return new CoordDist { Point = new Point(new Coordinate(79391.557978605473, 444657.40190000006)), Distance = 0 };
            yield return new CoordDist { Point = new Point(new Coordinate(12000, 6928)), Distance = 0 };
            yield return new CoordDist { Point = new Point(new Coordinate(20000, 10392)), Distance = 0 };
        }

        public void TestWGS84_RDNewTransformListOfCoordinates()
        {
            var csFact = new CoordinateSystemFactory();
            var ctFact = new CoordinateTransformationFactory();

            var rdnew = csFact.CreateFromWkt(
                    "PROJCS[\"Amersfoort / RD New\",GEOGCS[\"Amersfoort\",DATUM[\"Amersfoort\",SPHEROID[\"Bessel 1841\",6377397.155,299.1528128,AUTHORITY[\"EPSG\",\"7004\"]],TOWGS84[565.2369,50.0087,465.658,-0.406857,0.350733,-1.87035,4.0812],AUTHORITY[\"EPSG\",\"6289\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4289\"]],PROJECTION[\"Oblique_Stereographic\"],PARAMETER[\"latitude_of_origin\",52.15616055555555],PARAMETER[\"central_meridian\",5.38763888888889],PARAMETER[\"scale_factor\",0.9999079],PARAMETER[\"false_easting\",155000],PARAMETER[\"false_northing\",463000],UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],AXIS[\"X\",EAST],AXIS[\"Y\",NORTH],AUTHORITY[\"EPSG\",\"28992\"]]");
            //  "PROJCS[\"Amersfoort / RD New\",GEOGCS[\"Amersfoort\",DATUM[\"Amersfoort\",SPHEROID[\"Bessel 1841\",6377397.155,299.1528128]],PRIMEM[\"Greenwich\",0.0],UNIT[\"degree\",0.0174532925199433]],PROJECTION[\"Oblique Stereographic\"],PARAMETER[\"false_easting\",155000.0],PARAMETER[\"false_northing\",463000.0],PARAMETER[\"central_meridian\",5.387638888888891],PARAMETER[\"scale_factor\",0.9999079],PARAMETER[\"latitude_of_origin\",52.15616055555556],UNIT[\"m\",1.0],AUTHORITY[\"EPSG\",28992]]");
            var wgs = csFact.CreateFromWkt("GEOGCS[\"GCS_WGS_1984\",DATUM[\"D_WGS_1984\",SPHEROID[\"WGS_1984\",6378137,298.257223563]],PRIMEM[\"Greenwich\",0],UNIT[\"Degree\",0.0174532925199433]]");

            var utm33 = ProjectedCoordinateSystem.WGS84_UTM(33, true);
            var gcs_WGS84 = GeographicCoordinateSystem.WGS84;
            var trans = ctFact.CreateFromCoordinateSystems(wgs, rdnew);
            double[] point1 = { 5.125436, 52.238438 };
            double[] point2 = { 5.121831, 52.236053 };
            var tpot = new List<double[]> { point1, point2 };
            //XY[] points =
            //{
            //    new XY(5.125436, 52.238438), new XY(5.121831,52.236053)
            //};


            var points = trans.MathTransform.TransformList(tpot);

            foreach (var point in points)
            {
                Debug.WriteLine(point.ToString());
            }
        }
        private ICoordinateTransformation getTransformer()
        {
            var csFact = new CoordinateSystemFactory();
            var ctFact = new CoordinateTransformationFactory();

            var rdnew = csFact.CreateFromWkt(
                    "PROJCS[\"Amersfoort / RD New\",GEOGCS[\"Amersfoort\",DATUM[\"Amersfoort\",SPHEROID[\"Bessel 1841\",6377397.155,299.1528128,AUTHORITY[\"EPSG\",\"7004\"]],TOWGS84[565.2369,50.0087,465.658,-0.406857,0.350733,-1.87035,4.0812],AUTHORITY[\"EPSG\",\"6289\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4289\"]],PROJECTION[\"Oblique_Stereographic\"],PARAMETER[\"latitude_of_origin\",52.15616055555555],PARAMETER[\"central_meridian\",5.38763888888889],PARAMETER[\"scale_factor\",0.9999079],PARAMETER[\"false_easting\",155000],PARAMETER[\"false_northing\",463000],UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],AXIS[\"X\",EAST],AXIS[\"Y\",NORTH],AUTHORITY[\"EPSG\",\"28992\"]]");
            var wgs = csFact.CreateFromWkt("GEOGCS[\"GCS_WGS_1984\",DATUM[\"D_WGS_1984\",SPHEROID[\"WGS_1984\",6378137,298.257223563]],PRIMEM[\"Greenwich\",0],UNIT[\"Degree\",0.0174532925199433]]");
            // TODO: misschien is deze ook goed
            var gcs_WGS84 = GeographicCoordinateSystem.WGS84;
            return ctFact.CreateFromCoordinateSystems(wgs, rdnew);
        }

        private Coordinate[] hexRing(int width, Coordinate[] locations)
        {
            throw new NotImplementedException();
        }
        private (Coordinate, Coordinate) getCenterPointsHexagonCandidates(Coordinate location, double xw, double a)
        {
            var x = location.X / xw;
            var y = location.Y / a;
            var x1 = Math.Truncate(x);
            var x2 = Math.Ceiling(x);
            var y1 = Math.Truncate(y);
            var y2 = Math.Ceiling(y);
            //centerpoints van de hexagons liggen alleen op coordinaten waarbij ze allebei even zijn of allebei oneven
            //er vallen er altijd twee af
            Coordinate xy1 = null;
            Coordinate xy2 = null;
            if ((x1 % 2) == 0)
            {
                xy1 = new Coordinate(x1, ((y1 % 2) == 0) ? y1 : y2);
                xy2 = new Coordinate(x2, ((y1 % 2) == 0) ? y2 : y1);
            }
            else
            {
                xy1 = new Coordinate(x2, ((y1 % 2) == 0) ? y1 : y2);
                xy2 = new Coordinate(x1, ((y1 % 2) == 0) ? y2 : y1);
            }
            return (xy1, xy2);
        }
        private void DoIt()
        {
            //w is de lengte van een hexagon zijde in meters. Het is dezelfde lengte als de radius van een hexagon
            //De lengte moet configurabel zijn, uiteindelijk
            var w = 4000;
            // a is de apothem, zie https://www.mathsisfun.com/definitions/apothem.html
            var a = (Math.Sqrt(3) * w) / 2;

            // volgende kolom hexagons ligt in de holte van de 1ste kolom hexagon, ze grijpen in elkaar. Daarom ligt centerpoint.X de volgende kolom hexagons niet
            // op 2 * w maar op 3/4 * 2 * w
            double g = (3.0 / 4.0);
            var xw = g * (2 * w);
            //van een locatie wordt de x gedeeld door 2 keer w, y gedeeld door a.
            //omdat we liggende hexagons hebben (vlakke kant van hexagon boven)

            var location = new Coordinate(81090, 438855);
            // var location = new Coordinate(83393, 436442);
            //var location = new Coordinate(80116, 435809);
            var (xy1, xy2) = getCenterPointsHexagonCandidates(location, xw, a);
            //vertaal naar echte coordinaten
            if (xy1 != null && xy2 != null)
            {
                // xy2 = new Coordinate(12, 126);
                var t1 = new Coordinate((xy1.X * xw), xy1.Y * a);
                var t2 = new Coordinate((xy2.X * xw), xy2.Y * a);
                var cd1 = new CoordDist { Point = new Point(t1), Distance = 0 };
                var cd2 = new CoordDist { Point = new Point(t2), Distance = 0 };
                var mijPoint = new Point(location);
                var cwd = new CoordDist[] { cd1, cd2 }.Select(c => new CoordDist { Distance = c.Point.Distance(mijPoint), Point = c.Point });
                var ww = cwd.FirstOrDefault(c => c.Distance == cwd.Min(cc => cc.Distance));
                var arrexteriorring = new Coordinate[7];
                var vA = new Coordinate(ww.Point.Coordinate.X + w, ww.Point.Coordinate.Y);
                var vD = new Coordinate(ww.Point.Coordinate.X - w, ww.Point.Coordinate.Y);
                var vB = new Coordinate(ww.Point.Coordinate.X + w / 2, ww.Point.Coordinate.Y + (Math.Sqrt(3) * w) / 2);
                var vC = new Coordinate(ww.Point.Coordinate.X - w / 2, ww.Point.Coordinate.Y + (Math.Sqrt(3) * w) / 2);
                var vE = new Coordinate(ww.Point.Coordinate.X - w / 2, ww.Point.Coordinate.Y - (Math.Sqrt(3) * w) / 2);
                var vF = new Coordinate(ww.Point.Coordinate.X + w / 2, ww.Point.Coordinate.Y - (Math.Sqrt(3) * w) / 2);
                var arraytje = new Coordinate[] { vA, vB, vC, vD, vE, vF, vA };
                for (var v = 0; v < arraytje.Length; v++)
                {
                    arrexteriorring[v] = arraytje[v];
                }
                // var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
                var ring = new LinearRing(arrexteriorring);
                var polygon = new Polygon(ring);
                var attributes = new AttributesTable();
                attributes.Add("something", new { dikke = "deur" });
                IFeature feature = new Feature(polygon, attributes);
                FeatureCollection target = new FeatureCollection();
                target.Add(feature);
                string geoJson2;

                var serializer2 = GeoJsonSerializer.Create();
                using (var stringWriter = new StringWriter())
                using (var jsonWriter = new JsonTextWriter(stringWriter))
                {
                    serializer2.Serialize(jsonWriter, target);
                    geoJson2 = stringWriter.ToString();
                }
                MessageBox.Show(geoJson2.ToString());

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            TestWGS84_RDNewTransformListOfCoordinates();
        }

        private FeatureCollection transformToRD_New(string geoJsonFcPoints)
        {
            FeatureCollection fcExactLocations;
            var serializer = GeoJsonSerializer.Create();
            using (var stringReader = new StringReader(geoJsonFcPoints))
            using (var jsonReader = new JsonTextReader(stringReader))
            {
                fcExactLocations = serializer.Deserialize<FeatureCollection>(jsonReader);
            }
            var transformer = getTransformer();
            var target = new FeatureCollection();
            foreach (var pf in fcExactLocations)
            {
                var p = pf.Geometry as Point;
                var (xRd, yRd) = transformer.MathTransform.Transform(p.X, p.Y);
                Debug.WriteLine(xRd);
                Debug.WriteLine(yRd);
                var c = new Coordinate(xRd, yRd);
                var point = new Point(c);
                //  var newAttr = new AttributesTable();
                var newPf = new Feature(point, pf.Attributes);
                //var attrNames = pf.Attributes.GetNames();
                //var cnt = 1;
                //foreach (var an in attrNames)
                //{
                //    newAttr.Add(an + cnt.ToString(), pf.Attributes[an]);                    
                //    cnt++;
                //};
                target.Add(newPf);
            }
            return target;
        }
        private FeatureCollection DoIt2(FeatureCollection locations)
        {
            //w is de lengte van een hexagon zijde in meters. Het is dezelfde lengte als de radius van een hexagon
            //De lengte moet configurabel zijn, uiteindelijk
            var w = 4000;
            // a is de apothem, zie https://www.mathsisfun.com/definitions/apothem.html
            var a = (Math.Sqrt(3) * w) / 2;

            // volgende kolom hexagons ligt in de holte van de 1ste kolom hexagon, ze grijpen in elkaar. Daarom ligt centerpoint.X de volgende kolom hexagons niet
            // op 2 * w maar op 3/4 * 2 * w
            double g = (3.0 / 4.0);
            var xw = g * (2 * w);
            //van een locatie wordt de x gedeeld door 2 keer w, y gedeeld door a.
            //omdat we liggende hexagons hebben (vlakke kant van hexagon boven)
            var target = new FeatureCollection();
            var cnt = 1;
            foreach (var l in locations)
            {
                Debug.WriteLine(l.ToString());
                var p = l.Geometry as Point;
                var (xy1, xy2) = getCenterPointsHexagonCandidates(p.Coordinate, xw, a);
                if (xy1 != null && xy2 != null)
                {
                    // xy2 = new Coordinate(12, 126);
                    var t1 = new Coordinate((xy1.X * xw), xy1.Y * a);
                    var t2 = new Coordinate((xy2.X * xw), xy2.Y * a);
                    var cd1 = new CoordDist { Point = new Point(t1), Distance = 0 };
                    var cd2 = new CoordDist { Point = new Point(t2), Distance = 0 };
                  //  var mijPoint = new Point(location);
                    var cwd = new CoordDist[] { cd1, cd2 }.Select(c => new CoordDist { Distance = c.Point.Distance(p), Point = c.Point });
                    var ww = cwd.FirstOrDefault(c => c.Distance == cwd.Min(cc => cc.Distance));
                    var hexPresent = false;
                    foreach (var f in target) {
                        if (ww.Point.Intersects(f.Geometry)) {
                            hexPresent = true;
                            var attrNames = l.Attributes.GetNames();
                            foreach (var an in attrNames)
                            {
                                f.Attributes.Add(an + cnt.ToString(), l.Attributes[an]);
                            };
                            cnt++;
                        }
                    }
                    if (!hexPresent) {
                        var arrexteriorring = new Coordinate[7];
                        var vA = new Coordinate(ww.Point.Coordinate.X + w, ww.Point.Coordinate.Y);
                        var vD = new Coordinate(ww.Point.Coordinate.X - w, ww.Point.Coordinate.Y);
                        var vB = new Coordinate(ww.Point.Coordinate.X + w / 2, ww.Point.Coordinate.Y + (Math.Sqrt(3) * w) / 2);
                        var vC = new Coordinate(ww.Point.Coordinate.X - w / 2, ww.Point.Coordinate.Y + (Math.Sqrt(3) * w) / 2);
                        var vE = new Coordinate(ww.Point.Coordinate.X - w / 2, ww.Point.Coordinate.Y - (Math.Sqrt(3) * w) / 2);
                        var vF = new Coordinate(ww.Point.Coordinate.X + w / 2, ww.Point.Coordinate.Y - (Math.Sqrt(3) * w) / 2);
                        var arraytje = new Coordinate[] { vA, vB, vC, vD, vE, vF, vA };
                        for (var v = 0; v < arraytje.Length; v++)
                        {
                            arrexteriorring[v] = arraytje[v];
                        }
                        // var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
                        var ring = new LinearRing(arrexteriorring);
                        var polygon = new Polygon(ring);
                        IFeature feature2 = new Feature();
                        IFeature feature = new Feature(polygon, l.Attributes);
                        target.Add(feature);
                    }
                }

            }
            return target;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string geoJsonFcPoints = "{'type': 'FeatureCollection', 'features': [{'type': 'Feature', 'properties': { 'ident': 2, 'compententies': ['eten', 'drinken'], naam: 'Hendrik Slinger' }, 'geometry': { 'type': 'Point', 'coordinates': [5.125436, 52.238438] }}, " +
                "{'type': 'Feature', 'properties': { 'ident': 12, 'compententies': ['lopen', 'zwemmen'], naam: 'Malle Jan' }, 'geometry': { 'type': 'Point', 'coordinates': [5.121831, 52.236053] }}, " +
                "{'type': 'Feature', 'properties': { 'ident': 13, 'compententies': ['snacken', 'buizen'], naam: 'Dikke Dries' }, 'geometry': { 'type': 'Point', 'coordinates': [5.925436, 51.236053] }}]}";
            var locationsFC = transformToRD_New(geoJsonFcPoints);
            var target = DoIt2(locationsFC);
            string geoJson;

            var serializer = GeoJsonSerializer.Create();
            using (var stringWriter = new StringWriter())
            using (var jsonWriter = new JsonTextWriter(stringWriter))
            {
                serializer.Serialize(jsonWriter, target);
                geoJson = stringWriter.ToString();
            }
            MessageBox.Show(geoJson.ToString());
        }

    }

    public class CoordDist
    {
        public double Distance { get; set; }
        public NetTopologySuite.Geometries.Point Point { get; set; }
    }
}

//string geoJson = "{\"type\": \"Polygon\", \"coordinates\": [[ [81773, 432082], [82725, 431967], [82102, 431510], [81773, 432082] ]] }";
//var polygon = Geometry.FromGeoJson(geoJson);
//string geoJsonPoint = "{\"type\": \"Point\", \"coordinates\": [82083, 431726] }";
////string wkt = "POINT (82083 441726)";
//var point = Geometry.FromGeoJson(geoJsonPoint);
//// var pointWkt = point.ToWkt();
//var lineStringGeoJson = point.IsIntersects(polygon);// .ToGeoJson();
//MessageBox.Show(lineStringGeoJson.ToString());

