using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Logger = Modding.Logger;

// Taken and modified from
// https://raw.githubusercontent.com/KayDeeTee/HK-NGG/master/src/

namespace QoL
{
    internal static class FsmUtil
    {
        // ReSharper disable once InconsistentNaming
        private static readonly FieldInfo FsmStringParamsField;

        static FsmUtil()
        {
            FieldInfo[] fieldInfo =
                typeof(ActionData).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo t in fieldInfo)
            {
                if (t.Name != "fsmStringParams") continue;
                FsmStringParamsField = t;
                break;
            }
        }

        public static void RemoveAction(PlayMakerFSM fsm, string stateName, int index)
        {
            foreach (FsmState t in fsm.FsmStates)
            {
                if (t.Name != stateName) continue;
                FsmStateAction[] actions = t.Actions;

                FsmStateAction action = fsm.GetAction(stateName, index);
                actions = actions.Where(x => x != action).ToArray();
                Log(action.GetType().ToString());

                t.Actions = actions;
            }
        }

        public static void RemoveAnim(PlayMakerFSM fsm, string stateName, int index)
        {
            Tk2dPlayAnimationWithEvents anim = fsm.GetAction<Tk2dPlayAnimationWithEvents>(stateName, index);
            FsmEvent @event = new FsmEvent(anim.animationCompleteEvent ?? anim.animationTriggerEvent);
            fsm.RemoveAction(stateName, index);
            fsm.InsertAction(stateName, new NextFrameEvent
            {
                sendEvent = @event,
                Active = true,
                Enabled = true
            }, index);
        }

        public static FsmState GetState(PlayMakerFSM fsm, string stateName)
        {
            return fsm.FsmStates.Where(t => t.Name == stateName)
                .Select(t => new {t, actions = t.Actions})
                .Select(t1 => t1.t)
                .FirstOrDefault();
        }

        public static FsmState CopyState(PlayMakerFSM fsm, string stateName, string newState)
        {
            var state = new FsmState(fsm.GetState(stateName)) {Name = newState};

            List<FsmState> fsmStates = fsm.FsmStates.ToList();
            fsmStates.Add(state);
            fsm.Fsm.States = fsmStates.ToArray();

            return state;
        }

        public static FsmStateAction GetAction(PlayMakerFSM fsm, string stateName, int index)
        {
            foreach (FsmState t in fsm.FsmStates)
            {
                if (t.Name != stateName) continue;
                FsmStateAction[] actions = t.Actions;

                Array.Resize(ref actions, actions.Length + 1);

                return actions[index];
            }

            return null;
        }

        public static T GetAction<T>(PlayMakerFSM fsm, string stateName, int index) where T : FsmStateAction
        {
            return GetAction(fsm, stateName, index) as T;
        }

        public static void AddAction(PlayMakerFSM fsm, string stateName, FsmStateAction action)
        {
            foreach (FsmState t in fsm.FsmStates)
            {
                if (t.Name != stateName) continue;
                FsmStateAction[] actions = t.Actions;

                Array.Resize(ref actions, actions.Length + 1);
                actions[actions.Length - 1] = action;

                t.Actions = actions;
            }
        }

        public static void InsertAction(PlayMakerFSM fsm, string stateName, FsmStateAction action, int index)
        {
            foreach (FsmState t in fsm.FsmStates)
            {
                if (t.Name != stateName) continue;
                List<FsmStateAction> actions = t.Actions.ToList();

                actions.Insert(index, action);

                t.Actions = actions.ToArray();
            }
        }

        public static void ChangeTransition(PlayMakerFSM fsm, string stateName, string eventName, string toState)
        {
            foreach (FsmState t in fsm.FsmStates)
            {
                if (t.Name != stateName) continue;
                foreach (FsmTransition trans in t.Transitions)
                {
                    if (trans.EventName == eventName)
                    {
                        trans.ToState = toState;
                    }
                }
            }
        }

        public static void AddTransition(PlayMakerFSM fsm, string stateName, string eventName, string toState)
        {
            foreach (FsmState t in fsm.FsmStates)
            {
                if (t.Name != stateName) continue;
                List<FsmTransition> transitions = t.Transitions.ToList();
                transitions.Add(new FsmTransition
                {
                    FsmEvent = new FsmEvent(eventName),
                    ToState = toState
                });
                t.Transitions = transitions.ToArray();
            }
        }

        public static void RemoveTransitions(PlayMakerFSM fsm, IEnumerable<string> states,
            IEnumerable<string> transitions)
        {
            IEnumerable<string> enumerable = states as string[] ?? states.ToArray();

            foreach (FsmState t in fsm.FsmStates)
            {
                if (!enumerable.Contains(t.Name)) continue;

                t.Transitions = t.Transitions.Where(trans => !transitions.Contains(trans.ToState)).ToArray();
            }
        }

        public static void RemoveTransition(PlayMakerFSM fsm, string state, string transition)
        {
            foreach (FsmState t in fsm.FsmStates)
            {
                if (state != t.Name) continue;

                t.Transitions = t.Transitions.Where(trans => transition != trans.ToState).ToArray();
            }
        }

