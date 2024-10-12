using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GUIFramework.Core
{
    public class Component<T> : Component
    {
        protected Dictionary<Component, Func<T, object>> _mappings = new Dictionary<Component, Func<T, object>>();
        protected Action<T, Control> _propertyMapping = null;

        public T Value { get; private set; }
        /*void test()
        {
            Form(state, state=>state.Counter, Label)
        }*/

        public Component(Control control, Action<T, Control> propertyMapping = null) : base()
        {
            _propertyMapping = propertyMapping;
        }

        public override void SetValue(object value)
        {
            Value = (T)value;

            SetValue(Value);
            _propertyMapping?.Invoke(Value, _control);
        }

        protected override void SetChildrenRoot(Component root)
        {
            foreach (var child in _mappings.Keys)
                child.SetRoot(root);
        }

        protected override void SetChildrenParent(Component parent)
        {
            foreach (var child in _mappings.Keys)
                child.SetParent(parent);
        }

        public void SetValue(T value)
        {
            SetValueInternal(value);

            foreach (var control in _mappings.Keys)
            {
                var mapping = _mappings[control];
                var subValue = mapping(value);
                control.SetValue(subValue);
            }
        }

        public void AddMapping(Component control, Func<T, object> mapping)
        {
            if (mapping == null)
                mapping = t => t;

            _mappings.Add(control, mapping);

            var c = control.GetControl();

            c.Dock = DockStyle.Fill;
            _control.Controls.Add(c);
        }

        protected virtual void SetValueInternal(T value)
        {
            _control.Text = value?.ToString();
        }

        /*public static implicit operator (Func<T, T> mapping, Component<T> control) (Component<T> ctrl)
        {
            return (t => t, ctrl);
        }*/
    }
}
