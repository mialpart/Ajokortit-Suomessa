using Ajokortit.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Data.SQLite;
using System.Data.SqlClient;
using System.Collections;
using Newtonsoft.Json;
using System.Web.UI.HtmlControls;
using static System.Net.Mime.MediaTypeNames;

namespace Ajokortit.Controllers
{
    public class HomeController : Controller
    {
        public static int MAARA_NAISET;
        public static int MAARA_MIEHET;
        public string pathName = "~/Kortit/ajokortit.sqlite";
        
        /// <summary>
        /// Index näkymä
        /// </summary>
        /// <returns>Viewin</returns>
        public ActionResult Index()
        {
            var posti = Request.Form;
            string getPost;

            if (!string.IsNullOrEmpty(posti.Get("SelectedItem")))
            {
                getPost = posti.Get("SelectedItem");
            }
            else getPost = "";

            //Asetetaan viewbagiin alkuun koko suomen tiedot sukupuolittain
            List<DataPoint> dataPoints = Kortit(getPost);
            ViewBag.DataPoints = JsonConvert.SerializeObject(dataPoints);

            //Asetetaan viewbagiin alkuun koko suomen tiedot ikäluokittain
            List<DataPoint> dataPointsIka = KortitIkaluokat(getPost);
            ViewBag.DataPointsIka = JsonConvert.SerializeObject(dataPointsIka);
            

            ViewBag.Title = "Home Page";
            SelectLista model = new SelectLista();
            var maakunnat = kunnat();
            model.Items = GetSelectListItems(maakunnat);
            var a = ViewBag.DataPointsIka;


            return View(model);
        }

