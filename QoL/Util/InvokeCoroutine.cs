using System;
using System.Collections;
using HutongGames.PlayMaker;

namespace QoL.Util
{
    public class InvokeCoroutine : FsmStateAction
    {
        private readonly Func<IEnumerator> _coro;
        private readonly bool _wait;

        public InvokeCoroutine(Func<IEnumerator> f, bool wait)
        {
            _coro = f;
            _wait = wait;
        }

        private IEnumerator Coroutine()
        {
            yield return _coro?.Invoke();
            Finish();
        }

        public override void OnEnter()
        {
            Fsm.Owner.StartCoroutine(_wait ? Coroutine() : _coro?.Invoke());
            if (!_wait) Finish();
        }
    }
}