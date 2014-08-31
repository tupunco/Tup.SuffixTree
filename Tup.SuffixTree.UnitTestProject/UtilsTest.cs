using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tup.SuffixTree.UnitTestProject
{
    /// <summary>
    /// UtilsTest 的摘要说明
    /// </summary>
    [TestClass]
    public class UtilsTest
    {
        [TestMethod]
        public void TestNormalize()
        {
            String[] ins = new String[] { "200 S Main St", "Lakeshore Dr.", "lake-view", "St. Jacob's Cathedral" };
            String[] outs = new String[] { "200smainst", "lakeshoredr", "lakeview", "stjacobscathedral" };

            for (int i = 0; i < ins.Length; ++i)
            {
                String result = Utils.Normalize(ins[i]);
                Assert.AreEqual(outs[i], result);
            }
        }

        [TestMethod]
        public void TestGetSubstrings()
        {
            String @in = "banana";
            ISet<String> @out = Utils.GetSubstrings(@in);
            String[] outArr = new String[] { "b", "a", "n", "ba", "an", "na", "ban", "ana", "nan", "bana", "anan", "nana", "banan", "anana", "banana" };

            foreach (String s in outArr)
            {
                Assert.IsTrue(@out.Remove(s));
            }

            Assert.IsTrue(@out.IsEmpty());
        }
    }
}
