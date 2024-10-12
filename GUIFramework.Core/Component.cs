using System.Windows.Forms;

namespace GUIFramework.Core
{
    public class Component
    {
        public Component Root { get; private set; }
        public Component Parent { get; private set; }
        protected ActionHandler _actionHandler;
        protected Control _control;

        public Component()
        {
        }

        public Control GetControl()
        {
            return _control;
        }

        public void SetRoot(Component root)
        {
            Root = root;

            SetChildrenRoot(root);
        }

        public void SetParent(Component parent)
        {
            Parent = parent;

            SetChildrenParent(this);
        }

        protected virtual void SetChildrenRoot(Component root)
        {
        }

        protected virtual void SetChildrenParent(Component parent)
        {
        }

        public virtual void SetValue(object value)
        {
        }

        public void DispatchAction(string action, object payload)
        {
            _actionHandler?.Invoke(action, payload, () => Root);
            Parent?.DispatchAction(action, payload);
        }

        public void SetActionHandler(ActionHandler actionHandler)
        {
            _actionHandler = actionHandler;
        }
    }
}
