using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpKml.Dom
{
	public interface IGeometryCollection
	{
		IEnumerable<Geometry> Geometry { get; }
	}
}
