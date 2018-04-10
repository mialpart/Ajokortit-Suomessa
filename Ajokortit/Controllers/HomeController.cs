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

namespace Ajokortit.Controllers
{
    public class HomeController : Controller
    {
        public static int MAARA_NAISET;
        public static int MAARA_MIEHET;
        public string pathName = "~/Kortit/ajokortit.sqlite";
        

        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";
            
                return View();
        }

        /// <summary>
        /// Piirakkakaavio sukupuolen mukaan
        /// </summary>
        /// <returns></returns>
        public ActionResult Kortit()
        {
            MAARA_MIEHET = sukupuoli("Mies");
            MAARA_NAISET = sukupuoli("Nainen");

            string myTheme = @"<Chart BackColor= ""Transparent"" >
                           <ChartAreas>
                               <ChartArea Name=""Default"" BackColor=""Transparent""></ChartArea> 
                           </ChartAreas>
                           </Chart>";

           new Chart(width: 350, height: 350, theme: myTheme).AddSeries(
               chartType: "pie",
               xValue: new[] {"Mies \n" + MAARA_MIEHET +"",  "Nainen \n"+ MAARA_NAISET+""},
               yValues: new[] { MAARA_MIEHET, MAARA_NAISET}).Write("png");
            
            return null;
        }


        /// <summary>
        /// Histogrammi ian mukaan
        /// </summary>
        /// <returns>Graafi, jossa voimassa olevat ajokortit ikäluokittain</returns>
        public ActionResult KortitIkaluokat()
        {
            string fileName = Server.MapPath(pathName);

            var conn = new System.Data.SQLite.SQLiteConnection(fileName, true);
            conn.ConnectionString = @"Data Source = " + fileName + "; Verion = 3;";
            conn.Open();

            string ikaSql = "select count(*) from ajokortit where ika <= @Ika";
            SQLiteCommand commIka = new SQLiteCommand(ikaSql, conn);
            SQLiteParameter paramIka = new SQLiteParameter();
            paramIka.ParameterName = "@Ika";
            paramIka.Value = 19;
            commIka.Parameters.Add(paramIka);
            var teinit = commIka.ExecuteScalar();
            Convert.ToInt32(teinit);
            conn.Close();
            
            var nuoret = iat(20,29);
            var keskinuoret = iat(30, 39);
            var keski_ika = iat(40, 49);
            var keskivanha = iat(50, 59);
            var varhaiselake = iat(60, 69);
            
            conn.Open();
            string ikaSql3 = "select count(*) from ajokortit where ika >= @Ika";
            SQLiteCommand commIka3 = new SQLiteCommand(ikaSql3, conn);
            SQLiteParameter paramIka4 = new SQLiteParameter();
            paramIka4.ParameterName = "@Ika";
            paramIka4.Value = 70;
            commIka3.Parameters.Add(paramIka4);
            var elake = commIka3.ExecuteScalar();
            Convert.ToInt32(elake);

            new Chart(width: 800, height: 400).AddSeries(
                chartType: "column",
                xValue: new[] { "15-19 vuotiaat \n" + teinit + "", "20-29 vuotiaat \n" + nuoret + "", "30-39 vuotiaat \n" + keskinuoret + "", "40-49 vuotiaat \n" + keski_ika + "", "50-59 vuotiaat \n" + keskivanha+ "", "60-69 vuotiaat \n" + varhaiselake + "", "+70 vuotiaat \n" + elake + "" },
                yValues: new[] { teinit, nuoret, keskinuoret, keski_ika, keskivanha, varhaiselake,elake }).Write("png");
            
            conn.Close();
            return null;
        }

        /// <summary>
        /// Metodi jossa haetaan määrä tiettyjen ikäluokkien väliltä
        /// </summary>
        /// <param name="min">Minimi- ikä</param>
        /// <param name="max">Maksimi- ikä</param>
        /// <returns></returns>
        public Int32 iat(int min, int max) {

            string fileName = Server.MapPath(pathName);
            var conn = new System.Data.SQLite.SQLiteConnection(fileName, true);
            conn.ConnectionString = @"Data Source = " + fileName + "; Verion = 3;";
            conn.Open();

            string ikaSql2 = "select count(*) from ajokortit where ika >= @Ika and ika <= @Ika2";
            SQLiteCommand commIka2 = new SQLiteCommand(ikaSql2, conn);
            SQLiteParameter paramIka2 = new SQLiteParameter();
            SQLiteParameter paramIka3 = new SQLiteParameter();
            paramIka2.ParameterName = "@Ika";
            paramIka2.Value = min;
            paramIka3.ParameterName = "@Ika2";
            paramIka3.Value = max;
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
        public int sukupuoli(string sukupuoli)
        {
            string fileName = Server.MapPath(pathName);
            var conn = new System.Data.SQLite.SQLiteConnection(fileName, true);
            conn.ConnectionString = @"Data Source = " + fileName + "; Verion = 3;";
            conn.Open();

            string sql = "select count(*) from ajokortit where sukupuoli = @Sukupuoli ";
            SQLiteCommand comm = new SQLiteCommand(sql, conn);
            SQLiteParameter param = new SQLiteParameter();
            param.ParameterName = "@Sukupuoli";
            param.Value = sukupuoli;
            comm.Parameters.Add(param);
            var miehet = comm.ExecuteScalar();
            int maara = Convert.ToInt32(miehet);

            conn.Close();
            return (maara);
        }

    }
}
