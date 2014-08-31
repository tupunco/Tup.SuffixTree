using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tup.SuffixTree.UnitTestProject
{
    /// <summary>
    /// SuffixTreeTest 的摘要说明
    /// </summary>
    [TestClass]
    public class SuffixTreeTest
    {

        [TestMethod]
        public void TestBasicTreeGeneration()
        {
            GeneralizedSuffixTree @in = new GeneralizedSuffixTree();

            string word = "cacao";
            @in.Put(word, 0);

            /* test that every substring is contained within the tree */
            foreach (string s in Utils.GetSubstrings(word))
            {
                Assert.IsTrue(@in.Search(s).Contains(0));
            }
            Assert.IsNull(@in.Search("caco"));
            Assert.IsNull(@in.Search("cacaoo"));
            Assert.IsNull(@in.Search("ccacao"));

            @in = new GeneralizedSuffixTree();
            word = "bookkeeper";
            @in.Put(word, 0);
            foreach (string s in Utils.GetSubstrings(word))
            {
                Assert.IsTrue(@in.Search(s).Contains(0));
            }
            Assert.IsNull(@in.Search("books"));
            Assert.IsNull(@in.Search("boke"));
            Assert.IsNull(@in.Search("ookepr"));
        }

        [TestMethod]
        public void TestWeirdword()
        {
            GeneralizedSuffixTree @in = new GeneralizedSuffixTree();

            string word = "cacacato";
            @in.Put(word, 0);

            /* test that every substring is contained within the tree */
            foreach (string s in Utils.GetSubstrings(word))
            {
                Assert.IsTrue(@in.Search(s).Contains(0));
            }
        }

        [TestMethod]
        public void TestDouble()
        {
            // test whether the tree can handle repetitions
            GeneralizedSuffixTree @in = new GeneralizedSuffixTree();
            string word = "cacao";
            @in.Put(word, 0);
            @in.Put(word, 1);

            foreach (string s in Utils.GetSubstrings(word))
            {
                Assert.IsTrue(@in.Search(s).Contains(0));
                Assert.IsTrue(@in.Search(s).Contains(1));
            }
        }

        [TestMethod]
        public void TestBananaAddition()
        {
            GeneralizedSuffixTree @in = new GeneralizedSuffixTree();
            string[] words = new string[] { "banana", "bano", "ba" };
            var wLen = words.Length;
            for (int i = 0; i < wLen; ++i)
            {
                @in.Put(words[i], i);

                foreach (string s in Utils.GetSubstrings(words[i]))
                {
                    var result = @in.Search(s);
                    Assert.IsNotNull(result, "result null for string " + s + " after adding " + words[i]);
                    Assert.IsTrue(result.Contains(i), "substring " + s + " not found after adding " + words[i]);
                }

            }

            // verify post-addition
            for (int i = 0; i < wLen; ++i)
            {
                foreach (string s in Utils.GetSubstrings(words[i]))
                {
                    Assert.IsTrue(@in.Search(s).Contains(i));
                }
            }

            // add again, to see if it's stable
            for (int i = 0; i < wLen; ++i)
            {
                @in.Put(words[i], i + wLen);

                foreach (string s in Utils.GetSubstrings(words[i]))
                {
                    Assert.IsTrue(@in.Search(s).Contains(i + wLen));
                }
            }
        }

        [TestMethod]
        public void TestAddition()
        {
            GeneralizedSuffixTree @in = new GeneralizedSuffixTree();
            string[] words = new string[] { "cacaor", "caricato", "cacato", "cacata", "caricata", "cacao", "banana" };
            var wLen = words.Length;
            for (int i = 0; i < wLen; ++i)
            {
                @in.Put(words[i], i);

                foreach (string s in Utils.GetSubstrings(words[i]))
                {
                    var result = @in.Search(s);
                    Assert.IsNotNull(result, "result null for string " + s + " after adding " + words[i]);
                    Assert.IsTrue(result.Contains(i), "substring " + s + " not found after adding " + words[i]);
                }
            }
            // verify post-addition
            for (int i = 0; i < wLen; ++i)
            {
                foreach (string s in Utils.GetSubstrings(words[i]))
                {
                    var result = @in.Search(s);
                    Assert.IsNotNull(result, "result null for string " + s + " after adding " + words[i]);
                    Assert.IsTrue(result.Contains(i), "substring " + s + " not found after adding " + words[i]);
                }
            }

            // add again, to see if it's stable
            for (int i = 0; i < wLen; ++i)
            {
                @in.Put(words[i], i + wLen);

                foreach (string s in Utils.GetSubstrings(words[i]))
                {
                    Assert.IsTrue(@in.Search(s).Contains(i + wLen));
                }
            }

            @in.ComputeCount();
            TestResultsCount(@in.GetRoot());

            Assert.IsNull(@in.Search("aoca"));
        }

        [TestMethod]
        public void TestSampleAddition()
        {
            GeneralizedSuffixTree @in = new GeneralizedSuffixTree();
            string[] words = new string[] {"libertypike",
                "franklintn",
                "carothersjohnhenryhouse",
                "carothersezealhouse",
                "acrossthetauntonriverfromdightonindightonrockstatepark",
                "dightonma",
                "dightonrock",
                "6mineoflowgaponlowgapfork",
                "lowgapky",
                "lemasterjohnjandellenhouse",
                "lemasterhouse",
                "70wilburblvd",
                "poughkeepsieny",
                "freerhouse",
                "701laurelst",
                "conwaysc",
                "hollidayjwjrhouse",
                "mainandappletonsts",
                "menomoneefallswi",
                "mainstreethistoricdistrict",
                "addressrestricted",
                "brownsmillsnj",
                "hanoverfurnace",
                "hanoverbogironfurnace",
                "sofsavannahatfergusonaveandbethesdard",
                "savannahga",
                "bethesdahomeforboys",
                "bethesda"};
            var wLen = words.Length;
            for (int i = 0; i < wLen; ++i)
            {
                @in.Put(words[i], i);

                foreach (string s in Utils.GetSubstrings(words[i]))
                {
                    var result = @in.Search(s);
                    Assert.IsNotNull(result, "result null for string " + s + " after adding " + words[i]);
                    Assert.IsTrue(result.Contains(i), "substring " + s + " not found after adding " + words[i]);
                }
            }
            // verify post-addition
            for (int i = 0; i < wLen; ++i)
            {
                foreach (string s in Utils.GetSubstrings(words[i]))
                {
                    Assert.IsTrue(@in.Search(s).Contains(i));
                }
            }

            // add again, to see if it's stable
            for (int i = 0; i < wLen; ++i)
            {
                @in.Put(words[i], i + wLen);

                foreach (string s in Utils.GetSubstrings(words[i]))
                {
                    Assert.IsTrue(@in.Search(s).Contains(i + wLen));
                }
            }

            @in.ComputeCount();
            TestResultsCount(@in.GetRoot());

            Assert.IsNull(@in.Search("aoca"));
        }

        private void TestResultsCount(Node n)
        {
            foreach (Edge e in n.GetEdges().Values)
            {
                Assert.AreEqual(n.GetData(-1).Count, n.GetResultCount());
                TestResultsCount(e.Dest);
            }
        }

        //[TestMethod]
        ///* testing a test method :) */
        //public void TestGetSubstrings()
        //{
        //    var exp = new HashSet<string>(new string[] { "w", "r", "d", "wr", "rd", "wrd" });
        //    var ret = Utils.GetSubstrings("wrd");
        //    //Assert.IsTrue(ret.equals(exp));
        //}
    }
}
