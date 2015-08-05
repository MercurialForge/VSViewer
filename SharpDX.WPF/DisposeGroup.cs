using System;
using System.Linq;
using SharpDX;
using System.Collections.Generic;

namespace SharpDX.WPF
{
	/// <summary>
	/// SharpDX 1.3 requires explicit dispose of all its ComObject.
	/// This method makes it easier.
	/// (Remark: I attempted to hack a correct Dispose implementation but it crashed the app on first GC!)
	/// </summary>
	public class DisposeGroup : IDisposable
	{
        /// <summary>
        /// 
        /// </summary>        
		public void Add(params IDisposable[] objects)
		{
			m_list.AddRange(from o in objects where o != null select o);
		}

        /// <summary>
        /// 
        /// </summary>
		public T Add<T>(T ob)
			where T : IDisposable
		{
			if (ob != null)
				m_list.Add(ob);
			return ob;
		}

        /// <summary>
        /// 
        /// </summary>
		public void Dispose()
		{
            //for (int i = list.Count - 1; i >= 0; i--)
            //{
            //    var d = list[i];
            //    list.RemoveAt(i);
            //    d.Dispose();
            //}
            for (int i = 0; i < m_list.Count; i++)
            {
                m_list[i].Dispose();
            }
            m_list = null;
		}

        /// <summary>
        /// 
        /// </summary>
		private List<IDisposable> m_list = new List<IDisposable>();

	}
}
