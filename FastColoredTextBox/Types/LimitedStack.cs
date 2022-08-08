using System.Collections.Generic;

namespace FastColoredTextBoxNS.Types {
	/// <summary>
	/// Limited stack
	/// </summary>
	public class LimitedStack<T> : Stack<T> {
		/// <summary>
		/// Max stack length
		/// </summary>
		public int Max { get; private set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="maxItemCount">Maximum length of stack</param>
		public LimitedStack(int maxItemCount) => Max = maxItemCount;

		/// <summary>
		/// Push item
		/// </summary>
		public new void Push(T item) {
			if (Count - 1 > Max) {
				return;
			}

			base.Push(item);
		}
	}
}