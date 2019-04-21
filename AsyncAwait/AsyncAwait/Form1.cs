using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;

namespace AsyncAwait
{
    public partial class Form1 : Form
    {
        private List<Airport> _airportList;

        public Form1()
        {
            InitializeComponent();
            cbLoadFile.Checked = true;
            _airportList = null;
        }

        private async void Search_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;
            tbNotes.Text = null;

            if (string.IsNullOrWhiteSpace(tbCountry.Text)) {
                tbCountry.Text = "United States";
            }

            if (_airportList == null)
            {
                if (cbLoadFile.Checked)
                {
                    _airportList = await GetAirportsFromFile().ConfigureAwait(true);
                }
                else
                {
                    _airportList = await GetAirportsOnline().ConfigureAwait(true);
                }
            }

            if (_airportList != null)
            {
                dataGridView1.DataSource = _airportList.FindAll(x => x.country.Contains(tbCountry.Text));
            }
            dataGridView1.Refresh();
       }

        private async Task<List<Airport>> GetAirportsOnline()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(tbURL.Text).ConfigureAwait(false);
                try
                {
                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return JsonConvert.DeserializeObject<List<Airport>>(content);
                }
                catch (Exception ex)
                {
                    tbNotes.Text += ex.Message;
                }
                return null;
            }
        }

        private async Task<List<Airport>> GetAirportsFromFile()
        {
            return await Task.Run(async () =>
            {
                var airports = new List<Airport>();
                using (var strReader = new StreamReader(File.OpenRead($"..\\..\\Data\\Airports.txt")))
                {
                    string line;
                    while(( line = await strReader.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        Airport apt = GetAirport(line);
                        if (apt != null) airports.Add(apt);
                    }
                }
                return airports;
            }).ConfigureAwait(false);
        }

        private Airport GetAirport(string line)
        {
            string[] vals = line.Split(',');

            if (vals.Length != 18) return null;

            return  new Airport()
            {
                code = vals[0],
                lat = vals[1],
                lon = vals[2],
                name = vals[3],
                city = vals[4],
                state = vals[5],
                country = vals[6],
                woeid = vals[7],
                tz = vals[8],
                phone = vals[9],
                type = vals[10],
                email = vals[11],
                url = vals[12],
                runway_length = vals[13],
                elev = vals[14],
                icao = vals[15],
                direct_flights = vals[16],
                carriers = vals[17]
            };
        }
    }
}
