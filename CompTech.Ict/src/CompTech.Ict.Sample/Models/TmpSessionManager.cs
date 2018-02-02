using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CompTech.Ict.Sample.Models
{
    /*public class Session {
        [JsonProperty("id")]
		public int Id { get; set; }
    }*/

    public class SessionManager{
        public int Create(Comp_Graph gr){
            return 0;
        }

        public void Delete(int id){
            
        }
        
        public Comp_Graph Get(int id) {
            var g = new Comp_Graph();
            return g;
        }
    }
}