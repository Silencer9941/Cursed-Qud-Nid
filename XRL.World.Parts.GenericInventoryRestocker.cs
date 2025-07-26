// Assembly-CSharp, Version=2.0.210.20, Culture=neutral, PublicKeyToken=null
// XRL.World.Parts.GenericInventoryRestocker
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using XRL;
using XRL.World;
using XRL.World.Capabilities;
using XRL.World.Parts;
using XRL.World.Tinkering;

namespace XRL.World.Parts
{
	[Serializable]
	public class GenericInventoryRestocker : IPart
	{
		public List<string> Tables;

		public List<string> HeroTables;

		public string Table
		{
			get
			{
				if (!Tables.IsNullOrEmpty())
				{
					return Tables[0];
				}
				return null;
			}
			set
			{
				SetTables(ref Tables, value);
			}
		}

		public string HeroTable
		{
			get
			{
				if (!HeroTables.IsNullOrEmpty())
				{
					return HeroTables[0];
				}
				return null;
			}
			set
			{
				SetTables(ref HeroTables, value);
			}
		}

		public override bool SameAs(IPart p)
		{
			return false;
		}

		public override void Write(GameObject Basis, SerializationWriter Writer)
		{
			Writer.Write(Tables);
			Writer.Write(HeroTables);
		}

		public override void Read(GameObject Basis, SerializationReader Reader)
		{
			Tables = Reader.ReadStringList();
			HeroTables = Reader.ReadStringList();
		}

		public override bool WantEvent(int ID, int cascade)
		{
			if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<GetDebugInternalsEvent>.ID)
			{
				return ID == SingletonEvent<StartTradeEvent>.ID;
			}
			return true;
		}

		public override bool HandleEvent(GetDebugInternalsEvent E)
		{
			E.AddEntry(this, "Tables", Tables.IsNullOrEmpty() ? "None" : string.Join(", ", Tables));
			E.AddEntry(this, "HeroTables", HeroTables.IsNullOrEmpty() ? "None" : string.Join(", ", HeroTables));
			return base.HandleEvent(E);
		}

		public override bool WantTurnTick()
		{
			return true;
		}

		public override bool AllowStaticRegistration()
		{
			return true;
		}

		public override void Register(GameObject Object, IEventRegistrar Registrar)
		{
			Registrar.Register("MadeHero");
			base.Register(Object, Registrar);
		}

		public static Action<GameObject> GetCraftmarkApplication(GameObject Actor)
		{
			HasMakersMark part = Actor.GetPart<HasMakersMark>();
			string mark = part?.Mark;
			if (!mark.IsNullOrEmpty())
			{
				string markColor = part?.Color ?? "R";
				int.TryParse(Actor.GetPropertyOrTag("HeroGenericInventoryBasicBestowalChances"), out var basicBestowalChances);
				int.TryParse(Actor.GetPropertyOrTag("HeroGenericInventoryBasicBestowalPercentage"), out var basicBestowalPercentage);
				return delegate (GameObject obj)
				{
					if (TinkeringHelpers.EligibleForMakersMark(obj))
					{
						int num = 5;
						obj.RequirePart<MakersMark>().AddCrafter(Actor, mark, markColor);
						for (int i = 0; i < basicBestowalChances; i++)
						{
							if (!basicBestowalPercentage.in100())
							{
								break;
							}
							if (RelicGenerator.ApplyBasicBestowal(obj, null, 1, null, Standard: false, ShowInShortDescription: true))
							{
								num += 30;
							}
						}
						obj.RequirePart<Commerce>().Value += num;
					}
				};
			}
			return null;
		}

		public void AddTable(string Table)
		{
			if (Tables == null)
			{
				Tables = new List<string>();
			}
			Tables.Add(Table);
		}

		public void AddHeroTable(string Table)
		{
			if (HeroTables == null)
			{
				HeroTables = new List<string>();
			}
			HeroTables.Add(Table);
		}

		private void SetTables(ref List<string> List, string Value)
		{
			if (Value.IsNullOrEmpty())
			{
				List = null;
				return;
			}
			if (List == null)
			{
				List = new List<string>();
			}
			else
			{
				List.Clear();
			}
			DelimitedEnumeratorChar enumerator = Value.DelimitedBy(',').GetEnumerator();
			while (enumerator.MoveNext())
			{
				ReadOnlySpan<char> current = enumerator.Current;
				if (current.Length == Value.Length)
				{
					List.Add(Value);
					break;
				}
				List.Add(new string(current));
			}
		}
	}
}