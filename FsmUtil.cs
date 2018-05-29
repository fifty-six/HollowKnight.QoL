using System.Collections.Generic;
using HutongGames.PlayMaker;
using System.Reflection;
using System;
using System.Linq;
using System.Text;

namespace FsmUtil
{
    static class FsmUtil
    {
        public static T[] RemoveAt<T>(this T[] source, int index)
        {
            T[] dest = new T[source.Length - 1];
            if (index > 0)
                Array.Copy(source, 0, dest, 0, index);

            if (index < source.Length - 1)
                Array.Copy(source, index + 1, dest, index, source.Length - index - 1);


            return dest;
        }

        private static FieldInfo fsmStringParamsField;
        static FsmUtil()
        {
            FieldInfo[] fieldInfo = typeof(ActionData).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int j = 0; j < fieldInfo.Length; j++)
            {
                if (fieldInfo[j].Name == "fsmStringParams")
                {
                    fsmStringParamsField = fieldInfo[j];
                    break;
                }
            }
        }

        public static void RemoveAction(PlayMakerFSM fsm, string stateName, int index)
        {
            for (int i = 0; i < fsm.FsmStates.Length; i++)
            {
                if (fsm.FsmStates[i].Name == stateName)
                {
                    FsmStateAction[] actions = fsm.FsmStates[i].Actions;

                    Array.Resize(ref actions, actions.Length + 1);
                    Log(actions[0].GetType().ToString());

                    actions.RemoveAt(index);

                    fsm.FsmStates[i].Actions = actions;
                }
            }
        }

        public static FsmState GetState(PlayMakerFSM fsm, string stateName)
        {
            for (int i = 0; i < fsm.FsmStates.Length; i++)
            {
                if (fsm.FsmStates[i].Name == stateName)
                {
                    FsmStateAction[] actions = fsm.FsmStates[i].Actions;

                    return fsm.FsmStates[i];
                }
            }
            return null;
        }

        public static FsmStateAction GetAction(PlayMakerFSM fsm, string stateName, int index)
        {
            for (int i = 0; i < fsm.FsmStates.Length; i++)
            {
                if (fsm.FsmStates[i].Name == stateName)
                {
                    FsmStateAction[] actions = fsm.FsmStates[i].Actions;

                    Array.Resize(ref actions, actions.Length + 1);
                    Log(actions[index].GetType().ToString());

                    return actions[index];
                }
            }
            return null;
        }

        public static T GetAction<T>(PlayMakerFSM fsm, string stateName, int index) where T : FsmStateAction
        {
            for (int i = 0; i < fsm.FsmStates.Length; i++)
            {
                if (fsm.FsmStates[i].Name == stateName)
                {
                    FsmStateAction[] actions = fsm.FsmStates[i].Actions;

                    Array.Resize(ref actions, actions.Length + 1);
                    Log(actions[index].GetType().ToString());

                    return actions[index] as T;
                }
            }
            return null;
        }

        public static void AddAction(PlayMakerFSM fsm, string stateName, FsmStateAction action)
        {
            for (int i = 0; i < fsm.FsmStates.Length; i++)
            {
                if (fsm.FsmStates[i].Name == stateName)
                {
                    FsmStateAction[] actions = fsm.FsmStates[i].Actions;

                    Array.Resize(ref actions, actions.Length + 1);
                    actions[actions.Length - 1] = action;

                    fsm.FsmStates[i].Actions = actions;
                }
            }
        }

        public static void ChangeTransition(PlayMakerFSM fsm, string stateName, string eventName, string toState)
        {
            for (int i = 0; i < fsm.FsmStates.Length; i++)
            {
                if (fsm.FsmStates[i].Name == stateName)
                {
                    foreach (FsmTransition trans in fsm.FsmStates[i].Transitions)
                    {
                        if (trans.EventName == eventName)
                        {
                            trans.ToState = toState;
                        }
                    }
                }
            }
        }

        public static void RemoveTransitions(PlayMakerFSM fsm, List<string> states, List<string> transitions)
        {
            for (int i = 0; i < fsm.FsmStates.Length; i++)
            {
                if (states.Contains(fsm.FsmStates[i].Name))
                {
                    List<FsmTransition> transList = new List<FsmTransition>();
                    foreach (FsmTransition trans in fsm.FsmStates[i].Transitions)
                    {
                        if (!transitions.Contains(trans.ToState))
                        {
                            transList.Add(trans);
                        }
                        else
                        {
                            Log(string.Format("Removing {0} transition from {1}", trans.ToState, fsm.FsmStates[i].Name));
                        }
                    }
                    fsm.FsmStates[i].Transitions = transList.ToArray();
                }
            }
        }

