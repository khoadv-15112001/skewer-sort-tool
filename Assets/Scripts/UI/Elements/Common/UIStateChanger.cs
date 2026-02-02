using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class UIStateChanger : MonoBehaviour
{
    [Serializable]
    public class StateOfObjects
    {
        public int state;
        public GameObject[] objects;
    }

    [SerializeField] private List<StateOfObjects> states;

    public int CurrentState => _currentState;

    public int Max => states.Count;

    private int _currentState = 0;

    void OnEnable()
    {
        _currentState = 0;
        UpdateState();
    }

    private void UpdateState()
    {
        foreach (var item in states)
        {
            foreach (var obj in item.objects)
            {
                obj.SetActive(item.state == _currentState);
            }
        }
    }

    public void NextState()
    {
        _currentState++;
        UpdateState();
    }
       
    public void SetState(int state)
    {
        _currentState = state;
        UpdateState();
    }

#if UNITY_EDITOR
    public void OnValidate()
    {
        int idx = 0;
        foreach (var item in states)
        {
            item.state = idx;
            idx++;
        }
    }
#endif
}
