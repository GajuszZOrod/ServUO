#region References
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using Server.Diagnostics;
#endregion

namespace Server
{
	public static class ScriptCompiler
	{
		public static Assembly[] Assemblies { get; set; }

		public static bool Compile(bool debug, bool cache)
		{
			try
			{
				var assemblies = new HashSet<Assembly>
				{
					typeof(ScriptCompiler).Assembly,
					Assembly.LoadFrom("Scripts.dll"),
				};

				Assemblies = assemblies.ToArray();

				Console.WriteLine("Core: Verifying entity type serialization...");

				var watch = Stopwatch.StartNew();

				Core.VerifySerialization();

				watch.Stop();

				Console.WriteLine($"Core: Verified {Core.ScriptItems} item and {Core.ScriptMobiles} mobile types");

				return true;
			}
			catch (Exception ex)
			{
				ExceptionLogging.LogException(ex);
				return false;
			}
		}

		public static void Invoke(string method)
		{
			var invoke = new List<MethodInfo>();

			foreach (var a in Assemblies)
			{
				var types = a.GetTypes();

				foreach (var t in types)
				{
					var m = t.GetMethod(method, BindingFlags.Static | BindingFlags.Public);

					if (m != null)
					{
						invoke.Add(m);
					}
				}
			}

			invoke.Sort(new CallPriorityComparer());

			foreach (var m in invoke)
			{
				m.Invoke(null, null);
			}
		}

		private static readonly Dictionary<Assembly, TypeCache> m_TypeCaches = new Dictionary<Assembly, TypeCache>();
		private static TypeCache m_NullCache;

		public static TypeCache GetTypeCache(Assembly asm)
		{
			if (asm == null)
			{
				return m_NullCache ?? (m_NullCache = new TypeCache(null));
			}


			m_TypeCaches.TryGetValue(asm, out var c);

			if (c == null)
			{
				m_TypeCaches[asm] = c = new TypeCache(asm);
			}

			return c;
		}

		public static int FindHashByName(string name)
		{
			if (String.IsNullOrWhiteSpace(name))
			{
				return 0;
			}

			var hash = 0;

			for (var i = 0; hash == 0 && i < Assemblies.Length; ++i)
			{
				hash = GetTypeCache(Assemblies[i]).GetTypeHashByName(name);
			}

			return hash != 0 ? hash : GetTypeCache(Core.Assembly).GetTypeHashByName(name);
		}

		public static int FindHashByFullName(string fullName)
		{
			if (String.IsNullOrWhiteSpace(fullName))
			{
				return 0;
			}

			var hash = 0;

			for (var i = 0; hash == 0 && i < Assemblies.Length; ++i)
			{
				hash = GetTypeCache(Assemblies[i]).GetTypeHashByFullName(fullName);
			}

			return hash != 0 ? hash : GetTypeCache(Core.Assembly).GetTypeHashByFullName(fullName);
		}

		public static Type FindTypeByFullName(string fullName)
		{
			return FindTypeByFullName(fullName, true);
		}

		public static Type FindTypeByFullName(string fullName, bool ignoreCase)
		{
			if (String.IsNullOrWhiteSpace(fullName))
			{
				return null;
			}

			Type type = null;

			for (var i = 0; type == null && i < Assemblies.Length; ++i)
			{
				type = GetTypeCache(Assemblies[i]).GetTypeByFullName(fullName, ignoreCase);
			}

			return type ?? GetTypeCache(Core.Assembly).GetTypeByFullName(fullName, ignoreCase);
		}

		public static IEnumerable<Type> FindTypesByFullName(string name)
		{
			return FindTypesByFullName(name, true);
		}

		public static IEnumerable<Type> FindTypesByFullName(string name, bool ignoreCase)
		{
			if (String.IsNullOrWhiteSpace(name))
			{
				yield break;
			}

			for (var i = 0; i < Assemblies.Length; ++i)
			{
				foreach (var t in GetTypeCache(Assemblies[i]).GetTypesByFullName(name, ignoreCase))
				{
					yield return t;
				}
			}

			foreach (var t in GetTypeCache(Core.Assembly).GetTypesByFullName(name, ignoreCase))
			{
				yield return t;
			}
		}

		public static Type FindTypeByName(string name)
		{
			return FindTypeByName(name, true);
		}

		public static Type FindTypeByName(string name, bool ignoreCase)
		{
			if (String.IsNullOrWhiteSpace(name))
			{
				return null;
			}

			Type type = null;

			for (var i = 0; type == null && i < Assemblies.Length; ++i)
			{
				type = GetTypeCache(Assemblies[i]).GetTypeByName(name, ignoreCase);
			}

			return type ?? GetTypeCache(Core.Assembly).GetTypeByName(name, ignoreCase);
		}

		public static IEnumerable<Type> FindTypesByName(string name)
		{
			return FindTypesByName(name, true);
		}

