using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GUIFramework.TestApp
{
    [AsyncMethodBuilder(typeof(MyTaskMethodBuilder<>))]
    public class MyTask<T>
    {
        private string _caller;
        private IDialog<T> _dialog;

        public MyTask(IDialog<T> dialog, [CallerMemberName]string method = null)
        {
            _caller = method;
            _dialog = dialog;
            _dialog.Finished += Dialog_Closed;
        }

        public MyTask(T result, [CallerMemberName]string method = null)
        {
            _caller = method;
            Done = true;
            Result = result;
        }

        public MyTask<T> MapTo(State state, string propertyName)
        {
            var value = (T)state.GetType().GetProperty(propertyName).GetValue(state);

            return MapTo(value);
        }

        public MyTask<T> MapTo(T value)
        {
            Done = value != null;
            Result = value;

            return this;
        }

        public Awaiter<T> GetAwaiter()
        {
            if (!Done)
                _dialog.ShowDialog();

            return new Awaiter<T>(this, _caller);
        }

        private void Dialog_Closed(T result)
        {
            Done = true;
            Result = result;

            OnDone?.Invoke();
        }

        public bool Done;
        public T Result;

        public event Action OnDone;

    }

    public interface IDialog<T>
    {
        event Action<T> Finished;
        void ShowDialog();
    }

    public class Awaiter<T> : INotifyCompletion
    {
        MyTask<T> _task;
        Action _completion;
        string _caller;
        DateTime _start;

        public Awaiter(MyTask<T> task, string caller)
        {
            _caller = caller;
            _start = DateTime.Now;

            _task = task;
            _task.OnDone += _task_OnDone;
        }

        private void _task_OnDone()
        {
            var duration = DateTime.Now - _start;

            Debug.WriteLine($"Task done with {_task.Result}, duration {duration.TotalMilliseconds}, called from {_caller}");

            _completion();
        }

        public bool IsCompleted => _task.Done;
        public T GetResult() => _task.Result;
        public void OnCompleted(Action completion)
        {
            _completion = completion;

            //var stateMachine = completion.Target.GetType().GetField("m_stateMachine",
            //          BindingFlags.NonPublic | BindingFlags.Instance).GetValue(completion.Target);
        }
    }

    class MyTaskMethodBuilder<T>
    {
        public static MyTaskMethodBuilder<T> Create() => new MyTaskMethodBuilder<T>();

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }

        public void SetException(Exception exception)
        {
            //Task = new MyTask<T>(default(T));
        }

        public void SetResult(T result)
        {
            //Task = new MyTask<T>(result);
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {

        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {

        }

        public MyTask<T> Task { get; private set; }
    }

    public class State
    {
        public string Input1 { get; set; } = null;
        public string Input2 { get; set; } = null;
    }
}
