using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tup.SuffixTree
{
    /// <summary>
    /// A specialized implementation of Map that uses native char types and sorted
    /// arrays to keep minimize the memory footprint.
    /// Implements only the operations that are needed within the suffix tree context.
    /// </summary>
    class EdgeBag : Dictionary<char, Edge>
    { }
}
