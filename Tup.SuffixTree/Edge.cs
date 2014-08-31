
namespace Tup.SuffixTree
{
    /// <summary>
    /// Represents an Edge in the Suffix Tree.
    /// It has a label and a destination Node
    /// </summary>
    public class Edge
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="label"></param>
        /// <param name="dest"></param>
        public Edge(string label, Node dest)
        {
            this.Label = label;
            this.Dest = dest;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Label { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Node Dest { get; set; }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {

            return string.Format("[Edge Label:{0},Dest:{1}]", this.Label, this.Dest != null);
        }
    }
}
