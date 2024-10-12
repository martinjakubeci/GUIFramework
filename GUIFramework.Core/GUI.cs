using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUIFramework.Core
{
    public static class GUI
    {
        public static ActionHandler ActionHandler<T>(T initialState, Action<T> stateTracker, Func<T, string, object, T> handler)
        {
            bool running = false;
            var state = initialState;

            return (action, payload, root) =>
            {
                if (running)
                    return;

                running = true;

                var newState = handler(state, action, payload);

                if (newState != null)
                {
                    root().SetValue(newState);
                    stateTracker?.Invoke(newState);
                    state = newState;
                }

                running = false;
            };
        }

        public static Action<T> TimeMachine<T>(Component root, T initialState)
        {
            var history = new BindingList<T>();

            var handler = ActionHandler(history, null, (state, action, payload) =>
            {
                if (action == "LIST_SELECTION")
                    root.DispatchAction("STATE_CHANGE", payload);

                return state;
            });

            /*var form = Form(history,
                List(() =>
                    Label<T>()));

            Setup(form, handler);
            form.Show();*/

            history.Add(initialState.Copy());

            return state => history.Add(state.Copy());
        }

        public static Component<T> Map<T, U>(Func<T, U> mapping1, Component<U> control1)
        {
            var panel = new Component<T>(new Panel());

            panel.AddMapping(control1, t => mapping1(t));

            return panel;
        }

        //public static Component<T> Input<T>(Func<T, string> mapping, /*Action<T> updater,*/ string action)
        //{
        //    return new InputComponent<T>(mapping, /*updater,*/ action);

        //}

        public static Component<T> Input<T>(string action)
        {
            var textBox = new TextBox();
            var component = new Component<T>(textBox);

            textBox.TextChanged += (sender, e) => component.Root.DispatchAction(action, textBox.Text);

            return component;

        }

        public static Component<T> TabControl<T>(params Either<Component<T>, Component>[] tabs)
        {
            var tabControl = new TabControl();
            var component = new Component<T>(tabControl);

            foreach (var tab in tabs)
            {
                //var tabPage = new Component<T>(new TabPage(""));

                //tab.Match(c => tabPage.AddMapping(c, t => t), c => tabPage.AddMapping(c, t => t));
                //component.AddMapping(tabPage, t => t);

                tab.Match(c => component.AddMapping(c, t => t), c => component.AddMapping(c, t => t));
            }

            return component;
        }

        public static Component<T> Tab<T>(string header, Either<Component<T>, Component> component, Action<T, TabPage> propertyMapping = null, Action<TabPage> propertySetter = null)
        {
            var control = new TabPage(header);
            var tabPage = new Component<T>(control, (t, c) => propertyMapping?.Invoke(t, c as TabPage));

            propertySetter?.Invoke(control);
            component.Match(c => tabPage.AddMapping(c, t => t), c => tabPage.AddMapping(c, t => t));

            return tabPage;
        }

        public static Component<T> Stack<T>(params Either<Component<T>, Component>[] controls)
            => StackHorizontal(controls);

        public static Component<T> StackHorizontal<T>(params Either<Component<T>, Component>[] controls)
            => Grid(controls.Length, 1, controls);

        public static Component<T> StackVertical<T>(params Either<Component<T>, Component>[] controls)
            => Grid(1, controls.Length, controls);

        public static Component<T> Grid<T>(CellSize[] columns, CellSize[] rows, params Either<Component<T>, Component>[] controls)
        {
            var c = new TableLayoutPanel();
            c.ColumnCount = columns.Length;
            c.RowCount = rows.Length;

            foreach (var column in columns)
                column.Match(
                    relativeSize => c.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, relativeSize)),
                    fixedSize => c.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, fixedSize)),
                    () => c.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize, 0)));

            foreach (var row in rows)
                row.Match(
                    relativeSize => c.RowStyles.Add(new RowStyle(SizeType.Percent, relativeSize)),
                    fixedSize => c.RowStyles.Add(new RowStyle(SizeType.Absolute, fixedSize)),
                    () => c.RowStyles.Add(new RowStyle(SizeType.AutoSize, 0)));

            var panel = new Component<T>(c);

            foreach (var control in controls)
                control.Match(cc => panel.AddMapping(cc, t => t), cc => panel.AddMapping(cc, t => t));

            return panel;
        }

        public static CellSize[] CellDescriptors(params CellSize[] cells) => cells;
        public static CellSize[] Rows(params CellSize[] cells) => CellDescriptors(cells);
        public static CellSize[] Columns(params CellSize[] cells) => CellDescriptors(cells);
        public static CellSize Fixed(int fixedSize) => new CellSize(fixedSize);
        public static CellSize Relative(float relativeSize) => new CellSize(relativeSize);
        public static CellSize Auto() => new CellSize();
        public static Either<Component<T>, Component>[] Cells<T>(params Either<Component<T>, Component>[] controls) => controls;

        public class CellSize
        {
            public float? RelativeSize { get; private set; } = null;
            public int? FixedSize { get; private set; } = null;

            public CellSize(float relativeSize) => RelativeSize = relativeSize;
            public CellSize(int fixedSize) => FixedSize = fixedSize;
            public CellSize() { }

            public void Match(Action<float> relativeAction, Action<int> fixedAction, Action noAction)
            {
                if (RelativeSize.HasValue)
                    relativeAction(RelativeSize.Value);
                else if (FixedSize.HasValue)
                    fixedAction(FixedSize.Value);
                else
                    noAction();
            }
        }

        public static Component<T> Grid<T>(int width, int height, params Either<Component<T>, Component>[] controls)
        {
            var columns = new List<CellSize>();
            var rows = new List<CellSize>();

            var share = 100.0f / width;
            var rest = 100 - share * width;

            for (int i = 0; i < width; i++)
            {
                var toAdd = share;

                if (i == controls.Length - 1)
                    toAdd += rest;

                columns.Add(Relative(toAdd));
            }

            share = 100.0f / height;
            rest = 100 - share * height;

            for (int i = 0; i < height; i++)
            {
                var toAdd = share;

                if (i == controls.Length - 1)
                    toAdd += rest;

                rows.Add(Relative(toAdd));
            }

            return Grid(columns.ToArray(), rows.ToArray(), controls);
        }

        public static Form<T> Form<T>(T state, Component<T> control, [CallerMemberName]string header = null)
        {
            var form = new Form<T>(header);
            form.AddMapping(control, t => t);
            control.GetControl().Dock = DockStyle.Fill;

            form.SetValue(state);

            return form;
        }

        public static Component<T> Button<T>(string text, string action, Action<T, Button> propertyMapping = null, Func<T, object> payloadMapping = null)
        {
            var button = new Button() { Text = text };
            var component = new Component<T>(button, (t, c) => propertyMapping(t, c as Button));

            button.Click += (sender, e) =>
            {
                var payload = payloadMapping?.Invoke(component.Value);

                component.DispatchAction(action, payload);
            };

            return component;
        }

        /*public static Component Button(string text, string action)
        {
            var button = new Button() { Text = text };
            var ctrl = new Component(button);

            button.Click += (sender, e) => ctrl.DispatchAction(action, null);

            return ctrl;
        }

        public static Component<T> Label<T>()
        {
            return new Component<T>(
                new Label() { AutoSize = true }, 
                (t, control) => control.Text = t?.ToString());
        }

        public static Component Label(string text, Action<Label> propertySetter = null)
        {
            var label = new Label() { Text = text, AutoSize = true };

            propertySetter?.Invoke(label);

            return new Component(label);
        }

        public static Component Custom(Control control)
            => new Component(control);

        public static Component LabelFor<T, U>(Expression<Func<T, U>> expression)
        {
            var text = "undef";

            if (expression.Body is MemberExpression member)
                text = member.Member.Name.ToString();

            return new Component(new Label() { Text = text, AutoSize = true });
        }

        public static Component<BindingList<T>> List<T>(Func<Component<T>> listItemFactory)
        {
            var list = new ListComponent<T>(listItemFactory);

            return list;
        }*/

        public static void Run<T>(Form<T> root)
        {
            Application.Run(root.GetControl() as Form);
        }

        public static void Setup<T>(Form<T> root, ActionHandler handler)
        {
            root.SetActionHandler(handler);
            root.SetRoot(root);
            root.SetParent(null);
        }
    }

    public delegate void ActionHandler(string action, object payload, Func<Component> rootFactory);

    public class Form<T> : Component<T>
    {
        public Form(string header) : base(new Form() { Text = header })
        {
        }

        public void Show()
        {
            (_control as Form).Show();
        }
    }
}
