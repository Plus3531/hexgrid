﻿//using GeoLibrary.IO.GeoJson;
//using GeoLibrary.IO.Wkt;
//using GeoLibrary.Model;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
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

        private void DoIt()
        {
            //w is de lengte van een hexagon zijde in meters. Het is dezelfde lengte als de radius van een hexagon
            //De lengte moet configurabel zijn, uiteindelijk
            var w = 4000;
            // a is de apothem, zie https://www.mathsisfun.com/definitions/apothem.html
            var a = (Math.Sqrt(3) * w) / 2;

            // volgende kolom hexagons ligt in de holte van de 1ste kolom hexagon, ze grijpen in elkaar. Daarom ligt centerpoint.X de volgende kolom hexagons niet
            // op 2 * w maar op 3/4 * 2 * w
            var xw = 3 / 4 * 2 * w;
            //van een locatie wordt de x gedeeld door 2 keer w, y gedeeld door a.
            //omdat we liggende hexagons hebben (vlakke kant van hexagon boven)

            var location = new Coordinate(82083, 436727);
            var x = location.X / (2 * w);
            var y = location.Y / a;
            var x1 = Math.Truncate(x);
            var x2 = Math.Ceiling(x);
            var y1 = Math.Truncate(y);
            var y2 = Math.Ceiling(y);
            //centerpoints van de hexagons liggen alleen op coordinaten waarbij ze allebei even zijn of allebei oneven
            //er vallen er altijd twee af
            Coordinate xy1 = null;
            Coordinate xy2 = null;

            if ((x1 + y1) % 2 == 0)
            {
                xy1 = new Coordinate(x1, y1);
            }
            if ((x1 + y2) % 2 == 0)
            {
                xy1 = new Coordinate(x1, y2);
            }
            if ((x2 + y1) % 2 == 0)
            {
                xy2 = new Coordinate(x2, y1);
            }
            if ((x2 + y2) % 2 == 0)
            {
                xy2 = new Coordinate(x2, y2);
            }
            //vertaal naar echte coordinaten
            if (xy1 != null && xy2 != null)
            {
                var t1 = new Coordinate(((xy1.X - 1) * 2 * w) + xw, xy1.Y * a);
                var t2 = new Coordinate(((xy2.X - 1) * 2 * w) + xw, xy2.Y * a);
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
                for (var v=0;v<arraytje.Length;v++)
                {
                    arrexteriorring[v] = arraytje[v];                    
                }
                var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
                var ring = new LinearRing(arrexteriorring);
                var polygon = new Polygon(ring);
                string geoJson2;

                var serializer2 = GeoJsonSerializer.Create();
                using (var stringWriter = new StringWriter())
                using (var jsonWriter = new JsonTextWriter(stringWriter))
                {
                    serializer2.Serialize(jsonWriter, polygon);
                    geoJson2 = stringWriter.ToString();
                }
                MessageBox.Show(geoJson2.ToString());

            }
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