		public static IEnumerable<Type> FindTypesByName(string name, bool ignoreCase)
		{
			if (String.IsNullOrWhiteSpace(name))
			{
				yield break;
			}

			for (var i = 0; i < Assemblies.Length; ++i)
			{
				foreach (var t in GetTypeCache(Assemblies[i]).GetTypesByName(name, ignoreCase))
				{
					yield return t;
				}
			}

			foreach (var t in GetTypeCache(Core.Assembly).GetTypesByName(name, ignoreCase))
			{
				yield return t;
			}
		}

		public static Type FindTypeByNameHash(int hash)
		{
			Type type = null;

			for (var i = 0; type == null && i < Assemblies.Length; ++i)
			{
				type = GetTypeCache(Assemblies[i]).GetTypeByNameHash(hash);
			}

			return type ?? GetTypeCache(Core.Assembly).GetTypeByNameHash(hash);
		}

		public static IEnumerable<Type> FindTypesByNameHash(int hash)
		{
			for (var i = 0; i < Assemblies.Length; ++i)
			{
				foreach (var t in GetTypeCache(Assemblies[i]).GetTypesByNameHash(hash))
				{
					yield return t;
				}
			}

			foreach (var t in GetTypeCache(Core.Assembly).GetTypesByNameHash(hash))
			{
				yield return t;
			}
		}

		public static Type FindTypeByFullNameHash(int hash)
		{
			Type type = null;

			for (var i = 0; type == null && i < Assemblies.Length; ++i)
			{
				type = GetTypeCache(Assemblies[i]).GetTypeByFullNameHash(hash);
			}

			return type ?? GetTypeCache(Core.Assembly).GetTypeByFullNameHash(hash);
		}

		public static IEnumerable<Type> FindTypesByFullNameHash(int hash)
		{
			for (var i = 0; i < Assemblies.Length; ++i)
			{
				foreach (var t in GetTypeCache(Assemblies[i]).GetTypesByFullNameHash(hash))
				{
					yield return t;
				}
			}

			foreach (var t in GetTypeCache(Core.Assembly).GetTypesByFullNameHash(hash))
			{
				yield return t;
			}
		}
	}

	public class TypeCache
	{
		private readonly Type[] m_Types;
		private readonly TypeTable m_Names;
		private readonly TypeTable m_FullNames;

		public Type[] Types => m_Types;
		public TypeTable Names => m_Names;
		public TypeTable FullNames => m_FullNames;

		public Type GetTypeByNameHash(int hash)
		{
			return GetTypesByNameHash(hash).FirstOrDefault(t => t != null);
		}

		public IEnumerable<Type> GetTypesByNameHash(int hash)
		{
			return m_Names.Get(hash);
		}

		public Type GetTypeByFullNameHash(int hash)
		{
			return GetTypesByFullNameHash(hash).FirstOrDefault(t => t != null);
		}

		public IEnumerable<Type> GetTypesByFullNameHash(int hash)
		{
			return m_FullNames.Get(hash);
		}

		public Type GetTypeByName(string name, bool ignoreCase)
		{
			return GetTypesByName(name, ignoreCase).FirstOrDefault(t => t != null);
		}

		public IEnumerable<Type> GetTypesByName(string name, bool ignoreCase)
		{
			return m_Names.Get(name, ignoreCase);
		}

		public Type GetTypeByFullName(string fullName, bool ignoreCase)
		{
			return GetTypesByFullName(fullName, ignoreCase).FirstOrDefault(t => t != null);
		}

		public IEnumerable<Type> GetTypesByFullName(string fullName, bool ignoreCase)
		{
			return m_FullNames.Get(fullName, ignoreCase);
		}

		public int GetTypeHashByName(string name)
		{
			return m_Names.GetHash(name);
		}

		public int GetTypeHashByFullName(string fullName)
		{
			return m_FullNames.GetHash(fullName);
		}

		public TypeCache(Assembly asm)
		{
			if (asm == null)
			{
				m_Types = Type.EmptyTypes;
			}
			else
			{
				m_Types = asm.GetTypes();
			}

			m_Names = new TypeTable(m_Types.Length);
			m_FullNames = new TypeTable(m_Types.Length);

			foreach (var g in m_Types.ToLookup(t => t.Name))
			{
				m_Names.Add(g.Key, g);

				foreach (var type in g)
				{
					m_FullNames.Add(type.FullName, type);

					var attr = type.GetCustomAttribute<TypeAliasAttribute>(false);

					if (attr != null)
					{
						foreach (var a in attr.Aliases)
						{
							m_FullNames.Add(a, type);
						}
					}
				}
			}

			m_Names.Sort();
			m_FullNames.Sort();
		}
	}

	public class TypeTable
	{
		private readonly Dictionary<string, int> m_Hashes;
		private readonly Dictionary<int, HashSet<Type>> m_Hashed;
		private readonly Dictionary<string, HashSet<Type>> m_Sensitive;
		private readonly Dictionary<string, HashSet<Type>> m_Insensitive;

