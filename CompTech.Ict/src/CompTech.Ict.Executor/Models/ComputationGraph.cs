using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CompTech.Ict.Executor.Models
{
	public class ComputationGraph
	{
		[JsonProperty("dependencies")]
		public int[][] Dependecies { get; set; }

		[JsonProperty("operations")]
		public List<Operation> Operations { get; set; }

		[JsonProperty("mnemonicsTable")]
		public Dictionary<String, MnemonicsValue> MnemonicsValues { get; set; }
	}
	public class Operation
	{
		[JsonProperty("id")]
		public int Id { get; set; }
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("input")]
		public string [] Input { get; set; }
		[JsonProperty("output")]
		public string [] Output { get; set; }
		[JsonProperty("parameters")]
		public List<DataType> Parameters { get; set; }

	}
	public class DataType
	{
		[JsonProperty("name")]
		public string Name { get; set; }
		//parameters??
	}
	public class MnemonicsValue
	{
		[JsonProperty("value")]
		public string Value { get; set; }
		[JsonProperty("dataType")]
		public DataType Type { get; set; }
	}
}