        public static void ReplaceStringVariable(PlayMakerFSM fsm, List<string> states, Dictionary<string, string> dict)
        {
            for (int i = 0; i < fsm.FsmStates.Length; i++)
            {
                bool found = false;
                if (states.Contains(fsm.FsmStates[i].Name))
                {
                    foreach (FsmString str in (List<FsmString>)fsmStringParamsField.GetValue(fsm.FsmStates[i].ActionData))
                    {
                        List<FsmString> val = new List<FsmString>();
                        if (dict.ContainsKey(str.Value))
                        {
                            val.Add(dict[str.Value]);
                            found = true;
                        }
                        else
                        {
                            val.Add(str);
                        }

                        if (val.Count > 0)
                        {
                            fsmStringParamsField.SetValue(fsm.FsmStates[i].ActionData, val);
                        }
                    }
                    if (found)
                    {
                        fsm.FsmStates[i].LoadActions();
                    }
                }
            }
        }

        public static void ReplaceStringVariable(PlayMakerFSM fsm, string state, Dictionary<string, string> dict)
        {
            for (int i = 0; i < fsm.FsmStates.Length; i++)
            {
                bool found = false;
                if (fsm.FsmStates[i].Name == state || state == "")
                {
                    foreach (FsmString str in (List<FsmString>)fsmStringParamsField.GetValue(fsm.FsmStates[i].ActionData))
                    {
                        List<FsmString> val = new List<FsmString>();
                        if (dict.ContainsKey(str.Value))
                        {
                            val.Add(dict[str.Value]);
                            found = true;
                        }
                        else
                        {
                            val.Add(str);
                        }

                        if (val.Count > 0)
                        {
                            fsmStringParamsField.SetValue(fsm.FsmStates[i].ActionData, val);
                        }
                    }
                    if (found)
                    {
                        fsm.FsmStates[i].LoadActions();
                    }
                }
            }
        }

        public static void ReplaceStringVariable(PlayMakerFSM fsm, string state, string src, string dst)
        {
            Log(string.Format("Replacing FSM Strings"));
            for (int i = 0; i < fsm.FsmStates.Length; i++)
            {
                bool found = false;
                if (fsm.FsmStates[i].Name == state || state == "")
                {
                    Log(string.Format("Found FsmState with name \"{0}\" ", fsm.FsmStates[i].Name));
                    foreach (FsmString str in (List<FsmString>)fsmStringParamsField.GetValue(fsm.FsmStates[i].ActionData))
                    {
                        List<FsmString> val = new List<FsmString>();
                        Log(string.Format("Found FsmString with value \"{0}\" ", str));
                        if (str.Value.Contains(src))
                        {
                            val.Add(dst);
                            found = true;
                            Log(string.Format("Found FsmString with value \"{0}\", changing to \"{1}\" ", str, dst));
                        }
                        else
                        {
                            val.Add(str);
                        }

                        if (val.Count > 0)
                        {
                            fsmStringParamsField.SetValue(fsm.FsmStates[i].ActionData, val);
                        }
                    }
                    if (found)
                    {
                        fsm.FsmStates[i].LoadActions();
                    }
                }
            }
        }

        public static void Log(String str)
        {
            Modding.Logger.Log("[FSM UTIL]: " + str);
        }

    }
        ////////////////
        // Extensions //
        ////////////////

     public static class FsmutilExt
    {
        public static void RemoveAction(this PlayMakerFSM fsm, string stateName, int index) => FsmUtil.RemoveAction(fsm, stateName, index);

        public static FsmState GetState(this PlayMakerFSM fsm, string stateName) => FsmUtil.GetState(fsm, stateName);

        public static FsmStateAction GetAction(this PlayMakerFSM fsm, string stateName, int index) => FsmUtil.GetAction(fsm, stateName, index);

        public static T GetAction<T>(this PlayMakerFSM fsm, string stateName, int index) where T : FsmStateAction => FsmUtil.GetAction<T>(fsm, stateName, index);

        public static void AddAction(this PlayMakerFSM fsm, string stateName, FsmStateAction action) => FsmUtil.AddAction(fsm, stateName, action);

        public static void ChangeTransition(this PlayMakerFSM fsm, string stateName, string eventName, string toState) => FsmUtil.ChangeTransition(fsm, stateName, eventName, toState);

        public static void RemoveTransitions(this PlayMakerFSM fsm, List<string> states, List<string> transitions) => FsmUtil.RemoveTransitions(fsm, states, transitions);

        public static void ReplaceStringVariable(this PlayMakerFSM fsm, List<string> states, Dictionary<string, string> dict) => FsmUtil.ReplaceStringVariable(fsm, states, dict);

        public static void ReplaceStringVariable(this PlayMakerFSM fsm, string state, Dictionary<string, string> dict) => FsmUtil.ReplaceStringVariable(fsm, state, dict);

        public static void ReplaceStringVariable(this PlayMakerFSM fsm, string state, string src, string dst) => FsmUtil.ReplaceStringVariable(fsm, state, src, dst);
    }
}
