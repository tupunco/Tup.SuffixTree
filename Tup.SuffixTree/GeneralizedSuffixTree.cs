using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Tup.SuffixTree
{
    /// <summary>
    /// A Generalized Suffix Tree, based on the Ukkonen's paper "On-line construction of suffix trees"
    /// http://www.cs.helsinki.fi/u/ukkonen/SuffixT1withFigs.pdf
    ///
    /// Allows for fast storage and fast(er) retrieval by creating a tree-based index out of a set of strings.
    /// Unlike common suffix trees, which are generally used to build an index out of one (very) long string,
    /// a Generalized Suffix Tree can be used to build an index over many strings.
    ///
    /// Its main operations are put and search:
    /// Put adds the given key to the index, allowing for later retrieval of the given value.
    /// Search can be used to retrieve the set of all the values that were put in the index with keys that contain a given input.
    ///
    /// In particular, after put(K, V), search(H) will return a set containing V for any string H that is substring of K.
    ///
    /// The overall complexity of the retrieval operation (search) is O(m) where m is the length of the string to search within the index.
    ///
    /// Although the implementation is based on the original design by Ukkonen, there are a few aspects where it differs significantly.
    /// 
    /// The tree is composed of a set of nodes and labeled edges. The labels on the edges can have any length as long as it's greater than 0.
    /// The only constraint is that no two edges going out from the same node will start with the same character.
    /// 
    /// Because of this, a given (startNode, stringSuffix) pair can denote a unique path within the tree, and it is the path (if any) that can be
    /// composed by sequentially traversing all the edges (e1, e2, ...) starting from startNode such that (e1.label + e2.label + ...) is equal
    /// to the stringSuffix.
    /// See the search method for details.
    /// 
    /// The union of all the edge labels from the root to a given leaf node denotes the set of the strings explicitly contained within the GST.
    /// In addition to those strings, there are a set of different strings that are implicitly contained within the GST, and it is composed of
    /// the strings built by concatenating e1.label + e2.label + ... + $end, where e1, e2, ... is a proper path and $end is prefix of any of
    /// the labels of the edges starting from the last node of the path.
    ///
    /// This kind of "implicit path" is important in the testAndSplit method.
    /// </summary>
    public class GeneralizedSuffixTree
    {
        /// <summary>
        /// The index of the last item that was added to the GST
        /// </summary>
        private int last = 0;
        /// <summary>
        /// The root of the suffix tree
        /// </summary>
        private readonly Node root = new Node();
        /// <summary>
        /// The last leaf that was added during the update operation
        /// </summary>
        private Node activeLeaf = null;

        /// <summary>
        /// 
        /// </summary>
        public GeneralizedSuffixTree()
        {
            activeLeaf = root;
        }

        /// <summary>
        /// Searches for the given word within the GST.
        ///
        /// Returns all the indexes for which the key contains the <tt>word</tt> that was
        /// supplied as input.
        /// </summary>
        /// <param name="word">the key to search for</param>
        /// <returns>the collection of indexes associated with the input <tt>word</tt></returns>
        public ICollection<int> Search(string word)
        {
            return Search(word, -1);
        }
        /// <summary>
        /// Searches for the given word within the GST and returns at most the given number of matches.
        /// </summary>
        /// <param name="word">the key to search for</param>
        /// <param name="results"> the max number of results to return</param>
        /// <returns>at most <tt>results</tt> values for the given word</returns>
        public ICollection<int> Search(string word, int results)
        {
            Node tmpNode = SearchNode(word);
            if (tmpNode == null)
            {
                return null;
            }
            return tmpNode.GetData(results);
        }
        /// <summary>
        /// Searches for the given word within the GST and returns at most the given number of matches.
        /// </summary>
        /// <param name="word"word the key to search for></param>
        /// <param name="to">the max number of results to return</param>
        /// <returns>at most <tt>results</tt> values for the given word</returns>
        public ResultInfo SearchWithCount(string word, int to)
        {
            Node tmpNode = SearchNode(word);
            if (tmpNode == null)
            {
                return new ResultInfo(Collections.EMPTY_LIST, 0);
            }

            return new ResultInfo(tmpNode.GetData(to), tmpNode.GetResultCount());
        }

        /**
         * Returns the tree node (if present) that corresponds to the given string.
         */
        private Node SearchNode(string word)
        {
            /*
             * Verifies if exists a path from the root to a node such that the concatenation
             * of all the labels on the path is a superstring of the given word.
             * If such a path is found, the last node on it is returned.
             */
            Node currentNode = root;
            Edge currentEdge;
            var wordLen = word.Length;
            for (int i = 0; i < wordLen; ++i)
            {
                char ch = word[i];
                // follow the edge corresponding to this char
                currentEdge = currentNode.GetEdge(ch);
                if (null == currentEdge)
                {
                    // there is no edge starting with this char
                    return null;
                }
                else
                {
                    var label = currentEdge.Label;
                    var lenToMatch = Math.Min(wordLen - i, label.Length);
                    //if (!word.regionMatches(i, label, 0, lenToMatch))
                    if (string.Compare(word, i, label, 0, lenToMatch) != 0)
                    {
                        // the label on the edge does not correspond to the one in the string to search
                        return null;
                    }

                    if (label.Length >= wordLen - i)
                    {
                        return currentEdge.Dest;
                    }
                    else
                    {
                        // advance to next node
                        currentNode = currentEdge.Dest;
                        i += lenToMatch - 1;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Adds the specified <tt>index</tt> to the GST under the given <tt>key</tt>.
        ///
        /// Entries must be inserted so that their indexes are in non-decreasing order,
        /// otherwise an IllegalStateException will be raised.
        /// </summary>
        /// <param name="key">the string key that will be added to the index</param>
        /// <param name="index">the value that will be added to the index</param>
        /// <exception cref="ArgumentException">if an invalid index is passed as input</exception>
        public void Put(string key, int index)
        {
            if (index < last)
            {
                throw new ArgumentException("The input index must not be less than any of the previously inserted ones. Got " + index + ", expected at least " + last);
            }
            else
            {
                last = index;
            }

            // reset activeLeaf
            activeLeaf = root;

            string remainder = key;
            Node s = root;

            // proceed with tree construction (closely related to procedure in
            // Ukkonen's paper)
            var text = string.Empty;
            Pair<Node, string> active = null;
            // iterate over the string, one char at a time
            for (int i = 0; i < remainder.Length; i++)
            {
                // line 6
                text += remainder[i];
                // use intern to make sure the resulting string is in the pool.
                text = string.Intern(text);

                // line 7: update the tree with the new transitions due to this new char
                active = Update(s, text, remainder.Substring(i), index);
                // line 8: make sure the active pair is canonical
                active = Canonize(active.First, active.Second);

                s = active.First;
                text = active.Second;
            }

            // add leaf suffix link, is necessary
            if (null == activeLeaf.Suffix && activeLeaf != root && activeLeaf != s)
            {
                activeLeaf.Suffix = s;
            }
        }

        /**
         * Tests whether the string stringPart + t is contained in the subtree that has inputs as root.
         * If that's not the case, and there exists a path of edges e1, e2, ... such that
         *     e1.label + e2.label + ... + $end = stringPart
         * and there is an edge g such that
         *     g.label = stringPart + rest
         * 
         * Then g will be split in two different edges, one having $end as label, and the other one
         * having rest as label.
         *
         * @param inputs the starting node
         * @param stringPart the string to search
         * @param t the following character
         * @param remainder the remainder of the string to add to the index
         * @param value the value to add to the index
         * @return a pair containing
         *                  true/false depending on whether (stringPart + t) is contained in the subtree starting in inputs
         *                  the last node that can be reached by following the path denoted by stringPart starting from inputs
         *         
         */
        private Pair<Boolean, Node> TestAndSplit(Node inputs, string stringPart, char t, string remainder, int value)
        {
            // descend the tree as far as possible
            Pair<Node, string> ret = Canonize(inputs, stringPart);
            Node s = ret.First;
            string str = ret.Second;
            if (str.HasValue())
            {
                var str_fc = str[0];
                Edge g = s.GetEdge(str_fc);

                string label = g.Label;
                var strLen = str.Length;
                // must see whether "str" is substring of the label of an edge
                if (label.Length > strLen && label[strLen] == t)
                {
                    return new Pair<Boolean, Node>(true, s);
                }
                else
                {
                    // need to split the edge
                    string newlabel = label.Substring(strLen);

                    Debug.Assert(label.StartsWith(str));

                    // build a new node
                    Node r = new Node();
                    // build a new edge
                    Edge newedge = new Edge(str, r);

                    g.Label = newlabel;

                    // link s -> r
                    r.AddEdge(newlabel[0], g);
                    s.AddEdge(str_fc, newedge);

                    return new Pair<Boolean, Node>(false, r);
                }
            }
            else
            {
                Edge e = s.GetEdge(t);
                if (null == e)
                {
                    // if there is no t-transtion from s
                    return new Pair<Boolean, Node>(false, s);
                }
                else
                {
                    var label = e.Label;
                    if (remainder == label)
                    {
                        // update payload of destination node
                        e.Dest.AddRef(value);
                        return new Pair<Boolean, Node>(true, s);
                    }
                    else if (remainder.StartsWith(label))
                    {
                        return new Pair<Boolean, Node>(true, s);
                    }
                    else if (label.StartsWith(remainder))
                    {
                        // need to split as above
                        Node newNode = new Node();
                        newNode.AddRef(value);

                        Edge newEdge = new Edge(remainder, newNode);

                        e.Label = label.Substring(remainder.Length);

                        newNode.AddEdge(e.Label[0], e);

                        s.AddEdge(t, newEdge);

                        return new Pair<Boolean, Node>(false, s);
                    }
                    else
                    {
                        // they are different words. No prefix. but they may still share some common substr
                        return new Pair<Boolean, Node>(true, s);
                    }
                }
            }
        }

        /**
         * Return a (Node, string) (n, remainder) pair such that n is a farthest descendant of
         * s (the input node) that can be reached by following a path of edges denoting
         * a prefix of inputstr and remainder will be string that must be
         * appended to the concatenation of labels from s to n to get inpustr.
         */
        private Pair<Node, string> Canonize(Node s, string inputstr)
        {
            if (inputstr.IsEmpty())
                return new Pair<Node, string>(s, inputstr);

            Node currentNode = s;
            string str = inputstr;
            Edge g = s.GetEdge(str[0]);
            // descend the tree as long as a proper label is found
            while (g != null && str.StartsWith(g.Label))
            {
                str = str.Substring(g.Label.Length);
                currentNode = g.Dest;
                if (str.Length > 0)
                {
                    g = currentNode.GetEdge(str[0]);
                }
            }

            return new Pair<Node, string>(currentNode, str);
        }

        /**
         * Updates the tree starting from inputNode and by adding stringPart.
         * 
         * Returns a reference (Node, string) pair for the string that has been added so far.
         * This means:
         * - the Node will be the Node that can be reached by the longest path string (S1)
         *   that can be obtained by concatenating consecutive edges in the tree and
         *   that is a substring of the string added so far to the tree.
         * - the string will be the remainder that must be added to S1 to get the string
         *   added so far.
         * 
         * @param inputNode the node to start from
         * @param stringPart the string to add to the tree
         * @param rest the rest of the string
         * @param value the value to add to the index
         */
        private Pair<Node, string> Update(Node inputNode, string stringPart, string rest, int value)
        {
            Node s = inputNode;
            string tempstr = stringPart;
            char newChar = stringPart[stringPart.Length - 1];

            // line 1
            Node oldroot = root;

            // line 1b
            Pair<Boolean, Node> ret = TestAndSplit(s, tempstr.Substring(0, tempstr.Length - 1), newChar, rest, value);

            Node r = ret.Second;
            bool endpoint = ret.First;

            Node leaf;
            // line 2
            while (!endpoint)
            {
                // line 3
                Edge tempEdge = r.GetEdge(newChar);
                if (null != tempEdge)
                {
                    // such a node is already present. This is one of the main differences from Ukkonen's case:
                    // the tree can contain deeper nodes at this stage because different strings were added by previous iterations.
                    leaf = tempEdge.Dest;
                }
                else
                {
                    // must build a new leaf
                    leaf = new Node();
                    leaf.AddRef(value);

                    r.AddEdge(newChar, new Edge(rest, leaf));
                }

                // update suffix link for newly created leaf
                if (activeLeaf != root)
                {
                    activeLeaf.Suffix = leaf;
                }
                activeLeaf = leaf;

                // line 4
                if (oldroot != root)
                {
                    oldroot.Suffix = r;
                }

                // line 5
                oldroot = r;

                // line 6
                if (null == s.Suffix)
                { // root node
                    Debug.Assert(root == s);
                    // this is a special case to handle what is referred to as node _|_ on the paper
                    tempstr = tempstr.Substring(1);
                }
                else
                {
                    var canret = Canonize(s.Suffix, SafeCutLastChar(tempstr));
                    s = canret.First;
                    // use intern to ensure that tempstr is a reference from the string pool
                    tempstr = string.Intern(canret.Second + tempstr[tempstr.Length - 1]);
                }

                // line 7
                ret = TestAndSplit(s, SafeCutLastChar(tempstr), newChar, rest, value);
                r = ret.Second;
                endpoint = ret.First;
            }

            // line 8
            if (oldroot != root)
            {
                oldroot.Suffix = r;
            }
            oldroot = root;

            return new Pair<Node, string>(s, tempstr);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Node GetRoot()
        {
            return root;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="seq"></param>
        /// <returns></returns>
        private string SafeCutLastChar(string seq)
        {
            if (seq.IsEmpty())
                return string.Empty;

            return seq.Substring(0, seq.Length - 1);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int ComputeCount()
        {
            return root.ComputeAndCacheCount();
        }

        /// <summary>
        /// An utility object, used to store the data returned by the GeneralizedSuffixTree GeneralizedSuffixTree.searchWithCount method.
        /// It contains a collection of results and the total number of results present in the GST.
        /// @see GeneralizedSuffixTree#searchWithCount(java.lang.string, int) 
        /// </summary>
        public class ResultInfo
        {
            /**
             * The total number of results present in the database
             */
            public int totalResults;
            /**
             * The collection of (some) results present in the GST
             */
            public ICollection<int> results;

            public ResultInfo(ICollection<int> results, int totalResults)
            {
                this.totalResults = totalResults;
                this.results = results;
            }
        }

        /**
         * A private class used to return a tuples of two elements
         */
        private class Pair<A, B>
        {
            public Pair(A first, B second)
            {
                this.First = first;
                this.Second = second;
            }
            /// <summary>
            /// 
            /// </summary>
            public A First { get; private set; }
            /// <summary>
            /// 
            /// </summary>
            public B Second { get; private set; }
        }
    }
}