		public void Sort()
		{
			Sort(m_Hashed);
			Sort(m_Sensitive);
			Sort(m_Insensitive);
		}

		private static void Sort<T>(Dictionary<T, HashSet<Type>> types)
		{
			var sorter = new List<Type>();

			foreach (var list in types.Values)
			{
				sorter.AddRange(list);
				sorter.Sort(InternalSort);

				list.Clear();
				list.UnionWith(sorter);

				sorter.Clear();
			}

			sorter.TrimExcess();
		}

		private static int InternalSort(Type l, Type r)
		{
			if (l == r)
			{
				return 0;
			}

			if (l != null && r == null)
			{
				return -1;
			}

			if (l == null && r != null)
			{
				return 1;
			}

			var a = IsEntity(l);
			var b = IsEntity(r);

			if (a && b)
			{
				a = IsConstructable(l, out var x);
				b = IsConstructable(r, out var y);

				if (a && !b)
				{
					return -1;
				}

				if (!a && b)
				{
					return 1;
				}

				return x > y ? -1 : x < y ? 1 : 0;
			}

			return a ? -1 : b ? 1 : 0;
		}

		private static bool IsEntity(Type type)
		{
			return type.GetInterface("IEntity") != null;
		}

		private static bool IsConstructable(Type type, out AccessLevel access)
		{
			foreach (var ctor in type.GetConstructors().OrderBy(o => o.GetParameters().Length))
			{
				var attr = ctor.GetCustomAttribute<ConstructableAttribute>(false);

				if (attr != null)
				{
					access = attr.AccessLevel;
					return true;
				}
			}

			access = 0;
			return false;
		}

		public void Add(string key, IEnumerable<Type> types)
		{
			if (!String.IsNullOrWhiteSpace(key) && types != null)
			{
				Add(key, types.ToArray());
			}
		}

		public void Add(string key, params Type[] types)
		{
			if (String.IsNullOrWhiteSpace(key) || types == null || types.Length == 0)
			{
				return;
			}

			if (!m_Sensitive.TryGetValue(key, out var sensitive) || sensitive == null)
			{
				m_Sensitive[key] = new HashSet<Type>(types);
			}
			else if (types.Length == 1)
			{
				sensitive.Add(types[0]);
			}
			else
			{
				sensitive.UnionWith(types);
			}

			if (!m_Insensitive.TryGetValue(key, out var insensitive) || insensitive == null)
			{
				m_Insensitive[key] = new HashSet<Type>(types);
			}
			else if (types.Length == 1)
			{
				insensitive.Add(types[0]);
			}
			else
			{
				insensitive.UnionWith(types);
			}

			var hash = GenerateHash(key);

			if (!m_Hashed.TryGetValue(hash, out var hashed) || hashed == null)
			{
				m_Hashed[hash] = new HashSet<Type>(types);
			}
			else if (types.Length == 1)
			{
				hashed.Add(types[0]);
			}
			else
			{
				hashed.UnionWith(types);
			}
		}

		public IEnumerable<Type> Get(string key, bool ignoreCase)
		{
			if (String.IsNullOrWhiteSpace(key))
			{
				return Type.EmptyTypes;
			}

			HashSet<Type> t;

			if (ignoreCase)
			{
				m_Insensitive.TryGetValue(key, out t);
			}
			else
			{
				m_Sensitive.TryGetValue(key, out t);
			}

			if (t == null)
			{
				return Type.EmptyTypes;
			}

			return t.AsEnumerable();
		}

		public IEnumerable<Type> Get(int hash)
		{
			m_Hashed.TryGetValue(hash, out var t);

			if (t == null)
			{
				return Type.EmptyTypes;
			}

			return t.AsEnumerable();
		}

		public int GetHash(string key)
		{
			if (String.IsNullOrWhiteSpace(key))
			{
				return 0;
			}

			m_Hashes.TryGetValue(key, out var hash);

			return hash;
		}

		private int GenerateHash(string key)
		{
			if (String.IsNullOrWhiteSpace(key))
			{
				return 0;
			}

			var hash = GetHash(key);

			if (hash != 0)
			{
				return hash;
			}

			hash = key.Length;

			unchecked
			{
				for (var i = 0; i < key.Length; i++)
				{
					hash = (hash * 397) ^ Convert.ToInt32(key[i]);
				}
			}

			m_Hashes[key] = hash;

			return hash;
		}

		public TypeTable(int capacity)
		{
			m_Hashes = new Dictionary<string, int>();
			m_Hashed = new Dictionary<int, HashSet<Type>>(capacity);
			m_Sensitive = new Dictionary<string, HashSet<Type>>(capacity);
			m_Insensitive = new Dictionary<string, HashSet<Type>>(capacity, StringComparer.OrdinalIgnoreCase);
		}
	}
}