        public static void ReplaceStringVariable(PlayMakerFSM fsm, List<string> states, Dictionary<string, string> dict)
        {
            foreach (FsmState t in fsm.FsmStates)
            {
                bool found = false;
                if (!states.Contains(t.Name)) continue;
                foreach (FsmString str in (List<FsmString>) FsmStringParamsField.GetValue(t.ActionData))
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
                        FsmStringParamsField.SetValue(t.ActionData, val);
                    }
                }

                if (found)
                {
                    t.LoadActions();
                }
            }
        }

        public static void ReplaceStringVariable(PlayMakerFSM fsm, string state, Dictionary<string, string> dict)
        {
            foreach (FsmState t in fsm.FsmStates)
            {
                bool found = false;
                if (t.Name != state && state != "") continue;
                foreach (FsmString str in (List<FsmString>) FsmStringParamsField.GetValue(t.ActionData))
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
                        FsmStringParamsField.SetValue(t.ActionData, val);
                    }
                }

                if (found)
                {
                    t.LoadActions();
                }
            }
        }

        public static void ReplaceStringVariable(PlayMakerFSM fsm, string state, string src, string dst)
        {
            Log("Replacing FSM Strings");
            foreach (FsmState t in fsm.FsmStates)
            {
                bool found = false;
                if (t.Name != state && state != "") continue;
                Log($"Found FsmState with name \"{t.Name}\" ");
                foreach (FsmString str in (List<FsmString>) FsmStringParamsField.GetValue(t.ActionData))
                {
                    List<FsmString> val = new List<FsmString>();
                    Log($"Found FsmString with value \"{str}\" ");
                    if (str.Value.Contains(src))
                    {
                        val.Add(dst);
                        found = true;
                        Log($"Found FsmString with value \"{str}\", changing to \"{dst}\" ");
                    }
                    else
                    {
                        val.Add(str);
                    }

                    if (val.Count > 0)
                    {
                        FsmStringParamsField.SetValue(t.ActionData, val);
                    }
                }

                if (found)
                {
                    t.LoadActions();
                }
            }
        }

        public static void InsertMethod(PlayMakerFSM fsm, string stateName, int index, Action method)
        {
            InsertAction(fsm, stateName, new InvokeMethod(method), index);
        }

        private static void Log(string str)
        {
            Logger.Log("[FSM UTIL]: " + str);
        }
    }
    
    ///////////////////////
    // Method Invocation //
    ///////////////////////

    public class InvokeMethod : FsmStateAction
    {
        private readonly Action _action;

        public InvokeMethod(Action a)
        {
            _action = a;
        }
        
        public override void OnEnter()
        {
            _action?.Invoke();
            Finish();
        }
    }
    

    ////////////////
    // Extensions //
    ////////////////

    public static class FsmutilExt
    {
        public static void RemoveAnim(this PlayMakerFSM fsm, string stateName, int index) =>
            FsmUtil.RemoveAnim(fsm, stateName, index);

        public static void InsertAction(this PlayMakerFSM fsm, string stateName, FsmStateAction action, int index) =>
            FsmUtil.InsertAction(fsm, stateName, action, index);

        public static void RemoveAction(this PlayMakerFSM fsm, string stateName, int index) =>
            FsmUtil.RemoveAction(fsm, stateName, index);

        public static FsmState GetState(this PlayMakerFSM fsm, string stateName) => FsmUtil.GetState(fsm, stateName);

        public static FsmStateAction GetAction(this PlayMakerFSM fsm, string stateName, int index) =>
            FsmUtil.GetAction(fsm, stateName, index);

        public static T GetAction<T>(this PlayMakerFSM fsm, string stateName, int index) where T : FsmStateAction =>
            FsmUtil.GetAction<T>(fsm, stateName, index);

        public static FsmState CopyState(this PlayMakerFSM fsm, string stateName, string newState) =>
            FsmUtil.CopyState(fsm, stateName, newState);

        public static void AddAction(this PlayMakerFSM fsm, string stateName, FsmStateAction action) =>
            FsmUtil.AddAction(fsm, stateName, action);

        public static void
            ChangeTransition(this PlayMakerFSM fsm, string stateName, string eventName, string toState) =>
            FsmUtil.ChangeTransition(fsm, stateName, eventName, toState);

        public static void InsertMethod(this PlayMakerFSM fsm, string stateName, int index, Action method) =>
            FsmUtil.InsertMethod(fsm, stateName, index, method);

        public static void AddTransition(this PlayMakerFSM fsm, string stateName, string eventName, string toState) =>
            FsmUtil.AddTransition(fsm, stateName, eventName, toState);

        public static void RemoveTransitions(this PlayMakerFSM fsm, IEnumerable<string> states,
            IEnumerable<string> transitions) =>
            FsmUtil.RemoveTransitions(fsm, states, transitions);

        public static void RemoveTransition(this PlayMakerFSM fsm, string state, string transition) =>
            FsmUtil.RemoveTransition(fsm, state, transition);

        public static void ReplaceStringVariable(this PlayMakerFSM fsm, List<string> states,
            Dictionary<string, string> dict) => FsmUtil.ReplaceStringVariable(fsm, states, dict);

        public static void
            ReplaceStringVariable(this PlayMakerFSM fsm, string state, Dictionary<string, string> dict) =>
            FsmUtil.ReplaceStringVariable(fsm, state, dict);

        public static void ReplaceStringVariable(this PlayMakerFSM fsm, string state, string src, string dst) =>
            FsmUtil.ReplaceStringVariable(fsm, state, src, dst);
    }
}