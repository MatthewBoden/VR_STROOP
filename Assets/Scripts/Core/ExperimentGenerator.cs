using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UXF;
/// <summary>
/// Generates experiment parameters based off of JSON file and task objects based off experiment parameters
/// </summary>
public class ExperimentGenerator : MonoBehaviour
{
    /// <summary>
    /// List of the order of the pseudo randomization
    /// </summary>
    private List<int> pseudoRandomOrder = new List<int>();

    public List<int> PseudoRandomOrder
    {
        get { return pseudoRandomOrder; }
    }

    ///<summary>
    ///Read the experiment JSON file and generate the blocks
    ///</summary>
    public void GenerateBlocks(Session session)
    {
        var keys = session.settings.Keys;

        if(keys.First() == "session_1")
        {
            keys = session.settings.GetDict("session_" + session.number).Keys;
            session.settings = new Settings(session.settings.GetDict("session_" + session.number));
        }

        //set if using VR
        ExperimentController.Instance.UseVR = session.settings.GetBool("use_vr");
        //list of the number of trials per block
        List<int> trialsPerBlock = session.settings.GetIntList("trials_in_block");

        //set the total number of blocks
        ExperimentController.Instance.TotalNumOfBlocks = trialsPerBlock.Count;

        //loop and create each block
        //not very efficient but this just gurantees the creation of the blocks
        for (int i = 0; i < trialsPerBlock.Count; i++)
        {
            //the created block
            Block theBlock = session.CreateBlock(trialsPerBlock[i]);
            //set the total number of trials by adding all trials in each block
            ExperimentController.Instance.TotalNumOfTrials += trialsPerBlock[i];
            //set use VR for the block
            theBlock.settings.SetValue("use_vr", ExperimentController.Instance.UseVR);
        }

        Dictionary<string, object> dict = session.settings.GetDict("optional_params");

        if (dict.Keys.Contains("linkPseudoRandom"))
        {
            List<object> list = dict["linkPseudoRandom"] as List<object>;
            List<object> perTrialList = session.settings.GetObjectList(list[0].ToString());

            //the resulting list after being pseudo randomized
            List<object> pseudoList = new List<object>();
            //key minus the prefix
            string firstKey = list[0].ToString().Substring(15);

            for (int i = 0; i < perTrialList.Count; i++)
            {
                string currKey = perTrialList[i].ToString();
                if (currKey != null)
                {
                    pseudoList.Add(PseudoRandom(currKey, session.blocks[i]));
                    List<object> pairList = new List<object>();
                    for (int j = 1; j < list.Count; j++)
                    {
                        List<object> pairTrialList = session.settings.GetObjectList(list[j].ToString());
                        if (pairTrialList[j] != null)
                        {
                            foreach (int index in pseudoRandomOrder)
                            {
                                pairList.Add(session.settings.GetObjectList(pairTrialList[i].ToString())[index]);
                            }

                            string key = list[j].ToString().Substring(15);
                            session.blocks[i].settings.SetValue(key, pairList);
                        }
                    }
                }
                else
                {
                    pseudoList.Add(new List<object>());
                }
                session.blocks[i].settings.SetValue(firstKey, pseudoList[i]);
            }
        }

        //for each key in the JSON
        //loop through the keys and then set them for the blocks
        //prefixes will be removed to be more streamlined
        foreach (string key in keys)
        {
            //for per block parameters
            if (key.StartsWith("per_block_") && !key.StartsWith("per_block_list_"))
            {
                //key minus the prefix
                string newKey = key.Substring(10);
                List<object> perBlockList = new List<object>(session.settings.GetObjectList(key));
                ExperimentController.Instance.ExperimentLists[newKey] = new List<object>(perBlockList);
                //set the value for each block
                for (int i = 0; i < session.blocks.Count; i++)
                {
                    session.blocks[i].settings.SetValue(newKey, perBlockList[i]);
                }
            }
            //for per trial parameters, they will be pseudo randomized and then set
            else if (key.StartsWith("per_block_list_"))
            {
                //key minus the prefix
                string newKey = key.Substring(15);

                if (session.blocks[0].settings.ContainsKey(newKey))
                {
                    break;
                }

                session.blocks[0].settings.SetValue(newKey, session.settings.GetObjectList(key));

                //if we already have that list move on
                if (ExperimentController.Instance.ExperimentLists.ContainsKey(newKey))
                {
                    break;
                }

                //the number of elements in the per trial list
                //should be equal or divisible to the number of trials
                int perTrialCount = session.settings.GetObjectList(key).Count;
                //the per trial list
                List<object> perTrialList = session.settings.GetObjectList(key);
                //the resulting list after being pseudo randomized
                List<object> pseudoList = new List<object>();
                //loop for each block and pseudo randomize
                //the count of per trial should be the same as the number of blocks
                for (int i = 0; i < perTrialCount; i++)
                {
                    if (perTrialList[i] != null)
                    {
                        pseudoList.Add(PseudoRandom(perTrialList[i].ToString(), session.blocks[i]));
                    }
                    else
                    {
                        pseudoList.Add(new List<object>());
                    }
                    //set the value in the block
                    session.blocks[i].settings.SetValue(newKey, pseudoList[i]);
                }
            }
            //for anything else
            else
            {
                if (!ExperimentController.Instance.ExperimentLists.ContainsKey(key))
                {
                    ExperimentController.Instance.ExperimentLists[key] = new List<object>
                    {
                        session.settings.GetObject(key)
                    };
                }
            }
        }

        if(session.isTrialContinue)
        {
            for(int i = 0; i < session.blocks.Count; i++)
            {
                session.blocks[i].settings.baseDict = JsonSerializer.Load<Dictionary<string, object>>("block_" + i);
            }
            session.currentTrialNum = PlayerPrefs.GetInt("currentTrial");
            session.NextTrial.block = session.blocks[PlayerPrefs.GetInt("currentBlock")];
            session.NextTrial.numberInBlock = PlayerPrefs.GetInt("trialInBlock");
            session.resumeTrialCountCache = PlayerPrefs.GetInt("currentTrial");
        }

        else if(session.isBlockContinue)
        {
            for(int i = 0; i < session.blocks.Count; i++)
            {
                session.blocks[i].settings.baseDict = JsonSerializer.Load<Dictionary<string, object>>("block_" + i);
            }
            session.resumeTrialCountCache = 0;
            session.NextTrial.block = session.blocks[PlayerPrefs.GetInt("currentBlock")];

            for(int i = 0; i < PlayerPrefs.GetInt("currentBlock"); i++)
            {
                session.resumeTrialCountCache += session.blocks[i].trials.Count;
            }
            session.currentTrialNum = session.resumeTrialCountCache;
        }

        for(int i = 0; i < session.blocks.Count; i++)
        {
            JsonSerializer.Save("block_" + i, session.blocks[i].settings.baseDict);
        }
    }
    ///<summary>
    ///Create task objects based on experiment settings from JSON file. When creating a new Task object, said object needs to be added here or it will not be handled.
    ///</summary>
    public void GenerateTasks()
    {
        Session s = ExperimentController.Instance.Session;

        //for each block create a task
        //for every block there is a corresponding task object
        foreach (Block theBlock in s.blocks)
        {
            ExperimentController experimentController = ExperimentController.Instance;
            experimentController.taskPrefabNames.Add(Char.ToUpper(theBlock.settings.GetString("task")[0]) + theBlock.settings.GetString("task").Substring(1) + "Prefab");
        }
    }
    /// <summary>
    /// Pseudo randomizes the list of objects
    /// </summary>
    /// <param name="key"> the key of the list</param>
    /// <param name="theBlock"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public object PseudoRandom(string key, Block theBlock)
    {
        List<object> list = Session.instance.settings.GetObjectList(key);
        //a pseudo random version of the list

        //if it is not divisible by the number of trials
        if (theBlock.trials.Count % list.Count != 0)
        {
            throw new NullReferenceException("The total number of trials in block " + theBlock.number + " is not divisible by the number of elements in: " + key);
        }

        //if the passed key has these prefixes
        if (key.StartsWith("per_block_list_"))
        {
            key = key.Substring(15);
        }

        //If the experiment list does not contain this object list
        if (!ExperimentController.Instance.ExperimentLists.ContainsKey(key))
            ExperimentController.Instance.ExperimentLists[key] = new List<object>(list);

        if (list.Count == 1)
            return list[0];
        else if (list.Count == 0)
        {
            Debug.LogError(key + " contains no elements. Not possible to select");
            throw new NullReferenceException();
        }

        return GeneratePseudoRandom(list, theBlock.trials.Count / list.Count);
    }

    public object GeneratePseudoRandom(List<object> list, int listMod)
    {
        List<object> randomList = new List<object>
        {
            //set the capacity to be the size of the original list
            Capacity = list.Count * listMod
        };
        List<int> RandomOrder = GenerateIntegerList(0, (list.Count * listMod) - 1);
        pseudoRandomOrder = ShuffleList(RandomOrder);

        //loop through and randomly set new values
        foreach (int i in pseudoRandomOrder)
        {
            if (i >= list.Count)
            {
                randomList.Add(list[i % list.Count]);
            }
            else
            {
                randomList.Add(list[i]);
            }
        }

        return randomList;
    }
    /// <summary>
    /// Generates a list of integers from min to max
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    List<int> GenerateIntegerList(int min, int max)
    {
        List<int> list = new List<int>();
        for (int i = min; i <= max; i++)
        {
            list.Add(i);
        }
        return list;
    }
    /// <summary>
    /// Shuffles the list of integers
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    List<int> ShuffleList(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int temp = list[i];
            int randomIndex = UnityEngine.Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
        return list;
    }
}
