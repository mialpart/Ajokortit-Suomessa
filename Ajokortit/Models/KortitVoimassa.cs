
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ajokortit.Models
{
    /// <summary>
    /// Voimassa olevien ajokorttien luokka
    /// </summary>
    public class KortitVoimassa
    {
        //[PrimaryKey,AutoIncrement]
        //int ID { get; set; }
        //int Id { get; set; }
        string Ajokorttiluokka { get; set; }
        int Kunta{ get; set; }
        string Sukupuoli { get; set; }
        int Ika{ get; set; }
        int eeKdi_120{ get; set; }
        int eeKdi_121 { get; set; }

    }
}