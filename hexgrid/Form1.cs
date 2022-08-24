//using GeoLibrary.IO.GeoJson;
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
            foreach (var pf in fcPoints) {
                var foundPoly = featureCollection.FirstOrDefault(f => f.Geometry.Contains(pf.Geometry));
                
                if (foundPoly != null)
                {
                    var fit = target.FirstOrDefault(tf => tf.Attributes["id"] == foundPoly.Attributes["id"]);
                    if (fit != null)
                    {
                        foundPoly = fit;
                        cnt++;
                    }
                    else {
                        cnt = 1;
                    }
                    var attrNames = pf.Attributes.GetNames();
                    foreach (var an in attrNames) {
                        foundPoly.Attributes.Add(an+cnt.ToString(), pf.Attributes[an]);
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

