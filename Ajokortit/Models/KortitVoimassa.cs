
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ajokortit.Models
{
    /// <summary>
    /// Voimassa olevien ajokorttien luokka
    /// </summary>
    public class KortitVoimassa
    {

        string Ajokorttiluokka { get; set; }
        int Kunta{ get; set; }
        string Sukupuoli { get; set; }
        int Ika{ get; set; }
        int eeKdi_120{ get; set; }
        int eeKdi_121 { get; set; }

    }

    /// <summary>
    /// Lista jossa kaikki maakunnat.
    /// </summary>
    public class SelectLista
    {
        public string SelectedItem { get; set; }
        public IEnumerable<SelectListItem> Items { get; set; }
    }
    

}