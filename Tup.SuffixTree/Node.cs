using System;
using System.Collections.Generic;

namespace Tup.SuffixTree
{
    /// <summary>
    /// Represents a node of the generalized suffix tree graph
    /// <see cref="GeneralizedSuffixTree"/>
    /// </summary>
    public class Node
    {
        /// <summary>
        /// The payload array used to store the data (indexes) associated with this node.
        /// In this case, it is used to store all property indexes.
        /// 
        /// As it is handled, it resembles an ArrayList: when it becomes full it 
        /// is copied to another bigger array (whose size is equals to data.length +
        /// INCREMENT).
        /// 
        /// Originally it was a List<int> but it took too much memory, changing
        /// it to int[] take less memory because indexes are stored using native
        /// types.
        /// </summary>
        private int[] data;
        /**
         * Represents index of the last position used in the data int[] array.
         * 
         * It should always be less than data.length
         */
        private int lastIdx = 0;
        /**
         * The starting size of the int[] array containing the payload
         */
        private static readonly int START_SIZE = 0;
        /**
         * The increment in size used when the payload array is full
         */
        private static readonly int INCREMENT = 1;
        /**
         * The set of edges starting from this node
         */
        private IDictionary<char, Edge> edges;
        /**
         * The suffix link as described in Ukkonen's paper.
         * if str is the string denoted by the path from the root to this, this.suffix
         * is the node denoted by the path that corresponds to str without the first char.
         */
        private Node suffix;
        /**
         * The total number of <em>different</em> results that are stored in this
         * node and in underlying ones (i.e. nodes that can be reached through paths
         * starting from <tt>this</tt>.
         * 
         * This must be calculated explicitly using computeAndCacheCount
         * @see Node#computeAndCacheCount() 
         */
        private int resultCount = -1;

        /**
         * Creates a new Node
         */
        public Node()
        {
            edges = new EdgeBag();
            suffix = null;
            data = new int[START_SIZE];
        }

        /**
         * Returns all the indexes associated to this node and its children.
         * @return all the indexes associated to this node and its children
         */
        public ICollection<int> GetData()
        {
            return GetData(-1);
        }

        /**
         * Returns the first <tt>numElements</tt> elements from the ones associated to this node.
         *
         * Gets data from the payload of both this node and its children, the string representation
         * of the path to this node is a substring of the one of the children nodes.
         * 
         * @param numElements the number of results to return. Use -1 to get all
         * @return the first <tt>numElements</tt> associated to this node and children
         */
        public ICollection<int> GetData(int numElements)
        {
            var ret = new HashSet<int>();
            foreach (int num in data)
            {
                ret.Add(num);
                if (ret.Count == numElements)
                {
                    return ret;
                }
            }
            // need to get more matches from child nodes. This is what may waste time
            foreach (Edge e in edges.Values)
            {
                if (-1 == numElements || ret.Count < numElements)
                {
                    foreach (int num in e.Dest.GetData())
                    {
                        ret.Add(num);
                        if (ret.Count == numElements)
                        {
                            return ret;
                        }
                    }
                }
            }
            return ret;
        }

        /**
         * Adds the given <tt>index</tt> to the set of indexes associated with <tt>this</tt>
         */
        internal void AddRef(int index)
        {
            if (Contains(index))
            {
                return;
            }

            AddIndex(index);

            // add this reference to all the suffixes as well
            Node iter = this.suffix;
            while (iter != null)
            {
                if (iter.Contains(index))
                {
                    break;
                }
                iter.AddRef(index);
                iter = iter.suffix;
            }
        }

        /**
         * Tests whether a node contains a reference to the given index.
         * 
         * <b>IMPORTANT</b>: it works because the array is sorted by construction
         * 
         * @param index the index to look for
         * @return true <tt>this</tt> contains a reference to index
         */
        private bool Contains(int index)
        {
            return Array.BinarySearch(data, 0, lastIdx, index) >= 0;
            //int low = 0;
            //int high = lastIdx - 1;

            //while (low <= high)
            //{
            //    int mid = (low + high) >> 1; //TODO >>>1
            //    int midVal = data[mid];

            //    if (midVal < index)
            //        low = mid + 1;
            //    else if (midVal > index)
            //        high = mid - 1;
            //    else
            //        return true;
            //}
            //return false;
            // Java 5 equivalent to
            // return java.util.Arrays.binarySearch(data, 0, lastIdx, index) >= 0;
        }

        /**
         * Computes the number of results that are stored on this node and on its
         * children, and caches the result.
         * 
         * Performs the same operation on subnodes as well
         * @return the number of results
         */
        internal int ComputeAndCacheCount()
        {
            ComputeAndCacheCountRecursive();
            return resultCount;
        }

        private ISet<int> ComputeAndCacheCountRecursive()
        {
            var ret = new HashSet<int>();
            foreach (int num in data)
            {
                ret.Add(num);
            }
            foreach (Edge e in edges.Values)
            {
                foreach (int num in e.Dest.ComputeAndCacheCountRecursive())
                {
                    ret.Add(num);
                }
            }

            resultCount = ret.Count;
            return ret;
        }

        /**
         * Returns the number of results that are stored on this node and on its
         * children.
         * Should be called after having called computeAndCacheCount.
         * 
         * @throws IllegalStateException when this method is called without having called
         * computeAndCacheCount first
         * @see Node#computeAndCacheCount() 
         * @todo this should raise an exception when the subtree is changed but count
         * wasn't updated
         */
        /// <exception cref="ArgumentException"></exception>
        public int GetResultCount()
        {
            if (-1 == resultCount)
            {
                throw new ArgumentException("getResultCount() shouldn't be called without calling computeCount() first");
            }

            return resultCount;
        }

        internal void AddEdge(char ch, Edge e)
        {
            edges[ch] = e;
        }

        public Edge GetEdge(char ch)
        {
            Edge edge = null;
            edges.TryGetValue(ch, out edge);
            return edge;
        }

        public IDictionary<char, Edge> GetEdges()
        {
            return edges;
        }

        /// <summary>
        /// 
        /// </summary>
        internal Node Suffix
        {
            set { this.suffix = value; }
            get { return suffix; }
        }

        private void AddIndex(int index)
        {
            if (lastIdx == data.Length)
            {
                var copy = new int[data.Length + INCREMENT];
                Array.Copy(data, 0, copy, 0, data.Length);
                data = copy;
            }
            data[lastIdx++] = index;
        }
    }
}
