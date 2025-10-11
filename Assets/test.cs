using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

public class test : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public List<Test1> TEST = new List<Test1>();
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    [Serializable] public class Test1
    {
        public SerializedDictionary<int, Test2> test = new SerializedDictionary<int, Test2>();
    }
    [Serializable] public class Test2
    {
        public SerializedDictionary<int, TestInfo> test = new SerializedDictionary<int, TestInfo>();
    }
    [Serializable] public class TestInfo
    {
        public SkillsBaseSO skillsBaseSO = null;
        public int level = 0;
        public SerializedDictionary<CharacterData.TypeStatistic, int> statistics = new SerializedDictionary<CharacterData.TypeStatistic, int>();
    }
}
