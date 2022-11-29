//---------------------------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated by T4Model template for T4 (https://github.com/linq2db/linq2db).
//    Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
// </auto-generated>
//---------------------------------------------------------------------------------------------------

#pragma warning disable 1591

using System;
using System.Linq;

using LinqToDB;
using LinqToDB.Mapping;

namespace ME.DB
{
	/// <summary>
	/// Database       : MEDB
	/// Data Source    : MEDB
	/// Server Version : 3.24.0
	/// </summary>
	public partial class MEDBDB : LinqToDB.Data.DataConnection
	{
		public ITable<PlatformAction> PlatformActions { get { return this.GetTable<PlatformAction>(); } }

		public MEDBDB()
		{
			InitDataContext();
			InitMappingSchema();
		}

		public MEDBDB(string configuration)
			: base(configuration)
		{
			InitDataContext();
			InitMappingSchema();
		}

		partial void InitDataContext  ();
		partial void InitMappingSchema();
	}

	[Table("PlatformAction")]
	public partial class PlatformAction
	{
		[Column("id"),   PrimaryKey,  NotNull] public string Id   { get; set; } // varchar(36)
		[Column("name"),    Nullable         ] public string Name { get; set; } // varchar(36)
	}

	public static partial class TableExtensions
	{
		public static PlatformAction Find(this ITable<PlatformAction> table, string Id)
		{
			return table.FirstOrDefault(t =>
				t.Id == Id);
		}
	}
}

#pragma warning restore 1591
