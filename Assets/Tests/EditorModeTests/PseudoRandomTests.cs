using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UXF;

public class PseudoRandomTests
{
    // A Test behaves as an ordinary method
    [Test]
    public void PseudoRandomLengthTest()
    {
        ExperimentGenerator expGenerator = new GameObject().AddComponent<ExperimentGenerator>();
        List<object> intList = new List<object>()
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9
        };
        // List<object> pseudoRandomList = (List<object>)expGenerator.GeneratePseudoRandom(intList);
        // Assert.AreEqual(pseudoRandomList.Count, 10);
    }
    [Test]
    public void PseudoRandomContentTest()
    {
        ExperimentGenerator expGenerator = new GameObject().AddComponent<ExperimentGenerator>();
        List<object> intList = new List<object>()
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9
        };
        // List<object> pseudoRandomList = (List<object>)expGenerator.GeneratePseudoRandom(intList);
        // for (int i = 0; i < pseudoRandomList.Count; i++)
        // {
        //     Assert.IsTrue(intList.Contains(pseudoRandomList[i]));
        // }
    }
    [Test]
    public void PseudoRandomOrderTest()
    {
        ExperimentGenerator expGenerator = new GameObject().AddComponent<ExperimentGenerator>();
        List<bool> orderBoolList = new List<bool>();
        List<object> intList = new List<object>()
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9
        };
        // List<object> pseudoRandomList = (List<object>)expGenerator.GeneratePseudoRandom(intList);
        // for (int i = 0; i < pseudoRandomList.Count; i++)
        // {
        //     if(intList[i] != pseudoRandomList[i])
        //     {
        //         orderBoolList.Add(false);
        //     }
        //     else
        //     {
        //         orderBoolList.Add(true);
        //     }
        // }
        // Assert.IsTrue(orderBoolList.Contains(false));
    }

    [Test]
    public void PseudoRandomPairTest()
    {
        ExperimentGenerator expGenerator = new GameObject().AddComponent<ExperimentGenerator>();
        List<object> intList = new List<object>()
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9
        };
        // List<object> pseudoRandomList = (List<object>)expGenerator.GeneratePseudoRandom(intList);
        // List<object> pseudoRandomList2 = new List<object>();

        // foreach (int index in pseudoRandomList)
        // {
        //     pseudoRandomList2.Add(intList[index]);
        // }

        // for (int i = 0; i < pseudoRandomList.Count; i++)
        // {
        //     Assert.AreEqual(pseudoRandomList[i], pseudoRandomList2[i]);
        // }
    }
}
