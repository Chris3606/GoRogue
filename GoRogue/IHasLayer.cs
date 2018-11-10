using System;
using System.Collections.Generic;
using System.Text;

namespace GoRogue
{
	/// <summary>
	/// Interface to be implemented by objects that will be used in LayeredSpatialMap.
	/// </summary>
	public interface IHasLayer
	{
		/// <summary>
		/// The layer on which the object should reside.  Higher numbers indicate layers closer to the "top"
		/// </summary>
		int Layer { get; }
	}
}
