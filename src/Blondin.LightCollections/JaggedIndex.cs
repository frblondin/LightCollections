using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blondin.LightCollections
{
	public interface IJaggedIndex<T>
	{
		int Depth { get; }
		T this[int index] { get; }
		T[] GetValues();
	}

	public static partial class JaggedIndex
	{
		public static IJaggedIndex<T> Create<T>(params T[] keys)
		{
			return Create((IList<T>)keys);
		}

		public static IJaggedIndex<T> Create<T>(IList<T> keys)
		{
			switch (keys.Count)
			{
				case 1: return Create<T>(keys[0]);
				case 2: return Create<T>(keys[0], keys[1]);
				case 3: return Create<T>(keys[0], keys[1], keys[2]);
				case 4: return Create<T>(keys[0], keys[1], keys[2], keys[3]);
				case 5: return Create<T>(keys[0], keys[1], keys[2], keys[3], keys[4]);
				case 6: return Create<T>(keys[0], keys[1], keys[2], keys[3], keys[4], keys[5]);
				case 7: return Create<T>(keys[0], keys[1], keys[2], keys[3], keys[4], keys[5], keys[6]);
				case 8: return Create<T>(keys[0], keys[1], keys[2], keys[3], keys[4], keys[5], keys[6], keys[7]);
				case 9: return Create<T>(keys[0], keys[1], keys[2], keys[3], keys[4], keys[5], keys[6], keys[7], keys[8]);
				case 10: return Create<T>(keys[0], keys[1], keys[2], keys[3], keys[4], keys[5], keys[6], keys[7], keys[8], keys[9]);
				case 11: return Create<T>(keys[0], keys[1], keys[2], keys[3], keys[4], keys[5], keys[6], keys[7], keys[8], keys[9], keys[10]);
				case 12: return Create<T>(keys[0], keys[1], keys[2], keys[3], keys[4], keys[5], keys[6], keys[7], keys[8], keys[9], keys[10], keys[11]);
				case 13: return Create<T>(keys[0], keys[1], keys[2], keys[3], keys[4], keys[5], keys[6], keys[7], keys[8], keys[9], keys[10], keys[11], keys[12]);
				case 14: return Create<T>(keys[0], keys[1], keys[2], keys[3], keys[4], keys[5], keys[6], keys[7], keys[8], keys[9], keys[10], keys[11], keys[12], keys[13]);
				case 15: return Create<T>(keys[0], keys[1], keys[2], keys[3], keys[4], keys[5], keys[6], keys[7], keys[8], keys[9], keys[10], keys[11], keys[12], keys[13], keys[14]);
				default: throw new ArgumentOutOfRangeException();
			}
		}
	}

	public static partial class JaggedIndex
	{
		public static JaggedIndex1<T> Create<T>(T key0)
		{
			return new JaggedIndex1<T>(key0);
		}
	}

	public struct JaggedIndex1<T> : IJaggedIndex<T>, IEquatable<JaggedIndex1<T>>
	{
		public readonly T Key0;

		public int Depth { get { return 1; } }

		public T this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return this.Key0;
					default: throw new ArgumentOutOfRangeException();
				}
			}
		}

		public JaggedIndex1(T key0)
		{
			this.Key0 = key0;
		}

		public T[] GetValues()
		{
			return new[] { this.Key0 };
		}

		public override bool Equals(object key)
		{
            if (!(key is JaggedIndex1<T>)) return false;
            return Equals((JaggedIndex1<T>)key);
		}

		public bool Equals(JaggedIndex1<T> key)
		{
			var comparer = EqualityComparer<T>.Default;
			if (!comparer.Equals(this.Key0, key.Key0)) return false;
			return true;
		}

        public override int GetHashCode()
        {
            return Key0.GetHashCode();
        }

        public override string ToString()
        {
			return string.Format("[ {0} ]",
				Key0);
        }
	}
	public static partial class JaggedIndex
	{
		public static JaggedIndex2<T> Create<T>(T key0, T key1)
		{
			return new JaggedIndex2<T>(key0, key1);
		}
	}

	public struct JaggedIndex2<T> : IJaggedIndex<T>, IEquatable<JaggedIndex2<T>>
	{
		public readonly T Key0; public readonly T Key1;

		public int Depth { get { return 2; } }

		public T this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return this.Key0;
					case 1: return this.Key1;
					default: throw new ArgumentOutOfRangeException();
				}
			}
		}

		public JaggedIndex2(T key0, T key1)
		{
			this.Key0 = key0; this.Key1 = key1;
		}

		public T[] GetValues()
		{
			return new[] { this.Key0, this.Key1 };
		}

		public override bool Equals(object key)
		{
            if (!(key is JaggedIndex2<T>)) return false;
            return Equals((JaggedIndex2<T>)key);
		}

		public bool Equals(JaggedIndex2<T> key)
		{
			var comparer = EqualityComparer<T>.Default;
			if (!comparer.Equals(this.Key0, key.Key0)) return false;
			if (!comparer.Equals(this.Key1, key.Key1)) return false;
			return true;
		}

        public override int GetHashCode()
        {
            return TupleHack.CombineHashCodes2(
				Key0.GetHashCode(), Key1.GetHashCode());
        }

        public override string ToString()
        {
			return string.Format("[ {0}, {1} ]",
				Key0, Key1);
        }
	}
	public static partial class JaggedIndex
	{
		public static JaggedIndex3<T> Create<T>(T key0, T key1, T key2)
		{
			return new JaggedIndex3<T>(key0, key1, key2);
		}
	}

	public struct JaggedIndex3<T> : IJaggedIndex<T>, IEquatable<JaggedIndex3<T>>
	{
		public readonly T Key0; public readonly T Key1; public readonly T Key2;

		public int Depth { get { return 3; } }

		public T this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return this.Key0;
					case 1: return this.Key1;
					case 2: return this.Key2;
					default: throw new ArgumentOutOfRangeException();
				}
			}
		}

		public JaggedIndex3(T key0, T key1, T key2)
		{
			this.Key0 = key0; this.Key1 = key1; this.Key2 = key2;
		}

		public T[] GetValues()
		{
			return new[] { this.Key0, this.Key1, this.Key2 };
		}

		public override bool Equals(object key)
		{
            if (!(key is JaggedIndex3<T>)) return false;
            return Equals((JaggedIndex3<T>)key);
		}

		public bool Equals(JaggedIndex3<T> key)
		{
			var comparer = EqualityComparer<T>.Default;
			if (!comparer.Equals(this.Key0, key.Key0)) return false;
			if (!comparer.Equals(this.Key1, key.Key1)) return false;
			if (!comparer.Equals(this.Key2, key.Key2)) return false;
			return true;
		}

        public override int GetHashCode()
        {
            return TupleHack.CombineHashCodes3(
				Key0.GetHashCode(), Key1.GetHashCode(), Key2.GetHashCode());
        }

        public override string ToString()
        {
			return string.Format("[ {0}, {1}, {2} ]",
				Key0, Key1, Key2);
        }
	}
	public static partial class JaggedIndex
	{
		public static JaggedIndex4<T> Create<T>(T key0, T key1, T key2, T key3)
		{
			return new JaggedIndex4<T>(key0, key1, key2, key3);
		}
	}

	public struct JaggedIndex4<T> : IJaggedIndex<T>, IEquatable<JaggedIndex4<T>>
	{
		public readonly T Key0; public readonly T Key1; public readonly T Key2; public readonly T Key3;

		public int Depth { get { return 4; } }

		public T this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return this.Key0;
					case 1: return this.Key1;
					case 2: return this.Key2;
					case 3: return this.Key3;
					default: throw new ArgumentOutOfRangeException();
				}
			}
		}

		public JaggedIndex4(T key0, T key1, T key2, T key3)
		{
			this.Key0 = key0; this.Key1 = key1; this.Key2 = key2; this.Key3 = key3;
		}

		public T[] GetValues()
		{
			return new[] { this.Key0, this.Key1, this.Key2, this.Key3 };
		}

		public override bool Equals(object key)
		{
            if (!(key is JaggedIndex4<T>)) return false;
            return Equals((JaggedIndex4<T>)key);
		}

		public bool Equals(JaggedIndex4<T> key)
		{
			var comparer = EqualityComparer<T>.Default;
			if (!comparer.Equals(this.Key0, key.Key0)) return false;
			if (!comparer.Equals(this.Key1, key.Key1)) return false;
			if (!comparer.Equals(this.Key2, key.Key2)) return false;
			if (!comparer.Equals(this.Key3, key.Key3)) return false;
			return true;
		}

        public override int GetHashCode()
        {
            return TupleHack.CombineHashCodes4(
				Key0.GetHashCode(), Key1.GetHashCode(), Key2.GetHashCode(), Key3.GetHashCode());
        }

        public override string ToString()
        {
			return string.Format("[ {0}, {1}, {2}, {3} ]",
				Key0, Key1, Key2, Key3);
        }
	}
	public static partial class JaggedIndex
	{
		public static JaggedIndex5<T> Create<T>(T key0, T key1, T key2, T key3, T key4)
		{
			return new JaggedIndex5<T>(key0, key1, key2, key3, key4);
		}
	}

	public struct JaggedIndex5<T> : IJaggedIndex<T>, IEquatable<JaggedIndex5<T>>
	{
		public readonly T Key0; public readonly T Key1; public readonly T Key2; public readonly T Key3; public readonly T Key4;

		public int Depth { get { return 5; } }

		public T this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return this.Key0;
					case 1: return this.Key1;
					case 2: return this.Key2;
					case 3: return this.Key3;
					case 4: return this.Key4;
					default: throw new ArgumentOutOfRangeException();
				}
			}
		}

		public JaggedIndex5(T key0, T key1, T key2, T key3, T key4)
		{
			this.Key0 = key0; this.Key1 = key1; this.Key2 = key2; this.Key3 = key3; this.Key4 = key4;
		}

		public T[] GetValues()
		{
			return new[] { this.Key0, this.Key1, this.Key2, this.Key3, this.Key4 };
		}

		public override bool Equals(object key)
		{
            if (!(key is JaggedIndex5<T>)) return false;
            return Equals((JaggedIndex5<T>)key);
		}

		public bool Equals(JaggedIndex5<T> key)
		{
			var comparer = EqualityComparer<T>.Default;
			if (!comparer.Equals(this.Key0, key.Key0)) return false;
			if (!comparer.Equals(this.Key1, key.Key1)) return false;
			if (!comparer.Equals(this.Key2, key.Key2)) return false;
			if (!comparer.Equals(this.Key3, key.Key3)) return false;
			if (!comparer.Equals(this.Key4, key.Key4)) return false;
			return true;
		}

        public override int GetHashCode()
        {
            return TupleHack.CombineHashCodes5(
				Key0.GetHashCode(), Key1.GetHashCode(), Key2.GetHashCode(), Key3.GetHashCode(), Key4.GetHashCode());
        }

        public override string ToString()
        {
			return string.Format("[ {0}, {1}, {2}, {3}, {4} ]",
				Key0, Key1, Key2, Key3, Key4);
        }
	}
	public static partial class JaggedIndex
	{
		public static JaggedIndex6<T> Create<T>(T key0, T key1, T key2, T key3, T key4, T key5)
		{
			return new JaggedIndex6<T>(key0, key1, key2, key3, key4, key5);
		}
	}

	public struct JaggedIndex6<T> : IJaggedIndex<T>, IEquatable<JaggedIndex6<T>>
	{
		public readonly T Key0; public readonly T Key1; public readonly T Key2; public readonly T Key3; public readonly T Key4; public readonly T Key5;

		public int Depth { get { return 6; } }

		public T this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return this.Key0;
					case 1: return this.Key1;
					case 2: return this.Key2;
					case 3: return this.Key3;
					case 4: return this.Key4;
					case 5: return this.Key5;
					default: throw new ArgumentOutOfRangeException();
				}
			}
		}

		public JaggedIndex6(T key0, T key1, T key2, T key3, T key4, T key5)
		{
			this.Key0 = key0; this.Key1 = key1; this.Key2 = key2; this.Key3 = key3; this.Key4 = key4; this.Key5 = key5;
		}

		public T[] GetValues()
		{
			return new[] { this.Key0, this.Key1, this.Key2, this.Key3, this.Key4, this.Key5 };
		}

		public override bool Equals(object key)
		{
            if (!(key is JaggedIndex6<T>)) return false;
            return Equals((JaggedIndex6<T>)key);
		}

		public bool Equals(JaggedIndex6<T> key)
		{
			var comparer = EqualityComparer<T>.Default;
			if (!comparer.Equals(this.Key0, key.Key0)) return false;
			if (!comparer.Equals(this.Key1, key.Key1)) return false;
			if (!comparer.Equals(this.Key2, key.Key2)) return false;
			if (!comparer.Equals(this.Key3, key.Key3)) return false;
			if (!comparer.Equals(this.Key4, key.Key4)) return false;
			if (!comparer.Equals(this.Key5, key.Key5)) return false;
			return true;
		}

        public override int GetHashCode()
        {
            return TupleHack.CombineHashCodes6(
				Key0.GetHashCode(), Key1.GetHashCode(), Key2.GetHashCode(), Key3.GetHashCode(), Key4.GetHashCode(), Key5.GetHashCode());
        }

        public override string ToString()
        {
			return string.Format("[ {0}, {1}, {2}, {3}, {4}, {5} ]",
				Key0, Key1, Key2, Key3, Key4, Key5);
        }
	}
	public static partial class JaggedIndex
	{
		public static JaggedIndex7<T> Create<T>(T key0, T key1, T key2, T key3, T key4, T key5, T key6)
		{
			return new JaggedIndex7<T>(key0, key1, key2, key3, key4, key5, key6);
		}
	}

	public struct JaggedIndex7<T> : IJaggedIndex<T>, IEquatable<JaggedIndex7<T>>
	{
		public readonly T Key0; public readonly T Key1; public readonly T Key2; public readonly T Key3; public readonly T Key4; public readonly T Key5; public readonly T Key6;

		public int Depth { get { return 7; } }

		public T this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return this.Key0;
					case 1: return this.Key1;
					case 2: return this.Key2;
					case 3: return this.Key3;
					case 4: return this.Key4;
					case 5: return this.Key5;
					case 6: return this.Key6;
					default: throw new ArgumentOutOfRangeException();
				}
			}
		}

		public JaggedIndex7(T key0, T key1, T key2, T key3, T key4, T key5, T key6)
		{
			this.Key0 = key0; this.Key1 = key1; this.Key2 = key2; this.Key3 = key3; this.Key4 = key4; this.Key5 = key5; this.Key6 = key6;
		}

		public T[] GetValues()
		{
			return new[] { this.Key0, this.Key1, this.Key2, this.Key3, this.Key4, this.Key5, this.Key6 };
		}

		public override bool Equals(object key)
		{
            if (!(key is JaggedIndex7<T>)) return false;
            return Equals((JaggedIndex7<T>)key);
		}

		public bool Equals(JaggedIndex7<T> key)
		{
			var comparer = EqualityComparer<T>.Default;
			if (!comparer.Equals(this.Key0, key.Key0)) return false;
			if (!comparer.Equals(this.Key1, key.Key1)) return false;
			if (!comparer.Equals(this.Key2, key.Key2)) return false;
			if (!comparer.Equals(this.Key3, key.Key3)) return false;
			if (!comparer.Equals(this.Key4, key.Key4)) return false;
			if (!comparer.Equals(this.Key5, key.Key5)) return false;
			if (!comparer.Equals(this.Key6, key.Key6)) return false;
			return true;
		}

        public override int GetHashCode()
        {
            return TupleHack.CombineHashCodes7(
				Key0.GetHashCode(), Key1.GetHashCode(), Key2.GetHashCode(), Key3.GetHashCode(), Key4.GetHashCode(), Key5.GetHashCode(), Key6.GetHashCode());
        }

        public override string ToString()
        {
			return string.Format("[ {0}, {1}, {2}, {3}, {4}, {5}, {6} ]",
				Key0, Key1, Key2, Key3, Key4, Key5, Key6);
        }
	}
	public static partial class JaggedIndex
	{
		public static JaggedIndex8<T> Create<T>(T key0, T key1, T key2, T key3, T key4, T key5, T key6, T key7)
		{
			return new JaggedIndex8<T>(key0, key1, key2, key3, key4, key5, key6, key7);
		}
	}

	public struct JaggedIndex8<T> : IJaggedIndex<T>, IEquatable<JaggedIndex8<T>>
	{
		public readonly T Key0; public readonly T Key1; public readonly T Key2; public readonly T Key3; public readonly T Key4; public readonly T Key5; public readonly T Key6; public readonly T Key7;

		public int Depth { get { return 8; } }

		public T this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return this.Key0;
					case 1: return this.Key1;
					case 2: return this.Key2;
					case 3: return this.Key3;
					case 4: return this.Key4;
					case 5: return this.Key5;
					case 6: return this.Key6;
					case 7: return this.Key7;
					default: throw new ArgumentOutOfRangeException();
				}
			}
		}

		public JaggedIndex8(T key0, T key1, T key2, T key3, T key4, T key5, T key6, T key7)
		{
			this.Key0 = key0; this.Key1 = key1; this.Key2 = key2; this.Key3 = key3; this.Key4 = key4; this.Key5 = key5; this.Key6 = key6; this.Key7 = key7;
		}

		public T[] GetValues()
		{
			return new[] { this.Key0, this.Key1, this.Key2, this.Key3, this.Key4, this.Key5, this.Key6, this.Key7 };
		}

		public override bool Equals(object key)
		{
            if (!(key is JaggedIndex8<T>)) return false;
            return Equals((JaggedIndex8<T>)key);
		}

		public bool Equals(JaggedIndex8<T> key)
		{
			var comparer = EqualityComparer<T>.Default;
			if (!comparer.Equals(this.Key0, key.Key0)) return false;
			if (!comparer.Equals(this.Key1, key.Key1)) return false;
			if (!comparer.Equals(this.Key2, key.Key2)) return false;
			if (!comparer.Equals(this.Key3, key.Key3)) return false;
			if (!comparer.Equals(this.Key4, key.Key4)) return false;
			if (!comparer.Equals(this.Key5, key.Key5)) return false;
			if (!comparer.Equals(this.Key6, key.Key6)) return false;
			if (!comparer.Equals(this.Key7, key.Key7)) return false;
			return true;
		}

        public override int GetHashCode()
        {
            return TupleHack.CombineHashCodes8(
				Key0.GetHashCode(), Key1.GetHashCode(), Key2.GetHashCode(), Key3.GetHashCode(), Key4.GetHashCode(), Key5.GetHashCode(), Key6.GetHashCode(), Key7.GetHashCode());
        }

        public override string ToString()
        {
			return string.Format("[ {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7} ]",
				Key0, Key1, Key2, Key3, Key4, Key5, Key6, Key7);
        }
	}
	public static partial class JaggedIndex
	{
		public static JaggedIndex9<T> Create<T>(T key0, T key1, T key2, T key3, T key4, T key5, T key6, T key7, T key8)
		{
			return new JaggedIndex9<T>(key0, key1, key2, key3, key4, key5, key6, key7, key8);
		}
	}

	public struct JaggedIndex9<T> : IJaggedIndex<T>, IEquatable<JaggedIndex9<T>>
	{
		public readonly T Key0; public readonly T Key1; public readonly T Key2; public readonly T Key3; public readonly T Key4; public readonly T Key5; public readonly T Key6; public readonly T Key7; public readonly T Key8;

		public int Depth { get { return 9; } }

		public T this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return this.Key0;
					case 1: return this.Key1;
					case 2: return this.Key2;
					case 3: return this.Key3;
					case 4: return this.Key4;
					case 5: return this.Key5;
					case 6: return this.Key6;
					case 7: return this.Key7;
					case 8: return this.Key8;
					default: throw new ArgumentOutOfRangeException();
				}
			}
		}

		public JaggedIndex9(T key0, T key1, T key2, T key3, T key4, T key5, T key6, T key7, T key8)
		{
			this.Key0 = key0; this.Key1 = key1; this.Key2 = key2; this.Key3 = key3; this.Key4 = key4; this.Key5 = key5; this.Key6 = key6; this.Key7 = key7; this.Key8 = key8;
		}

		public T[] GetValues()
		{
			return new[] { this.Key0, this.Key1, this.Key2, this.Key3, this.Key4, this.Key5, this.Key6, this.Key7, this.Key8 };
		}

		public override bool Equals(object key)
		{
            if (!(key is JaggedIndex9<T>)) return false;
            return Equals((JaggedIndex9<T>)key);
		}

		public bool Equals(JaggedIndex9<T> key)
		{
			var comparer = EqualityComparer<T>.Default;
			if (!comparer.Equals(this.Key0, key.Key0)) return false;
			if (!comparer.Equals(this.Key1, key.Key1)) return false;
			if (!comparer.Equals(this.Key2, key.Key2)) return false;
			if (!comparer.Equals(this.Key3, key.Key3)) return false;
			if (!comparer.Equals(this.Key4, key.Key4)) return false;
			if (!comparer.Equals(this.Key5, key.Key5)) return false;
			if (!comparer.Equals(this.Key6, key.Key6)) return false;
			if (!comparer.Equals(this.Key7, key.Key7)) return false;
			if (!comparer.Equals(this.Key8, key.Key8)) return false;
			return true;
		}

        public override int GetHashCode()
        {
			var hash = TupleHack.CombineHashCodes8(
				Key0.GetHashCode(), Key1.GetHashCode(), Key2.GetHashCode(), Key3.GetHashCode(), Key4.GetHashCode(), Key5.GetHashCode(), Key6.GetHashCode(), Key7.GetHashCode());
            return TupleHack.CombineHashCodes2(hash,
				Key8.GetHashCode());
        }

        public override string ToString()
        {
			return string.Format("[ {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8} ]",
				Key0, Key1, Key2, Key3, Key4, Key5, Key6, Key7, Key8);
        }
	}
	public static partial class JaggedIndex
	{
		public static JaggedIndex10<T> Create<T>(T key0, T key1, T key2, T key3, T key4, T key5, T key6, T key7, T key8, T key9)
		{
			return new JaggedIndex10<T>(key0, key1, key2, key3, key4, key5, key6, key7, key8, key9);
		}
	}

	public struct JaggedIndex10<T> : IJaggedIndex<T>, IEquatable<JaggedIndex10<T>>
	{
		public readonly T Key0; public readonly T Key1; public readonly T Key2; public readonly T Key3; public readonly T Key4; public readonly T Key5; public readonly T Key6; public readonly T Key7; public readonly T Key8; public readonly T Key9;

		public int Depth { get { return 10; } }

		public T this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return this.Key0;
					case 1: return this.Key1;
					case 2: return this.Key2;
					case 3: return this.Key3;
					case 4: return this.Key4;
					case 5: return this.Key5;
					case 6: return this.Key6;
					case 7: return this.Key7;
					case 8: return this.Key8;
					case 9: return this.Key9;
					default: throw new ArgumentOutOfRangeException();
				}
			}
		}

		public JaggedIndex10(T key0, T key1, T key2, T key3, T key4, T key5, T key6, T key7, T key8, T key9)
		{
			this.Key0 = key0; this.Key1 = key1; this.Key2 = key2; this.Key3 = key3; this.Key4 = key4; this.Key5 = key5; this.Key6 = key6; this.Key7 = key7; this.Key8 = key8; this.Key9 = key9;
		}

		public T[] GetValues()
		{
			return new[] { this.Key0, this.Key1, this.Key2, this.Key3, this.Key4, this.Key5, this.Key6, this.Key7, this.Key8, this.Key9 };
		}

		public override bool Equals(object key)
		{
            if (!(key is JaggedIndex10<T>)) return false;
            return Equals((JaggedIndex10<T>)key);
		}

		public bool Equals(JaggedIndex10<T> key)
		{
			var comparer = EqualityComparer<T>.Default;
			if (!comparer.Equals(this.Key0, key.Key0)) return false;
			if (!comparer.Equals(this.Key1, key.Key1)) return false;
			if (!comparer.Equals(this.Key2, key.Key2)) return false;
			if (!comparer.Equals(this.Key3, key.Key3)) return false;
			if (!comparer.Equals(this.Key4, key.Key4)) return false;
			if (!comparer.Equals(this.Key5, key.Key5)) return false;
			if (!comparer.Equals(this.Key6, key.Key6)) return false;
			if (!comparer.Equals(this.Key7, key.Key7)) return false;
			if (!comparer.Equals(this.Key8, key.Key8)) return false;
			if (!comparer.Equals(this.Key9, key.Key9)) return false;
			return true;
		}

        public override int GetHashCode()
        {
			var hash = TupleHack.CombineHashCodes8(
				Key0.GetHashCode(), Key1.GetHashCode(), Key2.GetHashCode(), Key3.GetHashCode(), Key4.GetHashCode(), Key5.GetHashCode(), Key6.GetHashCode(), Key7.GetHashCode());
            return TupleHack.CombineHashCodes3(hash,
				Key8.GetHashCode(), Key9.GetHashCode());
        }

        public override string ToString()
        {
			return string.Format("[ {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9} ]",
				Key0, Key1, Key2, Key3, Key4, Key5, Key6, Key7, Key8, Key9);
        }
	}
	public static partial class JaggedIndex
	{
		public static JaggedIndex11<T> Create<T>(T key0, T key1, T key2, T key3, T key4, T key5, T key6, T key7, T key8, T key9, T key10)
		{
			return new JaggedIndex11<T>(key0, key1, key2, key3, key4, key5, key6, key7, key8, key9, key10);
		}
	}

	public struct JaggedIndex11<T> : IJaggedIndex<T>, IEquatable<JaggedIndex11<T>>
	{
		public readonly T Key0; public readonly T Key1; public readonly T Key2; public readonly T Key3; public readonly T Key4; public readonly T Key5; public readonly T Key6; public readonly T Key7; public readonly T Key8; public readonly T Key9; public readonly T Key10;

		public int Depth { get { return 11; } }

		public T this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return this.Key0;
					case 1: return this.Key1;
					case 2: return this.Key2;
					case 3: return this.Key3;
					case 4: return this.Key4;
					case 5: return this.Key5;
					case 6: return this.Key6;
					case 7: return this.Key7;
					case 8: return this.Key8;
					case 9: return this.Key9;
					case 10: return this.Key10;
					default: throw new ArgumentOutOfRangeException();
				}
			}
		}

		public JaggedIndex11(T key0, T key1, T key2, T key3, T key4, T key5, T key6, T key7, T key8, T key9, T key10)
		{
			this.Key0 = key0; this.Key1 = key1; this.Key2 = key2; this.Key3 = key3; this.Key4 = key4; this.Key5 = key5; this.Key6 = key6; this.Key7 = key7; this.Key8 = key8; this.Key9 = key9; this.Key10 = key10;
		}

		public T[] GetValues()
		{
			return new[] { this.Key0, this.Key1, this.Key2, this.Key3, this.Key4, this.Key5, this.Key6, this.Key7, this.Key8, this.Key9, this.Key10 };
		}

		public override bool Equals(object key)
		{
            if (!(key is JaggedIndex11<T>)) return false;
            return Equals((JaggedIndex11<T>)key);
		}

		public bool Equals(JaggedIndex11<T> key)
		{
			var comparer = EqualityComparer<T>.Default;
			if (!comparer.Equals(this.Key0, key.Key0)) return false;
			if (!comparer.Equals(this.Key1, key.Key1)) return false;
			if (!comparer.Equals(this.Key2, key.Key2)) return false;
			if (!comparer.Equals(this.Key3, key.Key3)) return false;
			if (!comparer.Equals(this.Key4, key.Key4)) return false;
			if (!comparer.Equals(this.Key5, key.Key5)) return false;
			if (!comparer.Equals(this.Key6, key.Key6)) return false;
			if (!comparer.Equals(this.Key7, key.Key7)) return false;
			if (!comparer.Equals(this.Key8, key.Key8)) return false;
			if (!comparer.Equals(this.Key9, key.Key9)) return false;
			if (!comparer.Equals(this.Key10, key.Key10)) return false;
			return true;
		}

        public override int GetHashCode()
        {
			var hash = TupleHack.CombineHashCodes8(
				Key0.GetHashCode(), Key1.GetHashCode(), Key2.GetHashCode(), Key3.GetHashCode(), Key4.GetHashCode(), Key5.GetHashCode(), Key6.GetHashCode(), Key7.GetHashCode());
            return TupleHack.CombineHashCodes4(hash,
				Key8.GetHashCode(), Key9.GetHashCode(), Key10.GetHashCode());
        }

        public override string ToString()
        {
			return string.Format("[ {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10} ]",
				Key0, Key1, Key2, Key3, Key4, Key5, Key6, Key7, Key8, Key9, Key10);
        }
	}
	public static partial class JaggedIndex
	{
		public static JaggedIndex12<T> Create<T>(T key0, T key1, T key2, T key3, T key4, T key5, T key6, T key7, T key8, T key9, T key10, T key11)
		{
			return new JaggedIndex12<T>(key0, key1, key2, key3, key4, key5, key6, key7, key8, key9, key10, key11);
		}
	}

	public struct JaggedIndex12<T> : IJaggedIndex<T>, IEquatable<JaggedIndex12<T>>
	{
		public readonly T Key0; public readonly T Key1; public readonly T Key2; public readonly T Key3; public readonly T Key4; public readonly T Key5; public readonly T Key6; public readonly T Key7; public readonly T Key8; public readonly T Key9; public readonly T Key10; public readonly T Key11;

		public int Depth { get { return 12; } }

		public T this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return this.Key0;
					case 1: return this.Key1;
					case 2: return this.Key2;
					case 3: return this.Key3;
					case 4: return this.Key4;
					case 5: return this.Key5;
					case 6: return this.Key6;
					case 7: return this.Key7;
					case 8: return this.Key8;
					case 9: return this.Key9;
					case 10: return this.Key10;
					case 11: return this.Key11;
					default: throw new ArgumentOutOfRangeException();
				}
			}
		}

		public JaggedIndex12(T key0, T key1, T key2, T key3, T key4, T key5, T key6, T key7, T key8, T key9, T key10, T key11)
		{
			this.Key0 = key0; this.Key1 = key1; this.Key2 = key2; this.Key3 = key3; this.Key4 = key4; this.Key5 = key5; this.Key6 = key6; this.Key7 = key7; this.Key8 = key8; this.Key9 = key9; this.Key10 = key10; this.Key11 = key11;
		}

		public T[] GetValues()
		{
			return new[] { this.Key0, this.Key1, this.Key2, this.Key3, this.Key4, this.Key5, this.Key6, this.Key7, this.Key8, this.Key9, this.Key10, this.Key11 };
		}

		public override bool Equals(object key)
		{
            if (!(key is JaggedIndex12<T>)) return false;
            return Equals((JaggedIndex12<T>)key);
		}

		public bool Equals(JaggedIndex12<T> key)
		{
			var comparer = EqualityComparer<T>.Default;
			if (!comparer.Equals(this.Key0, key.Key0)) return false;
			if (!comparer.Equals(this.Key1, key.Key1)) return false;
			if (!comparer.Equals(this.Key2, key.Key2)) return false;
			if (!comparer.Equals(this.Key3, key.Key3)) return false;
			if (!comparer.Equals(this.Key4, key.Key4)) return false;
			if (!comparer.Equals(this.Key5, key.Key5)) return false;
			if (!comparer.Equals(this.Key6, key.Key6)) return false;
			if (!comparer.Equals(this.Key7, key.Key7)) return false;
			if (!comparer.Equals(this.Key8, key.Key8)) return false;
			if (!comparer.Equals(this.Key9, key.Key9)) return false;
			if (!comparer.Equals(this.Key10, key.Key10)) return false;
			if (!comparer.Equals(this.Key11, key.Key11)) return false;
			return true;
		}

        public override int GetHashCode()
        {
			var hash = TupleHack.CombineHashCodes8(
				Key0.GetHashCode(), Key1.GetHashCode(), Key2.GetHashCode(), Key3.GetHashCode(), Key4.GetHashCode(), Key5.GetHashCode(), Key6.GetHashCode(), Key7.GetHashCode());
            return TupleHack.CombineHashCodes5(hash,
				Key8.GetHashCode(), Key9.GetHashCode(), Key10.GetHashCode(), Key11.GetHashCode());
        }

        public override string ToString()
        {
			return string.Format("[ {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11} ]",
				Key0, Key1, Key2, Key3, Key4, Key5, Key6, Key7, Key8, Key9, Key10, Key11);
        }
	}
	public static partial class JaggedIndex
	{
		public static JaggedIndex13<T> Create<T>(T key0, T key1, T key2, T key3, T key4, T key5, T key6, T key7, T key8, T key9, T key10, T key11, T key12)
		{
			return new JaggedIndex13<T>(key0, key1, key2, key3, key4, key5, key6, key7, key8, key9, key10, key11, key12);
		}
	}

	public struct JaggedIndex13<T> : IJaggedIndex<T>, IEquatable<JaggedIndex13<T>>
	{
		public readonly T Key0; public readonly T Key1; public readonly T Key2; public readonly T Key3; public readonly T Key4; public readonly T Key5; public readonly T Key6; public readonly T Key7; public readonly T Key8; public readonly T Key9; public readonly T Key10; public readonly T Key11; public readonly T Key12;

		public int Depth { get { return 13; } }

		public T this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return this.Key0;
					case 1: return this.Key1;
					case 2: return this.Key2;
					case 3: return this.Key3;
					case 4: return this.Key4;
					case 5: return this.Key5;
					case 6: return this.Key6;
					case 7: return this.Key7;
					case 8: return this.Key8;
					case 9: return this.Key9;
					case 10: return this.Key10;
					case 11: return this.Key11;
					case 12: return this.Key12;
					default: throw new ArgumentOutOfRangeException();
				}
			}
		}

		public JaggedIndex13(T key0, T key1, T key2, T key3, T key4, T key5, T key6, T key7, T key8, T key9, T key10, T key11, T key12)
		{
			this.Key0 = key0; this.Key1 = key1; this.Key2 = key2; this.Key3 = key3; this.Key4 = key4; this.Key5 = key5; this.Key6 = key6; this.Key7 = key7; this.Key8 = key8; this.Key9 = key9; this.Key10 = key10; this.Key11 = key11; this.Key12 = key12;
		}

		public T[] GetValues()
		{
			return new[] { this.Key0, this.Key1, this.Key2, this.Key3, this.Key4, this.Key5, this.Key6, this.Key7, this.Key8, this.Key9, this.Key10, this.Key11, this.Key12 };
		}

		public override bool Equals(object key)
		{
            if (!(key is JaggedIndex13<T>)) return false;
            return Equals((JaggedIndex13<T>)key);
		}

		public bool Equals(JaggedIndex13<T> key)
		{
			var comparer = EqualityComparer<T>.Default;
			if (!comparer.Equals(this.Key0, key.Key0)) return false;
			if (!comparer.Equals(this.Key1, key.Key1)) return false;
			if (!comparer.Equals(this.Key2, key.Key2)) return false;
			if (!comparer.Equals(this.Key3, key.Key3)) return false;
			if (!comparer.Equals(this.Key4, key.Key4)) return false;
			if (!comparer.Equals(this.Key5, key.Key5)) return false;
			if (!comparer.Equals(this.Key6, key.Key6)) return false;
			if (!comparer.Equals(this.Key7, key.Key7)) return false;
			if (!comparer.Equals(this.Key8, key.Key8)) return false;
			if (!comparer.Equals(this.Key9, key.Key9)) return false;
			if (!comparer.Equals(this.Key10, key.Key10)) return false;
			if (!comparer.Equals(this.Key11, key.Key11)) return false;
			if (!comparer.Equals(this.Key12, key.Key12)) return false;
			return true;
		}

        public override int GetHashCode()
        {
			var hash = TupleHack.CombineHashCodes8(
				Key0.GetHashCode(), Key1.GetHashCode(), Key2.GetHashCode(), Key3.GetHashCode(), Key4.GetHashCode(), Key5.GetHashCode(), Key6.GetHashCode(), Key7.GetHashCode());
            return TupleHack.CombineHashCodes6(hash,
				Key8.GetHashCode(), Key9.GetHashCode(), Key10.GetHashCode(), Key11.GetHashCode(), Key12.GetHashCode());
        }

        public override string ToString()
        {
			return string.Format("[ {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12} ]",
				Key0, Key1, Key2, Key3, Key4, Key5, Key6, Key7, Key8, Key9, Key10, Key11, Key12);
        }
	}
	public static partial class JaggedIndex
	{
		public static JaggedIndex14<T> Create<T>(T key0, T key1, T key2, T key3, T key4, T key5, T key6, T key7, T key8, T key9, T key10, T key11, T key12, T key13)
		{
			return new JaggedIndex14<T>(key0, key1, key2, key3, key4, key5, key6, key7, key8, key9, key10, key11, key12, key13);
		}
	}

	public struct JaggedIndex14<T> : IJaggedIndex<T>, IEquatable<JaggedIndex14<T>>
	{
		public readonly T Key0; public readonly T Key1; public readonly T Key2; public readonly T Key3; public readonly T Key4; public readonly T Key5; public readonly T Key6; public readonly T Key7; public readonly T Key8; public readonly T Key9; public readonly T Key10; public readonly T Key11; public readonly T Key12; public readonly T Key13;

		public int Depth { get { return 14; } }

		public T this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return this.Key0;
					case 1: return this.Key1;
					case 2: return this.Key2;
					case 3: return this.Key3;
					case 4: return this.Key4;
					case 5: return this.Key5;
					case 6: return this.Key6;
					case 7: return this.Key7;
					case 8: return this.Key8;
					case 9: return this.Key9;
					case 10: return this.Key10;
					case 11: return this.Key11;
					case 12: return this.Key12;
					case 13: return this.Key13;
					default: throw new ArgumentOutOfRangeException();
				}
			}
		}

		public JaggedIndex14(T key0, T key1, T key2, T key3, T key4, T key5, T key6, T key7, T key8, T key9, T key10, T key11, T key12, T key13)
		{
			this.Key0 = key0; this.Key1 = key1; this.Key2 = key2; this.Key3 = key3; this.Key4 = key4; this.Key5 = key5; this.Key6 = key6; this.Key7 = key7; this.Key8 = key8; this.Key9 = key9; this.Key10 = key10; this.Key11 = key11; this.Key12 = key12; this.Key13 = key13;
		}

		public T[] GetValues()
		{
			return new[] { this.Key0, this.Key1, this.Key2, this.Key3, this.Key4, this.Key5, this.Key6, this.Key7, this.Key8, this.Key9, this.Key10, this.Key11, this.Key12, this.Key13 };
		}

		public override bool Equals(object key)
		{
            if (!(key is JaggedIndex14<T>)) return false;
            return Equals((JaggedIndex14<T>)key);
		}

		public bool Equals(JaggedIndex14<T> key)
		{
			var comparer = EqualityComparer<T>.Default;
			if (!comparer.Equals(this.Key0, key.Key0)) return false;
			if (!comparer.Equals(this.Key1, key.Key1)) return false;
			if (!comparer.Equals(this.Key2, key.Key2)) return false;
			if (!comparer.Equals(this.Key3, key.Key3)) return false;
			if (!comparer.Equals(this.Key4, key.Key4)) return false;
			if (!comparer.Equals(this.Key5, key.Key5)) return false;
			if (!comparer.Equals(this.Key6, key.Key6)) return false;
			if (!comparer.Equals(this.Key7, key.Key7)) return false;
			if (!comparer.Equals(this.Key8, key.Key8)) return false;
			if (!comparer.Equals(this.Key9, key.Key9)) return false;
			if (!comparer.Equals(this.Key10, key.Key10)) return false;
			if (!comparer.Equals(this.Key11, key.Key11)) return false;
			if (!comparer.Equals(this.Key12, key.Key12)) return false;
			if (!comparer.Equals(this.Key13, key.Key13)) return false;
			return true;
		}

        public override int GetHashCode()
        {
			var hash = TupleHack.CombineHashCodes8(
				Key0.GetHashCode(), Key1.GetHashCode(), Key2.GetHashCode(), Key3.GetHashCode(), Key4.GetHashCode(), Key5.GetHashCode(), Key6.GetHashCode(), Key7.GetHashCode());
            return TupleHack.CombineHashCodes7(hash,
				Key8.GetHashCode(), Key9.GetHashCode(), Key10.GetHashCode(), Key11.GetHashCode(), Key12.GetHashCode(), Key13.GetHashCode());
        }

        public override string ToString()
        {
			return string.Format("[ {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13} ]",
				Key0, Key1, Key2, Key3, Key4, Key5, Key6, Key7, Key8, Key9, Key10, Key11, Key12, Key13);
        }
	}
	public static partial class JaggedIndex
	{
		public static JaggedIndex15<T> Create<T>(T key0, T key1, T key2, T key3, T key4, T key5, T key6, T key7, T key8, T key9, T key10, T key11, T key12, T key13, T key14)
		{
			return new JaggedIndex15<T>(key0, key1, key2, key3, key4, key5, key6, key7, key8, key9, key10, key11, key12, key13, key14);
		}
	}

	public struct JaggedIndex15<T> : IJaggedIndex<T>, IEquatable<JaggedIndex15<T>>
	{
		public readonly T Key0; public readonly T Key1; public readonly T Key2; public readonly T Key3; public readonly T Key4; public readonly T Key5; public readonly T Key6; public readonly T Key7; public readonly T Key8; public readonly T Key9; public readonly T Key10; public readonly T Key11; public readonly T Key12; public readonly T Key13; public readonly T Key14;

		public int Depth { get { return 15; } }

		public T this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return this.Key0;
					case 1: return this.Key1;
					case 2: return this.Key2;
					case 3: return this.Key3;
					case 4: return this.Key4;
					case 5: return this.Key5;
					case 6: return this.Key6;
					case 7: return this.Key7;
					case 8: return this.Key8;
					case 9: return this.Key9;
					case 10: return this.Key10;
					case 11: return this.Key11;
					case 12: return this.Key12;
					case 13: return this.Key13;
					case 14: return this.Key14;
					default: throw new ArgumentOutOfRangeException();
				}
			}
		}

		public JaggedIndex15(T key0, T key1, T key2, T key3, T key4, T key5, T key6, T key7, T key8, T key9, T key10, T key11, T key12, T key13, T key14)
		{
			this.Key0 = key0; this.Key1 = key1; this.Key2 = key2; this.Key3 = key3; this.Key4 = key4; this.Key5 = key5; this.Key6 = key6; this.Key7 = key7; this.Key8 = key8; this.Key9 = key9; this.Key10 = key10; this.Key11 = key11; this.Key12 = key12; this.Key13 = key13; this.Key14 = key14;
		}

		public T[] GetValues()
		{
			return new[] { this.Key0, this.Key1, this.Key2, this.Key3, this.Key4, this.Key5, this.Key6, this.Key7, this.Key8, this.Key9, this.Key10, this.Key11, this.Key12, this.Key13, this.Key14 };
		}

		public override bool Equals(object key)
		{
            if (!(key is JaggedIndex15<T>)) return false;
            return Equals((JaggedIndex15<T>)key);
		}

		public bool Equals(JaggedIndex15<T> key)
		{
			var comparer = EqualityComparer<T>.Default;
			if (!comparer.Equals(this.Key0, key.Key0)) return false;
			if (!comparer.Equals(this.Key1, key.Key1)) return false;
			if (!comparer.Equals(this.Key2, key.Key2)) return false;
			if (!comparer.Equals(this.Key3, key.Key3)) return false;
			if (!comparer.Equals(this.Key4, key.Key4)) return false;
			if (!comparer.Equals(this.Key5, key.Key5)) return false;
			if (!comparer.Equals(this.Key6, key.Key6)) return false;
			if (!comparer.Equals(this.Key7, key.Key7)) return false;
			if (!comparer.Equals(this.Key8, key.Key8)) return false;
			if (!comparer.Equals(this.Key9, key.Key9)) return false;
			if (!comparer.Equals(this.Key10, key.Key10)) return false;
			if (!comparer.Equals(this.Key11, key.Key11)) return false;
			if (!comparer.Equals(this.Key12, key.Key12)) return false;
			if (!comparer.Equals(this.Key13, key.Key13)) return false;
			if (!comparer.Equals(this.Key14, key.Key14)) return false;
			return true;
		}

        public override int GetHashCode()
        {
			var hash = TupleHack.CombineHashCodes8(
				Key0.GetHashCode(), Key1.GetHashCode(), Key2.GetHashCode(), Key3.GetHashCode(), Key4.GetHashCode(), Key5.GetHashCode(), Key6.GetHashCode(), Key7.GetHashCode());
            return TupleHack.CombineHashCodes8(hash,
				Key8.GetHashCode(), Key9.GetHashCode(), Key10.GetHashCode(), Key11.GetHashCode(), Key12.GetHashCode(), Key13.GetHashCode(), Key14.GetHashCode());
        }

        public override string ToString()
        {
			return string.Format("[ {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14} ]",
				Key0, Key1, Key2, Key3, Key4, Key5, Key6, Key7, Key8, Key9, Key10, Key11, Key12, Key13, Key14);
        }
	}
}
