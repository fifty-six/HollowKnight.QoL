using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;

namespace QoL.Util
{
    internal static class FsmUtil
    {
        [PublicAPI]
        public static void RemoveAction(this FsmState state, int index)
        {
            state.Actions = state.Actions.Where((x, ind) => ind != index).ToArray();
        }
        
        [PublicAPI]
        public static void RemoveAction<T>(this FsmState state) where T : FsmStateAction
        {
            state.Actions = state.Actions.RemoveFirst(x => x is T).ToArray();
        }

        public static void RemoveAllOfType<T>(this FsmState state) where T : FsmStateAction
        {
            state.Actions = state.Actions.Where(x => !(x is T)).ToArray();
        }

        [PublicAPI]
        public static void RemoveAnim(this PlayMakerFSM fsm, string stateName, int index)
        {
            var anim = fsm.GetAction<Tk2dPlayAnimationWithEvents>(stateName, index);

            var @event = new FsmEvent(anim.animationCompleteEvent ?? anim.animationTriggerEvent);

            FsmState state = fsm.GetState(stateName);

            state.RemoveAction(index);

            state.InsertAction
            (
                new NextFrameEvent
                {
                    sendEvent = @event,
                    Active = true,
                    Enabled = true
                },
                index
            );
        }

        [PublicAPI]
        public static FsmState GetState(this PlayMakerFSM fsm, string stateName)
        {
            return fsm.FsmStates.FirstOrDefault(t => t.Name == stateName);
        }

        [PublicAPI]
        public static FsmState CopyState(this PlayMakerFSM fsm, string stateName, string newState)
        {
            var state = new FsmState(fsm.GetState(stateName))
            {
                Name = newState
            };

            fsm.Fsm.States = fsm.FsmStates.Append(state).ToArray();

            return state;
        }
        
        [PublicAPI]
        public static T GetAction<T>(this FsmState state, int index) where T : FsmStateAction
        {
            FsmStateAction act = state.Actions[index];

            return (T) act;
        }

        [PublicAPI]
        public static T GetAction<T>(this PlayMakerFSM fsm, string stateName, int index) where T : FsmStateAction
        {
            FsmStateAction act = fsm.GetState(stateName).Actions[index];

            return (T) act;
        }
        
        [PublicAPI]
        public static T GetAction<T>(this FsmState state) where T : FsmStateAction
        {
            return state.Actions.OfType<T>().FirstOrDefault();
        }
        
        [PublicAPI]
        public static T GetAction<T>(this PlayMakerFSM fsm, string stateName) where T : FsmStateAction
        {
            return fsm.GetState(stateName).GetAction<T>();
        }

        [PublicAPI]
        public static void AddAction(this FsmState state, FsmStateAction action)
        {
            state.Actions = state.Actions.Append(action).ToArray();
        }

        [PublicAPI]
        public static void InsertAction(this FsmState state, FsmStateAction action, int index)
        {
            state.Actions = state.Actions.Insert(index, action).ToArray();

            action.Init(state);
        }
        
        [PublicAPI]
        public static void ChangeTransition(this PlayMakerFSM self, string state, string eventName, string toState)
        {
            self.GetState(state).ChangeTransition(eventName, toState);
        }

        [PublicAPI]
        public static void ChangeTransition(this FsmState state, string eventName, string toState)
        {
            state.Transitions.First(tr => tr.EventName == eventName).ToState = toState;
        }
        
        [PublicAPI]
        public static void AddTransition(this FsmState state, FsmEvent @event, string toState)
        {
            state.Transitions = state.Transitions.Append
             (
                 new FsmTransition
                 {
                     FsmEvent = @event,
                     ToState = toState
                 }
             )
             .ToArray();
        }
        
        [PublicAPI]
        public static void AddTransition(this FsmState state, string eventName, string toState)
        {
            state.AddTransition(new FsmEvent(eventName), toState);
        }

        [PublicAPI]
        public static void RemoveTransition(this FsmState state, string transition)
        {
            state.Transitions = state.Transitions.Where(trans => transition != trans.ToState).ToArray();
        }

        [PublicAPI]
        public static void AddCoroutine(this FsmState state, Func<IEnumerator> method)
        {
            state.InsertCoroutine(state.Actions.Length, method);
        }

        [PublicAPI]
        public static void AddMethod(this FsmState state, Action method)
        {
            state.InsertMethod(state.Actions.Length, method);
        }
        
        [PublicAPI]
        public static void InsertMethod(this FsmState state, int index, Action method)
        {
            state.InsertAction(new InvokeMethod(method), index);
        }
        
        [PublicAPI]
        public static void InsertCoroutine(this FsmState state, int index, Func<IEnumerator> coro, bool wait = true)
        {
            state.InsertAction(new InvokeCoroutine(coro, wait), index);
        }

        [PublicAPI]
        public static FsmInt GetOrCreateInt(this PlayMakerFSM fsm, string intName)
        {
            FsmInt prev = fsm.FsmVariables.IntVariables.FirstOrDefault(x => x.Name == intName);

            if (prev != null)
                return prev;
            
            var @new = new FsmInt(intName);

            fsm.FsmVariables.IntVariables = fsm.FsmVariables.IntVariables.Append(@new).ToArray();
            
            return @new;
        }

        [PublicAPI]
        public static FsmBool CreateBool(this PlayMakerFSM fsm, string boolName)
        {
            var @new = new FsmBool(boolName);
            
            fsm.FsmVariables.BoolVariables = fsm.FsmVariables.BoolVariables.Append(@new).ToArray();
            
            return @new;
        }

        [PublicAPI]
        public static void AddToSendRandomEventV3
        (
            this SendRandomEventV3 sre,
            string toState,
            float weight,
            int eventMaxAmount,
            int missedMaxAmount,
            [CanBeNull] string eventName = null,
            bool createInt = true
        )
        {
            var fsm = sre.Fsm.Owner as PlayMakerFSM;

            string state = sre.State.Name;

            eventName ??= toState.Split(' ').First();

            fsm.GetState(state).AddTransition(eventName, toState);

            sre.events = sre.events.Append(fsm.GetState(state).Transitions.Single(x => x.FsmEvent.Name == eventName).FsmEvent).ToArray();
            sre.weights = sre.weights.Append(weight).ToArray();
            sre.trackingInts = sre.trackingInts.Append(fsm.GetOrCreateInt($"Ms {eventName}")).ToArray();
            sre.eventMax = sre.eventMax.Append(eventMaxAmount).ToArray();
            sre.trackingIntsMissed = sre.trackingIntsMissed.Append(fsm.GetOrCreateInt($"Ct {eventName}")).ToArray();
            sre.missedMax = sre.missedMax.Append(missedMaxAmount).ToArray();
        }

        [PublicAPI]
        public static FsmState CreateState(this PlayMakerFSM fsm, string stateName)
        {
            var state = new FsmState(fsm.Fsm)
            {
                Name = stateName
            };
            
            fsm.Fsm.States = fsm.FsmStates.Append(state).ToArray();

            return state;
        }
    }
}