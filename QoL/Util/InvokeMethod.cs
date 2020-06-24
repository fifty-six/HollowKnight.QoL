using System;
using HutongGames.PlayMaker;

namespace QoL.Util {
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
}