        /// <summary>
        /// Haetaan iat JSON-muodossa
        /// </summary>
        /// <param name="a">Maakunta josta tiedot haetaan</param>
        /// <returns></returns>
        public ActionResult jsonSukupuol(string a)
        {
            List<DataPoint> dataPoints = Kortit(a);
            var datPoin = JsonConvert.SerializeObject(dataPoints);
            return Json(datPoin, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Haetaan sukupuolet JSON-muodossa
        /// </summary>
        /// <param name="a">Maakunta, josta tiedot halutaan hakea</param>
        /// <returns></returns>
        public ActionResult jsonIka(string a)
        {
            List<DataPoint> dataPointsIka = KortitIkaluokat(a);
            var dataPoin = JsonConvert.SerializeObject(dataPointsIka);
            return Json(dataPoin, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Piirakkakaavio sukupuolen mukaan. Haetaan tiedot tietokannasta
        /// </summary>
        /// <returns>Tiedot sukupuolittain listassa</returns>
        public List<DataPoint> Kortit( string maakunta)
        {
            MAARA_MIEHET = sukupuoli("Mies", maakunta);
            MAARA_NAISET = sukupuoli("Nainen", maakunta);

            var lista = new List<DataPoint> {
                new DataPoint("Mies", MAARA_MIEHET),
                new DataPoint("Nainen", MAARA_NAISET),};
            
            return lista;
        }


        /// <summary>
        /// Histogrammi ian mukaan. Haetaan tiedot tietokannasta
        /// </summary>
        /// <returns>Tiedot ikäluokittain listassa</returns>
        public List<DataPoint> KortitIkaluokat(string maakunta)
        {
            
            var teinit = ika(19, maakunta);
            var nuoret = iat(20,29, maakunta);
            var keskinuoret = iat(30, 39, maakunta);
            var keski_ika = iat(40, 49, maakunta);
            var keskivanha = iat(50, 59, maakunta);
            var varhaiselake = iat(60, 69, maakunta);
            var elake = ika(70, maakunta);

            var lista = new List<DataPoint> {
                new DataPoint("15-19 vuotiaat", teinit),
                new DataPoint("20-29 vuotiaat", nuoret),
                new DataPoint("30-39 vuotiaat", keskinuoret),
                new DataPoint("40-49 vuotiaat", keski_ika),
                new DataPoint("50-59 vuotiaat", keskivanha),
                new DataPoint("60-69 vuotiaat", varhaiselake),
                new DataPoint("+70 vuotiaat", elake),};

            return lista;
        }

        /// <summary>
        /// Haetaan kaikki maakunnat nimeltä listaan
        /// </summary>
        /// <returns>Palauttaa kaikki maakunnat dropdown listaan</returns>
        public IEnumerable<string> kunnat()
        {
            SQLiteDataReader reader;

            string fileName = Server.MapPath(pathName);

            //Avataan tietokanta
            var conn = new System.Data.SQLite.SQLiteConnection(fileName, true);
            conn.ConnectionString = @"Data Source = " + fileName + "; Verion = 3;";
            conn.Open();
            
            //Haetaan maakuntien nimet tietokannasta
            string maakunnatSql = "SELECT DISTINCT maakuntanimifi FROM ajokortit JOIN kunnat WHERE kuntanro = kunta";

            SQLiteCommand commKunnat = new SQLiteCommand(maakunnatSql, conn);
            reader = commKunnat.ExecuteReader();
            var lista = new List<string>();
            lista.Add("Yhteensä");
            while (reader.Read())
            {
                var kuntaLista = new ArrayList();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    kuntaLista.Add(reader.GetValue(i));
                    lista.Add(kuntaLista[i].ToString());
                }
            }

            conn.Close();
            return lista; //Palauttaa listassa kaikkien maakuntien nimet
        }

        /// <summary>
        /// Otetaan valittu itemi listasta. Sen mukaan haetaan myöhemmine tiedot tietokannasta
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        private IEnumerable<SelectListItem> GetSelectListItems(IEnumerable<string> elements)
        {
            // Create an empty list to hold result of the operation
            var selectList = new List<SelectListItem>();

            // For each string in the 'elements' variable, create a new SelectListItem object
            // that has both its Value and Text properties set to a particular value.
            // This will result in MVC rendering each item as:
            //     <option value="State Name">State Name</option>
            foreach (var element in elements)
            {
                selectList.Add(new SelectListItem
                {
                    Value = element,
                    Text = element
                });
            }
            return selectList;
        }


        /// <summary>
        /// Haetaan sekä nuorimpien että vanhimpien määrät
        /// </summary>
        /// <param name="ika">Ikä vuosina</param>
        /// <returns></returns>
        public Int32 ika(int ika, string maakunta)
        {
            string fileName = Server.MapPath(pathName);
            
            //Avataan tietokanta
            var conn = new System.Data.SQLite.SQLiteConnection(fileName, true);
            conn.ConnectionString = @"Data Source = " + fileName + "; Verion = 3;";
            conn.Open();

            string ikaSql;

            //Ehtolause-osio
            if (ika == 19 && (maakunta == "" || maakunta == "Yhteensä" || maakunta == null))
            {
                 ikaSql = "select count(*) from ajokortit where ika <= @Ika";
            }
            else if (ika == 70 && (maakunta == "" || maakunta == "Yhteensä" || maakunta == null))
            {
                ikaSql = "select count(*) from ajokortit where ika >= @Ika";
            }
            else if (ika == 70 && (maakunta != "" || maakunta != "Yhteensä" || maakunta != null))
            {
                ikaSql = "select count(*) from ajokortit inner join kunnat ON ajokortit.kunta = kunnat.kuntanro where ika >= @Ika and maakuntanimifi = @Maakunta";
            }
            else
            {
                ikaSql = "select count(*) from ajokortit inner join kunnat ON ajokortit.kunta = kunnat.kuntanro where ika <= @Ika and maakuntanimifi = @Maakunta";
            }


            SQLiteCommand commIka = new SQLiteCommand(ikaSql, conn);
            SQLiteParameter paramIka = new SQLiteParameter();
            paramIka.ParameterName = "@Ika";
            paramIka.Value = ika;

            if (maakunta != "" && maakunta != "Yhteensä" && maakunta != null)
            {

                SQLiteParameter paramM = new SQLiteParameter();
                paramM.ParameterName = "@Maakunta";
                paramM.Value = maakunta;
                commIka.Parameters.Add(paramM);
            }


            commIka.Parameters.Add(paramIka);
            var maara = commIka.ExecuteScalar();
            int maaraInt = Convert.ToInt32(maara);
            conn.Close();
            return maaraInt;
        }


        /// <summary>
        /// Metodi jossa haetaan määrä tiettyjen ikäluokkien väliltä
        /// </summary>
        /// <param name="min">Minimi- ikä</param>
        /// <param name="max">Maksimi- ikä</param>
        /// <returns></returns>
        public Int32 iat(int min, int max, string maakunta) {

            string fileName = Server.MapPath(pathName);
            var conn = new System.Data.SQLite.SQLiteConnection(fileName, true);
            conn.ConnectionString = @"Data Source = " + fileName + "; Verion = 3;";
            conn.Open();

            string ikaSql2;
            if (maakunta == null || maakunta =="Yhteensä" || maakunta == "")
            {
                ikaSql2 = "select count(*) from ajokortit where ika >= @Ika and ika <= @Ika2";

            } else ikaSql2 = "select count(*) from ajokortit inner join kunnat ON ajokortit.kunta = kunnat.kuntanro where ika >= @Ika and ika <= @Ika2 and maakuntanimifi = @Maakunta";
            

            SQLiteCommand commIka2 = new SQLiteCommand(ikaSql2, conn);
            SQLiteParameter paramIka2 = new SQLiteParameter();
            SQLiteParameter paramIka3 = new SQLiteParameter();
            paramIka2.ParameterName = "@Ika";
            paramIka2.Value = min;
            paramIka3.ParameterName = "@Ika2";
            paramIka3.Value = max;

            if (maakunta != null || maakunta != "Yhteensä" || maakunta != "")
            {
                SQLiteParameter paramMaa = new SQLiteParameter();
                paramMaa.ParameterName = "@Maakunta";
                paramMaa.Value = maakunta;
                commIka2.Parameters.Add(paramMaa);
            }

            commIka2.Parameters.Add(paramIka2);
            commIka2.Parameters.Add(paramIka3);
            var maara = commIka2.ExecuteScalar();
            int maaraInt = Convert.ToInt32(maara);
            conn.Close();
            return (maaraInt);
        }

        /// <summary>
        /// Metodi jossa haetaan määrät sukupuolien mukaan
        /// </summary>
        /// <param name="sukupuoli">Sukupuoli</param>
        /// <returns></returns>
        public int sukupuoli(string sukupuoli, string maakunta)
        {

            string fileName = Server.MapPath(pathName);
            var conn = new System.Data.SQLite.SQLiteConnection(fileName, true);
            conn.ConnectionString = @"Data Source = " + fileName + "; Verion = 3;";
            conn.Open();

            string sql;
            if (maakunta == "" || maakunta == "Yhteensä")
            {
                sql = "select count(*) from ajokortit where sukupuoli = @Sukupuoli ";
            }
            else
            {
                sql = "SELECT COUNT(*) FROM ajokortit inner JOIN kunnat ON ajokortit.kunta = kunnat.kuntanro WHERE sukupuoli = @Sukupuoli AND maakuntanimifi = @Maakunta";
            }

            SQLiteCommand comm = new SQLiteCommand(sql, conn);
            SQLiteParameter param = new SQLiteParameter();
            param.ParameterName = "@Sukupuoli";
            param.Value = sukupuoli;
            comm.Parameters.Add(param);


            if (sql != "" || sql != "Yhteensä")
            {
                SQLiteParameter paramM = new SQLiteParameter();
                paramM.ParameterName = "@Maakunta";
                paramM.Value = maakunta;
                comm.Parameters.Add(paramM);
            }

            var execute = comm.ExecuteScalar();
            int maara = Convert.ToInt32(execute);

            conn.Close();
            return (maara);
        }

    }
}
